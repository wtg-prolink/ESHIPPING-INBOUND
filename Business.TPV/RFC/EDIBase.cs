using Business.Service;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.RFC
{
    public class EDIBase
    {
        SAP.Middleware.Connector.RfcDestination _destination;
        protected IRfcFunction GetOperator(string functionCode, Dictionary<string, object> parameters, string location)
        {
            IRfcFunction function = CreateOperator(functionCode, parameters, location);
            function.Invoke(_destination);
            return function;
        }

        protected SAP.Middleware.Connector.RfcDestination Destination
        {
            get
            {
                return _destination;
            }
        }

        public void DsiposeDestination()
        {
            try
            {
                RFC.Manager.DisposeDestinationConfig();
                _destination = null;
            }
            catch (Exception) { }
        }


        IRfcFunction CreateOperator(string functionCode, Dictionary<string, object> parameters, string location, bool ForecastUseFlag = false)
        {
            FactoryCode code = TPV.FactoryCode.FQ;

            if (_destination == null)
            {
                _destination = RFC.Manager.CreateRfcDestination(code, location);
            }
            IRfcFunction function = null;
            try
            {
                function = _destination.Repository.CreateFunction(functionCode);
            }
            catch (Exception ex)
            {
                try
                {
                    _destination = RFC.Manager.CreateRfcDestination(code, location);
                    function = _destination.Repository.CreateFunction(functionCode);
                }
                catch (Exception e)
                {
                    DsiposeDestination();

                    throw new Exception("初始化连线失败:" + e.Message);
                }
            }
            if (function == null) throw new Exception(string.Format("初始化function:{0}失败！", functionCode));
            foreach (var item in parameters)
                function.SetValue(item.Key, item.Value);
            return function;
        }



        protected IRfcFunction GetOperatorForPost(string functionCode, Dictionary<string, object> parameters, string location)
        {
            return CreateOperator(functionCode, parameters, location);
        }

        protected ResultInfo ParseResult(IRfcFunction function)
        {
            try
            {
                string type = Prolink.Math.GetValueAsString(function.GetValue("ERROR_TYPE"));
                if (string.IsNullOrEmpty(type)) return ResultInfo.SucceedResult();
                return new ResultInfo
                {
                    ResultCode = Prolink.Math.GetValueAsString(function.GetValue("ERROR_CODE")),
                    Description = Prolink.Math.GetValueAsString(function.GetValue("ERROR_MESSAGE"))
                };
            }
            catch (Exception ex)
            {
                return ResultInfo.UnknowResult(ex);
            }
        }

        protected ResultInfo ParseEVResult(IRfcFunction function)
        {
            try
            {
                string type = Prolink.Math.GetValueAsString(function.GetValue("EV_TYPE"));
                if (string.IsNullOrEmpty(type)|| "S".Equals(type)) return ResultInfo.SucceedResult();
                return new ResultInfo
                {
                    ResultCode = Prolink.Math.GetValueAsString("ERROR"),
                    Description = Prolink.Math.GetValueAsString(function.GetValue("EV_MESSAGE"))
                };
            }
            catch (Exception ex)
            {
                return ResultInfo.UnknowResult(ex);
            }
        }

        protected virtual FactoryCode FactoryCode
        {
            get
            {
                return TPV.FactoryCode.FQ;
            }
        }
    }

    class EDIInfo
    {

    }

    abstract class EDIBase<T> : EDIBase where T : EDIInfo
    {
        public abstract List<T> Distinct(IEnumerable<T> items);
    }

    class PostSAPData
    {
        public List<IRfcTable> Tables { get; set; }
    }
}
