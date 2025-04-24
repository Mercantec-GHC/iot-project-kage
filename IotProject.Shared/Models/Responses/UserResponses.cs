namespace IotProject.Shared.Models.Responses
{
    public record UserCreateResponse(string Id, string Message = null!);
    public record UserLoginResponse(string Token,int Expires, string RefreshToken, string Message = null!);
}