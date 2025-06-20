using Microsoft.AspNetCore.Mvc;

namespace ArchVoyage.Controllers
{
    public class RoutesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
