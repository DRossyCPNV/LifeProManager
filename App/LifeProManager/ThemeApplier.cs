/// <file>frmMain.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.3</version>
/// <date>February 13th, 2022</date>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LifeProManager
{
    public class ThemeApplier
    {
        /// <summary>
        /// Applies a theme to a form and its controls
        /// </summary>
        /// <param name="idThemeToApply">The id of the theme to apply</param>
        /// <param name="formToApplyTheme">The form on which theme will be applied</param>
        public static void ApplyTheme(int idThemeToApply, Form formToApplyTheme)
        {
            // If dark theme will be applied
            if (idThemeToApply == 1)
            {
                if (formToApplyTheme.BackColor != Color.Black)
                {
                    formToApplyTheme.BackColor = Color.Black;

                    foreach (Control controlToEdit in formToApplyTheme.Controls)
                    {
                        UpdateColorControls(controlToEdit, idThemeToApply);
                    }
                }                    
            }

            // By default light theme will be applied
            else
            {
                if (formToApplyTheme.BackColor != Color.FromArgb(230, 235, 239))
                {
                    // Sets it on light blue
                    formToApplyTheme.BackColor = Color.FromArgb(230, 235, 239);

                    foreach (Control controlToEdit in formToApplyTheme.Controls)
                    {
                        UpdateColorControls(controlToEdit, idThemeToApply);
                    }
                }      
            }    
        }

        public static void UpdateColorControls(Control controlToColor, int idThemeToApply)
        {
            // If dark theme will be applied
            if (idThemeToApply == 1)
            {        
                if (controlToColor is Panel)
                {
                    // Sets it on dark grey
                    controlToColor.BackColor = Color.FromArgb(32, 33, 36);

                    foreach (Control labelToApply in controlToColor.Controls)
                    {
                        // Sets it on light grey
                        labelToApply.ForeColor = Color.FromArgb(232, 234, 237);
                    }
                }

                else if (controlToColor is Label)
                {
                    // Sets it on dark grey
                    controlToColor.BackColor = Color.FromArgb(32, 33, 36);

                    // Sets it on light grey
                    controlToColor.ForeColor = Color.FromArgb(232, 234, 237);
                }

                else if (controlToColor is TextBox)
                {
                    // Sets it on dark grey
                    controlToColor.BackColor = Color.FromArgb(32, 33, 36);

                    // Sets it on light grey
                    controlToColor.ForeColor = Color.FromArgb(232, 234, 237);
                }

                else if (controlToColor is ComboBox)
                {
                    // Sets it on dark grey
                    controlToColor.BackColor = Color.FromArgb(32, 33, 36);

                    // Sets it on light blue;
                    controlToColor.ForeColor = Color.FromArgb(138, 180, 248);
                }

                else if (controlToColor is CheckBox)
                {
                    // Sets it on dark grey
                    controlToColor.BackColor = Color.FromArgb(32, 33, 36);

                    // Sets it on light grey
                    controlToColor.ForeColor = Color.FromArgb(232, 234, 237);
                }

                else if (controlToColor is TabControl)
                {
                    foreach (Control tabPage in controlToColor.Controls)
                    {
                        tabPage.BackColor = Color.Black;

                        foreach (Control subControlToColor in tabPage.Controls)
                        {
                            UpdateColorControls(subControlToColor, 1);
                        }
                    }
                }
            }

            // By default light theme will be applied
            else
            {
                if (controlToColor is Panel)
                {
                    controlToColor.BackColor = Color.White;
                    
                    foreach (Control labelToApply in controlToColor.Controls)
                    {
                        labelToApply.ForeColor = Color.Black;
                    }
                }

                else if (controlToColor is Label)
                {
                    controlToColor.BackColor = Color.Transparent;
                    controlToColor.ForeColor = Color.Black;
                }

                else if (controlToColor is TextBox)
                {
                    controlToColor.BackColor = Color.White;
                    controlToColor.ForeColor = Color.Black;
                }

                else if (controlToColor is ComboBox)
                {
                    // Sets it on the active window's system color
                    controlToColor.BackColor = Color.FromKnownColor(KnownColor.Window);
                    controlToColor.ForeColor = Color.Black;
                }

                else if (controlToColor is CheckBox)
                {
                    // Sets it on light-greyed blue
                    controlToColor.BackColor = Color.FromArgb(230, 235, 239);
                    controlToColor.ForeColor = Color.Black;
                }

                else if (controlToColor is TabControl)
                {
                    foreach (Control tabPage in controlToColor.Controls)
                    {
                        // Sets it on light blue
                        tabPage.BackColor = Color.FromArgb(230, 235, 239);

                        foreach (Control subControlToColor in tabPage.Controls)
                        {
                            UpdateColorControls(subControlToColor, 0);
                        }
                    }
                }
            }
        }    
    }
}
