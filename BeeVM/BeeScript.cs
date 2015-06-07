using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeVM
{
    public struct BeeScript
    {
        public int VersionNumber;
        public Variable[] Globals;
        public Variable[] Constants;
        public Instruction[] Instructions;
        public String[] Literals;
        public int[] Properties;
        public int[] Callbacks;
    }
}
