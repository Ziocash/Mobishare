using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mobishare.App.Areas.Admin.Pages
{
    // [Authorize(Policy = PolicyNames.IsStaff)]
    public class IndexModel : PageModel
    {
        public IndexModel()
        {
            
        }
        public void OnGet()
        {
        }
    }
}
