using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using CoWorkManager.Models;

public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    private readonly RoleManager<IdentityRole> _roleManager;
    public CustomClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
        _roleManager = roleManager;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Add user-specific claims from the store 
        var userClaims = await UserManager.GetClaimsAsync(user);
        foreach (var c in userClaims)
        {
            if (!identity.HasClaim(c.Type, c.Value))
                identity.AddClaim(c);
        }

        var roles = await UserManager.GetRolesAsync(user);
        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var rc in roleClaims)
            {
                if (!identity.HasClaim(rc.Type, rc.Value))
                    identity.AddClaim(rc);
            }
        }

        if (!identity.HasClaim("FullName", user.FullName ?? string.Empty))
        {
            identity.AddClaim(new Claim("FullName", user.FullName ?? ""));
        }

        return identity;
    }
}
