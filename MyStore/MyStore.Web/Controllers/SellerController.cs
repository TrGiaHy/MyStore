using Microsoft.AspNetCore.Mvc;

namespace MyStore.Web.Controllers
{
    public class SellerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
