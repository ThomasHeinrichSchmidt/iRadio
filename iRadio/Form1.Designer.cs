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
            this.listBoxDisplay = new System.Windows.Forms.ListBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAntenna)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBuffer)).BeginInit();
            this.statusStrip1.SuspendLayout();
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
            this.textBox1.Location = new System.Drawing.Point(191, 43);
            this.textBox1.MaxLength = 10;
            this.textBox1.Name = "textBox1";
            this.textBox1.PlaceholderText = "Search ...";
            this.textBox1.Size = new System.Drawing.Size(161, 23);
            this.textBox1.TabIndex = 10;
            this.textBox1.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
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
    }
}

