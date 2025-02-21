using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace NotifyService.Task
{
    public class AutoDeleteSMINPO : IPlanTask
    {
        private string _hour = null;
        private bool flag = true;
        IPlanTaskMessenger _messenger;
        public AutoDeleteSMINPO(string hour)
        {
            _hour = hour;
        }
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            if (IsExecute(_hour, ref flag) && !flag)
            {
                AutoDelete();
            }
        }

        public bool IsExecute(string parmhour, ref bool flag)
        { 
            string[] hours = parmhour.Split(';');

            int hour = DateTime.Now.Hour;

            bool matchhour = false;
            foreach (string index in hours)
            {
                if (string.IsNullOrEmpty(index)) continue;
                if (Prolink.Math.GetValueAsInt(index) == hour)
                    matchhour = true;
            }

            if (matchhour && flag)
            {
                flag = false;
                return true;
            }
            else if (!matchhour && !flag)
            {
                flag = true;
                return false;
            }
            else
                return false;
        }

        public void AutoDelete()
        {
            string date = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd");
            string sql = string.Format(@"DELETE FROM SMINPO WHERE U_FID IS NULL AND CREATE_DATE<{0} ", SQLUtils.QuotedStr(date));
            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
    }
}
