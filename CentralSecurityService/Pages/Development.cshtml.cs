using CentralSecurityService.Common.Configuration;
using CentralSecurityService.Common.DataAccess.CentralSecurityService.Databases;
using CentralSecurityService.Common.DataAccess.CentralSecurityService.Entities;
using CentralSecurityService.Common.DataAccess.CentralSecurityService.Repositories;
using Eadent.Common.WebApi.ApiClient;
using Eadent.Common.WebApi.DataTransferObjects.Google;
using Eadent.Common.WebApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkiaSharp;

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
            long referenceId = 9235612786; // Example reference ID, replace with actual logic if needed.

            var formatted = $"{referenceId:R000_000_000}";

            return Page();
        }

        public async Task OnPostAsync(string action)
        {
            (bool success, decimal googleReCaptchaScore) = await GoogleReCaptchaAsync();

            GoogleReCaptchaScore = googleReCaptchaScore;

            if (googleReCaptchaScore < CentralSecurityServiceCommonSettings.Instance.GoogleReCaptcha.MinimumScore)
            {
                Logger.LogWarning("You are unable to Upload A File because of a poor Google ReCaptcha Score {GoogleReCaptchaScore}.", googleReCaptchaScore);
            }
            else
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

                    var outputFileName = $"{uniqueReferenceId:R000_000_000}_000-Thumbnail_Width_125-{Path.GetFileNameWithoutExtension(FileToUpload.FileName)}.jpg";

                    // TODO: Make path configurable or use a safer method to construct paths.
                    var inputFilePathAndName = Path.Combine("/CentralSecurityService/ReferenceFiles", inputFileName);

                    var outputFilePathAndName = Path.Combine("/CentralSecurityService/ReferenceFiles", outputFileName);

                    using (var fileStream = new FileStream(inputFilePathAndName, FileMode.Create))
                    {
                        await FileToUpload.CopyToAsync(fileStream);
                    }

                    SaveThumbnailAsJpeg(inputFilePathAndName, outputFilePathAndName, 125);

                    var referenceEntity = new ReferenceEntity()
                    {
                        UniqueReferenceId = uniqueReferenceId,
                        SubReferenceId = 0,
                        ReferenceTypeId = Common.Definitions.ReferenceType.Image,
                        ThumbnailFileName = outputFileName,
                        ReferenceName = inputFileName,
                        Description = Description,
                        Categorisations = Categorisations,
                        CreatedDateTimeUtc = DateTime.UtcNow,
                    };

                    ReferencesRepository.Create(referenceEntity);
                    ReferencesRepository.SaveChanges();
                }
                catch (Exception exception)
                {
                    Logger.LogError(exception, "An Exception occurred.");
                }
            }
        }

        protected async Task<(bool success, decimal googleReCaptchaScore)> GoogleReCaptchaAsync()
        {
            var verifyRequestDto = new ReCaptchaVerifyRequestDto()
            {
                secret = CentralSecurityServiceCommonSettings.Instance.GoogleReCaptcha.Secret,
                response = GoogleReCaptchaValue,
                remoteip = HttpHelper.GetLocalIpAddress(Request)
            };

            bool success = false;

            decimal googleReCaptchaScore = -1M;

            IApiClientResponse<ReCaptchaVerifyResponseDto> response = null;

            using (var apiClient = new ApiClientUrlEncoded(Logger, "https://www.google.com/"))
            {
                response = await apiClient.PostAsync<ReCaptchaVerifyRequestDto, ReCaptchaVerifyResponseDto>("/recaptcha/api/siteverify", verifyRequestDto, null);
            }

            if (response.ResponseDto != null)
            {
                googleReCaptchaScore = response.ResponseDto.score;

                success = true;
            }

            return (success, googleReCaptchaScore);
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
