using Core.FileTransfer;
using Model;
using RecurrenceGenerator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Calendar;

namespace PharmacyAssistant
{
    public partial class frmMyTasks : Form
    {
        private int _CurrentTaskID = 0;
        private int _CompletedTaskID = 0;
        private bool _Loaded = false;
        private Task _SelectedCurrentTask = null;
        private Task _SelectedCompletedTask = null;
        private List<Task> _CurrentTasks = new List<Task>();
        private List<Task> _CompletedTasks = new List<Task>();

        public frmMyTasks()
        {
            InitializeComponent();
        }

        private void AddTasksToCurrentList()
        {
            //System.Windows.Forms.Calendar.CalendarItem CalItem = null;

            //calTasks.MaximumViewDays = (Properties.Settings.Default.CalendarDisplayWeeks - 1) * 7;

            ListViewGroup OverdueGroup = new ListViewGroup();
            ListViewGroup DueGroup = new ListViewGroup();
            ListViewGroup FutureGroup = new ListViewGroup();

            // To change calendar working hours...
            //List<CalendarHighlightRange> HighLightRanges = new List<CalendarHighlightRange>();
            //Dim HighLightRange As New CalendarHighlightRange
 
            //'Set working hours Wednesday
            //HighLightRange.DayOfWeek = DayOfWeek.Wednesday
            //HighLightRange.StartTime = New TimeSpan(9, 0, 0)   '08h00
            //HighLightRange.EndTime = New TimeSpan(18, 0, 0)    '18h00
            //HighLightRanges.Add(HighLightRange)
 
            //'Set working hours Thursday
            //HighLightRange.DayOfWeek = DayOfWeek.Thursday
            //HighLightRange.StartTime = New TimeSpan(12, 0, 0)  '12h00
            //HighLightRange.EndTime = New TimeSpan(20, 30, 0)   '20h30
            //HighLightRanges.Add(HighLightRange)
 
            //'Set calendar highlight ranges
            //Calendar.HighlightRanges = HighLightRanges.ToArray


            ///


            OverdueGroup.Name = "Overdue"; OverdueGroup.Header = "Overdue"; OverdueGroup.HeaderAlignment = HorizontalAlignment.Center;
            DueGroup.Name = "Due"; DueGroup.Header = "Due"; DueGroup.HeaderAlignment = HorizontalAlignment.Center;
            FutureGroup.Name = "Future"; FutureGroup.Header = "Future"; FutureGroup.HeaderAlignment = HorizontalAlignment.Center;

            lvwTasks.Items.Clear(); // Clear items and groups

            // Setup calendar
            //calTasks.Items.Clear(); 
            //calTasks.AllowItemEdit = false; 
            //calTasks.AllowItemResize = false; 
            //calTasks.AllowNew = false; 
            //calTasks.AllowMouseWheel = false; 
            //calTasks.AutoScroll = false;

            lvwTasks.Groups.Add(OverdueGroup);
            lvwTasks.Groups.Add(DueGroup);
            lvwTasks.Groups.Add(FutureGroup);
            
            if (_CurrentTasks.Count > 0)
            {
                //if (_Tasks[0].DueDate < DateTime.Today)
                //{
                //    calTasks.ViewStart = _Tasks[0].DueDate;
                //    calTasks.ViewEnd = _Tasks[0].DueDate.AddDays(calTasks.MaximumViewDays - 1);
                //}
                //else
                //{
                //    calTasks.ViewStart = DateTime.Today;
                //    calTasks.ViewEnd = DateTime.Today.AddDays(calTasks.MaximumViewDays - 1);
                //}
                
                foreach (Task UserTask in _CurrentTasks)
                {
                    ListViewItem lvItem = new ListViewItem();
                    Color BackColour = new Color();
                    Color ForeColour = new Color();
                    string TaskName = UserTask.Name;
                    int TaskID = UserTask.ID;
                    int NotificationDays = UserTask.WarningPeriod; // Properties.Settings.Default.TaskWarningPeriod;
                    DateTime DuePeriod = DateTime.Today.AddDays(NotificationDays);

                    //DateTime CalendarEntryStartDate = UserTask.DueDate;
                    //DateTime CalendarEntryEndDate = CalendarEntryStartDate.AddMinutes(1439); // 1440 is total minutes in a day

                    //CalItem = new System.Windows.Forms.Calendar.CalendarItem(calTasks); //calTasks, CalendarEntryStartDate, CalendarEntryEndDate, UserTask.Name);

                    //CalItem.StartDate = CalendarEntryStartDate;
                    //CalItem.EndDate = CalendarEntryEndDate;
                    //CalItem.Text = UserTask.Name;
                    //CalItem.Tag = UserTask;
                    //CalItem.Locked = true;

                    lvItem.Text = TaskName;
                    lvItem.Tag = UserTask;

                    if (UserTask.DueDate < DateTime.Today)  // This is in the past
                    {
                        BackColour = Color.Red;
                        ForeColour = Color.White;

                        lvItem.BackColor = BackColour;
                        lvItem.ForeColor = ForeColour;
                        lvItem.Group = OverdueGroup;
                    }
                    else if (UserTask.DueDate < DuePeriod)  // Due
                    {
                        BackColour = Color.LightGreen;
                        ForeColour = Color.Black;

                        lvItem.BackColor = BackColour;
                        lvItem.ForeColor = ForeColour;
                        lvItem.Group = DueGroup;
                    }
                    else                                          // Future
                    {
                        BackColour = Color.White;
                        ForeColour = Color.Black;

                        lvItem.BackColor = BackColour;
                        lvItem.ForeColor = ForeColour;
                        lvItem.Group = FutureGroup;
                    }

                    lvwTasks.Items.Add(lvItem);
                    //CalItem.ApplyColor(BackColour);

                    //if (calTasks.ViewIntersects(CalItem)) calTasks.Items.Add(CalItem);
                }
            }
        }

        private void AddTasksToCompletedList()
        {
            lvwCompleteTasks.Items.Clear(); // Clear items and groups

            if (_CompletedTasks.Count > 0)
            {

                foreach (Task UserTask in _CompletedTasks)
                {
                    ListViewItem lvItem = new ListViewItem();

                    bool DueDateWithinRange = UserTask.DueDate.Date >= dtpFrom.Value.Date && UserTask.DueDate.Date <= dtpTo.Value.Date;
                    bool CompletionDateWithinRange = UserTask.CompletionDate.Date >= dtpFrom.Value.Date && UserTask.CompletionDate.Date <= dtpTo.Value.Date;

                    if (!chkFilter.Checked || (chkFilter.Checked && (DueDateWithinRange || CompletionDateWithinRange)))
                    {
                        lvItem.Tag = UserTask;

                        lvItem.BackColor = Color.White;
                        lvItem.ForeColor = Color.Black;

                        lvItem.Text = UserTask.Name;
                        lvItem.SubItems.Add(UserTask.DueDate.ToString("d"));
                        lvItem.SubItems.Add(UserTask.CompletionDate.ToString("d"));
                        lvItem.SubItems.Add(UserTask.CompletedStoreName.ToString());
                        lvItem.SubItems.Add(UserTask.CompletedBy.ToString());

                        lvwCompleteTasks.Items.Add(lvItem);
                    }
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCompleted_Click(object sender, EventArgs e)
        {
            string Query = "";

            // Close this task against the same roles
            foreach (Role ThisRole in _SelectedCurrentTask.Roles)
            {
                Query = string.Format("UPDATE TaskRole SET Complete = 1 WHERE ID = {0}", ThisRole.ID.ToString());

                Cursor.Current = Cursors.WaitCursor;
                Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);
                Cursor.Current = Cursors.Default;
            }

            // Remove documents against the original Task
            foreach (Document TaskDocument in _SelectedCurrentTask.Documents)
            {
                Query = string.Format("DELETE FROM TaskDocument WHERE TaskID = {0} AND DocumentID = {1} ", _SelectedCurrentTask.ID, TaskDocument.ID);

                Cursor.Current = Cursors.WaitCursor;
                Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);
                Cursor.Current = Cursors.Default;
            }

            if (txtNextDate.Text != "") // Recurrance
            {
                // Close this task
                Query = string.Format("UPDATE Task SET Complete = 1, CompletedBy = {0}, CompletionDate = GETDATE() WHERE ID = {1}", Global.UserID, _CurrentTaskID.ToString());

                Cursor.Current = Cursors.WaitCursor;
                Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);
                Cursor.Current = Cursors.Default;

                #region Create new task

                string TaskMandatory = "0";
                string TaskEnabled = "0";

                if (_SelectedCurrentTask.Mandatory) TaskMandatory = "1";
                if (_SelectedCurrentTask.Enabled) TaskEnabled = "1";

                Query = string.Format("INSERT INTO Task (" +
                                      "Name, Description, Complete, CreateDate, CertificateID, WarningPeriod, Recurrance, Enabled, DueDate, Mandatory) " +
                                      "VALUES (" +
                                      "'{0}','{1}',{2},'{3}',{4},{5},'{6}',{7},'{8}',{9}" +
                                      ");SELECT SCOPE_IDENTITY()",
                                      txtTaskName.Text.Replace("'", "''"),
                                      txtTaskDescription.Text.Replace("'", "''"),
                                      0,
                                      DateTime.Today.ToString("yyyyMMdd"),
                                      _SelectedCurrentTask.CertificateID.ToString(),
                                      _SelectedCurrentTask.WarningPeriod,
                                      _SelectedCurrentTask.Recurrance,
                                      TaskEnabled,
                                      RecurrenceHelper.GetNextDate(_SelectedCurrentTask.DueDate, _SelectedCurrentTask.Recurrance).ToString("yyyyMMdd"),
                                      TaskMandatory
                                      );

                Cursor.Current = Cursors.WaitCursor;
                int NewTaskID = Convert.ToInt32(Core.SQL.Functions.Execute(Query, Global.SqlConnectionString).Tables[0].Rows[0][0]);
                Cursor.Current = Cursors.Default;

                // Assign this task to same Roles as original Task
                foreach (Role ThisRole in _SelectedCurrentTask.Roles)
                {
                    Query = string.Format("INSERT INTO TaskRole (TaskID, RoleID) VALUES ({0}, {1})", NewTaskID, ThisRole.ID);

                    Cursor.Current = Cursors.WaitCursor;
                    Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);
                    Cursor.Current = Cursors.Default;
                }

                // Add documents for this task
                foreach (Document TaskDocument in _SelectedCurrentTask.Documents)
                {
                    Query = string.Format("INSERT INTO TaskDocument (TaskID, DocumentID) VALUES ({0}, {1})", NewTaskID, TaskDocument.ID);

                    Cursor.Current = Cursors.WaitCursor;
                    Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);
                    Cursor.Current = Cursors.Default;
                }

                // Add stores for this task
                foreach (Store TaskStore in _SelectedCurrentTask.Stores)
                {
                    Query = string.Format("INSERT INTO TaskStore (TaskID, StoreID) VALUES ({0}, {1})", NewTaskID, TaskStore.ID);

                    Cursor.Current = Cursors.WaitCursor;
                    Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);
                    Cursor.Current = Cursors.Default;
                }

                #endregion
            }
            else
            {
                // Close this task
                Query = string.Format("UPDATE Task SET Complete = 1, CompletedBy = {0}, CompletionDate = GETDATE() WHERE ID = {1}", Global.UserID, _CurrentTaskID.ToString());

                Cursor.Current = Cursors.WaitCursor;
                Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);
                Cursor.Current = Cursors.Default;
            }

            GetMyTasks();
            ResetForm();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ResetForm();
            GetMyTasks();
        }

        private void btnViewDocument_Click(object sender, EventArgs e)
        {
            // The openDocument method originally came from another form that tracked
            // the current document.  We don't do that in this form, so as a workaround
            // the Document has been saved in the Tag Property of the Listbox Item.
            // This happens when the ListView is populated
            OpenDocument((Document)lstDocuments.SelectedItem);
        }

        private void frmMyTasks_FormClosing(object sender, FormClosingEventArgs e)
        {
            Global.RemoveFormFromList(this);
        }

        private void frmMyTasks_Load(object sender, EventArgs e)
        {
            Global.AddFormToList(this);

            gpTitle.GradientStartColor = Global.Theme[19];  // Theme colour 19 is used for Tasks AND MyTasks

            lvwCompleteTasks.Columns.Clear();
            lvwCompleteTasks.Columns.Add("Name", 500, HorizontalAlignment.Left);
            lvwCompleteTasks.Columns.Add("Date Due", 80, HorizontalAlignment.Left);
            lvwCompleteTasks.Columns.Add("Date Completed", 80, HorizontalAlignment.Left);
            lvwCompleteTasks.Columns.Add("Store", 120, HorizontalAlignment.Left);
            lvwCompleteTasks.Columns.Add("Username", 120, HorizontalAlignment.Left);

            GetMyTasks();
            _Loaded = true;
        }

        private void GetMyTasks()
        {
            _CurrentTasks.Clear();
            _CompletedTasks.Clear();

            _CurrentTasks = Global.GetUserTasks(Properties.Settings.Default.TaskDisplayPeriod, Global.UserID);
            _CompletedTasks = Global.GetUserTasks(-1, Global.UserID);

            AddTasksToCurrentList();
            AddTasksToCompletedList();
        }

        private void GetCurrentDetails()
        {           
            // Go through the collection of tasks and select the task the user has chosen
            var SelectedTask = (from Task t in _CurrentTasks
                               where t.ID == _CurrentTaskID
                               select t).Single();

            _SelectedCurrentTask = (Task)SelectedTask;

            int NotificationDays = _SelectedCurrentTask.WarningPeriod; // Properties.Settings.Default.TaskWarningPeriod;
            DateTime DuePeriod = DateTime.Today.AddDays(NotificationDays);

            txtTaskName.Text = _SelectedCurrentTask.Name;
            txtTaskDescription.Text = _SelectedCurrentTask.Description;
            dtpTaskStart.Value = _SelectedCurrentTask.DueDate;
            txtTaskCertificate.Text = _SelectedCurrentTask.CertificateName.ToString();

            if (_SelectedCurrentTask.DueDate < DateTime.Today || _SelectedCurrentTask.DueDate < DuePeriod) btnCompleted.Enabled = _SelectedCurrentTask.Enabled;

            if (!Microsoft.VisualBasic.Information.IsDate(_SelectedCurrentTask.Recurrance))
            {
                DateTime RecurranceDate;
                bool Result = DateTime.TryParse(_SelectedCurrentTask.Recurrance, out RecurranceDate);
                bool Recurring = true;
                DateTime LastOccurance;

                // Check here for the next date (is it greater than the Last date?)

                RecurrenceInfo ExtraTaskInfo = RecurrenceHelper.GetFriendlySeriesInfo(_SelectedCurrentTask.Recurrance);

                DateTime NextDate = RecurrenceHelper.GetNextDate(_SelectedCurrentTask.DueDate, _SelectedCurrentTask.Recurrance);
                
                if (ExtraTaskInfo.EndDate != null && NextDate > ExtraTaskInfo.EndDate) Recurring = false;
                if (ExtraTaskInfo.NumberOfOccurrences > 0)
                {
                    LastOccurance = ExtraTaskInfo.StartDate.AddDays(ExtraTaskInfo.NumberOfOccurrences);

                    if (NextDate >= LastOccurance) Recurring = false;
                }

                if (Recurring)
                {
                    txtNextDate.Text = RecurrenceHelper.GetNextDate(_SelectedCurrentTask.DueDate, _SelectedCurrentTask.Recurrance).ToString("dddd , d MMMM  yyyy");
                }
            }
            else
            {
                txtNextDate.Text = ""; txtNextDate.Enabled = false;
            }
            
            // Documents
            foreach (Document ThisDocument in _SelectedCurrentTask.Documents)
            {
                //lstDocuments.Items.Add(new ListItem(ThisDocument.ID, ThisDocument.Name));
                lstDocuments.Items.Add(ThisDocument);
            }

            //Global.GetListItemData("SELECT d.ID, d.Name, d.Filename, d.Path FROM TaskDocument td LEFT JOIN Document d ON td.DocumentID = d.id LEFT JOIN Task t ON t.ID = td.TaskID WHERE t.ID = " + _CurrentTaskID.ToString(), lstDocuments);
        }

        private void GetCompletedDetails()
        {
            // Go through the collection of tasks and select the task the user has chosen
            var SelectedTask = (from Task t in _CompletedTasks
                                where t.ID == _CompletedTaskID
                                select t).Single();

            _SelectedCompletedTask = (Task)SelectedTask;

            int NotificationDays = _SelectedCompletedTask.WarningPeriod; // Properties.Settings.Default.TaskWarningPeriod;
            DateTime DuePeriod = DateTime.Today.AddDays(NotificationDays);

            ListViewItem lvItem = new ListViewItem();

            lvItem.Text = SelectedTask.Name;
            lvItem.SubItems.Add(SelectedTask.DueDate.ToString("d"));
            lvItem.SubItems.Add(SelectedTask.CompletionDate.ToString("d"));
            lvItem.SubItems.Add(SelectedTask.CompletedStoreName.ToString());
            lvItem.SubItems.Add(SelectedTask.CompletedBy.ToString());

            lvwCompleteTasks.Items.Add(lvItem);
            //txtCompletedTaskName.Text = _SelectedCompletedTask.Name;
            //txtCompletedTaskDescription.Text = _SelectedCompletedTask.Description;
            //dtpCompletedTaskStart.Value = _SelectedCompletedTask.DueDate;
            //txtCompletedTaskStore.Text = _SelectedCompletedTask.CompletedStoreName;
            //txtCompletedNextDate.Text = _SelectedCompletedTask.CompletionDate.ToString("dddd , d MMMM  yyyy");
            //txtCompletedBy.Text = _SelectedCompletedTask.CompletedBy;

            // Documents
            //foreach (Document ThisDocument in _SelectedCompletedTask.Documents)
            //{
                //lstDocuments.Items.Add(new ListItem(ThisDocument.ID, ThisDocument.Name));
                //lstCompletedDocuments.Items.Add(ThisDocument);
            //}

            //Global.GetListItemData("SELECT d.ID, d.Name, d.Filename, d.Path FROM TaskDocument td LEFT JOIN Document d ON td.DocumentID = d.id LEFT JOIN Task t ON t.ID = td.TaskID WHERE t.ID = " + _CurrentTaskID.ToString(), lstDocuments);
        }

        private void lstDocuments_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstDocuments.SelectedItem != null)
            {
                btnViewDocument.Enabled = true;
            }
            
        }

        private void lvwTasks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvwTasks.SelectedItems.Count > 0)
            {               
                ResetForm();

                _CurrentTaskID = ((Task)lvwTasks.SelectedItems[0].Tag).ID;

                GetCurrentDetails();
            }
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
                    Global.Common.Logging.WriteErrorEvent(String.Format("MyTasks form (OpenDocument) - {0}.\nThe message is: {1}", ex.StackTrace, ex.Message));
                }
            }

            // Downloaded or not, we can now open it
            Global.OpenDocument(LocalFilename);

            Cursor.Current = Cursors.Default;
        }

        private void ResetForm()
        {
            txtTaskCertificate.Text = "";
            txtTaskDescription.Text = "";
            txtTaskName.Text = "";
            txtNextDate.Text = "";
            dtpTaskStart.Value = DateTime.Now;
            btnViewDocument.Enabled = false;
            btnCompleted.Enabled = false;

            lstDocuments.Items.Clear();
        }

        private void btnApplyFilter_Click(object sender, EventArgs e)
        {
            AddTasksToCompletedList();
        }

    }
}
