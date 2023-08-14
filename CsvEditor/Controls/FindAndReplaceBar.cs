using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CsvEditor.Controls
{
    [TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
    [TemplatePart(Name = PART_Gripper, Type = typeof(Thumb))]
    [TemplatePart(Name = PART_FindInput, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_ReplaceBarRow, Type = typeof(RowDefinition))]
    [TemplatePart(Name = PART_ReplaceInput, Type = typeof(TextBox))]
    public class FindAndReplaceBar : Control
    {
        #region Variables
        private const string PART_Popup = "PART_Popup";
        private const string PART_RootContainer = "PART_RootContainer";
        private const string PART_Gripper = "PART_Gripper";
        private const string PART_FindInput = "PART_FindInput";
        private const string PART_ReplaceInput = "PART_ReplaceInput";
        private const string PART_ReplaceBarRow = "PART_ReplaceBarRow";

        private Popup popup = null;
        private FrameworkElement rootContainer = null;
        private Thumb gripper = null;
        private TextBox textBoxFind = null;
        private TextBox textBoxReplace = null;
        private RowDefinition replaceBarRow = null;

        private double windowWidth = 0.0;
        private double popupWidth = 0.0;
        private double originalWidth = 0.0;

        public static readonly RoutedCommand FindInSelectionCommand = new RoutedCommand("FindInSelectionCommand", typeof(FindAndReplaceBar));
        public static readonly RoutedCommand FindMatchCaseCommand = new RoutedCommand("FindMatchCaseCommand", typeof(FindAndReplaceBar));
        public static readonly RoutedCommand FindMatchWholeWordCommand = new RoutedCommand("FindMatchWholeWordCommand", typeof(FindAndReplaceBar));
        public static readonly RoutedCommand FindUseRegexCommand = new RoutedCommand("FindUseRegexCommand", typeof(FindAndReplaceBar));
        public static readonly RoutedCommand FindPreserveCaseCommand = new RoutedCommand("FindUseRegexCommand", typeof(FindAndReplaceBar));

        private readonly RoutedCommand findPreviousCommand;
        private readonly RoutedCommand findNextCommand;
        private readonly RoutedCommand findReplaceCommand;
        private readonly RoutedCommand findReplaceAllCommand;
        #endregion

        #region Events
        public static readonly RoutedEvent OptionsChangedEvent = EventManager.RegisterRoutedEvent("OptionsChanged", RoutingStrategy.Bubble,
            typeof(RoutedOptionsChangedEventHandler), typeof(FindAndReplaceBar));
        public event RoutedOptionsChangedEventHandler OptionsChanged
        {
            add { AddHandler(OptionsChangedEvent, value); }
            remove { RemoveHandler(OptionsChangedEvent, value); }
        }

        public static readonly RoutedEvent FindTextChangedEvent = EventManager.RegisterRoutedEvent("FindTextChanged", RoutingStrategy.Bubble,
            typeof(RoutedFindEventHandler), typeof(FindAndReplaceBar));
        public event RoutedFindEventHandler FindTextChanged
        {
            add { AddHandler(FindTextChangedEvent, value); }
            remove { RemoveHandler(FindTextChangedEvent, value); }
        }

        public static readonly RoutedEvent FindAcceptedEvent = EventManager.RegisterRoutedEvent("FindAccepted", RoutingStrategy.Bubble,
            typeof(RoutedFindEventHandler), typeof(FindAndReplaceBar));
        public event RoutedFindEventHandler FindAccepted
        {
            add { AddHandler(FindAcceptedEvent, value); }
            remove { RemoveHandler(FindAcceptedEvent, value); }
        }

        public static readonly RoutedEvent ReplaceTextChangedEvent = EventManager.RegisterRoutedEvent("ReplaceTextChanged", RoutingStrategy.Bubble,
            typeof(RoutedReplaceEventHandler), typeof(FindAndReplaceBar));
        public event RoutedReplaceEventHandler ReplaceTextChanged
        {
            add { AddHandler(ReplaceTextChangedEvent, value); }
            remove { RemoveHandler(ReplaceTextChangedEvent, value); }
        }

        public static readonly RoutedEvent ReplaceAcceptedEvent = EventManager.RegisterRoutedEvent("ReplaceAccepted", RoutingStrategy.Bubble,
            typeof(RoutedReplaceEventHandler), typeof(FindAndReplaceBar));
        public event RoutedReplaceEventHandler ReplaceAccepted
        {
            add { AddHandler(ReplaceAcceptedEvent, value); }
            remove { RemoveHandler(ReplaceAcceptedEvent, value); }
        }

        public static readonly RoutedEvent IsOpenChangedEvent = EventManager.RegisterRoutedEvent("IsOpenChanged", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(FindAndReplaceBar));
        public event RoutedEventHandler IsOpenChanged
        {
            add { AddHandler(IsOpenChangedEvent, value); }
            remove { RemoveHandler(IsOpenChangedEvent, value); }
        }

        public static readonly RoutedEvent IsReplaceChangedEvent = EventManager.RegisterRoutedEvent("IsReplaceChanged", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(FindAndReplaceBar));
        public event RoutedEventHandler IsReplaceChanged
        {
            add { AddHandler(IsReplaceChangedEvent, value); }
            remove { RemoveHandler(IsReplaceChangedEvent, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            nameof(IsOpen), typeof(bool), typeof(FindAndReplaceBar),
            new FrameworkPropertyMetadata(false, OnIsOpenChanged));

        public static readonly DependencyProperty IsReplaceProperty = DependencyProperty.Register(
            nameof(IsReplace), typeof(bool), typeof(FindAndReplaceBar),
            new FrameworkPropertyMetadata(true, OnIsReplaceChanged));

        public static readonly DependencyProperty HasSelectionProperty = DependencyProperty.Register(
            nameof(HasSelection), typeof(bool), typeof(FindAndReplaceBar),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty InSelectionProperty = DependencyProperty.Register(
            nameof(InSelection), typeof(bool), typeof(FindAndReplaceBar),
            new FrameworkPropertyMetadata(false, OnInSelectionChanged));

        public static readonly DependencyProperty MatchCaseProperty = DependencyProperty.Register(
            nameof(MatchCase), typeof(bool), typeof(FindAndReplaceBar),
            new FrameworkPropertyMetadata(false, OnMatchCaseChanged));

        public static readonly DependencyProperty MatchWholeWordProperty = DependencyProperty.Register(
            nameof(MatchWholeWord), typeof(bool), typeof(FindAndReplaceBar),
            new FrameworkPropertyMetadata(false, OnMatchWholeWordChanged));

        public static readonly DependencyProperty UseRegexProperty = DependencyProperty.Register(
            nameof(UseRegex), typeof(bool), typeof(FindAndReplaceBar),
            new FrameworkPropertyMetadata(false, OnUseRegexChanged));

        public static readonly DependencyProperty PreserveCaseProperty = DependencyProperty.Register(
            nameof(PreserveCase), typeof(bool), typeof(FindAndReplaceBar),
            new FrameworkPropertyMetadata(false, OnPreserveCaseChanged));

        public static readonly DependencyProperty FindTextProperty = DependencyProperty.Register(
            nameof(FindText), typeof(string), typeof(FindAndReplaceBar),
            new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty ReplaceTextProperty = DependencyProperty.Register(
            nameof(ReplaceText), typeof(string), typeof(FindAndReplaceBar),
            new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty FoundCountProperty = DependencyProperty.Register(
            nameof(FoundCount), typeof(int), typeof(FindAndReplaceBar),
            new FrameworkPropertyMetadata(0, OnCountOrIndexChanged));

        public static readonly DependencyProperty FindIndexProperty = DependencyProperty.Register(
            nameof(FindIndex), typeof(int), typeof(FindAndReplaceBar),
            new FrameworkPropertyMetadata(0, OnCountOrIndexChanged));

        private static readonly DependencyPropertyKey FindCountTextPropertyKey = DependencyProperty.RegisterReadOnly(
                nameof(FindCountText), typeof(string), typeof(FindAndReplaceBar),
                new FrameworkPropertyMetadata("No results"));

        public static readonly DependencyProperty FindCountTextProperty = FindCountTextPropertyKey.DependencyProperty;
        #endregion

        #region Properties
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set { SetValue(IsOpenProperty, value); }
        }

        public bool IsReplace
        {
            get => (bool)GetValue(IsReplaceProperty);
            set { SetValue(IsReplaceProperty, value); }
        }

        public bool HasSelection
        {
            get => (bool)GetValue(HasSelectionProperty);
            set { SetValue(HasSelectionProperty, value); }
        }

        public bool InSelection
        {
            get => (bool)GetValue(InSelectionProperty);
            set { SetValue(InSelectionProperty, value); }
        }

        public bool MatchCase
        {
            get => (bool)GetValue(MatchCaseProperty);
            set { SetValue(MatchCaseProperty, value); }
        }

        public bool MatchWholeWord
        {
            get => (bool)GetValue(MatchWholeWordProperty);
            set { SetValue(MatchWholeWordProperty, value); }
        }

        public bool UseRegex
        {
            get => (bool)GetValue(UseRegexProperty);
            set { SetValue(UseRegexProperty, value); }
        }

        public bool PreserveCase
        {
            get => (bool)GetValue(PreserveCaseProperty);
            set { SetValue(PreserveCaseProperty, value); }
        }

        public string FindText
        {
            get => (string)GetValue(FindTextProperty);
            set { SetValue(FindTextProperty, value); }
        }

        public string ReplaceText
        {
            get => (string)GetValue(ReplaceTextProperty);
            set { SetValue(ReplaceTextProperty, value); }
        }

        public int FoundCount
        {
            get => (int)GetValue(FoundCountProperty);
            set { SetValue(FoundCountProperty, value); }
        }

        public int FindIndex
        {
            get => (int)GetValue(FindIndexProperty);
            set { SetValue(FindIndexProperty, value); }
        }

        [Bindable(false), Browsable(false)]
        public string FindCountText
        {
            get => (string)GetValue(FindCountTextProperty);
        }

        public ICommand FindPreviousCommand
        {
            get => findPreviousCommand;
        }

        public ICommand FindNextCommand
        {
            get => findNextCommand;
        }

        public ICommand FindReplaceCommand
        {
            get => findReplaceCommand;
        }

        public ICommand FindReplaceAllCommand
        {
            get => findReplaceAllCommand;
        }
        #endregion

        #region Constructors
        static FindAndReplaceBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FindAndReplaceBar), new FrameworkPropertyMetadata(typeof(FindAndReplaceBar)));

            CommandManager.RegisterClassInputBinding(typeof(FindAndReplaceBar), new InputBinding(FindInSelectionCommand, 
                new KeyGesture(Key.L, ModifierKeys.Alt)));
            CommandManager.RegisterClassCommandBinding(typeof(FindAndReplaceBar), new CommandBinding(FindInSelectionCommand, 
                new ExecutedRoutedEventHandler(OnExecuteFindInSelection)));

            CommandManager.RegisterClassInputBinding(typeof(FindAndReplaceBar), new InputBinding(FindMatchCaseCommand, 
                new KeyGesture(Key.C, ModifierKeys.Alt)));
            CommandManager.RegisterClassCommandBinding(typeof(FindAndReplaceBar), new CommandBinding(FindMatchCaseCommand,
                new ExecutedRoutedEventHandler(OnExecuteFindMatchCase)));

            CommandManager.RegisterClassInputBinding(typeof(FindAndReplaceBar), new InputBinding(FindMatchWholeWordCommand, 
                new KeyGesture(Key.W, ModifierKeys.Alt)));
            CommandManager.RegisterClassCommandBinding(typeof(FindAndReplaceBar), new CommandBinding(FindMatchWholeWordCommand,
                new ExecutedRoutedEventHandler(OnExecuteFindMatchWholeWord)));

            CommandManager.RegisterClassInputBinding(typeof(FindAndReplaceBar), new InputBinding(FindUseRegexCommand, 
                new KeyGesture(Key.R, ModifierKeys.Alt)));
            CommandManager.RegisterClassCommandBinding(typeof(FindAndReplaceBar), new CommandBinding(FindUseRegexCommand,
                new ExecutedRoutedEventHandler(OnExecuteFindUseRegex)));

            CommandManager.RegisterClassInputBinding(typeof(FindAndReplaceBar), new InputBinding(FindPreserveCaseCommand, 
                new KeyGesture(Key.P, ModifierKeys.Alt)));
            CommandManager.RegisterClassCommandBinding(typeof(FindAndReplaceBar), new CommandBinding(FindPreserveCaseCommand,
                new ExecutedRoutedEventHandler(OnExecuteFindPreserveCase)));
        }

        public FindAndReplaceBar()
        {
            findNextCommand = new RoutedCommand("FindFindNext", typeof(FindAndReplaceBar));
            findNextCommand.InputGestures.Add(new KeyGesture(Key.Enter));
            findNextCommand.InputGestures.Add(new KeyGesture(Key.F3));
            CommandBindings.Add(new CommandBinding(findNextCommand, FindNext_Executed, FindNext_CanExecute));

            findPreviousCommand = new RoutedCommand("FindFindPrevious", typeof(FindAndReplaceBar));
            findPreviousCommand.InputGestures.Add(new KeyGesture(Key.F3, ModifierKeys.Shift));
            CommandBindings.Add(new CommandBinding(findPreviousCommand, FindPrevious_Executed, FindPrevious_CanExecute));

            findReplaceCommand = new RoutedCommand("FindReplace", typeof(FindAndReplaceBar));
            findReplaceCommand.InputGestures.Add(new KeyGesture(Key.Enter, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(findReplaceCommand, FindReplace_Executed, FindReplace_CanExecute));

            findReplaceAllCommand = new RoutedCommand("FindReplaceAll", typeof(FindAndReplaceBar));
            findReplaceAllCommand.InputGestures.Add(new KeyGesture(Key.Enter, ModifierKeys.Control | ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(findReplaceAllCommand, FindReplaceAll_Executed, FindReplaceAll_CanExecute));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Stop, Close_Executed, Close_CanExecute));

            Focusable = true;
        }
        #endregion

        #region Methods
        private static void OnIsOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is FindAndReplaceBar d)
            {
                d.OnIsOpenChanged((bool)e.OldValue, (bool)e.NewValue);
            }
        }

        protected virtual void OnIsOpenChanged(bool oldValue, bool newValue)
        { }

        private static void OnIsReplaceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is FindAndReplaceBar d)
            {
                if (d.replaceBarRow != null)
                {
                    var newValue = (bool)e.NewValue;
                    var height = newValue ? new GridLength(1, GridUnitType.Auto) : new GridLength(0, GridUnitType.Pixel);
                    d.replaceBarRow.Height = height;
                }

                d.OnIsReplaceChanged((bool)e.OldValue, (bool)e.NewValue);
                d.RaiseIsReplaceChangedEvent();
            }
        }

        protected virtual void OnIsReplaceChanged(bool oldValue, bool newValue)
        {
        }

        private static void OnInSelectionChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is FindAndReplaceBar d)
            {
                d.OnOptionsChanged(e.Property.Name, (bool)e.OldValue, (bool)e.NewValue);
                d.RaiseOptionsChangedEvent();
            }
        }

        private static void OnMatchCaseChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is FindAndReplaceBar d)
            {
                d.OnOptionsChanged(e.Property.Name, (bool)e.OldValue, (bool)e.NewValue);
                d.RaiseOptionsChangedEvent();
            }
        }

        private static void OnMatchWholeWordChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is FindAndReplaceBar d)
            {
                d.OnOptionsChanged(e.Property.Name, (bool)e.OldValue, (bool)e.NewValue);
                d.RaiseOptionsChangedEvent();
            }
        }

        private static void OnUseRegexChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is FindAndReplaceBar d)
            {
                d.OnOptionsChanged(e.Property.Name, (bool)e.OldValue, (bool)e.NewValue);
                d.RaiseOptionsChangedEvent();
            }
        }

        private static void OnPreserveCaseChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is FindAndReplaceBar d)
            {
                d.OnOptionsChanged(e.Property.Name, (bool)e.OldValue, (bool)e.NewValue);
                d.RaiseOptionsChangedEvent();
            }
        }

        protected virtual void OnOptionsChanged(string name, bool oldValue, bool newValue)
        {
        }

        private static void OnCountOrIndexChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is FindAndReplaceBar d)
            {
                string findCountText = d.FoundCount == 0 ? "No results" : $"{d.FindIndex} of {d.FoundCount}";
                
                d.SetValue(FindCountTextPropertyKey, findCountText);
                CommandManager.InvalidateRequerySuggested();

                d.OnCountOrIndexChanged((int)e.OldValue, (int)e.NewValue);
            }
        }

        protected virtual void OnCountOrIndexChanged(int oldValue, int newValue)
        { 
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

            rootContainer = GetTemplateChild(PART_RootContainer) as FrameworkElement;
            replaceBarRow = GetTemplateChild(PART_ReplaceBarRow) as RowDefinition;
            popup = GetTemplateChild(PART_Popup) as Popup;
            textBoxFind = GetTemplateChild(PART_FindInput) as TextBox;
            textBoxReplace = GetTemplateChild(PART_ReplaceInput) as TextBox;

            if (popup != null)
            {
                popup.Placement = PlacementMode.Custom;
                popup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(Popup_PlacementCallback);
                popup.Opened += Popup_Opened;
                popup.Closed += Popup_Closed;
            }
            if (textBoxFind != null)
            {
                textBoxFind.TextChanged += TextBoxFind_TextChanged;
            }
            if (textBoxReplace != null)
            {
                textBoxReplace.TextChanged += TextBoxReplace_TextChanged;
            }

            HookupGripperEvents();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (IsOpen && textBoxFind != null)
            {
                Keyboard.Focus(textBoxFind);
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
                Focus();
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
            if (textBoxFind != null)
            {
                Keyboard.Focus(textBoxFind);
            }
            RaiseIsOpenChangedEvent();
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            RaiseIsOpenChangedEvent();
        }

        private void TextBoxFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox t)
            {
                string newText = t.Text;
                OnFindTextChanged(newText);
                RaiseFindTextChangedEvent(newText);
            }
        }

        protected virtual void OnFindTextChanged(string text)
        {
        }

        private void TextBoxReplace_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox t)
            {
                string newText = t.Text;
                OnReplaceTextChanged(newText);
                RaiseReplaceTextChangedEvent(newText);
            }
        }

        protected virtual void OnReplaceTextChanged(string text)
        {
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
            if (rootContainer != null && popup != null)
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
                var maxWidth = Math.Min(windowWidth - 80, 600);
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
        private static void OnExecuteFindInSelection(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is FindAndReplaceBar c)
            {
                c.InSelection = !c.InSelection;
            }
        }

        private static void OnExecuteFindMatchCase(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is FindAndReplaceBar c)
            {
                c.MatchCase = !c.MatchCase;
            }
        }

        private static void OnExecuteFindMatchWholeWord(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is FindAndReplaceBar c)
            {
                c.MatchWholeWord = !c.MatchWholeWord;
            }
        }

        private static void OnExecuteFindUseRegex(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is FindAndReplaceBar c)
            {
                c.UseRegex = !c.UseRegex;
            }
        }

        private static void OnExecuteFindPreserveCase(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is FindAndReplaceBar c)
            {
                c.PreserveCase = !c.PreserveCase;
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

        private void FindNext_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanFind(SearchDirection.Next);
        }

        private void FindNext_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OnFindNextExecuted();
            RaiseFindAcceptedEvent(SearchDirection.Next);
        }

        protected virtual void OnFindNextExecuted()
        {   
        }

        private void FindPrevious_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanFind(SearchDirection.Previous);
        }

        private void FindPrevious_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OnFindPreviousExecuted();
            RaiseFindAcceptedEvent(SearchDirection.Previous);
        }

        protected virtual void OnFindPreviousExecuted()
        {   
        }

        private void FindReplace_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanReplace(SearchDirection.Next);
        }

        private void FindReplace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OnReplaceExecuted();
            RaiseReplaceAcceptedEvent(SearchDirection.None, false);
        }

        protected virtual void OnReplaceExecuted()
        {   
        }

        private void FindReplaceAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanReplace(SearchDirection.All);
        }

        private void FindReplaceAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OnReplaceAllExecuted();
            RaiseReplaceAcceptedEvent(SearchDirection.All, true);
        }

        protected virtual void OnReplaceAllExecuted()
        {   
        }

        protected virtual bool RaiseOptionsChangedEvent()
        {
            var args = new RoutedOptionsChangedEventArgs(MatchCase, MatchWholeWord, UseRegex, InSelection, PreserveCase);
            args.RoutedEvent = OptionsChangedEvent;
            RaiseEvent(args);
            return args.Handled;
        }

        protected virtual bool RaiseIsOpenChangedEvent()
        {
            var args = new RoutedEventArgs();
            args.RoutedEvent = IsOpenChangedEvent;
            RaiseEvent(args);
            return args.Handled;
        }

        protected virtual bool RaiseIsReplaceChangedEvent()
        {
            var args = new RoutedEventArgs();
            args.RoutedEvent = IsReplaceChangedEvent;
            RaiseEvent(args);
            return args.Handled;
        }

        protected virtual bool RaiseFindTextChangedEvent(string text)
        {
            if (textBoxFind == null) return false;

            RoutedFindEventArgs args = new RoutedFindEventArgs(text, SearchDirection.All, MatchCase, MatchWholeWord, UseRegex, InSelection);
            args.RoutedEvent = FindTextChangedEvent;
            RaiseEvent(args);

            return args.Handled;
        }

        protected virtual bool RaiseReplaceTextChangedEvent(string text)
        {
            if (textBoxReplace == null) return false;

            string searchText = textBoxFind.Text;

            RoutedReplaceEventArgs args = new RoutedReplaceEventArgs(searchText, text, SearchDirection.All, false,
                MatchCase, MatchWholeWord, UseRegex, InSelection, PreserveCase);

            args.RoutedEvent = ReplaceTextChangedEvent;
            RaiseEvent(args);
            return args.Handled;
        }

        protected virtual bool RaiseFindAcceptedEvent(SearchDirection direction)
        {
            if (textBoxFind == null) return false;

            string searchText = textBoxFind.Text;

            RoutedFindEventArgs args = new RoutedFindEventArgs(searchText, direction, MatchCase, MatchWholeWord, UseRegex, InSelection);
            args.RoutedEvent = FindAcceptedEvent;
            RaiseEvent(args);

            return args.Handled;
        }

        protected virtual bool RaiseReplaceAcceptedEvent(SearchDirection direction, bool replaceAll = false)
        {
            if (textBoxFind == null || textBoxReplace == null) return false;

            string searchText = textBoxFind.Text;
            if (string.IsNullOrEmpty(searchText)) return false;

            string replaceText = textBoxReplace.Text;

            RoutedReplaceEventArgs args = new RoutedReplaceEventArgs(searchText, replaceText, direction, replaceAll, 
                MatchCase, MatchWholeWord, UseRegex, InSelection, PreserveCase);

            args.RoutedEvent = ReplaceAcceptedEvent;
            RaiseEvent(args);

            return args.Handled;
        }
        #endregion

        public bool CanFind(SearchDirection direction)
        {
            bool result = IsOpen && textBoxFind != null && !string.IsNullOrEmpty(textBoxFind.Text) && FoundCount > 0;
            
            if (direction == SearchDirection.Next)
                result = result && FindIndex < FoundCount;
            else if (direction == SearchDirection.Previous)
                result = result && FindIndex > 1;

            return result;
        }

        public bool CanReplace(SearchDirection direction)
        {
            return CanFind(direction) && textBoxReplace != null && !string.IsNullOrEmpty(textBoxReplace.Text);
        }

        public void Show()
        {
            IsOpen = true;
            Focus();
        }

        public void Show(bool isReplace)
        {
            IsReplace = isReplace;
            Show();
        }

        public void Hide()
        {
            IsOpen = false;
        }
        #endregion
    }

    #region RoutedOptionsChangedEvent
    public delegate void RoutedOptionsChangedEventHandler(object sender, RoutedOptionsChangedEventArgs e);
    public class RoutedOptionsChangedEventArgs : RoutedEventArgs
    {
        public bool MatchCase { get; private set; }

        public bool MatchWholeWord { get; private set; }

        public bool UseRegex { get; private set; }

        public bool InSelection { get; private set; }

        public bool PreserveCase { get; private set; }

        public RoutedOptionsChangedEventArgs(bool matchCase = false, bool matchWholeWord = false, bool useRegex = false, bool inSelection = false, bool preserveCase = false)
        {
            MatchCase = matchCase;
            MatchWholeWord = matchWholeWord;
            UseRegex = useRegex;
            InSelection = inSelection;
            PreserveCase = preserveCase;
        }
    }
    #endregion

    public enum SearchDirection
    {
        None,
        Previous,
        Next,
        All
    }

    #region RoutedFindEvent
    public delegate void RoutedFindEventHandler(object sender, RoutedFindEventArgs e);
    public class RoutedFindEventArgs : RoutedEventArgs
    {
        public string SearchText { get; private set; }

        public bool MatchCase { get; private set; }

        public bool MatchWholeWord { get; private set; }

        public bool UseRegex { get; private set; }

        public bool InSelection { get; private set; }

        public SearchDirection SearchDirection { get; private set; }

        public RoutedFindEventArgs(string searchText, SearchDirection direction,
            bool matchCase = false, bool matchWholeWord = false, bool useRegex = false, bool inSelection = false)
        {
            SearchText = searchText;
            SearchDirection = direction;
            MatchCase = matchCase;
            MatchWholeWord = matchWholeWord;
            UseRegex = useRegex;
            InSelection = inSelection;
        }
    }
    #endregion

    #region RoutedReplaceEvent
    public delegate void RoutedReplaceEventHandler(object sender, RoutedReplaceEventArgs e);
    public class RoutedReplaceEventArgs : RoutedFindEventArgs
    {
        public string ReplaceText { get; private set; }

        public bool ReplaceAll { get; private set; }

        public bool PreserveCase { get; private set; }

        public RoutedReplaceEventArgs(string searchText, string replaceText, SearchDirection direction, bool replaceAll = false,
            bool matchCase = false, bool matchWholeWord = false, bool useRegex = false, bool inSelection = false, bool preserveCase = false)
            : base(searchText, direction, matchCase, matchWholeWord, useRegex, inSelection)
        {
            ReplaceText = replaceText;
            ReplaceAll = replaceAll;
            PreserveCase = preserveCase;
        }
    }
    #endregion
}
