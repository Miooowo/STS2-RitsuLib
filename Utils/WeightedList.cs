using System.Collections;
using MegaCrit.Sts2.Core.Random;

namespace STS2RitsuLib.Utils
{
    public interface IWeightedValue
    {
        int Weight { get; }
    }

    /// <summary>
    ///     Weighted random container with optional draw-without-replacement support.
    /// </summary>
    public class WeightedList<T> : IList<T>
    {
        private readonly List<Entry> _entries = [];

        public int TotalWeight { get; private set; }

        public bool IsReadOnly => false;
        public int Count => _entries.Count;

        public T this[int index]
        {
            get => _entries[index].Value;
            set => _entries[index] = new(value, _entries[index].Weight);
        }

        public void Add(T item)
        {
            Add(item, item is IWeightedValue weighted ? weighted.Weight : 1);
        }

        public void Clear()
        {
            _entries.Clear();
            TotalWeight = 0;
        }

        public bool Contains(T item)
        {
            return _entries.Any(entry => EqualityComparer<T>.Default.Equals(entry.Value, item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _entries.Select(entry => entry.Value).ToList().CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _entries.Select(entry => entry.Value).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            for (var i = 0; i < _entries.Count; i++)
                if (EqualityComparer<T>.Default.Equals(_entries[i].Value, item))
                    return i;

            return -1;
        }

        public void Insert(int index, T item)
        {
            Insert(index, item, item is IWeightedValue weighted ? weighted.Weight : 1);
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
                return false;

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            var entry = _entries[index];
            _entries.RemoveAt(index);
            TotalWeight -= entry.Weight;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item, int weight)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(weight);
            _entries.Add(new(item, weight));
            TotalWeight += weight;
        }

        public void AddRange(IEnumerable<T> items, Func<T, int>? weightSelector = null)
        {
            ArgumentNullException.ThrowIfNull(items);

            foreach (var item in items)
                Add(item, weightSelector?.Invoke(item) ?? (item is IWeightedValue weighted ? weighted.Weight : 1));
        }

        public T GetRandom(Rng rng, bool remove = false)
        {
            ArgumentNullException.ThrowIfNull(rng);

            if (_entries.Count == 0 || TotalWeight <= 0)
                throw new InvalidOperationException("Cannot roll from an empty weighted list.");

            var roll = rng.NextInt(TotalWeight);
            var cumulative = 0;

            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                cumulative += entry.Weight;
                if (roll >= cumulative)
                    continue;

                if (remove)
                    RemoveAt(i);

                return entry.Value;
            }

            throw new InvalidOperationException($"Weighted roll {roll} exceeded total weight {TotalWeight}.");
        }

        public bool TryGetRandom(Rng rng, out T value, bool remove = false)
        {
            if (_entries.Count == 0 || TotalWeight <= 0)
            {
                value = default!;
                return false;
            }

            value = GetRandom(rng, remove);
            return true;
        }

        public void Insert(int index, T item, int weight)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(weight);
            _entries.Insert(index, new(item, weight));
            TotalWeight += weight;
        }

        private readonly record struct Entry(T Value, int Weight);
    }
}
