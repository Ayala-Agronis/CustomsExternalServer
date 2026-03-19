
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
            // קודם ננסה מה־Environment (מומלץ ב־Azure), ואם אין – ניקח מה־Web.config
            key = Environment.GetEnvironmentVariable("JwtSecretKey")
                  ?? ConfigurationManager.AppSettings["JwtSecretKey"];

            issuer = Environment.GetEnvironmentVariable("JwtIssuer")
                     ?? ConfigurationManager.AppSettings["JwtIssuer"]
                     ?? "http://localhost" +
                     " " +
                     "" +
                     "" +
                     "/";
        }


        [Authorize]
        public IHttpActionResult GetUsers()



        {
            var users = db.Registration.ToList();
            return Ok(users);
        }




        [HttpPost]
        public IHttpActionResult Post(Registration registration)
        {
            // בדיקת תקינות ת"ז
            if (!IdValidator.IsValidId(registration.Id))
            {
                return BadRequest(".תעודת זהות אינה תקינה");
            }

            // בדיקה אם כבר קיים משתמש עם אותו מייל/ת"ז/טלפון
            bool emailExists = db.Registration.Any(r => r.Email == registration.Email);
            bool idExists = db.Registration.Any(r => r.Id == registration.Id);
            bool phoneExists = db.Registration.Any(r => r.Mobile == registration.Mobile);

            if (emailExists && idExists && phoneExists)
            {
                return BadRequest("משתמש עם כתובת מייל, תעודת זהות ומספר טלפון אלו כבר קיים במערכת.");
            }
            else if (emailExists)
            {
                return BadRequest("כתובת מייל זו כבר רשומה במערכת.");
            }
            else if (idExists)
            {
                return BadRequest("תעודת זהות זו כבר רשומה במערכת.");
            }
            else if (phoneExists)
            {
                return BadRequest("מספר טלפון זה כבר רשום במערכת.");
            }

            // שליחת מייל
            bool emailSent = SendEmailToUser(registration);
            if (!emailSent)
            {
                return BadRequest("שליחת מייל אישור נכשלה.");
            }

            registration.AllowPromotion = false;
            db.Registration.Add(registration);
            db.SaveChanges();

            return Ok(registration);
        }


        [Authorize]
        [HttpPut]
        public IHttpActionResult Put(int id, Registration user)
        {
            var oldValue = db.Registration.Find(id);

            if (oldValue == null)
            {
                return NotFound();
            }

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

            // בדיקה אם כתובת המייל כבר בשימוש על ידי משתמש אחר
            if (db.Registration.Any(r => r.Email == user.Email && r.RowId != id))
            {
                return BadRequest("כתובת מייל זו כבר רשומה למשתמש אחר.");
            }

            // בדיקה אם תעודת הזהות כבר בשימוש על ידי משתמש אחר
            if (db.Registration.Any(r => r.Id == user.Id && r.RowId != id))
            {
                return BadRequest("תעודת זהות זו כבר רשומה למשתמש אחר.");
            }

            // בדיקה אם מספר הטלפון כבר בשימוש על ידי משתמש אחר
            if (db.Registration.Any(r => r.Mobile == user.Mobile && r.RowId != id))
            {
                return BadRequest("מספר טלפון זה כבר רשום למשתמש אחר.");
            }

            db.Entry(oldValue).CurrentValues.SetValues(user);

            try
            {
                db.SaveChanges();
                changeLogService.LogChange(changeLog);
            }
            catch
            {
                return BadRequest("אירעה שגיאה בעת שמירת נתוני המשתמש.");
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
                            expires: DateTime.Now.AddDays(7),
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

        [Authorize]
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
                    Credentials = new NetworkCredential("reg@agronis.com", "sowrwjjumfponoqf"),
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
                    From = new MailAddress("reg@agronis.com", "CustomsIL"),
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
            //הוספתי
            System.Diagnostics.Debug.WriteLine("DECODED EMAIL: " + decodedEmail);

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

        //[HttpPost]
        //[Route("api/User/forgot-password")]
        //public IHttpActionResult ForgotPassword(ForgotPasswordRequest req)
        //{
        //    if (req == null || string.IsNullOrWhiteSpace(req.Email))
        //        return BadRequest("Email is required.");

        //    var email = req.Email.Trim().ToLower();
        //    var user = db.Registration.FirstOrDefault(u => u.Email.ToLower() == email);

        //    // לא מגלים אם המשתמש קיים או לא
        //    if (user == null)
        //        return Ok(new { message = "If the email exists, a reset link was sent." });

        //    var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
        //        .Replace("+", "").Replace("/", "").Replace("=", "");

        //    user.PasswordResetToken = token;
        //    user.PasswordResetExpires = DateTime.UtcNow.AddMinutes(30);
        //    db.SaveChanges();

        //    var clientBaseUrl = ConfigurationManager.AppSettings["ClientBaseUrl"];

        //    var resetLink = $"{clientBaseUrl}/reset-password?token={HttpUtility.UrlEncode(token)}";

        //    SendResetPasswordEmail(user.Email, user.FirstName, resetLink);

        //    return Ok(new { message = "If the email exists, a reset link was sent." });
        //}

        [HttpPost]
        [Route("api/User/forgot-password")]
        public IHttpActionResult ForgotPassword(ForgotPasswordRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Email))
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    message = "יש להזין כתובת מייל תקינה."
                });
            }

            var email = req.Email.Trim().ToLower();

            var user = db.Registration.FirstOrDefault(u => u.Email.ToLower() == email);

            if (user == null)
            {
                return Content(HttpStatusCode.NotFound, new
                {
                    message = "כתובת המייל אינה קיימת במערכת."
                });
            }

            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");

            user.PasswordResetToken = token;
            user.PasswordResetExpires = DateTime.UtcNow.AddMinutes(30);

            try
            {
                db.SaveChanges();
            }
            catch
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    message = "אירעה שגיאה בלתי צפויה. נסה שוב מאוחר יותר."
                });
            }

            var clientBaseUrl = ConfigurationManager.AppSettings["ClientBaseUrl"];
            var resetLink = $"{clientBaseUrl}/reset-password?token={HttpUtility.UrlEncode(token)}";

            bool emailSent = SendResetPasswordEmail(user.Email, user.FirstName, resetLink);

            if (!emailSent)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    message = "אירעה שגיאה בלתי צפויה. נסה שוב מאוחר יותר."
                });
            }

            return Ok(new
            {
                message = "נשלחה הודעה לכתובת המייל שהוזנה."
            });
        }

        [HttpPost]
        [Route("api/User/reset-password")]
        public IHttpActionResult ResetPassword(ResetPasswordRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Token) || string.IsNullOrWhiteSpace(req.NewPassword))
                return BadRequest("Token and new password are required.");

            if (req.NewPassword.Length < 6)
                return BadRequest("Password must be at least 6 characters.");

            var token = req.Token.Trim();
            var user = db.Registration.FirstOrDefault(u => u.PasswordResetToken == token);

            if (user == null || !user.PasswordResetExpires.HasValue || user.PasswordResetExpires.Value < DateTime.UtcNow)
                return BadRequest("Invalid or expired token.");

            // כרגע אצלך זה plaintext (כמו login), אז נשאיר עקבי:
            user.Password = req.NewPassword;

            user.PasswordResetToken = null;
            user.PasswordResetExpires = null;

            db.SaveChanges();
            return Ok(new { message = "Password updated successfully." });
        }

        private bool SendResetPasswordEmail(string email, string firstName, string resetLink)
        {
            try
            {
                var client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("reg@agronis.com", "sowrwjjumfponoqf"),
                    EnableSsl = true
                };

                string projectRoot = AppDomain.CurrentDomain.BaseDirectory;
                string emailPath = Path.Combine(projectRoot, "Templates", "HtmlResetPassword.html");

                string emailBody = File.Exists(emailPath)
                    ? File.ReadAllText(emailPath)
                    : $"<p>שלום {firstName},</p><p><a href='{resetLink}'>לאיפוס סיסמה</a></p>";

                emailBody = emailBody
                    .Replace("{FirstName}", firstName ?? "")
                    .Replace("{resetLink}", resetLink);

                var message = new MailMessage
                {
                    From = new MailAddress("reg@agronis.com", "CustomsIL"),
                    Subject = "איפוס סיסמה",
                    Body = emailBody,
                    IsBodyHtml = true
                };

                message.To.Add(email);
                client.Send(message);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class ForgotPasswordRequest
        {
            public string Email { get; set; }
        }

        public class ResetPasswordRequest
        {
            public string Token { get; set; }
            public string NewPassword { get; set; }
        }
    }
}
