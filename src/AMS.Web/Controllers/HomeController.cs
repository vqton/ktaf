using Microsoft.AspNetCore.Mvc;

namespace AMS.Web.Controllers
{
    /// <summary>
    /// Controller for the home page and dashboard.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Returns the dashboard view.
        /// </>
        public IActionResult Index()
        {
            return View();
        }
    }
}