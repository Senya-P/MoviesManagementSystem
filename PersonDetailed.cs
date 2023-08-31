using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Linq;

namespace MoviesManagementSystem
{
    /// <summary>
    /// Represents a detailed view of a person in the Movies Management System.
    /// </summary>
    public partial class PersonDetailed : Form
    {
        private const int margin = 216;
        private string personId;
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonDetailed"/> class.
        /// </summary>
        /// <param name="personId">The unique identifier of the person to display.</param>
        public PersonDetailed(string personId)
        {
            InitializeComponent();
            this.personId = personId;
            this.Paint += PaintBorder;
            this.MouseDown += PersonDetailed_MouseDown;
            this.MouseMove += PersonDetailed_MouseMove;
            this.MouseUp += PersonDetailed_MouseUp;
            Save.Visible = false;
            Save.Cursor = Cursors.Hand;
            Delete.Visible = LoginInfo.IsAdmin ? true : false;
            Delete.Cursor = Cursors.Hand;
            using (MySqlConnection connection = new MySqlConnection(Connection.connectionString))
            {
                using (var db = new MoviesDataContext())
                {
                    connection.Open();
                    var personToDisplay = (from person in db.person where personId == person.nconst select person).Single();
                    PrimaryName.Text = personToDisplay.primaryName;
                    Profession.Text = personToDisplay.primaryProfession;
                    Born.Text = personToDisplay.birthYear != "\\N" ? personToDisplay.birthYear : "Unknown";
                    if (personToDisplay.deathYear != "\\N")
                    {
                        Died.Text = personToDisplay.deathYear;
                    }
                    else
                    {
                        DiedLabel.Visible = false;
                        Died.Text = "    ";
                        Died.Cursor = Cursors.Hand;
                    }
                    if (LoginInfo.IsAdmin)
                    {
                        PrimaryName.Click += EditLabel;
                        Profession.Click += EditLabel;
                        Born.Click += EditLabel;
                        Died.Click += EditLabel;
                    }
                    var titles = personToDisplay.knownForTitles.Split(',');
                    Point lastPoint = new Point(margin, TitlesLabel.Location.Y);
                    foreach (var title in titles)
                    {
                        var name = GetTitleName(title, db);
                        if (name != null)   // otherwise ignore
                        {
                            Label label = new Label() { AutoSize = true };
                            label.Click += new EventHandler((sender, e) => ShowTitle(sender, e, title));
                            label.Cursor = Cursors.Hand;
                            label.MaximumSize = new Size(150, 40);
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
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// Event handler for the Delete button click, which deletes the person from the database and returns to the movie search form.
        /// </summary>
        private void Delete_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(Connection.connectionString))
            {
                using (var db = new MoviesDataContext())
                {
                    connection.Open();
                    var personToDelete = (from person in db.person where person.nconst == personId select person).Single();
                    db.person.Remove(personToDelete);
                    db.SaveChanges();
                    connection.Close();
                }
            }
            Program.MainFormManager.CurrentForm = new MovieSearch();
            this.Close();
        }
        /// <summary>
        /// Event handler for the Save button click, which updates the person details in database.
        /// </summary>
        private void Save_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(Connection.connectionString))
            {
                using (var db = new MoviesDataContext())
                {
                    connection.Open();
                    var personToUpdate = (from person in db.person where person.nconst == personId select person).Single();
                    personToUpdate.primaryName = PrimaryName.Text;
                    personToUpdate.primaryProfession = Profession.Text;
                    if (int.TryParse(Born.Text, out var birthYear))
                    {
                        if (birthYear > 999 & birthYear < 10000)
                            personToUpdate.birthYear = Born.Text;
                        else MessageBox.Show("Write correct year of birth.");
                    }
                    else
                        personToUpdate.birthYear = "\\N";
                    if (int.TryParse(Died.Text, out var deathYear))
                    {
                        if (deathYear > 999 & deathYear < 10000)
                            personToUpdate.deathYear = Died.Text;
                        else MessageBox.Show("Write correct year of death.");
                    }
                    else
                        personToUpdate.deathYear = "\\N";
                    db.SaveChanges();
                    connection.Close();
                    MessageBox.Show("Saved successfully");
                }
            }
            Save.Visible = false;
        }

        /// <summary>
        /// Event handler for editing label text when a label is clicked (only available to admins).
        /// </summary>
        /// <param name="sender">The object that triggered the event (label to be edited).</param>
        private void EditLabel(object sender, EventArgs e)
        {
            Save.Visible = true;
            var label = sender as Label;
            if (label.Name == "Died")
                DiedLabel.Visible = true;
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
        /// Event handler for displaying detailed information about a title when a label is clicked.
        /// <param name="id">The unique identifier of the title (tconst).</param>
        /// </summary
        private void ShowTitle(object sender, EventArgs e, string id)
        {
            var detailed = new MovieDetailed(id);
            detailed.ShowDialog();
            detailed.BringToFront();
        }
        /// <summary>
        /// Retrieves the primary title name of a title based on their unique identifier.
        /// </summary>
        /// <param name="tconst">The unique identifier of the title.</param>
        /// <param name="db">The MoviesDataContext used for database operations.</param>
        /// <returns>The title name.</returns>
        private string GetTitleName(string tconst, MoviesDataContext db)
        {
            var name = (from title in db.title where tconst == title.tconst select title.primaryTitle).SingleOrDefault();
            return name;

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
        private void Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void PaintBorder(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Color.Black, 3), this.DisplayRectangle);
        }

        
        private bool mouseDown;
        private Point lastLocation;

        // Those event handlers are used for window dragging
        private void PersonDetailed_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks == 1)
            {
                mouseDown = true;
            }
            lastLocation = e.Location;
        }

        private void PersonDetailed_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void PersonDetailed_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }
    }
}
