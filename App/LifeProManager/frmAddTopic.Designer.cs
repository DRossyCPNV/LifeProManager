namespace LifeProManager
{
    partial class frmAddTopic
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
            this.txtTopic = new System.Windows.Forms.TextBox();
            this.lblTopic = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdAddTopic = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtTopic
            // 
            this.txtTopic.Location = new System.Drawing.Point(79, 17);
            this.txtTopic.Name = "txtTopic";
            this.txtTopic.Size = new System.Drawing.Size(198, 20);
            this.txtTopic.TabIndex = 0;
            this.txtTopic.Text = "Nouveau thème";
            // 
            // lblTopic
            // 
            this.lblTopic.AutoSize = true;
            this.lblTopic.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblTopic.ForeColor = System.Drawing.Color.Black;
            this.lblTopic.Location = new System.Drawing.Point(18, 17);
            this.lblTopic.Name = "lblTopic";
            this.lblTopic.Size = new System.Drawing.Size(58, 20);
            this.lblTopic.TabIndex = 1;
            this.lblTopic.Text = "Thème";
            // 
            // cmdCancel
            // 
            this.cmdCancel.BackColor = System.Drawing.Color.Transparent;
            this.cmdCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cmdCancel.FlatAppearance.BorderSize = 0;
            this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdCancel.Image = global::LifeProManager.Properties.Resources.cancel;
            this.cmdCancel.Location = new System.Drawing.Point(171, 69);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(42, 42);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.UseVisualStyleBackColor = false;
            // 
            // cmdAddTopic
            // 
            this.cmdAddTopic.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddTopic.BackColor = System.Drawing.Color.Transparent;
            this.cmdAddTopic.BackgroundImage = global::LifeProManager.Properties.Resources.validate;
            this.cmdAddTopic.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cmdAddTopic.FlatAppearance.BorderSize = 0;
            this.cmdAddTopic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdAddTopic.Location = new System.Drawing.Point(238, 69);
            this.cmdAddTopic.Name = "cmdAddTopic";
            this.cmdAddTopic.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdAddTopic.Size = new System.Drawing.Size(42, 42);
            this.cmdAddTopic.TabIndex = 2;
            this.cmdAddTopic.UseVisualStyleBackColor = false;
            this.cmdAddTopic.Click += new System.EventHandler(this.cmdAddTopic_Click);
            // 
            // frmAddTopic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(168)))), ((int)(((byte)(208)))), ((int)(((byte)(230)))));
            this.ClientSize = new System.Drawing.Size(292, 123);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdAddTopic);
            this.Controls.Add(this.lblTopic);
            this.Controls.Add(this.txtTopic);
            this.Name = "frmAddTopic";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ajouter un thème";
            this.Load += new System.EventHandler(this.frmAddTopic_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtTopic;
        private System.Windows.Forms.Label lblTopic;
        private System.Windows.Forms.Button cmdAddTopic;
        private System.Windows.Forms.Button cmdCancel;
    }
}