using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IotProject.Shared.Models.Responses;
using IotProject.API.Data;
using Microsoft.EntityFrameworkCore;
using IotProject.Shared.Models.Database;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using IotProject.Shared.Models.Requests;
using IotProject.Shared.Utilities;
using Microsoft.AspNetCore.Cors;
using System.Net.Mime;

namespace IotProject.API.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class RoomController(AppDbContext context) : ControllerBase
	{
		// Get list of rooms by user id endpoint.
		[HttpGet("GetAll"), Authorize]
		public async Task<ActionResult<List<RoomGetResponse>>> GetAll()
		{
			var user = await GetSignedInUser();
			if (user == null) return StatusCode(500);

			var rooms = user.Rooms;
			if (rooms == null) return new List<RoomGetResponse>();

            return Ok(rooms.Select(r => new RoomGetResponse(r.Id, r.Name, r.Description)).ToList());
        }

		// Get room by room id endpoint.
		[HttpGet("GetRoom"), Authorize]
		public async Task<ActionResult<RoomGetResponse>> GetRoom(string id)
		{
            // Fetches data and checks for null or empty strings/references.
            var user = await GetSignedInUser();
            if (user == null) return StatusCode(500); 
			var room = user.Rooms.FirstOrDefault(r => r.Id == id);
			if (room == null) return NotFound($"Room with id: '{id}' was not found.");

			return Ok(new RoomGetResponse(room.Id, room.Name, room.Description));
		}

		// Create room endpoint. 
		[HttpPost("Create"), Authorize]
		public async Task<ActionResult<RoomCreateResponse>> Create(RoomCreateRequest requestModel)
		{
            // Fetches data and checks for null or empty strings/references.
            var user = await GetSignedInUser();
            if (user == null) return StatusCode(500);
            if (requestModel == null) return BadRequest();
			if (string.IsNullOrEmpty(requestModel.Name)) return BadRequest("Room information incomplete.");
			//if (await context.Rooms.AnyAsync(r => r.Name == requestModel.Name && r.OwnerId == userId)) return BadRequest("Room name already in use.");

			// Maps the requestModel to a room.
			var room = new Room
			{
				Id = await GenerateRoomID(),
				Name = requestModel.Name,
				OwnerId = user.Id,
				Description = requestModel.Description!,
				DateCreated = DateTime.UtcNow,
				DateUpdated = DateTime.UtcNow
			};

			await context.Rooms.AddAsync(room);
			await context.SaveChangesAsync();

			return StatusCode(201, new RoomCreateResponse(room.Id, "Room successfully created."));
		}

		// Update room endpoint.
		[HttpPatch("Update"), Authorize]
		public async Task<ActionResult> Update(RoomUpdateRequest requestModel)
		{
            // Fetches data and checks for null or empty strings/references.
            var user = await GetSignedInUser();
            if (user == null) return StatusCode(500);
            if (requestModel == null) return BadRequest();
            //if (await context.Rooms.AnyAsync(r => r.Name == requestModel.Name && r.OwnerId == userId)) return BadRequest("Room name already in use.");

            var room = user.Rooms.FirstOrDefault(r => r.Id == requestModel.Id);
			if (room == null) return NotFound($"Room with id: '{requestModel.Id}' was not found.");

			// Changes the required items on the room.
			room.Name = requestModel.Name;
			room.Description = requestModel.Description!;
			room.DateUpdated = DateTime.UtcNow;

			await context.SaveChangesAsync();

			return Ok();
		}

		// Delete room endpoint.
		[HttpDelete("Delete"), Authorize]
		public async Task<ActionResult> Delete(string id)
		{
            // Fetches data and checks for null or empty strings/references.
            var user = await GetSignedInUser();
            if (user == null) return StatusCode(500);
            var room = user.Rooms.FirstOrDefault(r => r.Id == id);
			if (room == null) return NotFound($"Room with id: '{id}' was not found.");

			// Removes the room from the database, and saves the changes.
			context.Rooms.Remove(room);
			await context.SaveChangesAsync();

			return Ok("Room successfully deleted.");
		}

        [HttpGet("GetDevices"), Authorize]
        public async Task<ActionResult<List<DeviceResponse>>> GetDevices(string id)
        {
            var user = await GetSignedInUser();
            if (user == null) return StatusCode(500);

			var room = user.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null) return NotFound($"Room with id: '{id}' was not found.");

            return Ok(room.Devices.Select(d =>
            {
                // Find the newest Data.
                var latest = d.Data.OrderByDescending(x => x.Timestamp).FirstOrDefault();

                // Create a new DeviceResponse and add it to the list.
                return new DeviceResponse(
                    Id: d.Id,
                    Name: d.Name ?? DeviceTypes.GetDeviceType(d.DeviceType)?.Name,
                    Type: d.DeviceType,
                    RoomId: d.RoomId!,
                    Data: latest?.Data,
                    LastUpdate: latest?.Timestamp
                );
            }));
        }

        [HttpPost("SetRoomImage"), Authorize]
        public async Task<ActionResult> SetRoomImage(IFormFile file, [FromQuery] string RoomId)
        {
            var user = await GetSignedInUser();
            if (user == null ) return StatusCode(500);

            var room = user.Rooms.FirstOrDefault(r => r.Id == RoomId);
            if (room == null) return NotFound($"Room with id: '{RoomId}' was not found.");

            if (file.Length == 0) return BadRequest("Invalid file.");

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var base64String = Convert.ToBase64String(fileBytes);

                var image = new RoomImage
                {
                    Image = base64String,
                    RoomId = room.Id,
                    FileName = file.FileName
                };

                if (room.Image == null)
                {
                    await context.RoomImages.AddAsync(image);
                }
                else
                {
                    // Update the existing image
                    room.Image.Image = base64String;
                    room.Image.FileName = file.FileName;
                }
                
                await context.SaveChangesAsync();

                return Ok("Image was successfully uploaded.");
            }
        }

        [HttpGet("GetRoomImage/{roomId}")]
        public async Task<ActionResult> GetRoomImage(string roomId)
        {
            var roomImage = await context.RoomImages.FirstOrDefaultAsync(r => r.RoomId == roomId);
            if (roomImage == null) return NotFound("Room image was not found.");
            
            try
            {
                var base64String = roomImage.Image;
                if (base64String.Contains("base64,"))
                {
                    base64String = base64String.Substring(base64String.IndexOf("base64,") + 7);
                }

                byte[] imageBytes = Convert.FromBase64String(base64String);
                var extension = Path.GetExtension(roomImage.FileName);

                string contentType = extension.ToLowerInvariant() switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".bmp" => "image/bmp",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };

                // Return image as a file
                return File(imageBytes, contentType);

            }
            catch (Exception ex)
            {
                return BadRequest("Invalid Base64 string: " + ex.Message);
            }
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
                .Include(u => u.Rooms) // Include the users rooms.
                .ThenInclude(r => r.Devices) // Include all devices in the room.
                .ThenInclude(d => d.Data) // Include the data from the device.
                .Include(u => u.Rooms)
                .ThenInclude(r => r.Image)
                .FirstOrDefaultAsync();
            if (user == null) return null;

            return user;
        }

        // Generates a unique GUID for the rooms. 
        private async Task<string> GenerateRoomID()
		{
			while (true)
			{
				var roomId = Guid.NewGuid().ToString();
				if (await context.Rooms.FindAsync(roomId) == null) return roomId;
			}
		}
	}
}