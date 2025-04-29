using System.ComponentModel.DataAnnotations;

namespace IotProject.Shared.Models.Requests
{
    public class UserCreateRequest
    {
		[Required(ErrorMessage = "First name is required.")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Last name is required.")]
		public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }

		[Required(ErrorMessage = "Password is required.")]
		public string Password { get; set; }

		[Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
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
