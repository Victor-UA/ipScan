using System;
using System.Drawing;
using ipScan.Base.IP;
using SourceGrid;

namespace ipScan.Base.Grid
{
    abstract public class GridCellController<T> : SourceGrid.Cells.Controllers.ControllerBase
    {        
        public SourceGrid.Cells.Views.Cell DefaultView { get; set; }
        public SourceGrid.Cells.Views.Cell MouseEnterView { get; set; }

        public GridCellController(Color MouseEnterBackColor)
        {
            MouseEnterView = new SourceGrid.Cells.Views.Cell();
            MouseEnterView.BackColor = MouseEnterBackColor;
            MouseEnterView.TextAlignment = DevAge.Drawing.ContentAlignment.MiddleCenter;
        }
        public override void OnMouseEnter(SourceGrid.CellContext sender, EventArgs e)
        {
            base.OnMouseEnter(sender, e);
            DefaultView = sender.Cell.View as SourceGrid.Cells.Views.Cell;
            sender.Cell.View = MouseEnterView;
            sender.Grid.InvalidateCell(sender.Position);
        }
        public override void OnMouseLeave(SourceGrid.CellContext sender, EventArgs e)
        {
            base.OnMouseLeave(sender, e);
            sender.Cell.View = DefaultView;
            sender.Grid.InvalidateCell(sender.Position);
        }        
    }
}
