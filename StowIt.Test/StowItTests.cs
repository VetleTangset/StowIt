using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StowIt.Core;

namespace StowIt.Test
{
    public class StowItTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly StowItDbContext _db;
        private readonly StorageService _service;
        private readonly string _photoFolder =
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        public StowItTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<StowItDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new StowItDbContext(options);
            _db.Database.EnsureCreated();
            _service = new StorageService(_db, _photoFolder);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
            if (Directory.Exists(_photoFolder))
            {
                Directory.Delete(_photoFolder, recursive: true);
            }
        }

        // Boxes
        [Fact]
        public void AddBox_NullBox_ThrowArgumentNullExeption()
        {
            Assert.Throws<ArgumentNullException>(() => _service.AddBox(null!));
        }

        [Fact]
        public void AddBox_NonNullBox_AddsBoxToList()
        {
            var box = new Box("Test");

            _service.AddBox(box);

            var storedBox = Assert.Single(_service.GetAllBoxes());
            Assert.Equal(box.Id, storedBox.Id);
        }

        [Fact]
        public void RemoveBox_UnknownId_ReturnFalse()
        {
            var result = _service.RemoveBox(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public void RemoveBox_ExistingId_ReturnTrue()
        {
            var box = new Box("Test");
            _service.AddBox(box);

            var result = _service.RemoveBox(box.Id);

            Assert.True(result);
            Assert.Empty(_service.GetAllBoxes());
        }

        [Fact]
        public void FindBoxById_NonExistingId_ReturnNull()
        {
            var result = _service.FindBoxById(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public void FindBoxById_ExistingId_ReturnNotNull()
        {
            var box = new Box("Test");
            _service.AddBox(box);

            var result = _service.FindBoxById(box.Id);

            Assert.NotNull(result);
        }

        [Fact]
        public void FindBoxByItem_EmptySearchString_ReturnEmpty()
        {
            var result = _service.FindBoxesByItem("");

            Assert.Empty(result);
        }

        [Fact]
        public void FindBoxByItem_ExistingSearchString_ReturnBoxContainingItem()
        {
            var box = new Box("Test");
            box.Items.Add(new Item("Blue wool sweater"));
            _service.AddBox(box);


            var result = _service.FindBoxesByItem("wool sweater");

            var foundBox = Assert.Single(result);
            Assert.Equal(box.Id, foundBox.Id);
        }

        //Items
        [Fact]
        public void GetItemInCategory_CategoryNone_ReturnEmptyList()
        {
            var result = _service.GetItemInCategory(Category.None);

            Assert.Empty(result);
        }

        [Fact]
        public void GetItemInCategory_CombinedCategories_FoundByOneFlag()
        {
            var box = new Box("Test");
            box.Items.Add(new Item("Drill") { Category = Category.Tools | Category.Electronics });
            _service.AddBox(box);

            var result = _service.GetItemInCategory(Category.Tools);

            Assert.Single(result);
        }

        [Fact]
        public void AddCategory_ExistingItem_KeepsOldAndAddsNew()
        {
            var box = new Box("Test");
            var item = new Item("Drill") { Category = Category.Tools };
            box.Items.Add(item);
            _service.AddBox(box);

            var result = _service.AddCategory(item.Id, Category.Electronics);

            Assert.True(result);
            Assert.Equal(Category.Tools | Category.Electronics, item.Category);
        }

        [Fact]
        public void AddCategory_UnknownItem_ReturnsFalse()
        {
            var result = _service.AddCategory(Guid.NewGuid(), Category.Tools);

            Assert.False(result);
        }

        [Fact]
        public void RemoveCategory_ExistingItem_RemovesOnlyThatCategory()
        {
            var box = new Box("Test");
            var item = new Item("Drill") { Category = Category.Tools | Category.Electronics };
            box.Items.Add(item);
            _service.AddBox(box);

            var result = _service.RemoveCategory(item.Id, Category.Tools);

            Assert.True(result);
            Assert.Equal(Category.Electronics, item.Category);
        }

        [Fact]
        public void AddItemToBox_NullItem_ThrowArgumentNullExeption()
        {
            var box = new Box("Test");
            _service.AddBox(box);

            Assert.Throws<ArgumentNullException>(() => _service.AddItemToBox(box.Id, null!));
        }

        [Fact]
        public void AddItemToBox_ExistingBox_AddItemAndReturnTrue()
        {
            var box = new Box("Test");
            _service.AddBox(box);

            var result = _service.AddItemToBox(box.Id, new Item("Test Item"));

            Assert.True(result);
            Assert.Single(box.Items);
        }

        [Fact]
        public void AddItemToBox_UnknownBox_ReturnFalse()
        {
            var result = _service.AddItemToBox(Guid.NewGuid(), new Item("Test Item"));

            Assert.False(result);
        }

        // Db
        [Fact]
        public void SavedData_IsLoadedByNewContext()
        {
            var box = new Box("Winter clothes") { Location = StorageArea.Basement };
            box.Items.Add(new Item("Blue wool sweater") { Category = Category.Clothing | Category.Seasonal });
            _service.AddBox(box);

            var options = new DbContextOptionsBuilder<StowItDbContext>()
                .UseSqlite(_connection)
                .Options;
            using var freshDb = new StowItDbContext(options);
            var freshService = new StorageService(freshDb, _photoFolder);

            var loaded = Assert.Single(freshService.GetAllBoxes());
            Assert.Equal(box.Id, loaded.Id);
            Assert.Equal(StorageArea.Basement, loaded.Location);

            var item = Assert.Single(loaded.Items);
            Assert.Equal("Blue wool sweater", item.Name);
            Assert.Equal(Category.Clothing | Category.Seasonal, item.Category);
        }

        [Fact]
        public void CategorySearch_RunsAgainstDatabase_FindsCombinedItem()
        {
            var box = new Box("Test");
            box.Items.Add(new Item("Drill") { Category = Category.Tools | Category.Electronics });
            box.Items.Add(new Item("Scarf") { Category = Category.Clothing });
            _service.AddBox(box);

            var result = _service.GetItemInCategory(Category.Tools);

            var found = Assert.Single(result);
            Assert.Equal("Drill", found.Name);
        }

        [Fact]
        public void FindBoxesByItem_NorwegianCharacters_IsCaseInsensitive()
        {
            var box = new Box("Kjøkken");
            box.Items.Add(new Item("Æblekasse"));
            _service.AddBox(box);

            var result = _service.FindBoxesByItem("æblekasse");

            Assert.Single(result);
        }

        // Photos
        [Fact]
        public void SetItemPhoto_ExistingItem_SavesFileAndFileName()
        {
            var box = new Box("Test");
            var item = new Item("Blue wool sweater");
            box.Items.Add(item);
            _service.AddBox(box);

            using var stream = new MemoryStream([1, 2, 3]);
            var result = _service.SetItemPhoto(item.Id, stream, ".jpg");

            Assert.True(result);
            Assert.NotNull(item.PhotoFileName);
            Assert.True(File.Exists(Path.Combine(_photoFolder, item.PhotoFileName)));
        }

        [Fact]
        public void SetItemPhoto_UnknownItem_ReturnsFalse()
        {
            using var stream = new MemoryStream([1, 2, 3]);

            var result = _service.SetItemPhoto(Guid.NewGuid(), stream, ".jpg");

            Assert.False(result);
        }

        [Fact]
        public void SetItemPhoto_BadExtension_Throws()
        {
            var box = new Box("Test");
            var item = new Item("Blue wool sweater");
            box.Items.Add(item);
            _service.AddBox(box);

            using var stream = new MemoryStream([1, 2, 3]);

            Assert.Throws<ArgumentException>(() => _service.SetItemPhoto(item.Id, stream, ".exe"));
        }

        [Fact]
        public void SetItemPhoto_ReplacingPhoto_DeletesOldFile()
        {
            var box = new Box("Test");
            var item = new Item("Blue wool sweater");
            box.Items.Add(item);
            _service.AddBox(box);

            using (var first = new MemoryStream([1, 2, 3]))
            {
                _service.SetItemPhoto(item.Id, first, ".jpg");
            }
            var oldFileName = item.PhotoFileName!;

            using (var second = new MemoryStream([4, 5, 6]))
            {
                _service.SetItemPhoto(item.Id, second, ".jpg");
            }

            Assert.NotEqual(oldFileName, item.PhotoFileName);
            Assert.False(File.Exists(Path.Combine(_photoFolder, oldFileName)));
            Assert.True(File.Exists(Path.Combine(_photoFolder, item.PhotoFileName!)));
        }

        [Fact]
        public void RemoveItemFromBox_ItemWithPhoto_DeletesPhotoFile()
        {
            var box = new Box("Test");
            var item = new Item("Blue wool sweater");
            box.Items.Add(item);
            _service.AddBox(box);

            using (var stream = new MemoryStream([1, 2, 3]))
            {
                _service.SetItemPhoto(item.Id, stream, ".jpg");
            }
            var photoPath = Path.Combine(_photoFolder, item.PhotoFileName!);
            Assert.True(File.Exists(photoPath));   // checks if the file is there before removal

            var result = _service.RemoveItemFromBox(box.Id, item.Id);

            Assert.True(result);
            Assert.False(File.Exists(photoPath));
            Assert.Null(_service.FindItemById(item.Id));
        }

        [Fact]
        public void RemoveBox_BoxWithPhotoItems_DeletesAllPhotoFiles()
        {
            var box = new Box("Test");
            var sweater = new Item("Blue wool sweater");
            var scarf = new Item("Red scarf");
            box.Items.Add(sweater);
            box.Items.Add(scarf);
            _service.AddBox(box);

            using (var s1 = new MemoryStream([1, 2, 3]))
            {
                _service.SetItemPhoto(sweater.Id, s1, ".jpg");
            }
            using (var s2 = new MemoryStream([4, 5, 6]))
            {
                _service.SetItemPhoto(scarf.Id, s2, ".png");
            }
            var path1 = Path.Combine(_photoFolder, sweater.PhotoFileName!);
            var path2 = Path.Combine(_photoFolder, scarf.PhotoFileName!);

            var result = _service.RemoveBox(box.Id);

            Assert.True(result);
            Assert.False(File.Exists(path1));
            Assert.False(File.Exists(path2));
        }
    }
}