using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpConfig
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Struct | AttributeTargets.Property)]
    public class SectionAttribute : Attribute
    {
        public string Name { get; set; }

        public SectionAttribute()
        {
            Name = Section.DefaultSectionName;
        }

        public SectionAttribute(string name)
        {
            Name = name;
        }
    }
}
