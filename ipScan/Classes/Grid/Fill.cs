using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using ipScan.Classes.IP;

namespace ipScan.Classes.Grid
{
    static class Fill
    {
        public static SourceGrid.Cells.Cell newCell(object Value, SourceGrid.Cells.Controllers.IController Controller = null)
        {
            SourceGrid.Cells.Cell cell = new SourceGrid.Cells.Cell(Value);
            if (Controller != null)
            {
                cell.AddController(Controller);
            }
            return cell;
        }

        public static void GridFill(SourceGrid.Grid grid, ListIPInfo IPList, 
            Func<IPInfo, Color, SourceGrid.Cells.Controllers.IController> newCellController = null, List<string> Fields = null, Dictionary<string, object> filter = null, string filtertype = "", bool casesensitive = false)
        {
            grid.Columns.Clear();
            grid.Rows.Clear();

            List<string> fields = Fields == null ?
                new List<string>() { "IP Address", "TTL", "Host Name" } :
                Fields;

            //Columns filling
            grid.ColumnsCount = fields.Count;
            grid.FixedRows = 1;
            grid.Rows.Insert(0);
            SourceGrid.Cells.ColumnHeader columnHeader = new SourceGrid.Cells.ColumnHeader(fields[0]);
            columnHeader.SortComparer = new IPAddressComparer();
            grid[0, 0] = columnHeader;
            for (int i = 1; i < (fields.Count); i++)
            {
                grid[0, i] = new SourceGrid.Cells.ColumnHeader(fields[i]);
            }

            //Data filling
            if (IPList != null)
            {
                for (int i = 0; i < IPList.Count; i++)
                {
                    grid.Rows.Insert(i + 1);

                    IPInfo ipInfo = IPList[i];
                    try
                    {
                        grid.Rows[grid.RowsCount - 1].Tag = new RowTag(i, ipInfo);
                    }
                    catch (Exception ex)
                    {
                        grid.Rows[grid.RowsCount - 1].Tag = new RowTag(i, null);
                        Debug.WriteLine(ex.StackTrace);
                    }

                    SourceGrid.Cells.Controllers.IController CellController = newCellController(ipInfo, Color.LightBlue);

                    grid[grid.RowsCount - 1, 0] = newCell(ipInfo.IPAddress, CellController);
                    grid[grid.RowsCount - 1, 1] = newCell(ipInfo.RoundtripTime, CellController);
                    grid[grid.RowsCount - 1, 2] = newCell(ipInfo.HostName, CellController);
                }
            }
            grid.AutoSizeCells();
        }

        public static void GridFill(SourceGrid.Grid grid, List<object> List,
            Func<object, Color, SourceGrid.Cells.Controllers.IController> newCellController = null, List<string> Fields = null, Dictionary<string, object> filter = null, string filtertype = "", bool casesensitive = false)
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
                    object row = List[i];
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
    }
}
