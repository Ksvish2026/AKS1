using AksTyreProduction.Web.Data;using Microsoft.AspNetCore.Mvc;using Microsoft.EntityFrameworkCore;
namespace AksTyreProduction.Web.Controllers;
public class AdminController(AppDbContext db):Controller {
 public async Task<IActionResult> Index(){ViewBag.Operators=await db.Operators.ToListAsync();ViewBag.Machines=await db.Machines.ToListAsync();ViewBag.Audits=await db.AuditEvents.OrderByDescending(x=>x.OccurredAt).Take(30).ToListAsync();return View();}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> AddOperator(string employeeId,string name,string role,decimal labourRate){db.Operators.Add(new AksTyreProduction.Web.Models.Operator{EmployeeId=employeeId,Name=name,Role=role,LabourRate=labourRate});await db.SaveChangesAsync();TempData["Success"]="Operator added.";return RedirectToAction(nameof(Index));}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> AddMachine(string machineId,string name,string type){db.Machines.Add(new AksTyreProduction.Web.Models.Machine{MachineId=machineId,Name=name,Type=type});await db.SaveChangesAsync();TempData["Success"]="Machine added.";return RedirectToAction(nameof(Index));}
}
