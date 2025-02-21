using System;
using System.Collections.Generic;
using System.Text;
using Prolink.V6.Model;

namespace Prolink.EDIBridge.Model
{
    public class OceanISFData : ModelData
    {
        public override Type GetChildType(string name)
        {
            name = GetClassNameFromName(name);
            if (typeof(OceanISFHouseData).FullName == name) return typeof(OceanISFHouseDataList);
            if (typeof(OceanISFCtnData).FullName == name) return typeof(OceanISFCtnDataList);
            if (typeof(OceanISFPartyData).FullName == name) return typeof(OceanISFPartyDataList);
            if (typeof(OceanISFPartData).FullName == name) return typeof(OceanISFPartDataList);
            return base.GetChildType(name);
        }
    }

    public class OceanISFDataList : ModelList
    {
        public override Type GetChildType(string name)
        {
            return typeof(OceanISFData);
        }
    }

    public class OceanISFHouseData : ModelData
    {
    }

    public class OceanISFHouseDataList : ModelList
    {
        public override Type GetChildType(string name)
        {
            return typeof(OceanISFHouseData);
        }
    }

    public class OceanISFCtnData : ModelData
    {
    }

    public class OceanISFCtnDataList : ModelList
    {
        public override Type GetChildType(string name)
        {
            return typeof(OceanISFCtnData);
        }
    }

    public class OceanISFPartyData : ModelData
    {
    }

    public class OceanISFPartyDataList : ModelList
    {
        public override Type GetChildType(string name)
        {
            return typeof(OceanISFPartyData);
        }
    }

    public class OceanISFPartData : ModelData
    {
    }

    public class OceanISFPartDataList : ModelList
    {
        public override Type GetChildType(string name)
        {
            return typeof(OceanISFPartData);
        }
    }

    //public class OceanISFPaData : ModelData
    //{
    //}

    //public class OceanISFPaDataList : ModelList
    //{
    //    public override Type GetChildType(string name)
    //    {
    //        return typeof(OceanISFPaData);
    //    }
    //}
}
