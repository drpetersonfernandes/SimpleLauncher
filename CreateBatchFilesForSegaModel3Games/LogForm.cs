namespace CreateBatchFilesForSegaModel3Games
{
    public partial class LogForm : Form
    {
        public LogForm()
        {
            InitializeComponent();

            // Handle the FormClosing event
            FormClosing += LogForm_FormClosing;
        }

        private void LogForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Exit the application
            Application.Exit();
            Environment.Exit(0);
        }

        public void LogMessage(string message)
        {
            textBoxLog.AppendText(message + Environment.NewLine);
            textBoxLog.SelectionStart = textBoxLog.Text.Length;
            textBoxLog.ScrollToCaret();
        }
    }
}