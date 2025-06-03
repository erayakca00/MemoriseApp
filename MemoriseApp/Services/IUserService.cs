using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using MemoriseApp.Components;
using MemoriseApp.Models; // User ve AuthResult için


namespace MemoriseApp.Services
{
    public interface IUserService
    {
        Task<AuthResult> RegisterUserAsync(string username, string email, string password);
        Task<AuthResult> LoginUserAsync(string usernameOrEmail, string password, bool rememberMe);
        Task LogoutUserAsync(); // Çıkış işlemi için
        Task<IdentityUser?> GetCurrentUserAsync(); // Mevcut kullanıcıyı almak için (isteğe bağlı, AuthenticationStateProvider da kullanılabilir)
        Task<IdentityUser?> GetUserByIdAsync(string userId); // Kullanıcıyı ID ile almak için
        Task<IdentityUser?> GetUserByUsernameAsync(string username); // Kullanıcıyı kullanıcı adı ile almak için

    }
}
