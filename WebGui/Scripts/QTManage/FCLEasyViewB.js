var $MainGrid;
function initQTGrid() {
    $MainGrid = $("#MainGrid");
    
    var colModel = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, hidden: true, search: false },
        { name: 'RfqNo', title: '@Resources.Locale.L_RQQuery_RfqNo', index: 'RfqNo', sorttype: 'string', width: 100, hidden: true, search: false, frozen:true },
        { name: 'AllIn', title: '@Resources.Locale.L_AirQuery_AllIn', index: 'AllIn', sorttype: 'string', width: 100, hidden: true, search: false, frozen:true },
        { name: 'TranMode', title: '@Resources.Locale.L_RQQuery_TranMode', index: 'TranMode', sorttype: 'string', width: 100, hidden: true, search: false, frozen:true },
        { name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', sorttype: 'string', width: 100, hidden: true, search: false, frozen:true },
        { name: 'Region', title: '@Resources.Locale.L_BaseLookup_Region', index: 'Region', sorttype: 'string', width: 80, hidden: false, search: true, searchoptions:{sopt:['cn']}, frozen:true},
        { name: 'PolCd', title: '@Resources.Locale.L_BaseLookup_PolCd', index: 'PolCd', sorttype: 'string', width: 80, hidden: false, search: true, searchoptions:{sopt:['cn']}, frozen:true },
        { name: 'PolNm', title: '@Resources.Locale.L_BaseLookup_PolName', index: 'PolNm', sorttype: 'string', width: 120, hidden: false, search: false, frozen:true },
        { name: 'PodCd', title: '@Resources.Locale.L_BaseLookup_PodCd', index: 'PodCd', sorttype: 'string', width: 80, hidden: false, search: true, searchoptions:{sopt:['cn']}, frozen:true },
        { name: 'PodNm', title: '@Resources.Locale.L_BaseLookup_PodName', index: 'PodNm', sorttype: 'string', width: 120, hidden: false, search: false, frozen:true },
        { name: 'Carrier', title: '@Resources.Locale.L_DNApproveManage_CaCd', index: 'Carrier', edittype: 'custom',  sorttype: 'string', width: 90, hidden: false, search: false},
        { name: 'LoadingFrom', title: 'Loading From', index: 'LoadingFrom', sorttype: 'string', width: 100, hidden: false, search: false },
        { name: 'LoadingTo', title: 'Loading To', index: 'LoadingTo', sorttype: 'string', width: 100, hidden: false, search: false },
        //{ name: 'ServiceMode', title: '@Resources.Locale.L_RQQuery_ServiceMode', index: 'ServiceMode', edittype: 'custom', editoptions: gridLookup(getTermop("ServiceMode")), sorttype: 'string', width: 100, hidden: false, search: false },
        { name: 'Cur', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Cur',  edittype: 'custom', sorttype: 'string', width: 90, hidden: false, search: false },
        { name: 'F2', title: "@Resources.Locale.L_SFCLQuery_F2", index: 'F2', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, search: false, editable: true },
        { name: 'F3', title: "@Resources.Locale.L_SFCLQuery_F3", index: 'F3', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, search: false, editable: true },
        { name: 'F4', title: "@Resources.Locale.L_SFCLQuery_F4", index: 'F4', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, search: false, editable: true },
        { name: 'F12', title: "@Resources.Locale.L_SFCLQuery_F12", index: 'F12', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, search: false, editable: true },
        { name: 'F13', title: "@Resources.Locale.L_SFCLQuery_F13", index: 'F13', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, search: false, editable: true },
        { name: 'F14', title: "@Resources.Locale.L_SFCLQuery_F14", index: 'F14', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, search: false, editable: true },
        { name: 'SailingDay', title: '@Resources.Locale.L_SFCLQuery_SailingDay', index: 'SailingDay', width: 100, align: 'left', sorttype: 'string', hidden: false, search: false, editable: true },
        { name: 'FreeOdt', title: '@Resources.Locale.L_SFCLQuery_FreeOdt', index: 'FreeOdt', width: 200, align: 'right', formatter: 'integer', hidden: false, search: false, editable: true },
        { name: 'FreeOdm', title: '@Resources.Locale.L_SFCLQuery_FreeOdm', index: 'FreeOdm', width: 200, align: 'right', formatter: 'integer', hidden: false, search: false, editable: true },
        { name: 'FreeDdt', title: '@Resources.Locale.L_SFCLQuery_FreeDdt', index: 'FreeDdt', width: 200, align: 'right', formatter: 'integer', hidden: false, search: false, editable: true },
        { name: 'FreeDdm', title: '@Resources.Locale.L_SFCLQuery_FreeDdm', index: 'FreeDdm', width: 200, align: 'right', formatter: 'integer', hidden: false, search: false, editable: true },
        { name: 'Tt', title: '@Resources.Locale.L_RouteSetup_Tt', index: 'Tt', width: 60, align: 'right', formatter: 'integer', hidden: false, search: false, editable: true },
        { name: 'ViaNm', title: '@Resources.Locale.L_SFCLQuery_ViaNm', index: 'ViaNm', sorttype: 'string', width: 200, hidden: false, search: false, editable: true },
        { name: 'Note', title: '@Resources.Locale.L_SFCLQuery_Note', index: 'Note', sorttype: 'string', width: 100, hidden: false, search: false, editable: true },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', sorttype: 'string', width: 200, hidden: false, search: false, editoptions: { size: 500, maxlength: 500 } },
        { name: 'SeqNo', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, search: false },
       { name: 'QuotNo', title: '@Resources.Locale.L_QTQuery_QuotNo', index: 'QuotNo', sorttype: 'string', width: 100, hidden: true, search: false }
    ];

    new genGrid(
        $MainGrid,
        {
            datatype: "local",
            loadonce:true,
            colModel: colModel,
            caption: "FCL",
            height: 300,
            rows: 999999,
            refresh: false,
            cellEdit: false,//禁用grid编辑功能
            pginput: false,
            sortable: true,
            pgbuttons: false,
            exportexcel: false,
            toppager:false,
            ignoreCase: true,
            sortname: "PolCd",
            loadComplete: function(data){
                dynamicHeight();
            }
        }
    );
}

$(function () {
    intQtView();
    initQTGrid();

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];
        if (data["sub"])
        {
            $MainGrid.jqGrid("clearGridData");
            $MainGrid.jqGrid("setGridParam", {
                datatype: 'local',
                data: data["sub"],
            }).trigger("reloadGrid");
        }

        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);

        MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");


        var multiEdocData = [
            { jobNo: _handler.topData["RqUid"], 'GROUP_ID': _handler.topData["RqGroupid"], 'CMP': _handler.topData["RqCmp"], 'STN': '*' }
        ];

        setRQData(data);
        _handler.beforLoadView();
    }
    loadQtView();
    MenuBarFuncArr.DelMenu(["MBAdd", "MBEdit", "MBCopy", "MBDel", "MBSave", "MBCancel", "MBEdoc", "QuotTypeBtn", "VoidBtn", "EXCEL_BTN"]);

    $("#MainGrid").jqGrid('filterToolbar',  {stringResult: true, searchOnEnter: false});
});



