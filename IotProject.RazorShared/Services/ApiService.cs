
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using IotProject.Shared.Models.Requests;
using IotProject.Shared.Models.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace IotProject.RazorShared.Services
{
    public  class ApiService(HttpClient httpClient, ILocalStorageService localStorage, ISessionStorageService sessionStorage, IJSRuntime jsRuntime, IConfiguration configuration)
    {
        // Predefines the JsonOptions, to be used by various functions.
        private readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

		#region Room Services
		/// <summary>
		/// Adds a new room using the provided creation request.
		/// </summary>
		/// <remarks>This method sends the room creation request to the API and returns the success status of the
		/// operation. Ensure that the <paramref name="request"/> contains valid data before calling this method.</remarks>
		/// <param name="request">The details of the room to be created, including its properties and configuration.</param>
		/// <returns><see langword="true"/> if the room was successfully created; otherwise, <see langword="false"/>.</returns>
		public async Task<bool> AddRoom(RoomCreateRequest request)
		{
			if (!await authorize()) return false;

			// Serialize the request and send it to the API
			var createAsJson = JsonSerializer.Serialize(request);
			var response = await httpClient.PostAsync("Room/create", new StringContent(createAsJson, Encoding.UTF8, "application/json"));

			return response.IsSuccessStatusCode;
		}

		/// <summary>
		/// Retrieves a list of all rooms, ordered by their names.
		/// </summary>
		/// <remarks>This method sends an HTTP GET request to the "Room/getrooms" endpoint to fetch room
		/// data. If the request is unauthorized or unsuccessful, an empty list is returned. The returned list is sorted
		/// alphabetically by the room names.</remarks>
		/// <returns>A list of <see cref="RoomGetResponse"/> objects representing the rooms. Returns an empty list if the request
		/// is unauthorized or fails.</returns>
		public async Task<List<RoomGetResponse>> GetAllRooms()
		{
			if (!await authorize()) return new List<RoomGetResponse>();

			var response = await httpClient.GetAsync("room/getall");
			if (!response.IsSuccessStatusCode) return new List<RoomGetResponse>();

			var roomResult = JsonSerializer.Deserialize<List<RoomGetResponse>>(await response.Content.ReadAsStringAsync(), JsonOptions);

			return roomResult?.OrderBy(r => r.Name).ToList()!;
		}

        /// <summary>
        /// Retrieves the details of a room by its unique identifier.
        /// </summary>
        /// <remarks>This method requires authorization to execute successfully. If authorization fails or
        /// the API endpoint returns a non-success status code, the method will return <see langword="null"/>.</remarks>
        /// <param name="id">The unique identifier of the room to retrieve. Cannot be null or empty.</param>
        /// <returns>A <see cref="RoomGetResponse"/> object containing the room details if the operation is successful;
        /// otherwise, <see langword="null"/>.</returns>
		public async Task<RoomGetResponse?> GetRoom(string id)
		{
			if (!await authorize()) return null;

			// Call the API endpoint
			var response = await httpClient.GetAsync($"Room/GetRoom?id={id}");
			if (!response.IsSuccessStatusCode) return null;

			var room = JsonSerializer.Deserialize<RoomGetResponse>(await response.Content.ReadAsStringAsync(), JsonOptions);
			return room;
		}

		/// <summary>
		/// Updates the details of an existing room using the provided update request.
		/// </summary>
		/// <remarks>This method sends a PATCH request to the "Room/Update" endpoint with the provided room update
		/// details. The method requires a valid JWT token to be present in either local storage or session storage. If the
		/// token is missing or invalid, the method will return <see langword="false"/>.</remarks>
		/// <param name="request">The <see cref="RoomUpdateRequest"/> object containing the updated room details. This parameter must not be null
		/// and should include all required fields for the update.</param>
		/// <returns><see langword="true"/> if the room update operation was successful; otherwise, <see langword="false"/>.</returns>
		public async Task<bool> UpdateRoom(RoomUpdateRequest request)
		{
			if (!await authorize()) return false;

			var updateAsJson = JsonSerializer.Serialize(request);
			var response = await httpClient.PatchAsync("Room/Update", new StringContent(updateAsJson, Encoding.UTF8, "application/json"));

			return response.IsSuccessStatusCode;
		}

		/// <summary>
		/// Deletes a room with the specified identifier.
		/// </summary>
		/// <remarks>This method requires authorization before performing the delete operation. If authorization
		/// fails, the method returns <see langword="false"/>.</remarks>
		/// <param name="id">The unique identifier of the room to delete. Cannot be null or empty.</param>
		/// <returns><see langword="true"/> if the room was successfully deleted; otherwise, <see langword="false"/>.</returns>
		public async Task<bool> DeleteRoom(string id)
		{
			if (!await authorize()) return false;

			var response = await httpClient.DeleteAsync($"Room/Delete?id={id}");

			return response.IsSuccessStatusCode;
		}

		/// <summary>
		/// Retrieves a list of devices associated with the specified room.
		/// </summary>
		/// <remarks>This method performs an authorization check before attempting to retrieve devices. If
		/// authorization fails, an empty list is returned. The method also handles cases where the API response is
		/// unsuccessful or the deserialization of the response content results in null.</remarks>
		/// <param name="roomId">The unique identifier of the room for which devices are to be retrieved. Cannot be null or empty.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="DeviceResponse"/>
		/// objects representing the devices in the specified room. Returns an empty list if the room has no devices or if the
		/// operation fails.</returns>
		public async Task<List<DeviceResponse>> GetDevices(string roomId)
		{
			if (!await authorize()) return new List<DeviceResponse>();

			// Call the API endpoint
			var response = await httpClient.GetAsync($"Room/GetDevices?id={roomId}");
			if (!response.IsSuccessStatusCode) return new List<DeviceResponse>();

			var devices = JsonSerializer.Deserialize<List<DeviceResponse>>(await response.Content.ReadAsStringAsync(), JsonOptions);
			return devices ?? new List<DeviceResponse>();
		}

		private string? GetClientApiUrl()
		{
			return Environment.GetEnvironmentVariable("API_CLIENT_URL") ?? configuration.GetConnectionString("ApiClientUrl");
        }

		/// <summary>
		/// Generates the URL for retrieving the image associated with a specific room.
		/// </summary>
		/// <remarks>The returned URL is constructed using the base address of the HTTP client and the specified room
		/// ID. Ensure that <paramref name="roomId"/> is valid and corresponds to an existing room.</remarks>
		/// <param name="roomId">The unique identifier of the room. This value cannot be null or empty.</param>
		/// <returns>A string containing the full URL to retrieve the room's image.</returns>
		public string GetImageUrl(string roomId)
		{
			return $"{GetClientApiUrl()}/room/getroomimage/{roomId}";
		}

		/// <summary>
		/// Generates a URL for uploading an image associated with a specific room.
		/// </summary>
		/// <remarks>The returned URL includes the base address of the HTTP client and the query parameter specifying
		/// the room ID. Ensure that <paramref name="roomId"/> is valid and properly formatted before calling this
		/// method.</remarks>
		/// <param name="roomId">The unique identifier of the room for which the image upload URL is generated. This parameter cannot be null or
		/// empty.</param>
		/// <returns>A string containing the URL to upload an image for the specified room.</returns>
		public string GetImageUploadUrl(string roomId)
		{
			return $"{GetClientApiUrl()}/room/setroomimage?roomid={roomId}";
		}

		/// <summary>
		/// Updates the image associated with a specific room by uploading a new image.
		/// </summary>
		/// <remarks>This method retrieves the upload URL for the specified room and uses JavaScript interop to invoke
		/// a client-side function  for uploading the image. Ensure that the JavaScript file containing the "UploadImage"
		/// function is properly referenced.</remarks>
		/// <param name="roomId">The unique identifier of the room for which the image is being updated. Cannot be null or empty.</param>
		/// <param name="elementId">The HTML element ID of the image input field used for selecting the image to upload. Cannot be null or empty.</param>
		/// <returns><see langword="true"/> if the image upload was successful; otherwise, <see langword="false"/>.</returns>
		public async Task<bool> SetRoomImage(string roomId, string elementId)
		{
			var uploadUrl = GetImageUploadUrl(roomId);
			var fileModule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/iotproject.razorshared/js/room_image.js");
			var success = await fileModule.InvokeAsync<bool>("UploadImage", uploadUrl, elementId);

			return success;
		}
		#endregion

		#region Device Services
		/// <summary>
		/// Registers a device with the server using the provided registration request.
		/// </summary>
		/// <remarks>This method sends a POST request to the "device/register" endpoint with the
		/// serialized  registration request as the payload. If the authorization fails or the server responds with  a
		/// non-success status code, the method returns <see langword="null"/>.</remarks>
		/// <param name="request">The device registration request containing the necessary information to register the device. Cannot be null.</param>
		/// <returns>A <see cref="string"/> containing the server's response if the registration is successful;  otherwise, <see
		/// langword="null"/>.</returns>
		public async Task<string?> RegisterDevice(DeviceRegisterRequest request)
        {
            if (!await authorize()) return null;

            var requestBody = JsonSerializer.Serialize(request);
            var response = await httpClient.PostAsync("device/register", new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadAsStringAsync();

            return result;
        }

        /// <summary>
        /// Retrieves a list of all devices from the remote service.
        /// </summary>
        /// <remarks>This method sends an HTTP GET request to the remote service to fetch device
        /// information. If authorization fails or the request is unsuccessful, an empty list is returned. The response
        /// is deserialized into a list of <see cref="DeviceResponse"/> objects.</remarks>
        /// <returns>A list of <see cref="DeviceResponse"/> objects representing the devices retrieved from the remote service.
        /// Returns an empty list if authorization fails or the request is unsuccessful.</returns>
		public async Task<List<DeviceResponse>> GetAllDevices()
		{
			if (!await authorize()) return new List<DeviceResponse>();

			var response = await httpClient.GetAsync("device/getdevices");
			if (!response.IsSuccessStatusCode) return new List<DeviceResponse>();

			// Converts the received string to a list containing DeviceResponses.
			var deviceResult = JsonSerializer.Deserialize<List<DeviceResponse>>(await response.Content.ReadAsStringAsync(), JsonOptions);

			// Returns the deserialized DeviceResult.
			return deviceResult!;
		}

        /// <summary>
        /// Retrieves the details of a device by its unique identifier.
        /// </summary>
        /// <remarks>This method requires authorization to execute successfully. If authorization fails or
        /// the device  cannot be found, the method returns <see langword="null"/>.</remarks>
        /// <param name="deviceId">The unique identifier of the device to retrieve. Cannot be null or empty.</param>
        /// <returns>A <see cref="DeviceResponse"/> object containing the device details if the operation is successful; 
        /// otherwise, <see langword="null"/>.</returns>
		public async Task<DeviceResponse?> GetDevice(string deviceId)
		{
			if (!await authorize()) return null;

			var response = await httpClient.GetAsync($"device/getdevice?deviceId={deviceId}");
			if (!response.IsSuccessStatusCode) return null;

			var deviceResult = JsonSerializer.Deserialize<DeviceResponse>(await response.Content.ReadAsStringAsync(), JsonOptions);
			return deviceResult;
		}

        /// <summary>
        /// Updates the name of a device using the provided request model.
        /// </summary>
        /// <remarks>This method sends an HTTP PUT request to update the device name. Ensure that the user
        /// is authorized before calling this method, as unauthorized requests will result in a failure.</remarks>
        /// <param name="requestModel">An object containing the new device name and any additional required information. The request model must not
        /// be null.</param>
        /// <returns><see langword="true"/> if the device name was successfully updated; otherwise, <see langword="false"/>.</returns>
		public async Task<bool> EditDeviceName(DeviceNameRequest requestModel)
		{
			if (!await authorize()) return false;

			var requestAsJson = JsonSerializer.Serialize(requestModel);
			var response = await httpClient.PutAsync("device/setname", new StringContent(requestAsJson, Encoding.UTF8, "application/json"));

			return response.IsSuccessStatusCode ? true : false;
		}

        /// <summary>
        /// Updates the room assignment for a device based on the provided request model.
        /// </summary>
        /// <remarks>This method sends an HTTP PUT request to update the room assignment for a device.
        /// Ensure that the caller is authorized before invoking this method.</remarks>
        /// <param name="requestModel">The request model containing the device identifier and the new room assignment. This parameter cannot be
        /// null.</param>
        /// <returns><see langword="true"/> if the room assignment was successfully updated; otherwise, <see langword="false"/>.</returns>
		public async Task<bool> EditDeviceRoom(DeviceRoomRequest requestModel)
		{
			if (!await authorize()) return false;

			var requestAsJson = JsonSerializer.Serialize(requestModel);
			var response = await httpClient.PutAsync("device/setroom", new StringContent(requestAsJson, Encoding.UTF8, "application/json"));

			return response.IsSuccessStatusCode ? true : false;
		}

        /// <summary>
        /// Removes a device from the system based on the specified request.
        /// </summary>
        /// <remarks>This method requires authorization before performing the removal operation. Ensure
        /// that the provided <paramref name="request"/> contains all necessary fields. The operation sends a POST
        /// request to the "device/removeDevice" endpoint.</remarks>
        /// <param name="request">The request containing the details of the device to be removed.  This must include valid device
        /// identification information.</param>
        /// <returns><see langword="true"/> if the device was successfully removed; otherwise, <see langword="false"/>.</returns>
		public async Task<bool> DeleteDevice(DeviceRemoveRequest request)
		{
			if (!await authorize()) return false;

			var requestAsJson = JsonSerializer.Serialize(request);
			var response = await httpClient.PostAsync("device/removedevice", new StringContent(requestAsJson, Encoding.UTF8, "application/json"));

			return response.IsSuccessStatusCode;
		}


        /// <summary>
        /// Updates the configuration settings for a specified device.
        /// </summary>
        /// <remarks>This method sends a request to update the device configuration using the provided
        /// settings. The caller must ensure that the device ID is valid and the configuration dictionary contains
        /// appropriate key-value pairs. Authorization is required to perform this operation.</remarks>
        /// <param name="deviceId">The unique identifier of the device whose configuration is being updated. Cannot be null or empty.</param>
        /// <param name="config">A dictionary containing the configuration settings to apply to the device. Keys represent configuration
        /// names, and values represent their corresponding settings.</param>
        /// <returns><see langword="true"/> if the configuration update was successful; otherwise, <see langword="false"/>.</returns>
		public async Task<bool> UpdateDeviceConfiguration(string deviceId, Dictionary<string, object> config)
		{
			if (!await authorize()) return false;

			// Create the request body using the correct model
			var requestBody = new DeviceSetConfigRequest
			{
				Id = deviceId,
				Config = config
			};

			var jsonContent = JsonSerializer.Serialize(requestBody);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

			var response = await httpClient.PostAsync("device/setconfiguration", content);
			return response.IsSuccessStatusCode;
		}

		/// <summary>
		/// Retrieves data for a specific device asynchronously.
		/// </summary>
		/// <remarks>This method performs an authorization check before attempting to retrieve device
		/// data. If the authorization fails, the method returns an empty list. The method also handles unsuccessful
		/// HTTP responses gracefully by returning an empty list.</remarks>
		/// <param name="id">The unique identifier of the device for which data is to be retrieved. Cannot be null or empty.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
		/// cref="DeviceDataResponse"/> objects representing the data for the specified device. Returns an empty list if
		/// the operation is unauthorized, the request fails, or no data is available.</returns>
		public async Task<List<DeviceDataResponse>> GetDeviceData(string id)
        {
            if (!await authorize()) return new List<DeviceDataResponse>();

            var response = await httpClient.GetAsync($"device/getdata?deviceid={id}");
            if (!response.IsSuccessStatusCode) return new List<DeviceDataResponse>();

            var result = JsonSerializer.Deserialize<List<DeviceDataResponse>>(await response.Content.ReadAsStringAsync(), JsonOptions);
            return result ?? new List<DeviceDataResponse>();
        }

        /// <summary>
        /// Attempts to authorize the user by retrieving a JWT token from local or session storage.
        /// </summary>
        /// <remarks>This method first checks for a JWT token in local storage. If the token is not found
        /// or is invalid, it attempts to retrieve the token from session storage. If a valid token is found, it
        /// configures the HTTP client's authorization header with the token.</remarks>
        /// <returns><see langword="true"/> if a valid JWT token is successfully retrieved and the authorization header is
        /// configured; otherwise, <see langword="false"/>.</returns>
        private async Task<bool> authorize()
        {
            var jwtToken = await localStorage.GetItemAsync<string>("JwtToken");

            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                // If unsuccessfull, tries to obtain the jwt token from Session Storage.
                jwtToken = await sessionStorage.GetItemAsync<string>("JwtToken");
                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return false;
                }
            }

            // Configures the authenticationHeader with the users JWT token.
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwtToken);
            return true;
        }
		#endregion
	}
}