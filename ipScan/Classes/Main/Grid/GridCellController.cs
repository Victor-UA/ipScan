using System;
using System.Drawing;
using ipScan.Base.Grid;
using ipScan.Base.IP;
using SourceGrid;

namespace ipScan.Classes.Main.Grid
{
    public class GridCellController : GridCellController<IPInfo>
    {
        public GridCellController(IPInfo Item, Color BackColor) : base(Item, BackColor)
        {
        }

        public override void OnDoubleClick(CellContext sender, EventArgs e)
        {
            base.OnDoubleClick(sender, e);                        
            Item.ShowHostForm();
        }
    }
}
