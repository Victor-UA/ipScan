using System;
using System.Drawing;
using ipScan.Classes.IP;
using SourceGrid;

namespace ipScan.Classes.Host.Grid
{
    public class GridCellController : SourceGrid.Cells.Controllers.ControllerBase
    {
        private SourceGrid.Cells.Views.Cell MouseEnterView = new SourceGrid.Cells.Views.Cell();
        private SourceGrid.Cells.Views.Cell MouseLeaveView = new SourceGrid.Cells.Views.Cell();

        public IPInfo ipInfo { get; private set; }

        public GridCellController(IPInfo IPInfo, Color BackColor)
        {
            MouseEnterView.BackColor = BackColor;
            ipInfo = IPInfo;
        }
        public GridCellController() : this(null, Color.LightGreen) { }
        public override void OnMouseEnter(SourceGrid.CellContext sender, EventArgs e)
        {
            base.OnMouseEnter(sender, e);
            sender.Cell.View = MouseEnterView;
            sender.Grid.InvalidateCell(sender.Position);
        }
        public override void OnMouseLeave(SourceGrid.CellContext sender, EventArgs e)
        {
            base.OnMouseLeave(sender, e);
            sender.Cell.View = MouseLeaveView;
            sender.Grid.InvalidateCell(sender.Position);
        }
        public override void OnDoubleClick(CellContext sender, EventArgs e)
        {
            base.OnDoubleClick(sender, e);
            ipInfo.ShowHostForm();
        }
    }
}
