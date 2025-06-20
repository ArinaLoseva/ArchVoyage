using ArchVoyage.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace ArchVoyage.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly AppDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public RegistrationController(AppDBContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Users model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Ошибка валидации: {error.ErrorMessage}");
                }
                return View("Index", model);
            }

            if (!await ValidateRecaptcha(model.RecaptchaToken))
            {
                Console.WriteLine("Ошибка: reCAPTCHA не пройдена.");
                ModelState.AddModelError("", "Проверка reCAPTCHA не пройдена.");
                return View("Index", model);
            }


            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Проверка существующего пользователя
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    Console.WriteLine("Ошибка: Email уже существует.");
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View("Index", model);
                }

                // Создание пользователя
                model.PasswordHash = HashPassword(model.Password);
                _context.Users.Add(model);
                await _context.SaveChangesAsync();

                // Создание TechData
                var techData = new TechData
                {
                    UserEmail = model.Email,
                    RefreshToken = null, // Или GenerateRefreshToken() если нужно сразу создать
                    RefreshTokenExpiryTime = null
                };
                _context.TechData.Add(techData);

                // Создание Forms
                var form = new Forms
                {
                    UserEmail = model.Email,
                    FullName = "",
                    City = "",     
                    Contacts = "", 
                    RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd")
                };
                _context.Forms.Add(form);

                var moderation = new Moderation
                {
                    UserEmail = model.Email
                };
                _context.Moderation.Add(moderation);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Ошибка сохранения в БД: {ex.Message}");
                ModelState.AddModelError("", "Ошибка сохранения в БД.");
                return View("Index", model);
            }

            var token = GenerateJwtToken(model);
            SetAuthCookies(model.Email, token);

            return Redirect("/FormCreater");
        }

        public async Task<bool> ValidateRecaptcha(string token)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", "6LdrQPIqAAAAAKDqWI9ePTc38X5Kd5qf9RO1ToGN"),
                new KeyValuePair<string, string>("response", token)
            });

            try
            {
                var response = await httpClient.PostAsync(
                    "https://www.google.com/recaptcha/api/siteverify",
                    content);

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"reCAPTCHA Response: {responseBody}"); // Логируем ответ

                var recaptchaResponse = JsonConvert.DeserializeObject<RecaptchaResponse>(responseBody);

                return recaptchaResponse.Success && recaptchaResponse.Score >= 0.5;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"reCAPTCHA Error: {ex.Message}");
                return false;
            }
        }

        public class RecaptchaResponse
        {
            public bool Success { get; set; }
            public double Score { get; set; }
            public string Action { get; set; }
            public DateTime Challenge_ts { get; set; }
            public string Hostname { get; set; }
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            return Convert.ToBase64String(salt) + ":" + hash;
        }

        private string GenerateJwtToken(Users user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("UserID", user.UserID.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: creds);

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
    }

}
