using System;
using System.Drawing;
using ipScan.Base.Grid;
using SourceGrid;

namespace ipScan.Classes.Host.Grid
{
    public class GridCellController : GridCellController<object>
    {
        public GridCellController(object Item, Color BackColor) : base(Item, BackColor)
        {
        }

        public override void OnDoubleClick(CellContext sender, EventArgs e)
        {
            base.OnDoubleClick(sender, e);            
        }
    }
}
