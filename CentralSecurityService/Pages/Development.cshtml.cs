using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CentralSecurityService.Pages
{
    public class DevelopmentModel : PageModel
    {
        private ILogger<DevelopmentModel> Logger { get; }

        private IWebHostEnvironment WebHostEnvironment { get; }

        [BindProperty]
        public long ReferenceId { get; set; }

        [BindProperty]
        public IFormFile FileToUpload { get; set; }

        public DevelopmentModel(ILogger<DevelopmentModel> logger, IWebHostEnvironment webHostEnvironment)
        {
            Logger = logger;
            WebHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> OnGetAsync(string fileName)
        {
            long referenceId = 9235612786; // Example reference ID, replace with actual logic if needed.

            var formatted = $"{referenceId:R000_000_000}";

            return Page();
        }

        public async Task OnPostAsync(string action)
        {
            try
            {
                if (FileToUpload == null || FileToUpload.Length == 0)
                {
                    Logger.LogWarning("No file was uploaded or the file is empty.");
                    return;
                }

                var inputFileName = $"{ReferenceId:R000_000_000}_000-{FileToUpload.FileName}";

                var outputFileName = $"{ReferenceId:R000_000_000}_001-Width_125-{FileToUpload.FileName}";

                // TODO: Make path configurable or use a safer method to construct paths.
                var inputFilePathAndName = Path.Combine("/CentralSecurityService/ReferenceFiles/Uploads", inputFileName);

                var outputFilePathAndName = Path.Combine("/CentralSecurityService/ReferenceFiles/Uploads", outputFileName);

                using (var fileStream = new FileStream(inputFilePathAndName, FileMode.Create))
                {
                    await FileToUpload.CopyToAsync(fileStream);
                }

                ResizeImage(inputFilePathAndName, outputFilePathAndName, 125);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "An Exception occurred.");
            }
        }

        // Courtesy of www.ChatGpt.com.
        public void ResizeImage(string inputFilePathAndName, string outputFilePathAndName, int resizedWidth)
        {
            using var originalImage = Image.FromFile(inputFilePathAndName);

            int resizedHeight = originalImage.Height * resizedWidth / originalImage.Width;

            using var resizedImage = new Bitmap(resizedWidth, resizedHeight);

            using (var graphics = Graphics.FromImage(resizedImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                graphics.DrawImage(originalImage, 0, 0, resizedWidth, resizedHeight);
            }

            resizedImage.Save(outputFilePathAndName);
        }
    }
}
