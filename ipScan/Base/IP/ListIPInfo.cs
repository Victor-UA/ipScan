using System.Collections.Generic;
using System.Linq;

namespace ipScan.Base.IP
{
    class ListIPInfo : List<IPInfo>
    {
        public ListIPInfo() : base()
        {
        }

        public string[] toArray()
        {
            string[] result = new string[this.Count()];
            int i = 0;
            foreach (IPInfo element in this)
            {
                result[i] = element.toString();
                i++;
            }
            return result;
        }
        public new ListIPInfo GetRange(int index, int count)
        {
            ListIPInfo result = new ListIPInfo();
            result.AddRange(base.GetRange(index, count));
            return result;
        }        

    }
}
