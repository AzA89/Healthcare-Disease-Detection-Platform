using Microsoft.AspNetCore.Mvc;

namespace NewProject.Controllers
{
  public class ThemeController : Controller
  {
      [HttpPost]
      public IActionResult ToggleTheme(string theme, string returnUrl)
      {
          // Set cookie for theme preference
          Response.Cookies.Append("PreferredTheme", theme, new CookieOptions
          {
              Expires = DateTimeOffset.Now.AddYears(1),
              IsEssential = true
          });
          
          // Redirect back to the originating page
          return Redirect(returnUrl ?? "/");
      }
  }
}

