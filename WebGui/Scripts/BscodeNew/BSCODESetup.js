var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $MainGrid,$SubGrid;
$(function () {
    _handler.key = "CdType";
    _handler.saveUrl = rootPath + "BscodeNew/UpdateData";
    _handler.inquiryUrl = rootPath + "";
    _handler.config = {};

    _handler.addData = function () {
        //初始化新增数据
        var data = {};
        //data[_handler.key] = uuid();
        setFieldValue([data]);
        $MainGrid.jqGrid("clearGridData");
        //getAutoNo("BatchNo", "rulecode=BATCH_NO&stn=" + stn);
    }

    _handler.saveData = function (dtd) {
        var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["CdType"] = encodeURIComponent($("#CdType").val());
        data["Cmp"] = encodeURIComponent($("#Cmp").val());
        console.log(data);
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
        _handler.beforLoadView();
        setdisabled(true);
        setToolBtnDisabled(true);
        MenuBarFuncArr.initEdoc($("#UId").val());
        MenuBarFuncArr.Enabled(["MBEdoc", "MBInvalid","MBDel"]);
        //MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], stn);

    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "BscodeNew/GetDetail", { uId: map.CdType, Cmp: map.Cmp, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    _handler.beforSave = function () {
        if ("EDT" === _handler.topData["CdType"]) {
            var ids = $MainGrid.jqGrid('getDataIDs');
            for (var i = 0 ; i < ids.length; i++) {
                var Code = getGridVal($MainGrid, ids[i], "Cd", null);
                if (Code.indexOf(" ") > 0) {
                    alert("The CODE for eDOC uploading type can be only using letters without any blank.");
                    return false;
                }
            }
        }
        return true;
    }

    $MainGrid = $("#MainGrid");

    var colModel = [];

    genMainGrid();

    _handler.beforLoadView = function () {

    }

    _initUI(["MBApply", "MBApprove", "MBErrMsg", "MBCopy", "MBSearch"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { CdType: _uid };
        MenuBarFuncArr.MBCancel();
    }
});

function statusIsInvalid()
{
    MenuBarFuncArr.Disabled(["MBEdoc","MBEdit","MBCopy","MBSave", "MBCancel", "MBInvalid", "SEND_BTN"]);
}

var _upri = getCookie("plv3.passport.upri");
var _cmp = getCookie("plv3.passport.basecompanyid");

function genMainGrid()
{
    var colModelSetting = [
        { name: 'Cd', editable: false, hidden: false },
        { name: 'CdDescp', editable: true, hidden: false },
        { name: 'ArCd', editable: true, hidden: false },
        { name: 'ApCd', editable: true, hidden: false },
        { name: 'Apply', editable: true, hidden: true },
        { name: 'CdType',  editable: false, hidden: true },
        { name: 'GroupId', editable: true, hidden: true },
        { name: 'Cmp', editable: false, hidden: false },
        { name: 'Stn', editable: false, hidden: false },
        { name: 'Dep', editable: false, hidden: true },
        { name: 'Location', editable: false, hidden: true },
        { name: 'OrderBy', editable: true, hidden: true },
        { name: 'EdiFlag', editable: true, hidden: true },
        { name: 'Inttra', editable: true, hidden: false },
        { name: 'AmsCode', editable: false, hidden: true },
        { name: 'ModifyBy', editable: false, hidden: true },
        { name: 'CreateBy', editable: false, hidden: true },
        { name: 'CreateDate', editable: false, hidden: true }
    ];
    var colModel = [];
    genColModel("BSCODE", "U_ID", "L_BSCODE", colModelSetting).done(function (result) {
        colModel = result;
        _handler.intiGrid("MainGrid", $MainGrid, {
            colModel: colModel, caption: "bscode", delKey: ["Cd","Cmp"],
            onAddRowFunc: function (rowid) {

            },
            beforeSelectRowFunc: function (rowid) {
                if (rowid != null && rowid.indexOf("jqg") >= 0) {
                    $MainGrid.setColProp('Cd', { editable: true });
                } else {
                    $MainGrid.setColProp('Cd', { editable: false });
                }
            },
            beforeAddRowFunc: function () {
                $("#MainGrid").setColProp('Cd', { editable: true });
            },
            afterSaveCellFunc: function (rowid) {
            },
            beforeDelRowFunc: function (rowid) {

                if(_upri!=="G"){
                    var cmp = getGridVal($MainGrid, rowid, "Cmp", null)||'';
                    if (cmp !== _cmp && cmp!=="") {
                        alert("Non group accounts are not allowed to delete other location");
                        return false;
                    }
                }
                return true;
            },
        });
    });
}