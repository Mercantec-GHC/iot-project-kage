namespace IotProject.Shared.Models.Responses
{
    public record DeviceRegisterResponse(string Id, string ApiKey, string Message = null!);

    public record DeviceResponse(
        string Id,
        string Type,
        string RoomId,
        Dictionary<string, object>? Data,
        DateTime? LastUpdate
    );

    public record DeviceDataResponse(
        Dictionary<string, object>? Data,
        DateTime? Timestamp
    );
}
