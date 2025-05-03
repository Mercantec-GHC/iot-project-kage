using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IotProject.Shared.Models.Responses;
using IotProject.API.Data;
using Microsoft.EntityFrameworkCore;
using IotProject.Shared.Models.Database;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using IotProject.Shared.Models.Requests;

namespace IotProject.API.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class RoomController(AppDbContext context) : ControllerBase
	{
		// Get list of rooms by user id endpoint.
		[HttpGet("get-all"), Authorize]
		public async Task<ActionResult<RoomGetAllResponse>> GetAll()
		{
			// Fetches data and checks for null or empty strings/references.
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId)) return StatusCode(500);
			var rooms = await context.Rooms
				.Where(r => r.OwnerId == userId)
				.ToListAsync();
			if (rooms.IsNullOrEmpty()) return StatusCode(500);

			// Maps the rooms to the required response class.
			List<RoomGetResponse> mappedRooms = new();
			foreach (var room in rooms)
			{
				var mappedRoom = new RoomGetResponse
				(
					room.Id,
					room.Name,
					room.Description
				);
				mappedRooms.Add(mappedRoom);
			}

			return Ok(new RoomGetAllResponse(mappedRooms));
		}

		// Get room by room id endpoint.
		[HttpGet("get-room"), Authorize]
		public async Task<ActionResult<RoomGetResponse>> GetRoom(string id)
		{
			// Fetches data and checks for null or empty strings/references.
			var room = await context.Rooms.FindAsync(id);
			if (room == null) return StatusCode(500);

			return Ok(new RoomGetResponse(room.Id, room.Name, room.Description));
		}

		// Create room endpoint. 
		[HttpPost("Create"), Authorize]
		public async Task<ActionResult<RoomCreateResponse>> Create(RoomCreateRequest requestModel)
		{
			// Fetches data and checks for null or empty strings/references.
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId)) return StatusCode(500);
			if (requestModel == null) return BadRequest();
			if (string.IsNullOrEmpty(requestModel.Name)) return BadRequest("Room information incomplete.");
			//if (await context.Rooms.AnyAsync(r => r.Name == requestModel.Name && r.OwnerId == userId)) return BadRequest("Room name already in use.");

			// Maps the requestModel to a room.
			var room = new Room
			{
				Id = await GenerateRoomID(),
				Name = requestModel.Name,
				OwnerId = userId,
				Description = requestModel.Description!,
				DateCreated = DateTime.UtcNow,
				DateUpdated = DateTime.UtcNow
			};

			await context.AddAsync(room);
			await context.SaveChangesAsync();

			return StatusCode(201, new RoomCreateResponse(Message: "Room successfully created."));
		}

		// Update room endpoint.
		[HttpPatch("Update"), Authorize]
		public async Task<ActionResult> Update(RoomUpdateRequest requestModel)
		{
			// Fetches data and checks for null or empty strings/references.
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId)) return StatusCode(500);
			if (requestModel == null) return BadRequest();
			//if (await context.Rooms.AnyAsync(r => r.Name == requestModel.Name && r.OwnerId == userId)) return BadRequest("Room name already in use.");
			var room = await context.Rooms.FindAsync(requestModel.Id);
			if (room == null) return StatusCode(500);

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
			var room = await context.Rooms.FindAsync(id);
			if (room == null) return StatusCode(500);

			// Removes the room from the database, and saves the changes.
			context.Rooms.Remove(room);
			await context.SaveChangesAsync();

			return Ok("Room successfully deleted.");
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