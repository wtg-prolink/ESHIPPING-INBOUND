using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.RFC
{
    class DestinationConfiguration : IDestinationConfiguration, IDisposable
    {
        RFCConfig _config;
        public DestinationConfiguration(RFCConfig config)
        {
            _config = config;
            RfcDestinationManager.RegisterDestinationConfiguration(this);
        }

        public bool ChangeEventsSupported()
        {
            return false;
        }

        public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

        public RfcConfigParameters GetParameters(string destinationName)
        {
            if (_config.Parmeters.Count <= 0 || !_config.Parmeters.ContainsKey(destinationName)) throw new Exception("RFC Config not exsit!");
            return _config.Parmeters[destinationName];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RfcDestinationManager.UnregisterDestinationConfiguration(this);
            }
        }
    }
}
