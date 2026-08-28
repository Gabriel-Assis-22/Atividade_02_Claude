namespace Application.DTOs;

public record LoginRequest(string Email, string Senha);
public record LoginResponse(string Token, string Nome, string Email, string Role);

public record RegisterRequest(string Nome, string Email, string Senha, string Role = "usuario");

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Token, string NovaSenha);

public record ValidateTokenResponse(int UserId, string Nome, string Email, string Role);
