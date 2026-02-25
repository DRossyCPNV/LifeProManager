namespace LifeProManager
{
    partial class frmAbout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAbout));
            this.cmdConfirm = new System.Windows.Forms.Button();
            this.lblLicence = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdConfirm
            // 
            this.cmdConfirm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdConfirm.BackColor = System.Drawing.Color.Transparent;
            this.cmdConfirm.BackgroundImage = global::LifeProManager.Properties.Resources.validate_filled;
            this.cmdConfirm.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cmdConfirm.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdConfirm.FlatAppearance.BorderSize = 0;
            this.cmdConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdConfirm.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cmdConfirm.Location = new System.Drawing.Point(271, 283);
            this.cmdConfirm.Margin = new System.Windows.Forms.Padding(4);
            this.cmdConfirm.Name = "cmdConfirm";
            this.cmdConfirm.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdConfirm.Size = new System.Drawing.Size(48, 32);
            this.cmdConfirm.TabIndex = 8;
            this.cmdConfirm.UseVisualStyleBackColor = false;
            this.cmdConfirm.Click += new System.EventHandler(this.cmdConfirm_Click);
            // 
            // lblLicence
            // 
            this.lblLicence.BackColor = System.Drawing.Color.Transparent;
            this.lblLicence.Font = new System.Drawing.Font("Segoe UI Semilight", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLicence.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(58)))), ((int)(((byte)(71)))));
            this.lblLicence.Location = new System.Drawing.Point(22, 22);
            this.lblLicence.Name = "lblLicence";
            this.lblLicence.Size = new System.Drawing.Size(546, 245);
            this.lblLicence.TabIndex = 9;
            this.lblLicence.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmAbout
            // 
            this.AcceptButton = this.cmdConfirm;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdConfirm;
            this.ClientSize = new System.Drawing.Size(593, 329);
            this.Controls.Add(this.lblLicence);
            this.Controls.Add(this.cmdConfirm);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAbout";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About this software";
            this.Load += new System.EventHandler(this.frmAbout_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdConfirm;
        private System.Windows.Forms.Label lblLicence;
    }
}