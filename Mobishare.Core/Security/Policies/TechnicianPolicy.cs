using Microsoft.AspNetCore.Authorization;

namespace Mobishare.Core.Security.Policies;

public class IsTechnician : IAuthorizationRequirement { }

public class IsTechnicianAuthorizationHandler : AuthorizationHandler<IsTechnician>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsTechnician requirement)
    {
        if (context.User.HasClaim(ClaimNames.Role, UserRole.Admin.ToString())
            || context.User.HasClaim(ClaimNames.Role, UserRole.Technician.ToString())
             || context.User.HasClaim(ClaimNames.Role, UserRole.Staff.ToString()))
            context.Succeed(requirement);

        return Task.FromResult(context);
    }
}