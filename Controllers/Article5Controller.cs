using Microsoft.AspNetCore.Mvc;

namespace ArchVoyage.Controllers
{
    public class Article5Controller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
