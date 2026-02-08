using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace task.Services

{
    public class ConsoleEmailSender : IEmailSender
    {
        public Task SendAsync(string toEmail, string subject, string body)
        {
            Console.WriteLine("=== Simulated Email Sending ===");
            Console.WriteLine($"To: {toEmail}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine("Body:");
            Console.WriteLine(body);
            Console.WriteLine("=== End of Simulated Email ===");
            return Task.CompletedTask;
        }
    }
}