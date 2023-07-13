using System;
using System.Windows.Input;
using System.Windows;

namespace CsvEditor.Commands
{
    public static class RoutedCommands
    {
        public static ICommand Refresh = CreateRoutedCommand("Refresh", typeof(Window), new KeyGesture(Key.F5));
        public static ICommand Settings = CreateRoutedCommand("Settings", typeof(Window), new KeyGesture(Key.F4));
        public static ICommand GoTo = CreateRoutedCommand("GoTo", typeof(Window), new KeyGesture(Key.G, ModifierKeys.Control));

        public static ICommand InsertColumnBefore = CreateRoutedCommand("InsertColumnBefore", typeof(Window));
        public static ICommand InsertColumnAfter = CreateRoutedCommand("InsertColumnAfter", typeof(Window));
        public static ICommand RemoveColumn = CreateRoutedCommand("RemoveColumn", typeof(Window));

        public static ICommand InsertRowAbove = CreateRoutedCommand("InsertRowAbove", typeof(Window), new KeyGesture(Key.Insert, ModifierKeys.Control | ModifierKeys.Shift));
        public static ICommand InsertRowBelow = CreateRoutedCommand("InsertRowBelow", typeof(Window), new KeyGesture(Key.Insert, ModifierKeys.Control));
        public static ICommand RemoveRow = CreateRoutedCommand("RemoveRow", typeof(Window), new KeyGesture(Key.Delete, ModifierKeys.Control));

        public static ICommand SortAscending = CreateRoutedCommand("SortAscending", typeof(Window));
        public static ICommand SortDescending = CreateRoutedCommand("SortDescending", typeof(Window));

        public static ICommand CreateRoutedCommand(string name, Type ownerType, params InputGesture[] gestures)
        {
            var cmd = new RoutedCommand(name, ownerType);
            if (gestures != null && gestures.Length > 0)
                cmd.InputGestures.AddRange(gestures);
            return cmd;
        }
    }
}
