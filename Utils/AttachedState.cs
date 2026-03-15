using System.Runtime.CompilerServices;

namespace STS2RitsuLib.Utils
{
    /// <summary>
    ///     Stores mod-attached state on arbitrary reference objects without subclassing or boxing through object APIs.
    /// </summary>
    public sealed class AttachedState<TKey, TValue>(Func<TKey, TValue>? valueFactory)
        where TKey : class
    {
        private readonly ConditionalWeakTable<TKey, Box> _table = [];
        private readonly Func<TKey, TValue> _valueFactory = valueFactory ?? (_ => default!);

        public AttachedState(Func<TValue>? defaultValueFactory = null)
            : this(_ => defaultValueFactory != null ? defaultValueFactory() : default!)
        {
        }

        public TValue this[TKey key]
        {
            get => GetOrCreate(key);
            set => Set(key, value);
        }

        public TValue GetOrCreate(TKey key)
        {
            ArgumentNullException.ThrowIfNull(key);
            return _table.GetValue(key, k => new(_valueFactory(k))).Value;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            ArgumentNullException.ThrowIfNull(key);

            if (_table.TryGetValue(key, out var box))
            {
                value = box.Value;
                return true;
            }

            value = default!;
            return false;
        }

        public TValue Set(TKey key, TValue value)
        {
            ArgumentNullException.ThrowIfNull(key);
            _table.Remove(key);
            _table.Add(key, new(value));
            return value;
        }

        public TValue Update(TKey key, Func<TValue, TValue> updater)
        {
            ArgumentNullException.ThrowIfNull(updater);
            var updated = updater(GetOrCreate(key));
            return Set(key, updated);
        }

        public bool Remove(TKey key)
        {
            ArgumentNullException.ThrowIfNull(key);
            return _table.Remove(key);
        }

        private sealed class Box(TValue value)
        {
            public TValue Value { get; } = value;
        }
    }
}
