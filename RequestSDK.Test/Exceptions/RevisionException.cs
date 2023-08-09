using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestSDK.Test.Exceptions
{
    internal sealed class NotSupportedException : Exception
    {
        public NotSupportedException(string exception) : base(exception) 
        {

        }
    }
}
