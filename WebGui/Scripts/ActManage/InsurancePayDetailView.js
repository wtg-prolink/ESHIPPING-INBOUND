var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $MainGrid,$SubGrid, $SubGrid2;
$(function () {
    Schemas = JSON.parse(decodeHtml(Schemas));
    CommonFunc.initField(Schemas);
    setdisabled(true);

    $MainGrid = $("#MainGrid");
   

    _handler.saveUrl = rootPath + "ActManage/SaveInsuranceDetailData";
    _handler.inquiryUrl = rootPath + "ActManage/GetSMIPCDetail";
    _handler.config = false;

    _handler.addData = function () {
        //初始化新增数据
        var dep = getCookie("plv3.passport.dep"),ext = getCookie("plv3.passport.ext");
        //var data = {
        //    "CreateBy": userId, "CreateDate": getDate(0, "-"), "Cmp": cmp , "CreateDep": dep, "CreateExt": ext
        //};
        data[_handler.key] = uuid();
        //setFieldValue([data]);
        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        //getAutoNo("PoNo", "rulecode=SMRV_NO&cmp=" + cmp);        
    }

    _handler.saveData = function (dtd) {
        var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = _uid;
        data["Job_No"] = JobNo;
        data["Job_Type"] = JobType;
        ajaxHttpSaveBar(dtd, _handler.saveUrl, data,
            function (result) {
                //_topData = keyData["mt"];
                if (result.message) {
                    alert(result.message);
                    return false;
                }
                else if (_handler.setFormData)
                    _handler.setFormData(result);
                return true;
            });
    }

    _handler.setFormData = function (data) {
        
        if (data["sub"])
            _handler.loadGridData("MainGrid", $MainGrid[0], data["sub"], [""]);
        else
            _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        //MenuBarFuncArr.initEdoc($("#UId").val());
        MenuBarFuncArr.Enabled(["MBCopy"]);
        //MenuBarFuncArr.Enabled(["SEND_BTN"]);
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "ActManage/GetSMIPCDetail", { uId: map.UId,JobType:JobType,loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
        
    }

    var colModel = [
       { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'UFid', title: 'U FID', index: 'UFid', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'SeqNo', title: 'SeqNo', index: 'SeqNo', sorttype: 'string', width: 60, editable: false, hidden: false },
       { name: 'JobNo', title: '@Resources.Locale.L_ContractSetup_JobNo', index: 'JobNo', sorttype: 'string', width: 100, editable: false, hidden: false },
       { name: 'JobType', title: '@Resources.Locale.L_SMIPC_JobType', index: 'JobType', sorttype: 'string', width: 100, hidden: false, editable: false, formatter: "select", editoptions: { value: '@Resources.Locale.L_InsurancePayDetailView_Script_75' }, edittype: 'select' },       
       { name: 'InsCd', title: '@Resources.Locale.L_SMIPC_InsCd', index: 'InsCd', sorttype: 'string', width: 100, editable: true, hidden: false },
       { name: 'ChgDescp', title: '@Resources.Locale.L_SMIPC_ChgDescp', index: 'ChgDescp', sorttype: 'string', width: 100, editable: true, hidden: false },
       { name: 'IpAmt', title: '@Resources.Locale.L_SMIPC_IpAmt', index: 'IpAmt', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
       { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', sorttype: 'string', width: 300, editable: true, hidden: false }
    ];


    _handler.intiGrid("MainGrid", $MainGrid, {
        colModel: colModel, caption: '@Resources.Locale.L_InsurancePaySetupView_Views_211', delKey: ["UId"], height: 100,
        onAddRowFunc: function (rowid) {
            var dep = getCookie("plv3.passport.dep")
            var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            setGridVal($MainGrid, rowid, "SeqNo", maxSeqNo + 1);            
            //setGridVal($MainGrid, rowid, "JobNo", $("#JobNo").val());
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
        }
    });


    _initUI(["MBApply", "MBApprove", "MBErrMsg","MBDel","MBAdd"]);//初始化UI工具栏

    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
});


