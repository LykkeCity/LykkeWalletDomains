using System;
using System.Linq;
using System.Threading.Tasks;

namespace Core.EventLogs
{
    public interface IApiSuccessfulCallRepository
    {
        Task InsertRecordAsync(string method, string clientId);
        Task<DateTime[]> GetCallHistoryAsync(string method, string clientId, TimeSpan period);
    }

    public static class ApiCallHistoryExt
    {
        public static bool IsCallEnabled(this DateTime[] history, TimeSpan period)
        {
            return history.Length == 1 || DateTime.UtcNow - history.Last() > period;
        }
    }
}