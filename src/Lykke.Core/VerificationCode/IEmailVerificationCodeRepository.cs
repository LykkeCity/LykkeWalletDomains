using System;
using System.Threading.Tasks;

namespace Core.VerificationCode
{
    public interface IEmailVerificationCode
    {
        string Id { get; }
        string Email { get; }
        string Code { get; }
        DateTime CreationDateTime { get; }
    }

    public class EmailEmailVerificationCode : IEmailVerificationCode
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime CreationDateTime { get; set; }
    }

    public interface IEmailVerificationCodeRepository
    {
        Task<IEmailVerificationCode> CreateAsync(string email, bool generateRealCode);

        /// <summary>
        /// Returns the latest generated code
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns></returns>
        Task<IEmailVerificationCode> GetActualCode(string email);

        Task<bool> CheckAsync(string email, string codeToCheck);

        Task DeleteCodesByEmailAsync(string email);
    }
}
