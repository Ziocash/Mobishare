using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Mobishare.Core.Security;

namespace Mobishare.App.Areas.Admin.Pages.UserSettings
{
    // [Authorize(Policy = PolicyNames.IsStaff)]
    public class SuspendedUserModel : PageModel
    {
        private readonly ILogger<SuspendedUserModel> _logger;
        private UserManager<IdentityUser> _userManager;
        public IEnumerable<(IdentityUser User, Claim Claim)> FilteredUser { get; set; }
        public IEnumerable<(IdentityUser User, Claim Claim)> SuspendeUser { get; set; }

        public SuspendedUserModel(ILogger<SuspendedUserModel> logger, UserManager<IdentityUser> userManager)
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

                if (user.LockoutEnd.HasValue)
                {
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
            }

            SuspendeUser = aus;
            FilteredUser = filteredUsers;

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveSuspension(string id)
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

            user.LockoutEnd = null;
            var result = await _userManager.UpdateAsync(user);
            
            _logger.LogInformation("User with id " + id + " is no longer suspened");

            if (!result.Succeeded)
            {
                _logger.LogError("Error while unsuspending user.");

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
                if (user.LockoutEnd.HasValue)
                {
                    aus.Add(new
                    (
                        user,
                        claims.First(c => c.Type == ClaimNames.Role)
                    ));
                    
                    _logger.LogInformation("Associate role claim to user " + user.Id);
                }
            }

            SuspendeUser = aus;
        }
    }
}
