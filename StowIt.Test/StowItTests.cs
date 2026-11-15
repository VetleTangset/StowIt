using StowIt.Core;

namespace StowIt.Test
{
    public class StowItTests
    {
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

            var result = service.FindBoxByItem("");

            Assert.Empty(result);
        }
        [Fact]
        public void AddItemToBox_NullItem_ReturnThrow()
        {
            var service = new StorageService();
            var box = new Box("Test");
            service.AddBox(box);

            Assert.Throws<ArgumentNullException>(() => service.AddItemToBox(box.Id, null!));
        }
        [Fact]
        public void FindBoxByItem_ExistingSearchString_ReturnBoxContainingItem()
        {
            var service = new StorageService();
            var box = new Box("Test");
            box.Items.Add(new Item("Blue wool sweater"));
            service.AddBox(box);


            var result = service.FindBoxByItem("wool sweater");

            var foundBox = Assert.Single(result);
            Assert.Equal(box.Id, foundBox.Id);
        }
    }
}