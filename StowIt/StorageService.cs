using Microsoft.EntityFrameworkCore;

namespace StowIt.Core
{
    public class StorageService
    {
        private readonly StowItDbContext _db;

        public StorageService(StowItDbContext db)
        {
            _db = db;
        }

        // Boxes
        public IReadOnlyList<Box> GetAllBoxes()
        {
            return _db.Boxes.Include(b => b.Items).ToList();
        }

        public void AddBox(Box box)
        {
            ArgumentNullException.ThrowIfNull(box);
            _db.Boxes.Add(box);
            _db.SaveChanges();
        }

        public bool RemoveBox(Guid id)
        {
            var box = _db.Boxes.FirstOrDefault(b => b.Id == id);
            if (box == null)
            {
                return false;
            }

            _db.Boxes.Remove(box);   
            _db.SaveChanges();
            return true;
        }

        public Box? FindBoxById(Guid id)
        {
            return _db.Boxes.Include(b => b.Items).FirstOrDefault(b => b.Id == id);
        }

        public IReadOnlyList<Box> FindBoxesByItem(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return [];
            }

            var terms = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return GetAllBoxes()
                .Where(b => b.Items.Any(i => terms.All(t => i.Name.Contains(t, StringComparison.OrdinalIgnoreCase))))
                .ToList();
        }

        // Items
        public Item? FindItemById(Guid id)
        {
            return _db.Items.FirstOrDefault(i => i.Id == id);
        }

        public IReadOnlyList<Item> GetItemInCategory(Category category)
        {
            if (category == Category.None)
            {
                return [];
            }

            return _db.Items.Where(i => (i.Category & category) == category).ToList();
        }

        public bool AddCategory(Guid itemId, Category category)
        {
            var item = FindItemById(itemId);
            if (item == null)
            {
                return false;
            }

            item.Category |= category;
            _db.SaveChanges();
            return true;
        }

        public bool RemoveCategory(Guid itemId, Category category)
        {
            var item = FindItemById(itemId);
            if (item == null)
            {
                return false;
            }

            item.Category &= ~category;
            _db.SaveChanges();
            return true;
        }

        public bool AddItemToBox(Guid boxId, Item item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var box = _db.Boxes.FirstOrDefault(b => b.Id == boxId);
            if (box == null)
            {
                return false;
            }

            box.Items.Add(item);
            _db.SaveChanges();
            return true;
        }

        public bool RemoveItemFromBox(Guid boxId, Guid itemId)
        {
            var box = FindBoxById(boxId);
            var item = box?.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                return false;
            }

            _db.Items.Remove(item);
            _db.SaveChanges();
            return true;
        }
    }
}