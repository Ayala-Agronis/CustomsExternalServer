
using System.Net.Mail;
using System.Text;
using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;
using System.Net;
//using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using System.Web.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using System.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using dotenv.net;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Data.Entity;
using OpenQA.Selenium.DevTools.V129.Database;
using CustomsExternal.Services;
using System.Web;
using System.Net.Mime;
using System.Net.Http;

namespace CustomsExternal.Controllers
{
    public class UserController : ApiController
    {
        private CustomsExternalEntities db = new CustomsExternalEntities();
        private ChangeLogService changeLogService = new ChangeLogService();

        private string key;
        private string issuer;

        public UserController()
        {
            var envFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
            DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { envFilePath }));
            key = Environment.GetEnvironmentVariable("JwtKey");
            issuer = Environment.GetEnvironmentVariable("JwtIssuer");
            key = "35GadUCymdzSR6PY6SjLTpDWNS6snwZNrEvdCwfq";
            issuer = "http://localhost/";
        }

        [Authorize]
        public IHttpActionResult GetUsers()
        {
            var users = db.Registration.ToList();
            return Ok(users);
        }


        // POST api/<UserController>
        [HttpPost]
        public IHttpActionResult Post(Registration registration)
        {
            if (!IdValidator.IsValidId(registration.Id))
            {
                return BadRequest(".תעודת זהות אינה תקינה");
            }

            bool emailSent = SendEmailToUser(registration);

            if (!emailSent)
            {
                return BadRequest("Failed to send confirmation email.");
            }
            registration.AllowPromotion = false;
            db.Registration.Add(registration);
            db.SaveChanges();

            return Ok(registration);
        }

        // PUT api/<UserController>/5
        [HttpPut]
        public IHttpActionResult Put(int id, Registration user)
        {
            Registration oldValue = db.Registration.Find(id);

            string oldValueString = Newtonsoft.Json.JsonConvert.SerializeObject(oldValue);
            string newValueString = Newtonsoft.Json.JsonConvert.SerializeObject(user);

            ChangeLog changeLog = new ChangeLog
            {
                Action = "put",
                Entity = "Registration",
                Timestamp = DateTime.Now,
                OldValue = oldValueString,
                NewValue = newValueString,
                UserId = user.Id
            };

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.RowId)
            {
                return BadRequest();
            }

            //db.Entry(user).State = EntityState.Modified;
            db.Entry(oldValue).CurrentValues.SetValues(user);


            try
            {
                db.SaveChanges();
                changeLogService.LogChange(changeLog);

            }
            catch
            {
                return BadRequest("An error occurred while saving the user data.");
            }
            return Ok(user);
        }

        public Object GetToken(string userId, string email)
        {
            //    key = Environment.GetEnvironmentVariable("JwtKey");
            //    issuer = Environment.GetEnvironmentVariable("JwtIssuer");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //Create a List of Claims, Keep claims name short    
            var permClaims = new List<Claim>();
            permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            permClaims.Add(new Claim("userid", userId));
            permClaims.Add(new Claim("email", email));
            //new Claim("email", email);

            //Create Security Token object by giving required parameters    
            var token = new JwtSecurityToken(issuer, //Issure    
                            issuer,  //Audience    
                            permClaims,
                            expires: DateTime.Now.AddDays(1),
                            //expires: DateTime.Now.AddMinutes(1),
                            signingCredentials: credentials);
            var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
            //return new { data = jwt_token };
            return jwt_token;
        }

        [HttpPost]
        [Route("api/User/login")]
        public IHttpActionResult Login(LoginRequest loginRequest)
        {
            var user = db.Registration.FirstOrDefault(u => u.Email == loginRequest.Email);

            if (user == null)
            {
                return Content(HttpStatusCode.Unauthorized, new { message = " אימייל אינו קיים " });

            }

            if (user.Password != loginRequest.Password)
            {
                return Content(HttpStatusCode.Unauthorized, new { message = " ססמא שגויה" });
                //return Unauthorized(); 
            }

            if (user.AllowPromotion != true)
            {
                return Content(HttpStatusCode.Unauthorized, new { message = " יש לאשר הרשמה בהודעה שנשלחה לאמייל שהוקש " });

            }


            LoginHistory login = new LoginHistory
            {
                UserId = user.Id,
                IpAddress = GetUserIpAddress(),
                LoginTime = DateTime.Now,
            };

            db.LoginHistory.Add(login);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            var token = GetToken(user.RowId.ToString(), user.Email);
            return Ok(new
            {
                token = token,
                user = user
            });
        }

        [HttpPost]
        [Route("api/User/loginByGoogle")]
        public IHttpActionResult LoginByGoogle(LoginRequest loginRequest)
        {
            var user = db.Registration.FirstOrDefault(u => u.Email == loginRequest.Email);

            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(user);
        }


        // DELETE api/<UserController>/5
        [HttpDelete]
        public void Delete(int id)
        {
        }
        private bool SendEmailToUser(Registration registration)
        {
            try
            {
                string baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
                string encodeEmail = Convert.ToBase64String(Encoding.UTF8.GetBytes(registration.Email));
                string confirmationLink = $"{baseUrl}/api/User/ConfirmEmail?email={encodeEmail}";

                var client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("moveappdriver@gmail.com", "wnxl xcik hptq xusj"),
                    EnableSsl = true
                };

                string projectRoot = AppDomain.CurrentDomain.BaseDirectory;
                string emailBody = "";
                string emailPath = Path.Combine(projectRoot, "Templates", "HtmlEmailPage.html");

                if (File.Exists(emailPath))
                {
                    emailBody = File.ReadAllText(emailPath);
                }
                else
                {
                    Console.WriteLine(" הקובץ לא נמצא בנתיב: " + emailPath);
                }

                emailBody = emailBody.Replace("{FirstName}", registration.FirstName)
                     .Replace("{LastName}", registration.LastName)
                     .Replace("{confirmationLink}", confirmationLink);

                var message = new MailMessage
                {
                    From = new MailAddress("moveappdriver@gmail.com", "CustomsIL"),
                    Subject = "אישור הרשמה",
                    Body = emailBody,
                    IsBodyHtml = true
                };

                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "logo.png");
                LinkedResource logoImage = new LinkedResource(imagePath)
                {
                    ContentId = "logo",
                    TransferEncoding = TransferEncoding.Base64,
                    ContentType = new System.Net.Mime.ContentType("image/png")
                };

                string imagePath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "background.png");
                LinkedResource logoImage2 = new LinkedResource(imagePath2)
                {
                    ContentId = "background",
                    TransferEncoding = TransferEncoding.Base64,
                    ContentType = new System.Net.Mime.ContentType("image/png")
                };

                string imagePath3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "box.png");
                LinkedResource logoImage3 = new LinkedResource(imagePath3)
                {
                    ContentId = "box",
                    TransferEncoding = TransferEncoding.Base64,
                    ContentType = new System.Net.Mime.ContentType("image/png")
                };

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(emailBody, null, "text/html");
                htmlView.LinkedResources.Add(logoImage);
                htmlView.LinkedResources.Add(logoImage2);
                htmlView.LinkedResources.Add(logoImage3);
                message.AlternateViews.Add(htmlView);
                message.To.Add(registration.Email);
                client.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // GET api/<UserController>/ConfirmEmail
        [HttpGet]
        [Route("api/User/ConfirmEmail")]
        public IHttpActionResult ConfirmEmail(string email)
        {

            string decodedEmail = DecodeEmail(email);
            var registration = db.Registration.FirstOrDefault(r => r.Email == decodedEmail);


            if (registration == null)
            {
                return BadRequest("Invalid confirmation token.");
            }

            registration.AllowPromotion = true;

            try
            {
                db.SaveChanges();
                // Load the confirmation HTML page template
                string projectRoot = AppDomain.CurrentDomain.BaseDirectory;
                string htmlTemplatePath = Path.Combine(projectRoot, "Templates", "HtmlEmailAnswerPage.html");

                if (!File.Exists(htmlTemplatePath))
                {
                    return InternalServerError(new Exception("Confirmation page template not found."));
                }

                string htmlContent = File.ReadAllText(htmlTemplatePath);
               
                string imageUrl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "logo.png");
                htmlContent = htmlContent.Replace("{logo}", imageUrl);

                string boxImageUrl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "box.png");
                htmlContent = htmlContent.Replace("{box}", boxImageUrl);
                //AlternateView htmlView = AlternateView.CreateAlternateViewFromString(emailBody, null, "text/html");
                //htmlView.LinkedResources.Add(imageUrl);
                //htmlView.LinkedResources.Add(boxImageUrl);

                //return Content(HttpStatusCode.OK, htmlContent, "text/html");
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(htmlContent, Encoding.UTF8, "text/html")
                };

                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        private string DecodeEmail(string encodedEmail)
        {
            var bytes = Convert.FromBase64String(encodedEmail);
            return Encoding.UTF8.GetString(bytes);
        }

        private string GenerateToken(string email)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(email + DateTime.UtcNow);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash).Replace("/", "").Replace("+", "").Substring(0, 20);
            }
        }

        public string GetUserIpAddress()
        {
            string ipAddress = HttpContext.Current.Request.UserHostAddress;

            string forwardedFor = HttpContext.Current.Request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                ipAddress = forwardedFor.Split(',')[0];
            }
            if (ipAddress == "::1")
            {
                ipAddress = "127.0.0.1";
            }
            if (ipAddress != "127.0.0.1")
            {
                Console.WriteLine(ipAddress);
            }
            return ipAddress;
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
