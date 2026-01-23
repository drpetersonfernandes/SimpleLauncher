using System.ComponentModel;

namespace Updater
{
    internal sealed partial class UpdateForm
    {
        private IContainer components = null;
        private TextBox logTextBox;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.logTextBox = new TextBox();
            this.SuspendLayout();
            // 
            // logTextBox
            // 
            this.logTextBox.Dock = DockStyle.Fill;
            this.logTextBox.Location = new Point(0, 0);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.ScrollBars = ScrollBars.Vertical;
            this.logTextBox.Size = new Size(584, 361);
            this.logTextBox.TabIndex = 0;
            // 
            // UpdateForm
            // 
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(584, 361);
            this.Controls.Add(this.logTextBox);
            this.Name = "UpdateForm";
            this.Text = "Updating...";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}