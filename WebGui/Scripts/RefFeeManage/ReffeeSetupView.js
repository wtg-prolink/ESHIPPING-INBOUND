var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
$(function(){

    _handler.saveUrl = rootPath + "RefFeeManage/UpdateData";
    _handler.inquiryUrl = rootPath + "";
    _handler.config = [];
    var ChgCd = "";
    _handler.beforDel = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
    }

    _handler.addData = function () {
        //初始化新增数据
        var data = {};
        data[_handler.key] = uuid();
        setFieldValue([data]);
        $("#FeeType").val("O");
        //alert(ChgCd);
        $("#ChgCd").val(ChgCd);
    }

    _handler.beforSave = function () {
        var ChgDescp = $("#ChgDescp").val();
        var Gw = $("#Gw").val();
        var Cbm = $("#Cbm").val();

        if(ChgDescp != "" && Gw != "")
        {
            alert("噸數描數、重量、CBM請擇一輸入");
            return false;
        }

        if(ChgDescp != "" && Cbm != "")
        {
            alert("噸數描數、重量、CBM請擇一輸入");
            return false;
        }

        if(Gw != "" && Cbm != "")
        {
            alert("噸數描數、重量、CBM請擇一輸入");
            return false;
        }

        return true;
    }

    _handler.saveData = function (dtd) {
        var changeData = getChangeValue();//获取所有改变的值
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        data["FeeType"] = $("#FeeType").val();
        //data["rfq_no"] = encodeURIComponent($("#RfqNo").val());
        ajaxHttpSaveBar(dtd, _handler.saveUrl, data,
            function (result) {
                if (result.message) {
                    alert(result.message);
                    return false;
                }
                else if (_handler.setFormData)
                    _handler.setFormData(result);
                return true;
            });
    }

    _handler.beforEdit = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
    }

    _handler.beforAdd = function () {//新增前设定
        ChgCd = befordata();
        if (ChgCd === "")
            return false;
        //$("#ChgCd").val(ChgCd);
        return true;
    }

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];

        setFieldValue(data["main"] || [{}]);
        _handler.beforLoadView();
        setdisabled(true);
        setToolBtnDisabled(true);
        MenuBarFuncArr.Enabled(["MBCopy"]);
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "RefFeeManage/GetDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
        });
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        var data = {};
        data[_handler.key] = uuid();       
        var dataRow, addData = [];
        $("#ModifyBy").val("");
        $("#ModifyDate").val("");
        $("#FeeType").val("O");
        var ChgCd = befordata();
        if (ChgCd === "")
            return false;
        $("#ChgCd").val(ChgCd);
        return true;
    }
    //befordata(ChgCd);
    _initUI(["MBApply", "MBApprove", "MBErrMsg", "MBSearch", "MBEdoc"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
});

function befordata() {
    var ChgCd = "";
    $.ajax({
        async: false,
        url: rootPath + "RefFeeManage/getChgCd",
        type: 'POST',
        data: { FeeType:'O'},
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.Notify("", errMsg, 500, "danger");
            if (errorFn) errorFn();
        },
        success: function (result) {
            if (result.message == "success") {
                ChgCd = result.ChgCd;
            }
            else {
                CommonFunc.Notify("", result.message, 500, "warning");
            }
        }
    });
    return ChgCd;
}

