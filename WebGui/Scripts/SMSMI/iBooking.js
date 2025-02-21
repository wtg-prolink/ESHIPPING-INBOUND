function IsBooking() {
    if (typeof ConfirmView != "undefined") {
        if (ConfirmView == "Y" || ConfirmView === "Y") {
            bookingeditable = false;
        }
    } else
        return true;
}

var colModel2 = [
	{ name: 'UId', title: '', index: 'UId', sorttype: 'string', editable: false, hidden: true },
	{ name: 'GroupId', title: '', index: 'GroupId', sorttype: 'string', editable: false, hidden: true },
	{ name: 'Cmp', title: '', index: 'Cmp', sorttype: 'string', editable: false, hidden: true },
	{ name: 'Stn', title: '', index: 'Stn', sorttype: 'string', editable: false, hidden: true },
	//{ name: 'ShipmentId', title: '', index: 'ShipmentId', sorttype: 'string', width: 90, hidden: false, editable: true },
	{ name: 'DnNo', title: '', index: 'DnNo', sorttype: 'string', width: 90, hidden: false, editable: false },
	{ name: 'InvNo', title: '', index: 'InvNo', sorttype: 'string', width: 90, hidden: false, editable: false }];

if (TRAN_TYPE != 'F' && TRAN_TYPE != 'R') {
    colModel2.push({
        name: 'ScmrequestDate', title: 'ScmrequestDate', index: 'ScmrequestDate', sorttype: 'string', width: 110, hidden: false, editable: false,
        formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: false,
        editoptions: myEditDateInit,
        formatter: 'date',
        formatoptions: {
            srcformat: 'ISO8601Long',
            newformat: 'Y-m-d',
            defaultValue: ""
        }
    });
    colModel2.push({
        name: 'EtaMsl', title: 'EtaMsl', index: 'EtaMsl', sorttype: 'string', width: 110, hidden: false, editable: IsBooking(),
        formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, 
        editoptions: myEditDateInit,
        formatter: 'date',
        formatoptions: {
            srcformat: 'ISO8601Long',
            newformat: 'Y-m-d',
            defaultValue: ""
        }
    });
    colModel2.push({ name: 'EtaMslTime', title: 'EtaMslTime', index: 'EtaMslTime', width: 70, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking(), formatter: "select", editoptions: { value: '0:00:00;1:01:00;2:02:00;3:03:00;4:04:00;5:05:00;6:06:00;7:07:00;8:08:00;9:09:00;10:10:00;11:11:00;12:12:00;13:13:00;14:14:00;15:15:00;16:16:00;17:17:00;18:18:00;19:19:00;20:20:00;21:21:00;22:22:00;23:23:00' }, edittype: 'select' });
}
var colmodel2last = [
	{ name: 'InvPrice', title: "", index: 'InvPrice', width: 90, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: false },
	{ name: 'InvCur', title: '', index: 'InvCur', sorttype: 'string', width: 90, hidden: false, editable: false },
	{ name: 'Amount', title: _getLang("L_DNApproveManage_Amount", "货值"), index: 'Amount', width: 80, align: 'right', sorttype: 'number', hidden: false, editable: false },
	{ name: 'Nw', title: '', index: 'Nw', width: 120, align: 'right', formatter: '', hidden: false },
	{ name: 'Gw', title: '', index: 'Gw', width: 120, align: 'right', formatter: '', hidden: false },
	{ name: 'Gwu', title: '', index: 'Gwu', sorttype: 'string', width: 70, hidden: false, editable: false },
	{ name: 'Cbm', title: '', index: 'Cbm', width: 120, align: 'right', formatter: '', hidden: false },
	{ name: 'Cbmu', title: '', index: 'Cbmu', sorttype: 'string', width: 70, hidden: false, editable: false },
	{ name: 'Qty', title: '', index: 'Qty', width: 120, align: 'right', formatter: '', hidden: false },
	{ name: 'Qtyu', title: '', index: 'Qtyu', sorttype: 'string', width: 70, hidden: false, editable: false },
	{ name: 'PkgNum', title: '', index: 'PkgNum', width: 120, align: 'right', formatter: 'integer', hidden: false },
	{ name: 'PkgUnit', title: '', index: 'PkgUnit', sorttype: 'string', width: 70, hidden: false, editable: false },
	{ name: 'PkgUnitDesc', title: '', index: 'PkgUnitDesc', sorttype: 'string', width: 70, hidden: false, editable: false },
	{ name: 'Cnt20', title: '', index: 'Cnt20', width: 120, align: 'right', formatter: 'integer', hidden: false },
	{ name: 'Cnt40', title: '', index: 'Cnt40', width: 120, align: 'right', formatter: 'integer', hidden: false },
	{ name: 'Cnt40hq', title: '', index: 'Cnt40hq', width: 120, align: 'right', formatter: 'integer', hidden: false },
	{ name: 'OthCntType', title: '', index: 'OthCntType', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'OthCntNum', title: '', index: 'OthCntNum', width: 120, align: 'right', formatter: 'integer', hidden: false },
	{ name: 'DlvArea', title: '', index: 'DlvArea', width: 120, align: 'left', formatter: 'string', hidden: true, editable: false },
	{ name: 'DlvAreaNm', title: '', index: 'DlvAreaNm', width: 120, align: 'left', formatter: 'string', hidden: false, editable: false },
	{ name: 'AddrCode', title: '', index: 'AddrCode', width: 120, align: 'left', formatter: 'string', hidden: true, editable: false },
	{ name: 'DlvAddr', title: '', index: 'DlvAddr', width: 120, align: 'left', formatter: 'string', hidden: false, editable: false },
	{ name: 'DivisionDescp', title: '', index: 'DivisionDescp', width: 160, align: 'left', formatter: 'string', hidden: false, editable: false },
	{
	    name: 'PickupCdate', title: 'Pickup Date', index: 'PickupCdate', sorttype: 'string', width: 110, hidden: false, editable: IsBooking(),
	    formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i' }, 
	    editoptions: myEditDateInit,
	    formatter: 'date',
	    formatoptions: {
	        srcformat: 'ISO8601Long',
	        newformat: 'Y-m-d H:i',
	        defaultValue: ""
	    }
	}, {
	    name: 'DischargeDate', title: 'DischargeDate', index: 'DischargeDate', sorttype: 'string', width: 110, hidden: false, editable: true,
	    formatter: 'date', formatoptions: { newformat: 'Y-m-d' },
	    editoptions: myEditDateInit,
	    formatter: 'date',
	    formatoptions: {
	        srcformat: 'ISO8601Long',
	        newformat: 'Y-m-d',
	        defaultValue: ""
	    }
	}, {
	    name: 'StorageDueDate', title: 'StorageDueDate', index: 'StorageDueDate', sorttype: 'string', width: 110, hidden: false, editable: true,
	    formatter: 'date', formatoptions: { newformat: 'Y-m-d' },
	    editoptions: myEditDateInit,
	    formatter: 'date',
	    formatoptions: {
	        srcformat: 'ISO8601Long',
	        newformat: 'Y-m-d',
	        defaultValue: ""
	    }
	}
];

colModel2 = colModel2.concat(colmodel2last);

if (TRAN_TYPE != 'F' && TRAN_TYPE != 'R') {
	colModel2.push(
		{ name: 'Priority', title: '', index: 'Priority', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking(), formatter: "select", editoptions: { value: Priority }, edittype: 'select' }
	);
	colModel2.push({ name: 'CallTruckStatus', title: 'CallTruckStatus', index: 'CallTruckStatus', width: 120, align: 'left', sorttype: 'string', hidden: false, formatter: "select", remark: 'A:Arrival;D:Finished TruckCalling;R:SlotTime Booked;C:Confirmed;I:Gate In;G:Berthing in the Dock;V:Cancel;P:Container Sealed;O:Gate Out;E:Truck left plant temporary;U:POD', editoptions: { value: 'A:Arrival;D:Finished TruckCalling;R:SlotTime Booked;C:Confirmed;I:Gate In;G:Berthing in the Dock;V:Cancel;P:Container Sealed;O:Gate Out;E:Truck left plant temporary;U:POD' } });
	colModel2.push({
	    name: 'IdateL', title: 'In Date', index: 'IdateL ', sorttype: 'string', width: 110, hidden: false, editable: false,
	    formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i' },
	    editoptions: myEditDateInit,
	    formatter: 'date',
	    formatoptions: {
	        srcformat: 'ISO8601Long',
	        newformat: 'Y-m-d H:i',
	        defaultValue: ""
	    }
	});
	colModel2.push({
	    name: 'PodDateL', title: 'Upload Pod Date', index: 'PodDateL ', sorttype: 'string', width: 110, hidden: false, editable: false,
	    formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i' }, 
	    editoptions: myEditDateInit,
	    formatter: 'date',
	    formatoptions: {
	        srcformat: 'ISO8601Long',
	        newformat: 'Y-m-d H:i',
	        defaultValue: ""
	    }
	});
	colModel2.push({
	    name: 'OdateL', title: 'Out Date', index: 'OdateL ', sorttype: 'string', width: 110, hidden: false, editable: false,
	    formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i' },
	    editoptions: myEditDateInit,
	    formatter: 'date',
	    formatoptions: {
	        srcformat: 'ISO8601Long',
	        newformat: 'Y-m-d H:i',
	        defaultValue: ""
	    }
	});
	colModel2.push({ name: 'PoNo', title: 'PoNo', index: 'PoNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() });
	colModel2.push({ name: 'WsCd', title: 'Ws Cd', index: 'WsCd', sorttype: 'string', width: 80, editable: false, hidden: false });
	colModel2.push({ name: 'WsNm', title: 'Ws Name', index: 'WsNm', sorttype: 'string', width: 120, editable: false, hidden: false });
	colModel2.push({ name: 'FinalWh', title: 'Final WH', index: 'FinalWh', sorttype: 'string', width: 80, editable: false, hidden: false });
	colModel2.push({ name: 'Wo', title: 'WO', index: 'Wo', sorttype: 'string', width: 80, editable: false, hidden: false });
}
else
{
    colModel2.push({
        name: 'EtaMsl', title: 'EtaMsl', index: 'EtaMsl', sorttype: 'string', width: 110, hidden: false, editable: true,
        formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, 
        editoptions: myEditDateInit,
        formatter: 'date',
        formatoptions: {
            srcformat: 'ISO8601Long',
            newformat: 'Y-m-d',
            defaultValue: ""
        }
    });
    colModel2.push({ name: 'EtaMslTime', title: 'EtaMslTime', index: 'EtaMslTime', width: 70, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: '0:00:00;1:01:00;2:02:00;3:03:00;4:04:00;5:05:00;6:06:00;7:07:00;8:08:00;9:09:00;10:10:00;11:11:00;12:12:00;13:13:00;14:14:00;15:15:00;16:16:00;17:17:00;18:18:00;19:19:00;20:20:00;21:21:00;22:22:00;23:23:00' }, edittype: 'select' });
    colModel2.push({ name: 'CallTruckStatus', title: 'CallTruckStatus', index: 'CallTruckStatus', width: 120, align: 'left', sorttype: 'string', hidden: false, formatter: "select", remark: 'A:Arrival;D:Finished TruckCalling;R:SlotTime Booked;C:Confirmed;I:Gate In;G:Berthing in the Dock;V:Cancel;P:Container Sealed;O:Gate Out;E:Truck left plant temporary;U:POD', editoptions: { value: 'A:Arrival;D:Finished TruckCalling;R:SlotTime Booked;C:Confirmed;I:Gate In;G:Berthing in the Dock;V:Cancel;P:Container Sealed;O:Gate Out;E:Truck left plant temporary;U:POD' } });
    colModel2.push({ name: 'PoNo', title: 'PoNo', index: 'PoNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true });
}
colModel2.push({ name: 'IsPostbill', title: 'IsPostbill', index: 'IsPostbill', width: 80, align: 'left', sorttype: 'string', hidden: false, editable: false });
colModel2.push({
    name: 'PostBillDate', title: 'PostBillDate', index: 'PostBillDate ', sorttype: 'string', width: 110, hidden: false, editable: false,
    formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i' },
    editoptions: myEditDateInit,
    formatter: 'date',
    formatoptions: {
        srcformat: 'ISO8601Long',
        newformat: 'Y-m-d H:i',
        defaultValue: ""
    }
});
colModel2.push({ name: 'Dep', title: '', index: 'Dep', sorttype: 'string', editable: false, hidden: false });

//$.extend(true, colModel2, SubGrid2Lang);
for (var i = 0; i < colModel2.length; i++) {
	for (var j = 0; j < SubGrid2Lang.length; j++) {
		if (colModel2[i]['name'] == SubGrid2Lang[j]['name']) {
			colModel2[i]['title'] = SubGrid2Lang[j]['title'];
			break;
		}
	}
}

function PkgUnitOp() { 
	var unit_op = getLookupOp("SubGrid4",
		{
			url: rootPath + LookUpConfig.QtyuUrl,
			config: LookUpConfig.QtyuLookup,
			returnFn: function (map, $grid) {
				return map.Cd;
			}
		}, LookUpConfig.GetCodeTypeAuto(groupId, "UB", $SubGrid4,
			function ($grid, rd, elem, rowid) {
				$(elem).val(rd.CD);
			}), { param: "" });
	return unit_op;
} 
 

if (TRAN_TYPE == 'A' || TRAN_TYPE == 'E' || TRAN_TYPE == 'L')
{
	var ScufcolModel=[];
	if (TRAN_TYPE == 'A' || TRAN_TYPE == 'E') {
		ScufcolModel.push({ name: 'DnNo', title: 'Dn No', index: 'DnNo', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: _dnNoList }, edittype: 'select' });
    } else {
		ScufcolModel.push({ name: 'DnNo', title: 'Dn No', index: 'DnNo', sorttype: 'string', width: 100, hidden: false, editable: false });
    }
	ScufcolModel.push({ name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true });
	ScufcolModel.push({ name: 'UFid', title: 'UFid', index: 'UFid', sorttype: 'string', editable: false, hidden: true });
	ScufcolModel.push({ name: 'L', title: _getLang("L_AirBookingSetup_Script_87", "长（cm）"), index: 'L', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false });
	ScufcolModel.push({ name: 'W', title: _getLang("L_AirBookingSetup_Script_88", "宽（cm）"), index: 'W', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false });
	ScufcolModel.push({ name: 'H', title: _getLang("L_AirBookingSetup_Script_89", "高（cm）"), index: 'H', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false });
	ScufcolModel.push({ name: 'Pkg', title: _getLang("L_DNManage_PkgNum", "件数"), index: 'Pkg', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false });
	ScufcolModel.push({ name: 'PkgUnit', title: _getLang("L_BaseLookup_Unit", "单位"), index: 'PkgUnit', editoptions: gridLookup(PkgUnitOp()), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true });
	ScufcolModel.push({ name: 'Vw', title: _getLang("L_AirBookingSetup_Script_90", "体积（m3）"), index: 'Vw', width: 140, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.00' }, editable: false, hidden: false });
	ScufcolModel.push({ name: 'Gw', title: _getLang("L_BaseLookup_Gw", "毛重"), index: 'Gw', width: 140, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.00' }, editable: true, hidden: false });
  
	if (TRAN_TYPE == 'A' || TRAN_TYPE == 'E') {
		ScufcolModel.push({ name: 'Sbw', title: _getLang("L_BaseLookup_Sbw", "单件计费重"), index: 'Sbw', width: 140, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 1, defaultValue: '0.0' }, editable: true, hidden: false });
    }
}

var BaseBooking_DnModel = [
		{ name: 'DnNo', title: 'Dn No', index: 'DnNo', sorttype: 'string', width: 120, editable: false, hidden: false },
		{ name: 'BlLevel', title: '', index: 'BlLevel', sorttype: 'string', editable: false, hidden: false, editable: true },
		{ name: 'ExportNo', title: '', index: 'ExportNo', sorttype: 'string', width: 120, hidden: false, editable: true },
		{ name: 'Unicode', title: '', index: 'Unicode', sorttype: 'string', width: 120, hidden: false, editable: true },
		{ name: 'ApproveNo', title: '', index: 'ApproveNo', sorttype: 'string', width: 120, hidden: false, editable: true },
	   {
		   name: 'AskTim', title: '', index: 'AskTim', width: 150, align: 'left', hidden: false, editable: true, sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
		   editoptions: myEditDateInit,
		   formatter: 'date',
		   formatoptions: {
			   srcformat: 'ISO8601Long',
			   newformat: 'Y-m-d',
			   defaultValue: ""
		   }
	   },
		{ name: 'EdeclNo', title: '', index: 'EdeclNo', sorttype: 'string', width: 120, hidden: false, editable: true },
		{ name: 'DeclDate', title: '', index: 'DeclDate', sorttype: 'string', width: 120, hidden: false, editable: true },
		{ name: 'DeclRlsDate', title: '', index: 'DeclRlsDate', sorttype: 'string', width: 120, hidden: false, editable: true },
		{ name: 'NextNum', title: '', index: 'NextNum', sorttype: 'string', width: 120, hidden: false, editable: true },
		{ name: 'Dremark', title: '', index: 'Dremark', sorttype: 'string', width: 120, hidden: false, editable: true }
];

BaseBooking_DnModel.push({ name: 'WsCd', title: 'Ws Cd', index: 'WsCd', sorttype: 'string', width: 80, editable: false, hidden: false });
BaseBooking_DnModel.push({ name: 'WsNm', title: 'Ws Name', index: 'WsNm', sorttype: 'string', width: 120, editable: false, hidden: false });
BaseBooking_DnModel.push({ name: 'FinalWh', title: 'Final WH', index: 'FinalWh', sorttype: 'string', width: 80, editable: false, hidden: false });

for (var i = 0; i < BaseBooking_DnModel.length; i++) {
    if (i < DnGridLang.length)
        BaseBooking_DnModel[i]['title'] = DnGridLang[i];
}

var colModel3 = [
	{ name: 'UId', title: '', index: 'UId', sorttype: 'string', editable: false, hidden: true },
	{ name: 'UFid', title: '', index: 'UId', sorttype: 'string', editable: false, hidden: true },
	{ name: 'CntrNo', title: '', index: 'CntrNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
	{ name: 'CntrType', title: '', index: 'CntrType', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true }, 
	{ name: 'DnNo', title: '', index: 'DnNo', sorttype: 'string', width: 90, hidden: false, editable: true },
	{ name: 'InvNo', title: '', index: 'InvNo', sorttype: 'string', width: 90, hidden: false, editable: true },
	{ name: 'OpartNo', title: '', index: 'OpartNo', sorttype: 'string', width: 90, hidden: false, editable: true },
	{ name: 'IpartNo', title: '', index: 'IpartNo', sorttype: 'string', width: 90, hidden: false, editable: true },
	{ name: 'GoodsDescp', title: '', index: 'GoodsDescp', sorttype: 'string', width: 140, hidden: false, editable: true },
	{ name: 'ProdDescp', title: '', index: 'ProdDescp', sorttype: 'string', width: 100, hidden: false, editable: true },
	{ name: 'Qty', title: '', index: 'Qty', width: 120, align: 'right', formatter: 'integer', hidden: false },
	{ name: 'Gw', title: '', index: 'Gw', width: 120, align: 'right', formatter: 'integer', hidden: false },
	{ name: 'Cbm', title: '', index: 'Cbm', width: 120, align: 'right', formatter: 'integer', hidden: false }, 
	{ name: 'SealNo1', title: '', index: 'SealNo1', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
	{ name: 'SealNo2', title: '', index: 'SealNo2', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
	{ name: 'DlvArea', title: '', index: 'DlvArea', width: 120, align: 'left',  sorttype: 'string', hidden: true, editable:false },
	{ name: 'DlvAreaNm', title: '', index: 'DlvAreaNm', width: 120, align: 'left', sorttype: 'string',  hidden: false, editable:false },
	{ name: 'AddrCode', title: '', index: 'AddrCode', width: 120, align: 'left', sorttype: 'string',  hidden: true, editable:false },
    { name: 'DlvAddr', title: '', index: 'DlvAddr', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
    { name: 'SoNo', title: '', index: 'SoNo', width: 120, align: 'left', sorttype: 'string', hidden: true, editable: false },
    { name: 'PoNo', title: '', index: 'PoNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
    { name: 'PartNo', title: '', index: 'PartNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
    { name: 'IhsCode', title: '', index: 'IhsCode', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
    { name: 'OhsCode', title: '', index: 'OhsCode', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
    { name: 'Resolution', title: '', index: 'Resolution', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'AsnNo', title: '', index: 'AsnNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false }	
];

if (TRAN_TYPE == 'T') {
	colModel3.push({ name: 'CntrStdQty', title: _getLang("L_CntrStdQty", "标准装柜量")   , index: 'CntrStdQty', width: 255, align: 'left', sorttype: 'string', hidden: false, editable: true });
}
colModel3.push({ name: 'RefNo', title: 'DN2 No.', index: 'RefNo', sorttype: 'string', width: 90, hidden: false, editable: true });
colModel3.push({ name: 'Brand', title: _getLang("L_DNApproveManage_Brand", "品牌"), index: 'Brand', sorttype: 'string', width: 90, hidden: false, editable: true });
colModel3.push({ name: 'Size', title: _getLang("L_BaseLookup_Dimension", "尺寸"), index: 'Size', sorttype: 'string', width: 90, hidden: false, editable: true });
colModel3.push({ name: 'Value1', title: _getLang("L_BaseLookup_Amt", "金额"), index: 'Value1', sorttype: 'string', width: 90, hidden: false, editable: true });
colModel3.push({ name: 'GrQty', title: 'GR Quantity', index: 'GrQty', align: 'right', formatter: 'integer', hidden: false });
colModel3.push({ name: 'GrDate', title: 'GR Date', index: 'GrDate', width: 150, align: 'left', hidden: false, editable: true, formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false });
colModel3.push({ name: 'GrStatus', title: 'GR Status', index: 'GrStatus', sorttype: 'string', width: 90, hidden: false, editable: true });
colModel3.push({ name: 'AsnDate', title: 'ASN Date', index: 'AsnDate', width: 150, align: 'left', hidden: false, editable: true, formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false });
colModel3.push({ name: 'Category', title: _getLang("L_DNApproveManage_Category", "类别"), index: 'Category', sorttype: 'string', width: 90, hidden: false, editable: true });
for (var i = 0; i < colModel3.length; i++) {
	if (colModel3[i]['title']!="") {
		continue;
	}
	colModel3[i]['title'] = SubGrid3Lang[i];
}


var CcolModel = [], TcolModel = [];

function getcntryop1(name) {
	var _name = name;
	var cnty_op = getLookupOp("CcGrid",
		{
			url: rootPath + LookUpConfig.CountryUrl,
			config: LookUpConfig.CountryLookup,
			returnFn: function (map, $grid) {
				//$grid = $("#CcGrid");
				var selRowId = $grid.jqGrid('getGridParam', 'selrow');//selrow
				setGridVal($grid, selRowId, 'CntryCd', map.CntryCd, "lookup");
				return map.CntryCd;
			}
		}, LookUpConfig.GetCountryAuto(groupId, undefined, function ($grid, rd, elem, rowid) {
			//$("#PartyType").val(rd.CD);
			var selRowId = rowid;
			setGridVal($grid, selRowId, 'CntryCd', rd.CNTRY_CD, null);
			$(elem).val(rd.CNTRY_CD);
		}));
	return cnty_op;
}

function getcntryop(name) {
	var _name = name;
	var cnty_op = getLookupOp("TcGrid",
		{
			url: rootPath + LookUpConfig.CountryUrl,
			config: LookUpConfig.CountryLookup,
			returnFn: function (map, $grid) {
				//$grid = $("#CcGrid");
				var selRowId = $grid.jqGrid('getGridParam', 'selrow');//selrow
				setGridVal($grid, selRowId, 'CntryCd', map.CntryCd, "lookup");
				return map.CntryCd;
			}
		}, LookUpConfig.GetCountryAuto(groupId, undefined, function ($grid, rd, elem, rowid) {
			//$("#PartyType").val(rd.CD);
			var selRowId = rowid;
			setGridVal($grid, selRowId, 'CntryCd', rd.CNTRY_CD, null);
			$(elem).val(rd.CNTRY_CD);
		}));
	return cnty_op;
}

function gettccntryop(name) {
	var _name = name;
	var cnty_op = getLookupOp("TcGrid",
		{
			url: rootPath + LookUpConfig.CountryUrl,
			config: LookUpConfig.CountryLookup,
			returnFn: function (map, $grid) {
				//$grid = $("#CcGrid");
				var selRowId = $grid.jqGrid('getGridParam', 'selrow');//selrow
				setGridVal($grid, selRowId, 'TcCntryCd', map.CntryCd, "lookup");
				return map.CntryCd;
			}
		}, LookUpConfig.GetCountryAuto(groupId, undefined, function ($grid, rd, elem, rowid) {
			//$("#PartyType").val(rd.CD);
			var selRowId = rowid;
			setGridVal($grid, selRowId, 'TcCntryCd', rd.CNTRY_CD, null);
			$(elem).val(rd.CNTRY_CD);
		}));
	return cnty_op;
}

if(TRAN_TYPE == 'F' || TRAN_TYPE == 'R')
{
	CcolModel = [
		{ name: 'UId', title: '', index: 'UId', sorttype: 'string', editable: false, hidden: true },
		//{ name: 'ShipmentId', title: '', index: 'ShipmentId', sorttype: 'string', width: 90, hidden: false, editable: true },
		{ name: 'CntrNo', title: '', index: 'CntrNo', sorttype: 'string', width: 90, hidden: false, editable: true },
		{ name: 'NewSeal', title: '', index: 'NewSeal', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'ImportNo', title: '', index: 'ImportNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'DecNo', title: '', index: 'DecNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'CerNo', title: '', index: 'CerNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'DecDate', title: '', index: 'DecDate', sorttype: 'string', width: 120, hidden: false, editable: true,
			formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
				 editoptions: myEditDateInit,
				 formatter: 'date',
				 formatoptions: {
					 srcformat: 'ISO8601Long',
					 newformat: 'Y-m-d',
					 defaultValue: ""
				 }
		 },
		{
		    name: 'RelDate', title: '', index: 'RelDate', sorttype: 'string', width: 120, hidden: false, editable: true,
				formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
					 editoptions: myEditDateInit,
					 formatter: 'date',
					 formatoptions: {
						 srcformat: 'ISO8601Long',
						 newformat: 'Y-m-d',
						 defaultValue: ""
					 }
		},
        { name: 'CcChannel', title: '', index: 'CcChannel', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: Ccchannel }, edittype: 'select' },
		{ name: 'Inspection', title: '', index: 'Inspection', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: 'Y:YES;N:NO' }, edittype: 'select' },
		{ name: 'DecCust', title: '', index: 'DecCust', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'DecReply', title: '', index: 'DecReply', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'Icdf', title: '', index: 'Icdf', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: 'Y:YES;N:NO' }, edittype: 'select' },
		{ name: 'HsQty', title: 'HS Code Qty', index: 'HsQty', width: 80, align: 'right', formatter: 'integer', formatoptions: { thousandsSeparator: " ", defaultValue: '0' }, hidden: false, editable: true },
		{ name: 'CntryCd', title: '', index: 'CntryCd', editoptions: gridLookup(getcntryop1("CntryNm")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true },
		{ name: 'PdQty', title: '', index: 'PdQty', width: 80, align: 'right', formatter: 'integer', formatoptions: { thousandsSeparator: " ", defaultValue: '0' }, hidden: false, editable: true },
		{ name: 'FobAmt', title: '', index: 'FobAmt', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
		{ name: 'CifAmt', title: '', index: 'CifAmt', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
		{ name: 'Pli', title: 'PLI#', index: 'Pli', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
		{ name: 'Li', title: 'LI#', index: 'Li', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
		{ name: 'SufCost', title: 'SUFRAMA Cost', index: 'SufCost', width: 130, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: false },
		{ name: 'CcRate', title: 'CC Declaration Exchange Rate', index: 'CcRate', width: 270, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, hidden: false, editable: false },
		{ name: 'AddQty', title: 'Addition QTY', index: 'AddQty', width: 130, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 3, defaultValue: '0.000' }, hidden: false, editable: false },
		{ name: 'SisFee', title: 'SISCOMEX Fee', index: 'SisFee', width: 130, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: false }
	];

	TcolModel = [
		{ name: 'UId', title: '', index: 'UId', sorttype: 'string', editable: false, hidden: true },
		{ name: 'CntrNo', title: '', index: 'CntrNo', sorttype: 'string', width: 90, hidden: false, editable: true },
		{ name: 'TcNewSeal', title: '', index: 'TcNewSeal', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'TcImportNo', title: '', index: 'TcImportNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'TcDecNo', title: '', index: 'TcDecNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'TcCerNo', title: '', index: 'TcCerNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{
		    name: 'TcDecDate', title: '', index: 'TcDecDate', sorttype: 'string', width: 120, hidden: false, editable: true,
				formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
					 editoptions: myEditDateInit,
					 formatter: 'date',
					 formatoptions: {
						 srcformat: 'ISO8601Long',
						 newformat: 'Y-m-d',
						 defaultValue: ""
					 }
		 },
		{
		    name: 'TcRelDate', title: '', index: 'TcRelDate', sorttype: 'string', width: 120, hidden: false, editable: true,
				formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
					 editoptions: myEditDateInit,
					 formatter: 'date',
					 formatoptions: {
						 srcformat: 'ISO8601Long',
						 newformat: 'Y-m-d',
						 defaultValue: ""
					 }
		},
        { name: 'TcCcChannel', title: '', index: 'TcCcChannel', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: Ccchannel }, edittype: 'select' },
		{ name: 'TcInspection', title: '', index: 'TcInspection', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: 'Y:YES;N:NO' }, edittype: 'select' },
		{ name: 'TcDecCust', title: '', index: 'TcDecCust', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'TcDecReply', title: '', index: 'TcDecReply', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'TcIcdf', title: '', index: 'TcIcdf', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: 'Y:YES;N:NO' }, edittype: 'select' },
		{ name: 'TcHsQty', title: 'HS Code Qty', index: 'TcHsQty', width: 80, align: 'right', formatter: 'integer', formatoptions: { thousandsSeparator: " ", defaultValue: '0' }, hidden: false, editable: true },
		{ name: 'TcCntryCd', title: '', index: 'TcCntryCd', editoptions: gridLookup(gettccntryop("CntryNm")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true }
	];
}
else
{
	CcolModel = [
		{ name: 'UId', title: '', index: 'UId', sorttype: 'string', editable: false, hidden: true },
		//{ name: 'ShipmentId', title: '', index: 'ShipmentId', sorttype: 'string', width: 90, hidden: false, editable: true },
		{ name: 'InvNo', title: '', index: 'InvNo', sorttype: 'string', width: 90, hidden: false, editable: true },
		{ name: 'ImportNo', title: '', index: 'ImportNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'DecNo', title: '', index: 'DecNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'CerNo', title: '', index: 'CerNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{
		    name: 'DecDate', title: '', index: 'DecDate', sorttype: 'string', width: 120, hidden: false, editable: true,
			formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
				 editoptions: myEditDateInit,
				 formatter: 'date',
				 formatoptions: {
					 srcformat: 'ISO8601Long',
					 newformat: 'Y-m-d',
					 defaultValue: ""
				 }
		 },
		{
		    name: 'RelDate', title: '', index: 'RelDate', sorttype: 'string', width: 120, hidden: false, editable: true,
				formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
					 editoptions: myEditDateInit,
					 formatter: 'date',
					 formatoptions: {
						 srcformat: 'ISO8601Long',
						 newformat: 'Y-m-d',
						 defaultValue: ""
					 }
		},
        { name: 'CcChannel', title: '', index: 'CcChannel', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: Ccchannel }, edittype: 'select' },
		{ name: 'Inspection', title: '', index: 'Inspection', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: 'Y:YES;N:NO' }, edittype: 'select' },
		{ name: 'DecCust', title: '', index: 'DecCust', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'DecReply', title: '', index: 'DecReply', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'Icdf', title: '', index: 'Icdf', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: 'Y:YES;N:NO' }, edittype: 'select' },
		{ name: 'HsQty', title: 'HS Code Qty', index: 'HsQty', width: 80, align: 'right', formatter: 'integer', formatoptions: { thousandsSeparator: " ", defaultValue: '0' }, hidden: false, editable: true },
		{ name: 'CntryCd', title: '', index: 'CntryCd', editoptions: gridLookup(getcntryop1("CntryNm")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true },
		{ name: 'PdQty', title: '', index: 'PdQty', width: 80, align: 'right', formatter: 'integer', formatoptions: { thousandsSeparator: " ", defaultValue: '0' }, hidden: false, editable: true },
		{ name: 'FobAmt', title: '', index: 'FobAmt', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
		{ name: 'CifAmt', title: '', index: 'CifAmt', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
		{ name: 'Pli', title: 'PLI#', index: 'Pli', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
		{ name: 'Li', title: 'LI#', index: 'Li', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
		{ name: 'SufCost', title: 'SUFRAMA Cost', index: 'SufCost', width: 130, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: false },
		{ name: 'CcRate', title: 'CC Declaration Exchange Rate', index: 'CcRate', width: 270, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, hidden: false, editable: false },
		{ name: 'AddQty', title: 'Addition QTY', index: 'AddQty', width: 130, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 3, defaultValue: '0.000' }, hidden: false, editable: false },
		{ name: 'SisFee', title: 'SISCOMEX Fee', index: 'SisFee', width: 130, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: false }
	];

	TcolModel = [
		{ name: 'UId', title: '', index: 'UId', sorttype: 'string', editable: false, hidden: true },
		//{ name: 'ShipmentId', title: '', index: 'ShipmentId', sorttype: 'string', width: 90, hidden: false, editable: true },
		{ name: 'InvNo', title: '', index: 'InvNo', sorttype: 'string', width: 90, hidden: false, editable: true },
		{ name: 'TcImportNo', title: '', index: 'TcImportNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		//{ name: 'TcImporter', title: '', index: 'TcImporter', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		//{ name: 'TcImporterNm', title: '', index: 'TcImporterNm', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		//{ name: 'TcImporterAddr', title: '', index: 'TcImporterAddr', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'TcDecNo', title: '', index: 'TcDecNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'TcCerNo', title: '', index: 'TcCerNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'TcDecDate', title: '', index: 'TcDecDate', sorttype: 'string', width: 120, hidden: false, editable: true,
				formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
					 editoptions: myEditDateInit,
					 formatter: 'date',
					 formatoptions: {
						 srcformat: 'ISO8601Long',
						 newformat: 'Y-m-d',
						 defaultValue: ""
					 }
		 },
		{ name: 'TcRelDate', title: '', index: 'TcRelDate', sorttype: 'string', width: 120, hidden: false, editable: true,
				formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
					 editoptions: myEditDateInit,
					 formatter: 'date',
					 formatoptions: {
						 srcformat: 'ISO8601Long',
						 newformat: 'Y-m-d',
						 defaultValue: ""
					 }
		},
        { name: 'TcCcChannel', title: '', index: 'TcCcChannel', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: Ccchannel }, edittype: 'select' },
		{ name: 'TcInspection', title: '', index: 'TcInspection', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: 'Y:YES;N:NO' }, edittype: 'select' },
		{ name: 'TcDecCust', title: '', index: 'TcDecCust', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'TcDecReply', title: '', index: 'TcDecReply', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'TcIcdf', title: '', index: 'TcIcdf', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: 'Y:YES;N:NO' }, edittype: 'select' },
		{ name: 'TcHsQty', title: 'HS Code Qty', index: 'TcHsQty', width: 80, align: 'right', formatter: 'integer', formatoptions: { thousandsSeparator: " ", defaultValue: '0' }, hidden: false, editable: true },
		{ name: 'TcCntryCd', title: '', index: 'TcCntryCd', editoptions: gridLookup(gettccntryop("CntryNm")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true }
	];
}


for (var i = 0; i < CcolModel.length; i++) {
	CcolModel[i]['title'] = CcGridLang[i];
}
if (typeof isBroker != "undefined" && isBroker) {
	CcolModel.push({ name: 'FobAmtUsd', title: 'Import Declaration FOB (USD)', index: 'FobAmtUsd', width: 160, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true });
}
for (var i = 0; i < TcolModel.length; i++) {
	TcolModel[i]['title'] = TcGridLang[i];
}

function getBackLcop(_partygrid, name) {
	var _name = name;
	var pod_op = getLookupOp(_partygrid,
		{
			url: rootPath + LookUpConfig.TruckPortCdUrl,
			config: LookUpConfig.TruckPortCdLookup,
			returnFn: function (map, $grid) {
				var selRowId = $grid.jqGrid('getGridParam', 'selrow');
				setGridVal($grid, selRowId, 'BackLocation', map.PortCd, 'lookup');
				return map.PortCd;
			}
		}, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem, rowid) {
			var selRowId = rowid;
			setGridVal($grid, selRowId, 'BackLocation', rd.PORT_CD, 'lookup');
			$(elem).val(rd.PORT_CD);
		}, function ($grid, elem, rowid) {
			var selRowId = rowid;
			setGridVal($grid, selRowId, 'BackLocation', "", null);
			$(elem).val("");
		}));
	return pod_op;
}

function getTrucker1op(name)
{
    var _name = name;
    var trucker_op = getLookupOp("CntrGrid",
		{
		    url: rootPath + LookUpConfig.PartyNo1Url,
		    config: LookUpConfig.PartyNoLookup,
		    returnFn: function (map, $grid) {
		        //$grid = $("#CcGrid");
		        var selRowId = $grid.jqGrid('getGridParam', 'selrow');//selrow
		        setGridVal($grid, selRowId, 'Trucker1', map.PartyNo, "lookup");
		        setGridVal($grid, selRowId, _name, map.PartyName, null);
		        return map.PartyNo;
		    }
		}, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem, rowid) {
		    var selRowId = rowid;
		    $grid = $("#CntrGrid");
		    setGridVal($grid, selRowId, 'Trucker1', rd.PARTY_NO, null);
		    setGridVal($grid, selRowId, _name, rd.PARTY_NAME, null);
		    $(elem).val(rd.PARTY_NO);
		}));
    return trucker_op;
}
function getTrucker2op(name) {
    var _name = name;
    var trucker_op = getLookupOp("CntrGrid",
		{
		    url: rootPath + LookUpConfig.PartyNo1Url,
		    config: LookUpConfig.PartyNoLookup,
		    returnFn: function (map, $grid) {
		        //$grid = $("#CcGrid");
		        var selRowId = $grid.jqGrid('getGridParam', 'selrow');//selrow
		        setGridVal($grid, selRowId, 'Trucker2', map.PartyNo, "lookup");
		        setGridVal($grid, selRowId, _name, map.PartyName, null);
		        return map.PartyNo;
		    }
		}, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem, rowid) {
		    var selRowId = rowid;
		    $grid = $("#CntrGrid");
		    setGridVal($grid, selRowId, 'Trucker2', rd.PARTY_NO, null);
		    setGridVal($grid, selRowId, _name, rd.PARTY_NAME, null);
		    $(elem).val(rd.PARTY_NO);
		}));
    return trucker_op;
}
function getTrucker3op(name) {
    var _name = name;
    var trucker_op = getLookupOp("CntrGrid",
		{
		    url: rootPath + LookUpConfig.PartyNo1Url,
		    config: LookUpConfig.PartyNoLookup,
		    returnFn: function (map, $grid) {
		        //$grid = $("#CcGrid");
		        var selRowId = $grid.jqGrid('getGridParam', 'selrow');//selrow
		        setGridVal($grid, selRowId, 'Trucker3', map.PartyNo, "lookup");
		        setGridVal($grid, selRowId, _name, map.PartyName, null);
		        return map.PartyNo;
		    }
		}, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem, rowid) {
		    var selRowId = rowid;
		    $grid = $("#CntrGrid");
		    setGridVal($grid, selRowId, 'Trucker3', rd.PARTY_NO, null);
		    setGridVal($grid, selRowId, _name, rd.PARTY_NAME, null);
		    $(elem).val(rd.PARTY_NO);
		}));
    return trucker_op;
}

if(typeof CntrGridLang != "undefined")
{
	var CntrModel = [
		{ name: 'UId', title: '', index: 'UId', sorttype: 'string', editable: false, hidden: true },
		{ name: 'CntrNo', title: '', index: 'CntrNo', sorttype: 'string', width: 90, hidden: false, editable: IsBooking() },
		{ name: 'CntrType', title: '', index: 'CntrType', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
        {
            name: 'ScmrequestDate', title: 'ScmrequestDate', index: 'ScmrequestDate', sorttype: 'string', width: 110, hidden: false, editable: false,
            formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: false,
            editoptions: myEditDateInit,
            formatter: 'date',
            formatoptions: {
                srcformat: 'ISO8601Long',
                newformat: 'Y-m-d',
                defaultValue: ""
            }
        },
        {
            name: 'EtaMsl', title: 'EtaMsl', index: 'EtaMsl', sorttype: 'string', width: 110, hidden: false, editable: IsBooking(),
            formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, 
            editoptions: myEditDateInit,
            formatter: 'date',
            formatoptions: {
                srcformat: 'ISO8601Long',
                newformat: 'Y-m-d',
                defaultValue: ""
            }
        },
        { name: 'EtaMslTime', title: 'EtaMslTime', index: 'EtaMslTime', width: 70, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking(), formatter: "select", editoptions: { value: '0:00:00;1:01:00;2:02:00;3:03:00;4:04:00;5:05:00;6:06:00;7:07:00;8:08:00;9:09:00;10:10:00;11:11:00;12:12:00;13:13:00;14:14:00;15:15:00;16:16:00;17:17:00;18:18:00;19:19:00;20:20:00;21:21:00;22:22:00;23:23:00' }, edittype: 'select' },
		{ name: 'NewSeal', title: '', index: 'NewSeal', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
		{ name: 'SealNo1', title: '', index: 'SealNo1', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
		{ name: 'SealNo2', title: '', index: 'SealNo2', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
		{ name: 'DnNo', title: '', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
		{ name: 'DivisionDescp', title: '', index: 'DivisionDescp', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
		{ name: 'DlvArea', title: '', index: 'DlvArea', width: 120, align: 'left',  sorttype: 'string',  hidden: true, editable:false },
		{ name: 'DlvAreaNm', title: '', index: 'DlvAreaNm', width: 120, align: 'left', sorttype: 'string',   hidden: false, editable:false },
		{ name: 'AddrCode', title: '', index: 'AddrCode', width: 120, align: 'left',  sorttype: 'string',  hidden: true, editable:false },
		{ name: 'DlvAddr', title: '', index: 'DlvAddr', width: 120, align: 'left',  sorttype: 'string',  hidden: false, editable:false },
		{ name: 'Pol1', title: '', index: 'Pol1', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
		{ name: 'PolNm1', title: '', index: 'PolNm1', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
		{name:'Pod1', title: '', index: 'Pod1', width: 120, align: 'left',  sorttype: 'string', hidden: true, editable:false},
		{name:'PodNm1', title: '', index: 'PodNm1', width: 120, align: 'left',  sorttype: 'string', hidden: true, editable:false},
		{ name: 'TranType1', title: '', index: 'TranType1', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking(), formatter: "select", editoptions: { value: ':;R:Railway;S:Ocean Shipping;A:Air;T:Truck;I:Intermodal' }, edittype: 'select' },
        { name: 'DepAddr1', title: '', index: 'DepAddr1', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
        { name: 'Trucker1', title: '', index: 'Trucker1', editoptions: gridLookup(getTrucker1op("TruckerNm1")), edittype: 'custom', hidden: false, width: 120, align: 'left', editable: IsBooking() },
		{ name: 'TruckerNm1', title: '', index: 'TruckerNm1', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
		{ name: 'Pol2', title: '', index: 'Pol2', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
		{ name: 'PolNm2', title: '', index: 'PolNm2', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
		{name:'Pod2', title: '', index: 'Pod2', width: 120, align: 'left',  sorttype: 'string', hidden: true, editable:false},
		{name:'PodNm2', title: '', index: 'PodNm2', width: 120, align: 'left',  sorttype: 'string', hidden: true, editable:false},
		{ name: 'TranType2', title: '', index: 'TranType2', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking(), formatter: "select", editoptions: { value: ':;R:Railway;S:Ocean Shipping;A:Air;T:Truck;I:Intermodal' }, edittype: 'select' },
        { name: 'DepAddr2', title: '', index: 'DepAddr2', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
        { name: 'Trucker2', title: '', index: 'Trucker2', editoptions: gridLookup(getTrucker2op("TruckerNm2")), edittype: 'custom', width: 120, align: 'left', hidden: false, editable: IsBooking() },
		{ name: 'TruckerNm2', title: '', index: 'TruckerNm2', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
		{ name: 'Pol3', title: '', index: 'Pol3', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
		{ name: 'PolNm3', title: '', index: 'PolNm3', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
		{name:'Pod3', title: '', index: 'Pod3', width: 120, align: 'left',  sorttype: 'string', hidden: true, editable:false},
		{name:'PodNm3', title: '', index: 'PodNm3', width: 120, align: 'left',  sorttype: 'string', hidden: true, editable:false},
		{ name: 'TranType3', title: '', index: 'TranType3', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking(), formatter: "select", editoptions: { value: ':;R:Railway;S:Ocean Shipping;A:Air;T:Truck;I:Intermodal' }, edittype: 'select' },
        { name: 'DepAddr3', title: '', index: 'DepAddr3', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
        { name: 'Trucker3', title: '', index: 'Trucker3', editoptions: gridLookup(getTrucker3op("TruckerNm3")), edittype: 'custom', width: 120, align: 'left', hidden: false, editable: IsBooking() },
		{ name: 'TruckerNm3', title: '', index: 'TruckerNm3', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() },
		{ name: 'BackLocation', title: '', index: 'BackLocation', editoptions: gridLookup(getBackLcop("CntrGrid", "BackLocation")), edittype: 'custom', sorttype: 'string', width: 120, hidden: false, editable: true },
		{
		    name: 'EmptyTime', title: '', index: 'EmptyTime', sorttype: 'string', width: 110, hidden: false, editable: IsBooking(),
			formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i' }, 
			editoptions: myEditDateInit,
			formatter: 'date',
			formatoptions: {
				srcformat: 'ISO8601Long',
				newformat: 'Y-m-d H:i',
				defaultValue: ""
			}
		}
	];
	CntrModel.push({
	    name: 'PickupCdate', title: 'PickupCdate', index: 'PickupCdate', sorttype: 'string', width: 110, hidden: false, editable: IsBooking(),
		formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i' },
		editoptions: myEditDateInit,
		formatter: 'date',
		formatoptions: {
			srcformat: 'ISO8601Long',
			newformat: 'Y-m-d H:i',
			defaultValue: ""
		}
	});

	CntrModel.push({ name: 'PinNo', title: 'PinNo', index: 'PinNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true });

	CntrModel.push({
		name: 'DischargeDate', title: 'DischargeDate', index: 'DischargeDate', sorttype: 'string', width: 110, hidden: false, editable: true,
		formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, 
		editoptions: myEditDateInit,
		formatter: 'date',
		formatoptions: {
			srcformat: 'ISO8601Long',
			newformat: 'Y-m-d',
			defaultValue: ""
		}
	});
	
	CntrModel.push({ name: 'Priority', title: '', index: 'Priority', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking(), formatter: "select", editoptions: { value: Priority }, edittype: 'select' });

	CntrModel.push({
	    name: 'DetentionDueDate', title: 'End of DET Free Time', index: 'DetentionDueDate', sorttype: 'string', width: 110, hidden: false, editable: IsBooking(),
	    formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i' }, 
	    editoptions: myEditDateInit,
	    formatter: 'date',
	    formatoptions: {
	        srcformat: 'ISO8601Long',
	        newformat: 'Y-m-d H:i',
	        defaultValue: ""
	    }
	});
	CntrModel.push({
	    name: 'DemurrageDueDate', title: 'End of DEM Free Time', index: 'DemurrageDueDate', sorttype: 'string', width: 110, hidden: false, editable: IsBooking(),
	    formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i' }, 
	    editoptions: myEditDateInit,
	    formatter: 'date',
	    formatoptions: {
	        srcformat: 'ISO8601Long',
	        newformat: 'Y-m-d H:i',
	        defaultValue: ""
	    }
	});

	CntrModel.push({
	    name: 'StorageDueDate', title: 'End of STO Free Time', index: 'StorageDueDate ', sorttype: 'string', width: 110, hidden: false, editable: IsBooking(),
	    formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i' }, 
	    editoptions: myEditDateInit,
	    formatter: 'date',
	    formatoptions: {
	        srcformat: 'ISO8601Long',
	        newformat: 'Y-m-d H:i',
	        defaultValue: ""
	    }
	});

	CntrModel.push({ name: 'CallTruckStatus', title: 'CallTruckStatus', index: 'CallTruckStatus', width: 120, align: 'left', sorttype: 'string', hidden: false, formatter: "select", remark: 'A:Arrival;D:Finished TruckCalling;R:SlotTime Booked;C:Confirmed;I:Gate In;G:Berthing in the Dock;V:Cancel;P:Container Sealed;O:Gate Out;E:Truck left plant temporary;U:POD', editoptions: { value: 'A:Arrival;D:Finished TruckCalling;R:SlotTime Booked;C:Confirmed;I:Gate In;G:Berthing in the Dock;V:Cancel;P:Container Sealed;O:Gate Out;E:Truck left plant temporary;U:POD' } });
	CntrModel.push({
	    name: 'IdateL', title: 'In Date', index: 'IdateL', sorttype: 'string', width: 110, hidden: false, editable: false,
	    formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i' }, hidden: false, editable: false,
	    editoptions: myEditDateInit,
	    formatter: 'date',
	    formatoptions: {
	        srcformat: 'ISO8601Long',
	        newformat: 'Y-m-d H:i',
	        defaultValue: ""
	    }
	});
	CntrModel.push({
	    name: 'PodDateL', title: 'Upload Pod Date', index: 'PodDateL ', sorttype: 'string', width: 110, hidden: false, editable: false,
	    formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i' }, hidden: false, editable: false,
	    editoptions: myEditDateInit,
	    formatter: 'date',
	    formatoptions: {
	        srcformat: 'ISO8601Long',
	        newformat: 'Y-m-d H:i',
	        defaultValue: ""
	    }
	});
	CntrModel.push({
	    name: 'OdateL', title: 'Out Date', index: 'OdateL ', sorttype: 'string', width: 110, hidden: false, editable: false,
	    formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i' }, 
	    editoptions: myEditDateInit,
	    formatter: 'date',
	    formatoptions: {
	        srcformat: 'ISO8601Long',
	        newformat: 'Y-m-d H:i',
	        defaultValue: ""
	    }
	});
	CntrModel.push({ name: 'PoNo', title: 'PoNo', index: 'PoNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: IsBooking() });

	CntrModel.push({ name: 'WsCd', title: 'Ws Cd', index: 'WsCd', sorttype: 'string', width: 80, editable: false, hidden: false });
	CntrModel.push({ name: 'WsNm', title: 'Ws Name', index: 'WsNm', sorttype: 'string', width: 120, editable: false, hidden: false });
	CntrModel.push({ name: 'FinalWh', title: 'Final WH', index: 'FinalWh', sorttype: 'string', width: 80, editable: false, hidden: false });
	CntrModel.push({ name: 'Wo', title: 'WO', index: 'Wo', sorttype: 'string', width: 80, wditable: false, hidden: false });
    
	CntrModel.push({ name: 'Qty', title: 'QTY', index: 'Qty', sorttype: 'string', width: 80, wditable: false, hidden: false });
	CntrModel.push({ name: 'Nw', title: 'Net Weight', index: 'Nw', sorttype: 'string', width: 80, wditable: false, hidden: false });
	CntrModel.push({ name: 'InvNo', title: 'Invoice No', index: 'InvNo', sorttype: 'string', width: 80, wditable: false, hidden: false });
	if (TRAN_TYPE == 'R') {
		CntrModel.push({
			name: 'EmpPickDate', title: 'Empty Pick Up Date', index: 'EmpPickDate ', sorttype: 'string', width: 110, hidden: false, editable: false,
			formatter: 'date', formatoptions: { newformat: 'Y-m-d' },
			editoptions: myEditDateInit,
			formatter: 'date',
			formatoptions: {
				srcformat: 'ISO8601Long',
				newformat: 'Y-m-d',
				defaultValue: ""
			}
		});
	}
	CntrModel.push({
		name: 'StoExceedTime', title: 'STO Exceeded Time (Estimation)', index: 'StoExceedTime', sorttype: 'number', width: 80, align: 'right', wditable: false, hidden: true,
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 0,
			defaultValue: ''
		}	});
	CntrModel.push({
		name: 'DetExceedTime', title: 'DET Exceeded Time (Estimation)', index: 'DetExceedTime', sorttype: 'number', width: 80, align: 'right', wditable: false, hidden: true,
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 0,
			defaultValue: ''
		}	});
	CntrModel.push({
		name: 'DemExceedTime', title: 'DEM Exceeded Time (Estimation)', index: 'DemExceedTime', sorttype: 'number', width: 80, align: 'right', wditable: false, hidden: true,
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 0,
			defaultValue: ''
		}	});
	CntrModel.push({
		name: 'StoEstFrom', title: 'STO EST. From (Date)', index: 'StoEstFrom', sorttype: 'string', width: 80, wditable: false, hidden: true,
		formatter: 'date', formatoptions: { newformat: 'Y-m-d' },
		editoptions: myEditDateInit,
		formatter: 'date',
		formatoptions: {
			srcformat: 'ISO8601Long',
			newformat: 'Y-m-d',
			defaultValue: ""
		}	});
	CntrModel.push({
		name: 'DetEstFrom', title: 'DET EST. From (Date)', index: 'DetEstFrom', sorttype: 'string', width: 80, wditable: false, hidden: true,
		formatter: 'date', formatoptions: { newformat: 'Y-m-d' },
		editoptions: myEditDateInit,
		formatter: 'date',
		formatoptions: {
			srcformat: 'ISO8601Long',
			newformat: 'Y-m-d',
			defaultValue: ""
		}	});
	CntrModel.push({
		name: 'DemEstFrom', title: 'DEM EST. From (Date)', index: 'DemEstFrom', sorttype: 'string', width: 80, wditable: false, hidden: true,
		formatter: 'date', formatoptions: { newformat: 'Y-m-d' },
		editoptions: myEditDateInit,
		formatter: 'date',
		formatoptions: {
			srcformat: 'ISO8601Long',
			newformat: 'Y-m-d',
			defaultValue: ""
		}	});
	CntrModel.push({
		name: 'StoEstTo', title: 'STO EST. To (Date)', index: 'StoEstTo', sorttype: 'string', width: 80, wditable: false, hidden: true,
		formatter: 'date', formatoptions: { newformat: 'Y-m-d' },
		editoptions: myEditDateInit,
		formatter: 'date',
		formatoptions: {
			srcformat: 'ISO8601Long',
			newformat: 'Y-m-d',
			defaultValue: ""
		}	});
	CntrModel.push({
		name: 'DetEstTo', title: 'DET EST. To (Date)', index: 'DetEstTo', sorttype: 'string', width: 80, wditable: false, hidden: true,
		formatter: 'date', formatoptions: { newformat: 'Y-m-d' },
		editoptions: myEditDateInit,
		formatter: 'date',
		formatoptions: {
			srcformat: 'ISO8601Long',
			newformat: 'Y-m-d',
			defaultValue: ""
		}	});
	CntrModel.push({
		name: 'DemEstTo', title: 'DEM EST. To (Date)', index: 'DemEstTo', sorttype: 'string', width: 80, wditable: false, hidden: true,
		formatter: 'date', formatoptions: { newformat: 'Y-m-d' },
		editoptions: myEditDateInit,
		formatter: 'date',
		formatoptions: {
			srcformat: 'ISO8601Long',
			newformat: 'Y-m-d',
			defaultValue: ""
		}	});
	CntrModel.push({ name: 'StoEstCur', title: 'STO EST. CUR', index: 'StoEstCur', sorttype: 'string', width: 80, wditable: false, hidden: true });
	CntrModel.push({ name: 'DetEstCur', title: 'DET EST. CUR', index: 'DetEstCur', sorttype: 'string', width: 80, wditable: false, hidden: true });
	CntrModel.push({ name: 'DemEstCur', title: 'DEM EST. CUR', index: 'DemEstCur', sorttype: 'string', width: 80, wditable: false, hidden: true });
	CntrModel.push({
		name: 'StoEstAmt', title: 'STO EST. Cost', index: 'StoEstAmt', sorttype: 'number', width: 80, align: 'right',wditable: false, hidden: true,
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 2,
			defaultValue: ''
		}	});
	CntrModel.push({
		name: 'DemEstAmt', title: 'DEM EST. Cost', index: 'DemEstAmt', sorttype: 'number', width: 80, align: 'right',wditable: false, hidden: true,
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 2,
			defaultValue: ''
		}	});
	CntrModel.push({
		name: 'DetEstAmt', title: 'DET EST. Cost', index: 'DetEstAmt', sorttype: 'number', width: 80, align: 'right', wditable: false, hidden: true,
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 2,
			defaultValue: ''
		}});
	CntrModel.push({ name: 'StoActCur', title: 'STO Acct. CUR', index: 'StoActCur', sorttype: 'string', width: 80, wditable: false, hidden: true });
	CntrModel.push({ name: 'DemActCur', title: 'DEM Acct. CUR', index: 'DemActCur', sorttype: 'string', width: 80, wditable: false, hidden: true });
	CntrModel.push({ name: 'DetActCur', title: 'DET Acct. CUR', index: 'DetActCur', sorttype: 'string', width: 80, wditable: false, hidden: true });
	CntrModel.push({
		name: 'StoActAmt', title: 'STO Acct. Cost', index: 'StoActAmt', sorttype: 'number', width: 80, align: 'right',wditable: false, hidden: true,
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 2,
			defaultValue: ''
		}	});
	CntrModel.push({
		name: 'DemActAmt', title: 'DEM Acct. Cost', index: 'DemActAmt', sorttype: 'number', width: 80, align: 'right', wditable: false, hidden: true,
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 2,
			defaultValue: ''
		}	});
	CntrModel.push({
		name: 'DetActAmt', title: 'DET Acct. Cost', index: 'DetActAmt', sorttype: 'number', width: 80, align: 'right', wditable: false, hidden: true,
		formatoptions:{
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 2,
			defaultValue: ''
		}	});
	CntrModel.push({
		name: 'StoAmtForecast', title: 'STO Cost (Forecast)', index: 'StoAmtForecast', sorttype: 'number', width: 80, align: 'right', wditable: false, hidden: true,
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 2,
			defaultValue: ''
		}	});
	CntrModel.push({
		name: 'DemAmtForecast', title: 'DEM Cost (Forecast)', index: 'DemAmtForecast', sorttype: 'number', width: 80, align: 'right', wditable: false, hidden: true,
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 2,
			defaultValue: ''
		}	});
	CntrModel.push({
		name: 'DetAmtForecast', title: 'DET Cost (Forecast)', index: 'DetAmtForecast', sorttype: 'number', width: 80, align: 'right',wditable: false, hidden: true,
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 2,
			defaultValue: ''
		}	});
	CntrModel.push({ name: 'StoCurForecast', title: 'STO CUR (Forecast)', index: 'StoCurForecast', sorttype: 'string', width: 80, wditable: false, hidden: true });
	CntrModel.push({ name: 'DemCurForecast', title: 'DEM CUR (Forecast)', index: 'DemCurForecast', sorttype: 'string', width: 80, wditable: false, hidden: true });
	CntrModel.push({ name: 'DetCurForecast', title: 'DET CUR (Forecast)', index: 'DetCurForecast', sorttype: 'string', width: 80, wditable: false, hidden: true });
	for (var i = 0; i < CntrModel.length; i++) {
	    if (i < CntrGridLang.length)
	        CntrModel[i]['title'] = CntrGridLang[i];
	}
}

var colModel5 = [
	{ name: 'UId', title: 'UID', index: 'UId', sorttype: 'string', editable: false, hidden: true },
	{ name: 'UFid', title: 'UFID', index: 'UId', sorttype: 'string', editable: false, hidden: true },
	{ name: 'InvNo', title: _getLang("L_SMIDN_InvoiceNo", "Invoice No"), index: 'InvNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false},
	{ name: 'DnNo', title:'Dn No', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'PlaNo', title: 'Pallet No', index: 'PlaNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'PlaSize', title: 'Pallet Size', index: 'PlaSize', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'CaseNo', title: 'Case No', index: 'CaseNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'CaseNum', title: 'Case#', index: 'CaseNum', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'IpartNo', title: _getLang("L_DNApproveManage_IpartNo", "对内机种名"), index: 'IpartNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'GoodsDescp', title: 'Goods Description', index: 'GoodsDescp', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'LgoodsDescp', title: 'Chinese Goods Description', index: 'LgoodsDescp', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'Qty', title: _getLang("L_BaseLookup_Qty", "数量"), index: 'Qty', width: 150, align: 'right', sorttype: 'int', hidden: false, editable: false, formatter: 'integer' },
	{ name: 'Qtyu', title: _getLang("L_BaseLookup_Qtyu", "数量单位"), index: 'Qtyu', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'TtlQty', title: _getLang("L_InvPkgSetup_TtlQty", "总数量"), index: 'TtlQty', width: 150, align: 'right', sorttype: 'int', hidden: false, editable: false, formatter: 'integer' },
	{
		name: 'Nw', title: _getLang("L_BaseLookup_Nw", "净重"), index: 'Nw', width: 150, align: 'right', sorttype: 'float', hidden: false, editable: false, formatter: 'number',
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 6,
			defaultValue: '0.000000'
		}
	},
	{
		name: 'TtlNw', title: _getLang("L_InvPkgSetup_TtlNw", "总净重"), index: 'TtlNw', width: 150, align: 'right', sorttype: 'float', hidden: false, editable: false, formatter: 'number',
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 6,
			defaultValue: '0.000000'
		}
	},
	{
		name: 'Gw', title: _getLang("L_BaseLookup_Gw", "毛重"), index: 'Gw', width: 150, align: 'right', sorttype: 'float', hidden: false, editable: false, formatter: 'number',
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 6,
			defaultValue: '0.000000'
		}
	},
	{
		name: 'TtlGw', title: _getLang("L_InvPkgSetup_TtlGw", "总毛重"), index: 'TtlGw', width: 150, align: 'right', sorttype: 'float', hidden: false, editable: false, formatter: 'number',
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 6,
			defaultValue: '0.000000'
		}
	},
	{
		name: 'Cbm', title: _getLang("L_BaseLookup_Cbm", "CBM"), index: 'Cbm', width: 150, align: 'right', sorttype: 'float', hidden: false, editable: false, formatter: 'number',
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 6,
			defaultValue: '0.000000'
		}
	},
	{
		name: 'TtlCbm', title: _getLang("L_InvPkgSetup_TtlCbm", "总CBM"), index: 'TtlCbm', width: 150, align: 'right', sorttype: 'float', hidden: false, editable: false, formatter: 'number',
		formatoptions: {
			decimalSeparator: ".",
			thousandsSeparator: ",",
			decimalPlaces: 6,
			defaultValue: '0.000000'
		}
	},
	{ name: 'IhsCode', title: _getLang("L_DNApproveManage_HisCode", "目的国商品编码"), index: 'IhsCode', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'OpartNo', title: _getLang("L_SMIDNP_OpartNo", "对外机种名"), index: 'OpartNo', sorttype: 'string', width: 90, hidden: false, editable: false },
	{ name: 'NcmNo', title: 'NCM NO', index: 'NcmNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'CntryOrn', title: 'Original Country', index: 'CntryOrn', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'PartNo', title: _getLang("L_DNApproveManage_PartNo", "客户物料号"), index: 'PartNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'VenCd', title: 'Vendor Code', index: 'VenCd', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'VenNm', title: 'Vendor Name', index: 'VenNm', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'VenAddr', title: 'Vendor Address', index: 'VenAddr', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'CntrNo', title: 'Container No.', index: 'CntrNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'Remark', title: _getLang("L_BSCSSetup_Remark", "备注"), index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'GwByPn', title: 'Gross Weight by PN', index: 'GwByPn', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'ModelName', title: 'Customer Model', index: 'ModelName', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
	{ name: 'CnCode', title: 'CN Code', index: 'CnCode', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false }
];

var shipmentColModel = [
	{ name: 'UId', title: 'UID', index: 'UId', sorttype: 'string', editable: false, hidden: true },
	{ name: 'ShipmentId', title: _getLang("L_DNApproveManage_ShipmentId", "Shipment ID"), index: 'ShipmentId', sorttype: 'string', width: 100, hidden: false, editable: false },
	{ name: 'MasterNo', title: 'Master B/L', index: 'MasterNo', sorttype: 'string', width: 100, hidden: false, editable: false },
	{ name: 'HouseNo', title: 'House B/L', index: 'HouseNo', sorttype: 'string', width: 100, hidden: false, editable: false },
	{ name: 'CntrInfo', title: 'Container No', index: 'CntrInfo', sorttype: 'string', width: 100, hidden: false, editable: false },
	{ name: 'InvoiceInfo', title: 'Invoice No', index: 'InvoiceInfo', sorttype: 'string', width: 100, hidden: false, editable: false },
	{
		name: 'Eta', title: 'ETA', index: 'Eta', sorttype: 'string', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false,
		formatter: 'date',
		formatoptions: {
			srcformat: 'ISO8601Long',
			newformat: 'Y-m-d',
			defaultValue: ""
		}, width: 100, editable: false
	},
	{
		name: 'Ata', title: 'ATA', index: 'Ata', sorttype: 'string', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false,
		formatter: 'date',
		formatoptions: {
			srcformat: 'ISO8601Long',
			newformat: 'Y-m-d',
			defaultValue: ""
		}, width: 100, editable: false
	},
	{
		name: 'DischargeDate', title: 'Discharge Date', index: 'DischargeDate', sorttype: 'string', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false,
		formatter: 'date',
		formatoptions: {
			srcformat: 'ISO8601Long',
			newformat: 'Y-m-d',
			defaultValue: ""
		}, width: 100, editable: false
	},
	{ name: 'PodCd', title: 'POD', index: 'PodCd', sorttype: 'string', width: 100, hidden: false, editable: false },
	{ name: 'PodName', title: 'POD Name', index: 'PodName', sorttype: 'string', width: 100, hidden: false, editable: false },
	{ name: 'Carrier', title: 'Carrier', index: 'Carrier', sorttype: 'string', width: 100, hidden: false, editable: false },
	{ name: 'CarrierNm', title: 'Carrier Name', index: 'CarrierNm', sorttype: 'string', width: 100, hidden: false, editable: false }
];



function genPartyGrid()
{
	_handler.intiGrid("SubGrid", $SubGrid, {
		colModel: returnPartyModel("SubGrid", $SubGrid), caption: 'Inbound Party', delKey: ["UId", "PartyType"],
		onAddRowFunc: function (rowid) {
		},
		beforeSelectRowFunc: function (rowid) {
			if (rowid != null && rowid.indexOf("jqg") >= 0) {
				$SubGrid.setColProp('PartyType', { editable: true });
			} else {
				$SubGrid.setColProp('PartyType', { editable: false });
			}
		},
		onAddRowFunc: function (rowid) {
			var UId = getGridVal($SubGrid, rowid, "UId", null);
			if (UId == "" || UId == null) {
				UId = genUid(uuid());
				$SubGrid.jqGrid('setCell', rowid, "UId", UId, 'edit-cell dirty-cell');
			}
		},
		beforeAddRowFunc: function (rowid) {
			$SubGrid.setColProp('PartyType', { editable: true });
		}
	});



	_handler.intiGrid("SubGrid2", $SubGrid2, {
		colModel: colModel2, 
		caption: commonLang["L_SMSMI_InvoiceDetail"],
		delKey: ["DnNo"],
		showcolumns: true,
		savelayout: true,
		exportexcel: true, url: rootPath + "SMSMI/SmidnExport" , postData: { "conditions": _uid },
		lockRKey: true,
		onAddRowFunc: function (rowid) {
		},
		beforeSelectRowFunc: function (rowid) {
		},
		onAddRowFunc: function (rowid) {
		},
		beforeAddRowFunc: function (rowid) {
			$SubGrid.setColProp('DnNo', { editable: true });
		},
		afterSaveCellFunc: function (rowid, name, val, iRow, iCol) {
			if(name == "ScmrequestDate")
			{
				var Eta = $("#Eta").val();
				if(Date.parse(val).valueOf() < Date.parse(Eta).valueOf())
				{
					alert("SCM Requested Date have to after ETA");
					setGridVal($SubGrid2, rowid, 'ScmrequestDate', "", null);
					return;
				}
			}
		}
	}, "DnNo", true);

	_handler.intiGrid("SubGrid3", $SubGrid3, {
		colModel: colModel3,
		caption: "DN Detail",//commonLang["L_SMSMI_ContainerInfo"],
		delKey: ["UId"],
        showcolumns: true,
		savelayout: true,
		exportexcel: true, url: rootPath + "SMSMI/SmidnpExport" , postData: { "conditions": _uid },
		onAddRowFunc: function (rowid) {
		},
		beforeSelectRowFunc: function (rowid) {
		},
		onAddRowFunc: function (rowid) {
		},
		beforeAddRowFunc: function (rowid) {
			$SubGrid.setColProp('DnNo', { editable: true });
		},
	}, "UId", false);

	if (TRAN_TYPE == 'A' || TRAN_TYPE == 'E' || TRAN_TYPE == 'L')
	{
		_handler.intiGrid("SubGrid4", $SubGrid4, {
			colModel: ScufcolModel, caption: 'DN Dimensions Info.', delKey: ["UId", "UFid"],
			onAddRowFunc: function (rowid) {
			},
			beforeSelectRowFunc: function (rowid) {
			},
			beforeAddRowFunc: function (rowid) {
			},
			afterSaveCellFunc: function (rowid, name, val, iRow, iCol) {
				var BkdData = $('#SubGrid4').jqGrid("getGridParam", "data");
				var Lamt = 0, StBoAmt = 0, BlAmt = 0, MtBoAmt = 0;
				var SumStBlAmt = $('#BkpGrid').jqGrid("getCol", "BlAmt", false, "sum");

				var l = $("#SubGrid4").jqGrid('getCell', rowid, "L");
				var w = $("#SubGrid4").jqGrid('getCell', rowid, "W");
				var h = $("#SubGrid4").jqGrid('getCell', rowid, "H");
				var Pkg = $("#SubGrid4").jqGrid('getCell', rowid, "Pkg");
				 
				var vw = l * w * h * Pkg / 1000000;
				$('#SubGrid4').jqGrid('setCell', rowid, "Vw", CommonFunc.formatFloatNoComma(vw, 4));

				var SumVw = $SubGrid4.jqGrid("getCol", "Vw", false, "sum");
				if (SumVw != 0) {
					$("#Cbm").val(CommonFunc.formatFloatNoComma(SumVw, 4));
				}
			}
		});
	}

	_handler.intiGrid("DnGrid", $DnGrid, {
		colModel: BaseBooking_DnModel, 
		caption: commonLang["L_DNManage_DNDeclaInfo"], 
		delKey: ["DnNo"],
		onAddRowFunc: function (rowid) {
		},
		beforeSelectRowFunc: function (rowid) {
		},
		beforeAddRowFunc: function (rowid) {
		}		
	}, "DnNo", false);

	var excelUrl = (TRAN_TYPE == 'F' || TRAN_TYPE == 'R') ? "SMSMI/SmicntrExport" : "SMSMI/SmidnExport";

	if (TRAN_TYPE != 'T') {
		_handler.intiGrid("CcGrid", $CcGrid, {
			colModel: CcolModel,
			caption: commonLang["L_SMSMI_InvoiceDetailCC"],
			delKey: ["UId"],
			showcolumns: true,
			savelayout: true,
			exportexcel: true, url: rootPath + excelUrl, postData: { "conditions": _uid },
			lockRKey: true,
			onAddRowFunc: function (rowid) {
			},
			beforeSelectRowFunc: function (rowid) {
			},
			onAddRowFunc: function (rowid) {
			},
			beforeAddRowFunc: function (rowid) {
				$SubGrid.setColProp('InvNo', { editable: true });
			},
		}, "UId", true);
    }

    if (TRAN_TYPE!='T') {
		_handler.intiGrid("TcGrid", $TcGrid, {
			colModel: TcolModel,
			caption: commonLang["L_SMSMI_InvoiceDetailTC"],
			delKey: ["UId"],
			showcolumns: true,
			savelayout: true,
			exportexcel: true, url: rootPath + excelUrl, postData: { "conditions": _uid },
			lockRKey: true,
			onAddRowFunc: function (rowid) {
			},
			beforeSelectRowFunc: function (rowid) {
			},
			onAddRowFunc: function (rowid) {
			},
			beforeAddRowFunc: function (rowid) {
				$SubGrid.setColProp('InvNo', { editable: true });
			},
		}, "UId", true);
    }

	if(TRAN_TYPE == 'F' || TRAN_TYPE == 'L' || TRAN_TYPE == 'R')
	{
		_handler.intiGrid("CntrGrid", $CntrGrid, {
		    colModel: CntrModel,
			caption: "Container Information",
			delKey: ["UId"],
            showcolumns: true,
            savelayout: true,
			onAddRowFunc: function (rowid) {

			},
			beforeSelectRowFunc: function (rowid) {
			    if (rowid != null && rowid.indexOf("jqg") >= 0) {
			        $CntrGrid.setColProp('StsCd', { editable: true });
			    } else {
			        $CntrGrid.setColProp('StsCd', { editable: false });
			    }
			},
			onAddRowFunc: function (rowid) {

			},
			beforeAddRowFunc: function (rowid) {
			    $CntrGrid.setColProp('StsCd', { editable: true });
			},
			afterSaveCellFunc: function (rowid, name, val, iRow, iCol) {
				if(name == "ScmrequestDate")
				{
					var Eta = $("#Eta").val();

					if(Date.parse(val).valueOf() < Date.parse(Eta).valueOf())
					{
						alert("SCM Requested Date have to after ETA");
						setGridVal($CntrGrid, rowid, 'ScmrequestDate', "", null);
						return;
					}
				}
			}
		}, "UId", true);
	}
	if (typeof $SubGrid5 != 'undefined') {
		_handler.intiGrid("SubGrid5", $SubGrid5, {
			colModel: colModel5,
			caption: "DN Pallet List",
			delKey: ["UId"],
			showcolumns: true,
			onAddRowFunc: function (rowid) {
			},
			beforeSelectRowFunc: function (rowid) {
			},
			beforeAddRowFunc: function (rowid) {
			}
		}, "UId", false);
    } 
}


function genTransloadGrid() {
	_handler.intiGrid("SubGrid", $SubGrid, {
		colModel: returnPartyModel("SubGrid", $SubGrid), caption: 'Inbound Party', delKey: ["UId", "PartyType"],
		onAddRowFunc: function (rowid) {
		},
		beforeSelectRowFunc: function (rowid) {
			if (rowid != null && rowid.indexOf("jqg") >= 0) {
				$SubGrid.setColProp('PartyType', { editable: true });
			} else {
				$SubGrid.setColProp('PartyType', { editable: false });
			}
		},
		onAddRowFunc: function (rowid) {
			var UId = getGridVal($SubGrid, rowid, "UId", null);
			if (UId == "" || UId == null) {
				UId = genUid(uuid());
				$SubGrid.jqGrid('setCell', rowid, "UId", UId, 'edit-cell dirty-cell');
			}
		},
		beforeAddRowFunc: function (rowid) {
			$SubGrid.setColProp('PartyType', { editable: true });
		}
	});

	_handler.intiGrid("CntrGrid", $CntrGrid, {
		colModel: CntrModel,
		caption: "Container Information",
		delKey: ["UId"],
		showcolumns: true,
		savelayout: true,
		onAddRowFunc: function (rowid) {

		},
		beforeSelectRowFunc: function (rowid) {
			if (rowid != null && rowid.indexOf("jqg") >= 0) {
				$CntrGrid.setColProp('StsCd', { editable: true });
			} else {
				$CntrGrid.setColProp('StsCd', { editable: false });
			}
		},
		onAddRowFunc: function (rowid) {

		},
		beforeAddRowFunc: function (rowid) {
			$CntrGrid.setColProp('StsCd', { editable: true });
		},
		afterSaveCellFunc: function (rowid, name, val, iRow, iCol) {
			if (name == "ScmrequestDate") {
				var Eta = $("#Eta").val();

				if (Date.parse(val).valueOf() < Date.parse(Eta).valueOf()) {
					alert("SCM Requested Date have to after ETA");
					setGridVal($CntrGrid, rowid, 'ScmrequestDate', "", null);
					return;
				}
			}
		}
	}, "UId", true);

	_handler.intiGrid("SubGrid5", $SubGrid5, {
		colModel: colModel5,
		caption: "DN Pallet List",
		delKey: ["UId"],
		onAddRowFunc: function (rowid) {
		},
		beforeSelectRowFunc: function (rowid) {
		},
		beforeAddRowFunc: function (rowid) {
		}
	}, "UId", false);


	_handler.intiGrid("ShipmentGrid", $ShipmentGrid, {
		colModel: shipmentColModel,
		caption: "Shipment List",
		delKey: ["UId"],
		showcolumns: true,
		onAddRowFunc: function (rowid) {
		},
		beforeSelectRowFunc: function (rowid) {
		},
		beforeAddRowFunc: function (rowid) {
		}
	}, "UId", false);
}

function returnPartyModel(_partygrid,_$partygrid) {//"SubGrid", $SubGrid
	function getpartyop(_partygrid,_$partygrid, name) {
		var _name = name;
		var city_op = getLookupOp(_partygrid,
			{
				url: rootPath + LookUpConfig.PartyNoUrl,
				config: LookUpConfig.PartyNoLookup,
				returnFn: function (returnObj, $grid) {
					var selRowId = $grid.jqGrid('getGridParam', 'selrow');

					setGridVal($grid, selRowId, 'PartyName', returnObj.PartyName, null);
					setGridVal($grid, selRowId, 'PartyMail', returnObj.PartyMail, null);
					setGridVal($grid, selRowId, 'PartAddr1', returnObj.PartAddr1, null);
					setGridVal($grid, selRowId, 'PartAddr2', returnObj.PartAddr2, null);
					setGridVal($grid, selRowId, 'PartAddr3', returnObj.PartAddr3, null);
					setGridVal($grid, selRowId, 'PartyAttn', returnObj.PartyAttn, null);
					setGridVal($grid, selRowId, 'State', returnObj.State, null);
					setGridVal($grid, selRowId, 'Zip', returnObj.Zip, null);
					setGridVal($grid, selRowId, 'PartyTel', returnObj.PartyTel, null);
					setGridVal($grid, selRowId, 'DebitTo', returnObj.BillTo, null);
					setGridVal($grid, selRowId, 'Cnty', returnObj.Cnty, null);
					setGridVal($grid, selRowId, 'CntyNm', returnObj.CntyNm, null);
					setGridVal($grid, selRowId, 'City', returnObj.City, null);
					setGridVal($grid, selRowId, 'CityNm', returnObj.CityNm, null);

					setGridVal($grid, selRowId, 'PartAddr4', returnObj.PartAddr4, null);
					setGridVal($grid, selRowId, 'PartAddr5', returnObj.PartAddr5, null);
					setGridVal($grid, selRowId, 'PartyName2', returnObj.PartyName2, null);
					setGridVal($grid, selRowId, 'PartyName3', returnObj.PartyName3, null);
					setGridVal($grid, selRowId, 'PartyName4', returnObj.PartyName4, null);
					setGridVal($grid, selRowId, 'FaxNo', returnObj.PartyFax, null);
					setGridVal($grid, selRowId, 'TaxNo', returnObj.TaxNo, null);
					return returnObj.PartyNo;
				}
			},
			{
				baseConditionFunc: function () {
					var selRowId = $("#" + _partygrid).jqGrid('getGridParam', 'selrow');
					var PartyType = $("#" + _partygrid).jqGrid('getCell', selRowId, 'PartyType');
					return "PARTY_TYPE LIKE '%" + PartyType + "%'";
					//var selRowId = $("#SubGrid").jqGrid('getGridParam', 'selrow');
					//var PartyType = $("#SubGrid").jqGrid('getCell', selRowId, 'PartyType');
					//return "PARTY_TYPE LIKE '%" + PartyType + ";%'";
				}
			}, LookUpConfig.GetPartyNoAuto(groupId, _$partygrid,
			function ($grid, rd, elem, rowid) {
				var selRowId = rowid;
				setGridVal($grid, selRowId, 'PartyName', rd.PARTY_NAME, null);
				setGridVal($grid, selRowId, 'PartyMail', rd.PARTY_MAIL, null);
				setGridVal($grid, selRowId, 'PartAddr1', rd.PART_ADDR1, null);
				setGridVal($grid, selRowId, 'PartAddr2', rd.PART_ADDR2, null); 
				setGridVal($grid, selRowId, 'PartAddr3', rd.PART_ADDR3, null);
				setGridVal($grid, selRowId, 'PartyAttn', rd.PARTY_ATTN, null);
				setGridVal($grid, selRowId, 'State', rd.STATE, null);
				setGridVal($grid, selRowId, 'Zip', rd.ZIP, null);
				setGridVal($grid, selRowId, 'PartyTel', rd.PARTY_TEL, null);
				setGridVal($grid, selRowId, 'PartyNo', rd.PARTY_NO, 'lookup');
				setGridVal($grid, selRowId, 'DebitTo', rd.BILL_TO, null);
				setGridVal($grid, selRowId, 'Cnty', rd.CNTY, null);
				setGridVal($grid, selRowId, 'CntyNm', rd.CNTY_NM, null);
				setGridVal($grid, selRowId, 'City', rd.CITY, null);
				setGridVal($grid, selRowId, 'CityNm', rd.CITY_NM, null);

				setGridVal($grid, selRowId, 'PartAddr4', rd.PART_ADDR4, null);
				setGridVal($grid, selRowId, 'PartAddr5', rd.PART_ADDR5, null);
				setGridVal($grid, selRowId, 'PartyName2', rd.PARTY_NAME2, null);
				setGridVal($grid, selRowId, 'PartyName3', rd.PARTY_NAME3, null);
				setGridVal($grid, selRowId, 'PartyName4', rd.PARTY_NAME4, null);
				setGridVal($grid, selRowId, 'FaxNo', rd.PARTY_FAX, null);
				setGridVal($grid, selRowId, 'TaxNo', rd.TAX_NO, null);
			}, function ($grid, rd, elem, rowid) {
				var selRowId = rowid;
				setGridVal($grid, selRowId, 'PartyName', '', null);
				setGridVal($grid, selRowId, 'PartyMail', '', null);
				setGridVal($grid, selRowId, 'PartAddr1', '', null);
				setGridVal($grid, selRowId, 'PartAddr2', '', null);
				setGridVal($grid, selRowId, 'PartAddr3', '', null);
				setGridVal($grid, selRowId, 'PartyAttn', '', null);
				setGridVal($grid, selRowId, 'State', '', null);
				setGridVal($grid, selRowId, 'Zip', '', null);
				setGridVal($grid, selRowId, 'PartyTel', '', null);
				setGridVal($grid, selRowId, 'PartyNo', '', 'lookup');
				setGridVal($grid, selRowId, 'DebitTo', '', null);
				setGridVal($grid, selRowId, 'Cnty', '', null);
				setGridVal($grid, selRowId, 'CntyNm', '', null);
				setGridVal($grid, selRowId, 'City', '', null);
				setGridVal($grid, selRowId, 'CityNm', '', null);
				setGridVal($grid, selRowId, 'PartAddr4', '', null);
				setGridVal($grid, selRowId, 'PartAddr5', '', null);
				setGridVal($grid, selRowId, 'PartyName2', '', null);
				setGridVal($grid, selRowId, 'PartyName3', '', null);
				setGridVal($grid, selRowId, 'PartyName4', '', null);
				setGridVal($grid, selRowId, 'FaxNo', '', null);
				setGridVal($grid, selRowId, 'TaxNo', '', null);
			}));
		city_op.param = "";
		return city_op;
	}
	function getop(_partygrid, _$partygrid, name) {
		var _name = name;
		var city_op = getLookupOp(_partygrid,
			{
				url: rootPath + LookUpConfig.PartyTypeUrl,
				config: LookUpConfig.PartyTypeLookup,
				returnFn: function (map, $grid) {
					var selRowId = $grid.jqGrid('getGridParam', 'selrow');
					setGridVal($grid, selRowId, 'TypeDescp', map.CdDescp, null);
					setGridVal($grid, selRowId, 'OrderBy', map.OrderBy, null);
					return map.Cd;
				}
			}, LookUpConfig.GetCodeTypeAuto(groupId, "PT", _$partygrid, function ($grid, rd, elem, rowid) {
				//$("#PartyType").val(rd.CD);
				var selRowId = rowid;
				setGridVal($grid, selRowId, 'TypeDescp', rd.CD_DESCP, null);
				setGridVal($grid, selRowId, 'OrderBy', rd.ORDER_BY, null);
				setGridVal($grid, selRowId, 'PartyType', rd.CD, 'lookup');
				//$(elem).val(rd.CD);
			}, function ($grid, elem, rowid) {
				//$("#PartyType").val(rd.CD);
				var selRowId = rowid;
				setGridVal($grid, selRowId, 'TypeDescp', "", null);
				setGridVal($grid, selRowId, 'OrderBy', "", null);
				$(elem).val("");
			}), {
			param: "",
			baseConditionFunc: function () {
				return "";
			}
		});
		return city_op;
	}

	var partyColModel = [
		{ name: 'UId', title: 'null', index: 'UId', sorttype: 'string', editable: false, hidden: true },
		{ name: 'PartyType', title: 'null', index: 'PartyType', editoptions: gridLookup(getop(_partygrid, _$partygrid, "TypeDescp")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true },
		{ name: 'TypeDescp', title: 'null', index: 'TypeDescp', sorttype: 'string', hidden: false, editable: true },
		{ name: 'PartyNo', title: 'null', index: 'PartyNo', editoptions: gridLookup(getpartyop(_partygrid, _$partygrid, "PartyName")), edittype: 'custom', sorttype: 'string', width: 150, hidden: false, editable: true },
		{ name: 'PartyName', title: 'null', index: 'PartyName', sorttype: 'string', width: 300, hidden: false, editoptions: { maxlength: 70 }, editable: true },
		{ name: 'PartyName2', title: 'null', index: 'PartyName2', sorttype: 'string', width: 200, hidden: false, editoptions: { maxlength: 70 }, editable: true },
		{ name: 'PartyName3', title: 'null', index: 'PartyName3', sorttype: 'string', width: 200, hidden: false, editoptions: { maxlength: 70 }, editable: true },
		{ name: 'PartyName4', title: 'null', index: 'PartyName4', sorttype: 'string', width: 200, hidden: false, editoptions: { maxlength: 70 }, editable: true },
		{ name: 'DebitTo', title: 'null', index: 'DebitTo', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 10 }, editable: true },
		{ name: 'PartyAttn', title: 'null', index: 'PartyAttn', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 20 }, editable: true },
		{ name: 'PartyTel', title: 'null', index: 'PartyTel', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 50 }, editable: true },
		{ name: 'FaxNo', title: 'null', index: 'FaxNo', sorttype: 'string', width: 100, hidden: false,editoptions: { maxlength: 30 }, editable: true },
		{ name: 'PartyMail', title: 'null', index: 'PartyMail', sorttype: 'string', width: 300, hidden: false, editoptions: { maxlength: 100 }, editable: true },
		{ name: 'PartyAddr1', title: 'null', index: 'PartyAddr1', sorttype: 'string', width: 100, hidden: false, editoptions: {maxlength: 60}, editable: true },
		{ name: 'PartyAddr2', title: 'null', index: 'PartyAddr2', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 40 }, editable: true },
		{ name: 'PartyAddr3', title: 'null', index: 'PartyAddr3', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 40 }, editable: true },
		{ name: 'PartyAddr4', title: 'null', index: 'PartyAddr4', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 40 }, editable: true },
		{ name: 'PartyAddr5', title: 'null', index: 'PartyAddr5', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 40 }, editable: true },
		{ name: 'Cnty', title: 'null', index: 'Cnty', editoptions: { maxlength: 2 }, width: 100, hidden: false, editable: true },
		{ name: 'CntyNm', title: 'null', index: 'CntyNm', sorttype: 'string', width: 200, hidden: false, editable: true },
		{ name: 'City', title: 'null', index: 'City', sorttype: 'string', width: 100, hidden: false, editable: true },
		{ name: 'CityNm', title: 'null', index: 'CityNm', sorttype: 'string', width: 200, hidden: false, editable: true },
		{ name: 'State', title: 'null', index: 'State', sorttype: 'string', width: 100, hidden: false, editable: true },
		{ name: 'Zip', title: 'null', index: 'Zip', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 10 }, editable: true },
		 { name: 'TaxNo', title: 'null', index: 'TaxNo', editoptions: { maxlength: 20 }, width: 100, hidden: false, editable: true },
		{ name: 'OrderBy', title: 'null', index: 'OrderBy', sorttype: 'string', width: 100, hidden: true, editable: true }
	];

	for (var i = 0; i < partyColModel.length; i++) {
		partyColModel[i]['title'] = PartyLang[i];
	}
	return partyColModel;
}

/*Edoc上傳後或更新類型後 更新燈號*/
var callBackFunc = function(jobNo, edocType){
	if (edocType == null) {
		return true;
	}

	$.ajax({
		url: rootPath + 'SMSMI/setLight',
		type: 'POST',
		dataType: 'json',
		data: {"OUid":$("#OUid").val(), "UId": $("#UId").val(), "Io": "I" },
		beforeSend: function () {
			//StatusBarArr.nowStatus(language["L_OrderManage_ComfirmNow"]);
			CommonFunc.ToogleLoading(true);
		},
		success: function (result) {
			if (result.message == "success") {
				//CommonFunc.Notify("", language["L_OrderManage_ComfirmSuccess"], 1000, "success");
				//$("#SummarySearch").trigger("click");
			}
			else {
				//CommonFunc.Notify("", result.message, 1000, "warning");
				//alert(result.message);
			}
			CommonFunc.ToogleLoading(false);
		},
		error: function () {
			//CommonFunc.Notify("", "", 1000, "danger");
			CommonFunc.ToogleLoading(false);
		}
	});
}
/*Edoc上傳後或更新類型後 更新燈號 end*/

/*注册放大镜&auto complete*/
$(function(){
	registBtnLookup($("#FreightCurLookup"), 
	{
		item: '#FreightCur',
		 url: rootPath + LookUpConfig.CurUrl, 
		 config: LookUpConfig.CurLookup, 
		 param: "", 
		 selectRowFn: function (map) {
			$("#FreightCur").val(map.Cur);
		}
	}, 
	undefined, 
	LookUpConfig.GetCurAuto(groupId, undefined, function ($grid, rd, elem) {
		$(elem).val(rd.CUR);
	}));

	registBtnLookup($("#CurLookup"), 
	{
		item: '#Cur',
		 url: rootPath + LookUpConfig.CurUrl, 
		 config: LookUpConfig.CurLookup, 
		 param: "", 
		 selectRowFn: function (map) {
			$("#Cur").val(map.Cur);
		}
	}, 
	undefined, 
	LookUpConfig.GetCurAuto(groupId, undefined, 
		function ($grid, rd, elem) {
			$(elem).val(rd.CUR);
		}, 
		function ($grid, elem) {
			$("#Cur").val("");
		}
	));

	registBtnLookup($("#IncotermCdLookup"), {
		item: '#IncotermCd', 
		url: rootPath + LookUpConfig.DlvTermUrl, 
		config: LookUpConfig.TermLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#IncotermCd").val(map.Cd);
		}
	}, 
	undefined, 
	LookUpConfig.GetCodeTypeAuto(groupId, "TD", undefined, 
		function ($grid, rd, elem) {
			$("#IncotermCd").val(rd.CD);
		}, 
		function ($grid, elem) {
			$("#IncotermCd").val("");
		}
	));

	registBtnLookup($("#PkgUnitLookup"), {
		item: '#PkgUnit', 
		url: rootPath + LookUpConfig.QtyuUrl, 
		config: LookUpConfig.QtyuLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#PkgUnit").val(map.Cd);
			$("#PkgUnitDesc").val(map.CdDescp);
		}
	}, 
	undefined, 
	LookUpConfig.GetCodeTypeAuto(groupId, "UB", undefined, 
		function ($grid, rd, elem) {
			$("#PkgUnit").val(rd.CD);
			$("#PkgUnitDesc").val(map.CD_DESCP);
		}, 
		function ($grid, elem) {
			$("#PkgUnit").val("");
			$("#PkgUnitDesc").val("");
		}
	));

	registBtnLookup($("#QtyuLookup"), {
		item: '#Qty', 
		url: rootPath + LookUpConfig.QtyuUrl, 
		config: LookUpConfig.QtyuLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#Qtyu").val(map.Cd);
		}
	}, 
	undefined, 
	LookUpConfig.GetCodeTypeAuto(groupId, "UB", undefined, 
		function ($grid, rd, elem) {
			$("#Qtyu").val(rd.CD);
		}, 
		function ($grid, elem) {
			$("#Qty").val("");
		}
	));

	registBtnLookup($("#TradeTermLookup"), {
		item: '#TradeTerm', 
		url: rootPath + LookUpConfig.TermUrl, 
		config: LookUpConfig.TermLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#TradeTerm").val(map.Cd);
		}
	}, 
	undefined, 
	LookUpConfig.GetCodeTypeAuto(groupId, "TINC", undefined, 
		function ($grid, rd, elem) {
		$("#TradeTerm").val(rd.CD);
		}, 
		function ($grid, elem) {
			$("#TradeTerm").val("");
		}
	));

	registBtnLookup($("#GwuLookup"), {
		item: '#Gwu', 
		url: rootPath + LookUpConfig.NwuUrl, 
		config: LookUpConfig.NwuLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#Gwu").val(map.Cd);
		}
	}, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "UT", undefined, 
		function ($grid, rd, elem) {
			$("#Gwu").val(rd.CD);
		}, 
		function ($grid, elem) {
			$("#Gwu").val("");
		}
	));

    registBtnLookup($("#CarrierLookup"), {
        item: '#Carrier',
        url: rootPath + LookUpConfig.TCARUrl,
        config: LookUpConfig.BSCodeLookup,
        param: "",
        selectRowFn: function (map) {
            $("#Carrier").val(map.Cd);
            $("#CarrierNm").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TCAR", undefined,
        function ($grid, rd, elem) {
            $("#Carrier").val(rd.CD);
            $("#CarrierNm").val(rd.CD_DESCP);
        },
        function ($grid, elem) {
            $("#Carrier").val("");
            $("#CarrierNm").val("");
        }
    ));

    registBtnLookup($("#rCarrierLookup"), {
        item: '#Carrier',
        url: rootPath + LookUpConfig.RCARUrl,
        config: LookUpConfig.BSCodeLookup,
        param: "",
        selectRowFn: function (map) {
            $("#Carrier").val(map.Cd);
            $("#CarrierNm").val(map.CdDescp);
        }, baseConditionFunc: function () {
            return "CD_TYPE='RCAR'";
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "RCAR", undefined,
        function ($grid, rd, elem) {
            $("#Carrier").val(rd.CD);
            $("#CarrierNm").val(rd.CD_DESCP);
        },
        function ($grid, elem) {
            $("#Carrier").val("");
            $("#CarrierNm").val("");
        }
    ));

	registBtnLookup($("#PorCdLookup"), {
		item: '#PorCd', 
		url: rootPath + LookUpConfig.CityPortUrl, 
		config: LookUpConfig.CityPortLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#PorCd").val(map.CntryCd + map.PortCd);
			$("#PorName").val(map.PortNm);
		}
	}, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, 
		function ($grid, rd, elem) {
			$("#PorCd").val(rd.CNTRY_CD + rd.PORT_CD);
			$("#PorName").val(rd.PORT_NM);
		}
	));

	registBtnLookup($("#PolCdLookup"), {
		item: '#PolCd', 
		url: rootPath + LookUpConfig.CityPortUrl, 
		config: LookUpConfig.CityPortLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#PolCd").val(map.CntryCd + map.PortCd);
			$("#PolName").val(map.PortNm);
		}
	}, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, 
		function ($grid, rd, elem) {
			$("#PolCd").val(rd.CNTRY_CD + rd.PORT_CD);
			$("#PolName").val(rd.PORT_NM);
		}
	));

	registBtnLookup($("#PodCdLookup"), {
		item: '#PodCd', 
		url: rootPath + LookUpConfig.CityPortUrl, 
		config: LookUpConfig.CityPortLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#PodCd").val(map.CntryCd + map.PortCd)
			$("#PodName").val(map.PortNm);
		}
	}, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, 
		function ($grid, rd, elem) {
			$("#PodCd").val(rd.CNTRY_CD + rd.PORT_CD);
			$("#PodName").val(rd.PORT_NM);
		}
	));

	registBtnLookup($("#DestCdLookup"), {
		item: '#DestCd', 
		url: rootPath + LookUpConfig.CityPortUrl, 
		config: LookUpConfig.CityPortLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#DestCd").val(map.CntryCd + map.PortCd);
			$("#DestName").val(map.PortNm);
		}
	}, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, 
		function ($grid, rd, elem) {
			$("#DestCd").val(rd.CNTRY_CD + rd.PORT_CD);
			$("#DestName").val(rd.PORT_NM);
		}
	));

	registBtnLookup($("#OexporterLookup"), {
		item: '#Oexporter', 
		url: rootPath + LookUpConfig.PartyNoUrl, 
		config: LookUpConfig.PartyNoLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#Oexporter").val(map.PartyNo);
			$("#OexporterNm").val(map.PartyName);
			$("#OexporterAddr").val(map.PartAddr1);
		}
	}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, 
		function ($grid, rd, elem) {
			$("#Oexporter").val(rd.PARTY_NO);
			$("#OexporterNm").val(rd.PARTY_NAME);
			$("#OexporterAddr").val(rd.PART_ADDR1);
		}
	));

	registBtnLookup($("#OimporterLookup"), {
		item: '#Oimporter', 
		url: rootPath + LookUpConfig.PartyNoUrl, 
		config: LookUpConfig.PartyNoLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#Oimporter").val(map.PartyNo);
			$("#OimporterNm").val(map.PartyName);
			$("#OimporterAddr").val(map.PartAddr1);
		}
	}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, 
		function ($grid, rd, elem) {
			$("#Oimporter").val(rd.PARTY_NO);
			$("#OimporterNm").val(rd.PARTY_NAME);
			$("#OimporterAddr").val(rd.PART_ADDR1);
		}
	));

	registBtnLookup($("#TcImporterLookup"), {
		item: '#TcImporter', 
		url: rootPath + LookUpConfig.PartyNoUrl, 
		config: LookUpConfig.PartyNoLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#TcImporter").val(map.PartyNo);
			$("#TcImporterNm").val(map.PartyName);
			$("#TcImporterAddr").val(map.PartAddr1);
		}
	}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, 
		function ($grid, rd, elem) {
			$("#TcImporter").val(rd.PARTY_NO);
			$("#TcImporterNm").val(rd.PARTY_NAME);
			$("#TcImporterAddr").val(rd.PART_ADDR1);
		}
	));

	registBtnLookup($("#Trucker1Lookup"), {
		item: '#Trucker1',
		url: rootPath + LookUpConfig.PartyNo1Url,
		config: LookUpConfig.PartyNoLookup,
		param: "",
		selectRowFn: function (map) {
			$("#Trucker1").val(map.PartyNo);
			$("#TruckerNm1").val(map.PartyName);
		}
	}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined,
		function ($grid, rd, elem) {
			$("#Trucker1").val(rd.PARTY_NO);
			$("#TruckerNm1").val(rd.PARTY_NAME);
		}
	));

	registBtnLookup($("#Trucker2Lookup"), {
		item: '#Trucker2',
		url: rootPath + LookUpConfig.PartyNo1Url,
		config: LookUpConfig.PartyNoLookup,
		param: "",
		selectRowFn: function (map) {
			$("#Trucker2").val(map.PartyNo);
			$("#TruckerNm2").val(map.PartyName);
		}
	}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined,
		function ($grid, rd, elem) {
			$("#Trucker2").val(rd.PARTY_NO);
			$("#TruckerNm2").val(rd.PARTY_NAME);
		}
	));

	registBtnLookup($("#Trucker3Lookup"), {
		item: '#Trucker3',
		url: rootPath + LookUpConfig.PartyNo1Url,
		config: LookUpConfig.PartyNoLookup,
		param: "",
		selectRowFn: function (map) {
			$("#Trucker3").val(map.PartyNo);
			$("#TruckerNm3").val(map.PartyName);
		}
	}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined,
		function ($grid, rd, elem) {
			$("#Trucker3").val(rd.PARTY_NO);
			$("#TruckerNm3").val(rd.PARTY_NAME);
		}
	));

	registBtnLookup($("#Pol1Lookup"), {
	    item: '#Pol1',
	    url: rootPath + LookUpConfig.TruckPortCdUrl,
	    config: LookUpConfig.TruckPortCdLookup,
	    param: "",
	    selectRowFn: function (map) {
	        $("#Pol1").val(map.PortCd);
	        $("#PolNm1").val(map.PortNm);
	    }
	}, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined,
		function ($grid, rd, elem) {
		    $("#Pol1").val(rd.PORT_CD);
		    $("#PolNm1").val(rd.PORT_NM);
		}
	));

	registBtnLookup($("#Pol2Lookup"), {
	    item: '#Pol2',
	    url: rootPath + LookUpConfig.TruckPortCdUrl,
	    config: LookUpConfig.TruckPortCdLookup,
	    param: "",
	    selectRowFn: function (map) {
	        $("#Pol2").val(map.PortCd);
	        $("#PolNm2").val(map.PortNm);
	    }
	}, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined,
		function ($grid, rd, elem) {
		    $("#Pol2").val(rd.PORT_CD);
		    $("#PolNm2").val(rd.PORT_NM);
		}, function ($grid, elem) {
		    $("#Pol2").val("");
		    $("#PolNm2").val("");
		}
	));

	registBtnLookup($("#Pol3Lookup"), {
	    item: '#Pol3',
	    url: rootPath + LookUpConfig.TruckPortCdUrl,
	    config: LookUpConfig.TruckPortCdLookup,
	    param: "",
	    selectRowFn: function (map) {
	        $("#Pol3").val(map.PortCd);
	        $("#PolNm3").val(map.PortNm);
	    }
	}, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined,
		function ($grid, rd, elem) {
		    $("#Pol3").val(rd.PORT_CD);
		    $("#PolNm3").val(rd.PORT_NM);
		}, function ($grid, elem) {
		    $("#Pol3").val("");
		    $("#PolNm3").val("");
		}
	));

    //Address放大鏡
	/*var options = {};
	options.gridUrl = rootPath + "TPVCommon/GetAddrData";
	options.registerBtn = $("#DepAddr1Lookup");
	options.focusItem = $("#DepAddrCd1");
	options.isMutiSel = true;
	options.param = "";
	options.gridFunc = function (map) {
	    $("#DepAddrCd1").val(map.AddrCode);
	    $("#DepAddr1").val(map.Addr);
	}

	options.lookUpConfig = LookUpConfig.BSADDRLookup;
	initLookUp(options);
	CommonFunc.AutoComplete("#DepAddrCd1", 1, "", "dt=bsaddr&GROUP_ID=" + groupId + "&ADDR_CODE=", "ADDR_CODE=showValue,ADDR_CODE,ADDR", function (event, ui) {
	    $(this).val(ui.item.returnValue.ADDR_CODE);
	    $("#DepAddr1").val(ui.item.returnValue.ADDR);
	    return false;
	});*/

	registBtnLookup($("#DepAddr1Lookup"), {
	    item: '#DepAddrCd1',
	    url: rootPath + LookUpConfig.TruckPortAddrUrl,
	    config: LookUpConfig.BSADDRLookup,
	    param: "",
	    selectRowFn: function (map) {
	        $("#DepAddrCd1").val(map.AddrCode);
	        $("#DepAddr1").val(map.Addr);
	    }  
	}, {
	    baseConditionFunc: function () {
	        var Pol1 = $("#Pol1").val();
	        return " PORT_CD = '" + Pol1 + "'";
	    }
	}, undefined, LookUpConfig.TruckPortAddrAuto(groupId, undefined,
		function ($grid, rd, elem) {
		    $("#DepAddrCd1").val(rd.AddrCode);
		    $("#DepAddr1").val(rd.Addr);
		}
	));

	registBtnLookup($("#DepAddr2Lookup"), {
	    item: '#DepAddrCd2',
	    url: rootPath + LookUpConfig.TruckPortAddrUrl,
	    config: LookUpConfig.BSADDRLookup,
	    param: "",
	    selectRowFn: function (map) {
	        $("#DepAddrCd2").val(map.AddrCode);
	        $("#DepAddr2").val(map.Addr);
	    }
	}, {
	    baseConditionFunc: function () {
	        var Pol2 = $("#Pol2").val();
	        return " PORT_CD = '" + Pol2 + "'";
	    }
	}, undefined, LookUpConfig.TruckPortAddrAuto(groupId, undefined,
		function ($grid, rd, elem) {
		    $("#DepAddrCd2").val(rd.AddrCode);
		    $("#DepAddr2").val(rd.Addr);
		}
	));

	registBtnLookup($("#DepAddr3Lookup"), {
	    item: '#DepAddrCd3',
	    url: rootPath + LookUpConfig.TruckPortAddrUrl,
	    config: LookUpConfig.BSADDRLookup,
	    param: "",
	    selectRowFn: function (map) {
	        $("#DepAddrCd3").val(map.AddrCode);
	        $("#DepAddr3").val(map.Addr);
	    }
	}, {
	    baseConditionFunc: function () {
	        var Pol3 = $("#Pol3").val();
	        return " PORT_CD = '" + Pol3 + "'";
	    }
	}, undefined, LookUpConfig.TruckPortAddrAuto(groupId, undefined,
		function ($grid, rd, elem) {
		    $("#DepAddrCd3").val(rd.AddrCode);
		    $("DepAddr3").val(rd.Addr);
		}
	));
	//產品組代碼
	registBtnLookup($("#ProductTypeLookup"), {
		item: '#ProductType',
		url: rootPath + LookUpConfig.ProductTypeUrl,
		config: LookUpConfig.ProductTypeLookup,
		param: "AP_CD",
		selectRowFn: function (map) {
			$("#ProductType").val(map.ApCd);
		}
	}, undefined, LookUpConfig.GetMaterialAuto(groupId, undefined,
		function ($grid, rd, elem) {
			$("#ProductType").val(rd.AP_CD);
		}
	));

	registBtnLookup($("#ContainerYardCdLookup"), {
		item: '#ContainerYardCd', 
		url: rootPath + LookUpConfig.ContainerYardUrl, 
		config: LookUpConfig.BSCodeLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#ContainerYardCd").val(map.Cd);
			$("#ContainerYardNm").val(map.CdDescp);
		}
	}, undefined, LookUpConfig.GetBsCodeAuto(groupId, undefined, 
		function ($grid, rd, elem) {
			$("#ContainerYardCd").val(rd.CD);
			$("#ContainerYardNm").val(rd.CD_DESCP);
		}
	));


	setBscData("TerminalCdLookup", "TerminalCd", "TerminalNm", "TMN", " AND AP_CD='"+$("#PodCd").val()+"'")


	function setBscData(lookUp, Cd, Nm, pType, con)
	{
		//SMPTY放大鏡
		options = {};
		options.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
		options.registerBtn = $("#"+lookUp);
		options.focusItem = $("#" + Cd);
		options.baseCondition = " GROUP_ID='"+groupId+"' AND CD_TYPE='"+pType+"'";
		options.baseConditionFunc = function(){
			return " AND AP_CD='" + $("#PodCd").val() + "'";
		}
		options.isMutiSel = true;
		options.gridFunc = function (map) {
			$("#" + Cd).val(map.Cd);

			if(Nm != "")
				$("#" + Nm).val(map.CdDescp);
		}

		options.lookUpConfig = LookUpConfig.BSCodeLookup;
		initLookUp(options);

		CommonFunc.oAutoComplete("#"+Cd, 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE~"+pType+"&CD=", "CD=showValue,CD,CD_DESCP", function (event, ui) {
			$(this).val(ui.item.returnValue.CD);

			if(Nm != "")
				$("#" + Nm).val(ui.item.returnValue.CD_DESCP);

			return false;
		});
	}

	$("#AddAddrBtn").on("click", function(){
		var TranType = $("#TranType").val();
		var $MyGrid = null;
		if(TranType == 'F' || TranType == 'R' )
		{
			$MyGrid = $("#CntrGrid");
			var ids = $MyGrid.jqGrid( 'getDataIDs' );
			for (var i = 0; i < ids.length; i++)
			 {
				var map =$MyGrid.jqGrid( 'getRowData', ids[i] );

				setGridVal($MyGrid, ids[i], 'DlvArea', $("#"+map.CntrNo).find("input[name='DlvArea']").val(), null);
				setGridVal($MyGrid, ids[i], 'DlvAreaNm', $("#"+map.CntrNo).find("input[name='DlvAreaNm']").val(), null);
				setGridVal($MyGrid, ids[i], 'AddrCode', $("#"+map.CntrNo).find("input[name='AddrCode']").val(), null);
				setGridVal($MyGrid, ids[i], 'DlvAddr', $("#" + map.CntrNo).find("input[name='DlvAddr']").val(), null);

				setGridVal($MyGrid, ids[i], 'WsCd', $("#" + map.CntrNo).find("input[name='WsCd']").val(), null);
				setGridVal($MyGrid, ids[i], 'WsNm', $("#" + map.CntrNo).find("input[name='WsNm']").val(), null);
				setGridVal($MyGrid, ids[i], 'FinalWh', $("#" + map.CntrNo).find("select[name='FinalWh']").val(), null);
			}
		} 
		else
		{
			$MyGrid = $("#SubGrid2");
			var ids = $MyGrid.jqGrid( 'getDataIDs' );
			for (var i = 0; i < ids.length; i++) {
			    setGridVal($MyGrid, ids[i], 'DlvArea', $("#tr" + ids[i]).find("input[name='DlvArea']").val(), null);
			    setGridVal($MyGrid, ids[i], 'DlvAreaNm', $("#tr" + ids[i]).find("input[name='DlvAreaNm']").val(), null);
			    setGridVal($MyGrid, ids[i], 'AddrCode', $("#tr" + ids[i]).find("input[name='AddrCode']").val(), null);
			    setGridVal($MyGrid, ids[i], 'DlvAddr', $("#tr" + ids[i]).find("input[name='DlvAddr']").val(), null);

			    setGridVal($MyGrid, ids[i], 'WsCd', $("#tr" + ids[i]).find("input[name='WsCd']").val(), null);
			    setGridVal($MyGrid, ids[i], 'WsNm', $("#tr" + ids[i]).find("input[name='WsNm']").val(), null);
			    setGridVal($MyGrid, ids[i], 'FinalWh', $("#tr" + ids[i]).find("select[name='FinalWh']").val(), null);
			}
		}
		

		$("#AddrModal").modal("hide");
		if (_handler.beforSave() === false) {
		} else
		    _handler.saveData();
	});

	$("#SetSameAddr").on("click", function(){
		var  DlvArea = $("input[name='DlvArea']:eq(0)").val();
		var  DlvAreaNm = $("input[name='DlvAreaNm']:eq(0)").val();
		var  AddrCode = $("input[name='AddrCode']:eq(0)").val();
		var  DlvAddr = $("input[name='DlvAddr']:eq(0)").val();
		var FinalWh = $("select[name='FinalWh']:eq(0)").val();
		var WsCd = $("input[name='WsCd']:eq(0)").val();
		var WsNm = $("input[name='WsNm']:eq(0)").val();

		$("input[name='DlvArea']").val(DlvArea);
		$("input[name='DlvAreaNm']").val(DlvAreaNm);
		$("input[name='AddrCode']").val(AddrCode);
		$("input[name='DlvAddr']").val(DlvAddr);
	    $("select[name='FinalWh']").val(FinalWh);
 
		$("input[name='WsCd']").val(WsCd);
		$("input[name='WsNm']").val(WsNm);
		
	});
});
/*注册放大镜&auto complete end*/

function SetAddrModal()
{
    function _clearLookup() {
        $("button[name='DlvAreaLookup']").each(function () {
            var el = $(this);
            var target = el.attr("data-target");
            if (!target)
                return;
            var myId = target.replace("lookupDialog_", "");
            $("#lookupStatus_" + myId).remove();
            if ($("#" + target).length <= 0)
                return;
            $("#" + target).remove();
        });

        $("button[name='AddrCodeLookup']").each(function () {
            var el = $(this);
            var target = el.attr("data-target");
            if (!target)
                return;
            var myId = target.replace("lookupDialog_", "");
            $("#lookupStatus_" + myId).remove();
            if ($("#" + target).length <= 0)
                return;
            $("#" + target).remove();
        });

        $("button[name='WsCdLookup']").each(function () {
            var el = $(this);
            var target = el.attr("data-target");
            if (!target)
                return;
            var myId = target.replace("lookupDialog_", "");
            $("#lookupStatus_" + myId).remove();
            if ($("#" + target).length <= 0)
                return;
            $("#" + target).remove();
        });
    }

    function _addUI(str, ids, i, map) {
        str += '<td>\
				<div class="input-group">\
                     <select id="FinalWh' + ids[i] + '"  name="FinalWh" finalwh-value="' + (map.FinalWh || '') + '" class="form-control input-sm" readonly="readonly" disabled="disabled" >\
                                        <option value=""></option>\
                                        <option value="Final">Final</option>\
                                        <option value="Temp">Temp</option>\
                                    </select>\
							</div></td>';

        str += '<td><div class="pure-g">\
                <div class="pure-u-sm-2-5">\
                     <div class="input-group">\
								<input type="text" class="form-control input-sm" id="WsCd' + ids[i] + '"  name="WsCd" value="' + (map.WsCd || '') + '" readonly="readonly" />\
                     </div>\
                </div>\
				<div class="pure-u-sm-3-5">\
								<input type="text" class="form-control input-sm" id="WsNm' + ids[i] + '"  name="WsNm" value="' + (map.WsNm || '') + '" readonly="readonly"/>\
                </div>\
							</div></td>';
        return str;
    }

    function _addLookUp() {
        $("button[name='WsCdLookup']").each(function () {
            var el = $(this);
            $(this).v3Lookup({
                url: rootPath + "TPVCommon/GetSmwhForLookup",
                focusItem: $(this),
                baseConditionFunc: function () {
                    var location = _handler.topData["Cmp"];
                    return " CMP='" + location + "'";
                },
                gridFunc: function (map) {
                    console.log($(this));
                    el.parent("span").parent().find("input[name='WsCd']").val(map.WsCd);
                    el.parent("span").parent().parent().parent().parent().parent().find("input[name='WsNm']").val(map.WsNm);
                },
                lookUpConfig: LookUpConfig.SMWHLookup
            });
        });

        $("select[name='FinalWh']").each(function () {
            var el = $(this);
            var val = el.attr("finalwh-value");
            if (val) el.val(val);
        });
    }

    var TranType = $("#TranType").val();
	var $MyGrid = null;
	if(TranType == 'R' || TranType == 'F')
	{
		$MyGrid = $("#CntrGrid");
	}
	else
	{
		$MyGrid = $("#SubGrid2");
	}

	var str = "";
	if (TranType == 'R' || TranType == 'F') {
		var ids = $MyGrid.jqGrid('getDataIDs');
		var str = "";

		for (var i = 0; i < ids.length; i++) {
			var map = $MyGrid.jqGrid('getRowData', ids[i]);
			str += '<thead><tr><th style="width: 15%;">Container No.</th><th style="width: 15%;">Delivery Area</th><th style="width: 20%;">Delivery Address</th><th style="width: 10%;">Final WH</th><th style="width: 40%;" >Warehouse</th></tr></thead>';
			str += '<tbody>';
			str += "<tr id='" + map.CntrNo + "'>";
			str += "<td>" + map.CntrNo + "</td>";
			str += '<td>\
				<div class="input-group">\
								<input type="hidden" class="form-control input-sm"  name="DlvArea" value="'+ map.DlvArea + '" />\
								<input type="text" class="form-control input-sm"  name="DlvAreaNm" value="'+ map.DlvAreaNm + '"  readonly="readonly"/>\
								<span class="input-group-btn">\
									<button class="btn btn-sm" type="button" id="DlvAreaLookup'+ i + '" name="DlvAreaLookup">\
									<span class="glyphicon glyphicon-search"></span>\
									</button>\
								</span>\
							</div></td>';
			str += '<td>\
				<div class="input-group">\
								<input type="hidden" class="form-control input-sm"  name="AddrCode" value="'+ map.AddrCode + '" />\
								<input type="text" class="form-control input-sm"  name="DlvAddr" value="'+ map.DlvAddr + '" readonly="readonly"/>\
								<span class="input-group-btn">\
									<button class="btn btn-sm" type="button" id="AddrCodeLookup'+ i + '" name="AddrCodeLookup" ref="DlvAreaLookup' + i + '">\
									<span class="glyphicon glyphicon-search"></span>\
									</button>\
								</span>\
							</div></td>';

			str = _addUI(str, ids, i, map);
			str += "</tr>";
			str += '</tbody>';
		}

		_clearLookup();
		$("#AddrModal").find(".table").html(str);

		$("button[name='DlvAreaLookup']").each(function () {
			var el = $(this);
			$(this).v3Lookup({
				url: rootPath + LookUpConfig.TruckPortCdUrl,
				focusItem: $(this),
				baseConditionFunc: function () {
					var location = _handler.topData["Cmp"];
					return " CMP='" + location + "'";
				},
				gridFunc: function (map) {
					console.log($(this));
					el.parent("span").siblings("input[name='DlvAreaNm']").val(map.PortNm);
					el.parent("span").siblings("input[name='DlvArea']").val(map.PortCd);
				},
				lookUpConfig: LookUpConfig.TruckPortCdLookup
			});
		});

		$("button[name='AddrCodeLookup']").each(function () {
			var el1 = $(this);
			var f_el1 = $(this).parent("span").siblings("input[name='DlvAddr']");
			var ref = $(this).attr("ref");
			console.log(ref);
			var f_el2 = $("#" + ref).parent("span").siblings("input[name='DlvArea']");
			$(this).v3Lookup({
				url: rootPath + 'TPVCommon/GetBsaddrForLookup',
				focusItem: $(this),
				baseConditionFunc: function () {
					var DlvArea = f_el2.val();
					console.log(DlvArea);
					var location = _handler.topData["Cmp"];
					return " PORT_CD='" + DlvArea + "' AND CMP='" + location + "'";
				},
				gridFunc: function (map) {
					el1.parent("span").siblings("input[name='DlvAddr']").val(map.Addr);
					el1.parent("span").siblings("input[name='AddrCode']").val(map.AddrCode);
					el1.parent("span").parent().parent().parent().find("select[name='FinalWh']").val(map.FinalWh);
					el1.parent("span").parent().parent().parent().find("input[name='WsCd']").val(map.WsCd);
					el1.parent("span").parent().parent().parent().find("input[name='WsNm']").val(map.WsNm);
				},
				lookUpConfig: LookUpConfig.TruckPortAddrLookup
			});
		});

		_addLookUp();
	}
	else if (TRAN_TYPE=='T') {
		var ids = $MyGrid.jqGrid('getDataIDs');
		for (var i = 0; i < ids.length; i++) {
			var map = $MyGrid.jqGrid('getRowData', ids[i]);
			var DlvAreaNm = map.DlvAreaNm;
			var DlvAddr = map.DlvAddr;
			if (isEmpty(DlvAreaNm) || DlvAreaNm === "null")
				DlvAreaNm = "";
			if (isEmpty(DlvAddr) || DlvAddr === "null")
				DlvAddr = "";
			str += '<thead><tr><th style="width: 15%;">DN NO</th><th style="width: 15%;">Inv NO</th><th style="width: 15%;">Delivery Area</th><th style="width: 15%;">Delivery Address</th><th style="width: 10%;">Final WH</th><th style="width: 30%;">Warehouse</th></tr></thead>';
			str += '<tbody>';
			str += "<tr id='tr" + ids[i] + "'>";
			str += "<td>" + map.DnNo + "</td>";
			str += "<td>" + map.InvNo + "</td>";
			str += '<td>\
				<div class="input-group">\
								<input type="hidden" class="form-control input-sm" id="DlvArea' + ids[i] + '"  name="DlvArea" value="' + (map.DlvArea || '') + '" />\
								<input type="text" class="form-control input-sm" id="DlvAreaNm' + ids[i] + '"  name="DlvAreaNm" value="' + (DlvAreaNm || '') + '"  readonly="readonly"/>\
								<span class="input-group-btn">\
									<button class="btn btn-sm" type="button" id="DlvAreaLookup'+ ids[i] + '" name="DlvAreaLookup">\
									<span class="glyphicon glyphicon-search"></span>\
									</button>\
								</span>\
							</div></td>';
			str += '<td>\
				<div class="input-group">\
								<input type="hidden" class="form-control input-sm" id="AddrCode' + ids[i] + '"  name="AddrCode" value="' + (map.AddrCode || '') + '" />\
								<input type="text" class="form-control input-sm" id="DlvAddr'+ ids[i] + '"  name="DlvAddr" value="' + (DlvAddr || '') + '" readonly="readonly"/>\
								<span class="input-group-btn">\
									<button class="btn btn-sm" type="button" id="AddrCodeLookup'+ ids[i] + '" name="AddrCodeLookup" ref="DlvAreaLookup' + ids[i] + '">\
									<span class="glyphicon glyphicon-search"></span>\
									</button>\
								</span>\
							</div></td>';

			str = _addUI(str, ids, i, map);
			str += "</tr>";
			str += '</tbody>';
		}
		_clearLookup();
		$("#AddrModal").find(".table").html(str);

		$("button[name='DlvAreaLookup']").each(function () {
			var el = $(this);
			$(this).v3Lookup({
				url: rootPath + 'TPVCommon/GetBsDestDataForLookup',
				focusItem: $(this),
				baseConditionFunc: function () {
					var cmp = _handler.topData["Cmp"];
					var stn = _handler.topData["Stn"];
					console.log(stn);
					return " FACTORY='" + stn + "' AND SHIP_TO='" + ShipToParty + "' AND CMP='" + cmp + "'";
					console.log(" FACTORY='" + stn + "' AND SHIP_TO='" + ShipToParty + "' AND CMP='" + cmp + "'");
				},
				gridFunc: function (map) {
					console.log($(this));
					el.parent("span").siblings("input[name='DlvAreaNm']").val(map.PortNm);
					el.parent("span").siblings("input[name='DlvArea']").val(map.PortCd);
				},
				lookUpConfig: LookUpConfig.TruckPortCdLookup
			});
		});

		$("button[name='AddrCodeLookup']").each(function () {
			var el1 = $(this);
			$(this).v3Lookup({
				url: rootPath + 'TPVCommon/GetDestAdddrDataForLookup',
				focusItem: $(this),
				baseConditionFunc: function () {
					var cmp = _handler.topData["Cmp"];
					var stn = _handler.topData["Stn"];
					return " FACTORY='" + stn + "' AND SHIP_TO='" + ShipToParty + "' AND CMP='" + cmp + "'";
				},
				gridFunc: function (map) {
					el1.parent("span").siblings("input[name='DlvAddr']").val(map.Addr);
					el1.parent("span").siblings("input[name='AddrCode']").val(map.AddrCode);
					el1.parent("span").parent().parent().parent().find("select[name='FinalWh']").val(map.FinalWh);
					el1.parent("span").parent().parent().parent().find("input[name='WsCd']").val(map.WsCd);
					el1.parent("span").parent().parent().parent().find("input[name='WsNm']").val(map.WsNm);
				},
				lookUpConfig: LookUpConfig.TruckPortAddrLookup
			});
		});
		_addLookUp();
    }
	else
	{
		var ids = $MyGrid.jqGrid( 'getDataIDs' );
		for (var i = 0; i < ids.length; i++) {
		    var map = $MyGrid.jqGrid('getRowData', ids[i]);
		    var DlvAreaNm = map.DlvAreaNm;
		    var DlvAddr = map.DlvAddr;
		    if (isEmpty(DlvAreaNm) || DlvAreaNm==="null")
		        DlvAreaNm = "";
		    if (isEmpty(DlvAddr) || DlvAddr === "null")
		        DlvAddr = "";
		    str += '<thead><tr><th style="width: 15%;">DN NO</th><th style="width: 15%;">Inv NO</th><th style="width: 15%;">Delivery Area</th><th style="width: 15%;">Delivery Address</th><th style="width: 10%;">Final WH</th><th style="width: 30%;">Warehouse</th></tr></thead>';
			str += '<tbody>';
			str += "<tr id='tr"+ids[i]+"'>";
			str += "<td>"+map.DnNo+"</td>";
			str += "<td>"+map.InvNo+"</td>";
			str += '<td>\
				<div class="input-group">\
								<input type="hidden" class="form-control input-sm" id="DlvArea' + ids[i] + '"  name="DlvArea" value="' + (map.DlvArea || '') + '" />\
								<input type="text" class="form-control input-sm" id="DlvAreaNm' + ids[i] + '"  name="DlvAreaNm" value="' + (DlvAreaNm || '') + '"  readonly="readonly"/>\
								<span class="input-group-btn">\
									<button class="btn btn-sm" type="button" id="DlvAreaLookup'+ids[i]+'" name="DlvAreaLookup">\
									<span class="glyphicon glyphicon-search"></span>\
									</button>\
								</span>\
							</div></td>';
			str += '<td>\
				<div class="input-group">\
								<input type="hidden" class="form-control input-sm" id="AddrCode' + ids[i] + '"  name="AddrCode" value="' + (map.AddrCode || '') + '" />\
								<input type="text" class="form-control input-sm" id="DlvAddr'+ids[i]+'"  name="DlvAddr" value="' +(DlvAddr || '') + '" readonly="readonly"/>\
								<span class="input-group-btn">\
									<button class="btn btn-sm" type="button" id="AddrCodeLookup'+ids[i]+'" name="AddrCodeLookup" ref="DlvAreaLookup'+ids[i]+'">\
									<span class="glyphicon glyphicon-search"></span>\
									</button>\
								</span>\
							</div></td>';

			str = _addUI(str, ids, i, map);
			str += "</tr>";
			str += '</tbody>';
		}
		_clearLookup();
		$("#AddrModal").find(".table").html(str);

		$("button[name='DlvAreaLookup']").each	(function(){
			var el = $(this);
			$(this).v3Lookup({
				url: rootPath + 'TPVCommon/GetBstportDataForLookup',
				focusItem: $(this),
				baseConditionFunc: function () {
				    var location = _handler.topData["Cmp"];
				    return " CMP='" + location + "'";
				},
				gridFunc: function(map){
					console.log($(this));
					el.parent("span").siblings("input[name='DlvAreaNm']").val(map.PortNm);
					el.parent("span").siblings("input[name='DlvArea']").val(map.PortCd);
				},
				lookUpConfig: LookUpConfig.TruckPortCdLookup
			});
		});

		$("button[name='AddrCodeLookup']").each(function(){
			var el1 = $(this);
			var f_el1 = $(this).parent("span").siblings("input[name='DlvAddr']");
			var ref = $(this).attr("ref");
			console.log(ref);
			var f_el2 = $("#"+ref).parent("span").siblings("input[name='DlvArea']");
			$(this).v3Lookup({
				url: rootPath + 'TPVCommon/GetBsaddrForLookup',
				focusItem: $(this),
				baseConditionFunc: function(){
					var DlvArea =f_el2.val();
					console.log(DlvArea);
					var location = _handler.topData["Cmp"];
					return " PORT_CD='" + DlvArea + "' AND CMP='" + location + "'";
				},
				gridFunc: function(map){
					el1.parent("span").siblings("input[name='DlvAddr']").val(map.Addr);
					el1.parent("span").siblings("input[name='AddrCode']").val(map.AddrCode);
					el1.parent("span").parent().parent().parent().find("select[name='FinalWh']").val(map.FinalWh);
					el1.parent("span").parent().parent().parent().find("input[name='WsCd']").val(map.WsCd);
					el1.parent("span").parent().parent().parent().find("input[name='WsNm']").val(map.WsNm);
				},
				lookUpConfig: LookUpConfig.TruckPortAddrLookup
			});
		});
		_addLookUp();
	}
}

function SetBookingEdit() {
    var status = $("#Status").val();
    if ("O" == status) {
        setdisabled(true);
        gridEditableCtrl({ editable: false, gridId: "SubGrid" });
        try{
            gridEditableCtrl({ editable: false, gridId: "CntrGrid" });
        }catch(e){}
        try{
            gridEditableCtrl({ editable: false, gridId: "SubGrid4" });
        }
        catch(e){}
        return;
	}
 
	//$("#DelayReason").attr('disabled',true);
	//$("#DelaySolution").attr('disabled', true);
	setdisabled(false);
	if ("E" != status && "A" != status && "S" != status) {
		gridEditableCtrl({ editable: false, gridId: "SubGrid" });
		try {
			gridEditableCtrl({ editable: false, gridId: "CntrGrid" });
		} catch (e) {
		}
		var readList = ["SetTranPlan", "TranType1", "TranType2", "TranType3", "Pol1", "Pol2", "Pol3", "PolNm1", "PolNm2", "PolNm3",
			"DepAddrCd1", "DepAddrCd2", "DepAddrCd3", "DepAddr1", "DepAddr2", "DepAddr3", "Trucker1", "Trucker2", "Trucker3",
			"TruckerNm1", "TruckerNm2", "TruckerNm3"];
		SetArrayDisable(readList, true);
	}
    var readonlys = ["Lgoods", "FreightCur", "FreightAmt", "InsuranceAmt", "IncotermCd", "IncotermDescp",
              "SvcContact", "Carrier", "CarrierNm", "IbWindow", "Eta", "Etd","EtdL","EtaL",
		"Atp", "Atd", "Ata", "LoadingFrom", "LoadingTo", "Atd1", "Atd2", "Atd3", "Atd4", "Atd5", "Atd6", "Atd7", "Ata1", "Ata2", "Ata3", "Ata4", "Ata5", "Ata6", "Ata7", "DelayReason", "DelaySolution"];
    SetArrayDisable(readonlys,true);
}

function CheckStatus() {
    var _status = $("#Status").val();
    return true;
    if (_status == "O" || _status == "B" || isEmpty(_status)) {
    } else {
        alert("@Resources.Locale.L_BaseBookingSetup_Script_115");
        return false;
    }
}

function SetArrayDisable(arr, isdisable) {
    if (isdisable === undefined || isdisable === "" || isdisable == null) {
        isdisable = true;
    }
    if (arr.constructor != Array) return;
    for (var i = 0; i < arr.length; i++) {
        $("#" + arr[i]).attr('disabled', isdisable);
        $("#" + arr[i]).parent().find("button").attr("disabled", isdisable);
    }
}

function IsRepeat(containerArray, PTlist)
{
    if (containerArray.length == 0)
        return true;
    if (isEmpty(PTlist))
        return true;
    var pt = PTlist.pt;
    var Ptlist = PTlist.pt.split("|");
    var newArr = [];
    for (var i = 0; i < containerArray.length ; i++)
        if (containerArray[i]["__state"] == 0)
        {
            PTlist.pt = PTlist.pt.split(containerArray[i]["PartyType"]).join("");
            pt = PTlist.pt;
            Ptlist = PTlist.pt.split("|");
        }
    for (var i = 0; i < containerArray.length ; i++)
    {

        if (containerArray[i]["__state"] != 1)
            continue;
        if (newArr.indexOf(containerArray[i]["PartyType"]) == -1) {
            newArr.push(containerArray[i]["PartyType"])
        } else
            return false;
        for (var j = 0; j < Ptlist.length; j++) {
            if (isEmpty(Ptlist[j]))
                continue;
            if (containerArray[i]["PartyType"] == Ptlist[j] && "IBCR" != Ptlist[j]) {
                PTlist.pt = pt;
                return false;
            }
        }
        PTlist.pt += "|" + containerArray[i]["PartyType"];
    }
    return true;
}

function isEmpty(val) {
    if (val === undefined || val === "" || val == null)
        return true;
    return false;
}

$(function () {
	$("#SetTranPlan").click(function () {
		setTimeout(function () {
			var TranPlanData = {};
			for (var i = 1; i < 3; i++) {
				TranPlanData["Pol" + i] = $("#Pol" + i).val();
				TranPlanData["PolNm" + i] = $("#PolNm" + i).val();
				TranPlanData["TranType" + i] = $("#TranType" + i).val();
				TranPlanData["DepAddr" + i] = $("#DepAddr" + i).val();
				TranPlanData["Trucker" + i] = $("#Trucker" + i).val();
				TranPlanData["TruckerNm" + i] = $("#TruckerNm" + i).val();
				TranPlanData["TEtd" + i] = $("#TEtd" + i).val();
				TranPlanData["TAtd" + i] = $("#TAtd" + i).val();
				TranPlanData["TEta" + i] = $("#TEta" + i).val();
				TranPlanData["TAta" + i] = $("#TAta" + i).val();
			}
			var ids = $CntrGrid.jqGrid('getDataIDs');
			for (var i = 0; i < ids.length; i++) {

				setGridVal($CntrGrid, ids[i], 'Pol1', TranPlanData["Pol1"], null);
				setGridVal($CntrGrid, ids[i], 'PolNm1', TranPlanData["PolNm1"], null);
				setGridVal($CntrGrid, ids[i], 'TranType1', TranPlanData["TranType1"], null);
				setGridVal($CntrGrid, ids[i], 'DepAddr1', TranPlanData["DepAddr1"], null);
				setGridVal($CntrGrid, ids[i], 'Trucker1', TranPlanData["Trucker1"], null);
				setGridVal($CntrGrid, ids[i], 'TruckerNm1', TranPlanData["TruckerNm1"], null);
				setGridVal($CntrGrid, ids[i], 'TEtd1', TranPlanData["TEtd1"], null);
				setGridVal($CntrGrid, ids[i], 'TAtd1', TranPlanData["TAtd1"], null);
				setGridVal($CntrGrid, ids[i], 'TEta1', TranPlanData["TEta1"], null);
				setGridVal($CntrGrid, ids[i], 'TAta1', TranPlanData["TAta1"], null);

				setGridVal($CntrGrid, ids[i], 'Pol2', TranPlanData["Pol2"], null);
				setGridVal($CntrGrid, ids[i], 'PolNm2', TranPlanData["PolNm2"], null);
				setGridVal($CntrGrid, ids[i], 'TranType2', TranPlanData["TranType2"], null);
				setGridVal($CntrGrid, ids[i], 'DepAddr2', TranPlanData["DepAddr2"], null);
				setGridVal($CntrGrid, ids[i], 'Trucker2', TranPlanData["Trucker2"], null);
				setGridVal($CntrGrid, ids[i], 'TruckerNm2', TranPlanData["TruckerNm2"], null);
				setGridVal($CntrGrid, ids[i], 'TEtd2', TranPlanData["TEtd2"], null);
				setGridVal($CntrGrid, ids[i], 'TAtd2', TranPlanData["TAtd2"], null);
				setGridVal($CntrGrid, ids[i], 'TEta2', TranPlanData["TEta2"], null);
				setGridVal($CntrGrid, ids[i], 'TAta2', TranPlanData["TAta2"], null);

				setGridVal($CntrGrid, ids[i], 'Pol3', TranPlanData["Pol3"], null);
				setGridVal($CntrGrid, ids[i], 'PolNm3', TranPlanData["PolNm3"], null);
				setGridVal($CntrGrid, ids[i], 'TranType3', TranPlanData["TranType3"], null);
				setGridVal($CntrGrid, ids[i], 'DepAddr3', TranPlanData["DepAddr3"], null);
				setGridVal($CntrGrid, ids[i], 'Trucker3', TranPlanData["Trucker3"], null);
				setGridVal($CntrGrid, ids[i], 'TruckerNm3', TranPlanData["TruckerNm3"], null);
				setGridVal($CntrGrid, ids[i], 'TEtd3', TranPlanData["TEtd3"], null);
				setGridVal($CntrGrid, ids[i], 'TAtd3', TranPlanData["TAtd3"], null);
				setGridVal($CntrGrid, ids[i], 'TEta3', TranPlanData["TEta3"], null);
				setGridVal($CntrGrid, ids[i], 'TAta3', TranPlanData["TAta3"], null);
			}
			alert("success");
		}, 800);
	});
});

function ChangeColor() {
	var _Estimate = ["PpolCd", "PporCd", "PpodCd", "PdestCd", "PpolName", "PporName", "PpodName", "PdestName", "Pgw", "Pcbm", "Pvw"];
	var _Actual = ["PolCd", "PorCd", "PodCd", "DestCd", "PolName", "PorName", "PodName", "DestName", "Gw", "Cbm", "Vw"];
	for (var i = 0; i < _Estimate.length; i++) {
		var _estimateval = $("#" + _Estimate[i]).val();
		var _actualval = $("#" + _Actual[i]).val();
		if (isEmpty(_estimateval) || isEmpty(_actualval)) continue;
		_estimateval = _estimateval.toUpperCase();
		_actualval = _actualval.toUpperCase();
		if (_estimateval != _actualval) {
			$("#" + _Actual[i]).css("color", "red");
		} else {
			$("#" + _Actual[i]).css("color", "black");
		}
	}
}