using LogiTrack.Data;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Data.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController(LogiTrackContext context) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<List<InventoryItem>>> GetAll(CancellationToken cancellationToken)
	{
		return await context.InventoryItems.ToListAsync(cancellationToken);
	}

	[HttpPost]
	public async Task<ActionResult<InventoryItem>> Create(
		InventoryItem item,
		CancellationToken cancellationToken)
	{
		context.InventoryItems.Add(item);
		await context.SaveChangesAsync(cancellationToken);

		return StatusCode(StatusCodes.Status201Created, item);
	}

	[HttpDelete("{id:int}")]
	public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
	{
		var item = await context.InventoryItems.FindAsync([id], cancellationToken);
		if (item is null)
		{
			return NotFound();
		}

		context.InventoryItems.Remove(item);
		await context.SaveChangesAsync(cancellationToken);

		return NoContent();
	}
}
