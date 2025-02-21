var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var mainKeyValue = {};
var groupId = getCookie("plv3.passport.groupid");
var gridSetting = {};

jQuery(document).ready(function ($) {
    var editable = false;
    var docHeight = $(document).height();
    gridHeight = docHeight - 150;
    //All column：StsCd,Edescp,Ldescp,Pict1,Pict2,Location,Issingle,RefBy,CreateBy,
    //CreateDate, ModifyBy, ModifyDate

    //Key：StsCd

    var colModel = [
          { name: 'UId', showname: 'ID', sorttype: 'string', hidden: true, viewable: false },
           { name: 'DnNo', title: 'DN NO', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ApproveType', title: '@Resources.Locale.L_ShipmentID_ApproveType', index: 'ApproveType', width: 80, align: 'left', sorttype: 'string', hidden: false, editoptions: { value: select_approve, defaultValue: default_approve } },
            { name: 'CombineInfo', title: '@Resources.Locale.L_ShipmentID_CombineInfo', index: 'CombineInfo', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ShipmentId', title: 'ShipmentId', index: 'ShipmentId', width: 130, align: 'left', sorttype: 'string', hidden: false },
           { name: 'DnType', title: '@Resources.Locale.L_ShipmentID_DNType', index: 'DnType', width: 85, align: 'left', sorttype: 'string', hidden: false },
           { name: 'SapId', title: 'SapId', index: 'SapId', width: 150, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Weekly', title: '@Resources.Locale.L_ContainUsage_Week', index: 'Weekly', width: 40, align: 'right', sorttype: 'number', hidden: false },
           { name: 'Month', title: '@Resources.Locale.L_ContainUsage_Month', index: 'Year', width: 40, align: 'right', sorttype: 'number', hidden: false },
           { name: 'Year ', title: 'Year', index: 'Year', width: 80, align: 'right', sorttype: 'number', hidden: false },
           { name: 'GroupId', title: '@Resources.Locale.L_UserSetUp_GroupId', index: 'GroupId', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Cmp', title: 'Location', index: 'Cmp', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Stn', title: '@Resources.Locale.L_GroupRelation_stnID', index: 'Stn', width: 70, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Dep', title: '@Resources.Locale.L_UserSetUp_Dep', index: 'Dep', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Bu', title: '@Resources.Locale.L_ShipmentID_Bu', index: 'Bu', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'OriginNo', title: '@Resources.Locale.L_ShipmentID_OrginNo', index: 'OriginNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
           { name: 'SapNo', title: 'SapNo', index: 'SapNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
           { name: 'RefNo', title: '@Resources.Locale.L_ShipmentID_RefNo', index: 'RefNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'TranType', title: '@Resources.Locale.L_UserSetUp_TranType', index: 'TranType', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'TranTypeDescp', title: '@Resources.Locale.L_DNApproveManage_TranDescp', index: 'TranTypeDescp', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'TranMode', title: 'TranMode', index: 'TranMode', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ApproveTo', title: '@Resources.Locale.L_ShipmentID_ApproveTo', index: 'ApproveTo', width: 80, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: select_role, defaultValue: default_role } },
           { name: 'ApproveBack', title: '@Resources.Locale.L_ShipmentID_ApproveBack', index: 'ApproveBack', width: 70, align: 'left', sorttype: 'string', hidden: false },
           { name: 'RelationInfo', title: 'RelationInfo', index: 'RelationInfo', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Cur', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Cur', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Etd', title: 'ETD', index: 'Etd', width: 150, align: 'left', sorttype: 'string', hidden: false },
           { name: 'AMOUNT1', title: '@Resources.Locale.L_ShipmentID_Amount1', index: 'AMOUNT1', width: 100, align: 'right', sorttype: 'number', hidden: false },
           { name: 'AMOUNT2', title: '@Resources.Locale.L_ShipmentID_Amount2', index: 'AMOUNT2', width: 100, align: 'right', sorttype: 'number', hidden: false },
           { name: 'AMOUNT2', title: '@Resources.Locale.L_ShipmentID_Amount3', index: 'AMOUNT2', width: 100, align: 'right', sorttype: 'number', hidden: false },
           { name: 'Incoterm', title: '@Resources.Locale.L_ShipmentID_Incoterm', index: 'Incoterm', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'IncotermDescp', title: '@Resources.Locale.L_ShipmentID_IncotermDesp', index: 'IncotermDescp', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'PayTermCd', title: '@Resources.Locale.L_ShipmentID_PayTermCd', index: 'PayTermCd', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'PayDescp', title: '@Resources.Locale.L_ShipmentID_PayDescp', index: 'PayDescp', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ChannelCd', title: '@Resources.Locale.L_ShipmentID_ChannelCd', index: 'ChannelCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Channel', title: '@Resources.Locale.L_ShipmentID_Channel', index: 'Channel', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'DivisionCd', title: '@Resources.Locale.L_ShipmentID_DivisionCd', index: 'DivisionCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'DivisionDescp', title: '@Resources.Locale.L_ShipmentID_DivisionDescp', index: 'DivisionDescp', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Sppl', title: 'Sppl', index: 'Sppl', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'SpplDescp', title: 'SpplDescp', index: 'SpplDescp', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ScCode', title: 'ScCode', index: 'ScCode', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ScDescp', title: 'ScDescp', index: 'ScDescp', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'InvNo', title: 'InvNo', index: 'InvNo', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'PackingNo', title: 'PackingNo', index: 'PackingNo', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'VatNo', title: 'VatNo', index: 'VatNo', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'PrepareRmk', title: '@Resources.Locale.L_ShipmentID_PrepareRmk', index: 'PrepareRmk', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ExportNo', title: '@Resources.Locale.L_ShipmentID_ExportNo', index: 'ExportNo', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'EdeclNo', title: '@Resources.Locale.L_ShipmentID_EdeclNo', index: 'EdeclNo', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ApproveNo', title: '@Resources.Locale.L_ShipmentID_ApproveNo', index: 'ApproveNo', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Pol', title: '@Resources.Locale.L_ShipmentID_Pol', index: 'Pol', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'PolNm', title: '@Resources.Locale.L_ShipmentID_PolNm', index: 'PolNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'PorteCd', title: '@Resources.Locale.L_ShipmentID_PorteCd', index: 'PorteCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'PorteDescp', title: '@Resources.Locale.L_ShipmentID_PorteDescp', index: 'PorteDescp', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Pod', title: '@Resources.Locale.L_ShipmentID_Pod', index: 'Pod', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'PodNm', title: '@Resources.Locale.L_ShipmentID_PodNm', index: 'PodNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'State', title: '@Resources.Locale.L_ShipmentID_State', index: 'State', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'Region', title: '@Resources.Locale.L_ShipmentID_Region', index: 'Region', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'RegionNm', title: '@Resources.Locale.L_ShipmentID_RegionNm', index: 'RegionNm', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'ShipTo', title: 'ShipTo', index: 'ShipTo', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ShipNm', title: 'ShipNm', index: 'ShipNm', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Cnee1Cd', title: '@Resources.Locale.L_ShipmentID_Cnee1Cd', index: 'Cnee1Cd', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Cnee1Nm', title: '@Resources.Locale.L_ShipmentID_Cnee1Nm', index: 'Cnee1Nm', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Cnee2Cd', title: '@Resources.Locale.L_ShipmentID_Cnee2Cd', index: 'Cnee2Cd', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Cnee2Nm', title: '@Resources.Locale.L_ShipmentID_Cnee2Nm', index: 'Cnee2Nm', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Cnee3Cd', title: '@Resources.Locale.L_ShipmentID_Cnee3Cd', index: 'Cnee3Cd', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Cnee3Nm', title: '@Resources.Locale.L_ShipmentID_Cnee3Nm', index: 'Cnee3Nm', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Notify1No', title: '@Resources.Locale.L_ShipmentID_Notify1No', index: 'Notify1No', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Notify1Nm', title: '@Resources.Locale.L_ShipmentID_Notify1Nm', index: 'Notify1Nm', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Notify2No', title: '@Resources.Locale.L_ShipmentID_Notify2No', index: 'Notify2No', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Notofy2Nm', title: '@Resources.Locale.L_ShipmentID_Notify2Nm', index: 'Notofy2Nm', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'FiCust', title: '@Resources.Locale.L_ShipmentID_FiCust', index: 'FiCust', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'FiCustNm', title: '@Resources.Locale.L_ShipmentID_FiCustNm', index: 'FiCustNm', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'SalesCustCd', title: '@Resources.Locale.L_ShipmentID_SalesCustCd', index: 'SalesCustCd', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'SalesCustNm', title: '@Resources.Locale.L_ShipmentID_SalesCustNm', index: 'SalesCustNm', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'CaCd', title: 'Carrier', index: 'CaCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'CaName', title: 'Carrier Name', index: 'CaName', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ExCd', title: 'Express', index: 'ExCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ExName ', title: 'Express Name', index: 'ExName', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'BaCd', title: '@Resources.Locale.L_ShipmentID_BaCd ', index: 'BaCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'BaNm ', title: 'name', index: 'BaNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'FwCd', title: 'Forwarder', index: 'FwCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'FwNm ', title: 'Forwarder name', index: 'FwNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'BkCd', title: 'Broker', index: 'BkCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'BkNm ', title: 'Broker name', index: 'BkNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'PkCd', title: 'Trucker', index: 'PkCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'PkNm ', title: 'Trucker name', index: 'PkNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ReveiveDate', title: '@Resources.Locale.L_ShipmentID_ReveiveDate', index: 'ReveiveDate', width: 80, align: 'left', sorttype: 'string', hidden: false },
           { name: 'CntrType', title: '@Resources.Locale.L_ShipmentID_CntrType', index: 'CntrType', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'CntrDescp', title: '@Resources.Locale.L_ShipmentID_CntrDescp', index: 'CntrDescp', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'CntrQty', title: '@Resources.Locale.L_ShipmentID_CntrQty', index: 'CntrQty', width: 80, align: 'right', sorttype: 'number', hidden: true },
           { name: 'Feu', title: '@Resources.Locale.L_ShipmentID_Feu', index: 'Feu', width: 80, align: 'right', sorttype: 'number', hidden: true },
           { name: 'ServiceMode', title: '@Resources.Locale.L_ShipmentID_ServiceMode', index: 'ServiceMode', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'ServiceLoading', title: '@Resources.Locale.L_ShipmentID_ServiceLoading', index: 'ServiceLoading', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'Qty', title: '@Resources.Locale.L_ShipmentID_Qty', index: 'Qty', width: 80, align: 'right', sorttype: 'integer', hidden: true },
           { name: 'Qtyu', title: 'Qtyu', index: 'Qtyu', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'Gw', title: '@Resources.Locale.L_ShipmentID_Gw', index: 'Gw', width: 80, align: 'right', sorttype: 'number', hidden: true },
           { name: 'Gwu', title: 'Gwu', index: 'Gwu', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'Nw', title: '@Resources.Locale.L_ShipmentID_Nw', index: 'Nw', width: 80, align: 'right', sorttype: 'number', hidden: true },
           { name: 'Cbm', title: 'Cbm', index: 'Cbm', width: 80, align: 'right', sorttype: 'number', hidden: true },
           { name: 'AcRemark', title: '@Resources.Locale.L_ShipmentID_AcRemark', index: 'AcRemark', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Cost', title: '@Resources.Locale.L_ShipmentID_Cost', index: 'Cost', width: 80, align: 'right', sorttype: 'number', hidden: true },
           { name: 'CreateBy', title: '@Resources.Locale.L_ShipmentID_CreateBy', index: 'CreateBy', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'CreateDep', title: '@Resources.Locale.L_ShipmentID_CreateDep', index: 'CreateDep', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'CreateExt', title: '@Resources.Locale.L_ShipmentID_CreateExt', index: 'CreateExt', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'CreateDate', title: '@Resources.Locale.L_ShipmentID_CreateDate', index: 'CreateDate', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'ModifyBy', title: '@Resources.Locale.L_ShipmentID_ModifyBy', index: 'ModifyBy', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'ModifyDate', title: '@Resources.Locale.L_ShipmentID_ModifyDate', index: 'ModifyDate', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'SpecProcid', title: '@Resources.Locale.L_ShipmentID_SpecProcid', index: 'SpecProcid', width: 100, align: 'right', sorttype: 'integer', hidden: false },
           { name: 'SpecDescp', title: '@Resources.Locale.L_ShipmentID_SpecDescp', index: 'SpecDescp', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'PkgMark', title: '@Resources.Locale.L_ShipmentID_PkgMark', index: 'PkgMark', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ShipMark', title: '@Resources.Locale.L_ShipmentID_ShipMark', index: 'ShipMark', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ActShipDate', title: '@Resources.Locale.L_ShipmentID_ActShipDate', index: 'ActShipDate', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'ActPostDate', title: '@Resources.Locale.L_ShipmentID_ActPostDate', index: 'ActPostDate', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'OrderReason', title: '@Resources.Locale.L_ShipmentID_OrderReason', index: 'OrderReason', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'Sloc', title: '@Resources.Locale.L_ShipmentID_Sloc', index: 'Sloc', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'Unicode', title: '@Resources.Locale.L_ShipmentID_Unicode', index: 'Unicode', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'AsdNo', title: '@Resources.Locale.L_ShipmentID_AsdNo', index: 'AsdNo', width: 80, align: 'left', sorttype: 'string', hidden: true },
           { name: 'BandType ', title: '@Resources.Locale.L_ShipmentID_BandType', index: 'BandType', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'CargoType', title: '@Resources.Locale.L_ShipmentID_CargoType', index: 'CargoType', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'TrackWay ', title: '@Resources.Locale.L_ShipmentID_TrackWay', index: 'TrackWay', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'CentDecl', title: '@Resources.Locale.L_ShipmentID_CentDecl', index: 'CentDecl', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'DeepProcess', title: '@Resources.Locale.L_ShipmentID_DeepProcess', index: 'DeepProcess', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'Plant', title: 'Plant', index: 'Plant', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'PlantNm', title: 'Plant Name', index: 'PlantNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'SealQty ', title: '@Resources.Locale.L_ShipmentID_SealQty', index: 'SealQty', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'BookingAgent', title:'@Resources.Locale.L_ShipmentID_BookingAgent', index: 'BookingAgent', width: 100, align: 'left', sorttype: 'string', hidden: false },
           { name: 'BookingNm', title:'@Resources.Locale.L_ShipmentID_BookingNm', index: 'BookingNm', width: 100, align: 'left', sorttype: 'string', hidden: false }
    ];


    $grid = $("#MainGrid");
    _dm.addDs("MainGrid", [], ["Cur"], $grid[0]);
    new genGrid(
    	$grid,
    	{
    	    data: [],
    	    loadonce: true,
    	    colModel: colModel,
    	    datatype: "json",
    	    url: rootPath + "BookingAction/ShipmentDNQuery?shipmentid=" + _shipmentid,
    	    cellEdit: false,//禁用grid编辑功能
    	    caption: "DN",
    	    height: gridHeight,
    	    rownumWidth: 50,
    	    refresh: true,
    	    rows: 9999,
    	    exportexcel: false,
    	    pginput: false,
    	    pgbuttons: false,
    	    ds: _dm.getDs("MainGrid"),
    	    sortorder: "asc",
    	    sortname: "CreateDate",
    	    delKey: "UId",
    	    beforeSelectRowFunc: function (rowid) {
    	        //main key 修改時不允與修改
    	        if (rowid != null && rowid.indexOf("jqg") >= 0) {
    	            $("#MainGrid").setColProp('Cur', { editable: true });
    	        } else {
    	            $("#MainGrid").setColProp('Cur', { editable: false });
    	        }
    	    },
    	    onAddRowFunc: function (rowid) {

    	    },
    	    beforeAddRowFunc: function (rowid) {
    	        //add row 時要可以編輯main key
    	        $("#MainGrid").setColProp('Cur', { editable: true });
    	    }
    	}
    );

    /*MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit"]);
        location.reload();
        gridEditableCtrl({ editable: false, gridId: "MainGrid" });
        editable = false;
    }

    MenuBarFuncArr.MBEdit = function () {
        gridEditableCtrl({ editable: true, gridId: "MainGrid" });
        editable = true;
    }

    MenuBarFuncArr.MBSave = function (dtd) {

        editable = false;
        var containerArray = $('#MainGrid').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;
        $.ajax({
            async: true,
            url: rootPath + "SYSTEM/CurrencyUpdate",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
                CommonFunc.Notify("", errMsg, 500, "danger");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                console.log(result.message);
                if (result.message !== "success") {
                    CommonFunc.Notify("", "保存失败", 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return;
                }

                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "保存成功", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                gridEditableCtrl({ editable: false, gridId: "MainGrid" });
                editable = false;

                dtd.resolve();
                //location.reload();
            }
        });
        return dtd.promise();
    }

    */
    initMenuBar(MenuBarFuncArr);

    MenuBarFuncArr.DelMenu(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);
    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBEdit"]);

});

