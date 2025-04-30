using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;   // Foreign key için gerekli
using Microsoft.AspNetCore.Identity; 

namespace MemoriseApp.Models
{
    public class Word
    {

        [Key] //Primary key
        public int WordID { get; set; }

        [Required]
        [MaxLength(100)]
        public string EngWordName { get; set; } = string.Empty; 

        [Required]
        [MaxLength(150)]
        public string TurWordName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? PicturePath { get; set; } // Resim yolu (nullable)

        [MaxLength(500)]
        public string? SoundPath { get; set; } // Ses yolu (nullable)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Oluşturulma tarihi  


        // İlişkiler  

        public virtual ICollection<WordSample>? WordSamples { get; set; } = new List<WordSample>(); // Kelime örnekleriyle olan ilişki (bir kelimenin birden fazla örneği olabilir)
        public virtual ICollection<UserWordProgress>? UserWordProgresses { get; set; } = new List<UserWordProgress>(); // Kullanıcı kelime ilerlemeleriyle olan ilişki (bir kelimenin birden fazla kullanıcıyla ilişkisi olabilir)

    }
}
