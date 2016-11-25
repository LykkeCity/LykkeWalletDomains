using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Assets.Margin
{
    public interface IMarginIssuer
    {
        string Id { get; }
        string Name { get; }
    }

    public class MarginIssuer : IMarginIssuer
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public static MarginIssuer Create(string id, string name)
        {
            return new MarginIssuer
            {
                Id = id,
                Name = name
            };
        }

        public static MarginIssuer CreateDefault()
        {
            return new MarginIssuer();
        }

    }

    public interface IMarginIssuerRepository
    {
        Task RegisterIssuerAsync(IMarginIssuer issuer);
        Task EditIssuerAsync(string id, IMarginIssuer issuer);
        Task<IEnumerable<IMarginIssuer>> GetIssuerAsync();
        Task<IMarginIssuer> GetIssuerAsync(string id);
    }
}