using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TrackingEDI.Business
{
    public class ExcelParser : BaseParser
    {
        /// <summary>
        /// ExcelParser.RegisterEditInstructFunc(mapping, FUN());
        /// ExcelParser ep=new ExcelParser();
        /// ml.add("del")
        /// ep.save(mapping,filename,ml);
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="filename"></param>
        /// <param name="ml"></param>
        public void Save(string mapping,string filename,MixedList ml,Dictionary<string,object> parm=null, int StartRow=0, int SheetIndex = 0)
        {
            DataTable dt = ExcelHelper.ImportExcelToDataTable(filename, StartRow, SheetIndex);
            ParseEditInstruct(dt, mapping, ml, parm);
        }

        public void Save(DataTable dt, string mapping, string filename, MixedList ml, Dictionary<string, object> parm = null)
        {
            if (dt == null)
                dt = ExcelHelper.ImportExcelToDataTable(filename);
            ParseEditInstruct(dt, mapping, ml, parm);
        }
    }
}
