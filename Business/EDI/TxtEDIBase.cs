using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.EDI
{
    public abstract class TxtEDIBase
    {
        public abstract IEnumerable<EDIItem> GetItems();

        public virtual string ToText()
        {
            StringBuilder data = new StringBuilder();
            foreach (var str in GetItemStrs())
            {
                data.Append(str);
            }
            return data.ToString();
        }

        protected IEnumerable<string> GetItemStrs()
        {
            foreach (EDIItem item in GetItems())
            {
                if (item == null) continue;
                yield return FormartText(item);
            }
        }

        protected virtual string FormartText(EDIItem item)
        {
            if (item == null) return string.Empty;
            string txt = string.IsNullOrEmpty(item.Text) ? string.Empty : item.Text.Replace(Environment.NewLine, "").Replace("\t", " ");
            int textLen = txt.Length;
            if (item.Lenght == textLen) return txt;
            if (item.Lenght < textLen) return txt.Substring(0, item.Lenght);
            else
            {
                int blankLen = item.Lenght - textLen;
                StringBuilder blankStr = new StringBuilder(blankLen);
                for (int i = 0; i < blankLen; i++)
                {
                    if (string.IsNullOrEmpty(item.SubstituteCharacter))
                        blankStr.Append(@" ");
                    else
                        blankStr.Append(item.SubstituteCharacter);
                }
                switch (item.SubstituteMode)
                {
                    case SubstituteModes.Left:
                        return string.Format("{0}{1}", blankStr.ToString(), txt);
                    default:
                        return string.Format("{0}{1}", txt, blankStr.ToString());
                }
            }
        }

        public virtual T ParseToEDI<T>(string txt) where T : TxtEDIBase, new()
        {
            return new T();
        }
    }

    public class EDIItem
    {
        public EDIItem()
        {
            
        }

        public EDIItem(int lenght) : this(-1, lenght) { }

        public EDIItem(int index, int lenght)
        {
            this.Index = index;
            this.Lenght = lenght;
        }

        public int Index
        {
            get;
            private set;
        }

        public string SubstituteCharacter
        {
            get;
            set;
        }

        public SubstituteModes SubstituteMode
        {
            get;
            set;
        }

        public int Lenght
        {
            get;
            private set;
        }

        public string Text
        {
            get;
            set;
        }

        public string Caption
        {
            get;
            set;
        }
    }

    public enum SubstituteModes { Right, Left }
}
