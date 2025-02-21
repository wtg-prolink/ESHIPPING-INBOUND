var QueryConfig = {};

QueryConfig.SMFSC = [
    { name: 'UId', title: 'ID', index:'UId', sorttype: 'string', hidden: true },
    { name: 'GroupId', title: 'Group ID', index:'GroupId', sorttype: 'string', hidden: true },
    { name: 'Cmp', title: 'Company', index: 'Cmp', width: 70, align: 'left', sorttype: 'string', classes: "uppercase", hidden: false },
    { name: 'CmpNm', title: 'Name', index: 'CmpNm', width: 120, align: 'left', sorttype: 'string', classes: "uppercase", hidden: false },
    { name: 'Carrier', title: 'Carrier', index: 'Carrier', width: 70, align: 'left', sorttype: 'string', hidden: false },
    //{ name: 'Carrier', title: 'Carrier Name', index: 'Carrier', width: 120, align: 'left', sorttype: 'string', hidden: false },
    { name: 'EffectDate', title: 'Effect Date', index: 'EffectDate', width: 120, align: 'left', sorttype: 'string', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } },
    { name: 'Farea', title: 'POL', index: 'Farea', width: 70, align: 'left', sorttype: 'string', hidden: false },
    { name: 'Area', title: 'To Area', index: 'Area', width: 70, align: 'left', sorttype: 'string', hidden: false },
    { name: 'AreaNm', title: 'Area Name', index: 'AreaNm', width: 70, align: 'left', sorttype: 'string', hidden: false },
    { name: 'Cur', title: 'Currency', index: 'Cur', width: 70, align: 'left', sorttype: 'string', hidden: false },
    { name: 'Gp20', title: '20 GP', index: 'Gp20', width: 100, align:'right', sorttype: 'float',
    	formatter: 'number', 
    	formatoptions: { 
    		decimalSeparator: ".", 
    		thousandsSeparator: ",", 
    		decimalPlaces: 2, 
    		defaultValue: '0.00'
    	}
    },
    { name: 'Gp40', title: '40 GP', index: 'Gp40', width: 100, align:'right', sorttype: 'float',
    	formatter: 'number', 
    	formatoptions: { 
    		decimalSeparator: ".", 
    		thousandsSeparator: ",", 
    		decimalPlaces: 2, 
    		defaultValue: '0.00'
    	}
    },
    { name: 'Hq40', title: '40 HQ', index: 'Hq40', width: 100, align:'right', sorttype: 'float',
    	formatter: 'number', 
    	formatoptions: { 
    		decimalSeparator: ".", 
    		thousandsSeparator: ",", 
    		decimalPlaces: 2, 
    		defaultValue: '0.00'
    	}
    },
    { name: 'OthRate', title: 'Other', index: 'OthRate', width: 100, align:'right', sorttype: 'float',
    	formatter: 'number', 
    	formatoptions: { 
    		decimalSeparator: ".", 
    		thousandsSeparator: ",", 
    		decimalPlaces: 2, 
    		defaultValue: '0.00'
    	}
    }
];

QueryConfig.SMCC = [
    { name: 'UId', title: 'ID', index:'UId', sorttype: 'string', hidden: true },
    { name: 'GroupId', title: 'Group ID', index:'GroupId', sorttype: 'string', hidden: true },
    { name: 'Cmp', title: 'Location', index: 'Cmp', width: 100, align: 'left', sorttype: 'string', classes: "uppercase", hidden: false },
    { name: 'Company', title: _getLang("L_BSCSSetup_Cmp", "客户代码"), index: 'Company', width: 70, align: 'left', sorttype: 'string', classes: "uppercase", hidden: false },
    { name: 'Dep', title: _getLang("L_ShipmentID_CreateDep", "部门"), index: 'Dep', width: 70, align: 'left', sorttype: 'string', hidden: false },
    { name: 'CostCenter', title: _getLang("L_BaseColModel_CostCenter", "成本中心"), index: 'CostCenter', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'ShortDescp', title: _getLang("L_BaseColModel_ShortDescp", "简要说明"), index: 'ShortDescp', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'Descp', title: _getLang("L_GroupRelation_comDesc", "描述"), index: 'Descp', width: 120, align: 'left', sorttype: 'string', hidden: false },
    { name: 'Principal', title: _getLang("L_BaseColModel_Principal", "负责人"), index: 'Principal', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'CreateBy', title: _getLang("L_Bsdate_CreateBy", "创建人"), index: 'CreateBy', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'CreateDate', title: _getLang("L_Bsdate_CreateDate", "创建时间"), index: 'CreateDate', width: 120, align: 'left', formatter: "date", hidden: false, formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d h:i A" } },
    { name: 'ModifyBy', title: _getLang("L_Bsdate_ModifyBy", "修改人"), index: 'ModifyBy', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'ModifyDate', title: _getLang("L_Bsdate_ModifyDate", "修改时间"), index: 'ModifyDate', width: 120, align: 'left', formatter: "date", hidden: false, formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d h:i A" } }
];

QueryConfig.SMWH = [
    { name: 'UId', title: 'u id', index: 'UId', sorttype: 'string', hidden: true, width:60 },
    { name: 'GroupId', title: 'Group ID', index: 'GroupId', sorttype: 'string', width:60 },
    { name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', width:60 },
    { name: 'MfNo', title: _getLang("L_GateAnalysis_MfNo", "厂区"), index: 'MfNo', width: 70, align: 'left', sorttype: 'string', hidden: false, classes: "uppercase" },
    { name: 'WsCd', title: _getLang("L_GateAnalysis_WsCd", "仓库代码"), index: 'WsCd', width: 70, align: 'left', sorttype: 'string', hidden: false, classes: "uppercase" },
    { name: 'WsNm', title: _getLang("L_GateSetup_WsNm", "仓库名称"), index: 'WsNm', width: 200, align: 'left', sorttype: 'string', hidden: false },
    { name: 'GateNumber', title: _getLang("L_GateSetup_GateNumber", "月台数"), index: 'GateNumber', width: 60, align: 'right', formatter: 'integer', hidden: false },
    { name: 'ProductLine', title: _getLang("L_GateReserveSetup_ProLine", "生产线"), index: 'ProductLine', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'RefGate', title: _getLang("L_GateSetup_RefGate", "月台号码"), index: 'RefGate', width: 80, align: 'left', sorttype: 'string', hidden: false },
    { name: 'Pic', title: _getLang("L_GateSetup_Pic", "负责人"), index: 'Pic', width: 120, align: 'left', sorttype: 'string', hidden: false},
    { name: 'DlvAreaNm', title: _getLang("L_DNApproveManage_RegionNm", "地区名称"), index: 'DlvAreaNm', width: 150, align: 'left', sorttype: 'string', hidden: false },
    { name: 'DlvAddrNm', title: _getLang("L_InvPkgSetup_ShprAddr", "Address"), index: 'DlvAddrNm', width: 200, align: 'left', sorttype: 'string', hidden: false },
    { name: 'Remark', title: _getLang("L_BSCSSetup_Remark", "备注"), index: 'Remark', width: 300, align: 'left', sorttype: 'string', hidden: false },
    { name: 'DlvAddr', title: _getLang("L_SMWH_AddrCode", "Address Code"), index: 'DlvAddr', width: 300, align: 'left', sorttype: 'string', hidden:false }
];

QueryConfig.BSCAA = [
    { name: 'UId', title: 'UId', index: 'UId', sorttype: 'string', width: 100, hidden: true },
    { name: 'Carrier', title: 'Carrier', index: 'Carrier', sorttype: 'string', width: 100, hidden: false },
    { name: 'CarrierNm', title: 'Carrier Name', index: 'CarrierNm', sorttype: 'string', width: 100, hidden: false },
    { name: 'Area', title: 'Area', index: 'Area', sorttype: 'string', width: 100, hidden: false },
    { name: 'AreaNm', title: 'AreaName', index: 'AreaNm', sorttype: 'string', width: 100, hidden: false },
    { name: 'AtdAtp', title: _getLang("L_BaseColModel_AtdAtp", "计费日期"), index: 'AtdAtp', sorttype: 'string', width: 100, hidden: false }
];

QueryConfig.EDITARGET = [
    { name: 'UId', title: 'ID', index:'UId', sorttype: 'string', hidden: true },
    { name: 'GroupId', title: 'Group ID', index:'GroupId', sorttype: 'string', hidden: true },
    { name: 'Cmp', title: 'Location', index: 'Cmp', width: 100, align: 'left', sorttype: 'string', classes: "uppercase", hidden: false },
    { name: 'EdiId', title: 'EDI Code', index: 'EdiId', width: 150, align: 'left', sorttype: 'string', classes: "uppercase", hidden: false },
    { name: 'EdiNm', title: 'EDI Name', index: 'EdiNm', width: 150, align: 'left', sorttype: 'string', hidden: false },
    { name: 'CustCd', title: _getLang("L_BaseColModel_CustCd", "EDI对象"), index: 'CustCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'CustNm', title: _getLang("L_BaseColModel_CustNm", "EDI对象名称"), index: 'CustNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'CustType', title: 'Type', index: 'CustType', width: 120, align: 'left', sorttype: 'string', hidden: false },
    { name: 'AttnNm', title: _getLang("L_BSCSSetup_CmpAttn", "联络人"), index: 'AttnNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'AttnTel', title: 'Tel.', index: 'AttnTel', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'AttnFax', title: 'Fax', index: 'AttnFax', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'AttnMail', title: 'E-mail', index: 'AttnMail', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'AttnAddr', title: _getLang("L_BSCSDataQuery_PartAddr", "地址"), index: 'AttnAddr', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'EdiAccount', title: 'Account', index: 'EdiAccount', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'EdiPwd', title: 'Password', index: 'EdiPwd', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'EdiIp', title: 'IP', index: 'EdiIp', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'CreateBy', title: _getLang("L_ShipmentID_CreateBy", "创建者"), index: 'CreateBy', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'CreateDate', title: _getLang("L_ShipmentID_CreateDate", "创建时间"), index: 'CreateDate', width: 120, align: 'left', sorttype: 'string', hidden: false, formatter: "date", formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
    { name: 'ModifyBy', title: _getLang("L_ShipmentID_ModifyBy", "修改者"), index: 'ModifyBy', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'ModifyDate', title: _getLang("L_ShipmentID_ModifyDate", "修改时间"), index: 'ModifyDate', width: 120, align: 'left', sorttype: 'string', hidden: false, formatter: "date", formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } }
];

QueryConfig.EDILOG = [
    { name: 'UId', title: 'ID', index:'UId', sorttype: 'string', hidden: true },
    //{ name: 'GroupId', title: 'Group ID', index:'GroupId', sorttype: 'string', hidden: true },
    //{ name: 'Cmp', title: 'Location', index: 'Cmp', width: 100, align: 'left', sorttype: 'string', classes: "uppercase", hidden: false },
    { name: 'EdiId', title: 'EDI Code', index: 'EdiId', width: 150, align: 'left', sorttype: 'string', classes: "uppercase", hidden: false },
    { name: 'EventDate', title: _getLang("L_BaseColModel_EventDate", "发生时间"), index: 'EventDate', width: 150, align: 'left', sorttype: 'string', hidden: false, formatter: "date", formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
    { name: 'Rs', title: _getLang("L_BaseColModel_Rs", "收/送"), index: 'Rs', width: 100, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'Receive:Receive;Send:Send' } },
    { name: 'Status', title: _getLang("L_GroupRelation_stnStatus", "状态"), index: 'Status', width: 100, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'Succeed:Success;Exception:Fail' } },
    { name: 'FromCd', title: _getLang("L_BaseColModel_FromCd", "传送方代号"), index: 'FromCd', width: 120, align: 'left', sorttype: 'string', hidden: false },
    { name: 'ToCd', title: _getLang("L_BaseColModel_ToCd", "接收方代号"), index: 'ToCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'DataFolder', title: _getLang("L_BaseColModel_DataFolde", "资料档案位置"), index: 'DataFolder', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'RefNo', title: _getLang("L_BaseColModel_RefNo", "引用编号"), index: 'RefNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'Sender', title: _getLang("L_BaseColModel_Sender", "传送者"), index: 'Sender', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'Remark', title: _getLang("L_IpPart_Remark", "备注"), index: 'AttnAddr', width: 100, align: 'left', sorttype: 'string', hidden: false, classes: "normal-white-space" }
];

QueryConfig.Bulletin = [
    { name: 'UId', title: 'ID', index:'UId', sorttype: 'string', hidden: true },
    { name: 'GroupId', title: 'Group ID', index:'GroupId', sorttype: 'string', hidden: true },
    { name: 'Cmp', title: 'Location', index: 'Cmp', width: 100, align: 'left', sorttype: 'string', classes: "uppercase", hidden: false },
    { name: 'BullType', title: 'Type', index: 'BullType', width: 100, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: "1:" + _getLang("L_Bulletin_Group", "集团公告") + ";2:" + _getLang("L_Bulletin_Factory", "厂别公告") + ";3:" + _getLang("L_Bulletin_Dep", "部门公告") + ";4:" + _getLang("L_Bulletin_LSP", "物流业者公告") + ";5:" + _getLang("L_Bulletin_Vendor", "供应商公告") } },
    { name: 'BullDate', title: 'Date', index: 'BullDate', width: 150, align: 'left', sorttype: 'string', hidden: false, formatter: "date", formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
    { name: 'BullTitle', title: 'Title', index: 'BullTitle', width: 300, align: 'left', sorttype: 'string', classes: "uppercase", hidden: false }
];

QueryConfig.BSCNTY = [
    { name: 'CntryCd', title: _getLang("L_CitySetup_CntryCd", "国家代码"), index: 'CntryCd', width: 120, sorttype: 'string', classes: "uppercase", editable: false },
    { name: 'CntryNm', title: _getLang("L_CntySetup_CntryNm", "国家名称"), index: 'CntryNm', width: 150, sorttype: 'string', classes: "uppercase" },
    { name: 'ShippingInstruction', title: _getLang("L_CntySetup_ShippingInstruction", "shipping instruction"), index: 'ShippingInstruction', width: 380, sorttype: 'string', classes: "uppercase" },
    { name: 'GroupId', title: _getLang("L_UserSetUp_GroupId", "集团"), index: 'GroupId', width: 180, sorttype: 'string', hidden: true },
    { name: 'Cmp', title: _getLang("L_PartyDocSetup_Cmp", "公司"), index: 'Cmp', width: 180, sorttype: 'string', hidden: true },
    { name: 'Stn', title: _getLang("L_IpPart_MafNo", "工厂"), index: 'Stn', width: 200, sorttype: 'string', hidden: true },
    { name: 'CreateBy', title: _getLang("L_Bsdate_CreateBy", "创建人"), index: 'CreateBy', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'CreateDate', title: _getLang("L_Bsdate_CreateDate", "创建时间"), index: 'CreateDate', width: 120, align: 'left', hidden: false, formatter: "date", formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d h:i A" } },
    { name: 'ModifyBy', title: _getLang("L_Bsdate_ModifyBy", "修改人"), index: 'ModifyBy', width: 100, align: 'left', sorttype: 'string', hidden: false },
    { name: 'ModifyDate', title: _getLang("L_Bsdate_ModifyDate", "修改时间"), index: 'ModifyDate', width: 120, align: 'left', hidden: false, formatter: "date", formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d h:i A" } }
]; 

QueryConfig.SYSQA = [
    { name: 'UId', title: 'ID', index: 'UId', width: 120, sorttype: 'string', classes: "uppercase", editable: false },
    { name: 'QaType', title: _getLang("L_BaseColModel_QaType", "Q/A类别"), index: 'QaType', width: 100, sorttype: 'string' },
    { name: 'QaTitle', title: 'Title', index: 'QaTitle', width: 150, sorttype: 'string' },
    { name: 'QaAnswer', title: 'Answer', index: 'QaAnswer', width: 380, sorttype: 'string' },
    { name: 'CreateBy', title: 'Create By', index: 'CreateBy', width: 380, sorttype: 'string' },
    { name: 'CreateDate', title: 'Create Date', index: 'CreateDate', width: 380, sorttype: 'string', formatter: "date", formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
    { name: 'ModifyBy', title: 'Modify By', index: 'ModifyBy', width: 380, sorttype: 'string' },
    { name: 'ModifyDate', title: 'Modify Date', index: 'ModifyDate', width: 380, sorttype: 'string', formatter: "date", formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
    { name: 'GroupId', title: _getLang("L_UserSetUp_GroupId", "集团"), index: 'GroupId', width: 180, sorttype: 'string', hidden: true },
    { name: 'Cmp', title: _getLang("L_PartyDocSetup_Cmp", "公司"), index: 'Cmp', width: 180, sorttype: 'string', hidden: true },
    { name: 'Stn', title: _getLang("L_IpPart_MafNo", "工厂"), index: 'Stn', width: 200, sorttype: 'string', hidden: true }
];

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}