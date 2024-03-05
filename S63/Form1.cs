using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AVCS.Kernel;
using System.Diagnostics;

//NOTE: if you receive the m_Key and m_ID as astring like "99,33221", then m_ID "99" to be converted to HEX 4 chars.
//Please find the sample below:
//byte[] buf = Encoding.ASCII.GetBytes(m_ID);
//string hexString = BitConverter.ToString(buf);
//hexString = hexString.Remove(2, 1);

namespace S63
{
    public partial class Form1 : Form
    {
        private string CellPath = "";
        Dictionary<string, Permit> pmts = new Dictionary<string, Permit>();

        public Form1()
        {
            InitializeComponent();
        }

        private void btCreateUP_Click(object sender, EventArgs e)
        {
            if( textHWID.Text.Length == 0)
            {
                MessageBox.Show("Enter valid HW ID.");
                return;
            }
            if (textManID.Text.Length == 0)
            {
                MessageBox.Show("Enter valid Manufacture ID.");
                return;
            }
            if (textManKey.Text.Length == 0)
            {
                MessageBox.Show("Enter valid Manufacture Key.");
                return;
            }
            byte[] hw_id = Encoding.ASCII.GetBytes(textHWID.Text);
            Array.Resize(ref hw_id, 6);
            hw_id[5] = hw_id[0];
            textUserPermit.Text = S63Crypt.CreateUserPermit(hw_id, textManKey.Text, textManID.Text);
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btPermitValidate_Click(object sender, EventArgs e)
        {
            if (textManID.Text.Length == 0)
            {
                MessageBox.Show("Enter valid Manufacture ID.");
                return;
            }
            if (textManKey.Text.Length == 0)
            {
                MessageBox.Show("Enter valid Manufacture Key.");
                return;
            }
            if (textUserPermit.Text.Length == 0)
            {
                MessageBox.Show("Enter valid UserPermit.");
                return;
            }
            if (textPermit.Text.Length < 64)
            {
                MessageBox.Show("Enter valid cell permit. The length should be 64.");
                return;
            }

            S63Crypt._MKey = textManKey.Text;
            S63Crypt._MId = textManID.Text;
            S63Crypt._UP = textUserPermit.Text;
            S63Crypt._HWID = S63Crypt.GetHWIDFromUserPermit(textManKey.Text, textUserPermit.Text);
            ENC.Params.S63Error res = ENCCollection.CheckPermit(textPermit.Text, pmts);
            MessageBox.Show("Permit status is: " + res.DisplayString());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textManKey.Text = "";
            textManID.Text = "";
            textUserPermit.Text = "";
            textPermit.Text = "";
        }

        private void btBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            fb.Description = "Select the path to encrypted cell, like ..\\ENC_ROOT\\US\\US410930";
            if (fb.ShowDialog() == DialogResult.Cancel) return;
            CellPath = fb.SelectedPath;
            textCell.Text = Path.GetFileName(CellPath);
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if( !string.IsNullOrEmpty(textPermit.Text))
                btPermitValidate.PerformClick();

            StringWriter sw = new StringWriter();

            ENCCollection.InstallCharts(sw, CellPath, pmts);

            sw.Close();
            File.WriteAllText("LogFile.txt", sw.ToString());
            Process.Start("LogFile.txt");

        }
    }
}
