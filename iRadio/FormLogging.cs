using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace iRadio
{
    public partial class FormLogging : Form
    {
        public FormLogging()
        {
            InitializeComponent();
        }

        private void FormLogging_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Program.formLogging = null;
        }

        private void FormLogging_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void ListBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                System.Text.StringBuilder copy_buffer = new System.Text.StringBuilder();
                copy_buffer.AppendLine("NOXON iRadio - " + Noxon.IP.ToString() + ":10100");
                foreach (object item in listBox1.Items) copy_buffer.AppendLine(item.ToString());
                copy_buffer.AppendLine("[" + DateTime.Now + "]");
                if (copy_buffer.Length > 0) Clipboard.SetDataObject(copy_buffer.ToString());
            }
        }
    }
}
