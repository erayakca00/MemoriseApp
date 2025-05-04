using MemoriseApp.Data; // DbContext ve Modeller için
using MemoriseApp.Models; // Modeller ayrı klasördeyse
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemoriseApp.Services 
{
    public class SrsService
    {
        private readonly ApplicationDbContext _context;

        // Tekrar aralıkları (gün olarak). Seviye 0 -> 1 gün, Seviye 1 -> 7 gün vb.
        // Dokümandaki: 1 gün, 1 hafta, 1 ay, 3 ay, 6 ay, 1 yıl
        private static readonly int[] RepetitionIntervalsDays =
        {
            1,    // Seviye 0 -> Seviye 1 (1 gün sonra)
            7,    // Seviye 1 -> Seviye 2 (1 hafta sonra)
            30,   // Seviye 2 -> Seviye 3 (1 ay sonra)
            90,   // Seviye 3 -> Seviye 4 (3 ay sonra)
            180,  // Seviye 4 -> Seviye 5 (6 ay sonra)
            365   // Seviye 5 -> Seviye 6 (1 yıl sonra) - Ulaşıldığında Mastered
        };

        private const int MaxRepetitionLevel = 6; // 6. seviyeye ulaşınca Mastered

        public SrsService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Metotlar Buraya Eklenecek ---

        // --- Tekrar Zamanı Gelen Kelimeleri Getir ---
        public async Task<List<Word>> GetWordsDueForReviewAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new List<Word>(); // Boş liste döndür
            }

            DateTime today = DateTime.UtcNow.Date; // Bugünün tarihini al (saat olmadan)

            // Kullanıcının tekrar zamanı gelmiş ve henüz ustalaşmadığı kelimelerin ID'lerini bul
            var dueWordIds = await _context.UserWordProgresses
                .Where(p => p.UserId == userId &&
                            p.NextReviewDate <= today && // Tekrar tarihi bugün veya daha önce olanlar
                            p.Status != WordLearningStatus.Mastered) // Ustalaşılmamış olanlar
                .OrderBy(p => p.NextReviewDate) // En eski tekrar tarihinden başla (isteğe bağlı)
                .Select(p => p.WordID) // Sadece WordID'leri al
                .ToListAsync();

            if (!dueWordIds.Any())
            {
                return new List<Word>(); // Tekrar edilecek kelime yoksa boş liste döndür
            }

            // İlgili Word nesnelerini veritabanından çek
            // Not: İlişkili WordSamples'ı da çekmek istersen .Include(w => w.WordSamples) ekleyebilirsin
            var dueWords = await _context.Words
                .Where(w => dueWordIds.Contains(w.WordID))
                // .Include(w => w.WordSamples) // Örnek cümleleri de istersen ekle
                .ToListAsync();

            // İsteğe bağlı: Çekilen kelimeleri tekrar tarihine göre sırala (eğer yukarıdaki sıralama yeterli değilse)
            // Bu, dueWordIds listesinin sırasını kullanarak yapılabilir.
            var orderedDueWords = dueWords
                                    .OrderBy(w => dueWordIds.IndexOf(w.WordID))
                                    .ToList();

            return orderedDueWords;
        }

        // --- Kullanıcı İçin Yeni Kelimeleri Getir ---
        public async Task<List<Word>> GetNewWordsAsync(string userId, int count)
        {
            if (string.IsNullOrEmpty(userId) || count <= 0)
            {
                return new List<Word>();
            }

            // Kullanıcının zaten ilerlemesi olan tüm kelimelerin ID'lerini al
            var learnedWordIds = await _context.UserWordProgresses
                .Where(p => p.UserId == userId)
                .Select(p => p.WordID)
                .Distinct() // Tekrarları kaldır
                .ToListAsync();

            // Henüz ilerlemesi olmayan kelimeleri bul ve istenen sayıda al
            // Not: İlişkili WordSamples'ı da çekmek istersen .Include(w => w.WordSamples) ekleyebilirsin
            var newWords = await _context.Words
                .Where(w => !learnedWordIds.Contains(w.WordID)) // İlerlemesi olmayanları filtrele
                .OrderBy(w => w.WordID) // Veya rastgele sırala: .OrderBy(w => Guid.NewGuid())
                                        // .Include(w => w.WordSamples) // Örnek cümleleri de istersen ekle
                .Take(count) // İstenen sayıda al
                .ToListAsync();

            return newWords;
        }

        public async Task ProcessAnswerAsync(string userId, int wordId, bool isCorrect)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            // Kullanıcının bu kelime için ilerleme kaydını bul veya oluştur
            var progress = await _context.UserWordProgresses
                                         .FirstOrDefaultAsync(p => p.UserId == userId && p.WordID == wordId);

            // Eğer bu kelimeyle ilk kez karşılaşıyorsa, yeni bir ilerleme kaydı oluştur
            if (progress == null)
            {
                progress = new UserWordProgress
                {
                    UserId = userId,
                    WordID = wordId,
                    RepetitionLevel = 0,
                    NextReviewDate = DateTime.UtcNow.Date.AddDays(1), // Varsayılan olarak yarın sorulsun
                    Status = WordLearningStatus.New,
                    ConsecutiveCorrectAnswers = 0 // Başlangıç
                };
                _context.UserWordProgresses.Add(progress);
                // İlk karşılaşma olduğu için hemen kaydedebiliriz veya sona bırakabiliriz.
                // Şimdilik sona bırakalım, tek transaction olsun.
            }

            DateTime now = DateTime.UtcNow; // Mevcut zamanı al

            if (isCorrect)
            {
                // --- Doğru Cevap Durumu ---
                progress.ConsecutiveCorrectAnswers++; // Ardışık doğruları artır (Story 3'teki 6 kuralı için)

                // Mevcut seviyeye göre bir sonraki seviyeyi ve tekrar tarihini belirle
                if (progress.RepetitionLevel < MaxRepetitionLevel)
                {
                    int currentLevel = progress.RepetitionLevel;
                    progress.RepetitionLevel++; // Seviyeyi artır

                    // Yeni seviyeye göre tekrar aralığını al (diziden)
                    // Dizinin index'i mevcut seviyeye karşılık gelir.
                    int intervalDays = RepetitionIntervalsDays[currentLevel];
                    progress.NextReviewDate = now.Date.AddDays(intervalDays); // Sadece tarih kısmını alıp gün ekle
                    progress.Status = WordLearningStatus.Reviewing; // Tekrar moduna geçti
                }
                else // Zaten MaxRepetitionLevel'daydı, tekrar doğru bildi
                {
                    // Mastered durumuna geçebilir veya son tekrar tarihini güncelleyebiliriz.
                    // Şimdilik Mastered yapalım
                    progress.Status = WordLearningStatus.Mastered;
                    
                }
                progress.LastReviewDate = now; // Son doğru bilme tarihini güncelle
            }
            else
            {
                // --- Yanlış Cevap Durumu ---
                progress.ConsecutiveCorrectAnswers = 0; // Ardışık doğruları sıfırla

                // Seviyeyi düşür veya sıfırla. Genellikle 1 veya 0'a düşürülür.
                // Seviye 0'a düşürmek, kelimeyi tekrar öğrenme sürecine sokar.
                progress.RepetitionLevel = 0; // Seviyeyi sıfırla

                // Yanlış bilindiği için kısa süre sonra tekrar sorulsun.
                progress.NextReviewDate = now.Date.AddDays(1);
                progress.Status = WordLearningStatus.Learning; // Tekrar öğrenme moduna geçti
                // LastReviewDate güncellenmez (çünkü yanlış cevap)
            }

            // Değişiklikleri veritabanına kaydet
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error saving progress for User: {userId}, Word: {wordId}. Error: {ex.InnerException?.Message ?? ex.Message}");
                // Hata yönetimi eklenebilir (örn: tekrar deneme, loglama)
                throw; // Hatayı yukarıya fırlat
            }
        }

    }
}