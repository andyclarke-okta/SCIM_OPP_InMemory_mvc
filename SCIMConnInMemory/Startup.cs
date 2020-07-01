using Microsoft.Owin;
using Owin;
using System.Web.Configuration;
using System.IdentityModel.Tokens;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.Jwt;

[assembly: OwinStartup(typeof(OktaSCIMConn.Startup))]

namespace OktaSCIMConn
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {


            string audience = WebConfigurationManager.AppSettings["oidc.customAuthServer.RedirectUri"];
            string authServer = WebConfigurationManager.AppSettings["oidc.AuthServer"];
            string issuer = WebConfigurationManager.AppSettings["oidc.Issuer"];



            TokenValidationParameters tvps = new TokenValidationParameters
                {
                ValidateIssuerSigningKey = false,
                ValidAudience = audience,
                ValidateAudience = true,
                ValidIssuer = issuer,
                ValidateIssuer = true,
                ValidateLifetime = true,
                ClockSkew = System.TimeSpan.FromMinutes(5)
            };

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                AccessTokenFormat = new JwtFormat(tvps,
                new OpenIdConnectCachingSecurityTokenProvider(authServer + "/.well-known/openid-configuration")),
            });


        }




    }
}