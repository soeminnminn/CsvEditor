using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CsvEditor.Controls
{
    public class NumericTextBox : TextBox
    {
        #region Variables

        #region ValueChanged Event
        //Due to a bug in Visual Studio, you cannot create event handlers for generic T args in XAML, so I have to use object instead.
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<object>), typeof(NumericTextBox));
        public event RoutedPropertyChangedEventHandler<object> ValueChanged
        {
            add
            {
                AddHandler(ValueChangedEvent, value);
            }
            remove
            {
                RemoveHandler(ValueChangedEvent, value);
            }
        }

        #endregion

        /// <summary>
        /// Flags if the Text and Value properties are in the process of being sync'd
        /// </summary>
        private bool _isSyncingTextAndValueProperties;
        private bool _internalValueSet;

        #endregion

        #region Constructor
        public NumericTextBox()
        {
            DataObject.AddPastingHandler(this, OnPasting);
        }
        #endregion

        #region Properties

        #region Maximum
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(decimal), typeof(NumericTextBox),
            new UIPropertyMetadata(100M, OnMaximumChanged, OnCoerceMaximum));
        public decimal Maximum
        {
            get
            {
                return (decimal)GetValue(MaximumProperty);
            }
            set
            {
                SetValue(MaximumProperty, value);
            }
        }

        private static void OnMaximumChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NumericTextBox control = o as NumericTextBox;
            if (control != null)
                control.OnMaximumChanged((decimal)e.OldValue, (decimal)e.NewValue);
        }

        protected virtual void OnMaximumChanged(decimal oldValue, decimal newValue)
        {
            this.Value = this.CoerceValueMinMax(this.Value);
        }

        private static object OnCoerceMaximum(DependencyObject d, object baseValue)
        {
            NumericTextBox control = d as NumericTextBox;
            if (control != null)
                return control.OnCoerceMaximum((decimal)baseValue);

            return baseValue;
        }

        protected virtual decimal OnCoerceMaximum(decimal baseValue)
        {
            return baseValue;
        }
        #endregion //Maximum

        #region Minimum
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(decimal), typeof(NumericTextBox),
            new UIPropertyMetadata(0M, OnMinimumChanged, OnCoerceMinimum));
        public decimal Minimum
        {
            get
            {
                return (decimal)GetValue(MinimumProperty);
            }
            set
            {
                SetValue(MinimumProperty, value);
            }
        }

        private static void OnMinimumChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NumericTextBox control = o as NumericTextBox;
            if (control != null)
                control.OnMinimumChanged((decimal)e.OldValue, (decimal)e.NewValue);
        }

        protected virtual void OnMinimumChanged(decimal oldValue, decimal newValue)
        {
            this.Value = this.CoerceValueMinMax(this.Value);
        }

        private static object OnCoerceMinimum(DependencyObject d, object baseValue)
        {
            NumericTextBox upDown = d as NumericTextBox;
            if (upDown != null)
                return upDown.OnCoerceMinimum((decimal)baseValue);

            return baseValue;
        }

        protected virtual decimal OnCoerceMinimum(decimal baseValue)
        {
            return baseValue;
        }
        #endregion //Minimum

        #region Increment
        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(decimal), typeof(NumericTextBox),
            new PropertyMetadata(1M, OnIncrementChanged, OnCoerceIncrement));
        public decimal Increment
        {
            get
            {
                return (decimal)GetValue(IncrementProperty);
            }
            set
            {
                SetValue(IncrementProperty, value);
            }
        }

        private static void OnIncrementChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NumericTextBox control = o as NumericTextBox;
            if (control != null)
                control.OnIncrementChanged((decimal)e.OldValue, (decimal)e.NewValue);
        }

        protected virtual void OnIncrementChanged(decimal oldValue, decimal newValue)
        {
        }

        private static object OnCoerceIncrement(DependencyObject d, object baseValue)
        {
            NumericTextBox control = d as NumericTextBox;
            if (control != null)
                return control.OnCoerceIncrement((decimal)baseValue);

            return baseValue;
        }

        protected virtual decimal OnCoerceIncrement(decimal baseValue)
        {
            return baseValue;
        }
        #endregion

        #region FormatString
        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(NumericTextBox),
            new UIPropertyMetadata(string.Empty, OnFormatStringChanged, OnCoerceFormatString));
        public string FormatString
        {
            get
            {
                return (string)GetValue(FormatStringProperty);
            }
            set
            {
                SetValue(FormatStringProperty, value);
            }
        }

        private static object OnCoerceFormatString(DependencyObject o, object baseValue)
        {
            NumericTextBox control = o as NumericTextBox;
            if (control != null)
                return control.OnCoerceFormatString((string)baseValue);

            return baseValue;
        }

        protected virtual string OnCoerceFormatString(string baseValue)
        {
            return baseValue ?? string.Empty;
        }

        private static void OnFormatStringChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NumericTextBox numericUpDown = o as NumericTextBox;
            if (numericUpDown != null)
                numericUpDown.OnFormatStringChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnFormatStringChanged(string oldValue, string newValue)
        {
            if (IsInitialized)
            {
                this.SyncTextAndValueProperties(false, null);
            }
        }
        #endregion //FormatString

        #region DefaultValue
        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(decimal), typeof(NumericTextBox),
            new UIPropertyMetadata(0M, OnDefaultValueChanged));
        public decimal DefaultValue
        {
            get
            {
                return (decimal)GetValue(DefaultValueProperty);
            }
            set
            {
                SetValue(DefaultValueProperty, value);
            }
        }

        private static void OnDefaultValueChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            ((NumericTextBox)source).OnDefaultValueChanged((decimal)args.OldValue, (decimal)args.NewValue);
        }

        protected virtual void OnDefaultValueChanged(decimal oldValue, decimal newValue)
        {
            if (this.IsInitialized && string.IsNullOrEmpty(Text))
            {
                this.SyncTextAndValueProperties(true, Text);
            }
        }
        #endregion //DefaultValue

        #region Value
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(decimal), typeof(NumericTextBox),
          new FrameworkPropertyMetadata(0M, OnValueChanged, OnCoerceValue));
        public decimal Value
        {
            get
            {
                return (decimal)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        private void SetValueInternal(decimal value)
        {
            _internalValueSet = true;
            try
            {
                this.Value = value;
            }
            finally
            {
                _internalValueSet = false;
            }
        }

        private static object OnCoerceValue(DependencyObject o, object basevalue)
        {
            return ((NumericTextBox)o).OnCoerceValue(basevalue);
        }

        protected virtual object OnCoerceValue(object newValue)
        {
            return newValue;
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NumericTextBox control = o as NumericTextBox;
            if (control != null)
                control.OnValueChanged((decimal)e.OldValue, (decimal)e.NewValue);
        }

        protected virtual void OnValueChanged(decimal oldValue, decimal newValue)
        {
            if (!_internalValueSet && this.IsInitialized)
            {
                SyncTextAndValueProperties(false, null);
            }

            this.RaiseValueChangedEvent(oldValue, newValue);
        }
        #endregion //Value

        #endregion

        #region Methods
        protected virtual void RaiseValueChangedEvent(decimal oldValue, decimal newValue)
        {
            RoutedPropertyChangedEventArgs<object> args = new RoutedPropertyChangedEventArgs<object>(oldValue, newValue);
            args.RoutedEvent = ValueChangedEvent;
            RaiseEvent(args);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // When both Value and Text are initialized, Value has priority.
            // To be sure that the value is not initialized, it should
            // have no local value, no binding, and equal to the default value.
            bool updateValueFromText =
              (this.ReadLocalValue(ValueProperty) == DependencyProperty.UnsetValue)
              && (BindingOperations.GetBinding(this, ValueProperty) == null)
              && (object.Equals(this.Value, ValueProperty.DefaultMetadata.DefaultValue));

            this.SyncTextAndValueProperties(updateValueFromText, Text);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!e.Handled && IsFocused)
            {
                if (e.Delta < 0)
                {
                    OnDecrement();
                    e.Handled = true;
                }
                else if (e.Delta > 0)
                {
                    OnIncrement();
                    e.Handled = true;
                }
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (this.IsInitialized)
                this.SyncTextAndValueProperties(true, Text);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Up)
            {
                OnIncrement();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                OnDecrement();
                e.Handled = true;
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            string newText = Text;
            if (SelectionLength > 0)
                newText = Text.Remove(SelectionStart, SelectionLength);

            newText = newText.Insert(SelectionStart, e.Text);
            if (!string.IsNullOrEmpty(newText))
            {
                if (!TryParse(newText, out _))
                {
                    e.Handled = true;
                }
            }

            base.OnPreviewTextInput(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (SelectionLength == 0)
            {
                SelectionStart = Text.Length;
            }
            base.OnGotFocus(e);
        }

        protected virtual void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!SyncTextAndValueProperties(false, text))
                {
                    e.CancelCommand();
                }
            }
        }

        private decimal CoerceValueMinMax(decimal value)
        {
            if (IsLowerThan(value, Minimum))
                return Minimum;
            else if (IsGreaterThan(value, Maximum))
                return Maximum;
            else
                return value;
        }

        private bool IsLowerThan(decimal? value1, decimal? value2)
        {
            if (value1 == null || value2 == null)
                return false;

            return value1.Value < value2.Value;
        }

        private bool IsGreaterThan(decimal? value1, decimal? value2)
        {
            if (value1 == null || value2 == null)
                return false;

            return value1.Value > value2.Value;
        }

        protected virtual void OnDecrement()
        {
            var result = Value - Increment;
            this.Value = this.CoerceValueMinMax(result);
        }

        protected virtual void OnIncrement()
        {
            var result = Value + Increment;
            this.Value = this.CoerceValueMinMax(result);
        }

        private bool TryParse(string text, out decimal value)
        {
            if (!string.IsNullOrEmpty(text))
            {
                decimal result;
                if (decimal.TryParse(text, out result))
                {
                    value = CoerceValueMinMax(result);
                    return true;
                }
            }
            value = DefaultValue;
            return false;
        }

        private string ToStringValue()
        {
            //Manage FormatString of type "{}{0:N2} °" (in xaml) or "{0:N2} °" in code-behind.
            if (FormatString.Contains("{0"))
                return string.Format(CultureInfo.InvariantCulture, FormatString, Value);

            return Value.ToString(FormatString, CultureInfo.InvariantCulture);
        }

        protected bool SyncTextAndValueProperties(bool updateValueFromText, string text)
        {
            if (_isSyncingTextAndValueProperties)
                return true;

            _isSyncingTextAndValueProperties = true;
            bool parsedTextIsValid = true;
            try
            {
                if (updateValueFromText)
                {
                    decimal newValue;
                    parsedTextIsValid = TryParse(text, out newValue);
                    if (parsedTextIsValid)
                    {
                        SetValueInternal(newValue);
                    }
                }

                if (parsedTextIsValid)
                {
                    string newText = ToStringValue();
                    if (Text != newText)
                    {
                        int caretIdx = CaretIndex;
                        if (Text.Length == caretIdx)
                            caretIdx = newText.Length;

                        Text = newText;

                        if (caretIdx >= newText.Length)
                            caretIdx = newText.Length;

                        CaretIndex = caretIdx;
                    }
                }
            }
            finally
            {
                _isSyncingTextAndValueProperties = false;
            }
            return parsedTextIsValid;
        }
        #endregion
    }
}
