using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Business.EDI
{
    public class EDIManager
    {
        public EDIManager(string path)
        {
            _path = path;
        }

        private string _path;

        public string CreateFile(TxtEDIBase edi, string fileName)
        {
            return CreateFile(new List<TxtEDIBase>() { edi }, fileName);
        }

        public string CreateFile(IEnumerable<TxtEDIBase> ediList, string fileName)
        {
            string data = string.Join(Environment.NewLine, ediList.Select(edi =>
            {
                if (edi != null)
                    return edi.ToText();
                else return string.Empty;
            }));
            if (string.IsNullOrEmpty(data)) return null;
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            string filePath = Path.Combine(_path, fileName);
            UTF8Encoding utf8 = new UTF8Encoding(false);
            File.WriteAllText(filePath, data, utf8);
            return filePath;
        }

        public static T TryParseTxtFile<T>(string fileName) where T : TxtEDIBase, new()
        {
            using (StreamReader reder = new StreamReader(fileName))
            {
                string txt = reder.ReadToEnd();
                return TryParseTxt<T>(txt);
            }
        }
        public static T TryParseTxt<T>(string txt) where T : TxtEDIBase, new()
        {
            T obj = new T();
            return obj.ParseToEDI<T>(txt);
        }
    }
}
