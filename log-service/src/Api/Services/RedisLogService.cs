using Api.DTOs;
using StackExchange.Redis;

namespace Api.Services;

public class RedisLogService(IConnectionMultiplexer redis, ILogger<RedisLogService> logger) : IRedisLogService
{
    private const string StreamKey = "audit_stream";

    public async Task<string> AddEventAsync(CreateLogEventRequest req)
    {
        var db = redis.GetDatabase();
        var ts = req.Timestamp ?? DateTime.UtcNow;

        var entries = new NameValueEntry[]
        {
            new("usuario_id", req.UsuarioId?.ToString() ?? string.Empty),
            new("acao", req.Acao),
            new("detalhes", req.Detalhes ?? string.Empty),
            new("ip_origem", req.IpOrigem ?? string.Empty),
            new("timestamp", ts.ToString("o"))
        };

        var messageId = await db.StreamAddAsync(StreamKey, entries);
        logger.LogInformation("Evento de auditoria registrado no Redis Stream: {Id} - {Acao} pelo usuario {UsuarioId}",
            messageId, req.Acao, req.UsuarioId);

        return messageId.ToString();
    }

    public async Task<IEnumerable<LogEventResponse>> GetEventsAsync(int limit = 50)
    {
        var db = redis.GetDatabase();

        // IDatabase.StreamRangeAsync(key, minId, maxId, count, messageOrder, flags)
        var streamEntries = await db.StreamRangeAsync(
            StreamKey,
            minId: default,
            maxId: default,
            count: limit,
            messageOrder: Order.Descending);

        var list = new List<LogEventResponse>();
        foreach (var entry in streamEntries)
        {
            var values = entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

            int? userId = null;
            if (values.TryGetValue("usuario_id", out var uidStr) && int.TryParse(uidStr, out var uid))
            {
                userId = uid;
            }

            values.TryGetValue("acao", out var acao);
            values.TryGetValue("detalhes", out var detalhes);
            values.TryGetValue("ip_origem", out var ipOrigem);

            DateTime timestamp = DateTime.UtcNow;
            if (values.TryGetValue("timestamp", out var tsStr) && DateTime.TryParse(tsStr, out var parsedTs))
            {
                timestamp = parsedTs;
            }

            list.Add(new LogEventResponse(
                entry.Id.ToString(),
                userId,
                acao ?? string.Empty,
                string.IsNullOrWhiteSpace(detalhes) ? null : detalhes,
                string.IsNullOrWhiteSpace(ipOrigem) ? null : ipOrigem,
                timestamp
            ));
        }

        return list;
    }
}
