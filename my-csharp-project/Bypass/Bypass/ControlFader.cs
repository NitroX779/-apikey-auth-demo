using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms; // for Guna2 controls

namespace Bypass    
{
    public static class ControlFader
    {
        public static void FadeOut(Guna2GradientButton btn, int interval = 30, int step = 15)
        {
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = interval;

            timer.Tick += (s, e) =>
            {
                if (btn.FillColor.A > step)
                {
                    Color c1 = Color.FromArgb(btn.FillColor.A - step, btn.FillColor);
                    Color c2 = Color.FromArgb(btn.FillColor2.A - step, btn.FillColor2);

                    btn.FillColor = c1;
                    btn.FillColor2 = c2;
                }
                else
                {
                    btn.Visible = false;
                    timer.Stop();
                    timer.Dispose();
                }
            };

            timer.Start();
        }

        public static void FadeIn(Guna2GradientButton btn, int interval = 30, int step = 15)
        {
            btn.Visible = true;

            // Start transparent
            btn.FillColor = Color.FromArgb(0, btn.FillColor);
            btn.FillColor2 = Color.FromArgb(0, btn.FillColor2);

            var timer = new System.Windows.Forms.Timer();
            timer.Interval = interval;

            timer.Tick += (s, e) =>
            {
                if (btn.FillColor.A < 255 - step)
                {
                    Color c1 = Color.FromArgb(btn.FillColor.A + step, btn.FillColor);
                    Color c2 = Color.FromArgb(btn.FillColor2.A + step, btn.FillColor2);

                    btn.FillColor = c1;
                    btn.FillColor2 = c2;
                }
                else
                {
                    // Cap at fully opaque
                    btn.FillColor = Color.FromArgb(255, btn.FillColor);
                    btn.FillColor2 = Color.FromArgb(255, btn.FillColor2);

                    timer.Stop();
                    timer.Dispose();
                }
            };

            timer.Start();
        }
    }
}
