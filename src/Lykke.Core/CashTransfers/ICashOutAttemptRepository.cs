using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.CashTransfers
{
    public interface ICashOutAttemptRepository : ICashOutBaseRepository
    {
        Task InsertRequestAsync<T>(ICashOutRequest request, PaymentSystem paymentSystem, T paymentFields);
        Task<IEnumerable<ICashOutRequest>> GetAllAttempts();
    }
}
