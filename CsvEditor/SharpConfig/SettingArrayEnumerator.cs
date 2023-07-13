// Copyright (c) 2013-2022 Cemalettin Dervis, MIT License.
// https://github.com/cemdervis/SharpConfig

namespace SharpConfig
{
    // Enumerates the elements of a Setting that represents an array.
    internal sealed class SettingArrayEnumerator
    {
        private readonly string _stringValue;
        private readonly bool _shouldCalcElemString;
        private int _idxInString;
        private readonly int _lastRBraceIdx;
        private int _prevElemIdxInString;
        private int _braceBalance;
        private bool _isInQuotes;
        private bool _isDone;

        public SettingArrayEnumerator(string value, bool shouldCalcElemString)
        {
            _stringValue = value;
            _idxInString = -1;
            _lastRBraceIdx = -1;
            _shouldCalcElemString = shouldCalcElemString;
            IsValid = true;
            _isDone = false;

            for (int i = 0; i < value.Length; ++i)
            {
                char ch = value[i];
                if (ch != ' ' && ch != '{')
                {
                    break;
                }

                if (ch != '{')
                {
                    continue;
                }

                _idxInString = i + 1;
                _braceBalance = 1;
                _prevElemIdxInString = i + 1;

                break;
            }

            // Abort if no valid '{' occurred.
            if (_idxInString < 0)
            {
                IsValid = false;
                _isDone = true;
                return;
            }

            // See where the last valid '}' is.
            for (int i = value.Length - 1; i >= 0; --i)
            {
                char ch = value[i];
                if (ch != ' ' && ch != '}')
                {
                    break;
                }

                if (ch != '}')
                {
                    continue;
                }

                _lastRBraceIdx = i;

                break;
            }

            // Abort if no valid '}' occurred.
            if (_lastRBraceIdx < 0)
            {
                IsValid = false;
                _isDone = true;
                return;
            }

            // See if this is an empty array such as "{    }" or "{}".
            // If so, this is a valid array, but with size 0.
            if (_idxInString == _lastRBraceIdx ||
                !IsNonEmptyValue(_stringValue, _idxInString, _lastRBraceIdx))
            {
                IsValid = true;
                _isDone = true;
                return;
            }
        }

        private void UpdateElementString(int idx)
        {
            Current = _stringValue.Substring(
              _prevElemIdxInString,
              idx - _prevElemIdxInString
              );

            Current = Current.Trim(' '); // trim spaces first

            // Now trim the quotes, but only the first and last, because
            // the setting value itself can contain quotes.
            if (Current[Current.Length - 1] == '\"')
            {
                Current = Current.Remove(Current.Length - 1, 1);
            }

            if (Current[0] == '\"')
            {
                Current = Current.Remove(0, 1);
            }
        }

        public bool Next()
        {
            if (_isDone)
            {
                return false;
            }

            int idx = _idxInString;
            while (idx <= _lastRBraceIdx)
            {
                char ch = _stringValue[idx];
                if (ch == '{' && !_isInQuotes)
                {
                    ++_braceBalance;
                }
                else if (ch == '}' && !_isInQuotes)
                {
                    --_braceBalance;
                    if (idx == _lastRBraceIdx)
                    {
                        // This is the last element.
                        if (!IsNonEmptyValue(_stringValue, _prevElemIdxInString, idx))
                        {
                            // Empty array element; invalid array.
                            IsValid = false;
                        }
                        else if (_shouldCalcElemString)
                        {
                            UpdateElementString(idx);
                        }
                        _isDone = true;
                        break;
                    }
                }
                else if (ch == '\"')
                {
                    int iNextQuoteMark = _stringValue.IndexOf('\"', idx + 1);
                    if (iNextQuoteMark > 0 && _stringValue[iNextQuoteMark - 1] != '\\')
                    {
                        idx = iNextQuoteMark;
                        _isInQuotes = false;
                    }
                    else
                    {
                        _isInQuotes = true;
                    }
                }
                else if (ch == Configuration.ArrayElementSeparator && _braceBalance == 1 && !_isInQuotes)
                {
                    if (!IsNonEmptyValue(_stringValue, _prevElemIdxInString, idx))
                    {
                        // Empty value in-between commas; this is an invalid array.
                        IsValid = false;
                    }
                    else if (_shouldCalcElemString)
                    {
                        UpdateElementString(idx);
                    }

                    _prevElemIdxInString = idx + 1;

                    // Yield.
                    ++idx;
                    break;
                }

                ++idx;
            }

            _idxInString = idx;

            if (_isInQuotes)
            {
                IsValid = false;
            }

            return IsValid;
        }

        private static bool IsNonEmptyValue(string s, int begin, int end)
        {
            for (; begin < end; ++begin)
            {
                if (s[begin] != ' ')
                {
                    return true;
                }
            }

            return false;
        }

        public string Current { get; private set; }

        public bool IsValid { get; private set; }
    }
}
