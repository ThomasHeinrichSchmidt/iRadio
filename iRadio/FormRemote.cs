﻿using System;
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
        readonly Bitmap zoneMap = iRadio.Properties.Resources.iRadio_Remote_Control_ColorMap;
        public static Dictionary<Color, char> RemoteKeys = new Dictionary<Color, char>()
        {
            {  Color.FromArgb(255, 128,  10), '1' },   // colors used in iRadio_Remote_Control_ColorMap to mark key positions on remote
            {  Color.FromArgb(255, 128,  20), '2' },   // chars are Noxon.Commands.Keys
            {  Color.FromArgb(255, 128,  30), '3' },
            {  Color.FromArgb(255, 128,  40), '4' },
            {  Color.FromArgb(255, 128,  50), '5' },
            {  Color.FromArgb(255, 128,  60), '6' },
            {  Color.FromArgb(255, 128,  70), '7' },
            {  Color.FromArgb(255, 128,  80), '8' },
            {  Color.FromArgb(255, 128,  90), '9' },
            {  Color.FromArgb(255, 128, 100), '0' },
            {  Color.FromArgb(160,  70, 160), 'X' },   // Shuffle
            {  Color.FromArgb(160,  70, 180), '*' },   // Repeat
            {  Color.FromArgb(255,   0,   0), 'E' },   // (E)rase favourite if entry in favourites list selected 
            {  Color.FromArgb(  0, 255,   0), 'A' },   // (A)dd favourite if channel/station playing 
            {  Color.FromArgb(  0, 128, 255), 'L' },
            {  Color.FromArgb( 50, 128, 255), 'U' },
            {  Color.FromArgb(100, 128, 255), 'R' },
            {  Color.FromArgb(150, 128, 255), 'D' },
            {  Color.FromArgb(  0, 255, 100), 'C' },   // (C)hannnel  + key 0..9 to store new preset
            {  Color.FromArgb( 30, 160,  70), 'P' },   // (P)lay
            {  Color.FromArgb( 30, 160, 100), '>' },   // Next
            {  Color.FromArgb( 30, 160, 130), '<' },   // Previous
            {  Color.FromArgb( 30, 160, 160), 'S' },   // Stop
            {  Color.FromArgb(255, 200,  10), 'H' },   // (H)ome
            {  Color.FromArgb(255, 200, 100), 'I' },   // (I)nternetradio
            {  Color.FromArgb(255, 200, 150), 'M' },   // (M)enu - settings
            {  Color.FromArgb(255, 200, 180), 'F' },   // (F)avorites
            {  Color.FromArgb(160, 200, 240), '-' },   // volume -
            {  Color.FromArgb(160, 200, 255), '+' },   // volume +
            {  Color.FromArgb(127, 127, 127), 'N' }    // I(N)fo
        };
        public FormRemote()
        {
            InitializeComponent();
        }
        private void FormRemote_Load(object sender, EventArgs e)
        {
            if ((lastPos.X > 0 && lastPos.Y > 0) && (sender is Form c))
            {
                c.Location = lastPos;
            }
        }
        private void FormRemote_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.formRemote = null;
            _dragging = false;
        }
        private async void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            Color color = zoneMap.GetPixel(e.X, e.Y);
            if (RemoteKeys.ContainsKey(color))
            {
                char command = RemoteKeys[color];
                System.Diagnostics.Debug.WriteLine("Remote: key {0} detected for color {1}", command, color);
                if (command != ' ') await Noxon.netStream.GetNetworkStream().CommandAsync(command);
            }
        }

        private int _xPos = -1;
        private int _yPos = -1;
        private bool _dragging = false;
        private static Point lastPos = new Point(-1, -1);
        // https://stackoverflow.com/questions/570582/move-a-picturebox-with-mouse
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _dragging = true;
            _xPos = e.X;
            _yPos = e.Y;
        }
        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging || !(sender is PictureBox c)) return;
            Point delta = new Point(c.TopLevelControl.Location.X + e.X - _xPos, c.TopLevelControl.Location.Y + e.Y - _yPos);
            c.TopLevelControl.Location = delta;
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!(sender is PictureBox c)) return;
            _dragging = false;
            if (c.TopLevelControl != null) lastPos = new Point(c.TopLevelControl.Location.X, c.TopLevelControl.Location.Y);
        }

        private void PictureBox1_DoubleClick(object sender, EventArgs e)
        {
            if (!(sender is PictureBox)) return;
            ((Form)this.TopLevelControl).Close();
        }
    }
}
