var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $MainGrid, $SubGrid;
var IoFlag = getCookie("plv3.passport.ioflag");
var viewable = false;
var editable = true;
_handler.key = "VenderCd";
var _fields = [];
$(function () {
    $MainGrid = $("#MainGrid");
    _handler.saveUrl = rootPath + "ReffeeManage/WeightUpdateData";
    _handler.inquiryUrl = rootPath + "";
    _handler.config = {};

    _handler.addData = function () {
        //初始化新增数据
        var data = {};

        setFieldValue([data]);
        $MainGrid.jqGrid("clearGridData");
    }

    _handler.saveData = function (dtd) {
        /*var nullCols = [], sameCols = [];
        sameCols.push({ name: "VenderDocNo", index: 10, text: jslang["L_ReffeeManage_VenderDocNo"] });
        nullCols.push({ name: "ChgCd", index: 10, text: jslang["L_ReffeeManage_ChgCd"] });
        nullCols.push({ name: "VenderDocNo", index: 10, text: jslang["L_ReffeeManage_VenderDocNo"] });
        nullCols.push({ name: "VenderCd", index: 10, text: jslang["L_ReffeeManage_VenderCd"] });
        if (_handler.checkData($MainGrid, nullCols, sameCols) === false) {
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve();
            return false;
        }*/
        //var allData = $MainGrid.jqGrid("getGridParam", "data");
        var saveData = [];

        var allData = $MainGrid.find("tr");
        var SeqNo = 0;
        $.each(allData, function (i, val) {
            if (typeof val.id != "undefined" && val.id != "") {
                //var SeqNo = getGridVal($MainGrid, val.id, "SeqNo", null);
                var FeeWeight = getGridVal($MainGrid, val.id, "FeeWeight", null);
                var FeeOp = getGridVal($MainGrid, val.id, "FeeOp", null);
                var Remark = getGridVal($MainGrid, val.id, "Remark", null);
                var ChgDescp = getGridVal($MainGrid, val.id, "ChgDescp", null);
                var Punit = $("#Punit").val();
                var CalType = getGridVal($MainGrid, val.id, "CalType", null);

                var Data = {
                    "SeqNo": SeqNo,
                    "FeeWeight": FeeWeight,
                    "FeeOp": FeeOp,
                    "Remark": Remark,
                    "ChgDescp": ChgDescp,
                    "Punit": Punit,
                    "CalType": CalType,
                    "FeeType": "D",
                    "__state": 1
                };

                saveData.push(Data);
                SeqNo++;
            }
        });

        //var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
        // var changeData = getChangeValue();//获取所有改变的值
        var changeData = { mt: [] };
        changeData["mt"] = saveData;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["VenderCd"] = encodeURIComponent($("#VenderCd").val());
        data["VenderNm"] = encodeURIComponent($("#VenderNm").val());
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

    _handler.delData = function () {
        var changeData = getAllKeyValue();//获取所有主键值
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["VenderCd"] = encodeURIComponent($("#VenderCd").val());
        data["VenderNm"] = encodeURIComponent($("#VenderNm").val());
        ajaxHttp(_handler.saveUrl, data,
            function (result) {
                if (result.message) {
                    CommonFunc.Notify("", result.message, 1000, "warning");
                    return false;
                }
                else if (_handler.setFormData) {
                    _handler.setFormData([{}]);
                }
                return true;
            });
    }

    _handler.setFormData = function (data) {
        console.log(data);
        if (data["main"]) {
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        }
        else {
            _handler.topData = [{}];
        }
        if (data["main"])
            _handler.loadGridData("MainGrid", $MainGrid[0], data["main"], [""]);
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

        if (typeof data["main"][0] !== "undefined") {
            if (data["main"][0].EdiFlag == "Y") {
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
        ajaxHttp(rootPath + "ReffeeManage/WeightGetDetail", { UId: map.VenderCd, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    _handler.beforSave = function () {
        return true;
    }

    

    var colModel = [];

    genMainGrid();

    _handler.beforLoadView = function () {

    }

    _initUI(["MBApply", "MBApprove", "MBErrMsg", "MBSearch", "MBEdoc", "MBCopy", "MBInvalid"]);//初始化UI工具栏
    if (!isEmpty(_VenderCd)) {
        _handler.topData = { VenderCd: _VenderCd };
        MenuBarFuncArr.MBCancel();
    }


    registBtnLookup($("#VenderCdLookup"), {
            item: '#VenderCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
                $("#VenderCd").val(map.PartyNo);
                $("#VenderNm").val(map.PartyName);
            }
        }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
            $("#VenderNm").val(rd.PARTY_NAME);
        }));
});

function statusIsInvalid() {
    MenuBarFuncArr.Disabled(["MBEdoc", "MBEdit", "MBDel", "MBSave", "MBCancel", "MBInvalid", "SEND_BTN"]);
}

function genMainGrid() {
    var colModelSetting = [
        { name: 'UId', editable: false, hidden: true, width: 50, order: 0 },
        { name: 'FeeType', editable: false, hidden: true, width: 50, order: 0 },
        { name: 'VenderCd', editable: false, hidden: true, width: 50, order: 0 },
        { name: 'VenderNm', editable: false, hidden: true, width: 50, order: 0 },
        { name: 'SeqNo', editable: false, hidden: true, width: 50, order: 1 },
        { name: 'ChgDescp', editable: true, hidden: false, width: 150, order: 2 },
        { name: 'FeeOp', editable: true, hidden: false, width: 150, order: 3, formatter: "select", editoptions: { value: '1:<;2:<=;3:>;4:>=' }, edittype: 'select'},
        { name: 'FeeWeight', editable: true, hidden: false, width: 150, order: 4 },
        { name: 'Remark', editable: true, hidden: false, width: 150, order: 5 },
        { name: 'GroupId', hidden: true },
        { name: 'Cmp', hidden: true },
        { name: 'Stn', hidden: true },
        { name: 'Dep', hidden: true },
        { name: 'ChgId', hidden: true },
        { name: 'ChgCd', hidden: true },
        { name: 'Gw', hidden: true },
        { name: 'Cbm', hidden: true },
        { name: 'Remark', hidden: true },
        { name: 'CreateBy', hidden: true },
        { name: 'CreateDate', hidden: true },
        { name: 'ModifyBy', hidden: true },
        { name: 'ModifyDate', hidden: true },
        { name: 'FeeType', hidden: true },
        { name: 'FeeFrom', hidden: true },
        { name: 'FeeTo', hidden: true },
         { name: 'Punit',hidden: true },
        { name: 'CalType', editable: true, hidden: false, width: 170, formatter: "select", editoptions: { value: 'C:summation;F:constant' }, edittype: 'select' }
    ]
    var colModel = [];
    genColModel("ECREFFEE", "U_ID", "L_ReffeeManage", colModelSetting).done(function (result) {
        colModel = result;
        _handler.intiGrid("MainGrid", $MainGrid, {
            colModel: colModel, caption: jslang["L_ReffeeManage_WeightList"], delKey: ["UId"], footerrow: false,
            onAddRowFunc: function (rowid) {
                if ($("#VenderCd").val() == "") {
                    alert(jslang["L_ExpressSetupView_Msg1"]);
                    $MainGrid.jqGrid('delRowData', rowid);
                    return false;
                }

                setGridVal($MainGrid, rowid, "VenderCd", $("#VenderCd").val());
                setGridVal($MainGrid, rowid, "FeeType", "D");
            },
            beforeSelectRowFunc: function (rowid) {
            },
            afterSaveCellFunc: function (rowid) {
            },
            loadComplete: function (data) {

            }
        });
    });
}