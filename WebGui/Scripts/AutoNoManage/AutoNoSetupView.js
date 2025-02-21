var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $MainGrid,$SubGrid;
var IoFlag = getCookie("plv3.passport.ioflag");
var viewable = false;
var editable = true;
_handler.key = "RuleCode";
var _fields = [];
$(function () {
    _handler.saveUrl = rootPath + "AutoNoManage/UpdateData";
    _handler.inquiryUrl = rootPath + "";
    _handler.config = {};

    _handler.addData = function () {
        //初始化新增数据
        var data = {};

        setFieldValue([data]);
        $MainGrid.jqGrid("clearGridData");
        $("#Cmp").val(_Cmp);
    }

    _handler.saveData = function (dtd) {
        /*var nullCols = [], sameCols = [];
        sameCols.push({ name: "VenderDocNo", index: 10, text: jslang["L_AutoNoManage_VenderDocNo"] });
        nullCols.push({ name: "ChgCd", index: 10, text: jslang["L_AutoNoManage_ChgCd"] });
        nullCols.push({ name: "VenderDocNo", index: 10, text: jslang["L_AutoNoManage_VenderDocNo"] });
        nullCols.push({ name: "VenderCd", index: 10, text: jslang["L_AutoNoManage_VenderCd"] });
        if (_handler.checkData($MainGrid, nullCols, sameCols) === false) {
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve();
            return false;
        }*/
        var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), "TruckFeeType": "Repair", autoReturnData: false };
        data["RuleCode"] = encodeURIComponent($("#RuleCode").val());
        data["Cmp"] = encodeURIComponent($("#Cmp").val());
        data["Stn"] = encodeURIComponent($("#Stn").val());
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
        console.log(data);
        if (data["main"])
        {
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        }
        else
        {
            _handler.topData = [{}];
        }
        if (data["sub"])
            _handler.loadGridData("MainGrid", $MainGrid[0], data["sub"], [""]);
        else
            _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);

        var col = $MainGrid.jqGrid('getCol', 'Status', false);//获取批文号码列的值
        $.each(col, function (index, colname) {
            if (colname == "N") {
                $MainGrid.jqGrid('setRowData', index + 1, false, 'gridTagClass');
            }
        });
        setFieldValue(data["main"] || [{}]);

        setdisabled(true);
        setToolBtnDisabled(true);
        MenuBarFuncArr.Enabled(["MBEdoc", "MBInvalid"]);

        if(typeof data["main"][0] !== "undefined")
        {
            if(data["main"][0].EdiFlag == "Y")
            {
                MenuBarFuncArr.Disabled(["MBEdit", "MBDel"]);
            }
        }
        MenuBarFuncArr.Enabled(["MBCopy"]);
    }

    _handler.loadMainData = function (map) {
        console.log(_handler.key);
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "AutoNoManage/GetDetail", { RuleCode: map.RuleCode, Cmp: map.Cmp, Stn: map.Stn, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    _handler.beforSave = function () {
        return true;
    }

    $MainGrid = $("#MainGrid");

    var colModel = [];

    genMainGrid();

    _handler.beforLoadView = function () {

    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        var data = {};
        //data[_handler.key] = uuid();       
        $("#Cmp").val("");
        var dataRow, addData = [];
        $("#Stn").val("");

        var rowIds = $MainGrid.getDataIDs();
        var fields = [];
        for (var i = 0; i < rowIds.length; i++) {
            var rowDatas = $MainGrid.jqGrid('getRowData', rowIds[i]);
            addData.push(rowDatas);
        }

        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        for (var i = 0; i < addData.length; i++) {
            $("#MainGrid").jqGrid("addRowData", undefined, addData[i], "last");
        }

        gridEditableCtrl({ editable: true, gridId: "MainGrid" });
    }

    _initUI(["MBApply", "MBApprove", "MBErrMsg", "MBSearch"]);//初始化UI工具栏
    if (!isEmpty(_RuleCode)) {
        _handler.topData = { RuleCode: _RuleCode, Cmp: _Cmp, Stn:_Stn };
        MenuBarFuncArr.MBCancel();
    }


    /*$("#CmpLookup").v3Lookup({
        url:　rootPath + "TPVCommon/GetSiteCmpData",
        gridFunc: function (map) {
            var value = map.Cd;
            console.log(value);
            $("#Cmp").val(value);
        },
        lookUpConfig: LookUpConfig.SiteLookup
    });

    $("#Cmp").v3AutoComplete({
        params: "dt=stn",
        keyinNum: "1",
        returnValue: "CMP&NAME=showValue,CMP,NAME",
        callBack: function (event, ui) {
            console.log(ui.item.returnValue.CMP);
            $(this).val(ui.item.returnValue.CMP);
            return false;
        },
        dymcFunc: function(){
            return "&TYPE=1&CMP~";
        }
    });*/

    registBtnLookup($("#CmpLookup"), {
        item: '#Cmp', url: rootPath + "Common/GetCompanyData", config: LookUpConfig.CmpLookup, param: "", selectRowFn: function (map) {
            $("#Cmp").val(map.Cmp);
        }
    }, undefined, LookUpConfig.GetCmpAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Cmp").val(rd.CMP);
        //elem.val(rd.NAME);
    }));

    $("#StnLookup").v3Lookup({
        url:　rootPath + "EcallCommon/GetSiteStnData",
        gridFunc: function (map) {
            var value = map.Stn;
            $("#Stn").val(value);
        },
        lookUpConfig: LookUpConfig.SiteLookup,
        baseConditionFunc: function () {
            return "&sopt_Type=eq&Type=2";
        }
    });

    $("#Stn").v3AutoComplete({
        params: "dt=stn",
        keyinNum: "1",
        returnValue: "STN&NAME=showValue,STN,NAME",
        callBack: function (event, ui) {
            $(this).val(ui.item.returnValue.STN);
            return false;
        },
        dymcFunc: function(){
            return "&TYPE=2&STN~";
        },
        clearFunc: function () {
            $("#Stn").val("");
        }
    });
});

function statusIsInvalid()
{
    MenuBarFuncArr.Disabled(["MBEdoc","MBEdit", "MBDel", "MBSave", "MBCancel", "MBInvalid", "SEND_BTN"]);
}

function genMainGrid()
{
    var UserOpt = {};
    UserOpt.gridUrl = rootPath + "EcallCommon/getUserIdByLookup";
    UserOpt.isMutiSel = true;
    UserOpt.param = "";
    UserOpt.gridId = "MainGrid";
    UserOpt.gridReturnFunc = function (map) {           
        var UId = map.UId,
            UName = map.UName;

        var rowid = $MainGrid.jqGrid('getGridParam', 'selrow');
        console.log(rowid);
        setGridVal($MainGrid, rowid+"", 'Driver', UId, "lookup");
        setGridVal($MainGrid, rowid, 'DriverNm', UName, null);

        return UId;
    }
    UserOpt.lookUpConfig = LookUpConfig.UserLookup;

    var colModelSetting = [
        { name: 'RuleCode', editable: false, hidden: true, width: 50, order: 0},
        { name: 'GroupId', editable: false, hidden: true, width: 50, order: 0},
        { name: 'Cmp', editable: false, hidden: true, width: 50, order: 0},
        { name: 'Stn', editable: false, hidden: true, width: 50, order: 0},
        { name: 'EffectDate', editable: false, hidden: true, width: 50, order: 0},
        { name: 'SeqNo', editable: true, hidden: false, width: 50, order: 1},
        { name: 'Code', editable: true, hidden: false, width: 50, order: 2},
        { name: 'CodeName', editable: true, hidden: false, width: 150, order: 3},
        { name: 'CodeContent', editable: true, hidden: false, width: 150, order: 4},
        { name: 'CodeLen', editable: true, hidden: false, width: 150, order: 5},
        { name: 'CondFlag', editable: true, hidden: false, width: 50, order: 6}
    ]
    var colModel = [];
    genColModel("SCS_AUTONO_ITEM", "RULE_CODE", "L_AutoNoManage", colModelSetting).done(function (result) {
        colModel = result;
        _handler.intiGrid("MainGrid", $MainGrid, {
            colModel: colModel, caption: jslang["L_AutoNoManage_CostList"], delKey: ["RuleCode","SeqNo","Cmp","Stn"],footerrow: false,
            onAddRowFunc: function (rowid) {
                if($("#RuleCode").val() == "")
                {
                    alert("請先輸入Rule Code");
                    $MainGrid.jqGrid('delRowData',rowid);
                    return false;
                }
                 if($("#Cmp").val() == "")
                {
                    alert("請先輸入Region");
                    $MainGrid.jqGrid('delRowData',rowid);
                    return false;
                }
                 if($("#Stn").val() == "")
                {
                    alert("請先輸入Control Tower");
                    $MainGrid.jqGrid('delRowData',rowid);
                    return false;
                }

                setGridVal($MainGrid, rowid, "RuleCode", $("#RuleCode").val());
                setGridVal($MainGrid, rowid, "Cmp", $("#Cmp").val());
                setGridVal($MainGrid, rowid, "Stn", $("#Stn").val());
                setGridVal($MainGrid, rowid, "EffectDate", $("#EffectDate").val());
            },
            beforeSelectRowFunc: function (rowid) {
            },
            afterSaveCellFunc: function (rowid) {
            },
            loadComplete: function(data) {

            }
        });
    });
}