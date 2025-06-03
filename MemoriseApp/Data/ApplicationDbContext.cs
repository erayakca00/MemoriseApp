using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MemoriseApp.Models; // Word, UserWordProgress, WordSample için gerekli namespace

namespace MemoriseApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        // Constructor - Burası doğru
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- DbSet Property'leri ---
        public DbSet<Word> Words { get; set; }
        public DbSet<UserWordProgress> UserWordProgresses { get; set; }
        public DbSet<WordSample> WordSamples { get; set; }


        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); 

            // Unique index tanımlamaları
            builder.Entity<UserWordProgress>()
                .HasIndex(p => new { p.UserId, p.WordID })
                .IsUnique(); // Kullanıcı ve kelime kombinasyonunun benzersiz olmasını sağla

            
            // Aynı kelimeye aynı örnek cümlenin tekrar eklenmesini engeller.
            builder.Entity<WordSample>()
                .HasIndex(s => new { s.WordID, s.SampleSentence })
                .IsUnique();
        }
    }
}