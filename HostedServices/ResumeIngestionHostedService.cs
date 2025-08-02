
using BrettGravesPortfolio.Services;

namespace BrettGravesPortfolio.HostedServices;

public class ResumeIngestionHostedService : IHostedService
{
    private readonly IServiceProvider _sp;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ResumeIngestionHostedService> _logger;

    public ResumeIngestionHostedService(IServiceProvider sp, IWebHostEnvironment env, ILogger<ResumeIngestionHostedService> logger)
    {
        _sp = sp;
        _env = env;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _sp.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IEmbeddingStore>();
        var chunker = scope.ServiceProvider.GetRequiredService<PdfChunker>();
        var oai = scope.ServiceProvider.GetRequiredService<OpenAiClients>();

        await store.InitAsync(cancellationToken);

        if (await store.HasAnyAsync(cancellationToken))
        {
            _logger.LogInformation("Embedding store already populated.");
            return;
        }

        try
        {
            var filesDir = Path.Combine(_env.WebRootPath, "files");
            var files = new[] {
                Path.Combine(filesDir, "BrettGraves.pdf"),
                Path.Combine(filesDir, "CoverLetter.pdf")
            }.Where(File.Exists).ToList();

            foreach (var f in files)
            {
                var text = chunker.ExtractText(f);
                foreach (var chunk in chunker.Chunk(text))
                {
                    var emb = await oai.EmbedAsync(chunk, cancellationToken);
                    await store.UpsertAsync(Path.GetFileName(f), chunk, emb, cancellationToken);
                }
            }

            _logger.LogInformation("Ingestion complete: {Count} files", files.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ingest résumé/cover letter context");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
