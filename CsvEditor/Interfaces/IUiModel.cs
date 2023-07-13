using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvEditor
{
    public interface IUiModel
    {
        bool ShowToolbar { get; set; }

        bool ShowStatusbar { get; set; }
    }
}
