using CentralSecurityService.Common.Configuration;
using CentralSecurityService.Common.DataAccess.CentralSecurityService.Databases;
using CentralSecurityService.Common.DataAccess.CentralSecurityService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CentralSecurityService.Pages
{
    public class DevelopmentModel : PageModel
    {
        private ILogger<DevelopmentModel> Logger { get; }

        private ICentralSecurityServiceDatabase CentralSecurityServiceDatabase { get; set; }

        private IReferencesRepository ReferencesRepository { get; set; }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        public string Categorisations{ get; set; }

        [BindProperty]
        public IFormFile FileToUpload { get; set; }

        public string GoogleReCaptchaSiteKey => CentralSecurityServiceCommonSettings.Instance.GoogleReCaptcha.SiteKey;

        public decimal GoogleReCaptchaScore { get; set; }

        [BindProperty]
        public string GoogleReCaptchaValue { get; set; }

        public DevelopmentModel(ILogger<DevelopmentModel> logger, ICentralSecurityServiceDatabase centralSecurityServiceDatabase, IReferencesRepository referencesRepository)
        {
            Logger = logger;
            CentralSecurityServiceDatabase = centralSecurityServiceDatabase;
            ReferencesRepository = referencesRepository;
        }

        public async Task<IActionResult> OnGetAsync(string fileName)
        {
#if !DEBUG
            return LocalRedirect("/");
#endif

            long referenceId = 9235612786; // Example referenceId, replace with actual logic if needed.

            var formatted = $"{referenceId:R000_000_000}";

            return Page();
        }
    }
}
