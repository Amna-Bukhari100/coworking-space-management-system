using CoWorkManager.Models;
using CoWorkManager.Models.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace CoWorkManager.Models.Repositories
{
    public class VisitorRepository : IVisitorRepository
    {
        private readonly string _connectionString;

        public VisitorRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public IEnumerable<Visitor> GetAll()
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM Visitors";
            var result = connection.Query<Visitor>(sql);
            return result ?? new List<Visitor>();
        }
        public Visitor? GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM Visitors WHERE VisitorId = @Id";
            return connection.QueryFirstOrDefault<Visitor>(sql, new { Id = id });
        }

        public IEnumerable<Visitor> GetByBookingId(int bookingId)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM Visitors WHERE BookingId = @BookingId";
            return connection.Query<Visitor>(sql, new { BookingId = bookingId }).ToList();
        }

        public void Add(Visitor visitor)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO Visitors (FullName, Email, Phone, BookingId) 
                VALUES (@FullName, @Email, @Phone, @BookingId);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            int newId = connection.QuerySingle<int>(sql, visitor);
            visitor.VisitorId = newId;
        }

        public void Update(Visitor visitor)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE Visitors
                SET FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone,
                    BookingId = @BookingId
                WHERE VisitorId = @VisitorId";
            connection.Execute(sql, visitor);
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "DELETE FROM Visitors WHERE VisitorId = @Id";
            connection.Execute(sql, new { Id = id });
        }
    }
}