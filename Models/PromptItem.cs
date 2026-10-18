namespace TeleprompterCli.Models;

public class PromptItem
{
    public int Index { get; set; }
    public int TotalCount { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCue { get; set; }
    public string CueName { get; set; } = string.Empty;
    public int ParagraphIndex { get; set; }
    public int WordCount { get; set; }

    public TimeSpan GetDuration(int wordsPerMinute)
    {
        if (IsCue)
        {
            var lower = CueName.ToLowerInvariant();
            if (lower.Contains("pausa"))
                return TimeSpan.FromSeconds(2.8);
            if (lower.Contains("ênfase") || lower.Contains("enfase"))
                return TimeSpan.FromSeconds(1.8);
            if (lower.Contains("reflexivo"))
                return TimeSpan.FromSeconds(2.0);
            return TimeSpan.FromSeconds(2.0);
        }

        // Duration based on reading speed (WPM)
        // Ensure wordsPerMinute is at least 30
        var effectiveWpm = Math.Max(30, wordsPerMinute);
        var seconds = (WordCount / (double)effectiveWpm) * 60.0;

        // Apply a realistic human floor and ceiling per sentence
        // Minimum 1.6s so short sentences are readable
        seconds = Math.Max(1.6, seconds);

        // Add small pause for punctuation: trailing commas/periods
        if (Text.EndsWith("...") || Text.EndsWith("…"))
            seconds += 0.8;
        else if (Text.EndsWith("?") || Text.EndsWith("!"))
            seconds += 0.5;

        return TimeSpan.FromSeconds(seconds);
    }
}
