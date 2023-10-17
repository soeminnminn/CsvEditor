using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CsvEditor.Plugin
{
    #region AssmInfo
    public class AssmInfo<T>
    {
        #region Variables
        private string m_assemblyPath;
        private Type m_type;
        private T m_module;
        private byte[] m_publicKey;
        #endregion

        #region Constructor
        public AssmInfo(string assemblyPath, Type type, T module, byte[] publicKey)
        {
            this.m_assemblyPath = assemblyPath;
            this.m_type = type;
            this.m_module = module;
            this.m_publicKey = publicKey;
        }
        #endregion

        #region Properties
        public string AssemblyPath
        {
            get { return this.m_assemblyPath; }
        }

        public Type Type
        {
            get { return this.m_type; }
        }

        public T Module
        {
            get { return this.m_module; }
        }

        public byte[] PublicKey
        {
            get { return this.m_publicKey; }
        }
        #endregion
    }
    #endregion

    #region AssmLoader
    public class AssmLoader<T> : IList<AssmInfo<T>>, IEnumerable<AssmInfo<T>>
    {
        #region Variables
        private string m_path = string.Empty;
        private List<AssmInfo<T>> m_list;
        #endregion

        #region Constructor
        public AssmLoader()
        {
            this.m_list = new List<AssmInfo<T>>();
        }
        public AssmLoader(string path)
            : this()
        {
            this.m_path = path;
        }
        #endregion

        #region Private Methods
        private void LoadInternal(string[] files)
        {
            this.m_list.Clear();

            if (files.Length == 0) return;

            foreach (string file in files)
            {
                Exception err = null;
                AssmInfo<T> assm = this.LoadAssembly(file, ref err);

                if (assm != null)
                    this.m_list.Add(assm);

                if (err != null)
                    System.Diagnostics.Debug.WriteLine(err.Message);
            }
        }

        private AssmInfo<T> LoadAssembly(string assemblyPath, ref Exception err)
        {
            try
            {
                if (!File.Exists(assemblyPath)) return null;

                Assembly asm = Assembly.LoadFrom(assemblyPath);

                AssemblyName assemblyName = asm.GetName();
                byte[] publicKey = assemblyName.GetPublicKey();

                Type[] assemblyTypes = asm.GetTypes();
                Type wantedType = typeof(T);

                Type foundType = null;
                foreach (Type type in assemblyTypes)
                {
                    if (wantedType.IsAssignableFrom(type) && !type.IsInterface)
                    {
                        foundType = type;
                        break;
                    }
                }

                if (foundType != null)
                {
                    T oLibrary = (T)asm.CreateInstance(foundType.FullName);
                    return new AssmInfo<T>(assemblyPath, foundType, oLibrary, publicKey);
                }
            }
            catch (Exception ex)
            {
                err = ex;
            }
            return null;
        }
        #endregion

        #region Public Methods
        public void Load()
        {
            if (!Directory.Exists(this.m_path)) return;
            this.Load(this.m_path);
        }

        public void Load(string path)
        {
            if (!Directory.Exists(path)) return;
            string[] files = Directory.GetFiles(path);
            this.LoadInternal(files);
        }

        public void Load(string path, string searchPattern)
        {
            if (!Directory.Exists(path)) return;
            string[] files = Directory.GetFiles(path, searchPattern);
            this.LoadInternal(files);
        }

        public void Load(string path, string searchPattern, SearchOption searchOption)
        {
            if (!Directory.Exists(path)) return;
            string[] files = Directory.GetFiles(path, searchPattern, searchOption);
            this.LoadInternal(files);
        }
        #endregion

        #region Implemented Methods
        public int IndexOf(AssmInfo<T> item)
        {
            return this.m_list.IndexOf(item);
        }

        public void Insert(int index, AssmInfo<T> item)
        {
            if (item != null)
                this.m_list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.m_list.RemoveAt(index);
        }

        public void Add(AssmInfo<T> item)
        {
            if (item != null)
                this.m_list.Add(item);
        }

        public bool Remove(AssmInfo<T> item)
        {
            if (item != null)
                return this.m_list.Remove(item);

            return false;
        }

        public void Clear()
        {
            this.m_list.Clear();
        }

        public bool Contains(AssmInfo<T> item)
        {
            return this.m_list.Contains(item);
        }

        public void CopyTo(AssmInfo<T>[] array, int arrayIndex)
        {
            this.m_list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<AssmInfo<T>> GetEnumerator()
        {
            List<AssmInfo<T>>.Enumerator emu = this.m_list.GetEnumerator();
            while (emu.MoveNext())
            {
                yield return emu.Current;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new List<AssmInfo<T>>.Enumerator();
        }
        #endregion

        #region Implemented Properties
        public AssmInfo<T> this[int index]
        {
            get { return this.m_list[index]; }
            set { throw new NotSupportedException(); }
        }

        public int Count
        {
            get { return this.m_list.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }
        #endregion
    }
    #endregion
}
