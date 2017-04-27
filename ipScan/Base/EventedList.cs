using System.Collections.Generic;
using System.Linq;

namespace ipScan.Base
{
    public class EventedList<T> : List<T>
    {
        public event ListChangedEventDelegate onChanged;
        public delegate void ListChangedEventDelegate();

        public new void Add(T item)
        {
            base.Add(item);
            if (onChanged != null && onChanged.GetInvocationList().Any())
            {
                onChanged();
            }
        }
    }
}
