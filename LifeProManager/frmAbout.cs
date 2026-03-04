/// <file>frmAbout.cs</file>
/// <author>Laurent Barraud</author>
/// <version>1.8</version>
/// <date>March 4th, 2026</date>

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

            // Cloud gradient 
            Color leftColor = Color.FromArgb(153, 248, 252, 252);
            Color rightColor = Color.FromArgb(153, 31, 61, 57);

            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                leftColor,
                rightColor,
                LinearGradientMode.ForwardDiagonal))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }
    }
}
