using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mobishare.Core.Security;

namespace TicketShop.Core.Security.Policies;

public class IsAdmin : IAuthorizationRequirement { }

public class IsAdminAuthorizationHandler : AuthorizationHandler<IsAdmin>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAdmin requirement)
    {
        if (context.User.HasClaim(ClaimNames.Role, UserRole.Admin.ToString())
           || context.User.HasClaim(ClaimNames.Role, UserRole.User.ToString())
            || context.User.HasClaim(ClaimNames.Role, UserRole.Technician.ToString())
             || context.User.HasClaim(ClaimNames.Role, UserRole.Staff.ToString()))
            context.Succeed(requirement);

        return Task.FromResult(context);
    }
}