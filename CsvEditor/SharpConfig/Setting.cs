// Copyright (c) 2013-2022 Cemalettin Dervis, MIT License.
// https://github.com/cemdervis/SharpConfig

using System;
using System.Text;

namespace SharpConfig
{
    /// <summary>
    /// Represents a setting in a <see cref="Configuration"/>.
    /// Settings are always stored in a <see cref="Section"/>.
    /// </summary>
    public sealed class Setting : ConfigurationElement
    {
        private int _cachedArraySize;
        private bool _shouldCalculateArraySize;
        private char _cachedArrayElementSeparator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Setting"/> class.
        /// </summary>
        public Setting(string name)
            : this(name, string.Empty)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Setting"/> class.
        /// </summary>
        ///
        /// <param name="name"> The name of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        public Setting(string name, object value)
            : base(name)
        {
            SetValue(value);
            _cachedArrayElementSeparator = Configuration.ArrayElementSeparator;
        }

        /// <summary>
        /// Gets a value indicating whether this setting's value is empty.
        /// </summary>
        public bool IsEmpty
          => string.IsNullOrEmpty(RawValue);

        /// <summary>
        /// Gets or sets the raw value of this setting.
        /// </summary>
        public string RawValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="string"/>.
        /// Note: this is a shortcut to GetValue and SetValue.
        /// </summary>
        public string StringValue
        {
            get => GetValue<string>().Trim('\"');
            set => SetValue(value.Trim('\"'));
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="string"/> array.
        /// Note: this is a shortcut to GetValueArray and SetValue.
        /// </summary>
        public string[] StringValueArray
        {
            get => GetValueArray<string>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as an <see cref="int"/>.
        /// Note: this is a shortcut to GetValue and SetValue.
        /// </summary>
        public int IntValue
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as an <see cref="int"/> array.
        /// Note: this is a shortcut to GetValueArray and SetValue.
        /// </summary>
        public int[] IntValueArray
        {
            get => GetValueArray<int>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="float"/>.
        /// Note: this is a shortcut to GetValue and SetValue.
        /// </summary>
        public float FloatValue
        {
            get => GetValue<float>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="float"/> array.
        /// Note: this is a shortcut to GetValueArray and SetValue.
        /// </summary>
        public float[] FloatValueArray
        {
            get => GetValueArray<float>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="double"/>.
        /// Note: this is a shortcut to GetValue and SetValue.
        /// </summary>
        public double DoubleValue
        {
            get => GetValue<double>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="double"/> array.
        /// Note: this is a shortcut to GetValueArray and SetValue.
        /// </summary>
        public double[] DoubleValueArray
        {
            get => GetValueArray<double>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="decimal"/>.
        /// Note: this is a shortcut to GetValue and SetValue.
        /// </summary>
        public decimal DecimalValue
        {
            get => GetValue<decimal>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="decimal"/> array.
        /// Note: this is a shortcut to GetValueArray and SetValue.
        /// </summary>
        public decimal[] DecimalValueArray
        {
            get => GetValueArray<decimal>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="bool"/>.
        /// Note: this is a shortcut to GetValue and SetValue.
        /// </summary>
        public bool BoolValue
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="bool"/> array.
        /// Note: this is a shortcut to GetValueArray and SetValue.
        /// </summary>
        public bool[] BoolValueArray
        {
            get => GetValueArray<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this settings as a <see cref="DateTime"/>.
        /// Note: this is a shortcut to GetValue and SetValue.
        /// </summary>
        public DateTime DateTimeValue
        {
            get => GetValue<DateTime>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="DateTime"/> array.
        /// Note: this is a shortcut to GetValueArray and SetValue.
        /// </summary>
        public DateTime[] DateTimeValueArray
        {
            get => GetValueArray<DateTime>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="byte"/>.
        /// Note: this is a shortcut to GetValueArray and SetValue.
        /// </summary>
        public byte ByteValue
        {
            get => GetValue<byte>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="byte"/> array.
        /// Note: this is a shortcut to GetValueArray and SetValue.
        /// </summary>
        public byte[] ByteValueArray
        {
            get => GetValueArray<byte>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="sbyte"/>.
        /// Note: this is a shortcut to GetValueArray and SetValue.
        /// </summary>
        public sbyte SByteValue
        {
            get => GetValue<sbyte>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="sbyte"/> array.
        /// Note: this is a shortcut to GetValueArray and SetValue.
        /// </summary>
        public sbyte[] SByteValueArray
        {
            get => GetValueArray<sbyte>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="char"/>.
        /// Note: this is a shortcut to GetValueArray and SetValue.
        /// </summary>
        public char CharValue
        {
            get => GetValue<char>();
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of this setting as a <see cref="char"/> array.
        /// Note: this is a shortcut to GetValueArray and SetValue.
        /// </summary>
        public char[] CharValueArray
        {
            get
            {
                // Decode the bytes back to chars.
                byte[] bytes = ByteValueArray;
                return bytes != null ? Encoding.UTF8.GetChars(ByteValueArray) : null;
            }
            set
            {
                if (value != null)
                {
                    // Encode the chars to bytes, because writing raw chars such as
                    // '\0' can mess up the configuration file and the parser.
                    ByteValueArray = Encoding.UTF8.GetBytes(value);
                }
                else
                {
                    SetEmptyValue();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this setting is an array.
        /// </summary>
        public bool IsArray
          => (ArraySize >= 0);

        /// <summary>
        /// Gets the size of the array that this setting represents.
        /// If this setting is not an array, -1 is returned.
        /// </summary>
        public int ArraySize
        {
            get
            {
                // If the user changed the array element separator during the lifetime
                // of this setting, we have to recalculate the array size.
                if (_cachedArrayElementSeparator != Configuration.ArrayElementSeparator)
                {
                    _cachedArrayElementSeparator = Configuration.ArrayElementSeparator;
                    _shouldCalculateArraySize = true;
                }

                if (_shouldCalculateArraySize)
                {
                    _cachedArraySize = CalculateArraySize();
                    _shouldCalculateArraySize = false;
                }

                return _cachedArraySize;
            }
        }

        private int CalculateArraySize()
        {
            int size = 0;
            var enumerator = new SettingArrayEnumerator(RawValue, false);
            while (enumerator.Next())
            {
                ++size;
            }

            return (enumerator.IsValid ? size : -1);
        }

        /// <summary>
        /// Gets this setting's value as a specific type.
        /// </summary>
        ///
        /// <param name="type">The type of the object to retrieve.</param>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="type"/> is null.</exception>
        /// <exception cref="InvalidOperationException">When <paramref name="type"/> is an array type.</exception>
        /// <exception cref="InvalidOperationException">When the setting represents an array.</exception>
        public object GetValue(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsArray)
            {
                throw new InvalidOperationException("To obtain an array value, use GetValueArray() instead of GetValue().");
            }

            if (IsArray)
            {
                throw new InvalidOperationException("The setting represents an array. Use GetValueArray() to obtain its value.");
            }

            return CreateObjectFromString(RawValue, type);
        }

        /// <summary>
        /// Gets this setting's value as an array of a specific type.
        /// Note: this only works if the setting represents an array. If it is not, then null is returned.
        /// </summary>
        /// <param name="elementType">
        ///     The type of elements in the array. All values in the array are going to be converted to objects of this type.
        ///     If the conversion of an element fails, an exception is thrown.
        /// </param>
        /// <returns>The values of this setting as an array.</returns>
        public object[] GetValueArray(Type elementType)
        {
            if (elementType.IsArray)
            {
                throw CreateJaggedArraysNotSupportedEx(elementType);
            }

            int myArraySize = this.ArraySize;
            if (ArraySize < 0)
            {
                return null;
            }

            object[] values = new object[myArraySize];

            if (myArraySize > 0)
            {
                var enumerator = new SettingArrayEnumerator(RawValue, true);
                int iElem = 0;
                while (enumerator.Next())
                {
                    values[iElem] = CreateObjectFromString(enumerator.Current, elementType);
                    ++iElem;
                }
            }

            return values;
        }

        /// <summary>
        /// Gets this setting's value as a specific type.
        /// </summary>
        ///
        /// <typeparam name="T">The type of the object to retrieve.</typeparam>
        /// 
        /// <exception cref="InvalidOperationException">When <typeparamref name="T"/> is an array type.</exception>
        /// <exception cref="InvalidOperationException">When the setting represents an array.</exception>
        public T GetValue<T>()
        {
            Type type = typeof(T);

            if (type.IsArray)
            {
                throw new InvalidOperationException("To obtain an array value, use GetValueArray() instead of GetValue().");
            }

            if (IsArray)
            {
                throw new InvalidOperationException("The setting represents an array. Use GetValueArray() to obtain its value.");
            }

            return (T)CreateObjectFromString(RawValue, type);
        }

        /// <summary>
        /// Gets this setting's value as an array of a specific type.
        /// Note: this only works if the setting represents an array. If it is not, then null is returned.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of elements in the array. All values in the array are going to be converted to objects of this type.
        ///     If the conversion of an element fails, an exception is thrown.
        /// </typeparam>
        /// <returns>The values of this setting as an array.</returns>
        public T[] GetValueArray<T>()
        {
            Type type = typeof(T);

            if (type.IsArray)
            {
                throw CreateJaggedArraysNotSupportedEx(type);
            }

            int myArraySize = ArraySize;
            if (myArraySize < 0)
            {
                return null;
            }

            var values = new T[myArraySize];

            if (myArraySize > 0)
            {
                var enumerator = new SettingArrayEnumerator(RawValue, true);
                int iElem = 0;
                while (enumerator.Next())
                {
                    values[iElem] = (T)CreateObjectFromString(enumerator.Current, type);
                    ++iElem;
                }
            }

            return values;
        }

        /// <summary>
        /// Gets this setting's value as a specific type, or a specified default value
        /// if casting the setting to the type fails.
        /// </summary>
        /// <param name="defaultValue">
        /// Default value if casting the setting to the specified type fails.
        /// </param>
        /// <param name="setDefault">
        /// If true, and casting the setting to the specified type fails, <paramref name="defaultValue"/> is set
        /// as this setting's new value.
        /// </param>
        /// <typeparam name="T">The type of the object to retrieve.</typeparam>
        public T GetValueOrDefault<T>(T defaultValue, bool setDefault = false)
        {
            Type type = typeof(T);

            if (type.IsArray)
            {
                throw new InvalidOperationException("GetValueOrDefault<T> cannot be used with arrays.");
            }

            if (IsArray)
            {
                throw new InvalidOperationException("The setting represents an array. Use GetValueArray() to obtain its value.");
            }

            var result = CreateObjectFromString(RawValue, type, true);

            if (result != null)
            {
                return (T)result;
            }

            if (setDefault)
            {
                SetValue(defaultValue);
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets this setting's value as a specific type.
        /// </summary>
        /// <typeparam name="T">The type of the object to retrieve.</typeparam>
        /// <param name="value">The values of this setting as an array.</param>
        /// <param name="passEmpty">Try parse empty or not.</param>
        public bool TryGetValue<T>(out T value, bool passEmpty = false)
        {
            Type type = typeof(T);

            value = default(T);

            if (type.IsArray) return false;
            if (IsArray) return false;
            if (string.IsNullOrEmpty(RawValue) && !passEmpty) return false;

            try
            {
                var result = CreateObjectFromString(RawValue, type, true);
                if (result != null)
                {
                    value = (T)result;
                    return true;
                }
            }
            catch(Exception)
            {   
            }
            return false;
        }

        /// <summary>
        /// Gets this setting's value as an array of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of the object to retrieve.</typeparam>
        /// <param name="values">The values of this setting as an array.</param>
        /// <param name="passEmpty">Try parse empty or not.</param>
        public bool TryGetValueArray<T>(out T[] values, bool passEmpty = false)
        {
            Type type = typeof(T);

            values = new T[0];

            if (type.IsArray) return false;
            if (string.IsNullOrEmpty(RawValue) && !passEmpty) return false;

            int myArraySize = ArraySize;
            if (myArraySize < 0) return false;

            values = new T[myArraySize];

            if (myArraySize > 0)
            {
                var enumerator = new SettingArrayEnumerator(RawValue, true);
                int iElem = 0;
                while (enumerator.Next())
                {
                    values[iElem] = (T)CreateObjectFromString(enumerator.Current, type);
                    ++iElem;
                }
            }

            return true;
        }

        // Converts the value of a single element to a desired type.
        private static object CreateObjectFromString(string value, Type dstType, bool tryConvert = false)
        {
            Type underlyingType = Nullable.GetUnderlyingType(dstType);
            if (underlyingType != null)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return null; // Returns Nullable<T>().
                }

                // Otherwise, continue with our conversion using
                // the underlying type of the nullable.
                dstType = underlyingType;
            }

            ITypeStringConverter converter = Configuration.FindTypeStringConverter(dstType);

            object obj = converter.TryConvertFromString(value, dstType);

            if (obj == null && !tryConvert)
            {
                throw SettingValueCastException.Create(value, dstType, null);
            }

            return obj;
        }

        /// <summary>
        /// Sets the value of this setting via an object.
        /// </summary>
        /// 
        /// <param name="value">The value to set.</param>
        public void SetValue(object value)
        {
            if (value == null)
            {
                SetEmptyValue();
                return;
            }

            Type type = value.GetType();
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                if (elementType != null && elementType.IsArray)
                {
                    throw CreateJaggedArraysNotSupportedEx(type.GetElementType());
                }

                var values = value as Array;
                if (values != null)
                {
                    string[] strings = new string[values.Length];

                    for (int i = 0; i < values.Length; i++)
                    {
                        object elemValue = values.GetValue(i);
                        ITypeStringConverter converter = Configuration.FindTypeStringConverter(elemValue.GetType());
                        strings[i] = GetValueForOutput(converter.ConvertToString(elemValue));
                    }

                    RawValue = $"{{{string.Join(Configuration.ArrayElementSeparator.ToString(), strings)}}}";
                }

                if (values != null)
                {
                    _cachedArraySize = values.Length;
                }

                _shouldCalculateArraySize = false;
            }
            else
            {
                ITypeStringConverter converter = Configuration.FindTypeStringConverter(type);
                RawValue = converter.ConvertToString(value);
                _shouldCalculateArraySize = true;
            }
        }

        private void SetEmptyValue()
        {
            RawValue = string.Empty;
            _cachedArraySize = -1;
            _shouldCalculateArraySize = false;
        }

        private static string GetValueForOutput(string rawValue)
        {
            if (Configuration.OutputRawStringValues)
            {
                return rawValue;
            }

            if (rawValue.StartsWith("{") && rawValue.EndsWith("}"))
            {
                return rawValue;
            }

            if (rawValue.StartsWith("\"") && rawValue.EndsWith("\""))
            {
                return rawValue;
            }

            if (
              rawValue.IndexOf(" ", StringComparison.Ordinal) >= 0 || (
              rawValue.IndexOfAny(Configuration.ValidCommentChars) >= 0 &&
              !Configuration.IgnoreInlineComments))
            {
                rawValue = "\"" + rawValue + "\"";
            }

            return rawValue;
        }

        /// <summary>
        /// Gets the element's expression as a string.
        /// An example for a section would be "[Section]".
        /// </summary>
        /// <returns>The element's expression as a string.</returns>
        protected override string GetStringExpression()
        {
            return Configuration.SpaceBetweenEquals ?
              $"{Name} = {GetValueForOutput(RawValue)}" :
              $"{Name}={GetValueForOutput(RawValue)}";
        }

        private static ArgumentException CreateJaggedArraysNotSupportedEx(Type type)
        {
            // Determine the underlying element type.
            Type elementType = type.GetElementType();
            while (elementType != null && elementType.IsArray)
            {
                elementType = elementType.GetElementType();
            }

            throw new ArgumentException(
                $"Jagged arrays are not supported. The type you have specified is '{type.Name}', but '{elementType?.Name}' was expected.");
        }
    }
}
