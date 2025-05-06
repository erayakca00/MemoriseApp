using Microsoft.AspNetCore.Identity; 
using System.Threading.Tasks;

namespace MemoriseApp.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Verilen kullanıcı adına sahip kullanıcıyı getirir. Bulamazsa null döner.
        /// </summary>
        Task<IdentityUser?> GetUserByUsernameAsync(string username);
        /// <summary>
        /// Yeni bir kullanıcı kaydeder. Başarılı olursa IdentityUser nesnesini,
        /// başarısız olursa (örn: kullanıcı adı zaten var, şifre politikası ihlali) null döner.
        /// Şifreyi otomatik olarak hash'ler.
        /// </summary>
        Task<IdentityUser?> RegisterUserAsync(string username, string password);
        // İleride eklenebilecek diğer metotlar:
        // Task<SignInResult> LoginUserAsync(string username, string password);
        // Task LogoutUserAsync();
        // Task<IdentityUser?> GetUserByIdAsync(string userId);
    
    }
}
