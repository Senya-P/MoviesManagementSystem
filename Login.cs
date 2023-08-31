using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MoviesManagementSystem
{
    /// <summary>
    /// Represents the login form for the Movies Management System.
    /// </summary>
    public partial class Login : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Login"/> class.
        /// </summary>
        public Login()
        {
            InitializeComponent();
            this.Paint += PaintBorder;
        }
        /// <summary>
        /// Event handler for the Login button. Gets login information and stores it in the LoginIfo class where it can be accessed.
        /// </summary>
        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (Username.Text == "" || Password.Text == "")
                MessageBox.Show("Username and password fields can't be empty");
            else
            {
                object obj;
                MySqlConnection connection = new MySqlConnection(Connection.builder.ConnectionString);

                connection.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT EXISTS (SELECT * FROM login WHERE `userId` = \"" + Username.Text + "\" AND `password` = \"" + Password.Text + "\")", connection);
                obj = cmd.ExecuteScalar();
                if (Convert.ToInt32(obj) > 0)   // Check if the user exists
                {
                    LoginInfo.UserID = Username.Text;
                    cmd = new MySqlCommand("SELECT `isAdmin` FROM login WHERE `userId` = \"" + Username.Text + "\"", connection);
                    obj = cmd.ExecuteScalar();
                    connection.Close();
                    if (Convert.ToInt32(obj) > 0)
                    {
                        LoginInfo.IsAdmin = true;
                    }
                    Program.MainFormManager.CurrentForm = new MovieSearch();
                }
                else
                {
                    MessageBox.Show("Username or password is incorrect");
                    connection.Close();
                }
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void PaintBorder(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Color.Black, 3), this.DisplayRectangle);
        }
    }
}
