using ArchVoyage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace ArchVoyage.Controllers
{
    public class OffersController : Controller
    {
        private readonly AppDBContext _context;
        private readonly IConfiguration _configuration;

        public OffersController(AppDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return ShowFilteredOffers(new List<string>());
        }

        [HttpPost]
        public IActionResult Index(List<string> selectedRoutes)
        {
            return ShowFilteredOffers(selectedRoutes);
        }

        private IActionResult ShowFilteredOffers(List<string> selectedRoutes)
        {
            // Базовый запрос для активных анкет
            var query = _context.Forms
                .Where(f => _context.Moderation
                    .Any(m => m.UserEmail == f.UserEmail && m.Checking && !m.Blocking));

            // Если есть выбранные маршруты, фильтруем по ним
            if (selectedRoutes != null && selectedRoutes.Any())
            {
                query = query.Where(f => _context.Routes
                    .Any(r => r.UserEmail == f.UserEmail && selectedRoutes.Contains(r.RouteName)));
            }

            var userForms = query.ToList();

            var viewModel = userForms.Select(f => new UserProfileViewModel
            {
                FullName = f.FullName,
                PhotoLink = f.PhotoLink,
                RegistrationDate = f.RegistrationDate,
                City = f.City,
                Transport = f.Transport,
                Contacts = f.Contacts,
                PersonalInformation = f.PersonalInformation,
                Routes = _context.Routes
                    .Where(r => r.UserEmail == f.UserEmail)
                    .Select(r => r.RouteName)
                    .ToList()
            }).ToList();

            return View(viewModel);
        }
    }

    public class UserProfileViewModel
    {
        public string FullName { get; set; }
        public string PhotoLink { get; set; }
        public string RegistrationDate { get; set; }
        public string City { get; set; }
        public string Transport { get; set; }
        public string Contacts { get; set; }
        public string PersonalInformation { get; set; }
        public List<string> Routes { get; set; }
    }
}