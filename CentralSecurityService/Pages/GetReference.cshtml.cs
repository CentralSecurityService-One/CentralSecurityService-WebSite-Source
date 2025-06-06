using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;

namespace CentralSecurityService.Pages
{
    public class GetExhibitModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync(string reference)
        {
            if (string.IsNullOrEmpty(reference))
                return BadRequest("Reference name cannot be null or empty.");

            // TODO: Make path configurable or use a safer method to construct paths.
            var filePathAndName = Path.Combine("/CentralSecurityService/References", reference);

            if (!System.IO.File.Exists(filePathAndName))
                return NotFound();

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePathAndName, out string contentType))
            {
                contentType = "application/octet-stream"; // Default fallback.
            }

            var image = System.IO.File.OpenRead(filePathAndName);

            return File(image, contentType); // ASP.NET Core [allegedly] disposes stream after response.
        }
    }
}
