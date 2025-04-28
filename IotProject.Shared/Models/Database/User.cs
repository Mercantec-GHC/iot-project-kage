namespace IotProject.Shared.Models.Database
{
    public class User : Base
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<Device> Devices { get; set; } = new List<Device>();

        // MFA prep
        public bool IsMfaEnabled { get; set; }
        public string? MfaSecretKey { get; set; }
        public DateTime? MfaSetupDate { get; set; }
        public string? MfaBackupCodes { get; set; }
    }

    public enum UserRole
    {
        User,
        Admin
    }
}
