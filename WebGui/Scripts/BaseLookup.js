//_getLang("L_BaseLookup_GroupList","Group List");
var LookUpConfig = {};

LookUpConfig.SiteLookup = {
    caption: _getLang("L_BaseLookup_GroupList", "Group List"),
    refresh: false,
    //之前columns中使用的fieldName，因后面直接使用此作为model，所以导致查询结果为空，所以将fieldName修改为name
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.SiteLookup1 = {
    caption: _getLang("L_BaseLookup_GroupList", "Group List"),
    refresh: false,
    //之前columns中使用的fieldName，因后面直接使用此作为model，所以导致查询结果为空，所以将fieldName修改为name
    columns: [{ name: "Cmp", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Name", title: "Name", width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.SiteLookup3 = {
    caption: "Location List",
    sortname: "Cd",
    sortorder: "asc",
    refresh: false,
    multiselect: true,
    columns: [{ name: "Cd", title: "Code", width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 500, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.ApproveDLookup = {
    caption: _getLang("L_BaseLookup_ApproveRList", "Approve Role List"),
    sortname: "ApproveGroup",
    sortorder: "asc",
    refresh: false,
    //之前columns中使用的fieldName，因后面直接使用此作为model，所以导致查询结果为空，所以将fieldName修改为name
    columns: [{ name: "ApproveGroup", title: _getLang("L_BaseLookup_RoleID", "Role ID"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "GroupDescp", title: _getLang("L_BaseLookup_RoleDescp", "Role Descp."), width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "SeniorStaff", title: _getLang("L_SeniorStaff", "是否高管"), width: 140, sorttype: "string", hidden: true, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "UId", title: "UId", width: 80, sorttype: "string", hidden: true, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
    //{ name: "Cmp", title: "公司", width: 100, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni']  },
    //{ name: "Stn", title: "站别", width: 100, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni']  }]
}

LookUpConfig.RoleLookup = {
    caption: "Role Search",
    sortname: "Fid",
    refresh: false,
    //之前columns中使用的fieldName，因后面直接使用此作为model，所以导致查询结果为空，所以将fieldName修改为name
    columns: [{ name: "Fid", title: _getLang("L_BaseLookup_RoleID", "Role ID"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Fdescp", title: _getLang("L_BSLCPOL_PolDescp", "Description"), width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "GroupId", title: _getLang("L_SYS_GROUP", "Group"), width: 80, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Cmp", title: _getLang("L_MailGroupSetup_Cmp", "Company"), width: 100, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Stn", title: _getLang("L_IpPart_Stn", "Station"), width: 100, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.NoticeRoleLookup = {
    caption: "Role Search",
    sortname: "Fid",
    refresh: false,
    multiselect: true,
    //之前columns中使用的fieldName，因后面直接使用此作为model，所以导致查询结果为空，所以将fieldName修改为name
    columns: [{ name: "Fid", title: _getLang("L_BaseLookup_RoleID", "Role ID"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Fdescp", title: _getLang("L_BaseLookup_RoleDescp", "Role Descp."), width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "GroupId", title: _getLang("L_SYS_GROUP", "Group"), width: 80, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Cmp", title: _getLang("L_MailGroupSetup_Cmp", "Company"), width: 100, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Stn", title: _getLang("L_IpPart_Stn", "Station"), width: 100, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.BlNoLookup = {
    caption: "BlNo Search",
    sortname: "BlNo",
    refresh: false,
    //之前columns中使用的fieldName，因后面直接使用此作为model，所以导致查询结果为空，所以将fieldName修改为name
    columns: [{ name: "WiNo", title: _getLang("L_BaseLookup_WiNo", "In Stock No"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "WoNo", title: _getLang("L_BaseLookup_WoNo", "Ex-Warehouse No"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "BlNo", title: _getLang("L_ActSetup_BlNo", "B/L#"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Gw", title: _getLang("L_BaseLookup_AcWoNo", "Actual In Stock No"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Egw", title: _getLang("L_BaseLookup_PreWoNo", "Estimated Weight of delivery"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.CntabLookup = {
    caption: "CtnNo Search",
    sortname: "CtnNo",
    refresh: false,
    //之前columns中使用的fieldName，因后面直接使用此作为model，所以导致查询结果为空，所以将fieldName修改为name
    columns: [{ name: "CtnNo", title: _getLang("L_BaseLookup_CtnNo", "Container No"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "BlNo", title: _getLang("L_ActSetup_BlNo", "B/L#"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Gw", title: _getLang("L_BaseLookup_CtnGw", "Containrt GW"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CtnType", title: _getLang("L_BaseLookup_CtnType", "Container Type"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.GroupLookup = {
    caption: "Group Search",
    sortname: "GroupId",
    refresh: false,
    //之前columns中使用的fieldName，因后面直接使用此作为model，所以导致查询结果为空，所以将fieldName修改为name
    columns: [{ name: "GroupId", title: _getLang("L_GroupRelation_groupID", "Group ID"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Name", title: _getLang("L_BaseLookup_GroupName", "Group Name"), width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Attn", title: _getLang("L_GroupRelation_comContact", "Contact"), width: 80, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "AttnTel", title: _getLang("L_UserSetUp_UEmail", "Email"), width: 100, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.CompanyLookup = {
    caption: "Company Search",
    sortname: "PortCode,PortType",
    refresh: false,
    columns: [{ fieldName: "CtryCode", title: "Ctry Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { fieldName: "PortName", title: "Port Name", width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { fieldName: "PortType", title: "Port Type", width: 80, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { fieldName: "PortCode", title: "Port Code", width: 100, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//department search
LookUpConfig.DepLookup = {
    caption: _getLang("L_BaseLookup_SerDep", "Search Group"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: "Department Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Department Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.BuLookup = {
    caption: _getLang("L_BaseLookup_SerSor", "Search Sort"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.LuLookup = {
    caption: _getLang("L_BaseLookup_SerDelv", "Search Delivery"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.CurLookup = {
    caption: _getLang("L_BaseLookup_SerCur", "Search Currency"),
    sortname: "Cur",
    refresh: false,
    columns: [{ name: "Cur", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CurDescp", title: "Description", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.CoLookup = {
    caption: _getLang("L_BaseLookup_SerItem", "Search Item"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.BankLookup = {
    caption: _getLang("L_BaseLookup_SerBank", "Search Bank"),
    sortname: "BankCd",
    refresh: false,
    columns: [{ name: "BankCd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "BankNm", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//供应商查询
LookUpConfig.BSCSVLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search"),
    sortname: "CustCd",
    sortorder: "desc",
    refresh: false,
    columns: [{ name: "CustCd", title: _getLang("L_BSCODESetup_Cd", "Party Code"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    // { name: "CntyCd", title: "国家代码", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    // { name: "CityCd", title: "港口代码", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalNm", title: "Name", width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalAddr", title: _getLang("L_BSCSDataQuery_PartAddr", "Address"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngNm", title: _getLang("L_GroupRelation_depEName", "Name in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngAddr", title: _getLang("L_GroupRelation_depEaddr", "Address in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CusBank", title: _getLang("L_BaseLookup_CusBank", "Opening Bank"), width: 50, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CusNo", title: _getLang("L_BaseLookup_CusNo", "Account"), width: 50, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "RarNo", title: "SWIFT", width: 50, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "TaxNo", title: _getLang("L_BaseLookup_TaxNo", "Tax No"), width: 50, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    ]
}

//供应商查询
LookUpConfig.BSCSSaleLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search"),
    sortname: "a.CustCd",
    sortorder: "desc",
    refresh: false,
    columns: [{ name: "CustCd", title: _getLang("L_BSCODESetup_Cd", "Party Code"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    // { name: "CntyCd", title: "国家代码", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    // { name: "CityCd", title: "港口代码", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalNm", title: "Name", width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalAddr", title: _getLang("L_BSCSDataQuery_PartAddr", "Address"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngNm", title: _getLang("L_GroupRelation_depEName", "Name in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngAddr", title: _getLang("L_GroupRelation_depEaddr", "Address in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}



//供应商查询
LookUpConfig.SupplierVLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search"),
    sortname: "CustCd",
    sortorder: "desc",
    refresh: false,
    columns: [{ name: "CustCd", title: _getLang("L_BSCODESetup_Cd", "Party Code"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    // { name: "CntyCd", title: "国家代码", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    // { name: "CityCd", title: "港口代码", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalNm", title: "Name", width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalAddr", title: _getLang("L_BSCSDataQuery_PartAddr", "Address"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngNm", title: _getLang("L_GroupRelation_depEName", "Name in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngAddr", title: _getLang("L_GroupRelation_depEaddr", "Address in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//銷售對象查询
LookUpConfig.BSCSDPLookup = {
    caption: _getLang("L_BaseLookup_SerSal", "Search Sales Target"),
    refresh: false,
    columns: [{ name: "CustCd", title: _getLang("L_BSCODESetup_Cd", "Party Code"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    // { name: "CntyCd", title: "国家代码", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    // { name: "CityCd", title: "港口代码", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalNm", title: "Name", width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalAddr", title: _getLang("L_BSCSSetup_CmpAddr", "Address"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngNm", title: _getLang("L_GroupRelation_depEName", "Name in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngAddr", title: _getLang("L_GroupRelation_depEaddr", "Address in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CustAttn", title: _getLang("L_BSCSSetup_CmpAttn", "Attn."), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CustTel", title: _getLang("L_BSCSSetup_CmpTel", "Tel No."), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "Email", title: _getLang("L_BSCSSetup_CmpEmail", "eMail Addr."), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.BSCSCarrierLookup = {
    caption: _getLang("L_BaseLookup_SerVessel", "Search Vessal"),
    refresh: false,
    columns: [{ name: "ScacCode", title: "Carrier Code", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalNm", title: "Name", width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalAddr", title: _getLang("L_BSCSDataQuery_PartAddr", "Address"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngNm", title: _getLang("L_GroupRelation_depEName", "Name in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngAddr", title: _getLang("L_GroupRelation_depEaddr", "Address in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CustAttn", title: _getLang("L_BSCSSetup_CmpAttn", "Attn."), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CustTel", title: _getLang("L_BSCSSetup_CmpTel", "Tel No."), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "Email", title: _getLang("L_BSCSDataQuery_PartyMail", "eMail Addr."), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//銷售對象查询
LookUpConfig.CustMLookup = {
    caption: _getLang("L_BaseLookup_SerSal", "Search Sales Target"),
    refresh: false,
    columns: [{ name: "CustCd", title: _getLang("L_BSCODESetup_Cd", "Party Code"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalNm", title: _getLang("L_GroupRelation_stnCName", "Name in Chinese"), width: 180, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalAddr", title: _getLang("L_GroupRelation_depCaddr", "Address in Chinese"), width: 320, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngNm", title: _getLang("L_GroupRelation_depEName", "Name in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngAddr", title: _getLang("L_GroupRelation_depEaddr", "Address in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CustAttn", title: _getLang("L_BSCSSetup_CmpAttn", "Attn."), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CustTel", title: _getLang("L_BSCSSetup_CmpTel", "Tel No."), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "Email", title: _getLang("L_BSCSDataQuery_PartyMail", "eMail Addr."), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "SalesCd", title: _getLang("L_BaseLookup_SalesCd", "User ID"), width: 300, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "SalesNm", title: _getLang("L_BaseLookup_SalesNm", "User Name"), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CntyCd", title: _getLang("L_CitySetup_CntryCd", "Country Code"), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CityCd", title: _getLang("L_TpvPortQuery_Port", "Port Code"), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }
    ]
}

//供应商查询
LookUpConfig.BSCSMLookup = {
    caption: _getLang("L_BaseLookup_SerClient", "Search Client"),
    sortname: "CustCd",
    refresh: false,
    columns: [{ name: "CustCd", title: _getLang("L_BSCSSetup_Cmp", "Company Code"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalNm", title: _getLang("L_ForecastQueryData_CustNm", "Customer Name"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalAddr", title: _getLang("L_BaseLookup_ClientAddr", "Customer Addr"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngNm", title: _getLang("L_BaseLookup_ClientENname", "Customer English name"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngAddr", title: _getLang("L_BaseLookup_ClientAddrEN", "Customer English Addr"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//付款單位查询
LookUpConfig.BSCSMPayunitLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search"),
    sortname: "CustCd",
    refresh: false,
    columns: [{ name: "CustCd", title: _getLang("L_BaseLookup_PayerCode", "Payment unit code"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalNm", title: _getLang("L_BaseLookup_PayerNM", "Payment unit Name"), Pol1Lookup: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalAddr", title: _getLang("L_BaseLookup_PayerAddr", "Pay unit address"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngNm", title: _getLang("L_BaseLookup_PayerNMEN", "Pay unit English Name"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngAddr", title: _getLang("L_BaseLookup_PayerAddrEN", "Pay unit English address"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//外檢查询
LookUpConfig.BSCSLLookup = {
    caption: _getLang("L_BaseLookup_SerExternal", "Search External Examination"),
    refresh: false,
    columns: [{ name: "CustCd", title: _getLang("L_BaseLookup_Ext", "External Examination") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalNm", title: _getLang("L_BaseLookup_Ext", "External Examination") + _getLang("L_BaseLookup_Nm", "Name"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CustAttn", title: _getLang("L_BSCSSetup_CmpAttn", "Attn."), width: 300, dt: "B", sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CustTel", title: _getLang("L_BSCSSetup_CmpTel", "Tel No."), width: 300, dt: "B", sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalAddr", title: _getLang("L_BaseLookup_Ext", "External Examination") + _getLang("L_BSCSDataQuery_PartAddr", "Address"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngNm", title: _getLang("L_BaseLookup_Ext", "External Examination") + _getLang("L_GroupRelation_comEName", "Name in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngAddr", title: _getLang("L_BaseLookup_Ext", "External Examination") + _getLang("L_GroupRelation_comEaddr", "Address in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//落箱查询
LookUpConfig.BSCSILookup = {
    caption: _getLang("L_BaseLookup_SerDrop", "Search Container Fee"),
    sortname: "CustCd",
    refresh: false,
    columns: [{ name: "CustCd", title: _getLang("L_BaseLookup_Drop", "Drop Container") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalNm", title: _getLang("L_BaseLookup_Drop", "Drop Container") + _getLang("L_BaseLookup_Nm", "Name"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalAddr", title: _getLang("L_BaseLookup_Drop", "Drop Container") + _getLang("L_BSCSDataQuery_PartAddr", "Address"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngNm", title: _getLang("L_BaseLookup_Drop", "Drop Container") + _getLang("L_GroupRelation_comEName", "Name in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngAddr", title: _getLang("L_BaseLookup_Drop", "Drop Container") + _getLang("L_GroupRelation_comEaddr", "Address in English"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//科目查询
LookUpConfig.BSCHGLookup = {
    caption: _getLang("L_BaseLookup_SerAccount", "Search Account"),
    sortname: "ChgCd",
    refresh: false,
    columns: [{ name: "ChgCd", title: _getLang("L_BaseLookup_Account", "Account") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "ChgCnm", title: _getLang("L_BaseLookup_Account", "Account") + _getLang("L_BaseLookup_Nm", "Name"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//貿易條款查询
LookUpConfig.BSCodeLookup = {
    caption: _getLang("L_BaseLookup_SerTerm", "Search Incoterm"),
    sortname: "Cd",
    sortorder: "asc",
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 500, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//EDOC TYPE
LookUpConfig.EdocTypeLookup = {
    caption: "EDOC TYPE",
    sortname: "Cd",
    sortorder: "asc",
    refresh: false,
    multiselect: true,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 500, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.MutiBSCodeLookup = {
    caption: "TCMP",
    sortname: "Cd",
    sortorder: "asc",
    refresh: false,
    multiselect: true,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 500, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//交易所查询
LookUpConfig.BSCodeTreLookup = {
    caption: _getLang("L_BaseLookup_SerExchange", "Search Exchange"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//簽核流程代碼查询
LookUpConfig.BSCodeAprLookup = {
    caption: _getLang("L_BaseLookup_SerAprProCd", "Approve Process Code"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//数量单位查询
LookUpConfig.UnitLookup = {
    caption: _getLang("L_BaseLookup_SerQTY", "Search Quantity"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: "Unit Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Unit Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//重量单位查询
LookUpConfig.NwuLookup = {
    caption: _getLang("L_BaseLookup_SerNw", "Search Nw Unit"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: "Weight Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Weight Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//货物名称查询
LookUpConfig.GoodsLookup = {
    caption: _getLang("L_BaseLookup_SerCarNm", "Search Cargo Name"),
    sortname: "Goods",
    refresh: false,
    columns: [{ name: "Goods", title: _getLang("L_BaseLookup_GoodsNo", "Goods No"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "GoodsDescp", title: _getLang("L_BaseLookup_GoodsDescp", "Goods Descp."), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'cn', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "HsCode", title: "HS Code", width: 130, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//员工查询
LookUpConfig.UserLookup = {
    caption: _getLang("L_BaseLookup_SerStaff", "Search Staff"),
    sortname: "UId",
    refresh: false,
    columns: [{ name: "Cmp", title: "Location", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "UId", title: "Id", width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "UName", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "UEmail", title: "Email", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//调度人查询
LookUpConfig.EmpLookup = {
    caption: _getLang("L_BaseLookup_SerEmp", "Search Dispatcher"),
    sortname: "EmpId",
    refresh: false,
    columns: [{ name: "EmpId", title: _getLang("L_BaseLookup_EmpId", "Dispatcher ID"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EmpCnm", title: _getLang("L_BaseLookup_Emp", "Dispatcher"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//员工查询
LookUpConfig.MultiUserLookup = {
    caption: _getLang("L_BaseLookup_SerStaff", "Search Staff"),
    sortname: "UId",
    refresh: false,
    multiselect: true,
    columns: [{ name: "UId", title: _getLang("L_BaseLookup_Emp", "Dispatcher") + "Id", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "UName", title: _getLang("L_BaseLookup_Emp", "Dispatcher") + _getLang("L_BaseLookup_Nm", "Name"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "UPhone", title: _getLang("L_BaseLookup_Emp", "Dispatcher") + _getLang("L_BSCSSetup_CmpTel", "Tel No."), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "UEmail", title: _getLang("L_BaseLookup_Emp", "Dispatcher") + _getLang("L_BSCSSetup_CmpEmail", "eMail Addr."), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//国家查询
LookUpConfig.CntryLookup = {
    caption: _getLang("L_BaseLookup_SerCnt", "Search Container"),
    sortname: "CntryCd",
    refresh: false,
    columns: [{ name: "CntryCd", title: _getLang("L_BaseLookup_Country", "Country") + " ID", width: 80, init: true, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CntryNm", title: _getLang("L_CntySetup_CntryNm", "Country Name"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//港口查询
LookUpConfig.CityLookup = {
    caption: _getLang("L_BaseLookup_SerPort", "Search Port"),
    sortname: "CityCd",
    refresh: false,
    columns: [
        { name: "CntyCd", title: _getLang("L_BaseLookup_Country", "Country"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "CityCd", title: "Id", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "CityNm", title: "Name", init: true, width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//TRACKING 城市查询
LookUpConfig.BSCITYLookup = {
    caption: _getLang("L_BaseLookup_SerPort", "Search Port"),
    sortname: "CntryCd",
    refresh: false,
    columns: [
        { name: "CntryCd", title: _getLang("L_BaseLookup_Country", "Country"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PortCd", title: "Id", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PortNm", title: "Name", init: true, width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//客户建档委托人查询
LookUpConfig.BSCSELookup = {
    caption: _getLang("L_BaseLookup_SerClient", "Search Client"),
    sortname: "CustCd",
    refresh: false,
    columns: [{ name: "CustCd", title: _getLang("L_BaseLookup_SerClient", "Search Client") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalNm", title: _getLang("L_BaseLookup_SerClient", "Search Client") + _getLang("L_BaseLookup_Nm", "Name"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalAddr", title: _getLang("L_BaseLookup_SerClient", "Search Client") + _getLang("L_BSCSDataQuery_PartAddr", "Address"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//仓库查询
LookUpConfig.BSCSWLookup = {
    caption: _getLang("L_BaseLookup_SerWHouse", "Search Warehouse"),
    sortname: "CustCd",
    refresh: false,
    columns: [{ name: "CustCd", title: _getLang("L_BaseLookup_Warehouse", "Warehouse") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalNm", title: _getLang("L_BaseLookup_Warehouse", "Warehouse") + _getLang("L_BaseLookup_Nm", "Name"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//物流反馈
LookUpConfig.LBLookup = {
    caption: _getLang("L_BaseLookup_SerLogFeb", "Logistics feedback"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//客户反馈
LookUpConfig.SBLookup = {
    caption: _getLang("L_BaseLookup_SerCusFeb", "Customer feedback"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//料號查詢
LookUpConfig.PartLookup = {
    caption: "Part Search",
    sortname: "PartNo",
    multiSort: false,
    refresh: false,
    exportexcel: true,
    multiSort: true,
    columns: [
        { name: 'SupplierCd', title: _getLang('L_IpPart_SupplierCd', 'Supplier Code'), width: 120, align: 'center', sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PartNo', title: _getLang('L_IpPart_PartNo', 'No'), width: 140, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Cdescp', title: _getLang('L_BaseLookup_CnCarNm', 'Goods Description of Chinese'), width: 140, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Descp', title: _getLang('L_BaseLookup_EngCarNm', 'Eng. Cargo Name'), width: 350, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'MafNo', title: _getLang('L_BaseLookup_FacNo', 'Factory Code'), width: 150, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PartType', title: _getLang('L_AddBulletin_BullType', 'Type'), width: 70, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PartOth', title: _getLang('L_IpPart_PartOth', 'Others'), width: 70, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PartCd', title: _getLang('L_IpPart_PartCd', 'Code'), width: 70, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PartPkg', title: _getLang('L_IpPart_PartPkg', 'Package'), width: 70, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'SupplierNm', title: _getLang('L_IpPart_SupplierCd', 'Supplier Code') + _getLang('L_BaseLookup_Nm', 'Name'), width: 250, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Cntry', title: _getLang('L_IpPart_Cntry', 'Country Name'), width: 70, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Goods', title: _getLang('L_IpPart_Goods', '???非e-Shipping模块？'), width: 100, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'GoodsDescp', title: _getLang('L_BaseLookup_Goods', 'commodity description') + _getLang('L_BaseLookup_Nm', 'Name'), width: 250, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'GroupId', title: _getLang('L_SYS_GROUP', 'Group'), width: 70, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.BSCSSetupLookup = {
    caption: "BSCSSetup Search",
    sortname: "UId",
    multiSort: false,
    refresh: false,
    exportexcel: true,
    multiSort: true,
    columns: [
        { name: 'PartyNo', title: 'PartyNo', width: 150, align: 'center', sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'GroupId', title: _getLang('L_SYS_GROUP', 'Group'), width: 150, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Cmp', title: _getLang('L_MailGroupSetup_Cmp', 'Company'), width: 150, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Stn', title: _getLang('L_IpPart_Stn', 'Station'), width: 150, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//汇率建档查询
LookUpConfig.ExchangeRateSetupLookup = {
    caption: "ExchangeRateSetup Search",
    sortname: "Etype",
    multiSort: false,
    refresh: false,
    exportexcel: true,
    multiSort: true,
    columns: [
        { name: 'Etype', title: _getLang('L_ExchangeRate_Etype', 'Type'), width: 150, align: 'center', sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Edate', title: _getLang('L_ExchangeRate_Edate', 'Date'), width: 150, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Fcur', title: _getLang('L_ExchangeRate_Fcur', 'From'), width: 150, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Tcur', title: _getLang('L_ExchangeRate_Tcur', 'To'), width: 150, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'ExRate', title: _getLang('L_ExchangeRate_ExRate', 'Exchange Rate'), width: 150, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Remark', title: _getLang('L_BSCSSetup_Remark', 'Remark'), width: 350, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//城市建档查询
LookUpConfig.CitySetupLookup = {
    caption: "CitySetup Search",
    sortname: "PortCd",
    multiSort: false,
    refresh: false,
    exportexcel: true,
    multiSort: true,
    columns: [
        { name: 'AsType', title: _getLang('L_AirSetup_QuotType', 'Category'), width: 130, align: 'center', sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PortCd', title: _getLang('L_CitySetup_PortCd', 'City Code'), width: 130, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PortNm', title: _getLang('L_CitySetup_PortNm', 'City Name'), width: 130, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'CntryCd', title: _getLang('L_CitySetup_CntryCd', 'Country Code'), width: 130, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'CntryName', title: _getLang('L_CntySetup_CntryNm', 'Country Name'), width: 130, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Gm', title: _getLang('L_CitySetup_Gm', 'Time Zone'), width: 130, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Region', title: _getLang('L_BaseLookup_Region', 'Region'), width: 130, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Ns', title: _getLang('L_CitySetup_Ns', 'Longitude'), width: 130, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Ew', title: _getLang('L_CitySetup_Ew', 'Latitude'), width: 130, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//region
LookUpConfig.StatePortLookup = {
    caption: "region Search",
    sortname: "State",
    multiSort: false,
    refresh: false,
    exportexcel: true,
    multiSort: true,
    columns: [
        { name: 'CntryCd', title: _getLang('L_CitySetup_CntryCd', 'Country Code'), width: 160, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'State', title: _getLang('L_BaseLookup_State', 'State'), width: 160, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.TruckPortCdLookup = {
    caption: "TruckPort Search",
    sortname: "PortCd",
    multiSort: false,
    refresh: false,
    exportexcel: true,
    multiSort: true,
    columns: [
        { name: 'CntryCd', title: 'Country Code', init: true, width: 60, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PortCd', title: _getLang('L_BSCSSetup_City', 'City Name') + _getLang('L_BSCODESetup_Cd', 'Party Code'), width: 60, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PortNm', title: _getLang('L_BSCSSetup_City', 'City Name') + _getLang('L_BaseLookup_Nm', 'Name'), width: 160, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'State', title: _getLang('L_BaseLookup_State', 'State'), width: 60, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Region', title: _getLang('L_BaseLookup_Region', 'Region'), width: 60, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Cmp', title: 'CMP', init: true, width: 60, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

LookUpConfig.TruckPortAddrLookup = {
    caption: "Address Search",
    sortname: "AddrCode",
    multiSort: false,
    refresh: false,
    exportexcel: false,
    multiSort: true,
    columns: [
        { name: 'AddrCode', title: 'Address Code', width: 100, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Addr', title: 'Address', width: 160, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'WsCd', title: 'Warehouse code', width: 120, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'WsNm', title: 'Warehouse Name', width: 160, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'FinalWh', title: 'Final WH', width: 60, editable: false, hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

//採購單號
LookUpConfig.pomLookup = {
    caption: _getLang("L_BaseLookup_SerPoInfo", "Search Purchase Order Info"),
    //sortname: "Cd",
    refresh: false,
    columns: [{ name: "PoNo", title: _getLang("L_BaseLookup_PoNo", "Purchase Order No"), width: 160, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "GoodsCd", title: _getLang("L_BaseLookup_GoodsCd", "Goods Code"), width: 80, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "GoodsNm", title: _getLang("L_BaseLookup_GoodsNm", "Goods Name"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CustNm", title: _getLang("L_BaseLookup_Customer", "Customer"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CtNo", title: _getLang("L_BaseLookup_CtNo", "Contract No"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },

    //{ name: "PartNo", title: "料号", width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni']  },
    //{ name: "Descp", title: "Descp", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni']  },
    { name: "Wqty", title: "Wqty", width: 80, align: "right", sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Bqty", title: "Bqty", width: 80, align: "right", sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CntNo", title: _getLang("L_BaseLookup_CtnNo", "Container No"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Qty", title: _getLang("L_ProSts_QTY", "Quantity"), width: 80, align: "right", sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CustCd", title: _getLang("L_BaseLookup_Client", "Client") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 80, sorttype: "string", hidden: true, viewable: true },
    { name: "MafNo", title: _getLang("L_BaseLookup_FacNo", "Factory Code"), width: 80, sorttype: "string", hidden: false, viewable: true },
    { name: "Bamt", title: "Bamt", width: 80, sorttype: "string", hidden: true, viewable: true },
    { name: "Nw", title: _getLang("L_BaseLookup_Nw", "Net weight"), width: 80, sorttype: "string", hidden: false, viewable: true },
    { name: "Nwu", title: _getLang("L_BaseLookup_Unit", "Unit"), width: 80, sorttype: "string", hidden: false, viewable: true },
    { name: "Cur", title: _getLang("L_IpPart_Crncy", "Currency"), width: 80, sorttype: "string", hidden: false, viewable: true },
    { name: "ExRate", title: _getLang("L_ActSetup_ExRate", "EX-Rate"), width: 80, sorttype: "string", hidden: false, viewable: true },
    {
        name: "PoAmt", title: _getLang("L_BaseLookup_Amt", "Amount"), width: 80, sorttype: "float", hidden: false, viewable: true, formatter: "number",
        formatoptions: {
            decimalSeparator: ".",
            thousandsSeparator: ",",
            decimalPlaces: 2,
            defaultValue: '0.00'
        }
    },
    { name: "Remark", title: _getLang("L_BSCSSetup_Remark", "Remark"), width: 150, sorttype: "string", hidden: false, viewable: true },
    { name: "UId", title: "UId", width: 80, sorttype: "string", hidden: true, viewable: true },]
}

//交單單據
LookUpConfig.DocRmkLookup = {
    caption: _getLang("L_BaseLookup_SerBlRecp", "Search Bill Receipt"),
    sortname: "Cd",
    refresh: false,
    multiselect: true,
    columns: [{ name: "Cd", title: _getLang("L_BaseLookup_BlRecpCd", "Bill Receipt Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: _getLang("L_BaseLookup_RecpNm", "Receipt Name"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//批文查询
LookUpConfig.IpbtmLookup = {
    caption: _getLang("L_BaseLookup_SerApproval", "Search Approval"),
    sortname: "BtNo",
    refresh: false,
    columns: [{ name: 'BtNo', title: _getLang('L_BaseLookup_BtNo', 'Approval No'), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'BtDate', title: _getLang('L_BaseLookup_BtDate', 'Approval Date'), width: 150, align: 'center', formatter: 'date', formatoptions: { newformat: 'd-M-Y' }, datefmt: 'dd-M-yy' },
    { name: "Goods", title: _getLang("L_BaseLookup_GoodA", "commodity"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "GoodsDescp", title: "Name", width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Cntry", title: _getLang("L_BSCSDataQuery_Cnty", "Country"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CntryNm", title: _getLang("L_BSCSDataQuery_CntyNm", "Country Name"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "DepositQty", title: _getLang("L_BaseLookup_DepositQty", "Deposit Quantity"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Qtyu", title: _getLang("L_BaseLookup_Unit", "Unit"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "BatCmp", title: _getLang("L_BaseLookup_BatCmp", "Apply Company"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Remark", title: _getLang("L_BSCSSetup_Remark", "Remark"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

//合同查询
LookUpConfig.IpctmLookup = {
    caption: _getLang("L_BaseLookup_SerCt", "Search Contract"),
    sortname: "CtNo",
    refresh: false,
    columns: [{ name: "CtNo", title: _getLang("L_BaseLookup_CtNo", "Contract No"), width: 170, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "SctNo", title: _getLang("L_BaseLookup_Supplier", "Supplier") + _getLang("L_BaseLookup_CtNo", "Contract No"), width: 170, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "MafNo", title: _getLang("L_BaseLookup_FacNo", "Factory Code"), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Term", title: _getLang("L_LogisticsRule_Term", "Incoterm"), width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Pod", title: _getLang("L_LogisticsRule_Pod", "Port of Discharge"), width: 60, hidden: false },
    { name: "GroupId", title: _getLang("L_SYS_GROUP", "Group"), width: 80, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Cmp", title: _getLang("L_MailGroupSetup_Cmp", "Company"), width: 80, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Stn", title: _getLang("L_IpPart_Stn", "Station"), width: 80, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Dep", title: _getLang("L_UserSetUp_Dep", "Department"), width: 80, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Bu", title: _getLang("L_IpPart_Bu", "???非e-Shipping模块？"), width: 80, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PoDate", title: _getLang("L_BaseLookup_CtDate", "Contract Date"), width: 100, sorttype: "date", formatter: "date", formatoptions: { srcformat: 'Y-m-d', newformat: 'Y-m-d' }, hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "SupplierCd", title: _getLang("L_IpPart_SupplierCd", "Supplier Code"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "SupplierNm", title: _getLang("L_IpPart_SupplierCd", "Supplier Code") + _getLang("L_BaseLookup_Nm", "Name"), width: 220, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CustCd", title: _getLang("L_BaseLookup_Customer", "Customer"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CustNm", title: _getLang("L_BaseLookup_Customer", "Customer") + _getLang("L_BaseLookup_Nm", "Name"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "ContactQty", title: _getLang("L_DNApproveManage_Qty", "Quantity"), width: 80, sorttype: "int", align: "right", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "ContactQtyu", title: _getLang("L_BaseLookup_Qtyu", "Unit"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "MType", title: "MType", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "GoodsCd", title: _getLang("L_BaseLookup_GoodA", "commodity") + _getLang("L_BSCODESetup_Cd", "Party Code"), hidden: true },
    { name: "GoodsNm", title: _getLang("L_BaseLookup_GoodsName", "Goods Name"), hidden: true },
    { name: "CntryCd", title: _getLang("L_BSCSDataQuery_Cnty", "Country") + _getLang("L_BSCODESetup_Cd", "Party Code"), hidden: true },
    { name: "CntryNm", title: _getLang("L_BSCSDataQuery_CntyNm", "Country Name"), hidden: true },
    { name: "Pol", title: _getLang("L_LogisticsRuleSetup_Pol", "Port of Loading"), hidden: true },
    { name: "Cur", title: "Cur", hidden: true },
    { name: "Term", title: "Term", hidden: true },
    { name: "TermLoc", title: "TermLoc", hidden: true },
    { name: "CargoFinal", title: "CargoFinal", hidden: true },
    { name: "FinalDay", title: "FinalDay", hidden: true },
    { name: "MafNo", title: "MafNo", hidden: true },
    { name: "LcNo", title: "LcNo", hidden: true },
    { name: "PolDescp", title: "PolDescp", hidden: true },
    { name: "PodDescp", title: "PodDescp", hidden: true },
    { name: "ContactCd", title: "ContactCd", hidden: true },
    { name: "ContactNm", title: "ContactNm", hidden: true }]
}


//批文查询不含批文號碼與日期
LookUpConfig.IpbtmGoodsLookup = {
    caption: _getLang("L_BaseLookup_SerApproval", "Search Approval"),
    sortname: "Goods",
    refresh: false,
    columns: [
        { name: "Goods", title: _getLang("L_BaseLookup_GoodsCode", "Goods Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "GoodsDescp", title: "Name", width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Cntry", title: _getLang("L_BSCSDataQuery_Cnty", "Country"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "CntryNm", title: _getLang("L_BSCSDataQuery_CntyNm", "Country Name"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "DepositQty", title: _getLang("L_BaseLookup_DepositQty", "Deposit Quantity"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Qtyu", title: _getLang("L_BaseLookup_Unit", "Unit"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "BatCmp", title: _getLang("L_BaseLookup_BatCmp", "Apply Company"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Remark", title: _getLang("L_BSCSSetup_Remark", "Remark"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

//箱型查询
LookUpConfig.CntLookup = {
    caption: _getLang("L_BaseLookup_SerCnt", "Search Container"),
    sortname: "CntType",
    refresh: false,
    columns: [{ name: "CntType", title: "CntType", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CntEdescp", title: "CntEdescp", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CntCdescp", title: "CntCdescp", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//金屬查詢
LookUpConfig.ElmtLookup = {
    caption: _getLang("L_BaseLookup_SerMatal", "Search Metal"),
    sortname: "ElCd",
    refresh: false,
    columns: [
        { name: "ElCd", title: _getLang("L_BaseLookup_Metal", "Metal"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "ElDescp", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Elu", title: _getLang("L_BaseLookup_Unit", "Unit"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Range", title: _getLang("L_BaseLookup_Range", "Range"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "PpriceBase", title: _getLang("L_BaseLookup_PpriceBase", "Price of basic point"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

//料号类别 search
LookUpConfig.PartTypeLookup = {
    caption: _getLang("L_BaseLookup_SerPartType", "Search Part Type"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: _getLang("L_AddBulletin_BullType", "Type") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: _getLang("L_AddBulletin_BullType", "Type") + _getLang("L_BaseLookup_Nm", "Name"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//料号类别 search
LookUpConfig.PartOthLookup = {
    caption: _getLang("L_BaseLookup_SerPatOth", "Search Part Others"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: _getLang("L_BaseLookup_Oth", "Others") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: _getLang("L_BaseLookup_Oth", "Others") + _getLang("L_BaseLookup_Nm", "Name"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//料号编号 search
LookUpConfig.PartCdLookup = {
    caption: _getLang("L_BaseLookup_SerPartNo", "Search Part No"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: _getLang("L_IpPart_PartCd", "Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//料号编号 search
LookUpConfig.PartPkgLookup = {
    caption: _getLang("L_BaseLookup_SerPartPkg", "Search Part Package"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: _getLang("L_IpPart_PartPkg", "Package") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: _getLang("L_IpPart_PartPkg", "Package") + _getLang("L_BaseLookup_Nm", "Name"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//工厂search
LookUpConfig.MafNoLookup = {
    caption: _getLang("L_BaseLookup_SerFactory", "Search Factory"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: _getLang("L_IpPart_MafNo", "Factory") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: _getLang("L_IpPart_MafNo", "Factory") + _getLang("L_BaseLookup_Nm", "Name"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//站别search
LookUpConfig.StnLookup = {
    caption: _getLang("L_BaseLookup_SerStn", "Search Station"),
    sortname: "Stn",
    refresh: false,
    columns: [{ name: "Stn", title: _getLang("L_IpPart_Stn", "Station") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Name", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//预算分析search
LookUpConfig.PtNoLookup = {
    caption: _getLang("L_BaseLookup_SerBud", "Search Budget"),
    sortname: "PtNo",
    refresh: false,
    columns: [{ name: 'PtNo', title: _getLang('L_BaseLookup_BudNo', 'Budget No'), width: 150, align: 'left', sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'PtDate', title: _getLang('L_BaseLookup_BudDate', 'Budget Date'), width: 150, hidden: false, formatter: "date", formatoptions: { srcformat: 'Y-m-d', newformat: 'Y-m-d' }, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'SupplierNm', title: _getLang('L_BaseLookup_Supplier', 'Supplier'), width: 250, align: 'left', sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'BuyerNm', title: _getLang('L_BaseLookup_BuyerNm', 'Buyer Name'), width: 250, align: 'left', sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'Goods', title: _getLang('L_BaseLookup_GoodsNo', 'Goods No'), width: 100, align: 'left', sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'GoodsDescp', title: _getLang('L_BaseLookup_GoodsDescp', 'Goods Descp.'), width: 150, align: 'left', sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'Gw', title: _getLang('L_BaseLookup_Gw', 'GW'), width: 100, align: 'right', sorttype: "string", formatter: "number", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'Gwu', title: _getLang('L_BaseLookup_Gwu', 'Unit of G.W.'), width: 100, align: 'left', sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'Nw', title: _getLang('L_BaseLookup_Nw', 'Net weight'), width: 100, align: 'right', sorttype: "string", formatter: "number", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'Nwu', title: _getLang('L_BaseLookup_Nwu', 'N.W. Unit'), width: 100, align: 'left', sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'Cur', title: _getLang('L_InvPkgSetup_Cur', 'Currency'), width: 100, align: 'left', sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'Scur', title: _getLang('L_BaseLookup_SalCur', 'Sales Currency'), width: 100, align: 'left', sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'WtPs', title: _getLang('L_BaseLookup_WtPs', 'Water Pounds'), width: 100, align: 'right', sorttype: "string", formatter: "number", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'Remark', title: _getLang('L_BSCSSetup_Remark', 'Remark'), width: 250, align: 'right', sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

//开证银行search
LookUpConfig.BsBankLookup = {
    caption: _getLang("L_BaseLookup_SerBank", "Search Bank"),
    sortname: "BankCd",
    refresh: false,
    columns: [{ name: 'BankCd', title: _getLang('L_BaseLookup_Bank', 'Bank') + _getLang('L_BSCODESetup_Cd', 'Party Code'), width: 100, align: 'left', sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'BankNm', title: _getLang('L_BaseLookup_Bank', 'Bank') + _getLang('L_BaseLookup_Nm', 'Name'), width: 250, align: 'left', sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//开证银行search
LookUpConfig.LcbBankLookup = {
    caption: _getLang("L_BaseLookup_SerBank", "Search Bank"),
    sortname: "BankCd",
    sortorder: "asc",
    refresh: false,
    columns: [{ name: 'BankCd', title: _getLang('L_BaseLookup_Bank', 'Bank') + _getLang('L_BSCODESetup_Cd', 'Party Code'), width: 100, align: 'left', sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'BankNm', title: _getLang('L_BaseLookup_Bank', 'Bank') + _getLang('L_BaseLookup_Nm', 'Name'), width: 250, align: 'left', sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'Cur', title: _getLang('L_IpPart_Crncy', 'Currency'), width: 70, align: 'left', sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'Amt', title: _getLang('L_BaseLookup_CredAmt', 'Credit Amount'), width: 150, align: 'right', sorttype: "float", formatter: 'number', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'Prate', title: _getLang('L_BaseLookup_SecDepR', 'Security Deposit Rate'), width: 150, align: 'right', sorttype: "float", formatter: 'number', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'EndDate', title: _getLang('L_BaseLookup_EndDate', 'End Date'), width: 120, align: 'left', sorttype: "string", hidden: false, viewable: true, formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'Rmk', title: _getLang('L_BSCSSetup_Remark', 'Remark'), width: 250, align: 'left', sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

//有色采购单查询
LookUpConfig.PomCLookup = {
    caption: _getLang("L_BaseLookup_SerBuyDetail", "Search Purchase Detail"),
    sortname: "PoNo",
    refresh: false,
    columns: [
        { name: "PoNo", title: _getLang("L_BaseLookup_PoNo", "Purchase Order No"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "CustCd", title: _getLang("L_BaseLookup_Client", "Client") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 100, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "CustNm", title: _getLang("L_BaseLookup_Client", "Client") + _getLang("L_BaseLookup_Nm", "Name"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "SupplierCd", title: _getLang("L_IpPart_SupplierCd", "Supplier Code"), width: 100, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "SupplierNm", title: _getLang("L_IpPart_SupplierNm", "Supplier Name"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "CntryCd", title: _getLang("L_BaseLookup_OrgCntCd", "Country of Origin/Code"), width: 80, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "CntryNm", title: _getLang("L_BaseLookup_OrgCntNm", "Country of Origin/Name"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "GoodsCd", title: _getLang("L_BaseLookup_GoodsCode", "Goods Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "GoodsNm", title: _getLang("L_BaseLookup_GoodsName", "Goods Name"), width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "SctNo", title: _getLang("L_BaseLookup_SctNo", "Foreign contract"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "ContactCd", title: _getLang("L_BaseLookup_IssuePartyCd", "Issuing Party Code"), width: 100, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "ContactNm", title: _getLang("L_BaseLookup_IssuePartyNm", "Issuing Party Name"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "LcNo", title: _getLang("L_BaseLookup_LcNo", "L/C No"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "ExRate", title: _getLang("L_ActSetup_ExRate", "EX-Rate"), width: 80, sorttype: "string", formatter: "number", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "ContactQty", title: _getLang("L_DNApproveManage_Qty", "Quantity"), width: 120, sorttype: "string", formatter: "number", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "ContactQtyu", title: _getLang("L_BaseLookup_Qtyu", "Unit"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "Nw", title: _getLang("L_BaseLookup_BlNw", "Bill NW"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "Cur", title: _getLang("L_IpPart_Crncy", "Currency"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "Bu", title: _getLang("L_IpPart_Bu", "???非e-Shipping模块？"), width: 80, sorttype: "string", hidden: true, viewable: true, formatter: "select", editoptions: { value: _getLang('L_BaseLookup_Script_1', 'A: colored; B: grain; C: cold chain; D: daily chemical; E: other') }, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "Dep", title: _getLang("L_UserSetUp_Dep", "Department"), width: 80, sorttype: "string", hidden: true, viewable: true, formatter: "select", editoptions: { value: _getLang('L_BaseLookup_Script_2', 'A: trade; B: platform') }, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] }
    ]
}

//有色销售单查询
LookUpConfig.SomCLookup = {
    caption: _getLang("L_BaseLookup_SerSlDetail", "Search Sales Detail"),
    sortname: "SoNo",
    refresh: false,
    columns: [
        { name: "SoNo", title: _getLang("L_BaseLookup_SoldNo", "Sales No"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "GcustCd", title: _getLang("L_BaseLookup_BuyerCd", "Buyer Code"), width: 120, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "GcustNm", title: _getLang("L_BaseLookup_BuyerNm", "Buyer Name"), width: 250, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        //{ name: "SupplierCd", title: "供应方", width: 250, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        //{ name: "SupplierNm", title: "供应方", width: 250, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        //{ name: "CntryCd", title: "原产国", width: 70, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        //{ name: "CntryNm", title: "原产国", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "GoodsCd", title: _getLang("L_BaseLookup_GoodsCode", "Goods Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "GoodsNm", title: _getLang("L_BaseLookup_GoodsName", "Goods Name"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "ContactCd", title: _getLang("L_BaseLookup_ContactCd", "Contracter Code"), width: 250, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "ContactNm", title: _getLang("L_BaseLookup_ContactNm", "Contracter Name"), width: 250, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        //{ name: "LcNo", title: "信用证号", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        //{ name: "ExRate", title: "汇率", width: 80, sorttype: "string", formatter: "number", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Geqty", title: _getLang("L_DNApproveManage_Qty", "Quantity"), width: 120, sorttype: "string", formatter: "number", align: "right", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "Qtyu", title: _getLang("L_BaseLookup_Qtyu", "Unit"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "Cur", title: _getLang("L_IpPart_Crncy", "Currency"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "Bu", title: _getLang("L_IpPart_Bu", "???非e-Shipping模块？"), width: 80, sorttype: "string", hidden: true, viewable: true, formatter: "select", editoptions: { value: _getLang('L_BaseLookup_Script_1', 'A: colored; B: grain; C: cold chain; D: daily chemical; E: other') }, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "Dep", title: _getLang("L_UserSetUp_Dep", "Department"), width: 80, sorttype: "string", hidden: true, viewable: true, formatter: "select", editoptions: { value: _getLang('L_BaseLookup_Script_2', 'A: trade; B: platform') }, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] }
    ]
}


//入库查询
LookUpConfig.WcmCLookup = {
    caption: _getLang("L_BaseLookup_SerStockIn", "Search In Stock"),
    sortname: "PsNo",
    refresh: false,
    columns: [
        { name: "WsCd", title: _getLang("L_BaseLookup_Warehouse", "Warehouse") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 70, sorttype: "string", align: "left", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "WsNm", title: _getLang("L_BaseLookup_Warehouse", "Warehouse") + _getLang("L_BaseLookup_Nm", "Name"), width: 200, sorttype: "string", align: "left", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "WiNo", title: _getLang("L_BaseLookup_WiNo", "In Stock No"), width: 140, sorttype: "string", align: "left", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "PsNo", title: _getLang("L_BaseLookup_PsNo", "Purchase No"), width: 140, sorttype: "string", align: "left", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "BlNo", title: _getLang("L_ActSetup_BlNo", "B/L#"), width: 140, sorttype: "string", align: "left", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "Goods", title: _getLang("L_BaseLookup_Goods", "commodity description"), width: 70, sorttype: "string", align: "left", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "GoodsDescp", title: _getLang("L_IpPart_GoodsDescp", "Main of the commodity"), width: 140, sorttype: "string", align: "left", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'cn'] },
        { name: "Bgw", title: _getLang("L_BaseLookup_StroageGw", "Stroage Gw"), width: 120, sorttype: "float", align: "right", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Lgw", title: _getLang("L_BaseLookup_Lgw", "Site feedback weight"), width: 120, sorttype: "float", align: "right", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Nwu", title: _getLang("L_BaseLookup_NwUnit", "Unit of Weight"), width: 70, sorttype: "string", align: "left", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Ucost", title: _getLang("L_BaseLookup_Cost", "Costs/Ton"), width: 90, sorttype: "float", align: "right", formatter: "currency", formatoptions: { thousandsSeparator: ",", decimalPlaces: 2 }, hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "StoreCd", title: _getLang("L_DNApproveManage_Sloc", "Place of Storage"), width: 80, sorttype: "string", align: "left", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "UId", title: "U ID", width: 80, sorttype: "string", align: "left", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

//客戶建檔查詢
LookUpConfig.SmptyLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search"),
    sortname: "PartyNo",
    sortorder: "asc",
    refresh: false,
    columns: [
        { name: "UId", title: "UId", init: false, width: 100, sorttype: "string", hidden: true, viewable: false },
        { name: "PartyNo", title: _getLang("L_BSCODESetup_Cd", "Party Code"), init: true, width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PartyType", title: _getLang("L_BaseLookup_CusPartyType", "Party Type"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PartyName", title: "Name", width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PartyAttn", title: _getLang("L_BSCSDataQuery_PartyAttn", "Attn."), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PartAddr1", title: _getLang("L_BSCSDataQuery_PartAddr", "Address") + "1", width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PartAddr2", title: _getLang("L_BSCSDataQuery_PartAddr2", "Address2"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PartAddr3", title: _getLang("L_BSCSDataQuery_PartAddr3", "Address3"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PartAddr4", title: _getLang("L_BSCSDataQuery_PartAddr4", "Address4"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PartAddr5", title: _getLang("L_BSCSDataQuery_PartAddr5", "Address5"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PartyTel", title: _getLang("L_BSCSSetup_CmpTel", "Tel No."), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PartyFax", title: _getLang("L_BSCSSetup_CmpFax", "Fax No."), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "Cnty", title: "Cnty", width: 40, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "CntyNm", title: "CntyNm", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "City", title: "City", width: 40, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "CityNm", title: "CityNm", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "State", title: "State", width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Zip", title: "Zip", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "ImRecord", title: "ImRecord", width: 180, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}
LookUpConfig.GetSmptyAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NAME%",
        autoCompParams: "PARTY_NO=showValue,PARTY_NO,PARTY_NAME",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

//if用户表
LookUpConfig.IfUsersLookup = {
    caption: _getLang("L_BaseLookup_SerUser", "Search User"),
    sortname: "EmpId",
    refresh: false,
    columns: [
        { name: "EmpId", title: _getLang("L_UserPermission_User", "User") + " ID", width: 140, sorttype: "string", align: "left", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "EmpCnm", title: _getLang("L_UserSetUp_U_NAME", "User Name"), width: 180, sorttype: "string", align: "left", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "EmpTel", title: _getLang("L_BSCSSetup_CmpTel", "Tel No."), width: 150, sorttype: "string", align: "left", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "EmpCell", title: _getLang("L_BaseLookup_Cell", "Cellphone"), width: 150, sorttype: "string", align: "left", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "EmpMail", title: "Email", width: 300, sorttype: "string", align: "left", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

//结账对象
LookUpConfig.BSCSMPLookup = {
    caption: _getLang("L_BaseLookup_SerCheckout", "Search Checkout Target"),
    sortname: "CustCd",
    refresh: false,
    columns: [{ name: "CustCd", title: _getLang("L_BaseLookup_Client", "Client") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalNm", title: _getLang("L_BaseLookup_Client", "Client") + _getLang("L_BaseLookup_Nm", "Name"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "LocalAddr", title: _getLang("L_BaseLookup_ClientAddr", "Customer Addr"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngNm", title: _getLang("L_BaseLookup_ClientENname", "Customer English name"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "EngAddr", title: _getLang("L_BaseLookup_ClientAddrEN", "Customer English Addr"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//公司别查询
LookUpConfig.CmpLookup = {
    caption: "Location",
    sortname: "Cmp",
    refresh: false,
    columns: [{ name: "Cmp", title: "Location Code", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "Name", title: "Location Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.GetCmpUrl = "Common/GetCompanyData";
LookUpConfig.GetCmpAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=stn&TYPE=1&GROUP_ID=" + groupId + "&CMP%",
        autoCompParams: "CMP&NAME=showValue,CMP,NAME",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.MutiLocationLookup = {
    caption: "Location",
    sortname: "Cmp",
    sortorder: "asc",
    refresh: false,
    multiselect: true,
    columns: [{ name: "Cmp", title: "Location Code", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "Name", title: "Location Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}


//運輸類別search
LookUpConfig.TranModeLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search") + _getLang("L_RouteSetup_Tran_mode", "delivery type"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//港口查询
LookUpConfig.CityPortLookup = {
    caption: _getLang("L_BaseLookup_SerPort", "Search Port"),
    sortname: "CntryCd",
    sortorder: "asc",
    refresh: false,
    columns: [
        { name: "CntryCd", title: _getLang("L_CitySetup_CntryCd", "Country Code"), init: true, width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PortCd", title: _getLang("L_TpvPortQuery_Port", "Port Code"), width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PortNm", title: "Name", init: true, width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.CityPortUrl = "Common/GetCityPortData";//城市 港口
LookUpConfig.GetCityPortAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=port1&GROUP_ID=" + groupId + "&CNTRY_CD+PORT_CD=",
        autoCompParams: "CNTRY_CD&PORT_CD=showValue,CNTRY_CD,PORT_CD,PORT_NM",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.GetCityPortAuto2 = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=port1&GROUP_ID=" + groupId + "&PORT_CD%",
        autoCompParams: "CNTRY_CD&PORT_CD=showValue,CNTRY_CD,PORT_CD,PORT_NM,REGION",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

//港口和卡车送货点合并查询 港口：POD=CNTRY_CD+PORT_CD,卡车送货点POD=PORT_CD
LookUpConfig.CityPortLookup2 = {
    caption: _getLang("L_BaseLookup_SerPort", "Search Port"),
    sortname: "CntryCd",
    sortorder: "asc",
    refresh: false,
    columns: [
        { name: "CntryCd", title: _getLang("L_CitySetup_CntryCd", "Country Code"), init: true, width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PortCd", title: _getLang("L_TpvPortQuery_Port", "Port Code"), width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PortNm", title: "Name", init: true, width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "Pod", title: "Pod", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.CityAndTruckPortUrl = "Common/GetCityAndTruckPortData";//抓取城市港口和卡车送货点建档

LookUpConfig.GetCityAndTruckPortAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=cityport&GROUP_ID=" + groupId + "&POD=",
        autoCompParams: "CNTRY_CD&PORT_CD=showValue,CNTRY_CD,PORT_CD,PORT_NM",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.CityPortsLookup = {
    caption: _getLang("L_BaseLookup_SerPort", "Search Port"),
    sortname: "CntryCd",
    sortorder: "asc",
    multiselect: true,
    refresh: false,
    columns: [
        { name: "Cmp", title: "Location", width: 40, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "CntryCd", title: _getLang("L_CitySetup_CntryCd", "Country Code"), init: true, width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PortCd", title: _getLang("L_TpvPortQuery_Port", "Port Code"), width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PortNm", title: "Name", init: true, width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//国家代码查询
LookUpConfig.CntyCdLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search") + _getLang("L_CitySetup_CntryCd", "Country Code"),
    sortname: "CntryCd",
    refresh: false,
    columns: [{ name: "CntryCd", title: _getLang("L_CitySetup_CntryCd", "Country Code"), width: 180, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CntryNm", title: _getLang("L_CntySetup_CntryNm", "Country Name"), width: 180, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}

//headoffice and billto
LookUpConfig.PartyNoLookup = {
    caption: "PartyNo",
    sortname: "PartyNo",
    refresh: false,
    columns: [{ name: "PartyType", title: "PartyType", width: 250, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartyNo", title: "PartyNo", init: true, width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartyName", title: "PartyName", init: true, width: 250, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartyName2", title: "PartyName2", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartyName3", title: "PartyName3", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartyName4", title: "PartyName4", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartyMail", title: "Mail", width: 450, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartAddr1", title: "Addr1", width: 250, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartAddr2", title: "Addr2", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartAddr3", title: "Addr3", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartAddr4", title: "Addr4", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartAddr5", title: "Addr5", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Cnty", title: "Cnty", width: 40, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CntyNm", title: "CntyNm", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "City", title: "City", width: 40, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CityNm", title: "CityNm", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartyAttn", title: "PartyAttn", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "HeadOffice", title: "BillTo", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartyTel", title: "Tel", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartyFax", title: "Fax", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "State", title: "State", width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "BillTo", title: "Bill To", width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Zip", title: "Zip", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "TaxNo", title: "TaxNo", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}
LookUpConfig.PartyNoUrl = "Common/GetPartyNoData";
LookUpConfig.PartyNo1Url = "Common/GetPartyNo1Data";
LookUpConfig.GetPartyNoAuto = function (groupId, $grid, autoFn, clearFn) {
    var op =
    {
        autoCompDt: "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO%",
        autoCompParams: "PARTY_TYPE&PARTY_NAME&PARTY_NO=showValue,STATE,ZIP,PARTY_NO,PARTY_NAME,PARTY_NAME2,PARTY_NAME3,PARTY_NAME4,PARTY_MAIL,PART_ADDR1,PART_ADDR2,PART_ADDR3,PART_ADDR4,PART_ADDR5,PARTY_FAX,PARTY_ATTN,PARTY_TEL,PARTY_TYPE,CNTY,CNTY_NM,CITY,CITY_NM,TAX_NO",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        },
        autoClearFunc: function (elem, event, rowid) {
            if (clearFn) clearFn($grid, elem, rowid);
        }
    };
    return op;
}

//货况类型查询
LookUpConfig.StatusLookup = {
    caption: _getLang("L_BaseLookup_SerCarSts", "Search Status Type"),
    sortname: "StsCd",
    refresh: false,
    columns: [
        { name: "StsCd", title: _getLang("L_AirTransport_StsCd", "Status Code"), width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "Ldescp", title: _getLang("L_GroupRelation_stnCName", "Name in Chinese"), init: true, width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "Edescp", title: _getLang("L_GroupRelation_depEName", "Name in English"), init: true, width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.StatusUrl = "Common/GetStatusData";//城市 港口
LookUpConfig.GetStatusAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=status&STS_CD%",
        autoCompParams: "STS_CD&LDESCP&EDESCP=showValue,STS_CD,LDESCP,EDESCP",
        autoCompFunc: function (elem, event, ui, rowid) {
            //alert(rowid);
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}
LookUpConfig.LocationLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search") + " Location",
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Location", title: "Location", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.ExpressLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search") + " Express",
    sortname: "PartyNo",
    refresh: false,
    columns: [{ name: "PartyNo", title: _getLang("L_MailGroupSetup_Cmp", "Company"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartyName", title: _getLang("L_MailGroupSetup_Cmp", "Company") + _getLang("L_BaseLookup_Nm", "Name"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}


LookUpConfig.TrgnLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search") + " Region",
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: "Region Cd", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Region" + _getLang("L_BaseLookup_Nm", "Name"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.TrgnUrl = "Common/GetRegionData";//Region
LookUpConfig.GetTrgnAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=status&STS_CD%",
        autoCompParams: "STS_CD&LDESCP&EDESCP=showValue,STS_CD,LDESCP,EDESCP",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

//貿易條款search
LookUpConfig.TermLookup = {
    caption: _getLang("L_BaseLookup_SerTerm", "Search Incoterm"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: _getLang("L_RouteSetup_Term", "Trade term"), width: 80, init: true, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.TermUrl = "Common/GetTermData";//貿易條款
LookUpConfig.DlvTermUrl = "Common/GetDlvTermUrlData";//DLV Term
LookUpConfig.TranModeUrl = "Common/GetTranModeData";//運輸類別



//货况通知设定查询
LookUpConfig.NotifyLookup = {
    caption: _getLang("L_BaseLookup_SerCarNotifySet", "Search Cargo Notify Setting"),
    sortname: "SeqNo",
    refresh: false,
    columns: [
        { name: "UId", title: "Uid", width: 250, sorttype: "string", hidden: true, viewable: false },
        { name: "Stn", title: "Stn", width: 250, sorttype: "string", hidden: true, viewable: false },
        { name: "GroupId", title: "GroupId", width: 250, sorttype: "string", hidden: true, viewable: false },
        { name: "Cmp", title: "Cmp", width: 250, sorttype: "string", hidden: true, viewable: false },
        { name: "PartyType", title: "Party Type", width: 250, sorttype: "string", hidden: true, viewable: false },
        { name: "SeqNo", title: _getLang("L_NRSSetup_SeqNo", "Serial number"), width: 70, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "NotifyCd", title: _getLang("L_BaseLookup_Notify", "Notify") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 90, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "RequestCd", title: _getLang("L_BaseLookup_Request", "Request") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 90, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.NotifyUrl = "TKBL/GetNotifyData";//城市 港口
LookUpConfig.NotifyItemUrl = "TKBL/GetNotifyItem";

//Party Type多選查询
LookUpConfig.PartyTypeLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search") + " Party Type",
    sortname: "Cd",
    refresh: false,
    columns: [
        { name: "Cd", title: _getLang("L_BSCODESetup_Cd", "Party Code"), width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "CdDescp", title: "Name", init: true, width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "OrderBy", title: _getLang("L_BaseLookup_OrderBy", "Ranking"), width: 60, sorttype: "string", hidden: true, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },]
}

//Party Type查询
LookUpConfig.MutiPartyTypeLookup = {
    caption: "Party Type " + _getLang("L_BaseLookup_Ser", "Search"),
    sortname: "Cd",
    rows: 200,
    refresh: false,
    multiselect: true,
    columns: [
        { name: "Cd", title: _getLang("L_BSCODESetup_Cd", "Party Code"), width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "CdDescp", title: "Name", init: true, width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.PartyTypeUrl = "Common/GetPartyTypeData";
LookUpConfig.GetPartyTypeAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=bsc&&CD_TYPE='PT'&GROUP_ID=" + groupId + "&CD%",
        autoCompParams: "CD&CD_DESCP=showValue,CD,CD_DESCP",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

//SMQTM
LookUpConfig.SmqtmLookup = {
    caption: "Trailer quotation list",
    sortname: "QuotDate",
    rows: 200,
    refresh: false,
    multiselect: false,
    sortorder: 'desc',
    columns: [
        { name: "QuotNo", title: "Quotation No", width: 150, init: true, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "QuotDate", title: "Quotation Date", width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "LspCd", title: "LSP ID", init: true, width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "LspNm", title: "LSP Name", width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.SmqtmUrl = "TPVCommon/GetSmqtmForLookup";
LookUpConfig.GetSmqtmAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=qtm&GROUP_ID=" + groupId + "&QUOT_NO=",
        autoCompParams: "QUOT_NO=showValue,QUOT_NO",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

//Mail Type查询
LookUpConfig.MailTypeLookup = {
    caption: _getLang("L_BaseLookup_SerMailType", "Search Mail Type"),
    sortname: "Cd",
    refresh: false,
    columns: [
        { name: "Cd", title: "Code", width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "CdDescp", title: "Name", init: true, width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.PDocLookup = function (select_tranmode, select_term) {
    return {
        caption: _getLang("L_BaseLookup_SerEDocSet", "Search E-doc Setting"),
        sortname: "TranMode",
        refresh: false,
        columns: [
            { name: "UId", title: "Uid", width: 250, sorttype: "string", hidden: true, viewable: false },
            { name: "Stn", title: "Stn", width: 250, sorttype: "string", hidden: true, viewable: false },
            { name: "GroupId", title: "GroupId", width: 250, sorttype: "string", hidden: true, viewable: false },
            { name: "Cmp", title: "Cmp", width: 250, sorttype: "string", hidden: true, viewable: false },
            { name: "TranMode", title: _getLang("L_RouteSetup_Tran_mode", "delivery type"), width: 100, formatter: "select", edittype: 'select', editoptions: { value: select_tranmode }, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: "Term", title: _getLang("L_ShipmentID_Incoterm", "Incoterm"), width: 100, formatter: "select", edittype: 'select', editoptions: { value: select_term }, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: "FreightTerm", title: _getLang("L_BaseLookup_FreightTerm", "Freight Term"), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: "PartyType", title: _getLang("L_BaseLookup_PartyType", "Party Type"), width: 90, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: "PartyDescp", title: _getLang("L_BaseLookup_PartyDescp", "Party Descp."), width: 190, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: "StsCd", title: _getLang("L_BaseLookup_ActiveSts", "Active Status"), width: 90, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: "StsDescp", title: _getLang("L_BaseLookup_StsDescp", "Status Descp."), width: 190, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
    }
}
LookUpConfig.PDocUrl = "TKBL/GetPDocData";//
LookUpConfig.PDocItemUrl = "TKBL/GetPDocItem";
LookUpConfig.MailGroupUrl = "TKBL/GeMailGroupData";//
LookUpConfig.CustomerLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search") + _getLang("L_BaseLookup_Client", "Client"),
    sortname: "CmpId",
    refresh: false,
    columns: [{ name: "CmpId", title: _getLang("L_BaseLookup_Client", "Client") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CmpName", title: _getLang("L_BaseLookup_Client", "Client") + _getLang("L_BaseLookup_Nm", "Name"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.CustomerUrl = "Common/GetCustomerData";//

LookUpConfig.BuUrl = "Common/GetBuData";//

//目的地
LookUpConfig.DeliveryLookup = {
    caption: _getLang("L_BaseLookup_SerDest", "Search Destination"),
    sortname: "PortCd",
    refresh: false,
    columns: [{ name: "CntryCd", title: "Cntry Cd", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PortCd", title: "Port Cd", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PortNm", title: "Port Nm", width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "State", title: "State", width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Region", title: "Region", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.DeliveryUrl = "Common/GetDeliveryData";//Region
LookUpConfig.GetDeliveryAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=status&STS_CD%",
        autoCompParams: "STS_CD&LDESCP&EDESCP=showValue,STS_CD,LDESCP,EDESCP",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}


LookUpConfig.ShipmentLookup = {
    caption: _getLang("L_BaseLookup_SerBl", "Search Bill"),
    sortname: "PortCd",
    refresh: false,
    columns: [{ name: 'UId', showname: 'ID', sorttype: 'string', hidden: true, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'Cstatus', title: 'Cargo Status', index: 'Cstatus', width: 100, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'A:Pickup;B:Receive;C:Onboard;D:Arrival;E:Delivery' } },
    { name: 'TranMode', title: 'Tran Mode', index: 'TranMode', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'TranType', title: 'Tran Type', index: 'TranType', width: 80, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: '1:Air;2:SeaLCL;3:SeaFCL;4:OExpress;5:DExpress;6:Dosmatics' } },
    { name: 'HouseNo', title: 'House No', index: 'HouseNo', width: 120, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: 'MasterNo', title: 'Master No', index: 'MasterNo', width: 120, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.ShipmentUrl = "Common/GetShipmentData";
LookUpConfig.ShipmentItemUrl = "TKBL/GetShipmentItemData";

LookUpConfig.CompanyByPartyTypeLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search") + _getLang("L_MailGroupSetup_Cmp", "Company"),
    sortname: "Cmp",
    refresh: false,
    columns: [{ name: "Cmp", title: _getLang("L_MailGroupSetup_Cmp", "Company") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
    { name: "CmpName", title: _getLang("L_MailGroupSetup_Cmp", "Company") + _getLang("L_MailGroupSetup_Name", "Name"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.GetCompanyByPartyTypeUrl = "Common/GetCompanyByPartyType";


LookUpConfig.GetCodeTypeAuto = function (groupId, type, $grid, autoFn, clearFn) {
    var op =
    {
        autoCompDt: "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE=" + type + "&CD%",
        autoCompParams: "CD&CD_DESCP=showValue,CD,CD_DESCP,ORDER_BY",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        },
        autoClearFunc: function (elem, event, rowid) {
            if (clearFn)
                clearFn($grid, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.GetCodeTypeAuto1 = function (groupId, $grid, autoFn, autoCompGetValueFunc) {
    var op =
    {
        autoCompDt: "dt=bsc&GROUP_ID=" + groupId + "&CD%",
        autoCompParams: "CD&CD_DESCP=showValue,CD,CD_DESCP,ORDER_BY",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        },
        autoCompGetValueFunc: autoCompGetValueFunc
    };
    return op;
}

LookUpConfig.BookingLookup = {
    caption: _getLang("L_BaseLookup_SerInquiry", "Search Inquiry Data"),
    sortname: "CreateDate",
    refresh: false,
    columns: [
        { name: 'UId', showname: 'UId', sorttype: 'string', hidden: true, viewable: false },
        { name: 'Status', title: _getLang('L_SYS_STATUS', 'Status'), index: 'Status', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'RfqNo', title: _getLang('L_RQQuery_RfqNo', 'enquiry NO.'), index: 'RfqNo', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'RfqFrom', title: _getLang('L_RQQuery_RfqFrom', 'start date'), index: 'RfqFrom', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'RfqTo', title: _getLang('L_RQQuery_RfqTo', 'Due date'), index: 'RfqTo', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'RfqDate', title: _getLang('L_BaseLookup_QuotedDate', 'Quoted Date'), index: 'RfqDate', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'TranMode', title: _getLang('L_BaseLookup_TranMode', 'TRAN.Mode'), index: 'TranMode', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Rlocation', title: _getLang('L_BaseLookup_Rlocation', 'BID Inviting factory'), index: 'Rlocation', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'RlocationNm', title: 'Name', index: 'RlocationNm', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'FreightTerm', title: _getLang('L_BaseLookup_FreightPayer', 'Freight Payer'), index: 'FreightTerm', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Incoterm', title: 'Incoterm', index: 'Incoterm', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'ServiceMode', title: 'Server Mode', index: 'RfqNo', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Remark', title: _getLang('L_BSCSSetup_Remark', 'Remark'), index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
};

LookUpConfig.CurUrl = "Common/GetBscurData";
LookUpConfig.GetCurAuto = function (groupId, type, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=crn&GROUP_ID=" + groupId + "&CUR%",
        autoCompParams: "CUR&CUR_DESCP=showValue,CUR,CUR_DESCP",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

//貿易條款search
LookUpConfig.ServiceModeLookup = {
    caption: _getLang("L_BaseLookup_SerLoadType", "Search Loading Type"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: _getLang("L_BaseLookup_LoadType", "Load Type"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.ServiceModeUrl = "Common/GetServiceModeData";//装货类型
LookUpConfig.GetPartyTypeNoAuto = function (groupId, party_type, $grid, autoFn) {
    var autoCompDt = "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO%";
    if (party_type) autoCompDt = "dt=smpty&PARTY_TYPE=" + party_type + "&GROUP_ID=" + groupId + "&PARTY_NO%";
    var op =
    {
        autoCompDt: autoCompDt,
        autoCompParams: "PARTY_TYPE&PARTY_NAME&PARTY_NO=showValue,PARTY_NO,PARTY_NAME,PARTY_MAIL,PART_ADDR1,PART_ADDR2,PART_ADDR3,PARTY_ATTN,PARTY_TEL,PARTY_TYPE",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}
LookUpConfig.TrackingTranModeUrl = "Common/GetTrackingTranModeData";//運輸類別

LookUpConfig.FileTypeLookup = {
    caption: _getLang("L_BaseLookup_SerFileType", "Search File Type"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: _getLang("L_BaseLookup_FileType", "File Type"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.FileTypeUrl = "Common/GetFileTypeData";//装货类型

LookUpConfig.LocationTypeLookup = {
    caption: _getLang("L_BaseLookup_SerLocation", "Search Cargo Location"),
    sortname: "Cd",
    refresh: false,
    columns: [{ name: "Cd", title: _getLang("L_BaseLookup_Location", "Location") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "OrderBy", title: _getLang("L_DNDetailVeiw_OrderBy", "sequence"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.LocationTypeUrl = "Common/GetLocationTypeData";//装货类型

LookUpConfig.QtyuLookup = {
    caption: _getLang("L_BaseLookup_QtyUnitList", "Quantity Unit List"),
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, init: true, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
};
LookUpConfig.QtyuUrl = "Common/GetUnitData";//

LookUpConfig.ShipmentInfoLookup = {
    caption: "Dn No",
    refresh: false,
    columns: [{ name: "Name", title: "Dn No", width: 80, init: true, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
};

LookUpConfig.ShipmentInfoUrl = "Common/GetShipmentInfo";

LookUpConfig.NwuLookup = {
    caption: _getLang("L_BaseLookup_NwUList", "List of Weight unit"),
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
};
LookUpConfig.NwuUrl = "Common/GetNwuData";//

LookUpConfig.WhLookup = {
    caption: _getLang("L_BaseLookup_WarehouseList", "Warehouse List"),
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
};
LookUpConfig.WhUrl = "Common/GetWHData";//

LookUpConfig.RQDataItemUrl = "RQManage/GetRQSetupDataItem";//装货类型

LookUpConfig.TrackWayLookup = {
    caption: _getLang("L_BaseLookup_Land", "Inland Transprotation"),
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
};
LookUpConfig.TrackWayUrl = "Common/GetTrackWayData";//

LookUpConfig.CargoTypeLookup = {
    caption: _getLang("L_DNApproveManage_CargoType", "Cargo Type"),
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
};
LookUpConfig.CargoTypeUrl = "Common/GetCargoTypeData";//
LookUpConfig.QTDataItemUrl = "QTManage/GetQTDataItem";

//Dn Lookup
LookUpConfig.dnLookup = {
    caption: _getLang("L_BaseLookup_SerDN", "Search DN"),
    //sortname: "Cd",
    refresh: false,
    columns: [
        { name: "DnNo", title: "DN NO", width: 160, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "TranMode", title: _getLang("L_DNApproveManage_TranMode", "Delivery Mode"), width: 80, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "RefNo", title: _getLang("L_BaseLookup_CarRefNo", "Cargo Ref No"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "ShipmentId", title: "Shipment ID", width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "DivisionCd", title: _getLang("L_ShipmentID_DivisionCd", "Production Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "DivisionDescp", title: _getLang("L_ShipmentID_DivisionDescp", "Product"), width: 150, align: "right", sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

//State Lookup
LookUpConfig.StateLookup = {
    caption: _getLang("L_BsStateQuery_StateList", "State List"),
    sortname: "StateCd",
    refresh: false,
    //之前columns中使用的fieldName，因后面直接使用此作为model，所以导致查询结果为空，所以将fieldName修改为name
    columns: [{ name: "CntryCd", title: _getLang("L_BaseLookup_CntyCd", "Country Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CntryNm", title: _getLang("L_BaseLookup_CntyNm", "Country Name"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "RegionCd", title: _getLang("L_BaseLookup_RgCd", "Region Code"), width: 80, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "RegionNm", title: _getLang("L_BaseLookup_RgNm", "Region Name"), width: 80, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "StateCd", title: _getLang("L_BaseLookup_StaCd", "State Code"), width: 80, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "StateNm", title: _getLang("L_BaseLookup_StaNm", "State Name"), width: 120, sorttype: "string", hidden: false, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}


//费用代码
LookUpConfig.ChgUrl1 = "TPVCommon/GetChgDataForLookup";
LookUpConfig.ChgUrl = "Common/GetChgData";
LookUpConfig.ChgLookup = {
    caption: _getLang("L_BaseLookup_SerChgCd", "Search Fee Code"),
    //sortname: "Cd",
    refresh: false,
    columns: [
        { name: "TranMode", title: _getLang("L_PartyDocSetup_TranMode", "Trans Type"), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], formatter: "select", editoptions: { value: _getLang('L_BaseLookup_Script_5', 'A:A.Air; S:S.Sea; T:T.Truck; R:R.Railroad; D:D.; E:E.Express; L:L.LCL; F:F.FCL; O:ALL') }, edittype: 'select' },
        { name: "ChgCd", title: _getLang("L_SLCLQuery_F1", "Expense") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "ChgDescp", title: _getLang("L_SLCLQuery_F1", "Expense") + _getLang("L_GroupRelation_groupDesc", "Description"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Repay", title: _getLang("L_SMCHGSetup_ChgRepay", "Class"), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], formatter: "select", editoptions: { value: _getLang('L_BaseLookup_Script_4', 'M:M.Must be charged ;C:C.Collecting while happened;Y:Y.Collected by agent;A:A.AT Cost') }, edittype: 'select' },
        { name: "ChgType", title: _getLang("L_SMCHGSetup_ChgType", "Charge Type"), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], formatter: "select", editoptions: { value: 'F:F.Freight Charge;O:O.Original Fee;D:D.Destination Fee' }, edittype: 'select' }
    ]
}

LookUpConfig.GetChgAuto = function (groupId, tran_mode, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=chg&GROUP_ID=" + groupId + "&CHG_TYPE%",//TRAN_MODE
        autoCompParams: "CHG_CD&CHG_DESCP=showValue,CHG_CD,CHG_TYPE,REPAY,CHG_DESCP",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.GetChgAuto1 = function (groupId, tran_mode, $grid, autoFn, autoCompGetValueFunc, dymcFunc) {
    var op =
    {
        autoCompDt: "dt=chg&GROUP_ID=" + groupId + "&CHG_CD%",//TRAN_MODE
        autoCompParams: "CHG_CD&CHG_DESCP=showValue,CHG_CD,CHG_DESCP,REPAY,CHG_TYPE",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        },
        autoCompGetValueFunc: autoCompGetValueFunc,
        dymcFunc: dymcFunc
    };
    return op;
}

LookUpConfig.CountryLookup = {
    caption: _getLang("L_BaseLookup_SerCntSetup", "Search Country Setup"),
    sortname: "CntryCd",
    refresh: false,
    columns: [
        { name: "CntryCd", title: "Code", width: 60, init: true, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "CntryNm", title: "Name", width: 200, init: true, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.CountryUrl = "Common/GetCountryData";//城市 港口
LookUpConfig.GetCountryAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=country&GROUP_ID=" + groupId + "&CNTRY_CD%",
        autoCompParams: "CNTRY_CD&CNTRY_NM=showValue,CNTRY_CD,CNTRY_NM",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

//仓库查询
LookUpConfig.SMWHLookup = {
    caption: _getLang("L_BaseLookup_SerWHouse", "Search Warehouse"),
    sortname: "WsCd",
    refresh: false,
    columns: [{ name: "WsCd", title: _getLang("L_BaseLookup_Warehouse", "Warehouse") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "WsNm", title: _getLang("L_BaseLookup_Warehouse", "Warehouse") + _getLang("L_MailGroupSetup_Name", "Name"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "MfNo", title: _getLang("L_GateAnalysis_MfNo", "Plant"), width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "DlvArea", title: "Dlv Area", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "DlvAreaNm", title: "Dlv Area Name", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "DlvAddr", title: "Dlv Address", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

LookUpConfig.SMWHUrl = "TPVCommon/GetSmwhForLookup";

LookUpConfig.GetSMWHAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=smwh&GROUP_ID=" + groupId + "&WS_CD=",
        autoCompParams: "WS_CD&WS_NM&CMP=showValue,WS_CD,WS_NM,CMP",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.GetSMWHAuto1 = function (groupId, $grid, autoFn, dymcFunc, clearFn) {
    var op =
    {
        autoCompDt: "dt=smwh&GROUP_ID=" + groupId + "&WS_CD=",
        autoCompParams: "WS_CD&WS_NM&CMP=showValue,WS_CD,WS_NM,CMP",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        },
        autoClearFunc: function (elem, event, rowid) {
            if (clearFn)
                clearFn($grid, elem, rowid);
        },
        autoCompGetValueFunc: dymcFunc
    };
    return op;
}

//月台查询
LookUpConfig.SMWHGTLookup = {
    caption: _getLang("L_BaseLookup_SerGate", "Search Gate"),
    sortname: "GateNo",
    refresh: false,
    columns: [{ name: "Cmp", title: "Location", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "WsCd", title: _getLang("L_BaseLookup_Warehouse", "Warehouse") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "WsNm", title: _getLang("L_BaseLookup_Warehouse", "Warehouse") + _getLang("L_BaseLookup_Nm", "Name"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "GateNo", title: _getLang("L_BaseLookup_GateNo", "Gate No"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Lift", title: _getLang("L_BaseLookup_Lift", "Lift or not"), width: 80, sorttype: "string", hidden: false, viewable: true, formatter: "select", edittype: 'select', editoptions: { value: 'Y:Yes;N:No' }, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Status", title: _getLang("L_GroupRelation_stnStatus", "Status"), width: 80, sorttype: "string", hidden: false, viewable: true, formatter: "select", edittype: 'select', editoptions: { value: 'Y:' + _getLang('L_BaseLookup_Usable', 'Usable') + ';S:' + _getLang('L_BaseLookup_NotInServer', 'Temporary Closed') + ';A:' + _getLang('L_BaseLookupUsing_Using', 'Using') + ';N:' + _getLang('L_BaseLookup_NotUse', 'Out of Service') }, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

LookUpConfig.ProvinceLookup = {
    caption: _getLang("L_BaseLookup_SerState", "Search State"),
    sortname: "StateCd",
    refresh: false,
    columns: [{ name: "CntryCd", title: _getLang("L_CitySetup_CntryCd", "Country Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CntryNm", title: _getLang("L_CntySetup_CntryNm", "Country Name"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "StateCd", title: _getLang("L_BaseLookup_State", "State") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 150, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "StateNm", title: _getLang("L_BaseLookup_State", "State") + _getLang("L_BaseLookup_Nm", "Name"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "RegionCd", title: _getLang("L_CitySetup_Region", "Region") + _getLang("L_BSCODESetup_Cd", "Party Code"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "RegionNm", title: _getLang("L_CitySetup_Region", "Region") + _getLang("L_BaseLookup_Nm", "Name"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}
LookUpConfig.ProvinceUrl = "Common/GetProvince";
LookUpConfig.GetProvinceAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=bsstate&GROUP_ID=" + groupId + "&STATE_CD%",
        autoCompParams: "CNTRY_NM&STATE_NM&REGION_NM=showValue,CNTRY_CD,CNTRY_NM,STATE_CD,STATE_NM,REGION_CD,REGION_NM",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}
//Material
LookUpConfig.ProductTypeLookup = {
    caption: "Code List",
    sortname: "ApCd",
    sortorder: "asc",
    refresh: false,
    columns: [{ name: "ApCd", title: "SCAC", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
};

LookUpConfig.BSCodeLookup = {
    caption: "Code List",
    sortname: "Cd",
    sortorder: "asc",
    refresh: false,
    columns: [{ name: "Cd", title: "Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CdDescp", title: "Name", width: 400, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
};
LookUpConfig.ChannelUrl = "Common/GetChannelData";//销售渠道
LookUpConfig.DivisionUrl = "Common/GetDivisionData";//產品組代碼
LookUpConfig.PorteUrl = "Common/GetPorteData";//口岸
LookUpConfig.ProductTypeUrl = "TPVCommon/GetMaterialForLookup";
LookUpConfig.ContainerYardUrl = "TPVCommon/GetCntyDataForLookup";
LookUpConfig.GetBsCodeAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE%",
        autoCompParams: "CD=showValue,CD,CD_DESCP",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}
LookUpConfig.GetMaterialAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=bsc&GROUP_ID=" + groupId + "&AP_CD%",
        autoCompParams: "AP_CD=showValue,AP_CD",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}
LookUpConfig.TpvportLookup = {
    caption: _getLang("L_BaseLookup_CdList", "Code List"),
    refresh: false,
    columns: [
        { name: "Cnty", title: "Cnty", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "CntyNm", title: "Cnty Name", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Port", title: "Port", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "PortNm", title: "Port Name", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "ProlinkCd", title: "Prolink Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Remark", title: "Remark", width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
};
//lookUpConfig.TpvportUrl = "Common/GetProvince"; //GetProvince//GetTpvPortData

LookUpConfig.TpvportLUrl = "Common/GetTpvPortLData";
LookUpConfig.TpvportDUrl = "Common/GetTpvPortDData";
LookUpConfig.GetTpvPortAuto = function (groupId, type, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=tpvport&Flag=" + type + "&PROLINK_CD%",
        autoCompParams: "CNTY&CNTY_NM&PORT&PORT_NM&PROLINK_CD=showValue,PROLINK_CD,PORT_NM",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.AirVoyageLookup = {
    caption: _getLang("L_BaseLookup_CdList", "Code List"),
    refresh: false,
    columns: [
        { name: "Cnty", title: "Cnty", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "CntyNm", title: "Cnty Name", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Port", title: "Port", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "PortNm", title: "Port Name", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "ProlinkCd", title: "Prolink Code", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "Remark", title: "Remark", width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
};
LookUpConfig.AirVoyageUrl = "Common/GetPolOrPodData"; //CitySetupLookup

LookUpConfig.GetAirVoyageAuto = function (groupId, type, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=tpvport&Flag=" + type + "&PROLINK_CD%",
        autoCompParams: "CNTY&CNTY_NM&PORT&PORT_NM&PROLINK_CD=showValue,PROLINK_CD,PORT_NM",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}


LookUpConfig.AirVoyageLookup = {
    caption: _getLang("L_BaseLookup_SerCity", "Search State"),
    sortname: "PortCd",
    refresh: false,
    columns: [
        { name: "CntryCd", title: "Country", width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PortCd", title: "City", width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "PortNm", title: "Name", init: true, width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
};

LookUpConfig.BstruckcLookup = {
    caption: _getLang("L_BaseLookup_SerTruck", "Search Truck"),
    sortname: "TruckNo",
    refresh: false,
    columns: [
        { name: "PartyNo", title: "Party No", width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "TruckNo", title: _getLang("L_BaseLookup_TruckNo", "Truck No"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }
    ]
};

LookUpConfig.BstruckdLookup = {
    caption: _getLang("L_BaseLookup_SerDriver", "Search Driver"),
    sortname: "DriverName",
    refresh: false,
    columns: [
        { name: "PartyNo", title: "Party No", width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "DriverName", title: _getLang("L_GateReserve_Driver", "Driver"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "DriverId", title: _getLang("L_GateReserve_DriverId", "ID No"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "DriverPhone", title: _getLang("L_BaseLookup_Phone", "Phone#"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }
    ]
};

LookUpConfig.TVKOUrl = "Common/GetTVKOData";
LookUpConfig.TCARUrl = "Common/GetTCARData";
LookUpConfig.RCARUrl = "Common/GetRCARData";
LookUpConfig.OrderTypeUrl = "Common/GetOrderTypeData";
LookUpConfig.CarTypeUrl = "Common/GetCarTypeData";
LookUpConfig.TruckPortCdUrl = "Common/GetTruckPortCdData";
LookUpConfig.TruckPortCdAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=bstport&GROUP_ID=" + groupId + "&PORT_CD%",
        autoCompParams: "REGION&STATE&PORT_NM&PORT_CD=showValue,PORT_CD,PORT_NM,STATE,REGION",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.TruckPortAddrUrl = "Common/GetTruckPortAddrData";
LookUpConfig.TruckPortAddrAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=bsaddr&GROUP_ID=" + groupId + "&ADDR_CODE%",
        autoCompParams: "ADDR&ADDR_CODE=showValue,ADDR_CODE,ADDR",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.PartyNoLookup1 = {
    caption: "PartyNo",
    sortname: "PartyNo",
    refresh: false,
    columns: [{ name: "PartyType", title: "PartyType", init: true, width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartyNo", title: "PartyNo", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartyName", title: "PartyName", init: true, width: 280, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}


LookUpConfig.MultiPartyNoLookup = {
    caption: "PartyNo",
    sortname: "PartyNo",
    refresh: false,
    multiselect: true,
    columns: [{ name: "PartyType", title: "PartyType", init: true, width: 230, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartyNo", title: "PartyNo", width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "PartyName", title: "PartyName", init: true, width: 180, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

LookUpConfig.TALNUrl = "Common/GetTALNData";

LookUpConfig.SmrvLookup = {
    caption: _getLang("L_BaseLookup_ReserveList", "Reserve List"),
    sortname: "UseDate",
    sortorder: "desc",
    refresh: false,
    columns: [
        { name: "ReserveNo", title: _getLang("L_BaseLookup_ReserveNo", "Reserve No"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "BatNo", title: _getLang("L_GateReserve_BatNo", "Batch NO"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "CallDate", title: _getLang("L_BaseLookup_CallDate", "Call Date"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "UseDate", title: _getLang("L_BaseLookup_UseDate", "Use Date"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "ShipmentId", title: "Shipment ID", width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "DnNo", title: "DN NO", width: 500, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

LookUpConfig.CostCenterLookup = {
    caption: _getLang("L_BaseLookup_SerCostCenter", "Search Cost Center"),
    sortname: "CostCenter",
    refresh: false,
    columns: [{ name: "Cmp", title: "Location", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Company", title: _getLang("L_MailGroupSetup_Cmp", "Company"), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Dep", title: _getLang("L_UserSetUp_Dep", "Department"), width: 70, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "CostCenter", title: _getLang("L_BaseColModel_CostCenter", "Cost Center"), width: 100, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "ShortDescp", title: _getLang("L_BaseColModel_ShortDescp", "Brief Descp."), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Descp", title: _getLang("L_GroupRelation_groupDesc", "Description"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
    { name: "Principal", title: _getLang("L_BaseColModel_Principal", "Owner"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}
LookUpConfig.CostCenterUrl = "Common/GetCostCenter";
LookUpConfig.GetCostCenterAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=smcc&GROUP_ID=" + groupId + "&COST_CENTER%",
        autoCompParams: "DESCP&PRINCIPAL&SHORT_DESCP&COST_CENTER=showValue,COST_CENTER,SHORT_DESCP,DESCP,PRINCIPAL,DEP",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.GetUserAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=user&GROUP_ID=" + groupId + "&U_ID%",
        autoCompParams: "U_ID&U_EMAIL=showValue,U_ID,U_EMAIL",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        },
        autoClearFunc: function (elem, event, rowid) {
            clearFn($grid, elem, rowid);
        }
    };
    return op;
}


LookUpConfig.DnQueryLookup = {
    caption: _getLang("L_BaseLookup_SerDN", "Search DN"),
    refresh: false,
    columns: [
        { name: "DnNo", title: "DN NO", width: 160, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'TranType', title: _getLang('L_DNFlowManage_TranType', 'Trans Type'), index: 'TranType', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TranTypeDescp', title: _getLang('L_DNFlowManage_TranTypeDescp', 'TransType Description'), index: 'TranTypeDescp', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Pol', title: _getLang('L_DNApproveManage_Pol', 'Loading Port Code'), index: 'Pol', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'PolNm', title: _getLang('L_DNApproveManage_PolNm', 'Loading port Name'), index: 'PolNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'PorteCd', title: _getLang('L_DNApproveManage_PorteCd', 'Export Port Code'), index: 'PorteCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'PorteDescp', title: _getLang('L_DNApproveManage_PorteDescp', 'Export Port'), index: 'PorteDescp', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'State', title: _getLang('L_DNApproveManage_Pod', 'Destination') + _getLang('L_BaseLookup_State', 'State'), index: 'State', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'Pod', title: _getLang('L_DNApproveManage_Pod', 'Destination'), index: 'Pod', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'PodNm', title: _getLang('L_DNApproveManage_PodNm', 'POD Name'), index: 'PodNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CombineInfo', title: _getLang('L_DNApproveManage_CombineInfo', 'Combined DN Info.'), index: 'CombineInfo', width: 250, align: 'left', sorttype: 'string', hidden: false, classes: "normal-white-space" },
        { name: 'ShipMark', title: _getLang('L_DNApproveManage_ShipMark', 'Shipping Mark'), index: 'ShipMark', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Goods', title: 'Commodity', index: 'Goods', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Shipper', title: 'Shipper', index: 'Shipper', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CarType', title: _getLang('L_BaseLookup_CarType', 'Models'), index: 'CarType', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Consignee', title: 'Consignee', index: 'Consignee', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CargoType', title: _getLang('L_DNApproveManage_CargoType', 'Cargo Type'), index: 'CargoType', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Notify', title: 'Notify', index: 'Notify', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Shipto', title: 'Shipto', index: 'Shipto', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Billto', title: 'Billto', index: 'Billto', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Region', title: _getLang('L_CitySetup_Region', 'Region') + _getLang('L_BSCODESetup_Cd', 'Party Code'), index: 'Region', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'RegionNm', title: _getLang('L_CitySetup_Region', 'Region') + _getLang('L_BaseLookup_Nm', 'Name'), index: 'RegionNm', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'Qtyu', title: _getLang('L_BaseLookup_Qtyu', 'Unit'), index: 'Qtyu', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'Gwu', title: _getLang('L_BaseLookup_Gwu', 'Unit of G.W.'), index: 'Gwu', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'FreightTerm', title: 'Freight Term', index: 'FreightTerm', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'Horn', title: 'L_Horn', index: 'horn', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'Battery', title: _getLang('L_DryCell', 'dry battary or not?'), index: 'Battery', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'Qty', title: _getLang('L_BaseLookup_Qty', 'QTY'), index: 'Qty', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'Nw', title: _getLang('L_BaseLookup_Nw', 'Net weight'), index: 'Nw', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'Cbm', title: _getLang('L_DNApproveManage_Cbm', 'Volume'), index: 'Cbm', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'Gw', title: _getLang('L_BaseLookup_Gw', 'GW'), index: 'Gw', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'PkgNum', title: 'PkgNum', index: 'PkgNum', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'PkgUnit', title: 'PkgUnit', index: 'PkgUnit', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'PkgUnitDesc', title: 'PkgUnitDesc', index: 'PkgUnitDesc', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'CostCenter', title: 'CostCenter', index: 'CostCenter', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'TradeTerm', title: 'TradeTerm', index: 'TradeTerm', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'TradetermDescp', title: 'TradetermDescp', index: 'TradetermDescp', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'Tcbm', title: 'Tcbm', index: 'Tcbm', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'State', title: 'State', index: 'State', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'CombineOther', title: 'CombineOther', index: 'CombineOther', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'CentDecl', title: 'CentDecl', index: 'CentDecl', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'Amount1', title: 'Amount1', index: 'Amount1', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'Etd', title: 'Etd', index: 'Etd', width: 80, align: 'left', sorttype: 'string', hidden: true }
    ]
};
LookUpConfig.DnInfoUrl = "Common/GetDnQueryData";
LookUpConfig.GetDnInfoAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=smdnsmbd&GROUP_ID=" + groupId + "&DN_NO%",
        autoCompParams: "DN_NO&TRAN_TYPE=showValue,DN_NO,TRAN_TYPE,TRAN_TYPE_DESCP,POL,POL_NM,PORTE_CD,PORTE_DESCP,STATE,POD,POD_NM,COMBINE_INFO,SHIP_MARK," +
            "GOODS,SHIPPER,CAR_TYPE,CONSIGNEE,CARGO_TYPE,NOTIFY,SHIPTO,BILLTO,REGION,REGION_NM,QTYU,GWU,FREIGHT_TERM,HORN,BATTERY,QTY,NW,CBM,GW,PKG_NUM,PKG_UNIT,PKG_UNIT_DESC,COST_CENTER,TRADE_TERM,TCBM,STATE," +
            "TRADETERM_DESCP,COMBINE_OTHER,CENT_DECL,AMOUNT1,ETD",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        },
        autoClearFunc: function (elem, event, rowid) {
            clearFn($grid, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.SmdnpLookup = {
    caption: _getLang("L_BaseLookup_SerDNDetail", "Search DN Detail"),
    sortname: "IpartNo",
    sortorder: "asc",
    refresh: false,
    columns: [
        { name: 'DnNo', title: 'DnNo', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'IpartNo', title: _getLang('L_DNApproveManage_IpartNo', 'Internal Model Name'), index: 'IpartNo', width: 200, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'OpartNo', title: _getLang('L_DNApproveManage_OpartNo', 'External Model Name'), index: 'OpartNo', width: 200, align: 'left', sorttype: 'string', hidden: false },
        { name: 'GoodsDescp', title: _getLang('L_DNApproveManage_GoodsDescp', 'Name of product'), index: 'GoodsDescp', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'ProdDescp', title: _getLang('L_DNApproveManage_ProdDescp', 'Product Name'), index: 'ProdDescp', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'ProdSpec', title: _getLang('L_DNApproveManage_ProdSpec', 'Product Description'), index: 'ProdSpec', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'PoNo', title: 'PO NO', index: 'PoNo', width: 300, align: 'left', sorttype: 'string', hidden: false },
        { name: 'SoNo', title: 'SO NO', index: 'SoNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'PartNo', title: _getLang('L_DNApproveManage_PartNo', 'Customer Part No'), index: 'PartNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'OhsCode', title: _getLang('L_DNApproveManage_OhsCode', 'HS Code of  Exportation Country'), index: 'OhsCode', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'IhsCode', title: _getLang('L_DNApproveManage_HisCode', 'HS Code of Importation Country'), index: 'IhsCode', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Qty', title: 'QTY', index: 'Qty', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Qtyu', title: _getLang('L_BaseLookup_Unit', 'Unit'), index: 'Qtyu', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Cbm', title: 'CBM', index: 'Cbm', width: 70, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false },
        { name: 'Gw', title: 'GW', index: 'Gw', width: 150, align: 'right', sorttype: 'string', hidden: false }
    ]
}


LookUpConfig.SmsmLookup = {
    caption: _getLang("L_BaseLookup_SerBl", "Search Bill"),
    sortname: "TranType",
    sortorder: "asc",
    refresh: false,
    columns: [
        { name: 'TranType', title: _getLang('L_ForecastQueryData_TranType', 'Tran Type'), index: 'TranType', width: 80, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'F:FCL;L:LCL;A:AIR;D:INLAND EXPRESS;E:EXPRESS;R:Railroad;T:TRUCK;' }, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'DnNo', title: 'Dn No', index: 'DnNo', width: 110, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'MasterNo', title: _getLang('L_ActSetup_BlNo', 'B/L#'), index: 'MasterNo', width: 110, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'CombineInfo', title: _getLang('L_DNApproveManage_CombineInfo', 'Combined DN Info.'), index: 'CombineInfo', width: 250, align: 'left', sorttype: 'string', hidden: false, classes: "normal-white-space", sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'ShipmentInfo', title: _getLang('L_BaseLookup_ShipmentInfo', 'And Shipment messages'), index: 'ShipmentInfo', width: 250, align: 'left', sorttype: 'string', hidden: false, classes: "normal-white-space", sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Status', title: _getLang('L_GroupRelation_stnStatus', 'Status'), index: 'Status', width: 80, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'A:' + _getLang('L_UserQuery_Unprocess_en', 'UnProcessed') + ';B:' + _getLang('L_BSTSetup_Book', 'Booking') + ';C:' + _getLang('L_UserQuery_ComBA', 'Confirm Booking Agent') + ';D:' + _getLang('L_UserQuery_Call', 'Order Container') + ';I:' + _getLang('L_UserQuery_In', 'Gate In') + ';P:' + _getLang('L_UserQuery_SealCnt', 'Container Sealed') + ';O:' + _getLang('L_UserQuery_Out', 'Gate Out') + ';G:' + _getLang('L_UserQuery_Declara', 'Declaration') + ';H:' + _getLang('L_UserQuery_Release', 'Release') + ';V:' + _getLang('L_BSCSDateQuery_Cancel', 'Cancel') + ';Z:' + _getLang('L_UserQuery_Return', 'Return Cargo') }, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PorCd', title: 'POR', index: 'PorCd', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PorName', title: 'POR Name', index: 'PorName', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'PolCd', title: 'POL', index: 'PolCd', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PolName', title: 'POL Name', index: 'PolName', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'ViaCd', title: 'VIA', index: 'ViaCd', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'ViaName', title: 'VIA Name', index: 'ViaName', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'PodCd', title: 'POD', index: 'PodCd', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PodName', title: 'POD Name', index: 'PodName', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'DestCd', title: 'DEST', index: 'DestCd', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'DestName', title: 'DEST Name', index: 'DestName', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'Cur', title: _getLang('L_InvPkgSetup_Cur', 'Currency'), index: 'Cur', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Qty', title: _getLang('L_BaseLookup_Qty', 'QTY'), index: 'Qty', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Qtyu', title: _getLang('L_BaseLookup_Qtyu', 'Unit'), index: 'Qtyu', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Nw', title: _getLang('L_BaseLookup_Nw', 'Net weight'), index: 'Nw', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Gw', title: _getLang('L_BaseLookup_Gw', 'GW'), index: 'Gw', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Gwu', title: _getLang('L_BaseLookup_Gwu', 'Unit of G.W.'), index: 'Gwu', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Cbm', title: 'CBM', index: 'Cbm', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Cw', title: 'CW', index: 'Cw', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PkgNum', title: 'Total Package', index: 'PkgNum', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PkgUnit', title: 'Unit', index: 'PkgUnit', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Cmp', title: 'Location', index: 'Cmp', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

LookUpConfig.Smsm4SMIRMLookup = {
    caption: _getLang("L_BaseLookup_SerBl", "Search Bill"),
    sortname: "TranType",
    sortorder: "asc",
    refresh: false,
    columns: [
        { name: 'TranType', title: _getLang('L_ForecastQueryData_TranType', 'Tran Type'), index: 'TranType', width: 80, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'F:FCL;L:LCL;A:AIR;D:INLAND EXPRESS;E:EXPRESS;R:Railroad;T:TRUCK;' }, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'DnNo', title: 'Dn No', index: 'DnNo', width: 110, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'MasterNo', title: _getLang('L_AirTransport_MasterNo', 'Master AWB#'), index: 'MasterNo', width: 110, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'HouseNo', title: _getLang('L_AirTransport_HouseNo', 'House AWB#'), index: 'HouseNo', width: 110, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'CntrNo', title: _getLang('L_DNFlowManage_CntrNo', 'Container#'), index: 'CntrNo', width: 110, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'CombineInfo', title: _getLang('L_DNApproveManage_CombineInfo', 'Combined DN Info.'), index: 'CombineInfo', width: 250, align: 'left', sorttype: 'string', hidden: false, classes: "normal-white-space", sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'ShipmentInfo', title: _getLang('L_BaseLookup_ShipmentInfo', 'And Shipment messages'), index: 'ShipmentInfo', width: 250, align: 'left', sorttype: 'string', hidden: false, classes: "normal-white-space", sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Status', title: _getLang('L_GroupRelation_stnStatus', 'Status'), index: 'Status', width: 80, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'A:' + _getLang('L_UserQuery_Unprocess_en', 'UnProcessed') + ';B:' + _getLang('L_BSTSetup_Book', 'Booking') + ';C:' + _getLang('L_UserQuery_ComBA', 'Confirm Booking Agent') + ';D:' + _getLang('L_UserQuery_Call', 'Order Container') + ';I:' + _getLang('L_UserQuery_In', 'Gate In') + ';P:' + _getLang('L_UserQuery_SealCnt', 'Container Sealed') + ';O:' + _getLang('L_UserQuery_Out', 'Gate Out') + ';G:' + _getLang('L_UserQuery_Declara', 'Declaration') + ';H:' + _getLang('L_UserQuery_Release', 'Release') + ';V:' + _getLang('L_BSCSDateQuery_Cancel', 'Cancel') + ';Z:' + _getLang('L_UserQuery_Return', 'Return Cargo') }, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'TradeTerm', title: _getLang('L_LogisticsRule_Term', 'Incoterm'), index: 'TradeTerm', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'TradetermDescp', title: 'TradetermDescp', index: 'TradetermDescp', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Region', title: 'Region', index: 'Region', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Goods', title: _getLang('L_SMIPM_Goods', 'Name of goods'), index: 'Goods', width: 250, align: 'left', sorttype: 'string', hidden: false, classes: "normal-white-space", sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PorCd', title: 'POR', index: 'PorCd', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PorName', title: 'POR Name', index: 'PorName', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'PolCd', title: 'POL', index: 'PolCd', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PolName', title: 'POL Name', index: 'PolName', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'ViaCd', title: 'VIA', index: 'ViaCd', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'ViaName', title: 'VIA Name', index: 'ViaName', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'PodCd', title: 'POD', index: 'PodCd', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PodName', title: 'POD Name', index: 'PodName', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'DestCd', title: 'DEST', index: 'DestCd', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'DestName', title: 'DEST Name', index: 'DestName', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'ShCd', title: 'ShipperCD', index: 'ShCd', width: 100, align: 'left', sorttype: 'string', hidden: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'ShNm', title: 'ShipperNm', index: 'ShNm', width: 100, align: 'left', sorttype: 'string', hidden: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'FcCd', title: 'FI Customer', index: 'FcCd', width: 100, align: 'left', sorttype: 'string', hidden: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'FcNm', title: 'FI Name', index: 'FcNm', width: 100, align: 'left', sorttype: 'string', hidden: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'CsCd', title: 'Consignee', index: 'CsCd', width: 100, align: 'left', sorttype: 'string', hidden: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'CsNm', title: 'Consignee Name', index: 'CsNm', width: 100, align: 'left', sorttype: 'string', hidden: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'Carrier', title: 'Carrier', index: 'Carrier', width: 100, align: 'left', sorttype: 'string', hidden: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'CarrierNm', title: 'Carrier Name', index: 'CarrierNm', width: 100, align: 'left', sorttype: 'string', hidden: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'LspNo', title: 'Lsp No', index: 'LspNo', width: 100, align: 'left', sorttype: 'string', hidden: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'LspNm', title: 'Lsp Name', index: 'LspNm', width: 100, align: 'left', sorttype: 'string', hidden: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" },
        { name: 'Cur', title: _getLang('L_InvPkgSetup_Cur', 'Currency'), index: 'Cur', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Gvalue', title: _getLang('L_BaseLookup_Gvalue', 'value'), index: 'Gvalue', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Qty', title: _getLang('L_BaseLookup_Qty', 'QTY'), index: 'Qty', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Qtyu', title: _getLang('L_BaseLookup_Qtyu', 'Unit'), index: 'Qtyu', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Nw', title: _getLang('L_BaseLookup_Nw', 'Net weight'), index: 'Nw', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Gw', title: _getLang('L_BaseLookup_Gw', 'GW'), index: 'Gw', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Gwu', title: _getLang('L_BaseLookup_Gwu', 'Unit of G.W.'), index: 'Gwu', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Cbm', title: 'CBM', index: 'Cbm', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Cw', title: 'CW', index: 'Cw', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PkgNum', title: 'Total Package', index: 'PkgNum', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PkgUnit', title: 'Unit', index: 'PkgUnit', width: 100, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Cmp', title: 'Location', index: 'Cmp', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}
LookUpConfig.Smsm4SMIRMUrl = "ActManage/GetSmsmForSMIPMLookup";

LookUpConfig.SmbidModel = {
    caption: _getLang("L_Layout_FeeDetail", "Cost Details"),
    sortname: "ShipmentId",
    sortorder: "asc",
    refresh: false,
    columnID: "UId",
    rows: 10000,
    columns: [
        { name: 'UId', title: 'UId', index: 'UId', width: 100, align: 'left', sorttype: 'string', hidden: true },
        { name: 'TranType', title: _getLang('L_ForecastQueryData_TranType', 'Tran Type'), index: 'TranType', width: 80, align: 'left', sorttype: 'string', hidden: false, formatter: "select", remark: 'F:FCL;L:LCL;A:AIR;T:INLAND TRADE;B:CUSTOM CLERANCE;E:Express;R:Railroad;P:Trucker;C:Trailer', editoptions: { value: 'F:FCL;L:LCL;A:AIR;T:INLAND TRADE;B:CUSTOM CLERANCE;E:Express;R:Railroad;P:Trucker;C:Trailer' }, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'QexRate', title: _getLang('L_BaseLookup_Scripts_7', 'Local Estimated rate'), index: 'QexRate', width: 100, align: 'right', sorttype: 'float', hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Bamt', title: _getLang('L_ActSetup_Amt', 'Payment amount'), index: 'Bamt', width: 100, align: 'right', sorttype: 'float', hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Lamt', title: _getLang('L_ActManage_LocIvAmt', 'Local payment amount'), index: 'Lamt', width: 100, align: 'right', sorttype: 'float', hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'ExRate', title: _getLang('L_QTManage_ExRate', 'Exchange rate of application'), index: 'ExRate', width: 100, align: 'right', sorttype: 'float', hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'UnitPrice', title: _getLang('L_QTManage_ExRate', 'Exchange rate of application'), index: 'ExRate', width: 100, align: 'right', sorttype: 'float', hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Qty', title: _getLang('L_BaseLookup_Qty', 'QTY'), index: 'Qty', width: 100, align: 'right', sorttype: 'float', hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Tax', title: _getLang('L_ActDeatilManage_Views_69', 'tax rate'), index: 'Tax', width: 100, align: 'right', sorttype: 'float', hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Qlamt', title: _getLang('L_ActManage_LocWithAmt', 'Local Estimated amount'), index: 'Qlamt', width: 100, align: 'right', sorttype: 'float', hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'ChgUnit', title: _getLang('L_BaseLookup_Scripts_8', 'Billing unit'), index: 'ChgUnit', sorttype: 'string', width: 80, hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'BiRemark', title: _getLang('L_BaseLookup_Scripts_9', 'Please note'), index: 'BiRemark', sorttype: 'string', width: 80, hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'DebitTo', title: 'Debit To', index: 'DebitTo', sorttype: 'string', width: 80, hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'DebitNm', title: 'Debit To Name', index: 'DebitNm', sorttype: 'string', width: 80, hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Status', title: _getLang('L_BaseLookup_Scripts_10', 'Whether accord with'), index: 'Status', sorttype: 'string', width: 80, hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'CheckDescp', title: _getLang('L_ActSetup_CheckDescp', 'Rejected reason/Remark by TPV LST:'), index: 'CheckDescp', sorttype: 'string', width: 80, hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'DebitNo', title: _getLang('L_ActCheckQueryView_Views_30', 'LSP Statement#'), index: 'DebitNo', sorttype: 'string', width: 80, hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Cur', title: _getLang('L_ActQuery_Cur', 'Payment currency'), index: 'Cur', sorttype: 'string', width: 80, hidden: true, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'ShipmentId', title: 'Shipment Id', index: 'ShipmentId', sorttype: 'string', width: 100, hidden: false, init: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'BlNo', title: _getLang('L_ActSetup_BlNo', 'B/L#'), index: 'BlNo', sorttype: 'string', width: 120, hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'DebitDate', title: _getLang('L_BaseLookup_DebitDate', 'Debit Date'), init: true, index: 'DebitDate', sorttype: 'date', width: 100, hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'ChgCd', title: _getLang('L_SLCLQuery_F1', 'Expense') + _getLang('L_BSCODESetup_Cd', 'Party Code'), index: 'ChgCd', sorttype: 'string', width: 80, hidden: false, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'ChgDescp', title: _getLang('L_SLCLQuery_F1', 'Expense') + _getLang('L_SMCHGSetup_Descp', 'Description'), index: 'ChgDescp', sorttype: 'string', width: 120, hidden: false, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Qcur', title: _getLang('L_BaseLookup_WithholdCur', 'Estimated Currency'), index: 'Qcur', sorttype: 'string', width: 80, hidden: false, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Qamt', title: _getLang('L_BaseLookup_WithholdAmt', 'Estimated Amount'), index: 'Qamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'QunitPrice', title: _getLang('L_BaseLookup_WithholdPrice', 'Estimated Price'), index: 'QunitPrice', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'float', hidden: false, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'QchgUnit', title: _getLang('L_BaseLookup_Unit', 'Unit'), index: 'QchgUnit', sorttype: 'string', width: 80, hidden: false, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Qqty', title: _getLang('L_BaseLookup_WithholdQty', 'Estimated Quantity'), index: 'Qqty', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, sorttype: 'string', width: 100, hidden: false, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Qqtyu', title: _getLang('L_BaseLookup_Unit', 'Unit'), index: 'Qqtyu', sorttype: 'string', width: 80, hidden: false, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Remark', title: _getLang('L_ActDeatilManage_Views_71', 'Remark for Accrued Expenses'), index: 'Remark', sorttype: 'string', width: 200, hidden: false, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'MasterNo', title: 'Master No', index: 'MasterNo', sorttype: 'string', width: 100, hidden: false, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'CntrInfo', title: 'Container Info', index: 'CntrInfo', sorttype: 'string', width: 100, hidden: false, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PodCd', title: 'Pod', index: 'PodCd', sorttype: 'string', width: 100, hidden: false, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'InvoiceInfo', title: 'Invoice Info.', index: 'InvoiceInfo', sorttype: 'string', width: 200, hidden: false, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'BiRemark', title: _getLang('L_ActDeatilManage_BiRemark', 'LSP Reference'), index: 'BiRemark', sorttype: 'string', width: 200, hidden: false, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'DecNo', title: 'Reference NO', index: 'DecNo', sorttype: 'string', width: 200, hidden: false, sopt: ['bt', 'eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
};

LookUpConfig.EDMLookup = {
    caption: _getLang("L_BaseLookup_SerEDM", "Search EDM"),
    sortname: "TpltName",
    sortorder: "asc",
    refresh: false,
    multiselect: true,
    columns: [
        { name: 'UId', title: 'UId', index: 'UId', width: 70, sorttype: 'string', hidden: true },
        { name: 'TpltType', title: _getLang('L_BaseLookup_TpltType', 'Template Type'), index: 'TpltType', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TpltName', title: _getLang('L_BaseLookup_TpltName', 'Template Name'), index: 'TpltName', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TpltContent', title: 'TpltContent', index: 'TpltContent', width: 70, align: 'left', sorttype: 'string', hidden: false },
    ]
};


LookUpConfig.GetSmsmAuto = function (groupId, $grid, autoFn, clearFn) {
    var op =
    {
        autoCompDt: "dt=smsm&GROUP_ID=" + groupId + "&SHIPMENT_ID=",
        autoCompParams: "SHIPMENT_ID=showValue,SHIPMENT_ID",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        },
        autoClearFunc: function (elem, event, rowid) {
            if (clearFn) clearFn($grid, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.GetSmsmiAuto = function (groupId, $grid, autoFn, clearFn) {
    var op =
    {
        autoCompDt: "dt=smsmi&GROUP_ID=" + groupId + "&SHIPMENT_ID=",
        autoCompParams: "SHIPMENT_ID=showValue,SHIPMENT_ID",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        },
        autoClearFunc: function (elem, event, rowid) {
            if (clearFn) clearFn($grid, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.BankInfoLookup = {
    caption: _getLang("L_BaseLookup_Scripts_11", "Bank information inquiry"),
    sortname: "BankInfo",
    sortorder: "asc",
    refresh: false,
    columns: [
        { name: 'UId', title: 'UId', index: 'UId', width: 70, sorttype: 'string', hidden: true },
        { name: 'Cmp', title: _getLang('L_DRule_LspNoo', 'Carrier code'), index: 'Cmp', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'LspNm', title: _getLang('L_AirQuery_LspNm', 'LSP Name'), index: 'LspNm', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Crncy', title: _getLang('L_InvPkgSetup_Cur', 'Currency'), index: 'Crncy', width: 60, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CollectBank', title: _getLang('L_DNManage_ReBank', 'Name of Bank'), index: 'CollectBank', sorttype: 'string', width: 150, hidden: false },
        { name: 'AccountName', title: _getLang('L_DNManage_Ac', 'Account Name'), index: 'AccountName', sorttype: 'string', width: 250, hidden: false },
        { name: 'BankInfo', title: _getLang('L_DNManage_BkAc', 'Bank Account'), index: 'BankInfo', sorttype: 'string', width: 250, hidden: false },
        { name: 'SwiftCode', title: 'Swift Code', index: 'SwiftCode', sorttype: 'string', width: 250, hidden: false },
        { name: 'BankType', title: _getLang("L_BankInfo_BankType", "银行类型"), index: 'BankType', sorttype: 'string', width: 250, hidden: false },
        { name: 'remark', title: _getLang('L_BSCSSetup_Remark', 'Remark'), index: 'remark', width: 200, align: 'left', sorttype: 'string', hidden: false }
    ]
}

LookUpConfig.BankInfoUrl = "Common/GetBankInfoData";

LookUpConfig.InsInfoLookup = {
    caption: _getLang("L_BaseLookup_Scripts_12", "Insurer inquiry"),
    sortname: "CreateBy",
    sortorder: "asc",
    refresh: false,
    columns: [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'UFid', title: 'U FID', index: 'UFid', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'SeqNo', title: 'SeqNo', index: 'SeqNo', sorttype: 'string', width: 60, editable: false, hidden: true },
        { name: 'JobNo', title: _getLang('L_ContractSetup_JobNo', 'Job#'), index: 'JobNo', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: "JobDate", title: _getLang("L_ExchangeRate_Edate", "Date"), index: "JobDate", width: 150, align: "left", sorttype: "string", formatter: "date", formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d" }, hidden: false },
        { name: 'JobCmp', title: _getLang('L_MailGroupSetup_Cmp', 'Company'), index: 'JobCmp', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'CreateBy', title: _getLang('L_SMIPR_CreateBy', 'Applicant'), index: 'CreateBy', sorttype: 'string', width: 100, editable: false, hidden: false },
        { name: 'Remark', title: _getLang('L_BaseLookup_Scripts_13', 'State description'), index: 'Remark', sorttype: 'string', width: 300, editable: true, hidden: false }
    ]
}
LookUpConfig.InsInfoUrl = "ActManage/GetInsFeelookup";
LookUpConfig.GetInsInfoAuto = function (groupId, $grid, autoFn, clearFn) {
    var op =
    {
        autoCompDt: "dt=smipr&CREATE_BY%",
        autoCompParams: "CREATE_BY=showValue,CREATE_BY",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        },
        autoClearFunc: function (elem, event, rowid) {
            if (clearFn) clearFn($grid, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.BslcpolLookup = {
    caption: _getLang("L_BaseLookup_Scripts_14", "Quotation for Hong Kong"),
    sortname: "Pol",
    sortorder: "asc",
    refresh: false,
    multiselect: true,
    columns: [
        { name: 'Pol', title: 'POL', index: 'Pol', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PolDescp', title: 'POL Name', index: 'PolDescp', width: 100, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'], classes: "normal-white-space" }
    ]
}

LookUpConfig.BsdistLookup = {
    caption: "區域查询",
    sortname: "DistCd",
    refresh: false,
    columns: [
        { name: "DistCd", title: "區域", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "DistNm", title: "區域名稱", init: true, width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'ne', 'eq', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: "StateCd", title: "省份", init: true, width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['cn', 'ne', 'eq', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
    ]
}

LookUpConfig.BsdistCdAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=bsdist&GROUP_ID=" + groupId + "&DIST_CD%",
        autoCompParams: "STATE_CD&DIST_NM&DIST_CD=showValue,DIST_CD,DIST_NM,STATE_CD",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.BSTPORTLookup = {
    caption: _getLang("L_BaseLookup_BSTPort", "CityData"),
    sortname: "PortCd",
    refresh: false,
    columns: [
        { name: 'PortCd', title: _getLang('L_CitySetup_PortCd', 'City Code'), index: 'PortCd', width: 120, sorttype: 'string', hidden: false },
        { name: 'CntryCd', title: _getLang('L_CitySetup_CntryCd', 'Country Code'), index: 'CntryCd', width: 120, init: true, sorttype: 'string', viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PortNm', title: _getLang('L_CitySetup_PortNm', 'City Name'), index: 'PortNm', width: 200, sorttype: 'string', hidden: false },
        { name: 'State', title: _getLang('L_BsStateQuery_StateCd', 'State Code'), index: 'State', width: 150, sorttype: 'string', hidden: false },
        { name: 'Region', title: _getLang('L_CitySetup_Region', 'Region'), index: 'Region', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Ns', title: _getLang('L_CitySetup_Ns', 'Longitude'), index: 'Ns', width: 150, align: 'right', formatter: 'number', formatoptions: { decimalPlaces: 6 } },
        { name: 'Ew', title: _getLang('L_CitySetup_Ew', 'Latitude'), index: 'Ew', width: 150, align: 'right', formatter: 'number', formatoptions: { decimalPlaces: 6 } }
    ]
}

LookUpConfig.BSADDRLookup = {
    caption: _getLang("L_BaseLookup_BSTPort", "CityData"),
    sortname: "PortCd",
    refresh: false,
    columns: [
        { name: 'CntryCd', title: _getLang('L_TruckPort_CntryCd', 'Country Code'), index: 'CntryCd', sorttype: 'string', editable: false, hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'PortCd', title: _getLang('L_TruckPort_PortId', 'City Code'), index: 'PortCd', sorttype: 'string', editable: false, hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'AddrCode', title: _getLang('L_TruckPort_AddrCode', 'Address Code'), index: 'AddrCode', sorttype: 'string', width: 100, hidden: false, editable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Addr', title: _getLang('L_TruckPort_Addr', 'Address'), index: 'Addr', sorttype: 'string', width: 250, hidden: false, editable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'OuterFlag', title: _getLang('L_TruckPort_OuterFlag', 'Third party WH'), index: 'OuterFlag', sorttype: 'string', width: 100, hidden: false, editable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: 'FinalWh', title: _getLang('L_TruckPort_FinalWh', 'Final WH'), index: 'FinalWh', sorttype: 'string', width: 100, hidden: false, editable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }
    ]
}

LookUpConfig.MultWarehouseLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search") + "Warehourse Type",
    sortname: "WsCd",
    multiselect: true,
    refresh: false,
    columns: [
        { name: "Cmp", title: "Cmp", width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "WsCd", title: _getLang("L_BSCODESetup_Cd", "Party Code"), width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "WsNm", title: "Name", init: true, width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.WarehouseLookup = {
    caption: _getLang("L_BaseLookup_Ser", "Search") + "Warehourse Type",
    sortname: "WsCd",
    refresh: false,
    columns: [
        { name: "Cmp", title: "Cmp", width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "WsCd", title: _getLang("L_BSCODESetup_Cd", "Party Code"), width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "WsNm", title: "Name", init: true, width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

//拖卡車BSCODE=TCAR查询
LookUpConfig.TcarLookup = {
    caption: "Carrier Code",
    sortname: "Cd",
    rows: 200,
    refresh: false,
    multiselect: false,
    columns: [
        { name: "Cd", title: _getLang("L_BSCODESetup_Cd", "Party Code"), width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "CdDescp", title: "Name", init: true, width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}
LookUpConfig.GetTcarAuto = function (groupId, $grid, autoFn) {
    var op =
    {
        autoCompDt: "dt=bsc&&CD_TYPE='TCAR'&GROUP_ID=" + groupId + "&CD%",
        autoCompParams: "CD=showValue,CD,CD_DESCP",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        }
    };
    return op;
}

LookUpConfig.MutiQATypeUrl = "Common/GetQAType";
LookUpConfig.MutiQATypeLookup = {
    caption: "QA Type " + _getLang("L_BaseLookup_Ser", "Search"),
    sortname: "Cd",
    rows: 200,
    refresh: false,
    multiselect: true,
    columns: [
        { name: "Cd", title: _getLang("L_BSCODESetup_Cd", "Party Code"), width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "CdDescp", title: "Name", init: true, width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

LookUpConfig.MutiCntrTypeUrl = "Common/GetCntrType";
LookUpConfig.MutiCntrTypeLookup = {
    caption: "Tons",
    sortname: "ChgCd",
    rows: 200,
    refresh: false,
    multiselect: true,
    columns: [
        { name: "ChgCd", title: _getLang("L_BSCODE_Cd", "Code"), width: 60, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
        { name: "ChgDescp", title: "Name", init: true, width: 300, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
}

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}
