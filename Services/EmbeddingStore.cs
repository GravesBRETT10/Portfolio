
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace BrettGravesPortfolio.Services;

public interface IEmbeddingStore
{
    Task InitAsync(CancellationToken ct);
    Task<bool> HasAnyAsync(CancellationToken ct);
    Task UpsertAsync(string source, string text, float[] vector, CancellationToken ct);
    Task<IReadOnlyList<(string Text, float[] Vector)>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<string>> SearchAsync(string query, int topK, CancellationToken ct);
}

public class SqliteEmbeddingStore : IEmbeddingStore
{
    private readonly string _dbPath;
    private readonly OpenAiClients _oai;

    public SqliteEmbeddingStore(IWebHostEnvironment env, OpenAiClients oai)
    {
        _dbPath = Path.Combine(env.ContentRootPath, "App_Data", "embeddings.db");
        Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);
        _oai = oai;
    }

    private SqliteConnection GetConn() => new SqliteConnection($"Data Source={_dbPath}");

    public async Task InitAsync(CancellationToken ct)
    {
        using var conn = GetConn();
        await conn.OpenAsync(ct);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS Chunks(
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Source TEXT NOT NULL,
            Text TEXT NOT NULL,
            VectorJson TEXT NOT NULL
        );
        """;
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<bool> HasAnyAsync(CancellationToken ct)
    {
        using var conn = GetConn();
        await conn.OpenAsync(ct);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT EXISTS(SELECT 1 FROM Chunks LIMIT 1);";
        var result = (long)(await cmd.ExecuteScalarAsync(ct) ?? 0L);
        return result == 1;
    }

    public async Task UpsertAsync(string source, string text, float[] vector, CancellationToken ct)
    {
        using var conn = GetConn();
        await conn.OpenAsync(ct);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Chunks(Source, Text, VectorJson) VALUES($s,$t,$v)";
        cmd.Parameters.AddWithValue("$s", source);
        cmd.Parameters.AddWithValue("$t", text);
        cmd.Parameters.AddWithValue("$v", JsonSerializer.Serialize(vector));
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<(string Text, float[] Vector)>> GetAllAsync(CancellationToken ct)
    {
        using var conn = GetConn();
        await conn.OpenAsync(ct);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Text, VectorJson FROM Chunks;";
        using var r = await cmd.ExecuteReaderAsync(ct);
        var list = new List<(string, float[])>();
        while (await r.ReadAsync(ct))
        {
            var text = r.GetString(0);
            var vecJson = r.GetString(1);
            var vec = JsonSerializer.Deserialize<float[]>(vecJson) ?? Array.Empty<float>();
            list.Add((text, vec));
        }
        return list;
    }

    public async Task<IReadOnlyList<string>> SearchAsync(string query, int topK, CancellationToken ct)
    {
        var qVec = await _oai.EmbedAsync(query, ct);
        var all = await GetAllAsync(ct);
        var scored = all.Select(a => new { a.Text, Score = CosineSim(qVec, a.Vector) })
                        .OrderByDescending(x => x.Score)
                        .Take(topK)
                        .Select(x => x.Text)
                        .ToList();
        return scored;
    }

    private static float CosineSim(float[] a, float[] b)
    {
        if (a.Length != b.Length || a.Length == 0) return 0f;
        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }
        if (na == 0 || nb == 0) return 0f;
        return (float)(dot / (Math.Sqrt(na) * Math.Sqrt(nb)));
    }
}
