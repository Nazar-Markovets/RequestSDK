using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestSDK.Test.Exceptions
{
    internal sealed class RevisionException : Exception
    {
        public RevisionException(string exception) : base("\n" + exception) 
        {

        }
    }
}
