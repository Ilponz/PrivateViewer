using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

// ==============================================================================
// METADATI UFFICIALI E FIRMA INDELEBILE DELL'AUTORE (Proprietà file .EXE Windows)
// ==============================================================================
[assembly: AssemblyTitle("PrivateViewer (Chromium Edition)")]
[assembly: AssemblyDescription("Created by Alex Ponzin (@Ilponz) - Anti-Capture Protected Utility")]
[assembly: AssemblyCompany("Alex Ponzin (Ilponz)")]
[assembly: AssemblyProduct("PrivateViewer")]
[assembly: AssemblyCopyright("Copyright © 2026 Alex Ponzin (@Ilponz). All rights reserved.")]
[assembly: AssemblyTrademark("Original Software by Alex Ponzin")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace PrivateViewerApp
{
    public class MainForm : Form, IMessageFilter
    {
        // Costanti autore
        public const string AUTHOR_NAME = "Alex Ponzin";
        public const string AUTHOR_HANDLE = "@Ilponz";
        public const string REPO_URL = "https://github.com/Ilponz/PrivateViewer";

        // Win32 API Constants & Functions
        private const uint WDA_NONE = 0x00000000;
        private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;
        private const int WM_KEYDOWN = 0x0100;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowDisplayAffinity(IntPtr hWnd, out uint pdwAffinity);

        // Top Bar Controls
        private Panel topPanel;
        private Label lblTitle;
        private Button btnToggleAffinity;
        private CheckBox chkAlwaysOnTop;
        private TrackBar tbOpacity;
        private Label lblOpacity;

        // Bottom Watermark Panel (Indelebile)
        private Panel bottomPanel;
        private Label lblFooterStatus;
        private LinkLabel lnkWatermark;

        // Tabs
        private TabControl tabControl;
        private TabPage tabNotes;
        private TabPage tabBrowser;

        // Notes Tab Controls
        private TextBox txtNotes;

        // Browser Tab Controls (WebView2 & Advanced Navigation)
        private Panel browserBar;
        private Button btnBack;
        private Button btnForward;
        private Button btnReload;
        private Button btnHome;
        private TextBox txtUrl;
        private Button btnGo;
        private WebView2 webView;
        private ToolTip toolTip;
        private bool isLoading = false;

        // System Tray (Boss Key)
        private NotifyIcon trayIcon;
        private bool isProtected = true;

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "PrivateViewer (Chromium Edition) - by Alex Ponzin (@Ilponz)";
            this.Size = new Size(980, 660);
            this.MinimumSize = new Size(680, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.KeyPreview = true;

            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 4000;
            toolTip.InitialDelay = 500;

            // Installa message filter globale per garantire che la Boss Key (ESC) funzioni sempre
            Application.AddMessageFilter(this);

            // ==========================================
            // 1. Barra Superiore di Controllo
            // ==========================================
            topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 50;
            topPanel.BackColor = Color.FromArgb(40, 40, 40);
            topPanel.Padding = new Padding(10, 8, 10, 8);

            lblTitle = new Label();
            lblTitle.Text = "🛡️ PrivateViewer";
            lblTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(12, 14);

            btnToggleAffinity = new Button();
            btnToggleAffinity.Text = "🛡️ Invisibile a Cattura: ATTIVO";
            btnToggleAffinity.BackColor = Color.FromArgb(34, 139, 34);
            btnToggleAffinity.ForeColor = Color.White;
            btnToggleAffinity.FlatStyle = FlatStyle.Flat;
            btnToggleAffinity.FlatAppearance.BorderSize = 0;
            btnToggleAffinity.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnToggleAffinity.Size = new Size(240, 32);
            btnToggleAffinity.Location = new Point(170, 9);
            btnToggleAffinity.Cursor = Cursors.Hand;
            btnToggleAffinity.Click += BtnToggleAffinity_Click;
            toolTip.SetToolTip(btnToggleAffinity, "Attiva o disattiva la protezione da registrazione e screenshot");

            chkAlwaysOnTop = new CheckBox();
            chkAlwaysOnTop.Text = "Sempre in primo piano";
            chkAlwaysOnTop.ForeColor = Color.LightGray;
            chkAlwaysOnTop.Font = new Font("Segoe UI", 9);
            chkAlwaysOnTop.AutoSize = true;
            chkAlwaysOnTop.Location = new Point(430, 15);
            chkAlwaysOnTop.CheckedChanged += (s, e) => { this.TopMost = chkAlwaysOnTop.Checked; };

            lblOpacity = new Label();
            lblOpacity.Text = "Opacità:";
            lblOpacity.ForeColor = Color.LightGray;
            lblOpacity.Font = new Font("Segoe UI", 9);
            lblOpacity.AutoSize = true;
            lblOpacity.Location = new Point(600, 15);

            tbOpacity = new TrackBar();
            tbOpacity.Minimum = 30;
            tbOpacity.Maximum = 100;
            tbOpacity.Value = 100;
            tbOpacity.TickStyle = TickStyle.None;
            tbOpacity.Size = new Size(110, 30);
            tbOpacity.Location = new Point(660, 12);
            tbOpacity.Scroll += (s, e) => { this.Opacity = tbOpacity.Value / 100.0; };

            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(btnToggleAffinity);
            topPanel.Controls.Add(chkAlwaysOnTop);
            topPanel.Controls.Add(lblOpacity);
            topPanel.Controls.Add(tbOpacity);

            // ==========================================
            // 2. Barra Inferiore Watermark & Autore
            // ==========================================
            bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 26;
            bottomPanel.BackColor = Color.FromArgb(22, 22, 22);
            bottomPanel.Padding = new Padding(10, 4, 10, 4);

            lblFooterStatus = new Label();
            lblFooterStatus.Text = "🔒 Schermo Protetto | Boss Key: [ESC] per nascondere";
            lblFooterStatus.Font = new Font("Segoe UI", 8.5f);
            lblFooterStatus.ForeColor = Color.DarkGray;
            lblFooterStatus.AutoSize = true;
            lblFooterStatus.Location = new Point(8, 5);

            lnkWatermark = new LinkLabel();
            lnkWatermark.Text = "⚡ Ideato & Sviluppato da Alex Ponzin (@Ilponz) - GitHub";
            lnkWatermark.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lnkWatermark.LinkColor = Color.FromArgb(0, 168, 255);
            lnkWatermark.ActiveLinkColor = Color.DeepSkyBlue;
            lnkWatermark.VisitedLinkColor = Color.FromArgb(0, 168, 255);
            lnkWatermark.AutoSize = true;
            lnkWatermark.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lnkWatermark.Location = new Point(bottomPanel.Width - 365, 5);
            lnkWatermark.Cursor = Cursors.Hand;
            lnkWatermark.LinkClicked += (s, e) => {
                try { Process.Start(REPO_URL); } catch { }
            };
            toolTip.SetToolTip(lnkWatermark, "Clicca per visitare il repository ufficiale di Alex Ponzin");

            bottomPanel.Controls.Add(lblFooterStatus);
            bottomPanel.Controls.Add(lnkWatermark);

            // ==========================================
            // 3. Tab Control
            // ==========================================
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 9);

            // --- SCHEDA 1: NOTE PRIVATE ---
            tabNotes = new TabPage("📝 Note Private");
            tabNotes.BackColor = Color.FromArgb(25, 25, 25);

            Panel notesToolbar = new Panel();
            notesToolbar.Dock = DockStyle.Top;
            notesToolbar.Height = 36;
            notesToolbar.BackColor = Color.FromArgb(35, 35, 35);

            Button btnClearNotes = new Button();
            btnClearNotes.Text = "Pulisci Note";
            btnClearNotes.FlatStyle = FlatStyle.Flat;
            btnClearNotes.ForeColor = Color.White;
            btnClearNotes.BackColor = Color.FromArgb(60, 60, 60);
            btnClearNotes.Size = new Size(100, 26);
            btnClearNotes.Location = new Point(10, 5);
            btnClearNotes.Click += (s, e) => { txtNotes.Clear(); };

            Button btnSaveNotes = new Button();
            btnSaveNotes.Text = "Salva su File";
            btnSaveNotes.FlatStyle = FlatStyle.Flat;
            btnSaveNotes.ForeColor = Color.White;
            btnSaveNotes.BackColor = Color.FromArgb(60, 60, 60);
            btnSaveNotes.Size = new Size(100, 26);
            btnSaveNotes.Location = new Point(120, 5);
            btnSaveNotes.Click += (s, e) => {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Text Files (*.txt)|*.txt";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(sfd.FileName, txtNotes.Text);
                        MessageBox.Show("Note salvate con successo!", "PrivateViewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };

            notesToolbar.Controls.Add(btnClearNotes);
            notesToolbar.Controls.Add(btnSaveNotes);

            txtNotes = new TextBox();
            txtNotes.Multiline = true;
            txtNotes.ScrollBars = ScrollBars.Both;
            txtNotes.Dock = DockStyle.Fill;
            txtNotes.BackColor = Color.FromArgb(20, 20, 20);
            txtNotes.ForeColor = Color.FromArgb(220, 220, 220);
            txtNotes.Font = new Font("Consolas", 10);
            txtNotes.BorderStyle = BorderStyle.None;
            txtNotes.Text = "// =============================================================================\r\n" +
                            "// PrivateViewer (Chromium Edition)\r\n" +
                            "// Ideato e creato da: Alex Ponzin (@Ilponz)\r\n" +
                            "// Repository Ufficiale: https://github.com/Ilponz/PrivateViewer\r\n" +
                            "// =============================================================================\r\n" +
                            "// Questa finestra e tutto il suo contenuto sono invisibili a screenshot e stream.\r\n\r\n";

            tabNotes.Controls.Add(txtNotes);
            tabNotes.Controls.Add(notesToolbar);

            // --- SCHEDA 2: BROWSER PRIVATO (Chromium WebView2) ---
            tabBrowser = new TabPage("🌐 Browser Privato");
            tabBrowser.BackColor = Color.FromArgb(25, 25, 25);

            // Barra di Navigazione Avanzata
            browserBar = new Panel();
            browserBar.Dock = DockStyle.Top;
            browserBar.Height = 40;
            browserBar.BackColor = Color.FromArgb(35, 35, 35);
            browserBar.Padding = new Padding(6, 6, 6, 6);

            btnBack = CreateNavButton("◀", 8, 6, 32, 28, "Indietro (Alt+Freccia Sinistra)");
            btnBack.Enabled = false;
            btnBack.Click += (s, e) => { if (webView != null && webView.CanGoBack) webView.GoBack(); };

            btnForward = CreateNavButton("▶", 44, 6, 32, 28, "Avanti (Alt+Freccia Destra)");
            btnForward.Enabled = false;
            btnForward.Click += (s, e) => { if (webView != null && webView.CanGoForward) webView.GoForward(); };

            btnReload = CreateNavButton("🔄", 80, 6, 32, 28, "Ricarica (F5)");
            btnReload.Click += (s, e) => {
                if (webView != null && webView.CoreWebView2 != null)
                {
                    if (isLoading)
                        webView.Stop();
                    else
                        webView.Reload();
                }
            };

            btnHome = CreateNavButton("🏠", 116, 6, 32, 28, "Pagina iniziale (Google)");
            btnHome.Click += (s, e) => { NavigateTo("https://www.google.com"); };

            txtUrl = new TextBox();
            txtUrl.Text = "https://www.google.com";
            txtUrl.Font = new Font("Segoe UI", 9.5f);
            txtUrl.Location = new Point(154, 7);
            txtUrl.Width = browserBar.Width - 224;
            txtUrl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            txtUrl.BackColor = Color.FromArgb(24, 24, 24);
            txtUrl.ForeColor = Color.White;
            txtUrl.BorderStyle = BorderStyle.FixedSingle;
            txtUrl.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    NavigateUrl();
                    e.SuppressKeyPress = true;
                }
            };
            txtUrl.Click += (s, e) => { txtUrl.SelectAll(); };

            btnGo = new Button();
            btnGo.Text = "➔";
            btnGo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnGo.FlatStyle = FlatStyle.Flat;
            btnGo.FlatAppearance.BorderSize = 0;
            btnGo.ForeColor = Color.White;
            btnGo.BackColor = Color.FromArgb(0, 122, 204);
            btnGo.Size = new Size(54, 28);
            btnGo.Location = new Point(browserBar.Width - 62, 6);
            btnGo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGo.Cursor = Cursors.Hand;
            btnGo.Click += (s, e) => { NavigateUrl(); };
            toolTip.SetToolTip(btnGo, "Naviga all'indirizzo o cerca su Google (Invio)");

            browserBar.Controls.Add(btnBack);
            browserBar.Controls.Add(btnForward);
            browserBar.Controls.Add(btnReload);
            browserBar.Controls.Add(btnHome);
            browserBar.Controls.Add(txtUrl);
            browserBar.Controls.Add(btnGo);

            // WebView2 Control
            webView = new WebView2();
            webView.Dock = DockStyle.Fill;
            webView.DefaultBackgroundColor = Color.FromArgb(25, 25, 25);

            tabBrowser.Controls.Add(webView);
            tabBrowser.Controls.Add(browserBar);

            tabControl.TabPages.Add(tabNotes);
            tabControl.TabPages.Add(tabBrowser);

            this.Controls.Add(tabControl);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topPanel);

            // Icona di notifica nella System Tray per "Boss Key"
            trayIcon = new NotifyIcon();
            trayIcon.Text = "PrivateViewer - Alex Ponzin (Clicca per ripristinare)";
            trayIcon.Icon = SystemIcons.Shield;
            trayIcon.Visible = false;
            trayIcon.Click += (s, e) => {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.Activate();
                trayIcon.Visible = false;
            };

            // Inizializzazione Display Affinity e WebView2 al caricamento
            this.HandleCreated += (s, e) => {
                ApplyProtection(true);
            };

            this.Load += (s, e) => {
                InitializeWebView();
            };
        }

        private Button CreateNavButton(string text, int x, int y, int width, int height, string tip)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.FromArgb(50, 50, 50);
            btn.ForeColor = Color.White;
            btn.Size = new Size(width, height);
            btn.Location = new Point(x, y);
            btn.Cursor = Cursors.Hand;
            toolTip.SetToolTip(btn, tip);
            return btn;
        }

        private async void InitializeWebView()
        {
            try
            {
                string dataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PrivateViewer", "WebView2Profile"
                );
                Directory.CreateDirectory(dataFolder);

                var env = await CoreWebView2Environment.CreateAsync(null, dataFolder, null);
                await webView.EnsureCoreWebView2Async(env);

                // Configura eventi del browser Chromium
                webView.NavigationStarting += WebView_NavigationStarting;
                webView.NavigationCompleted += WebView_NavigationCompleted;
                webView.SourceChanged += WebView_SourceChanged;

                if (webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
                }

                NavigateTo("https://www.google.com");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Impossibile inizializzare Microsoft Edge WebView2.\r\n" +
                    "Dettaglio: " + ex.Message + "\r\n\r\n" +
                    "Verifica che il runtime Microsoft Edge WebView2 sia installato nel sistema.",
                    "Errore WebView2", MessageBoxButtons.OK, MessageBoxIcon.Error
                );
            }
        }

        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            isLoading = true;
            btnReload.Text = "✕";
            toolTip.SetToolTip(btnReload, "Interrompi caricamento (Esc)");
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            isLoading = false;
            btnReload.Text = "🔄";
            toolTip.SetToolTip(btnReload, "Ricarica (F5)");
            UpdateNavButtons();
        }

        private void WebView_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            if (webView.Source != null && !txtUrl.Focused)
            {
                txtUrl.Text = webView.Source.ToString();
            }
            UpdateNavButtons();
        }

        private void CoreWebView2_DocumentTitleChanged(object sender, object e)
        {
            if (webView.CoreWebView2 != null)
            {
                string title = webView.CoreWebView2.DocumentTitle;
                if (!string.IsNullOrWhiteSpace(title))
                {
                    if (title.Length > 25)
                        title = title.Substring(0, 22) + "...";
                    tabBrowser.Text = "🌐 " + title;
                }
                else
                {
                    tabBrowser.Text = "🌐 Browser Privato";
                }
            }
        }

        private void UpdateNavButtons()
        {
            btnBack.Enabled = webView.CanGoBack;
            btnBack.ForeColor = btnBack.Enabled ? Color.White : Color.Gray;
            btnForward.Enabled = webView.CanGoForward;
            btnForward.ForeColor = btnForward.Enabled ? Color.White : Color.Gray;
        }

        private void NavigateUrl()
        {
            string query = txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(query)) return;

            // URL completo o schema
            if (query.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                query.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                query.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                NavigateTo(query);
            }
            // Dominio web senza protocollo (es. wikipedia.org o localhost:3000)
            else if (!query.Contains(" ") && (query.Contains(".") || query.StartsWith("localhost", StringComparison.OrdinalIgnoreCase)))
            {
                NavigateTo("https://" + query);
            }
            // Ricerca web su Google
            else
            {
                NavigateTo("https://www.google.com/search?q=" + Uri.EscapeDataString(query));
            }
        }

        private void NavigateTo(string url)
        {
            if (webView != null && webView.CoreWebView2 != null)
            {
                try
                {
                    webView.CoreWebView2.Navigate(url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Errore di navigazione: " + ex.Message, "Browser Privato", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ApplyProtection(bool enable)
        {
            uint affinity = enable ? WDA_EXCLUDEFROMCAPTURE : WDA_NONE;
            bool ok = SetWindowDisplayAffinity(this.Handle, affinity);
            if (ok)
            {
                isProtected = enable;
                if (enable)
                {
                    btnToggleAffinity.Text = "🛡️ Invisibile a Cattura: ATTIVO";
                    btnToggleAffinity.BackColor = Color.FromArgb(34, 139, 34);
                }
                else
                {
                    btnToggleAffinity.Text = "⚠️ Visibile a Cattura (DISATTIVO)";
                    btnToggleAffinity.BackColor = Color.FromArgb(160, 60, 60);
                }
            }
            else
            {
                int err = Marshal.GetLastWin32Error();
                MessageBox.Show("Impossibile impostare Display Affinity. Codice errore: " + err, "Errore API", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnToggleAffinity_Click(object sender, EventArgs e)
        {
            ApplyProtection(!isProtected);
        }

        private void HideToTray()
        {
            this.Hide();
            trayIcon.Visible = true;
        }

        // Intercettazione messaggi Windows a livello globale per il form e i controlli
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_KEYDOWN)
            {
                Keys key = (Keys)(int)m.WParam & Keys.KeyCode;
                if (key == Keys.Escape)
                {
                    HideToTray();
                    return true;
                }
                else if (key == Keys.F5 && tabControl.SelectedTab == tabBrowser)
                {
                    if (webView != null && webView.CoreWebView2 != null)
                    {
                        webView.Reload();
                        return true;
                    }
                }
            }
            return false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                HideToTray();
                return true;
            }
            else if (keyData == Keys.F5 && tabControl.SelectedTab == tabBrowser)
            {
                if (webView != null && webView.CoreWebView2 != null)
                {
                    webView.Reload();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Application.RemoveMessageFilter(this);
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
            if (webView != null)
            {
                webView.Dispose();
            }
            base.OnFormClosing(e);
        }
    }
}
