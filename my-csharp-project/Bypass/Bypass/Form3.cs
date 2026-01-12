using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;

namespace Bypass
{
    public partial class Form3 : Form
    {
        // Store original positions
        private Point originalTextBoxPosition;
        private Point originalButtonPosition;
        // Drag variables
        private bool isDragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        
        public Form3()
        {
            InitializeComponent();
            // Store original positions
            originalTextBoxPosition = guna2TextBox1.Location;
            originalButtonPosition = guna2Button1.Location;
            
            // Setup drag functionality
            SetupDragFunctionality();
            
            // Setup initial state with enhanced animations
            SetupInitialAnimations();
            // Start background animations
            StartBackgroundEffects();
        }

        private void SetupInitialAnimations()
        {
            // Initially hide the controls
            guna2TextBox1.Visible = false;
            guna2Button1.Visible = false;
            
            // Apply glow effect to toggle switch
            ApplyGlowEffect(guna2ToggleSwitch1);
            
            // Add subtle pulse animation to toggle switch
            _ = Task.Run(() => AddPulseAnimation(guna2ToggleSwitch1));
        }

        private void StartBackgroundEffects()
        {
            // Apply gradient background effect like login page
            ApplyPanelGradients();
            
            // Start color cycling animation
            _ = Task.Run(ColorCycleAnimation);
        }

        private void ApplyPanelGradients()
        {
            // Apply dark gradient similar to login page
            if (guna2Panel3 != null)
            {
                guna2Panel3.BackColor = Color.FromArgb(10, 10, 10);
            }
            
            // Style guna2Panel1 with rounded borders only
            if (guna2Panel1 != null)
            {
                guna2Panel1.BorderRadius = 15;
            }
        }

        private async Task ColorCycleAnimation()
        {
            // Only animate guna2Panel3 background
            Color[] colors = {
                Color.FromArgb(39, 174, 96),
                Color.FromArgb(46, 204, 113),
                Color.FromArgb(39, 174, 96),
                Color.FromArgb(30, 150, 80)
            };
            
            int colorIndex = 0;
            
            while (true)
            {
                try
                {
                    if (this.IsHandleCreated)
                    {
                        colorIndex = (colorIndex + 1) % colors.Length;
                        await Task.Delay(3000);
                    }
                    else
                    {
                        break;
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        private void ApplyGlowEffect(Control control)
        {
            // This would require custom painting or using Bunifu/Guna effects
            // For now, we'll enhance the visual appearance
        }

        private async Task AddPulseAnimation(Control control)
        {
            while (true)
            {
                try
                {
                    if (control.IsHandleCreated)
                    {
                        // Subtle scaling effect
                        control.Invoke((MethodInvoker)delegate {
                            control.Scale(new SizeF(1.02f, 1.02f));
                        });
                        await Task.Delay(1000);
                        control.Invoke((MethodInvoker)delegate {
                            control.Scale(new SizeF(0.98f, 0.98f));
                        });
                        await Task.Delay(1000);
                    }
                    else
                    {
                        break;
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        private string GetHWID()
        {
            string hwid = "";
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    hwid = obj["ProcessorId"].ToString();
                    break;
                }
            }
            catch
            {
                hwid = "Unknown";
            }
            return hwid;
        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {
            // Add subtle radial gradient effect like login page
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                guna2Panel3.ClientRectangle,
                Color.FromArgb(10, 10, 10),
                Color.FromArgb(15, 15, 23),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, guna2Panel3.ClientRectangle);
            }
            
            // Add floating particles effect
            DrawFloatingParticles(e.Graphics);
        }


        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            // Add text change effects similar to login page
            if (!string.IsNullOrEmpty(guna2TextBox1.Text))
            {
                // Green glow effect when text is entered
                guna2TextBox1.FocusedState.BorderColor = Color.FromArgb(39, 174, 96);
            }
            else
            {
                // Reset to default
                guna2TextBox1.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            }
        }

        private void guna2Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            // Add interactive glow effect on mouse move
            if (guna2Panel1 != null)
            {
                // Increase shadow intensity near mouse
                float distance = Distance(e.Location, new Point(guna2Panel1.Width/2, guna2Panel1.Height/2));
                float intensity = Math.Max(0, 1 - (distance / (guna2Panel1.Width/2)));
                
                guna2Panel1.ShadowDecoration.Depth = (int)(10 + intensity * 10);
            }
        }

        private void guna2Panel1_MouseLeave(object sender, EventArgs e)
        {
            // Reset shadow when mouse leaves
            if (guna2Panel1 != null)
            {
                guna2Panel1.ShadowDecoration.Depth = 10;
            }
        }

        private float Distance(Point p1, Point p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private void DrawFloatingParticles(Graphics g)
        {
            // Draw subtle floating particles like login page
            Random rand = new Random();
            
            for (int i = 0; i < 15; i++)
            {
                float x = rand.Next(0, guna2Panel3.Width);
                float y = rand.Next(0, guna2Panel3.Height);
                float size = rand.Next(1, 3);
                
                using (var brush = new SolidBrush(Color.FromArgb(30, 39, 174, 96)))
                {
                    g.FillEllipse(brush, x, y, size, size);
                }
            }
        }

        private void SetupDragFunctionality()
        {
            if (guna2Panel3 != null)
            {
                guna2Panel3.MouseDown += Guna2Panel3_MouseDown;
                guna2Panel3.MouseMove += Guna2Panel3_MouseMove;
                guna2Panel3.MouseUp += Guna2Panel3_MouseUp;
            }
        }

        private void Guna2Panel3_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void Guna2Panel3_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }

        private void Guna2Panel3_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {
            // Keep default painting - no custom effects
        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            string apiKey = guna2TextBox1.Text;
            string hwid = GetHWID();
            if (string.IsNullOrEmpty(apiKey))
            {
                label2.Text = "Inserisci una chiave API";
                label2.ForeColor = Color.Red;
                return;
            }

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                client.DefaultRequestHeaders.Add("x-hwid", hwid);
                try
                {
                    HttpResponseMessage response = await client.PostAsync("http://localhost:3000/api/validate-key", null);
                    string responseContent = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        label2.Text = "Done";
                        label2.ForeColor = Color.Green;
                    }
                    else
                    {
                        try
                        {
                            var json = System.Text.Json.JsonDocument.Parse(responseContent);
                            string error = json.RootElement.GetProperty("error").GetString();
                            label2.Text = error;
                            label2.ForeColor = Color.Red;
                        }
                        catch
                        {
                            label2.Text = "Errore sconosciuto";
                            label2.ForeColor = Color.Red;
                        }
                    }
                }
                catch (Exception ex)
                {
                    label2.Text = "Errore: " + ex.Message;
                    label2.ForeColor = Color.Red;
                }
            }
        }



        private async void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch1.Checked)
            {
                // Check connection to server
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync("http://localhost:3000");
                        if (response.IsSuccessStatusCode)
                        {
                            // Connected, show controls
                            AnimateShowControls();
                        }
                        else
                        {
                            MessageBox.Show("Impossibile connettersi al server.");
                            guna2ToggleSwitch1.Checked = false;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Errore di connessione al server.");
                        guna2ToggleSwitch1.Checked = false;
                    }
                }
            }
            else
            {
                // Hide controls
                guna2TextBox1.Visible = false;
                guna2Button1.Visible = false;
            }
        }



        private async void AnimateShowControls()
        {
            guna2TextBox1.Visible = true;
            guna2Button1.Visible = true;

            // Reset to original positions first
            guna2TextBox1.Location = originalTextBoxPosition;
            guna2Button1.Location = originalButtonPosition;
        }
    }
}
