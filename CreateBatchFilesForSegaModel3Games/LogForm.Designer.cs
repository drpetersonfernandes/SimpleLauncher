namespace CreateBatchFilesForSegaModel3Games
{
    partial class LogForm
    {
        private System.Windows.Forms.TextBox textBoxLog;

        private void InitializeComponent()
        {
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            
            // 
            // textBoxLog
            // 
            this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLog.Multiline = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.Size = new System.Drawing.Size(800, 450);
            this.textBoxLog.TabIndex = 0;

            // 
            // LogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBoxLog);
            this.Name = "LogForm";
            this.Text = "Log Viewer";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}