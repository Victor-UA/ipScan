using System;
using System.Drawing;
using ipScan.Base.Grid;
using ipScan.Base.IP;
using SourceGrid;

namespace ipScan.Classes.Host.Grid
{
    public class GridCellController : GridCellController<PortInfo>
    {
        public GridCellController(PortInfo Item, Color BackColor) : base(BackColor)
        {
        }

        public override void OnDoubleClick(CellContext sender, EventArgs e)
        {
            base.OnDoubleClick(sender, e);            
        }
    }
}
