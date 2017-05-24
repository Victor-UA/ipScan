using System;
using System.Collections;
using System.Diagnostics;

namespace ipScan.Base.IP
{
    class PhysicalAddressComparer : IComparer
    {
        public PhysicalAddressComparer() : base()
        {

        }

        public int Compare(object x, object y)
        {
            try
            {
                UInt64 X;
                UInt64 Y;
                string xString = x.ToString();
                string yString = y.ToString();
                if (!string.IsNullOrEmpty(xString))
                {
                    try
                    {
                        X = UInt64.Parse(xString, System.Globalization.NumberStyles.HexNumber);
                    }
                    catch (Exception)
                    {
                        X = 0;
                    }
                }
                else
                {
                    X = 0;
                }
                if (!string.IsNullOrEmpty(yString))
                {
                    try
                    {
                        Y = UInt64.Parse(yString, System.Globalization.NumberStyles.HexNumber);
                    }
                    catch (Exception)
                    {
                        Y = 0;
                    }
                }
                else
                {
                    Y = 0;
                }
                if (X > Y)
                    return 1;
                if (X < Y)
                    return -1;
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
