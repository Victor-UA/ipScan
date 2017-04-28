using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using ipScan.Base.IP;
using ipScan.Classes.Grid;

namespace ipScan.Base.Grid
{
    static class Fill
    {
        private static SourceGrid.Cells.Cell newCell<T, TSub>(
            T Item,
            TSub subItem,                 
            SourceGrid.Cells.Controllers.IController Controller = null)
        {
            SourceGrid.Cells.Cell cell = new SourceGrid.Cells.Cell(subItem);
            cell.Tag = Item;
            if (Controller != null)
            {
                cell.AddController(Controller);
            }
            return cell;
        }
        
        public static void GridFill<T>(
            SourceGrid.Grid grid, 
            List<T> List,
            Func<T, Color, GridCellController<T>> newCellController = null,                         
            List<string> Fields = null)
        {
            grid.Columns.Clear();
            grid.Rows.Clear(); 

            List<string> fields = Fields == null ?
                new List<string>() { "Undefined" } :
                Fields;

            //Columns filling
            grid.ColumnsCount = fields.Count;
            grid.FixedRows = 1;
            grid.Rows.Insert(0);
            for (int i = 0; i < (fields.Count); i++)
            {
                grid[0, i] = new SourceGrid.Cells.ColumnHeader(fields[i]);
            }

            //Data filling
            GridUpdateOrInsertRows(grid, List, newCellController);
        }        


        public static void GridAddRows<T>(
            SourceGrid.Grid grid,
            List<T> List, 
            Func<T, Color, GridCellController<T>> newCellController = null)
        {
            if (List != null)
            {                
                int index = grid.RowsCount;
                for (int i = 0; i < List.Count; i++)
                {
                    GridInsertRow(grid, List[i], newCellController);
                }
            }
            grid.AutoSizeCells();
        }

        public static void GridUpdateOrInsertRows<T>(
            SourceGrid.Grid grid,
            List<T> List,
            Func<T, Color, GridCellController<T>> newCellController = null)
        {
            if (List != null)
            {
                for (int i = 0; i < List.Count; i++)
                {
                    T item = List[i];
                    if (!GridSeekRowAndUpdate(grid, item))
                    {
                        GridInsertRow(grid, item, newCellController);
                    }
                }                
            }
            grid.AutoSizeCells();
        }

        public static bool GridSeekRowAndUpdate(SourceGrid.Grid grid, object item)
        {
            bool rowExists = false;
            for (int j = 1; j < grid.RowsCount; j++)
            {
                if (grid.Rows[j].Tag == item)
                {
                    GridUpdateRow(grid, item, j);
                    rowExists = true;
                    break;
                }
            }
            return rowExists;
        }

        public static void GridUpdateRow(SourceGrid.Grid grid, object item, int Index)
        {
            grid.Rows[Index].Tag = item;
            Type itemType = item.GetType();
            if (itemType == typeof(IPInfo))
            {
                IPInfo ipInfo = item as IPInfo;
                grid[Index, 0].Value = ipInfo.IPAddress;
                grid[Index, 0].Tag = ipInfo;
                grid[Index, 1].Value = ipInfo.RoundtripTime;
                grid[Index, 1].Tag = ipInfo;
                grid[Index, 2].Value = ipInfo.HostName;
                grid[Index, 2].Tag = ipInfo;                
                return;
            }

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

        public static void GridInsertRow<T>(
            SourceGrid.Grid grid,
            T item, 
            Func<T, Color, GridCellController<T>> newCellController = null,
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
                grid[index, 0] = newCell(ipInfo, ipInfo.IPAddress, CellController);            
                grid[index, 1] = newCell(ipInfo, ipInfo.RoundtripTime, CellController);
                grid[index, 2] = newCell(ipInfo, ipInfo.HostName, CellController);
                return;
            }
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
