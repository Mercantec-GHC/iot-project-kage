using Blazored.LocalStorage;
using Blazored.SessionStorage;
using IotProject.Shared.Models.Responses;
using System.Net.Http.Headers;
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
            var response = await httpClient.GetAsync("Room/get-all");
            if (!response.IsSuccessStatusCode) return new List<RoomGetResponse>();

            var roomResult = JsonSerializer.Deserialize<List<RoomGetResponse>>(await response.Content.ReadAsStringAsync(), JsonOptions);
            
            return roomResult!;
        }
    }
}
