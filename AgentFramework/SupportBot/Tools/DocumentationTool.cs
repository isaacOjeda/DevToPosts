using System.ComponentModel;

namespace SupportBot.Tools;

public class DocumentationTool
{
    private readonly string _docsPath;

    public DocumentationTool(string docsPath)
    {
        _docsPath = docsPath;
    }

    [Description("Busca en la documentación interna del sistema información sobre un tema específico. Úsala siempre antes de responder una pregunta del usuario.")]
    public string GetDocumentation(
        [Description("El tema o concepto sobre el que buscar. Por ejemplo: 'accesos', 'facturación', 'reportes', 'contraseña', etc.")]
        string topic)
    {
        if (!Directory.Exists(_docsPath))
            return "No hay documentación disponible en este momento.";

        var files = Directory.GetFiles(_docsPath, "*.md");

        if (files.Length == 0)
            return "No hay documentación disponible en este momento.";

        var keywords = topic.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var matchingFiles = files.Where(file =>
        {
            var fileName = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
            var content = File.ReadAllText(file).ToLowerInvariant();
            return keywords.Any(k => fileName.Contains(k) || content.Contains(k));
        }).ToList();

        var filesToRead = matchingFiles.Count > 0 ? matchingFiles : files.ToList();

        var builder = new System.Text.StringBuilder();
        foreach (var file in filesToRead)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var content = File.ReadAllText(file);
            builder.AppendLine($"=== {fileName.ToUpperInvariant()} ===");
            builder.AppendLine(content);
            builder.AppendLine();
        }

        return builder.ToString();
    }
}
