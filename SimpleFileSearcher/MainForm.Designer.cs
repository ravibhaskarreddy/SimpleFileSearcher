using System.Drawing;
using System.Windows.Forms;

namespace SimpleFileSearcher
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // Folder Selection
        private TextBox txtFolderPath;
        private Button btnBrowse;
        private FolderBrowserDialog fbd;

        // Environment → Server → API Selection
        private ComboBox cmbEnvironment;
        private CheckedListBox clbServers;
        private CheckedListBox clbApis;
        private Button btnSelectAllServers;
        private Button btnSelectAllApis;

        // Search Filters
        private TextBox txtSearchText;
        private TextBox txtFileTypes;
        private DateTimePicker dtFrom;
        private DateTimePicker dtTo;
        private Button btnSearch;
        private Button btnCancel;
        private CheckBox chkDarkMode;

        // Search Results
        private ListView lvResults;
        private TextBox txtPreview;
        private Label lblStatus;

        // Context Menu for ListView
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem menuCopyPath;
        private ToolStripMenuItem menuCopyName;
        private ToolStripMenuItem menuOpenFile;
        private ToolStripMenuItem menuOpenFolder;

        // Bottom Action Buttons
        private Button btnExportAll;
        private Button btnClearSelection;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Initialize UI components and layout
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // Controls Initialization
            txtFolderPath = new TextBox();
            btnBrowse = new Button();
            fbd = new FolderBrowserDialog();

            cmbEnvironment = new ComboBox();
            clbServers = new CheckedListBox();
            clbApis = new CheckedListBox();
            btnSelectAllServers = new Button();
            btnSelectAllApis = new Button();

            txtSearchText = new TextBox();
            txtFileTypes = new TextBox();
            dtFrom = new DateTimePicker();
            dtTo = new DateTimePicker();
            btnSearch = new Button();
            btnCancel = new Button();
            chkDarkMode = new CheckBox();

            lvResults = new ListView();
            txtPreview = new TextBox();
            lblStatus = new Label();

            btnExportAll = new Button();
            btnClearSelection = new Button();

            contextMenu = new ContextMenuStrip();
            menuCopyPath = new ToolStripMenuItem("Copy Full Path");
            menuCopyName = new ToolStripMenuItem("Copy File Name");
            menuOpenFile = new ToolStripMenuItem("Open File");
            menuOpenFolder = new ToolStripMenuItem("Open Folder");

            // Add menu items to context menu
            contextMenu.Items.AddRange(new ToolStripItem[]
            {
                menuCopyPath, menuCopyName, menuOpenFile, menuOpenFolder
            });

            // Folder Selection
            txtFolderPath.SetBounds(10, 10, 600, 25);
            txtFolderPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            btnBrowse.SetBounds(620, 10, 100, 25);
            btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Text = "Browse...";

            // Environment / Server / API Controls
            cmbEnvironment.SetBounds(10, 45, 120, 25);
            cmbEnvironment.Items.AddRange(new string[] { "SIT", "UAT", "STG", "PROD" });

            clbServers.SetBounds(140, 45, 240, 80);
            clbApis.SetBounds(390, 45, 240, 80);

            btnSelectAllServers.SetBounds(140, 130, 120, 25);
            //btnSelectAllServers.SetBounds(200, 130, 120, 25);-middle
            btnSelectAllServers.Text = "Select All Servers";

            btnSelectAllApis.SetBounds(390, 130, 120, 25);
            //btnSelectAllApis.SetBounds(450, 130, 120, 25);-middle
            btnSelectAllApis.Text = "Select All APIs";

            // Search Inputs
            txtSearchText.SetBounds(10, 165, 370, 25);

            txtFileTypes.SetBounds(390, 165, 180, 25);
            // txtFileTypes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFileTypes.Text = "*.txt;*.log;*.json";

            dtFrom.SetBounds(10, 195, 150, 25);
            dtTo.SetBounds(230, 195, 150, 25);

            btnSearch.SetBounds(390, 195, 80, 25);
            btnSearch.Text = "Search";

            btnCancel.SetBounds(490, 195, 80, 25);
            btnCancel.Text = "Cancel";

            chkDarkMode.SetBounds(620, 195, 100, 25);
            chkDarkMode.Text = "Dark Mode";

            // Results View
            lvResults.SetBounds(10, 230, 760, 200);
            lvResults.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lvResults.View = View.Details;
            lvResults.Columns.Add("File Path", 500);
            lvResults.Columns.Add("Matches", 200);
            lvResults.FullRowSelect = true;
            lvResults.GridLines = true;
            lvResults.ContextMenuStrip = contextMenu;

            // Preview Box
            txtPreview.SetBounds(10, 440, 760, 120);
            txtPreview.Multiline = true;
            txtPreview.ScrollBars = ScrollBars.Both;
            txtPreview.ReadOnly = true;
            txtPreview.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Status Label
            lblStatus.SetBounds(10, 600, 300, 25);
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // Action Buttons
            btnExportAll.Size = new Size(100, 25);
            btnExportAll.Text = "Export All";
            btnExportAll.Location = new Point(520, txtPreview.Bottom + 35);
            btnExportAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            btnClearSelection.Size = new Size(120, 25);
            btnClearSelection.Text = "Clear Selection";
            btnClearSelection.Location = new Point(630, txtPreview.Bottom + 35);
            btnClearSelection.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            // Form properties
            this.Text = "Ravi.Reddy's File Searcher with Env/Server/API";
            this.ClientSize = new Size(784, 680);

            // Add all controls to form
            this.Controls.AddRange(new Control[]
            {
                txtFolderPath, btnBrowse,
                cmbEnvironment, clbServers, clbApis,
                btnSelectAllServers, btnSelectAllApis,
                txtSearchText, txtFileTypes, dtFrom, dtTo,
                btnSearch, btnCancel, chkDarkMode,
                lvResults, txtPreview, lblStatus,
                btnExportAll, btnClearSelection
            });
        }
    }
}
 