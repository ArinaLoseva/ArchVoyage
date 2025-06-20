using Microsoft.AspNetCore.Mvc;

namespace ArchVoyage.Controllers
{
    public class Article3Controller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
