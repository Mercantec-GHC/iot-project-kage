using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace IotProject.Auth.Services
{
    // Definér en simpel, "dummy" authentication handler
    public class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public CustomAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Her kan du vælge at validere brugeren eller blot returnere en tom ClaimsPrincipal.
            // Hvis du allerede har din custom AuthenticationStateProvider til at håndtere brugerinfo,
            // kan du returnere en tom handler, der blot "befragter" contextet.
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
