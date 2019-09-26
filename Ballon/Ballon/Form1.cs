using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Ballon
{
    public partial class frmBallon : Form
    {
        public frmBallon()
        {
            InitializeComponent();
        }

        int x = 0;
        int y = 0;

        private void cmdStart_Click(object sender, EventArgs e)
        {
            Random random = new Random();
            x = random.Next(50, (frmBallon.ActiveForm.Width - 50));
            y = random.Next(50, (frmBallon.ActiveForm.Height - 50));

            pctBallon.SetBounds(x, y, pctBallon.Width, pctBallon.Height);
            pctBallon.Visible = true;

            timer1.Interval = 5;
            timer1.Enabled = true;

            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            --y;
            if (y > 50) {
                pctBallon.SetBounds(x, y, pctBallon.Width, pctBallon.Height);
            }
            else {
                timer1.Stop();
            }
        }
    }
}
