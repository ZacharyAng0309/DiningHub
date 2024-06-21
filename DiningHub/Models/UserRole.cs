using Microsoft.AspNetCore.Identity;

public class UserRole : IdentityRole
{
    public int UserRoleId { get; set; }
    public string RoleName { get; set; }
}