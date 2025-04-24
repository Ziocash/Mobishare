using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Mobishare.Core.Security;

namespace Mobishare.App.Areas.Admin.Pages.UserSettings
{
    // [Authorize(Policy = PolicyNames.IsStaff)]
    public class UserPermissionsModel : PageModel
    {
        private readonly ILogger<UserPermissionsModel> _logger;
        private UserManager<IdentityUser> _userManager;
        public IEnumerable<(IdentityUser User, Claim Claim)> FilteredUser { get; set; }
        public IEnumerable<(IdentityUser User, Claim Claim)> UsersWithRoles { get; set; }

        public UserPermissionsModel(ILogger<UserPermissionsModel> logger, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [BindProperty]
        public InputEventModel Input { get; set; }

        // [AtLeastOneValueRequired]
        public class InputEventModel
        {
            public string? Email { get; set; }
            public string? Role { get; set; }
        }

        public async Task<IActionResult> OnPostFilterUsers()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model states. Model states status: " + !ModelState.IsValid);

                return Page();
            }

            var users = _userManager.Users.ToList();
            var filteredUsers = new List<(IdentityUser User, Claim Claim)>();
            var aus = new List<(IdentityUser User, Claim Claim)>();

            foreach (var user in users)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var roleClaim = claims.First(c => c.Type == ClaimNames.Role);

                aus.Add(new
                (
                    user,
                    roleClaim
                ));

                _logger.LogInformation("Associate role claim " + roleClaim + " to user " + user.Id);

                if ((string.IsNullOrEmpty(Input.Email) || user.Email.Contains(Input.Email, StringComparison.OrdinalIgnoreCase)) &&
                    (Input.Role.IsNullOrEmpty() || (roleClaim != null && roleClaim.Value == Input.Role)))
                {
                    _logger.LogInformation("Associate role claim " + roleClaim + " to fitered user " + user.Id);

                    filteredUsers.Add((user, roleClaim));
                }
            }

            UsersWithRoles = aus;
            FilteredUser = filteredUsers;

            return Page();
        }

        public async Task<IActionResult> OnPostEditRole(string id, string newRole)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(newRole))
            {
                _logger.LogWarning("Invalid id or role claim. Both are required.");

                return Page();
            }

            var user = await _userManager.FindByIdAsync(id);

            _logger.LogInformation("Sercherd user by id: " + id);

            if (user == null)
            {
                _logger.LogError("User with id " + id + " does not exist.");

                return Page();
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var roleClaim = claims.FirstOrDefault(c => c.Type == ClaimNames.Role);

            if (roleClaim != null)
            {
                var removeResult = await _userManager.RemoveClaimAsync(user, roleClaim);

                _logger.LogInformation("Removed claim from user.");

                if (!removeResult.Succeeded)
                {
                    _logger.LogError("Error removing claim from user.");

                    return Page();
                }
            }

            var addResult = await _userManager.AddClaimAsync(user, new Claim(ClaimNames.Role, newRole));

            _logger.LogInformation("Added new role claim " + newRole + " to user " + user.Id + ".");

            if (!addResult.Succeeded)
            {
                _logger.LogError("Error while adding new role claim " + newRole + " to user " + user.Id + ".");

                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSuspend(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Invalid id. Id is required.");

                return Page();
            }

            var user = await _userManager.FindByIdAsync(id);

            _logger.LogInformation("Sercherd user by id: " + id);

            if (user == null)
            {
                _logger.LogError("User with id " + id + " does not exist.");

                return Page();
            }

            user.LockoutEnd = DateTime.MaxValue;
            var result = await _userManager.UpdateAsync(user);
            _logger.LogInformation("User with id " + id + " is suspened");

            if (!result.Succeeded)
            {
                _logger.LogError("Error while suspending user.");

                return Page();
            }
            

            return RedirectToPage();
        }

        public async Task OnGetAsync()
        {
            var users = _userManager.Users.ToList();
            var aus = new List<(IdentityUser User, Claim Claim)>();

            foreach (var user in users)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                aus.Add(new
                (
                    user,
                    claims.First(c => c.Type == ClaimNames.Role)
                ));

                _logger.LogInformation("Associate role claim to user " + user.Id);
            }

            UsersWithRoles = aus;
        }
    }
}
