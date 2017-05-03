using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace WhoWantsToBeAMillionaireSEDBEditor
{
    class Methods
    {
        public static byte[] ReadFull(Stream stream)
        {
            byte[] buffer = new byte[3207];

            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }

        public static string ConvertToLatin(string str, int ASCII_N)
        {
            try
            {
                Encoding Unicode_text = Encoding.Unicode;
                byte[] temp_string = new byte[str.Length];
                string temp_str;
                temp_string = UnicodeEncoding.GetEncoding(ASCII_N).GetBytes(str);
                temp_str = UnicodeEncoding.GetEncoding(1252).GetString(temp_string);
                temp_string = Encoding.Convert(UnicodeEncoding.GetEncoding(1252), Unicode_text, temp_string);
                return Unicode_text.GetString(temp_string);
            }
            catch
            {
                return "Error";
            }
        }
    }
}
