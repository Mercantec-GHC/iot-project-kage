﻿using System.ComponentModel.DataAnnotations;

namespace IotProject.Shared.Models.Requests
{
    public class UserCreateRequest
    {
		[Required(ErrorMessage = "First name is required.")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Last name is required.")]
		public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

		[Required(ErrorMessage = "Password is required.")]
        [StringLength(64, ErrorMessage = "Password must be at least 8 characters.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_])[^\s]{8,}$", ErrorMessage = "Password is not secure.")]
        public string Password { get; set; }

		[Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
		public string ConfirmPassword { get; set; }
    }

    public class UserLoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set;}

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }

    public class UserRefreshRequest
    {
        public string Token { get; set; }
    }

    public class UserEditInformationRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }

    public class UserEditPasswordRequest
    {
        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "A new password is required.")]
        [StringLength(64, ErrorMessage = "Password must be at least 8 characters.",  MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_])[^\s]{8,}$", ErrorMessage = "Password is not secure.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class UserDeleteRequest
    {
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}