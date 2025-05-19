namespace IotProject.Shared.Models.Responses
{
    public record DeviceRegisterResponse(string Id, string ApiKey);
    public record DeviceNameResponse(string? Name, string? Message = null);

    public record DeviceResponse(
        string Id,
        string? Name,
        string Type,
        string RoomId,
        Dictionary<string, object>? Data,
        DateTime? LastUpdate
    );

    public record DeviceDataResponse(
        Dictionary<string, object>? Data,
        DateTime? Timestamp
    );

    public record DeviceGetConfigResponse(
        Dictionary<string, object> Config,
        long Timestamp
    );
}
