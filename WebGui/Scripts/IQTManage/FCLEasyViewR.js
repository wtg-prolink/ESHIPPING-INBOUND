var $MainGrid;
var _fields = [];
function initQTGrid() {
    $MainGrid = $("#MainGrid");
    
    var colModel = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100,  hidden: true, search:false },
        { name: 'RfqNo', title: 'RfqNo', index: 'RfqNo', sorttype: 'string', width: 100,  hidden: true, search:false },
        { name: 'LspCd', title: 'LspCd', index: 'LspCd', sorttype: 'string', width: 100,  hidden: true, search:false },
        { name: 'AllIn', title: 'AllIn', index: 'AllIn', sorttype: 'string', width: 100,  hidden: true, search:false },
        { name: 'PolCd', title: 'PolCd', index: 'PolCd', sorttype: 'string', width: 100,  hidden: true, search:false },
        { name: 'PolNm', title: 'PolNm', index: 'PolNm', sorttype: 'string', width: 100,  hidden: true, search:false },
        { name: 'Incoterm', title: 'Incoterm', index: 'Incoterm', sorttype: 'string', width: 100,  hidden: true, search:false },
        { name: 'TranMode', title: 'TranMode', index: 'TranMode', sorttype: 'string', width: 100,  hidden: true, search:false },
        { name: 'Carrier', title: '@Resources.Locale.L_DNApproveManage_CaCd', index: 'Carrier', edittype: 'custom', sorttype: 'string', width: 120, hidden: false, search:false, editable: true },
        { name: 'PodCd', title: '@Resources.Locale.L_FCLFSetup_PodCd', index: 'PodCd', sorttype: 'string', width: 100, hidden: false, search: true },
        { name: 'PodNm', title: '@Resources.Locale.L_FCLFSetup_PodCd'+" Name", index: 'PodNm', sorttype: 'string', width: 100, search:false,  hidden: false },
        { name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', edittype: 'custom', sorttype: 'string', width: 100, hidden: false, search:false, },
        { name: 'ChgDescp', title: '@Resources.Locale.L_SMCHGSetup_ChgDescp', index: 'ChgDescp', sorttype: 'string', width: 150, hidden: false, editable: false,search:false },
        { name: 'ChgType', title: '@Resources.Locale.L_SMCHGSetup_ChgType', index: 'ChgType', sorttype: 'string', width: 100, hidden: false,  formatter: "select", editoptions: { value: 'F:F.Freight Charge;O:O.Original Fee;D:D.Destination Fee' }, edittype: 'select', search:false},
        { name: 'Punit', title: '@Resources.Locale.L_AirQuery_Punit', index: 'Punit', sorttype: 'string', width: 100, hidden: false,  formatter: "select", editoptions: { value: _unitSelect }, edittype: 'select', search:false, },
        { name: 'Cur', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Cur', sorttype: 'string', edittype: 'custom', width: 100, align: 'left', hidden: false, search:false },
        { name: 'F3', title: '@Resources.Locale.L_FCLFSetup_F3', index: 'F3', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, search:false},
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 300, align: 'left', sorttype: 'string', hidden: false,  editoptions: { size: 500, maxlength: 500 }, search:false },
        { name: 'ServiceMode', title: 'Service Mode', index: 'ServiceMode', sorttype: 'string', width: 100,  hidden: true, search:false },
        { name: 'LoadingFrom', title: 'Loading From', index: 'LoadingFrom', sorttype: 'string', width: 100,  hidden: true, search:false },
        { name: 'LoadingTo', title: 'Loading To', index: 'LoadingTo', sorttype: 'string', width: 100,  hidden: true, search:false },
        { name: 'SeqNo', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, search:false, editable: false },
       { name: 'QuotNo', title: '@Resources.Locale.L_QTQuery_QuotNo', index: 'QuotNo', sorttype: 'string', width: 100, editable: false, hidden: true, search:false }
    ];
    new genGrid(
        $MainGrid,
        {
            datatype: "local",
            loadonce:true,
            colModel: colModel,
            isModel: true,
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


$(function() {
    SetCntUnit();
    intQtView();
    initQTGrid();
    _handler.inquiryUrl = "";


    _handler.beforLoadView = function () {
        //$("#RfqNo").attr('isKey', true);
        //$("#RfqNo").attr('disabled', true);
        var requires = ["QuotNo", "QuotDate", "EffectFrom", "EffectTo", "Rlocation", "LspCd"];
        var readonlys = ["Rlocation", "RlocationNm", "LspNm"];
      
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('readonly', true);
        }

        for (var i = 0; i < requires.length; i++) {
            $("#" + requires[i]).attr('required', true);
            $("[for=" + requires[i] + "]").css("color", "rgb(255, 0, 0)");
        }
    }

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

        if (_handler.topData.Period === "B") {
            $("#EXCEL_BTN").show();
        }
        else
            $("#EXCEL_BTN").hide();
        setRQData(data);
    }
    getSelectOptions();
    loadQtView(1);

    MenuBarFuncArr.DelMenu(["MBAdd", "MBEdit", "MBCopy", "MBDel", "MBSave", "MBCancel", "MBEdoc", "QuotTypeBtn", "VoidBtn", "EXCEL_BTN", "MBSearch"]);

    $("#MainGrid").jqGrid('filterToolbar',  {stringResult: true, searchOnEnter: false});
});



