using System;
using Microsoft.AspNetCore.DataProtection;

namespace CacheFactory.Distributed
{
    internal class NoopDataProtector : IDataProtector
    {
        private static readonly Lazy<NoopDataProtector> s_instance =
            new Lazy<NoopDataProtector>(() => new NoopDataProtector());
        public static NoopDataProtector Instance => s_instance.Value;

        private NoopDataProtector() { }

        public IDataProtector CreateProtector(string purpose)
        {
            return this;
        }

        public byte[] Protect(byte[] plaintext)
        {
            return plaintext;
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return protectedData;
        }
    }
}
