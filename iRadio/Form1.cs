using iRadio.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;


namespace iRadio
{
    public partial class Form1 : Form
    {
        readonly bool allowDirectDisplayControl = false;

        public Form1()
        {
            InitializeComponent();
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
                ReloadSettings(); // force new settings to be added to user.config - increment version to enforce
            }
            System.Diagnostics.Debug.WriteLine("Settings.Default.LogCommands = {0}", Settings.Default.LogCommands);
            System.Diagnostics.Debug.WriteLine("Settings.Default.LogTimestamps = {0}", Settings.Default.LogTimestamps);
        }

        private void ReloadSettings()
        {
            // Settings.Default.Reload();  // needed to read actual values from user.config - but prevents to add new settings to file
            foreach (SettingsPropertyValue p in Settings.Default.PropertyValues)
            {
                p.IsDirty = true;
                System.Diagnostics.Debug.WriteLine("ReloadSettings: {0}={1}, dirty={2}", p.Name, p.SerializedValue, p.IsDirty);
            }
            Settings.Default.Save();
        }

        readonly System.Timers.Timer focusTimer = new System.Timers.Timer(5000);        // reset focus to listBoxDisplay
        private static StreamWriter parsedElementsWriter;
        private static StreamWriter nonParsedElementsWriter;
        private static TextWriter stdOut;

        public static StreamWriter NonParsedElementsWriter { get => nonParsedElementsWriter; set => nonParsedElementsWriter = value; }
        public static StreamWriter ParsedElementsWriter { get => parsedElementsWriter; set => parsedElementsWriter = value; }
        public static TextWriter StdOut { get => stdOut; set => stdOut = value; }

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
            StdOut = Console.Out;
            Console.WriteLine("Console.WriteLine()");

            try
            {
                NonParsedElementsWriter = new StreamWriter(new FileStream("./iRadio-non-parsed-elements.txt", FileMode.Create, FileAccess.Write));
                ParsedElementsWriter = new StreamWriter(new FileStream("./iRadio-logging.txt", FileMode.Create, FileAccess.Write));
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

                    Noxon.Parse(iRadioNetData, ParsedElementsWriter, NonParsedElementsWriter, StdOut, Program.FormShow);
                });
                System.Diagnostics.Debug.WriteLine("Parse canceled, due to 'Nicht verfÃ¼gbar' - restarting Parse() now");
                await Task.Run(() => NoxonAsync.OpenAsync());
            } while (true);

        }
        private static void ParseFocus()
        {
            System.Diagnostics.Debug.WriteLine("ParseFocus: reset focus to iRadio display");
            Program.form.listBoxDisplay.Invoke((MethodInvoker)delegate
            {
                Program.form.listBoxFavs.ClearSelected();
                Program.form.listBoxDisplay.Focus();
                Program.form.pictureBoxRefresh.BackColor = System.Drawing.SystemColors.Control;
            });
        }

        static readonly Dictionary<Keys, char> KeyCommands = new Dictionary<Keys, char>()
        {
            { Keys.D1, '1' },
            { Keys.D2, '2' },
            { Keys.D3, '3' },
            { Keys.D4, '4' },
            { Keys.D5, '5' },
            { Keys.Left, 'L' },
            { Keys.Right, 'R' },
            { Keys.Enter, 'R' },
            { Keys.Up, 'U' },
            { Keys.Down, 'D' },
            { Keys.VolumeUp, '+' },
            { Keys.Add, '+' },
            { Keys.Oemplus, '+' },
            { Keys.VolumeDown, '-' },
            { Keys.Subtract, '-' },
            { Keys.OemMinus, '-' },
            { Keys.BrowserFavorites, 'F' },
            { Keys.Home, 'H' },                // h --> KeyDown: H  // H -->   KeyDown: ShiftKey, KeyDown: H
            { Keys.F1, ' ' },
            { Keys.Multiply, '*' },
            { Keys.PageDown, '>' },
            { Keys.PageUp, '<' }
        };
        private async void Form_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Form_KeyDown: {0}", e.KeyCode);

            bool isLetterOrDigit = char.IsLetterOrDigit((char)e.KeyCode) && !e.Control;
            if (Noxon.netStream != null && Noxon.textEntry && textBoxSearch.Visible)
            {
                if (isLetterOrDigit)
                {
                    KeysConverter kc = new KeysConverter();
                    if (textBoxSearch.Text.Length < textBoxSearch.MaxLength) textBoxSearch.Text += kc.ConvertToString(e.KeyCode)[0];
                    return;
                }
                else if (e.KeyCode == Keys.Back)
                {
                    if (textBoxSearch.Text.Length > 0) textBoxSearch.Text = textBoxSearch.Text[0..^1];
                    return;
                }
                else if (Noxon.Commands.ContainsKey(GetChar(e)) || KeyCommands.ContainsKey(e.KeyCode))
                {
                    // continue below
                }
                else
                {
                    return;
                }
            }
            if (Noxon.netStream == null
                || (Noxon.textEntry && textBoxSearch.Focused && (isLetterOrDigit || e.KeyCode == Keys.Enter))
                || listBoxFavs.Focused)
            {   // if nothing is received or text entry instead of local hotkeys
                return;
            }
            char command = ' ';
            if (KeyCommands.ContainsKey(e.KeyCode)) command = KeyCommands[e.KeyCode];
            if (command == ' ' && isLetterOrDigit)
            {
                char c = GetChar(e);
                if (Noxon.Commands.ContainsKey(c)) command = c;
            }
            if (command != ' ')
            {
                await Noxon.netStream.GetNetworkStream().CommandAsync(command);
                e.SuppressKeyPress = true;  // Stops other controls on the form receiving event.
            }
        }

        private static char GetChar(KeyEventArgs e)
        {
            KeysConverter kc = new KeysConverter();
            char c = kc.ConvertToString(e.KeyCode)[0];  // first char in keyboard string, e.g. (F)avorites, (H)ome, (M)enu, ... 
            return c;
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
                    if (Noxon.textEntry)
                    {
                        char shortcut = b.Name.Last();  // button1, ... button9 
                        Macro m = new iRadio.Macro("Button1_Click", new string[] { "H", shortcut.ToString() }); // (H)ome + key 0..9 to enforce new channel (even if searching was active)
                        await Task.Run(() => m.Execute());
                    }
                    else
                    {
                        Task<int> ret = Noxon.netStream.GetNetworkStream().CommandAsync(command);
                        await ret;
                    }
                    toolTip1.Show(Noxon.currentArtist, b);
                }
                focusTimer.Stop();
                focusTimer.Start();
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

        private async void ListBoxDisplay_Click(object sender, EventArgs e)
        {
            if (sender is ListBox box)
            {
                if (allowDirectDisplayControl)
                {
                    int index = box.SelectedIndex;
                    await AdjustNoxonDisplay(index);
                }
            }
        }
        private async void ListBoxDisplay_DoubleClick(object sender, EventArgs e)
        {
            if (sender is ListBox)
            {
                await Noxon.netStream.GetNetworkStream().CommandAsync('R');
            }
            // TODO: await Task.Run(() => NoxonAsync.cancellation.Cancel());  // but how to access cancellation?
            //       System.Diagnostics.Debug.WriteLine("ListBoxDisplay_DoubleClick: would be nice to request cancellation.Cancel()");
        }
        private async void ListBoxDisplay_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (allowDirectDisplayControl)
            {
                System.Diagnostics.Debug.WriteLine("ListBoxDisplay_SelectedIndexChanged: listBoxDisplay.SelectedIndex = {0}, FormShow.selectedIndex = {1}", listBoxDisplay.SelectedIndex, FormShow.selectedIndex);
                if (listBoxDisplay.SelectedIndex != FormShow.selectedIndex)   // ignore selection changes 
                {
                    await AdjustNoxonDisplay(listBoxDisplay.SelectedIndex);
                }
            }
            else
            {
                listBoxDisplay.SelectedIndexChanged -= new EventHandler(ListBoxDisplay_SelectedIndexChanged);
                listBoxDisplay.SelectedIndex = FormShow.selectedIndex;
                listBoxDisplay.SelectedIndexChanged += new EventHandler(ListBoxDisplay_SelectedIndexChanged);
            }
        }
        private async Task AdjustNoxonDisplay(int index)
        {
            listBoxDisplay.SelectedIndexChanged -= new EventHandler(ListBoxDisplay_SelectedIndexChanged);

            string direction = "D";
            int steps = index - FormShow.selectedIndex;
            if (steps < 0) direction = "U";
            List<string> mstrings = new List<string>();
            for (int i = 0; i < Math.Abs(steps); i++) mstrings.Add(direction);
            Macro m = new iRadio.Macro("ListBoxDisplay_Click", mstrings.ToArray());
            await Task.Run(() => m.Execute());

            listBoxDisplay.SelectedIndexChanged += new EventHandler(ListBoxDisplay_SelectedIndexChanged);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.NoxonIP = Noxon.IP.ToString();
            Settings.Default.Volume = Program.form.trackBarVolume.Value;
            Settings.Default.Save();
        }

        public async void TextBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("TextBoxSearch_KeyDown: {0}, Ctrl={1}", e.KeyCode, e.Control);
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
            {
                if (textBoxSearch.Text.Length == 0) return;
                // await Task.Run(() => Noxon.netStream.GetNetworkStream().StringAsync(textBox1.Text));
                await Noxon.netStream.GetNetworkStream().CommandStringAsync(textBoxSearch.Text);
                await Noxon.netStream.GetNetworkStream().CommandAsync('R');
                await RefreshNoxonDisplay();
                textBoxSearch.Text = null;
                listBoxDisplay.Focus();
                Noxon.textEntry = false;
            }
            else if (char.IsLetterOrDigit((char)e.KeyCode) && !e.Control)
            {
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.End || e.KeyCode == Keys.Home || e.KeyCode == Keys.Back)
            {
                // let textBox handle this
            }
            else if (e.Control && e.KeyCode == Keys.C)  // Ctrl-C copies to clipboard
            {
                System.Text.StringBuilder copy_buffer = new System.Text.StringBuilder();
                copy_buffer.AppendLine(textBoxSearch.Text);
                if (copy_buffer.Length > 0) Clipboard.SetDataObject(copy_buffer.ToString());
            }
            else if (e.Control && e.KeyCode == Keys.V)  // Ctrl-V pastes from clipboard
            {
                string text = Clipboard.GetText();
                text = text.Substring(0, Math.Min(text.Length, textBoxSearch.TextLength));
                textBoxSearch.Text = text;
            }
        }

        private static readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        private const int delayBeforeNextRefreshNoxonDisplay = 500;
        public static async Task RefreshNoxonDisplay()
        {
            while (Noxon.Busy) Thread.Sleep(100);
            if (stopwatch.IsRunning)
            {
                if (stopwatch.Elapsed.TotalMilliseconds < delayBeforeNextRefreshNoxonDisplay)
                {
                    return;
                }
                stopwatch.Restart();
            }
            else
            {
                stopwatch.Start();
            }
            await Noxon.netStream.GetNetworkStream().CommandAsync('D');  // refresh display
            await Noxon.netStream.GetNetworkStream().CommandAsync('U');
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

        private void PictureBoxFind_Click(object sender, EventArgs e)
        {
            Program.form.TextBoxSearch_KeyDown(this, new KeyEventArgs(Keys.Enter));
        }

        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (!(sender is TextBox t)) return;
            t.SelectionStart = t.Text.Length;
            t.SelectionLength = 0;
        }
    }
}                                                                            