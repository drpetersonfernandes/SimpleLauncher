namespace CreateBatchFilesForXbox360XBLAGames
{
    public partial class LogForm : Form
    {
        public LogForm()
        {
            InitializeComponent();
        }

        public void LogMessage(string message)
        {
            textBoxLog.AppendText(message + Environment.NewLine);
            textBoxLog.SelectionStart = textBoxLog.Text.Length;
            textBoxLog.ScrollToCaret();
        }
    }
}