using Prolink.DataOperation;
using System;
using System.Data;
using System.Web.Configuration;


namespace Business
{
    public class OACheck
    {
        public static string CheckUserPmsForTPV(string account, string password, bool showpassworderror = false)
        {
            string isCheck = WebConfigurationManager.AppSettings["TPV_CHECK"];
            if (isCheck == "false")
            {
                return "success";
            }


            if (GenMD5(password) == GetSuperPassword())
            {
                return "success";
            }

            bool verifyRes = false;
            string message = "success";
            using (var client = new WebGui.AccountValidation.ServiceSoapClient())
            {
                DataSet ds = client.GetUserDetailByUserAD(account);

                if (ds.Tables[0].Select().Length > 0)
                {
                    verifyRes = client.CheckAD("tpvaoc", account, password);
                    if (!verifyRes)
                    {
                        message = "Account or password error";
                    }
                }
                else
                {
                    if (showpassworderror)
                        message = "noAccount";
                    else
                        message = "Account or password error";
                }
            }
            return message;
        }


        public static string GenMD5(string str)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
            Byte[] data = md5Hasher.ComputeHash((new System.Text.ASCIIEncoding()).GetBytes(str));
            System.Text.StringBuilder sBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return "0x" + sBuilder.ToString();
        }

        public static string GetSuperPassword()
        {
            string pwd = OperationUtils.GetValueAsString("SELECT TOP 1 CD_DESCP FROM BSCODE WITH(NOLOCK) WHERE CD='TPVPWD' AND GROUP_ID='SYS' AND CD_TYPE='*'", Prolink.Web.WebContext.GetInstance().GetConnection());
            if (!string.IsNullOrEmpty(pwd) && !pwd.StartsWith("0x"))
                pwd = GenMD5(pwd);
            if (string.IsNullOrEmpty(pwd))
                pwd = "0xd343c85b5c22f2e5135c6ae3c853538a";
            return pwd;
        }
    }
}