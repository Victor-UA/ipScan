using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using ipScan.Base.IP;
using ipScan.Classes.Grid;

namespace ipScan.Base.Grid
{
    public abstract class Fill
    {
        protected SourceGrid.Cells.Cell newCell<T, TSub>(
            T Item,
            TSub subItem,                 
            SourceGrid.Cells.Controllers.IController Controller = null)
        {
            SourceGrid.Cells.Cell cell = new SourceGrid.Cells.Cell(subItem)
            {
                Tag = Item                
            };
            if (Controller != null)
            {
                cell.AddController(Controller);
            }
            return cell;
        }

        public void GridFill<T>(
            SourceGrid.Grid grid,
            List<T> List,
            Func<T, Color, GridCellController> newCellController = null,
            List<KeyValuePair<String, IComparer>> Fields = null)
        {
            grid.Columns.Clear();
            grid.Rows.Clear();

            List<KeyValuePair<String, IComparer>> fields = Fields ?? new List<KeyValuePair<String, IComparer>>() { new KeyValuePair<string, IComparer>("Undefined", null) };

            //Columns filling
            grid.ColumnsCount = fields.Count;
            grid.FixedRows = 1;
            grid.Rows.Insert(0);
            for (int i = 0; i < (fields.Count); i++)
            {
                SourceGrid.Cells.ColumnHeader ColumnHeader = new SourceGrid.Cells.ColumnHeader(fields[i].Key)
                {
                    SortComparer = fields[i].Value
                };
                grid[0, i] = ColumnHeader;
            }

            //Data filling
            GridUpdateOrInsertRows(grid, List, newCellController);
        }

        public void GridFill<T>(
            SourceGrid.Grid grid, 
            List<T> List,
            Func<T, Color, GridCellController> newCellController = null,                         
            List<string> Fields = null)
        {
            List<KeyValuePair<String, IComparer>> fields = new List<KeyValuePair<String, IComparer>>();
            if (Fields == null)
            {
                fields.Add(new KeyValuePair<string, IComparer>("Undefined", null));
            }
            else
            {
                for (int i = 0; i < Fields.Count; i++)
                {
                    fields.Add(new KeyValuePair<string, IComparer>(Fields[i], null));
                }
            }
            GridFill(grid, List, newCellController, fields);
        }        


        public void GridAddRows<T>(
            SourceGrid.Grid grid,
            List<T> List, 
            Func<T, Color, GridCellController> newCellController = null)
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

        public void GridUpdateOrInsertRows<T>(
            SourceGrid.Grid grid,
            List<T> List,
            Func<T, Color, GridCellController> newCellController = null)
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

        public bool GridSeekRowAndUpdate(SourceGrid.Grid grid, object item)
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

        public virtual void GridUpdateRow(SourceGrid.Grid grid, object item, int Index)
        {
            grid.Rows[Index].Tag = item;
            Type itemType = item.GetType();                      

            {                
                grid[Index, 0].Value = item;
            }
        }

        public virtual void GridInsertRow<T>(
            SourceGrid.Grid grid,
            T item, 
            Func<T, Color, GridCellController> newCellController = null,
            int Index = -1)
        {
            int index = Index < 0 ? grid.RowsCount : Index;
            grid.Rows.Insert(index);            
            grid.Rows[index].Tag = item;

            Type itemType = item.GetType();            

            {
                SourceGrid.Cells.Controllers.IController CellController = newCellController(item, Color.LightBlue);
                grid[index, 0] = newCell(item, item, CellController);
            }
            
        }
    }
}