using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Storage;

public class S3StorageService : IStorageService
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly string _bucketName;
    private readonly string _publicBaseUrl;
    private readonly string _region;
    private readonly ILogger<S3StorageService> _logger;
    private bool _bucketInitialized = false;

    public S3StorageService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<S3StorageService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _endpoint = (configuration["GARAGE_ENDPOINT"]
                     ?? Environment.GetEnvironmentVariable("GARAGE_ENDPOINT")
                     ?? configuration["MINIO_ENDPOINT"]
                     ?? Environment.GetEnvironmentVariable("MINIO_ENDPOINT")
                     ?? "http://garage:3900").TrimEnd('/');

        _accessKey = configuration["GARAGE_ACCESS_KEY"]
                     ?? Environment.GetEnvironmentVariable("GARAGE_ACCESS_KEY")
                     ?? configuration["MINIO_ROOT_USER"]
                     ?? "GK0123456789abcdef01234567";

        _secretKey = configuration["GARAGE_SECRET_KEY"]
                     ?? Environment.GetEnvironmentVariable("GARAGE_SECRET_KEY")
                     ?? configuration["MINIO_ROOT_PASSWORD"]
                     ?? "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

        _bucketName = configuration["GARAGE_BUCKET_NAME"]
                      ?? Environment.GetEnvironmentVariable("GARAGE_BUCKET_NAME")
                      ?? configuration["MINIO_BUCKET_NAME"]
                      ?? Environment.GetEnvironmentVariable("MINIO_BUCKET_NAME")
                      ?? "profile-photos";

        _publicBaseUrl = configuration["GARAGE_PUBLIC_BASE_URL"]
                         ?? Environment.GetEnvironmentVariable("GARAGE_PUBLIC_BASE_URL")
                         ?? configuration["MINIO_PUBLIC_BASE_URL"]
                         ?? Environment.GetEnvironmentVariable("MINIO_PUBLIC_BASE_URL")
                         ?? "/storage";

        _region = "garage";
    }

    public async Task EnsureBucketCreatedAsync()
    {
        if (_bucketInitialized) return;

        var bucketUri = $"{_endpoint}/{_bucketName}";

        for (int tentativa = 1; tentativa <= 5; tentativa++)
        {
            try
            {
                using var headReq = CreateSignedRequest(HttpMethod.Head, bucketUri, null, "application/octet-stream");
                var headRes = await _httpClient.SendAsync(headReq);

                if (headRes.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Bucket '{Bucket}' verificado com sucesso no Garage S3.", _bucketName);
                    _bucketInitialized = true;
                    return;
                }

                if (headRes.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    using var putReq = CreateSignedRequest(HttpMethod.Put, bucketUri, null, "application/xml");
                    var putRes = await _httpClient.SendAsync(putReq);
                    if (putRes.IsSuccessStatusCode || putRes.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        _logger.LogInformation("Bucket '{Bucket}' criado ou existente no Garage S3.", _bucketName);
                        _bucketInitialized = true;
                        return;
                    }
                }

                _logger.LogWarning("Tentativa {Tentativa}/5 status {Status} ao verificar bucket '{Bucket}'.", 
                    tentativa, headRes.StatusCode, _bucketName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Tentativa {Tentativa}/5 erro ao verificar bucket: {Msg}", tentativa, ex.Message);
            }

            await Task.Delay(1000);
        }

        _bucketInitialized = true; // Permite prosseguir se o bucket já foi criado via CLI
    }

    public async Task<string> UploadFileAsync(Stream stream, string objectKey, string contentType)
    {
        await EnsureBucketCreatedAsync();

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var bytes = ms.ToArray();

        var safeKey = objectKey.TrimStart('/');
        var targetUri = $"{_endpoint}/{_bucketName}/{safeKey}";

        using var request = CreateSignedRequest(HttpMethod.Put, targetUri, bytes, contentType);
        request.Content = new ByteArrayContent(bytes);
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erro no upload S3: status {Status}, body: {Body}", response.StatusCode, errorBody);
            throw new InvalidOperationException($"Falha no upload para o Garage S3: {response.StatusCode} - {errorBody}");
        }

        _logger.LogInformation("Arquivo {ObjectKey} enviado com sucesso ao bucket {Bucket}", objectKey, _bucketName);
        return objectKey;
    }

    public async Task DeleteFileAsync(string objectKey)
    {
        try
        {
            var safeKey = objectKey.TrimStart('/');
            var targetUri = $"{_endpoint}/{_bucketName}/{safeKey}";
            using var request = CreateSignedRequest(HttpMethod.Delete, targetUri, null, "application/octet-stream");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Arquivo {ObjectKey} removido do bucket {Bucket}", objectKey, _bucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao tentar remover arquivo {ObjectKey} do storage", objectKey);
        }
    }

    public async Task<(Stream Stream, string ContentType)?> GetFileAsync(string objectKey)
    {
        try
        {
            var safeKey = objectKey.TrimStart('/');
            var targetUri = $"{_endpoint}/{_bucketName}/{safeKey}";
            using var request = CreateSignedRequest(HttpMethod.Get, targetUri, null, "application/octet-stream");
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            var stream = await response.Content.ReadAsStreamAsync();
            return (stream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter arquivo {ObjectKey} do storage", objectKey);
            return null;
        }
    }

    public string GetPublicUrl(string objectKey)
    {
        var trimmedBase = _publicBaseUrl.TrimEnd('/');
        var trimmedKey = objectKey.TrimStart('/');
        return $"{trimmedBase}/{trimmedKey}";
    }

    private HttpRequestMessage CreateSignedRequest(HttpMethod method, string requestUri, byte[]? payload, string contentType)
    {
        var uri = new Uri(requestUri);
        var now = DateTime.UtcNow;
        var amzDate = now.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);
        var dateStamp = now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        var payloadBytes = payload ?? Array.Empty<byte>();
        var payloadHash = Convert.ToHexString(SHA256.HashData(payloadBytes)).ToLowerInvariant();

        var request = new HttpRequestMessage(method, uri);
        request.Headers.Host = $"{uri.Host}:{uri.Port}";
        request.Headers.Add("x-amz-date", amzDate);
        request.Headers.Add("x-amz-content-sha256", payloadHash);

        var canonicalUri = uri.AbsolutePath;
        var canonicalQuery = string.Empty;

        // Cabeçalhos canônicos ordenados alfabeticamente
        var hostHeader = $"{uri.Host}:{uri.Port}";
        var canonicalHeaders = $"host:{hostHeader}\nx-amz-content-sha256:{payloadHash}\nx-amz-date:{amzDate}\n";
        var signedHeaders = "host;x-amz-content-sha256;x-amz-date";

        var canonicalRequest = $"{method.Method}\n{canonicalUri}\n{canonicalQuery}\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";
        var canonicalRequestHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest))).ToLowerInvariant();

        var algorithm = "AWS4-HMAC-SHA256";
        var credentialScope = $"{dateStamp}/{_region}/s3/aws4_request";
        var stringToSign = $"{algorithm}\n{amzDate}\n{credentialScope}\n{canonicalRequestHash}";

        var signingKey = GetSignatureKey(_secretKey, dateStamp, _region, "s3");
        var signature = Convert.ToHexString(HMACSHA256.HashData(signingKey, Encoding.UTF8.GetBytes(stringToSign))).ToLowerInvariant();

        var authHeader = $"{algorithm} Credential={_accessKey}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";
        request.Headers.TryAddWithoutValidation("Authorization", authHeader);

        return request;
    }

    private static byte[] GetSignatureKey(string key, string dateStamp, string regionName, string serviceName)
    {
        var kSecret = Encoding.UTF8.GetBytes("AWS4" + key);
        var kDate = HMACSHA256.HashData(kSecret, Encoding.UTF8.GetBytes(dateStamp));
        var kRegion = HMACSHA256.HashData(kDate, Encoding.UTF8.GetBytes(regionName));
        var kService = HMACSHA256.HashData(kRegion, Encoding.UTF8.GetBytes(serviceName));
        return HMACSHA256.HashData(kService, Encoding.UTF8.GetBytes("aws4_request"));
    }
}
