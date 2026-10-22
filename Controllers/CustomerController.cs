using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BTL_PTTKYC.Data;
using BTL_PTTKYC.Models;

namespace BTL_PTTKYC.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CustomerRepository _customerRepo;
        private readonly OrderRepository _orderRepo;

        public CustomerController(CustomerRepository customerRepo, OrderRepository orderRepo)
        {
            _customerRepo = customerRepo;
            _orderRepo = orderRepo;
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Customer"))
            {
                return RedirectToAction("Profile");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new CustomerRegisterViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(CustomerRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_customerRepo.RegisterCustomer(model, out string otpCode, out string errorMessage))
            {
                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng nhập mã OTP để xác thực Số điện thoại và Gmail của bạn.";
                // Cung cấp mã OTP mô phỏng để người dùng trải nghiệm ngay lập tức trong bài tập lớn
                TempData["SimulatedOtp"] = otpCode;
                TempData["TargetPhone"] = model.Phone;
                TempData["TargetEmail"] = model.Email;

                return RedirectToAction("VerifyOtp", new { target = model.Phone, returnUrl = model.ReturnUrl });
            }

            ModelState.AddModelError(string.Empty, errorMessage);
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyOtp(string? target, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(target))
            {
                target = TempData["TargetPhone"]?.ToString() ?? TempData["TargetEmail"]?.ToString() ?? "";
            }

            if (string.IsNullOrEmpty(target))
            {
                return RedirectToAction("Register");
            }

            var vm = new CustomerVerifyOtpViewModel
            {
                PhoneOrEmail = target,
                SimulatedOtp = TempData["SimulatedOtp"]?.ToString(),
                ReturnUrl = returnUrl
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(CustomerVerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_customerRepo.VerifyOtp(model.PhoneOrEmail, model.OtpCode, out CustomerModel? customer, out string errorMessage))
            {
                if (customer != null)
                {
                    // Tự động đăng nhập cho khách hàng
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
                        new Claim(ClaimTypes.Name, customer.CustomerName),
                        new Claim(ClaimTypes.Role, "Customer"),
                        new Claim(ClaimTypes.MobilePhone, customer.Phone ?? string.Empty),
                        new Claim(ClaimTypes.Email, customer.Email ?? string.Empty)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProperties);

                    TempData["SuccessMessage"] = $"Chúc mừng {customer.CustomerName}! Tài khoản của bạn đã được xác thực SĐT và Gmail thành công.";

                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Profile");
                }
            }

            ModelState.AddModelError(string.Empty, errorMessage);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResendOtp(string target, string? returnUrl = null)
        {
            if (_customerRepo.ResendOtp(target, out string newOtp, out string errorMessage))
            {
                TempData["SuccessMessage"] = "Mã xác thực OTP mới đã được gửi thành công.";
                TempData["SimulatedOtp"] = newOtp;
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("VerifyOtp", new { target = target, returnUrl = returnUrl });
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Customer"))
            {
                return RedirectToAction("Profile");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new CustomerLoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(CustomerLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var customer = _customerRepo.Authenticate(model.Identifier, model.Password, out string errorMessage);
            if (customer == null)
            {
                if (errorMessage.Contains("chưa hoàn tất xác thực OTP"))
                {
                    TempData["WarningMessage"] = errorMessage;
                    return RedirectToAction("VerifyOtp", new { target = model.Identifier, returnUrl = model.ReturnUrl });
                }

                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
                new Claim(ClaimTypes.Name, customer.CustomerName),
                new Claim(ClaimTypes.Role, "Customer"),
                new Claim(ClaimTypes.MobilePhone, customer.Phone ?? string.Empty),
                new Claim(ClaimTypes.Email, customer.Email ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Profile");
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public IActionResult Profile()
        {
            var customerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(customerIdClaim, out int customerId))
            {
                return RedirectToAction("Login");
            }

            var customer = _customerRepo.GetCustomerById(customerId);
            if (customer == null)
            {
                return RedirectToAction("Login");
            }

            var orders = _orderRepo.GetOrdersByCustomerId(customerId);
            var purchased = _customerRepo.GetPurchasedProducts(customerId);

            var vm = new CustomerProfileViewModel
            {
                Customer = customer,
                Orders = orders,
                PurchasedProducts = purchased
            };

            return View(vm);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public IActionResult Orders()
        {
            var customerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(customerIdClaim, out int customerId))
            {
                return RedirectToAction("Login");
            }

            var orders = _orderRepo.GetOrdersByCustomerId(customerId);
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
