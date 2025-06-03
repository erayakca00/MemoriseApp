using MemoriseApp.Models; // AuthResult için (eğer Models namespace'indeyse, değilse doğru namespace'i kullanın)
using Microsoft.AspNetCore.Components.Authorization; // AuthenticationStateProvider için
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic; // List<Claim> için
using System.Linq;
using System.Security.Claims; // ClaimTypes ve Claim için
using System.Threading.Tasks;

namespace MemoriseApp.Services
{
    public class UserService : IUserService // IUserService'inizin de RegisterUserAsync'e firstName ve lastName parametrelerini aldığından emin olun
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public UserService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<AuthResult> RegisterUserAsync(string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                return AuthResult.Failure("Kullanıcı adı boş olamaz.");
            if (string.IsNullOrWhiteSpace(email))
                return AuthResult.Failure("E-posta boş olamaz.");
            if (string.IsNullOrWhiteSpace(password))
                return AuthResult.Failure("Şifre boş olamaz.");

            var existingUserByEmail = await _userManager.FindByEmailAsync(email);
            if (existingUserByEmail != null)
            {
                return AuthResult.Failure("Bu e-posta adresi zaten kullanılıyor.");
            }

            var existingUserByName = await _userManager.FindByNameAsync(username);
            if (existingUserByName != null)
            {
                return AuthResult.Failure("Bu kullanıcı adı zaten kullanılıyor.");
            }

            var user = new IdentityUser { UserName = username, Email = email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, password);
            return AuthResult.Failure(result.Errors.Select(e => e.Description));
        }

        // UserService.cs -> LoginUserAsync
        public async Task<AuthResult> LoginUserAsync(string usernameOrEmail, string password, bool rememberMe)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usernameOrEmail))
                    return AuthResult.Failure("Kullanıcı adı veya e-posta boş olamaz.");
                if (string.IsNullOrWhiteSpace(password))
                    return AuthResult.Failure("Şifre boş olamaz.");

                IdentityUser? user = null;
                try
                {
                    Console.WriteLine($"Kullanıcı aranıyor: {usernameOrEmail}");
                    user = await _userManager.FindByNameAsync(usernameOrEmail);
                    if (user == null && usernameOrEmail.Contains("@"))
                    {
                        Console.WriteLine($"E-posta ile kullanıcı aranıyor: {usernameOrEmail}");
                        user = await _userManager.FindByEmailAsync(usernameOrEmail);
                    }
                }
                catch (Exception exUserFind)
                {
                    Console.WriteLine($"Kullanıcı bulma sırasında hata: {exUserFind.ToString()}");
                    return AuthResult.Failure("Kullanıcı doğrulamada bir sorun oluştu.");
                }


                if (user == null)
                {
                    Console.WriteLine("Kullanıcı bulunamadı.");
                    return AuthResult.Failure("Kullanıcı adı veya şifre hatalı.");
                }

                Console.WriteLine($"Kullanıcı bulundu: {user.UserName}. PasswordSignInAsync çağrılıyor...");
                SignInResult result;
                try
                {
                    result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);
                }
                catch (Exception exSignIn)
                {
                    Console.WriteLine("!!!!!!!! PASSWORD SIGN IN ASYNC İÇİNDE HATA !!!!!!!");
                    Console.WriteLine($"Hata Mesajı: {exSignIn.Message}");
                    if (exSignIn.InnerException != null)
                    {
                        Console.WriteLine($"İç Hata Mesajı: {exSignIn.InnerException.Message}");
                        Console.WriteLine($"İç Hata StackTrace: {exSignIn.InnerException.StackTrace}");
                    }
                    Console.WriteLine($"Tam Hata StackTrace: {exSignIn.StackTrace}");
                    Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    return AuthResult.Failure("Giriş işlemi sırasında kritik bir veritabanı veya yapılandırma hatası oluştu.");
                }


                Console.WriteLine($"PasswordSignInAsync sonucu: Succeeded={result.Succeeded}, IsLockedOut={result.IsLockedOut}, IsNotAllowed={result.IsNotAllowed}");

                if (result.Succeeded)
                {
                    return AuthResult.Success();
                }
                // ... (diğer result kontrolleri) ...
                return AuthResult.Failure("Kullanıcı adı veya şifre hatalı (son kontrol).");
            }
            catch (Exception exOuter) // Genel bir hata yakalayıcı
            {
                Console.WriteLine($"LoginUserAsync genel hata: {exOuter.ToString()}");
                return AuthResult.Failure("Giriş servisinde beklenmedik bir hata oluştu.");
            }
        }
        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityUser?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            return await _userManager.FindByNameAsync(username);
        }

        public async Task<IdentityUser?> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IdentityUser?> GetCurrentUserAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var userPrincipal = authState.User;

            if (userPrincipal?.Identity?.IsAuthenticated ?? false)
            {
                var userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    return await _userManager.FindByIdAsync(userId);
                }
            }
            return null;
        }
    }
}