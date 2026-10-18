using System.Text;
using TeleprompterCli.Services;
using TeleprompterCli.UI;

namespace TeleprompterCli;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        bool isTestMode = args.Any(a => a.Equals("--test", StringComparison.OrdinalIgnoreCase));
        string? filePath = null;

        var pathArg = args.FirstOrDefault(a => !a.StartsWith("--"));
        if (!string.IsNullOrWhiteSpace(pathArg))
        {
            filePath = CleanPath(pathArg);
        }

        while (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))

        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERRO] Arquivo não encontrado: {filePath}");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    TELEPROMPTER CLI (.NET 8)                       ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Informe o caminho do arquivo .txt do roteiro");
            Console.WriteLine("(Dica: você pode simplesmente arrastar o arquivo .txt para cá):");
            Console.Write("> ");

            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Nenhum caminho informado. Encerrando.");
                return;
            }

            filePath = CleanPath(input);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nCarregando roteiro: {filePath}...");
        Console.ResetColor();

        List<Models.PromptItem> items;
        try
        {
            items = TextParser.ParseFile(filePath);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[ERRO ao processar arquivo]: {ex.Message}");
            Console.ResetColor();
            return;
        }

        if (items.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[AVISO] O arquivo está vazio ou não contém texto legível.");
            Console.ResetColor();
            return;
        }

        if (isTestMode)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n=== [DIAGNÓSTICO DO ROTEIRO - MODO TESTE] ===");
            Console.ResetColor();
            Console.WriteLine($"Total de itens: {items.Count}");
            Console.WriteLine($"Marcações de cena (Cues): {items.Count(i => i.IsCue)}");
            Console.WriteLine($"Frases de fala: {items.Count(i => !i.IsCue)}");
            var totalDuration = TimeSpan.FromSeconds(items.Sum(i => i.GetDuration(130).TotalSeconds));
            Console.WriteLine($"Tempo total estimado (@ 130 PPM): {totalDuration:mm\\:ss}");
            Console.WriteLine();

            Console.WriteLine("--- Primeiros 8 itens: ---");
            for (int i = 0; i < Math.Min(8, items.Count); i++)
            {
                var it = items[i];
                var type = it.IsCue ? "[CUE] " : "[FALA]";
                var duration = it.GetDuration(130).TotalSeconds;
                Console.WriteLine($"{it.Index:D3}. {type} ({duration:F1}s | {it.WordCount} palavras): {it.Text}");
            }

            Console.WriteLine("\n--- Últimos 4 itens: ---");
            for (int i = Math.Max(0, items.Count - 4); i < items.Count; i++)
            {
                var it = items[i];
                var type = it.IsCue ? "[CUE] " : "[FALA]";
                var duration = it.GetDuration(130).TotalSeconds;
                Console.WriteLine($"{it.Index:D3}. {type} ({duration:F1}s | {it.WordCount} palavras): {it.Text}");
            }
            Console.WriteLine("\nTeste concluído com sucesso.");
            return;
        }

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // Prevent abrupt process kill
            cts.Cancel();
        };

        var engine = new TeleprompterEngine(items, filePath);
        await engine.RunAsync(cts.Token);

        TerminalHelper.ShowCursor();
        TerminalHelper.ResetColors();
        Console.WriteLine("\nTeleprompter finalizado. Até a próxima!\n");
    }

    private static string CleanPath(string raw)
    {
        var cleaned = raw.Trim();
        if ((cleaned.StartsWith('"') && cleaned.EndsWith('"')) ||
            (cleaned.StartsWith('\'') && cleaned.EndsWith('\'')))
        {
            cleaned = cleaned.Substring(1, cleaned.Length - 2);
        }
        return cleaned.Trim();
    }
}
