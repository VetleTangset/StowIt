namespace StowIt.Core
{
    public class Item
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public Category Category { get; set; }

        public Item(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"{Name} ({Category})";
        }
    }
}
