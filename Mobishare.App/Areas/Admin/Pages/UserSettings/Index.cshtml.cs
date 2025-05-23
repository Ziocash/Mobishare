using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Security;

namespace Mobishare.App.Areas.Admin.Pages.UserSettings
{
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
