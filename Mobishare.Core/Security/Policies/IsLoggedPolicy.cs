using System;
using Microsoft.AspNetCore.Authorization;

namespace Mobishare.Core.Security.Policies;

public class IsLoggedPolicy : IAuthorizationRequirement { }

public class IsLoggedPolicyHandler : AuthorizationHandler<IsLoggedPolicy>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsLoggedPolicy requirement)
    {
        if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            context.Succeed(requirement);

        return Task.FromResult(context);
    }
}