using System;
using System.Drawing;
using SourceGrid;

namespace ipScan.Base.IP.Host.Grid
{
    public class GridCellController : Base.Grid.GridCellController
    {
        public GridCellController(Color BackColor) : base(BackColor)
        {
        }

        public override void OnDoubleClick(CellContext sender, EventArgs e)
        {
            base.OnDoubleClick(sender, e);            
        }
    }
}
