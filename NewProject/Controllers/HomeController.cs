using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using NewProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace graduationProjects.Controllers
{
  public class HomeController : Controller
  {
      private readonly ILogger<HomeController> _logger;

      public HomeController(ILogger<HomeController> logger)
      {
          _logger = logger;
      }

      public IActionResult Index()
      {
          return View("Views/Home/Index.cshtml");
      }

      public IActionResult About()
      {
          return View();
      }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        [Route("Error/{statusCode?}")]
        public IActionResult Error(int? statusCode = null)
        {
            var error = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode ?? 500
            };

            if (statusCode == 404)
            {
                error.ErrorMessage = "The page you are looking for does not exist.";
            }
            else if (statusCode == 403)
            {
                error.ErrorMessage = "You do not have permission to access this resource.";
            }
            else
            {
                error.ErrorMessage = "An unexpected error occurred. Please try again later.";
            }

            Response.StatusCode = error.StatusCode;
            return View(error);
        }
    }

  public class ContactViewModel
  {
      public string Name { get; set; }
      public string Email { get; set; }
      public string Subject { get; set; }
      public string Message { get; set; }
      public string Phone { get; set; }
  }
}

