using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
namespace MoviesManagementSystem
{
    /// <summary>
    /// Represents a detailed view of a movie in the Movies Management System.
    /// </summary>
    public partial class MovieDetailed : Form
    {
        private const int margin = 216;
        private string titleId;
        private List<Episode> episodesList = new();
        /// <summary>
        /// Initializes a new instance of the <see cref="MovieDetailed"/> class.
        /// </summary>
        /// <param name="titleId">The unique identifier of the movie to display.</param>
        public MovieDetailed(string titleId)
        {
            Debug.WriteLine("begin");
            this.titleId = titleId;
            InitializeComponent();
            this.Paint += PaintBorder;
            this.MouseDown += MovieDetailed_MouseDown;
            this.MouseMove += MovieDetailed_MouseMove;
            this.MouseUp += MovieDetailed_MouseUp;
            Save.Visible = false;
            Save.Cursor = Cursors.Hand;
            Delete.Visible = LoginInfo.IsAdmin ? true : false;
            Delete.Cursor = Cursors.Hand;
            ShowAll.Visible = false;
            using (MySqlConnection connection = new MySqlConnection(Connection.connectionString))
            {
                using (var db = new MoviesDataContext())
                {
                    connection.Open();

                    var title = (from t in db.title where t.tconst == titleId select t).Single();
                    var crew = (from c in db.crew where c.tconst == titleId select c).SingleOrDefault();
                    var rating = (from r in db.ratings where r.tconst == titleId select r).SingleOrDefault();
                    var episodes = (from e in db.episode where e.parentTconst == titleId select e).ToList();
                    var names = (from p in db.principals where p.tconst == titleId select p).ToList();

                    Point lastPoint = new Point(margin, Rating.Location.Y);
                    Title.Text = title.primaryTitle;
                    if (title.primaryTitle != title.originalTitle)
                        Title.Text += " (" + title.originalTitle + ")";
                    TitleType.Text = title.titleType;
                    Genres.Text = title.genres != "\\N" ? title.genres : "Unknown";
                    if (title.startYear == "\\N")
                        Years.Text = "Unknown year of release";
                    else
                    {
                        Years.Text = title.startYear;
                        if (title.endYear != "\\N" && title.startYear != title.endYear)
                            Years.Text += " - " + title.endYear;
                    }

                    Runtime.Text = title.runtimeMinutes != "\\N" && title.runtimeMinutes != null ? title.runtimeMinutes + " minutes" : "Unknown";
                    Rating.Text = rating != null ? rating.averageRating : "Insufficient data to calculate the rating";
                    Rating.Text += rating != null ? " / " + rating.numVotes + " votes" : "";

                    if (title.isAdult == "0")
                        Adult.Text = "";
                    else Adult.Text = "+18";

                    if (LoginInfo.IsAdmin)
                    {
                        Title.Click += EditLabel;
                        TitleType.Click += EditLabel;
                        Genres.Click += EditLabel;
                        Years.Click += EditLabel;
                        Runtime.Click += EditLabel;
                    }
                    if (crew != null)
                    {
                        var directors = crew.directors.Split(',');
                        lastPoint = new Point(margin, Directors.Location.Y);
                        foreach (var director in directors)
                        {
                            var name = GetName(director, db);
                            if (name != null)
                            {
                                Label label = new Label() { AutoSize = true };
                                label.Click += new EventHandler((sender, e) => ShowPerson(sender, e, director));
                                label.MaximumSize = new Size(150, 40);
                                label.Cursor = Cursors.Hand;
                                label.Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular);
                                label.Text = name;
                                label.Location = lastPoint;
                                lastPoint.X += label.Width + 50;
                                if (label.Location.X > this.Width - 50)
                                {
                                    label.Visible = false;
                                }
                                this.Controls.Add(label);
                            }
                        }

                        var writers = crew.writers.Split(',');
                        lastPoint = new Point(margin, Writers.Location.Y);
                        foreach (var writer in writers)
                        {
                            var name = GetName(writer, db);
                            if (name != null)
                            {
                                Label label = new Label() { AutoSize = true };
                                label.Click += new EventHandler((sender, e) => ShowPerson(sender, e, writer));
                                label.MaximumSize = new Size(150, 40);
                                label.Cursor = Cursors.Hand;
                                label.Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular);
                                label.Text = name;
                                label.Location = lastPoint;
                                lastPoint.X += label.Width + 50;
                                if (label.Location.X > this.Width - 50)
                                {
                                    lastPoint = new Point(margin, lastPoint.Y + 40);
                                    label.Location = lastPoint;
                                    lastPoint.X += label.Width + 50;
                                }
                                this.Controls.Add(label);
                            }
                        }
                    }

                    lastPoint = new Point(margin, lastPoint.Y + 40);
                    Stars.Location = new Point(Stars.Location.X, lastPoint.Y);
                    foreach (var name in names)
                    {
                        var primaryName = GetName(name.nconst, db);
                        if (primaryName != null)
                        {
                            Label label = new Label() { AutoSize = true };
                            label.Click += new EventHandler((sender, e) => ShowPerson(sender, e, name.nconst));
                            label.MaximumSize = new Size(500, 40);
                            label.Cursor = Cursors.Hand;
                            label.Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular);
                            label.Text = primaryName + " - " + name.category.Replace('_', ' ');
                            if (name.characters != "\\N")
                                label.Text += ", as " + String.Join("", name.characters.Split('[', ']', '"'));
                            label.Location = lastPoint;
                            lastPoint.X += 500;
                            if (lastPoint.X > margin + 500)
                            {
                                lastPoint = new Point(margin, lastPoint.Y + 30);
                            }
                            this.Controls.Add(label);
                        }
                        
                    }

                    lastPoint = new Point(margin, lastPoint.Y + 10);
                    Episodes.Location = new Point(Episodes.Location.X, lastPoint.Y);
                    
                    episodesList = episodes;
                    if (episodesList.Count() > 14)
                    {
                        ShowAll.Visible = true;
                        ShowAll.Cursor = Cursors.Hand;
                        ShowAll.Location = new Point(Episodes.Location.X, Episodes.Location.Y + 40);
                        ShowAll.Click += new EventHandler((sender, e) => ShowAllEpisodes(sender, e, episodesList, Title.Text));
                    }
                    foreach (var episode in episodes)
                    {
                        Label label = new Label() { AutoSize = true };
                        label.Click += new EventHandler((sender, e) => ShowEpisode(sender, e, episode.tconst));
                        label.Cursor = Cursors.Hand;
                        label.Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular);
                        label.Text = "S" + episode.seasonNumber + ".E" + episode.episodeNumber;
                        label.Location = lastPoint;
                        lastPoint.X += label.Width + 50;
                        if (label.Location.X > this.Width - 50)
                        {
                            lastPoint = new Point(margin, lastPoint.Y + 30);
                            label.Location = lastPoint;
                            lastPoint.X += label.Width + 50;
                        }
                        if (label.Location.Y > this.Height - 50)
                        {
                            label.Visible = false;
                        }
                        this.Controls.Add(label);
                    }
                    if (episodes.FirstOrDefault() == null)
                    {
                        Episodes.Visible = false;
                    }
                    
                    connection.Close();
                }
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Retrieves the primary name of a person based on their unique identifier.
        /// </summary>
        /// <param name="nconst">The unique identifier of the person.</param>
        /// <param name="db">The MoviesDataContext used for database operations.</param>
        /// <returns>The name of the person if exists, otherwise, null.</returns>
        private string GetName(string nconst, MoviesDataContext db)
        {
            var name = (from person in db.person where nconst == person.nconst select person.primaryName).SingleOrDefault();
            return name;

        }
        /// <summary>
        /// Event handler for displaying detailed information about an episode when a label is clicked.
        /// <param name="id">The unique identifier of the title (tconst).</param>
        /// </summary
        private void ShowEpisode(object sender, EventArgs e, string id)
        {
            var detailed = new MovieDetailed(id);
            detailed.ShowDialog();
            detailed.BringToFront();
        }
        /// <summary>
        /// Event handler for displaying detailed information about a person when a label is clicked.
        /// <param name="id">The unique identifier of the person (nconst).</param>
        /// </summary
        private void ShowPerson(object sender, EventArgs e, string id)
        {
            var detailed = new PersonDetailed(id);
            detailed.ShowDialog();
            detailed.BringToFront();
        }
        /// <summary>
        /// Event handler for showing all episodes of a TV series when the "Show All" button is clicked.
        /// </summary>
        private void ShowAllEpisodes(object sender, EventArgs e, List<Episode> episodes, string title)
        {
            var episode = new Episodes(title, episodes);
            episode.ShowDialog();
            episode.BringToFront();
        }
        /// <summary>
        /// Event handler for editing label text when a label is clicked (only available to admins).
        /// </summary>
        /// <param name="sender">The object that triggered the event (label to be edited).</param>
        private void EditLabel(object sender, EventArgs e)
        {
            Save.Visible = true;
            var label = sender as Label;
            var textBox = new TextBox();
            textBox.Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular);
            textBox.Location = label.Location;
            textBox.Width = label.Width;
            textBox.Text = label.Text;
            textBox.TextChanged += new EventHandler((sender, e) => ChangeLabelText(sender, e, label));
            textBox.KeyPress += new KeyPressEventHandler(DeleteTextBox);
            Controls.Add(textBox);
            textBox.BringToFront();
        }
        /// <summary>
        /// Event handler for ending text editing by pressing the Enter key.
        /// </summary>
        private void DeleteTextBox(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                var textBox = sender as TextBox;
                Controls.Remove(textBox);
                textBox.Dispose();
            }
        }
        /// <summary>
        /// Event handler for the label text update.
        /// </summary>
        private void ChangeLabelText(object sender, EventArgs e, Label label)
        {
            var textBox = sender as TextBox;
            label.Text = textBox.Text;
            textBox.Width = label.Width;
        }
        /// <summary>
        /// Event handler for the Delete button click, which deletes the movie from the database and returns to the movie search form.
        /// </summary>
        private void Delete_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(Connection.connectionString))
            {
                using (var db = new MoviesDataContext())
                {
                    connection.Open();
                    if (episodesList.Count > 0)
                    {
                        DialogResult dialogResult = MessageBox.Show("Delete all related episodes?", "", MessageBoxButtons.YesNo);
                        // delete all the episodes contained in the title from all tables
                        if (dialogResult == DialogResult.Yes)
                        {
                            for (int i = 0; i < episodesList.Count; i++)
                            {
                                var titleToDelete = (from title in db.title where title.tconst == episodesList[i].tconst select title).Single();
                                var episodeToDelete = (from episode in db.episode where episode.tconst == episodesList[i].tconst select episode).Single();
                                var crewToDetete = (from crew in db.crew where crew.tconst == episodesList[i].tconst select crew).SingleOrDefault();
                                var principalToDelete = (from principal in db.principals where principal.tconst == episodesList[i].tconst select principal).ToArray();
                                var ratingToDelete = (from rating in db.ratings where rating.tconst == episodesList[i].tconst select rating).SingleOrDefault();
                                db.title.Remove(titleToDelete);
                                db.episode.Remove(episodeToDelete);
                                if (crewToDetete != null)
                                    db.crew.Remove(crewToDetete);
                                if (principalToDelete != null)
                                    db.principals.RemoveRange(principalToDelete);
                                if (ratingToDelete != null)
                                    db.ratings.Remove(ratingToDelete);
                            }
                        }
                    }
                    // delete the title itself from all tables
                    var singleTitleToDelete = (from title in db.title where title.tconst == titleId select title).Single();
                    var singleEpisodeToDelete = (from episode in db.episode where episode.tconst == titleId select episode).SingleOrDefault();
                    var singleCrewToDetete = (from crew in db.crew where crew.tconst == titleId select crew).SingleOrDefault();
                    var singlePrincipalToDelete = (from principal in db.principals where principal.tconst == titleId select principal).ToArray();
                    var singleRatingToDelete = (from rating in db.ratings where rating.tconst == titleId select rating).SingleOrDefault();
                    db.title.Remove(singleTitleToDelete);
                    if (singleEpisodeToDelete != null)
                        db.episode.Remove(singleEpisodeToDelete);
                    if (singleCrewToDetete != null)
                        db.crew.Remove(singleCrewToDetete);
                    if (singlePrincipalToDelete != null)
                        db.principals.RemoveRange(singlePrincipalToDelete);
                    if (singleRatingToDelete != null)
                        db.ratings.Remove(singleRatingToDelete);
                    db.SaveChanges();
                    connection.Close();
                }
            }
            // title was deleted, repeat the search
            Program.MainFormManager.CurrentForm = new MovieSearch();
            this.Close();

        }
        /// <summary>
        /// Event handler for the Save button click, which updates the movie details in database.
        /// </summary>
        private void Save_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(Connection.connectionString))
            {
                using (var db = new MoviesDataContext())
                {
                    connection.Open();
                    var titleToUpdate = (from title in db.title where title.tconst == titleId select title).Single();
                    var titleParsed = Title.Text.Split(new char[] { '(', ')' });
                    if (titleParsed.Count() == 1)
                    {
                        titleToUpdate.primaryTitle = titleParsed[0];
                        titleToUpdate.originalTitle = titleParsed[0];
                    }
                    else
                    {
                        titleToUpdate.primaryTitle = titleParsed[0].Substring(0, titleParsed[0].Length - 1);
                        titleToUpdate.originalTitle = titleParsed[1];
                    }
                    titleToUpdate.titleType = TitleType.Text;
                    if (int.TryParse(Runtime.Text, out _))
                        titleToUpdate.runtimeMinutes = Runtime.Text.Split()[0];
                    else
                        titleToUpdate.runtimeMinutes = "\\N";
                    titleToUpdate.genres = Genres.Text;
                    if (Years.Text == "Unknown year of release")
                    {

                        titleToUpdate.startYear = "\\N";
                        titleToUpdate.endYear = "\\N";
                    }
                    else
                    {
                        titleToUpdate.startYear = Years.Text.Split()[0];
                        if (Years.Text.Split().Count() > 2)
                        {
                            titleToUpdate.endYear = Years.Text.Split()[2];
                        }
                        else
                        {
                            titleToUpdate.endYear = "\\N";
                        }
                    }
                    db.SaveChanges();
                    connection.Close();
                    MessageBox.Show("Saved successfully");
                }
            }

        }
        private void PaintBorder(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Color.Black, 3), this.DisplayRectangle);
        }
        private bool mouseDown;
        private Point lastLocation;

        // Those event handlers are used for window dragging

        private void MovieDetailed_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks == 1)
            {
                mouseDown = true;
            }
            lastLocation = e.Location;
        }

        private void MovieDetailed_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void MovieDetailed_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

    }
}
