using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CentralSecurityService.Pages
{
    public class ReferencesModel : PageModel
    {
        private readonly ILogger<ReferencesModel> _logger;

        public ReferencesModel(ILogger<ReferencesModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            _logger.LogInformation("References page accessed.");
        }
    }
}
