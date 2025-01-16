using dotenv.net;
using DotNetEnv;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using static System.Net.WebRequestMethods;

namespace CustomsExternal
{

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            DotEnv.Load();
            Env.Load();


            //var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            //var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
            //var jwtIssuer = Env.GetString("JWT_ISSUER");
            //var jwtKey = Env.GetString("JWT_KEY");
            var jwtKey = "35GadUCymdzSR6PY6SjLTpDWNS6snwZNrEvdCwfq";
            var jwtIssuer = "http://localhost/";

            //if (string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtKey))
            //{
            //    throw new InvalidOperationException("JWT_ISSUER or JWT_KEY is not defined in the .env file.");
            //}
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,                        
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtIssuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ClockSkew = TimeSpan.Zero
                    }
                });

            app.Use(async (context, next) =>
            {
                try
                {
                    await next.Invoke();
                }
                catch (SecurityTokenExpiredException)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ReasonPhrase = "Token Expired";
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    context.Response.ReasonPhrase = "Internal Server Error";
                }
            });
        }
    }
}