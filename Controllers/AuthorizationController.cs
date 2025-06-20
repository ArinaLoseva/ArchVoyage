using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ArchVoyage.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace ArchVoyage.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly AppDBContext _context;
        private readonly IConfiguration _configuration;

        public AuthorizationController(AppDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            var parts = passwordHash.Split(':');
            var salt = Convert.FromBase64String(parts[0]);
            var hash = parts[1];

            var hashedInput = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            return hashedInput == hash;
        }

        private string GenerateJwtToken(Users user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserID", user.UserID.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private void SetAuthCookies(string email, string token)
        {
            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.Now.AddDays(30),
                SameSite = SameSiteMode.Strict
            });

            Response.Cookies.Append("user_email", email, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.Now.AddDays(30),
                SameSite = SameSiteMode.Strict
            });
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }

        [HttpPost]
        public IActionResult Auth(Users user, TechData data)
        {
            var dbUser = _context.Users.SingleOrDefault(u => u.Email == user.Email);

            if (dbUser != null && VerifyPassword(user.Password, dbUser.PasswordHash))
            {
                if (dbUser.Email == "admin@gmail.com")
                {
                    return Redirect("/AdminPanel");
                }

                var jwtToken = GenerateJwtToken(dbUser);
                var refreshToken = GenerateRefreshToken();

                // Получаем или создаем TechData
                var techData = _context.TechData.FirstOrDefault(t => t.UserEmail == dbUser.Email);
                if (techData == null)
                {
                    techData = new TechData { UserEmail = dbUser.Email };
                    _context.TechData.Add(techData);
                }

                techData.RefreshToken = refreshToken;
                techData.RefreshTokenExpiryTime = Convert.ToString(DateTime.UtcNow.AddDays(1));
                _context.SaveChanges();

                SetAuthCookies(dbUser.Email, jwtToken);

                return Redirect("/PersonalAccount");
            }
            ModelState.AddModelError("", "Invalid username or password.");

            return View("Index");
        }
    }
}
