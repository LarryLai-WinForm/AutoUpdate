namespace AutoUpdate.Form
{
    partial class AutoUpdateForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox_MSG = new System.Windows.Forms.TextBox();
            this.button_Close = new System.Windows.Forms.Button();
            this.backgroundWorker_Update = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // textBox_MSG
            // 
            this.textBox_MSG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_MSG.BackColor = System.Drawing.Color.Black;
            this.textBox_MSG.ForeColor = System.Drawing.Color.White;
            this.textBox_MSG.Location = new System.Drawing.Point(12, 12);
            this.textBox_MSG.Multiline = true;
            this.textBox_MSG.Name = "textBox_MSG";
            this.textBox_MSG.ReadOnly = true;
            this.textBox_MSG.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_MSG.Size = new System.Drawing.Size(575, 218);
            this.textBox_MSG.TabIndex = 0;
            this.textBox_MSG.TabStop = false;
            this.textBox_MSG.TextChanged += new System.EventHandler(this.textBox_MSG_TextChanged);
            // 
            // button_Close
            // 
            this.button_Close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Close.BackColor = System.Drawing.SystemColors.Control;
            this.button_Close.Enabled = false;
            this.button_Close.Location = new System.Drawing.Point(512, 236);
            this.button_Close.Name = "button_Close";
            this.button_Close.Size = new System.Drawing.Size(75, 23);
            this.button_Close.TabIndex = 1;
            this.button_Close.Text = "Close";
            this.button_Close.UseVisualStyleBackColor = false;
            this.button_Close.Click += new System.EventHandler(this.button_Close_Click);
            // 
            // backgroundWorker_Update
            // 
            this.backgroundWorker_Update.WorkerReportsProgress = true;
            this.backgroundWorker_Update.WorkerSupportsCancellation = true;
            this.backgroundWorker_Update.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_Update_DoWork);
            this.backgroundWorker_Update.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_Update_ProgressChanged);
            // 
            // AutoUpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(599, 263);
            this.ControlBox = false;
            this.Controls.Add(this.button_Close);
            this.Controls.Add(this.textBox_MSG);
            this.Font = new System.Drawing.Font("細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AutoUpdateForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AutoUpdate";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.AutoUpdateForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_MSG;
        private System.Windows.Forms.Button button_Close;
        private System.ComponentModel.BackgroundWorker backgroundWorker_Update;
    }
}