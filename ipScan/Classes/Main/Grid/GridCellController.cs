using System;
using System.Drawing;
using ipScan.Base.Grid;
using ipScan.Base.IP;
using SourceGrid;

namespace ipScan.Classes.Main.Grid
{
    public class GridCellController : GridCellController<IPInfo>
    {
        public GridCellController(IPInfo Item, Color BackColor) : base(BackColor)
        {
        }

        public override void OnDoubleClick(CellContext sender, EventArgs e)
        {
            base.OnDoubleClick(sender, e);
            ((sender.Cell as SourceGrid.Cells.Cell).Tag as IPInfo).ShowHostForm();
        }
    }
}
