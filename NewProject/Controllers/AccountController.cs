using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewProject.Models;
using NewProject.Models.ViewModels;
using NewProject.Services;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewProject.Data;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;

namespace NewProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IEmailService emailService,
            IWebHostEnvironment environment,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailService = emailService;
            _environment = environment;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber ?? string.Empty
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new Models.ViewModels.ProfileViewModel
            {
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(Models.ViewModels.ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User updated their profile successfully.");
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) 
            {
                // Don't reveal that the user does not exist
                _logger.LogWarning($"Password reset requested for non-existent user: {model.Email}"); 
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // Generate password reset token
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Create a short, user-friendly verification code (6 digits)
            var verificationCode = GenerateVerificationCode();
            
            // Store the verification code and the actual reset token in TempData
            // In production, consider using a more secure storage like database
            TempData[$"ResetCode_{model.Email}"] = verificationCode;
            TempData[$"ResetToken_{model.Email}"] = code;
            
            // Send email with verification code
            var emailSubject = "Reset Your Password";
            var message = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
                        .header {{ background-color: #007bff; color: white; padding: 15px; text-align: center; border-radius: 5px 5px 0 0; }}
                        .code {{ font-size: 24px; font-weight: bold; text-align: center; margin: 20px 0; padding: 10px; background-color: #f8f9fa; border: 1px solid #ddd; border-radius: 5px; }}
                        .footer {{ margin-top: 20px; font-size: 12px; text-align: center; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Reset Your Password</h2>
                        </div>
                        <p>Hello,</p>
                        <p>We received a request to reset your password. Enter the verification code below to reset your password:</p>
                        <div class='code'>{verificationCode}</div>
                        <p>This code will expire in 10 minutes.</p>
                        <p>If you did not request a password reset, please ignore this email or contact support if you have concerns.</p>
                        <div class='footer'>
                            <p>This is an automated message, please do not reply to this email.</p>
                            <p>&copy; {DateTime.Now.Year} MedScan Pro. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            try
            {
                await _emailService.SendEmailAsync(model.Email, emailSubject, message);
                _logger.LogInformation($"Password reset verification code sent to {model.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending password reset email to {model.Email}");
                ModelState.AddModelError("", "Error sending email. Please try again later.");
                return View(model);
            }

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? email = null)
        {
            var model = new ResetPasswordViewModel();
            if (!string.IsNullOrEmpty(email))
            {
                model.Email = email;
            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verify the user exists
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            // Get the stored verification code and token
            var storedCode = TempData[$"ResetCode_{model.Email}"]?.ToString();
            var storedToken = TempData[$"ResetToken_{model.Email}"]?.ToString();

            // Verify the code matches
            if (string.IsNullOrEmpty(storedCode) || string.IsNullOrEmpty(storedToken) || storedCode != model.Code)
            {
                ModelState.AddModelError("Code", "Invalid verification code or code has expired.");
                return View(model);
            }

            // Reset the password using the stored token
            var result = await _userManager.ResetPasswordAsync(user, storedToken, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Password reset successful for {model.Email}");
                // Clear the stored token and code
                TempData.Remove($"ResetCode_{model.Email}");
                TempData.Remove($"ResetToken_{model.Email}");
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string? returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2faViewModel
            {
                RememberMe = rememberMe,
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View(model);
            }
        }

        private string GenerateVerificationCode()
        {
            // Generate a 6-digit code
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}