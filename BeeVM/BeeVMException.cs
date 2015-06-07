using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeVM
{
    public class BeeVMException : Exception
    {
        public BeeVMException(string Message, Exception innerException) : base(Message, innerException) { }
        public BeeVMException(string Message) : base(Message) { }
    }
}
