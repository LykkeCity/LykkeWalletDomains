using System.Threading.Tasks;

namespace Core.Ethereum
{
    public interface ISrvEthereumHelper
    {
        Task<string> GetContract();
        Task<string> GetContractByAddress(string address);
        Task AddAddressForContract(string contract, string address);
    }
}