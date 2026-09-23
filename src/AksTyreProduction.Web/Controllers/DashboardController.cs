using AksTyreProduction.Web.Data;
using AksTyreProduction.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AksTyreProduction.Web.Controllers;
public class DashboardController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var jobs=await db.RetreadJobs.Include(x=>x.MaterialUsages).Include(x=>x.StationTransactions).ThenInclude(x=>x.Operator).Include(x=>x.StationTransactions).ThenInclude(x=>x.Machine).Include(x=>x.Tyre).ThenInclude(x=>x.Customer).ToListAsync();
        var tx=jobs.SelectMany(x=>x.StationTransactions).ToList(); var today=DateTime.Today;
        var vm=new DashboardVm {
            ReceivedToday=jobs.Count(x=>x.ReceivedAt.Date==today), InProduction=jobs.Count(x=>x.Status==JobStatus.InProduction), Completed=jobs.Count(x=>x.Status==JobStatus.Dispatched),
            Rejected=jobs.Count(x=>x.Status==JobStatus.Rejected), AwaitingRepair=jobs.Count(x=>x.CurrentStage=="Casing Repair"), AwaitingQc=jobs.Count(x=>x.CurrentStage=="QC Release"), ReadyForDispatch=jobs.Count(x=>x.Status==JobStatus.ReadyForDispatch),
            MaterialCost=jobs.Sum(x=>x.MaterialCost), LabourCost=jobs.Sum(x=>x.LabourCost), TotalCost=jobs.Sum(x=>x.TotalCost),
            MaterialCostsByCategory=await db.MaterialUsages.Include(x=>x.Material).GroupBy(x=>x.Material.Category).ToDictionaryAsync(x=>x.Key,x=>x.Sum(u=>u.TotalCost)),
            Wip=jobs.Where(x=>x.Status==JobStatus.InProduction).GroupBy(x=>x.CurrentStage).ToDictionary(x=>x.Key,x=>x.Count()),
            AverageMinutes=tx.Where(x=>x.EndedAt!=null).GroupBy(x=>x.Station).ToDictionary(x=>x.Key,x=>Math.Round(x.Average(t=>t.DurationMinutes),1)),
            Recent=tx.OrderByDescending(x=>x.StartedAt).Take(8).ToList(), Attention=jobs.Where(x=>x.Status==JobStatus.Rejected||x.CurrentStage=="Casing Repair").Take(8).ToList(),
            OperatorProductivity=tx.Where(x=>x.EndedAt!=null).GroupBy(x=>x.Operator?.Name??"Unknown").ToDictionary(x=>x.Key,x=>x.Count()),
            MachineUtilisationMinutes=tx.Where(x=>x.EndedAt!=null&&x.Machine!=null).GroupBy(x=>x.Machine!.Name).ToDictionary(x=>x.Key,x=>(double)x.Sum(t=>t.DurationMinutes)),
            Bottleneck=jobs.Where(x=>x.Status==JobStatus.InProduction).GroupBy(x=>x.CurrentStage).OrderByDescending(x=>x.Count()).Select(x=>$"{x.Key} ({x.Count()} tyres)").FirstOrDefault()??"None"
        }; return View(vm);
    }
}
