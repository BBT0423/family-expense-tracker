using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;

// One-time migration: pulls real Transactions/Categories/Budgets out of the
// original web app's SQL Server database and writes them into the desktop
// app's SQLite database, collapsing 4 real per-person accounts into the
// desktop's single seeded device account + a Member tag per original person.
//
// Deliberately raw ADO.NET on both sides, not EF Core: SummaryYearSalaryDbContext
// (target side) overwrites CreatedAt/UpdatedAt on every SaveChanges call, which
// would destroy the real historical dates (2024-2026) on every migrated row.

const string DefaultSourceConnectionString =
    "Server=BBT0423;Database=SummaryYearSalaryDb;Trusted_Connection=True;TrustServerCertificate=True";

const string AdminEmail = "pongpan22120@gmail.com";

// Household budgets that collided on (CategoryId, Month, Year) after collapsing
// 4 accounts into 1 are combined by summing each person's LimitAmount, rather
// than arbitrarily keeping one and discarding the others.
const string BudgetCollisionStrategy = "Sum";

if (args.Length >= 1 && args[0] == "--count-db")
{
    using var countConn = new SqliteConnection($"Data Source={args[1]}");
    countConn.Open();
    foreach (var table in new[] { "Transactions", "Categories", "Members", "IdentityUsers" })
    {
        using var cmd = countConn.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM {table}";
        Console.WriteLine($"{table}: {cmd.ExecuteScalar()}");
    }
    using var cmd2 = countConn.CreateCommand();
    cmd2.CommandText = "SELECT strftime('%Y-%m', TransactionDate) AS ym, COUNT(*) FROM Transactions GROUP BY ym ORDER BY ym DESC LIMIT 5";
    using var reader = cmd2.ExecuteReader();
    while (reader.Read()) Console.WriteLine($"  {reader.GetString(0)}: {reader.GetInt32(1)}");
    return 0;
}

if (args.Length >= 1 && args[0] == "--export-xlsx")
{
    if (args.Length < 2)
    {
        Console.WriteLine("Usage: dotnet run -- --export-xlsx <output-path.xlsx> [--source-db <connection-string>]");
        return 1;
    }
    var outputPath = args[1];
    var sourceConn = DefaultSourceConnectionString;
    for (var i = 2; i < args.Length; i++)
    {
        if (args[i] == "--source-db")
            sourceConn = args[++i];
    }
    ExportExcel.Run(sourceConn, outputPath);
    return 0;
}

var options = ParseArgs(args);
if (options is null)
{
    PrintUsage();
    return 1;
}

Console.WriteLine(options.DryRun
    ? "=== DRY RUN — all writes will be rolled back at the end ==="
    : "=== LIVE RUN — writes will be committed ===");
Console.WriteLine($"Source: {options.SourceConnectionString}");
Console.WriteLine($"Target: {options.TargetDbPath}");
Console.WriteLine();

using var source = new SqlConnection(options.SourceConnectionString);
await source.OpenAsync();

using var target = new SqliteConnection($"Data Source={options.TargetDbPath}");
await target.OpenAsync();

using var tx = target.BeginTransaction();

try
{
    var adminUserId = GetAdminUserId(target, tx);
    Console.WriteLine($"Target device account Id: {adminUserId}");

    var (categoryIdMap, newCategoryCount) = MigrateCategories(source, target, tx, adminUserId);
    Console.WriteLine($"Categories: {categoryIdMap.Count} source rows mapped ({newCategoryCount} newly inserted).");

    var memberIdByOldUserId = CreateMembers(target, tx);
    Console.WriteLine($"Members created: {memberIdByOldUserId.Count}");

    var txnCount = MigrateTransactions(source, target, tx, adminUserId, categoryIdMap, memberIdByOldUserId);
    Console.WriteLine($"Transactions migrated: {txnCount}");

    var budgetCount = MigrateBudgets(source, target, tx, adminUserId, categoryIdMap);
    Console.WriteLine($"Budgets migrated: {budgetCount}");

    Console.WriteLine();
    PrintVerification(target, tx);

    if (options.DryRun)
    {
        tx.Rollback();
        Console.WriteLine();
        Console.WriteLine("Dry run complete — rolled back, target database unchanged.");
    }
    else
    {
        tx.Commit();
        Console.WriteLine();
        Console.WriteLine("Committed — target database updated.");
    }

    return 0;
}
catch
{
    tx.Rollback();
    throw;
}

// ─── Category migration ──────────────────────────────────────────────────

static (Dictionary<int, long> Map, int NewInserts) MigrateCategories(
    SqlConnection source, SqliteConnection target, SqliteTransaction tx, string adminUserId)
{
    var existingByKey = new Dictionary<(int Type, string Name), long>();
    using (var cmd = target.CreateCommand())
    {
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT Id, Name, Type FROM Categories";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var id = reader.GetInt64(0);
            var name = reader.GetString(1);
            var type = reader.GetInt32(2);
            existingByKey[(type, NormalizeName(name))] = id;
        }
    }

    var oldToNew = new Dictionary<int, long>();
    var newInserts = 0;

    using var srcCmd = source.CreateCommand();
    srcCmd.CommandText = @"
        SELECT Id, Name, Type, Icon, Color, IsDeleted
        FROM Categories
        ORDER BY Id";
    using var srcReader = srcCmd.ExecuteReader();
    while (srcReader.Read())
    {
        var oldId = srcReader.GetInt32(0);
        var name = srcReader.GetString(1);
        var type = srcReader.GetInt32(2);
        var icon = srcReader.GetString(3);
        var color = srcReader.GetString(4);
        var isDeleted = srcReader.GetBoolean(5);

        var key = (type, NormalizeName(name));
        if (existingByKey.TryGetValue(key, out var existingId))
        {
            oldToNew[oldId] = existingId;
            continue;
        }

        using var insert = target.CreateCommand();
        insert.Transaction = tx;
        insert.CommandText = @"
            INSERT INTO Categories (UserId, Name, Type, Icon, Color, IsDefault, CreatedAt, UpdatedAt, IsDeleted)
            VALUES ($userId, $name, $type, $icon, $color, 0, $createdAt, NULL, $isDeleted);
            SELECT last_insert_rowid();";
        insert.Parameters.AddWithValue("$userId", adminUserId);
        insert.Parameters.AddWithValue("$name", name);
        insert.Parameters.AddWithValue("$type", type);
        insert.Parameters.AddWithValue("$icon", icon);
        insert.Parameters.AddWithValue("$color", color);
        insert.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
        insert.Parameters.AddWithValue("$isDeleted", isDeleted);
        var newId = (long)insert.ExecuteScalar()!;

        oldToNew[oldId] = newId;
        existingByKey[key] = newId; // catch duplicate names within the source itself
        newInserts++;
    }

    return (oldToNew, newInserts);
}

static string NormalizeName(string name) => name.Trim().ToLowerInvariant();

// ─── Member creation ──────────────────────────────────────────────────────

static Dictionary<string, long> CreateMembers(SqliteConnection target, SqliteTransaction tx)
{
    // (source IdentityUsers.Id, placeholder display name, tag color)
    var seed = new (string OldUserId, string Name, string Color)[]
    {
        ("14e0b062-2aaa-4940-b148-a2762b005798", "paa (migrated)", "#4A90D9"),
        ("7208473c-c949-4d41-a4a7-889d52e9a919", "mae (migrated)", "#E67E22"),
        ("86e95f16-4a7d-4e7e-8b7b-4d085111727d", "noei (migrated)", "#8E44AD"),
        ("c3ba29f0-1ea1-417c-bda0-63d3238d793a", "pongpan22120 (migrated)", "#27AE60"),
    };

    var map = new Dictionary<string, long>();
    foreach (var (oldUserId, name, color) in seed)
    {
        using var insert = target.CreateCommand();
        insert.Transaction = tx;
        insert.CommandText = @"
            INSERT INTO Members (Name, Color, IsActive, CreatedAt, UpdatedAt, IsDeleted)
            VALUES ($name, $color, 1, $createdAt, NULL, 0);
            SELECT last_insert_rowid();";
        insert.Parameters.AddWithValue("$name", name);
        insert.Parameters.AddWithValue("$color", color);
        insert.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
        var newId = (long)insert.ExecuteScalar()!;
        map[oldUserId] = newId;
    }

    return map;
}

// ─── Transaction migration ────────────────────────────────────────────────

static string GetAdminUserId(SqliteConnection target, SqliteTransaction tx)
{
    using var cmd = target.CreateCommand();
    cmd.Transaction = tx;
    cmd.CommandText = "SELECT Id FROM IdentityUsers WHERE Email = $email";
    cmd.Parameters.AddWithValue("$email", AdminEmail);
    using var reader = cmd.ExecuteReader();
    if (!reader.Read())
    {
        throw new InvalidOperationException(
            $"No IdentityUsers row for '{AdminEmail}' found in the target database. " +
            "Boot the desktop app against this database at least once first so seeding runs.");
    }
    var id = reader.GetString(0);
    if (reader.Read())
    {
        throw new InvalidOperationException(
            $"More than one IdentityUsers row for '{AdminEmail}' — clean up test accounts before migrating.");
    }
    return id;
}

static int MigrateTransactions(
    SqlConnection source, SqliteConnection target, SqliteTransaction tx,
    string adminUserId, Dictionary<int, long> categoryIdMap, Dictionary<string, long> memberIdByOldUserId)
{
    using var srcCmd = source.CreateCommand();
    srcCmd.CommandText = @"
        SELECT Id, UserId, CategoryId, Type, Amount, Note, TransactionDate,
               CreatedAt, UpdatedAt, IsDeleted
        FROM Transactions
        ORDER BY Id";
    using var srcReader = srcCmd.ExecuteReader();

    var count = 0;
    while (srcReader.Read())
    {
        var oldId = srcReader.GetInt32(0);
        var oldUserId = srcReader.GetString(1);
        var oldCategoryId = srcReader.GetInt32(2);
        var type = srcReader.GetInt32(3);
        var amount = srcReader.GetDecimal(4);
        var note = srcReader.IsDBNull(5) ? null : srcReader.GetString(5);
        var transactionDate = srcReader.GetDateTime(6);
        var createdAt = srcReader.GetDateTime(7);
        var updatedAt = srcReader.IsDBNull(8) ? (DateTime?)null : srcReader.GetDateTime(8);
        var isDeleted = srcReader.GetBoolean(9);

        if (!categoryIdMap.TryGetValue(oldCategoryId, out var newCategoryId))
        {
            throw new InvalidOperationException(
                $"Transaction {oldId} references CategoryId {oldCategoryId}, which has no mapping.");
        }
        memberIdByOldUserId.TryGetValue(oldUserId, out var memberId);

        using var insert = target.CreateCommand();
        insert.Transaction = tx;
        insert.CommandText = @"
            INSERT INTO Transactions
              (Id, UserId, CreatedByUserId, CategoryId, MemberId, Type, Amount, Note, TransactionDate,
               CreatedAt, UpdatedAt, IsDeleted)
            VALUES
              ($id, $userId, $userId, $categoryId, $memberId, $type, $amount, $note, $transactionDate,
               $createdAt, $updatedAt, $isDeleted);";
        insert.Parameters.AddWithValue("$id", oldId);
        insert.Parameters.AddWithValue("$userId", adminUserId);
        insert.Parameters.AddWithValue("$categoryId", newCategoryId);
        insert.Parameters.AddWithValue("$memberId", memberId == 0 ? DBNull.Value : memberId);
        insert.Parameters.AddWithValue("$type", type);
        insert.Parameters.AddWithValue("$amount", amount);
        insert.Parameters.AddWithValue("$note", (object?)note ?? DBNull.Value);
        insert.Parameters.AddWithValue("$transactionDate", transactionDate.ToString("O"));
        insert.Parameters.AddWithValue("$createdAt", createdAt.ToString("O"));
        insert.Parameters.AddWithValue("$updatedAt", (object?)updatedAt?.ToString("O") ?? DBNull.Value);
        insert.Parameters.AddWithValue("$isDeleted", isDeleted);
        insert.ExecuteNonQuery();

        count++;
    }

    return count;
}

// ─── Budget migration ──────────────────────────────────────────────────────

static int MigrateBudgets(
    SqlConnection source, SqliteConnection target, SqliteTransaction tx,
    string adminUserId, Dictionary<int, long> categoryIdMap)
{
    var rows = new List<(int Id, int CategoryId, int Month, int Year, decimal LimitAmount,
        DateTime CreatedAt, DateTime? UpdatedAt, bool IsDeleted)>();

    using (var srcCmd = source.CreateCommand())
    {
        srcCmd.CommandText = @"
            SELECT Id, CategoryId, Month, Year, LimitAmount, CreatedAt, UpdatedAt, IsDeleted
            FROM Budgets
            ORDER BY Year, Month, CategoryId";
        using var reader = srcCmd.ExecuteReader();
        while (reader.Read())
        {
            var oldCategoryId = reader.GetInt32(1);
            if (!categoryIdMap.TryGetValue(oldCategoryId, out var newCategoryId))
            {
                throw new InvalidOperationException(
                    $"Budget {reader.GetInt32(0)} references CategoryId {oldCategoryId}, which has no mapping.");
            }

            rows.Add((
                reader.GetInt32(0),
                (int)newCategoryId,
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetDecimal(4),
                reader.GetDateTime(5),
                reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                reader.GetBoolean(7)));
        }
    }

    var groups = rows.GroupBy(r => (r.CategoryId, r.Month, r.Year));
    var inserted = 0;

    foreach (var group in groups)
    {
        var list = group.ToList();
        using var insert = target.CreateCommand();
        insert.Transaction = tx;

        if (list.Count == 1)
        {
            var b = list[0];
            insert.CommandText = @"
                INSERT INTO Budgets (Id, UserId, CategoryId, Month, Year, LimitAmount, CreatedAt, UpdatedAt, IsDeleted)
                VALUES ($id, $userId, $categoryId, $month, $year, $limitAmount, $createdAt, $updatedAt, $isDeleted);";
            insert.Parameters.AddWithValue("$id", b.Id);
            insert.Parameters.AddWithValue("$userId", adminUserId);
            insert.Parameters.AddWithValue("$categoryId", b.CategoryId);
            insert.Parameters.AddWithValue("$month", b.Month);
            insert.Parameters.AddWithValue("$year", b.Year);
            insert.Parameters.AddWithValue("$limitAmount", b.LimitAmount);
            insert.Parameters.AddWithValue("$createdAt", b.CreatedAt.ToString("O"));
            insert.Parameters.AddWithValue("$updatedAt", (object?)b.UpdatedAt?.ToString("O") ?? DBNull.Value);
            insert.Parameters.AddWithValue("$isDeleted", b.IsDeleted);
        }
        else
        {
            // Collision: multiple original accounts had a budget for the same
            // category+month+year. Merge per BudgetCollisionStrategy ("Sum") —
            // treated as each person's slice of one combined household cap.
            var summed = list.Sum(b => b.LimitAmount);
            var earliestCreated = list.Min(b => b.CreatedAt);
            var latestUpdated = list.Max(b => b.UpdatedAt ?? b.CreatedAt);
            var anyDeleted = list.All(b => b.IsDeleted);

            Console.WriteLine(
                $"  [budget merge] category={list[0].CategoryId} {list[0].Month}/{list[0].Year}: " +
                $"{string.Join(" + ", list.Select(b => $"#{b.Id}={b.LimitAmount}"))} => {summed} ({BudgetCollisionStrategy})");

            insert.CommandText = @"
                INSERT INTO Budgets (UserId, CategoryId, Month, Year, LimitAmount, CreatedAt, UpdatedAt, IsDeleted)
                VALUES ($userId, $categoryId, $month, $year, $limitAmount, $createdAt, $updatedAt, $isDeleted);";
            insert.Parameters.AddWithValue("$userId", adminUserId);
            insert.Parameters.AddWithValue("$categoryId", list[0].CategoryId);
            insert.Parameters.AddWithValue("$month", list[0].Month);
            insert.Parameters.AddWithValue("$year", list[0].Year);
            insert.Parameters.AddWithValue("$limitAmount", summed);
            insert.Parameters.AddWithValue("$createdAt", earliestCreated.ToString("O"));
            insert.Parameters.AddWithValue("$updatedAt", latestUpdated.ToString("O"));
            insert.Parameters.AddWithValue("$isDeleted", anyDeleted);
        }

        insert.ExecuteNonQuery();
        inserted++;
    }

    return inserted;
}

// ─── Verification ──────────────────────────────────────────────────────────

static void PrintVerification(SqliteConnection target, SqliteTransaction tx)
{
    Console.WriteLine("--- Verification (against uncommitted target state) ---");

    using (var cmd = target.CreateCommand())
    {
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT COUNT(*) FROM Transactions";
        Console.WriteLine($"Transactions total: {cmd.ExecuteScalar()}");
    }

    using (var cmd = target.CreateCommand())
    {
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT m.Name, COUNT(*) FROM Transactions t
            JOIN Members m ON m.Id = t.MemberId
            GROUP BY m.Name ORDER BY m.Name";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            Console.WriteLine($"  {reader.GetString(0)}: {reader.GetInt32(1)}");
    }

    using (var cmd = target.CreateCommand())
    {
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT Type, SUM(Amount) FROM Transactions GROUP BY Type";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            Console.WriteLine($"  Type {reader.GetInt32(0)} sum: {reader.GetDecimal(1)}");
    }

    using (var cmd = target.CreateCommand())
    {
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT COUNT(*) FROM Categories";
        Console.WriteLine($"Categories total: {cmd.ExecuteScalar()}");
    }

    using (var cmd = target.CreateCommand())
    {
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT COUNT(*) FROM Budgets";
        Console.WriteLine($"Budgets total: {cmd.ExecuteScalar()}");
    }

    using (var cmd = target.CreateCommand())
    {
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT COUNT(*) FROM IdentityUsers";
        Console.WriteLine($"IdentityUsers total: {cmd.ExecuteScalar()}");
    }

    using (var cmd = target.CreateCommand())
    {
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT COUNT(*) FROM Transactions t
            LEFT JOIN Categories c ON c.Id = t.CategoryId
            WHERE c.Id IS NULL";
        Console.WriteLine($"Transactions with dangling CategoryId: {cmd.ExecuteScalar()}");
    }

    using (var cmd = target.CreateCommand())
    {
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT COUNT(*) FROM Transactions t
            WHERE t.MemberId IS NOT NULL
              AND NOT EXISTS (SELECT 1 FROM Members m WHERE m.Id = t.MemberId)";
        Console.WriteLine($"Transactions with dangling MemberId: {cmd.ExecuteScalar()}");
    }
}

// ─── CLI ────────────────────────────────────────────────────────────────────

static Options? ParseArgs(string[] args)
{
    string? targetDb = null;
    var sourceConn = DefaultSourceConnectionString;
    var dryRun = false;

    for (var i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--target-db":
                targetDb = args[++i];
                break;
            case "--source-db":
                sourceConn = args[++i];
                break;
            case "--dry-run":
                dryRun = true;
                break;
        }
    }

    if (targetDb is null)
        return null;

    return new Options(sourceConn, targetDb, dryRun);
}

static void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run -- --export-xlsx <output-path.xlsx> [--source-db <connection-string>]");
    Console.WriteLine("      Exports source data as an .xlsx matching the app's own Import wizard template");
    Console.WriteLine("      (no database writes at all — import it through the app's UI yourself).");
    Console.WriteLine();
    Console.WriteLine("  dotnet run -- --target-db <path-to-sqlite-file> [--source-db <connection-string>] [--dry-run]");
    Console.WriteLine("      Writes directly into a SQLite database file.");
    Console.WriteLine();
    Console.WriteLine("  --target-db   Required. Path to a SQLite file that the desktop app has already");
    Console.WriteLine("                booted against at least once (so schema + seeds already exist).");
    Console.WriteLine("  --source-db   Optional. Defaults to the local SQL Server instance:");
    Console.WriteLine($"                {DefaultSourceConnectionString}");
    Console.WriteLine("  --dry-run     Run everything, print the verification report, then roll back");
    Console.WriteLine("                instead of committing — the target file is left untouched.");
}

record Options(string SourceConnectionString, string TargetDbPath, bool DryRun);
