using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;

namespace CentralSecurityService.Pages
{
    public class DevelopmentModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("File name cannot be null or empty.");

            var filePathAndName = Path.Combine("/DevelopmentImages", fileName);

            if (!System.IO.File.Exists(filePathAndName))
                return NotFound();

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePathAndName, out string contentType))
            {
                contentType = "application/octet-stream"; // default fallback
            }

            var image = System.IO.File.OpenRead(filePathAndName);

            return File(image, contentType); // ASP.NET Core [allegedly] disposes stream after response.
        }
    }
}
public class ImageController : Controller
{
    public IActionResult GetImage(string fileName)
    {
        var path = Path.Combine("C:/ServerImages/", fileName);
        if (!System.IO.File.Exists(path))
            return NotFound();

        var image = System.IO.File.OpenRead(path);
        return File(image, "image/jpeg"); // or use "image/png" depending on file type
    }
}