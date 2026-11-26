using Microsoft.EntityFrameworkCore;
using StowIt.Core;

var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "StowIt", "stowit.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

var photoFolder = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "StowIt", "photos");

var options = new DbContextOptionsBuilder<StowItDbContext>()
    .UseSqlite($"Data Source={dbPath}")
    .Options;

using var db = new StowItDbContext(options);
db.Database.Migrate();

var service = new StorageService(db, photoFolder);



var box = service.GetAllBoxes().FirstOrDefault();
if (box == null)
{
    box = new Box("Winter clothes") { Location = StorageArea.Basement };
    service.AddBox(box);
}

var item = new Item("Blue wool sweater") { Category = Category.Clothing | Category.Seasonal };
service.AddItemToBox(box.Id, item);


for (int i = 0; i < 2; i++)
{
    using var stream = File.OpenRead(@"C:\Users\vetlu\Downloads\Doge_Andy.jpg");
    service.SetItemPhoto(item.Id, stream, ".jpg");
}

Console.WriteLine(service.GetPhotoPath(item));