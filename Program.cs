using System.Linq;
using LogiTrack.Data;

using (var context = new LogiTrackContext())
{
    if (!context.InventoryItems.Any())
    {
        context.InventoryItems.Add(new LogiTrack.Models.InventoryItem
        {
            Name = "Pallet Jack",
            Quantity = 12,
            Location = "Warehouse A"
        });

        context.SaveChanges();
    }

    var items = context.InventoryItems.ToList();

    foreach (var item in items)
    {
        item.DisplayInfo();
    }
}