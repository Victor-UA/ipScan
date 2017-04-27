using System;
using System.Collections;
using System.Diagnostics;

namespace ipScan.Base.IP
{
    class IPAddressComparer : IComparer
    {
        public IPAddressComparer() : base()
        {

        }
        public int Compare(object x, object y)
        {
            try
            {
                string[] X = x.ToString().Split('.');
                string[] Y = y.ToString().Split('.');
                for (int i = 0; i < 4; i++)
                {
                    if (int.Parse(X[i]) > int.Parse(Y[i]))
                    {
                        return 1;
                    }
                    if (int.Parse(X[i]) < int.Parse(Y[i]))
                    {
                        return -1;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
                throw new NotImplementedException();
            }
        }
        /*
        public int Compare(object x, object y)
        {
            return ((new SourceGrid.ValueCellComparer()).Compare(y, x));
        }
        */
    }
}
