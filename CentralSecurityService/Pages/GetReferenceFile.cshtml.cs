using CentralSecurityService.Common.DataAccess.CentralSecurityService.Entities;
using CentralSecurityService.Common.DataAccess.CentralSecurityService.Repositories;
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

        private IReferencesRepository ReferencesRepository { get; }

        public GetReferenceFileModel(ILogger<GetReferenceFileModel> logger, IWebHostEnvironment webHostEnvironment, IReferencesRepository referencesRepository)
        {
            Logger = logger;
            WebHostEnvironment = webHostEnvironment;
            ReferencesRepository = referencesRepository;
        }

        public async Task<IActionResult> OnGetAsync(string type, string referenceFile)
        {
            if (string.IsNullOrWhiteSpace(type))
                return BadRequest("Type name cannot be null or whitespace.");

            if (string.IsNullOrWhiteSpace(referenceFile))
                return BadRequest("Reference File name cannot be null or whitespace.");

            ReferenceEntity referenceEntity = null;

            if (type == "Thumbnail")
            {
                referenceEntity = ReferencesRepository.GetLastOrDefault(where => where.ThumbnailFileName == referenceFile, orderBy => orderBy.ReferenceId);
            }
            else if (type == "Full")
            {
                referenceEntity = ReferencesRepository.GetLastOrDefault(where => where.ReferenceName == referenceFile, orderBy => orderBy.ReferenceId);
            }

            if ((referenceEntity == null) || ((referenceEntity != null) && referenceEntity.Redacted))
            {
                return NotFound("The Reference File is Invalid or Redacted.");
            }

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
