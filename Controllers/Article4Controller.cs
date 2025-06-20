using Microsoft.AspNetCore.Mvc;

namespace ArchVoyage.Controllers
{
    public class Article4Controller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
