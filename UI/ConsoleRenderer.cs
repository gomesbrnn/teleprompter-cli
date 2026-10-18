using System.Text;
using TeleprompterCli.Models;

namespace TeleprompterCli.UI;

public class ConsoleRenderer
{
    private const string ANSI_RESET = "\x1b[0m";
    private const string ANSI_BOLD = "\x1b[1m";
    private const string ANSI_DIM = "\x1b[2m";

    private const string COLOR_HEADER_BG = "\x1b[48;5;236m\x1b[38;5;255m";
    private const string COLOR_CONTROLS_BG = "\x1b[48;5;235m\x1b[38;5;250m";
    private const string COLOR_GRAY = "\x1b[38;5;244m";
    private const string COLOR_DARK_GRAY = "\x1b[38;5;238m";
    private const string COLOR_BRIGHT_WHITE = "\x1b[97m";
    private const string COLOR_CYAN = "\x1b[96m";
    private const string COLOR_YELLOW = "\x1b[93m";
    private const string COLOR_GREEN_BADGE = "\x1b[42m\x1b[97m\x1b[1m";
    private const string COLOR_YELLOW_BADGE = "\x1b[43m\x1b[30m\x1b[1m";
    private const string COLOR_EYELINE_BORDER = "\x1b[38;5;39m";

    public void RenderFull(
        IReadOnlyList<PromptItem> items,
        int currentIndex,
        bool isPaused,
        int wordsPerMinute,
        TimeSpan elapsedSentence,
        TimeSpan currentSentenceDuration,
        TimeSpan totalElapsed,
        string fileName)
    {
        int width = Math.Max(50, Console.WindowWidth - 1);
        int height = Math.Max(14, Console.WindowHeight);

        // Usable rows strictly avoids touching the bottom row to prevent console scrolling
        int usableHeight = Math.Max(12, height - 2);

        var lines = new List<string>(usableHeight);

        // Row 0: Top Header
        var title = $" TELEPROMPTER CLI  |  {Path.GetFileName(fileName)} ";
        lines.Add(FormatHeaderBar(title, width));

        // Row 1: HUD Status Line
        lines.Add(FormatHudLine(items.Count, currentIndex, isPaused, wordsPerMinute, totalElapsed, width));

        // Row 2: CONTROLES SEMPRE NO TOPO!
        lines.Add(FormatControlsBar(width));

        // Row 3: Progress Bar
        lines.Add(FormatProgressBarLine(elapsedSentence, currentSentenceDuration, isPaused, width));

        // Row 4: Divider
        lines.Add(FormatDivider(width, '═', '╠', '╣'));

        // Reading Area (Rows 5 to usableHeight - 2)
        int readingLinesAvailable = usableHeight - 6; // rows 0..4 reserved + 1 bottom border
        if (readingLinesAvailable < 4) readingLinesAvailable = 4;

        int contentWidth = Math.Max(30, width - 8);

        // Current item
        var currentItem = (currentIndex >= 0 && currentIndex < items.Count)
            ? items[currentIndex]
            : null;

        var currentWrappedLines = currentItem != null
            ? WrapText(currentItem.Text, contentWidth)
            : new List<string> { "(Fim do roteiro)" };

        // Focus box height: top border (1) + text lines + bottom border (1)
        int focusBoxHeight = currentWrappedLines.Count + 2;
        int remainingForContext = Math.Max(0, readingLinesAvailable - focusBoxHeight);
        int historyLinesAllowed = remainingForContext / 2;
        int previewLinesAllowed = remainingForContext - historyLinesAllowed;

        // Collect History lines
        var historyLines = new List<string>();
        for (int i = currentIndex - 1; i >= 0 && historyLines.Count < historyLinesAllowed; i--)
        {
            var item = items[i];
            var wrapped = WrapText(item.Text, contentWidth);
            for (int w = wrapped.Count - 1; w >= 0; w--)
            {
                var prefix = (w == 0) ? (item.IsCue ? "  [CUE] " : "      ") : "      ";
                historyLines.Insert(0, $"{prefix}{wrapped[w]}");
                if (historyLines.Count >= historyLinesAllowed) break;
            }
        }

        // Collect Preview lines
        var previewLines = new List<string>();
        for (int i = currentIndex + 1; i < items.Count && previewLines.Count < previewLinesAllowed; i++)
        {
            var item = items[i];
            var wrapped = WrapText(item.Text, contentWidth);
            for (int w = 0; w < wrapped.Count; w++)
            {
                var prefix = (w == 0) ? (item.IsCue ? "  [CUE] " : "      ") : "      ";
                previewLines.Add($"{prefix}{wrapped[w]}");
                if (previewLines.Count >= previewLinesAllowed) break;
            }
        }

        // History Lines (Dimmed)
        int printedHistory = 0;
        foreach (var h in historyLines)
        {
            lines.Add(FormatLineContent($"{COLOR_DARK_GRAY}{h}{ANSI_RESET}", width));
            printedHistory++;
        }
        for (int p = printedHistory; p < historyLinesAllowed; p++)
        {
            lines.Add(FormatLineContent("", width));
        }

        // Eye-Line Top Border
        var cueTag = currentItem != null && currentItem.IsCue ? " [INSTRUÇÃO DE CENA] " : " [LINHA DE FOCO] ";
        lines.Add(FormatEyelineBorder(width, cueTag, isTop: true));

        // Eye-Line Current Text
        for (int i = 0; i < currentWrappedLines.Count; i++)
        {
            var lineText = currentWrappedLines[i];
            string formattedText;

            if (currentItem != null && currentItem.IsCue)
            {
                formattedText = $"   {COLOR_YELLOW}{ANSI_BOLD}★  {lineText}  ★{ANSI_RESET}";
            }
            else
            {
                var bullet = (i == 0) ? $"   {COLOR_CYAN}{ANSI_BOLD}▶{ANSI_RESET} " : "     ";
                formattedText = $"{bullet}{COLOR_BRIGHT_WHITE}{ANSI_BOLD}{lineText}{ANSI_RESET}";
            }

            lines.Add(FormatFocusLine(formattedText, width));
        }

        // Eye-Line Bottom Border
        lines.Add(FormatEyelineBorder(width, "", isTop: false));

        // Preview Lines (Soft gray)
        int printedPreview = 0;
        foreach (var pr in previewLines)
        {
            lines.Add(FormatLineContent($"{COLOR_GRAY}{pr}{ANSI_RESET}", width));
            printedPreview++;
        }
        for (int p = printedPreview; p < previewLinesAllowed; p++)
        {
            lines.Add(FormatLineContent("", width));
        }

        // Bottom border
        lines.Add(FormatDivider(width, '═', '╚', '╝'));

        // Write to terminal safely without trailing newline on the last line
        try
        {
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < lines.Count; i++)
            {
                Console.SetCursorPosition(0, i);
                // Clear from cursor to end of line to eliminate remnants from previous frames
                Console.Write(lines[i] + "\x1b[K");
            }
        }
        catch
        {
            // Fallback to ANSI home
            Console.Write("\x1b[H" + string.Join("\n", lines));
        }
    }

    public void RenderHudOnly(
        int totalItems,
        int currentIndex,
        bool isPaused,
        int wordsPerMinute,
        TimeSpan elapsedSentence,
        TimeSpan currentSentenceDuration,
        TimeSpan totalElapsed)
    {
        int width = Math.Max(50, Console.WindowWidth - 1);

        try
        {
            // Row 1: Status & metrics
            Console.SetCursorPosition(0, 1);
            Console.Write(FormatHudLine(totalItems, currentIndex, isPaused, wordsPerMinute, totalElapsed, width) + "\x1b[K");

            // Row 3: Progress bar
            Console.SetCursorPosition(0, 3);
            Console.Write(FormatProgressBarLine(elapsedSentence, currentSentenceDuration, isPaused, width) + "\x1b[K");
        }
        catch
        {
            // Ignore if terminal is being resized or cursor positioning fails
        }
    }

    private static string FormatHeaderBar(string title, int width)
    {
        var cleanTitle = title.Length > width - 4 ? title[..(width - 4)] : title;
        int leftPad = (width - 2 - cleanTitle.Length) / 2;
        int rightPad = width - 2 - cleanTitle.Length - leftPad;

        var sb = new StringBuilder();
        sb.Append(COLOR_HEADER_BG);
        sb.Append('╔');
        sb.Append(new string('═', Math.Max(0, leftPad)));
        sb.Append(ANSI_BOLD);
        sb.Append(cleanTitle);
        sb.Append(ANSI_RESET);
        sb.Append(COLOR_HEADER_BG);
        sb.Append(new string('═', Math.Max(0, rightPad)));
        sb.Append('╗');
        sb.Append(ANSI_RESET);
        return sb.ToString();
    }

    private static string FormatControlsBar(int width)
    {
        string keys = (width < 85)
            ? $" {ANSI_BOLD}[Espaço]{ANSI_RESET} Pausa │ {ANSI_BOLD}[←/→]{ANSI_RESET} Frase │ {ANSI_BOLD}[↑/↓]{ANSI_RESET} Vel │ {ANSI_BOLD}[R]{ANSI_RESET} Início │ {ANSI_BOLD}[Ctrl+C]{ANSI_RESET} Sair "
            : $" {ANSI_BOLD}[Espaço]{ANSI_RESET} Pausar/Retomar │ {ANSI_BOLD}[← / →]{ANSI_RESET} Frase │ {ANSI_BOLD}[↑ / ↓]{ANSI_RESET} Velocidade │ {ANSI_BOLD}[R]{ANSI_RESET} Reiniciar │ {ANSI_BOLD}[Ctrl+C]{ANSI_RESET} Sair ";

        return PadWithBorders(keys, width, borderChar: '║', borderColor: COLOR_HEADER_BG);
    }

    private static string FormatHudLine(int total, int index, bool isPaused, int wpm, TimeSpan totalElapsed, int width)
    {
        string statusBadge = isPaused
            ? $" {COLOR_YELLOW_BADGE} ⏸ PAUSADO {ANSI_RESET} "
            : $" {COLOR_GREEN_BADGE} ▶ EM REPRODUÇÃO {ANSI_RESET} ";

        int currentNumber = Math.Min(index + 1, total);
        double percent = total > 0 ? (currentNumber / (double)total) * 100.0 : 0;

        string inner;
        if (width < 80)
        {
            inner = $"{statusBadge}│ {ANSI_BOLD}{currentNumber}{ANSI_RESET}/{total} ({percent:F0}%) │ {COLOR_CYAN}{wpm} PPM{ANSI_RESET} │ {totalElapsed:mm\\:ss}";
        }
        else
        {
            string progressStr = $" Frase: {ANSI_BOLD}{currentNumber}{ANSI_RESET}/{total} ({percent:F0}%) ";
            string speedStr = $" Velocidade: {COLOR_CYAN}{wpm} PPM{ANSI_RESET} ";
            string timeStr = $" Tempo: {totalElapsed:mm\\:ss} ";
            inner = $"{statusBadge}│{progressStr}│{speedStr}│{timeStr}";
        }

        return PadWithBorders(inner, width);
    }

    private static string FormatProgressBarLine(TimeSpan elapsed, TimeSpan duration, bool isPaused, int width)
    {
        int barLength = Math.Clamp(width - 45, 8, 40);
        double ratio = duration.TotalMilliseconds > 0
            ? Math.Clamp(elapsed.TotalMilliseconds / duration.TotalMilliseconds, 0.0, 1.0)
            : 0;

        int filled = (int)Math.Round(ratio * barLength);
        int empty = Math.Max(0, barLength - filled);

        var barColor = isPaused ? COLOR_YELLOW : COLOR_CYAN;
        var bar = $"{barColor}{new string('█', filled)}{COLOR_DARK_GRAY}{new string('░', empty)}{ANSI_RESET}";

        var secLeft = Math.Max(0, (duration - elapsed).TotalSeconds);
        var label = (width < 70)
            ? $" Próx: {secLeft:F1}s [{bar}] "
            : $" Próxima frase em: {secLeft:F1}s [{bar}] ";

        return PadWithBorders(label, width);
    }

    private static string FormatEyelineBorder(int width, string title, bool isTop)
    {
        var sb = new StringBuilder();
        sb.Append(COLOR_EYELINE_BORDER);
        sb.Append(isTop ? "╠══" : "╚══");

        if (!string.IsNullOrEmpty(title) && width > 50)
        {
            sb.Append(ANSI_BOLD);
            sb.Append(title);
            sb.Append(ANSI_RESET);
            sb.Append(COLOR_EYELINE_BORDER);
        }

        int filledLength = 3 + ((!string.IsNullOrEmpty(title) && width > 50) ? title.Length : 0);
        int remaining = Math.Max(0, width - 2 - filledLength);
        sb.Append(new string('═', remaining));
        sb.Append(isTop ? "══╣" : "══╝");
        sb.Append(ANSI_RESET);
        return sb.ToString();
    }

    private static string FormatFocusLine(string textWithAnsi, int width)
    {
        return PadWithBorders(textWithAnsi, width, borderChar: '║', borderColor: COLOR_EYELINE_BORDER);
    }

    private static string FormatLineContent(string textWithAnsi, int width)
    {
        return PadWithBorders(textWithAnsi, width, borderChar: '║');
    }

    private static string FormatDivider(int width, char symbol, char leftChar = '╟', char rightChar = '╢')
    {
        var sb = new StringBuilder();
        sb.Append(COLOR_GRAY);
        sb.Append(leftChar);
        sb.Append(new string(symbol, Math.Max(0, width - 2)));
        sb.Append(rightChar);
        sb.Append(ANSI_RESET);
        return sb.ToString();
    }

    private static string PadWithBorders(string textWithAnsi, int width, char borderChar = '║', char? rightBorder = null, string? borderColor = null)
    {
        char left = borderChar;
        char right = rightBorder ?? borderChar;
        string bColor = borderColor ?? COLOR_GRAY;

        int availableSpace = Math.Max(0, width - 2);
        int visibleLen = GetVisibleLength(textWithAnsi);

        string content = textWithAnsi;
        if (visibleLen > availableSpace)
        {
            content = TruncateWithAnsi(textWithAnsi, availableSpace);
            visibleLen = GetVisibleLength(content);
        }

        int pad = Math.Max(0, availableSpace - visibleLen);

        var sb = new StringBuilder();
        sb.Append(bColor);
        sb.Append(left);
        sb.Append(ANSI_RESET);
        sb.Append(content);
        sb.Append(new string(' ', pad));
        sb.Append(bColor);
        sb.Append(right);
        sb.Append(ANSI_RESET);
        return sb.ToString();
    }

    private static string TruncateWithAnsi(string text, int maxVisibleChars)
    {
        var sb = new StringBuilder();
        int visibleCount = 0;
        bool inAnsi = false;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\x1b')
            {
                inAnsi = true;
                sb.Append(text[i]);
                continue;
            }

            if (inAnsi)
            {
                sb.Append(text[i]);
                if (char.IsLetter(text[i]) || text[i] == 'm')
                {
                    inAnsi = false;
                }
                continue;
            }

            if (visibleCount >= maxVisibleChars)
                break;

            sb.Append(text[i]);
            visibleCount++;
        }

        sb.Append(ANSI_RESET);
        return sb.ToString();
    }

    private static int GetVisibleLength(string text)
    {
        int count = 0;
        bool inAnsi = false;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\x1b')
            {
                inAnsi = true;
                continue;
            }
            if (inAnsi)
            {
                if (char.IsLetter(text[i]) || text[i] == 'm')
                {
                    inAnsi = false;
                }
                continue;
            }
            count++;
        }
        return count;
    }

    public static List<string> WrapText(string text, int maxWidth)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(text))
            return result;

        var words = text.Split(' ');
        var line = new StringBuilder();

        foreach (var word in words)
        {
            if (line.Length == 0)
            {
                line.Append(word);
            }
            else if (line.Length + 1 + word.Length <= maxWidth)
            {
                line.Append(' ');
                line.Append(word);
            }
            else
            {
                result.Add(line.ToString());
                line.Clear();
                line.Append(word);
            }
        }

        if (line.Length > 0)
        {
            result.Add(line.ToString());
        }

        return result;
    }
}
