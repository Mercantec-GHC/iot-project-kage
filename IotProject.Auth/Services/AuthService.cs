using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using IotProject.Shared.Models.Requests;
using System.Text.Json;
using System.Text;
using IotProject.Shared.Models.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;

namespace IotProject.Auth.Services
{
    public class AuthService(HttpClient httpClient, ILocalStorageService localStorage, ISessionStorageService sessionStorage, AuthenticationStateProvider authenticationStateProvider)
    {
        // Predefines the JsonOptions, to be used by various functions.
        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Attempts to login a user, using a <see cref="UserLoginRequest"/>.
        /// If <paramref name="remember"/> is set to true, the jwt token will be saved in the users Local Storage,
        /// otherwise in the Session Storage.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="remember"></param>
        /// <returns><see cref="bool"/> of true, if the user was successfully logged in.</returns>
        public async Task<bool> Login(UserLoginRequest request, bool remember = false)
        {
            // Creates and posts the login request.
            var loginAsJson = JsonSerializer.Serialize(request);
            var response = await httpClient.PostAsync("auth/login", new StringContent(loginAsJson, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            // Deserializes response as a UserLoginResponse.
            var loginResult = JsonSerializer.Deserialize<UserLoginResponse>(await response.Content.ReadAsStringAsync(), JsonOptions);
            if (remember)
            {
                // Stores the token in Local Storage.
                await localStorage.SetItemAsync("JwtToken", loginResult!.Token);
                await localStorage.SetItemAsync("RefreshToken", loginResult.RefreshToken);
            }
            else
            {
                // Stores the token in Session Storage.
                await sessionStorage.SetItemAsync("JwtToken", loginResult!.Token);
                await sessionStorage.SetItemAsync("RefreshToken", loginResult.RefreshToken);
            }
            // Calls the CustomAuthenticationStateProvider to mark the user as signed in.
            ((CustomAuthenticationStateProvider)authenticationStateProvider).MarkUserAsAuthenticated(loginResult.Token);
            
            return true;
        }

        /// <summary>
        /// Attempts to retrieve a new JWT token, using a refresh token given by the API server.
        /// </summary>
        /// <returns><see cref="bool"/> of true, if a new JWT token was retrived.</returns>
        public async Task<bool> Refresh()
        {
            // Tries to obtain the refresh token from Local Storage.
            var refreshToken = await localStorage.GetItemAsync<string>("RefreshToken");
            bool isLocalStorage = true;

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                // If unsuccessfull, tries to obtain the refresh token from Session Storage.
                refreshToken = await sessionStorage.GetItemAsync<string>("RefreshToken");
                isLocalStorage = false;
                if (string.IsNullOrWhiteSpace(refreshToken)) return false;
            }

            // Creates a new UserRefreshRequest with the given token.
            UserRefreshRequest request = new UserRefreshRequest
            {
                Token = refreshToken,
            };

            // Sends the refresh token to the API server.
            var refreshAsJson = JsonSerializer.Serialize(request);
            var response = await httpClient.PostAsync("auth/refresh", new StringContent(refreshAsJson, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            // Deserializes response as a UserLoginResponse.
            var loginResult = JsonSerializer.Deserialize<UserLoginResponse>(await response.Content.ReadAsStringAsync(), JsonOptions);
            if (isLocalStorage)
            {
                // Stores the token in Local Storage.
                await localStorage.SetItemAsync("JwtToken", loginResult!.Token);
                await localStorage.SetItemAsync("RefreshToken", loginResult.RefreshToken);
            }
            else
            {
                // Stores the token in Session Storage.
                await sessionStorage.SetItemAsync("JwtToken", loginResult!.Token);
                await sessionStorage.SetItemAsync("RefreshToken", loginResult.RefreshToken);
            }
            // Calls the CustomAuthenticationStateProvider to mark the user as signed in.
            ((CustomAuthenticationStateProvider)authenticationStateProvider).MarkUserAsAuthenticated(loginResult.Token);

            return true;
        }

        /// <summary>
        /// Requests the client to remove the stored tokens, and signs out the user.
        /// </summary>
        /// <returns></returns>
        public async Task Logout()
        {
            await localStorage.RemoveItemAsync("JwtToken");
            await localStorage.RemoveItemAsync("RefreshToken");
            await sessionStorage.RemoveItemAsync("JwtToken");
            await sessionStorage.RemoveItemAsync("RefreshToken");
            ((CustomAuthenticationStateProvider)authenticationStateProvider).MarkUserAsLoggedOut();
        }

        /// <summary>
        /// Attempts to update user password.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A <see cref="UserEditPasswordResponse"/> if successfull, otherwise null.</returns>
        public async Task<bool> EditPassword(UserEditPasswordRequest request)
        {
            // Tries to obtain the jwt token from Local Storage.
            var jwtToken = await localStorage.GetItemAsync<string>("JwtToken");
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                // If unsuccessfull, tries to obtain the jwt token from Session Storage.
                jwtToken = await sessionStorage.GetItemAsync<string>("JwtToken");
                if (string.IsNullOrWhiteSpace(jwtToken)) return false;
            }

            var createAsJson = JsonSerializer.Serialize(request);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwtToken);
            var response = await httpClient.PatchAsync("auth/updatepassword", new StringContent(createAsJson, Encoding.UTF8, "application/json"));

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Attempts to update user infomation.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A <see cref="UserEditInformationRequest"/> if successfull, otherwise null.</returns>
        public async Task<bool> EditInfo(UserEditInformationRequest request)
        {
            // Tries to obtain the jwt token from Local Storage.
            var jwtToken = await localStorage.GetItemAsync<string>("JwtToken");
            bool isLocalStorage = true;

            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                // If unsuccessfull, tries to obtain the jwt token from Session Storage.
                jwtToken = await sessionStorage.GetItemAsync<string>("JwtToken");
                isLocalStorage = false;
                if (string.IsNullOrWhiteSpace(jwtToken)) return false;
            }

            var createAsJson = JsonSerializer.Serialize(request);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwtToken);
            var response = await httpClient.PatchAsync("auth/updateinfo", new StringContent(createAsJson, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            // Deserializes response as a UserLoginResponse.
            var loginResult = JsonSerializer.Deserialize<UserLoginResponse>(await response.Content.ReadAsStringAsync(), JsonOptions);
            if (isLocalStorage)
            {
                // Stores the token in Local Storage.
                await localStorage.SetItemAsync("JwtToken", loginResult!.Token);
                await localStorage.SetItemAsync("RefreshToken", loginResult.RefreshToken);
            }
            else
            {
                // Stores the token in Session Storage.
                await sessionStorage.SetItemAsync("JwtToken", loginResult!.Token);
                await sessionStorage.SetItemAsync("RefreshToken", loginResult.RefreshToken);
            }
            // Calls the CustomAuthenticationStateProvider to mark the user as signed in.
            ((CustomAuthenticationStateProvider)authenticationStateProvider).MarkUserAsAuthenticated(loginResult.Token);

            return true;
        }

        /// <summary>
        /// Attempts to sign up a new user.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A <see cref="UserCreateResponse"/> if successfull, otherwise null.</returns>
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
        /// <returns><see cref="bool"/> of true, if the current user can stay signed in.</returns>
        public async Task<bool> ConfirmTokens()
        {
            // Using try / catch to handle server side rendering.
            try
            {
                // Tries to obtain the stored token from the client.
                var savedToken = await localStorage.GetItemAsync<string>("JwtToken");
                if (string.IsNullOrWhiteSpace(savedToken))
                {
                    savedToken = await sessionStorage.GetItemAsync<string>("JwtToken");
                    if (string.IsNullOrWhiteSpace(savedToken))
                    {
                        // If no token was found, makes sure the user is signed out.
                        await Logout();
                        return false;
                    }
                }

                // Creates a new JwtSecurityTokenHandler to validate if the JWT token has expired.
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(savedToken) as JwtSecurityToken;
                var expiry = jsonToken?.ValidTo;

                if (expiry.HasValue && expiry.Value < DateTime.UtcNow)
                {
                    // If the token has expired, tries to refresh the tokens.
                    if (!await Refresh())
                    {
                        // If not successfull, signs out the user.
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
