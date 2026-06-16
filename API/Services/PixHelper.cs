using System.Text;
using System.Globalization;

namespace ABrechozeiraApp.Services
{
    public static class PixHelper
    {
        public static string GeneratePixPayload(string pixKey, string merchantName, string merchantCity, decimal amount, string transactionId = "BRECHOZEIRA")
        {
            var payload = new StringBuilder();
            payload.Append("000201"); // Payload Format Indicator

            var gui = "0014br.gov.bcb.pix";
            var keyField = $"01{pixKey.Length:D2}{pixKey}";
            var accountInfo = $"{gui}{keyField}";
            payload.Append($"26{accountInfo.Length:D2}{accountInfo}");

            payload.Append("52040000"); // Merchant Category Code
            payload.Append("5303986"); // Transaction Currency

            var amountStr = amount.ToString("0.00", CultureInfo.InvariantCulture);
            payload.Append($"54{amountStr.Length:D2}{amountStr}"); // Transaction Amount

            payload.Append("5802BR"); // Country Code

            // Clean limits
            merchantName = merchantName.Length > 25 ? merchantName.Substring(0, 25) : merchantName;
            payload.Append($"59{merchantName.Length:D2}{merchantName}");

            merchantCity = merchantCity.Length > 15 ? merchantCity.Substring(0, 15) : merchantCity;
            payload.Append($"60{merchantCity.Length:D2}{merchantCity}");

            var txIdField = $"05{transactionId.Length:D2}{transactionId}";
            payload.Append($"62{txIdField.Length:D2}{txIdField}");

            payload.Append("6304"); // CRC Prefix

            string payloadStr = payload.ToString();
            string crc = ComputeCrc16(payloadStr);
            return $"{payloadStr}{crc}";
        }

        private static string ComputeCrc16(string payload)
        {
            int crc = 0xFFFF;
            int polynomial = 0x1021;

            foreach (var b in Encoding.ASCII.GetBytes(payload))
            {
                crc ^= b << 8;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (crc << 1) ^ polynomial;
                    else
                        crc <<= 1;
                }
            }
            return (crc & 0xFFFF).ToString("X4");
        }
    }
}
