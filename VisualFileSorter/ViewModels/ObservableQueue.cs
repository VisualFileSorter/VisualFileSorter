using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualFileSorter.ViewModels
{
    public class ObservableQueue<T> : INotifyCollectionChanged, IEnumerable<T>
    {
        private ObservableCollection<T> _collection;

        public ObservableQueue()
        {
            this.ListenToCollection(new ObservableCollection<T>());
        }

        public ObservableQueue(IEnumerable<T> enumerable)
        {
            this.ListenToCollection(new ObservableCollection<T>(enumerable));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Enqueue(T item)
        {
            this._collection.Add(item);
        }

        /// <summary>
        /// Returns and removes the item at the bottom (start) of the queue
        /// </summary>
        public T Dequeue()
        {
            if (this._collection.Count == 0)
            {
                return default(T);
            }

            var item = this._collection[0];
            this._collection.RemoveAt(0);
            return item;
        }

        public ObservableCollection<T> GetCollection()
        {
            return this._collection;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this._collection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._collection.GetEnumerator();
        }

        private void ListenToCollection(ObservableCollection<T> collection)
        {
            if (this._collection != null)
            {
                throw new InvalidOperationException("ListenToCollection method has been called twice");
            }

            this._collection = collection;
            this._collection.CollectionChanged += _collection_CollectionChanged;
        }

        void _collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, e);
            }
        }
    }
}
