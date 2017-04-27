using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ipScan.Classes.IP;

namespace ipScan.Classes
{
    class BufferResult<T>
    {
        public List<T> Buffer { get; private set; }
        public int Index { get; private set; }
        public BufferResult()
        {
            Buffer = new List<T>();
        }

        public void AddLine(T Line)
        {
            Buffer.Add(Line);
        }
        public void AddLines(List<T> Lines)
        {
            Buffer.AddRange(Lines);
        }

        public List<T> getBuffer(int Count, bool changeIndex = true)
        {                                   
            if(changeIndex)
                Index += Count;            
            return Buffer.GetRange(Index - Count, Count);
            
        }
        public List<T> getBuffer(bool changeIndex = true)
        {
            int count = Buffer.Count() - Index;
            return getBuffer(count, changeIndex);
        }
        public List<T> getBuffer(List<T> oldLines)
        {
            int count = Buffer.Count() - Index;
            oldLines.AddRange(getBuffer(count));
            return oldLines;
        }

        public List<T> getBufferSorted(List<T> oldLines)
        {
            int count = Buffer.Count() - Index;
            oldLines.AddRange(getBuffer(count));
            oldLines.Sort();
            return oldLines;
        }
        public List<T> getAllBuffer()
        {
            return Buffer;
        }
        public List<T> getAllBufferSorted(IComparer<T> Comparer)
        {
            List<T> sortedBuffer = new List<T>();
            sortedBuffer.AddRange(Buffer);
            sortedBuffer.Sort(Comparer);
            return sortedBuffer;
        }

        public int getBufferTotalCount
        {
            get
            {
                return Buffer.Count();
            }
        }
        public void Clear()
        {
            Buffer.Clear();
        }
    }
}
