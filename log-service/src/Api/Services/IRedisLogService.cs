using Api.DTOs;

namespace Api.Services;

public interface IRedisLogService
{
    Task<string> AddEventAsync(CreateLogEventRequest req);
    Task<IEnumerable<LogEventResponse>> GetEventsAsync(int limit = 50);
}
