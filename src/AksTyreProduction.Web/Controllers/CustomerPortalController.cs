using AksTyreProduction.Web.Data;
using AksTyreProduction.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AksTyreProduction.Web.Controllers;
[AllowAnonymous]
public class CustomerPortalController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        if(string.IsNullOrWhiteSpace(q))return View(null);
        var tyre=await db.Tyres.Include(x=>x.Customer).Include(x=>x.RetreadJobs).ThenInclude(x=>x.StationTransactions).SingleOrDefaultAsync(x=>x.AksTyreId==q||x.SerialNumber==q);
        if(tyre==null){ViewBag.Error="No tyre found for that reference.";return View(null);}var job=tyre.RetreadJobs.OrderByDescending(x=>x.RetreadNumber).First();
        return View(new CustomerProgressDto(tyre.AksTyreId,tyre.Customer.Name,tyre.Brand,tyre.Size,tyre.SerialNumber,job.RetreadNumber,job.CurrentStage,job.EstimatedCompletion,job.StationTransactions.Where(x=>x.EndedAt!=null&&x.Result!=TransactionResult.Fail).Select(x=>x.Station).Distinct().ToList()));
    }
}
