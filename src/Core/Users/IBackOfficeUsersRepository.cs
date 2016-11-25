using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Users
{
    public interface IBackOfficeUser
    {
        string Id { get; }
        string FullName { get; }
        string[] Roles { get; }
        bool IsAdmin { get; }
    }

    public class BackOfficeUser : IBackOfficeUser
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string[] Roles { get; set; }
        public bool IsAdmin { get; set; }


        public static BackOfficeUser CreateDefaultAdminUser(string id)
        {
            return new BackOfficeUser
            {
                Id = id,
                FullName = "Admin",
                IsAdmin = true,
                Roles = new string[0]
            };
        }

        public static IBackOfficeUser CreateDefault()
        {
            return new BackOfficeUser
            {
                IsAdmin = false,
                Roles = new string[0]
            };
        }
    }

    public interface IBackOfficeUsersRepository
    {
        Task CreateAsync(IBackOfficeUser backOfficeUser, string password);
        Task UpdateAsync(IBackOfficeUser backOfficeUser);

        Task<IBackOfficeUser> AuthenticateAsync(string username, string password);
        Task<IBackOfficeUser> GetAsync(string id);

        Task<bool> UserExists(string id);
        Task ChangePasswordAsync(string id, string newPassword);
        Task<IEnumerable<IBackOfficeUser>> GetAllAsync();

    }
}
