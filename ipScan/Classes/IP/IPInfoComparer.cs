using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ipScan.Classes.IP
{
    class IPInfoComparer : IComparer<IPInfo>
    {
        public int Compare(IPInfo x, IPInfo y)
        {
            try
            {
                string[] X = x.IPAddress.ToString().Split('.');
                string[] Y = y.IPAddress.ToString().Split('.');
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
    }
}
