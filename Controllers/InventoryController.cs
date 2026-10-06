using LogiTrack.Data;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LogiTrack.Data.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory")]
public class InventoryController(LogiTrackContext context, IMemoryCache cache) : ControllerBase
{
	private const string InventoryCacheKey = "inventory-items";

	[HttpGet]
	public async Task<ActionResult<List<InventoryItem>>> GetAll(CancellationToken cancellationToken)
	{
		var items = await cache.GetOrCreateAsync(
			InventoryCacheKey,
			async entry =>
			{
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
				return await context.InventoryItems
					.AsNoTracking()
					.ToListAsync(cancellationToken);
			});

		return Ok(items ?? new List<InventoryItem>());
	}

	[HttpPost]
	public async Task<ActionResult<InventoryItem>> Create(
		InventoryItem item,
		CancellationToken cancellationToken)
	{
		context.InventoryItems.Add(item);
		await context.SaveChangesAsync(cancellationToken);
		cache.Remove(InventoryCacheKey);

		return StatusCode(StatusCodes.Status201Created, item);
	}

	[HttpDelete("{id:int}")]
	[Authorize(Roles = "Manager")]
	public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
	{
		var item = await context.InventoryItems.FindAsync([id], cancellationToken);
		if (item is null)
		{
			return NotFound();
		}

		context.InventoryItems.Remove(item);
		await context.SaveChangesAsync(cancellationToken);
		cache.Remove(InventoryCacheKey);

		return NoContent();
	}
}
