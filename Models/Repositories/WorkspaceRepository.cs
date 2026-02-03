using CoWorkManager.Models;
using CoWorkManager.Models.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace CoWorkManager.Models.Repositories
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private readonly string _connectionString;

        public WorkspaceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Workspace> GetAll()
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM Workspaces";
            return connection.Query<Workspace>(sql).ToList();
        }

        public Workspace? GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM Workspaces WHERE WorkspaceId = @Id";
            return connection.QueryFirstOrDefault<Workspace>(sql, new { Id = id });
        }

        public void Add(Workspace workspace)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO Workspaces (Name, Type, PricePerHour, IsAvailable) 
                VALUES (@Name, @Type, @PricePerHour, @IsAvailable)";
            connection.Execute(sql, workspace);
        }

        public void Update(Workspace workspace)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE Workspaces
                SET Name = @Name,
                    Type = @Type,
                    PricePerHour = @PricePerHour,
                    IsAvailable = @IsAvailable
                WHERE WorkspaceId = @WorkspaceId";
            connection.Execute(sql, workspace);
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "DELETE FROM Workspaces WHERE WorkspaceId = @Id";
            connection.Execute(sql, new { Id = id });
        }
    }
}