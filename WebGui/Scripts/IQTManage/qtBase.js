function _checkEdit() {
    if (!_handler.topData["SysEdit"]) {
        CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_PassInquiry", 500, "warning");
        return false;
    }

    if (_handler.topData["RqStatus"] === "N") {
        CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasBid", 500, "warning");
        return false;
    }

    if (_handler.topData["RqStatus"] === "C") {
        CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_IqEnd", 500, "warning");
        return false;
    }

    if (_handler.topData["RqStatus"] === "D") {
        CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasClosed", 500, "warning");
        return false;
    }

    if (_handler.topData["QuotType"] === "B") {
        CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasGetBid", 500, "warning");
        return false;
    }

    if (_handler.topData["QuotType"] === "F") {
        CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasWinBid", 500, "warning");
        return false;
    }

    if (_handler.topData["QuotType"] === "V") {
        CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasDiscd", 500, "warning");
        return false;
    }

    if (_handler.topData["QuotType"] === "T") {
        CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_Terminated", 500, "warning");
        return false;
    }
}

var _FContractHtml = '<div class="pure-u-sm-5-60"><label for= "LspCd" class= "control-label" >@Resources.Locale.L_AirSetup_LspCd</label></div >\
<div class="pure-u-sm-10-60 control-group">\
    <div class="input-group" >\
        <input type="text" class="form-control input-sm" dt="mt" id="LspCd" name="LspCd" fieldname="LspCd" />\
        <span class="input-group-btn">\
            <button class="btn btn-sm btn-default" type="button" id="LspCdLookup"><span class="glyphicon glyphicon-search"></span></button>\
        </span>\
    </div >\
</div >\
<div class="pure-u-sm-5-60"><input type = "text" class="form-control input-sm" dt = "mt" id = "LspNm" fieldname = "LspNm" name = "LspNm" /></div >\
<div class="pure-u-sm-10-60"><input type="text" class="form-control input-sm" dt="mt" id="LspNm1" fieldname="LspNm1" name="LspNm1" /></div>\
<div class="pure-u-sm-5-60 label-right"><label for= "Constract" class= "control-label" >@Resources.Locale.L_BSCSDataQuery_Contract</label ></div >\
<div class="pure-u-sm-10-60 control-group"><select class="form-control input-sm" dt = "mt" id = "Constract" name = "Constract" fieldname = "Constract" >\
<option value=""></option><option value="S">@Resources.Locale.L_Contract_Shipper</option><option value="F">@Resources.Locale.L_Contract_Forwarder</option></select></div>\
<div class="pure-u-sm-5-60 label-right"><label for="ServiceMode" class="control-label">@Resources.Locale.L_RQQuery_ServiceMode</label></div>\
<div class="pure-u-sm-4-60 control-group"><input type="text" class="form-control input-sm" dt="mt" id="LoadingFrom" name="LoadingFrom" fieldname="LoadingFrom" /></div>\
<div class="pure-u-sm-2-60 control-group" style="text-align:center"><label class="control-label">-</label></div>\
<div class="pure-u-sm-4-60 control-group"><input type="text" class="form-control input-sm" dt="mt" id="LoadingTo" name="LoadingTo" fieldname="LoadingTo" /></div>';

var _RContractHtml = '<div class="pure-u-sm-5-60"><label for= "LspCd" class= "control-label" >@Resources.Locale.L_AirSetup_LspCd</label></div >\
<div class="pure-u-sm-10-60 control-group">\
    <div class="input-group" >\
        <input type="text" class="form-control input-sm" dt="mt" id="LspCd" name="LspCd" fieldname="LspCd" />\
        <span class="input-group-btn">\
            <button class="btn btn-sm btn-default" type="button" id="LspCdLookup"><span class="glyphicon glyphicon-search"></span></button>\
        </span>\
    </div >\
</div >\
<div class="pure-u-sm-5-60"><input type = "text" class="form-control input-sm" dt = "mt" id = "LspNm" fieldname = "LspNm" name = "LspNm" /></div >\
<div class="pure-u-sm-10-60"><input type="text" class="form-control input-sm" dt="mt" id="LspNm1" fieldname="LspNm1" name="LspNm1" /></div>\
<div class="pure-u-sm-5-60 label-right"><label for= "ContactType" class= "control-label" >@Resources.Locale.L_RQQuery_ContactType</label ></div >\
<div class="pure-u-sm-10-60 control-group"><select class="form-control input-sm" dt = "mt" id = "ContractType" name = "ContractType" fieldname = "ContractType" ></select></div>\
<div class="pure-u-sm-5-60 label-right"><label for="ServiceMode" class="control-label">@Resources.Locale.L_RQQuery_ServiceMode</label></div>\
<div class="pure-u-sm-4-60 control-group"><input type="text" class="form-control input-sm" dt="mt" id="LoadingFrom" name="LoadingFrom" fieldname="LoadingFrom" /></div>\
<div class="pure-u-sm-2-60 control-group" style="text-align:center"><label class="control-label">-</label></div>\
<div class="pure-u-sm-4-60 control-group"><input type="text" class="form-control input-sm" dt="mt" id="LoadingTo" name="LoadingTo" fieldname="LoadingTo" /></div>';

function setContractHtml() {
    var tranmode = _handler.topData["TranMode"];
    var html = _FContractHtml;
    if (tranmode == "R")
        html = _RContractHtml;
    $("#ContractHtml").html(html);
    if (tranmode == "R")
        appendSelectOption($("#ContractType"), rcnOptions);
}

function intQtView() {
    _handler.saveUrl = rootPath + "IQTManage/SaveQTData";
    _handler.inquiryUrl = rootPath + "IQTManage/GetAirData";
    _handler.config = LookUpConfig.BookingLookup;

    _handler.afterEdit = function () {
        if (!isEmpty($("#QuotNo").val())) {
            $("#QuotNo").attr('disabled', true);
        }
        _handler.beforLoadView(); 
    }

  

    _handler.beforEdit = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
        if (_checkEdit() === false) return false;
       
    }

    _handler.addData = function () {
        //初始化新增数据
        var data = { TranMode: "A" };
        data[_handler.key] = uuid();
        setFieldValue([data]);
    }

    _handler.loadMainData = function (map) {
        if (!map || (!map[_handler.key] && !map["RfqNo"])) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + LookUpConfig.QTDataItemUrl, { uId: map.UId, LspCd: map.LspCd, RfqNo: map.RfqNo, loading: true },
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

 
    _handler.beforLoadView = function () {
        var requires = ["QuotNo"];
        var readonlys = ["RfqNo", "RfqDate", "Cur", "FreightTerm", "ServiceMode", "EffectFrom", "Incoterm", "EffectTo", "OutIn", "Status", "Remark", "PolCd", "PodCd", "LspCd", "LspNm", "QuotType", "LoadingFrom", "LoadingTo", "QuotDate", "RfqFrom", "RfqTo", "LspNm1", "ContractType"];
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('readonly', true);
            $("#" + readonlys[i]).parent().find("button").prop("disabled", true);
        }

        for (var i = 0; i < requires.length; i++) {
            $("#" + requires[i]).attr('required', true);
            $("[for=" + requires[i] + "]").css("color", "rgb(255, 0, 0)");
        }
    }
}

function loadQtView(type) {
    try{
        $("#EXCEL_UPLOAD_FROM").append('<input type="hidden" id="jobNo" name="jobNo"/>');
        $("#uploadExcelFile").find(".modal-title").text(_getLang("L_qtBase_Script_168", "Quotation upload(note:Old price will be covered )"));
        $("#uploadExcel").bootstrapFileInput();
        $("#EXCEL_UPLOAD_FROM").submit(function () {
             
            var id = _handler.topData[_handler.key];
            $("#jobNo").val(id);
            var postData = new FormData($(this)[0]);
            $.ajax({
                url: rootPath + "IQTManage/FileUpload",
                type: 'POST',
                data: postData,
                async: true,
                beforeSend: function () {
                    CommonFunc.ToogleLoading(true);
                },
                "complete": function (xmlHttpRequest, successMsg) {
                    CommonFunc.ToogleLoading(false);
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.ToogleLoading(false);
                },
                success: function (data) { 
                    var d= /CallBack\("[\s\S]*?"\)/.exec(data);
                    CommonFunc.ToogleLoading(false); 
                    var msg = "";
                    if (d.length > 0) {
                        msg = d[0];
                        if (msg&&msg.length > -12)
                            msg = msg.substr(10, msg.length - 12);
                    }
                    if (msg !== "Y") {
                        alert(_getLang("L_qtBase_Script_169", "Importing quotation failed") + msg);
                        return false;
                    }
                    alert(_getLang("L_QTSetup_IpIqS","Import quotation successfully"));
                    $("#uploadExcelFile").modal("hide");
                    MenuBarFuncArr.MBCancel();
                },
                cache: false,
                contentType: false,
                processData: false
            });

            return false;
        });

        $("#QT_EXCEL_UPLOAD_FROM").submit(function(){
                $("#u_id1").val($("#UId").val());
                $("#rfq_no1").val($("#RfqNo").val());
                $("#quot_no1").val($("#QuotNo").val());
                $("#lsp_cd").val($("#LspCd").val());
                var postData = new FormData($(this)[0]);
                CommonFunc.ToogleLoading(true);
                $.ajax({
                    url: rootPath + "IQTManage/QtFileUpload",
                    type: 'POST',
                    data: postData,
                    async: true,
                    beforeSend: function(){
                        CommonFunc.ToogleLoading(true);
                    },
                    error: function(){
                        CommonFunc.ToogleLoading(false);
                        CommonFunc.Notify("", "Error", 1300, "warning");
                    },
                    success: function (data) { 
                        CommonFunc.ToogleLoading(false);
                        if (data.message != "success") {
                            CommonFunc.Notify("", "汇入失败：" + data.message, 1300, "warning");
                            return false;
                        }
                        if(data.excelMsg == "")
                        {
                            CommonFunc.Notify("", "汇入成功", 500, "success");
                            _handler.loadGridData("MainGrid", $("#MainGrid")[0], [], [""]);
                            if (data.subData)
                                _handler.loadGridData("MainGrid", $("#MainGrid")[0], data.subData, [""]);
                        }
                        else
                        {
                            CommonFunc.Notify("", data.excelMsg, 500, "warning");
                        }
                        
                        $("#ExcelWindow").modal("hide");
                    },
                    cache: false,
                    contentType: false,
                    processData: false
                });

                return false;
            });
    }
    catch(e){}
     

    MenuBarFuncArr.AddMenu("IQQT", "glyphicon glyphicon-envelope", "Enquiry", function () {
        var uid = $("#UId").val();
        if (uid == "") {
            alert("Please Choose Data To Send!");
            return false;
        }
        if ($("#QuotType").val() != "P" && $("#QuotType").val() != "I") {
            alert("This operation cannot be performed in this state !");
            return false;
        }
        var confirmReload = window.confirm("Are you sure to send Mail？");
        if (!confirmReload) return;
        var uid = $("#UId").val();
        var lspcd = $("#LspCd").val();
        $.ajax({
            async: true,
            url: rootPath + "IQTManage/SendMail",
            type: 'POST',
            data: { "uid": uid, "LspCd": lspcd },
            "error": function (xmlHttpRequest, errMsg) {
                var resJson = $.parseJSON(errMsg)
                CommonFunc.Notify("", resJson.errMsg, 500, "warning");
            },
            success: function (data) {
                if (!data.IsOk) {
                    alert(data.message);
                    return false;
                }
                else {
                    CommonFunc.Notify("", "success", 500, "success");
                }
            }
        });
    });

    if (_Op !== "N" && type !== 1) {
        MenuBarFuncArr.AddMenu("QuotTypeBtn", "glyphicon glyphicon-ok", _getLang("L_QTSetup_IqBid", "Submit quotation / tender"), function () {
            if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
                CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
                return false;
            }
            if (_checkEdit() === false) return false;
            var truthBeTold = window.confirm(_getLang("L_QTSetup_BidConf", "Please confirm whether to submit the quotation."));
            if (!truthBeTold) {
                return;
            }

            try {
                var containerArray = $MainGrid.getDataIDs();
                if (containerArray.length <= 0) {
                    alert(_getLang("L_qtBase_Script_170", "Quotation information can\'t be null,please check"));
                    return;
                }
            }
            catch (e) { }

            CommonFunc.ToogleLoading(true);
            $.ajax({
                async: true,
                url: rootPath + "IRQManage/SubmitQT",
                type: 'POST',
                dataType: "json",
                data: {
                    "uId": _handler.topData["UId"]
                },
                "complete": function (xmlHttpRequest, successMsg) {
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.ToogleLoading(false);
                },
                success: function (result) {
                    alert(result.message); 
                    if (result.flag) {
                        MenuBarFuncArr.MBCancel();
                    }
                    else
                        CommonFunc.ToogleLoading(false);
                }
            });
        }); 

        MenuBarFuncArr.AddMenu("VoidBtn", "glyphicon glyphicon-ban-circle", _getLang("L_MenuBar_Audit", "Termination"), function () {
            if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
                CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
                return false;
            }
            if (_checkEdit(1) === false) return false;
            var truthBeTold = window.confirm(_getLang("L_qtBase_Script_171", "cancel this offer or not"));
            if (!truthBeTold) {
                return;
            }
            CommonFunc.ToogleLoading(true);
            $.ajax({
                async: true,
                url: rootPath + "RQManage/VoidQT",
                type: 'POST',
                dataType: "json",
                data: {
                    "uId": _handler.topData["UId"]
                },
                "complete": function (xmlHttpRequest, successMsg) {
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.ToogleLoading(false);
                },
                success: function (result) {
                    alert(result.message); 
                    if (result.flag) {
                        MenuBarFuncArr.MBCancel();
                    }
                    else
                        CommonFunc.ToogleLoading(false);
                }
            });
        });
    }

    if (type !== 1 && type !== 2) {
        MenuBarFuncArr.AddMenu("EXCEL_BTN", "glyphicon glyphicon-upload", _getLang("L_qtBase_Script_173", "Excel file quotation imported"), function () {
            if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
                CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
                return false;
            }
            if (_checkEdit() === false) return false;
            var id = _handler.topData[_handler.key]; 
            $("#uploadExcelFile").modal("show");
        });
    }

    
    MenuBarFuncArr.AddMenu("BACK_BTN", "glyphicon glyphicon-log-out", _getLang("L_qtBase_Scripts_309", "Return back"), function () {
        if (_Op === "N")
            setNavTabActive({ id: "RQSetup", href: rootPath + "RQManage/RQSetup/" + _RQUid, title: _getLang('L_RQQuery_EntInquiry','quotaiton input'), search: 'uid=' + _RQUid });
        else {
            setNavTabActive({ id: "QT010", href: rootPath + "IQTManage/QTQuery", title: _getLang('L_qtBase_Scripts_310','Quotation inquiry') });
        }
    });

    if (type === 1) {
        _checkEdit = function () { }
        _initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    }
    else if (type === 2) {
        _initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    }
    else
        _initUI(["MBApply", "MBInvalid", "MBCopy", "MBApprove", "MBErrMsg", "MBAdd", "MBDel", "MBSearch"]);//初始化UI工具栏
    try {
        if (_Op === "N") {
            if (type === 1)
                MenuBarFuncArr.DelMenu(["MBEdit", "EXCEL_BTN", "MBCopy", "MBSave", "MBCancel", "MBAdd", "MBDel"]);
            else
                MenuBarFuncArr.DelMenu(["MBEdit", "EXCEL_BTN", "MBSave", "MBCancel", "MBDel"]);
        }
    }
    catch (e) { }
    if (!isEmpty(_uid) || !isEmpty(_RfqNo)) {
        _handler.topData = { UId: _uid, LspCd: _LspCd, RfqNo: _RfqNo };
        MenuBarFuncArr.MBCancel();
    }

    if (typeof (isa) != 'undefined' && isa == "Y") {
        MenuBarFuncArr.DelMenu(["MBEdit", "EXCEL_BTN", "MBCopy", "MBSave", "MBCancel", "MBAdd", "MBDel", "VoidBtn", "QuotTypeBtn", "MBPreview"]);
        btnGroup();
    }
}

var _tran = "", _bu = "P";
var rcnOptions = [];
var acnOptions = [];
function getSelectOptions() {
    $.ajax({
        async: false,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("FCLBooking") },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var _shownull = { cd: '', cdDescp: '' };
            var trnOptions = data.TTRN || [];
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];
            appendSelectOption($("#TranMode"), trnOptions);

            var pkOptions = data.PK || [];
            appendSelectOption($("#ServiceMode"), pkOptions);
            appendSelectOption($("#LoadingFrom"), pkOptions);
            appendSelectOption($("#LoadingTo"), pkOptions);

            rcnOptions = data.RCN || [];
            rcnOptions.unshift(_shownull);
            acnOptions = data.ACN || [];
            acnOptions.unshift(_shownull);
            appendSelectOption($("#ContractType"), acnOptions);
            if (_handler.topData) {
                $("#ServiceMode").val(_handler.topData["ServiceMode"]);
                $("#LoadingFrom").val(_handler.topData["LoadingFrom"]);
                $("#LoadingTo").val(_handler.topData["LoadingTo"]);
            }
        }
    });
}

function setDefutltGridData(grid, rowid, lookUps) {
    lookUps = lookUps || {};

    if (lookUps["Cur"])
        setGridVal(grid, rowid, "Cur", $("#Cur").val() || _handler.topData["Cur"], "lookup");
    else
        setGridVal(grid, rowid, "Cur", $("#Cur").val() || _handler.topData["Cur"]);
    if (lookUps["PolCd"])
        setGridVal(grid, rowid, "PolCd", $("#PolCd").val() || _handler.topData["PolCd"], "lookup");
    else
        setGridVal(grid, rowid, "PolCd", $("#PolCd").val() || _handler.topData["PolCd"]);
    if (lookUps["PodCd"])
        setGridVal(grid, rowid, "PodCd", $("#PodCd").val() || _handler.topData["PodCd"], "lookup");
    else
        setGridVal(grid, rowid, "PodCd", $("#PodCd").val() || _handler.topData["PodCd"]);
    if (lookUps["TranMode"])
        setGridVal(grid, rowid, "TranMode", $("#TranMode").val() || _handler.topData["TranMode"], "lookup");
    else
        setGridVal(grid, rowid, "TranMode", $("#TranMode").val() || _handler.topData["TranMode"]);
    if (lookUps["Incoterm"])
        setGridVal(grid, rowid, "Incoterm", $("#Incoterm").val() || _handler.topData["Incoterm"], "lookup");
    else
        setGridVal(grid, rowid, "Incoterm", $("#Incoterm").val() || _handler.topData["Incoterm"]);
   
    setGridVal(grid, rowid, "RfqNo", $("#RfqNo").val() || _handler.topData["RfqNo"]);
    setGridVal(grid, rowid, "AllIn", "A");
    setGridVal(grid, rowid, "PolNm", $("#PolNm").val() || _handler.topData["PolNm"]);
    setGridVal(grid, rowid, "PodNm", $("#PodNm").val() || _handler.topData["PodNm"]);
    setGridVal(grid, rowid, "LspCd", $("#LspCd").val() || _handler.topData["LspCd"]);
    if (lookUps["LoadingFrom"])
        setGridVal(grid, rowid, "LoadingFrom", $("#LoadingFrom").val() || _handler.topData["LoadingFrom"], "lookup");
    else
        setGridVal(grid, rowid, "LoadingFrom", $("#LoadingFrom").val() || _handler.topData["LoadingFrom"]);
    if (lookUps["LoadingTo"])
        setGridVal(grid, rowid, "LoadingTo", $("#LoadingTo").val() || _handler.topData["LoadingTo"], "lookup");
    else
        setGridVal(grid, rowid, "LoadingTo", $("#LoadingTo").val() || _handler.topData["LoadingTo"]);
    setGridVal(grid, rowid, "QuotNo", $("#QuotNo").val() || _handler.topData["QuotNo"]);
}

function setDefutltGridData1(grid, rowid) {
    setGridVal(grid, rowid, "RfqNo",$("#RfqNo").val()|| _handler.topData["RfqNo"]);
    setGridVal(grid, rowid, "Cur", $("#Cur").val() || _handler.topData["Cur"]);
    setGridVal(grid, rowid, "AllIn", "A");
    setGridVal(grid, rowid, "PolCd", $("#PolCd").val() || _handler.topData["PolCd"], "lookup");
    setGridVal(grid, rowid, "PolNm", $("#PolNm").val() || _handler.topData["PolNm"]);
    setGridVal(grid, rowid, "PodCd", $("#PodCd").val() || _handler.topData["PodCd"], "lookup");
    setGridVal(grid, rowid, "PodNm", $("#PodNm").val() || _handler.topData["PodNm"]);
    setGridVal(grid, rowid, "Incoterm", $("#Incoterm").val() || _handler.topData["Incoterm"]);
    setGridVal(grid, rowid, "TranMode", $("#TranMode").val() || _handler.topData["TranMode"], "lookup");
    setGridVal(grid, rowid, "LspCd", $("#LspCd").val() || _handler.topData["LspCd"]);
    setGridVal(grid, rowid, "QuotNo", $("#QuotNo").val() || _handler.topData["QuotNo"]);
}


function CallBack(returnMessage) {
    if (returnMessage === "Y") {
        CommonFunc.Notify("", _getLang("L_qtBase_Scripts_311", "Import succesfully"), 500, "warning");
        $("#ExcelWindow").modal("hide");
    }
    else {
        CommonFunc.Notify("", returnMessage, 500, "warning");
        $("#ExcelWindow").modal("hide");
    }
    MenuBarFuncArr.MBCancel();
}

function setRQData(data) {
    if (_handler.topData["Period"] === "R")
        $("#RFQ_INFO").show();
    else
        $("#RFQ_INFO").hide();

    $("[fieldname1]").each(function (index, obj) {
        var jq = $(obj);
        jq.prop("disabled", true);
        var fieldName = jq.attr("fieldname1");
        jq.val("");
        if (data["rq"] && data["rq"].length > 0) {
            if (data["rq"][0][fieldName])
                jq.val(data["rq"][0][fieldName] || "");
        }
    });

}

function dynamicHeight()
{
    var docHeight = $(window).height();
    var addAreaHeight = $("#AddArea").height();
    var gridHeight = docHeight - addAreaHeight;
    if(gridHeight > 400)
    {
        gridHeight = gridHeight - 200;
    }
    else
    {
        gridHeight = 200;
    }
    $("#MainGrid").parents('div.ui-jqgrid-bdiv').css("height",gridHeight+"px");
}


var GetChgAuto1 = function (groupId, tran_mode, $grid, autoFn,autoCompGetValueFunc, dymcFunc, clearFn) {
    var op =
    {
        autoCompDt: "dt=chg&GROUP_ID=" + groupId + "&CMP="+ basecmp +"&CHG_CD=",//TRAN_MODE
        autoCompParams: "CHG_CD&CHG_DESCP=showValue,CHG_CD,CHG_DESCP,REPAY,CHG_TYPE",
        autoCompFunc: function (elem, event, ui, rowid) {
            autoFn($grid, ui.item.returnValue, elem, rowid);
        },
        autoClearFunc: function (elem, event, rowid) {
            if(typeof clearFn == "function")
                clearFn($grid, elem, rowid);
        },
        autoCompGetValueFunc:autoCompGetValueFunc,
        dymcFunc: dymcFunc
    };
    return op;
}

function getNowDate() {
    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: true,
        url: rootPath + "IQTManage/GetNowDate",
        type: 'POST',
        data: {},
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.ToogleLoading(false);
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
        },
        success: function (result) {
            $("#QuotDateL").val(result.nowDate);
        }
    });
}

function SetColModel(Option, colmodel, index1) {
    Option = stringToJson(Option);
    $.each(Option, function (index, colname) {
        index1 = index1++;
        var col = { name: colname.cd, title: colname.cdDescp, index: colname.cd, width: 60, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true };
        colmodel.splice(index1, 0, col);
    });
    return colmodel;
}

function stringToJson(obj) {
    if (obj == "" || obj == null || obj == undefined) return [];
    var val;
    try {
        return JSON.parse(decodeHtml(obj));
    }
    catch (e) {
        return [];
    }
}

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}