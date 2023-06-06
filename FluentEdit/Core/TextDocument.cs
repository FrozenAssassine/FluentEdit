using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentEdit.Core
{
    internal class TextDocument
    {
        public Encoding CurrentEncoding { get; set; } =  Encoding.UTF8;
        public string FileToken { get; set; } = "";
        public string FileName { get; set; } = "";
        public bool UnsavedChanges { get; set; } = false;

    }
}
