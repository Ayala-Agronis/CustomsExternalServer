
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

namespace CustomsExternal.Controllers
{
    public class UserController : ApiController
    {
        private CustomsExternalEntities db = new CustomsExternalEntities();
        //private readonly IConfiguration _configuration;
        private string key;
        private string issuer;

        public UserController()
        {
            DotEnv.Load();
            //key = Environment.GetEnvironmentVariable("JwtKey");
            //issuer = Environment.GetEnvironmentVariable("JwtIssuer");
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
            bool emailSent = SendEmailToUser(registration);

            if (!emailSent)
            {
                return BadRequest("Failed to send confirmation email.");
            }
            registration.AllowPromotion = false;
            db.Registration.Add(registration);
            db.SaveChanges();

            return Ok(registration);
            //return CreatedAtAction(nameof(GetById), new { id = registration.RowId }, registration.RowId);
        }

        public Object GetToken(string userId, string email)
        {
            //var key = ConfigurationManager.AppSettings["JwtKey"];

            //var issuer = ConfigurationManager.AppSettings["JwtIssuer"];

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
                            //expires: DateTime.Now.AddDays(1),
                            expires: DateTime.Now.AddMinutes(1),
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


            //return Ok(user);
            var token = GetToken(user.RowId.ToString(), user.Email);
            return Ok(new { token = token });
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

        // PUT api/<UserController>/5
        [HttpPut]
        public void Put(int id, string value)
        {
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

                var message = new MailMessage
                {
                    From = new MailAddress("moveappdriver@gmail.com", "customsIL"),
                    Subject = "אישור הרשמה",
                    Body = $"<p>שלום {registration.FirstName} {registration.LastName},</p><p>אנא אשר את הרישום שלך על ידי לחיצה על הקישור למטה:</p><a href='{confirmationLink}'>אשר את הרישום</a>",
                    IsBodyHtml = true
                };

                message.To.Add(registration.Email);
                client.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //private async Task SendEmailToUser(Registration registration)
        //{
        //    try
        //    {
        //        string baseUrl = System.Configuration.ConfigurationManager.AppSettings["BaseUrl"];
        //        string encodeEmail = Convert.ToBase64String(Encoding.UTF8.GetBytes(registration.Email));
        //        string confirmationLink = $"{baseUrl}/api/User/ConfirmEmail?email={encodeEmail}";

        //        using (var client = new SmtpClient())
        //        {
        //            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        //            await client.AuthenticateAsync("moveappdriver@gmail.com", "wnxl xcik hptq xusj");

        //            var message = new MimeMessage();
        //            message.From.Add(new MailboxAddress("YourApp", "moveappdriver@gmail.com"));
        //            message.To.Add(MailboxAddress.Parse(registration.Email));
        //            message.Subject = "Confirm Your Registration";

        //            message.Body = new TextPart("html")
        //            {
        //                Text = $"<p>שלום {registration.FirstName} {registration.LastName},</p><p>אנא אשר את הרישום שלך על ידי לחיצה על הקישור למטה:</p><a href='{confirmationLink}'>אשר את הרישום</a>"
        //            };

        //            await client.SendAsync(message);
        //            await client.DisconnectAsync(true);

        //            Console.WriteLine("Confirmation email sent successfully!");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Failed to send email: {ex.Message}");
        //    }
        //}

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
                return Ok("האימייל אושר בהצלחה. תודה!");
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

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
