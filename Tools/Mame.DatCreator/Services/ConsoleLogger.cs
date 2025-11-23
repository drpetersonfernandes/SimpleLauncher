namespace Mame.DatCreator.Services;

public class ConsoleLogger
{
    public void Info(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public void Warning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARN] {message}");
        Console.ResetColor();
    }

    public void Error(string message, Exception? ex = null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        if (ex != null)
        {
            Console.WriteLine(ex.ToString());
        }

        Console.ResetColor();
    }
}