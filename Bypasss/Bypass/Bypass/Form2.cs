using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bypass
{
    public partial class Form2 : Form
    {
        private string selectedEmulator = "";
        private string selectedFreeFire = "";
        private bool MSISelected = false;
        private const string CertHash = "c8750f0d";
        private const string EmbeddedResourceName = "Bypass.c8750f0d.0";
        private static string ProxyAddress = " 45.137.98.182:1744";
        private static string adbPort = "5555";
        private string targetDevice = "127.0.0.1:5555";
        private System.Windows.Forms.Timer emulatorMonitorTimer;
        private System.Windows.Forms.Timer progressTimer;
        private bool processCompleted = false;
        private int progressValue = 0;
        private System.Windows.Forms.Timer closeAppTimer; // Timer to close app after 30 seconds

        public Form2()
        {
            InitializeComponent();
            this.Size = new Size(383, 353);
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeLogTextBox();
            InitializeEmulatorMonitor();
            InitializeProgressTimer();
            InitializeCloseAppTimer(); // Initialize the close app timer

            // Nascondi tutti i panel all'avvio tranne guna2Panel7
            guna2Panel3.Hide();
            guna2Panel4.Hide();
            guna2Panel7.Show();

            // Imposta stato iniziale
            UpdateProgressUI(0, "Wait...", Color.Gray);

            // Debug: mostra risorse embedded
            CheckEmbeddedResources();

            // Add drag functionality to panels
            AddDragFunctionality();
        }

        private void AddDragFunctionality()
        {
            // Add mouse events to panels to enable dragging
            AddDragToPanel(guna2Panel1);
            AddDragToPanel(guna2Panel2);
            AddDragToPanel(guna2Panel3);
            AddDragToPanel(guna2Panel4);
            AddDragToPanel(guna2Panel5);
            AddDragToPanel(guna2Panel6);
            AddDragToPanel(guna2Panel7);
            AddDragToPanel(guna2Panel8);
        }

        private void AddDragToPanel(Guna2Panel panel)
        {
            panel.MouseDown += Panel_MouseDown;
            panel.MouseMove += Panel_MouseMove;
            panel.MouseUp += Panel_MouseUp;

            // Also add to child controls
            foreach (Control control in panel.Controls)
            {
                if (!(control is Guna2GradientButton)) // Exclude buttons
                {
                    control.MouseDown += Panel_MouseDown;
                    control.MouseMove += Panel_MouseMove;
                    control.MouseUp += Panel_MouseUp;
                }
            }
        }

        private Point mouseOffset;
        private bool isDragging = false;

        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                mouseOffset = new Point(-e.X, -e.Y);
            }
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                this.Location = mousePos;
            }
        }

        private void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void InitializeCloseAppTimer()
        {
            closeAppTimer = new System.Windows.Forms.Timer();
            closeAppTimer.Interval = 30000; // 30 seconds
            closeAppTimer.Tick += CloseAppTimer_Tick;
        }

        private void CloseAppTimer_Tick(object sender, EventArgs e)
        {
            // Close the application after 30 seconds
            closeAppTimer.Stop();
            Application.Exit();
        }

        private void InitializeEmulatorMonitor()
        {
            emulatorMonitorTimer = new System.Windows.Forms.Timer();
            emulatorMonitorTimer.Interval = 5000;
            emulatorMonitorTimer.Tick += EmulatorMonitorTimer_Tick;
        }

        private void InitializeProgressTimer()
        {
            progressTimer = new System.Windows.Forms.Timer();
            progressTimer.Interval = 100;
            progressTimer.Tick += ProgressTimer_Tick;
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (progressValue < 100)
            {
                progressValue += 2;
                guna2ProgressBar1.Value = progressValue;

                // Aggiorna il colore in base al progresso
                if (progressValue >= 80)
                {
                    guna2ProgressBar1.ProgressColor = Color.LimeGreen;
                    guna2ProgressBar1.ProgressColor2 = Color.LimeGreen;
                }
                else if (progressValue >= 50)
                {
                    guna2ProgressBar1.ProgressColor = Color.Orange;
                    guna2ProgressBar1.ProgressColor2 = Color.Orange;
                }
            }
            else
            {
                progressTimer.Stop();
            }
        }

        private void StartProgressAnimation()
        {
            progressValue = 0;
            guna2ProgressBar1.Value = 0;
            guna2ProgressBar1.ProgressColor = Color.RoyalBlue;
            guna2ProgressBar1.ProgressColor2 = Color.RoyalBlue;
            progressTimer.Start();
        }

        private void StopProgressAnimation()
        {
            progressTimer.Stop();
            guna2ProgressBar1.Value = 100;
            guna2ProgressBar1.ProgressColor = Color.LimeGreen;
            guna2ProgressBar1.ProgressColor2 = Color.LimeGreen;
        }

        private void UpdateProgressUI(int progress, string status, Color buttonColor)
        {
            if (guna2ProgressBar1.InvokeRequired)
            {
                guna2ProgressBar1.Invoke(new Action<int, string, Color>(UpdateProgressUI), progress, status, buttonColor);
                return;
            }

            guna2ProgressBar1.Value = progress;
            label11.Text = status;
            guna2Button1.FillColor = buttonColor;
            guna2Button1.ForeColor = Color.White;
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
                    Log("🔌 Emulatore chiuso, disattivazione proxy e chiusura applicazione...");
                    DisableProxy();
                    emulatorMonitorTimer.Stop();

                    // Chiudi l'applicazione dopo 2 secondi
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
                Log($"⚠️ Errore nel monitor emulatore: {ex.Message}");
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
                        string disableResult = RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell settings put global http_proxy :0");
                        if (!disableResult.Contains("error"))
                        {
                            Log("✅ Proxy disattivato");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"⚠️ Errore disattivazione proxy: {ex.Message}");
            }
        }

        private void KillEmulatorProcesses()
        {
            try
            {
                Log("🛑 Chiusura forzata dell'emulatore...");

                string[] processesToKill = { "HD-Player", "HD-Adb", "HD-MultiInstanceManager", "BstkSVC" };
                bool killedAny = false;

                foreach (var procName in processesToKill)
                {
                    foreach (var proc in Process.GetProcessesByName(procName))
                    {
                        try
                        {
                            Log($"🔪 Termino processo: {procName} (PID: {proc.Id})");
                            proc.Kill();
                            proc.WaitForExit(3000);
                            Log($"✅ Processo terminato: {procName}");
                            killedAny = true;
                        }
                        catch (Exception ex)
                        {
                            Log($"⚠️ Impossibile terminare {procName}: {ex.Message}");
                        }
                    }
                }

                if (killedAny)
                {
                    Log("✅ Tutti i processi dell'emulatore terminati");
                }
                else
                {
                    Log("ℹ️ Nessun processo dell'emulatore trovato in esecuzione");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ Errore durante la chiusura dell'emulatore: {ex.Message}");
            }
        }

        private void InitializeLogTextBox()
        {
            if (txtLog != null)
            {
                txtLog.ReadOnly = true;
                txtLog.BackColor = Color.FromArgb(15, 15, 15);
                txtLog.ForeColor = Color.LimeGreen;
                txtLog.Font = new Font("Consolas", 9);
                txtLog.ScrollBars = ScrollBars.Vertical;
                txtLog.Text = "=== LOGS STARTED ===\r\n";
            }
        }

        private void CheckEmbeddedResources()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resources = assembly.GetManifestResourceNames();

                Log("🔍 Risorse embedded disponibili:");
                foreach (var resource in resources)
                {
                    Log($"   📁 {resource}");
                }
            }
            catch (Exception ex)
            {
                Log($"⚠️ Errore controllo risorse: {ex.Message}");
            }
        }

        private async void guna2GradientButton8_Click(object sender, EventArgs e)
        {
            StartCompleteProcess();
            guna2Panel8.Visible = true;
        }

        // PULSANTE PER BLUESTACKS
        private void guna2GradientButton2_Click(object sender, EventArgs e)
        {
            SelectEmulator("BlueStacks NXT");
        }

        // PULSANTE PER MSI
        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            SelectEmulator("MSI App Player");
        }

        // PULSANTE PER FREE FIRE
        private void guna2GradientButton5_Click(object sender, EventArgs e)
        {
            SelectFreeFire("Free Fire");
        }

        // PULSANTE PER FREE FIRE MAX
        private void guna2GradientButton6_Click(object sender, EventArgs e)
        {
            SelectFreeFire("Free Fire Max");
        }

        private void SelectEmulator(string emulator)
        {
            selectedEmulator = emulator;
            MSISelected = (emulator == "MSI App Player");
            Log($"✅ Selezionato: {emulator}");

            // Inizia l'animazione della progress bar
            StartProgressAnimation();
            UpdateProgressUI(0, "Connecting...", Color.RoyalBlue);

            // Nascondi il panel degli emulatori e mostra guna2Panel7
            guna2Panel3.Hide();
            ShowPanel7();

            // Continua con il processo
            ContinueAfterEmulatorSelection();
        }

        private void SelectFreeFire(string freeFire)
        {
            selectedFreeFire = freeFire;
            Log($"✅ Selezionato: {freeFire}");

            // Nascondi il panel dei Free Fire
            //guna2Panel4.Hide();

            // Continua con il processo
            ContinueAfterFreeFireSelection();
        }

        private void ShowPanel3()
        {
            guna2Panel7.Hide();
            guna2Panel3.Location = new Point(0, 0);
            guna2Panel3.Show();
            guna2Panel3.BringToFront();
        }

        private void ShowPanel4()
        {
            guna2Panel7.Hide();
            guna2Panel4.Location = new Point(0, 0);
            guna2Panel4.Show();
            guna2Panel4.BringToFront();
        }

        private void ShowPanel7()
        {
            guna2Panel3.Hide();
            guna2Panel4.Hide();
            guna2Panel7.Location = new Point(0, 0);
            guna2Panel7.Show();
            guna2Panel7.BringToFront();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (emulatorMonitorTimer != null)
            {
                emulatorMonitorTimer.Stop();
                emulatorMonitorTimer.Dispose();
            }

            if (progressTimer != null)
            {
                progressTimer.Stop();
                progressTimer.Dispose();
            }

            if (closeAppTimer != null)
            {
                closeAppTimer.Stop();
                closeAppTimer.Dispose();
            }

            if (!processCompleted || e.CloseReason == CloseReason.UserClosing)
            {
                Log("👤 Chiusura manuale dell'applicazione...");
                DisableProxy();
                KillEmulatorProcesses();
                Log("✅ Applicazione chiusa completamente");
            }
        }

        private async void StartCompleteProcess()
        {
            try
            {
                guna2GradientButton8.Enabled = false;
                guna2GradientButton8.Text = "Processing...";
                ClearLogs();

                Log("🚀 Avvio processo completo...");

                // Nascondi guna2Panel7 e mostra guna2Panel3 per selezione emulatore
                ShowPanel3();
                Log("👆 Seleziona un emulatore");
            }
            catch (Exception ex)
            {
                Log($"💥 ERRORE CRITICO: {ex.Message}", false, true);
                MessageBox.Show($"Errore durante il processo: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                guna2GradientButton8.Enabled = true;
                guna2GradientButton8.Text = "Start Complete Process";
            }
        }

        private async void ContinueAfterEmulatorSelection()
        {
            try
            {
                // 2. REQUEST ACCESS
                UpdateProgressUI(20, "Request Access...", Color.Orange);
                Log("📁 Richiesta accesso in corso...");
                if (!await RequestAccess())
                {
                    Log("❌ Request Access fallito!", false, true);
                    UpdateProgressUI(0, "Failed!", Color.Red);
                    return;
                }
                Log("✅ Accesso ottenuto con successo!", true);

                // 3. ATTESA 30 SECONDI
                UpdateProgressUI(40, "Waiting Emulator...", Color.Orange);
                Log("⏳ Attendo 30 secondi che l'emulatore sia pronto...");
                for (int i = 30; i > 0; i--)
                {
                    UpdateProgressUI(40 + (int)((30 - i) / 30.0 * 20), $"Waiting {i}s...", Color.Orange);
                    Log($"⏰ {i} secondi rimanenti...");
                    await Task.Delay(1000);
                }

                // 4. INSTALLAZIONE CERTIFICATO
                UpdateProgressUI(60, "Installing Certificate...", Color.Orange);
                Log("🔐 Installazione certificato...");
                if (!await InstallCertificate())
                {
                    Log("❌ Installazione certificato fallita!", false, true);
                    UpdateProgressUI(0, "Failed!", Color.Red);
                    return;
                }
                Log("✅ Certificato installato con successo!", true);

                // Completa la progress bar e mostra stato finale
                StopProgressAnimation();
                UpdateProgressUI(100, "Finish!", Color.LimeGreen);

                // Aspetta un po' per mostrare lo stato finale
                await Task.Delay(1000);

                // Mostra il panel per la selezione di Free Fire
                ShowPanel4();
                Log("👆 Seleziona Free Fire");
            }
            catch (Exception ex)
            {
                Log($"💥 ERRORE CRITICO: {ex.Message}", false, true);
                UpdateProgressUI(0, "Error!", Color.Red);
                MessageBox.Show($"Errore durante il processo: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                guna2GradientButton8.Enabled = true;
                guna2GradientButton8.Text = "Start Complete Process";
            }
        }

        private async void ContinueAfterFreeFireSelection()
        {
            try
            {
                // 6. APPLICA BYPASS E AVVIA FREE FIRE
                Log($"🎮 Attivazione bypass per {selectedFreeFire}...");
                if (await ActivateBypassAndStartFreeFire())
                {
                    Log($"✅ Bypass attivato e {selectedFreeFire} avviato con successo!", true);
                    Log("🎉 PROCESSO COMPLETATO CON SUCCESSO!");

                    processCompleted = true;

                    // Nascondi l'applicazione
                    this.Hide();

                    // Additional hiding to ensure the form is completely hidden
                    this.Visible = false;
                    this.Opacity = 0; // Make fully transparent as an additional hiding method

                    // Avvia il monitor dell'emulatore
                    emulatorMonitorTimer.Start();

                    Log("👻 Applicazione nascosta - Monitoraggio emulatore attivo");
                }
                else
                {
                    Log($"❌ Impossibile attivare bypass per {selectedFreeFire}", false, true);
                    guna2GradientButton8.Enabled = true;
                    guna2GradientButton8.Text = "Start Complete Process";
                }
            }
            catch (Exception ex)
            {
                Log($"💥 ERRORE CRITICO: {ex.Message}", false, true);
                MessageBox.Show($"Errore durante il processo: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                guna2GradientButton8.Enabled = true;
                guna2GradientButton8.Text = "Start Complete Process";
            }
        }

        private async Task<bool> RequestAccess()
        {
            return await Task.Run(() =>
            {
                try
                {
                    Log("🛑 Fermo processi emulatore per Request Access...");

                    string[] processesToKill = { "HD-Player", "HD-Adb", "HD-MultiInstanceManager", "BstkSVC" };
                    foreach (var procName in processesToKill)
                    {
                        foreach (var proc in Process.GetProcessesByName(procName))
                        {
                            try
                            {
                                Log($"🔪 Termino processo: {procName} (PID: {proc.Id})");
                                proc.Kill();
                                proc.WaitForExit(2000);
                                Log($"✅ Processo terminato: {procName}");
                            }
                            catch (Exception ex)
                            {
                                Log($"⚠️ Impossibile terminare {procName}: {ex.Message}");
                            }
                        }
                    }

                    string engineRoot = MSISelected ?
                        @"C:\ProgramData\Bluestacks_msi5\Engine" :
                        @"C:\ProgramData\BlueStacks_nxt\Engine";

                    Log($"📁 Verifico directory: {engineRoot}");
                    if (!Directory.Exists(engineRoot))
                    {
                        Log($"❌ Directory Engine non trovata: {engineRoot}", false, true);
                        return false;
                    }

                    Log("✏️ Modifica file di configurazione...");
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

                    Log("✅ Access granted.", true);

                    string emulatorExe;
                    if (MSISelected)
                        emulatorExe = @"C:\Program Files\BlueStacks_msi5\HD-Player.exe";
                    else
                        emulatorExe = @"C:\Program Files\BlueStacks_nxt\HD-Player.exe";

                    if (File.Exists(emulatorExe))
                    {
                        Log($"🎯 Riavvio emulatore: {emulatorExe}");
                        Process.Start(emulatorExe);
                        Log("✅ Emulator started successfully.", true);
                    }
                    else
                    {
                        Log($"❌ Emulator executable not found at: {emulatorExe}", false, true);
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Log($"❌ Request Access failed: {ex.Message}", false, true);
                    return false;
                }
            });
        }

        private async Task<bool> InstallCertificate()
        {
            return await Task.Run(() =>
            {
                try
                {
                    Log("🔍 Cerco ADB...");
                    string adb = ResolveAdbPath();
                    if (!File.Exists(adb))
                    {
                        Log("❌ ADB non trovato!", false, true);
                        return false;
                    }
                    Log($"✅ ADB trovato: {adb}");

                    bool adbConnected = false;
                    for (int attempt = 1; attempt <= 10; attempt++)
                    {
                        UpdateProgressUI(60 + (attempt * 2), $"Connecting ADB {attempt}/10...", Color.Orange);
                        Log($"🔗 Tentativo connessione ADB {attempt}/10...");
                        if (ConnectAdb(adb))
                        {
                            adbConnected = true;
                            break;
                        }
                        Thread.Sleep(3000);
                    }

                    if (!adbConnected)
                    {
                        Log("❌ Connessione ADB fallita dopo 10 tentativi!", false, true);
                        return false;
                    }

                    Log("📱 Rilevamento dispositivi connessi...");
                    string devicesOutput = RunAdbCommandWithOutput(adb, "devices");
                    Log($"📋 Lista dispositivi: {devicesOutput}");

                    if (!devicesOutput.Contains(targetDevice))
                    {
                        Log($"❌ Dispositivo {targetDevice} non trovato!", false, true);
                        return false;
                    }

                    Log($"🎯 Utilizzo dispositivo: {targetDevice}");

                    UpdateProgressUI(80, "Extracting Certificate...", Color.Orange);
                    Log("📦 Estrazione certificato embedded...");
                    string tempCertPath = ExtractEmbeddedCert(EmbeddedResourceName, CertHash);
                    if (!File.Exists(tempCertPath))
                    {
                        Log("❌ Certificato embedded non trovato!", false, true);
                        return false;
                    }
                    Log($"✅ Certificato estratto: {tempCertPath}");

                    UpdateProgressUI(85, "Pushing Certificate...", Color.Orange);
                    Log("📤 Push certificato su dispositivo...");
                    string pushResult = RunAdbCommandWithOutput(adb, $"-s {targetDevice} push \"{tempCertPath}\" /sdcard/{CertHash}.0");
                    if (pushResult.Contains("error") || pushResult.Contains("failed"))
                    {
                        Log($"❌ Push certificato fallito: {pushResult}", false, true);
                        return false;
                    }
                    Log("✅ Certificato pushato sul dispositivo");

                    UpdateProgressUI(90, "Installing Certificate...", Color.Orange);
                    Log("⚙️ Installazione certificato nel sistema...");
                    string suPath = "/boot/android/android/system/xbin/bstk/su";
                    string installCmd = $"{suPath} -c '" +
                        $"mount -o rw,remount /dev/sda1 /system && " +
                        $"cp /sdcard/{CertHash}.0 /system/etc/security/cacerts/{CertHash}.0 && " +
                        $"chmod 644 /system/etc/security/cacerts/{CertHash}.0 && " +
                        $"chcon u:object_r:system_file:s0 /system/etc/security/cacerts/{CertHash}.0 && " +
                        $"mount -o ro,remount /dev/sda1 /system && " +
                        $"rm /sdcard/{CertHash}.0 && " +
                        $"setprop ctl.restart zygote'";

                    string installResult = RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell \"{installCmd}\"");

                    UpdateProgressUI(95, "Restarting System...", Color.Orange);
                    Log("⏳ Attendo riavvio sistema (15 secondi)...");
                    Thread.Sleep(15000);

                    Log("🔗 Riconnessione ADB dopo riavvio...");
                    bool reconnected = false;
                    for (int attempt = 1; attempt <= 10; attempt++)
                    {
                        Log($"🔗 Tentativo riconnessione ADB {attempt}/10...");
                        if (ConnectAdb(adb))
                        {
                            reconnected = true;
                            break;
                        }
                        Thread.Sleep(3000);
                    }

                    if (!reconnected)
                    {
                        Log("❌ Riconnessione ADB fallita dopo riavvio!", false, true);
                        return false;
                    }

                    UpdateProgressUI(98, "Verifying Certificate...", Color.Orange);
                    Log("🔍 Verifica installazione certificato...");
                    bool certificateVerified = false;

                    for (int attempt = 1; attempt <= 5; attempt++)
                    {
                        Log($"🔍 Tentativo verifica {attempt}/5...");
                        string verifyResult = RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell \"[ -f /system/etc/security/cacerts/{CertHash}.0 ] && echo EXISTS\"");

                        if (verifyResult.Contains("EXISTS"))
                        {
                            certificateVerified = true;
                            break;
                        }
                        else if (verifyResult.Contains("error: closed") || verifyResult.Contains("device not found"))
                        {
                            Log("🔗 Connessione chiusa, riconnetto...");
                            ConnectAdb(adb);
                            Thread.Sleep(3000);
                        }
                        else
                        {
                            Log($"⚠️ Verifica fallita: {verifyResult}");
                        }

                        Thread.Sleep(3000);
                    }

                    if (certificateVerified)
                    {
                        try
                        {
                            File.Delete(tempCertPath);
                            Log("🧹 File temporaneo eliminato");
                        }
                        catch { }
                        Log("✅ Certificato verificato con successo", true);
                        return true;
                    }
                    else
                    {
                        Log("🔄 Ultimo tentativo di verifica...");
                        string finalVerify = RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell ls /system/etc/security/cacerts/{CertHash}.0");
                        if (!finalVerify.Contains("No such file") && !finalVerify.Contains("error"))
                        {
                            try
                            {
                                File.Delete(tempCertPath);
                                Log("🧹 File temporaneo eliminato");
                            }
                            catch { }
                            Log("✅ Certificato verificato con successo", true);
                            return true;
                        }
                    }

                    Log("❌ Verifica certificato fallita dopo tutti i tentativi", false, true);
                    return false;
                }
                catch (Exception ex)
                {
                    Log($"❌ Errore installazione certificato: {ex.Message}", false, true);
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

                    Log($"🎯 Target package: {packageName}");

                    string adb = ResolveAdbPath();

                    Log("🛑 Fermo applicazione se in esecuzione...");
                    string stopResult = RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell am force-stop {packageName}");
                    Thread.Sleep(2000);

                    Log("🌐 Applicazione proxy...");
                    string proxyResult = RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell settings put global http_proxy {ProxyAddress}");
                    if (proxyResult.Contains("error") || proxyResult.Contains("failed"))
                    {
                        Log($"❌ Applicazione proxy fallita: {proxyResult}", false, true);
                        return false;
                    }
                    Log($"✅ Proxy applicato: {ProxyAddress}");

                    Log("🚀 Avvio applicazione...");
                    string startResult = RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell monkey -p {packageName} -c android.intent.category.LAUNCHER 1");
                    Thread.Sleep(3000);

                    if (startResult.Contains("error") || startResult.Contains("failed"))
                    {
                        Log($"❌ Avvio applicazione fallito: {startResult}", false, true);
                        return false;
                    }

                    Log($"✅ {selectedFreeFire} avviato con successo!");

                    // Hide the form immediately after starting Free Fire
                    // We need to invoke this on the UI thread
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() =>
                        {
                            this.Hide();
                            this.Visible = false;
                            this.Opacity = 0;
                        }));
                    }
                    else
                    {
                        this.Hide();
                        this.Visible = false;
                        this.Opacity = 0;
                    }

                    // ATTESA 30 SECONDI PRIMA DI DISABILITARE IL PROXY
                    Log("⏳ Attendo 30 secondi prima di disattivare il proxy...");
                    for (int i = 300000; i > 0; i--)
                    {
                        Log($"⏰ Disattivazione proxy tra {i} secondi...");
                        Thread.Sleep(1000);
                    }

                    // DISABILITA IL PROXY DOPO 30 SECONDI
                    Log("🔌 Disattivazione proxy dopo 30 secondi...");
                    string disableResult = RunAdbCommandWithOutput(adb, $"-s {targetDevice} shell settings put global http_proxy :0");
                    if (!disableResult.Contains("error"))
                    {
                        Log("✅ Proxy disattivato dopo 30 secondi");
                    }
                    else
                    {
                        Log($"⚠️ Impossibile disattivare proxy: {disableResult}");
                    }

                    // Start the timer to close the application after 30 seconds
                    closeAppTimer.Start();

                    Log("✅ Bypass completato! Free Fire avviato e proxy disattivato dopo 30 secondi");
                    return true;
                }
                catch (Exception ex)
                {
                    Log($"❌ Errore attivazione bypass: {ex.Message}", false, true);
                    return false;
                }
            });
        }

        private bool ConnectAdb(string adbExe)
        {
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
                proc.WaitForExit();

                if (output.Contains("connected") || output.Contains("already"))
                {
                    Log("✅ ADB connected.", true);
                    return true;
                }
                Log($"❌ ADB connection failed: {output} {error}", false, true);
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

        private string ExtractEmbeddedCert(string resourceName, string hash)
        {
            string tempCertPath = "";

            try
            {
                tempCertPath = Path.Combine(Path.GetTempPath(), hash + ".0");
                Log($"📁 Tentativo di estrarre certificato con hash: {hash}");

                var assembly = Assembly.GetExecutingAssembly();
                string assemblyName = assembly.GetName().Name;

                // Lista completa di tutti i possibili nomi di risorse
                string[] possibleResourceNames = {
            resourceName, // Nome originale passato
            "Bypass.Bypass.c8750f0d.0", // Namespace.ClassName.ResourceName
            $"{assemblyName}.Bypass.c8750f0d.0", // AssemblyName.ClassName.ResourceName
            $"{assemblyName}.c8750f0d.0", // AssemblyName.ResourceName
            "Bypass.c8750f0d.0", // Solo ResourceName
            "c8750f0d.0", // Nome semplice
            "Bypass_BFV1.c8750f0d.0", // Con underscore
            "Bypass BFV1.c8750f0d.0", // Con spazio
            "Bypass_OFIC.c8750f0d.0", // Alternativo
            $"{assemblyName}.Resources.c8750f0d.0", // In cartella Resources
            $"{assemblyName}.Properties.Resources.c8750f0d.0", // In Properties/Resources
            "Resources.c8750f0d.0", // Cartella Resources
            "Properties.Resources.c8750f0d.0" // Cartella Properties/Resources
        };

                Stream resourceStream = null;
                string foundResource = "";

                // Prima verifica quali risorse esistono realmente
                var allResources = assembly.GetManifestResourceNames();
                Log("🔍 Risorse embedded disponibili:");
                foreach (var res in allResources)
                {
                    Log($"   📁 {res}");
                }

                // Poi cerca tra i nomi possibili
                foreach (var possibleName in possibleResourceNames)
                {
                    Log($"🔍 Cerco risorsa: {possibleName}");
                    resourceStream = assembly.GetManifestResourceStream(possibleName);
                    if (resourceStream != null)
                    {
                        foundResource = possibleName;
                        Log($"✅ Risorsa trovata: {possibleName}");
                        break;
                    }
                }

                if (resourceStream == null)
                {
                    // Ultimo tentativo: cerca qualsiasi risorsa che contenga l'hash
                    foreach (var actualResource in allResources)
                    {
                        if (actualResource.Contains(hash) || actualResource.Contains("c8750f0d"))
                        {
                            Log($"🔍 Provo risorsa contenente hash: {actualResource}");
                            resourceStream = assembly.GetManifestResourceStream(actualResource);
                            if (resourceStream != null)
                            {
                                foundResource = actualResource;
                                Log($"✅ Risorsa trovata per contenuto: {actualResource}");
                                break;
                            }
                        }
                    }
                }

                if (resourceStream == null)
                {
                    Log("❌ Nessuna risorsa trovata con i nomi provati");
                    return string.Empty;
                }

                // Estrai la risorsa
                using (resourceStream)
                using (var fileStream = File.Create(tempCertPath))
                {
                    resourceStream.CopyTo(fileStream);
                }

                if (File.Exists(tempCertPath))
                {
                    FileInfo fileInfo = new FileInfo(tempCertPath);
                    Log($"✅ Certificato estratto: {tempCertPath} ({fileInfo.Length} bytes)");
                    return tempCertPath;
                }
                else
                {
                    Log("❌ File temporaneo non creato");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Log($"❌ Errore estrazione certificato: {ex.Message}");
                return string.Empty;
            }
        }

        private string RunAdbCommandWithOutput(string adbPath, string arguments)
        {
            try
            {
                Log($"🔧 Eseguo: {adbPath} {arguments}");
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit(10000);

                    string result = output + error;

                    if (!string.IsNullOrEmpty(output.Trim()))
                        Log($"📤 Output: {output.Trim()}");
                    if (!string.IsNullOrEmpty(error.Trim()))
                        Log($"📥 Error: {error.Trim()}");

                    return result;
                }
            }
            catch (Exception ex)
            {
                Log($"❌ ADB Command failed: {ex.Message}", false, true);
                return $"error: {ex.Message}";
            }
        }

        private void EditConfigs(string engineRoot)
        {
            try
            {
                Log($"📝 Modifica configs in: {engineRoot}");
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
                        Log($"   📄 Elaboro file: {Path.GetFileName(file)}");
                        string content = File.ReadAllText(file, Encoding.UTF8);

                        content = System.Text.RegularExpressions.Regex.Replace(content,
                            @"(<HardDisk\b[^>]*location\s*=\s*""Root\.vhd""[^>]*type\s*=\s*"")Readonly(""\s*/?>)",
                            @"$1Normal$2", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                        content = System.Text.RegularExpressions.Regex.Replace(content,
                            @"(<HardDisk\b[^>]*location\s*=\s*""Data\.vhdx""[^>]*type\s*=\s*"")Readonly(""\s*/?>)",
                            @"$1Normal$2", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                        File.WriteAllText(file, content, Encoding.UTF8);
                        Log($"   ✅ File modificato: {Path.GetFileName(file)}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"EditConfigs failed: {ex.Message}");
            }
        }

        private void ClearLogs()
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(ClearLogs));
                return;
            }
            txtLog.Text = "=== LOGS CLEARED ===\r\n";
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void Log(string message, bool success = false, bool error = false)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => Log(message, success, error)));
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string icon = success ? "✅" : error ? "❌" : "📝";
            string logMessage = $"[{timestamp}] {icon} {message}\r\n";

            txtLog.AppendText(logMessage);
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
            Debug.WriteLine(logMessage.Trim());
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Log("🔧 Applicazione avviata - Pronto per iniziare");
        }

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            ClearLogs();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            // Pulsante di stato - può essere usato per azioni aggiuntive
            if (label11.Text == "Finish!")
            {
                MessageBox.Show("Processo completato con successo!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Form2_Load_1(object sender, EventArgs e)
        {
            guna2Panel8.Visible = false;
        }
    }
}