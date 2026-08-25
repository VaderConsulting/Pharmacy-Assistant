using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Core.SQL;

namespace RPM_Import
{
    public partial class frmMain : Form
    {
        private string _SourceConnectionString = Properties.Settings.Default.SourceConnectionString;
        private string _DestinationConnectionString = Properties.Settings.Default.DestinationConnectionString;
        private List<Product> _UpdatedProducts = new List<Product>();
        private List<Product> _UnalteredProducts = new List<Product>();
        private List<Product> _NewProducts = new List<Product>();
        private List<Product> _NewAndUpdatedProducts = new List<Product>();

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
            {
                _SourceConnectionString = Properties.Settings.Default.SourceDevConnectionString;
                _DestinationConnectionString = Properties.Settings.Default.DestinationDevConnectionString;
            }

            txtSourceConnectionString.Text = _SourceConnectionString;
            txtDestinationConnectionString.Text = _DestinationConnectionString;
            txtImportQuery.Text = Properties.Settings.Default.ImportQuery;

            string[] Args = Environment.GetCommandLineArgs();

            if (Args.Length > 1)
            {
                this.Show();
                this.Refresh();

                tabPages.SelectedIndex = 1;
                
                DoImport(false);
                Environment.Exit(0);
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            DoImport(true);
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            DoImport(false);
        }

        private void DoImport(bool Preview)
        {
            int SourceRowCount = 0;
            int Progress = 0;
            string ExistingSelectQuery = "SELECT ID, UPI, Price FROM Product";
            string CatalogueSelectQuery = "";
            // Get the last catalogue ID in the system
            string LastCatalogueQuery = "SELECT TOP 1 AutoSpecialsID AS ID, AutoSpecialsName AS Name FROM AutoSpecials ORDER BY AutoSpecialsID DESC";
            string LastKnownCatalogueQuery = "SELECT TOP 1 ID, RPMID, Name FROM Catalog ORDER BY RPMID DESC";
            bool NewCatalogue = false;
            int NewCatalogueID = 0;
            int Counter = 0;
            System.Data.SqlClient.SqlDataAdapter CatalogueDataAdapter = null;

            DataRow LastCatalogue = Functions.GetDataRowFromDataset(Functions.Execute(LastCatalogueQuery, txtSourceConnectionString.Text), 0, 0);
            DataRow LastKnownCatalogue = Functions.GetDataRowFromDataset(Functions.Execute(LastKnownCatalogueQuery, txtDestinationConnectionString.Text), 0, 0);

            if (Convert.ToInt32(LastCatalogue["ID"]) != Convert.ToInt32(LastKnownCatalogue["RPMID"]))
            {
                // There is a new catalogue since the last runtime
                lblStatus.Text = "Found a new catalogue: " + LastCatalogue["ID"].ToString(); lblStatus.Refresh();

                NewCatalogue = true;
                NewCatalogueID = (int)LastCatalogue["ID"];

                CatalogueSelectQuery = "SELECT s.AutoSpecialsName AS CatalogueName, a.StartDate, a.LastDate, p.CosmosUPI AS UPI, CAST(a.OrigRetailPrice AS float) / 100 AS SavemorPrice, CAST(a.AutoSpecialsRetail AS float) / 100 AS CatalogPrice, CAST(a.SpecialPrice AS float) / 100 AS CostPrice, CAST(CAST(a.AutoSpecialsRetail AS float) / 100 - CAST(a.SpecialPrice AS float) / 100 AS float) AS NormalSavings, CAST(CAST(a.OrigRetailPrice AS float) / 100 - CAST(a.SpecialPrice AS float) / 100 AS float) AS RetailSaving FROM AutoSpecialsItems AS a LEFT OUTER JOIN Product AS p ON p.ProductID = a.ProductID LEFT OUTER JOIN AutoSpecials AS s ON s.AutoSpecialsID = a.AutoSpecialsID WHERE a.StoreID = 2 AND a.AutoSpecialsID = " + NewCatalogueID.ToString();
            }

            _NewAndUpdatedProducts.Clear();
            _NewProducts.Clear();
            _UnalteredProducts.Clear();
            _UpdatedProducts.Clear();
            lstNew.Items.Clear();
            lstUnaltered.Items.Clear();
            lstUpdates.Items.Clear();

            this.Refresh();

            System.Data.DataSet LocalSourceDataSet = new DataSet("Products");
            System.Data.DataSet LocalDestinationDataSet = new DataSet("Products");
            System.Data.DataSet LocalCatalogueDataSet = new DataSet("Products");

            DataTable LocalDestinationTable = null;
            DataTable LocalCatalogueTable = null;

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                lblStatus.Text = "Creating and opening connection to RPM"; lblStatus.Refresh();
                // Create and open connection to source database (RPM)
                System.Data.SqlClient.SqlConnection SourceConnection = new System.Data.SqlClient.SqlConnection(txtSourceConnectionString.Text);
                SourceConnection.Open();

                lblStatus.Text = "Creating and opening connection to Products"; lblStatus.Refresh();
                // Create and open connection to destination database (Savemor)
                System.Data.SqlClient.SqlConnection DestinationConnection = new System.Data.SqlClient.SqlConnection(txtDestinationConnectionString.Text);
                DestinationConnection.Open();

                lblStatus.Text = "Creating Data Adapters"; lblStatus.Refresh();
                // Create data adapters
                System.Data.SqlClient.SqlDataAdapter SourceDataAdapter = new System.Data.SqlClient.SqlDataAdapter(txtImportQuery.Text, SourceConnection);
                System.Data.SqlClient.SqlDataAdapter ExistingDataAdapter = new System.Data.SqlClient.SqlDataAdapter(ExistingSelectQuery, DestinationConnection);
                if (NewCatalogue) CatalogueDataAdapter = new System.Data.SqlClient.SqlDataAdapter(CatalogueSelectQuery, SourceConnection);

                lblStatus.Text = "Filling Source data"; lblStatus.Refresh();
                // Fill table from Source
                SourceDataAdapter.Fill(LocalSourceDataSet, "RPMProducts");
                SourceRowCount = LocalSourceDataSet.Tables[0].Rows.Count;

                lblStatus.Text = "Filling existing data"; lblStatus.Refresh();
                // Fill table from existing data
                ExistingDataAdapter.Fill(LocalDestinationDataSet, "ExistingProducts");

                LocalDestinationTable = LocalDestinationDataSet.Tables["ExistingProducts"];

                if (NewCatalogue)
                {
                    lblStatus.Text = "Filling catalogue data"; lblStatus.Refresh();
                    CatalogueDataAdapter.Fill(LocalCatalogueDataSet, "CatalogueProducts");

                    LocalCatalogueTable = LocalCatalogueDataSet.Tables["CatalogueProducts"];

                    // Before we can go any further, we need to get the product ID's for each of the catalogue records we want to import by doing a join
                    // across the two DataTables.  The product ID's are in LocalDestinationTable, the rest is in LocalCatalogueTable

                    var results = from CatalogueTable in LocalCatalogueTable.AsEnumerable()
                                  join ProductsTable in LocalDestinationTable.AsEnumerable() on (int)CatalogueTable["UPI"] equals (int)ProductsTable["UPI"]
                                  select new
                                  {
                                      CatalogPrice = Convert.ToDouble(CatalogueTable["CatalogPrice"]),
                                      CatalogID = (int)NewCatalogueID,
                                      ProductID = Convert.ToInt32(ProductsTable["ID"]),
                                      StartDate = Convert.ToDateTime(CatalogueTable["StartDate"]),
                                      LastDate = Convert.ToDateTime(CatalogueTable["LastDate"])
                                  };

                    results = results.AsEnumerable();
                    
                    
                    // Now to add each of these to the ProductCatalog table
                    foreach (var Row in results)
                    {
                        Counter++;
                        lblStatus.Text = string.Format("Loading catalogue pricing ({0} of {1})", Counter, LocalCatalogueTable.Rows.Count); lblStatus.Refresh();
                        string INSERTQuery = string.Format("INSERT INTO ProductCatalog (Price, CatalogID, ProductID, StartDate, EndDate) VALUES ({0},{1},{2},'{3}','{4}')", Row.CatalogPrice, Row.CatalogID, Row.ProductID, Row.StartDate.Date.ToString("yyyyMMdd"), Row.LastDate.Date.ToString("yyyyMMdd"));

                        Functions.ExecuteNonQuery(INSERTQuery, txtDestinationConnectionString.Text);
                    }

                    // Finally, add the RPM Catalog ID to the Catalog table so that we know about it
                    lblStatus.Text = string.Format("Inserting catalogue data)"); lblStatus.Refresh();
                    string CatalogINSERTQuery = string.Format("INSERT INTO Catalog (RPMID, Name) VALUES ({0},'{1}')", LastCatalogue["ID"], LastCatalogue["Name"].ToString().Replace("'", "''"));

                    Functions.ExecuteNonQuery(CatalogINSERTQuery, txtDestinationConnectionString.Text);
                }

                progressBar.Value = 0;
                progressBarNew.Value = 0;
                progressBarUpdates.Value = 0;
                progressBarUnaltered.Value = 0;

                progressBar.Maximum = SourceRowCount;
                progressBarNew.Maximum = SourceRowCount;
                progressBarUpdates.Maximum = SourceRowCount;
                progressBarUnaltered.Maximum = SourceRowCount;

                lblStatus.Text = "Looping through Source data"; lblStatus.Refresh();
                // Loop through each row in the source data
                foreach (DataRow Row in LocalSourceDataSet.Tables[0].Rows)
                {
                    int RPMID = (int)Row["ProductID"];  // This is the RPM ProductID
                    decimal NewPrice = Convert.ToDecimal(Row["Price"]);
                    Int32 UPI = (Int32)Row["UPI"];
                    string Name = (string)Row["Name"];
                    bool Deleted = (bool)Row["Deleted"];

                    // Search for corresponding row in existing data
                    string SearchQuery = "SELECT ID, ISNULL(Price,CAST(0.0 AS float)) AS Price FROM ExistingProducts WHERE UPI = " + UPI;

                    // Use Linq to find the data we need
                    var ExistingDataRow = from p in LocalDestinationTable.AsEnumerable()
                                          where p.Field<int>("UPI") == UPI
                                          select p;

                    if (ExistingDataRow.Count() > 0)
                    {
                        // Existing product
                        DataRow ExistingProduct = (DataRow)ExistingDataRow.Single();

                        decimal ExistingPrice = Convert.ToDecimal(ExistingProduct.ItemArray[2]);
                        int ExistingProductID = Convert.ToInt32(ExistingProduct.ItemArray[0]);

                        if (ExistingPrice == NewPrice)
                        {
                            lstUnaltered.Items.Add(UPI.ToString());

                            Product Unaltered = new Product(); Unaltered.UPI = UPI;
                            Unaltered.OldPrice = ExistingPrice;

                            _UnalteredProducts.Add(Unaltered);

                            progressBarUnaltered.Value += 1;

                            lblUnalteredCount.Text = _UnalteredProducts.Count.ToString();
                        }
                        else
                        {
                            lstUpdates.Items.Add(UPI.ToString());

                            Product Updated = new Product(); Updated.UPI = UPI;
                            Updated.NewPrice = NewPrice;
                            Updated.OldPrice = ExistingPrice;
                            Updated.ProductID = ExistingProductID;

                            _UpdatedProducts.Add(Updated);
                            _NewAndUpdatedProducts.Add(Updated);

                            progressBarUpdates.Value += 1;

                            lblUpdateCount.Text = _UpdatedProducts.Count.ToString();
                        }
                    }
                    else
                    {
                        lstNew.Items.Add(UPI.ToString());
                        //lblNewCount.Text = lstNew.Items.Count.ToString();

                        Product NewProduct = new Product();
                        NewProduct.UPI = UPI;
                        NewProduct.NewPrice = NewPrice;
                        NewProduct.OldPrice = 0;
                        NewProduct.Name = Name;
                        NewProduct.ProductID = 0;
                        NewProduct.RPMID = RPMID;

                        _NewProducts.Add(NewProduct);
                        _NewAndUpdatedProducts.Add(NewProduct);

                        progressBarNew.Value += 1;

                        lblNewCount.Text = _NewProducts.Count.ToString();
                    }

                    Progress += 1;
                    progressBar.Value = Progress;

                    Application.DoEvents();

                    #region Ignore

                    //using (System.Data.SqlClient.SqlDataAdapter DestinationDataAdapter = new System.Data.SqlClient.SqlDataAdapter(SearchQuery, DestinationConnection))
                    //{
                    //    DataSet QueryDataSet = new DataSet();
                    //    DestinationDataAdapter.Fill(QueryDataSet, "Product");
                    //    string Audit = "";


                    //    if (QueryDataSet.Tables[0].Rows.Count > 0)  // Found corresponding row
                    //    {
                    //        double CurrentPrice = Convert.ToDouble(QueryDataSet.Tables[0].Rows[0]["Price"]);
                    //        if (CurrentPrice != NewPrice)
                    //        {
                    //            lstUpdates.Items.Add(UPI.ToString());

                    //            lblUpdateCount.Text = lstUpdates.Items.Count.ToString();
                    //            //Console.WriteLine("UPI " + UPI.ToString() + " has a price update.");

                    //            string UpdateQuery = "UPDATE Product SET Price = " + NewPrice.ToString() + " WHERE ID = " + QueryDataSet.Tables[0].Rows[0]["ID"];

                    //            if (Preview)  // Don't actually change anything
                    //            {

                    //            }
                    //            else  // Update existing Product
                    //            {
                    //                //Core.SQL.Functions.ExecuteNonQuery(UpdateQuery, _DestinationConnectionString);
                    //            }

                    //            Audit = "";
                    //        }
                    //        else  // Unaltered
                    //        {
                    //            lstUnaltered.Items.Add(UPI.ToString());

                    //            lblUnalteredCount.Text = lstUnaltered.Items.Count.ToString();
                    //        }
                    //    }
                    //    else  // No corresponding row
                    //    {
                    //        lstNew.Items.Add(UPI.ToString());

                    //        lblNewCount.Text = lstNew.Items.Count.ToString();

                    //        string InsertQuery = "INSERT INTO Product () VALUES ()";

                    //        Audit = "";

                    //        if (Preview) // Don't actually change anything
                    //        {

                    //        }
                    //        else // Insert NEW Product
                    //        {

                    //        }


                    //    }

                    //    Progress += 1;
                    //    progressBar.Value = Progress;
                    //    //this.Refresh();
                    //    Application.DoEvents();
                    //}
                    #endregion
                }

                lblStatus.Text = "Creating Temporary Table"; lblStatus.Refresh();

                // For speed, we now do a Bulk upsert.
                //Make a temp table in sql server that matches our production table
                string TemporaryTableCreateStatement = "CREATE TABLE #RPMImportProducts([UPI] [int] NULL,[Name] [nvarchar](max) NULL,[Price] [decimal](18, 2) NULL,[CoreProduct] [bit] NULL,[CustomString1] [nvarchar](max) NULL)";

                //Create a datatable that matches the temp table exactly. (WARNING: order of columns must match the order in the table)
                DataTable TemporaryTable = new DataTable();
                TemporaryTable.Columns.Add(new DataColumn("UPI", typeof(Int32)));
                TemporaryTable.Columns.Add(new DataColumn("Name", typeof(string)));
                TemporaryTable.Columns.Add(new DataColumn("Price", typeof(Decimal)));
                TemporaryTable.Columns.Add(new DataColumn("CoreProduct", typeof(bool)));
                TemporaryTable.Columns.Add(new DataColumn("CustomString1", typeof(string)));

                lblStatus.Text = "Adding new and updated products to Temporary Table"; lblStatus.Refresh();

                //Add prices in our list to our DataTable
                foreach (Product product in _NewAndUpdatedProducts)
                {
                    DataRow row = TemporaryTable.NewRow();
                    row["UPI"] = product.UPI.ToString();
                    row["Price"] = product.NewPrice.ToString();  //product.NewPrice.ToString();
                    row["Name"] = product.Name;
                    row["CoreProduct"] = product.CoreProduct;
                    row["CustomString1"] = product.RPMID.ToString();

                    TemporaryTable.Rows.Add(row);
                }

                lblStatus.Text = "Connecting to Products database"; lblStatus.Refresh();
                //Connect to DB
                string conString = _DestinationConnectionString;
                using (SqlConnection con = new SqlConnection(conString))
                {
                    con.Open();

                    lblStatus.Text = "Writing Temporary Table to database"; lblStatus.Refresh();
                    //Execute the command to make a temp table
                    SqlCommand cmd = new SqlCommand(TemporaryTableCreateStatement, con);
                    cmd.ExecuteNonQuery();

                    //BulkCopy the data in the DataTable to the temp table
                    using (SqlBulkCopy bulk = new SqlBulkCopy(con))
                    {
                        bulk.DestinationTableName = "#RPMImportProducts";
                        bulk.WriteToServer(TemporaryTable);
                    }

                    if (!Preview)
                    {
                        lblStatus.Text = "Merging Temporary Table into Products"; lblStatus.Refresh();
                        //Now use the merge command to upsert from the temp table to the production table
                        string MergeSqlStatement = "MERGE INTO Product AS Target " +
                                                   "USING #RPMImportProducts AS Source " +
                                                   "ON " +
                                                   "Target.UPI=Source.UPI " +
                                                   "WHEN MATCHED THEN " +
                                                   "UPDATE SET Target.Price=Source.Price " +
                                                   "WHEN NOT MATCHED THEN " +
                                                   "INSERT (UPI,Price,Name,CoreProduct,CustomString1) VALUES (Source.UPI,Source.Price,Source.Name,0,Source.CustomString1);";

                        cmd.CommandText = MergeSqlStatement;
                        cmd.ExecuteNonQuery();
                    }

                    lblStatus.Text = "Removing Temporary Table"; lblStatus.Refresh();
                    //Clean up the temp table
                    cmd.CommandText = "drop table #RPMImportProducts";
                    cmd.ExecuteNonQuery();

                    // Auditing

                    lblStatus.Text = "Creating Temporary Table (Audit)"; lblStatus.Refresh();

                    // For speed, we now do a Bulk upsert.
                    //Make a temp table in sql server that matches our production table
                    string TemporaryAuditTableCreateStatement = "CREATE TABLE #RPMImportAudit([Description] [nvarchar](max) NULL,[TableName] [nvarchar](max) NULL,[FieldName] [nvarchar](max) NULL, [RecordID] [int] NULL, [Username] [nvarchar](max) NULL, [PreviousValue] [nvarchar](max) NULL, [NewValue] [nvarchar](max) NULL, [ApplicationName] [nvarchar](max) NULL)";

                    //Create a datatable that matches the temp table exactly. (WARNING: order of columns must match the order in the table)
                    DataTable TemporaryAuditTable = new DataTable();
                    TemporaryAuditTable.Columns.Add(new DataColumn("Description", typeof(string)));
                    TemporaryAuditTable.Columns.Add(new DataColumn("TableName", typeof(string)));
                    TemporaryAuditTable.Columns.Add(new DataColumn("FieldName", typeof(string)));
                    TemporaryAuditTable.Columns.Add(new DataColumn("RecordID", typeof(Int32)));
                    TemporaryAuditTable.Columns.Add(new DataColumn("Username", typeof(string)));
                    TemporaryAuditTable.Columns.Add(new DataColumn("PreviousValue", typeof(string)));
                    TemporaryAuditTable.Columns.Add(new DataColumn("NewValue", typeof(string)));
                    TemporaryAuditTable.Columns.Add(new DataColumn("ApplicationName", typeof(string)));

                    lblStatus.Text = "Adding Audit data to Temporary Table"; lblStatus.Refresh();

                    //Add prices in our list to our DataTable
                    foreach (Product product in _NewAndUpdatedProducts)
                    {
                        DataRow row = TemporaryAuditTable.NewRow();
                        if (product.OldPrice == 0.0M || product.OldPrice == 0M)
                        {
                            row["Description"] = "Import product from RPM";
                        }
                        else
                        {
                            row["Description"] = "Update price from RPM";
                        }
                        row["TableName"] = "Product";
                        row["FieldName"] = "Price";
                        row["RecordID"] = product.ProductID;
                        row["Username"] = "-";
                        row["PreviousValue"] = product.OldPrice;
                        row["NewValue"] = product.NewPrice;
                        row["ApplicationName"] = Application.ProductName;

                        TemporaryAuditTable.Rows.Add(row);
                    }

                    lblStatus.Text = "Connecting to Auditing database"; lblStatus.Refresh();
                    //Connect to DB
                    conString = _DestinationConnectionString;
                    using (SqlConnection AuditConnection = new SqlConnection(conString))
                    {
                        AuditConnection.Open();

                        lblStatus.Text = "Writing Temporary Table to database"; lblStatus.Refresh();
                        //Execute the command to make a temp table
                        SqlCommand AuditCmd = new SqlCommand(TemporaryAuditTableCreateStatement, AuditConnection);
                        AuditCmd.ExecuteNonQuery();

                        //BulkCopy the data in the DataTable to the temp table
                        using (SqlBulkCopy bulk = new SqlBulkCopy(AuditConnection))
                        {
                            bulk.DestinationTableName = "#RPMImportAudit";
                            bulk.WriteToServer(TemporaryAuditTable);
                        }

                        if (!Preview)
                        {
                            lblStatus.Text = "Merging Temporary Table into Audit"; lblStatus.Refresh();
                            //Now use the merge command to upsert from the temp table to the production table
                            string MergeSqlStatement = "MERGE INTO Audit AS Target " +
                                                       "USING #RPMImportAudit AS Source " +
                                                       "ON " +
                                                       "Target.RecordID=-1 " + // Force it to always insert
                                                       "WHEN MATCHED THEN " +
                                                       "UPDATE SET Target.Username='' " +
                                                       "WHEN NOT MATCHED THEN " +
                                                       "INSERT (Description,TableName,FieldName,RecordID,Username,PreviousValue,NewValue,ApplicationName) VALUES (Source.Description,Source.TableName,Source.FieldName,Source.RecordID,Source.Username,Source.PreviousValue,Source.NewValue,Source.ApplicationName);";

                            AuditCmd.CommandText = MergeSqlStatement;
                            AuditCmd.ExecuteNonQuery();
                        }

                        lblStatus.Text = "Removing Temporary Audit Table"; lblStatus.Refresh();
                        //Clean up the temp table
                        AuditCmd.CommandText = "drop table #RPMImportAudit";
                        AuditCmd.ExecuteNonQuery();
                    }


                    lblStatus.Text = "Cleaning up"; lblStatus.Refresh();
                    // =================================================================================
                    // Cleanup
                    SourceDataAdapter.Dispose();

                    SourceConnection.Close();
                    SourceConnection.Dispose();

                    DestinationConnection.Close();
                    DestinationConnection.Dispose();

                    lblStatus.Text = "Idle"; lblStatus.Refresh();
                }
            }
            catch (Exception ex)
            {
                Console.Write("Error! " + ex.InnerException);
            }

            Cursor.Current = Cursors.Default;
        }

        private void Audit(string Description, string TableName, string FieldName, int RecordID, string Username, string PreviousValue, string NewValue, string ApplicationName, bool OverrideOptions)
        {

            string Query = "INSERT INTO audit (Description, TableName, FieldName, RecordID, Username, PreviousValue, NewValue, ApplicationName) VALUES ('" + Description + "', '" + TableName + "', '" + FieldName + "'," + RecordID + ",'" + Username + "','" + PreviousValue + "','" + NewValue + "','" + ApplicationName + "' )";

            if (NewValue != PreviousValue) Core.SQL.Functions.Execute(Query, _DestinationConnectionString);
        }
    }

    public class Product
    {
        public int RPMID { get; set; }
        public int ProductID { get; set; }
        public int UPI { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string Name { get; set; }
        public bool CoreProduct { get; set; }

        public Product()
        {
            RPMID = 0;
            ProductID = 0;
            UPI = 0;
            OldPrice = 0.0M;
            NewPrice = 0.0M;
            Name = "";
            CoreProduct = false;
        }
    }

    public class AuditDetails
    {
        public string Description { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public int RecordID { get; set; }
        public string UserName { get; set; }
        public string PreviousValue { get; set; }
        public string NewValue { get; set; }
        public string ApplicationName { get; set; }

        public AuditDetails()
        {
            Description = "";
            TableName = "";
            FieldName = "";
            RecordID = 0;
            UserName = "";
            PreviousValue = "";
            NewValue = "";
            ApplicationName = Application.ProductName;
        }
    }
}
