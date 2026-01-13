namespace Bypass
{
    partial class Form3
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            guna2Panel3 = new Guna.UI2.WinForms.Guna2Panel();
            guna2Panel1 = new Guna.UI2.WinForms.Guna2Panel();
            guna2TextBox1 = new Guna.UI2.WinForms.Guna2TextBox();
            label1 = new Label();
            label17 = new Label();
            guna2Panel3.SuspendLayout();
            guna2Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // guna2Panel3
            // 
            guna2Panel3.BackColor = Color.FromArgb(15, 15, 23);
            guna2Panel3.BorderColor = Color.Black;
            guna2Panel3.Controls.Add(guna2Panel1);
            guna2Panel3.Controls.Add(label17);
            guna2Panel3.CustomizableEdges = customizableEdges1;
            guna2Panel3.Dock = DockStyle.Fill;
            guna2Panel3.Location = new Point(0, 0);
            guna2Panel3.Name = "guna2Panel3";
            guna2Panel3.ShadowDecoration.CustomizableEdges = customizableEdges2;
            guna2Panel3.Size = new Size(343, 235);
            guna2Panel3.TabIndex = 13;
            guna2Panel3.Paint += guna2Panel3_Paint;
            // 
            // guna2Panel1
            // 
            guna2Panel1.BackColor = Color.FromArgb(15, 15, 25);
            guna2Panel1.Controls.Add(guna2TextBox1);
            guna2Panel1.Controls.Add(label1);
            guna2Panel1.CustomizableEdges = customizableEdges3;
            guna2Panel1.Location = new Point(26, 58);
            guna2Panel1.Name = "guna2Panel1";
            guna2Panel1.ShadowDecoration.CustomizableEdges = customizableEdges4;
            guna2Panel1.Size = new Size(291, 155);
            guna2Panel1.TabIndex = 14;
            // 
            // guna2TextBox1
            // 
            guna2TextBox1.AccessibleName = "fdgdfg";
            guna2TextBox1.BackColor = Color.Transparent;
            guna2TextBox1.BorderColor = Color.FromArgb(35, 35, 35);
            guna2TextBox1.BorderRadius = 8;
            guna2TextBox1.BorderThickness = 3;
            guna2TextBox1.CustomizableEdges = customizableEdges5;
            guna2TextBox1.DefaultText = "";
            guna2TextBox1.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            guna2TextBox1.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            guna2TextBox1.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            guna2TextBox1.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            guna2TextBox1.FillColor = Color.FromArgb(35, 35, 35);
            guna2TextBox1.FocusedState.BorderColor = Color.Transparent;
            guna2TextBox1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            guna2TextBox1.ForeColor = Color.Gray;
            guna2TextBox1.HoverState.BorderColor = Color.Transparent;
            guna2TextBox1.Location = new Point(13, 97);
            guna2TextBox1.Name = "guna2TextBox1";
            guna2TextBox1.PlaceholderForeColor = Color.Gray;
            guna2TextBox1.PlaceholderText = "Enter Your Keys";
            guna2TextBox1.SelectedText = "";
            guna2TextBox1.ShadowDecoration.CustomizableEdges = customizableEdges6;
            guna2TextBox1.Size = new Size(266, 30);
            guna2TextBox1.TabIndex = 3;
            guna2TextBox1.TextChanged += guna2TextBox1_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.ControlDarkDark;
            label1.Location = new Point(85, 65);
            label1.Name = "label1";
            label1.Size = new Size(127, 20);
            label1.TabIndex = 2;
            label1.Text = "Enter Your Keys";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.BackColor = Color.Transparent;
            label17.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label17.ForeColor = Color.Silver;
            label17.Location = new Point(132, 9);
            label17.Name = "label17";
            label17.Size = new Size(91, 20);
            label17.TabIndex = 13;
            label17.Text = "Contental X";
            // 
            // Form3
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(343, 235);
            Controls.Add(guna2Panel3);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Form3";
            Text = "Form3";
            guna2Panel3.ResumeLayout(false);
            guna2Panel3.PerformLayout();
            guna2Panel1.ResumeLayout(false);
            guna2Panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Guna.UI2.WinForms.Guna2Panel guna2Panel3;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel1;
        private Label label17;
        private Label label1;
        private Guna.UI2.WinForms.Guna2TextBox guna2TextBox1;
    }
}