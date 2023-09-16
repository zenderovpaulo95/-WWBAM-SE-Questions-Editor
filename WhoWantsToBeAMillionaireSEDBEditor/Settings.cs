using System;
using System.Xml.Serialization;

namespace WhoWantsToBeAMillionaireSEDBEditor
{
    [Serializable()]
    public class Settings
    {
        public static void SaveSettings(Settings settings)
        {
            string xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + "config.xml";
            XmlSerializer xmlS = new XmlSerializer(typeof(Settings));
            System.IO.TextWriter xmlW = new System.IO.StreamWriter(xmlPath);
            xmlS.Serialize(xmlW, settings);

            xmlW.Flush();
            xmlW.Close();
        }

        private int _ASCIICode;
        private bool _nonUnicodeChecked;

        [XmlAttribute("windows")]
        public int ASCIICode
        {
            get { return _ASCIICode; }
            set { _ASCIICode = value; }
        }

        [XmlAttribute("nonUnicodeChecked")]
        public bool NonUnicodeChecked
        {
            get { return _nonUnicodeChecked; }
            set { _nonUnicodeChecked = value; }
        }


        public Settings(int ascii, bool nonUnicodeSet)
        {
            this.ASCIICode = ascii;
            this.NonUnicodeChecked = nonUnicodeSet;
        }
        
        public Settings() { }
    }
}
