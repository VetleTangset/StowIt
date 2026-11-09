namespace StowIt.Core
{
// Box class with unique ID, a label, and optional storage area location
    public class Box
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Label { get; set; } = string.Empty;
        public StorageArea? Location { get; set; }
        public List<Item> Items { get; set; } = [];
        public Box(string label)
        {
            Label = label;
        }

        public override string ToString()
        {
            return $"{Label} ({Location?.ToString() ?? "Undefined"})";
        }
    }
}
