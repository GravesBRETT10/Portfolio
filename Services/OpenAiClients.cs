using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BrettGravesPortfolio.Services;

public class OpenAiClients
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _cfg;

    public OpenAiClients(IHttpClientFactory httpFactory, IConfiguration cfg)
    {
        _httpFactory = httpFactory;
        _cfg = cfg;
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct)
    {
        var (isAzure, url, key, modelOrDeployment) = GetEmbeddingEndpoint();
        var client = _httpFactory.CreateClient();
        if (isAzure)
            client.DefaultRequestHeaders.Add("api-key", key);
        else
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);

        // Use 'object' so both sides of ?: have the same static type
        object payload = isAzure
            ? new { input = text }
            : new { input = text, model = modelOrDeployment };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        using var resp = await client.SendAsync(req, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"OpenAI {(int)resp.StatusCode}: {body}");

        using var doc = JsonDocument.Parse(body);

        // OpenAI/Azure both return: data[0].embedding (array of numbers)
        var arr = doc.RootElement.GetProperty("data")[0].GetProperty("embedding").EnumerateArray();
        var list = new List<float>();
        foreach (var v in arr)
        {
            // value may be double; cast to float safely
            list.Add((float)v.GetDouble());
        }
        return list.ToArray();
    }

    public async Task<string> ChatAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        var (isAzure, url, key, modelOrDeployment) = GetChatEndpoint();
        var client = _httpFactory.CreateClient();
        if (isAzure)
            client.DefaultRequestHeaders.Add("api-key", key);
        else
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);

        object payload = isAzure
            ? new
            {
                messages = new object[] {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            }
            : new
            {
                model = modelOrDeployment,
                messages = new object[] {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.2
            };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        using var resp = await client.SendAsync(req, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"OpenAI {(int)resp.StatusCode}: {body}");

        using var doc = JsonDocument.Parse(body);
        var contentText = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return contentText ?? string.Empty;
    }

    private (bool isAzure, string url, string key, string modelOrDeployment) GetChatEndpoint()
    {
        var azureEndpoint = _cfg["AZURE_OPENAI_ENDPOINT"];
        var azureKey = _cfg["AZURE_OPENAI_API_KEY"];
        var azureDeployment = _cfg["AZURE_OPENAI_CHAT_DEPLOYMENT"];
        if (!string.IsNullOrWhiteSpace(azureEndpoint) &&
            !string.IsNullOrWhiteSpace(azureKey) &&
            !string.IsNullOrWhiteSpace(azureDeployment))
        {
            var url = $"{azureEndpoint.TrimEnd('/')}/openai/deployments/{azureDeployment}/chat/completions?api-version=2024-06-01";
            return (true, url, azureKey!, azureDeployment!);
        }

        var key = _cfg["OPENAI_API_KEY"] ?? "";
        var model = _cfg["OPENAI_MODEL"] ?? "gpt-4o-mini";
        var oaiUrl = "https://api.openai.com/v1/chat/completions";
        return (false, oaiUrl, key, model);
    }

    private (bool isAzure, string url, string key, string modelOrDeployment) GetEmbeddingEndpoint()
    {
        var azureEndpoint = _cfg["AZURE_OPENAI_ENDPOINT"];
        var azureKey = _cfg["AZURE_OPENAI_API_KEY"];
        var azureDeployment = _cfg["AZURE_OPENAI_EMBEDDING_DEPLOYMENT"];
        if (!string.IsNullOrWhiteSpace(azureEndpoint) &&
            !string.IsNullOrWhiteSpace(azureKey) &&
            !string.IsNullOrWhiteSpace(azureDeployment))
        {
            var url = $"{azureEndpoint.TrimEnd('/')}/openai/deployments/{azureDeployment}/embeddings?api-version=2024-06-01";
            return (true, url, azureKey!, azureDeployment!);
        }

        var key = _cfg["OPENAI_API_KEY"] ?? "";
        var model = _cfg["OPENAI_EMBEDDING_MODEL"] ?? "text-embedding-3-small";
        var oaiUrl = "https://api.openai.com/v1/embeddings";
        return (false, oaiUrl, key, model);
    }
}