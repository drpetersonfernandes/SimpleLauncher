namespace Updater;

file static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// The first argument (args[0]) should be the Process ID of the main application to wait for.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new UpdateForm(args));
    }
}