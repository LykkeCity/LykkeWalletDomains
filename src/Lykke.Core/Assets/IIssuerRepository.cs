using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Assets
{
    public interface IIssuer
    {
        string Id { get; }
        string Name { get; }
        byte[] Icon { get; }
    }

    public class Issuer : IIssuer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public byte[] Icon { get; set; }


        public static Issuer Create(string id, string name, byte[] icon)
        {
            return new Issuer
            {
                Id = id,
                Name = name,
                Icon = icon
            };
        }

        public static Issuer CreateDefault()
        {
            return new Issuer
            {

            };
        }

    }

    public interface IIssuerRepository
    {
        Task RegisterIssuerAsync(IIssuer issuer);
        Task EditIssuerAsync(string id, IIssuer issuer);
        Task<IEnumerable<IIssuer>> GetIssuerAsync();
        Task<IIssuer> GetIssuerAsync(string id);
    }



}