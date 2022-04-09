using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Login : Form
    {
        #region Params

        SqlConnection con = new SqlConnection("Data Source=DESKTOP-A0D2PVJ;Initial Catalog=Kursach;Persist Security Info=True;User ID=Kekw");

        bool isAdminForm = false;
        bool isWorkerForm = false;
        bool isHelperForm = false;

        string userName = "";
        int userID = 0;

        #endregion

        public Login()
        {
            InitializeComponent();
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

        private void OpenSignUpSection(object sender, EventArgs e)
        {
            SignUpOn();
            SignInOff();
        }

        private void OpenSignInSection(object sender, EventArgs e)
        {
            SignInOn();
            SignUpOff();
        }

        #endregion

        #region Backend

        private void SignUp(object sender, EventArgs e)
        {
            if (CheckRegistrationParams()) {
                string query1 = "Insert into Users (ID, Name, Login, Password, Admin, User1, User2)";
                query1 += " Select (Count(ID) + 1), @Name, @Login, @Password, 'False', 'False', 'True' from Users";
                SqlCommand cmd1 = new SqlCommand(query1, con);
                cmd1.Parameters.AddWithValue("@Name", nameTextBox1.Text);
                cmd1.Parameters.AddWithValue("@Login", loginTextBox2.Text);
                cmd1.Parameters.AddWithValue("@Password", passwordTextBox2.Text);
                con.Open();
                cmd1.ExecuteNonQuery();
                con.Close();
                SignInOn();
                SignUpOff();
            }
        }
        private void SignIn(object sender, EventArgs e)
        {
            string query = $"Select * from Users Where Login = '{loginTextBox1.Text}' AND Password = '{passwordTextBox1.Text}'";
            SqlCommand cmd = new SqlCommand(query, con);
            con.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            if (rd.HasRows)
            {
                rd.Read();
                userName = rd.GetString(1);
                userID = rd.GetInt32(0);
                isAdminForm = rd.GetBoolean(4);
                isWorkerForm = rd.GetBoolean(5);
                isHelperForm = rd.GetBoolean(6);
                string query1 = "Insert into ActionLog (ID, Datetime, UserID, Action) ";
                query1 += $"Select (Count(ID) + 1), @DateTime, @UserID, 'Logged in' from ActionLog";
                SqlCommand cmd1 = new SqlCommand(query1, con);
                cmd1.Parameters.AddWithValue("@DateTime", DateTime.Now);
                cmd1.Parameters.AddWithValue("@UserID", rd.GetInt32(0));
                rd.Close();
                cmd1.ExecuteNonQuery();
                con.Close();
            }
            else
            {
                incorrectLoginLabel.Visible = true;
                con.Close();
            }

            if (isAdminForm)
            {
                Admin adminForm = new Admin();
                adminForm.SetUserData(userName, userID);
                adminForm.Show();
                Hide();
            }
            else if (isWorkerForm)
            {
                Worker workerForm = new Worker();
                workerForm.SetUserData(userName, userID);
                workerForm.Show();
                Hide();
            }
            else if (isHelperForm)
            {
                Helper helperForm = new Helper();
                helperForm.SetUserData(userName, userID);
                helperForm.Show();
                Hide();
            }

        }

        #endregion

        #region Helper functions

        private void SignUpOn()
        {
            loginLabel2.Visible = true;
            loginTextBox2.Visible = true;
            nameLabel1.Visible = true;
            nameTextBox1.Visible = true;
            passwordLabel2.Visible = true;
            passwordTextBox2.Visible = true;
            signUpButton1.Visible = true;
            orlabel2.Visible = true;
            signInlabel2.Visible = true;
            signUplabel2.Visible = true;
        }
        private void SignUpOff()
        {
            loginLabel2.Visible = false;
            loginTextBox2.Visible = false;
            nameLabel1.Visible = false;
            nameTextBox1.Visible = false;
            passwordLabel2.Visible = false;
            passwordTextBox2.Visible = false;
            signUpButton1.Visible = false;
            orlabel2.Visible = false;
            signInlabel2.Visible = false;
            signUplabel2.Visible = false;
            RegistrationErrorsOff();
        }
        private void SignInOn()
        {
            signInlabel1.Visible = true;
            loginlabel1.Visible = true;
            loginTextBox1.Visible = true;
            passwordlabel1.Visible = true;
            passwordTextBox1.Visible = true;
            signInButton1.Visible = true;
            signUplabel1.Visible = true;
            signInlabel1.Visible = true;
            orlabel1.Visible = true;
        }
        private void SignInOff()
        {
            signInlabel1.Visible = false;
            loginlabel1.Visible = false;
            loginTextBox1.Visible = false;
            passwordlabel1.Visible = false;
            passwordTextBox1.Visible = false;
            signInButton1.Visible = false;
            signUplabel1.Visible = false;
            signInlabel1.Visible = false;
            orlabel1.Visible = false;
            incorrectLoginLabel.Visible = false;
        }
        private void RegistrationErrorsOff()
        {
            enterLoginLabel.Visible = false;
            enterNameLabel.Visible = false;
            enterPassLabel.Visible = false;
            incorrectPassLabel.Visible = false;
            loginExistsLabel.Visible = false;
        }
        private bool CheckRegistrationParams()
        {
            RegistrationErrorsOff();
            bool isDataCorrect = true;
            con.Close();
            if (loginTextBox2.Text == "")
            {
                enterLoginLabel.Visible = true;
                isDataCorrect = false;
            }
            else
            {
                string query = $"Select Login from Users Where Login = '{loginTextBox2.Text}'";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    rd.Read();
                    if (loginTextBox2.Text == rd.GetString(0))
                    {
                        loginExistsLabel.Visible = true;
                        isDataCorrect = false;
                    }
                }
                con.Close();
            }
            if (passwordTextBox2.Text == "")
            {
                enterPassLabel.Visible = true;
                isDataCorrect = false;
            }
            else if (passwordTextBox2.Text.Length < 8)
            {
                incorrectPassLabel.Visible = true;
                isDataCorrect = false;
            }
            if (nameTextBox1.Text == "")
            {
                enterNameLabel.Visible = true;
                isDataCorrect = false;
            }

            return isDataCorrect;
        }

        #endregion

    }
}
    