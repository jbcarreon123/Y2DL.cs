using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Util;

namespace Y2DL.Models
{
    public class Listable<T> : IEnumerable<T>
    {
        private List<T> items;

        public Listable()
        {
            items = new List<T>();
        }

        public Listable(T item)
        {
            items = new List<T> { item };
        }

        public Listable(IEnumerable<T> collection)
        {
            items = new List<T>(collection);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            items.Add(item);
        }

        public void AddRange(IEnumerable<T> itemsEnumerable)
        {
            items.AddRange(itemsEnumerable);
        }

        public bool Remove(T item)
        {
            return items.Remove(item);
        }
        
        public void RemoveDuplicates()
        {
            items = items.Distinct().ToList();
        }

        public Repeatable<T> ToRepeatable()
        {
            return new Repeatable<T>(items);
        }
        
        public static implicit operator List<T>(Listable<T> listable)
        {
            return listable.items;
        }
        
        public static implicit operator Listable<T>(T item)
        {
            return new Listable<T>(item);
        }
        
        public static implicit operator Repeatable<T>(Listable<T> listable)
        {
            return listable.items;
        }
        
        public static implicit operator T(Listable<T> listable)
        {
            return listable.items.FirstOrDefault();
        }
        
        public static implicit operator Listable<T>(List<T> list)
        {
            return new Listable<T>(list);
        }
        
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= items.Count)
                {
                    throw new IndexOutOfRangeException("Index is out of range.");
                }

                return items[index];
            }
            set
            {
                if (index < 0 || index >= items.Count)
                {
                    throw new IndexOutOfRangeException("Index is out of range.");
                }

                items[index] = value;
            }
        }
    }
}
