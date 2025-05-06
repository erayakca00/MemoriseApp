using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging; // Loglama için (isteğe bağlı ama önerilir)
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MemoriseApp.Services // Namespace'i projenize göre ayarlayın
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<UserService> _logger; // Loglama için

        // UserManager ve ILogger'ı dependency injection ile alıyoruz
        public UserService(UserManager<IdentityUser> userManager, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IdentityUser?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }
            // Identity'nin FindByNameAsync metodunu kullanıyoruz
            return await _userManager.FindByNameAsync(username);
        }

        public async Task<IdentityUser?> RegisterUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Boş kullanıcı adı veya şifre ile kayıt denemesi.");
                return null; // Veya ArgumentException fırlatılabilir
            }

            // Servis içinde de kullanıcı adı kontrolü yapmak iyi bir pratik olabilir
            var existingUser = await GetUserByUsernameAsync(username);
            if (existingUser != null)
            {
                _logger.LogWarning("Mevcut kullanıcı adı ile kayıt denemesi: {Username}", username);
                return null; // Kullanıcı zaten var
            }

            // Yeni IdentityUser nesnesi oluştur
            var user = new IdentityUser
            {
                UserName = username,
                // Identity genellikle bir Email bekler. Şimdilik placeholder kullanalım
                // veya kayıt formuna Email alanı ekleyelim.
                Email = $"{username}@placeholder.com",
                EmailConfirmed = true // E-posta onayını şimdilik atlayalım
            };

            // UserManager ile kullanıcıyı oluştur (şifre otomatik hash'lenir)
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Kullanıcı başarıyla kaydedildi: {Username}", username);
                return user; // Başarılıysa kullanıcı nesnesini dön
            }
            else
            {
                // Hataları logla
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Kullanıcı kaydı başarısız oldu ({Username}): {Errors}", username, errors);
                return null; // Başarısızsa null dön
            }
        }
    }
}