using AksTyreProduction.Web.Data;
using AksTyreProduction.Web.Models;
using AksTyreProduction.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AksTyreProduction.Web.Controllers;
public class MaterialsController(AppDbContext db,ProductionService service) : Controller
{
    public async Task<IActionResult> Index()=>View(await db.Materials.Include(x=>x.Batches).OrderBy(x=>x.Name).ToListAsync());
    public async Task<IActionResult> Traceability(string? lot){ViewBag.Lot=lot;return View(string.IsNullOrWhiteSpace(lot)?[]:await db.MaterialUsages.Include(x=>x.Material).Include(x=>x.MaterialBatch).Include(x=>x.RetreadJob).ThenInclude(x=>x.Tyre).ThenInclude(x=>x.Customer).Where(x=>x.MaterialBatch!.LotNumber.Contains(lot)).ToListAsync());}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Use(int jobId,int materialId,decimal quantity,int? batchId,int? transactionId,int tyreId){try{await service.AddMaterialAsync(jobId,materialId,quantity,batchId,transactionId);TempData["Success"]="Material usage and historical price recorded.";}catch(Exception ex){TempData["Error"]=ex.Message;}return RedirectToAction("Details","Tyres",new{id=tyreId});}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Update(int id,decimal currentUnitCost,decimal quantityOnHand,decimal reorderLevel){if(!User.IsInRole(ApplicationRoles.Administrator) && !User.IsInRole(ApplicationRoles.Management)){TempData["Error"]="Only Administrator or Management may change material pricing.";return RedirectToAction(nameof(Index));}var material=await db.Materials.FindAsync(id);if(material!=null){material.CurrentUnitCost=currentUnitCost;material.QuantityOnHand=quantityOnHand;material.ReorderLevel=reorderLevel;await db.SaveChangesAsync();TempData["Success"]="Material master updated. Historical usage costs were not changed.";}return RedirectToAction(nameof(Index));}
}
