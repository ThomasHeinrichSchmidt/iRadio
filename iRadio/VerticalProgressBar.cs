using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace iRadio
{
    [System.ComponentModel.Description("Vertical Progress Bar")]
    [System.Drawing.ToolboxBitmap(typeof(ProgressBar))]
    [System.ComponentModel.Browsable(true)]
    public partial class VerticalProgressBar : ProgressBar
    {
        public VerticalProgressBar()
        {
            InitializeComponent();
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x04;

                if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6)
                {
                    cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED 
                }
                return cp;
            }
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x000F)
            {
                using System.Drawing.Graphics graphics = CreateGraphics();
                using System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(ForeColor);
                SizeF textSize = graphics.MeasureString(Text, Font);
                graphics.DrawString(Text, Font, brush, (Width - textSize.Width) / 2, (Height - textSize.Height) / 2);
            }
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Always)]
        [System.ComponentModel.Browsable(true)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                Refresh();
            }
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Always)]
        [System.ComponentModel.Browsable(true)]
        public override System.Drawing.Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                Refresh();
            }
        }
    }
}
