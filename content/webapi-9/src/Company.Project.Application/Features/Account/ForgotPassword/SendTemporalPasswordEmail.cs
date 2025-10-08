using Company.Project.Application.Common.Options;
using Company.Project.Application.Features.Account.ConfirmEmail;
using Company.Project.Application.Responses.Authentication;

using Microsoft.Extensions.Options;

namespace Company.Project.Application.Features.Account.ForgotPassword;

public sealed class SendTemporalPasswordEmail(
    IEmailSender emailSender,
    ILogger<SendConfirmationEmail> logger,
    IOptionsMonitor<ApplicationSettings> optionsMonitor
)
{
    public async Task ScheduleEmail(ForgottenPasswordResponse model)
    {
        await SendEmail(model);
        logger.LogToInfo($"Job ID: {model.Email}. Account confirmation has been enqueued.");
    }

    public async Task SendEmail(ForgottenPasswordResponse model)
    {
        var message = GenerateForgottenHtml(model.PasswordResetToken, model.FullName);

        var response = await emailSender.SendEmailAsync(
            model.Email,
            "FundRaiser - Password Reset",
            message,
            model.FullName,
            useCc: false
        );

        if (response)
        {
            logger.LogInformation("Email sent successfully");
        }
        else
        {
            logger.LogError("Email failed to send to {Email}", model.Email);
        }
    }

    private string GenerateForgottenHtml(string temporaryPassword, string name)
    {
        var emailDetails = optionsMonitor.CurrentValue;

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Reset</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            line-height: 1.6;
            color: #333333;
            background-color: #f8fafc;
        }}
        
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }}
        
        .header {{
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
            padding: 40px 30px;
            text-align: center;
        }}
        
        .header h1 {{
            font-size: 24px;
            font-weight: 600;
            margin-bottom: 8px;
        }}
        
        .header p {{
            font-size: 16px;
            opacity: 0.9;
        }}
        
        .content {{
            padding: 40px 30px;
        }}
        
        .greeting {{
            font-size: 18px;
            color: #1f2937;
            margin-bottom: 24px;
        }}
        
        .message {{
            font-size: 16px;
            color: #4b5563;
            margin-bottom: 32px;
            line-height: 1.6;
        }}
        
        .password-box {{
            background-color: #f3f4f6;
            border: 2px dashed #d1d5db;
            border-radius: 8px;
            padding: 24px;
            text-align: center;
            margin: 32px 0;
        }}
        
        .password-label {{
            font-size: 14px;
            color: #6b7280;
            margin-bottom: 8px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            font-weight: 500;
        }}
        
        .password {{
            font-size: 24px;
            font-weight: 700;
            color: #1f2937;
            font-family: 'Monaco', 'Menlo', 'Ubuntu Mono', monospace;
            letter-spacing: 2px;
            background-color: #ffffff;
            padding: 16px 24px;
            border-radius: 6px;
            border: 1px solid #e5e7eb;
            display: inline-block;
            margin-top: 8px;
        }}
        
        .instructions {{
            background-color: #fef3c7;
            border-left: 4px solid #f59e0b;
            padding: 16px 20px;
            margin: 24px 0;
            border-radius: 0 6px 6px 0;
        }}
        
        .instructions h3 {{
            color: #92400e;
            font-size: 16px;
            margin-bottom: 8px;
            font-weight: 600;
        }}
        
        .instructions p {{
            color: #a16207;
            font-size: 14px;
            margin-bottom: 4px;
        }}
        
        .security-notice {{
            background-color: #fef2f2;
            border-left: 4px solid #ef4444;
            padding: 16px 20px;
            margin: 24px 0;
            border-radius: 0 6px 6px 0;
        }}
        
        .security-notice h3 {{
            color: #dc2626;
            font-size: 16px;
            margin-bottom: 8px;
            font-weight: 600;
        }}
        
        .security-notice p {{
            color: #b91c1c;
            font-size: 14px;
            margin-bottom: 4px;
        }}
        
        .footer {{
            background-color: #f9fafb;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #e5e7eb;
        }}
        
        .footer-brand {{
            font-size: 18px;
            font-weight: 600;
            color: #10b981;
            margin-bottom: 8px;
        }}
        
        .footer-info {{
            font-size: 14px;
            color: #6b7280;
            margin-bottom: 4px;
        }}
        
        .footer-link {{
            color: #10b981;
            text-decoration: none;
        }}
        
        .footer-link:hover {{
            text-decoration: underline;
        }}
        
        @media (max-width: 600px) {{
            .email-container {{
                margin: 0;
                border-radius: 0;
            }}
            
            .header, .content, .footer {{
                padding: 24px 20px;
            }}
            
            .password {{
                font-size: 20px;
                padding: 12px 16px;
            }}
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1>{emailDetails.EmailSettings.BusinessNameTag}</h1>
            <p>Secure Password Recovery</p>
        </div>
        
        <div class=""content"">
            <div class=""greeting"">
                Hello {name}! 👋
            </div>
            
            <div class=""message"">
                We received a request to reset your password for your {emailDetails.EmailSettings.BusinessNameTag} account. 
                We've generated a temporary password for you to use to log in securely.
            </div>
            
            <div class=""password-box"">
                <div class=""password-label"">Your Temporary Password</div>
                <div class=""password"">{temporaryPassword}</div>
            </div>
            
            <div class=""instructions"">
                <h3>📋 Next Steps</h3>
                <p>1. Use this temporary password to log in to your account</p>
                <p>2. Immediately change your password to something secure</p>
                <p>3. Make sure to save your new password in a safe place</p>
            </div>
            
            <div class=""security-notice"">
                <h3>🔒 Security Notice</h3>
                <p>• If you didn't request this reset, please contact support immediately</p>
                <p>• Never share your password with anyone</p>
            </div>
        </div>
        
        <div class=""footer"">
            <div class=""footer-brand"">{emailDetails.EmailSettings.TeamNameTag}</div>
            <div class=""footer-info"">Email: <a href=""mailto:{emailDetails.EmailSettings.FromAddress}"" class=""footer-link"">{emailDetails.EmailSettings.FromAddress}</a></div>
            <div class=""footer-info"">Website: <a href=""{emailDetails.EmailSettings.Website}"" class=""footer-link"">{emailDetails.EmailSettings.Website}</a></div>
            <div class=""footer-info"" style=""margin-top: 16px; font-size: 12px; color: #9ca3af;"">
                This email was sent because you requested a password reset. If you didn't make this request, please ignore this email.
            </div>
        </div>
    </div>
</body>
</html>";
    }
}
