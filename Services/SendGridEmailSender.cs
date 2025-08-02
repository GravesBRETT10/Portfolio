using SendGrid;
using SendGrid.Helpers.Mail;

namespace BrettGravesPortfolio.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly string _apiKey;

        public SendGridEmailSender(IConfiguration cfg)
        {
            _apiKey = cfg["SENDGRID_API_KEY"] ?? string.Empty;
        }

        public async Task SendAsync(string to, string from, string subject, string text, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("SENDGRID_API_KEY not configured.");

            var client = new SendGridClient(_apiKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress(from),
                Subject = subject,
                PlainTextContent = text,
                HtmlContent = $"<pre style='font-family:ui-monospace,Consolas,monospace'>{System.Net.WebUtility.HtmlEncode(text)}</pre>"
            };
            msg.AddTo(new EmailAddress(to));

            var resp = await client.SendEmailAsync(msg, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Body.ReadAsStringAsync(ct);
                throw new Exception($"SendGrid {(int)resp.StatusCode}: {body}");
            }
        }
    }
}