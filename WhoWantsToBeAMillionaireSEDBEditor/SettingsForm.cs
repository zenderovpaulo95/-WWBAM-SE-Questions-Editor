using System;
using System.Windows.Forms;

namespace WhoWantsToBeAMillionaireSEDBEditor
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            cbUnicode.Checked = Form1.settings.NonUnicodeChecked;
            numericUpDown1.Value = Form1.settings.ASCIICode;
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            Form1.settings.ASCIICode = Convert.ToInt32(numericUpDown1.Value);
            Form1.settings.NonUnicodeChecked = cbUnicode.Checked;

            Settings.SaveSettings(Form1.settings);

            Close();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if(numericUpDown1.Value == 1252)
            {
                cbUnicode.Checked = false;
                cbUnicode.Enabled = false;
            }
            else
            {
                cbUnicode.Checked = Form1.settings.NonUnicodeChecked;
                cbUnicode.Enabled = true;
            }
        }
    }
}
