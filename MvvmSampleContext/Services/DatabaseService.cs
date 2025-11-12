using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MvvmSampleContext.Models;

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
            CREATE TABLE IF NOT EXISTS Drones
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                SerialNumber TEXT NOT NULL,
                Manufacturer TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            """;
        await tableCommand.ExecuteNonQueryAsync();

        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Drones;";
        var count = (long)(await countCommand.ExecuteScalarAsync() ?? 0);

        _isInitialized = true;

        if (count == 0)
        {
            var now = DateTime.UtcNow;
            var seed = new[]
            {
                new DroneRecord
                {
                    Name = "Surveyor X1",
                    SerialNumber = "SRV-001",
                    Manufacturer = "Contoso Drones",
                    UpdatedAt = now,
                },
                new DroneRecord
                {
                    Name = "Logistics Pro",
                    SerialNumber = "LOG-204",
                    Manufacturer = "Northwind Robotics",
                    UpdatedAt = now,
                },
                new DroneRecord
                {
                    Name = "Rescue Scout",
                    SerialNumber = "RSC-778",
                    Manufacturer = "Adventure Works Aerospace",
                    UpdatedAt = now,
                },
            };

            foreach (var record in seed)
            {
                await InsertAsync(record);
            }
        }
    }

    public async Task<IReadOnlyList<DroneRecord>> GetAllAsync()
    {
        await InitializeAsync();

        var items = new List<DroneRecord>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, Name, SerialNumber, Manufacturer, UpdatedAt
            FROM Drones
            ORDER BY Name COLLATE NOCASE;
            """;

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new DroneRecord
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                SerialNumber = reader.GetString(2),
                Manufacturer = reader.GetString(3),
                UpdatedAt = DateTime.Parse(reader.GetString(4), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            });
        }

        return items;
    }

    public async Task<int> InsertAsync(DroneRecord record)
    {
        await InitializeAsync();

        record.UpdatedAt = DateTime.UtcNow;

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO Drones (Name, SerialNumber, Manufacturer, UpdatedAt)
            VALUES ($name, $serial, $manufacturer, $updatedAt);
            SELECT last_insert_rowid();
            """;

        command.Parameters.AddWithValue("$name", record.Name);
        command.Parameters.AddWithValue("$serial", record.SerialNumber);
        command.Parameters.AddWithValue("$manufacturer", record.Manufacturer);
        command.Parameters.AddWithValue("$updatedAt", record.UpdatedAt.ToString("O"));

        var result = await command.ExecuteScalarAsync();
        var id = Convert.ToInt32(result);
        record.Id = id;

        return id;
    }

    public async Task UpdateAsync(DroneRecord record)
    {
        await InitializeAsync();

        record.UpdatedAt = DateTime.UtcNow;

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE Drones
            SET Name = $name,
                SerialNumber = $serial,
                Manufacturer = $manufacturer,
                UpdatedAt = $updatedAt
            WHERE Id = $id;
            """;

        command.Parameters.AddWithValue("$name", record.Name);
        command.Parameters.AddWithValue("$serial", record.SerialNumber);
        command.Parameters.AddWithValue("$manufacturer", record.Manufacturer);
        command.Parameters.AddWithValue("$updatedAt", record.UpdatedAt.ToString("O"));
        command.Parameters.AddWithValue("$id", record.Id);

        await command.ExecuteNonQueryAsync();
    }

    private static string BuildDefaultDatabasePath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MvvmSampleContext");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "drones.db");
    }
}
