//Authors: Roman Ivantsov, Philipp Serr

using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;

namespace Irony.Tests {
#if USE_NUNIT
    using NUnit.Framework;
    using TestClass = NUnit.Framework.TestFixtureAttribute;
    using TestMethod = NUnit.Framework.TestAttribute;
    using TestInitialize = NUnit.Framework.SetUpAttribute;
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

  [TestClass]
  public class NumberLiteralTests : TerminalTestsBase {

    [TestMethod]
    public void GeneralTest() {
      NumberLiteral number = new NumberLiteral("Number");
      number.DefaultIntTypes = new TypeCode[] { TypeCode.Int32, TypeCode.Int64, NumberLiteral.TypeCodeBigInt };
      SetTerminal(number);
      TryMatch("123");
      CheckType(typeof(int));
      Assert.IsTrue((int)_token.Value == 123, "Failed to read int value");
      TryMatch("123.4");
      Assert.IsTrue(Math.Abs(Convert.ToDouble(_token.Value) - 123.4) < 0.000001, "Failed to read float value");
      //100 digits
      string sbig = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
      TryMatch(sbig);
      Assert.IsTrue(_token.Value.ToString() == sbig, "Failed to read big integer value");
    }//method

    //The following "sign" test methods and a fix are contributed by ashmind codeplex user
     [TestMethod]
    public void TestSignedDoesNotMatchSingleMinus() {
      var number = new NumberLiteral("number", NumberOptions.AllowSign);
      SetTerminal(number);
      TryMatch("-");
      Assert.IsNull(_token, "Parsed single '-' as a number value.");
    }

    [TestMethod]
    public void TestSignedDoesNotMatchSinglePlus() {
       var number = new NumberLiteral("number", NumberOptions.AllowSign);
       SetTerminal(number);
       TryMatch("+");
       Assert.IsNull(_token, "Parsed single '+' as a number value.");
     }
    
    [TestMethod]
    public void TestSignedMatchesNegativeCorrectly() {
      var number = new NumberLiteral("number", NumberOptions.AllowSign);
      SetTerminal(number);
      TryMatch("-500");
      Assert.AreEqual(-500, _token.Value, "Negative number was parsed incorrectly; expected: {0}, scanned: {1}", "-500", _token.Value);
    }

    [TestMethod]
    public void TestCSharpNumber() {
      double eps = 0.0001;
      SetTerminal(TerminalFactory.CreateCSharpNumber("Number"));

      //Simple integers and suffixes
      TryMatch("123 ");
      CheckType(typeof(int));
      Assert.IsTrue(_token.Details != null, "ScanDetails object not found in token.");
      Assert.IsTrue((int)_token.Value == 123, "Failed to read int value");

      TryMatch(Int32.MaxValue.ToString());
      CheckType(typeof(int));
      Assert.IsTrue((int)_token.Value == Int32.MaxValue, "Failed to read Int32.MaxValue.");

      TryMatch(UInt64.MaxValue.ToString());
      CheckType(typeof(ulong));
      Assert.IsTrue((ulong)_token.Value == UInt64.MaxValue, "Failed to read uint64.MaxValue value");

      TryMatch("123U ");
      CheckType(typeof(UInt32));
      Assert.IsTrue((UInt32)_token.Value == 123, "Failed to read uint value");

      TryMatch("123L ");
      CheckType(typeof(long));
      Assert.IsTrue((long)_token.Value == 123, "Failed to read long value");

      TryMatch("123uL ");
      CheckType(typeof(ulong));
      Assert.IsTrue((ulong)_token.Value == 123, "Failed to read ulong value");

      //Hex representation
      TryMatch("0x012 ");
      CheckType(typeof(Int32));
      Assert.IsTrue((Int32)_token.Value == 0x012, "Failed to read hex int value");

      TryMatch("0x12U ");
      CheckType(typeof(UInt32));
      Assert.IsTrue((UInt32)_token.Value == 0x012, "Failed to read hex uint value");

      TryMatch("0x012L ");
      CheckType(typeof(long));
      Assert.IsTrue((long)_token.Value == 0x012, "Failed to read hex long value");

      TryMatch("0x012uL ");
      CheckType(typeof(ulong));
      Assert.IsTrue((ulong)_token.Value == 0x012, "Failed to read hex ulong value");

      //Floating point types
      TryMatch("123.4 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value #1");

      TryMatch("1234e-1 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 1234e-1) < eps, "Failed to read double value #2");

      TryMatch("12.34e+01 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value  #3");

      TryMatch("0.1234E3 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value  #4");

      TryMatch("123.4f ");
      CheckType(typeof(float));
      Assert.IsTrue(Math.Abs((Single)_token.Value - 123.4) < eps, "Failed to read float(single) value");

      TryMatch("123.4m ");
      CheckType(typeof(decimal));
      Assert.IsTrue(Math.Abs((decimal)_token.Value - 123.4m) < Convert.ToDecimal(eps), "Failed to read decimal value");

      TryMatch("123. "); //should ignore dot and read number as int. compare it to python numbers - see below
      CheckType(typeof(int));
      Assert.IsTrue((int)_token.Value == 123, "Failed to read int value with trailing dot");

      //Quick parse
      TryMatch("1 ");
      CheckType(typeof(int));
      //When going through quick parse path (for one-digit numbers), the NumberScanInfo record is not created and hence is absent in Attributes
      Assert.IsTrue(_token.Details == null, "Quick parse test failed: ScanDetails object is found in token - quick parse path should not produce this object.");
      Assert.IsTrue((int)_token.Value == 1, "Failed to read quick-parse value");
    }

    [TestMethod]
    public void TestVBNumber() {
      double eps = 0.0001;
      SetTerminal(TerminalFactory.CreateVbNumber("Number"));

      //Simple integer
      TryMatch("123 ");
      CheckType(typeof(int));
      Assert.IsTrue(_token.Details != null, "ScanDetails object not found in token.");
      Assert.IsTrue((int)_token.Value == 123, "Failed to read int value");

      //Test all suffixes
      TryMatch("123S ");
      CheckType(typeof(Int16));
      Assert.IsTrue((Int16)_token.Value == 123, "Failed to read short value");

      TryMatch("123I ");
      CheckType(typeof(Int32));
      Assert.IsTrue((Int32)_token.Value == 123, "Failed to read int value");

      TryMatch("123% ");
      CheckType(typeof(Int32));
      Assert.IsTrue((Int32)_token.Value == 123, "Failed to read int value");

      TryMatch("123L ");
      CheckType(typeof(long));
      Assert.IsTrue((long)_token.Value == 123, "Failed to read long value");

      TryMatch("123& ");
      CheckType(typeof(Int64));
      Assert.IsTrue((Int64)_token.Value == 123, "Failed to read long value");

      TryMatch("123us ");
      CheckType(typeof(UInt16));
      Assert.IsTrue((UInt16)_token.Value == 123, "Failed to read ushort value");

      TryMatch("123ui ");
      CheckType(typeof(UInt32));
      Assert.IsTrue((UInt32)_token.Value == 123, "Failed to read uint value");

      TryMatch("123ul ");
      CheckType(typeof(ulong));
      Assert.IsTrue((ulong)_token.Value == 123, "Failed to read ulong value");

      //Hex and octal 
      TryMatch("&H012 ");
      CheckType(typeof(int));
      Assert.IsTrue((int)_token.Value == 0x012, "Failed to read hex int value");

      TryMatch("&H012L ");
      CheckType(typeof(long));
      Assert.IsTrue((long)_token.Value == 0x012, "Failed to read hex long value");

      TryMatch("&O012 ");
      CheckType(typeof(int));
      Assert.IsTrue((int)_token.Value == 10, "Failed to read octal int value"); //12(oct) = 10(dec)

      TryMatch("&o012L ");
      CheckType(typeof(long));
      Assert.IsTrue((long)_token.Value == 10, "Failed to read octal long value");

      //Floating point types
      TryMatch("123.4 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value #1");

      TryMatch("1234e-1 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 1234e-1) < eps, "Failed to read double value #2");

      TryMatch("12.34e+01 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value  #3");

      TryMatch("0.1234E3 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value  #4");

      TryMatch("123.4R ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value #5");

      TryMatch("123.4# ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value #6");

      TryMatch("123.4f ");
      CheckType(typeof(float));
      Assert.IsTrue(Math.Abs((Single)_token.Value - 123.4) < eps, "Failed to read float(single) value");

      TryMatch("123.4! ");
      CheckType(typeof(float));
      Assert.IsTrue(Math.Abs((Single)_token.Value - 123.4) < eps, "Failed to read float(single) value");

      TryMatch("123.4D ");
      CheckType(typeof(decimal));
      Assert.IsTrue(Math.Abs((decimal)_token.Value - 123.4m) < Convert.ToDecimal(eps), "Failed to read decimal value");

      TryMatch("123.4@ ");
      CheckType(typeof(decimal));
      Assert.IsTrue(Math.Abs((decimal)_token.Value - 123.4m) < Convert.ToDecimal(eps), "Failed to read decimal value");

      //Quick parse
      TryMatch("1 ");
      CheckType(typeof(int));
      //When going through quick parse path (for one-digit numbers), the NumberScanInfo record is not created and hence is absent in Attributes
      Assert.IsTrue(_token.Details == null, "Quick parse test failed: ScanDetails object is found in token - quick parse path should not produce this object.");
      Assert.IsTrue((int)_token.Value == 1, "Failed to read quick-parse value");
    }


    [TestMethod]
    public void TestPythonNumber() {
      double eps = 0.0001;
      SetTerminal(TerminalFactory.CreatePythonNumber("Number"));

      //Simple integers and suffixes
      TryMatch("123 ");
      CheckType(typeof(int));
      Assert.IsTrue(_token.Details != null, "ScanDetails object not found in token.");
      Assert.IsTrue((int)_token.Value == 123, "Failed to read int value");

      TryMatch("123L ");
      CheckType(typeof(long));
      Assert.IsTrue((long)_token.Value == 123, "Failed to read long value");

      //Hex representation
      TryMatch("0x012 ");
      CheckType(typeof(int));
      Assert.IsTrue((int)_token.Value == 0x012, "Failed to read hex int value");

      TryMatch("0x012l "); //with small "L"
      CheckType(typeof(long));
      Assert.IsTrue((long)_token.Value == 0x012, "Failed to read hex long value");

      //Floating point types
      TryMatch("123.4 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value #1");

      TryMatch("1234e-1 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 1234e-1) < eps, "Failed to read double value #2");

      TryMatch("12.34e+01 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value  #3");

      TryMatch("0.1234E3 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value  #4");

      TryMatch(".1234 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 0.1234) < eps, "Failed to read double value with leading dot");

      TryMatch("123. ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 123.0) < eps, "Failed to read double value with trailing dot");

      //Big integer
      string sbig = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"; //100 digits
      TryMatch(sbig);
      Assert.IsTrue(_token.Value.ToString() == sbig, "Failed to read big integer value");

      //Quick parse
      TryMatch("1,");
      CheckType(typeof(int));
      Assert.IsTrue(_token.Details == null, "Quick parse test failed: ScanDetails object is found in token - quick parse path should produce this object.");
      Assert.IsTrue((int)_token.Value == 1, "Failed to read quick-parse value");

    }

    [TestMethod]
    public void TestSchemeNumber() {
      double eps = 0.0001;
      SetTerminal(TerminalFactory.CreateSchemeNumber("Number"));


      //Just test default float value (double), and exp symbols (e->double, s->single, d -> double)
      TryMatch("123.4 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value #1");

      TryMatch("1234e-1 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 1234e-1) < eps, "Failed to read single value #2");

      TryMatch("1234s-1 ");
      CheckType(typeof(Single));
      Assert.IsTrue(Math.Abs((Single)_token.Value - 1234e-1) < eps, "Failed to read single value #3");

      TryMatch("12.34d+01 ");
      CheckType(typeof(double));
      Assert.IsTrue(Math.Abs((double)_token.Value - 123.4) < eps, "Failed to read double value  #4");
    }//method

    [TestMethod]
    public void TestNumberWithUnderscore() {
      var number = new NumberLiteral("number", NumberOptions.AllowUnderscore);
      SetTerminal(number);

      //Simple integers and suffixes
      TryMatch("1_234_567");
      CheckType(typeof(int));
      Assert.IsTrue((int)_token.Value == 1234567, "Failed to read int value with underscores.");
    }//method


    //There was a bug discovered in NumberLiteral - it cannot parse appropriately the int.MinValue value.
    // This test ensures that the issue is fixed.
    [TestMethod]
    public void TestMinMaxValues() {
        var number = new NumberLiteral("number", NumberOptions.AllowSign);
        number.DefaultIntTypes = new TypeCode[] { TypeCode.Int32 };
        SetTerminal(number);
        var s = int.MinValue.ToString();
        TryMatch(s);
        Assert.IsFalse(_token.IsError(), "Failed to scan int.MinValue, scanner returned an error."); 
        CheckType(typeof(int));
        Assert.IsTrue((int)_token.Value == int.MinValue, "Failed to scan int.MinValue, scanned value does not match.");
        s = int.MaxValue.ToString();
        TryMatch(s);
        Assert.IsFalse(_token.IsError(), "Failed to scan int.MaxValue, scanner returned an error.");
        CheckType(typeof(int));
        Assert.IsTrue((int)_token.Value == int.MaxValue, "Failed to read int.MaxValue");
    }//method

  }//class
}//namespace
