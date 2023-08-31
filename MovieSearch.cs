using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MoviesManagementSystem
{
    /// <summary>
    /// Represents the form for searching movies in the Movies Management System.
    /// </summary>
    public partial class MovieSearch : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MovieSearch"/> class.
        /// </summary>
        public MovieSearch()
        {
            InitializeComponent();
            this.Paint += PaintBorder;
            Admin.Visible = LoginInfo.IsAdmin ? true : false;
            Admin.Text = LoginInfo.UserID;
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        /// <summary>
        /// Event handler for searching based on the user input and selected filters.
        /// Retrieves search criteria and builds the SQL command.
        /// </summary>
        private void SearchButton_Click(object sender, EventArgs e)
        {
            var search = Search.Text;
            var sortBy = SortBy.Controls.OfType<RadioButton>().Where(x => x.Checked).First();
            var titleTypes = TitleType.Controls.OfType<CheckBox>().Where(x => x.Checked).ToList();
            var genres = Genres.Controls.OfType<CheckBox>().Where(x => x.Checked).Select(x => x.Text).ToList();
            genres.Sort();
            var adult = Adult.Controls.OfType<RadioButton>().Where(x => x.Checked).First();
            var isAdult = adult.Text == "Include" ? "1" : "0";
            var fromDate = FromDate.Text; // year only
            var toDate = ToDate.Text;
            var fromRating = FromRaiting.Text;
            var toRating = ToRaiting.Text;
            var fromVotes = FromVotes.Text;
            var toVotes = ToVotes.Text;
            var fromTime = FromTime.Text;
            var toTime = ToTime.Text;
            string command = "SELECT `title`.tconst, primaryTitle FROM movies.`title` LEFT JOIN movies.`ratings` ON title.tconst = ratings.tconst WHERE MATCH(primaryTitle) AGAINST(@search IN BOOLEAN MODE)";
            bool noErrors = true;
            if (titleTypes.Any())
            {
                command += " AND titleType IN(";
                for (int i = 0; i < titleTypes.Count; i++)
                {
                    command += " '" + titleTypes[i].Text + "'";
                    if (i != titleTypes.Count - 1)
                        command += ",";
                }
                command += ")";
            }
            if (genres.Any())
            {
                command += " AND genres = '"; // sorted alphabetically
                string genresString = "";
                for (int i = 0; i < genres.Count; i++)
                {
                    genresString += genres[i];
                    if (i != genres.Count - 1)
                        genresString += ',';
                }
                command += genresString + "'";
            }
            if (isAdult == "0")
                command += " AND isAdult = " + isAdult;
            if (fromTime != String.Empty)
            {
                command += " AND runtimeMinutes+0 >= " + "'" + fromTime + "+0'";
            }
            if (toTime != String.Empty)
            {
                command += " AND runtimeMinutes+0 <= " + "'" + toTime + "+0'";
            }
            if (fromDate != String.Empty)
            {
                if (int.TryParse(fromDate, out _))
                    command += " AND startYear+0 >= " + "'" + fromDate + "+0'";
                else
                {
                    MessageBox.Show("Write the year in the correct format");
                    noErrors = false;
                }
            }
            if (toDate != String.Empty)
            {
                if (int.TryParse(toDate, out _))
                    command += " AND startYear+0 <= " + "'" + toDate + "+0'";
                else
                {
                    MessageBox.Show("Write the year in the correct format");
                    noErrors = false;
                }
            }           
            if (fromRating != String.Empty)
            {
                command += " AND ratings.averageRating+0 >= " + "'" + fromRating + "+0'";
            }
            if (toRating != String.Empty)
            {
                command += " AND ratings.averageRating+0 <= " + "'" + fromRating + "+0'";
            }
            if (fromVotes != String.Empty)
            {
                command += " AND ratings.numVotes+0 >= " + "'" + fromVotes + "+0'";
            }
            if (toVotes != String.Empty)
            {
                command += " AND ratings.numVotes+0 <= " + "'" + toVotes + "+0'";
            }
            if (sortBy.Text == "Rating")
                command += " ORDER BY ratings.averageRating+0 DESC;"; // +0 is used for casting varchar to a number
            else if (sortBy.Text == "Number of Votes")
                command += " ORDER BY ratings.numVotes+0 DESC;";
            else if (sortBy.Text == "A-Z")
                command += " ORDER BY title.primaryTitle ASC;";
            else if (sortBy.Text == "Release Date")
                command += " ORDER BY title.startYear+0 ASC;";
            if (search.Trim().Length > 0 && noErrors)
                Program.MainFormManager.CurrentForm = new SearchResult(command, search);
        }

        private void PaintBorder(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Color.Black, 3), this.DisplayRectangle);
        }

        private void Admin_Click(object sender, EventArgs e)
        {
            Program.MainFormManager.CurrentForm = new Admin();
        }
    }
}
