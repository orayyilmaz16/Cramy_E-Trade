using Cramy.Domain.Entities;
using Cramy.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Cramy.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // =========================================================
        // REGISTER
        // =========================================================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // =========================================================
        // LOGIN
        // =========================================================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Geçersiz giriş denemesi");
            return View(model);
        }

        // =========================================================
        // LOGOUT
        // =========================================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(LogoutSuccess));
        }

        [HttpGet]
        public IActionResult LogoutSuccess()
        {
            return View();
        }

        // =========================================================
        // ACCESS DENIED
        // =========================================================
 
        

        // =========================================================
        // PROFILE (Detail / Edit)
        // =========================================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return RedirectToAction(nameof(Login));

            var vm = new ProfileViewModel
            {
                Email = user.Email ?? user.UserName ?? "",
                PhoneNumber = user.PhoneNumber
                // FirstName = user.FirstName,
                // LastName = user.LastName
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user is null) return RedirectToAction(nameof(Login));

            user.PhoneNumber = model.PhoneNumber;

            // Eğer ApplicationUser'da FirstName/LastName varsa açın:
            // user.FirstName = model.FirstName;
            // user.LastName = model.LastName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["ProfileSuccess"] = "Profil bilgileriniz güncellendi.";
            return RedirectToAction(nameof(Profile));
        }

        // =========================================================
        // CHANGE PASSWORD
        // =========================================================
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user is null) return RedirectToAction(nameof(Login));

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["PasswordSuccess"] = "Şifreniz başarıyla güncellendi.";
            return RedirectToAction(nameof(ChangePassword));
        }

        // =========================================================
        // DELETE ACCOUNT
        // =========================================================
        [Authorize]
        [HttpGet]
        public IActionResult DeleteAccount() => View();

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccountConfirmed()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return RedirectToAction(nameof(Login));

            await _signInManager.SignOutAsync();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["DeleteError"] = "Hesap silme işlemi başarısız oldu.";
                return RedirectToAction(nameof(DeleteAccount));
            }

            TempData["DeleteSuccess"] = "Hesabınız silindi.";
            return RedirectToAction("Index", "Home");
        }

        // =========================================================
        // FORGOT PASSWORD + RESET PASSWORD
        // =========================================================
        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Opsiyonel: Email doğrulaması kullanmıyorsanız bu kontrolü kaldırın.
            // var emailConfirmedOk = user != null && await _userManager.IsEmailConfirmedAsync(user);

            // Güvenlik: kullanıcı yoksa da aynı mesaj
            if (user == null /* || !emailConfirmedOk */)
            {
                TempData["ForgotPasswordInfo"] =
                    "Eğer bu e-posta adresi sistemimizde kayıtlıysa, şifre sıfırlama bağlantısı gönderilmiştir.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Token'ı URL-safe yap
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var safeToken = WebEncoders.Base64UrlEncode(tokenBytes);

            var resetUrl = Url.Action(
                action: nameof(ResetPassword),
                controller: "Account",
                values: new { email = user.Email, token = safeToken },
                protocol: Request.Scheme);

            // TODO: resetUrl'i e-posta ile gönderin.
            // Ör: _emailSender.SendAsync(user.Email, "Şifre Sıfırlama", $"Link: {resetUrl}");

            TempData["ForgotPasswordInfo"] =
                "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi. Lütfen gelen kutunuzu kontrol edin.";

            return RedirectToAction(nameof(ForgotPassword));
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return RedirectToAction(nameof(Login));

            var tokenBytes = WebEncoders.Base64UrlDecode(token);
            var normalToken = Encoding.UTF8.GetString(tokenBytes);

            var vm = new ResetPasswordViewModel
            {
                Email = email,
                Token = normalToken
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Güvenlik: kullanıcı yoksa da başarılı gibi davran
            if (user == null)
            {
                TempData["ResetSuccess"] = "Şifre sıfırlama işlemi tamamlandı. Artık giriş yapabilirsiniz.";
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
            {
                TempData["ResetSuccess"] = "Şifreniz güncellendi. Artık giriş yapabilirsiniz.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
    }
}
