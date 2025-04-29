using System.ComponentModel.DataAnnotations;

namespace IotProject.Shared.Models.Requests
{
    public class UserCreateRequest
    {
		[Required(ErrorMessage = "First name is required.")]
		public required string FirstName { get; set; }

		[Required(ErrorMessage = "Last name is required.")]
		public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public required string Email { get; set; }

		[Required(ErrorMessage = "Password is required.")]
        [StringLength(64, ErrorMessage = "Password must be at least 8 characters.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_])[^\s]{8,}$", ErrorMessage = "Password is not secure.")]
        public required string Password { get; set; }

		[Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
		public required string ConfirmPassword { get; set; }
    }

    public class UserLoginRequest
    {
        public required string Email { get; set;}
        public required string Password { get; set; }
    }

    public class UserRefreshRequest
    {
        public required string Token { get; set; }
    }

    public class UserEditInformationRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }

    public class UserEditPasswordRequest
    {
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(64, ErrorMessage = "Password must be at least 8 characters.",  MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_])[^\s]{8,}$", ErrorMessage = "Password is not secure.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public required string ConfirmPassword { get; set; }
    }
}
