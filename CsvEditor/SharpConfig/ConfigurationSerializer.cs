using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SharpConfig
{
    public partial class Configuration
    {
        public void Serialize<T>(T cfg) where T : class
        {
            Clear();

            Type t = typeof(T);
            object o = (object)cfg;

            var rootAttr = t.GetCustomAttribute<SectionAttribute>();
            if (rootAttr != null)
            {
                //
            }
        }

        public T Deserialize<T>() where T : class
        {
            return default(T);
        }
    }
}
