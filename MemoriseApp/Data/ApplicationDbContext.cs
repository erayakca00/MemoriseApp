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

           builder.Entity<UserWordProgress>(entity =>
            {
                entity.HasOne(uwp => uwp.User) // UserWordProgress'teki 'User' navigation property'sini kullan
                      .WithMany()               // IdentityUser tarafında bir koleksiyon navigation property'si yok (veya belirtmiyoruz)
                      .HasForeignKey(uwp => uwp.UserId) // UserWordProgress'teki 'UserId' property'sini foreign key olarak kullan
                      .IsRequired();            // Bu foreign key zorunlu

                entity.HasOne(uwp => uwp.Word)
                      .WithMany(w => w.UserWordProgresses)
                      .HasForeignKey(uwp => uwp.WordID)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<WordSample>()
                .HasIndex(s => new { s.WordID, s.SampleSentence })
                .IsUnique();   
        }
    }
}