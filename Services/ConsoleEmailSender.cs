using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace task.Services

{
    public class ConsoleEmailSender : IEmailSender
    {
        public Task<string> SendVerificationAsync(string toEmail, string verifyLink)
        {
            Console.WriteLine($"Verify {toEmail}: {verifyLink}");
            return Task.FromResult(verifyLink);
        }
    }
}