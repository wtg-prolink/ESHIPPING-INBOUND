using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.Utils
{
    public static class XmlDocUtils
    {
        public static bool TryGetSingleNodeValue(this XmlDocument doc, string name, out string value)
        {
            value = null;
            XmlNodeList nodeList = doc.GetElementsByTagName(name);
            if (nodeList == null || nodeList.Count <= 0) return false;
            value = nodeList[0].InnerText;
            return true;
        }
    }

}
namespace Prolink.DataOperation
{
    public static class EditInstructExtension
    {
        /// <summary>
        /// 合并另一个EditInstructList
        /// </summary>
        /// <param name="eiList"></param>
        /// <param name="list"></param>
        public static void MergeEditInstructList(this EditInstructList eiList, EditInstructList list)
        {
            if (eiList == null) eiList = new EditInstructList();
            if (list != null && list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                    eiList.Add(list[i]);
            }
        }
        public static void MergeEditInstructList(this EditInstructList eiList, IEnumerable<EditInstruct> list)
        {
            if (eiList == null) eiList = new EditInstructList();
            foreach (var item in list)
                eiList.Add(item);
        }
        public static void MergeEditInstruct(this EditInstructList eiList, EditInstruct ei)
        {
            if (ei == null) return;
            eiList.Add(ei);
        }
    }
}
