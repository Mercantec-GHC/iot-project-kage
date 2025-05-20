using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IotProject.RazorShared.Services
{
    public class CustomAuthenticationStateProvider(ILocalStorageService localStorage, ISessionStorageService sessionStorage) : AuthenticationStateProvider
    {
        // A predefined Anonymous Authentication State.
        private static AuthenticationState Anonymous => new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        /// <summary>
        /// Retrieves the Authentication State of the current user.
        /// </summary>
        /// <returns>The state of the user if they are signed in, otherwise returns an anonymous state.</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {

            // Using try / catch to handle server side rendering.
            try
            {
                // Tries to obtain a token from client local storage.
                var savedToken = await localStorage.GetItemAsync<string>("JwtToken");
                if (string.IsNullOrWhiteSpace(savedToken))
                {
                    // Tries to obtain a token from client session storage.
                    savedToken = await sessionStorage.GetItemAsync<string>("JwtToken");
                    if (string.IsNullOrWhiteSpace(savedToken))
                    {
                        // If no token exsists, return the Anonymous state.
                        return Anonymous;
                    }
                }
                // Create an Authentication State from the saved token.
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(savedToken), "jwt")));
            }
            catch
            {
                return Anonymous;
            }
        }

        /// <summary>
        /// Marks the user as signed in.
        /// </summary>
        /// <param name="token">JWT token containing the users claims.</param>
        public void MarkUserAsAuthenticated(string token)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        /// <summary>
        /// Marks the user as signed out.
        /// </summary>
        public void MarkUserAsLoggedOut()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }

        /// <summary>
        /// Parses the users claims from a JWT token.
        /// </summary>
        /// <param name="jwt"></param>
        /// <returns>A list of claims.</returns>
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;
            return jsonToken?.Claims!;
        }
    }
}
