using System.Net.Http.Headers;
using System.Net.Http;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using IotProject.RazorShared.Models.Devices;
using IotProject.Shared.Models.Requests;
using IotProject.Shared.Models.Responses;
using IotProject.Shared.Utilities;
using System.Text;
using System.Text.Json;


namespace IotProject.RazorShared.Services
{
    public class IotDeviceService(HttpClient httpClient, ILocalStorageService localStorage, ISessionStorageService sessionStorage) 
    {
        // Predefines the JsonOptions, to be used by various functions.
        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Get a list of all the users owned devices.
        /// </summary>
        /// <returns>A <see cref="List"/> of <see cref="DeviceResponse"/>.</returns>
        public async Task<List<DeviceResponse>> GetAllDevices()
        {
            // Tries to obtain the jwt token from Local Storage.
            var jwtToken = await localStorage.GetItemAsync<string>("JwtToken");

            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                // If unsuccessfull, tries to obtain the jwt token from Session Storage.
                jwtToken = await sessionStorage.GetItemAsync<string>("JwtToken");
                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    // Return an empty list if no token is found.
                    return new List<DeviceResponse>();
                }
            }

            // Configures the authenticationHeader with the users JWT token.
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwtToken);
            var response = await httpClient.GetAsync("device/getdevices");
            if (!response.IsSuccessStatusCode)
            {
                return new List<DeviceResponse>();
            }

            // Converts the received string to a list containing DeviceResponses.
            var deviceResult = JsonSerializer.Deserialize<List<DeviceResponse>>(await response.Content.ReadAsStringAsync(), JsonOptions);

            // Returns the deserialized DeviceResult.
            return deviceResult!;
        }

        public async Task<DeviceResponse?> GetDevice()
        {
			// Tries to obtain the jwt token from Local Storage.
			var jwtToken = await localStorage.GetItemAsync<string>("JwtToken");

			if (string.IsNullOrWhiteSpace(jwtToken))
			{
				// If unsuccessfull, tries to obtain the jwt token from Session Storage.
				jwtToken = await sessionStorage.GetItemAsync<string>("JwtToken");
				if (string.IsNullOrWhiteSpace(jwtToken))
				{
                    // Return an empty list if no token is found.
                    return null;
				}
			}

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwtToken);
            var response = await httpClient.GetAsync("device/getdevice");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var deviceResult = JsonSerializer.Deserialize<DeviceResponse>(await response.Content.ReadAsStringAsync(), JsonOptions);
            return deviceResult;
		}
    }
}