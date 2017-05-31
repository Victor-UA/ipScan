using System;
using System.Drawing;
using SourceGrid;

namespace ipScan.Base.IP.Grid
{
    public class Fill : Base.Grid.Fill
    {
        public override void GridUpdateRow(
            SourceGrid.Grid grid, object item, int Index)
        {
            grid.Rows[Index].Tag = item;
            Type itemType = item.GetType();
            if (itemType == typeof(IPInfo))
            {
                IPInfo ipInfo = item as IPInfo;
                grid[Index, 0].Value = ipInfo.IPAddressStr;
                grid[Index, 0].Tag = ipInfo;
                grid[Index, 1].Value = ipInfo.RoundtripTime;
                grid[Index, 1].Tag = ipInfo;
                grid[Index, 2].Value = ipInfo.HostName;
                grid[Index, 2].Tag = ipInfo;
                grid[Index, 3].Value = ipInfo.HostMac;
                grid[Index, 2].Tag = ipInfo;
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
            if (itemType == typeof(IPInfo))
            {
                IPInfo ipInfo = item as IPInfo;
                SourceGrid.Cells.Controllers.IController CellController = newCellController(item, Color.LightBlue);
                grid[index, 0] = newCell(ipInfo, ipInfo.IPAddressStr, CellController);
                grid[index, 1] = newCell(ipInfo, ipInfo.RoundtripTime, CellController);
                grid[index, 2] = newCell(ipInfo, ipInfo.HostName, CellController);
                grid[index, 3] = newCell(ipInfo, ipInfo.HostMac, CellController);
                return;
            }            

            {
                SourceGrid.Cells.Controllers.IController CellController = newCellController(item, Color.LightBlue);
                grid[index, 0] = newCell(item, item, CellController);
            }
        }        
    }
}
