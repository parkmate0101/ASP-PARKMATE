using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace ParkMate.Services
{
    // Short-lived, server-side token used only for the receipt download
    // button immediately after a successful payment. This prevents the
    // user from being asked to login a second time while keeping the
    // normal Payments page download protected by the user's session.
    public class ReceiptAccessService
    {
        private readonly ConcurrentDictionary<string, ReceiptAccessEntry> _tokens = new();
        private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

        public string CreateToken(int paymentId)
        {
            RemoveExpired();

            byte[] bytes = RandomNumberGenerator.GetBytes(32);
            string token = Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            _tokens[token] = new ReceiptAccessEntry(
                paymentId,
                DateTime.UtcNow.Add(Lifetime));

            return token;
        }

        public bool TryGetPaymentId(string token, out int paymentId)
        {
            paymentId = 0;

            if (string.IsNullOrWhiteSpace(token) ||
                !_tokens.TryGetValue(token, out var entry))
            {
                return false;
            }

            if (entry.ExpiresAtUtc <= DateTime.UtcNow)
            {
                _tokens.TryRemove(token, out _);
                return false;
            }

            paymentId = entry.PaymentId;
            return true;
        }

        private void RemoveExpired()
        {
            DateTime now = DateTime.UtcNow;

            foreach (var item in _tokens)
            {
                if (item.Value.ExpiresAtUtc <= now)
                {
                    _tokens.TryRemove(item.Key, out _);
                }
            }
        }

        private sealed record ReceiptAccessEntry(
            int PaymentId,
            DateTime ExpiresAtUtc);
    }
}
