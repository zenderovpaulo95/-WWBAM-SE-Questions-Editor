using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms.VisualStyles;

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

        public static string ConvertFromLatin(string str)
        {
            try
            {
                byte[] temp_string = Encoding.Unicode.GetBytes(str);
                temp_string = Encoding.Convert(Encoding.Unicode, Encoding.GetEncoding(1252), temp_string);
                temp_string = Encoding.Convert(Encoding.GetEncoding(Form1.settings.ASCIICode), Encoding.Unicode, temp_string);
                
                return Encoding.Unicode.GetString(temp_string);
            }
            catch
            {
                return "Error";
            }
        }

        public static string ConvertToLatin(string str)
        {
            try
            {
                byte[] temp_string = Encoding.Unicode.GetBytes(str);
                temp_string = Encoding.Convert(Encoding.Unicode, Encoding.GetEncoding(Form1.settings.ASCIICode), temp_string);
                temp_string = Encoding.Convert(Encoding.GetEncoding(1252), Encoding.Unicode, temp_string);

                return Encoding.Unicode.GetString(temp_string);
            }
            catch
            {
                return "Error";
            }
        }
    }
}
