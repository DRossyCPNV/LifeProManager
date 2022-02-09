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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAddTopic));
            this.txtTopic = new System.Windows.Forms.TextBox();
            this.lblTopic = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdAddTopic = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtTopic
            // 
            resources.ApplyResources(this.txtTopic, "txtTopic");
            this.txtTopic.Name = "txtTopic";
            // 
            // lblTopic
            // 
            resources.ApplyResources(this.lblTopic, "lblTopic");
            this.lblTopic.ForeColor = System.Drawing.Color.Black;
            this.lblTopic.Name = "lblTopic";
            // 
            // cmdCancel
            // 
            resources.ApplyResources(this.cmdCancel, "cmdCancel");
            this.cmdCancel.BackColor = System.Drawing.Color.Transparent;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.FlatAppearance.BorderSize = 0;
            this.cmdCancel.Image = global::LifeProManager.Properties.Resources.cancel;
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.UseVisualStyleBackColor = false;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdAddTopic
            // 
            resources.ApplyResources(this.cmdAddTopic, "cmdAddTopic");
            this.cmdAddTopic.BackColor = System.Drawing.Color.Transparent;
            this.cmdAddTopic.BackgroundImage = global::LifeProManager.Properties.Resources.validate;
            this.cmdAddTopic.FlatAppearance.BorderSize = 0;
            this.cmdAddTopic.Name = "cmdAddTopic";
            this.cmdAddTopic.UseVisualStyleBackColor = false;
            this.cmdAddTopic.Click += new System.EventHandler(this.cmdAddTopic_Click);
            // 
            // frmAddTopic
            // 
            this.AcceptButton = this.cmdAddTopic;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(235)))), ((int)(((byte)(239)))));
            this.CancelButton = this.cmdCancel;
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdAddTopic);
            this.Controls.Add(this.lblTopic);
            this.Controls.Add(this.txtTopic);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmAddTopic";
            this.Load += new System.EventHandler(this.frmAddTopic_Load);
            this.Move += new System.EventHandler(this.frmAddTopic_Move);
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