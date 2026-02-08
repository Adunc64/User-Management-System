using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

using task.Data;
using task.Models;
using task.Helpers;
using task.Services;


namespace task.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IEmailSender _email;

        public AccountController(AppDbContext db, IEmailSender email)
        {
            _db = db;
            _email = email;
        }

        [HttpGet] //Register page
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVm regvm)
        {
            if (!ModelState.IsValid)
            {
                return View(regvm);
            }

            var email = regvm.Email.Trim().ToLower();

            var exists = await _db.Users.AnyAsync(u => u.Email == email);
            if (exists)
            {
                ModelState.AddModelError(nameof(regvm.Email), "Email is already registered.");
                return View(regvm);
            }

            var token = TokenGenerator.CreateToken();
            var user = new User
            {
                Email = email,
                Username = regvm.Username?.Trim() ?? string.Empty,
                PasswordHash = PasswordHasher.Hash(regvm.Password),
                Status = task.Models.User.UserStatus.Unverified,
                LastLoginTime = null,

                EmailVerificationToken = token,
                EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24),
                EmailVerifiedAt = null
            };

            _db.Users.Add(user);
            try
            {
                await _db.SaveChangesAsync();
                var verifyLink = Url.Action(
                    action: "VerifyEmail",
                    controller: "Account",
                    values: new { userId = user.Id, token = token },
                    protocol: Request.Scheme
                );
                // Send mail
                await _email.SendAsync(
                    toEmail: user.Email,
                    subject: "Verify your email",
                    body: $"Click to verify: {verifyLink}"
                );
            }
            catch (DbUpdateException)
            {
                // If two users register same email at same time, unique index triggers here
                ModelState.AddModelError(nameof(regvm.Email), "This email is already registered.");
                return View(regvm);
            }

            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }

        [HttpGet] //Email verification 
        public async Task<IActionResult> VerifyEmail(int userId, string token)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return Content("Invalid verification link.");
            }

            if (user.EmailVerificationToken == null || user.EmailVerificationTokenExpiresAt == null)
            {
                return Content("Invalid verification link.");
            }

            if (user.EmailVerificationToken != token)
            {
                return Content("Invalid verification link.");
            }

            if (user.EmailVerificationTokenExpiresAt.Value < DateTime.UtcNow)
            {
                return Content("Verification link has expired.");
            }

            //for blocked user
            if (user.Status == task.Models.User.UserStatus.Blocked)
            {
                return Content("Your account is blocked. Please contact support.");
            }

            user.Status = task.Models.User.UserStatus.Active;
            user.EmailVerifiedAt = DateTime.UtcNow.ToString("o");

            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiresAt = null;
            await _db.SaveChangesAsync();


            TempData["Success"] = "Email verified successfully! You can now log in.";
            return RedirectToAction("Login");
        }

        [HttpGet] //login page
        public IActionResult Login() 
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVm loginVm)
        {
            if(!ModelState.IsValid)
            {
                return View(loginVm);
            }

            var email = loginVm.Email.Trim().ToLower();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            //user not found
            if(user == null)  
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(loginVm);
            }

            //check password
            var hash = PasswordHasher.Hash(loginVm.Password);
            if(user.PasswordHash != hash)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(loginVm);
            }

            //blocked user can't login
            if(user.Status == task.Models.User.UserStatus.Blocked)
            {
                ModelState.AddModelError("", "Your account is blocked. Please contact support.");
                return View(loginVm);
            }

            if (user.Status == task.Models.User.UserStatus.Unverified)
            {
                ModelState.AddModelError("", "Please verify your email before logging in.");
                return View(loginVm);
            }

            // Update last login time
            user.LastLoginTime = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            //create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction("Index", "Users");
        }
    
    
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

    }
}