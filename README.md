# Research_Assistant Web Application

This is a web application built using ASP.NET Core to help users upload and manage their research papers. It also provides keyword extraction using NLP techniques such as TF-IDF, and allows users to search through the papers by various metadata.

## Features:
- **User Authentication & Roles**: Admin and User roles, with login and registration functionality.
- **PDF Paper Upload**: Upload PDF files and extract metadata such as title, author, and publication date.
- **Keyword Extraction**: Extract and display keywords using NLP techniques like TF-IDF.
- **Search Papers**: Users can search papers by title, author, or keywords.

## Setup

1. Clone the repository.
2. Run `dotnet restore` to restore dependencies.
3. Run `dotnet build` to build the project.
4. Set up your database with `dotnet ef database update`.
5. Run the application with `dotnet run`.

## License

This project is licensed under the MIT License.
