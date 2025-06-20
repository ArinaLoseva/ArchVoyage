using ArchVoyage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArchVoyage.Controllers
{
    public class AdminPanelController : Controller
    {
        private readonly AppDBContext _context;
        private readonly IConfiguration _configuration;

        public AdminPanelController(AppDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            // Получаем анкеты пользователей, ожидающих модерацию
            var userForms = _context.Forms
                .Where(f => _context.Moderation
                    .Any(m => m.UserEmail == f.UserEmail && m.Checking == false))
                .ToList();

            var viewModel = userForms.Select(f => new AdminPanelViewModel
            {
                UserEmail = f.UserEmail,
                FullName = f.FullName,
                PhotoLink = f.PhotoLink,
                City = f.City,
                Transport = f.Transport,
                Contacts = f.Contacts,
                PersonalInformation = f.PersonalInformation,

            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult BanUser(string email)
        {
            var moderation = _context.Moderation.FirstOrDefault(m => m.UserEmail == email);
            if (moderation != null)
            {
                moderation.Blocking = true;
                moderation.Checking = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ApproveUser(string email)
        {
            var moderation = _context.Moderation.FirstOrDefault(m => m.UserEmail == email);
            if (moderation != null)
            {
                moderation.Checking = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }

    public class AdminPanelViewModel
    {
        public string UserEmail { get; set; }
        public string FullName { get; set; }
        public string PhotoLink { get; set; }
        public string City { get; set; }
        public string Transport { get; set; }
        public string Contacts { get; set; }
        public string PersonalInformation { get; set; }
    }
}
