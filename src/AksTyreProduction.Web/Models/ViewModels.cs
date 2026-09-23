using System.ComponentModel.DataAnnotations;

namespace AksTyreProduction.Web.Models;

public class DashboardVm
{
    public int ReceivedToday { get; set; } public int InProduction { get; set; } public int Completed { get; set; }
    public int Rejected { get; set; } public int AwaitingRepair { get; set; } public int AwaitingQc { get; set; } public int ReadyForDispatch { get; set; }
    public decimal MaterialCost { get; set; } public decimal LabourCost { get; set; } public decimal TotalCost { get; set; }
    public Dictionary<string,decimal> MaterialCostsByCategory { get; set; }=[];
    public Dictionary<string,int> Wip { get; set; }=[]; public Dictionary<string,double> AverageMinutes { get; set; }=[];
    public List<StationTransaction> Recent { get; set; }=[]; public List<RetreadJob> Attention { get; set; }=[];
    public Dictionary<string,int> OperatorProductivity { get; set; }=[]; public Dictionary<string,double> MachineUtilisationMinutes { get; set; }=[]; public string Bottleneck { get; set; }="None";
}
public class ReceiveVm
{
    [Required] public string Brand { get; set; }="Michelin"; [Required] public string Size { get; set; }="11R22.5";
    [Required] public string SerialNumber { get; set; }=""; [Required] public string JobNumber { get; set; }=""; [Required] public int CustomerId { get; set; }
    public string OriginalTreadPattern { get; set; }=""; public string CasingCondition { get; set; }="Good"; public string Application { get; set; }="Commercial"; public DateTime? EstimatedCompletion { get; set; }
}
public class StationVm
{
    public string Station { get; set; }="Buffing"; public string? TyreCode { get; set; } public RetreadJob? Job { get; set; }
    public List<Operator> Operators { get; set; }=[]; public List<Machine> Machines { get; set; }=[]; public StationTransaction? Active { get; set; }
}
public record CustomerProgressDto(string AksTyreId,string Customer,string Brand,string Size,string SerialNumber,int RetreadNumber,string CurrentStage,DateTime? EstimatedCompletion,IReadOnlyList<string> CompletedStages);
