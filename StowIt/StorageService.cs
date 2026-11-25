using Microsoft.EntityFrameworkCore;

namespace StowIt.Core
{
    public class StorageService
    {
        private readonly StowItDbContext _db;
        private readonly string _photoFolder;

        public StorageService(StowItDbContext db, string photoFolder)
        {
            _db = db;
            _photoFolder = photoFolder;
            Directory.CreateDirectory(_photoFolder);
        }

        // Photos
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

        public bool SetItemPhoto(Guid itemId, Stream photo, string extension)
        {
            ArgumentNullException.ThrowIfNull(photo);
            ArgumentException.ThrowIfNullOrWhiteSpace(extension);

            if (!extension.StartsWith('.'))
            {
                extension = "." + extension;
            }

            extension = extension.ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"Unsupported photo type: {extension}", nameof(extension));
            }

            var item = _db.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return false;

            var fileName = $"{Guid.NewGuid()}{extension}";
            using (var file = File.Create(Path.Combine(_photoFolder, fileName)))
            {
                photo.CopyTo(file);
            }

            DeletePhotoFile(item.PhotoFileName);
            item.PhotoFileName = fileName;
            _db.SaveChanges();
            return true;
        }

        public string? GetPhotoPath(Item item) =>
            item.PhotoFileName == null ? null : Path.Combine(_photoFolder, item.PhotoFileName);

        private void DeletePhotoFile(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var path = Path.Combine(_photoFolder, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        // Boxes
        public IReadOnlyList<Box> GetAllBoxes()
        {
            return _db.Boxes.Include(b => b.Items).ToList();
        }

        public Box AddBox(Box box)
        {
            ArgumentNullException.ThrowIfNull(box);
            ArgumentException.ThrowIfNullOrWhiteSpace(box.Label);

            _db.Boxes.Add(box);
            _db.SaveChanges();
            return box;
        }

        public bool RemoveBox(Guid id)
        {
            var box = _db.Boxes.FirstOrDefault(b => b.Id == id);
            if (box == null)
            {
                return false;
            }

            foreach (var item in box.Items)
            {
                DeletePhotoFile(item.PhotoFileName);
            }

            _db.Boxes.Remove(box);   
            _db.SaveChanges();
            return true;
        }

        public bool UpdateBox(Guid id, string label, StorageArea? location)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(label);

            var box = _db.Boxes.FirstOrDefault(b => b.Id == id);
            if (box == null)
            {
                return false;
            }

            box.Label = label;
            box.Location = location;
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

        public bool MoveItem(Guid itemId, Guid fromBoxId, Guid toBoxId)
        {
            if (fromBoxId == toBoxId)
            {
                return false;
            }

            var from = FindBoxById(fromBoxId);   // includes Items
            var to = FindBoxById(toBoxId);
            if (from == null || to == null)
            {
                return false;
            }

            var item = from.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                return false;
            }

            from.Items.Remove(item);
            to.Items.Add(item);
            _db.SaveChanges();
            return true;
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

            DeletePhotoFile(item.PhotoFileName);
            _db.Items.Remove(item);
            _db.SaveChanges();
            return true;
        }
    }
}