using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MvvmSampleContext.Models;
using MvvmSampleContext.Exceptions;

namespace MvvmSampleContext.Services;

public class DatabaseService
{
    private readonly string _databasePath;
    private readonly string _connectionString;
    private bool _isInitialized;

    public DatabaseService(string? databasePath = null)
    {
        _databasePath = databasePath ?? BuildDefaultDatabasePath();
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ToString();
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        try
        {
            var directory = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var tableCommand = connection.CreateCommand();
            tableCommand.CommandText =
                """
                CREATE TABLE IF NOT EXISTS Employees
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    EmployeeNumber TEXT NOT NULL,
                    Department TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );
                """;
            await tableCommand.ExecuteNonQueryAsync();

            var countCommand = connection.CreateCommand();
            countCommand.CommandText = "SELECT COUNT(*) FROM Employees;";
            var count = (long)(await countCommand.ExecuteScalarAsync() ?? 0);

            _isInitialized = true;

            if (count == 0)
            {
                var now = DateTime.UtcNow;
                var seed = new[]
                {
                    new EmployeeRecord
                    {
                        Name = "山田 太郎",
                        EmployeeNumber = "EMP-001",
                        Department = "営業部",
                        UpdatedAt = now,
                    },
                    new EmployeeRecord
                    {
                        Name = "佐藤 花子",
                        EmployeeNumber = "EMP-002",
                        Department = "開発部",
                        UpdatedAt = now,
                    },
                    new EmployeeRecord
                    {
                        Name = "中村 健",
                        EmployeeNumber = "EMP-003",
                        Department = "人事部",
                        UpdatedAt = now,
                    },
                };

                foreach (var record in seed)
                {
                    await InsertAsync(record);
                }
            }
        }
        catch (Exception ex)
        {
            throw new DataAccessException("データベースの初期化に失敗しました。", ex);
        }

    }

    public async Task<IReadOnlyList<EmployeeRecord>> GetAllAsync()
    {
        await InitializeAsync();

        try
        {
            var items = new List<EmployeeRecord>();

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT Id, Name, EmployeeNumber, Department, UpdatedAt
                FROM Employees
                ORDER BY Name COLLATE NOCASE;
                """;

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new EmployeeRecord
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    EmployeeNumber = reader.GetString(2),
                    Department = reader.GetString(3),
                    UpdatedAt = DateTime.Parse(reader.GetString(4), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                });
            }

            return items;
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            throw new DataAccessException("従業員一覧の取得に失敗しました。", ex);
        }

    }

    public async Task<int> InsertAsync(EmployeeRecord record)
    {
        await InitializeAsync();

        try
        {
            record.UpdatedAt = DateTime.UtcNow;

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                """
                INSERT INTO Employees (Name, EmployeeNumber, Department, UpdatedAt)
                VALUES ($name, $number, $department, $updatedAt);
                SELECT last_insert_rowid();
                """;

            command.Parameters.AddWithValue("$name", record.Name);
            command.Parameters.AddWithValue("$number", record.EmployeeNumber);
            command.Parameters.AddWithValue("$department", record.Department);
            command.Parameters.AddWithValue("$updatedAt", record.UpdatedAt.ToString("O"));

            var result = await command.ExecuteScalarAsync();
            var id = Convert.ToInt32(result);
            record.Id = id;

            return id;
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            throw new DataAccessException("従業員情報の追加に失敗しました。", ex);
        }

    }

    public async Task UpdateAsync(EmployeeRecord record)
    {
        await InitializeAsync();

        try
        {
            record.UpdatedAt = DateTime.UtcNow;

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                """
                UPDATE Employees
                SET Name = $name,
                    EmployeeNumber = $number,
                    Department = $department,
                    UpdatedAt = $updatedAt
                WHERE Id = $id;
                """;

            command.Parameters.AddWithValue("$name", record.Name);
            command.Parameters.AddWithValue("$number", record.EmployeeNumber);
            command.Parameters.AddWithValue("$department", record.Department);
            command.Parameters.AddWithValue("$updatedAt", record.UpdatedAt.ToString("O"));
            command.Parameters.AddWithValue("$id", record.Id);

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            throw new DataAccessException("従業員情報の更新に失敗しました。", ex);
        }

    }

    private static string BuildDefaultDatabasePath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MvvmSampleContext");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "employees.db");
    }
}
