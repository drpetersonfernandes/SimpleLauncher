using System.Windows;

namespace Mame.DatCreator;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var app = new Application();
        var mainWindow = new MainWindow();
        app.Run(mainWindow);
    }
}