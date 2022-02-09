using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LifeProManager
{
    public class SkinApplier
    {
        /// <summary>
        /// Applies a skin to a form and its controls
        /// </summary>
        /// <param name="idThemeToApply">The id of the theme to apply</param>
        /// <param name="idOpenForms">The id of the open form</param>
        public static void ApplyTheme(int idThemeToApply)
        {
           foreach (Form formToApplyTheme in Application.OpenForms)
            {
                foreach (Control controlToEdit in formToApplyTheme.Controls)
                {
                    UpdateColorControls(controlToEdit, idThemeToApply);
                }

                // If dark theme will be applied
                if (idThemeToApply == 1)
                {
                    formToApplyTheme.BackColor = Color.FromArgb(32, 33, 36);
                }

                // By default light theme will be applied
                else
                {
                    formToApplyTheme.BackColor = Color.FromArgb(230, 235, 239);
                }
            }     
        }

        public static void UpdateColorControls(Control controlToColor, int idThemeToApply)
        {
            if (controlToColor is TabPage)
            {
                // If dark theme will be applied
                if (idThemeToApply == 1)
                {
                    // Sets it on dark grey
                    controlToColor.BackColor = Color.FromArgb(32, 33, 36);
                }
                // By default light theme will be applied
                else
                {
                    // Sets it on light blue
                    controlToColor.BackColor = Color.FromArgb(230, 235, 239);
                }

            }

            else if (controlToColor is Panel)
            {
                // If dark theme will be applied
                if (idThemeToApply == 1)
                {
                    // Sets it on dark grey
                    controlToColor.BackColor = Color.FromArgb(32, 33, 36);
                }
                // By default light theme will be applied
                else
                {
                    controlToColor.BackColor = Color.White;
                }
            }

            else if (controlToColor is Label) 
            {
                // If dark theme will be applied
                if (idThemeToApply == 1)
                {
                    // Sets it on dark grey
                    controlToColor.BackColor = Color.FromArgb(32, 33, 36);

                    // Sets it on light grey
                    controlToColor.ForeColor = Color.FromArgb(232, 234, 237);
                }
                // By default light theme will be applied
                else
                {
                    controlToColor.BackColor = Color.Transparent;
                    controlToColor.ForeColor = Color.Black;
                }
            }

            else if (controlToColor is TextBox)
            {
                // If dark theme will be applied
                if (idThemeToApply == 1)
                {
                    // Sets it on dark grey
                    controlToColor.BackColor = Color.FromArgb(32, 33, 36);

                    // Sets it on light grey
                    controlToColor.ForeColor = Color.FromArgb(232, 234, 237);
                }

                // By default light theme will be applied
                else
                {
                    controlToColor.BackColor = Color.White;
                    controlToColor.ForeColor = Color.Black;
                }
            }

            else if (controlToColor is ComboBox)
            {
                // If dark theme will be applied
                if (idThemeToApply == 1)
                {
                    // Sets it on dark grey
                    controlToColor.BackColor = Color.FromArgb(32, 33, 36);

                    // Sets it on light blue;
                    controlToColor.ForeColor = Color.FromArgb(138, 180, 248);
                }
                // By default light theme will be applied
                else
                {
                    // Sets it on the active window's system color
                    controlToColor.BackColor = Color.FromKnownColor(KnownColor.Window);
                    controlToColor.ForeColor = Color.Black;
                }
            }

            else if (controlToColor is CheckBox)
            {
                // If dark theme will be applied
                if (idThemeToApply == 1)
                {
                    // Sets it on dark grey
                    controlToColor.BackColor = Color.FromArgb(32, 33, 36);

                    // Sets it on light grey
                    controlToColor.ForeColor = Color.FromArgb(232, 234, 237);
                }
                // By default light theme will be applied
                else
                {
                    // Sets it on light-greyed blue
                    controlToColor.BackColor = Color.FromArgb(230, 235, 239);
                    controlToColor.ForeColor = Color.Black;
                }
            }

            else if (controlToColor is Button)
            {
                // If dark theme will be applied
                if (idThemeToApply == 1)
                {
                    // Sets it on dark grey
                    controlToColor.BackColor = Color.FromArgb(32, 33, 36);
                }
                // By default light theme will be applied
                else
                {
                    controlToColor.BackColor = Color.Transparent;
                }

            }  

            // Calls the update function in a recursive way to also theme the controls in tab pages
            foreach (Control subControl in controlToColor.Controls)
            {
                UpdateColorControls(subControl, idThemeToApply);
            }
        }    
    }
}
