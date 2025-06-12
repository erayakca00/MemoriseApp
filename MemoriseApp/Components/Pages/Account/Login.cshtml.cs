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

        // YENÝ: Butonun devre dýþý olup olmadýðýný kontrol etmek için
        public bool IsLoginButtonDisabled { get; set; } = false;

        public class InputModel
        {
            [Required(ErrorMessage = "Kullanýcý adý veya e-posta zorunludur.")]
            [Display(Name = "Kullanýcý Adý veya E-posta")]
            public string UserNameOrEmail { get; set; } = string.Empty;

            [Required(ErrorMessage = "Þifre zorunludur.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Beni Hatýrla?")]
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
                IsLoginButtonDisabled = true; // Butonu devre dýþý býrak (UI bunu kullanacak)
                                              // StateHasChanged() gibi bir mekanizma Razor Pages'de doðrudan yok,
                                              // sayfa yeniden render edildiðinde bu deðer okunur.

                var userName = Input.UserNameOrEmail;
                IdentityUser? user = null; // user'ý burada tanýmla

                // Kullanýcýyý bulma
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
                    _logger.LogError(ex, "Kullanýcý bulunurken hata oluþtu.");
                    ModelState.AddModelError(string.Empty, "Kullanýcý doðrulama sýrasýnda bir sorun oluþtu.");
                    IsLoginButtonDisabled = false; // Hata durumunda butonu tekrar aktif et
                    return Page();
                }


                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Kullanýcý adý veya þifre hatalý.");
                    IsLoginButtonDisabled = false; // Butonu tekrar aktif et
                    return Page();
                }

                // Giriþ denemesi
                Microsoft.AspNetCore.Identity.SignInResult result; // <<< TAM NAMESPACE KULLANILDI
                try
                {
                    result = await _signInManager.PasswordSignInAsync(userName ?? user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PasswordSignInAsync sýrasýnda hata oluþtu.");
                    ModelState.AddModelError(string.Empty, "Giriþ iþlemi sýrasýnda kritik bir hata oluþtu.");
                    IsLoginButtonDisabled = false; // Hata durumunda butonu tekrar aktif et
                    return Page();
                }


                if (result.Succeeded)
                {
                    _logger.LogInformation("Kullanýcý {UserName} giriþ yaptý.", user.UserName);
                    // IsLoginButtonDisabled = false; // Yönlendirme olacaðý için gerekmeyebilir
                    return LocalRedirect(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Kullanýcý {UserName} hesabý kilitlendi.", user.UserName);
                    return RedirectToPage("./Lockout");
                }
                else // Diðer baþarýsýzlýk durumlarý (yanlýþ þifre vb.)
                {
                    ModelState.AddModelError(string.Empty, "Kullanýcý adý veya þifre hatalý.");
                    // IsLoginButtonDisabled = false; // Zaten aþaðýdaki Page() return'ünden önce ayarlanacak
                }
            }

            // ModelState valid deðilse veya yukarýdaki bir koþul Page() döndürmediyse
            IsLoginButtonDisabled = false; // Butonu tekrar aktif et
            return Page(); // Hatalarý veya formu yeniden göster
        }
    }
}