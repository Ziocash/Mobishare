using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Security;

namespace Mobishare.App.Areas.Admin.Pages.Reports
{
    [Authorize(Policy = PolicyNames.IsTechnician)]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
