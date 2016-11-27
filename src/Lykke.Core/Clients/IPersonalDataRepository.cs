using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IPersonalData
    {
        DateTime Regitered { get; }
        string Id { get; }
        string Email { get; }
        string FullName { get; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Country { get; }
        string Zip { get; }
        string City { get; }
        string Address { get; }
        string ContactPhone { get; }
        string ReferralCode { get; }
    }

    public interface IFullPersonalData : IPersonalData
    {
        string PasswordHint { get; set; }
    }

    public class FullPersonalData : IFullPersonalData
    {
        public DateTime Regitered { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string ContactPhone { get; set; }
        public string ReferralCode { get; set; }
        public string PasswordHint { get; set; }

        public static FullPersonalData Create(IClientAccount src, string fullName, string pwdHint)
        {
            return new FullPersonalData
            {
                Id = src.Id,
                Email = src.Email,
                ContactPhone = src.Phone,
                Regitered = src.Registered,
                FullName = fullName,
                Country = "CHE",
                PasswordHint = pwdHint
            };
        }
    }

    public interface IPersonalDataRepository
    {
        Task<IPersonalData> GetAsync(string id);
        Task<IFullPersonalData> GetFullAsync(string id);
        Task<IEnumerable<IPersonalData>> GetAsync(IEnumerable<string> id);
        Task SaveAsync(IFullPersonalData personalData);
        Task<IPersonalData> ScanAndFindAsync(Func<IPersonalData, bool> func);


        Task GetByChunksAsync(Action<IEnumerable<IPersonalData>> callback);

        Task ChangeFullNameAsync(string id, string fullName);
        Task ChangeFirstNameAsync(string id, string firstName);
        Task ChangeLastNameAsync(string id, string lastName);
        Task ChangeCountryAsync(string id, string country);
        Task ChangeCityAsync(string id, string city);
        Task ChangeZipAsync(string id, string zip);
        Task ChangeAddressAsync(string id, string zip);
        Task ChangeContactPhoneAsync(string id, string phoneNumber);
        Task UpdateAsync(IPersonalData personalData);
        Task UpdateGeolocationDataAsync(string id, string countryCode, string city);
        Task ChangePasswordHintAsync(string id, string newHint);
        Task SetReferralCode(string id, string refCode);
    }

}
