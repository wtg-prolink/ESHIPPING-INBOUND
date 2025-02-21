using EnterpriseDT.Net.Ftp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Business.Utils
{
    public class FtpConnection : FTPConnection
    {
        public FtpConnection() : base(new FtpClient()) { }

        public FTPFile[] GetFileInfosForLastModifyLongDateTime(string directory)
        {
            //FtpClient ftp = ftpClient as FtpClient;
            //if (ftp == null)
            return GetFileInfos(directory);
            //lock (ftp)
            //{
            //    return ftp.DirDetailsForLastModifyLongDateTime(directory);
            //}
        }
    }

    public class FtpClient : FTPClient
    {
        public FTPFile[] DirDetailsForLastModifyLongDateTime(string dirname)
        {
            FTPFile[] files = DirDetails(dirname);
            if (files == null || files.Length <= 0) return files;
            FieldInfo lastModifiedTimeInfo = typeof(FTPFile).GetField("lastModifiedTime", BindingFlags.NonPublic | BindingFlags.Instance);
            if (lastModifiedTimeInfo == null) return files;
            Dictionary<FTPFile, DateTime> datetimes;
            if (!TryGetLastModifyLongDateTime(dirname, files, out datetimes))
                return files;
            foreach (var item in datetimes)
            {
                lastModifiedTimeInfo.SetValue(item.Key, item.Value);
            }
            return files;
        }

        private bool TryGetLastModifyLongDateTime(string dirname, FTPFile[] files, out Dictionary<FTPFile, DateTime> datetimes)
        {
            FTPDataSocket socket = null;
            datetimes = new Dictionary<FTPFile, DateTime>();
            try
            {
                FieldInfo controlInfo = base.GetType().GetField("control", BindingFlags.NonPublic | BindingFlags.Instance);
                if (controlInfo == null) return false;
                FTPControlSocket control = controlInfo.GetValue(this) as FTPControlSocket;
                if (control == null) return false;
                MethodInfo createDataSocket = typeof(FTPControlSocket).GetMethod("CreateDataSocket", BindingFlags.NonPublic | BindingFlags.Instance);
                if (createDataSocket == null) return false;
                socket = createDataSocket.Invoke(control, new object[] { ConnectMode }) as FTPDataSocket;
                if (socket == null) return false;
                foreach (var item in files)
                {
                    string command = string.Format("mdtm {0}", System.IO.Path.Combine(dirname, item.Name));
                    command = command.Trim();
                    FTPReply reply = control.SendCommand(command);
                    DateTime time;
                    if (!DateTime.TryParseExact(reply.ReplyText, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out time)) return false;
                    datetimes.Add(item, time);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (socket != null)
                {
                    try
                    {
                        MethodInfo closeInfo = typeof(FTPDataSocket).GetMethod("Close", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (closeInfo != null)
                        {
                            closeInfo.Invoke(socket, new object[] { });
                            socket = null;
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
