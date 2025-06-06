using CentralSecurityService.Configuration;
using CentralSecurityService.DataAccess.CentralSecurityService.Databases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CentralSecurityService.Pages
{
    public class DevelopmentModel : PageModel
    {
        private ILogger<DevelopmentModel> Logger { get; }

        private ICentralSecurityServiceDatabase CentralSecurityServiceDatabase { get; set; }

        [BindProperty]
        public IFormFile FileToUpload { get; set; }

        public DevelopmentModel(ILogger<DevelopmentModel> logger, ICentralSecurityServiceDatabase centralSecurityServiceDatabase)
        {
            Logger = logger;
            CentralSecurityServiceDatabase = centralSecurityServiceDatabase;
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

                long uniqueReferenceId = CentralSecurityServiceDatabase.GetNextUniqueReferenceId();

                var inputFileName = $"{uniqueReferenceId:R000_000_000}_000-{FileToUpload.FileName}";

                var outputFileName = $"{uniqueReferenceId:R000_000_000}_001-Width_125-{Path.GetFileNameWithoutExtension(FileToUpload.FileName)}.jpg";

                // TODO: Make path configurable or use a safer method to construct paths.
                var inputFilePathAndName = Path.Combine("/CentralSecurityService/ReferenceFiles/Uploads", inputFileName);

                var outputFilePathAndName = Path.Combine("/CentralSecurityService/ReferenceFiles/Uploads", outputFileName);

                using (var fileStream = new FileStream(inputFilePathAndName, FileMode.Create))
                {
                    await FileToUpload.CopyToAsync(fileStream);
                }

                SaveThumbnailAsJpeg(inputFilePathAndName, outputFilePathAndName, 125);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "An Exception occurred.");
            }
        }

        // Courtesy of www.ChatGpt.com (Modified).
        private static void SaveThumbnailAsJpeg(string inputFilePathAndName, string outputFilePathAndName, int targetWidth)
        {
            // Load the image.
            using var inputStream = System.IO.File.OpenRead(inputFilePathAndName);

            using var original = SKBitmap.Decode(inputStream);

            int targetHeight = original.Height * targetWidth / original.Width;

            // Resize/Get Thumbnail.
            using var thumbnail = original.Resize(new SKImageInfo(targetWidth, targetHeight), new SKSamplingOptions(new SKCubicResampler()));

            if (thumbnail == null)
                throw new Exception("Failed to resize image / Get Thumbnail.");

            using var image = SKImage.FromBitmap(thumbnail);

            using var outputStream = System.IO.File.OpenWrite(outputFilePathAndName);

            // Encode to JPEG (or use .Encode(SKEncodedImageFormat.Png, 100) for PNG).
            image.Encode(SKEncodedImageFormat.Jpeg, 90).SaveTo(outputStream);
        }
    }
}
