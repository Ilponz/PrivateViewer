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
[assembly: AssemblyTitle("PrivateViewer (Chromium Multi-Tab & Responsive Edition)")]
[assembly: AssemblyDescription("Created by Alex Ponzin (@Ilponz) - Anti-Capture Multi-Tab Protected Workspace")]
[assembly: AssemblyCompany("Alex Ponzin (Ilponz)")]
[assembly: AssemblyProduct("PrivateViewer")]
[assembly: AssemblyCopyright("Copyright © 2026 Alex Ponzin (@Ilponz). All rights reserved.")]
[assembly: AssemblyTrademark("Original Software by Alex Ponzin")]
[assembly: AssemblyVersion("2.2.0.0")]
[assembly: AssemblyFileVersion("2.2.0.0")]

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

        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowDisplayAffinity(IntPtr hWnd, out uint pdwAffinity);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        // Core WebView2 Shared Environment
        private static CoreWebView2Environment sharedEnv;

        // Top Control Panels (Responsive)
        private Panel topContainer;
        private Panel topPanel;
        private FlowLayoutPanel flowTopLeft;
        private Label lblTitle;
        private Button btnToggleAffinity;
        private CheckBox chkAlwaysOnTop;
        private CheckBox chkHideTaskbar;
        private Panel pnlOpacity;
        private TrackBar tbOpacity;
        private Label lblOpacity;

        // Quick Tools Buttons (Responsive Flow)
        private FlowLayoutPanel quickBar;
        private Button btnNewTab;
        private Button btnOpenFile;
        private Button btnChatGPT;
        private Button btnGemini;
        private Button btnWhatsApp;
        private Button btnWebApps;
        private ContextMenuStrip menuWebApps;

        // Bottom Watermark Panel (Indelebile & Responsive)
        private Panel bottomPanel;
        private Label lblFooterStatus;
        private LinkLabel lnkWatermark;

        // Tabs
        private TabControl tabControl;
        private TabPage tabNotes;
        private TextBox txtNotes;
        private int tabCounter = 1;

        // Tooltip & Tray
        private ToolTip toolTip;
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
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
            this.Text = "PrivateViewer - by Alex Ponzin (@Ilponz)";
            this.Size = new Size(1000, 680);
            this.MinimumSize = new Size(540, 360);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.KeyPreview = true;
            this.AllowDrop = true;

            // Nascondi dalla barra delle applicazioni Windows (Stealth Mode)
            this.ShowInTaskbar = false;

            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 4000;
            toolTip.InitialDelay = 400;

            // Installa message filter globale per Boss Key (ESC) e Ctrl+T/W
            Application.AddMessageFilter(this);

            // Setup Tray Icon & Context Menu
            SetupTrayIcon();

            // ==========================================
            // CONTENITORE SUPERIORE (Top Container)
            // ==========================================
            topContainer = new Panel();
            topContainer.Dock = DockStyle.Top;
            topContainer.AutoSize = true;
            topContainer.BackColor = Color.FromArgb(35, 35, 35);

            // 1. Barra Impostazioni & Stealth
            topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 44;
            topPanel.BackColor = Color.FromArgb(40, 40, 40);
            topPanel.Padding = new Padding(6, 4, 6, 4);

            pnlOpacity = new Panel();
            pnlOpacity.Dock = DockStyle.Right;
            pnlOpacity.Width = 175;
            pnlOpacity.BackColor = Color.Transparent;

            lblOpacity = new Label();
            lblOpacity.Text = "Opacità:";
            lblOpacity.ForeColor = Color.LightGray;
            lblOpacity.Font = new Font("Segoe UI", 9);
            lblOpacity.AutoSize = true;
            lblOpacity.Location = new Point(4, 12);

            tbOpacity = new TrackBar();
            tbOpacity.Minimum = 30;
            tbOpacity.Maximum = 100;
            tbOpacity.Value = 100;
            tbOpacity.TickStyle = TickStyle.None;
            tbOpacity.Size = new Size(105, 30);
            tbOpacity.Location = new Point(62, 8);
            tbOpacity.Scroll += (s, e) => { this.Opacity = tbOpacity.Value / 100.0; };

            pnlOpacity.Controls.Add(lblOpacity);
            pnlOpacity.Controls.Add(tbOpacity);

            flowTopLeft = new FlowLayoutPanel();
            flowTopLeft.Dock = DockStyle.Fill;
            flowTopLeft.WrapContents = false;
            flowTopLeft.AutoScroll = false;
            flowTopLeft.Padding = new Padding(2);
            flowTopLeft.BackColor = Color.Transparent;

            lblTitle = new Label();
            lblTitle.Text = "🛡️ PrivateViewer";
            lblTitle.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Margin = new Padding(4, 8, 8, 4);

            btnToggleAffinity = new Button();
            btnToggleAffinity.Text = "🛡️ Protetto da Cattura";
            btnToggleAffinity.BackColor = Color.FromArgb(34, 139, 34);
            btnToggleAffinity.ForeColor = Color.White;
            btnToggleAffinity.FlatStyle = FlatStyle.Flat;
            btnToggleAffinity.FlatAppearance.BorderSize = 0;
            btnToggleAffinity.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnToggleAffinity.Size = new Size(170, 28);
            btnToggleAffinity.Margin = new Padding(4, 4, 8, 4);
            btnToggleAffinity.Cursor = Cursors.Hand;
            btnToggleAffinity.Click += BtnToggleAffinity_Click;
            toolTip.SetToolTip(btnToggleAffinity, "Attiva o disattiva la protezione da registrazione e screenshot");

            chkAlwaysOnTop = new CheckBox();
            chkAlwaysOnTop.Text = "Sempre in primo";
            chkAlwaysOnTop.ForeColor = Color.LightGray;
            chkAlwaysOnTop.Font = new Font("Segoe UI", 9);
            chkAlwaysOnTop.AutoSize = true;
            chkAlwaysOnTop.Margin = new Padding(4, 8, 8, 4);
            chkAlwaysOnTop.CheckedChanged += (s, e) => { this.TopMost = chkAlwaysOnTop.Checked; };

            chkHideTaskbar = new CheckBox();
            chkHideTaskbar.Text = "👻 Stealth";
            chkHideTaskbar.ForeColor = Color.YellowGreen;
            chkHideTaskbar.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            chkHideTaskbar.AutoSize = true;
            chkHideTaskbar.Checked = true;
            chkHideTaskbar.Margin = new Padding(4, 8, 8, 4);
            chkHideTaskbar.CheckedChanged += (s, e) => {
                this.ShowInTaskbar = !chkHideTaskbar.Checked;
            };
            toolTip.SetToolTip(chkHideTaskbar, "Invisibile nella barra delle applicazioni di Windows (raggiungibile dalla System Tray)");

            flowTopLeft.Controls.Add(lblTitle);
            flowTopLeft.Controls.Add(btnToggleAffinity);
            flowTopLeft.Controls.Add(chkAlwaysOnTop);
            flowTopLeft.Controls.Add(chkHideTaskbar);

            topPanel.Controls.Add(flowTopLeft);
            topPanel.Controls.Add(pnlOpacity);

            // 2. Barra Rapida: Nuova Scheda, File, Web Apps (Auto-Wrap Flow)
            quickBar = new FlowLayoutPanel();
            quickBar.Dock = DockStyle.Top;
            quickBar.AutoSize = true;
            quickBar.WrapContents = true;
            quickBar.BackColor = Color.FromArgb(28, 28, 28);
            quickBar.Padding = new Padding(6, 4, 6, 4);

            btnNewTab = CreateToolbarButton("➕ Scheda", 95, 28, Color.FromArgb(0, 122, 204), "Apre una nuova scheda protetta (Ctrl + T)");
            btnNewTab.Click += (s, e) => { AddNewBrowserTab("https://www.google.com"); };

            btnOpenFile = CreateToolbarButton("📂 File / PDF", 100, 28, Color.FromArgb(55, 55, 55), "Apri un PDF o file locale in modalità protetta");
            btnOpenFile.Click += BtnOpenFile_Click;

            btnChatGPT = CreateToolbarButton("🤖 ChatGPT", 95, 28, Color.FromArgb(16, 140, 110), "Avvia ChatGPT in una nuova scheda");
            btnChatGPT.Click += (s, e) => { AddNewBrowserTab("https://chatgpt.com"); };

            btnGemini = CreateToolbarButton("🧠 Gemini", 85, 28, Color.FromArgb(70, 70, 130), "Avvia Google Gemini");
            btnGemini.Click += (s, e) => { AddNewBrowserTab("https://gemini.google.com"); };

            btnWhatsApp = CreateToolbarButton("💬 WhatsApp", 95, 28, Color.FromArgb(30, 135, 70), "Avvia WhatsApp Web");
            btnWhatsApp.Click += (s, e) => { AddNewBrowserTab("https://web.whatsapp.com"); };

            btnWebApps = CreateToolbarButton("🚀 Altre App ▾", 105, 28, Color.FromArgb(55, 55, 55), "Telegram, Wikipedia, Calcolatrice, Traduttore, Documenti...");
            btnWebApps.Click += (s, e) => {
                menuWebApps.Show(btnWebApps, new Point(0, btnWebApps.Height));
            };

            SetupWebAppsMenu();

            quickBar.Controls.Add(btnNewTab);
            quickBar.Controls.Add(btnOpenFile);
            quickBar.Controls.Add(btnChatGPT);
            quickBar.Controls.Add(btnGemini);
            quickBar.Controls.Add(btnWhatsApp);
            quickBar.Controls.Add(btnWebApps);

            topContainer.Controls.Add(quickBar);
            topContainer.Controls.Add(topPanel);

            // ==========================================
            // 3. Barra Inferiore Watermark & Autore
            // ==========================================
            bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 24;
            bottomPanel.BackColor = Color.FromArgb(20, 20, 20);
            bottomPanel.Padding = new Padding(6, 2, 6, 2);

            lblFooterStatus = new Label();
            lblFooterStatus.Text = "🔒 Protetto | Trascina PDF o file qui per aprirli";
            lblFooterStatus.Font = new Font("Segoe UI", 8.5f);
            lblFooterStatus.ForeColor = Color.DarkGray;
            lblFooterStatus.Dock = DockStyle.Left;
            lblFooterStatus.AutoSize = true;

            lnkWatermark = new LinkLabel();
            lnkWatermark.Text = "⚡ Ideato da Alex Ponzin (@Ilponz)";
            lnkWatermark.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lnkWatermark.LinkColor = Color.FromArgb(0, 168, 255);
            lnkWatermark.ActiveLinkColor = Color.DeepSkyBlue;
            lnkWatermark.VisitedLinkColor = Color.FromArgb(0, 168, 255);
            lnkWatermark.Dock = DockStyle.Right;
            lnkWatermark.AutoSize = true;
            lnkWatermark.Cursor = Cursors.Hand;
            lnkWatermark.LinkClicked += (s, e) => {
                try { Process.Start(REPO_URL); } catch { }
            };
            toolTip.SetToolTip(lnkWatermark, "Visita il repository ufficiale di Alex Ponzin su GitHub");

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
            notesToolbar.Height = 34;
            notesToolbar.BackColor = Color.FromArgb(35, 35, 35);
            notesToolbar.Padding = new Padding(4);

            Button btnClearNotes = CreateToolbarButton("Pulisci Note", 90, 26, Color.FromArgb(60, 60, 60), "Cancella gli appunti");
            btnClearNotes.Click += (s, e) => { txtNotes.Clear(); };

            Button btnSaveNotes = CreateToolbarButton("Salva su File", 95, 26, Color.FromArgb(60, 60, 60), "Salva le note in un file .txt");
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

            notesToolbar.Controls.Add(btnSaveNotes);
            notesToolbar.Controls.Add(btnClearNotes);

            txtNotes = new TextBox();
            txtNotes.Multiline = true;
            txtNotes.ScrollBars = ScrollBars.Both;
            txtNotes.Dock = DockStyle.Fill;
            txtNotes.BackColor = Color.FromArgb(20, 20, 20);
            txtNotes.ForeColor = Color.FromArgb(220, 220, 220);
            txtNotes.Font = new Font("Consolas", 10);
            txtNotes.BorderStyle = BorderStyle.None;
            txtNotes.Text = "// =============================================================================\r\n" +
                            "// PrivateViewer (Multi-Tab & Responsive Edition)\r\n" +
                            "// Ideato e creato da: Alex Ponzin (@Ilponz)\r\n" +
                            "// Repository: https://github.com/Ilponz/PrivateViewer\r\n" +
                            "// =============================================================================\r\n" +
                            "// Questa finestra e tutto il suo contenuto sono invisibili a screenshot e stream.\r\n" +
                            "// Premi Ctrl+T per una nuova scheda, o usa i tasti in alto per avviare Web Apps.\r\n\r\n";

            tabNotes.Controls.Add(txtNotes);
            tabNotes.Controls.Add(notesToolbar);
            tabControl.TabPages.Add(tabNotes);

            this.Controls.Add(tabControl);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topContainer);

            // Supporto Drag and Drop di file (PDF o immagini)
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

        private void SetupTrayIcon()
        {
            trayMenu = new ContextMenuStrip();
            var itemShow = trayMenu.Items.Add("Mostra PrivateViewer", null, (s, e) => RestoreWindow());
            itemShow.Font = new Font(trayMenu.Font, FontStyle.Bold);
            trayMenu.Items.Add("Nascondi nella Tray (Boss Key)", null, (s, e) => HideToTray());
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Nuova Scheda (Ctrl+T)", null, (s, e) => {
                RestoreWindow();
                AddNewBrowserTab("https://www.google.com");
            });
            trayMenu.Items.Add("Protezione Cattura On/Off", null, (s, e) => ApplyProtection(!isProtected));
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Esci da PrivateViewer", null, (s, e) => {
                trayIcon.Visible = false;
                Application.Exit();
            });

            trayIcon = new NotifyIcon();
            trayIcon.Text = "PrivateViewer - Alex Ponzin (Clicca per mostrare)";
            trayIcon.Icon = SystemIcons.Shield;
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;

            // Click singolo e doppio click ripristinano affidabilmente la finestra
            trayIcon.Click += (s, e) => {
                if (e is MouseEventArgs && ((MouseEventArgs)e).Button == MouseButtons.Left)
                {
                    ToggleWindowFromTray();
                }
            };
            trayIcon.DoubleClick += (s, e) => {
                RestoreWindow();
            };
        }

        private Button CreateToolbarButton(string text, int width, int height, Color backColor, string tip)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = backColor;
            btn.ForeColor = Color.White;
            btn.Size = new Size(width, height);
            btn.Margin = new Padding(3, 2, 3, 2);
            btn.Cursor = Cursors.Hand;
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
        // GESTIONE SCHEDE MULTIPLE BROWSER (RESPONSIVE TABLELAYOUT)
        // ==============================================================================
        public async void AddNewBrowserTab(string initialUrl)
        {
            TabPage tabPage = new TabPage("🌐 Scheda " + tabCounter++);
            tabPage.BackColor = Color.FromArgb(25, 25, 25);

            // Barra di navigazione responsive con TableLayoutPanel a larghezza 100% dinamica
            TableLayoutPanel navTable = new TableLayoutPanel();
            navTable.Dock = DockStyle.Top;
            navTable.Height = 36;
            navTable.BackColor = Color.FromArgb(35, 35, 35);
            navTable.Padding = new Padding(2);
            navTable.ColumnCount = 7;
            navTable.RowCount = 1;

            navTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32)); // Back
            navTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32)); // Forward
            navTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32)); // Reload
            navTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32)); // Home
            navTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // URL Box (100% flessibile)
            navTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42)); // Go
            navTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36)); // Close Tab

            Button btnBack = CreateNavButton("◀", "Indietro (Alt+Freccia Sinistra)");
            btnBack.Enabled = false;

            Button btnForward = CreateNavButton("▶", "Avanti (Alt+Freccia Destra)");
            btnForward.Enabled = false;

            Button btnReload = CreateNavButton("🔄", "Ricarica (F5)");
            Button btnHome = CreateNavButton("🏠", "Pagina iniziale (Google)");

            TextBox txtUrl = new TextBox();
            txtUrl.Text = initialUrl;
            txtUrl.Font = new Font("Segoe UI", 9.5f);
            txtUrl.Dock = DockStyle.Fill;
            txtUrl.BackColor = Color.FromArgb(24, 24, 24);
            txtUrl.ForeColor = Color.White;
            txtUrl.BorderStyle = BorderStyle.FixedSingle;
            txtUrl.Margin = new Padding(4, 5, 4, 3);
            txtUrl.Click += (s, e) => { txtUrl.SelectAll(); };

            Button btnGo = new Button();
            btnGo.Text = "➔";
            btnGo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnGo.FlatStyle = FlatStyle.Flat;
            btnGo.FlatAppearance.BorderSize = 0;
            btnGo.ForeColor = Color.White;
            btnGo.BackColor = Color.FromArgb(0, 122, 204);
            btnGo.Dock = DockStyle.Fill;
            btnGo.Margin = new Padding(2);
            btnGo.Cursor = Cursors.Hand;
            toolTip.SetToolTip(btnGo, "Naviga o cerca su Google (Invio)");

            Button btnCloseTab = new Button();
            btnCloseTab.Text = "✕";
            btnCloseTab.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnCloseTab.FlatStyle = FlatStyle.Flat;
            btnCloseTab.FlatAppearance.BorderSize = 0;
            btnCloseTab.ForeColor = Color.LightGray;
            btnCloseTab.BackColor = Color.FromArgb(65, 30, 30);
            btnCloseTab.Dock = DockStyle.Fill;
            btnCloseTab.Margin = new Padding(2);
            btnCloseTab.Cursor = Cursors.Hand;
            toolTip.SetToolTip(btnCloseTab, "Chiudi questa scheda (Ctrl + W)");

            navTable.Controls.Add(btnBack, 0, 0);
            navTable.Controls.Add(btnForward, 1, 0);
            navTable.Controls.Add(btnReload, 2, 0);
            navTable.Controls.Add(btnHome, 3, 0);
            navTable.Controls.Add(txtUrl, 4, 0);
            navTable.Controls.Add(btnGo, 5, 0);
            navTable.Controls.Add(btnCloseTab, 6, 0);

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
            tabPage.Controls.Add(navTable);
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
                            if (title.Length > 20) title = title.Substring(0, 17) + "...";
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
            if (tab == tabNotes) return;

            tabControl.TabPages.Remove(tab);
            try { webView.Dispose(); } catch { }
            try { tab.Dispose(); } catch { }

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

        private Button CreateNavButton(string text, string tip)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.FromArgb(50, 50, 50);
            btn.ForeColor = Color.White;
            btn.Dock = DockStyle.Fill;
            btn.Margin = new Padding(2);
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
                    btnToggleAffinity.Text = "🛡️ Protetto da Cattura";
                    btnToggleAffinity.BackColor = Color.FromArgb(34, 139, 34);
                }
                else
                {
                    btnToggleAffinity.Text = "⚠️ Visibile (Disattivo)";
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

        // ==============================================================================
        // GESTIONE RIPRISTINO / VISIBILITÀ DA TRAY ICON (AFFIDABILE AL 100%)
        // ==============================================================================
        public void RestoreWindow()
        {
            if (!this.Visible)
            {
                this.Show();
            }
            this.WindowState = FormWindowState.Normal;
            ShowWindow(this.Handle, SW_RESTORE);
            ShowWindow(this.Handle, SW_SHOW);
            SetForegroundWindow(this.Handle);
            this.BringToFront();
            this.Activate();
        }

        private void ToggleWindowFromTray()
        {
            if (!this.Visible || this.WindowState == FormWindowState.Minimized)
            {
                RestoreWindow();
            }
            else
            {
                if (Form.ActiveForm == this)
                {
                    HideToTray();
                }
                else
                {
                    RestoreWindow();
                }
            }
        }

        private void HideToTray()
        {
            this.Hide();
            trayIcon.Visible = true;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Se minimizzato tramite tasto '-', nascondi nella Tray per non rimanere bloccato
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
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
