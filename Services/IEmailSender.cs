namespace BrettGravesPortfolio.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string from, string subject, string text, CancellationToken ct);
    }
}