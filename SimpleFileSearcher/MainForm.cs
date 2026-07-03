using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleFileSearcher
{
    public partial class MainForm : Form
    {
        private bool cancelRequested = false;

        // Environment → Server path mapping
        private readonly Dictionary<string, List<string>> envToServers = new Dictionary<string, List<string>>
        {
            ["SIT"] = new List<string>
            {
                @"\\nau2s-wappkcztr\Marketplacelogs\API\",
                @"\\nau2s-wapp5guus\\Marketplacelogs\\API\\",
                @"\\nau2s-wappcadis\\Marketplacelogs\\API\\",
                @"\\nau2s-wappre9dq\\Marketplacelogs\\API\\"
            },
            ["UAT"] = new List<string>
            {
                @"\\nau2u-wapp4rvku\\MarketplaceLogs\\API\\",
                @"\\nau2u-wapp4stvk\\MarketplaceLogs\\API\\",
                @"\\nau2u-wapp4vuxp\\MarketplaceLogs\\API\\",
                @"\\nau2u-wapp4rnld\\MarketplaceLogs\\API\\"
            },
            ["STG"] = new List<string>
            {
                @"\\nau2r-wapp9wfzb\\Marketplacelogs\\API\\",
                @"\\nau2r-wapp9senk\\API\\",
                @"\\nau2r-wapp9ktrz\\API\\"
            },
            ["PROD"] = new List<string>
            {
                @"\\nau2p-wappbrstu\\MarketplaceLogs\\API\\",
                @"\\nau2p-wappbstrg\\MarketplaceLogs\\API\\",
                @"\\nau2p-wappbgulp\\MarketplaceLogs\\API\\",
                @"\\nau2p-wappbywre\\MarketplaceLogs\\API\\"


            }
        };

        public MainForm()
        {
            InitializeComponent();
            HookUIEvents();
        }

        /// <summary>
        /// Hook all UI control event handlers.
        /// </summary>
        private void HookUIEvents()
        {
            cmbEnvironment.SelectedIndexChanged += (s, e) => LoadServers();
            clbServers.ItemCheck += (s, e) => this.BeginInvoke((Action)LoadApis);

            btnSelectAllServers.Click += (s, e) => ToggleAll(clbServers);
            btnSelectAllApis.Click += (s, e) => ToggleAll(clbApis);
            btnClearSelection.Click += (s, e) => ClearSelection();
            chkDarkMode.CheckedChanged += (s, e) => ApplyDarkTheme(chkDarkMode.Checked);
            btnExportAll.Click += (s, e) => ExportAllPreviewText();

            btnBrowse.Click += (s, e) =>
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtFolderPath.Text = fbd.SelectedPath;
            };

            btnCancel.Click += (s, e) =>
            {
                cancelRequested = true;
                lblStatus.Text = "Cancelling...";
            };

            btnSearch.Click += async (s, e) => await ExecuteSearchAsync();

            lvResults.SelectedIndexChanged += (s, e) =>
            {
                if (lvResults.SelectedItems.Count > 0)
                    txtPreview.Text = lvResults.SelectedItems[0].Tag as string ?? string.Empty;
            };

            menuCopyPath.Click += (s, e) => CopySelectedPath(true);
            menuCopyName.Click += (s, e) => CopySelectedPath(false);
            menuOpenFile.Click += (s, e) => OpenFile();
            menuOpenFolder.Click += (s, e) => OpenFolder();
        }

        /// <summary>
        /// Populate server paths for the selected environment.
        /// </summary>
        private void LoadServers()
        {
            clbServers.Items.Clear();
            clbApis.Items.Clear();

            if (cmbEnvironment.SelectedItem is string env && envToServers.ContainsKey(env))
            {
                foreach (var server in envToServers[env])
                    clbServers.Items.Add(server, false);
            }
        }

        /// <summary>
        /// Load API subdirectories from selected servers.
        /// </summary>
        private void LoadApis()
        {
            clbApis.Items.Clear();

            var selectedServers = clbServers.CheckedItems.Cast<string>().ToList();
            foreach (var server in selectedServers)
            {
                if (!Directory.Exists(server))
                    continue;

                try
                {
                    foreach (var sub in Directory.GetDirectories(server))
                    {
                        string name = Path.GetFileName(sub);
                        if (!clbApis.Items.Contains(name))
                            clbApis.Items.Add(name, false);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error accessing {server}:\n{ex.Message}");
                }
            }
        }

        /// <summary>
        /// Main async search execution with folder resolution and result collection.
        /// </summary>
        private async Task ExecuteSearchAsync()
        {
            lvResults.Items.Clear();
            txtPreview.Clear();
            cancelRequested = false;
            lblStatus.Text = "Searching...";

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Determine search folders
            var finalFolders = new List<string>();
            if (string.IsNullOrWhiteSpace(txtFolderPath.Text))
            {
                foreach (string server in clbServers.CheckedItems)
                {
                    foreach (string api in clbApis.CheckedItems)
                    {
                        finalFolders.Add(Path.Combine(server, api));
                    }
                }
            }
            else
            {
                finalFolders.Add(txtFolderPath.Text);
            }

            string searchText = txtSearchText.Text;
            string[] types = txtFileTypes.Text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            // Inclusive date range
            DateTime from = dtFrom.Value.Date;
            DateTime to = dtTo.Value.Date.AddDays(1).AddSeconds(-1);
            Console.WriteLine($"Filter range → From: {from}, To: {to}");

            var allResults = new List<SearchResult>();

            // Run search per folder
            foreach (var folder in finalFolders)
            {
                if (!Directory.Exists(folder))
                    continue;

                var results = await Task.Run(() =>
                    FileSearcher.SearchFiles(folder, searchText, types, from, to, () => cancelRequested));

                allResults.AddRange(results);
            }

            // Display results
            foreach (var res in allResults)
            {
                var item = new ListViewItem(new[] { res.Path, $"Matches: {res.MatchCount}" })
                {
                    Tag = res.Preview
                };
                lvResults.Items.Add(item);
            }

            stopwatch.Stop();

            lblStatus.Text = cancelRequested
                ? "Search cancelled."
                : $"Search complete. {allResults.Count} file(s) matched in {stopwatch.Elapsed.TotalSeconds:F2} seconds.";
        }

        // -- UI Utility Methods --

        private void ToggleAll(CheckedListBox clb)
        {
            for (int i = 0; i < clb.Items.Count; i++)
                clb.SetItemChecked(i, true);
        }

        private void ApplyDarkTheme(bool enabled)
        {
            Color bg = enabled ? Color.FromArgb(45, 45, 48) : SystemColors.Control;
            Color fg = enabled ? Color.White : SystemColors.ControlText;
            Color textboxBg = enabled ? Color.FromArgb(30, 30, 30) : Color.White;
            Color listBg = enabled ? Color.FromArgb(28, 28, 28) : Color.White;

            BackColor = bg;

            foreach (Control c in Controls)
            {
                switch (c)
                {
                    case TextBox _:
                    case DateTimePicker _:
                    case ComboBox _:
                        c.BackColor = textboxBg;
                        c.ForeColor = fg;
                        break;

                    case ListView lv:
                        lv.BackColor = listBg;
                        lv.ForeColor = fg;
                        lv.GridLines = true;
                        break;

                    default:
                        c.BackColor = bg;
                        c.ForeColor = fg;
                        break;
                }
            }
        }

        private void ClearSelection()
        {
            cmbEnvironment.SelectedIndex = -1;
            clbServers.Items.Clear();
            clbApis.Items.Clear();
            txtFolderPath.ReadOnly = false;
            txtFolderPath.Clear();
        }

        private void CopySelectedPath(bool full)
        {
            if (lvResults.SelectedItems.Count == 0)
                return;

            string path = lvResults.SelectedItems[0].SubItems[0].Text;
            Clipboard.SetText(full ? path : Path.GetFileName(path));
        }

        private void OpenFile()
        {
            if (lvResults.SelectedItems.Count == 0)
                return;

            string path = lvResults.SelectedItems[0].SubItems[0].Text;
            if (File.Exists(path))
                System.Diagnostics.Process.Start(path);
            else
                MessageBox.Show("File not found.");
        }

        private void OpenFolder()
        {
            if (lvResults.SelectedItems.Count == 0)
                return;

            string path = lvResults.SelectedItems[0].SubItems[0].Text;
            if (File.Exists(path))
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{path}\"");
            else
                MessageBox.Show("File not found.");
        }

        private void ExportAllPreviewText()
        {
            if (lvResults.Items.Count == 0)
            {
                MessageBox.Show("No results to export.");
                return;
            }

            using (var sfd = new SaveFileDialog
            {
                Filter = "Text Files|*.txt",
                FileName = $"AllSearchPreviews_{DateTime.Now:ddMMyyyy_HHmm}.txt"
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    using (var sw = new StreamWriter(sfd.FileName))
                    {
                        foreach (ListViewItem item in lvResults.Items)
                        {
                            string path = item.SubItems[0].Text;
                            string preview = item.Tag?.ToString() ?? "";
                            sw.WriteLine($"--- {path} ---{Environment.NewLine}{preview}{Environment.NewLine}");
                        }
                    }

                    MessageBox.Show("Export successful.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export failed: " + ex.Message);
                }
            }
        }
    }
}
 