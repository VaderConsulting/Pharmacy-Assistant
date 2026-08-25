using Core.FileTransfer;
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PharmacyAssistant
{
    public partial class frmDocuments : Form
    {
        private delegate void TreeviewUpdater(TreeNode Node, string Key, string Text, int ImageIndex, int SelectedImageIndex);

        private MouseButtons _Buttons = System.Windows.Forms.MouseButtons.None;
        private List<Document> _Documents = new List<Document>();

        //public string ConditionName { get; set; }
        //public string Ingredientname { get; set; }

        private Document _SelectedDocument = null;
        private FTPEntry _SelectedFTPEntry = null;
        private TreeNode _SelectedTreeNode = null;
        private TreeviewUpdater UpdateDelegate;

        public frmDocuments()
        {
            InitializeComponent();
        }

        private void AddDocument()
        {
            frmUploadDocument UploadDocumentForm = new frmUploadDocument();

            UploadDocumentForm.DocumentPath = txtPath.Text;

            DialogResult Result = UploadDocumentForm.ShowDialog();

            if (Result == System.Windows.Forms.DialogResult.OK)
            {
                DoStartup();
            }

        }

        private void AddFolder()
        {
            frmCreateFolder CreateFolderForm = new frmCreateFolder();

            CreateFolderForm.ThisFTPEntry = _SelectedFTPEntry;

            DialogResult Result = CreateFolderForm.ShowDialog();

            if (Result == System.Windows.Forms.DialogResult.OK)
            {
                DoStartup();                
            }
        }

        private void addFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddFolder();
        }

        private void AddNodeToTreeview(TreeNode ParentNode, string Key, string Text, int ImageIndex, int SelectedImageIndex)
        {
            if (tvwFolders.InvokeRequired)
            {
                this.Invoke(UpdateDelegate, ParentNode, Key, Text, ImageIndex, SelectedImageIndex);
            }
            else
            {
                //TreeNode RootNode = tvwFolders.Nodes.Add("Documents", "Documents", 0, 0);

                FTPEntry NewFTPEntry = new FTPEntry();
                TreeNode ChildNode = new TreeNode();
                ChildNode.ImageKey = Key;
                ChildNode.Text = Text;
                ChildNode.ImageIndex = ImageIndex;
                ChildNode.SelectedImageIndex = SelectedImageIndex;
                ChildNode.Tag = NewFTPEntry;
                ChildNode.Name = NewFTPEntry.Path + "/" + NewFTPEntry.Filename;
                
                if (ParentNode == null)
                {
                    NewFTPEntry.Path = "";
                    NewFTPEntry.Filename = "";
                    NewFTPEntry.IsFolder = true;

                    TreeNode NewNode = new TreeNode();
                    NewNode.ImageKey = Key;
                    NewNode.Text = Text;
                    NewNode.ImageIndex = ImageIndex;
                    NewNode.SelectedImageIndex = SelectedImageIndex;
                    NewNode.Tag = NewFTPEntry;
                    NewNode.Name = NewFTPEntry.Path + "/" + NewFTPEntry.Filename;

                    tvwFolders.Nodes.Add(NewNode);

                    ParentNode = tvwFolders.Nodes[0];
                }
                else
                {
                    FTPEntry ParentEntry = (FTPEntry)ParentNode.Tag;
                    if (ParentEntry.Filename != "")
                    {
                        NewFTPEntry.Path = ParentEntry.Path + "/" + ParentEntry.Filename;
                    }
                    else
                    {
                        NewFTPEntry.Path = ParentEntry.Path;
                    }
                    
                    NewFTPEntry.Filename = Key;
                    NewFTPEntry.IsFolder = true;

                    TreeNode NewNode = new TreeNode();
                    NewNode.ImageKey = Key;
                    NewNode.Text = Text;
                    NewNode.ImageIndex = ImageIndex;
                    NewNode.SelectedImageIndex = SelectedImageIndex;
                    NewNode.Tag = NewFTPEntry;
                    NewNode.Name = NewFTPEntry.Path + "/" + NewFTPEntry.Filename;

                    if (!ParentNode.Nodes.ContainsKey(NewFTPEntry.Path + "/" + NewFTPEntry.Filename)) ParentNode.Nodes.Add(NewNode);

                    ChildNode = ParentNode.Nodes[ParentNode.Nodes.Count - 1];
                }

                GetFTPFileList(ParentNode, Text, false, PharmacyAssistant.Global.FTPEntrySelection.Folder);

                Cursor.Current = Cursors.AppStarting;
            }

        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddDocument();
        }

        private void btnAddDocument_Click(object sender, EventArgs e)
        {
            AddDocument();
        }

        private void btnAddFolder_Click(object sender, EventArgs e)
        {
            AddFolder();
        }

        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "Search";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDeleteDocument_Click(object sender, EventArgs e)
        {
            DeleteDocument();
        }

        private void btnDeleteFolder_Click(object sender, EventArgs e)
        {
            DeleteFolder();
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            GetDocumentInformation();
        }

        //private void GetDocumentList()
        //{
        //    DataSet Data = null;

        //    lstDocuments.Items.Clear();

        //    string Query = "select id, name from brand";

        //    Data = Core.SQL.Functions.Execute(Query, Global.SqlConnectionString);

        //    foreach (DataRow Row in Data.Tables[0].Rows)
        //    {
        //        ListItem Item = new ListItem((int)Row[0], (string)Row[1]);
        //        if (lstItems.Items.Contains(Item))
        //            lblDuplicates.Visible = true;
        //        lstItems.Items.Add(Item);
        //    }
        //}

        private void btnSearch_Click(object sender, EventArgs e)
        {
            DoSearch(txtSearch.Text);
        }

        private void btnViewDocument_Click(object sender, EventArgs e)
        {
            GetDocument();
        }

        private void DeleteDocument()
        {
            MessageBox.Show("Delete Document feature not implemented at this time");
        }

        private void DeleteFolder()
        {
            // Refresh the folder to ensure we know the latest info before we attempt to delete it
        }

        private void deleteFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteFolder();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteDocument();
        }

        private void DoSearch(string SearchTerm)
        {
            if (txtSearch.Text != "Search")
            {
                RefreshFileList(SearchTerm);
            }
            else
            {
                RefreshFileList("");
            }
        }

        private void DoStartup()
        {
            _SelectedDocument = null;
            _SelectedFTPEntry = null;
            tvwFolders.Nodes.Clear();
            lvwDocuments.Clear();
            btnAddDocument.Enabled = false;
            btnAddFolder.Enabled = false;
            btnDeleteDocument.Enabled = false;
            btnDeleteFolder.Enabled = false;
            
            lvwDocuments.Columns.Add("Filename", 250);
            lvwDocuments.Columns.Add("Name", 150);
            //lvwDocuments.Columns.Add("Path", 100);  // displayed in txtPath, so why add it here??
            lvwDocuments.Columns.Add("Public", 50);
            lvwDocuments.Columns.Add("Keywords", 100);
            
            GetFilelist();

            btnAddDocument.Enabled = Global.Permissions.Contains("Create Document");
        }

        private void frmDocuments_FormClosing(object sender, FormClosingEventArgs e)
        {
            Global.RemoveFormFromList(this);
        }

        private void frmDocuments_Load(object sender, EventArgs e)
        {
            Global.AddFormToList(this);

            gpTitle.Image = PharmacyAssistant.Properties.Resources.supervista_general_book_256;
            this.Icon = Properties.Resources.supervista_general_book;
            gpTitle.GradientStartColor = Global.Theme[5];

            DoStartup();
            
            // When editing the folder names is to be allowed, uncomment the following line
            //tvwFolders.LabelEdit = Global.Permissions.Contains("Write Document Folder");

            this.Show();
            this.Refresh();

        }

        private void GetDocument()
        {
            if (lvwDocuments.SelectedItems.Count > 0)
            {
                Document Doc = new Document();

                Doc.FileName = _SelectedFTPEntry.Filename;
                Doc.Path = _SelectedFTPEntry.Path;

                //Doc.FileName = _SelectedDocument.FileName;
                //Doc.Path = _SelectedDocument.Path;

                lblStatus.Text = "Opening Document";
                Application.DoEvents();

                OpenDocument(Doc);

                lblStatus.Text = "Idle";
            }
        }

        private void GetDocumentInformation()
        {
            frmDocumentInfo DocumentInfo = new frmDocumentInfo();

            DocumentInfo.ThisDocument = _SelectedDocument;
            DocumentInfo.ThisFile = _SelectedFTPEntry;

            DialogResult Result = DocumentInfo.ShowDialog();

            if (Result == System.Windows.Forms.DialogResult.OK)
            {
                _SelectedDocument = DocumentInfo.ThisDocument;
                ListViewItem SelectedListViewDocument = lvwDocuments.SelectedItems[0];

                SelectedListViewDocument.Text = _SelectedDocument.FileName;
                SelectedListViewDocument.SubItems[1].Text = _SelectedDocument.Name;
                if (SelectedListViewDocument.SubItems.Count > 2) 
                {
                    SelectedListViewDocument.SubItems[2].Text = _SelectedDocument.Public.ToString(); 
                }
                else 
                { 
                    SelectedListViewDocument.SubItems.Add(_SelectedDocument.Public.ToString());
                }
                if (SelectedListViewDocument.SubItems.Count > 3)
                {
                    SelectedListViewDocument.SubItems[3].Text = _SelectedDocument.Keywords;
                }
                else
                {
                    SelectedListViewDocument.SubItems.Add(_SelectedDocument.Keywords);
                }
                
            }
        }

        private void GetFilelist()
        {
            lblStatus.Text = "Loading folder list...";

            tvwFolders.Nodes.Clear();
            _Documents.Clear();

            Cursor.Current = Cursors.AppStarting;
            BackgroundWorker Worker = new BackgroundWorker();

            UpdateDelegate = new TreeviewUpdater(AddNodeToTreeview);
            Worker.DoWork += Worker_DoWork;
            Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            Worker.RunWorkerAsync();
        }

        private void GetFTPFileList(TreeNode ParentNode, string Path, bool Recursive, PharmacyAssistant.Global.FTPEntrySelection Selection)
        {
            string PathFromRoot = "";

            tvwFolders.BeginUpdate();

            if (ParentNode != null)
            {
                FTPEntry ParentEntry = (FTPEntry)ParentNode.Tag;
                if (ParentEntry.Filename != "")
                {
                    PathFromRoot = ParentEntry.Path + "/" + ParentEntry.Filename + "/";
                }
                else
                {
                    PathFromRoot = ParentEntry.Path + "/";
                }

            }

            // Get list of folders on FTP server
            List<FTPEntry> Entries = Global.GetFTPDirectoryEntries(true, PathFromRoot + Path, Selection);

            foreach (FTPEntry Entry in Entries)
            {
                TreeNode Node = new TreeNode();

                Node.Text = Entry.Filename;
                Node.Tag = Entry;

                if (Entry.IsFolder)
                {
                    Node.ImageIndex = 1;
                    Node.SelectedImageIndex = 1;
                }
                else
                {
                    Node.ImageIndex = 2;
                    Node.SelectedImageIndex = 2;
                }

                ParentNode.Nodes.Add(Node);

                if (Recursive && Entry.IsFolder && (Selection == Global.FTPEntrySelection.Folder || Selection == Global.FTPEntrySelection.FileAndFolder))
                {
                    GetFTPFileList(Node, Entry.Path + "/" + Entry.Filename, Recursive, Selection);
                }
            }

            tvwFolders.EndUpdate();
        }

        private void lvwDocuments_Leave(object sender, EventArgs e)
        {
            btnViewDocument.Enabled = false;
        }

        private void lvwDocuments_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_SelectedDocument != null) GetDocument();
        }

        private void lvwDocuments_MouseUp(object sender, MouseEventArgs e)
        {
            // This event occurs if you click anywhere in the control
            
            _Buttons = e.Button;

            if (_Buttons == System.Windows.Forms.MouseButtons.Right)
            {
                if (lvwDocuments.SelectedItems.Count > 0)
                {
                    openToolStripMenuItem.Enabled = true;
                    propertiesToolStripMenuItem.Enabled = true;
                }
                else
                {
                    openToolStripMenuItem.Enabled = false;
                    propertiesToolStripMenuItem.Enabled = false;
                }

                cmsRightClickDocumentList.Show(lvwDocuments, e.Location);
            }
        }

        private void lvwDocuments_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "Retrieving details for the selected Document";

            if (lvwDocuments.SelectedItems.Count > 0)
            {
                _SelectedFTPEntry = (FTPEntry)lvwDocuments.SelectedItems[0].Tag;

                txtPath.Text = _SelectedFTPEntry.Path;

                btnViewDocument.Enabled = true;
                btnInfo.Enabled = true;
                btnDeleteDocument.Enabled = Global.Permissions.Contains("Delete Document");

                // Get corresponding Document information
                var SelectedDocument = (from Document d in _Documents
                                        where d.FileName.ToLower() == _SelectedFTPEntry.Filename.ToLower() && d.Path.ToLower() == _SelectedFTPEntry.Path.ToLower()
                                        select d).FirstOrDefault();

                if (SelectedDocument != null)
                {
                    _SelectedDocument = (Document)SelectedDocument;
                }
                else
                {
                    // No corresponding database entry
                    _SelectedDocument = null;
                    lblStatus.Text = "No corresponding database entry for the selected Document";
                }
            }
            else
            {
                _SelectedDocument = null;
                _SelectedFTPEntry = null;

                btnViewDocument.Enabled = false;
                btnInfo.Enabled = false;
                btnDeleteDocument.Enabled = false;
            }

            lblStatus.Text = "Idle";
        }

        private void OpenDocument(Document Doc)
        {
            // Check if the document is already present
            string LocalFolder = Application.UserAppDataPath;
            string Filename = Doc.FileName;
            string LocalFilename = System.IO.Path.Combine(LocalFolder, Filename);
            bool FilePresent = File.Exists(LocalFilename);

            Cursor.Current = Cursors.WaitCursor;

            if (!FilePresent)
            {
                FTP Ftp = new FTP();
                Ftp.UseCompression = false;

                Ftp.RemoteHost = Properties.Settings.Default.FTPHost;
                Ftp.RemoteUsername = Properties.Settings.Default.FTPUsername;
                Ftp.RemotePassword = Properties.Settings.Default.FTPPassword;

                try
                {
                    Ftp.Login();
                    Ftp.Download(Doc.Path + "/" + Filename, LocalFilename);

                }
                catch (Exception ex)
                {
                    Global.Common.Logging.WriteErrorEvent(String.Format("Linked Documents form (OpenDocument) - {0}.\nThe message is: {1}", ex.StackTrace, ex.Message));
                }
            }

            // Downloaded or not, we can now open it
            Global.OpenDocument(LocalFilename);

            Cursor.Current = Cursors.Default;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetDocument();
        }

        private void PerformNHSConditionSearch(string Term)
        {
            ProcessStartInfo ProcessInfo = new ProcessStartInfo();

            ProcessInfo.FileName = "http://www.nhs.uk/medicine-guides/pages/MedicineForCondition.aspx?condition=" + Term;
            ProcessInfo.UseShellExecute = true;

            System.Diagnostics.Process.Start(ProcessInfo);
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetDocumentInformation();
        }

        private void radViewDetails_CheckedChanged(object sender, EventArgs e)
        {
            lvwDocuments.View = View.Details;
        }

        private void radViewLarge_CheckedChanged(object sender, EventArgs e)
        {
            lvwDocuments.View = View.LargeIcon;
        }

        private void radViewList_CheckedChanged(object sender, EventArgs e)
        {
            lvwDocuments.View = View.List;
        }

        private void radViewSmall_CheckedChanged(object sender, EventArgs e)
        {
            lvwDocuments.View = View.SmallIcon;
        }

        private void RefreshFileList(string SearchTerm)
        {
            //if (tvwFolders.SelectedNode != null)
            if (_SelectedTreeNode != null)
            {
                //TreeNode Node = tvwFolders.SelectedNode;
                FTPEntry Entry = (FTPEntry)_SelectedTreeNode.Tag;

                if (Entry != null)
                {
                    if (Entry.Filename != "")
                    {
                        txtPath.Text = Entry.Path + "/" + Entry.Filename;
                    }
                    else
                    {
                        txtPath.Text = Entry.Path;
                    }
                }

                if (Entry != null && Entry.IsFolder)
                {
                    lblStatus.Text = "Loading file list...";
                    this.Refresh();
                    lvwDocuments.Items.Clear();

                    // Get list of folders on FTP server
                    Cursor.Current = Cursors.WaitCursor;
                    Application.DoEvents();
                    List<FTPEntry> Entries = Global.GetFTPDirectoryEntries(true, Entry.Path + "/" + Entry.Filename, Global.FTPEntrySelection.File);
                    Cursor.Current = Cursors.Default;

                    if (SearchTerm != "") lblStatus.Text = "Searching Documents...";
                    this.Refresh();

                    lvwDocuments.BeginUpdate();

                    foreach (FTPEntry FileEntry in Entries)
                    {
                        ListViewItem Document = new ListViewItem();
                        bool AddDocument = false;

                        Document.Text = FileEntry.Filename;
                        Document.ImageIndex = 2;
                        Document.Tag = FileEntry;

                        // Get corresponding Document information
                        var SelectedDocument = (from Document d in _Documents
                                                where (d.FileName.ToLower() == FileEntry.Filename.ToLower() && d.Path.ToLower() == FileEntry.Path.ToLower())
                                                select d).FirstOrDefault();  // was .SingleOrDefault()

                        if (SearchTerm != "" && SelectedDocument != null)
                        {
                            lblStatus.Text = "Looking through keywords: " + SelectedDocument.Keywords;
                            this.Refresh();
                            
                            // Perform search on Database info
                            if (
                                SelectedDocument.Name.ToLower().Contains(SearchTerm.ToLower()) || 
                                SelectedDocument.Keywords.ToLower().Contains(SearchTerm.ToLower())
                               )
                            {
                                AddDocument = true;
                            }

                            var MatchingConditions = from c in SelectedDocument.Conditions where c.Name.ToLower().Contains(SearchTerm.ToLower()) select c;

                            if (MatchingConditions.Count() > 0)
                            {
                                AddDocument = true;
                            }
                        }

                        if (SearchTerm != "")
                        {
                            if (FileEntry.Filename.ToLower().Contains(SearchTerm.ToLower()))
                            {
                                AddDocument = true;
                            }
                        }
                        else
                        {
                            AddDocument = true;
                        }

                        if (AddDocument)
                        {
                            if (SelectedDocument != null)
                            {
                                Document.SubItems.Add(SelectedDocument.Name);
                            }
                            else
                            {
                                Document.SubItems.Add("");
                            }
                            
                            if (SelectedDocument != null)
                            {
                                Document.SubItems.Add(SelectedDocument.Public.ToString());
                                Document.SubItems.Add(SelectedDocument.Keywords);
                            }

                            lvwDocuments.Items.Add(Document);
                        }
                    }

                    lvwDocuments.EndUpdate();
                }
                lblStatus.Text = "Idle";
            }
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoSearch(txtSearch.Text);
        }

        private void tvwFolders_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Console.WriteLine("tvwFolders_AfterSelect()");

            ClickTreeNode();

            if (e.Action == TreeViewAction.ByMouse)
            {
                //ClickTreeNode();
            }
        }

        private void ClickTreeNode()
        {
            if (_SelectedTreeNode != null)
            {
                lblStatus.Text = "Loading folder list...";
                this.Refresh();
                Console.WriteLine("ClickTreeNode()");
                if (_SelectedTreeNode.Tag != null) _SelectedFTPEntry = (FTPEntry)_SelectedTreeNode.Tag;
                btnAddFolder.Enabled = Global.Permissions.Contains("Create Document Folder");

                List<FTPEntry> Entries = Global.GetFTPDirectoryEntries(true, _SelectedFTPEntry.Path + "/" + _SelectedFTPEntry.Filename, Global.FTPEntrySelection.Folder);

                foreach (FTPEntry Entry in Entries)
                {
                    AddNodeToTreeview(_SelectedTreeNode, Entry.Filename, Entry.Filename, 1, 1);
                }

                DoSearch(txtSearch.Text);

                lblStatus.Text = "Idle";
            }
        }

        private void tvwFolders_MouseClick(object sender, MouseEventArgs e)
        {
            Console.WriteLine("tvwFolders_MouseClick()");
            //_SelectedTreeNode = tvwFolders.GetNodeAt(e.Location);

            //tvwFolders.SelectedNode = _SelectedTreeNode;

            if (_SelectedTreeNode != null)
            {
                btnDeleteFolder.Enabled = Global.Permissions.Contains("Delete Document Folder");
            }
            else
            {
                btnDeleteFolder.Enabled = false;
            }
        }

        private void tvwFolders_MouseUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine("tvwFolders_MouseUp()");

            TreeNode SelectedNode = tvwFolders.GetNodeAt(e.Location);
            if (SelectedNode != null)
            {
                _SelectedTreeNode = SelectedNode; //= tvwFolders.GetNodeAt(e.Location);
                tvwFolders.SelectedNode = _SelectedTreeNode;

                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    addFolderToolStripMenuItem.Enabled = Global.Permissions.Contains("Create Document Folder");

                    if (_SelectedTreeNode != null)
                    {
                        deleteFolderToolStripMenuItem.Enabled = Global.Permissions.Contains("Delete Document Folder");
                    }
                    else
                    {
                        deleteFolderToolStripMenuItem.Enabled = false;
                    }

                    cmsRightClickFolderList.Show(tvwFolders, e.Location);
                }
            }
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Search")
            {
                txtSearch.Text = "";
            }
            else
            {
                txtSearch.SelectAll();
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                DoSearch(txtSearch.Text);
            }
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (txtSearch.Text == "") txtSearch.Text = "Search";
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            AddNodeToTreeview(null, "documents", "Documents", 0, 0);
            _SelectedFTPEntry = new FTPEntry();
            _SelectedFTPEntry.Path = "/documents";
            _SelectedFTPEntry.IsFolder = true;

            tvwFolders.Nodes[0].Tag = _SelectedFTPEntry;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Cursor.Current = Cursors.Default;

            tvwFolders.ExpandAll();

            _Documents = Global.GetAllDocuments();

            lblStatus.Text = "Idle";
        }

        private void tvwFolders_AfterExpand(object sender, TreeViewEventArgs e)
        {
            
        }
    }
}
