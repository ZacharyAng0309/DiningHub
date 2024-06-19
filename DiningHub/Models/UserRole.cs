using Microsoft.AspNetCore.Identity;

public class UserRole : IdentityRole
{
    public string Description { get; set; }
}