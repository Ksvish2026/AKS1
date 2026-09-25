using AksTyreProduction.Web.Models;

namespace AksTyreProduction.Tests;

public class AuthModelTests
{
    [Fact]
    public void AllowedRolesIncludeManagementAndQc()
    {
        Assert.Contains("Administrator", ApplicationRoles.All);
        Assert.Contains("Management", ApplicationRoles.All);
        Assert.Contains("QC", ApplicationRoles.All);
    }

    [Fact]
    public void SelectedRoleFallsBackToDefaultRole()
    {
        var user = new AppUser { RolesCsv = "Administrator,Management", DefaultRole = "Management" };
        Assert.Equal("Management", user.SelectedRole);
    }
}
