using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Admin : Form
    {
        #region Params

        SqlConnection con = new SqlConnection("Data Source=DESKTOP-A0D2PVJ;Initial Catalog=Kursach;Persist Security Info=True;User ID=Kekw");
        SqlDataAdapter adap;
        DataSet ds;
        SqlCommandBuilder cmdb;

        string userName = "";
        int userID = 0;
        string currentTable = "";

        #endregion

        public Admin()
        {
            InitializeComponent();
            Bunifu.Utils.ScrollbarBinder.BindDatagridView(userDataGridView, userDataScrollBar1);
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.WindowsShutDown) return;
        }

        #region Frontend

        private void CloseApp(object sender, EventArgs e)
        {
            Close();
        }
        private void MinimizeApp(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void OpenActionLogTab(object sender, EventArgs e)
        {
            ActionLogOn();
            ChangeFilter("ActionLog");
            LoadTable("ActionLog");
        }
        private void OpenUserDatabaseTab(object sender, EventArgs e)
        {
            UserDataBaseOn();
            ChangeFilter("Users");
            LoadTable("Users");
        }
        private void OpenAllTablesTab(object sender, EventArgs e)
        {
            AllTablesTabOn();
            ChangeFilter(tableDropdown.Text);
            LoadTable(tableDropdown.Text);
        }
        private void OpenMakeRequestTab(object sender, EventArgs e)
        {
            MakeRequestOn();
        }
        private void OpenMakeReportTabButton(object sender, EventArgs e)
        {
            con.Open();
            adap = new SqlDataAdapter($"Select Number as 'Repair number', [Date], Brand as 'Car brand', Model as 'Car model', malfunction.[Name] as 'Malfunction', worker.[Name] as 'Worker name' From repairList, car, worker, malfunction Where car.ID = CarID AND worker.ID = WorkerID AND malfunction.ID = repairList.MalfunctionID ", con);
            ds = new System.Data.DataSet();
            adap.Fill(ds);
            userDataGridView.DataSource = ds.Tables[0];
            con.Close();
            if (ds.Tables[0].Rows.Count == 0)
            {
                noResultsLabel.Visible = true;
            }
            MakeReportON();
        }

        #endregion

        #region Backend

        public void SetUserData(string userName, int userID)
        {
            this.userName = userName;
            this.userID = userID;
        }
        private void Admin_Load_1(object sender, EventArgs e)
        {
            nameLabel.Text = userName;
            LoadTable("Users");
        }
        private void UpdateDataBase(object sender, EventArgs e)
        {
            string query1 = "Insert into ActionLog (ID, Datetime, UserID, Action) ";
            query1 += $"Select (Count(ID) + 1), @DateTime, @UserID, 'Updated rows in {currentTable}' from ActionLog";
            SqlCommand cmd1 = new SqlCommand(query1, con);
            cmd1.Parameters.AddWithValue("@DateTime", DateTime.Now);
            cmd1.Parameters.AddWithValue("@UserID", userID);
            con.Open();
            cmd1.ExecuteNonQuery();
            con.Close();
            cmdb = new SqlCommandBuilder(adap);
                adap.Update(ds);
                ds.AcceptChanges();
                ds.Clear();
                adap.Fill(ds);
        }
        private void DeleteRows(object sender, EventArgs e)
        {
            string query1 = "Insert into ActionLog (ID, Datetime, UserID, Action) ";
            query1 += $"Select (Count(ID) + 1), @DateTime, @UserID, 'Deleted rows from {currentTable}' from ActionLog";
            SqlCommand cmd1 = new SqlCommand(query1, con);
            cmd1.Parameters.AddWithValue("@DateTime", DateTime.Now);
            cmd1.Parameters.AddWithValue("@UserID", userID);
            con.Open();
            cmd1.ExecuteNonQuery();
            con.Close();
            foreach (DataGridViewRow item in this.userDataGridView.SelectedRows)
            {
                userDataGridView.Rows.RemoveAt(item.Index);
            }
            cmdb = new SqlCommandBuilder(adap);
            adap.Update(ds);
        }
        private void FilterDataGrid(object sender, EventArgs e)
        {
            noResultsLabel.Visible = false;
            DataView dv = ds.Tables[0].DefaultView;
            dv.RowFilter = $"Convert({filterDropdown.Text}, System.String) LIKE '%{filterTextBox.Text}%'";
            userDataGridView.DataSource = dv;
            if (dv.Count == 0)
            {
                noResultsLabel.Visible = true;
            }
        }
        private void ChangeTable(object sender, EventArgs e)
        {
            noResultsLabel.Visible = false;
            ChangeFilter(tableDropdown.Text);
            LoadTable(tableDropdown.Text);
        }
        private void ChangeRequest(object sender, EventArgs e)
        {
            label3.Visible = false;
            switch (requestDropdown.Text)
            {
                case "Get cars by dates":
                    param1TextBox.Visible = true;
                    param2TextBox.Visible = true;
                    break;
                case "Get malfunction by prices":
                    param1TextBox.Visible = true;
                    param2TextBox.Visible = true;
                    break;
                case "Get car count by dates":
                    param1TextBox.Visible = true;
                    param2TextBox.Visible = true;
                    break;
                case "Get total workers salary":
                    param1TextBox.Visible = false;
                    param2TextBox.Visible = false;
                    break;
                case "Get worker count by cars":
                    param1TextBox.Visible = false;
                    param2TextBox.Visible = false;
                    break;
                case "Get malfunction count by cars":
                    param1TextBox.Visible = false;
                    param2TextBox.Visible = false;
                    break;
                case "Max malfunction price":
                    param1TextBox.Visible = false;
                    param2TextBox.Visible = false;
                    break;
                case "Car list with comments":
                    param1TextBox.Visible = false;
                    param2TextBox.Visible = false;
                    break;
                case "Worker list with comments":
                    param1TextBox.Visible = false;
                    param2TextBox.Visible = false;
                    break;
                default:
                    param1TextBox.Visible = true;
                    param2TextBox.Visible = false;
                    break;
            }
        }
        private void MakeRequest(object sender, EventArgs e)
        {
            label3.Visible = false;
            string query = "";
            switch (requestDropdown.Text)
            {
                case "Get cars by dates":
                    query = $"Select car.ID as 'CarID', [Date], Model, Brand From repairList, car Where[Date] Between '{param1TextBox.Text}' AND '{param2TextBox.Text}' AND CarID = car.ID";
                    ExecuteRequest(query);
                    break;
                case "Get malfunction by prices":
                    query = $"Select Name, Price From malfunction Where price Between '{param1TextBox.Text}' and '{param2TextBox.Text}'";
                    ExecuteRequest(query);
                    break;
                case "Get car count by dates":
                    query = $"Select Count(*) as 'Amount of repairs' From repairList Where Date Between '{param1TextBox.Text}' and '{param2TextBox.Text}'";
                    ExecuteRequest(query);
                    break;
                case "Get total workers salary":
                    query = $"Select Sum(Salary) as 'Total salary' From Worker";
                    ExecuteRequest(query);
                    break;
                case "Get worker count by cars":
                    query = $"Select CarID, Count(WorkerID) as 'Amount of workers' From repairList Group by CarID";
                    ExecuteRequest(query);
                    break;
                case "Get malfunction count by cars":
                    query = $"Select CarID, Count(MalfunctionID) as 'Amount of malfunctions' From repairList Group by CarID";
                    ExecuteRequest(query);
                    break;
                case "Max malfunction price":
                    query = $"Select Name, Price From malfunction Where Price >= ALL( Select Price from malfunction )";
                    ExecuteRequest(query);
                    break;
                case "Car list with comments":
                    query = $"Select ID, Brand, Model, ProductionYear, 'Has two or more malfunctions' as 'Comment' From car Where( Select Count(*) From repairList where CarID = car.ID ) >= 2 Union Select ID, Brand, Model, ProductionYear, 'The name of the owner starts with D' From car Where OwnerID in ( Select ID From[owner] Where[Name] Like 'D%' ) Union Select ID, Brand, Model, ProductionYear, 'Has been produced in year 2019' From car Where ProductionYear = 2019 Order by id";
                    ExecuteRequest(query);
                    break;
                case "Worker list with comments":
                    query = $"Select ID, Name, Salary, 'Has a salary of 10000 or more' as 'Comment' From Worker Where Salary >= 10000 Union Select ID, Name, Salary, 'Is working on two or more cars' From Worker Where( Select COUNT(*) From repairList Where WorkerID = Worker.ID ) >= 2 Union Select ID, Name, Salary, 'Name starts with letter I' From Worker Where Name Like 'I%' Order by ID";
                    ExecuteRequest(query);
                    break;
                case "Get workers by car":
                    query = $"Select worker.[Name], CarID From repairList, worker Where CarID = '{param1TextBox.Text}' AND worker.ID = repairList.WorkerID";
                    ExecuteRequest(query);
                    break;
                case "Get malfunctions by car":
                    query = $"Select malfunction.[Name], CarID From repairList, malfunction Where CarID = '{param1TextBox.Text}' AND malfunction.ID = repairList.MalfunctionID";
                    ExecuteRequest(query);
                    break;
                case "Get owners by letter":
                    query = $"Select ID, [Name] From owner Where[Name] Like '{param1TextBox.Text}%'";
                    ExecuteRequest(query);
                    break;
                case "Get workers by letter":
                    query = $"Select ID, [Name] From Worker Where Name Like '{param1TextBox.Text}%'";
                    ExecuteRequest(query);
                    break;
                case "Worker salary 10000+":
                    query = $"Select Name, Salary From Worker Where Salary = ANY( Select Salary From Worker Where Salary > '{param1TextBox.Text}')";
                    ExecuteRequest(query);
                    break;
                case "Get cars by worker count":
                    query = $"Select car.ID, car.Model, car.Brand From car Where( Select count(*) From repairList Where CarID = car.id ) >= '{param1TextBox.Text}'";
                    ExecuteRequest(query);
                    break;
                case "Get cars by malfunction count":
                    query = $"Select car.ID, car.Model, car.Brand From car Where( Select count(*) From repairList Where CarID = car.id ) >= '{param1TextBox.Text}'";
                    ExecuteRequest(query);
                    break;
                case "Cars not exist by year":
                    query = $"Select car.ID, car.Model, car.Brand From car Where exists( Select* From repairList Where YEAR([Date]) != '{param1TextBox.Text}' and CarID = car.ID )";
                    ExecuteRequest(query);
                    break;
                case "Add comment to workers by price":
                    query = $"Update Worker Set Comment = 'Has salary more than {param1TextBox.Text}' Where Salary >= '{param1TextBox.Text}'";
                    con.Open();
                    SqlCommand cmd = new SqlCommand(query, con);
                    int i = cmd.ExecuteNonQuery();
                    if (i > 0)
                    {
                        label3.Visible = true;
                    }
                    else
                    {
                        con.Close();
                        break;
                    }
                    con.Close();
                    break;
                case "Add comment to cars by malfunction":
                    query = $"Update Car Set Comment = 'Has {param1TextBox.Text} or more malfunctions' Where( Select Count(*) From carMalfunction Where car_id = Car.ID ) >= '{param1TextBox.Text}'";
                    con.Open();
                    SqlCommand cmd1 = new SqlCommand(query, con);
                    int i1 = cmd1.ExecuteNonQuery();
                    if (i1 > 0)
                    {
                        label3.Visible = true;
                    }
                    else
                    {
                        con.Close();
                        break;
                    }
                    con.Close();
                    break;
            }
        }
        private void ExecuteRequest(string query)
        {
            string query1 = "Insert into ActionLog (ID, Datetime, UserID, Action) ";
            query1 += $"Select (Count(ID) + 1), @DateTime, @UserID, 'Made request({requestDropdown.Text})' from ActionLog";
            SqlCommand cmd1 = new SqlCommand(query1, con);
            cmd1.Parameters.AddWithValue("@DateTime", DateTime.Now);
            cmd1.Parameters.AddWithValue("@UserID", userID);
            con.Open();
            cmd1.ExecuteNonQuery();
            con.Close();
            noResultsLabel.Visible = false;
            con.Open();
            adap = new SqlDataAdapter(query, con);
            ds = new System.Data.DataSet();
            adap.Fill(ds);
            userDataGridView.DataSource = ds.Tables[0];
            con.Close();
            if (ds.Tables[0].Rows.Count == 0)
            {
                noResultsLabel.Visible = true;
            }
        }
        private void LoadTable(string tableName)
        {
            currentTable = tableName;
            con.Open();
            adap = new SqlDataAdapter($"Select * from {tableName}", con);
            ds = new System.Data.DataSet();
            adap.Fill(ds);
            userDataGridView.DataSource = ds.Tables[0];
            con.Close();
            if (ds.Tables[0].Rows.Count == 0)
            {
                noResultsLabel.Visible = true;
            }
        }
        private void ChangeFilter(string tableName)
        {
            filterDropdown.Items.Clear();
            string query = $"USE Kursach SELECT COLUMN_NAME,* FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND TABLE_SCHEMA = 'dbo'";
            SqlCommand cmd = new SqlCommand(query, con);
            con.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            if (rd.HasRows)
            {
                rd.Read();
                filterDropdown.Text = rd.GetString(0);
                while (rd.Read())
                {
                    filterDropdown.Items.Add(rd.GetString(0));
                }
            }
            con.Close();
        }
        private void ChangeReport(object sender, EventArgs e)
        {
            switch (reportDropdown.Text)
            {
                case "All repairs report":
                    con.Open();
                    adap = new SqlDataAdapter($"Select Number as 'Repair number', [Date], Brand as 'Car brand', Model as 'Car model', malfunction.[Name] as 'Malfunction', worker.[Name] as 'Worker name' From repairList, car, worker, malfunction Where car.ID = CarID AND worker.ID = WorkerID AND malfunction.ID = repairList.MalfunctionID ", con);
                    ds = new System.Data.DataSet();
                    adap.Fill(ds);
                    userDataGridView.DataSource = ds.Tables[0];
                    con.Close();
                    if (ds.Tables[0].Rows.Count == 0)
                    {
                        noResultsLabel.Visible = true;
                    }
                    break;
                case "All owners report":
                    con.Open();
                    adap = new SqlDataAdapter($"Select [Name] as 'Owner name', TotalPayment as 'Total payment', COUNT(repairList.CarID) as 'Amount of visits' From[owner], repairList, car Where car.OwnerID = owner.ID AND repairList.CarID = car.ID Group by owner.Name, owner.TotalPayment", con);
                    ds = new System.Data.DataSet();
                    adap.Fill(ds);
                    userDataGridView.DataSource = ds.Tables[0];
                    con.Close();
                    if (ds.Tables[0].Rows.Count == 0)
                    {
                        noResultsLabel.Visible = true;
                    }
                    break;
            }
        }
        private void GeneratePDF(object sender, EventArgs e)
        {
            string query1 = "Insert into ActionLog (ID, Datetime, UserID, Action) ";
            query1 += $"Select (Count(ID) + 1), @DateTime, @UserID, 'Generated PDF report' from ActionLog";
            SqlCommand cmd1 = new SqlCommand(query1, con);
            cmd1.Parameters.AddWithValue("@DateTime", DateTime.Now);
            cmd1.Parameters.AddWithValue("@UserID", userID);
            con.Open();
            cmd1.ExecuteNonQuery();
            con.Close();
            if (userDataGridView.Rows.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = "Output.pdf";
                bool fileError = false;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(sfd.FileName))
                    {
                        try
                        {
                            File.Delete(sfd.FileName);
                        }
                        catch (IOException ex)
                        {
                            fileError = true;
                            MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                        }
                    }
                    if (!fileError)
                    {
                        try
                        {
                            PdfPTable pdfTable = new PdfPTable(userDataGridView.Columns.Count);
                            pdfTable.DefaultCell.Padding = 3;
                            pdfTable.WidthPercentage = 100;
                            pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;

                            foreach (DataGridViewColumn column in userDataGridView.Columns)
                            {
                                PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText));
                                pdfTable.AddCell(cell);
                            }

                            foreach (DataGridViewRow row in userDataGridView.Rows)
                            {
                                foreach (DataGridViewCell cell in row.Cells)
                                {
                                    pdfTable.AddCell(cell.Value.ToString());
                                }
                            }

                            using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                            {
                                Document pdfDoc = new Document(PageSize.A4, 10f, 20f, 20f, 10f);
                                PdfWriter.GetInstance(pdfDoc, stream);
                                pdfDoc.Open();
                                pdfDoc.Add(pdfTable);
                                pdfDoc.Close();
                                stream.Close();
                            }

                            MessageBox.Show("Data Exported Successfully !!!", "Info");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error :" + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("No Record To Export !!!", "Info");
            }
        }
        #endregion

        #region Helper functions

        private void UserDataBaseOn()
        {
            filterDropdown.Visible = true;
            filterTextBox.Visible = true;
            userDataGridView.AllowUserToAddRows = true;
            userDataGridView.AllowUserToDeleteRows = true;
            generatePDFButton.Visible = false;
            reportDropdown.Visible = false;
            userDataGridView.ReadOnly = false;
            noResultsLabel.Visible = false;
            tableDropdown.Visible = false;
            MakeRequestOff();
            deleteButton.Visible = true;
            updateButton.Visible = true;
        }
        private void MakeRequestOff()
        {
            makeRequestButton.Visible = false;
            filterDropdown.Visible = true;
            requestDropdown.Visible = false;
            param1TextBox.Visible = false;
            param2TextBox.Visible = false;
        }
        private void MakeReportON()
        {
            reportDropdown.Visible = true;
            generatePDFButton.Visible = true;
            userDataGridView.ReadOnly = false;
            noResultsLabel.Visible = false;
            tableDropdown.Visible = false;
            userDataGridView.AllowUserToAddRows = false;
            userDataGridView.AllowUserToDeleteRows = false;
            MakeRequestOff();
            deleteButton.Visible = false;
            updateButton.Visible = false;
            filterDropdown.Visible = false;
            filterTextBox.Visible = false;
        }
        private void AllTablesTabOn()
        {
            filterDropdown.Visible = true;
            filterTextBox.Visible = true;
            generatePDFButton.Visible = false;
            reportDropdown.Visible = false;
            userDataGridView.ReadOnly = false;
            noResultsLabel.Visible = false;
            tableDropdown.Visible = true;
            MakeRequestOff();
            deleteButton.Visible = true;
            updateButton.Visible = true;
            userDataGridView.AllowUserToAddRows = true;
            userDataGridView.AllowUserToDeleteRows = true;
        }
        private void ActionLogOn()
        {
            filterDropdown.Visible = true;
            filterTextBox.Visible = true;
            generatePDFButton.Visible = false;
            reportDropdown.Visible = false;
            userDataGridView.ReadOnly = true;
            noResultsLabel.Visible = false;
            tableDropdown.Visible = false;
            filterTextBox.Visible = true;
            MakeRequestOff();
            deleteButton.Visible = false;
            updateButton.Visible = false;
        }
        private void MakeRequestOn()
        {
            generatePDFButton.Visible = false;
            reportDropdown.Visible = false;
            makeRequestButton.Visible = true;
            userDataGridView.ReadOnly = true;
            tableDropdown.Visible = false;
            filterDropdown.Visible = false;
            filterTextBox.Visible = false;
            requestDropdown.Visible = true;
            param1TextBox.Visible = true;
            param2TextBox.Visible = true;
            deleteButton.Visible = false;
            updateButton.Visible = false;
        }

        #endregion

    }
}
