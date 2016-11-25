using System;
using System.Threading.Tasks;

namespace Core.VerificationCode
{
    public interface ISmsVerificationCode
    {
        string Id { get; }
        string Phone { get; }
        string Code { get; }
        DateTime CreationDateTime { get; }
    }

    public class SmsVerificationCode : ISmsVerificationCode
    {
        public string Id { get; set; }
        public string Phone { get; set; }
        public string Code { get; set; }
        public DateTime CreationDateTime { get; set; }
    }

    public interface ISmsVerificationCodeRepository
    {
        Task<ISmsVerificationCode> CreateAsync(string phoneNum, bool generateRealCode);

        /// <summary>
        /// Returns the latest generated code
        /// </summary>
        /// <param name="phoneNum">Phone number</param>
        /// <returns></returns>
        Task<ISmsVerificationCode> GetActualCode(string phoneNum);

        Task<bool> CheckAsync(string phoneNum, string codeToCheck);

        Task DeleteCodesByPhoneNumAsync(string phoneNum);
    }
}
