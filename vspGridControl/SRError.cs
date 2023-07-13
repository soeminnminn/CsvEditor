using System;
using System.Globalization;
using System.Resources;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    internal class SRError
    {
        public static string ColumnIsNotCheckBox(int num)
        {
            return Keys.GetString("ColumnIsNotCheckBox", new object[] { num });
        }

        public static string ColumnWidthShouldBeLessThanMax(int max)
        {
            return Keys.GetString("ColumnWidthShouldBeLessThanMax", new object[] { max });
        }

        public static string NonExistingGridSelectionBlock(string x, string y, string right, string bottom)
        {
            return Keys.GetString("NonExistingGridSelectionBlock", new object[] { x, y, right, bottom });
        }

        public static string ShouldSetHeaderStateForCheckBox(int num)
        {
            return Keys.GetString("ShouldSetHeaderStateForCheckBox", new object[] { num });
        }

        public static string ShouldSetHeaderStateForRegualrCol(int num)
        {
            return Keys.GetString("ShouldSetHeaderStateForRegualrCol", new object[] { num });
        }

        public static string AutoScrollMoreThanZero
        {
            get
            {
                return Keys.GetString("AutoScrollMoreThanZero");
            }
        }

        public static string CannotPrintEmptyGrid
        {
            get
            {
                return Keys.GetString("CannotPrintEmptyGrid");
            }
        }

        public static string CannotSetMergeItemState
        {
            get
            {
                return Keys.GetString("CannotSetMergeItemState");
            }
        }

        public static string CellHeightShouldBeGreaterThanZero
        {
            get
            {
                return Keys.GetString("CellHeightShouldBeGreaterThanZero");
            }
        }

        public static string ColumnIndexShouldBeInRange
        {
            get
            {
                return Keys.GetString("ColumnIndexShouldBeInRange");
            }
        }

        public static string ColumnWidthShouldBeGreaterThanZero
        {
            get
            {
                return Keys.GetString("ColumnWidthShouldBeGreaterThanZero");
            }
        }

        public static CultureInfo Culture
        {
            get
            {
                return Keys.Culture;
            }
            set
            {
                Keys.Culture = value;
            }
        }

        public static string CurControlIsNotIGridEmbedded
        {
            get
            {
                return Keys.GetString("CurControlIsNotIGridEmbedded");
            }
        }

        public static string DeriveToImplementCustomColumn
        {
            get
            {
                return Keys.GetString("DeriveToImplementCustomColumn");
            }
        }

        public static string FirstScrollableColumnShouldBeValid
        {
            get
            {
                return Keys.GetString("FirstScrollableColumnShouldBeValid");
            }
        }

        public static string FirstScrollableRowShouldBeValid
        {
            get
            {
                return Keys.GetString("FirstScrollableRowShouldBeValid");
            }
        }

        public static string FirstScrollableWillBeBad
        {
            get
            {
                return Keys.GetString("FirstScrollableWillBeBad");
            }
        }

        public static string GridColumnInfoCollectionIsReadOnly
        {
            get
            {
                return Keys.GetString("GridColumnInfoCollectionIsReadOnly");
            }
        }

        public static string GridColumnNumberShouldBeAboveZero
        {
            get
            {
                return Keys.GetString("GridColumnNumberShouldBeAboveZero");
            }
        }

        public static string HorizScrollUnitShouldBeGreaterThanZero
        {
            get
            {
                return Keys.GetString("HorizScrollUnitShouldBeGreaterThanZero");
            }
        }

        public static string InvalidCellType
        {
            get
            {
                return Keys.GetString("InvalidCellType");
            }
        }

        public static string InvalidColIndexForMergedResizeProp
        {
            get
            {
                return Keys.GetString("InvalidColIndexForMergedResizeProp");
            }
        }

        public static string InvalidCurrentSelBlockForClicpboard
        {
            get
            {
                return Keys.GetString("InvalidCurrentSelBlockForClicpboard");
            }
        }

        public static string InvalidCustomColType
        {
            get
            {
                return Keys.GetString("InvalidCustomColType");
            }
        }

        public static string InvalidGridStateForClipboard
        {
            get
            {
                return Keys.GetString("InvalidGridStateForClipboard");
            }
        }

        public static string InvalidMergedHeaderResizeProportion
        {
            get
            {
                return Keys.GetString("InvalidMergedHeaderResizeProportion");
            }
        }

        public static string InvalidSelIndexInEmbeddedCombo
        {
            get
            {
                return Keys.GetString("InvalidSelIndexInEmbeddedCombo");
            }
        }

        public static string InvalidSelStringInEmbeddedCombo
        {
            get
            {
                return Keys.GetString("InvalidSelStringInEmbeddedCombo");
            }
        }

        public static string InvalidThreadForMethod
        {
            get
            {
                return Keys.GetString("InvalidThreadForMethod");
            }
        }

        public static string LastNonScrollCannotBeMerged
        {
            get
            {
                return Keys.GetString("LastNonScrollCannotBeMerged");
            }
        }

        public static string NoIGridEmbeddedControl
        {
            get
            {
                return Keys.GetString("NoIGridEmbeddedControl");
            }
        }

        public static string NoIGridEmbeddedControlManagement
        {
            get
            {
                return Keys.GetString("NoIGridEmbeddedControlManagement");
            }
        }

        public static string NoMultiBlockSelInSingleSelMode
        {
            get
            {
                return Keys.GetString("NoMultiBlockSelInSingleSelMode");
            }
        }

        public static string NumOfRowsShouldBeGreaterThanZero
        {
            get
            {
                return Keys.GetString("NumOfRowsShouldBeGreaterThanZero");
            }
        }

        public static string OnlySolidBrush
        {
            get
            {
                return Keys.GetString("OnlySolidBrush");
            }
        }

        public static string RowIndexShouldBeInRange
        {
            get
            {
                return Keys.GetString("RowIndexShouldBeInRange");
            }
        }

        public static string ScrollPosShouldBeMoreOrEqualZero
        {
            get
            {
                return Keys.GetString("ScrollPosShouldBeMoreOrEqualZero");
            }
        }

        public static string ShouldBeNoDataForMergedColumHeader
        {
            get
            {
                return Keys.GetString("ShouldBeNoDataForMergedColumHeader");
            }
        }

        internal class Keys
        {
            public const string AutoScrollMoreThanZero = "AutoScrollMoreThanZero";
            public const string CannotPrintEmptyGrid = "CannotPrintEmptyGrid";
            public const string CannotSetMergeItemState = "CannotSetMergeItemState";
            public const string CellHeightShouldBeGreaterThanZero = "CellHeightShouldBeGreaterThanZero";
            public const string ColumnIndexShouldBeInRange = "ColumnIndexShouldBeInRange";
            public const string ColumnIsNotCheckBox = "ColumnIsNotCheckBox";
            public const string ColumnWidthShouldBeGreaterThanZero = "ColumnWidthShouldBeGreaterThanZero";
            public const string ColumnWidthShouldBeLessThanMax = "ColumnWidthShouldBeLessThanMax";
            public const string CurControlIsNotIGridEmbedded = "CurControlIsNotIGridEmbedded";
            public const string DeriveToImplementCustomColumn = "DeriveToImplementCustomColumn";
            public const string FirstScrollableColumnShouldBeValid = "FirstScrollableColumnShouldBeValid";
            public const string FirstScrollableRowShouldBeValid = "FirstScrollableRowShouldBeValid";
            public const string FirstScrollableWillBeBad = "FirstScrollableWillBeBad";
            public const string GridColumnInfoCollectionIsReadOnly = "GridColumnInfoCollectionIsReadOnly";
            public const string GridColumnNumberShouldBeAboveZero = "GridColumnNumberShouldBeAboveZero";
            public const string HorizScrollUnitShouldBeGreaterThanZero = "HorizScrollUnitShouldBeGreaterThanZero";
            public const string InvalidCellType = "InvalidCellType";
            public const string InvalidColIndexForMergedResizeProp = "InvalidColIndexForMergedResizeProp";
            public const string InvalidCurrentSelBlockForClicpboard = "InvalidCurrentSelBlockForClicpboard";
            public const string InvalidCustomColType = "InvalidCustomColType";
            public const string InvalidGridStateForClipboard = "InvalidGridStateForClipboard";
            public const string InvalidMergedHeaderResizeProportion = "InvalidMergedHeaderResizeProportion";
            public const string InvalidSelIndexInEmbeddedCombo = "InvalidSelIndexInEmbeddedCombo";
            public const string InvalidSelStringInEmbeddedCombo = "InvalidSelStringInEmbeddedCombo";
            public const string InvalidThreadForMethod = "InvalidThreadForMethod";
            public const string LastNonScrollCannotBeMerged = "LastNonScrollCannotBeMerged";
            public const string NoIGridEmbeddedControl = "NoIGridEmbeddedControl";
            public const string NoIGridEmbeddedControlManagement = "NoIGridEmbeddedControlManagement";
            public const string NoMultiBlockSelInSingleSelMode = "NoMultiBlockSelInSingleSelMode";
            public const string NonExistingGridSelectionBlock = "NonExistingGridSelectionBlock";
            public const string NumOfRowsShouldBeGreaterThanZero = "NumOfRowsShouldBeGreaterThanZero";
            public const string OnlySolidBrush = "OnlySolidBrush";
            public const string RowIndexShouldBeInRange = "RowIndexShouldBeInRange";
            public const string ScrollPosShouldBeMoreOrEqualZero = "ScrollPosShouldBeMoreOrEqualZero";
            public const string ShouldBeNoDataForMergedColumHeader = "ShouldBeNoDataForMergedColumHeader";
            public const string ShouldSetHeaderStateForCheckBox = "ShouldSetHeaderStateForCheckBox";
            public const string ShouldSetHeaderStateForRegualrCol = "ShouldSetHeaderStateForRegualrCol";

            private static CultureInfo culture = null;
            private static ResourceManager resourceManager = new ResourceManager("Microsoft.SqlServer.Management.UI.Grid.SRError", typeof(SRError).Module.Assembly);
            
            public static string GetString(string key)
            {
                return resourceManager.GetString(key, culture);
            }

            public static string GetString(string key, params object[] args)
            {
                return string.Format(resourceManager.GetString(key, culture), args);
            }

            public static CultureInfo Culture
            {
                get
                {
                    return culture;
                }
                set
                {
                    culture = value;
                }
            }
        }
    }
}

