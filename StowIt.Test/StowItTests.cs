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
    }
}