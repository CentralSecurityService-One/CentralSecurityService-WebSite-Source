using CentralSecurityService.Common.DataAccess.CentralSecurityService.Databases;
using CentralSecurityService.Common.DataAccess.CentralSecurityService.Entities;
using CentralSecurityService.Common.DataAccess.CentralSecurityService.Repositories;
using CentralSecurityService.Common.Definitions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkiaSharp;

namespace CentralSecurityService.Pages
{
    public class TestsModel : PageModel
    {
        private ILogger<TestsModel> Logger { get; }

        private ICentralSecurityServiceDatabase CentralSecurityServiceDatabase { get; set; }

        private IReferencesRepository ReferencesRepository { get; set; }

        public string Message { get; set; }

        public TestsModel(ILogger<TestsModel> logger, ICentralSecurityServiceDatabase centralSecurityServiceDatabase, IReferencesRepository referencesRepository)
        {
            Logger = logger;
            CentralSecurityServiceDatabase = centralSecurityServiceDatabase;
            ReferencesRepository = referencesRepository;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string action)
        {
            if (action == "Add Test Data")
            {
            }

            return Page();
        }
    }
}
