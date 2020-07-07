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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.progressWifi = new System.Windows.Forms.ProgressBar();
            this.pictureBoxAntenna = new System.Windows.Forms.PictureBox();
            this.labelPlaying = new System.Windows.Forms.Label();
            this.progressBuffer = new System.Windows.Forms.ProgressBar();
            this.pictureBoxBuffer = new System.Windows.Forms.PictureBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
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
            // listBox1
            // 
            this.listBox1.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 11;
            this.listBox1.Location = new System.Drawing.Point(33, 21);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(657, 301);
            this.listBox1.TabIndex = 1;
            // 
            // progressWifi
            // 
            this.progressWifi.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.progressWifi.Location = new System.Drawing.Point(554, 338);
            this.progressWifi.Name = "progressWifi";
            this.progressWifi.Size = new System.Drawing.Size(100, 10);
            this.progressWifi.TabIndex = 4;
            // 
            // pictureBoxAntenna
            // 
            this.pictureBoxAntenna.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxAntenna.Image")));
            this.pictureBoxAntenna.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBoxAntenna.InitialImage")));
            this.pictureBoxAntenna.Location = new System.Drawing.Point(660, 328);
            this.pictureBoxAntenna.Name = "pictureBoxAntenna";
            this.pictureBoxAntenna.Size = new System.Drawing.Size(30, 20);
            this.pictureBoxAntenna.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxAntenna.TabIndex = 5;
            this.pictureBoxAntenna.TabStop = false;
            // 
            // labelPlaying
            // 
            this.labelPlaying.AutoSize = true;
            this.labelPlaying.Location = new System.Drawing.Point(336, 338);
            this.labelPlaying.Name = "labelPlaying";
            this.labelPlaying.Size = new System.Drawing.Size(34, 15);
            this.labelPlaying.TabIndex = 6;
            this.labelPlaying.Text = "01:42";
            // 
            // progressBuffer
            // 
            this.progressBuffer.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.progressBuffer.Location = new System.Drawing.Point(554, 388);
            this.progressBuffer.Name = "progressBuffer";
            this.progressBuffer.Size = new System.Drawing.Size(100, 10);
            this.progressBuffer.TabIndex = 4;
            // 
            // pictureBoxBuffer
            // 
            this.pictureBoxBuffer.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxBuffer.Image")));
            this.pictureBoxBuffer.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBoxBuffer.InitialImage")));
            this.pictureBoxBuffer.Location = new System.Drawing.Point(660, 378);
            this.pictureBoxBuffer.Name = "pictureBoxBuffer";
            this.pictureBoxBuffer.Size = new System.Drawing.Size(30, 20);
            this.pictureBoxBuffer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxBuffer.TabIndex = 5;
            this.pictureBoxBuffer.TabStop = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSplitButton1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBox1});
            this.toolStripSplitButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton1.Image")));
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(32, 20);
            this.toolStripSplitButton1.Text = "toolStripSplitButton1";
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(100, 23);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.AutoSize = false;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(100, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.pictureBoxBuffer);
            this.Controls.Add(this.progressBuffer);
            this.Controls.Add(this.labelPlaying);
            this.Controls.Add(this.pictureBoxAntenna);
            this.Controls.Add(this.progressWifi);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "NOXON iRadio";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAntenna)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBuffer)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        public System.Windows.Forms.ListBox listBox1;
        public System.Windows.Forms.ProgressBar progressWifi;
        private System.Windows.Forms.PictureBox pictureBoxAntenna;
        public System.Windows.Forms.Label labelPlaying;
        private System.Windows.Forms.PictureBox pictureBoxBuffer;
        public System.Windows.Forms.ProgressBar progressBuffer;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}

