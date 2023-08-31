using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using MySqlConnector;
using System.Drawing;

namespace MoviesManagementSystem
{
    /// <summary>
    /// Represents the administrator interface for managing data in the Movies Management System.
    /// </summary>
    public partial class Admin : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Admin"/> class.
        /// </summary>
        public Admin()
        {
            InitializeComponent();
            this.Paint += PaintBorder;
            UserName.Text = LoginInfo.UserID.ToString();
        }
        /// <summary>
        /// Event handler for the Browse button click, allowing the administrator to select and upload CSV/TSV files to the database.
        /// </summary>
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "*.csv|*.tsv";
            DialogResult result = openFileDialog.ShowDialog();
            string[] filenames;
            if (result == DialogResult.OK)
            {
                filenames = openFileDialog.FileNames;
                foreach (var file in filenames)
                {
                    Filenames.Text += RetrieveTableName(file) + ' ';
                }
                if (filenames != null && filenames.Length > 0)
                {
                    BrowseButton.Enabled = false;
                    UploadFilesToDatabase(filenames);
                    BrowseButton.Enabled = true;
                }
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        /// <summary>
        /// Event handler for the Log Out button click, which returns to the login screen.
        /// </summary>
        private void LogOut_Click(object sender, EventArgs e)
        {
            Program.MainFormManager.CurrentForm = new Login();
        }

        /// <summary>
        /// Uploads the selected CSV/TSV files to the database.
        /// </summary>
        /// <param name="filenames">An array of file paths to the selected files.</param>
        private void UploadFilesToDatabase(string[] filenames)
        {
            Connection.builder.AllowLoadLocalInfile = true;
            for (int i = 0; i < filenames.Length; i++)
            {
                if (File.Exists(filenames[i]))
                {
                    using (TextFieldParser tfp = new TextFieldParser(filenames[i]))
                    {
                        using (MySqlConnection connection = new MySqlConnection(Connection.builder.ConnectionString))
                        {
                            connection.Open();
                            Filenames.Text = "Uploading " + RetrieveTableName(filenames[i]) + " to database...";
                            //Configure everything to upload to the database via bulk copy.
                            MySqlBulkCopy sbc = new MySqlBulkCopy(connection);
                            {
                                //Configure the bulk copy settings
                                sbc.DestinationTableName = RetrieveTableName(filenames[i]);
                                sbc.BulkCopyTimeout = 2000;
                                ProcessFile(tfp, filenames[i], connection, sbc);
                            }
                            Filenames.Text = "Uploaded successfully";
                            connection.Close();
                        }
                        tfp.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Failed to upload " + filenames[i] + " file");
                }
            }
        }
        /// <summary>
        /// Retrieves the table name from the given filename by parsing the file path.
        /// </summary>
        /// <param name="filename">The filename including the file extension.</param>
        /// <returns>The extracted table name.</returns>
        private string RetrieveTableName(string filename)
        {
            var parsed = filename.Split('/', '.', '\\');
            return parsed[parsed.Length - 2];
        }

        /// <summary>
        /// Processes the specified CSV/TSV file and inserts its data into the database.
        /// </summary>
        /// <param name="tfp">The TextFieldParser for reading the file.</param>
        /// <param name="file">The path to the file being processed.</param>
        /// <param name="connection">The MySQL database connection.</param>
        /// <param name="sbc">The MySQLBulkCopy object for bulk data insertion.</param>
        private void ProcessFile(TextFieldParser tfp, string file, MySqlConnection connection, MySqlBulkCopy sbc)
        {
            int batchSize = 5000;
            string[][] currentLines = new string[batchSize][];
            // must be the same as the database's name
            DataTable currentRecords = new DataTable(RetrieveTableName(file));
            var splitted = file.Split('.');
            string extention = splitted[splitted.Length - 1];
            char separator = extention == "csv" ? ';' : '\t';
            string[] columnNames = File.ReadLines(file).First().Split(separator); // lazy evaluation, reads only the first line

            int batchCount = 0; // The number of records currently processed for SQL bulk copy
            bool blnFileHasMoreLines = true;
            int intLineReadCounter = 0;
            object oSyncLock = new object();
            tfp.TextFieldType = FieldType.Delimited;
            tfp.Delimiters = extention == "csv" ? new string[] { ";" } : new string[] { "\t" };
            tfp.HasFieldsEnclosedInQuotes = false;

            //Create the datatable with the column names of a given type
            for (int x = 0; x < columnNames.Length; x++)
                currentRecords.Columns.Add(columnNames[x], typeof(string));

            //create database table
            string cmd = "CREATE TABLE `movies`.`" + RetrieveTableName(file) + "` (";
            for (int i = 0; i < columnNames.Length; i++)
            {
                cmd += "`" + columnNames[i] + "` VARCHAR(100) NOT NULL ";
                if (i != columnNames.Length - 1)
                    cmd += ", ";
            }
            cmd += ") ENGINE = InnoDB;";
            MySqlCommand create = new MySqlCommand(cmd, connection);
            create.ExecuteScalar();
            while (blnFileHasMoreLines)
            {
                while (intLineReadCounter < batchSize && !tfp.EndOfData)
                {
                    currentLines[intLineReadCounter] = tfp.ReadFields();
                    intLineReadCounter += 1;
                    batchCount += 1;
                }
                //Process each line in parallel.
                Parallel.For(0, intLineReadCounter, x =>
                //for (int x=0; x < intLineReadCounter; x++)
                {
                    List<object> values = null;

                    values = new List<object>(currentRecords.Columns.Count);
                    for (int i = 0; i < currentLines[x].Length; i++)
                        values.Add(currentLines[x][i].ToString());
                    
                    lock (oSyncLock)
                    {
                        currentRecords.LoadDataRow(values.ToArray(), true);
                    }
                    values.Clear();
                });
                if (batchCount >= batchSize)
                {   // Do the SQL bulk copy and save the info into the database
                    sbc.WriteToServer(currentRecords);
                    batchCount = 0;
                    currentRecords.Clear();
                }
                if (intLineReadCounter < batchCount && currentLines[intLineReadCounter] == null)
                    blnFileHasMoreLines = false;

                intLineReadCounter = 0;
                Array.Clear(currentLines, 0, currentLines.Length);

            }
            // Write the rest
            if (currentRecords.Rows.Count > 0)
            {
                sbc.WriteToServer(currentRecords);
            }
            if (currentRecords != null)
                currentRecords.Clear();
            if (currentLines != null)
                Array.Clear(currentLines, 0, currentLines.Length);
            oSyncLock = null;
        }
        private void PaintBorder(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Color.Black, 3), this.DisplayRectangle);
        }

        private void Search_Click(object sender, EventArgs e)
        {
            Program.MainFormManager.CurrentForm = new MovieSearch();
        }
    }
}
