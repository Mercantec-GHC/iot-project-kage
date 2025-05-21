namespace IotProject.Shared.Models.Database
{
    public class RoomImage
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public Room Room { get; set; }
        public string FileName { get; set; }
        public string Image { get; set; }
    }
}
