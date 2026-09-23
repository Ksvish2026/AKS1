using AksTyreProduction.Web.Data;using AksTyreProduction.Web.Services;using Microsoft.AspNetCore.Mvc;using Microsoft.EntityFrameworkCore;
namespace AksTyreProduction.Web.Controllers;
public class InvoicesController(AppDbContext db,ProductionService service):Controller
{
    public async Task<IActionResult> Index()=>View(await db.Invoices.Include(x=>x.RetreadJob).ThenInclude(x=>x.Tyre).ThenInclude(x=>x.Customer).OrderByDescending(x=>x.CreatedAt).ToListAsync());
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Create(int jobId,int tyreId,string invoiceNumber){try{await service.CreateInvoiceAsync(jobId,invoiceNumber);TempData["Success"]="Draft invoice created from the calculated selling price.";}catch(Exception ex){TempData["Error"]=ex.Message;}return RedirectToAction("Details","Tyres",new{id=tyreId});}
}
