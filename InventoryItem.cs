namespace LogiTrack.Models;
using System.ComponentModel.DataAnnotations;

public class InventoryItem
{
    [Key]
    public int ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Location { get; set; } = string.Empty;

    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    public void DisplayInfo()
    {
        Console.WriteLine($"Item: {Name} | Quantity: {Quantity} | Location: {Location}");
    }
}