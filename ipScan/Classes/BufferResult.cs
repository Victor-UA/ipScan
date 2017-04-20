using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ipScan.Classes
{
    class BufferResult
    {
        public ListIPInfo Buffer { get; private set; }
        public int Index { get; private set; }
        public BufferResult()
        {
            Buffer = new ListIPInfo();
        }

        public void AddLine(IPInfo Line)
        {
            Buffer.Add(Line);
        }
        public void AddLines(ListIPInfo Lines)
        {
            Buffer.AddRange(Lines);
        }
        public ListIPInfo getBuffer(int count, bool changeIndex = true)
        {
            if(changeIndex)
                Index += count;
            return Buffer.GetRange(Index - count, count);
        }
        public ListIPInfo getBuffer()
        {
            int count = Buffer.Count() - Index;
            return this.getBuffer(count);
        }
        public ListIPInfo getAllBuffer()
        {
            return Buffer;
        }
        public ListIPInfo getBuffer(ListIPInfo oldLines)
        {
            int count = Buffer.Count() - Index;
            oldLines.AddRange(getBuffer(count));
            return oldLines;
        }
        public ListIPInfo getBufferSorted(ListIPInfo oldLines)
        {
            int count = Buffer.Count() - Index;
            oldLines.AddRange(getBuffer(count));
            oldLines.Sort();
            return oldLines;
        }
        public ListIPInfo getAllBufferSorted()
        {
            ListIPInfo sortedBuffer = new ListIPInfo();
            sortedBuffer.AddRange(Buffer);
            sortedBuffer.Sort(new IPInfoComparer());
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
