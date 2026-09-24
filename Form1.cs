using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace App
{
    public partial class Form1 : Form
    {
        private List<TextBox> dynamicTextBoxes = new List<TextBox>();
        private List<Button> dynamicButtons = new List<Button>();
        private const int MaxSnippets = 8;
        private const int MinSnippets = 2;
        private Timer autoSaveTimer;
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;
        private const int MOD_ALT = 0x0001;
        private const int MOD_CONTROL = 0x0002;

        public Form1()
        {
            InitializeComponent();
        }

        private string GetSaveFilePath()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OctoCopy");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, "OctoCopy_Data.json");
        }

        private void SaveData()
        {
            if (autoSaveTimer != null) autoSaveTimer.Stop();
            string path = GetSaveFilePath();
            
            AppSettings settings = new AppSettings();
            settings.Snippets = dynamicTextBoxes.Select(t => t.Text).ToArray();
            
            if (alwaysOnTopToolStripMenuItem != null)
            {
                settings.AlwaysOnTop = alwaysOnTopToolStripMenuItem.Checked;
                settings.MinimizeOnClose = minimizeToTrayOnCloseToolStripMenuItem.Checked;
                settings.MinimizeAfterCopy = minimizeToTrayAfterCopyToolStripMenuItem.Checked;
                settings.OpacityLevel = this.Opacity;
                settings.DarkMode = darkModeToolStripMenuItem.Checked;
                
                if (this.WindowState == FormWindowState.Normal)
                {
                    settings.WindowLocationX = this.Location.X;
                    settings.WindowLocationY = this.Location.Y;
                }
            }
            else
            {
                settings.AlwaysOnTop = true;
                settings.MinimizeOnClose = true;
                settings.MinimizeAfterCopy = false;
            }

            var serializer = new JavaScriptSerializer();
            File.WriteAllText(path, serializer.Serialize(settings));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeTrayIcon();
            
            autoSaveTimer = new Timer();
            autoSaveTimer.Interval = 800;
            autoSaveTimer.Tick += (s, ev) => 
            {
                SaveData();
            };

            List<string> savedData = new List<string>();
            string path = GetSaveFilePath();
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var serializer = new JavaScriptSerializer();
                
                if (json.TrimStart().StartsWith("["))
                {
                    // Old format (array of strings)
                    var data = serializer.Deserialize<string[]>(json);
                    if (data != null) savedData.AddRange(data);
                }
                else
                {
                    // New format
                    var settings = serializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        if (settings.Snippets != null) savedData.AddRange(settings.Snippets);
                        alwaysOnTopToolStripMenuItem.Checked = settings.AlwaysOnTop;
                        minimizeToTrayOnCloseToolStripMenuItem.Checked = settings.MinimizeOnClose;
                        minimizeToTrayAfterCopyToolStripMenuItem.Checked = settings.MinimizeAfterCopy;
                        this.Opacity = settings.OpacityLevel > 0 ? settings.OpacityLevel : 1.0;
                        darkModeToolStripMenuItem.Checked = settings.DarkMode;
                        
                        if (settings.WindowLocationX.HasValue && settings.WindowLocationY.HasValue)
                        {
                            Point savedLoc = new Point(settings.WindowLocationX.Value, settings.WindowLocationY.Value);
                            bool isVisible = false;
                            foreach (Screen screen in Screen.AllScreens)
                            {
                                if (screen.WorkingArea.IntersectsWith(new Rectangle(savedLoc, this.Size)))
                                {
                                    isVisible = true;
                                    break;
                                }
                            }
                            
                            if (isVisible)
                            {
                                this.StartPosition = FormStartPosition.Manual;
                                this.Location = savedLoc;
                            }
                        }
                    }
                }
            }
            
            this.TopMost = alwaysOnTopToolStripMenuItem.Checked;
            
            if (savedData != null && savedData.Count > 0)
            {
                int numRows = Math.Max(MinSnippets, Math.Min(MaxSnippets, savedData.Count));
                
                for (int i = 0; i < numRows; i++)
                {
                    AddSnippetRow();
                    if (i < savedData.Count)
                    {
                        dynamicTextBoxes[i].Text = savedData[i];
                    }
                }
            }
            else
            {
                // Default to 2 empty rows
                AddSnippetRow();
                AddSnippetRow();
            }
            UpdateUI();

            // Adjust form height based on number of rows to take less space on initial open
            AdjustFormHeight();

            // Add resize event to handle textbox widths correctly when scrollbar appears/disappears
            this.snippetPanel.Resize += (s, ev) => 
            {
                foreach (var txt in dynamicTextBoxes)
                {
                    txt.Width = snippetPanel.ClientSize.Width - txt.Left - 12;
                }
            };

            // Register global hotkey: Ctrl + Alt + C (0x43 is 'C')
            RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, 0x43);
            
            this.KeyPreview = true;
            ApplyTheme();
        }
        
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyTheme(); // Ensure DWM dark mode applies exactly when handle is ready
        }

        private void AdjustFormHeight()
        {
            int numRowsActual = dynamicTextBoxes.Count;
            int desiredPanelHeight = numRowsActual * 29 + 10;
            
            // Adjust form's ClientSize based on the desired panel height + non-panel overhead (about 64 pixels now due to menu)
            this.ClientSize = new Size(this.ClientSize.Width, desiredPanelHeight + 64);
        }

        private void InitializeTrayIcon()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Opening += TrayMenu_Opening;

            trayIcon = new NotifyIcon();
            trayIcon.Text = "OctoCopy";
            trayIcon.Icon = this.Icon;
            trayIcon.ContextMenuStrip = trayMenu;
            
            // Show context menu on left click as well (right click is automatic)
            trayIcon.MouseUp += (s, e) => 
            {
                if (e.Button == MouseButtons.Left)
                {
                    typeof(NotifyIcon).GetMethod("ShowContextMenu", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(trayIcon, null);
                }
            };
            
            this.FormClosing += Form1_FormClosing;
        }

        private void RestoreWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            trayIcon.Visible = false;
            this.Activate();
            this.BringToFront();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                SaveData();
                if (minimizeToTrayOnCloseToolStripMenuItem.Checked)
                {
                    e.Cancel = true;
                    this.Hide();
                    trayIcon.Visible = true;
                }
                else
                {
                    trayIcon.Visible = false;
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            base.OnFormClosed(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                RestoreWindow();
            }
            base.WndProc(ref m);
        }

        private bool ctrlHeld = false;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control && !ctrlHeld)
            {
                ctrlHeld = true;
                UpdateCopyButtonTexts();
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (!e.Control && ctrlHeld)
            {
                ctrlHeld = false;
                UpdateCopyButtonTexts();
            }
            base.OnKeyUp(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            if (ctrlHeld)
            {
                ctrlHeld = false;
                UpdateCopyButtonTexts();
            }
            base.OnDeactivate(e);
        }

        private void UpdateCopyButtonTexts()
        {
            string[] shortcutLabels = { "Z", "X", "C", "V", "A", "S", "D", "F" };
            for (int i = 0; i < dynamicButtons.Count; i++)
            {
                Button btn = dynamicButtons[i];
                if (btn.BackColor != Color.LightGreen)
                {
                    btn.Text = (ctrlHeld && i < shortcutLabels.Length) ? shortcutLabels[i] : "Copy";
                }
            }
        }
        
        private void TrayMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            trayMenu.Items.Clear();
            
            for (int i = 0; i < dynamicTextBoxes.Count; i++)
            {
                string text = dynamicTextBoxes[i].Text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    string display = text.Replace("\r", "").Replace("\n", " ");
                    if (display.Length > 40) display = display.Substring(0, 37) + "...";
                    
                    var item = new ToolStripMenuItem(display);
                    string textToCopy = text; // capture in closure
                    item.Click += (s, ev) => Clipboard.SetText(textToCopy);
                    trayMenu.Items.Add(item);
                }
            }
            
            if (trayMenu.Items.Count > 0)
                trayMenu.Items.Add(new ToolStripSeparator());
                
            var openItem = new ToolStripMenuItem("Open OctoCopy");
            openItem.Click += (s, ev) => RestoreWindow();
            trayMenu.Items.Add(openItem);
            
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, ev) => 
            {
                SaveData();
                trayIcon.Visible = false;
                Application.Exit();
            };
            trayMenu.Items.Add(exitItem);
        }

        private void AddSnippetRow()
        {
            if (dynamicTextBoxes.Count >= MaxSnippets) return;

            // Reset AutoScrollPosition before adding controls to ensure correct layout positioning
            Point scrollPos = snippetPanel.AutoScrollPosition;
            snippetPanel.AutoScrollPosition = new Point(0, 0);

            int index = dynamicTextBoxes.Count;
            int yPos = index * 29;

            Button btnCopy = new Button();
            btnCopy.Text = "Copy";
            btnCopy.Size = new Size(57, 23);
            btnCopy.Location = new Point(0, yPos);
            btnCopy.Tag = index;
            btnCopy.Click += DynamicCopy_Click;
            
            TextBox txtSnippet = new TextBox();
            // Width is managed by snippetPanel.Resize event now
            int txtWidth = snippetPanel.ClientSize.Width - 64 - 12;
            txtSnippet.Size = new Size(txtWidth, 20); 
            txtSnippet.Location = new Point(64, yPos + 2); 
            txtSnippet.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            txtSnippet.Tag = index;
            txtSnippet.TextChanged += (s, ev) => 
            {
                if (autoSaveTimer != null)
                {
                    autoSaveTimer.Stop();
                    autoSaveTimer.Start();
                }
            };

            bool dark = darkModeToolStripMenuItem != null && darkModeToolStripMenuItem.Checked;
            if (dark)
            {
                txtSnippet.BackColor = Color.FromArgb(26, 26, 26);
                txtSnippet.ForeColor = Color.FromArgb(241, 241, 241);
                txtSnippet.BorderStyle = BorderStyle.FixedSingle;
                
                btnCopy.BackColor = Color.FromArgb(34, 34, 34);
                btnCopy.ForeColor = Color.FromArgb(241, 241, 241);
                btnCopy.FlatStyle = FlatStyle.Flat;
                btnCopy.FlatAppearance.BorderColor = Color.FromArgb(51, 51, 51);
                btnCopy.FlatAppearance.BorderSize = 1;
            }

            snippetPanel.Controls.Add(btnCopy);
            snippetPanel.Controls.Add(txtSnippet);

            dynamicButtons.Add(btnCopy);
            dynamicTextBoxes.Add(txtSnippet);
            
            // Restore scroll position
            snippetPanel.AutoScrollPosition = new Point(Math.Abs(scrollPos.X), Math.Abs(scrollPos.Y));
            
            UpdateUI();
            AdjustFormHeight();
            if (autoSaveTimer != null) SaveData();
        }

        private void RemoveSnippetRow()
        {
            if (dynamicTextBoxes.Count <= MinSnippets) return;

            int lastIndex = dynamicTextBoxes.Count - 1;
            
            TextBox txtSnippet = dynamicTextBoxes[lastIndex];
            Button btnCopy = dynamicButtons[lastIndex];
            
            snippetPanel.Controls.Remove(txtSnippet);
            snippetPanel.Controls.Remove(btnCopy);
            
            txtSnippet.Dispose();
            btnCopy.Dispose();
            
            dynamicTextBoxes.RemoveAt(lastIndex);
            dynamicButtons.RemoveAt(lastIndex);
            
            UpdateUI();
            AdjustFormHeight();
            if (autoSaveTimer != null) SaveData();
        }

        private void DynamicCopy_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                int index = (int)btn.Tag;
                TextBox txt = dynamicTextBoxes[index];
                if (txt.TextLength != 0)
                {
                    Clipboard.SetText(txt.Text);
                    TriggerVisualFeedback(btn);
                }
                else
                {
                    Clipboard.Clear();
                }
            }
        }

        private async void TriggerVisualFeedback(Button btn)
        {
            if (btn.BackColor == Color.LightGreen) return;

            Color oldColor = btn.BackColor;
            int index = (int)btn.Tag;
            
            btn.Text = "✓";
            btn.BackColor = Color.LightGreen;
            
            await Task.Delay(500);
            
            if (!btn.IsDisposed)
            {
                string[] shortcutLabels = { "Z", "X", "C", "V", "A", "S", "D", "F" };
                btn.Text = (ctrlHeld && index < shortcutLabels.Length) ? shortcutLabels[index] : "Copy";
                btn.BackColor = oldColor;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Hide();
                trayIcon.Visible = true;
                return true;
            }

            if ((keyData & Keys.Control) == Keys.Control)
            {
                Keys keyCode = keyData & Keys.KeyCode;
                int index = -1;
                
                Keys[] shortcuts = { Keys.Z, Keys.X, Keys.C, Keys.V, Keys.A, Keys.S, Keys.D, Keys.F };
                for (int i = 0; i < shortcuts.Length; i++)
                {
                    if (keyCode == shortcuts[i])
                    {
                        index = i;
                        break;
                    }
                }

                if (index >= 0 && index < dynamicTextBoxes.Count)
                {
                    string text = dynamicTextBoxes[index].Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        Clipboard.SetText(text);
                        TriggerVisualFeedback(dynamicButtons[index]);
                        
                        if (minimizeToTrayAfterCopyToolStripMenuItem.Checked)
                        {
                            MinimizeAppAsync();
                        }
                    }
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void MinimizeAppAsync()
        {
            await Task.Delay(1000);
            this.Hide();
            trayIcon.Visible = true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddSnippetRow();
            // Scroll to bottom
            snippetPanel.AutoScrollPosition = new Point(0, snippetPanel.VerticalScroll.Maximum);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            RemoveSnippetRow();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to clear all snippets? This cannot be undone.", "Confirm Clear All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                foreach (var txt in dynamicTextBoxes)
                {
                    txt.Clear();
                }
                SaveData();
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            Form helpForm = new Form();
            helpForm.Text = "OctoCopy Help";
            helpForm.Size = new Size(450, 620);
            helpForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            helpForm.MaximizeBox = false;
            helpForm.MinimizeBox = false;
            helpForm.StartPosition = FormStartPosition.Manual;
            helpForm.Location = new Point(this.Location.X + (this.Width - 450) / 2, this.Location.Y + 75);
            helpForm.ShowInTaskbar = false;
            helpForm.TopMost = this.TopMost; // Fixes the window hiding behind the app!
            
            bool dark = darkModeToolStripMenuItem != null && darkModeToolStripMenuItem.Checked;
            if (dark)
            {
                helpForm.BackColor = Color.FromArgb(18, 18, 18);
                helpForm.ForeColor = Color.FromArgb(241, 241, 241);
            }

            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.FlowDirection = FlowDirection.TopDown;
            panel.WrapContents = false;
            panel.AutoScroll = true;
            panel.Padding = new Padding(15, 15, 25, 15);
            
            Font fontBold = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            Font fontRegular = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            void AddSection(string title, string body)
            {
                Label lblTitle = new Label();
                lblTitle.AutoSize = true;
                lblTitle.Font = fontBold;
                lblTitle.Text = title;
                lblTitle.Margin = new Padding(0, 0, 0, 5);
                
                Label lblBody = new Label();
                lblBody.AutoSize = true;
                lblBody.MaximumSize = new Size(380, 0);
                lblBody.Font = fontRegular;
                lblBody.Text = body;
                lblBody.Margin = new Padding(0, 0, 0, 20);
                
                panel.Controls.Add(lblTitle);
                panel.Controls.Add(lblBody);
            }

            AddSection("Welcome to OctoCopy!", 
                "OctoCopy is a lightning-fast clipboard manager designed to stay out of your way and save you time.");
                
            AddSection("Where did the app go?", 
                "When you click the 'X' to close this window, OctoCopy doesn't actually quit. It hides down in your system tray (near your clock) so it's always ready when you need it.");
                
            AddSection("Quick Copy Shortcuts", 
                "While looking at OctoCopy, just hold down the CTRL key. You'll see letters pop up on the screen. Keep holding CTRL and press a letter (like CTRL + Z) to instantly copy that text.");
                
            AddSection("Summon & Dismiss", 
                "Bring up OctoCopy from anywhere by pressing CTRL + ALT + C. If you just want to glance at your snippets, simply press ESCAPE to instantly hide the window.");
                
            AddSection("Customize your experience", 
                "Click on 'Options' at the top of the main window to tweak things. You can turn on Dark Mode, make the window see-through, or set it to automatically hide itself as soon as you copy something.");
                
            AddSection("Where is the Save button?", 
                "Everything you type is automatically saved in the background. Even if you completely restart your computer, your text will always saved.");

            helpForm.Controls.Add(panel);
            
            if (dark)
            {
                IntPtr h = helpForm.Handle; // Force native handle creation
                SetTitleBarTheme(helpForm, true);
            }
            
            helpForm.ShowDialog(this);
        }

        private void alwaysOnTopToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = alwaysOnTopToolStripMenuItem.Checked;
            SaveData();
        }

        private void settings_CheckedChanged(object sender, EventArgs e)
        {
            SaveData();
        }

        private void darkModeToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            ApplyTheme();
            SaveData();
        }

        private void opacityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form opacityForm = new Form();
            opacityForm.Text = "Opacity";
            opacityForm.Size = new Size(300, 100);
            opacityForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            opacityForm.MaximizeBox = false;
            opacityForm.MinimizeBox = false;
            opacityForm.StartPosition = FormStartPosition.CenterParent;
            opacityForm.ShowInTaskbar = false;
            opacityForm.TopMost = this.TopMost;
            
            Label lbl = new Label();
            lbl.Text = "Adjust Window Transparency:";
            lbl.Location = new Point(10, 10);
            lbl.AutoSize = true;
            
            TrackBar trackBar = new TrackBar();
            trackBar.Location = new Point(10, 30);
            trackBar.Size = new Size(260, 45);
            trackBar.Minimum = 20;
            trackBar.Maximum = 100;
            trackBar.Value = (int)(this.Opacity * 100);
            trackBar.TickFrequency = 10;
            
            trackBar.ValueChanged += (s, ev) => 
            {
                this.Opacity = trackBar.Value / 100.0;
            };
            
            opacityForm.FormClosed += (s, ev) => 
            {
                SaveData();
            };
            
            opacityForm.Controls.Add(lbl);
            opacityForm.Controls.Add(trackBar);
            
            if (darkModeToolStripMenuItem.Checked)
            {
                opacityForm.BackColor = Color.FromArgb(18, 18, 18);
                opacityForm.ForeColor = Color.FromArgb(241, 241, 241);
                
                IntPtr h = opacityForm.Handle; // Force native handle creation
                SetTitleBarTheme(opacityForm, true);
            }

            opacityForm.ShowDialog(this);
        }

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;

        private void SetTitleBarTheme(Form targetForm, bool dark)
        {
            try
            {
                // Try Immersive Dark Mode (Win 10 / 11 fallback)
                int useImmersiveDarkMode = dark ? 1 : 0;
                DwmSetWindowAttribute(targetForm.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int));
                DwmSetWindowAttribute(targetForm.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useImmersiveDarkMode, sizeof(int));

                // Win 11 Specific: Explicitly force the title bar background and text colors
                // Format is 0x00bbggrr (Blue Green Red). -1 means use default system color.
                int captionColor = dark ? 0x00121212 : -1; 
                int textColor = dark ? 0x00FFFFFF : -1; 

                DwmSetWindowAttribute(targetForm.Handle, DWMWA_CAPTION_COLOR, ref captionColor, sizeof(int));
                DwmSetWindowAttribute(targetForm.Handle, DWMWA_TEXT_COLOR, ref textColor, sizeof(int));
                
                // Force window frame to redraw and apply the dark mode attribute immediately
                if (targetForm.IsHandleCreated)
                {
                    SetWindowPos(targetForm.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
                }
            }
            catch
            {
                // Fallback gracefully if DWM API fails or is unsupported
            }
        }

        private void ApplyTheme()
        {
            if (darkModeToolStripMenuItem == null) return;
            
            bool dark = darkModeToolStripMenuItem.Checked;
            SetTitleBarTheme(this, dark);
            
            Color bgForm = dark ? Color.FromArgb(18, 18, 18) : SystemColors.Control;
            Color bgMenu = dark ? Color.FromArgb(26, 26, 26) : SystemColors.Control;
            Color bgTextbox = dark ? Color.FromArgb(26, 26, 26) : SystemColors.Window;
            Color bgButton = dark ? Color.FromArgb(34, 34, 34) : SystemColors.Control;
            Color fg = dark ? Color.FromArgb(241, 241, 241) : SystemColors.ControlText;
            Color fgButton = dark ? Color.FromArgb(241, 241, 241) : SystemColors.ControlText;
            
            this.BackColor = bgForm;
            this.ForeColor = fg;
            snippetPanel.BackColor = bgForm;
            
            menuStrip1.BackColor = bgMenu;
            menuStrip1.ForeColor = fg;

            foreach (var txt in dynamicTextBoxes)
            {
                txt.BackColor = bgTextbox;
                txt.ForeColor = fg;
                txt.BorderStyle = dark ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
            }

            foreach (var btn in dynamicButtons)
            {
                if (btn.BackColor != Color.LightGreen) // don't override flash
                {
                    btn.BackColor = bgButton;
                    btn.ForeColor = fgButton;
                }
                btn.FlatStyle = dark ? FlatStyle.Flat : FlatStyle.Standard;
                if (dark) btn.FlatAppearance.BorderColor = Color.FromArgb(51, 51, 51);
                if (dark) btn.FlatAppearance.BorderSize = 1;
                else btn.FlatAppearance.BorderSize = 0;
            }
            
            btnAdd.BackColor = bgButton;
            btnAdd.ForeColor = fgButton;
            btnAdd.FlatStyle = dark ? FlatStyle.Flat : FlatStyle.Standard;
            if (dark) btnAdd.FlatAppearance.BorderColor = Color.FromArgb(51, 51, 51);
            if (dark) btnAdd.FlatAppearance.BorderSize = 1;
            else btnAdd.FlatAppearance.BorderSize = 0;
            
            btnRemove.BackColor = bgButton;
            btnRemove.ForeColor = fgButton;
            btnRemove.FlatStyle = dark ? FlatStyle.Flat : FlatStyle.Standard;
            if (dark) btnRemove.FlatAppearance.BorderColor = Color.FromArgb(51, 51, 51);
            if (dark) btnRemove.FlatAppearance.BorderSize = 1;
            else btnRemove.FlatAppearance.BorderSize = 0;
            
            button9.BackColor = bgButton;
            button9.ForeColor = fgButton;
            button9.FlatStyle = dark ? FlatStyle.Flat : FlatStyle.Standard;
            if (dark) button9.FlatAppearance.BorderColor = Color.FromArgb(51, 51, 51);
            if (dark) button9.FlatAppearance.BorderSize = 1;
            else button9.FlatAppearance.BorderSize = 0;
        }

        private void UpdateUI()
        {
            btnAdd.Enabled = dynamicTextBoxes.Count < MaxSnippets;
            btnRemove.Enabled = dynamicTextBoxes.Count > MinSnippets;
        }
    }

    public class AppSettings
    {
        public string[] Snippets { get; set; }
        public bool AlwaysOnTop { get; set; }
        public bool MinimizeOnClose { get; set; }
        public bool MinimizeAfterCopy { get; set; }
        public double OpacityLevel { get; set; } = 1.0;
        public bool DarkMode { get; set; } = false;
        public int? WindowLocationX { get; set; }
        public int? WindowLocationY { get; set; }
    }
}
