using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ArchVoyage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArchVoyage.Controllers
{
    public class FormCreaterController : Controller
    {
        private readonly AppDBContext _context;

        public FormCreaterController(AppDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var email = GetUserEmailFromCookies();
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Index", "Authorization");

            var form = await _context.Forms.FirstOrDefaultAsync(f => f.UserEmail == email);
            form ??= new Forms { UserEmail = email };

            return View(form);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Forms model)
        {
            var email = GetUserEmailFromCookies();
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            if (!ModelState.IsValid)
                return View(model);

            model.UserEmail = email;

            var existingForm = await _context.Forms.FindAsync(email);
            if (existingForm != null)
            {
                model.RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd");
                _context.Entry(existingForm).CurrentValues.SetValues(model);
            }
            else
            {
                model.RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd");
                _context.Forms.Add(model);
            }

            await _context.SaveChangesAsync();
            return Redirect("/PersonalAccount");
        }

        public IActionResult Success()
        {
            return View();
        }

        private string GetUserEmailFromCookies()
        {
            // Сначала пробуем из обычных кук
            var email = Request.Cookies["user_email"];
            if (!string.IsNullOrEmpty(email))
                return email;

            // Если нет, пробуем извлечь из JWT
            var token = Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            }

            return null;
        }
    }
}