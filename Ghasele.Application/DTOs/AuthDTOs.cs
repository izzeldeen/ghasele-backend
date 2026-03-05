using System;

namespace Ghasele.Application.DTOs
{
    public record RegisterRequest(string Password, string FullName, string PhoneNumber);
    
    public record LoginRequest(string PhoneNumber, string Password);
    
    public record AuthResponse(string Token, Guid Id, string Username, string? Email, string FullName, string PhoneNumber);

    public record UpdateFcmTokenRequest(Guid UserId, string Token);
}
