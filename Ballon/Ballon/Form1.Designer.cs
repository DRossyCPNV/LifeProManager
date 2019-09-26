namespace Ballon
{
    partial class frmBallon
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cmdStart = new System.Windows.Forms.Button();
            this.pctBallon = new System.Windows.Forms.PictureBox();
            this.tmrTicTac = new System.Windows.Forms.Timer(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pctBallon)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdStart
            // 
            this.cmdStart.Location = new System.Drawing.Point(12, 676);
            this.cmdStart.Name = "cmdStart";
            this.cmdStart.Size = new System.Drawing.Size(75, 23);
            this.cmdStart.TabIndex = 0;
            this.cmdStart.Text = "Start";
            this.cmdStart.UseVisualStyleBackColor = true;
            this.cmdStart.Click += new System.EventHandler(this.cmdStart_Click);
            // 
            // pctBallon
            // 
            this.pctBallon.Image = global::Ballon.Properties.Resources.BallonBleu;
            this.pctBallon.Location = new System.Drawing.Point(108, 93);
            this.pctBallon.Name = "pctBallon";
            this.pctBallon.Size = new System.Drawing.Size(55, 50);
            this.pctBallon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pctBallon.TabIndex = 1;
            this.pctBallon.TabStop = false;
            this.pctBallon.Visible = false;
            // 
            // tmrTicTac
            // 
            this.tmrTicTac.Interval = 25;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // frmBallon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(984, 711);
            this.Controls.Add(this.pctBallon);
            this.Controls.Add(this.cmdStart);
            this.Name = "frmBallon";
            this.Text = "Ballon";
            ((System.ComponentModel.ISupportInitialize)(this.pctBallon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdStart;
        private System.Windows.Forms.PictureBox pctBallon;
        private System.Windows.Forms.Timer tmrTicTac;
        private System.Windows.Forms.Timer timer1;
    }
}

