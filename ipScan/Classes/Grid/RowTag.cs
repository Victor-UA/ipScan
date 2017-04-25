namespace ipScan.Classes.Grid
{
    public class RowTag : object
    {
        public int Index { get; set; }
        public object Key { get; set; }
        public RowTag(int index, object keyValue)
        {
            Index = index;
            Key = keyValue;
        }
        public RowTag() : this(-1, null) { }
    }
}
