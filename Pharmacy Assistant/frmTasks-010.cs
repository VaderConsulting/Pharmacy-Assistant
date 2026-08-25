using Model;
using RecurrenceGenerator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PharmacyAssistant
{
    public partial class frmTasks010 : Form
    {
        private List<int> _Certificates = new List<int>();
        private string _CurrentRecurranceValue = "";
        private int _CurrentTaskID = 0;
        private List<ListItem> _Documents = new List<ListItem>();
        private List<ListItem> _Roles = new List<ListItem>();
        private List<ListItem> _Stores = new List<ListItem>();
        private int _TaskCertificateID = 0;

        // New
        private Task _SelectedTask = null;
        private List<Task> _Tasks = new List<Task>();

        public frmTasks010()
        {
            InitializeComponent();
        }

        private void btnCertificate_Click(object sender, EventArgs e)
        {
            ChooseListItem("Certificate",true);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCreateTask_Click(object sender, EventArgs e)
        {
            ResetForm();

            txtWarningPeriod.Text = Properties.Settings.Default.EventWarningPeriod.ToString();

            txtTaskName.Enabled = true;
            txtTaskDescription.Enabled = true;
            txtTaskCertificate.Enabled = true;
            txtWarningPeriod.Enabled = true;
            txtNextDate.Enabled = true;
            dtpTaskDueDate.Enabled = true;
            radOnce.Enabled = true;
            radRecurring.Enabled = true;
            lstDocuments.Enabled = true;
            lstRoles.Enabled = true;
            lstStores.Enabled = true;
            chkEnabled.Enabled = true;
            chkMandatory.Enabled = true;
            btnCertificate.Enabled = true;
            btnDocuments.Enabled = true;
            btnEditFrequency.Enabled = true;
            btnSave.Enabled = true;
            btnStores.Enabled = true;
            btnRoles.Enabled = true;

            chkEnabled.Checked = true;
            chkMandatory.Checked = true;

            txtTaskName.Focus();
        }

        private void btnDeleteTask_Click(object sender, EventArgs e)
        {
            if (_CurrentTaskID != 0)
            {
                string Query = "DELETE FROM Task WHERE ID = " + _CurrentTaskID;

                Core.SQL.Functions.Execute(Query, Global.SqlConnectionString);

                GetTasks();

                ResetForm();
            }
        }

        private void btnDocuments_Click(object sender, EventArgs e)
        {
            ChooseListItem("Documents", false);

            //if (_CurrentTaskID == 0)
            //{
            //    MessageBox.Show("Please save the Task first", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //}
            //else
            //{
            //    frmListItemSelection008 LinkedDocuments = new frmListItemSelection008(null, Helper.ItemTypes.Task);

            //    LinkedDocuments.ListDisplayName = "Documents";
            //    LinkedDocuments.SingleItemConstraint = false;
            //    LinkedDocuments.ParentObjectID = _CurrentTaskID;
            //    LinkedDocuments.ReturnListOnly = false;

            //    LinkedDocuments.ShowDialog();
            //}
        }

        private void btnEditFrequency_Click(object sender, EventArgs e)
        {
            frmRecurrance RecurranceForm = new frmRecurrance();

            RecurranceForm.ParentID = _CurrentTaskID;
            RecurranceForm.ParentType = "Task";

            _CurrentRecurranceValue = "D" + dtpTaskDueDate.Value.ToString("yyyyMMdd") + dtpTaskDueDate.Value.ToString("yyyyMMdd") + "000001001";

            // Give a recurrance value to build up from
            RecurranceForm.RecurranceValue = _CurrentRecurranceValue;

            DialogResult Result = RecurranceForm.ShowDialog();

            if (Result == System.Windows.Forms.DialogResult.OK)
            {
                _CurrentRecurranceValue = RecurranceForm.RecurranceValue;

                radRecurring.Checked = true;

                switch (_CurrentRecurranceValue.Substring(0, 1))
                {
                    case "D":
                        {
                            lblFrequency.Text = "Daily";
                            break;
                        }
                    case "W":
                        {
                            lblFrequency.Text = "Weekly";
                            break;
                        }
                    case "M":
                        {
                            lblFrequency.Text = "Monthly";
                            break;
                        }
                    case "Y":
                        {
                            lblFrequency.Text = "Yearly";
                            break;
                        }
                }
            }
            else
            {
                lblFrequency.Text = "Once";
                radOnce.Checked = true;
            }
                
            //if (_CurrentTaskID != 0)  // Only load the recurrance value from the database if this is an update
            //{
            //    // Get the final recurrance value
            //    string Query = "SELECT Recurrance FROM Task WHERE ID = " + _CurrentTaskID;

            //    DataSet Data = Core.SQL.Functions.Execute(Query, Global.SqlConnectionString);
            //    _CurrentRecurranceValue = Convert.ToString(Core.SQL.Functions.GetFieldFromDataRow(Core.SQL.Functions.GetDataRowFromDataset(Data, 0, 0), 0));   //RecurranceForm.RecurranceValue;
            //}
            //txtNextDate.Text = RecurrenceHelper.GetNextDate(DateTime.Now, _CurrentRecurranceValue).ToString("d MMM, yyyy");
            txtNextDate.Text = RecurrenceHelper.GetNextDate(dtpTaskDueDate.Value, _CurrentRecurranceValue).ToString("d MMM, yyyy");

            
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GetTasks();

            ResetForm();
        }

        private void btnRoles_Click(object sender, EventArgs e)
        {
            ChooseListItem("Role", false);
            
            //if (_CurrentTaskID == 0)
            //{
            //    MessageBox.Show("Please save the Task first", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //}
            //else
            //{               
                //frmListItemSelection008 LinkedRoles = new frmListItemSelection008(null, Helper.ItemTypes.Task);

                //LinkedRoles.ListDisplayName = "Roles";
                //LinkedRoles.SingleItemConstraint = false;
                //LinkedRoles.ParentObjectID = _CurrentTaskID;
                //LinkedRoles.ReturnListOnly = false;

                //LinkedRoles.ShowDialog();
            //}
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveTask();

            GetTasks();

            ResetForm();
        }

        private void SaveTask()
        {
            ListItem SelectedItem = null;
            string Recurrance = "";
            string Query = "";
            string TaskEnabled = "0";
            string TaskMandatory = "0";

            if (chkEnabled.Checked) TaskEnabled = "1";
            if (chkMandatory.Checked) TaskMandatory = "1";

            // Build recurrance value
            if (radOnce.Checked)
            {
                // If we stored the single occurance the same way as multiples, it would be...
                //Recurrance = "D" + dtpEventStart.Value.ToString("yyyyMMdd") + dtpEventStart.Value.ToString("yyyyMMdd") + "000001001";
                Recurrance = dtpTaskDueDate.Value.ToString();
            }
            else
            {
                Recurrance = _CurrentRecurranceValue;
            }

            if (_CurrentTaskID == 0) // New task
            {
                Query = string.Format("INSERT INTO Task (" +
                                      "Name, Description, Complete, CreateDate, CertificateID, WarningPeriod, Recurrance, Enabled, DueDate, Mandatory) " +
                                      "VALUES (" +
                                      "'{0}','{1}',{2},'{3}',{4},{5},'{6}',{7},'{8}',{9}" +
                                      ");SELECT SCOPE_IDENTITY()",
                                      txtTaskName.Text.Replace("'", "''"),
                                      txtTaskDescription.Text.Replace("'", "''"),
                                      0,
                                      DateTime.Today.ToString("yyyyMMdd"),
                                      _TaskCertificateID.ToString(),
                                      txtWarningPeriod.Text.Replace("'", "''"),
                                      Recurrance,
                                      TaskEnabled,
                                      dtpTaskDueDate.Value.ToString("yyyyMMdd"),
                                      TaskMandatory
                                      );
            }
            else // Updated task
            {
                Query = string.Format("UPDATE Task SET Name='{0}', Description = '{1}', Complete = {2}, CreateDate = '{3}', CertificateID = {4}, WarningPeriod = {5}, Recurrance = '{6}', Enabled = {7}, DueDate = '{8}', Mandatory = {9} WHERE ID = {10}",
                                      txtTaskName.Text.Replace("'", "''"),
                                      txtTaskDescription.Text.Replace("'", "''"),
                                      0,
                                      DateTime.Today.ToString("yyyyMMdd"),
                                      _TaskCertificateID.ToString(),
                                      txtWarningPeriod.Text.Replace("'", "''"),
                                      Recurrance,
                                      TaskEnabled,
                                      dtpTaskDueDate.Value.ToString("yyyyMMdd"),
                                      TaskMandatory,
                                      _CurrentTaskID.ToString()
                                     );
            }

            Cursor.Current = Cursors.WaitCursor;
            DataSet Data = Core.SQL.Functions.Execute(Query, Global.SqlConnectionString);

            if (_CurrentTaskID == 0) _CurrentTaskID = Convert.ToInt32(Data.Tables[0].Rows[0][0]);

            // Remove any existing roles for this task
            Query = "DELETE FROM TaskRole WHERE TaskID = " + _CurrentTaskID;
            Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);

            // Save Roles against this task
            foreach (ListItem Role in lstRoles.Items)
            {
                Query = String.Format("INSERT INTO TaskRole (TaskID, RoleID) VALUES ({0}, {1})", _CurrentTaskID, Role.ID);
                Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);
            }

            // Remove any existing stores for this task
            Query = "DELETE FROM TaskStore WHERE TaskID = " + _CurrentTaskID;
            Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);

            // Save Stores against this task
            foreach (ListItem Store in lstStores.Items)
            {
                Query = String.Format("INSERT INTO TaskStore (TaskID, StoreID) VALUES ({0}, {1})", _CurrentTaskID, Store.ID);
                Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);
            }

            // Remove any existing documents for this task
            Query = "DELETE FROM TaskDocument WHERE TaskID = " + _CurrentTaskID;
            Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);

            // Save Documents against this task
            foreach (ListItem Document in lstDocuments.Items)
            {
                Query = String.Format("INSERT INTO TaskDocument (TaskID, DocumentID) VALUES ({0}, {1})", _CurrentTaskID, Document.ID);
                Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);
            }

            Cursor.Current = Cursors.Default;

            if (lstItems.SelectedItem != null) SelectedItem = (ListItem)lstItems.SelectedItem;

            ResetForm();

            Global.GetAllTasks(lstItems);

            if (SelectedItem != null) lstItems.SelectedItem = SelectedItem;
        }

        private void btnStores_Click(object sender, EventArgs e)
        {
            ChooseListItem("Stores",false);
            
            //if (_CurrentTaskID == 0)
            //{
            //    MessageBox.Show("Please save the Task first", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //}
            //else
            //{
                //frmListItemSelection008 SelectStoreForm = new frmListItemSelection008(null, Helper.ItemTypes.Task);

                //SelectStoreForm.ListDisplayName = "Stores";
                //SelectStoreForm.SingleItemConstraint = false;
                //SelectStoreForm.ParentObjectID = _CurrentTaskID;
                //SelectStoreForm.ReturnListOnly = false;

                //SelectStoreForm.ShowDialog();
            //}
        }

        private void chkEnabled_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ChooseListItem(string ItemType, bool SingleItemConstraint)
        {
            frmListItemSelection008 ItemSelection = new frmListItemSelection008(null, Helper.ItemType.Task);
            ItemSelection.ListDisplayName = ItemType;
            ItemSelection.ParentObjectID = _CurrentTaskID;
            ItemSelection.SingleItemConstraint = SingleItemConstraint; //true;
            ItemSelection.ReturnListOnly = true;

            ItemSelection.ShowDialog();

            switch (ItemType)
            {
                case "Certificate":
                case "Certificates":
                    {
                        _Certificates = ItemSelection.SelectedIDList;

                        if (_Certificates.Count > 0)
                        {
                            _TaskCertificateID = _Certificates[0];
                            txtTaskCertificate.Text = (string)Core.SQL.Functions.GetFieldFromDataRow(
                                                         Core.SQL.Functions.GetDataRowFromDataset(
                                                         Core.SQL.Functions.Execute(
                                                         "SELECT Name FROM Certificate WHERE ID = " + _TaskCertificateID.ToString(),
                                                         Global.SqlConnectionString), 0, 0), 0);
                        }
                        else
                        { 
                            _TaskCertificateID = 0;
                            txtTaskCertificate.Text = "";
                        }
                        break;
                    }
                case "Role":
                case "Roles":
                    {
                        _Roles = ItemSelection.SelectedListItems;

                        lstRoles.Items.Clear();

                        if (_Roles.Count > 0)
                        {
                            foreach (ListItem Role in _Roles)
                            {
                                lstRoles.Items.Add(Role);
                            }
                            
                            //Global.GetListItemData("SELECT r.ID, r.Name FROM TaskRole tr LEFT JOIN Role r ON tr.roleid = r.id LEFT JOIN Task t ON t.ID = tr.TaskID WHERE t.ID = " + _CurrentTaskID, lstRoles);
                        }
                        break;
                    }
                case "Store":
                case "Stores":
                    {
                        _Stores = ItemSelection.SelectedListItems;

                        lstStores.Items.Clear();

                        if (_Stores.Count > 0)
                        {
                            foreach (ListItem Store in _Stores)
                            {
                                lstStores.Items.Add(Store);
                            }
                            
                            //Global.GetListItemData("SELECT s.ID, s.Name FROM TaskStore ts LEFT JOIN Store s ON ts.StoreID = s.id LEFT JOIN Task t ON t.ID = ts.StoreID WHERE t.ID = " + _CurrentTaskID, lstStores);
                        }
                        break;
                    }
                case "Document":
                case "Documents":
                    {
                        _Documents = ItemSelection.SelectedListItems;

                        lstDocuments.Items.Clear();

                        if (_Documents.Count > 0)
                        {
                            foreach (ListItem Document in _Documents)
                            {
                                lstDocuments.Items.Add(Document);
                            }
                            
                            //Global.GetListItemData("SELECT d.ID, d.Name FROM Task t LEFT JOIN TaskDocument td ON td.DocumentID = t.id LEFT JOIN Document d ON d.ID = td.DocumentID WHERE t.ID = " + _CurrentTaskID, lstDocuments);
                        }
                        break;
                    }
            }
        }

        private void frmTasks010_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.ShowCompletedTasks = chkShowCompletedTasks.Checked;
            Properties.Settings.Default.Save();
            
            Global.RemoveFormFromList(this);
        }

        private void frmTasks010_Load(object sender, EventArgs e)
        {
            Global.AddFormToList(this);
            
            gpTitle.Image = PharmacyAssistant.Properties.Resources.realvista_projectmanagment_task_256;
            gpTitle.GradientStartColor = Global.Theme[19];

            chkShowCompletedTasks.Checked = Properties.Settings.Default.ShowCompletedTasks;

            GetTasks();

            ResetForm();
        }

        private void GetTasks()
        {
            _Tasks = Global.GetAllTasks(chkShowCompletedTasks.Checked);

            lstItems.Items.Clear();

            foreach (Task ThisTask in _Tasks)
            {
                lstItems.Items.Add(new ListItem(ThisTask.ID, ThisTask.Name));
            }
        }

        private void lstItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstItems.SelectedItem != null)
            {
                ResetForm();

                _CurrentTaskID = ((ListItem)(lstItems.SelectedItem)).ID;

                // Go through the collection of tasks and select the task the user has chosen
                var SelectedTask = (from Task t in _Tasks
                                    where t.ID == _CurrentTaskID
                                    select t).Single();

                _SelectedTask = (Task)SelectedTask;

                txtTaskName.Text        = _SelectedTask.Name;
                txtTaskDescription.Text = _SelectedTask.Description;
                _TaskCertificateID      = _SelectedTask.CertificateID;
                chkEnabled.Checked      = _SelectedTask.Enabled;
                chkMandatory.Checked    = _SelectedTask.Mandatory;
                chkComplete.Checked     = _SelectedTask.Complete;
                txtTaskCertificate.Text = _SelectedTask.CertificateName;
                txtCompletedBy.Text     = _SelectedTask.CompletedBy;

                if (_SelectedTask.WarningPeriod != 0) txtWarningPeriod.Text = _SelectedTask.WarningPeriod.ToString();
                if (_SelectedTask.DueDate       != null) dtpTaskDueDate.Value = _SelectedTask.DueDate;

                if (_SelectedTask.Recurrance != "")
                {
                    if (Microsoft.VisualBasic.Information.IsDate(_SelectedTask.Recurrance))
                    {
                        _CurrentRecurranceValue = _SelectedTask.Recurrance;
                        txtNextDate.Text = "";
                        radOnce.Checked = true;
                    }
                    else
                    {
                        _CurrentRecurranceValue = _SelectedTask.Recurrance;
                        txtNextDate.Text = RecurrenceHelper.GetNextDate(dtpTaskDueDate.Value, _CurrentRecurranceValue).ToString("d MMM, yyyy");
                        radRecurring.Checked = true;

                        switch (_CurrentRecurranceValue.Substring(0, 1))
                        {
                            case "D":
                                {
                                    lblFrequency.Text = "Daily";
                                    break;
                                }
                            case "W":
                                {
                                    lblFrequency.Text = "Weekly";
                                    break;
                                }
                            case "M":
                                {
                                    lblFrequency.Text = "Monthly";
                                    break;
                                }
                            case "Y":
                                {
                                    lblFrequency.Text = "Yearly";
                                    break;
                                }
                        }
                    }
                }

                txtTaskCertificate.Text = _SelectedTask.CertificateName;

                foreach (Document ThisDocument in _SelectedTask.Documents)
                {
                    lstDocuments.Items.Add(new ListItem(ThisDocument.ID, ThisDocument.Name));
                }

                foreach (Store ThisStore in _SelectedTask.Stores)
                {
                    lstStores.Items.Add(new ListItem(ThisStore.ID, ThisStore.Name));
                }

                foreach (Role ThisRole in _SelectedTask.Roles)
                {
                    lstRoles.Items.Add(new ListItem(ThisRole.ID, ThisRole.Name));
                }

                if (Global.Permissions.Contains("Write Task"))
                {
                    txtTaskName.Enabled = true;
                    txtTaskDescription.Enabled = true;
                    txtTaskCertificate.Enabled = true;
                    txtWarningPeriod.Enabled = true;
                    txtNextDate.Enabled = true;
                    dtpTaskDueDate.Enabled = true;
                    radOnce.Enabled = true;
                    radRecurring.Enabled = true;
                    lstDocuments.Enabled = true;
                    lstRoles.Enabled = true;
                    lstStores.Enabled = true;
                    //chkComplete.Enabled = true;
                    chkEnabled.Enabled = true;
                    chkMandatory.Enabled = true;
                    btnCertificate.Enabled = true;
                    btnDocuments.Enabled = true;
                    btnEditFrequency.Enabled = true;
                    btnSave.Enabled = true;
                    btnStores.Enabled = true;
                    btnRoles.Enabled = true;
                }

                grpRecurring.Enabled = radRecurring.Enabled;

                btnDeleteTask.Enabled = Global.Permissions.Contains("Delete Task");
                btnCopyTask.Enabled = Global.Permissions.Contains("Create Task");
            }
        }

        private void radOnce_CheckedChanged(object sender, EventArgs e)
        {
            grpRecurring.Enabled = radRecurring.Enabled;
        }

        private void radRecurring_CheckedChanged(object sender, EventArgs e)
        {
            grpRecurring.Enabled = radRecurring.Enabled;
        }

        private void ResetForm()
        {
            // Clear values
            txtTaskCertificate.Text = "";
            txtTaskDescription.Text = "";
            txtTaskName.Text = "";
            txtNextDate.Text = "";
            txtWarningPeriod.Text = Properties.Settings.Default.TaskWarningPeriod.ToString();
            dtpTaskDueDate.Value = DateTime.Now;
            lblFrequency.Text = "";
            radOnce.Checked = true;
            radRecurring.Checked = false;
            chkComplete.Checked = false;
            chkMandatory.Checked = false;
            chkEnabled.Checked = false;
            lstDocuments.Items.Clear();
            lstStores.Items.Clear();
            lstRoles.Items.Clear();

            // Disable controls
            txtTaskName.Enabled = false;
            txtTaskDescription.Enabled = false;
            txtTaskCertificate.Enabled = false;
            txtWarningPeriod.Enabled = false;
            txtNextDate.Enabled = false;
            dtpTaskDueDate.Enabled = false;
            radOnce.Enabled = false;
            radRecurring.Enabled = false;
            lstDocuments.Enabled = false;
            lstRoles.Enabled = false;
            lstStores.Enabled = false;
            chkComplete.Enabled = false;
            chkEnabled.Enabled = false;
            chkMandatory.Enabled = false;
            btnCertificate.Enabled = false;
            btnDocuments.Enabled = false;
            //btnEditFrequency.Enabled = false;
            btnDeleteTask.Enabled = false;
            btnCreateTask.Enabled = Global.Permissions.Contains("Create Task");
            btnCopyTask.Enabled = false;
            btnStores.Enabled = false;
            btnRoles.Enabled = false;
            btnSave.Enabled = false;

            // Clear fields
            _CurrentTaskID = 0;
            _CurrentRecurranceValue = "";
            _TaskCertificateID = 0;
            _Stores.Clear();
            _Roles.Clear();
            _Documents.Clear();
        }

        private void btnCopyTask_Click(object sender, EventArgs e)
        {
            Task NewTask = new Task();
            string TaskMandatory = "0";
            string TaskEnabled = "0";
            string TaskComplete = "0";
            string Query = "";
            DateTime NextTaskDate = DateTime.MinValue;

            if (_SelectedTask.Mandatory) TaskMandatory = "1";
            if (_SelectedTask.Enabled) TaskEnabled = "1";
            if (_SelectedTask.Complete) TaskComplete = "1";
            
            NewTask.CertificateID  =_SelectedTask.CertificateID;
            NewTask.CertificateName = _SelectedTask.CertificateName;
            NewTask.Complete = _SelectedTask.Complete;
            NewTask.CompletionDate = _SelectedTask.CompletionDate;
            NewTask.CreateDate  = DateTime.Today;
            NewTask.Description = _SelectedTask.Description;
            NewTask.Documents = _SelectedTask.Documents;
            NewTask.DueDate = _SelectedTask.DueDate;
            NewTask.Enabled = _SelectedTask.Enabled;
            NewTask.Mandatory = _SelectedTask.Mandatory;
            NewTask.Name = "Copy of " + _SelectedTask.Name;
            NewTask.Recurrance = _SelectedTask.Recurrance;
            NewTask.Roles = _SelectedTask.Roles;
            NewTask.Stores = _SelectedTask.Stores;
            NewTask.WarningPeriod = _SelectedTask.WarningPeriod;

            if (radOnce.Checked)
            {
                NextTaskDate = dtpTaskDueDate.Value;
            }
            else
            {
                NextTaskDate = RecurrenceHelper.GetNextDate(NewTask.DueDate, NewTask.Recurrance);
            }

            Query = string.Format("INSERT INTO Task (" +
                                      "Name, Description, Complete, CreateDate, CertificateID, WarningPeriod, Recurrance, Enabled, DueDate, Mandatory) " +
                                      "VALUES (" +
                                      "'{0}','{1}',{2},'{3}',{4},{5},'{6}',{7},'{8}',{9}" +
                                      ");SELECT SCOPE_IDENTITY()",
                                      NewTask.Name.Replace("'", "''"),
                                      NewTask.Description.Replace("'", "''"),
                                      TaskComplete,
                                      DateTime.Today.ToString("yyyyMMdd"),
                                      NewTask.CertificateID.ToString(),
                                      NewTask.WarningPeriod,
                                      NewTask.Recurrance,
                                      TaskEnabled,
                                      NextTaskDate,
                                      TaskMandatory
                                      );

            Cursor.Current = Cursors.WaitCursor;
            int NewTaskID = Convert.ToInt32(Core.SQL.Functions.Execute(Query, Global.SqlConnectionString).Tables[0].Rows[0][0]);
            Cursor.Current = Cursors.Default;

            // Assign this task to same Roles as original Task
            foreach (Role ThisRole in _SelectedTask.Roles)
            {
                Query = string.Format("INSERT INTO TaskRole (TaskID, RoleID) VALUES ({0}, {1})", NewTaskID, ThisRole.ID);

                Cursor.Current = Cursors.WaitCursor;
                Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);
                Cursor.Current = Cursors.Default;
            }

            // Add documents for this task
            foreach (Document TaskDocument in _SelectedTask.Documents)
            {
                Query = string.Format("INSERT INTO TaskDocument (TaskID, DocumentID) VALUES ({0}, {1})", NewTaskID, TaskDocument.ID);

                Cursor.Current = Cursors.WaitCursor;
                Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);
                Cursor.Current = Cursors.Default;
            }

            // Add stores for this task
            foreach (Store TaskStore in _SelectedTask.Stores)
            {
                Query = string.Format("INSERT INTO TaskStore (TaskID, StoreID) VALUES ({0}, {1})", NewTaskID, TaskStore.ID);

                Cursor.Current = Cursors.WaitCursor;
                Core.SQL.Functions.ExecuteNonQuery(Query, Global.SqlConnectionString);
                Cursor.Current = Cursors.Default;
            }

            GetTasks();
            ResetForm();
        }

        private void chkShowCompletedTasks_CheckedChanged(object sender, EventArgs e)
        {
            GetTasks();

            ResetForm();
        }
    }
}
