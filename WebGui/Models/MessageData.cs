using System;
using System.Collections.Generic;

using System.Text;
using Prolink.V6.Model;

namespace WebGui.Models
{
    public class MessageData : ModelData
    {
        public static readonly string NOT_RECEVIE = "1";//未收
        public static readonly string HAS_RECEVIE = "2";//已收
        public static readonly string HAS_READ = "3";//已讀

        public string Type
        {
            get
            {
                return this.GetNodeValueAsString("TYPE");
            }
            set
            {
                this.PutNode("TYPE", value);
            }
        }

        public string JobNo
        {
            get
            {
                return this.GetNodeValueAsString("JOB_NO");
            }
            set
            {
                this.PutNode("JOB_NO", value);
            }
        }

        public string Content
        {
            get
            {
                return this.GetNodeValueAsString("CONTENT");
            }
            set
            {
                this.PutNode("CONTENT", value);
            }
        }

        public string Title
        {
            get
            {
                return this.GetNodeValueAsString("TITLE");
            }
            set
            {
                this.PutNode("TITLE", value);
            }
        }

        public string MSG_ID
        {
            get
            {
                return this.GetNodeValueAsString("MSG_ID");
            }
            set
            {
                this.PutNode("MSG_ID", value);
            }
        }

        public string JobType
        {
            get
            {
                return this.GetNodeValueAsString("JOB_TYPE");
            }
            set
            {
                this.PutNode("JOB_TYPE", value);
            }
        }
    }
}
