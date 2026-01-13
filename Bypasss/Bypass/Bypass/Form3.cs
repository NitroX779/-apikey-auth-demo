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

namespace Bypass
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            string apiKey = guna2TextBox1.Text.Trim();
            
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("Please enter an API key.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool isValid = await ValidateApiKey(apiKey);
                
                if (isValid)
                {
                    // Key is valid, proceed to main application
                    MessageBox.Show("Authentication successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Here you would typically open your main form or continue with the application
                    // For now, we'll just close this form
                    this.Hide();
                    // Open your main form here
                    // MainForm mainForm = new MainForm();
                    // mainForm.Show();
                }
                else
                {
                    MessageBox.Show("Invalid or expired API key.", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during validation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<bool> ValidateApiKey(string apiKey)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Set the base address of your server
                    client.BaseAddress = new Uri("http://localhost:3000"); // Change this to your actual server URL
                    
                    // Add the API key to the headers
                    client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
                    
                    // Add HWID to headers for validation
                    string hwid = GetHardwareId();
                    client.DefaultRequestHeaders.Add("X-HWID", hwid);

                    // Make the request to validate the key
                    HttpResponseMessage response = await client.PostAsync("/api/validate-key", null);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        // Parse the response if needed
                        return true;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // Key is banned
                        MessageBox.Show("This key has been banned.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return false;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Gone)
                    {
                        // Key is expired
                        MessageBox.Show("This key has expired.", "Access Expired", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Server error: {errorResponse}", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Network error: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private string GetHardwareId()
        {
            // Simple HWID generation - you might want to use a more robust method
            string cpuId = Environment.ProcessorCount.ToString();
            string machineName = Environment.MachineName;
            string userName = Environment.UserName;
            
            // Combine and hash to create a simple HWID
            string rawHwid = $"{cpuId}-{machineName}-{userName}";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawHwid));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}