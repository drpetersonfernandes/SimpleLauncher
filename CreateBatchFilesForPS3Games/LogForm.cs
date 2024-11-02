using System;
using System.Windows.Forms;

namespace CreateBatchFilesForPS3Games
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
        }
    }
}