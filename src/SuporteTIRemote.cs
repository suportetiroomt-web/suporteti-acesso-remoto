using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SuporteTIRemote
{
    internal sealed class MainForm : Form
    {
        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_MAXIMIZEBOX = 0x00010000;
        private const int WS_SYSMENU = 0x00080000;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_FRAMECHANGED = 0x0020;

        private readonly Panel hostPanel;
        private readonly Label statusLabel;
        private readonly System.Windows.Forms.Timer attachTimer;
        private readonly DateTime launchStarted = DateTime.Now.AddSeconds(-2);
        private readonly List<Process> ownedProcesses = new List<Process>();
        private IntPtr embeddedWindow = IntPtr.Zero;
        private string payloadPath;
        private string rustDeskConfigPath;
        private string originalRustDeskConfig;
        private bool originalRustDeskConfigExisted;
        private bool compatibilityConfigPrepared;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr child, IntPtr newParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int value);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int width, int height, uint flags);

        internal MainForm()
        {
            Text = "Suporte TI - Acesso Remoto";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            BackColor = Color.FromArgb(5, 16, 31);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(284, 446);

            var header = new Panel {
                Dock = DockStyle.Top,
                Height = 92,
                BackColor = Color.FromArgb(4, 27, 51)
            };
            Controls.Add(header);

            var logo = new PictureBox {
                Image = LoadBrandLogo(),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Location = new Point(18, 12),
                Size = new Size(46, 46)
            };
            header.Controls.Add(logo);

            var brand = new Label {
                Text = "SUPORTE TI",
                ForeColor = Color.FromArgb(58, 190, 255),
                Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(72, 14)
            };
            header.Controls.Add(brand);

            var subtitle = new Label {
                Text = "ACESSO REMOTO SEGURO",
                ForeColor = Color.FromArgb(151, 169, 190),
                Font = new Font("Segoe UI Semibold", 8F),
                AutoSize = true,
                Location = new Point(73, 49)
            };
            header.Controls.Add(subtitle);

            var instruction = new Label {
                Text = "Informe o ID e a senha abaixo ao técnico.",
                ForeColor = Color.FromArgb(197, 211, 228),
                AutoSize = true,
                Location = new Point(22, 70)
            };
            header.Controls.Add(instruction);

            hostPanel = new Panel {
                BackColor = Color.White,
                Location = new Point(40, 108),
                Size = new Size(204, 272),
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(hostPanel);

            statusLabel = new Label {
                Text = "Iniciando conexão segura...",
                ForeColor = Color.FromArgb(151, 169, 190),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(12, 390),
                Size = new Size(260, 22)
            };
            Controls.Add(statusLabel);

            var footer = new Label {
                Text = "Servidor privado Suporte TI",
                ForeColor = Color.FromArgb(91, 118, 147),
                Font = new Font("Segoe UI", 7.5F),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(12, 414),
                Size = new Size(260, 18)
            };
            Controls.Add(footer);

            Shown += delegate { StartPayload(); };
            FormClosing += delegate { StopPayload(); };

            attachTimer = new System.Windows.Forms.Timer();
            attachTimer.Interval = 500;
            attachTimer.Tick += delegate { AttachRustDeskWindow(); };
        }

        private void StartPayload()
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SuporteTI", "AcessoRemoto");
                Directory.CreateDirectory(dir);
                payloadPath = Path.Combine(dir, "SuporteTI-qs-rustdesk-licensed-9Jybm5WaukGdlRncvBXdz5iclZnclNnI6ISehxWZyJCLiIiOikGchJCLi8mZulmLpRXZ0J3bwV3cuIXZ2JXZzJiOiQ3cvhmIsISPzpXMZZlZ2h1UDd2VydlYLl0T15WOtpFT0w2MQNGONZkRBF0apdGeycWUyIiOikXZrJye.exe");
                ExtractPayload(payloadPath);
                PrepareCompatibilityConfig();
                var process = Process.Start(new ProcessStartInfo(payloadPath) { UseShellExecute = true });
                if (process != null) ownedProcesses.Add(process);
                attachTimer.Start();
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Não foi possível iniciar: " + ex.Message;
                statusLabel.ForeColor = Color.FromArgb(255, 110, 110);
            }
        }

        private static void ExtractPayload(string destination)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var source = assembly.GetManifestResourceStream("RustDeskPayload"))
            {
                if (source == null) throw new InvalidOperationException("Motor RustDesk não encontrado.");
                using (var target = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None))
                    source.CopyTo(target);
            }
        }

        private static Image LoadBrandLogo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var source = assembly.GetManifestResourceStream("SuporteTILogo"))
            {
                if (source == null) return null;
                using (var image = Image.FromStream(source))
                    return new Bitmap(image);
            }
        }

        private void PrepareCompatibilityConfig()
        {
            var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configDirectory = Path.Combine(roaming, "RustDesk", "config");
            Directory.CreateDirectory(configDirectory);
            rustDeskConfigPath = Path.Combine(configDirectory, "RustDesk2.toml");
            originalRustDeskConfigExisted = File.Exists(rustDeskConfigPath);
            originalRustDeskConfig = originalRustDeskConfigExisted
                ? File.ReadAllText(rustDeskConfigPath, Encoding.UTF8)
                : string.Empty;

            var compatible = SetTomlOption(originalRustDeskConfig, "enable-hwcodec", "N");
            compatible = SetTomlOption(compatible, "enable-directx-capture", "N");
            File.WriteAllText(rustDeskConfigPath, compatible, new UTF8Encoding(false));
            compatibilityConfigPrepared = true;
        }

        private static string SetTomlOption(string content, string key, string value)
        {
            var newline = content.Contains("\r\n") ? "\r\n" : "\n";
            var optionLine = key + " = '" + value + "'";
            var keyPattern = @"(?m)^\s*" + Regex.Escape(key) + @"\s*=.*$";
            if (Regex.IsMatch(content, keyPattern))
                return new Regex(keyPattern).Replace(content, optionLine, 1);

            var options = Regex.Match(content, @"(?m)^\[options\]\s*$");
            if (!options.Success)
            {
                if (content.Length > 0 && !content.EndsWith("\n")) content += newline;
                return content + newline + "[options]" + newline + optionLine + newline;
            }

            var sectionStart = options.Index + options.Length;
            var nextSection = Regex.Match(content.Substring(sectionStart), @"(?m)^\[[^\]]+\]\s*$");
            var insertAt = nextSection.Success ? sectionStart + nextSection.Index : content.Length;
            var prefix = content.Substring(0, insertAt);
            var suffix = content.Substring(insertAt);
            if (!prefix.EndsWith("\n")) prefix += newline;
            return prefix + optionLine + newline + suffix;
        }

        private void AttachRustDeskWindow()
        {
            if (embeddedWindow != IntPtr.Zero) return;
            foreach (var process in Process.GetProcessesByName("rustdesk"))
            {
                try
                {
                    if (process.StartTime < launchStarted) continue;
                    process.Refresh();
                    if (process.MainWindowHandle == IntPtr.Zero) continue;
                    embeddedWindow = process.MainWindowHandle;
                    ownedProcesses.Add(process);
                    var style = GetWindowLong(embeddedWindow, GWL_STYLE);
                    style &= ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU);
                    SetWindowLong(embeddedWindow, GWL_STYLE, style);
                    SetParent(embeddedWindow, hostPanel.Handle);
                    SetWindowPos(embeddedWindow, IntPtr.Zero, 0, 0, 900, 610, SWP_SHOWWINDOW | SWP_FRAMECHANGED);
                    statusLabel.Text = "● Conectado ao servidor Suporte TI";
                    statusLabel.ForeColor = Color.FromArgb(65, 211, 142);
                    attachTimer.Stop();
                    return;
                }
                catch { }
            }
        }

        private void StopPayload()
        {
            attachTimer.Stop();
            foreach (var process in ownedProcesses)
            {
                try
                {
                    if (!process.HasExited) process.Kill();
                    process.WaitForExit(2000);
                }
                catch { }
            }
            RestoreCompatibilityConfig();
        }

        private void RestoreCompatibilityConfig()
        {
            if (!compatibilityConfigPrepared || string.IsNullOrEmpty(rustDeskConfigPath)) return;
            try
            {
                if (originalRustDeskConfigExisted)
                    File.WriteAllText(rustDeskConfigPath, originalRustDeskConfig, new UTF8Encoding(false));
                else if (File.Exists(rustDeskConfigPath))
                    File.Delete(rustDeskConfigPath);
            }
            catch { }
            compatibilityConfigPrepared = false;
        }

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
