using IotProject.API.Data;
using IotProject.Shared.Models.Database;
using IotProject.Shared.Models.Requests;
using IotProject.Shared.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IotProject.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DeviceController(AppDbContext context) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<ActionResult<DeviceRegisterResponse>> RegisterDevice(DeviceRegisterRequest requestModel)
        {
            // Builds a new Device object.
            var device = new Device
            {
                Id = await GenerateDeviceID(),
                ApiKey = Guid.NewGuid().ToString(),
                OwnerId = requestModel.OwnerId,
                DeviceType = requestModel.DeviceType,
                Config = requestModel.Config,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };

            // Adds the Device to the database.
            await context.AddAsync(device);
            await context.SaveChangesAsync();

            return Ok(new DeviceRegisterResponse(device.Id, device.ApiKey, $"Device of type '{device.DeviceType}' successfully registered."));
        }

        [HttpGet("GetDevices"), Authorize]
        public async Task<ActionResult<List<DeviceResponse>>> GetDevices()
        {
            var user = await GetSignedInUser();
            if (user == null) return StatusCode(500);

            return Ok(user.Devices.Select(d =>
            {
                // Find the newest Data.
                var latest = d.Data.OrderByDescending(x => x.Timestamp).FirstOrDefault();

                // Create a new DeviceResponse and add it to the list.
                return new DeviceResponse(
                    Id: d.Id,
                    Type: d.DeviceType,
                    RoomId: d.RoomId!,
                    Data: latest?.Data,
                    LastUpdate: latest?.Timestamp
                );
            }));
        }

        [HttpGet("GetData"), Authorize]
        public async Task<ActionResult<List<DeviceDataResponse>>> GetData([FromQuery] string deviceId)
        {
            var user = await GetSignedInUser();
            if (user == null) return StatusCode(500);

            // Find the device with the given id and include all data.
            var device = await context.Devices.Where(d => d.Id == deviceId)
                .Include(d => d.Data)
                .FirstOrDefaultAsync();

            if (device == null) return NotFound($"Device with id: '{deviceId}', was not found.");

            return Ok(device.Data.OrderByDescending(d => d.Timestamp)
                .Select(d => new DeviceDataResponse(Data: d.Data, Timestamp: d.Timestamp))
                .ToList());
        }

        [HttpPost("PostData")]
        public async Task<ActionResult> PostData([FromHeader] string deviceId, [FromHeader] string apiKey, [FromBody] Dictionary<string, object> data)
        {
            // Finds the device using deviceId and validates it with an apiKey.
            var device = await context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId && d.ApiKey == apiKey);
            if (device == null) return Unauthorized("Device authentication incorrect!");

            DeviceData deviceData = new DeviceData
            {
                DeviceId = deviceId,
                Timestamp = DateTime.UtcNow,
                Data = data
            };
            await context.AddAsync(deviceData);
            await context.SaveChangesAsync();

            return Ok("Data successfully posted.");
        }

        /// <summary>
        /// Finds the currently signed in user, using the <see cref="ClaimTypes"/> NameIdentifier from the JWT token.
        /// </summary>
        /// <returns>The currently signed in user. If no user was found, returns null.</returns>
        private async Task<User?> GetSignedInUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return null;
            var user = await context.Users.Where(u => u.Id == userId)
                .Include(u => u.Devices) // Include all owned devices.
                .ThenInclude(d => d.Data) // Include the data from the device.
                .FirstOrDefaultAsync();
            if (user == null) return null;

            return user;
        }

        /// <summary>
        /// Generates a GUID for the Device Id, checks if GUID exists in the database.
        /// </summary>
        /// <returns>A unique id not already used.</returns>
        private async Task<string> GenerateDeviceID()
        {
            while (true)
            {
                var userId = Guid.NewGuid().ToString();
                if (await context.Devices.FindAsync(userId) == null) return userId;
            }
        }
    }
}
