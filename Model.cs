using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MoviesManagementSystem
{
    // <summary>
    /// Represents the database context for the Movies Management System.
    /// </summary>
    public class MoviesDataContext : DbContext
    {
        public DbSet<Person> person { get; set; }
        public DbSet<Episode> episode { get; set; }
        public DbSet<Title> title { get; set; }
        public DbSet<Principals> principals { get; set; }
        public DbSet<Crew> crew { get; set; }
        public DbSet<Ratings> ratings { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(Connection.connectionString, ServerVersion.AutoDetect(Connection.connectionString));
        }
    }
    public class Person
    {
        [Key]
        public string nconst { get; set; }
        public string primaryName { get; set; }
        public string birthYear { get; set; }
        public string deathYear { get; set; }
        public string primaryProfession { get; set; }
        public string knownForTitles { get; set; }
    }
    public class Title
    {
        [Key]
        public string tconst { get; set; }
        public string titleType { get; set; }
        public string primaryTitle { get; set; }
        public string originalTitle { get; set; }
        public string isAdult { get; set; }
        public string startYear { get; set; }
        public string endYear { get; set; }
        public string runtimeMinutes { get; set; }
        public string genres { get; set; }

    }

    public class Episode
    {
        [Key]
        public string tconst { get; set; }
        public string parentTconst { get; set; }
        public string seasonNumber { get; set; }
        public string episodeNumber { get; set; }

    }
    [PrimaryKey(nameof(tconst), nameof(nconst))]
    public class Principals
    {
        public string tconst { get; set; }
        public string ordering { get; set; }
        public string nconst { get; set; }
        public string category { get; set; }
        public string job { get; set; }
        public string characters { get; set; }

    }
    public class Crew
    {
        [Key]
        public string tconst { get; set; }
        public string directors { get; set; }
        public string writers { get; set; }
    }
    public class Ratings
    {
        [Key]
        public string tconst { get; set; }
        public string averageRating { get; set; }
        public string numVotes { get; set; }
    }
}
