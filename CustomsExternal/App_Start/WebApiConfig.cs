using Microsoft.Owin.Security.OAuth;
using System.Web.Http;

namespace CustomsExternal
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // ❌ הסרנו את EnableCorsAttribute – CORS מנוהל רק דרך OWIN (Startup.cs)

            // Web API configuration and services
            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
        }
    }
}
