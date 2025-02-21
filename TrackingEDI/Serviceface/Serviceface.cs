using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrackingEDI.Serviceface
{
    /// <summary>
    /// 服务管理
    /// 问题单:92554   需求：danny  add by fish 2015-4-27
    /// </summary>
    public class Serviceface
    {
        private static ItraceServiceface _itraceServiceface = null;
        public static ItraceServiceface GetItrace()
        {
            if (_itraceServiceface == null)
                _itraceServiceface = new ItraceServiceface();
            return _itraceServiceface;
        }
    }
}
