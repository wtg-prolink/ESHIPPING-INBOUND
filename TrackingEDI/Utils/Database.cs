using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;

using Prolink.Data;
using Prolink.Log;
using Prolink.DataOperation;
using Prolink.Web;
using Prolink.V6.Persistence;

namespace TrackingEDI.Utils
{
	public class Database 
	{
		public static Prolink.Data.DelegateConnection GetConnection() 
		{
            return Prolink.Web.WebContext.GetInstance().GetConnection();
		}

		public static DataTable GetDataTable(string sqlStr,string [] keys) 
		{
            return OperationUtils.GetDataTable(sqlStr, keys, Prolink.Web.WebContext.GetInstance().GetConnection());
		}

		public static DataTable GetDataTable(string sqlStr,string [] keys,int beginRow) 
		{
            return OperationUtils.GetDataTable(sqlStr, keys,GetConnection(), beginRow);
		}

		public static DataTable GetDataTable(string sqlStr,string [] keys,int beginRow,int endRow) 
		{
            return OperationUtils.GetDataTable(sqlStr, keys,GetConnection(), beginRow, endRow);
		}

		public static DataTable GetDataTable(InquiryInstruct ii) 
		{
            return OperationUtils.GetDataTable(ii, GetConnection());
		}

		public static DataTable GetDataTable(InquiryInstruct ii,int beginRow) 
		{
            return OperationUtils.GetDataTable(ii, GetConnection(), beginRow);
		}

		public static DataTable GetDataTable(InquiryInstruct ii,int beginRow,int endRow) 
		{
            return OperationUtils.GetDataTable(ii, GetConnection(), beginRow, endRow);
		}

		public static int[] ExecuteUpdate(EditInstruct ti)
		{
            return OperationUtils.ExecuteUpdate(ti, GetConnection());
		}

		public static int[] ExecuteUpdate(EditInstructList list)
		{
			MixedList ml=new MixedList();
			for (int i=0;i<list.Count;i++) ml.Add(list[i]);
			return ExecuteUpdate(ml);
		}

		public static int[] ExecuteUpdate(MixedList list)
		{
            return OperationUtils.ExecuteUpdate(list, GetConnection());
		}

		public static int[] ExecuteUpdate(string [] list)
		{
            return OperationUtils.ExecuteUpdate(list, GetConnection());
		}

		public static int ExecuteUpdate(SQLInstruct sql)
		{
            return OperationUtils.ExecuteUpdate(sql, GetConnection());
		}

		public static int ExecuteUpdate(string sql)
		{
            return OperationUtils.ExecuteUpdate(sql, GetConnection());
		}

		public static object Execute(Transaction t) 
		{
            return OperationUtils.Execute(t, GetConnection());
		}

		public static int GetValueAsInt(string sql)
		{
			return OperationUtils.GetValueAsInt(sql, GetConnection());
		}
		
		public static double GetValueAsFloat(string sql)
		{
            return OperationUtils.GetValueAsFloat(sql, GetConnection());
		}

		public static string GetValueAsString(string sql)
		{
            return OperationUtils.GetValueAsString(sql, GetConnection());
		}

		public static Logger Logger
		{
			get
			{
				return OperationUtils.Logger;
			}

			set
			{
				OperationUtils.Logger=value;
			}
		}
	}
}
