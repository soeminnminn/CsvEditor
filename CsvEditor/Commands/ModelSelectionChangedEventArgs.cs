using System;

namespace CsvEditor.Commands
{
    public class ModelSelectionChangedEventArgs : EventArgs
    {
        public int RowIndex { get; set; }

        public int ColumnIndex { get; set; }

        public string CellValue { get; set; }
    }

    public delegate void ModelSelectionChangedEventHandler(object sender, ModelSelectionChangedEventArgs args);
}
