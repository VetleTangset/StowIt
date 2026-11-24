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

        public StowItTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();   // the in-memory database lives as long as this connection

            var options = new DbContextOptionsBuilder<StowItDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new StowItDbContext(options);
            _db.Database.EnsureCreated();
            _service = new StorageService(_db);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
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
    }
}