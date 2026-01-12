using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Bypass
{
    public partial class Form6 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        [DllImport("kernel32.dll")]
        private static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);
        // Definisci il messaggio per il movimento del form
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        // Importa la funzione per inviare messaggi a Windows
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public Form6()
        {
            InitializeComponent();
            //AttachAndCenterToHDPlayer();
            this.guna2Panel31.MouseDown += Form3_MouseDown;
            this.guna2Panel31.MouseMove += Form3_MouseMove;
            this.guna2Panel31.MouseUp += Form3_MouseUp;
            // Imposta il colore di tutti i Guna2Button all'avvio del form
            SetGuna2ButtonsBackColor(this, Color.FromArgb(57, 255, 20));

        }

        private void SetGuna2ButtonsBackColor(Control parent, Color color)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Guna2Button button)
                {
                    button.FillColor = color; // FillColor è il colore di sfondo del Guna2Button
                    button.ForeColor = Color.Black;
                    button.BorderRadius = 8;
                    button.BorderThickness = 0;
                    button.HoverState.FillColor = Color.FromArgb(80, 255, 140);
                    button.HoverState.ForeColor = Color.Black;
                    button.ShadowDecoration.Enabled = true;
                    button.ShadowDecoration.Color = Color.FromArgb(57, 255, 20);
                }

                // Chiamata ricorsiva per i controlli figli
                if (ctrl.HasChildren)
                {
                    SetGuna2ButtonsBackColor(ctrl, color);
                }
            }
        }
        private void AttachAndCenterToHDPlayer()
        {
            // Trova il processo HD-Player
            Process[] processes = Process.GetProcessesByName("HD-Player");
            if (processes.Length > 0)
            {
                IntPtr hWndHDPlayer = processes[0].MainWindowHandle;

                if (hWndHDPlayer != IntPtr.Zero)
                {
                    // Ancorare il form come child di HD-Player
                    SetParent(this.Handle, hWndHDPlayer);

                    // Adatta le dimensioni del form per corrispondere a HD-Player
                    if (GetWindowRect(hWndHDPlayer, out RECT rect))
                    {
                        // Posiziona la finestra in alto a destra con un po' più di margine
                        PositionTopRightToHDPlayer(rect);
                    }
                }
                else
                {
                    MessageBox.Show("Finestra HD-Player non trovata.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("HD-Player non è in esecuzione.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PositionTopRightToHDPlayer(RECT rect)
        {
            // Calcola la posizione x per spostarsi a destra ma non troppo
            int x = rect.Right - this.Width - 50; // Aggiunto più margine da destra

            // Calcola la posizione y per abbassare un po' la finestra
            int y = rect.Top + 50; // Abbassato leggermente dalla posizione originale

            // Assicurati che la finestra non esca dai limiti dello schermo
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

            // Limita la posizione per non uscire dallo schermo
            x = Math.Max(0, Math.Min(x, screenWidth - this.Width));
            y = Math.Max(0, Math.Min(y, screenHeight - this.Height));

            // Imposta la posizione della finestra
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(x, y);
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        }
        private void Form3_MouseClick(object sender, MouseEventArgs e)
        {
            this.MouseMove += Form3_MouseMove;
        }

        private void Form3_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture(); // Rilascia la cattura del mouse
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0); // Simula il movimento della finestra
            }

        }

        private void Form3_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void Form3_MouseUp(object sender, MouseEventArgs e)
        {
            this.MouseUp += Form3_MouseUp;
        }
        private void guna2Panel31_Paint(object sender, PaintEventArgs e)
        {

            this.MouseDown += Form3_MouseDown;
            this.MouseMove += Form3_MouseMove;
            this.MouseUp += Form3_MouseUp;
        }



        private void guna2Button7_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel33_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }



     

        private void Form6_Load(object sender, EventArgs e)
        {
            SetupMockupLayout();
        }

        private void SetupMockupLayout()
        {
            var accent = Color.FromArgb(57, 255, 20);

            if (label32 != null)
            {
                label32.Text = "MENU BYPASS EMULATOR!";
                label32.ForeColor = accent;
            }
            if (label17 != null)
            {
                label17.Text = "Manual Installation Bypass Configuration";
                label17.ForeColor = accent;
            }
            if (guna2Button1 != null) guna2Button1.Text = "Open";
            if (guna2Button8 != null) guna2Button8.Text = "Open";

            if (label1 != null) label1.Text = "Free Fire";
            if (label2 != null) label2.Text = "Free Fire Max";

            if (guna2Panel2 != null) guna2Panel2.Visible = false;
            if (guna2Panel3 != null) guna2Panel3.Visible = false;

            if (guna2Panel1 != null)
            {
                var meta1 = new System.Windows.Forms.Label();
                meta1.Text = "BlueStacks App Player\nExpires: 25/11/2026\nModule: Pie 64";
                meta1.ForeColor = Color.DarkGray;
                meta1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                meta1.AutoSize = true;
                int x1 = (guna2CirclePictureBox2 != null ? guna2CirclePictureBox2.Right + 10 : 80);
                meta1.Location = new Point(x1, 14);
                guna2Panel1.Controls.Add(meta1);
            }

            if (guna2Panel34 != null)
            {
                var meta2 = new System.Windows.Forms.Label();
                meta2.Text = "BlueStacks App Player\nExpires: 25/11/2026\nModule: Pie 64";
                meta2.ForeColor = Color.DarkGray;
                meta2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                meta2.AutoSize = true;
                int x2 = (guna2CirclePictureBox1 != null ? guna2CirclePictureBox1.Right + 10 : 80);
                meta2.Location = new Point(x2, 12);
                guna2Panel34.Controls.Add(meta2);
            }

            if (guna2Panel33 == null) return;

            int yBase = 0;
            if (guna2Panel34 != null)
                yBase = guna2Panel34.Bottom + 16;
            else if (guna2Panel1 != null)
                yBase = guna2Panel1.Bottom + 16;
            else
                yBase = 180;

            var lblEmulator = new System.Windows.Forms.Label();
            lblEmulator.Text = "Emulator :";
            lblEmulator.ForeColor = Color.White;
            lblEmulator.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblEmulator.AutoSize = true;
            lblEmulator.Location = new Point(12, yBase);

            var cmbEmulator = new Guna2ComboBox();
            cmbEmulator.Items.AddRange(new object[] { "BlueStacks 5", "MSI App Player" });
            cmbEmulator.SelectedIndex = 0;
            cmbEmulator.Size = new Size(180, 30);
            cmbEmulator.Location = new Point(110, yBase - 4);
            cmbEmulator.FillColor = Color.FromArgb(20, 20, 20);
            cmbEmulator.ForeColor = Color.White;
            cmbEmulator.BorderColor = accent;
            cmbEmulator.BorderRadius = 8;

            var panelActions = new Guna2Panel();
            panelActions.Size = new Size(guna2Panel33.Width - 20, 60);
            panelActions.Location = new Point(10, yBase + 40);
            panelActions.BackColor = Color.FromArgb(15, 15, 14);
            panelActions.BorderColor = Color.FromArgb(30, 30, 30);
            panelActions.BorderThickness = 1;

            int gap = 10;
            int bw = (panelActions.Width - gap * 5) / 4;
            int by = 12;
            int x = gap;

            Guna2Button Btn(string text)
            {
                var b = new Guna2Button();
                b.Text = text;
                b.Size = new Size(bw, 36);
                b.Location = new Point(x, by);
                b.FillColor = accent;
                b.ForeColor = Color.Black;
                b.BorderRadius = 8;
                b.HoverState.FillColor = Color.FromArgb(80, 255, 140);
                b.HoverState.ForeColor = Color.Black;
                b.ShadowDecoration.Enabled = true;
                b.ShadowDecoration.Color = accent;
                x += bw + gap;
                return b;
            }

            var btnRemove = Btn("Remove");
            var btnRequest = Btn("Request Access");
            var btnInstall = Btn("Install");
            var btnDisconnect = Btn("Disconnect");


            panelActions.Controls.Add(btnRemove);
            panelActions.Controls.Add(btnRequest);
            panelActions.Controls.Add(btnInstall);
            panelActions.Controls.Add(btnDisconnect);

            var lblLog = new System.Windows.Forms.Label();
            lblLog.Text = "Log: Proxy revocato.";
            lblLog.ForeColor = Color.DarkGray;
            lblLog.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblLog.AutoSize = true;
            lblLog.Location = new Point(14, guna2Panel33.Height - 24);

            guna2Panel33.Controls.Add(lblEmulator);
            guna2Panel33.Controls.Add(cmbEmulator);
            guna2Panel33.Controls.Add(panelActions);
            guna2Panel33.Controls.Add(lblLog);
        }

      

    }
}
