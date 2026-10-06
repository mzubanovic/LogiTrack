using LogiTrack.Data;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrderController : ControllerBase
{
	private readonly LogiTrackContext _context;

	public OrderController(LogiTrackContext context)
	{
		_context = context;
	}

	[HttpGet]
	public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
	{
		var orders = await _context.Orders
			.AsNoTracking()
			.Select(order => new
			{
				order.OrderId,
				order.CustomerName,
				order.DatePlaced,
				Items = order.Items.Select(item => new
				{
					item.ItemId,
					item.Name,
					item.Quantity,
					item.Location,
					item.OrderId
				}).ToList()
			})
			.ToListAsync(cancellationToken);

		return Ok(orders);
	}

	[HttpGet("{id:int}")]
	public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
	{
		var order = await _context.Orders
			.AsNoTracking()
			.Where(order => order.OrderId == id)
			.Select(order => new
			{
				order.OrderId,
				order.CustomerName,
				order.DatePlaced,
				Items = order.Items.Select(item => new
				{
					item.ItemId,
					item.Name,
					item.Quantity,
					item.Location,
					item.OrderId
				}).ToList()
			})
			.FirstOrDefaultAsync(cancellationToken);

		if (order is null)
		{
			return NotFound();
		}

		return Ok(order);
	}

	[HttpPost]
	public async Task<IActionResult> Create(Order order, CancellationToken cancellationToken)
	{
		var itemIds = order.Items.Select(item => item.ItemId).Distinct().ToList();
		var inventoryItems = await _context.InventoryItems
			.Where(item => itemIds.Contains(item.ItemId))
			.ToListAsync(cancellationToken);

		if (inventoryItems.Count != itemIds.Count)
		{
			return NotFound();
		}

		order.Items = inventoryItems;
		_context.Orders.Add(order);
		await _context.SaveChangesAsync(cancellationToken);

		return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, ToResponse(order));
	}

	private static object ToResponse(Order order)
	{
		return new
		{
			order.OrderId,
			order.CustomerName,
			order.DatePlaced,
			Items = order.Items.Select(item => new
			{
				item.ItemId,
				item.Name,
				item.Quantity,
				item.Location,
				item.OrderId
			})
		};
	}

	[HttpDelete("{id:int}")]
	public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
	{
		var order = await _context.Orders
			.Include(order => order.Items)
			.FirstOrDefaultAsync(order => order.OrderId == id, cancellationToken);

		if (order is null)
		{
			return NotFound();
		}

		_context.Orders.Remove(order);
		await _context.SaveChangesAsync(cancellationToken);

		return NoContent();
	}

}
