using CoWorkManager.Models;
using CoWorkManager.Models.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoWorkManager.Models.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly string _connectionString;

        public BookingRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Booking> GetAll()
        {
            using var connection = new SqlConnection(_connectionString);
            // Updated SQL to join with Visitors, Workspaces, and Users
            string sql = @"
        SELECT b.*, 
               v.VisitorId, v.FullName, v.Email, v.Phone, v.BookingId,
               w.WorkspaceId, w.Name, w.Type, w.PricePerHour, w.IsAvailable,
               u.Id, u.FullName, u.Email
        FROM Bookings b
        LEFT JOIN Visitors v ON b.BookingId = v.BookingId
        INNER JOIN Workspaces w ON b.WorkspaceId = w.WorkspaceId
        LEFT JOIN AspNetUsers u ON b.ApplicationUserId = u.Id";

            var bookingDictionary = new Dictionary<int, Booking>();

            var result = connection.Query<Booking, Visitor?, Workspace, ApplicationUser, Booking>(
                sql,
                (booking, visitor, workspace, user) =>
                {
                    if (!bookingDictionary.TryGetValue(booking.BookingId, out var currentBooking))
                    {
                        currentBooking = booking;
                        currentBooking.Workspace = workspace;
                        currentBooking.ApplicationUser = user;
                        currentBooking.Visitors = new List<Visitor>();
                        bookingDictionary.Add(currentBooking.BookingId, currentBooking);
                    }

                    if (visitor != null)
                    {
                        currentBooking.Visitors.Add(visitor);
                    }

                    return currentBooking;
                },
                splitOn: "VisitorId,WorkspaceId,Id"
            );

            return bookingDictionary.Values;
        }

        public Booking? GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT b.*, 
                       v.VisitorId, v.FullName, v.Email, v.Phone, v.BookingId,
                       w.WorkspaceId, w.Name, w.Type, w.PricePerHour, w.IsAvailable
                FROM Bookings b
                LEFT JOIN Visitors v ON b.BookingId = v.BookingId
                INNER JOIN Workspaces w ON b.WorkspaceId = w.WorkspaceId
                WHERE b.BookingId = @Id";

            var bookingDictionary = new Dictionary<int, Booking>();

            return connection.Query<Booking, Visitor?, Workspace, Booking>(
                sql,
                (booking, visitor, workspace) =>
                {
                    if (!bookingDictionary.TryGetValue(booking.BookingId, out var currentBooking))
                    {
                        currentBooking = booking;
                        currentBooking.Workspace = workspace;
                        currentBooking.Visitors = new List<Visitor>();
                        bookingDictionary.Add(currentBooking.BookingId, currentBooking);
                    }

                    if (visitor != null)
                    {
                        currentBooking.Visitors.Add(visitor);
                    }
                    return currentBooking;
                },
                new { Id = id },
                splitOn: "VisitorId,WorkspaceId"
            ).FirstOrDefault();
        }

        public void Add(Booking booking)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO Bookings (WorkspaceId, StartTime, EndTime, BookingStatus, ApplicationUserId)
                VALUES (@WorkspaceId, @StartTime, @EndTime, @BookingStatus, @ApplicationUserId);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            int newId = connection.QuerySingle<int>(sql, booking);
            booking.BookingId = newId;
        }

        public void Update(Booking booking)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE Bookings
                SET WorkspaceId = @WorkspaceId,
                    StartTime = @StartTime,
                    EndTime = @EndTime,
                    BookingStatus = @BookingStatus,
                    ApplicationUserId = @ApplicationUserId
                WHERE BookingId = @BookingId";
            connection.Execute(sql, booking);
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "DELETE FROM Bookings WHERE BookingId = @Id";
            connection.Execute(sql, new { Id = id });
        }

        // NEW: Time-Overlap Fairness Check
        public bool IsWorkspaceOccupied(int workspaceId, DateTime start, DateTime end)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT COUNT(*) 
                FROM Bookings 
                WHERE WorkspaceId = @WorkspaceId 
                AND BookingStatus IN ('Confirmed', 'Pending')
                AND @RequestedStart < EndTime 
                AND @RequestedEnd > StartTime";

            int count = connection.ExecuteScalar<int>(sql, new
            {
                WorkspaceId = workspaceId,
                RequestedStart = start,
                RequestedEnd = end
            });

            return count > 0;
        }
    }
}