using System.Diagnostics;
using System.Windows.Forms;

namespace JournalTrace.View.Info
{
    public partial class FormInfo : Form
    {
        public FormInfo()
        {
            InitializeComponent();
            Util.ModernTheme.ApplyToForm(this);
        }

        private void lnkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Solduramigopmo");
        }
    }
}
