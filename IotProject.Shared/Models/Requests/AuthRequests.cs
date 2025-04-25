namespace IotProject.Shared.Models.Requests
{
    public class UserCreateRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class UserLoginRequest
    {
        public string Email { get; set;}
        public string Password { get; set; }
    }

    public class UserRefreshRequest
    {
        public string Token { get; set; }
    }
}
