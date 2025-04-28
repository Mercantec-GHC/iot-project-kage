using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using IotProject.Shared.Models.Requests;
using System.Text.Json;
using System.Text;
using IotProject.Shared.Models.Responses;

namespace IotProject.Auth.Services
{
    public class AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authenticationStateProvider)
    {
        public async Task<bool> Login(UserLoginRequest request)
        {
            var loginAsJson = JsonSerializer.Serialize(request);
            var response = await httpClient.PostAsync("auth/login", new StringContent(loginAsJson, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var loginResult = JsonSerializer.Deserialize<UserLoginResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            await localStorage.SetItemAsync("JwtToken", loginResult.Token);
            await localStorage.SetItemAsync("RefreshToken", loginResult.RefreshToken);
            ((CustomAuthenticationStateProvider)authenticationStateProvider).MarkUserAsAuthenticated(loginResult.Token);
            
            return true;
        } 

        public async Task Logout()
        {
            await localStorage.RemoveItemAsync("JwtToken");
            await localStorage.RemoveItemAsync("RefreshToken");
            ((CustomAuthenticationStateProvider)authenticationStateProvider).MarkUserAsLoggedOut();
        }
    }
}
