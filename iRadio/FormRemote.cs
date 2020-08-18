using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace iRadio
{
    public partial class FormRemote : Form
    {
        public FormRemote()
        {
            InitializeComponent();
        }

        private void FormRemote_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.formRemote = null;
        }
    }
}
