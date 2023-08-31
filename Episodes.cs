using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MoviesManagementSystem
{
    /// <summary>
    /// Represents a form displaying episodes of a TV series in the Movies Management System.
    /// </summary>
    public partial class Episodes : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Episodes"/> class.
        /// </summary>
        /// <param name="title">The title name.</param>
        /// <param name="episodes">List of episodes of the title.</param>
        public Episodes(string title, List<Episode> episodes)
        {
            InitializeComponent();
            this.Paint += PaintBorder;
            Title.Text = title + " - Episodes";
            DGV.DataSource = episodes;
            DGV.Columns[0].Visible = false; //tconst column
            DGV.Columns[1].Visible = false; //parentTconst column
            DGV.ColumnHeadersVisible = false;
            DGV.AllowUserToAddRows = false;
            DGV.Cursor = Cursors.Hand;
            DGV.EditMode = DataGridViewEditMode.EditProgrammatically;
            DGV.CellClick += new DataGridViewCellEventHandler(ShowEpisode);

        }
        /// <summary>
        /// Event handler for displaying detailed information about an episode when a cell is clicked.
        /// </summary>
        void ShowEpisode(object sender, DataGridViewCellEventArgs e)
        {
            string id = (string)DGV[0, e.RowIndex].Value; //retrieve tconst
            var detailed = new MovieDetailed(id);
            detailed.ShowDialog();
            detailed.BringToFront();
        }
        private void Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void PaintBorder(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Color.Black, 3), this.DisplayRectangle);
        }
    }
}
