using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Security;

namespace Mobishare.App.Areas.Admin.Pages.UserSettings
{
    // [Authorize(Policy = PolicyNames.IsStaff)]
    public class UserActivityModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
