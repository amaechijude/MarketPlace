namespace MarketPlace.Api.Infrastucture.OtpValidation;

public static class EmailTemplates
{
    public static string BuildOtpTemplate(string name, string otp)
    {
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Verify Your Account</title>
                <style>
                    body {
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                        background-color: #f4f6f8;
                        color: #333333;
                        margin: 0;
                        padding: 0;
                        -webkit-font-smoothing: antialiased;
                    }
                    .wrapper {
                        background-color: #f4f6f8;
                        width: 100%;
                        table-layout: fixed;
                        padding-bottom: 40px;
                    }
                    .container {
                        max-width: 600px;
                        margin: 40px auto;
                        background-color: #ffffff;
                        border-radius: 12px;
                        box-shadow: 0 4px 16px rgba(0, 0, 0, 0.05);
                        overflow: hidden;
                        border: 1px solid #e1e4e8;
                    }
                    .header {
                        background-color: #0f172a;
                        padding: 32px;
                        text-align: center;
                    }
                    .header h1 {
                        color: #ffffff;
                        margin: 0;
                        font-size: 24px;
                        font-weight: 700;
                        letter-spacing: 2px;
                    }
                    .content {
                        padding: 40px 32px;
                        line-height: 1.6;
                    }
                    .greeting {
                        font-size: 18px;
                        font-weight: 600;
                        margin-bottom: 16px;
                        color: #0f172a;
                    }
                    .instructions {
                        margin-bottom: 24px;
                        color: #475569;
                        font-size: 15px;
                    }
                    .otp-box {
                        text-align: center;
                        margin: 32px 0;
                        padding: 24px;
                        background-color: #f8fafc;
                        border: 1px dashed #cbd5e1;
                        border-radius: 8px;
                    }
                    .otp-code {
                        font-family: 'Courier New', Courier, monospace;
                        font-size: 36px;
                        font-weight: 700;
                        letter-spacing: 8px;
                        color: #2563eb;
                        margin: 0;
                    }
                    .expiry {
                        font-size: 13px;
                        color: #64748b;
                        margin-top: 12px;
                    }
                    .footer {
                        background-color: #f8fafc;
                        padding: 24px 32px;
                        text-align: center;
                        font-size: 12px;
                        color: #64748b;
                        border-top: 1px solid #e2e8f0;
                    }
                    .footer p {
                        margin: 4px 0;
                    }
                </style>
            </head>
            <body>
                <div class="wrapper">
                    <div class="container">
                        <div class="header">
                            <h1>GITGBRAND</h1>
                        </div>
                        <div class="content">
                            <div class="greeting">Hello {{name}},</div>
                            <div class="instructions">
                                Thank you for verifying your identity with GitgBrand. Use the verification code (OTP) below to proceed:
                            </div>
                            <div class="otp-box">
                                <div class="otp-code">{{otp}}</div>
                                <div class="expiry">This code is valid for 30 minutes.</div>
                            </div>
                            <div class="instructions">
                                If you did not make this request, you can safely ignore this email.
                            </div>
                        </div>
                        <div class="footer">
                            <p>&copy; {{DateTime.UtcNow.Year}} GitgBrand. All rights reserved.</p>
                            <p>This is an automated message, please do not reply directly to this email.</p>
                        </div>
                    </div>
                </div>
            </body>
            </html>
            """;
    }
}
