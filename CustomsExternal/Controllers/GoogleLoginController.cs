using System.Web;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using System.Web.Http;
using System.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System;
using dotenv.net;
using System.IO;
using System.Xml.Linq;

namespace CustomsExternal.Controllers
{
    public class GoogleLoginController : ApiController
    {
        private CustomsExternalEntities db = new CustomsExternalEntities();

        private const string TokenUrl = "https://oauth2.googleapis.com/token";
        private string RedirectUri;

        private string ClientId;
        private string ClientSecret;

        public GoogleLoginController()
        {       
            var envFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
            DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { envFilePath }));

            ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
            ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
            RedirectUri = Environment.GetEnvironmentVariable("REDIRECT_URI");                    
        }

        public class Request
        {
            public string Code { get; set; }
        }
       
        
        [System.Web.Http.Route("api/GoogleLogin/auth")]
        [System.Web.Http.HttpPost]
        public async Task<object> GetAccessTokenAsync(Request request)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", request.Code },
                { "client_id", ClientId },
                { "client_secret", ClientSecret },
                { "redirect_uri", RedirectUri },
                { "grant_type", "authorization_code" }
            });

                var response = await client.PostAsync(TokenUrl, content);
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Response: {errorContent}");

                var responseString = await response.Content.ReadAsStringAsync();

                // קבלת ה-access token מהתשובה
                dynamic responseJson = JsonConvert.DeserializeObject(responseString);
                string f = responseJson.access_token;
                //Console.WriteLine(responseJson.access_token);
                var details = await GetUserProfileAsync(f);
                return details;
            }
        }

        [System.Web.Http.Route("api/GoogleLogin/getInfo" + "/{authorizationCode}")]
        public async Task<object> GetUserProfileAsync(string accessToken)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"https://www.googleapis.com/oauth2/v3/userinfo?access_token={accessToken}");
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic userJson = JsonConvert.DeserializeObject(responseString);

                var userProfile = new
                {
                    FirstName = (string)userJson.given_name,
                    LastName = (string)userJson.family_name,
                    Email = (string)userJson.email,
                    Mobile = userJson.phone_number != null ? (string)userJson.phone_number : null
                };

                return userProfile;
            }
        }
     
        public void LoginWithGoogle()
        {
            string clientId = ConfigurationManager.AppSettings["GoogleClientID"];
            string redirectUri = ConfigurationManager.AppSettings["RedirectUri"];
            string scope = "email profile";

            string googleOAuthUrl = $"https://accounts.google.com/o/oauth2/auth?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&scope={scope}";

            Redirect(googleOAuthUrl);
        }


        public async Task<IHttpActionResult> Callback(string code)
        {
            string clientId = ConfigurationManager.AppSettings["GoogleClientId"];
            string clientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];
            string redirectUri = ConfigurationManager.AppSettings["RedirectUri"];

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                }
            });

            var token = await flow.ExchangeCodeForTokenAsync(
                userId: "me",
                code: code,
                redirectUri: redirectUri,
                taskCancellationToken: System.Threading.CancellationToken.None
            );

            return Ok(token);
        }
    }
}


