using System;

namespace CsvEditor.Models
{
    public enum StartUpModes : int
    {
        Blank = 0,
        LastFile = 1,
        NewFile = 2
    }

    public class StartUpMode
    {
        #region Variables
        private readonly string displayText;
        private readonly int value;
        #endregion

        #region Properties
        public static StartUpMode Blank
        {
            get => new StartUpMode(StartUpModes.Blank);
        }

        public static StartUpMode LastFile
        {
            get => new StartUpMode(StartUpModes.LastFile);
        }

        public static StartUpMode NewFile
        {
            get => new StartUpMode(StartUpModes.NewFile);
        }

        public int Value
        {
            get => value;
        }

        public string DisplayText
        {
            get => displayText;
        }
        #endregion

        #region Constructor
        internal StartUpMode(StartUpModes value)
        {
            switch (value)
            {
                case StartUpModes.LastFile:
                    this.value = (int)value;
                    displayText = SR.OpenLastFile;
                    break;
                case StartUpModes.NewFile:
                    this.value = (int)value;
                    displayText = SR.CreateNewFile;
                    break;
                default:
                    this.value = 0;
                    displayText = SR.StartUpBlank;
                    break;
            }
        }

        internal StartUpMode(int value)
        {
            switch (value)
            {
                case (int)StartUpModes.LastFile:
                    this.value = (int)StartUpModes.LastFile;
                    displayText = SR.OpenLastFile;
                    break;
                case (int)StartUpModes.NewFile:
                    this.value = (int)StartUpModes.NewFile;
                    displayText = SR.CreateNewFile;
                    break;
                default:
                    this.value = (int)StartUpModes.Blank;
                    displayText = SR.StartUpBlank;
                    break;
            }
        }
        #endregion

        #region Methods
        public override bool Equals(object obj)
        {
            if (obj is int val)
                return val == value;
            else if (obj is string str)
                return displayText == str;
            else if (obj is StartUpMode other)
                return other.value == value;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return displayText;
        }
        #endregion

        #region Operators
        public static implicit operator StartUpMode(StartUpModes value)
            => new StartUpMode(value);
        public static explicit operator StartUpModes(StartUpMode obj)
        {
            if (obj == null) return StartUpModes.Blank;
            return (StartUpModes)obj.Value;
        }

        public static bool operator ==(StartUpMode obj, int value)
        {
            if (obj != null)
                return obj.value == value;
            return false;
        }

        public static bool operator !=(StartUpMode obj, int value)
        {
            if (obj != null)
                return obj.value != value;
            return true;
        }

        public static bool operator ==(StartUpMode obj, StartUpModes value)
        {
            if (obj != null)
                return obj.value == (int)value;
            return false;
        }

        public static bool operator !=(StartUpMode obj, StartUpModes value)
        {
            if (obj != null)
                return obj.value != (int)value;
            return true;
        }
        #endregion
    }
}
