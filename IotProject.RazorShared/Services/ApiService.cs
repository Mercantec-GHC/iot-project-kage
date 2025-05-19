
using System.Net.Http.Headers;
using System.Net.Http;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using System.Text.Json;
using IotProject.Shared.Models.Requests;
using System.Text;
using IotProject.Shared.Models.Responses;

namespace IotProject.RazorShared.Services
{
    public  class ApiService(HttpClient httpClient, ILocalStorageService localStorage, ISessionStorageService sessionStorage)
    {
        // Predefines the JsonOptions, to be used by various functions.
        private readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public async Task<string?> RegisterDevice(DeviceRegisterRequest request)
        {
            if (!await authorize()) return null;

            var requestBody = JsonSerializer.Serialize(request);
            var response = await httpClient.PostAsync("device/register", new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadAsStringAsync();

            return result;
        }

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
    }
}
