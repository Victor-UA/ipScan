using System;
using System.Drawing;
using ipScan.Base.IP;
using SourceGrid;

namespace ipScan.Base.Grid
{
    abstract public class GridCellController<T> : SourceGrid.Cells.Controllers.ControllerBase
    {
        protected SourceGrid.Cells.Views.Cell MouseEnterView = new SourceGrid.Cells.Views.Cell();
        protected SourceGrid.Cells.Views.Cell MouseLeaveView = new SourceGrid.Cells.Views.Cell();

        public T Item { get; private set; }

        public GridCellController(T Item, Color BackColor)
        {
            MouseEnterView.BackColor = BackColor;
            this.Item = Item;
        }
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
    }
}
