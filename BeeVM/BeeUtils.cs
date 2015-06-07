using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeVM
{
    static public class BeeUtils
    {
        static public short ConvertFromBytes(byte byteA, byte byteB)
        {
            byte[] array  = new byte[2];
            array[1] = byteA; array[0] = byteB;
            return BitConverter.ToInt16(array, 0);
        }

        static public void ConvertToBytes ( short value , out byte byteA , out byte byteB )
        {
            var array = BitConverter.GetBytes(value);
            byteA = array[1];
            byteB = array[0];
        }
    }
}
