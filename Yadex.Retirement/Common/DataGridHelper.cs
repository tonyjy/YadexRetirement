using System.Windows.Media;
using System.Data;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Yadex.Retirement.Common;

/// <summary>
/// Class to help to retrieve the Data Grid
/// </summary>
public static class DataGridHelper
{

    public static string ConvertToCsv(DataGrid dataGrid)
    {
        // Append columns
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(',', dataGrid.Columns.Select(x => x.Header.ToString())));

        // Append rows
        var rowsCount = dataGrid.Items.Count;
        var columnsCount = dataGrid.Columns.Count;

        for (var row = 0; row < rowsCount; row++)
        {
            var columns = new string[columnsCount];
            for (var col = 0; col < columnsCount; col++)
            {
                var dgc = GetCell(dataGrid, row, col);
                columns[col] = ((TextBlock)dgc.Content).Text;
            }

            sb.AppendLine(string.Join(',', columns));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Retrieve the cell contains
    /// </summary>
    /// <param name="dg">DataGrid</param>
    /// <param name="row">row</param>
    /// <param name="column">column</param>
    /// <returns>DataGrid Cell content</returns>
    public static DataGridCell GetCell(DataGrid dg, int row, int column)
    {
        DataGridRow rowContainer = GetRow(dg, row);

        if (rowContainer != null)
        {
            DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

            // try to get the cell but it may possibly be virtualized
            DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
            if (cell == null)
            {
                // now try to bring into view and retrieve the cell
                dg.ScrollIntoView(rowContainer, dg.Columns[column]);
                cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
            }
            return cell;
        }
        return null;
    }
    /// <summary>
    /// Get row content
    /// </summary>
    /// <param name="dg">Datagrid</param>
    /// <param name="index">Index</param>
    /// <returns>DataGridRow</returns>
    public static DataGridRow GetRow(DataGrid dg, int index)
    {
        DataGridRow row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(index);
        if (row == null)
        {
            // may be virtualized, bring into view and try again
            dg.ScrollIntoView(dg.Items[index]);
            row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(index);
        }
        return row;
    }
    static T GetVisualChild<T>(Visual parent) where T : Visual
    {
        T child = default(T);
        int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < numVisuals; i++)
        {
            Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
            child = v as T;
            if (child == null)
            {
                child = GetVisualChild<T>(v);
            }
            if (child != null)
            {
                break;
            }
        }
        return child;
    }
}
