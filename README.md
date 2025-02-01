## About
This is a full-stack blog application built using modern web technologies. The backend is powered by **ASP.NET Core Web APIs** with **Entity Framework (EF)**, while the frontend leverages **HTMX** and **Handlebars.NET** for dynamic rendering. Styling is handled using **Tailwind CSS** along with **DaisyUI** for enhanced UI components.

### Features:
- Supports storing blog posts either **locally** on the server or in an **S3 bucket**.
- Blog content is stored as raw **Markdown** (`[filename].md`) alongside a **JSON metadata file** (`[filename].md.json`) containing details such as:
  - Last edited timestamp
  - Upload time
  - Title and description
- User authentication system with a **SQLite database**, with future plans to support **comments and additional features**.
- Options for **bulk caching or single file caching** for both blogs and views/components. This allows choosing between loading everything into RAM at project startup or loading only the necessary files on demand, optimizing performance based on disk read times.

## Tech Stack
- **Backend:** ASP.NET Core Web APIs, Entity Framework
- **Frontend:** HTMX, Handlebars.NET
- **Styling:** Tailwind CSS, DaisyUI
- **Database:** SQLite (future support for extended DB features)
- **Storage:** Local server storage or AWS S3

This project demonstrates my ability to build and integrate **full-stack applications** with modern technologies, emphasizing performance, maintainability, and scalability. Future enhancements will include user interactions, comments, and extended database support.

