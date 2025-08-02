using UglyToad.PdfPig;

namespace BrettGravesPortfolio.Services;

public class PdfChunker
{
    // Tries PDF first; if it fails, uses a sidecar .txt with the same name.
    public string ExtractText(string filePath)
    {
        try
        {
            using var doc = PdfDocument.Open(filePath);
            var sb = new System.Text.StringBuilder();
            foreach (var page in doc.GetPages())
            {
                sb.AppendLine(page.Text);
                sb.AppendLine();
            }
            return sb.ToString();
        }
        catch
        {
            var txt = Path.ChangeExtension(filePath, ".txt");
            return File.Exists(txt) ? File.ReadAllText(txt) : "";
        }
    }

    public IEnumerable<string> Chunk(string text, int chunkSize = 800, int overlap = 150)
    {
        var clean = (text ?? string.Empty)
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("  ", " ");

        int i = 0;
        while (i < clean.Length)
        {
            int end = Math.Min(i + chunkSize, clean.Length);
            yield return clean.Substring(i, end - i);
            if (end == clean.Length) break;
            i = Math.Max(0, end - overlap);
        }
    }
}