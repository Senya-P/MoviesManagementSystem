
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MoviesManagementSystem
{
    /// <summary>
    /// Represents the form for displaying search results in the Movies Management System.
    /// </summary>
    public partial class SearchResult : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResult"/> class.
        /// </summary>
        /// <param name="command">The SQL command used to retrieve search results.</param>
        /// <param name="search">The user input used for parameterized SQL.</param>
        public SearchResult(string command, string search)
        {
            InitializeComponent();
            this.Paint += PaintBorder;
            SaveResult.Cursor = Cursors.Hand;
            GoBack.Cursor = Cursors.Hand;
            using (MySqlConnection connection = new MySqlConnection(Connection.connectionString))
            {
                connection.Open();
                MySqlCommand mySqlCommand = new(command, connection);
                var searchParsed = search.Split();
                string searchString = "";
                foreach (var s in searchParsed)
                    searchString += " +" + s;   // used for Boolean Full-Text Searches, include all the words from user search
                mySqlCommand.Parameters.AddWithValue("@search", searchString);
                mySqlCommand.CommandTimeout = 120;
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(mySqlCommand))
                {
                    System.Data.DataSet ds = new System.Data.DataSet();
                    adapter.SelectCommand = mySqlCommand;
                    adapter.Fill(ds);
                    DGV.DataSource = ds.Tables[0];
                }
                connection.Close();
            }

            DGV.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            DGV.Columns[0].Visible = false; //tconst column

            DGV.ColumnHeadersVisible = false;
            DGV.AllowUserToAddRows = false;
            DGV.AllowUserToDeleteRows = false;
            DGV.EditMode = DataGridViewEditMode.EditProgrammatically;
            DGV.Cursor = Cursors.Hand;
            DGV.CellClick += new DataGridViewCellEventHandler(ShowDetailed);
            DGV.Show();

        }
        public SearchResult()
        {
            InitializeComponent();

        }
        /// <summary>
        /// Event handler for showing detailed movie information when a row in the DataGridView is clicked.
        /// </summary>
        void ShowDetailed(object sender, DataGridViewCellEventArgs e)
        {
            object o = DGV[0, e.RowIndex].Value;
            string id;
            if (o is not DBNull)
            {
                id = (string)o; //retrieve tconst
                var detailed = new MovieDetailed(id);
                detailed.ShowDialog();
                detailed.BringToFront();
            }

        }
        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        /// <summary>
        /// Event handler for the Go Back button click, which returns to the movie search form.
        /// </summary>
        private void GoBack_Click(object sender, EventArgs e)
        {
            Program.MainFormManager.CurrentForm = new MovieSearch();
        }
        /// <summary>
        /// Event handler for the Save Result button click, which allows users to save search results as a CSV or TSV file.
        /// </summary>
        private void SaveResult_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Tab-separated values|*.tsv|Comma-separated values|*.csv";
            dialog.Title = "Save  file";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.FileName != "")
                {
                    FileStream fs = (FileStream)dialog.OpenFile();
                    using (TextWriter tw = new StreamWriter(fs))
                    {
                        char separator;
                        if (dialog.FilterIndex == 1)
                            separator = '\t';
                        else
                            separator = ',';
                        tw.WriteLine("tconst" + separator + "primaryTitle");
                        for (int i = 0; i < DGV.Rows.Count - 1; i++)
                        {
                            for (int j = 0; j < DGV.Columns.Count; j++)
                            {
                                tw.Write($"{DGV.Rows[i].Cells[j].Value}");

                                if (j != DGV.Columns.Count - 1)
                                {
                                    tw.Write(separator);
                                }
                            }
                            tw.Write('\n');
                        }
                    }
                    fs.Close();
                }
            }
        }
        private void PaintBorder(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Color.Black, 3), this.DisplayRectangle);
        }
    }
}
