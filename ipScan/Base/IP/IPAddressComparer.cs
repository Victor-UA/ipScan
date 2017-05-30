using System;
using System.Collections;
using System.Diagnostics;
using System.Net;

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
                string[] X;
                string[] Y;
                try
                {
                    X = x.ToString().Split('.');
                }
                catch (Exception)
                {
                    X = new string[] { "0","0","0","0" };
                }
                try
                {
                    Y = y.ToString().Split('.');
                }
                catch (Exception)
                {
                    Y = new string[] { "0", "0", "0", "0" };
                }
                
                for (int i = 0; i < 4; i++)
                {
                    int xInt;
                    int yInt;
                    string xString = X[i];
                    string yString = Y[i];
                    if (string.IsNullOrEmpty(xString))
                    {
                        xInt = 0;
                    }
                    else
                    {
                        try
                        {
                            xInt = int.Parse(xString);
                        }
                        catch (Exception)
                        {
                            xInt = 0;
                        }
                    }
                    if (string.IsNullOrEmpty(yString))
                    {
                        yInt = 0;
                    }
                    else
                    {
                        try
                        {
                            yInt = int.Parse(yString);
                        }
                        catch (Exception)
                        {
                            yInt = 0;
                        }
                    }

                    if (xInt > yInt)
                    {
                        return 1;
                    }
                    if (xInt < yInt)
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
