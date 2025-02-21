//判断文字是否为空
function isEmpty(val) {
    if (val === undefined || val === "" || val == null)
        return true;
    return false;
}

function getData(url, data, callBackFn) {
    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: true,
        url: url,
        type: 'POST',
        data: data,
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            console.log(result);
            var resJson = $.parseJSON(result);
            callBackFn(resJson);
        }
    });
}

var CommonModelConfig = {};

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}

CommonModelConfig.ShipFeeColModel = [
    { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: false, hidden: true },
    { name: 'RfqNo', title: _getLang("L_ActManage_IqNo", "询价单号"), index: 'RfqNo', sorttype: 'string', width: 100, editable: true, hidden: false },
    { name: 'QuotNo', title: _getLang("L_QTQuery_QuotNo", "报价单号"), index: 'QuotNo', sorttype: 'string', width: 100, editable: true, hidden: false },
    { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', sorttype: 'string', width: 100, editable: true, hidden: false },
    { name: 'BlNo', title: _getLang("L_ChgApproveManage_Views_190", "提单号"), index: 'BlNo', sorttype: 'string', width: 100, editable: true, hidden: false },
    { name: 'DebitNo', title: _getLang("L_ActQuery_InvNo", "帐单号码"), index: 'DebitNo', width: 120, align: 'left', sorttype: 'string', hidden: false },
    { name: 'DebitDate', title: _getLang("L_ActQuery_DebitDate", "帐单日期"), index: 'DebitDate', width: 120, align: 'left', sorttype: 'string', hidden: false },
    { name: 'LspNo', title: _getLang("L_DRule_LspNo", "物流代码"), index: 'LspNo', sorttype: 'string', width: 120, editable: true, hidden: false },
    { name: 'LspNm', title: _getLang("L_AirQuery_LspNm", "物流业者名称"), index: 'LspNm', sorttype: 'string', width: 120, editable: true, hidden: false },
    { name: 'ChgCd', title: _getLang("L_SMCHGSetup_ChgCd", "费用代码"), index: 'ChgCd', sorttype: 'string', width: 80, editable: true, hidden: false },
    { name: 'ChgDescp', title: _getLang("L_SMCHGSetup_ChgDescp", "费用说明"), index: 'ChgDescp', sorttype: 'string', width: 120, editable: true, hidden: false },
    { name: 'ChgType', title: _getLang("L_SMCHGSetup_ChgRepay", "费用类"), index: 'ChgType', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: 'F:F.Freight Charge;O:O.Original Fee;D:D.Destination Fee' }, edittype: 'select' },
        { name: 'Repay', title: _getLang("L_QTSetup_chgCd", "费用别"), index: 'Repay', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: _getLang("L_FCLChgSetup_Script_166", "M:M.必收费用;C:C.发生才收;Y:Y.代收代付;A:A.AT Cost") }, edittype: 'select' },
    { name: 'Qcur', title: _getLang("L_BaseLookup_WithholdCur", "预提币别"), index: 'Qcur', sorttype: 'string', width: 80, hidden: false, editable: true },
    { name: 'QchgUnit', title: _getLang("L_BaseLookup_Unit", "单位"), index: 'QchgUnit', sorttype: 'string', width: 80, hidden: false, editable: true },
    {
        name: 'QunitPrice', title: _getLang("L_BaseLookup_WithholdPrice", "预提单价"), index: 'QunitPrice',
        width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'float',
        hidden: false
   , editable: true
    },
    { name: 'Qqty', title: _getLang("L_BaseLookup_WithholdQty", "预提数量"), index: 'Qqty', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, sorttype: 'string', width: 100, hidden: false, editable: true },
    { name: 'Qamt', title: _getLang("L_ActSetup_Qamt", "预提金额"), index: 'Qamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, editable: true },
    {
        name: 'QexRate', title: _getLang("L_ActSetup_ExRate", "汇率"), index: 'QexRate',
        width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'float',
        hidden: false
   , editable: true
    },
        { name: 'Qlcur', title: _getLang("L_BaseLookup_localWithholdCur", "本地预提币别"), index: 'Qlcur', sorttype: 'string', width: 100, hidden: false, editable: true },
    { name: 'Qlamt', title: _getLang("L_ActManage_LocWithAmt", "本地预提金额"), index: 'Qlamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00', editable: true }, width: 100, editable: true },
    { name: 'Qlrate', title: _getLang("L_BaseLookup_Scripts_7", "本地预提汇率"), index: 'Qlrate', width: 100, align: 'right', sorttype: 'float', hidden: false, editable: true },
    { name: 'Qtax', title: _getLang("L_ActManage_WithTax", "预提税率"), index: 'Qtax', sorttype: 'float', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 80, hidden: false, editable: true },
    { name: 'CheckDescp', title: _getLang("L_ActManage_AuditReason", "审核原因"), index: 'CheckDescp', sorttype: 'string', width: 150, hidden: false, classes: "normal-white-space", editable: true },
    { name: 'Cur', title: _getLang("L_ActQuery_Cur", "请款币别"), index: 'Cur', sorttype: 'string', width: 80, hidden: false, editable: true },
    {
        name: 'UnitPrice', title: _getLang("L_QTManage_IvUP", "请款单价"), index: 'UnitPrice',
        width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'float',
        hidden: false
   , editable: true
    },
    { name: 'Qty', title: _getLang("L_QTManage_IvQty", "请款数量"), index: 'Qty', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000', editable: true }, sorttype: 'string', width: 100, hidden: false },
    { name: 'Bamt', title: _getLang("L_ActSetup_Amt", "请款金额"), index: 'Bamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, editable: true },
    {
        name: 'ExRate', title: _getLang("L_QTManage_ExRate", "请款汇率"), index: 'ExRate',
        width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'float',
        hidden: false
   , editable: true
    },
    { name: 'Lamt', title: _getLang("L_ActManage_LocIvAmt", "本地请款金额"), index: 'Lamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, editable: true },
    { name: 'Tax', title: _getLang("L_QTManage_IvTax", "请款税率"), index: 'Tax', sorttype: 'float', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00', editable: true }, width: 80, hidden: false },
    { name: 'Remark', title: _getLang("L_BSCSSetup_Remark", "备注"), index: 'Remark', sorttype: 'string', width: 200, hidden: false, editable: true },
    { name: 'CostCenter', title: _getLang("L_SMCHGSetup_ChgLevel", "成本中心"), index: 'CostCenter', sorttype: 'string', width: 200, hidden: false, editable: true },
    { name: 'ProfitCenter', title: _getLang("L_ActManage_ProCenter", "利润中心"), index: 'ProfitCenter', sorttype: 'string', width: 200, hidden: false, editable: true },
    { name: 'InvoiceInfo', title: 'Invoice Info.', index: 'InvoiceInfo', sorttype: 'string', width: 200, hidden: false, editable: true },
    { name: 'DecNo', title: "Reference NO", index: 'DecNo', sorttype: 'string', width: 100, hidden: false, editable: true }
];

//订舱menu公用部分
var BookingMenu = [
    {
        id: "btnException",
        name: "Exception",
        func: function () {
            var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
            var uid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'UId');
            var shipmentid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ShipmentId');
            if (!uid) {
                CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", "Please Select a Record"), 500, "warning");
                return;
            }
            initErrMsg($("#btnException"), { 'GROUP_ID': groupId, 'CMP': cmp, 'STN': stn, 'UId': uid, 'JobNo': shipmentid }, true, $("#containerInfoGrid"));
        }
    }
]