
public class Building
{
    public string Name { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Building(string name, int width, int height)
    {
        Name = name;
        Width = width;
        Height = height;
    }
}