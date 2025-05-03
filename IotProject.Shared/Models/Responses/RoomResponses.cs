using IotProject.Shared.Models.Database;
namespace IotProject.Shared.Models.Responses
{
	public record RoomGetAllResponse(List<RoomGetResponse> Rooms);
	public record RoomGetResponse(string Id, string Name, string Description, string Message = null!);
	public record RoomCreateResponse(string Message = null!);
}