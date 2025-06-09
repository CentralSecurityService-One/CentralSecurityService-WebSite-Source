using CentralSecurityService.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;

namespace CentralSecurityService.Pages
{
    public class GetReferenceFileModel : PageModel
    {
        private ILogger<GetReferenceFileModel> Logger { get; }

        private IWebHostEnvironment WebHostEnvironment { get; }

        public GetReferenceFileModel(ILogger<GetReferenceFileModel> logger, IWebHostEnvironment webHostEnvironment)
        {
            Logger = logger;
            WebHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> OnGetAsync(string referenceFile)
        {
            if (string.IsNullOrEmpty(referenceFile))
                return BadRequest("Reference File name cannot be null or empty.");

            string referenceFilesFolder = null;

            if (WebHostEnvironment.IsDevelopment())
            {
                referenceFilesFolder = CentralSecurityServiceSettings.Instance.References.DevelopmentReferenceFilesFolder;
            }
            else
            {
                referenceFilesFolder = CentralSecurityServiceSettings.Instance.References.ProductionReferenceFilesFolder;
            }

            var filePathAndName = Path.Combine(referenceFilesFolder, referenceFile);

            if (!System.IO.File.Exists(filePathAndName))
                return NotFound();

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePathAndName, out string contentType))
            {
                contentType = "application/octet-stream"; // Default fallback.
            }

            var file = System.IO.File.OpenRead(filePathAndName);

            return File(file, contentType); // ASP.NET Core [allegedly] disposes stream after response.
        }
    }
}
