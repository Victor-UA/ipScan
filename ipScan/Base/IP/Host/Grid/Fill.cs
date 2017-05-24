using System;
using System.Drawing;

namespace ipScan.Base.IP.Host.Grid
{
    public class Fill : Base.Grid.Fill
    {
        public override void GridUpdateRow(
            SourceGrid.Grid grid, object item, int Index)
        {
            grid.Rows[Index].Tag = item;
            Type itemType = item.GetType();            
            if (itemType == typeof(PortInfo))
            {
                PortInfo portInfo = item as PortInfo;
                grid[Index, 0].Value = portInfo.Port;
                grid[Index, 0].Tag = portInfo;
                grid[Index, 1].Value = portInfo.ProtocolType;
                grid[Index, 1].Tag = portInfo;
                grid[Index, 2].Value = portInfo.isOpen;
                grid[Index, 2].Tag = portInfo;
                return;
            }

            {
                grid[Index, 0].Value = item;
            }
        }
        public override void GridInsertRow<T>(
            SourceGrid.Grid grid,
            T item,
            Func<T, Color, Base.Grid.GridCellController> newCellController = null, 
            int Index = -1)                
        {
            int index = Index < 0 ? grid.RowsCount : Index;
            grid.Rows.Insert(index);
            grid.Rows[index].Tag = item;

            Type itemType = item.GetType();
            if (itemType == typeof(PortInfo))
            {
                PortInfo portInfo = item as PortInfo;
                SourceGrid.Cells.Controllers.IController CellController = newCellController(item, Color.LightBlue);
                grid[index, 0] = newCell(portInfo, portInfo.Port, CellController);
                grid[index, 1] = newCell(portInfo, portInfo.ProtocolType, CellController);
                grid[index, 2] = newCell(portInfo, portInfo.isOpen, CellController);
                return;
            }

            {
                SourceGrid.Cells.Controllers.IController CellController = newCellController(item, Color.LightBlue);
                grid[index, 0] = newCell(item, item, CellController);
            }
        }        
    }
}
