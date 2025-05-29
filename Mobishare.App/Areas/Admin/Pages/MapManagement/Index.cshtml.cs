using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Security;

namespace Mobishare.App.Areas.Admin.Pages.MapManagement
{
    [Authorize(Policy = PolicyNames.IsStaff)]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
