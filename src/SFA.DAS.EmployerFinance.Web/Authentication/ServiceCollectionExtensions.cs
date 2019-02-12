using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Web.Cookies;

namespace SFA.DAS.EmployerFinance.Web.Authentication
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDasOidcAuthentication(this IServiceCollection services, IOidcConfiguration oidcConfiguration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services
                .AddAuthentication(o =>
                {
                    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    o.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(o =>
                {
                    o.AccessDeniedPath = "/Error/403"; // TODO: once in same branch as error handling, point to forbidden error page
                    o.Cookie.Name = CookieNames.Authentication;
                    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    o.SlidingExpiration = true;
                })
                .AddOpenIdConnect(o =>
                {
                    o.Authority = oidcConfiguration.Authority;
                    o.MetadataAddress = oidcConfiguration.MetadataAddress;
                    o.ResponseType = "code";
                    o.ClientId = oidcConfiguration.ClientId;
                    o.ClientSecret = oidcConfiguration.ClientSecret;
    
                    o.ClaimActions.MapUniqueJsonKey("sub", "id");
                    
                    o.Events.OnRemoteFailure = c =>
                    {
                        if (c.Failure.Message.Contains("Correlation failed"))
                        {
                            // TODO: Logging
                            
                            c.Response.Redirect("/"); // TODO: Confirm correlation failure behaviour
                            c.HandleResponse();
                        }
    
                        return Task.CompletedTask;
                    };
                });
            
            return services;
        }
    }
}