namespace Matomo.Maui.Models;

public class Dimension
{
    public Dimension(string name, int id, string currentValue = "")
    {
        Name = name;
        Id = id;
        Value = currentValue;
    }

    public Dimension(string name, int id, int currentValue) : this(name, id, currentValue.ToString())
    {

    }

    public string Name { get; set; }
    public int Id { get; set; }
    public string Value { get; set; }
}
