using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MoviesManagementSystem
{
    /// <summary>
    /// Manages the main application context and form switching.
    /// </summary>
    public class MainFormManager : ApplicationContext
    {
        protected bool exitAppOnClose;

        public Form CurrentForm
        {
            get { return MainForm; }
            set
            {
                if (MainForm != null)
                {
                    // close the current form, but don't exit the application
                    exitAppOnClose = false;
                    MainForm.Close();
                    exitAppOnClose = true;
                }
                // switch to the new form
                MainForm = value;
                MainForm.Show();
            }
        }

        public MainFormManager()
        {
            exitAppOnClose = true;
        }

        // when a form is closed, don't exit the application if this is a swap
        protected override void OnMainFormClosed(object sender, EventArgs e)
        {
            if (exitAppOnClose)
            {
                base.OnMainFormClosed(sender, e);
            }
        }
    }
    internal class Program
    {
        private static MainFormManager mainFormManager;

        public static MainFormManager MainFormManager
        {
            get { return mainFormManager; }
        }

        public Program()
        {
            mainFormManager = new MainFormManager();

            mainFormManager.CurrentForm = new Login();
            Application.Run(mainFormManager);
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new Program();
        }
    }
    /// <summary>
    /// Provides database connection information.
    /// </summary>
    public static class Connection
    {
        public static readonly MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
        {
            Server = "127.0.0.1",
            UserID = "root",
            Password = "53037636",
            Database = "movies",
        };
        public static readonly string connectionString = "Server=127.0.0.1; User ID=root; Password=53037636; Database=movies";
    }
    /// <summary>
    /// Stores user login information.
    /// </summary>
    public static class LoginInfo
    {
        public static string UserID { get; set;}
        public static bool IsAdmin = false;
    }


}
