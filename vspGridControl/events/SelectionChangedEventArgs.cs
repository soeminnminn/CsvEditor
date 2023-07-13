using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class SelectionChangedEventArgs : EventArgs
    {
        private BlockOfCellsCollection m_SelectedBlocks;

        protected SelectionChangedEventArgs()
        {
        }

        public SelectionChangedEventArgs(BlockOfCellsCollection blocks)
        {
            this.m_SelectedBlocks = blocks;
        }

        public BlockOfCellsCollection SelectedBlocks
        {
            get
            {
                return this.m_SelectedBlocks;
            }
        }
    }

    public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs args);
}

