using Microsoft.AspNetCore.Mvc;

namespace ArchVoyage.Controllers
{
    public class Article1Controller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
