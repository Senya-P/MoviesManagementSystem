# Movies Management System - Developer Documentation

## Introduction

The Movies Management System is a C# Windows Forms application that allows users to browse and manage information about movies, TV shows, and the people associated with them. This documentation provides an overview of the system's architecture, key features, and usage guidelines for developers.

## Application Overview

The system follows a desktop application architecture, implemented in C#. It utilizes the Windows Forms framework for the graphical user interface (GUI) and MySQL for data storage. The Entity Framework is used for database operations.

### Technologies Used

-   **C# and .NET Framework:** The programming language and framework for building the application.
-   **Windows Forms:** Used for creating the graphical user interface (GUI).
-   **MySQL Database:** Stores data.
-   **Entity Framework Core:** The Object-Relational Mapping framework for database operations.
-   **System.Linq:** Performs functions defined in the database on objects.
-   **MySql.Data.MySqlClient:** Provides MySQL database connectivity.
-   **MySqlConnector.MySqlBulkCopy:** Efficient data load to a MySQL Server.
-   **System.Threading.Tasks:** Parallel data processing.
-   **Visual Studio:** IDE for C# development.

### Database Schema

The program uses the [IMDb non-commercial dataset](https://developer.imdb.com/non-commercial-datasets/).  Its data is uploaded to MySQL server into a `movie` database with the following tables: 
-   **person:** Stores information about individuals in the industry.
-   **title:** Stores movie and TV show data.
-   **episode:** Stores data related to episodes of TV shows.
-   **principals:** Stores information about people's roles in movies or TV shows.
-   **crew:** Contains data about the crew members of movies or TV shows.
-   **ratings:** Stores movie ratings and vote counts.

The data files must have appropriate names so that the data is correctly mapped to the model.
The Entity Framework maps these tables to C# classes, providing an object-oriented way to interact with the database.
The database connection string is defined in `Connection` class. 

## Functionality

### Movie Search

-   Users can search for movies by various criteria, such as title, genre, and release year. All the data entered by the user is combined into a MySQl query and then sent to the server.
-   The search results are displayed in a list, and users can click on a movie to view details. When the user clicks, the title ID is retrieved and the detail page is displayed.

### Movie Details

-   Detailed information about a selected title is displayed: all details for a given title are retrieved from the server by its ID, then mapped to objects using LINQ, and the page is dynamically generated based on the recieved data.
-   This includes title name, title type, genre, release year, runtime, rating, cast, and crew information.
-   Users can also view episodes of TV shows. 
-   Users can also open a person's detailed page by clicking on their name.
-   All lables have a connected event handler that, when clicked, displays the detail page of a title or person by given unique ID.
-   Admin can modify the information.
-   If the list of episodes does not fit on the page, a "Show All" button is displayed, which, when clicked, displays the full list.

### Person Details

-   Users can view detailed information about individuals in the industry, including their primary name, profession, birth and death years, and known titles.
-    Admin can modify the information.

### Admin Dashboard

-   Administrators have access to an admin dashboard for data management.
-   They can upload data from CSV or TSV files to the database. File is read in batches using `Microsoft.VisualBasic.FileIO.TextFieldParser` and then processed in parallel and loaded to server using `MySqlConnector.MySqlBulkCopy`.
-    Administrators can also change information about a title or person by clicking on the label. This triggers an event handler that displays a text box where the information can be changed. When editing is done, the text box is removed and the updated information is displayed. To save the page, the administrator clicks the "Save" button, which sends the data to the MySQL server.
-    Administrators can irrevocably delete a person's or title's record.

## Future Improvements

-   Implement user profiles with favorite lists and user reviews.
-   Add support for additional data formats for data uploads.
-   Add the ability for administrators to add titles and people to the database.
-   Add better data validation. The present version of the system relies on the administrator being familiar with the database format, so only basic validation is implemented.
