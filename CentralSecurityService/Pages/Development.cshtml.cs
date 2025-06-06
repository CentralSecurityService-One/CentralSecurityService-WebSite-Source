using CentralSecurityService.Configuration;
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

                long uniqueReferenceId = GetNextUniqueReferenceId();

                var inputFileName = $"{uniqueReferenceId:R000_000_000}_000-{FileToUpload.FileName}";

                var outputFileName = $"{uniqueReferenceId:R000_000_000}_001-Width_125-{Path.GetFileNameWithoutExtension(FileToUpload.FileName)}.jpg";

                // TODO: Make path configurable or use a safer method to construct paths.
                var inputFilePathAndName = Path.Combine("/CentralSecurityService/ReferenceFiles/Uploads", inputFileName);

                var outputFilePathAndName = Path.Combine("/CentralSecurityService/ReferenceFiles/Uploads", outputFileName);

                using (var fileStream = new FileStream(inputFilePathAndName, FileMode.Create))
                {
                    await FileToUpload.CopyToAsync(fileStream);
                }

                //ResizeImage(inputFilePathAndName, outputFilePathAndName, 125);
                ResizeImageSkiaSharp(inputFilePathAndName, outputFilePathAndName, 125);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "An Exception occurred.");
            }
        }

        // Courtesy of www.ChatGpt.com (Modified).
        // TODO: If ever running on Linux use SkiaSharp or ImageSharp instead of System.Drawing.Common.
        public void ResizeImage(string inputFilePathAndName, string outputFilePathAndName, int resizedWidth)
        {
            using var originalImage = System.Drawing.Image.FromFile(inputFilePathAndName);

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

        public static void ResizeImageSkiaSharp(string inputPath, string outputPath, int targetWidth)
        {
            // Load the image
            using var inputStream = System.IO.File.OpenRead(inputPath);

            using var original = SKBitmap.Decode(inputStream);

            int targetHeight = original.Height * targetWidth / original.Width;

            // Resize
            using var resized = original.Resize(new SKImageInfo(targetWidth, targetHeight), new SKSamplingOptions(new SKCubicResampler()));

            if (resized == null)
                throw new Exception("Failed to resize image.");

            using var image = SKImage.FromBitmap(resized);
            using var outputStream = System.IO.File.OpenWrite(outputPath);

            // Encode to JPEG (or use .Encode(SKEncodedImageFormat.Png, 100) for PNG).
            image.Encode(SKEncodedImageFormat.Jpeg, 90).SaveTo(outputStream);
        }

        public long GetNextUniqueReferenceId()
        {
            long newId;

            using (SqlConnection conn = new SqlConnection(CentralSecurityServiceSettings.Instance.Database.ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand($"SELECT NEXT VALUE FOR {CentralSecurityServiceSettings.Instance.Database.DatabaseSchema}.UniqueReferenceId;", conn))
                {
                    object result = cmd.ExecuteScalar();
                    newId = Convert.ToInt64(result);
                }
            }

            return newId;
        }
    }
}
