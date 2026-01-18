using System;
using System.Text;
using DotNetEnv;
using dotenv.net;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Owin;
using System.Web.Cors;
using System.Configuration;

namespace CustomsExternal
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 1. טען משתני סביבה
            DotEnv.Load();
            Env.Load();

            // 2. הגדר מדיניות CORS – ללא AllowAnyOrigin
            var corsPolicy = new CorsPolicy
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
                SupportsCredentials = true
            };

            corsPolicy.Origins.Add("http://localhost:4200");
            corsPolicy.Origins.Add("https://localhost:44308");
            corsPolicy.Origins.Add("http://localhost:5000");
            corsPolicy.Origins.Add("https://customsexternal20250624201845.azurewebsites.net");

            app.UseCors(new CorsOptions
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = context => System.Threading.Tasks.Task.FromResult(corsPolicy)
                }
            });

            // 3. טיפול ב־OPTIONS
            app.Use(async (context, next) =>
            {
                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.StatusCode = 200;
                    return;
                }
                await next.Invoke();
            });

            // ✅ 4. קריאת ערכים בבטחה מה־Environment או מה־Web.config
            var jwtKey = Environment.GetEnvironmentVariable("JwtSecretKey")
                         ?? ConfigurationManager.AppSettings["JwtSecretKey"];

            var jwtIssuer = Environment.GetEnvironmentVariable("JwtIssuer")
                            ?? ConfigurationManager.AppSettings["JwtIssuer"]
                            ?? "http://localhost/";

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new Exception("Missing JWT secret key (JwtSecretKey).");
            }

            // 5. הגדרת אימות JWT
            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                TokenValidationParameters = new TokenValidationParameters
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

            // 6. טיפול בשגיאות
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
                catch (Exception)
                {
                    context.Response.StatusCode = 500;
                    context.Response.ReasonPhrase = "Internal Server Error";
                }
            });
        }
    }
}
