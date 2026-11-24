using Microsoft.EntityFrameworkCore;
using StowIt.Core;

var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "StowIt", "stowit.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

var options = new DbContextOptionsBuilder<StowItDbContext>()
    .UseSqlite($"Data Source={dbPath}")
    .Options;

using var db = new StowItDbContext(options);
db.Database.Migrate();

var service = new StorageService(db);