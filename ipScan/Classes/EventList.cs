using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ipScan.Classes
{
    public class EventList<T> : List<T>
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
