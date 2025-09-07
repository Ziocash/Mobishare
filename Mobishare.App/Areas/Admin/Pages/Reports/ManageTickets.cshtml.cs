using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Security;

namespace MyApp.Namespace
{
    [Authorize(Policy = PolicyNames.IsTechnician)]
    public class ManageTicketsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
