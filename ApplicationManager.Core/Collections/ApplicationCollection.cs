using System;
using System.Collections;
using System.Collections.Generic;

namespace ApplicationManager.Core.Collections
{
    public class ApplicationCollection<T> : IEnumerable<T>
        where T : class
    {
        private readonly List<T> _items;

        public ApplicationCollection(IEnumerable<T> items)
        {
            _items = new List<T>(items ?? Array.Empty<T>());
        }

        public int Count => _items.Count;
        public T this[int index] => _items[index];

        public IEnumerator<T> GetEnumerator() => new ApplicationEnumerator(_items);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // REIKALAVIMAS: Sukurtas ir naudojamas iteratorius (0.5 t.)
        // Metodas YieldAll() naudoja 'yield return' ir yra panaudotas Program.cs.
        public IEnumerable<T> YieldAll()
        {
            for (int i = 0; i < _items.Count; i++)
                yield return _items[i];
        }

        // REIKALAVIMAS: Teisingai atlikote implementaciją IEnumerable<T> / IEnumerator<T> (2 t.)
        // ApplicationCollection<T> realizuoja IEnumerable<T>, o vidinis ApplicationEnumerator realizuoj IEnumerator<T>.

        private sealed class ApplicationEnumerator : IEnumerator<T>
        {
            private readonly List<T> _list;
            private int _index = -1;

            public ApplicationEnumerator(List<T> list) => _list = list;

            public T Current => _list[_index];
            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _index++;
                return _index < _list.Count;
            }

            public void Reset() => _index = -1;
            public void Dispose() { }
        }
    }
}
