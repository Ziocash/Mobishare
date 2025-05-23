using Microsoft.AspNetCore.Authorization;

namespace Mobishare.Core.Security.Policies;

public class IsStaff : IAuthorizationRequirement { }

public class IsStaffAuthorizationHandler : AuthorizationHandler<IsStaff>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsStaff requirement)
    {
        if (context.User.HasClaim(ClaimNames.Role, UserRole.Admin.ToString())
            || context.User.HasClaim(ClaimNames.Role, UserRole.Staff.ToString()))
            context.Succeed(requirement);

        return Task.FromResult(context);
    }
}