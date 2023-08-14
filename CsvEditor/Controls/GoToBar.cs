using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CsvEditor.Controls
{
    [TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
    [TemplatePart(Name = PART_Gripper, Type = typeof(Thumb))]
    [TemplatePart(Name = PART_InputGrid, Type = typeof(Grid))]
    [TemplatePart(Name = PART_XInput, Type = typeof(NumericTextBox))]
    [TemplatePart(Name = PART_YInput, Type = typeof(NumericTextBox))]
    public class GoToBar : Control
    {
        #region Variables
        private const string PART_Popup = "PART_Popup";
        private const string PART_RootContainer = "PART_RootContainer";
        private const string PART_Gripper = "PART_Gripper";
        private const string PART_InputGrid = "PART_InputGrid";
        private const string PART_XInput = "PART_XInput";
        private const string PART_YInput = "PART_YInput";

        public static readonly RoutedCommand GoToXyCommand = new RoutedCommand("GoToXyCommand", typeof(GoToBar));
        private readonly RoutedCommand goCommand;

        private Popup popup = null;
        private FrameworkElement rootContainer = null;
        private Thumb gripper = null;
        private Grid inputGrid = null;
        private NumericTextBox textBoxX = null;
        private NumericTextBox textBoxY = null;

        private double windowWidth = 0.0;
        private double popupWidth = 0.0;
        private double originalWidth = 0.0;
        #endregion

        #region Events
        public static readonly RoutedEvent AcceptedEvent = EventManager.RegisterRoutedEvent("GotoAccepted", RoutingStrategy.Bubble,
            typeof(RoutedGotoEventHandler), typeof(GoToBar));
        public event RoutedGotoEventHandler Accepted
        {
            add { AddHandler(AcceptedEvent, value); }
            remove { RemoveHandler(AcceptedEvent, value); }
        }

        public static readonly RoutedEvent IsOpenChangedEvent = EventManager.RegisterRoutedEvent("IsOpenChanged", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(GoToBar));
        public event RoutedEventHandler IsOpenChanged
        {
            add { AddHandler(IsOpenChangedEvent, value); }
            remove { RemoveHandler(IsOpenChangedEvent, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty IsXyProperty = DependencyProperty.Register(
            nameof(IsXy), typeof(bool), typeof(GoToBar),
            new FrameworkPropertyMetadata(false, OnIsXyChanged));

        public static readonly DependencyProperty XMaximumProperty = DependencyProperty.Register(
            nameof(XMaximum), typeof(decimal), typeof(GoToBar),
            new UIPropertyMetadata(100M, OnXMaximumChanged));

        public static readonly DependencyProperty YMaximumProperty = DependencyProperty.Register(
            nameof(YMaximum), typeof(decimal), typeof(GoToBar),
            new UIPropertyMetadata(100M, OnYMaximumChanged));

        public static readonly DependencyProperty XValueProperty = DependencyProperty.Register(
           nameof(XValue), typeof(decimal), typeof(GoToBar),
           new FrameworkPropertyMetadata(0M, OnXValueChanged));

        public static readonly DependencyProperty YValueProperty = DependencyProperty.Register(
            nameof(YValue), typeof(decimal), typeof(GoToBar),
            new FrameworkPropertyMetadata(0M, OnYValueChanged));

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            nameof(IsOpen), typeof(bool), typeof(GoToBar),
            new FrameworkPropertyMetadata(false, OnIsOpenChanged));
        #endregion

        #region Properties
        public ICommand GoCommand
        {
            get => goCommand;
        }

        public bool IsXy
        {
            get => (bool)GetValue(IsXyProperty);
            set { SetValue(IsXyProperty, value); }
        }

        public decimal XMaximum
        {
            get => (decimal)GetValue(XMaximumProperty);
            set { SetValue(XMaximumProperty, value); }
        }

        public decimal YMaximum
        {
            get => (decimal)GetValue(YMaximumProperty);
            set { SetValue(YMaximumProperty, value); }
        }

        public decimal XValue
        {
            get => (decimal)GetValue(XValueProperty);
            set { SetValue(XValueProperty, value); }
        }

        public decimal YValue
        {
            get => (decimal)GetValue(YValueProperty);
            set { SetValue(YValueProperty, value); }
        }

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set { SetValue(IsOpenProperty, value); }
        }
        #endregion

        #region Constructors
        static GoToBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GoToBar), new FrameworkPropertyMetadata(typeof(GoToBar)));

            CommandManager.RegisterClassInputBinding(typeof(GoToBar), new InputBinding(GoToXyCommand,
                new KeyGesture(Key.X, ModifierKeys.Alt)));
            CommandManager.RegisterClassCommandBinding(typeof(GoToBar), new CommandBinding(GoToXyCommand,
                new ExecutedRoutedEventHandler(OnExecuteGoToXy)));
        }

        public GoToBar()
        {
            goCommand = new RoutedCommand("GoToGo", typeof(GoToBar));
            goCommand.InputGestures.Add(new KeyGesture(Key.Enter));

            CommandBindings.Add(new CommandBinding(goCommand, Go_Executed, Go_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Stop, Close_Executed, Close_CanExecute));

            Focusable = true;
        }
        #endregion

        #region Methods
        private static void OnIsXyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is GoToBar d)
            {
                d.EnableXyMode((bool)e.NewValue);
                d.OnIsXyChanged((bool)e.OldValue, (bool)e.NewValue);
                d.OnGotFocus(null);
            }
        }

        protected virtual void OnIsXyChanged(bool oldValue, bool newValue)
        { }

        private static void OnYMaximumChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is GoToBar d)
            {
                d.OnYMaximumChanged((decimal)e.OldValue, (decimal)e.NewValue);
            }
        }

        protected virtual void OnYMaximumChanged(decimal oldValue, decimal newValue)
        { 
        }

        private static void OnXMaximumChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is GoToBar d)
            {
                d.OnXMaximumChanged((decimal)e.OldValue, (decimal)e.NewValue);
            }
        }

        protected virtual void OnXMaximumChanged(decimal oldValue, decimal newValue)
        {
        }

        private static void OnYValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is GoToBar d)
            {
                d.OnYValueChanged((decimal)e.OldValue, (decimal)e.NewValue);
            }
        }

        protected virtual void OnYValueChanged(decimal oldValue, decimal newValue)
        { 
        }

        private static void OnXValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is GoToBar d)
            {
                d.OnXValueChanged((decimal)e.OldValue, (decimal)e.NewValue);
            }
        }

        protected virtual void OnXValueChanged(decimal oldValue, decimal newValue)
        {
        }

        private static void OnIsOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is GoToBar d)
            {
                d.OnIsOpenChanged((bool)e.OldValue, (bool)e.NewValue);
            }
        }

        protected virtual void OnIsOpenChanged(bool oldValue, bool newValue)
        { 
        }

        private void EnableXyMode(bool enabled)
        {
            if (inputGrid == null) return;
            if (inputGrid.ColumnDefinitions.Count != 4) return;

            GridLength lengthLabel = enabled ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            GridLength lengthInput = enabled ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

            inputGrid.ColumnDefinitions[0].Width = lengthLabel;
            inputGrid.ColumnDefinitions[1].Width = lengthInput;
            inputGrid.ColumnDefinitions[2].Width = lengthLabel;
        }

        private static Window GetParentWindow(DependencyObject o)
        {
            var parent = VisualTreeHelper.GetParent(o);
            if (parent != null)
                return GetParentWindow(parent);
            var fe = o as FrameworkElement;
            if (fe is Window)
                return fe as Window;
            if (fe != null && fe.Parent != null)
                return GetParentWindow(fe.Parent);
            return null;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var window = GetParentWindow(this);
            if (window != null)
            {
                window.LocationChanged += Window_LocationChanged;
                window.SizeChanged += Window_SizeChanged;
                window.Activated += Window_Activated;
                window.Deactivated += Window_Deactivated;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            popup = GetTemplateChild(PART_Popup) as Popup;
            rootContainer = GetTemplateChild(PART_RootContainer) as FrameworkElement;
            inputGrid = GetTemplateChild(PART_InputGrid) as Grid;
            textBoxY = GetTemplateChild(PART_YInput) as NumericTextBox;
            textBoxX = GetTemplateChild(PART_XInput) as NumericTextBox;

            if (popup != null)
            {
                popup.Placement = PlacementMode.Custom;
                popup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(Popup_PlacementCallback);
                popup.Opened += Popup_Opened;
                popup.Closed += Popup_Closed;
            }

            HookupGripperEvents();
            EnableXyMode(IsXy);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (e != null) base.OnGotFocus(e);

            if (IsOpen && textBoxY != null)
            {
                Keyboard.Focus(textBoxY);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            windowWidth = e.NewSize.Width;
            RepositionPopup();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            RepositionPopup();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (popup != null && popup.Child != null && popup.IsOpen)
            {
                popup.Child.Visibility = Visibility.Hidden;
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (popup != null && popup.Child != null && popup.IsOpen)
            {
                popup.Child.Visibility = Visibility.Visible;
                this.Focus();
            }
        }

        private CustomPopupPlacement[] Popup_PlacementCallback(Size popupSize, Size targetSize, Point offset)
        {
            if (popupWidth == 0.0)
            {
                popupWidth = popupSize.Width;
            }

            var offsetX = popupWidth - popupSize.Width;

            CustomPopupPlacement placement = new CustomPopupPlacement(new Point(offsetX, -25), PopupPrimaryAxis.None);
            return new CustomPopupPlacement[] { placement };
        }

        private void RepositionPopup()
        {
            if (popup != null && popup.IsOpen)
            {
                var offset = popup.HorizontalOffset;
                popup.HorizontalOffset = offset + 1;
                popup.HorizontalOffset = offset;
            }
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            if (textBoxX != null)
            {
                Keyboard.Focus(textBoxY);
            }
            RaiseIsOpenChangedEvent();
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            RaiseIsOpenChangedEvent();
        }

        #region Gripper Methods
        private void HookupGripperEvents()
        {
            UnhookGripperEvents();

            gripper = GetTemplateChild(PART_Gripper) as Thumb;

            if (gripper != null)
            {
                gripper.DragStarted += new DragStartedEventHandler(OnGripperDragStarted);
                gripper.DragDelta += new DragDeltaEventHandler(OnGripperDragDelta);
                gripper.DragCompleted += new DragCompletedEventHandler(OnGripperDragCompleted);
            }
        }

        private void UnhookGripperEvents()
        {
            if (gripper != null)
            {
                gripper.DragStarted -= new DragStartedEventHandler(OnGripperDragStarted);
                gripper.DragDelta -= new DragDeltaEventHandler(OnGripperDragDelta);
                gripper.DragCompleted -= new DragCompletedEventHandler(OnGripperDragCompleted);
                gripper = null;
            }
        }

        private void OnGripperDragStarted(object sender, DragStartedEventArgs e)
        {
            if (rootContainer != null)
            {
                originalWidth = rootContainer.ActualWidth;
                e.Handled = true;
            }
        }

        private void OnGripperDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (rootContainer != null)
            {
                double width = rootContainer.ActualWidth - e.HorizontalChange;
                width = Math.Max(width, MinWidth);
                var maxWidth = Math.Min(windowWidth - 300, 400);
                width = Math.Min(width, maxWidth);

                UpdateWidth(width);
                e.Handled = true;
            }
        }

        private void OnGripperDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (e.Canceled)
            {
                UpdateWidth(originalWidth);
            }
            e.Handled = true;
        }

        private void UpdateWidth(double width)
        {
            if (popup != null && rootContainer != null && width > 0)
            {
                rootContainer.Width = width;
            }
        }
        #endregion

        #region Event Methods
        private static void OnExecuteGoToXy(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is GoToBar c)
            {
                c.IsXy = !c.IsXy;
            }
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsOpen;
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IsOpen = false;
            OnCloseExecuted();
        }

        protected virtual void OnCloseExecuted()
        {   
        }

        private void Go_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsOpen && textBoxX != null && !string.IsNullOrEmpty(textBoxX.Text);
        }

        private void Go_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (textBoxY == null) return;
            if (string.IsNullOrEmpty(textBoxY.Text)) return;

            OnGoExecuted();
            RaiseAcceptedEvent();

            IsOpen = false;
        }

        protected virtual void OnGoExecuted()
        {
        }

        protected virtual bool RaiseIsOpenChangedEvent()
        {
            var args = new RoutedEventArgs();
            args.RoutedEvent = IsOpenChangedEvent;
            RaiseEvent(args);
            return args.Handled;
        }

        protected virtual bool RaiseAcceptedEvent()
        {
            if (textBoxX == null) return false;
            if (string.IsNullOrEmpty(textBoxY.Text)) return false;

            var x = textBoxX != null && !string.IsNullOrEmpty(textBoxX.Text) ? textBoxX.Value : 0;
            var y = textBoxY.Value;

            RoutedGotoEventArgs args = new RoutedGotoEventArgs(x, y);
            args.RoutedEvent = AcceptedEvent;
            RaiseEvent(args);

            return args.Handled;
        }
        #endregion

        public void Show()
        {
            IsOpen = true;
            Focus();
        }

        public void Show(int maxY, int maxX)
        {
            YMaximum = maxY;
            XMaximum = maxX;
            Show();
        }

        public void Hide()
        {
            IsOpen = false;
        }
        #endregion
    }

    #region RoutedGotoEvent
    public delegate void RoutedGotoEventHandler(object sender, RoutedGotoEventArgs e);
    public class RoutedGotoEventArgs : RoutedEventArgs
    {
        public decimal X { get; private set; }
        public decimal Y { get; private set; }

        public RoutedGotoEventArgs(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }
    }
    #endregion
}
