using Microsoft.AspNetCore.Mvc;

namespace FEENALOoFINALE.Controllers
{
    public class LandingController : Controller
    {
        public IActionResult Index()
        {
            // If user is authenticated, redirect to dashboard
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            
            // Show the public landing page for unauthenticated users
            return View();
        }
    }
}
