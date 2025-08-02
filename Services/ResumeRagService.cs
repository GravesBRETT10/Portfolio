
namespace BrettGravesPortfolio.Services;

public interface IResumeRagService
{
    Task<string> AskAsync(string question, CancellationToken ct);
}

public class ResumeRagService : IResumeRagService
{
    private readonly IEmbeddingStore _store;
    private readonly OpenAiClients _oai;

    public ResumeRagService(IEmbeddingStore store, OpenAiClients oai)
    {
        _store = store;
        _oai = oai;
    }

    public async Task<string> AskAsync(string question, CancellationToken ct)
    {
        var contexts = await _store.SearchAsync(question, topK: 6, ct);
        if (contexts.Count == 0)
        {
            return "I don't have résumé context loaded yet. Please try again after the initial ingestion finishes.";
        }
        var system = "You are Brett's portfolio assistant. Answer the question using the provided context only. If something is unclear, say you don't know and suggest contacting Brett. Keep answers concise and specific, professional but friendly.";
        var joined = string.Join("\n---\n", contexts);
        var prompt = $"Context:\n{joined}\n\nQuestion: {question}";
        var ans = await _oai.ChatAsync(system, prompt, ct);
        return ans.Trim();
    }
}
