using System.Threading.Tasks;

namespace Core.Exchange
{
    public interface IDoneLimitOrdersRepositry
    {
        Task AddAsync(ILimitOrder limitOrder);
        Task<ILimitOrder> GetAsync(string id);
    }
}
