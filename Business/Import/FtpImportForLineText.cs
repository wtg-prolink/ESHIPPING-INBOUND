using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Business.Import
{
    public abstract class FtpImportForLineText : FtpImportBaseForConfig
    {
        protected IEnumerable<string> CreateLines(string filePath)
        {
            using (StreamReader reder = new StreamReader(filePath))
            {
                string txt = reder.ReadLine();
                while (!string.IsNullOrEmpty(txt))
                {
                    yield return txt;
                    txt = reder.ReadLine();
                }
            }
        }

        protected Func<string[], int, string> GetValue = (strs, index) =>
            {
                if (strs == null || strs.Length <= 0) return null;
                if (strs.Length <= index) return null;
                return strs[index];
            };
    }
}
