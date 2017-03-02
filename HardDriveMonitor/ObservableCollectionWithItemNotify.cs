using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace HardDriveMonitor
{
    public class ObservableCollectionWithItemNotify<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        //   public event Action<object,EventArgs> SomethingInCollectionChanged;

        /*  protected virtual void OnSomethingInCollectionChanged(object o, EventArgs e)
        {
            Action<object, EventArgs> handler = SomethingInCollectionChanged;
            if (handler != null) handler(o, e);
        }*/

        public ObservableCollectionWithItemNotify()
        {
            CollectionChanged += items_CollectionChanged;
        }


        public ObservableCollectionWithItemNotify(IEnumerable<T> collection)
            : base(collection)
        {
            CollectionChanged += items_CollectionChanged;
            foreach (INotifyPropertyChanged item in collection)
                item.PropertyChanged += item_PropertyChanged;
        }

        private void items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e != null)
            {
                if (e.OldItems != null)
                    foreach (INotifyPropertyChanged item in e.OldItems)
                        item.PropertyChanged -= item_PropertyChanged;

                if (e.NewItems != null)
                    foreach (INotifyPropertyChanged item in e.NewItems)
                        item.PropertyChanged += item_PropertyChanged;
            }
            //OnSomethingInCollectionChanged(sender, e);
        }

        private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var reset = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(reset);
            //OnSomethingInCollectionChanged(sender, e);
        }
    }
}