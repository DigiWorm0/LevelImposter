namespace LevelImposter
{
    partial class LevelImposter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LevelImposter));
            this.applyButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.revertButton = new System.Windows.Forms.Button();
            this.logo = new System.Windows.Forms.PictureBox();
            this.logoBeta = new System.Windows.Forms.Label();
            this.browseButton = new System.Windows.Forms.Button();
            this.mapLabel = new System.Windows.Forms.Label();
            this.browseDialog = new System.Windows.Forms.OpenFileDialog();
            this.gameDirDialog = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.logo)).BeginInit();
            this.SuspendLayout();
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(107)))), ((int)(((byte)(230)))));
            this.applyButton.Enabled = false;
            this.applyButton.FlatAppearance.BorderSize = 0;
            this.applyButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.applyButton.Font = new System.Drawing.Font("Bahnschrift SemiBold", 11F, System.Drawing.FontStyle.Bold);
            this.applyButton.ForeColor = System.Drawing.Color.White;
            this.applyButton.Location = new System.Drawing.Point(12, 130);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(283, 37);
            this.applyButton.TabIndex = 4;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = false;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Cursor = System.Windows.Forms.Cursors.Default;
            this.progressBar.Location = new System.Drawing.Point(12, 104);
            this.progressBar.MarqueeAnimationSpeed = 40;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(397, 20);
            this.progressBar.TabIndex = 9;
            // 
            // revertButton
            // 
            this.revertButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.revertButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(193)))), ((int)(((byte)(49)))), ((int)(((byte)(49)))));
            this.revertButton.FlatAppearance.BorderSize = 0;
            this.revertButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.revertButton.Font = new System.Drawing.Font("Bahnschrift SemiBold", 11F, System.Drawing.FontStyle.Bold);
            this.revertButton.ForeColor = System.Drawing.Color.White;
            this.revertButton.Location = new System.Drawing.Point(311, 130);
            this.revertButton.Name = "revertButton";
            this.revertButton.Size = new System.Drawing.Size(98, 37);
            this.revertButton.TabIndex = 6;
            this.revertButton.Text = "Revert";
            this.revertButton.UseVisualStyleBackColor = false;
            // 
            // logo
            // 
            this.logo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logo.Image = ((System.Drawing.Image)(resources.GetObject("logo.Image")));
            this.logo.Location = new System.Drawing.Point(49, -2);
            this.logo.Name = "logo";
            this.logo.Size = new System.Drawing.Size(293, 70);
            this.logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.logo.TabIndex = 10;
            this.logo.TabStop = false;
            // 
            // logoBeta
            // 
            this.logoBeta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.logoBeta.AutoSize = true;
            this.logoBeta.Font = new System.Drawing.Font("Bahnschrift SemiLight", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logoBeta.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.logoBeta.Location = new System.Drawing.Point(326, 33);
            this.logoBeta.Name = "logoBeta";
            this.logoBeta.Size = new System.Drawing.Size(74, 35);
            this.logoBeta.TabIndex = 11;
            this.logoBeta.Text = "Beta";
            // 
            // browseButton
            // 
            this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseButton.BackColor = System.Drawing.Color.DimGray;
            this.browseButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.browseButton.FlatAppearance.BorderSize = 0;
            this.browseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.browseButton.Font = new System.Drawing.Font("Bahnschrift SemiBold", 11F, System.Drawing.FontStyle.Bold);
            this.browseButton.ForeColor = System.Drawing.Color.White;
            this.browseButton.Location = new System.Drawing.Point(311, 61);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(98, 37);
            this.browseButton.TabIndex = 13;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = false;
            // 
            // mapLabel
            // 
            this.mapLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mapLabel.Font = new System.Drawing.Font("Bahnschrift SemiLight", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mapLabel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.mapLabel.Location = new System.Drawing.Point(7, 61);
            this.mapLabel.Name = "mapLabel";
            this.mapLabel.Size = new System.Drawing.Size(288, 37);
            this.mapLabel.TabIndex = 14;
            this.mapLabel.Text = "No Map Selected";
            this.mapLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LevelImposter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(421, 177);
            this.Controls.Add(this.mapLabel);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.logoBeta);
            this.Controls.Add(this.logo);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.revertButton);
            this.Controls.Add(this.applyButton);
            this.Font = new System.Drawing.Font("Bahnschrift SemiLight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(437, 216);
            this.Name = "LevelImposter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Level Imposter v0.1.0";
            ((System.ComponentModel.ISupportInitialize)(this.logo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.Button applyButton;
        public System.Windows.Forms.Button revertButton;
        public System.Windows.Forms.ProgressBar progressBar;
        public System.Windows.Forms.PictureBox logo;
        public System.Windows.Forms.Label logoBeta;
        public System.Windows.Forms.Button browseButton;
        public System.Windows.Forms.Label mapLabel;
        public System.Windows.Forms.OpenFileDialog browseDialog;
        public System.Windows.Forms.OpenFileDialog gameDirDialog;
    }
}

