namespace Domain.Services;

public interface IStorageService
{
    Task EnsureBucketCreatedAsync();
    Task<string> UploadFileAsync(Stream stream, string objectKey, string contentType);
    Task DeleteFileAsync(string objectKey);
    Task<(Stream Stream, string ContentType)?> GetFileAsync(string objectKey);
    string GetPublicUrl(string objectKey);
}
