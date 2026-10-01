using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models;

public class Order
{
    [Key]
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime DatePlaced { get; set; }

    public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();

    public void AddItem(InventoryItem item)
    {
        Items.Add(item);
        item.Order = this;
        item.OrderId = OrderId;
    }

    public void RemoveItem(int itemId)
    {
        var item = Items.FirstOrDefault(i => i.ItemId == itemId);

        if (item != null)
        {
            Items.Remove(item);
            item.Order = null;
            item.OrderId = null;
        }
    }

    public string GetOrderSummary()
    {
        return $"Order #{OrderId} for {CustomerName} | Items: {Items.Count} | Placed: {DatePlaced:M/d/yyyy}";
    }
}