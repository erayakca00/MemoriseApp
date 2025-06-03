using System.ComponentModel.DataAnnotations;
using MemoriseApp.Models; // Eğer AuthResult.cs burada ise

namespace MemoriseApp.Models
{
    public class LoginModel // Veya LoginRequestModel
    {
        [Required(ErrorMessage = "Kullanıcı adı veya e-posta zorunludur.")]
        [Display(Name = "Kullanıcı Adı veya E-posta")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}