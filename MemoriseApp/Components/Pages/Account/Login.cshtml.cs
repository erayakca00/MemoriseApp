using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MemoriseApp.Components.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        // YEN�: Butonun devre d��� olup olmad���n� kontrol etmek i�in
        public bool IsLoginButtonDisabled { get; set; } = false;

        public class InputModel
        {
            [Required(ErrorMessage = "Kullan�c� ad� veya e-posta zorunludur.")]
            [Display(Name = "Kullan�c� Ad� veya E-posta")]
            public string UserNameOrEmail { get; set; } = string.Empty;

            [Required(ErrorMessage = "�ifre zorunludur.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Beni Hat�rla?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                IsLoginButtonDisabled = true; // Butonu devre d��� b�rak (UI bunu kullanacak)
                                              // StateHasChanged() gibi bir mekanizma Razor Pages'de do�rudan yok,
                                              // sayfa yeniden render edildi�inde bu de�er okunur.

                var userName = Input.UserNameOrEmail;
                IdentityUser? user = null; // user'� burada tan�mla

                // Kullan�c�y� bulma
                try
                {
                    user = await _userManager.FindByNameAsync(Input.UserNameOrEmail);
                    if (user == null && Input.UserNameOrEmail.Contains("@"))
                    {
                        user = await _userManager.FindByEmailAsync(Input.UserNameOrEmail);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kullan�c� bulunurken hata olu�tu.");
                    ModelState.AddModelError(string.Empty, "Kullan�c� do�rulama s�ras�nda bir sorun olu�tu.");
                    IsLoginButtonDisabled = false; // Hata durumunda butonu tekrar aktif et
                    return Page();
                }


                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Kullan�c� ad� veya �ifre hatal�.");
                    IsLoginButtonDisabled = false; // Butonu tekrar aktif et
                    return Page();
                }

                // Giri� denemesi
                Microsoft.AspNetCore.Identity.SignInResult result; // <<< TAM NAMESPACE KULLANILDI
                try
                {
                    result = await _signInManager.PasswordSignInAsync(userName ?? user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PasswordSignInAsync s�ras�nda hata olu�tu.");
                    ModelState.AddModelError(string.Empty, "Giri� i�lemi s�ras�nda kritik bir hata olu�tu.");
                    IsLoginButtonDisabled = false; // Hata durumunda butonu tekrar aktif et
                    return Page();
                }


                if (result.Succeeded)
                {
                    _logger.LogInformation("Kullan�c� {UserName} giri� yapt�.", user.UserName);
                    // IsLoginButtonDisabled = false; // Y�nlendirme olaca�� i�in gerekmeyebilir
                    return LocalRedirect(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Kullan�c� {UserName} hesab� kilitlendi.", user.UserName);
                    return RedirectToPage("./Lockout");
                }
                else // Di�er ba�ar�s�zl�k durumlar� (yanl�� �ifre vb.)
                {
                    ModelState.AddModelError(string.Empty, "Kullan�c� ad� veya �ifre hatal�.");
                    // IsLoginButtonDisabled = false; // Zaten a�a��daki Page() return'�nden �nce ayarlanacak
                }
            }

            // ModelState valid de�ilse veya yukar�daki bir ko�ul Page() d�nd�rmediyse
            IsLoginButtonDisabled = false; // Butonu tekrar aktif et
            return Page(); // Hatalar� veya formu yeniden g�ster
        }
    }
}