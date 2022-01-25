using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;

namespace IdentityServer.Controllers;

public class AuthorizationController : Controller
{
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Retrieve the user principal stored in the authentication cookie.
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        // If the user principal can't be extracted, redirect the user to the login page.
        if (!result.Succeeded)
        {
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        // Create a new claims principal

        var claims = new List<Claim>
            {
                // 'subject' claim which is required
                new Claim(OpenIddictConstants.Claims.Subject, result.Principal.Identity.Name),
                new Claim(OpenIddictConstants.Claims.Username, result.Principal.Identity.Name),
                new Claim(OpenIddictConstants.Claims.Audience, "IdentityServerWebClients"),
            };

        var email = result.Principal.Claims.FirstOrDefault(q => q.Type == ClaimTypes.Email);
        if (email is not null)
        {
            claims.Add(new Claim(OpenIddictConstants.Claims.Email, email.Value));
        }


        var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Set requested scopes (this is not done automatically)
        claimsPrincipal.SetScopes(request.GetScopes());

        foreach (var claim in claimsPrincipal.Claims)
        {
            claim.SetDestinations(claim.Type switch
            {
                // If the "profile" scope was granted, allow the "name" claim to be
                // added to the access and identity tokens derived from the principal.
                OpenIddictConstants.Claims.Name when claimsPrincipal.HasScope(OpenIddictConstants.Scopes.Profile) => new[]
                {
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken
                },

                // Never add the "secret_value" claim to access or identity tokens.
                // In this case, it will only be added to authorization codes,
                // refresh tokens and user/device codes, that are always encrypted.
                "secret_value" => Array.Empty<string>(),

                // Otherwise, add the claim to the access tokens only.
                _ => new[]
                {
                    OpenIddictConstants.Destinations.AccessToken
                }
            });
        }



        // Signing in with the OpenIddict authentiction scheme trigger OpenIddict to issue a code (which can be exchanged for an access token)
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        ClaimsPrincipal claimsPrincipal;

        if (request.IsClientCredentialsGrantType())
        {
            // Note: the client credentials are automatically validated by OpenIddict:
            // if client_id or client_secret are invalid, this action won't be invoked.

            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // Subject (sub) is a required field, we use the client id as the subject identifier here.
            identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? throw new InvalidOperationException());

            // Add some claim, don't forget to add destination otherwise it won't be added to the access token.
            identity.AddClaim("some-claim", "some-value", OpenIddictConstants.Destinations.AccessToken);

            claimsPrincipal = new ClaimsPrincipal(identity);

            claimsPrincipal.SetScopes(request.GetScopes());
        }
        else if (request.IsAuthorizationCodeGrantType())
        {
            // Retrieve the claims principal stored in the authorization code
            claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        }
        else if (request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the refresh token.
            claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        }
        else
        {
            throw new InvalidOperationException("The specified grant type is not supported.");
        }

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    public async Task<IActionResult> Userinfo()
    {
        var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

        return Ok(new
        {
            Sub = claimsPrincipal.GetClaim(OpenIddictConstants.Claims.Subject),
            Name = claimsPrincipal.GetClaim(OpenIddictConstants.Claims.Subject),
            Occupation = "Developer",
            Age = 31
        });
    }
}