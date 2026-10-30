using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BTL_PTTKYC.Data;
using BTL_PTTKYC.Models;

namespace BTL_PTTKYC.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountRepository _accountRepo;

        public AccountController(AccountRepository accountRepo)
        {
            _accountRepo = accountRepo;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
                if (User.IsInRole("Sales")) return RedirectToAction("Orders", "Sales");
                if (User.IsInRole("Warehouse")) return RedirectToAction("Index", "Warehouse");
                if (User.IsInRole("Technician")) return RedirectToAction("Index", "Technician");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var account = _accountRepo.Authenticate(model.Username, model.Password);
            if (account == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác hoặc tài khoản đã bị khóa.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim("EmployeeName", account.EmployeeName),
                new Claim(ClaimTypes.Role, account.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            if (account.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (account.Role == "Sales")
            {
                return RedirectToAction("Orders", "Sales");
            }
            else if (account.Role == "Warehouse")
            {
                return RedirectToAction("Index", "Warehouse");
            }
            else if (account.Role == "Technician")
            {
                return RedirectToAction("Workbench", "Technician");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
