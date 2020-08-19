using iRadio.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;


namespace iRadio
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }
        }

        readonly System.Timers.Timer focusTimer = new System.Timers.Timer(5000);        // reset focus to listBoxDisplay

        private async void Form1_Load(object sender, EventArgs e)
        {
            Noxon.IP = IPAddress.Parse(Settings.Default.NoxonIP);  // use previous IP first
            Task<bool> isOpen = Task.Run(() => NoxonAsync.OpenAsync()); // https://stackoverflow.com/questions/14962969/how-can-i-use-async-to-increase-winforms-performance
            try
            {
                await isOpen;
                for (int i = 1; i <= 9; i++) ((Button)this.Controls["button" + i.ToString()]).Enabled = isOpen.Result;  // enable buttons1..9
                Program.formLogging.Text = "NOXON iRadio - " + Noxon.IP.ToString() + ":10100";
            }
            catch (SocketException exs)
            {
                MessageBox.Show(exs.Message, "iRadio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Noxon.netStream = null;
            }

            Program.form.trackBarVolume.Value = Settings.Default.Volume;
            foreach (string entry in Properties.Settings.Default.FavoritesList)
            {
                Program.form.listBoxFavs.Items.Add(entry);
            }
            StreamWriter nonParsedElementsWriter, parsedElementsWriter;
            TextWriter stdOut = Console.Out;
            Console.WriteLine("Console.WriteLine()");

            try
            {
                nonParsedElementsWriter = new StreamWriter(new FileStream("./iRadio-non-parsed-elements.txt", FileMode.Create, FileAccess.Write));
                parsedElementsWriter = new StreamWriter(new FileStream("./iRadio-logging.txt", FileMode.Create, FileAccess.Write));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot open iRadio .txt files for writing");
                Console.WriteLine(ex.Message);
                return;
            }

            if (Noxon.netStream == null) return;
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form_KeyDown);

            focusTimer.Elapsed += (sender, e) => ParseFocus();

            do
            {
                await Task.Run(() =>
                {
                    IEnumerable<XElement> iRadioNetData =
                    from el in NoxonAsync.StreamiRadioNet(Noxon.netStream)
                    select el;

                    Noxon.Parse(iRadioNetData, parsedElementsWriter, nonParsedElementsWriter, stdOut, Program.FormShow);
                });
                System.Diagnostics.Debug.WriteLine("Parse canceled, due to 'Nicht verfÃ¼gbar' - restarting Parse() now");
                await Task.Run(() => NoxonAsync.OpenAsync());
            } while (true);

        }
        private static void ParseFocus()
        {
            System.Diagnostics.Debug.WriteLine("ParseFocus: reset focus");
            Program.form.listBoxDisplay.Invoke((MethodInvoker)delegate
            {
                Program.form.listBoxFavs.ClearSelected();
                Program.form.listBoxDisplay.Focus();
                Program.form.pictureBoxRefresh.BackColor = System.Drawing.SystemColors.Control;
            });
        }
        private async void Form_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("KeyDown: {0}", e.KeyCode);

            bool isLetterOrDigit = char.IsLetterOrDigit((char)e.KeyCode);
            if (Noxon.netStream != null && isLetterOrDigit && Noxon.textEntry && textBox1.Visible)
            {
                KeysConverter kc = new KeysConverter();
                textBox1.Text += kc.ConvertToString(e.KeyCode)[0];
                return;
            }
            if (Noxon.netStream == null
                || (Noxon.textEntry && textBox1.Focused && (isLetterOrDigit || e.KeyCode == Keys.Enter))
                || listBoxFavs.Focused)
            {   // if nothing is received or text entry instead of local hotkeys
                return;
            }
            char command = ' ';

            if (e.Control && e.KeyCode == Keys.S)       // Ctrl-S Save
            {
                // TODO: Do what you want here
            }
            if (e.KeyCode == Keys.D1) command = '1';
            if (e.KeyCode == Keys.D2) command = '2';
            if (e.KeyCode == Keys.D3) command = '3';
            if (e.KeyCode == Keys.D4) command = '4';
            if (e.KeyCode == Keys.D5) command = '5';  // h --> KeyDown: H  // H -->   KeyDown: ShiftKey, KeyDown: H
            if (e.KeyCode == Keys.Left) command = 'L';
            if (e.KeyCode == Keys.Right) command = 'R';
            if (e.KeyCode == Keys.Enter) command = 'R';
            if (e.KeyCode == Keys.Up) command = 'U';
            if (e.KeyCode == Keys.Down) command = 'D';
            if (e.KeyCode == Keys.VolumeUp) command = '+';
            if (e.KeyCode == Keys.Oemplus) { command = '+'; }
            if (e.KeyCode == Keys.VolumeDown) command = '-';
            if (e.KeyCode == Keys.OemMinus) { command = '-'; }
            if (e.KeyCode == Keys.BrowserFavorites) command = 'F';
            if (e.KeyCode == Keys.Home) command = 'H';
            if (e.KeyCode == Keys.F1) { command = ' '; }
            if (e.KeyCode == Keys.Multiply) { command = '*'; }
            if (e.KeyCode == Keys.PageDown) { command = '>'; }
            if (e.KeyCode == Keys.PageUp) { command = '<'; }
            if (command == ' ' && isLetterOrDigit)
            {
                KeysConverter kc = new KeysConverter();
                char c = kc.ConvertToString(e.KeyCode)[0];  // first char in keyboard string, e.g. (F)avorites, (H)ome, (M)enu, ... 
                if (Noxon.Commands.ContainsKey(c)) command = c;
            }
            if (command != ' ')
            {
                Task<int> ret = Noxon.netStream.GetNetworkStream().CommandAsync(command);
                await ret;
                e.SuppressKeyPress = true;  // Stops other controls on the form receiving event.
            }
        }

        private void StatusStrip1_DoubleClick(object sender, EventArgs e)
        {
            if (Program.formLogging == null) Program.formLogging = new FormLogging();
            Program.formLogging.Show();
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            if (sender is Button b)
            {
                char command = b.Name.Last();  // button1, ... button9 
                if (Noxon.netStream != null && ('1' <= command && command <= '9'))
                {
                    Task<int> ret = Noxon.netStream.GetNetworkStream().CommandAsync(command);
                    await ret;
                    toolTip1.Show(Noxon.currentArtist, b);
                }
            }
        }
        private async void Button1_MouseClick(object sender, MouseEventArgs e)
        {
            if (sender is Button b && Control.ModifierKeys == Keys.Control)
            {
                char shortcut = b.Name.Last();  // button1, ... button9 
                if (Noxon.netStream != null && ('1' <= shortcut && shortcut <= '9'))
                {
                    Macro m = new iRadio.Macro("Button1_MouseClick", new string[] { "C", shortcut.ToString() }); // (C)hannnel + key 0..9 to store new preset
                    await Task.Run(() => m.Execute());
                    toolTip1.Show(Noxon.currentArtist, b);
                }
            }

        }

        private void ListBoxDisplay_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxDisplay.SelectedIndex != FormShow.selectedIndex)
            {
                listBoxDisplay.SelectedIndexChanged -= new EventHandler(ListBoxDisplay_SelectedIndexChanged);
                listBoxDisplay.SelectedIndex = FormShow.selectedIndex;
                listBoxDisplay.SelectedIndexChanged += new EventHandler(ListBoxDisplay_SelectedIndexChanged);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.NoxonIP = Noxon.IP.ToString();
            Properties.Settings.Default.Volume = Program.form.trackBarVolume.Value;
            Properties.Settings.Default.Save();
        }

        public async void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // await Task.Run(() => Noxon.netStream.GetNetworkStream().StringAsync(textBox1.Text));
                await Noxon.netStream.GetNetworkStream().StringAsync(textBox1.Text);
                await Noxon.netStream.GetNetworkStream().CommandAsync('R');
                while (Noxon.Busy) Thread.Sleep(100);
                await Noxon.netStream.GetNetworkStream().CommandAsync('D');  // refresh display
                await Noxon.netStream.GetNetworkStream().CommandAsync('U');
                textBox1.Text = null;
                listBoxDisplay.Focus();
                Noxon.textEntry = false;
            }
        }

        private async void PictureBoxRefresh_Click(object sender, EventArgs e)
        {
            pictureBoxRefresh.BackColor = System.Drawing.Color.LawnGreen;  // TODO: rotate refresh picture: Image img = pictureBox1.Image; img.RotateFlip(RotateFlipType.Rotate90FlipNone);  pictureBox1.Image = img;
            focusTimer.Start();
            Task<bool> get = Task.Run(() => Favorites.Get());
            await get;
            if (Favorites.List() != null)
            {
                Properties.Settings.Default.FavoritesList = Favorites.List();
                Program.form.listBoxFavs.Items.Clear();
                foreach (string entry in Properties.Settings.Default.FavoritesList)
                {
                    Program.form.listBoxFavs.Items.Add(entry);
                }
            }
            return;
        }

        private void ListBoxDisplay_DoubleClick(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ListBoxDisplay_DoubleClick: request cancellation.Cancel()");
            // TODO: await Task.Run(() => NoxonAsync.cancellation.Cancel());  // but how to access cancellation?
        }

        private async void ListBoxFavs_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listBoxFavs.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                focusTimer.Stop();
                focusTimer.Start();
                await SelectFavorite(listBoxFavs.SelectedIndex);
                return;
            }
        }

        private async void ListBoxFavs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                focusTimer.Stop();
                focusTimer.Start();
                await SelectFavorite(listBoxFavs.SelectedIndex);
                return;
            }
        }

        private async Task SelectFavorite(int selectedIndex)
        {
            List<string> down = new List<string> { "F" };   // goto favorites
            // choose the least number of steps
            string direction = "D";
            int steps = selectedIndex;
            if (selectedIndex > Program.form.listBoxFavs.Items.Count / 2)
            {
                direction = "U";
                steps = Program.form.listBoxFavs.Items.Count - selectedIndex;
            }
            for (int i = 0; i < steps; i++) down.Add(direction);  // step as often as necessary
            down.Add("R");  // choose station
            Macro m = new iRadio.Macro("listBoxFavs_KeyDown/MouseDoubleClick", down.ToArray());
            await Task.Run(() => m.Execute());
        }

        private void ListBoxFavs_Enter(object sender, EventArgs e)
        {
            focusTimer.Stop();
            focusTimer.Start();
        }

        private void ListBoxFavs_SelectedIndexChanged(object sender, EventArgs e)
        {
            focusTimer.Stop();
            focusTimer.Start();
        }

        private async void PictureBoxStop_Click(object sender, EventArgs e)
        {
            await Noxon.netStream.GetNetworkStream().CommandAsync('S');
        }

        private async void PictureBoxPlayPause_Click(object sender, EventArgs e)
        {
            await Noxon.netStream.GetNetworkStream().CommandAsync('P');
        }

        private async void PictureBoxPrevious_Click(object sender, EventArgs e)
        {
            await Noxon.netStream.GetNetworkStream().CommandAsync('<');
        }

        private async void PictureBoxNext_Click(object sender, EventArgs e)
        {
            await Noxon.netStream.GetNetworkStream().CommandAsync('>');
        }

        private async void PictureBoxAllDirections_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;
            System.Diagnostics.Debug.WriteLine("pictureBoxAllDirections_Click: coordinates = {0}, size = {1}", coordinates, pictureBoxAllDirections.Size);

            int minY = 1;
            int maxY = 18;
            int minX = 18;
            int maxX = 32;
            int H = pictureBoxAllDirections.Size.Height;
            int W = pictureBoxAllDirections.Size.Width;
            char command = ' ';
            if (minX <= coordinates.X && coordinates.X <= maxX)
            {
                if (minY <= coordinates.Y && coordinates.Y < maxY)
                    command = 'U';
                else if (Size.Height - minY >= coordinates.Y && coordinates.Y >= H - maxY)
                    command = 'D';
            }
            else if (maxY <= coordinates.Y && coordinates.Y < H - maxY)
            {
                if (1 <= coordinates.X && coordinates.X < minX)
                    command = 'L';
                else if (W - 1 >= coordinates.X && coordinates.X >= W - minX)
                    command = 'R';
            }
            System.Diagnostics.Debug.WriteLine("pictureBoxAllDirections_Click: command = {0}", command);
            if (command != ' ') await Noxon.netStream.GetNetworkStream().CommandAsync(command);

            //                                       50  X
            // ........................................                      1
            // ................../@@@..................                      
            // ................@@@@@@@@@...............
            // ..............@@@..@@@..@@@.............
            // ...................@@@..................                      18
            // ...................@@@..................                  18 ...  32
            // ......@@@.......................@@@.....
            // ....@@@...........................@@@...
            // ..@@@@@@@@@@@@.............@@@@@@@@@@@@.    
            // ....@@@...........................@@@@..
            // ......@@@.......................@@@.....
            // ...................@@@..................
            // ...................@@@..................
            //........... ...@@@..@@@..@@@.............
            //.......... ......@@@@@@@@@...............
            //............ .......@@@..................     }
            // 50  
            // Y
        }

        private async void PictureBoxShuffle_Click(object sender, EventArgs e)
        {
            await Noxon.netStream.GetNetworkStream().CommandAsync('X'); // toggle shuffle
        }

        private void PictureBoxRemote_Click(object sender, EventArgs e)
        {
            if (Program.formRemote == null)
            {
                Program.formRemote = new FormRemote();
                Program.formRemote.Show();
            }
            else
            {
                Program.formRemote.Close();
            }
        }
    }
}                                                                            