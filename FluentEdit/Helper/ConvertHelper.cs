using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentEdit.Helper
{
    internal class ConvertHelper
    {
        public static int ToInt(string val, int defaultValue = 0)
        {
            if (int.TryParse(val, out int result))
                return result;
            return defaultValue;
        }
    }
}
