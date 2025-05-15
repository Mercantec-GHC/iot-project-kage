using Blazored.LocalStorage;
using Blazored.SessionStorage;
using IotProject.Shared.Models.Requests;
using IotProject.Shared.Models.Responses;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace IotProject.Auth.Services
{
    public class RoomService(HttpClient httpClient, ILocalStorageService localStorage, ISessionStorageService sessionStorage)
    {
        // Predefines the JsonOptions, to be used by various functions.
        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public async Task<List<RoomGetResponse>> GetAll()
        {
            // Check if logged in
            var jwtToken = await localStorage.GetItemAsync<string>("JwtToken");
            if (string.IsNullOrEmpty(jwtToken))
            {
                jwtToken = await sessionStorage.GetItemAsync<string>("jwtToken");
                if (string.IsNullOrEmpty(jwtToken)) return new List<RoomGetResponse>();
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwtToken);
            var response = await httpClient.GetAsync("Room/getall");
            if (!response.IsSuccessStatusCode) return new List<RoomGetResponse>();

            var roomResult = JsonSerializer.Deserialize<List<RoomGetResponse>>(await response.Content.ReadAsStringAsync(), JsonOptions);
            
            return roomResult!;
        }

        public async Task<RoomGetResponse?> GetRoom(string id)
        {
            // Try to get the JWT token from local or session storage
            var jwtToken = await localStorage.GetItemAsync<string>("JwtToken") ?? await sessionStorage.GetItemAsync<string>("JwtToken");
            if (string.IsNullOrWhiteSpace(jwtToken)) return null;

            // Set the authorization header
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwtToken);

            // Call the API endpoint
            var response = await httpClient.GetAsync($"Room/GetRoom?id={id}");
            if (!response.IsSuccessStatusCode) return null;

            var room = JsonSerializer.Deserialize<RoomGetResponse>(await response.Content.ReadAsStringAsync(), JsonOptions);
            return room;
        }


        public async Task<bool> AddRoom(RoomCreateRequest request)
        {
            // Try to get the JWT token from local or session storage
            var jwtToken = await localStorage.GetItemAsync<string>("JwtToken") ?? await sessionStorage.GetItemAsync<string>("JwtToken");
            if (string.IsNullOrWhiteSpace(jwtToken)) return false;

            // Set the authorization header
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwtToken);

            // Serialize the request and send it to the API
            var createAsJson = JsonSerializer.Serialize(request);
            var response = await httpClient.PostAsync("Room/create", new StringContent(createAsJson, Encoding.UTF8, "application/json"));

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateRoom(RoomUpdateRequest request)
        {
            var jwtToken = await localStorage.GetItemAsync<string>("JwtToken") ?? await sessionStorage.GetItemAsync<string>("JwtToken");
            if (string.IsNullOrWhiteSpace(jwtToken)) return false;

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwtToken);
            var updateAsJson = JsonSerializer.Serialize(request);
            var response = await httpClient.PatchAsync("Room/Update", new StringContent(updateAsJson, Encoding.UTF8, "application/json"));

            return response.IsSuccessStatusCode;
        }
    }
}
