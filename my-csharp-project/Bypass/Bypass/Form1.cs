using Bypass.Animations;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bypass
{
    public partial class Form1 : Form
    {
        // Particle system variables
        private const int ParticleCount = 50;
        private PointF[] _particlePositions = new PointF[ParticleCount];
        private PointF[] _particleTargetPositions = new PointF[ParticleCount];
        private float[] _particleSpeeds = new float[ParticleCount];
        private float[] _particleSizes = new float[ParticleCount];
        private float[] _particleRotations = new float[ParticleCount];
        private Random _random = new Random();
        private Color _particleColor = Color.FromArgb(255, 0, 255);

        // Variables for form dragging
        private bool isDragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        // Particle drawing settings
        private int _glowSize = 3;
        private int _triangleSize = 1;
        private int _maxGlowLayers = 3;

        // Buffer per il double buffering manuale
        private Bitmap _backBuffer;
        private Graphics _bufferGraphics;

        // Variabili per il bypass system
        private string selectedEmulator = "";
        private string selectedFreeFire = "";
        private bool MSISelected = false;
        private const string CertHash = "c8750f0d";
        private const string EmbeddedResourceName = "Bypass.c8750f0d.0";
        private static string ProxyAddress = "45.137.98.182:1744";
        private string targetDevice = "127.0.0.1:5555";
        private System.Windows.Forms.Timer emulatorMonitorTimer;
        private System.Windows.Forms.Timer closeAppTimer;
        private bool processCompleted = false;
        private CircularProgressAnimator _progressAnimator;
        private System.Windows.Forms.Timer progressTimer;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(15, 10, 25);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.DoubleBuffer, true);

            _progressAnimator = new CircularProgressAnimator(guna2CircleProgressBar1);
            RecreateBackBuffer();

            // Enable form dragging
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;

            // Initialize particles
            InitializeParticles();

            // Timer per l'animazione delle particelle
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 20;
            timer.Tick += Timer_Tick;
            timer.Start();

            // Inizializza i timer per il bypass
            InitializeEmulatorMonitor();
            InitializeCloseAppTimer();
            InitializeProgressTimer();

            // Debug: check embedded resources
            CheckAllEmbeddedResources();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitializeUI();
            ApplyNeonStyleToButtons(this);
            ConfigureExistingButtons();
        }

        private void InitializeUI()
        {
            if (guna2GradientPanel3 != null)
            {
                guna2GradientPanel3.Location = new Point(26, 186);
                guna2GradientPanel3.Hide();
            }
            if (guna2GradientPanel4 != null)
            {
                guna2GradientPanel4.Location = new Point(26, 286);
                guna2GradientPanel4.Hide();
            }
            if (guna2CircleProgressBar1 != null) guna2CircleProgressBar1.Hide();
            if (lblStatus != null) lblStatus.Hide();
        }

        private void InitializeProgressTimer()
        {
            progressTimer = new System.Windows.Forms.Timer();
            progressTimer.Interval = 300;
            progressTimer.Tick += ProgressTimer_Tick;
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (guna2CircleProgressBar1 != null && guna2CircleProgressBar1.Value < 100)
            {
                guna2CircleProgressBar1.Value += 1;
                if (guna2CircleProgressBar1.Value > 100)
                    guna2CircleProgressBar1.Value = 0;
            }
        }

        private void InitializeEmulatorMonitor()
        {
            emulatorMonitorTimer = new System.Windows.Forms.Timer();
            emulatorMonitorTimer.Interval = 5000;
            emulatorMonitorTimer.Tick += EmulatorMonitorTimer_Tick;
        }

        private void InitializeCloseAppTimer()
        {
            closeAppTimer = new System.Windows.Forms.Timer();
            closeAppTimer.Interval = 30000;
            closeAppTimer.Tick += CloseAppTimer_Tick;
        }

        private void CloseAppTimer_Tick(object sender, EventArgs e)
        {
            closeAppTimer.Stop();
            Debug.WriteLine("⏰ Timer di chiusura scaduto - Chiudo l'applicazione");
            Application.Exit();
        }

        private void ConfigureExistingButtons()
        {
            try
            {
                if (guna2Button1 != null)
                {
                    guna2Button1.Click -= new EventHandler(MSIButton_Click);
                    guna2Button1.Click += new EventHandler(MSIButton_Click);
                }

                if (guna2Button2 != null)
                {
                    guna2Button2.Click -= new EventHandler(BlueStacksButton_Click);
                    guna2Button2.Click += new EventHandler(BlueStacksButton_Click);
                }

                if (guna2Button3 != null)
                {
                    guna2Button3.Click -= new EventHandler(FreeFireButton_Click);
                    guna2Button3.Click += new EventHandler(FreeFireButton_Click);
                }

                if (guna2Button4 != null)
                {
                    guna2Button4.Click -= new EventHandler(FreeFireMaxButton_Click);
                    guna2Button4.Click += new EventHandler(FreeFireMaxButton_Click);
                }

                if (guna2Button1 == null || guna2Button2 == null)
                {
                    FindAndConfigureAlternativeButtons();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore configurazione pulsanti: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MSIButton_Click(object sender, EventArgs e)
        {
            SelectEmulator("MSI App Player");
        }

        private void BlueStacksButton_Click(object sender, EventArgs e)
        {
            SelectEmulator("BlueStacks NXT");
        }

        private void FreeFireButton_Click(object sender, EventArgs e)
        {
            SelectFreeFire("Free Fire");
        }

        private void FreeFireMaxButton_Click(object sender, EventArgs e)
        {
            SelectFreeFire("Free Fire Max");
        }

        private void FindAndConfigureAlternativeButtons()
        {
            var allButtons = GetAllControls(this).OfType<Guna.UI2.WinForms.Guna2GradientButton>();

            foreach (var button in allButtons)
            {
                if (button.Text.ToUpper().Contains("MSI") && !button.Text.ToUpper().Contains("MAX"))
                {
                    button.Click += (s, e) => SelectEmulator("MSI App Player");
                }
                else if (button.Text.ToUpper().Contains("BLUESTACKS"))
                {
                    button.Click += (s, e) => SelectEmulator("BlueStacks NXT");
                }
                else if (button.Text.ToUpper().Contains("FREE FIRE") && !button.Text.ToUpper().Contains("MAX"))
                {
                    button.Click += (s, e) => SelectFreeFire("Free Fire");
                }
                else if (button.Text.ToUpper().Contains("FREE FIRE MAX"))
                {
                    button.Click += (s, e) => SelectFreeFire("Free Fire Max");
                }
            }
        }

        private IEnumerable<Control> GetAllControls(Control control)
        {
            var controls = control.Controls.Cast<Control>();
            return controls.SelectMany(ctrl => GetAllControls(ctrl)).Concat(controls);
        }

        private void SelectEmulator(string emulator)
        {
            selectedEmulator = emulator;
            MSISelected = (emulator == "MSI App Player");

            if (guna2GradientPanel1 != null) guna2GradientPanel1.Hide();
            if (guna2GradientPanel2 != null) guna2GradientPanel2.Hide();

            ShowProgressPanel();
            StartBypassProcess();
        }

        private void SelectFreeFire(string freeFire)
        {
            selectedFreeFire = freeFire;
            Debug.WriteLine($"✅ Free Fire selezionato: {freeFire}");
            HideFormImmediately();
            ContinueAfterFreeFireSelection();
        }

        private void HideFormImmediately()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(HideFormImmediately));
                return;
            }

            this.Hide();
            this.Visible = false;
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;

            Debug.WriteLine("🎮 Form nascosto immediatamente dopo selezione Free Fire");
        }

        private void HideFreeFirePanels()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(HideFreeFirePanels));
                return;
            }

            if (guna2GradientPanel3 != null) guna2GradientPanel3.Hide();
            if (guna2GradientPanel4 != null) guna2GradientPanel4.Hide();
            Debug.WriteLine("🔒 Pannelli Free Fire nascosti");
        }

        private void ShowProgressPanel()
        {
            if (guna2CircleProgressBar1 == null || lblStatus == null) return;

            guna2CircleProgressBar1.Location = new Point(
                (this.Width - guna2CircleProgressBar1.Width) / 2,
                (this.Height - guna2CircleProgressBar1.Height) / 2
            );

            lblStatus.Location = new Point(
                (this.Width - lblStatus.Width) / 2 - 40,
                guna2CircleProgressBar1.Location.Y + guna2CircleProgressBar1.Height + 10
            );

            guna2CircleProgressBar1.Show();
            lblStatus.Show();
            guna2CircleProgressBar1.BringToFront();
            lblStatus.BringToFront();

            if (_progressAnimator != null)
            {
                if (progressTimer != null)
                    progressTimer.Stop();

                _progressAnimator.StartAnimation();
                _progressAnimator.SetValue(0);
            }
        }

        private void UpdateProgressStatus(string status, int progress = -1)
        {
            if (lblStatus != null)
            {
                if (lblStatus.InvokeRequired)
                {
                    lblStatus.Invoke(new Action<string, int>(UpdateProgressStatus), status, progress);
                    return;
                }
                lblStatus.Text = status;
                lblStatus.ForeColor = Color.FromArgb(255, 230, 255);
                lblStatus.Refresh();
            }

            if (progress >= 0 && _progressAnimator != null)
            {
                _progressAnimator.SetValue(progress);
            }
        }

        // === METODI PRINCIPALI PER INSTALLAZIONE CERTIFICATO ===

        private async void StartBypassProcess()
        {
            try
            {
                Debug.WriteLine("🚀 AVVIO PROCESSO BYPASS COMPLETO");

                // 1. REQUEST ACCESS
                UpdateProgressStatus("Requesting Access...", 10);
                if (!await RequestAccess())
                {
                    Debug.WriteLine("❌ REQUEST ACCESS FALLITO");
                    MessageBox.Show("Request Access failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ShowEmulatorSelection();
                    return;
                }
                Debug.WriteLine("✅ REQUEST ACCESS COMPLETATO");

                // 2. ATTESA 30 SECONDI
                UpdateProgressStatus("Waiting for Emulator...", 20);
                Debug.WriteLine("⏳ ATTESA 30 SECONDI PER EMULATORE");
                for (int i = 30; i > 0; i--)
                {
                    int progress = 20 + (int)((30 - i) / 30.0 * 30);
                    UpdateProgressStatus($"Waiting {i}s for emulator...", progress);
                    await Task.Delay(1000);
                }

                // 3. INSTALLAZIONE CERTIFICATO
                UpdateProgressStatus("Installing Certificate...", 60);
                Debug.WriteLine("🔐 INIZIO INSTALLAZIONE CERTIFICATO");
                if (await InstallCertificate())
                {
                    UpdateProgressStatus("Certificate Installed Successfully!", 90);
                    Debug.WriteLine("✅ CERTIFICATO INSTALLATO CON SUCCESSO");

                    await Task.Delay(2000);
                    UpdateProgressStatus("Complete!", 100);

                    ShowFreeFirePanelsAfterCertificate();
                }
                else
                {
                    Debug.WriteLine("❌ INSTALLAZIONE CERTIFICATO FALLITA");
                    MessageBox.Show("Certificate installation failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ShowEmulatorSelection();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 ERRORE CRITICO: {ex.Message}");
                MessageBox.Show($"Error during bypass process: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowEmulatorSelection();
            }
        }

        private async Task<bool> InstallCertificate()
        {
            return await Task.Run(() =>
            {
                try
                {
                    Debug.WriteLine("🔍 CERCO ADB...");
                    string adb = ResolveAdbPath();
                    if (!File.Exists(adb))
                    {
                        Debug.WriteLine("❌ ADB NON TROVATO!");
                        return false;
                    }
                    Debug.WriteLine($"✅ ADB TROVATO: {adb}");

                    // CONNESSIONE ADB
                    bool adbConnected = false;
                    for (int attempt = 1; attempt <= 12; attempt++)
                    {
                        Debug.WriteLine($"🔗 TENTATIVO CONNESSIONE ADB {attempt}/12...");
                        if (ConnectAdb(adb))
                        {
                            adbConnected = true;
                            break;
                        }
                        Thread.Sleep(3000);
                    }

                    if (!adbConnected)
                    {
                        Debug.WriteLine("❌ CONNESSIONE ADB FALLITA DOPO 12 TENTATIVI!");
                        return false;
                    }

                    // VERIFICA DISPOSITIVI
                    string devicesOutput = RunAdbCommandWithOutput(adb, "devices");
                    Debug.WriteLine($"📱 DISPOSITIVI CONNESSI: {devicesOutput}");

                    if (!devicesOutput.Contains(targetDevice))
                    {
                        Debug.WriteLine($"❌ DISPOSITIVO {targetDevice} NON TROVATO!");
                        return false;
                    }

                    // ESTRAZIONE CERTIFICATO
                    Debug.WriteLine("📦 ESTRAZIONE CERTIFICATO EMBEDDED...");
                    string tempCertPath = ExtractEmbeddedCert(EmbeddedResourceName, CertHash);
                    if (string.IsNullOrEmpty(tempCertPath) || !File.Exists(tempCertPath))
                    {
                        Debug.WriteLine("❌ CERTIFICATO EMBEDDED NON TROVATO O ESTRAZIONE FALLITA!");
                        return false;
                    }

                    // VERIFICA FILE CERTIFICATO
                    FileInfo certInfo = new FileInfo(tempCertPath);
                    if (certInfo.Length == 0)
                    {
                        Debug.WriteLine("❌ FILE CERTIFICATO VUOTO!");
                        File.Delete(tempCertPath);
                        return false;
                    }
                    Debug.WriteLine($"✅ CERTIFICATO ESTRATTO: {tempCertPath} ({certInfo.Length} bytes)");

                    // PUSH CERTIFICATO
                    Debug.WriteLine("📤 PUSH CERTIFICATO SU DISPOSITIVO...");
                    string pushResult = RunAdbCommandWithOutput(adb, $"-s {targetDevice} push \"{tempCertPath}\" /sdcard/{CertHash}.0");
                    if (pushResult.Contains("error") || pushResult.Contains("failed") || pushResult.Contains("denied"))
                    {
                        Debug.WriteLine($"❌ PUSH CERTIFICATO FALLITO: {pushResult}");
                        File.Delete(tempCertPath);
                        return false;
                    }
                    Debug.WriteLine("✅ CERTIFICATO PUSHATO SUL DISPOSITIVO");

                    // INSTALLAZIONE CERTIFICATO NEL SISTEMA
                    Debug.WriteLine("⚙️ INSTALLAZIONE CERTIFICATO NEL SISTEMA...");

                    // PRIMO METODO: con su
                    string installCmd = $"/system/xbin/su -c '" +
                        $"mount -o rw,remount /system && " +
                        $"cp /sdcard/{CertHash}.0 /system/etc/security/cacerts/{CertHash}.0 && " +
                        $"chmod 644 /system/etc/security/cacerts/{CertHash}.0 && " +
                        $"chown root:root /system/etc/security/cacerts/{CertHash}.0 && " +
                        $"mount -o ro,remount /system && " +
                        $"rm /sdcard/{CertHash}.0 && " +
                        $"setprop ctl.restart zygote'";

                    Debug.WriteLine($"🔧 COMANDO INSTALLAZIONE: {installCmd}");
                    string installResult = RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell \"{installCmd}\"");
                    Debug.WriteLine($"📋 RISULTATO INSTALLAZIONE: {installResult}");

                    // ATTESA RIAVVIO
                    Debug.WriteLine("⏳ ATTENDO RIAVVIO SISTEMA (25 SECONDI)...");
                    Thread.Sleep(25000);

                    // RICONNESSIONE DOPO RIAVVIO
                    bool reconnected = false;
                    for (int attempt = 1; attempt <= 15; attempt++)
                    {
                        Debug.WriteLine($"🔗 TENTATIVO RICONNESSIONE ADB {attempt}/15...");
                        if (ConnectAdb(adb))
                        {
                            reconnected = true;
                            break;
                        }
                        Thread.Sleep(3000);
                    }

                    if (!reconnected)
                    {
                        Debug.WriteLine("❌ RICONNESSIONE ADB FALLITA DOPO RIAVVIO!");
                        File.Delete(tempCertPath);
                        return false;
                    }

                    // VERIFICA INSTALLAZIONE
                    Debug.WriteLine("🔍 VERIFICA INSTALLAZIONE CERTIFICATO...");
                    bool certificateVerified = false;

                    for (int attempt = 1; attempt <= 10; attempt++)
                    {
                        string verifyCmd = $"/system/xbin/su -c 'ls -la /system/etc/security/cacerts/{CertHash}.0 && echo CERTIFICATE_EXISTS'";
                        string verifyResult = RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell \"{verifyCmd}\"");

                        Debug.WriteLine($"🔍 TENTATIVO VERIFICA {attempt}/10: {verifyResult}");

                        if (verifyResult.Contains("CERTIFICATE_EXISTS") ||
                            (verifyResult.Contains(CertHash) && !verifyResult.Contains("No such file")))
                        {
                            certificateVerified = true;
                            break;
                        }

                        // Metodo alternativo di verifica
                        string altVerify = RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell \"[ -f /system/etc/security/cacerts/{CertHash}.0 ] && echo FILE_EXISTS\"");
                        if (altVerify.Contains("FILE_EXISTS"))
                        {
                            certificateVerified = true;
                            break;
                        }

                        Thread.Sleep(3000);
                    }

                    // PULIZIA FILE TEMPORANEO
                    try
                    {
                        File.Delete(tempCertPath);
                        Debug.WriteLine("🧹 FILE TEMPORANEO ELIMINATO");
                    }
                    catch { }

                    if (certificateVerified)
                    {
                        Debug.WriteLine("✅ CERTIFICATO INSTALLATO E VERIFICATO CON SUCCESSO!");
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("❌ VERIFICA CERTIFICATO FALLITA DOPO TUTTI I TENTATIVI");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"💥 ECCEZIONE INSTALLAZIONE CERTIFICATO: {ex.Message}");
                    return false;
                }
            });
        }

        private string ExtractEmbeddedCert(string resourceName, string hash)
        {
            string tempCertPath = "";

            try
            {
                tempCertPath = Path.Combine(Path.GetTempPath(), hash + ".0");
                Debug.WriteLine($"🔍 TENTATIVO ESTRAZIONE CERTIFICATO: {hash}");

                var assembly = Assembly.GetExecutingAssembly();
                Stream resourceStream = null;
                string foundResource = "";

                // PRIMA CERCA CON IL NOME ORIGINALE
                resourceStream = assembly.GetManifestResourceStream(resourceName);
                if (resourceStream != null)
                {
                    foundResource = resourceName;
                    Debug.WriteLine($"✅ RISORSA TROVATA CON NOME ORIGINALE: {resourceName}");
                }
                else
                {
                    // CERCA TRA TUTTE LE RISORSE
                    var allResources = assembly.GetManifestResourceNames();
                    Debug.WriteLine("🔍 SCANSIONE TUTTE LE RISORSE EMBEDDED:");

                    foreach (var res in allResources)
                    {
                        Debug.WriteLine($"   📁 {res}");

                        // CERCA PER HASH O NOMI SIMILI
                        if (res.Contains(hash) ||
                            res.Contains("c8750f0d") ||
                            res.ToLower().Contains("cert") ||
                            res.EndsWith(".0"))
                        {
                            Debug.WriteLine($"   🔍 PROVO: {res}");
                            resourceStream = assembly.GetManifestResourceStream(res);
                            if (resourceStream != null)
                            {
                                foundResource = res;
                                Debug.WriteLine($"   ✅ RISORSA TROVATA: {res}");
                                break;
                            }
                        }
                    }
                }

                if (resourceStream == null)
                {
                    Debug.WriteLine("❌ NESSUNA RISORSA CERTIFICATO TROVATA!");
                    return string.Empty;
                }

                // ESTRAI LA RISORSA
                using (resourceStream)
                using (var fileStream = File.Create(tempCertPath))
                {
                    resourceStream.CopyTo(fileStream);
                }

                if (File.Exists(tempCertPath))
                {
                    FileInfo fileInfo = new FileInfo(tempCertPath);
                    Debug.WriteLine($"✅ CERTIFICATO ESTRATTO: {tempCertPath} ({fileInfo.Length} bytes)");
                    return tempCertPath;
                }
                else
                {
                    Debug.WriteLine("❌ CREAZIONE FILE TEMPORANEO FALLITA");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 ERRORE ESTRAZIONE CERTIFICATO: {ex.Message}");
                return string.Empty;
            }
        }

        private void CheckAllEmbeddedResources()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resources = assembly.GetManifestResourceNames();

                Debug.WriteLine("🔍 ELENCO COMPLETO RISORSE EMBEDDED:");
                foreach (var resource in resources)
                {
                    Debug.WriteLine($"   📁 {resource}");

                    // Se contiene l'hash, è il certificato
                    if (resource.Contains(CertHash) || resource.Contains("c8750f0d"))
                    {
                        Debug.WriteLine($"   ✅ ⭐⭐⭐ CERTIFICATO TROVATO: {resource} ⭐⭐⭐");

                        // Verifica dimensione
                        try
                        {
                            using (var stream = assembly.GetManifestResourceStream(resource))
                            {
                                if (stream != null)
                                {
                                    Debug.WriteLine($"   📏 DIMENSIONE: {stream.Length} bytes");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"   ❌ ERRORE ACCESSO: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ ERRORE SCANSIONE RISORSE: {ex.Message}");
            }
        }

        // === METODI AUSILIARI ===

        private bool ConnectAdb(string adbExe)
        {
            try
            {
                // Disconnetti prima per pulizia
                RunAdbCommandWithOutput(adbExe, "disconnect");
                Thread.Sleep(1000);

                var psi = new ProcessStartInfo
                {
                    FileName = adbExe,
                    Arguments = $"connect {targetDevice}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psi))
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    string error = proc.StandardError.ReadToEnd();
                    proc.WaitForExit(15000); // Timeout 15 secondi

                    bool success = output.Contains("connected") ||
                                  output.Contains("already connected") ||
                                  output.ToLower().Contains("already connected to");

                    if (success)
                    {
                        Debug.WriteLine("✅ ADB CONNESSO CON SUCCESSO");
                        return true;
                    }

                    Debug.WriteLine($"❌ CONNESSIONE ADB FALLITA: {output} {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 ERRORE CONNESSIONE ADB: {ex.Message}");
                return false;
            }
        }

        private string ResolveAdbPath()
        {
            string msi = @"C:\Program Files\Bluestacks_msi5\HD-Adb.exe";
            string nxt = @"C:\Program Files\BlueStacks_nxt\HD-Adb.exe";

            if (MSISelected)
            {
                if (File.Exists(msi))
                    return msi;
            }
            else
            {
                if (File.Exists(nxt))
                    return nxt;
            }

            return MSISelected ? msi : nxt;
        }

        private string RunAdbCommandWithOutput(string adbPath, string arguments)
        {
            try
            {
                Debug.WriteLine($"🔧 ESEGUO: {adbPath} {arguments}");
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit(30000); // Timeout 30 secondi

                    string result = output + error;

                    if (!string.IsNullOrEmpty(output.Trim()))
                        Debug.WriteLine($"📤 OUTPUT: {output.Trim()}");
                    if (!string.IsNullOrEmpty(error.Trim()))
                        Debug.WriteLine($"📥 ERROR: {error.Trim()}");

                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 ADB COMMAND FAILED: {ex.Message}");
                return $"error: {ex.Message}";
            }
        }

        // === METODI UI ===

        private void ShowFreeFirePanelsAfterCertificate()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ShowFreeFirePanelsAfterCertificate));
                return;
            }

            try
            {
                if (progressTimer != null) progressTimer.Stop();
                if (guna2CircleProgressBar1 != null) guna2CircleProgressBar1.Hide();
                if (lblStatus != null) lblStatus.Hide();

                if (guna2GradientPanel3 != null)
                {
                    guna2GradientPanel3.Location = new Point(26, 186);
                    guna2GradientPanel3.Show();
                    guna2GradientPanel3.BringToFront();
                }

                if (guna2GradientPanel4 != null)
                {
                    guna2GradientPanel4.Location = new Point(26, 286);
                    guna2GradientPanel4.Show();
                    guna2GradientPanel4.BringToFront();
                }

                this.Refresh();
                Debug.WriteLine("✅ PANNELLI FREE FIRE MOSTRATI");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing Free Fire panels: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ContinueAfterFreeFireSelection()
        {
            try
            {
                HideAllPanels();
                await StartFreeFireDirectly();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during Free Fire startup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowEmulatorSelection();
            }
        }

        private async Task StartFreeFireDirectly()
        {
            try
            {
                Debug.WriteLine("🎮 AVVIO FREE FIRE DIRETTAMENTE...");

                if (await ActivateBypassAndStartFreeFire())
                {
                    processCompleted = true;
                    Debug.WriteLine("✅ FREE FIRE AVVIATO CON SUCCESSO");

                    this.Hide();
                    this.Visible = false;
                    this.Opacity = 0;
                    this.ShowInTaskbar = false;
                    this.WindowState = FormWindowState.Minimized;

                    Debug.WriteLine("📱 FORM NASCOSTO IN BACKGROUND");

                    if (emulatorMonitorTimer != null)
                    {
                        emulatorMonitorTimer.Start();
                        Debug.WriteLine("⏰ TIMER MONITOR EMULATORE AVVIATO");
                    }

                    if (closeAppTimer != null)
                    {
                        closeAppTimer.Start();
                        Debug.WriteLine("⏰ TIMER CHIUSURA APP (30s) AVVIATO");
                    }
                }
                else
                {
                    MessageBox.Show("Free Fire startup failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ShowEmulatorSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during Free Fire startup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowEmulatorSelection();
            }
        }

        private void HideAllPanels()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(HideAllPanels));
                return;
            }

            if (guna2GradientPanel1 != null) guna2GradientPanel1.Hide();
            if (guna2GradientPanel2 != null) guna2GradientPanel2.Hide();
            if (guna2GradientPanel3 != null) guna2GradientPanel3.Hide();
            if (guna2GradientPanel4 != null) guna2GradientPanel4.Hide();
            if (guna2CircleProgressBar1 != null) guna2CircleProgressBar1.Hide();
            if (lblStatus != null) lblStatus.Hide();

            Debug.WriteLine("🔒 TUTTI I PANNELLI NASCOSTI");
        }

        private void ShowEmulatorSelection()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ShowEmulatorSelection));
                return;
            }

            if (progressTimer != null) progressTimer.Stop();
            if (guna2CircleProgressBar1 != null) guna2CircleProgressBar1.Hide();
            if (lblStatus != null) lblStatus.Hide();
            if (guna2GradientPanel1 != null) guna2GradientPanel1.Show();
            if (guna2GradientPanel2 != null) guna2GradientPanel2.Show();
            if (guna2GradientPanel3 != null) guna2GradientPanel3.Hide();
            if (guna2GradientPanel4 != null) guna2GradientPanel4.Hide();
        }

        // === METODI BYPASS ===

        private async Task<bool> RequestAccess()
        {
            return await Task.Run(() =>
            {
                try
                {
                    string[] processesToKill = { "HD-Player", "HD-Adb", "HD-MultiInstanceManager", "BstkSVC" };
                    foreach (var procName in processesToKill)
                    {
                        foreach (var proc in Process.GetProcessesByName(procName))
                        {
                            try
                            {
                                proc.Kill();
                                proc.WaitForExit(2000);
                            }
                            catch { }
                        }
                    }

                    string engineRoot = MSISelected ?
                        @"C:\ProgramData\Bluestacks_msi5\Engine" :
                        @"C:\ProgramData\BlueStacks_nxt\Engine";

                    if (!Directory.Exists(engineRoot))
                    {
                        return false;
                    }

                    EditConfigs(engineRoot);

                    string managerDir = Path.Combine(engineRoot, "Manager");
                    if (Directory.Exists(managerDir))
                    {
                        var logFiles = Directory.GetFiles(managerDir, "BstkServer.log")
                            .Concat(Directory.GetFiles(managerDir, "BstkServer.log.*"));

                        foreach (var file in logFiles)
                        {
                            try { File.Delete(file); }
                            catch
                            {
                                try { using (var fs = new FileStream(file, FileMode.Create, FileAccess.Write)) { } } catch { }
                            }
                        }
                    }

                    foreach (var instanceDir in Directory.GetDirectories(engineRoot))
                    {
                        string logsDir = Path.Combine(instanceDir, "Logs");
                        if (Directory.Exists(logsDir))
                        {
                            foreach (var file in Directory.GetFiles(logsDir, "BstkCore.log*"))
                            {
                                try { File.Delete(file); } catch { }
                            }
                        }
                    }

                    string emulatorExe;
                    if (MSISelected)
                        emulatorExe = @"C:\Program Files\BlueStacks_msi5\HD-Player.exe";
                    else
                        emulatorExe = @"C:\Program Files\BlueStacks_nxt\HD-Player.exe";

                    if (File.Exists(emulatorExe))
                    {
                        Process.Start(emulatorExe);
                    }
                    else
                    {
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Request Access failed: {ex.Message}");
                    return false;
                }
            });
        }

        private async Task<bool> ActivateBypassAndStartFreeFire()
        {
            return await Task.Run(() =>
            {
                try
                {
                    string packageName = selectedFreeFire == "Free Fire" ?
                        "com.dts.freefireth" : "com.dts.freefiremax";

                    Debug.WriteLine($"🎯 TARGET PACKAGE: {packageName}");

                    string adb = ResolveAdbPath();

                    Debug.WriteLine("🛑 FERMO APPLICAZIONE...");
                    RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell am force-stop {packageName}");
                    Thread.Sleep(2000);

                    Debug.WriteLine("🌐 APPLICAZIONE PROXY...");
                    string proxyResult = RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell settings put global http_proxy {ProxyAddress}");
                    if (proxyResult.Contains("error") || proxyResult.Contains("failed"))
                    {
                        Debug.WriteLine($"❌ APPLICAZIONE PROXY FALLITA: {proxyResult}");
                        return false;
                    }

                    Debug.WriteLine($"✅ PROXY APPLICATO: {ProxyAddress}");

                    Debug.WriteLine("🚀 AVVIO APPLICAZIONE...");
                    string startResult = RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell monkey -p {packageName} -c android.intent.category.LAUNCHER 1");

                    if (startResult.Contains("error") || startResult.Contains("failed"))
                    {
                        Debug.WriteLine($"❌ AVVIO APPLICAZIONE FALLITO: {startResult}");
                        return false;
                    }

                    Debug.WriteLine($"✅ {selectedFreeFire} AVVIATO CON SUCCESSO!");
                    Thread.Sleep(3000);

                    Debug.WriteLine("⏳ ATTENDO 30 SECONDI PRIMA DI DISATTIVARE PROXY...");
                    for (int i = 30; i > 0; i--)
                    {
                        Debug.WriteLine($"⏰ DISATTIVAZIONE PROXY TRA {i} SECONDI...");
                        Thread.Sleep(1000);
                    }

                    Debug.WriteLine("🔌 DISATTIVAZIONE PROXY DOPO 30 SECONDI...");
                    string disableResult = RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell settings put global http_proxy :0");
                    if (!disableResult.Contains("error"))
                    {
                        Debug.WriteLine("✅ PROXY DISATTIVATO DOPO 30 SECONDI");
                    }
                    else
                    {
                        Debug.WriteLine($"⚠️ IMPOSSIBILE DISATTIVARE PROXY: {disableResult}");
                    }

                    Debug.WriteLine("🎉 BYPASS COMPLETATO!");

                    Task.Delay(1000).ContinueWith(t =>
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                Debug.WriteLine("✅ FORM CHIUSO");
                                Application.Exit();
                            }));
                        }
                        else
                        {
                            Debug.WriteLine("✅ FORM CHIUSO");
                            Application.Exit();
                        }
                    });

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ ERRORE ATTIVAZIONE BYPASS: {ex.Message}");
                    return false;
                }
            });
        }

        private void EditConfigs(string engineRoot)
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(engineRoot))
                {
                    string baseName = Path.GetFileName(dir);
                    string[] files = {
                Path.Combine(dir, "Android.bstk.in"),
                Path.Combine(dir, baseName + ".bstk"),
                Path.Combine(dir, baseName + ".bstk-prev")
            };

                    foreach (string file in files.Where(File.Exists))
                    {
                        string content = File.ReadAllText(file, System.Text.Encoding.UTF8);

                        content = System.Text.RegularExpressions.Regex.Replace(content,
                            @"(<HardDisk\b[^>]*location\s*=\s*""Root\.vhd""[^>]*type\s*=\s*"")Readonly(""\s*/?>)",
                            @"$1Normal$2", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                        content = System.Text.RegularExpressions.Regex.Replace(content,
                            @"(<HardDisk\b[^>]*location\s*=\s*""Data\.vhdx""[^>]*type\s*=\s*"")Readonly(""\s*/?>)",
                            @"$1Normal$2", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                        File.WriteAllText(file, content, System.Text.Encoding.UTF8);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditConfigs failed: {ex.Message}");
            }
        }

        private void EmulatorMonitorTimer_Tick(object sender, EventArgs e)
        {
            if (processCompleted)
            {
                CheckAndCloseIfEmulatorClosed();
            }
        }

        private void CheckAndCloseIfEmulatorClosed()
        {
            try
            {
                bool emulatorRunning = Process.GetProcessesByName("HD-Player").Length > 0;

                if (!emulatorRunning)
                {
                    DisableProxy();
                    if (emulatorMonitorTimer != null) emulatorMonitorTimer.Stop();

                    Task.Delay(2000).ContinueWith(t =>
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() => Application.Exit()));
                        }
                        else
                        {
                            Application.Exit();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Monitor error: {ex.Message}");
            }
        }

        private void DisableProxy()
        {
            try
            {
                string adb = ResolveAdbPath();
                if (File.Exists(adb))
                {
                    if (ConnectAdb(adb))
                    {
                        RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell settings put global http_proxy :0");
                        Debug.WriteLine("Proxy disattivato");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Proxy disable error: {ex.Message}");
            }
        }

        // === METODI GRAFICI ===

        private void RecreateBackBuffer()
        {
            _backBuffer?.Dispose();
            _bufferGraphics?.Dispose();

            if (this.Width > 0 && this.Height > 0)
            {
                _backBuffer = new Bitmap(this.Width, this.Height);
                _bufferGraphics = Graphics.FromImage(_backBuffer);
                _bufferGraphics.SmoothingMode = SmoothingMode.HighQuality;
            }
        }

        private void InitializeParticles()
        {
            for (int i = 0; i < ParticleCount; i++)
            {
                float startY = _random.Next(-Height, Height);
                _particlePositions[i] = new PointF(_random.Next(Width + 1), startY);
                _particleSpeeds[i] = 0.5f + (float)_random.NextDouble() * 2;
                _particleSizes[i] = _random.Next(3);
                _particleTargetPositions[i] = new PointF(_random.Next(Width), Height * 2);
                _particleRotations[i] = (float)(_random.NextDouble() * Math.PI * 2);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateParticles();
            Invalidate();
        }

        private void UpdateParticles()
        {
            for (int i = 0; i < ParticleCount; i++)
            {
                float deltaTime = 1.0f / 50;
                _particlePositions[i] = Lerp(_particlePositions[i], _particleTargetPositions[i], deltaTime * (_particleSpeeds[i] / 50));
                _particleRotations[i] += deltaTime * 0.5f;

                if (_particlePositions[i].Y > Height + 50)
                {
                    _particlePositions[i] = new PointF(_random.Next(Width + 1), -20f);
                    _particleTargetPositions[i] = new PointF(_random.Next(Width), Height * 2);
                    _particleSpeeds[i] = 0.5f + (float)_random.NextDouble() * 2;
                    _particleSizes[i] = _random.Next(3);
                    _particleRotations[i] = 0;
                }
            }
        }

        private PointF Lerp(PointF start, PointF end, float t)
        {
            return new PointF(start.X + (end.X - start.X) * t, start.Y + (end.Y - start.Y) * t);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_backBuffer == null)
                RecreateBackBuffer();

            if (_bufferGraphics != null)
            {
                _bufferGraphics.Clear(this.BackColor);
                PaintParticlesOnTop(new PaintEventArgs(_bufferGraphics, this.ClientRectangle));
                DrawProfessionalTitle(_bufferGraphics);
                e.Graphics.DrawImage(_backBuffer, 0, 0);
            }
            else
            {
                base.OnPaint(e);
                PaintParticlesOnTop(e);
                DrawProfessionalTitle(e.Graphics);
            }
        }

        private void PaintParticlesOnTop(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            for (int i = 0; i < ParticleCount; i++)
            {
                DrawTriangleWithGlow(e.Graphics, _particlePositions[i], _particleSizes[i], _particleRotations[i]);
            }
        }

        private void DrawTriangleWithGlow(Graphics graphics, PointF position, float size, float rotation)
        {
            float angle = (float)(Math.PI * 2 / 3);
            PointF[] vertices = new PointF[3];

            float finalSize = _triangleSize + size;

            for (int i = 0; i < 3; i++)
            {
                vertices[i] = new PointF(
                    position.X + finalSize * (float)Math.Cos(rotation + i * angle),
                    position.Y + finalSize * (float)Math.Sin(rotation + i * angle)
                );
            }

            for (int j = 0; j < _maxGlowLayers; j++)
            {
                int alpha = 15 - 3 * j;
                using (Brush glowBrush = new SolidBrush(Color.FromArgb(alpha, _particleColor.R, _particleColor.G, _particleColor.B)))
                {
                    float currentGlowSize = finalSize + j * (_glowSize / 2f);
                    graphics.FillEllipse(glowBrush,
                        position.X - currentGlowSize / 2,
                        position.Y - currentGlowSize / 2,
                        currentGlowSize,
                        currentGlowSize);
                }
            }

            using (Brush brush = new SolidBrush(_particleColor))
            {
                graphics.FillPolygon(brush, vertices);
            }
        }

        private void DrawProfessionalTitle(Graphics g)
        {
            using (Font titleFont = new Font("Segoe UI", 16, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(255, 230, 255)))
            {
                string title = "Contental X";
                SizeF titleSize = g.MeasureString(title, titleFont);

                float bgX = (Width - titleSize.Width - 9) / 2;
                float bgY = 15;
                float bgWidth = titleSize.Width + 8;
                float bgHeight = titleSize.Height + 8;

                using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                    new RectangleF(bgX, bgY, bgWidth, bgHeight),
                    Color.FromArgb(150, 60, 0, 60),
                    Color.FromArgb(100, 30, 0, 30),
                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(bgBrush, bgX, bgY, bgWidth, bgHeight);
                }

                using (Pen borderPen = new Pen(Color.FromArgb(200, 200, 50, 200), 1.5f))
                {
                    g.DrawRectangle(borderPen, bgX, bgY, bgWidth, bgHeight);
                }

                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                {
                    g.DrawString(title, titleFont, shadowBrush, (Width - titleSize.Width) / 2 + 1, 16);
                }

                g.DrawString(title, titleFont, titleBrush, (Width - titleSize.Width) / 2, 15);
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _progressAnimator?.Dispose();
                _backBuffer?.Dispose();
                _bufferGraphics?.Dispose();
                emulatorMonitorTimer?.Dispose();
                closeAppTimer?.Dispose();
                progressTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecreateBackBuffer();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void ApplyNeonStyleToButtons(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Guna2Button b)
                {
                    b.FillColor = Color.FromArgb(57, 255, 20);
                    b.ForeColor = Color.Black;
                    b.BorderRadius = 8;
                    b.HoverState.FillColor = Color.FromArgb(80, 255, 140);
                    b.HoverState.ForeColor = Color.Black;
                    b.ShadowDecoration.Enabled = true;
                    b.ShadowDecoration.Color = Color.FromArgb(57, 255, 20);
                }
                else if (c is Guna2GradientButton gb)
                {
                    gb.FillColor = Color.FromArgb(57, 255, 20);
                    gb.FillColor2 = Color.FromArgb(20, 120, 20);
                    gb.ForeColor = Color.Black;
                    gb.BorderRadius = 10;
                    gb.HoverState.FillColor = Color.FromArgb(80, 255, 140);
                    gb.HoverState.FillColor2 = Color.FromArgb(30, 180, 60);
                    gb.HoverState.ForeColor = Color.Black;
                    gb.ShadowDecoration.Enabled = true;
                    gb.ShadowDecoration.Color = Color.FromArgb(57, 255, 20);
                }

                if (c.HasChildren)
                    ApplyNeonStyleToButtons(c);
            }
        }
    }
}

namespace Bypass.Animations
{
    using Guna.UI2.WinForms;
    using System;
    using System.Windows.Forms;

    public sealed class CircularProgressAnimator : IDisposable
    {
        private readonly Guna2CircleProgressBar _bar;
        private readonly Timer _timer;

        public CircularProgressAnimator(Guna2CircleProgressBar bar)
        {
            _bar = bar;
            _timer = new Timer { Interval = 20 };
            _timer.Tick += (s, e) =>
            {
                if (_bar == null) return;
                int v = _bar.Value + 1;
                if (v > 100) v = 0;
                _bar.Value = v;
            };
        }

        public void StartAnimation()
        {
            if (_bar != null) _bar.Value = 0;
            _timer.Start();
        }

        public void SetValue(int value)
        {
            if (_bar == null) return;
            if (value < 0) value = 0;
            if (value > 100) value = 100;
            _bar.Value = value;
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}