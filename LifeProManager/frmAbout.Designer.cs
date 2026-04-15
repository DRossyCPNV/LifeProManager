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
            this.cmdValidate = new System.Windows.Forms.Button();
            this.lblLicence = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdValidate
            // 
            this.cmdValidate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdValidate.BackColor = System.Drawing.Color.Transparent;
            this.cmdValidate.BackgroundImage = global::LifeProManager.Properties.Resources.validate_filled;
            this.cmdValidate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cmdValidate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdValidate.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdValidate.FlatAppearance.BorderSize = 0;
            this.cmdValidate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.cmdValidate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdValidate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdValidate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cmdValidate.Location = new System.Drawing.Point(271, 283);
            this.cmdValidate.Margin = new System.Windows.Forms.Padding(4);
            this.cmdValidate.Name = "cmdValidate";
            this.cmdValidate.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdValidate.Size = new System.Drawing.Size(48, 32);
            this.cmdValidate.TabIndex = 8;
            this.cmdValidate.UseVisualStyleBackColor = false;
            this.cmdValidate.Click += new System.EventHandler(this.cmdConfirm_Click);
            // 
            // lblLicence
            // 
            this.lblLicence.BackColor = System.Drawing.Color.Transparent;
            this.lblLicence.Font = new System.Drawing.Font("Segoe UI Semilight", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLicence.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(61)))), ((int)(((byte)(64)))));
            this.lblLicence.Location = new System.Drawing.Point(22, 22);
            this.lblLicence.Name = "lblLicence";
            this.lblLicence.Size = new System.Drawing.Size(546, 245);
            this.lblLicence.TabIndex = 9;
            this.lblLicence.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdValidate;
            this.ClientSize = new System.Drawing.Size(593, 329);
            this.Controls.Add(this.lblLicence);
            this.Controls.Add(this.cmdValidate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAbout";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About this software";
            this.Load += new System.EventHandler(this.frmAbout_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdValidate;
        private System.Windows.Forms.Label lblLicence;
    }
}