namespace StowIt.Core
{
    public class StorageService
    {
        // Boxes
        private readonly List<Box> _boxes = [];

        public IReadOnlyList<Box> GetAllBoxes() => _boxes.AsReadOnly();

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

        public IReadOnlyList<Box> FindBoxsByItem(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return [];
            }

            var terms = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return _boxes.Where(b => b.Items.Any(i => terms.All(t => i.Name.Contains(t, StringComparison.OrdinalIgnoreCase)))).ToList();
        }

        //Items
        private IEnumerable<Item> AllItems() => _boxes.SelectMany(b => b.Items);

        public Item? FindItemById(Guid id)
        {
            return _boxes.SelectMany(b => b.Items).FirstOrDefault(i => i.Id == id);
        }

        public IReadOnlyList<Item> GetItemInCategory(Category category)
        {
            if (category == Category.None)
            {
                return [];
            }
            return AllItems().Where(i => i.Category.HasFlag(category)).ToList();
        }

        public bool AddCategory(Guid itemId, Category category)
        {
            var item = FindItemById(itemId);
            if (item == null)
            {
                return false;
            }

            item.Category |= category;
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
            return true;
        }

        public bool AddItemToBox(Guid boxId, Item item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var box = FindBoxById(boxId);
            if (box == null)
            {
                return false;
            }

            box.Items.Add(item);
            return true;
        }

        public bool RemoveItemFromBox(Guid boxId, Guid itemId)
        {
            var box = FindBoxById(boxId);
            if (box == null)
            {
                return false;
            }
            
            return box.Items.RemoveAll(i => i.Id == itemId) > 0;
        }
    }
}
