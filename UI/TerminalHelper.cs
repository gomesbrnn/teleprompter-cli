using System.Runtime.InteropServices;
using System.Text;

namespace TeleprompterCli.UI;

public static class TerminalHelper
{
    private const int STD_OUTPUT_HANDLE = -11;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    public static void InitializeTerminal()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                var handle = GetStdHandle(STD_OUTPUT_HANDLE);
                if (GetConsoleMode(handle, out uint mode))
                {
                    mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
                    SetConsoleMode(handle, mode);
                }
            }
            catch
            {
                // Ignore fallback to standard console if API is not available
            }
        }

        HideCursor();
    }

    public static void HideCursor()
    {
        try
        {
            Console.CursorVisible = false;
            Console.Write("\x1b[?25l");
        }
        catch { }
    }

    public static void ShowCursor()
    {
        try
        {
            Console.CursorVisible = true;
            Console.Write("\x1b[?25h");
        }
        catch { }
    }

    public static void ResetColors()
    {
        Console.Write("\x1b[0m");
    }

    public static void ClearScreen()
    {
        Console.Write("\x1b[2J\x1b[H");
    }
}
