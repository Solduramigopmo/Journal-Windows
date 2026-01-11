namespace JournalTrace.View.Info
{
    partial class FormInfo
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.lnkGitHub = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(320, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Journal Windows";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAuthor
            // 
            this.lblAuthor.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblAuthor.Location = new System.Drawing.Point(12, 50);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(320, 20);
            this.lblAuthor.TabIndex = 1;
            this.lblAuthor.Text = "fayin";
            this.lblAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lnkGitHub
            // 
            this.lnkGitHub.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lnkGitHub.LinkColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.lnkGitHub.Location = new System.Drawing.Point(12, 75);
            this.lnkGitHub.Name = "lnkGitHub";
            this.lnkGitHub.Size = new System.Drawing.Size(320, 20);
            this.lnkGitHub.TabIndex = 2;
            this.lnkGitHub.TabStop = true;
            this.lnkGitHub.Text = "github.com/Solduramigopmo";
            this.lnkGitHub.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lnkGitHub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGitHub_LinkClicked);
            // 
            // FormInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 110);
            this.Controls.Add(this.lnkGitHub);
            this.Controls.Add(this.lblAuthor);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormInfo";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag = "info";
            this.Text = "Information";
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.LinkLabel lnkGitHub;
    }
}
