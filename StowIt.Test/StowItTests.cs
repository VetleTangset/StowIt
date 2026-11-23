using StowIt.Core;

namespace StowIt.Test
{
    public class StowItTests
    {
        // Boxes
        [Fact]
        public void AddBox_NullBox_ThrowArgumentNullExeption()
        {
            var service = new StorageService();

            Assert.Throws<ArgumentNullException>(() => service.AddBox(null!));
        }

        [Fact]
        public void AddBox_NonNullBox_AddsBoxToList()
        {
            var service = new StorageService();
            var box = new Box("Test");

            service.AddBox(box);

            var storedBox = Assert.Single(service.GetAllBoxes());
            Assert.Equal(box.Id, storedBox.Id);
        }

        [Fact]
        public void RemoveBox_UnknownId_ReturnFalse()
        {
            var service = new StorageService();

            var result = service.RemoveBox(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public void RemoveBox_ExistingId_ReturnTrue()
        {
            var service = new StorageService();
            var box = new Box("Test");
            service.AddBox(box);

            var result = service.RemoveBox(box.Id);

            Assert.True(result);
            Assert.Empty(service.GetAllBoxes());
        }

        [Fact]
        public void FindBoxById_NonExistingId_ReturnNull()
        {
            var service = new StorageService();

            var result = service.FindBoxById(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public void FindBoxById_ExistingId_ReturnNotNull()
        {
            var service = new StorageService();
            var box = new Box("Test");
            service.AddBox(box);

            var result = service.FindBoxById(box.Id);

            Assert.NotNull(result);
        }

        [Fact]
        public void FindBoxByItem_EmptySearchString_ReturnEmpty()
        {
            var service = new StorageService();

            var result = service.FindBoxsByItem("");

            Assert.Empty(result);
        }

        [Fact]
        public void FindBoxByItem_ExistingSearchString_ReturnBoxContainingItem()
        {
            var service = new StorageService();
            var box = new Box("Test");
            box.Items.Add(new Item("Blue wool sweater"));
            service.AddBox(box);


            var result = service.FindBoxsByItem("wool sweater");

            var foundBox = Assert.Single(result);
            Assert.Equal(box.Id, foundBox.Id);
        }

        //Items
        [Fact]
        public void GetItemInCategory_CategoryNone_ReturnEmptyList()
        {
            var service = new StorageService();

            var result = service.GetItemInCategory(Category.None);

            Assert.Empty(result);
        }

        [Fact]
        public void GetItemInCategory_CombinedCategories_FoundByOneFlag()
        {
            var service = new StorageService();
            var box = new Box("Test");
            box.Items.Add(new Item("Drill") { Category = Category.Tools | Category.Electronics });
            service.AddBox(box);

            var result = service.GetItemInCategory(Category.Tools);

            Assert.Single(result);
        }

        [Fact]
        public void AddCategory_ExistingItem_KeepsOldAndAddsNew()
        {
            var service = new StorageService();
            var box = new Box("Test");
            var item = new Item("Drill") { Category = Category.Tools };
            box.Items.Add(item);
            service.AddBox(box);

            var result = service.AddCategory(item.Id, Category.Electronics);

            Assert.True(result);
            Assert.Equal(Category.Tools | Category.Electronics, item.Category);
        }

        [Fact]
        public void AddCategory_UnknownItem_ReturnsFalse()
        {
            var service = new StorageService();

            var result = service.AddCategory(Guid.NewGuid(), Category.Tools);

            Assert.False(result);
        }

        [Fact]
        public void RemoveCategory_ExistingItem_RemovesOnlyThatCategory()
        {
            var service = new StorageService();
            var box = new Box("Test");
            var item = new Item("Drill") { Category = Category.Tools | Category.Electronics };
            box.Items.Add(item);
            service.AddBox(box);

            var result = service.RemoveCategory(item.Id, Category.Tools);

            Assert.True(result);
            Assert.Equal(Category.Electronics, item.Category);
        }

        [Fact]
        public void AddItemToBox_NullItem_ThrowArgumentNullExeption()
        {
            var service = new StorageService();
            var box = new Box("Test");
            service.AddBox(box);

            Assert.Throws<ArgumentNullException>(() => service.AddItemToBox(box.Id, null!));
        }

        [Fact]
        public void AddItemToBox_ExistingBox_AddItemAndReturnTrue()
        {
            var service = new StorageService();
            var box = new Box("Test");
            service.AddBox(box);

            var result = service.AddItemToBox(box.Id, new Item("Test Item"));

            Assert.True(result);
            Assert.Single(box.Items);
        }

        [Fact]
        public void AddItemToBox_UnknownBox_ReturnFalse()
        {
            var service = new StorageService();

            var result = service.AddItemToBox(Guid.NewGuid(), new Item("Test Item"));

            Assert.False(result);
        }
    }
}