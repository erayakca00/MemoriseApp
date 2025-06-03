using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MemoriseApp.Models;
using Microsoft.AspNetCore.Identity; // IdentityUser için

namespace MemoriseApp.Models 
{
    public enum WordLearningStatus
    {
        New,        // Henüz hiç sorulmamış veya ilk öğrenme aşamasında yanlış yapılmış
        Learning,   // İlk 6 doğru cevap aşamasında
        Reviewing,  // Tekrar aralıklarında soruluyor
        Mastered    // Tüm tekrar aralıkları başarıyla tamamlandı
    }

    public class UserWordProgress
    {
        [Key]
        public int UserWordProgressID { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Foreign Key (IdentityUser'ın Id'si string'dir)

        [Required]
        public int WordID { get; set; } // Foreign Key

        [Required]
        public int RepetitionLevel { get; set; } = 0; // 0: Yeni, 1: 1 gün, 2: 1 hafta, ..., 6: 1 yıl

        [Required]
        public int ConsecutiveCorrectAnswers { get; set; } = 0; // Story 3'teki ilk 6 ardışık doğru için

        public DateTime? LastReviewDate { get; set; } // En son ne zaman doğru bilindiği

        [Required]
        public DateTime NextReviewDate { get; set; } // Bir sonraki sorulma zamanı

        [Required]
        public WordLearningStatus Status { get; set; } = WordLearningStatus.New; // Öğrenme durumu

        // İlişkiler (Navigation Properties)
       // [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [ForeignKey("WordID")]
        public virtual Word? Word { get; set; }
    }
}