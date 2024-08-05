using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Design;
using Redurl.Models;

namespace Redurl.Services
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<UrlModel> Urls { get; set; }
    }

    public class EFService
    {
        private readonly DataContext _context;

        public EFService(DataContext context)
        {
            _context = context;
        }

        public async Task SaveUrl(string key, string url)
        {
            var urlModel = new UrlModel
            {
                OriginalUrl = url,
                ShortenedUrl = key
            };
            _context.Urls.Add(urlModel);
             Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Database operation");
            Console.ResetColor();
            await _context.SaveChangesAsync();
        }

        public async Task<string?> GetUrl(string key)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Database operation");
            Console.ResetColor();
            var urlModel = await _context.Urls.FirstOrDefaultAsync(u => u.ShortenedUrl == key);
            return urlModel?.OriginalUrl;
        }

        public async Task<Dictionary<string, string>> GetKeysValues()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Database operation");
            Console.ResetColor();
            var urlModels = await _context.Urls.ToListAsync();
            return urlModels.ToDictionary(u => u.ShortenedUrl, u => u.OriginalUrl);
        }

        public async Task<string?> GetKey(string url)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Database operation");
            Console.ResetColor();
            var urlModel = await _context.Urls.FirstOrDefaultAsync(u => u.OriginalUrl == url);
            return urlModel?.ShortenedUrl;
        }
    }
}
