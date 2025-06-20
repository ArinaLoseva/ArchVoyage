using Microsoft.AspNetCore.Mvc;

namespace ArchVoyage.Controllers
{
    public class Article2Controller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
