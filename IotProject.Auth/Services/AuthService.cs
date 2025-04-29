using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using IotProject.Shared.Models.Requests;
using System.Text.Json;
using System.Text;
using IotProject.Shared.Models.Responses;
using System.IdentityModel.Tokens.Jwt;

namespace IotProject.Auth.Services
{
    public class AuthService(HttpClient httpClient, ILocalStorageService localStorage, ISessionStorageService sessionStorage, AuthenticationStateProvider authenticationStateProvider)
    {
        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public async Task<bool> Login(UserLoginRequest request, bool remember = false)
        {
            var loginAsJson = JsonSerializer.Serialize(request);
            var response = await httpClient.PostAsync("auth/login", new StringContent(loginAsJson, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var loginResult = JsonSerializer.Deserialize<UserLoginResponse>(await response.Content.ReadAsStringAsync(), JsonOptions);
            if (remember)
            {
                await localStorage.SetItemAsync("JwtToken", loginResult!.Token);
                await localStorage.SetItemAsync("RefreshToken", loginResult.RefreshToken);
            }
            else
            {
                await sessionStorage.SetItemAsync("JwtToken", loginResult!.Token);
                await sessionStorage.SetItemAsync("RefreshToken", loginResult.RefreshToken);
            }
            ((CustomAuthenticationStateProvider)authenticationStateProvider).MarkUserAsAuthenticated(loginResult.Token);
            
            return true;
        }

        public async Task<bool> Refresh()
        {
            var refreshToken = await localStorage.GetItemAsync<string>("RefreshToken");
            bool remember = true;

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                refreshToken = await sessionStorage.GetItemAsync<string>("RefreshToken");
                remember = false;
                if (string.IsNullOrWhiteSpace(refreshToken)) return false;
            }

            UserRefreshRequest request = new UserRefreshRequest
            {
                Token = refreshToken,
            };
            var refreshAsJson = JsonSerializer.Serialize(request);
            var response = await httpClient.PostAsync("auth/refresh", new StringContent(refreshAsJson, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var loginResult = JsonSerializer.Deserialize<UserLoginResponse>(await response.Content.ReadAsStringAsync(), JsonOptions);
            if (remember)
            {
                await localStorage.SetItemAsync("JwtToken", loginResult!.Token);
                await localStorage.SetItemAsync("RefreshToken", loginResult.RefreshToken);
            }
            else
            {
                await sessionStorage.SetItemAsync("JwtToken", loginResult!.Token);
                await sessionStorage.SetItemAsync("RefreshToken", loginResult.RefreshToken);
            }
            ((CustomAuthenticationStateProvider)authenticationStateProvider).MarkUserAsAuthenticated(loginResult.Token);

            return true;
        }

        public async Task Logout()
        {
            await localStorage.RemoveItemAsync("JwtToken");
            await localStorage.RemoveItemAsync("RefreshToken");
            await sessionStorage.RemoveItemAsync("JwtToken");
            await sessionStorage.RemoveItemAsync("RefreshToken");
            ((CustomAuthenticationStateProvider)authenticationStateProvider).MarkUserAsLoggedOut();
        }

        public async Task<UserCreateResponse?> Register(UserCreateRequest request)
        {
            var createAsJson = JsonSerializer.Serialize(request);
            var response = await httpClient.PostAsync("auth/register", new StringContent(createAsJson, Encoding.UTF8, "application/json"));
			if (!response.IsSuccessStatusCode)
			{
                return null;
			}

            return JsonSerializer.Deserialize<UserCreateResponse>(await response.Content.ReadAsStringAsync(), JsonOptions);
		}

        /// <summary>
        /// To be used on page change. Verifies and requests new tokens, otherwise signs out the user.
        /// </summary>
        /// <returns>If the current user is signed in.</returns>
        public async Task<bool> ConfirmTokens()
        {
            // Using try / catch to handle server side rendering.
            try
            {
                var savedToken = await localStorage.GetItemAsync<string>("JwtToken");
                if (string.IsNullOrWhiteSpace(savedToken))
                {
                    savedToken = await sessionStorage.GetItemAsync<string>("JwtToken");
                    if (string.IsNullOrWhiteSpace(savedToken))
                    {
                        await Logout();
                        return false;
                    }
                }

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(savedToken) as JwtSecurityToken;
                var expiry = jsonToken?.ValidTo;

                if (expiry.HasValue && expiry.Value < DateTime.UtcNow)
                {
                    if (!await Refresh())
                    {
                        await Logout();
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
