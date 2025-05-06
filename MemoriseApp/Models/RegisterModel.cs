using System.ComponentModel.DataAnnotations;

namespace MemoriseApp.Models 
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        
        public string Username { get; set; } = string.Empty;

        // Email alanı genellikle Identity için gereklidir.
        // İsterseniz bunu forma ekleyebilir veya şimdilik UserName'den türetebiliriz.
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter uzunluğunda olmalıdır.", MinimumLength = 6)] // Identity'nin kendi kuralları da vardır
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")] // Password property'si ile karşılaştırır
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}