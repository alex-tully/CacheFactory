namespace CacheFactory.Memory
{
    internal class CacheEntryKey
    {
        // WE DON'T NEED TO OVERRIDE ==, != etc.. as this class is internal and won't be used outside of this context

        private readonly string _name;
        private readonly object _key;

        public CacheEntryKey(string name, object key)
        {
            _name = name;
            _key = key;
        }

        public override int GetHashCode()
        {
            return (_name.GetHashCode() + _key.GetHashCode()) * 17;
        }

        public override bool Equals(object obj)
        {
            if (obj is CacheEntryKey key)
            {
                return _key.Equals(key._key) && _name.Equals(key._name);
            }

            return false;
        }
    }
}
