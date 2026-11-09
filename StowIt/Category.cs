namespace StowIt.Core
{
    [Flags]
    public enum Category
    {
        None = 0,
        Clothing = 1,
        Shoes = 2,
        Electronics = 4,
        Books = 8,
        Appliances = 16,
        Jackets = 32,
        Seasonal = 64,
        Miscellaneous = 128,
        Decor = 256,
        Hobbies = 512,
        Tools = 1024,
        Bathroom = 2048,
        Kitchen = 4096
    }
}
