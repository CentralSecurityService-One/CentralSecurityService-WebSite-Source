using CentralSecurityService.Common.Configuration;
using CentralSecurityService.Configuration;
using Eadent.Common.WebApi.ApiClient;
using Eadent.Common.WebApi.DataTransferObjects.Google;
using Eadent.Common.WebApi.Helpers;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MimeKit;
using System.ComponentModel.DataAnnotations;

namespace CentralSecurityService.Pages
{
    public class ContactModel : PageModel
    {
        private ILogger<ContactModel> Logger { get; }

        public string GoogleReCaptchaSiteKey => CentralSecurityServiceCommonSettings.Instance.GoogleReCaptcha.SiteKey;

        public decimal GoogleReCaptchaScore { get; set; }

        [BindProperty]
        public string GoogleReCaptchaValue { get; set; }

        [BindProperty]
        [EmailAddress(ErrorMessage = "Invalid E-Mail Address.")]
        public string? EMailAddress { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Message is required.")]
        public string Message { get; set; }

        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public ContactModel(ILogger<ContactModel> logger)
        {
            Logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        // Credit to GitHub Copilot (Modified).
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            (bool success, decimal googleReCaptchaScore) = await GoogleReCaptchaAsync();

            GoogleReCaptchaScore = googleReCaptchaScore;

            if (googleReCaptchaScore < CentralSecurityServiceCommonSettings.Instance.GoogleReCaptcha.MinimumScore)
            {
                Logger.LogWarning("You are unable to send a message because of a poor Google ReCaptcha Score {GoogleReCaptchaScore}.", googleReCaptchaScore);

                ErrorMessage = "You are unable to send a message because of a poor Google ReCaptcha Score. Please try again later.";
            }
            else
            {
                var eMailSettings = CentralSecurityServiceSensitiveSettings.Instance.EMail;

                var subject = "CentralSecurityService.one Contact Page Submission.";

                DateTime utcNow = DateTime.UtcNow;

                string htmlBody = $"<html>www.CentralSecurityService.one Contact.<br><br>" +
                    $"Machine Name: <strong>{Environment.MachineName}</strong><br><br>" +
                    $"Url: <strong>{Request.Scheme}://{Request.Host}{Request.Path}</strong><br><br>" +
                    $"Date & Time (UTC): <strong>{utcNow:dddd, d-MMM-yyyy h:mm:ss tt}</strong><br><br>" +
                    $"Date & Time (Local): <strong>{utcNow.ToLocalTime():dddd, d-MMM-yyyy h:mm:ss tt}</strong><br><br>";
                
                if (!string.IsNullOrWhiteSpace(EMailAddress))
                    htmlBody += $"Sender E-Mail: {EMailAddress}<br><br>";

                htmlBody += $"Message:<br><br>";
                htmlBody += Message.Replace("\n", "<br>") + "<br><br>";
                htmlBody += "</html>";
                
                var eMailMessage = new MimeMessage();
                eMailMessage.From.Add(new MailboxAddress(eMailSettings.FromEMailName, eMailSettings.FromEMailAddress));
                eMailMessage.To.Add(new MailboxAddress(eMailSettings.ToEMailName, eMailSettings.ToEMailAddress));
                eMailMessage.Subject = subject;
                eMailMessage.Body = new TextPart("html")
                {
                    Text = htmlBody
                };

                try
                {
                    using var smtp = new SmtpClient();
                    await smtp.ConnectAsync(eMailSettings.HostName, eMailSettings.HostPort, true);
                    await smtp.AuthenticateAsync(eMailSettings.SenderEMailAddress, eMailSettings.SenderPassword);
                    await smtp.SendAsync(eMailMessage);
                    await smtp.DisconnectAsync(true);

                    SuccessMessage = "Your message has been sent.";
                    ModelState.Clear();
                }
                catch
                {
                    ErrorMessage = "There was an error sending your message. Please try again later.";

                    Logger.LogWarning("There was an error sending the contact form message from {EMailAddress}.", EMailAddress ?? "Unknown E-Mail Address");
                }
            }

            return Page();
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
    }
}
