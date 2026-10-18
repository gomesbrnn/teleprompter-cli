using TeleprompterCli.Models;
using TeleprompterCli.UI;

namespace TeleprompterCli.Services;

public class TeleprompterEngine
{
    private readonly IReadOnlyList<PromptItem> _items;
    private readonly string _filePath;
    private readonly ConsoleRenderer _renderer;

    private int _currentIndex;
    private bool _isPaused = true; // Starts paused so the speaker is ready to hit Space
    private int _wordsPerMinute = 130;
    private TimeSpan _elapsedSentence = TimeSpan.Zero;
    private TimeSpan _totalElapsed = TimeSpan.Zero;

    private int _lastWidth = -1;
    private int _lastHeight = -1;

    public TeleprompterEngine(IReadOnlyList<PromptItem> items, string filePath)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
        _filePath = filePath;
        _renderer = new ConsoleRenderer();
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        TerminalHelper.InitializeTerminal();
        TerminalHelper.ClearScreen();

        _lastWidth = Console.WindowWidth;
        _lastHeight = Console.WindowHeight;

        var tickInterval = TimeSpan.FromMilliseconds(50);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var lastTick = stopwatch.Elapsed;

        bool needsFullRedraw = true;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var now = stopwatch.Elapsed;
                var delta = now - lastTick;
                lastTick = now;

                // Detect terminal resize
                if (Console.WindowWidth != _lastWidth || Console.WindowHeight != _lastHeight)
                {
                    _lastWidth = Console.WindowWidth;
                    _lastHeight = Console.WindowHeight;
                    TerminalHelper.ClearScreen();
                    needsFullRedraw = true;
                }

                // 1. Process Input
                while (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(intercept: true);

                    // Check Ctrl+C or Escape or Q
                    if ((keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) && keyInfo.Key == ConsoleKey.C) ||
                        keyInfo.Key == ConsoleKey.Escape ||
                        keyInfo.Key == ConsoleKey.Q)
                    {
                        return;
                    }

                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.Spacebar:
                            _isPaused = !_isPaused;
                            needsFullRedraw = true;
                            break;

                        case ConsoleKey.LeftArrow:
                            if (_currentIndex > 0)
                            {
                                _currentIndex--;
                                _elapsedSentence = TimeSpan.Zero;
                                needsFullRedraw = true;
                            }
                            break;

                        case ConsoleKey.RightArrow:
                            if (_currentIndex < _items.Count - 1)
                            {
                                _currentIndex++;
                                _elapsedSentence = TimeSpan.Zero;
                                needsFullRedraw = true;
                            }
                            break;

                        case ConsoleKey.UpArrow:
                            _wordsPerMinute = Math.Min(350, _wordsPerMinute + 10);
                            needsFullRedraw = true;
                            break;

                        case ConsoleKey.DownArrow:
                            _wordsPerMinute = Math.Max(40, _wordsPerMinute - 10);
                            needsFullRedraw = true;
                            break;

                        case ConsoleKey.R:
                            _currentIndex = 0;
                            _elapsedSentence = TimeSpan.Zero;
                            _isPaused = true;
                            needsFullRedraw = true;
                            break;
                    }
                }

                // 2. Compute timing & auto-advancement
                var currentItem = (_currentIndex >= 0 && _currentIndex < _items.Count)
                    ? _items[_currentIndex]
                    : null;

                var currentDuration = currentItem?.GetDuration(_wordsPerMinute) ?? TimeSpan.FromSeconds(3);

                if (!_isPaused && currentItem != null)
                {
                    _elapsedSentence += delta;
                    _totalElapsed += delta;

                    if (_elapsedSentence >= currentDuration)
                    {
                        if (_currentIndex < _items.Count - 1)
                        {
                            _currentIndex++;
                            _elapsedSentence = TimeSpan.Zero;
                            needsFullRedraw = true;
                        }
                        else
                        {
                            // End of script reached
                            _isPaused = true;
                            _elapsedSentence = currentDuration;
                            needsFullRedraw = true;
                        }
                    }
                }

                // 3. Render frame
                if (needsFullRedraw)
                {
                    _renderer.RenderFull(
                        _items,
                        _currentIndex,
                        _isPaused,
                        _wordsPerMinute,
                        _elapsedSentence,
                        currentDuration,
                        _totalElapsed,
                        _filePath);
                    needsFullRedraw = false;
                }
                else if (!_isPaused)
                {
                    // While playing, update only the HUD & progress bar at the top!
                    // Reading text below is untouched, completely eliminating screen flicker!
                    _renderer.RenderHudOnly(
                        _items.Count,
                        _currentIndex,
                        _isPaused,
                        _wordsPerMinute,
                        _elapsedSentence,
                        currentDuration,
                        _totalElapsed);
                }

                // 4. Delay until next tick
                try
                {
                    await Task.Delay(tickInterval, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
        finally
        {
            TerminalHelper.ResetColors();
            TerminalHelper.ShowCursor();
            Console.WriteLine();
        }
    }
}
