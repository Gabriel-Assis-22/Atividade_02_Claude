namespace Api.DTOs;

public record CreateLogEventRequest(
    int? UsuarioId,
    string Acao,
    string? Detalhes = null,
    string? IpOrigem = null,
    DateTime? Timestamp = null
);

public record LogEventResponse(
    string Id,
    int? UsuarioId,
    string Acao,
    string? Detalhes,
    string? IpOrigem,
    DateTime Timestamp
);
