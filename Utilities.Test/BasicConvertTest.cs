using System;
using System.Globalization;

using Xunit;

using Convert = SidekickNet.Utilities.BasicConvert;

namespace SidekickNet.Utilities.Test
{
    public class BasicConvertTest
    {
        [Fact]
        public void Convert_Boolean_To_Int()
        {
            var b = true;
            var i = Convert.ToType<int>(b);
            Assert.Equal(b, Convert.ToType<bool>(i));

            b = false;
            i = Convert.ToType<int>(b);
            Assert.Equal(b, Convert.ToType<bool>(i));
        }

        [Fact]
        public void Convert_Int_To_Long()
        {
            Assert.Equal(int.MaxValue, Convert.ToType<long>(int.MaxValue));
        }

        [Fact]
        public void Convert_Int_To_Short()
        {
            int i = short.MaxValue;
            Assert.Equal(short.MaxValue, Convert.ToType<short>(i));
        }

        [Fact]
        public void Convert_Int_To_Short_Overflow()
        {
            int i = short.MaxValue + 1;
            Assert.Throws<OverflowException>(() => Convert.ToType<short>(i));
        }

        [Fact]
        public void Convert_Int_To_Boolean()
        {
            Assert.True(Convert.ToType<bool>(1));
            Assert.True(Convert.ToType<bool>(-1));
            Assert.False(Convert.ToType<bool>(0));
        }

        [Fact]
        public void Convert_Int_To_String()
        {
            var i = int.MaxValue;
            var s = Convert.ToType<string>(i);

            // If s is in correct format, converting it back to int should yield the same value as the original
            Assert.Equal(i, Convert.ToType<int>(s));
        }

        [Fact]
        public void Convert_Double_To_String()
        {
            // Double values have 15-17 digits precision
            var d = double.MaxValue;
            var s = Convert.ToType<string>(d);

            // If s is in correct format, converting it back to double should yield the same value as the original
            Assert.Equal(d, Convert.ToType<double>(s));
        }

        [Fact]
        public void Convert_Decimal_To_String()
        {
            // Decimal values have 28-29 digits precision
            var m = decimal.MaxValue;
            var s = Convert.ToType<string>(m);

            // If s is in correct format, converting it back to decimal should yield the same value as the original
            Assert.Equal(m, Convert.ToType<decimal>(s));
        }

        [Fact]
        public void Convert_Struct_To_String()
        {
            var guid = Guid.NewGuid();
            var s = Convert.ToType<string>(guid);
            Assert.Equal(guid, Convert.ToType<Guid>(s));

            var now = DateTime.Now;
            Assert.Equal(now.ToString(CultureInfo.InvariantCulture), Convert.ToType<string>(now));
        }

        [Fact]
        public void Convert_String_To_Number()
        {
            Assert.Equal(1.1, Convert.ToType<double>("1.1"));
        }

        [Fact]
        public void Convert_String_To_Boolean()
        {
            Assert.True(Convert.ToType<bool>("true"));
            Assert.False(Convert.ToType<bool>("false"));
            Assert.False(Convert.ToType<bool>(string.Empty));
        }

        [Fact]
        public void Convert_String_To_DateTime()
        {
            var dateTime = DateTime.Now;
            Assert.Equal(dateTime, Convert.ToType<DateTime>(dateTime.ToString("o")));
        }

        [Fact]
        public void Convert_String_To_Guid()
        {
            var guid = Guid.NewGuid();
            Assert.Equal(guid, Convert.ToType<Guid>(guid.ToString()));
        }

        [Fact]
        public void Convert_Empty_String_To_Null()
        {
            Assert.Null(Convert.ToType<int?>(string.Empty));
        }

        [Fact]
        public void Convert_Empty_String_To_Default_Value()
        {
            Assert.Equal(0, Convert.ToType<int>(string.Empty));
            Assert.Equal(DateTime.MinValue, Convert.ToType<DateTime>(string.Empty));
            Assert.Equal(Guid.Empty, Convert.ToType<Guid>(string.Empty));
        }

        private const string WhiteSpaces = " \t\n";

        [Fact]
        public void Convert_White_Spaces_To_Null()
        {
            Assert.Null(Convert.ToType<int?>(WhiteSpaces));
        }

        [Fact]
        public void Convert_White_Spaces_To_Default_Value()
        {
            Assert.Equal(0, Convert.ToType<int>(WhiteSpaces));
            Assert.Equal(DateTime.MinValue, Convert.ToType<DateTime>(WhiteSpaces));
            Assert.Equal(Guid.Empty, Convert.ToType<Guid>(WhiteSpaces));
        }

        [Fact]
        public void Convert_Null_To_Default_Value()
        {
            Guid? guid = default;
            Assert.Equal(Guid.Empty, Convert.ToType<Guid>(guid));
        }

        [Fact]
        public void Convert_DBNull_To_Null()
        {
            Assert.Null(Convert.ToType<Guid?>(DBNull.Value));
        }

        [Fact]
        public void Convert_DBNull_To_Default_Value()
        {
            Assert.Equal(Guid.Empty, Convert.ToType<Guid>(DBNull.Value));
        }

        [Fact]
        public void Convert_Nullable_To_Nullable()
        {
            Guid? guid = Guid.NewGuid();
            Assert.Equal(guid, Convert.ToType<Guid?>(guid));
        }

        [Fact]
        public void Convert_Nullable_To_Non_Nullable()
        {
            Guid? guid = Guid.NewGuid();
            Assert.Equal(guid, Convert.ToType<Guid>(guid));
        }
    }
}
