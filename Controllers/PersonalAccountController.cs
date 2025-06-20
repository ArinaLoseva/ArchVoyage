using ArchVoyage.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ArchVoyage.Controllers
{
    public class PersonalAccountController : Controller
    {
        private readonly AppDBContext _context;
        private readonly IConfiguration _configuration;

        public PersonalAccountController(AppDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Проверка и валидация JWT-токена
        private ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                return tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out _);
            }
            catch
            {
                return null;
            }
        }

        // Извлечение email из токена
        private string? GetAuthenticatedUserEmail()
        {
            var token = Request.Cookies["jwt_token"];
            if (string.IsNullOrEmpty(token))
                return null;

            var claimsPrincipal = ValidateToken(token);
            if (claimsPrincipal == null)
                return null;

            var emailClaim = claimsPrincipal.FindFirst(ClaimTypes.Email);
            return emailClaim?.Value;
        }

        public IActionResult Index()
        {
            var userEmail = GetAuthenticatedUserEmail();
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Index", "Authorization");

            var form = _context.Forms.FirstOrDefault(f => f.UserEmail == userEmail);
            if (form == null)
            {
                form = new Forms
                {
                    UserEmail = userEmail,
                    RegistrationDate = DateTime.Now.ToString("dd.MM.yyyy")
                };
                _context.Forms.Add(form);
                _context.SaveChanges();
            }

            // Получаем выбранные маршруты пользователя из таблицы Routes
            var userRoutes = _context.Routes
                .Where(r => r.UserEmail == userEmail)
                .Select(r => r.RouteName)
                .ToList();

            // Список всех возможных маршрутов
            var allRoutes = new List<string>
            {
        "Белорусская готика и неоготика",
        "Достопримечательности по дороге в Гродно",
        "Белорусское барокко",
        "50км от Минска",
        "Из Минска в Витебск",
        "7 неизвестных заброшек Беларуси"
            };

            // Передаем данные в представление
            ViewBag.UserRoutes = userRoutes;
            ViewBag.AllRoutes = allRoutes;

            return View(form);
        }

        [HttpPost]
        public IActionResult Edit([FromForm] Forms updatedForm, [FromForm] List<string> selectedRoutes)
        {
            var userEmail = GetAuthenticatedUserEmail();
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Index", "Authorization");

            // Обновление основной информации
            var existingForm = _context.Forms.FirstOrDefault(f => f.UserEmail == userEmail);
            if (existingForm == null)
                return RedirectToAction("Index", "Authorization");

            existingForm.FullName = updatedForm.FullName;
            existingForm.City = updatedForm.City;
            existingForm.Transport = updatedForm.Transport;
            existingForm.Contacts = updatedForm.Contacts;
            existingForm.PersonalInformation = updatedForm.PersonalInformation;
            existingForm.PhotoLink = updatedForm.PhotoLink;

            // Удаляем старые маршруты
            var oldRoutes = _context.Routes.Where(r => r.UserEmail == userEmail).ToList();
            _context.Routes.RemoveRange(oldRoutes);

            // Добавляем новые маршруты
            if (selectedRoutes != null && selectedRoutes.Any())
            {
                foreach (var routeName in selectedRoutes)
                {
                    _context.Routes.Add(new Routes
                    {
                        UserEmail = userEmail,
                        RouteName = routeName
                    });
                }
            }

            // Сброс статуса модерации
            var moderation = _context.Moderation.FirstOrDefault(m => m.UserEmail == userEmail);
            if (moderation != null)
            {
                moderation.Checking = false;
                moderation.Blocking = false;
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }



        // POST: /PersonalAccount/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt_token");
            Response.Cookies.Delete("refreshToken");
            return RedirectToAction("Index", "Authorization");
        }

    }
}
