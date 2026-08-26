namespace Application.DTOs.Auth;

public record LoginRequest(string Email, string Senha);
public record RegisterRequest(string Nome, string Email, string Senha);
public record AuthResponse(string Token, string Nome, string Email);
