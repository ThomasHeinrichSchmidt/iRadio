namespace iRadio
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.progressWifi = new System.Windows.Forms.ProgressBar();
            this.pictureBoxAntenna = new System.Windows.Forms.PictureBox();
            this.labelPlaying = new System.Windows.Forms.Label();
            this.progressBuffer = new System.Windows.Forms.ProgressBar();
            this.pictureBoxBuffer = new System.Windows.Forms.PictureBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pictureBoxShuffle = new System.Windows.Forms.PictureBox();
            this.pictureBoxRepeat = new System.Windows.Forms.PictureBox();
            this.listBoxDisplay = new System.Windows.Forms.ListBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.pictureBoxStop = new System.Windows.Forms.PictureBox();
            this.pictureBoxPrevious = new System.Windows.Forms.PictureBox();
            this.pictureBoxPlayPause = new System.Windows.Forms.PictureBox();
            this.pictureBoxNext = new System.Windows.Forms.PictureBox();
            this.pictureBoxAllDirections = new System.Windows.Forms.PictureBox();
            this.trackBarVolume = new System.Windows.Forms.TrackBar();
            this.listBoxFavs = new System.Windows.Forms.ListBox();
            this.labelFavs = new System.Windows.Forms.Label();
            this.pictureBoxRefresh = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAntenna)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBuffer)).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxShuffle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRepeat)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPrevious)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPlayPause)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxNext)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAllDirections)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRefresh)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.FlatAppearance.BorderSize = 2;
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.button1.Location = new System.Drawing.Point(33, 388);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(32, 32);
            this.button1.TabIndex = 0;
            this.button1.Text = "&1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // progressWifi
            // 
            this.progressWifi.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.progressWifi.Location = new System.Drawing.Point(630, 145);
            this.progressWifi.Name = "progressWifi";
            this.progressWifi.Size = new System.Drawing.Size(100, 8);
            this.progressWifi.TabIndex = 4;
            // 
            // pictureBoxAntenna
            // 
            this.pictureBoxAntenna.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxAntenna.Image")));
            this.pictureBoxAntenna.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBoxAntenna.InitialImage")));
            this.pictureBoxAntenna.Location = new System.Drawing.Point(736, 139);
            this.pictureBoxAntenna.Name = "pictureBoxAntenna";
            this.pictureBoxAntenna.Size = new System.Drawing.Size(30, 20);
            this.pictureBoxAntenna.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxAntenna.TabIndex = 5;
            this.pictureBoxAntenna.TabStop = false;
            // 
            // labelPlaying
            // 
            this.labelPlaying.AutoSize = true;
            this.labelPlaying.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelPlaying.Location = new System.Drawing.Point(33, 138);
            this.labelPlaying.Name = "labelPlaying";
            this.labelPlaying.Size = new System.Drawing.Size(38, 15);
            this.labelPlaying.TabIndex = 6;
            this.labelPlaying.Text = "01:42";
            // 
            // progressBuffer
            // 
            this.progressBuffer.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.progressBuffer.Location = new System.Drawing.Point(630, 172);
            this.progressBuffer.Name = "progressBuffer";
            this.progressBuffer.Size = new System.Drawing.Size(100, 8);
            this.progressBuffer.TabIndex = 4;
            // 
            // pictureBoxBuffer
            // 
            this.pictureBoxBuffer.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxBuffer.Image")));
            this.pictureBoxBuffer.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBoxBuffer.InitialImage")));
            this.pictureBoxBuffer.Location = new System.Drawing.Point(736, 165);
            this.pictureBoxBuffer.Name = "pictureBoxBuffer";
            this.pictureBoxBuffer.Size = new System.Drawing.Size(30, 20);
            this.pictureBoxBuffer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxBuffer.TabIndex = 5;
            this.pictureBoxBuffer.TabStop = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            this.statusStrip1.DoubleClick += new System.EventHandler(this.StatusStrip1_DoubleClick);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.AutoSize = false;
            this.toolStripStatusLabel1.DoubleClickEnabled = true;
            this.toolStripStatusLabel1.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.toolStripStatusLabel1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripStatusLabel1.Image")));
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(200, 17);
            this.toolStripStatusLabel1.Text = "connecting ...";
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelTitle.Location = new System.Drawing.Point(33, 41);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(44, 21);
            this.labelTitle.TabIndex = 8;
            this.labelTitle.Text = "Title";
            // 
            // toolTip1
            // 
            this.toolTip1.ShowAlways = true;
            // 
            // pictureBoxShuffle
            // 
            this.pictureBoxShuffle.ErrorImage = null;
            this.pictureBoxShuffle.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxShuffle.Image")));
            this.pictureBoxShuffle.InitialImage = null;
            this.pictureBoxShuffle.Location = new System.Drawing.Point(87, 139);
            this.pictureBoxShuffle.Name = "pictureBoxShuffle";
            this.pictureBoxShuffle.Size = new System.Drawing.Size(25, 25);
            this.pictureBoxShuffle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxShuffle.TabIndex = 5;
            this.pictureBoxShuffle.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBoxShuffle, "Shuffle");
            this.pictureBoxShuffle.Visible = false;
            // 
            // pictureBoxRepeat
            // 
            this.pictureBoxRepeat.ErrorImage = null;
            this.pictureBoxRepeat.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxRepeat.Image")));
            this.pictureBoxRepeat.InitialImage = null;
            this.pictureBoxRepeat.Location = new System.Drawing.Point(123, 139);
            this.pictureBoxRepeat.Name = "pictureBoxRepeat";
            this.pictureBoxRepeat.Size = new System.Drawing.Size(25, 25);
            this.pictureBoxRepeat.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxRepeat.TabIndex = 5;
            this.pictureBoxRepeat.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBoxRepeat, "Repeat This / Repeat All");
            this.pictureBoxRepeat.Visible = false;
            // 
            // listBoxDisplay
            // 
            this.listBoxDisplay.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.listBoxDisplay.Font = new System.Drawing.Font("Lucida Console", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.listBoxDisplay.FormattingEnabled = true;
            this.listBoxDisplay.ItemHeight = 16;
            this.listBoxDisplay.Items.AddRange(new object[] {
            "Artist",
            "Album",
            "Track",
            "00:00"});
            this.listBoxDisplay.Location = new System.Drawing.Point(33, 65);
            this.listBoxDisplay.Name = "listBoxDisplay";
            this.listBoxDisplay.Size = new System.Drawing.Size(733, 68);
            this.listBoxDisplay.TabIndex = 9;
            this.listBoxDisplay.SelectedIndexChanged += new System.EventHandler(this.ListBoxDisplay_SelectedIndexChanged);
            // 
            // textBox1
            // 
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(311, 39);
            this.textBox1.MaxLength = 10;
            this.textBox1.Name = "textBox1";
            this.textBox1.PlaceholderText = "Search ...";
            this.textBox1.Size = new System.Drawing.Size(161, 23);
            this.textBox1.TabIndex = 10;
            this.textBox1.Visible = false;
            this.textBox1.WordWrap = false;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox1_KeyDown);
            // 
            // pictureBoxStop
            // 
            this.pictureBoxStop.ErrorImage = null;
            this.pictureBoxStop.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxStop.Image")));
            this.pictureBoxStop.InitialImage = null;
            this.pictureBoxStop.Location = new System.Drawing.Point(191, 154);
            this.pictureBoxStop.Name = "pictureBoxStop";
            this.pictureBoxStop.Size = new System.Drawing.Size(25, 25);
            this.pictureBoxStop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxStop.TabIndex = 5;
            this.pictureBoxStop.TabStop = false;
            // 
            // pictureBoxPrevious
            // 
            this.pictureBoxPrevious.ErrorImage = null;
            this.pictureBoxPrevious.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxPrevious.Image")));
            this.pictureBoxPrevious.InitialImage = null;
            this.pictureBoxPrevious.Location = new System.Drawing.Point(251, 154);
            this.pictureBoxPrevious.Name = "pictureBoxPrevious";
            this.pictureBoxPrevious.Size = new System.Drawing.Size(25, 25);
            this.pictureBoxPrevious.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxPrevious.TabIndex = 5;
            this.pictureBoxPrevious.TabStop = false;
            // 
            // pictureBoxPlayPause
            // 
            this.pictureBoxPlayPause.ErrorImage = null;
            this.pictureBoxPlayPause.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxPlayPause.Image")));
            this.pictureBoxPlayPause.InitialImage = null;
            this.pictureBoxPlayPause.Location = new System.Drawing.Point(311, 154);
            this.pictureBoxPlayPause.Name = "pictureBoxPlayPause";
            this.pictureBoxPlayPause.Size = new System.Drawing.Size(25, 25);
            this.pictureBoxPlayPause.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxPlayPause.TabIndex = 5;
            this.pictureBoxPlayPause.TabStop = false;
            // 
            // pictureBoxNext
            // 
            this.pictureBoxNext.ErrorImage = null;
            this.pictureBoxNext.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxNext.Image")));
            this.pictureBoxNext.InitialImage = null;
            this.pictureBoxNext.Location = new System.Drawing.Point(371, 154);
            this.pictureBoxNext.Name = "pictureBoxNext";
            this.pictureBoxNext.Size = new System.Drawing.Size(25, 25);
            this.pictureBoxNext.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxNext.TabIndex = 5;
            this.pictureBoxNext.TabStop = false;
            // 
            // pictureBoxAllDirections
            // 
            this.pictureBoxAllDirections.ErrorImage = null;
            this.pictureBoxAllDirections.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxAllDirections.Image")));
            this.pictureBoxAllDirections.InitialImage = null;
            this.pictureBoxAllDirections.Location = new System.Drawing.Point(557, 139);
            this.pictureBoxAllDirections.Name = "pictureBoxAllDirections";
            this.pictureBoxAllDirections.Size = new System.Drawing.Size(50, 50);
            this.pictureBoxAllDirections.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxAllDirections.TabIndex = 5;
            this.pictureBoxAllDirections.TabStop = false;
            // 
            // trackBarVolume
            // 
            this.trackBarVolume.Enabled = false;
            this.trackBarVolume.Location = new System.Drawing.Point(431, 154);
            this.trackBarVolume.Maximum = 100;
            this.trackBarVolume.Name = "trackBarVolume";
            this.trackBarVolume.Size = new System.Drawing.Size(117, 45);
            this.trackBarVolume.TabIndex = 11;
            this.trackBarVolume.TickFrequency = 10;
            // 
            // listBoxFavs
            // 
            this.listBoxFavs.FormattingEnabled = true;
            this.listBoxFavs.ItemHeight = 15;
            this.listBoxFavs.Location = new System.Drawing.Point(33, 221);
            this.listBoxFavs.Name = "listBoxFavs";
            this.listBoxFavs.Size = new System.Drawing.Size(733, 154);
            this.listBoxFavs.TabIndex = 12;
            // 
            // labelFavs
            // 
            this.labelFavs.AutoSize = true;
            this.labelFavs.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelFavs.Location = new System.Drawing.Point(33, 197);
            this.labelFavs.Name = "labelFavs";
            this.labelFavs.Size = new System.Drawing.Size(79, 21);
            this.labelFavs.TabIndex = 8;
            this.labelFavs.Text = "Favorites";
            // 
            // pictureBoxRefresh
            // 
            this.pictureBoxRefresh.ErrorImage = null;
            this.pictureBoxRefresh.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxRefresh.Image")));
            this.pictureBoxRefresh.InitialImage = null;
            this.pictureBoxRefresh.Location = new System.Drawing.Point(123, 197);
            this.pictureBoxRefresh.Name = "pictureBoxRefresh";
            this.pictureBoxRefresh.Size = new System.Drawing.Size(25, 25);
            this.pictureBoxRefresh.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxRefresh.TabIndex = 5;
            this.pictureBoxRefresh.TabStop = false;
            this.pictureBoxRefresh.Click += new System.EventHandler(this.PictureBoxRefresh_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pictureBoxRefresh);
            this.Controls.Add(this.labelFavs);
            this.Controls.Add(this.listBoxFavs);
            this.Controls.Add(this.trackBarVolume);
            this.Controls.Add(this.pictureBoxAllDirections);
            this.Controls.Add(this.pictureBoxNext);
            this.Controls.Add(this.pictureBoxPlayPause);
            this.Controls.Add(this.pictureBoxPrevious);
            this.Controls.Add(this.pictureBoxStop);
            this.Controls.Add(this.pictureBoxRepeat);
            this.Controls.Add(this.pictureBoxShuffle);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.listBoxDisplay);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.pictureBoxBuffer);
            this.Controls.Add(this.progressBuffer);
            this.Controls.Add(this.labelPlaying);
            this.Controls.Add(this.pictureBoxAntenna);
            this.Controls.Add(this.progressWifi);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "NOXON iRadio";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAntenna)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBuffer)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxShuffle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRepeat)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPrevious)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPlayPause)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxNext)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAllDirections)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRefresh)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.ProgressBar progressWifi;
        private System.Windows.Forms.PictureBox pictureBoxAntenna;
        public System.Windows.Forms.Label labelPlaying;
        private System.Windows.Forms.PictureBox pictureBoxBuffer;
        public System.Windows.Forms.ProgressBar progressBuffer;
        private System.Windows.Forms.StatusStrip statusStrip1;
        public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        public System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.ToolTip toolTip1;
        public System.Windows.Forms.ListBox listBoxDisplay;
        public System.Windows.Forms.TextBox textBox1;
        public System.Windows.Forms.Button button1;
        public System.Windows.Forms.PictureBox pictureBoxShuffle;
        public System.Windows.Forms.PictureBox pictureBoxRepeat;
        public System.Windows.Forms.PictureBox pictureBoxStop;
        public System.Windows.Forms.PictureBox pictureBoxPrevious;
        public System.Windows.Forms.PictureBox pictureBoxPlayPause;
        public System.Windows.Forms.PictureBox pictureBoxNext;
        public System.Windows.Forms.PictureBox pictureBoxAllDirections;
        public System.Windows.Forms.TrackBar trackBarVolume;
        private System.Windows.Forms.Label labelFavs;
        public System.Windows.Forms.ListBox listBoxFavs;
        private System.Windows.Forms.PictureBox pictureBoxRefresh;
    }
}

