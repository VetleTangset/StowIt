using System.Xml.Serialization;

namespace StowIt.Core
{
    public class StorageService
    {
        private readonly List<Box> _boxes = [];
        public void AddBox(Box box)
        {
            ArgumentNullException.ThrowIfNull(box);
            _boxes.Add(box);
        }
        public bool RemoveBox(Guid id)
        {
            return _boxes.RemoveAll(b => b.Id == id) > 0;
        }
        public Box? FindBoxById(Guid id)
        {
            return _boxes.FirstOrDefault(b => b.Id == id);
        }
        public IReadOnlyList<Box> FindBoxByItem(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return [];
            }

            var terms = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return _boxes.Where(b => b.Items.Any(i => terms.All(t => i.Name.Contains(t, StringComparison.OrdinalIgnoreCase)))).ToList();
        }
        public IReadOnlyList<Item> ItemInCategory(Category category)
        {
            if (category == Category.None)
            {
                return [];
            }
            var itemsInCategory = _boxes.SelectMany(b => b.Items).Where(i => i.Category.HasFlag(category)).ToList();
            return itemsInCategory;
        }
        public IReadOnlyList<Box> GetAllBoxes() => _boxes.AsReadOnly();
        public bool AddItemToBox(Guid boxID, Item item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var box = FindBoxById(boxID);
            if (box == null)
            {
                return false;
            }

            box.Items.Add(item);
            return true;
        }
        public bool RemoveItemFromBox(Guid id, Item item)
        {
            ArgumentNullException.ThrowIfNull(item);
            var box = _boxes.FirstOrDefault(i => i.Id == id);
            if (box == null)
            {
                return false;
            }

            box.Items.Remove(item);
            return true;

        }
    }
}
