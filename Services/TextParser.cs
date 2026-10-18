using System.Text;
using System.Text.RegularExpressions;
using TeleprompterCli.Models;

namespace TeleprompterCli.Services;

public static class TextParser
{
    private static readonly Regex CueRegex = new(@"\[([^\]]+)\]", RegexOptions.Compiled);
    private static readonly Regex SentenceSplitRegex = new("(?<=[.!?…][\"'”’]?)\\s+(?=[A-Za-z0-9\"'“‘\\[])", RegexOptions.Compiled);

    public static List<PromptItem> ParseFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Arquivo não encontrado: {filePath}");

        string content;
        try
        {
            // First attempt with strict UTF-8
            var utf8Strict = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            content = File.ReadAllText(filePath, utf8Strict);
        }
        catch (DecoderFallbackException)
        {
            // Fallback to Latin1/Windows-1252 for older files
            content = File.ReadAllText(filePath, Encoding.Latin1);
        }

        return ParseText(content);
    }

    public static List<PromptItem> ParseText(string text)
    {
        var rawLines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var items = new List<PromptItem>();

        var currentParagraph = new StringBuilder();
        int paragraphIndex = 0;

        foreach (var line in rawLines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                if (currentParagraph.Length > 0)
                {
                    ProcessParagraph(currentParagraph.ToString(), paragraphIndex++, items);
                    currentParagraph.Clear();
                }
            }
            else
            {
                if (currentParagraph.Length > 0)
                    currentParagraph.Append(' ');
                currentParagraph.Append(trimmed);
            }
        }

        if (currentParagraph.Length > 0)
        {
            ProcessParagraph(currentParagraph.ToString(), paragraphIndex++, items);
        }

        // Finalize index and totals
        for (int i = 0; i < items.Count; i++)
        {
            items[i].Index = i + 1;
            items[i].TotalCount = items.Count;
        }

        return items;
    }

    private static void ProcessParagraph(string paragraph, int paragraphIndex, List<PromptItem> items)
    {
        // Tokenize into cues and text segments
        int lastIndex = 0;
        var matches = CueRegex.Matches(paragraph);

        foreach (Match match in matches)
        {
            if (match.Index > lastIndex)
            {
                var textBefore = paragraph.Substring(lastIndex, match.Index - lastIndex).Trim();
                if (!string.IsNullOrEmpty(textBefore))
                {
                    SplitAndAddSentences(textBefore, paragraphIndex, items);
                }
            }

            var cueContent = match.Groups[1].Value.Trim();
            items.Add(new PromptItem
            {
                Text = $"[{cueContent}]",
                IsCue = true,
                CueName = cueContent,
                ParagraphIndex = paragraphIndex,
                WordCount = CountWords(cueContent)
            });

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < paragraph.Length)
        {
            var remaining = paragraph.Substring(lastIndex).Trim();
            if (!string.IsNullOrEmpty(remaining))
            {
                SplitAndAddSentences(remaining, paragraphIndex, items);
            }
        }
    }

    private static void SplitAndAddSentences(string text, int paragraphIndex, List<PromptItem> items)
    {
        var rawSentences = SentenceSplitRegex.Split(text);

        foreach (var raw in rawSentences)
        {
            var clean = raw.Trim();
            if (string.IsNullOrEmpty(clean))
                continue;

            items.Add(new PromptItem
            {
                Text = clean,
                IsCue = false,
                ParagraphIndex = paragraphIndex,
                WordCount = CountWords(clean)
            });
        }
    }

    private static int CountWords(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return 0;

        var words = s.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        return words.Length;
    }
}
