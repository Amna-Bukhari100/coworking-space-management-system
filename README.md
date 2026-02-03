# Co-Working Space Management System

## Description
A full-stack, web-based management platform designed to streamline operations for shared workspaces. This system provides a seamless experience for members to book resources and for administrators to manage daily operations using a robust MVC architecture.

## Features
- **Real-Time Desk & Room Booking**: Interactive interface for members to reserve workspace resources based on live availability.
- **Member Management**: Secure registration and profile management system for workspace community members.
- **Visitor Tracking**: Digital logging system to maintain security records and visitor history within the facility.
- **Admin Dashboard**: Centralized control center for monitoring occupancy rates, managing active bookings, and viewing usage analytics.
- **Real-Time Notifications**: Integrated SignalR hubs to provide instant updates on booking statuses and facility announcements.

## Tech Stack
- **Framework**: ASP.NET Core MVC (C#)
- **Real-Time Communication**: SignalR
- **Database**: SQL Server (Entity Framework Core)
- **Frontend**: HTML5, CSS3, Bootstrap 5, JavaScript
- **Architecture**: Model-View-Controller (MVC) with Repository Pattern

## Project Structure
- **Controllers**: Handles the logic for member actions and admin requests.
- **Models**: Defines the database schema for Members, Bookings, and Resources.
- **Views**: Razor-based frontend templates for the user interface.
- **Hubs**: Contains SignalR logic for real-time communication.
- **Data**: Managed via Entity Framework Core for SQL Server integration.

## Installation & Setup
1. **Clone the Repository**: 
   `git clone https://github.com/Amna-Bukhari100/coworking-space-management-system.git`
2. **Update Connection String**: Modify the `appsettings.json` file to point to your local SQL Server instance.
3. **Apply Migrations**: Run `Update-Database` in the Package Manager Console to create the SQL schema.
4. **Run**: Press `F5` in Visual Studio to launch the application.
