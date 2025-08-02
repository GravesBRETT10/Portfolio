using System.Text;

namespace BrettGravesPortfolio.Services
{
    public class FileEmailSender : IEmailSender
    {
        public async Task SendAsync(string to, string from, string subject, string text, CancellationToken ct)
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "App_Data");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "contact.log");

            var lines = new[]
            {
                $"[{DateTime.UtcNow:O}] TO:{to} FROM:{from}",
                $"SUBJECT: {subject}",
                text,
                new string('-', 70)
            };
            await File.AppendAllLinesAsync(path, lines, Encoding.UTF8, ct);
        }
    }
}