using System;
using System.Text;

namespace QRStudio.Engine.Codec.Util
{
	public class QRCodeUtility
	{
		public static int sqrt(int val)
		{
			int temp, g = 0, b = 0x8000, bshft = 15;
			do 
			{
				if (val >= (temp = (((g << 1) + b) << bshft--)))
				{
					g += b;
					val -= temp;
				}
			}
			while ((b >>= 1) > 0);
			
			return g;
		}

        public static bool IsUniCode(string value)
        {
            byte[] ascii = AsciiStringToByteArray(value);
            byte[] unicode = UnicodeStringToByteArray(value);
            string value1 = FromASCIIByteArray(ascii);
            string value2 = FromUnicodeByteArray(unicode);
            if (value1 != value2)
                return true;
            return false;
        }

        public static bool IsUnicode(byte[] byteData)
        {
            string value1 = FromASCIIByteArray(byteData);
            string value2 = FromUnicodeByteArray(byteData);
            byte[] ascii = AsciiStringToByteArray(value1);
            byte[] unicode = UnicodeStringToByteArray(value2);
            if (ascii[0] != unicode[0])
                return true;
            return false;
        }

        public static string FromASCIIByteArray(byte[] characters)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            string constructedString = encoding.GetString(characters);
            return constructedString;
        }

        public static string FromUnicodeByteArray(byte[] characters)
        {
            UnicodeEncoding encoding = new UnicodeEncoding();
            string constructedString = encoding.GetString(characters);
            return constructedString;
        }

        public static byte[] AsciiStringToByteArray(string str)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            return encoding.GetBytes(str);
        }

        public static byte[] UnicodeStringToByteArray(string str)
        {
            UnicodeEncoding encoding = new UnicodeEncoding();
            return encoding.GetBytes(str);
        }
	}
}