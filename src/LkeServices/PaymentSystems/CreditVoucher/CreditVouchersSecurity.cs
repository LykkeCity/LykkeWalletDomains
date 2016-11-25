using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Core.PaymentSystems.CreditVoucher;
using Core.Settings;

namespace LkeServices.PaymentSystems.CreditVoucher
{
    public class CreditVouchersSecurity: ICreditVouchersSecurity
    {
        private readonly CreditVouchersSettings _creditVouchersSettings;

        public CreditVouchersSecurity(BaseSettings baseSettings)
        {
            _creditVouchersSettings = baseSettings.PaymentSystems.CreditVouchers;
        }

        //See api documentation https://www.creditvouchers.com/site/getapi.html Appendix A
        public string CalculateHeaderCheckSum(IDictionary<string, string> inputProperties)
        {
            var propertyNamesToExclude = new[] { "CheckSumHeader", "CheckSum" };
            var properties = inputProperties
                .Where(p => !string.IsNullOrEmpty(p.Value) && !propertyNamesToExclude.Contains(p.Key))
                .ToList();

            var paramsCount = properties.Count.ToString("00");

            var paramsNames = string.Join(",", properties.Select(p => p.Key));

            var paramsValuesLengths = string.Join("", properties.Select(p => CalculateBytesLength(p.Value)));

            return $"{paramsCount}{paramsNames},{paramsValuesLengths}";
        }

        //See api documentation https://www.creditvouchers.com/site/getapi.html Appendix A
        public string CalculateCheckSum(IDictionary<string, string> inputProperties, string header)
        {
            var propertyNamesToExclude = new[] { "CheckSumHeader", "CheckSum" };
            var properties = inputProperties
                .Where(p => !string.IsNullOrEmpty(p.Value) && !propertyNamesToExclude.Contains(p.Key))
                .ToList();

            var paramsValues = string.Join("", properties.Select(p => p.Value));
            var hashedApiPassword = ComputeMd5Hash(_creditVouchersSettings.ApiPassword);

            var inputString = $"{header}{paramsValues}{hashedApiPassword}{_creditVouchersSettings.ApiKey}";

            return ComputeMd5Hash(inputString).ToUpper();
        }

        private string ComputeMd5Hash(string source)
        {
            using (var provider = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(source);
                var encrypted = provider.ComputeHash(bytes);
                return BitConverter.ToString(encrypted).Replace("-", "").ToLower();
            }
        }

        private string CalculateBytesLength(string source)
        {
            return Encoding.UTF8.GetBytes(source).Length.ToString("000");
        }
    }
}
