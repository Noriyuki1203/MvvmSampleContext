using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
                CREATE TABLE IF NOT EXISTS Departments
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Employees
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    EmployeeNumber TEXT NOT NULL,
                    Department TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS FamilyMembers
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EmployeeId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    Relationship TEXT NOT NULL,
                    Age INTEGER NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    FOREIGN KEY (EmployeeId) REFERENCES Employees (Id)
                );
                """;
            await tableCommand.ExecuteNonQueryAsync();

            var departmentCountCommand = connection.CreateCommand();
            departmentCountCommand.CommandText = "SELECT COUNT(*) FROM Departments;";
            var departmentCount = (long)(await departmentCountCommand.ExecuteScalarAsync() ?? 0);

            var employeeCountCommand = connection.CreateCommand();
            employeeCountCommand.CommandText = "SELECT COUNT(*) FROM Employees;";
            var employeeCount = (long)(await employeeCountCommand.ExecuteScalarAsync() ?? 0);

            var familyCountCommand = connection.CreateCommand();
            familyCountCommand.CommandText = "SELECT COUNT(*) FROM FamilyMembers;";
            var familyCount = (long)(await familyCountCommand.ExecuteScalarAsync() ?? 0);

            _isInitialized = true;

            if (departmentCount == 0)
            {
                var now = DateTime.UtcNow;
                var departments = new[]
                {
                    new DepartmentRecord { Name = "営業部", UpdatedAt = now },
                    new DepartmentRecord { Name = "開発部", UpdatedAt = now },
                    new DepartmentRecord { Name = "人事部", UpdatedAt = now },
                };

                foreach (var department in departments)
                {
                    await InsertDepartmentAsync(department);
                }
            }

            if (employeeCount == 0)
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

            if (familyCount == 0)
            {
                var employees = await GetAllAsync();
                var now = DateTime.UtcNow;

                FamilyMemberRecord? Create(string employeeNumber, string name, string relation, int age)
                {
                    var employee = employees.FirstOrDefault(x => x.EmployeeNumber == employeeNumber);
                    return employee is null
                        ? null
                        : new FamilyMemberRecord
                        {
                            EmployeeId = employee.Id,
                            Name = name,
                            Relationship = relation,
                            Age = age,
                            UpdatedAt = now,
                        };
                }

                var familySeed = new List<FamilyMemberRecord?>
                {
                    Create("EMP-001", "山田 花子", "配偶者", 35),
                    Create("EMP-001", "山田 太郎 Jr.", "子", 8),
                    Create("EMP-002", "佐藤 太一", "配偶者", 33),
                    Create("EMP-003", "中村 葵", "配偶者", 31),
                    Create("EMP-003", "中村 優", "子", 4),
                };

                foreach (var member in familySeed)
                {
                    if (member is null)
                    {
                        continue;
                    }

                    await InsertFamilyMemberAsync(member);
                }
            }
        }
        catch (Exception ex)
        {
            throw new DataAccessException("データベースの初期化に失敗しました。", ex);
        }
    }

    public async Task<IReadOnlyList<DepartmentRecord>> GetDepartmentsAsync()
    {
        await InitializeAsync();

        try
        {
            var items = new List<DepartmentRecord>();

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT Id, Name, UpdatedAt
                FROM Departments
                ORDER BY Name COLLATE NOCASE;
                """;

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new DepartmentRecord
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    UpdatedAt = DateTime.Parse(reader.GetString(2), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                });
            }

            return items;
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            throw new DataAccessException("部署一覧の取得に失敗しました。", ex);
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

    public async Task<IReadOnlyList<EmployeeRecord>> GetEmployeesByDepartmentAsync(string? departmentName)
    {
        await InitializeAsync();

        try
        {
            var items = new List<EmployeeRecord>();

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            if (string.IsNullOrWhiteSpace(departmentName))
            {
                command.CommandText =
                    """
                    SELECT Id, Name, EmployeeNumber, Department, UpdatedAt
                    FROM Employees
                    ORDER BY Name COLLATE NOCASE;
                    """;
            }
            else
            {
                command.CommandText =
                    """
                    SELECT Id, Name, EmployeeNumber, Department, UpdatedAt
                    FROM Employees
                    WHERE Department = $department
                    ORDER BY Name COLLATE NOCASE;
                    """;
                command.Parameters.AddWithValue("$department", departmentName);
            }

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

    public async Task<IReadOnlyList<FamilyMemberRecord>> GetFamiliesByEmployeeAsync(int employeeId)
    {
        await InitializeAsync();

        try
        {
            var items = new List<FamilyMemberRecord>();

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT Id, EmployeeId, Name, Relationship, Age, UpdatedAt
                FROM FamilyMembers
                WHERE EmployeeId = $employeeId
                ORDER BY Id;
                """;
            command.Parameters.AddWithValue("$employeeId", employeeId);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new FamilyMemberRecord
                {
                    Id = reader.GetInt32(0),
                    EmployeeId = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    Relationship = reader.GetString(3),
                    Age = reader.GetInt32(4),
                    UpdatedAt = DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                });
            }

            return items;
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            throw new DataAccessException("家族情報の取得に失敗しました。", ex);
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

    public async Task<int> InsertDepartmentAsync(DepartmentRecord record)
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
                INSERT INTO Departments (Name, UpdatedAt)
                VALUES ($name, $updatedAt);
                SELECT last_insert_rowid();
                """;

            command.Parameters.AddWithValue("$name", record.Name);
            command.Parameters.AddWithValue("$updatedAt", record.UpdatedAt.ToString("O"));

            var result = await command.ExecuteScalarAsync();
            var id = Convert.ToInt32(result);
            record.Id = id;
            return id;
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            throw new DataAccessException("部署情報の追加に失敗しました。", ex);
        }
    }

    public async Task<int> InsertFamilyMemberAsync(FamilyMemberRecord record)
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
                INSERT INTO FamilyMembers (EmployeeId, Name, Relationship, Age, UpdatedAt)
                VALUES ($employeeId, $name, $relationship, $age, $updatedAt);
                SELECT last_insert_rowid();
                """;

            command.Parameters.AddWithValue("$employeeId", record.EmployeeId);
            command.Parameters.AddWithValue("$name", record.Name);
            command.Parameters.AddWithValue("$relationship", record.Relationship);
            command.Parameters.AddWithValue("$age", record.Age);
            command.Parameters.AddWithValue("$updatedAt", record.UpdatedAt.ToString("O"));

            var result = await command.ExecuteScalarAsync();
            var id = Convert.ToInt32(result);
            record.Id = id;
            return id;
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            throw new DataAccessException("家族情報の追加に失敗しました。", ex);
        }
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        await InitializeAsync();

        try
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync();

            var deleteFamilies = connection.CreateCommand();
            deleteFamilies.Transaction = transaction;
            deleteFamilies.CommandText =
                """
                DELETE FROM FamilyMembers
                WHERE EmployeeId = $employeeId;
                """;
            deleteFamilies.Parameters.AddWithValue("$employeeId", id);
            await deleteFamilies.ExecuteNonQueryAsync();

            var deleteEmployee = connection.CreateCommand();
            deleteEmployee.Transaction = transaction;
            deleteEmployee.CommandText =
                """
                DELETE FROM Employees
                WHERE Id = $id;
                """;
            deleteEmployee.Parameters.AddWithValue("$id", id);
            await deleteEmployee.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            throw new DataAccessException("従業員の削除に失敗しました。", ex);
        }
    }

    public async Task DeleteDepartmentAsync(DepartmentRecord record)
    {
        await InitializeAsync();

        try
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync();

            var deleteFamilies = connection.CreateCommand();
            deleteFamilies.Transaction = transaction;
            deleteFamilies.CommandText =
                """
                DELETE FROM FamilyMembers
                WHERE EmployeeId IN (
                    SELECT Id FROM Employees WHERE Department = $department
                );
                """;
            deleteFamilies.Parameters.AddWithValue("$department", record.Name);
            await deleteFamilies.ExecuteNonQueryAsync();

            var deleteEmployees = connection.CreateCommand();
            deleteEmployees.Transaction = transaction;
            deleteEmployees.CommandText =
                """
                DELETE FROM Employees
                WHERE Department = $department;
                """;
            deleteEmployees.Parameters.AddWithValue("$department", record.Name);
            await deleteEmployees.ExecuteNonQueryAsync();

            var deleteDepartment = connection.CreateCommand();
            deleteDepartment.Transaction = transaction;
            deleteDepartment.CommandText =
                """
                DELETE FROM Departments
                WHERE Id = $id;
                """;
            deleteDepartment.Parameters.AddWithValue("$id", record.Id);
            await deleteDepartment.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            throw new DataAccessException("部署の削除に失敗しました。", ex);
        }
    }

    public async Task DeleteFamilyMemberAsync(int id)
    {
        await InitializeAsync();

        try
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                """
                DELETE FROM FamilyMembers
                WHERE Id = $id;
                """;
            command.Parameters.AddWithValue("$id", id);

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            throw new DataAccessException("家族の削除に失敗しました。", ex);
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
