using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using BadApi.Data;
using BadApi.OverPosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BadApi.Account;

public class BasicAuthenticationHandler(
    Database database,
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Missing Authorization Header");
        }

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers.Authorization!);
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter??"")).Split(':');
            if (credentials.Length != 2)
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
            var username = credentials[0];
            var password = credentials[1];

            var user = await database.Users.Login(username, password);
            if (user == null)
            {
                return AuthenticateResult.Fail("Invalid Username or Password");
            }
            
            Request.HttpContext.Items["User"] = user;

            Claim[] claims = [
                new (ClaimTypes.Name, user.Name),
                .. user.Roles.Select(role => new Claim("role", role))
            ];
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization Header");
        }
    }
}

public static class RequestExtensions
{
    public static UserDetailsResponse GetUser(this HttpContext context)
    {
        if( !context.User.Identity?.IsAuthenticated ?? true )
        {
            throw new InvalidOperationException("Not logged in");
        }
        
        if (!context.Items.TryGetValue("User", out var user) || user is not UserDetailsResponse userDetails)
        {
            throw new InvalidOperationException("User not found on the HttpContext");
        }
        return userDetails;
    }
}