namespace IotProject.Shared.Models.Database
{
    public class RefreshToken
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
    }
}
