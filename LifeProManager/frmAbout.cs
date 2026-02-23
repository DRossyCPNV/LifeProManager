/// <file>frmAbout.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon</author>
/// <version>1.7.1</version>
/// <date>February 24th, 2026</date>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LifeProManager
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
        }

        private void frmAbout_Load(object sender, EventArgs e)
        {
            LocalizationManager.LoadLocalizedStringsFor(this);
        }
        private void cmdConfirm_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void LoadLocalizedStrings()
        {
            // --- Window title ---
            this.Text = LocalizationManager.GetString("AboutThisApp");

            // --- Label ---
            lblLicence.Text = LocalizationManager.GetString("lblLicenceText");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Gradient colors with 80% opacity
            Color leftColor = Color.FromArgb(204, 0xAC, 0xCF, 0xDD);
            Color rightColor = Color.FromArgb(204, 0x5D, 0x82, 0xA8);

            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                leftColor,
                rightColor,
                LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }
    }
}
