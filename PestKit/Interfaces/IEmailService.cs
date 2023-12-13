namespace PestKit.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string emailTo, string subject, string body, bool isHTML = false);
    }
}
