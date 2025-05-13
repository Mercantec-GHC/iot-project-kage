namespace IotProject.Shared.Models.Responses
{
    public record UserCreateResponse(string Id, string? Message = null);
    public record UserLoginResponse(string Token,int Expires, string RefreshToken, string? Message = null);
    public record UserMeResponse(string Id, string FirstName, string LastName, string Email, string Role, string? Message = null);

}