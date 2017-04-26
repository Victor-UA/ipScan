﻿using System.Linq;
using ipScan.Classes.IP;

namespace ipScan.Classes.Main
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

        public ListIPInfo getBuffer(int Count, bool changeIndex = true)
        {                                   
            if(changeIndex)
                Index += Count;            
            return Buffer.GetRange(Index - Count, Count);
            
        }
        public ListIPInfo getBuffer(bool changeIndex = true)
        {
            int count = Buffer.Count() - Index;
            return getBuffer(count, changeIndex);
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
        public ListIPInfo getAllBuffer()
        {
            return Buffer;
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
