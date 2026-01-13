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
            // Store original positions from Designer
            originalTextBoxPosition = new Point(45, 129);
            originalButtonPosition = new Point(89, 183);

            // Manually assign click event to button
            if (guna2Button1 != null)
            {
                guna2Button1.Click += guna2Button1_Click;
            }

            // Setup drag functionality
            SetupDragFunctionality();

            // Setup initial state
            SetupInitialAnimations();
            // Start background animations
            StartBackgroundEffects();
        }

        private void SetupInitialAnimations()
        {
            // Make controls visible by default
            guna2TextBox1.Visible = true;
            guna2Button1.Visible = true;
            
            // Ensure label2 is visible and reset to original style
            if (label2 != null)
            {
                label2.Visible = true;
                label2.Text = "Logs:";
                label2.ForeColor = SystemColors.ControlDarkDark;
                label2.BackColor = Color.Transparent;
                label2.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            }
            
            // Set positions from Designer
            guna2TextBox1.Location = new Point(45, 129);
            guna2Button1.Location = new Point(89, 183);
            
            // Hide panel2 initially
            if (guna2Panel2 != null)
            {
                guna2Panel2.Visible = false;
            }

            // Toggle switch removed - no longer needed
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
                float distance = Distance(e.Location, new Point(guna2Panel1.Width / 2, guna2Panel1.Height / 2));
                float intensity = Math.Max(0, 1 - (distance / (guna2Panel1.Width / 2)));

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

        // Server configuration
        private readonly string serverUrl = "https://apikey-auth-demo.onrender.com";
        
        private async Task<ValidationResult> ValidateApiKey(string apiKey)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Set timeout for the request
                    client.Timeout = TimeSpan.FromSeconds(10);
                    
                    // Add API key and HWID to headers
                    client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
                    client.DefaultRequestHeaders.Add("X-HWID", GetHWID());
                    
                    // Make the validation request
                    HttpResponseMessage response = await client.PostAsync($"{serverUrl}/api/validate-key", null);
                    
                    // Check specific response codes for different error types
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return new ValidationResult { IsValid = true, Message = "API key valida! Accesso consentito.", Color = Color.Green };
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // Key is banned
                        return new ValidationResult { IsValid = false, Message = "KEY BANNATA - Accesso negato", Color = Color.Red };
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Gone)
                    {
                        // Key is expired
                        return new ValidationResult { IsValid = false, Message = "KEY SCADUTA - Rinnovare la licenza", Color = Color.Orange };
                    }
                    else
                    {
                        // Other errors
                        return new ValidationResult { IsValid = false, Message = "API key non valida", Color = Color.Red };
                    }
                }
                catch (Exception ex)
                {
                    // Log the error for debugging
                    Console.WriteLine($"Validation error: {ex.Message}");
                    return new ValidationResult { IsValid = false, Message = "Errore di connessione: " + ex.Message, Color = Color.Red };
                }
            }
        }
        
        // Helper class to return validation results with messages
        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public string Message { get; set; }
            public Color Color { get; set; }
        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            string apiKey = guna2TextBox1.Text;
            if (string.IsNullOrEmpty(apiKey))
            {
                label2.Text = "Inserisci una API key";
                label2.ForeColor = Color.Red;
                return;
            }

            try
            {
                label2.Text = "Verifica in corso...";
                label2.ForeColor = Color.Orange;
                label2.Refresh(); // Force immediate update

                // Validate API key with server
                ValidationResult result = await ValidateApiKey(apiKey);
                
                // Update label with specific message from validation
                label2.Text = result.Message;
                label2.ForeColor = result.Color;
                label2.Refresh();
                
                if (result.IsValid)
                {
                    // Animate transition to panel2
                    await AnimatePanelTransition();
                }
            }
            catch (Exception ex)
            {
                label2.Text = "Errore di connessione: " + ex.Message;
                label2.ForeColor = Color.Red;
                label2.Refresh();
            }
        }



        // guna2ToggleSwitch1 removed - not needed anymore



        private async void AnimateShowControls()
        {
            try
            {
                // Make controls visible first
                if (guna2TextBox1 != null && guna2Button1 != null)
                {
                    guna2TextBox1.Invoke((MethodInvoker)delegate {
                        guna2TextBox1.Visible = true;
                        guna2TextBox1.Location = new Point(29, 158); // Fixed position
                    });
                    
                    guna2Button1.Invoke((MethodInvoker)delegate {
                        guna2Button1.Visible = true;
                        guna2Button1.Location = new Point(29, 218);  // Fixed position
                    });
                }

                // Small delay to ensure UI updates
                await Task.Delay(100);

                // Force refresh to ensure controls are displayed
                this.Invoke((MethodInvoker)delegate {
                    this.Refresh();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Show controls error: {ex.Message}");
                // Fallback - direct assignment with fixed positions
                if (guna2TextBox1 != null) 
                {
                    guna2TextBox1.Visible = true;
                    guna2TextBox1.Location = new Point(29, 158);
                }
                if (guna2Button1 != null) 
                {
                    guna2Button1.Visible = true;
                    guna2Button1.Location = new Point(29, 218);
                }
            }
        }

// Helper functions removed - using direct label2 assignment

        private async Task AnimatePanelTransition()
        {
            try
            {
                if (guna2Panel1 != null && guna2Panel2 != null && guna2Panel3 != null)
                {
                    // Simple transition without opacity (Guna2Panel doesn't support Opacity)
                    
                    // Hide panel1
                    if (guna2Panel1.IsHandleCreated)
                    {
                        guna2Panel1.Invoke((MethodInvoker)delegate {
                            guna2Panel1.Visible = false;
                        });
                    }
                    
                    await Task.Delay(100); // Small delay for smoothness
                    
                    // Show panel2 in the specified position
                    if (guna2Panel2.IsHandleCreated)
                    {
                        guna2Panel2.Invoke((MethodInvoker)delegate {
                            guna2Panel2.Visible = true;
                            guna2Panel2.Location = new Point(29, 58);
                            // Bring to front for proper layering
                            guna2Panel2.BringToFront();
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback in case of errors
                Console.WriteLine($"Animation error: {ex.Message}");
                
                // Direct switch without animation
                if (guna2Panel1 != null && guna2Panel2 != null)
                {
                    guna2Panel1.Visible = false;
                    guna2Panel2.Visible = true;
                    guna2Panel2.Location = new Point(29, 58);
                }
            }
        }
    }
}
