using MemoriseApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemoriseApp.Models
{
    public class WordSample
    {
        [Key]
        public int WordSampleID { get; set; }

        [Required]
        public int WordID { get; set; } // Foreign Key

        [Required]
        public string SampleSentence { get; set; } = string.Empty; // Örnek cümle

        // İlişki (Navigation Property)
        [ForeignKey("WordID")]
        public virtual Word? Word { get; set; } // Bu örneğin ait olduğu kelime
    }
}