namespace ipScan.Classes.Grid
{
    public class RowTag : object
    {
        public int Index { get; set; }
        public object Key { get; set; }
        public RowTag(int index, object Key)
        {
            Index = index;
            this.Key = Key;
        }
        public RowTag() : this(-1, null) { }
        public RowTag(object Key) : this(-1, Key) { }
    }
}
