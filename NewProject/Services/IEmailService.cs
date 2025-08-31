using System.Threading.Tasks;

namespace NewProject.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
