var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
$(function(){

    _handler.saveUrl = rootPath + "BsdistManage/UpdateData";
    _handler.inquiryUrl = rootPath + "";
    _handler.config = [];

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
    }


    MenuBarFuncArr.EndFunc = function(){

    }

    _handler.saveData = function (dtd) {
        var changeData = getChangeValue();//获取所有改变的值
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());

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
        ajaxHttp(rootPath + "BsdistManage/GetDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
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
    }

    _initUI(["MBApply", "MBApprove", "MBErrMsg", "MBSearch", "MBEdoc"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    $("#CntryCdLookup").v3Lookup({
        url: rootPath + "TPVCommon/GetCountryDataForLookup",
        gridFunc: function(map){
            var cntryCd = map.CntryCd,
                cntryNm = map.CntryNm;
            $("#CntryCd").val(cntryCd);
            $("#CntryNm").val(cntryNm);
        },
        lookUpConfig: LookUpConfig.CntyCdLookup
    });

    $("#CntryCd").v3AutoComplete({
        params: "dt=country&CNTRY_CD=",
        returnValue: "CNTRY_CD=showValue,CNTRY_CD,CNTRY_NM",
        callBack: function(event, ui){
            $("input[name='CntryNm']").val(ui.item.returnValue.CNTRY_NM);
            $(this).val(ui.item.returnValue.CNTRY_CD);
            return false;
        },
        clearFunc: function(){
            $("#CntryCd").val("");
            $("#CntryNm").val("");
        }
    });

    $("#CityCdLookup").v3Lookup({
        url: rootPath + "TPVCommon/GetBstportDataForLookup",
        gridFunc: function(map){
            var cd = map.PortCd,
                nm = map.PortNm;
            $("#CityCd").val(cd);
            $("#CityNm").val(nm);
        },
        baseConditionFunc: function(){
            var CntryCd = $("#CntryCd").val();
            if(CntryCd != "")
            {
                return "&sopt_CntryCd=eq&CntryCd=" + CntryCd;
            }
            else
            {
                return "";
            }
            
        },
        lookUpConfig: LookUpConfig.TruckPortCdLookup
    });

    $("#CityCd").v3AutoComplete({
        params: "dt=bstport",
        returnValue: "PORT_CD=showValue,PORT_CD,PORT_NM",
        keyinNum:1,
        callBack: function(event, ui){
            $("#CityNm").val(ui.item.returnValue.PORT_NM);
            $(this).val(ui.item.returnValue.PORT_CD);
            return false;
        },
        dymcFunc: function(){
            var CntryCd = $("#CntryCd").val();

            if(CntryCd != "")
            {
                return "&CNTRY_CD=" + CntryCd + "&PORT_CD~";
            }
            else
            {
                return "";
            }
        },
        clearFunc: function(){
            $("#CityCd").val("");
            $("#CityNm").val("");
        }
    });

    $("#StateCdLookup").v3Lookup({
        url: rootPath + "TPVCommon/GetStateDataForLookup",
        gridFunc: function(map){
            $("#StateCd").val(map.StateCd);
            $("#StateNm").val(map.StateNm);
        },
        baseConditionFunc: function(){
            var CntryCd = $("#CntryCd").val();
            if(CntryCd != "")
            {
                return "&sopt_CntryCd=eq&CntryCd=" + CntryCd;
            }
            else
            {
                return "";
            }
            
        },
        lookUpConfig: LookUpConfig.StateLookup
    });

    $("#StateCd").v3AutoComplete({
        params: "dt=state",
        returnValue: "STATE_CD&STATE_NM=showValue,STATE_CD,STATE_NM",
        keyinNum:1,
        callBack: function(event, ui){
            var map = ui.item.returnValue;
            $(this).val(ui.item.returnValue.STATE_CD);
            $("#StateNm").val(ui.item.returnValue.STATE_NM);
            return false;
        },
        dymcFunc: function(){
            var CntryCd = $("#CntryCd").val();

            if(CntryCd != "")
            {
                return "&CNTRY_CD=" + CntryCd + "&STATE_CD~";
            }
            else
            {
                return false;
            }
        },
        clearFunc: function(){
            $("#StateCd").val("");
            $("#StateNm").val("");
        }
    });

});