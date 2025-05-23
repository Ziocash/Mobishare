using Microsoft.AspNetCore.Authorization;

namespace Mobishare.Core.Security.Policies;

public class IsAdmin : IAuthorizationRequirement { }

public class IsAdminAuthorizationHandler : AuthorizationHandler<IsAdmin>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAdmin requirement)
    {
        if (context.User.HasClaim(ClaimNames.Role, UserRole.Admin.ToString()))
            context.Succeed(requirement);

        return Task.FromResult(context);
    }
}