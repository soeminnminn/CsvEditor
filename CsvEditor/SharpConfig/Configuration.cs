// Copyright (c) 2013-2022 Cemalettin Dervis, MIT License.
// https://github.com/cemdervis/SharpConfig

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpConfig
{
    /// <summary>
    /// Represents a configuration.
    /// Configurations contain one or multiple sections
    /// that in turn can contain one or multiple settings.
    /// The <see cref="Configuration"/> class is designed
    /// to work with classic configuration formats such as
    /// .ini and .cfg, but is not limited to these.
    /// </summary>
    public partial class Configuration : IEnumerable<Section>
    {
        private static CultureInfo s_cultureInfo;
        private static char s_preferredCommentChar;
        private static char s_arrayElementSeparator;
        private static readonly Dictionary<Type, ITypeStringConverter> s_typeStringConverters;

        internal readonly List<Section> _sections;

        static Configuration()
        {
            // For now, clone the invariant culture so that the
            // deprecated DateTimeFormat/NumberFormat properties still work,
            // but without modifying the real invariant culture instance.
            s_cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();

            ValidCommentChars = new[] { '#', ';' };
            s_preferredCommentChar = '#';
            s_arrayElementSeparator = ',';

            FallbackConverter = new FallbackStringConverter();

            // Add all stock converters.
            s_typeStringConverters = new Dictionary<Type, ITypeStringConverter>
      {
        { typeof(bool), new BoolStringConverter() },
        { typeof(byte), new ByteStringConverter() },
        { typeof(char), new CharStringConverter() },
        { typeof(DateTime), new DateTimeStringConverter() },
        { typeof(decimal), new DecimalStringConverter() },
        { typeof(double), new DoubleStringConverter() },
        { typeof(Enum), new EnumStringConverter() },
        { typeof(short), new Int16StringConverter() },
        { typeof(int), new Int32StringConverter() },
        { typeof(long), new Int64StringConverter() },
        { typeof(sbyte), new SByteStringConverter() },
        { typeof(float), new SingleStringConverter() },
        { typeof(string), new StringStringConverter() },
        { typeof(ushort), new UInt16StringConverter() },
        { typeof(uint), new UInt32StringConverter() },
        { typeof(ulong), new UInt64StringConverter() }
      };

            IgnoreInlineComments = false;
            IgnorePreComments = false;
            SpaceBetweenEquals = false;
            OutputRawStringValues = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration()
        {
            _sections = new List<Section>();
        }

        /// <summary>
        /// Gets an enumerator that iterates through the configuration.
        /// </summary>
        public IEnumerator<Section> GetEnumerator()
          => _sections.GetEnumerator();

        /// <summary>
        /// Gets an enumerator that iterates through the configuration.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
          => GetEnumerator();

        /// <summary>
        /// Adds a section to the configuration.
        /// </summary>
        /// <param name="section">The section to add.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="section"/> is null.</exception>
        /// <exception cref="ArgumentException">When the section already exists in the configuration.</exception>
        public void Add(Section section)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            if (Contains(section))
            {
                throw new ArgumentException("The specified section already exists in the configuration.");
            }

            _sections.Add(section);
        }

        /// <summary>
        /// Adds a section with a specific name to the configuration.
        /// </summary>
        /// <param name="sectionName">The name of the section to add.</param>
        /// <returns>The added section.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="sectionName"/> is null or empty.</exception>
        public Section Add(string sectionName)
        {
            var section = new Section(sectionName);
            Add(section);
            return section;
        }

        /// <summary>
        /// Removes a section from the configuration by its name.
        /// If there are multiple sections with the same name, only the first section is removed.
        /// To remove all sections that have the name name, use the RemoveAllNamed() method instead.
        /// </summary>
        /// <param name="sectionName">The case-sensitive name of the section to remove.</param>
        /// <returns>True if a section with the specified name was removed; false otherwise.</returns>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="sectionName"/> is null or empty.</exception>
        public bool Remove(string sectionName)
        {
            return string.IsNullOrEmpty(sectionName)
              ? throw new ArgumentNullException(nameof(sectionName))
              : Remove(FindSection(sectionName));
        }

        /// <summary>
        /// Removes a section from the configuration.
        /// </summary>
        /// <param name="section">The section to remove.</param>
        /// <returns>True if the section was removed; false otherwise.</returns>
        public bool Remove(Section section)
          => _sections.Remove(section);

        /// <summary>
        /// Removes all sections that have a specific name.
        /// </summary>
        /// <param name="sectionName">The case-sensitive name of the sections to remove.</param>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="sectionName"/> is null or empty.</exception>
        public void RemoveAllNamed(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                throw new ArgumentNullException(nameof(sectionName));
            }

            while (Remove(sectionName))
            {
                // Nothing to do.
            }
        }

        /// <summary>
        /// Clears the configuration of all sections.
        /// </summary>
        public void Clear()
          => _sections.Clear();

        /// <summary>
        /// Determines whether a specified section is contained in the configuration.
        /// </summary>
        /// <param name="section">The section to check for containment.</param>
        /// <returns>True if the section is contained in the configuration; false otherwise.</returns>
        public bool Contains(Section section)
          => _sections.Contains(section);

        /// <summary>
        /// Determines whether a specifically named section is contained in the configuration.
        /// </summary>
        /// <param name="sectionName">The name of the section.</param>
        /// <returns>True if the section is contained in the configuration; false otherwise.</returns>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="sectionName"/> is null or empty.</exception>
        public bool Contains(string sectionName)
        {
            return string.IsNullOrEmpty(sectionName)
              ? throw new ArgumentNullException(nameof(sectionName))
              : FindSection(sectionName) != null;
        }

        /// <summary>
        /// Determines whether a specifically named section is contained in the configuration,
        /// and whether that section in turn contains a specifically named setting.
        /// </summary>
        /// <param name="sectionName">The name of the section.</param>
        /// <param name="settingName">The name of the setting.</param>
        /// <returns>True if the section and the respective setting was found; false otherwise.</returns>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="sectionName"/> or <paramref name="settingName"/> is null or empty.</exception>
        public bool Contains(string sectionName, string settingName)
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                throw new ArgumentNullException(nameof(sectionName));
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new ArgumentNullException(nameof(settingName));
            }

            Section section = FindSection(sectionName);

            return section != null && section.Contains(settingName);
        }

        /// <summary>
        /// Gets the string representation of the configuration. It represents the same contents
        /// as if the configuration was saved to a file or stream.
        /// </summary>
        public string StringRepresentation
        {
            get
            {
                var sb = new StringBuilder();

                // Write all sections.
                bool isFirstSection = true;

                void WriteSection(Section section)
                {
                    if (!isFirstSection)
                    {
                        sb.AppendLine();
                    }

                    // Leave some space between this section and the element that is above,
                    // if this section has pre-comments and isn't the first section in the configuration.
                    if (!isFirstSection && section.PreComment != null)
                    {
                        sb.AppendLine();
                    }

                    if (section.Name != Section.DefaultSectionName)
                    {
                        sb.AppendLine(section.ToString());
                    }

                    // Write all settings.
                    foreach (Setting setting in section)
                    {
                        sb.AppendLine(setting.ToString());
                    }

                    if (section.Name != Section.DefaultSectionName || section.SettingCount > 0)
                    {
                        isFirstSection = false;
                    }
                }

                // Write the default section first.
                Section defaultSection = DefaultSection;

                if (defaultSection.SettingCount > 0)
                {
                    WriteSection(DefaultSection);
                }

                // Now the rest.
                foreach (Section section in _sections.Where(section => section != defaultSection))
                {
                    WriteSection(section);
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Registers a type converter to be used for setting value conversions.
        /// </summary>
        /// <param name="converter">The converter to register.</param>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="converter"/> is null.</exception>
        /// <exception cref="InvalidOperationException">When a converter for the converter's type is already registered.</exception>
        public static void RegisterTypeStringConverter(ITypeStringConverter converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            Type type = converter.ConvertibleType;
            if (s_typeStringConverters.ContainsKey(type))
            {
                throw new InvalidOperationException($"A converter for type '{type.FullName}' is already registered.");
            }

            s_typeStringConverters.Add(type, converter);
        }

        /// <summary>
        /// Deregisters a type converter from setting value conversion.
        /// </summary>
        /// <param name="type">The type whose associated converter to deregister.</param>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="type"/> is null.</exception>
        /// <exception cref="InvalidOperationException">When no converter is registered for <paramref name="type"/>.</exception>
        public static void DeregisterTypeStringConverter(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!s_typeStringConverters.ContainsKey(type))
            {
                throw new InvalidOperationException($"No converter is registered for type '{type.FullName}'.");
            }

            s_typeStringConverters.Remove(type);
        }

        internal static ITypeStringConverter FindTypeStringConverter(Type type)
        {
            if (type.IsEnum)
            {
                type = typeof(Enum);
            }

            if (!s_typeStringConverters.TryGetValue(type, out ITypeStringConverter converter))
            {
                converter = FallbackConverter;
            }

            return converter;
        }

        internal static ITypeStringConverter FallbackConverter { get; private set; }

        /// <summary>
        /// Loads a configuration from a file.
        /// </summary>
        ///
        /// <param name="filename">The location of the configuration file.</param>
        /// <param name="encoding">The encoding applied to the contents of the file. Specify null to auto-detect the encoding.</param>
        ///
        /// <returns>
        /// The loaded <see cref="Configuration"/> object.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="filename"/> is null or empty.</exception>
        /// <exception cref="FileNotFoundException">When the specified configuration file is not found.</exception>
        public static Configuration LoadFromFile(string filename, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("Configuration file not found.", filename);
            }

            return (encoding == null) ?
                LoadFromString(File.ReadAllText(filename)) :
                LoadFromString(File.ReadAllText(filename, encoding));
        }

        /// <summary>
        /// Loads a configuration from a text stream.
        /// </summary>
        ///
        /// <param name="stream">   The text stream to load the configuration from.</param>
        /// <param name="encoding"> The encoding applied to the contents of the stream. Specify null to auto-detect the encoding.</param>
        ///
        /// <returns>
        /// The loaded <see cref="Configuration"/> object.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="stream"/> is null.</exception>
        public static Configuration LoadFromStream(Stream stream, Encoding encoding = null)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            StreamReader reader = encoding == null ?
                new StreamReader(stream) :
                new StreamReader(stream, encoding);

            string source;

            using (reader)
                source = reader.ReadToEnd();

            return LoadFromString(source);
        }

        /// <summary>
        /// Loads a configuration from text (source code).
        /// </summary>
        ///
        /// <param name="source">The text (source code) of the configuration.</param>
        ///
        /// <returns>
        /// The loaded <see cref="Configuration"/> object.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="source"/> is null.</exception>
        public static Configuration LoadFromString(string source)
        {
            return source == null
              ? throw new ArgumentNullException(nameof(source))
              : ConfigurationReader.ReadFromString(source);
        }

        /// <summary>
        /// Loads a configuration from a binary file using a specific <see cref="BinaryReader"/>.
        /// </summary>
        ///
        /// <param name="filename">The location of the configuration file.</param>
        /// <param name="reader">  The reader to use. Specify null to use the default <see cref="BinaryReader"/>.</param>
        ///
        /// <returns>
        /// The loaded configuration.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="filename"/> is null or empty.</exception>
        public static Configuration LoadFromBinaryFile(string filename, BinaryReader reader = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }

            using (FileStream stream = File.OpenRead(filename))
            {
                return LoadFromBinaryStream(stream, reader);
            }
        }

        /// <summary>
        /// Loads a configuration from a binary stream, using a specific <see cref="BinaryReader"/>.
        /// </summary>
        ///
        /// <param name="stream">The stream to load the configuration from.</param>
        /// <param name="reader">The reader to use. Specify null to use the default <see cref="BinaryReader"/>.</param>
        ///
        /// <returns>
        /// The loaded configuration.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="stream"/> is null.</exception>
        public static Configuration LoadFromBinaryStream(Stream stream, BinaryReader reader = null)
        {
            return stream == null
              ? throw new ArgumentNullException(nameof(stream))
              : ConfigurationReader.ReadFromBinaryStream(stream, reader);
        }

        /// <summary>
        /// Saves the configuration to a file using the default character encoding, which is UTF8.
        /// </summary>
        ///
        /// <param name="filename">The location of the configuration file.</param>
        public void SaveToFile(string filename)
          => SaveToFile(filename, null);

        /// <summary>
        /// Saves the configuration to a file.
        /// </summary>
        ///
        /// <param name="filename">The location of the configuration file.</param>
        /// <param name="encoding">The character encoding to use. Specify null to use the default encoding, which is UTF8.</param>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="filename"/> is null or empty.</exception>
        public void SaveToFile(string filename, Encoding encoding)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }

            using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                SaveToStream(stream, encoding);
            }
        }

        /// <summary>
        /// Saves the configuration to a stream using the default character encoding, which is UTF8.
        /// </summary>
        ///
        /// <param name="stream">The stream to save the configuration to.</param>
        public void SaveToStream(Stream stream)
          => SaveToStream(stream, null);

        /// <summary>
        /// Saves the configuration to a stream.
        /// </summary>
        ///
        /// <param name="stream">The stream to save the configuration to.</param>
        /// <param name="encoding">The character encoding to use. Specify null to use the default encoding, which is UTF8.</param>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="stream"/> is null.</exception>
        public void SaveToStream(Stream stream, Encoding encoding)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            ConfigurationWriter.WriteToStreamTextual(this, stream, encoding);
        }

        /// <summary>
        /// Saves the configuration to a binary file, using the default <see cref="BinaryWriter"/>.
        /// </summary>
        ///
        /// <param name="filename">The location of the configuration file.</param>
        public void SaveToBinaryFile(string filename)
          => SaveToBinaryFile(filename, null);

        /// <summary>
        /// Saves the configuration to a binary file, using a specific <see cref="BinaryWriter"/>.
        /// </summary>
        ///
        /// <param name="filename">The location of the configuration file.</param>
        /// <param name="writer">  The writer to use. Specify null to use the default writer.</param>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="filename"/> is null or empty.</exception>
        public void SaveToBinaryFile(string filename, BinaryWriter writer)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }

            using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                SaveToBinaryStream(stream, writer);
        }

        /// <summary>
        /// Saves the configuration to a binary stream, using the default <see cref="BinaryWriter"/>.
        /// </summary>
        ///
        /// <param name="stream">The stream to save the configuration to.</param>
        public void SaveToBinaryStream(Stream stream)
          => SaveToBinaryStream(stream, null);

        /// <summary>
        /// Saves the configuration to a binary file, using a specific <see cref="BinaryWriter"/>.
        /// </summary>
        ///
        /// <param name="stream">The stream to save the configuration to.</param>
        /// <param name="writer">The writer to use. Specify null to use the default writer.</param>
        /// 
        /// <exception cref="ArgumentNullException">When <paramref name="stream"/> is null.</exception>
        public void SaveToBinaryStream(Stream stream, BinaryWriter writer)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            ConfigurationWriter.WriteToStreamBinary(this, stream, writer);
        }

        /// <summary>
        /// Gets or sets the CultureInfo that is used for value conversion in SharpConfig.
        /// The default value is CultureInfo.InvariantCulture.
        /// </summary>
        /// 
        /// <exception cref="ArgumentNullException">When a null reference is set.</exception>
        public static CultureInfo CultureInfo
        {
            get => s_cultureInfo;
            set => s_cultureInfo = value ?? throw new ArgumentNullException("value");
        }

        /// <summary>
        /// Gets the array that contains all valid comment delimiting characters.
        /// The current value is { '#', ';' }.
        /// </summary>
        public static char[] ValidCommentChars { get; private set; }

        /// <summary>
        /// Gets or sets the preferred comment char when saving configurations.
        /// The default value is '#'.
        /// </summary>
        /// 
        /// <exception cref="ArgumentException">When an invalid character is set.</exception>
        public static char PreferredCommentChar
        {
            get => s_preferredCommentChar;
            set
            {
                if (!Array.Exists(ValidCommentChars, c => c == value))
                {
                    throw new ArgumentException("The specified char '" + value + "' is not allowed as a comment char.");
                }

                s_preferredCommentChar = value;
            }
        }

        /// <summary>
        /// Gets or sets the array element separator character for settings.
        /// The default value is ','.
        /// NOTE: remember that after you change this value while <see cref="Setting"/> instances exist,
        /// to expect their ArraySize and other array-related values to return different values.
        /// </summary>
        /// 
        /// <exception cref="ArgumentException">When a zero-character ('\0') is set.</exception>
        public static char ArrayElementSeparator
        {
            get => s_arrayElementSeparator;
            set
            {
                if (value == '\0')
                {
                    throw new ArgumentException("Zero-character is not allowed.");
                }

                s_arrayElementSeparator = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether string values are written
        /// without quotes, but including everything in between.
        /// Example:
        /// The following setting value
        ///     MySetting=" Example value"
        /// is written to a file in the following manner
        ///     MySetting= Example value
        /// </summary>
        public static bool OutputRawStringValues { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether inline-comments
        /// should be ignored when parsing a configuration.
        /// </summary>
        public static bool IgnoreInlineComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether pre-comments
        /// should be ignored when parsing a configuration.
        /// </summary>
        public static bool IgnorePreComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether space between
        /// equals should be added when creating a configuration.
        /// </summary>
        public static bool SpaceBetweenEquals { get; set; }

        /// <summary>
        /// Gets the number of sections that are in the configuration.
        /// </summary>
        public int SectionCount => _sections.Count;

        /// <summary>
        /// Gets or sets a section by index.
        /// </summary>
        /// <param name="index">The index of the section in the configuration.</param>
        /// 
        /// <returns>
        /// The section at the specified index.
        /// Note: no section is created when using this accessor.
        /// </returns>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">When the index is out of range.</exception>
        public Section this[int index]
          => index < 0 || index >= _sections.Count
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : _sections[index];

        /// <summary>
        /// Gets or sets a section by its name.
        /// If there are multiple sections with the same name, the first section is returned.
        /// If you want to obtain all sections that have the same name, use the GetSectionsNamed() method instead.
        /// </summary>
        ///
        /// <param name="name">The case-sensitive name of the section.</param>
        ///
        /// <returns>
        /// The section if found, otherwise a new section with
        /// the specified name is created, added to the configuration and returned.
        /// </returns>
        public Section this[string name]
        {
            get
            {
                Section section = FindSection(name);

                if (section == null)
                {
                    section = new Section(name);
                    Add(section);
                }

                return section;
            }
        }

        /// <summary>
        /// Gets the default, hidden section.
        /// </summary>
        public Section DefaultSection
          => this[Section.DefaultSectionName];

        /// <summary>
        /// Gets all sections that have a specific name.
        /// </summary>
        /// <param name="name">The case-sensitive name of the sections.</param>
        /// <returns>
        /// The found sections.
        /// Change from 3.2.9.1 to 3.3: Returns an <see cref="IEnumerable{T}"/> internally as of version 3.3. Previously returned a <see cref="List{T}"/>.
        /// </returns>
        public IEnumerable<Section> GetSectionsNamed(string name)
        {
            return _sections.Where(section => string.Equals(section.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        // Finds a section by its name.
        private Section FindSection(string name)
        {
            return _sections.FirstOrDefault(section => string.Equals(section.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
