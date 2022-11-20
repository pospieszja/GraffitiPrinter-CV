using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraffitiPrinter.UI.Models
{
    public class GraffitiDocument
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public byte[] Binary { get; set; }
    }
}
