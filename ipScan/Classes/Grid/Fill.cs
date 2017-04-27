using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using ipScan.Classes.IP;

namespace ipScan.Classes.Grid
{
    static class Fill
    {
        public static SourceGrid.Cells.Cell newCell(
            object Value, 
            SourceGrid.Cells.Controllers.IController Controller = null)
        {
            SourceGrid.Cells.Cell cell = new SourceGrid.Cells.Cell(Value);
            if (Controller != null)
            {
                cell.AddController(Controller);
            }
            return cell;
        }
        
        public static void GridFill<T>(
            SourceGrid.Grid grid, 
            List<T> List,
            Func<T, Color, SourceGrid.Cells.Controllers.IController> newCellController = null,                         
            List<string> Fields = null)
        {
            grid.Columns.Clear();
            grid.Rows.Clear(); 

            List<string> fields = Fields == null ?
                new List<string>() { "Port" } :
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
            if (List != null)
            {                
                for (int i = 0; i < List.Count; i++)
                {
                    grid.Rows.Insert(i + 1);
                    T row = List[i];
                    try
                    {
                        grid.Rows[grid.RowsCount - 1].Tag = new RowTag(i, row);
                    }
                    catch (Exception ex)
                    {
                        grid.Rows[grid.RowsCount - 1].Tag = new RowTag(i, null);
                        Debug.WriteLine(ex.StackTrace);
                    }
                    
                    SourceGrid.Cells.Controllers.IController CellController = newCellController == null ? null : newCellController(row, Color.LightBlue);
                    grid[grid.RowsCount - 1, 0] = newCell(row, CellController);                    
                }
            }
            grid.AutoSizeCells();
        }        


        public static void GridAddRows<T>(
            SourceGrid.Grid grid,
            List<T> List, 
            Func<T, Color, SourceGrid.Cells.Controllers.IController> newCellController = null)
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
            Func<T, Color, SourceGrid.Cells.Controllers.IController> newCellController = null)
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
                if (((RowTag)grid.Rows[j].Tag).Key == item)
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
            if (item.GetType() == typeof(IPInfo))
            {
                IPInfo ipInfo = item as IPInfo;
                grid[Index, 0].Value = ipInfo.IPAddress;
                grid[Index, 1].Value = ipInfo.RoundtripTime;
                grid[Index, 2].Value = ipInfo.HostName;
                return;
            }
            

            {
                grid[Index, 0].Value = item;
            }
        }

        public static void GridInsertRow<T>(
            SourceGrid.Grid grid,
            T item, 
            Func<T, Color, SourceGrid.Cells.Controllers.IController> newCellController = null,
            int Index = -1)
        {
            int index = Index < 0 ? grid.RowsCount : Index;
            grid.Rows.Insert(index);

            RowTag rowTag = new RowTag(item);
            grid.Rows[index].Tag = rowTag;

            if (item.GetType() == typeof(IPInfo))
            {
                IPInfo ipInfo = item as IPInfo;
                SourceGrid.Cells.Controllers.IController CellController = newCellController(item, Color.LightBlue);
                grid[index, 0] = newCell(ipInfo.IPAddress, CellController);            
                grid[index, 1] = newCell(ipInfo.RoundtripTime, CellController);
                grid[index, 2] = newCell(ipInfo.HostName, CellController);
                return;
            }

            {
                SourceGrid.Cells.Controllers.IController CellController = newCellController(item, Color.LightBlue);
                grid[index, 0] = newCell(item, CellController);
            }
            
        }
    }
}
