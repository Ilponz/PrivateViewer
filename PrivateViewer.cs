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
[assembly: AssemblyTitle("PrivateViewer (Chromium Multi-Tab & WebApps Suite)")]
[assembly: AssemblyDescription("Created by Alex Ponzin (@Ilponz) - Anti-Capture Multi-Tab Protected Workspace")]
[assembly: AssemblyCompany("Alex Ponzin (Ilponz)")]
[assembly: AssemblyProduct("PrivateViewer")]
[assembly: AssemblyCopyright("Copyright © 2026 Alex Ponzin (@Ilponz). All rights reserved.")]
[assembly: AssemblyTrademark("Original Software by Alex Ponzin")]
[assembly: AssemblyVersion("2.1.0.0")]
[assembly: AssemblyFileVersion("2.1.0.0")]

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

        // Core WebView2 Shared Environment
        private static CoreWebView2Environment sharedEnv;

        // Top Control Panels
        private Panel topPanel;
        private Panel quickBar;
        private Label lblTitle;
        private Button btnToggleAffinity;
        private CheckBox chkAlwaysOnTop;
        private CheckBox chkHideTaskbar;
        private Panel pnlOpacity;
        private TrackBar tbOpacity;
        private Label lblOpacity;

        // Quick Tools Buttons
        private Button btnNewTab;
        private Button btnOpenFile;
        private Button btnChatGPT;
        private Button btnGemini;
        private Button btnWhatsApp;
        private Button btnWebApps;
        private ContextMenuStrip menuWebApps;
        private Label lblShortcuts;

        // Bottom Watermark Panel (Indelebile)
        private Panel bottomPanel;
        private Label lblFooterStatus;
        private LinkLabel lnkWatermark;

        // Tabs
        private TabControl tabControl;
        private TabPage tabNotes;
        private TextBox txtNotes;
        private int tabCounter = 1;

        // Tooltip & State
        private ToolTip toolTip;
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
            this.Text = "PrivateViewer (Multi-Tab & WebApps Suite) - by Alex Ponzin (@Ilponz)";
            this.Size = new Size(1100, 720);
            this.MinimumSize = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.KeyPreview = true;
            this.AllowDrop = true;

            // Nascondi dalla barra delle applicazioni Windows (Stealth Mode)
            this.ShowInTaskbar = false;

            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 4000;
            toolTip.InitialDelay = 500;

            // Installa message filter globale per garantire che la Boss Key (ESC) e Ctrl+T/W funzionino sempre
            Application.AddMessageFilter(this);

            // ==========================================
            // 1. Barra Superiore: Impostazioni & Stealth
            // ==========================================
            topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 46;
            topPanel.BackColor = Color.FromArgb(40, 40, 40);
            topPanel.Padding = new Padding(10, 6, 10, 6);

            lblTitle = new Label();
            lblTitle.Text = "🛡️ PrivateViewer";
            lblTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(12, 12);

            btnToggleAffinity = new Button();
            btnToggleAffinity.Text = "🛡️ Invisibile a Cattura: ATTIVO";
            btnToggleAffinity.BackColor = Color.FromArgb(34, 139, 34);
            btnToggleAffinity.ForeColor = Color.White;
            btnToggleAffinity.FlatStyle = FlatStyle.Flat;
            btnToggleAffinity.FlatAppearance.BorderSize = 0;
            btnToggleAffinity.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnToggleAffinity.Size = new Size(220, 30);
            btnToggleAffinity.Location = new Point(165, 8);
            btnToggleAffinity.Cursor = Cursors.Hand;
            btnToggleAffinity.Click += BtnToggleAffinity_Click;
            toolTip.SetToolTip(btnToggleAffinity, "Attiva o disattiva la protezione da registrazione e screenshot");

            chkAlwaysOnTop = new CheckBox();
            chkAlwaysOnTop.Text = "Sempre in primo piano";
            chkAlwaysOnTop.ForeColor = Color.LightGray;
            chkAlwaysOnTop.Font = new Font("Segoe UI", 9);
            chkAlwaysOnTop.AutoSize = true;
            chkAlwaysOnTop.Location = new Point(400, 13);
            chkAlwaysOnTop.CheckedChanged += (s, e) => { this.TopMost = chkAlwaysOnTop.Checked; };

            chkHideTaskbar = new CheckBox();
            chkHideTaskbar.Text = "👻 Invisibile in Barra";
            chkHideTaskbar.ForeColor = Color.YellowGreen;
            chkHideTaskbar.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            chkHideTaskbar.AutoSize = true;
            chkHideTaskbar.Checked = true;
            chkHideTaskbar.Location = new Point(570, 13);
            chkHideTaskbar.CheckedChanged += (s, e) => {
                this.ShowInTaskbar = !chkHideTaskbar.Checked;
            };
            toolTip.SetToolTip(chkHideTaskbar, "Se attivo, il programma scompare completamente dalla barra delle applicazioni di Windows");

            // Pannello Opacità ancorato a destra in modo stabile
            pnlOpacity = new Panel();
            pnlOpacity.Dock = DockStyle.Right;
            pnlOpacity.Width = 190;
            pnlOpacity.BackColor = Color.Transparent;

            lblOpacity = new Label();
            lblOpacity.Text = "Opacità:";
            lblOpacity.ForeColor = Color.LightGray;
            lblOpacity.Font = new Font("Segoe UI", 9);
            lblOpacity.AutoSize = true;
            lblOpacity.Location = new Point(10, 13);

            tbOpacity = new TrackBar();
            tbOpacity.Minimum = 30;
            tbOpacity.Maximum = 100;
            tbOpacity.Value = 100;
            tbOpacity.TickStyle = TickStyle.None;
            tbOpacity.Size = new Size(110, 30);
            tbOpacity.Location = new Point(70, 9);
            tbOpacity.Scroll += (s, e) => { this.Opacity = tbOpacity.Value / 100.0; };

            pnlOpacity.Controls.Add(lblOpacity);
            pnlOpacity.Controls.Add(tbOpacity);

            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(btnToggleAffinity);
            topPanel.Controls.Add(chkAlwaysOnTop);
            topPanel.Controls.Add(chkHideTaskbar);
            topPanel.Controls.Add(pnlOpacity);

            // ==========================================
            // 2. Barra Rapida: Nuova Scheda, File, Web Apps
            // ==========================================
            quickBar = new Panel();
            quickBar.Dock = DockStyle.Top;
            quickBar.Height = 40;
            quickBar.BackColor = Color.FromArgb(28, 28, 28);
            quickBar.Padding = new Padding(8, 5, 8, 5);

            btnNewTab = new Button();
            btnNewTab.Text = "➕ Nuova Scheda";
            btnNewTab.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnNewTab.FlatStyle = FlatStyle.Flat;
            btnNewTab.FlatAppearance.BorderSize = 0;
            btnNewTab.BackColor = Color.FromArgb(0, 122, 204);
            btnNewTab.ForeColor = Color.White;
            btnNewTab.Size = new Size(125, 28);
            btnNewTab.Location = new Point(8, 6);
            btnNewTab.Cursor = Cursors.Hand;
            btnNewTab.Click += (s, e) => { AddNewBrowserTab("https://www.google.com"); };
            toolTip.SetToolTip(btnNewTab, "Apre una nuova scheda del browser protetta (Ctrl + T)");

            btnOpenFile = new Button();
            btnOpenFile.Text = "📂 Apri PDF / File";
            btnOpenFile.Font = new Font("Segoe UI", 9);
            btnOpenFile.FlatStyle = FlatStyle.Flat;
            btnOpenFile.FlatAppearance.BorderSize = 0;
            btnOpenFile.BackColor = Color.FromArgb(55, 55, 55);
            btnOpenFile.ForeColor = Color.White;
            btnOpenFile.Size = new Size(120, 28);
            btnOpenFile.Location = new Point(140, 6);
            btnOpenFile.Cursor = Cursors.Hand;
            btnOpenFile.Click += BtnOpenFile_Click;
            toolTip.SetToolTip(btnOpenFile, "Visualizza un documento PDF, pagina HTML o immagine in modalità protetta");

            // Web Apps Dirette a 1 clic
            btnChatGPT = CreateQuickAppButton("🤖 ChatGPT", 268, 6, 105, 28, Color.FromArgb(16, 140, 110), "https://chatgpt.com", "Avvia ChatGPT in una scheda protetta");
            btnGemini = CreateQuickAppButton("🧠 Gemini", 380, 6, 95, 28, Color.FromArgb(70, 70, 130), "https://gemini.google.com", "Avvia Google Gemini in una scheda protetta");
            btnWhatsApp = CreateQuickAppButton("💬 WhatsApp", 482, 6, 110, 28, Color.FromArgb(30, 135, 70), "https://web.whatsapp.com", "Avvia WhatsApp Web in una scheda protetta");

            btnWebApps = new Button();
            btnWebApps.Text = "🚀 Altre App ▾";
            btnWebApps.Font = new Font("Segoe UI", 9);
            btnWebApps.FlatStyle = FlatStyle.Flat;
            btnWebApps.FlatAppearance.BorderSize = 0;
            btnWebApps.BackColor = Color.FromArgb(55, 55, 55);
            btnWebApps.ForeColor = Color.White;
            btnWebApps.Size = new Size(115, 28);
            btnWebApps.Location = new Point(600, 6);
            btnWebApps.Cursor = Cursors.Hand;
            btnWebApps.Click += (s, e) => {
                menuWebApps.Show(btnWebApps, new Point(0, btnWebApps.Height));
            };
            toolTip.SetToolTip(btnWebApps, "Altre Web Apps: Telegram, Wikipedia, Calcolatrice Scientifica, Traduttore, Google Docs...");

            SetupWebAppsMenu();

            Panel pnlShortcuts = new Panel();
            pnlShortcuts.Dock = DockStyle.Right;
            pnlShortcuts.Width = 340;
            pnlShortcuts.BackColor = Color.Transparent;

            lblShortcuts = new Label();
            lblShortcuts.Text = "Scorciatoie: [Ctrl+T] Scheda | [Ctrl+W] Chiudi | [ESC] Boss Key";
            lblShortcuts.Font = new Font("Segoe UI", 8.5f);
            lblShortcuts.ForeColor = Color.DarkGray;
            lblShortcuts.AutoSize = true;
            lblShortcuts.Location = new Point(10, 11);
            pnlShortcuts.Controls.Add(lblShortcuts);

            quickBar.Controls.Add(btnNewTab);
            quickBar.Controls.Add(btnOpenFile);
            quickBar.Controls.Add(btnChatGPT);
            quickBar.Controls.Add(btnGemini);
            quickBar.Controls.Add(btnWhatsApp);
            quickBar.Controls.Add(btnWebApps);
            quickBar.Controls.Add(pnlShortcuts);

            // ==========================================
            // 3. Barra Inferiore Watermark & Autore
            // ==========================================
            bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 26;
            bottomPanel.BackColor = Color.FromArgb(20, 20, 20);
            bottomPanel.Padding = new Padding(10, 4, 10, 4);

            lblFooterStatus = new Label();
            lblFooterStatus.Text = "🔒 Schermo Protetto | Trascina qui qualsiasi PDF o file per aprirlo";
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
            toolTip.SetToolTip(lnkWatermark, "Visita il repository ufficiale di Alex Ponzin");

            bottomPanel.Controls.Add(lblFooterStatus);
            bottomPanel.Controls.Add(lnkWatermark);

            // ==========================================
            // 4. Tab Control (Note & Schede Browser)
            // ==========================================
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 9);

            // --- SCHEDA PREDEFINITA: NOTE PRIVATE ---
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
                            "// PrivateViewer (Multi-Tab & WebApps Suite)\r\n" +
                            "// Ideato e creato da: Alex Ponzin (@Ilponz)\r\n" +
                            "// Repository Ufficiale: https://github.com/Ilponz/PrivateViewer\r\n" +
                            "// =============================================================================\r\n" +
                            "// Questa finestra e tutto il suo contenuto sono invisibili a screenshot e stream.\r\n" +
                            "// Puoi aprire schede multiple con il tasto [+] o premendo Ctrl + T.\r\n\r\n";

            tabNotes.Controls.Add(txtNotes);
            tabNotes.Controls.Add(notesToolbar);
            tabControl.TabPages.Add(tabNotes);

            this.Controls.Add(tabControl);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(quickBar);
            this.Controls.Add(topPanel);

            // Icona di notifica nella System Tray per "Boss Key"
            trayIcon = new NotifyIcon();
            trayIcon.Text = "PrivateViewer - Alex Ponzin (Clicca per mostrare)";
            trayIcon.Icon = SystemIcons.Shield;
            trayIcon.Visible = true;
            trayIcon.Click += (s, e) => {
                if (this.Visible)
                {
                    this.Activate();
                }
                else
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.Activate();
                }
            };

            // Supporto Drag and Drop di file (es. PDF o immagini)
            this.DragEnter += (s, e) => {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
            };
            this.DragDrop += (s, e) => {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    foreach (string file in files)
                    {
                        AddNewBrowserTab(new Uri(file).AbsoluteUri);
                    }
                }
            };

            // Inizializzazione Display Affinity e prima scheda
            this.HandleCreated += (s, e) => {
                ApplyProtection(isProtected);
            };

            this.Load += (s, e) => {
                AddNewBrowserTab("https://www.google.com");
            };
        }

        private Button CreateQuickAppButton(string text, int x, int y, int width, int height, Color backColor, string url, string tip)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = backColor;
            btn.ForeColor = Color.White;
            btn.Size = new Size(width, height);
            btn.Location = new Point(x, y);
            btn.Cursor = Cursors.Hand;
            btn.Click += (s, e) => { AddNewBrowserTab(url); };
            toolTip.SetToolTip(btn, tip);
            return btn;
        }

        private void SetupWebAppsMenu()
        {
            menuWebApps = new ContextMenuStrip();
            menuWebApps.Items.Add("✈️ Telegram Web", null, (s, e) => AddNewBrowserTab("https://web.telegram.org"));
            menuWebApps.Items.Add("📖 Wikipedia", null, (s, e) => AddNewBrowserTab("https://it.wikipedia.org"));
            menuWebApps.Items.Add(new ToolStripSeparator());
            menuWebApps.Items.Add("🧮 Calcolatrice Scientifica", null, (s, e) => AddNewBrowserTab("https://www.desmos.com/scientific"));
            menuWebApps.Items.Add("🌐 Google Traduttore", null, (s, e) => AddNewBrowserTab("https://translate.google.com"));
            menuWebApps.Items.Add("📄 Google Documenti", null, (s, e) => AddNewBrowserTab("https://docs.google.com"));
            menuWebApps.Items.Add("🔍 Google Ricerca", null, (s, e) => AddNewBrowserTab("https://www.google.com"));
        }

        private void BtnOpenFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Seleziona un Documento PDF o File da visualizzare";
                ofd.Filter = "File supportati (*.pdf;*.html;*.txt;*.jpg;*.png)|*.pdf;*.html;*.htm;*.txt;*.jpg;*.png;*.webp;*.svg|PDF (*.pdf)|*.pdf|Tutti i file (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string fileUrl = new Uri(ofd.FileName).AbsoluteUri;
                    AddNewBrowserTab(fileUrl);
                }
            }
        }

        private async Task EnsureSharedEnvAsync()
        {
            if (sharedEnv == null)
            {
                string dataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PrivateViewer", "WebView2Profile"
                );
                Directory.CreateDirectory(dataFolder);
                sharedEnv = await CoreWebView2Environment.CreateAsync(null, dataFolder, null);
            }
        }

        // ==============================================================================
        // GESTIONE SCHEDE MULTIPLE BROWSER
        // ==============================================================================
        public async void AddNewBrowserTab(string initialUrl)
        {
            TabPage tabPage = new TabPage("🌐 Scheda " + tabCounter++);
            tabPage.BackColor = Color.FromArgb(25, 25, 25);

            // Barra di navigazione della scheda
            Panel browserBar = new Panel();
            browserBar.Dock = DockStyle.Top;
            browserBar.Height = 40;
            browserBar.BackColor = Color.FromArgb(35, 35, 35);
            browserBar.Padding = new Padding(6);

            Button btnBack = CreateNavButton("◀", 8, 6, 32, 28, "Indietro (Alt+Freccia Sinistra)");
            btnBack.Enabled = false;

            Button btnForward = CreateNavButton("▶", 44, 6, 32, 28, "Avanti (Alt+Freccia Destra)");
            btnForward.Enabled = false;

            Button btnReload = CreateNavButton("🔄", 80, 6, 32, 28, "Ricarica (F5)");
            Button btnHome = CreateNavButton("🏠", 116, 6, 32, 28, "Pagina iniziale (Google)");

            TextBox txtUrl = new TextBox();
            txtUrl.Text = initialUrl;
            txtUrl.Font = new Font("Segoe UI", 9.5f);
            txtUrl.Location = new Point(154, 7);
            txtUrl.Width = browserBar.Width - 268;
            txtUrl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            txtUrl.BackColor = Color.FromArgb(24, 24, 24);
            txtUrl.ForeColor = Color.White;
            txtUrl.BorderStyle = BorderStyle.FixedSingle;
            txtUrl.Click += (s, e) => { txtUrl.SelectAll(); };

            Button btnGo = new Button();
            btnGo.Text = "➔";
            btnGo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnGo.FlatStyle = FlatStyle.Flat;
            btnGo.FlatAppearance.BorderSize = 0;
            btnGo.ForeColor = Color.White;
            btnGo.BackColor = Color.FromArgb(0, 122, 204);
            btnGo.Size = new Size(46, 28);
            btnGo.Location = new Point(browserBar.Width - 100, 6);
            btnGo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGo.Cursor = Cursors.Hand;
            toolTip.SetToolTip(btnGo, "Naviga o cerca su Google (Invio)");

            Button btnCloseTab = new Button();
            btnCloseTab.Text = "✕";
            btnCloseTab.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnCloseTab.FlatStyle = FlatStyle.Flat;
            btnCloseTab.FlatAppearance.BorderSize = 0;
            btnCloseTab.ForeColor = Color.LightGray;
            btnCloseTab.BackColor = Color.FromArgb(65, 30, 30);
            btnCloseTab.Size = new Size(36, 28);
            btnCloseTab.Location = new Point(browserBar.Width - 46, 6);
            btnCloseTab.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCloseTab.Cursor = Cursors.Hand;
            toolTip.SetToolTip(btnCloseTab, "Chiudi questa scheda (Ctrl + W)");

            browserBar.Controls.Add(btnBack);
            browserBar.Controls.Add(btnForward);
            browserBar.Controls.Add(btnReload);
            browserBar.Controls.Add(btnHome);
            browserBar.Controls.Add(txtUrl);
            browserBar.Controls.Add(btnGo);
            browserBar.Controls.Add(btnCloseTab);

            WebView2 webView = new WebView2();
            webView.Dock = DockStyle.Fill;
            webView.DefaultBackgroundColor = Color.FromArgb(25, 25, 25);

            bool isTabLoading = false;

            Action executeNavigate = () => {
                string q = txtUrl.Text.Trim();
                if (string.IsNullOrEmpty(q)) return;
                if (q.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    q.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                    q.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                {
                    try { webView.CoreWebView2.Navigate(q); } catch { }
                }
                else if (!q.Contains(" ") && (q.Contains(".") || q.StartsWith("localhost", StringComparison.OrdinalIgnoreCase)))
                {
                    try { webView.CoreWebView2.Navigate("https://" + q); } catch { }
                }
                else
                {
                    try { webView.CoreWebView2.Navigate("https://www.google.com/search?q=" + Uri.EscapeDataString(q)); } catch { }
                }
            };

            txtUrl.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    executeNavigate();
                    e.SuppressKeyPress = true;
                }
            };
            btnGo.Click += (s, e) => { executeNavigate(); };

            btnBack.Click += (s, e) => { if (webView.CanGoBack) webView.GoBack(); };
            btnForward.Click += (s, e) => { if (webView.CanGoForward) webView.GoForward(); };
            btnHome.Click += (s, e) => { if (webView.CoreWebView2 != null) webView.CoreWebView2.Navigate("https://www.google.com"); };
            btnReload.Click += (s, e) => {
                if (webView.CoreWebView2 != null)
                {
                    if (isTabLoading) webView.Stop();
                    else webView.Reload();
                }
            };

            btnCloseTab.Click += (s, e) => { CloseBrowserTab(tabPage, webView); };

            tabPage.Controls.Add(webView);
            tabPage.Controls.Add(browserBar);
            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;

            try
            {
                await EnsureSharedEnvAsync();
                await webView.EnsureCoreWebView2Async(sharedEnv);

                webView.NavigationStarting += (s, e) => {
                    isTabLoading = true;
                    btnReload.Text = "✕";
                };

                webView.NavigationCompleted += (s, e) => {
                    isTabLoading = false;
                    btnReload.Text = "🔄";
                    btnBack.Enabled = webView.CanGoBack;
                    btnBack.ForeColor = btnBack.Enabled ? Color.White : Color.Gray;
                    btnForward.Enabled = webView.CanGoForward;
                    btnForward.ForeColor = btnForward.Enabled ? Color.White : Color.Gray;
                };

                webView.SourceChanged += (s, e) => {
                    if (webView.Source != null && !txtUrl.Focused)
                    {
                        txtUrl.Text = webView.Source.ToString();
                    }
                };

                if (webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.DocumentTitleChanged += (s, e) => {
                        string title = webView.CoreWebView2.DocumentTitle;
                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            if (title.Length > 22) title = title.Substring(0, 19) + "...";
                            tabPage.Text = "🌐 " + title;
                        }
                    };
                    webView.CoreWebView2.Navigate(initialUrl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore caricamento WebView2: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseBrowserTab(TabPage tab, WebView2 webView)
        {
            if (tab == tabNotes) return; // Non chiudere la scheda delle note

            tabControl.TabPages.Remove(tab);
            try { webView.Dispose(); } catch { }
            try { tab.Dispose(); } catch { }

            // Se non ci sono più schede aperte oltre a Note, apri una nuova scheda vuota
            if (tabControl.TabPages.Count == 1)
            {
                AddNewBrowserTab("https://www.google.com");
            }
        }

        private void CloseCurrentBrowserTab()
        {
            if (tabControl.SelectedTab != null && tabControl.SelectedTab != tabNotes)
            {
                TabPage current = tabControl.SelectedTab;
                WebView2 wv = null;
                foreach (Control c in current.Controls)
                {
                    if (c is WebView2) { wv = (WebView2)c; break; }
                }
                CloseBrowserTab(current, wv);
            }
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
                MessageBox.Show("Impossibile impostare Display Affinity. Codice: " + err, "Errore API", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        // ==============================================================================
        // GESTIONE SCORCIATOIE DA TASTIERA GLOBALI (Boss Key, Ctrl+T, Ctrl+W, Ctrl+Tab)
        // ==============================================================================
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_KEYDOWN)
            {
                Keys key = (Keys)(int)m.WParam & Keys.KeyCode;

                // Boss Key: ESC
                if (key == Keys.Escape)
                {
                    HideToTray();
                    return true;
                }

                bool ctrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                if (ctrlPressed)
                {
                    if (key == Keys.T)
                    {
                        AddNewBrowserTab("https://www.google.com");
                        return true;
                    }
                    else if (key == Keys.W)
                    {
                        CloseCurrentBrowserTab();
                        return true;
                    }
                    else if (key == Keys.Tab)
                    {
                        if (tabControl.TabCount > 1)
                        {
                            tabControl.SelectedIndex = (tabControl.SelectedIndex + 1) % tabControl.TabCount;
                            return true;
                        }
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
            if (keyData == (Keys.Control | Keys.T))
            {
                AddNewBrowserTab("https://www.google.com");
                return true;
            }
            if (keyData == (Keys.Control | Keys.W))
            {
                CloseCurrentBrowserTab();
                return true;
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
            base.OnFormClosing(e);
        }
    }
}
