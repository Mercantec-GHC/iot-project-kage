
using System.Net.Http.Headers;
using System.Net.Http;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using System.Text.Json;

namespace IotProject.RazorShared.Services
{
    public  class ApiService(HttpClient httpClient, ILocalStorageService localStorage, ISessionStorageService sessionStorage)
    {
        // Predefines the JsonOptions, to be used by various functions.
        private readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

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
