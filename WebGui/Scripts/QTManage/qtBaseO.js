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
}

function intQtView() {
    _handler.saveUrl = rootPath + "QTManage/SaveQTData";
    _handler.inquiryUrl = rootPath + "QTManage/GetAirData";
    _handler.config = LookUpConfig.BookingLookup;

    _handler.afterEdit = function () {
        if (!isEmpty($("#QuotNo").val())) {
            $("#QuotNo").attr('disabled', true);
        }
        _handler.beforLoadView();
        //if (!isEmpty($("#QuotDate").val())) {
        //    $("#QuotDate").attr('disabled', true);
        //    $("#QuotDate").parent().find("button").attr('disabled', true);
        //}
        //else {
        //    $("#QuotDate").val(getDate());
        //}
        //if ($("#QuotType").val() === "Q") {
        //    $("#QuotType").val("N");
        //    $("#QuotDate").val(getDate());
        //}
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
        //var requires = ["RfqNo", "RfqFrom", "RfqTo", "EffectFrom", "EffectTo", "ServiceMode", "RfqDate", "Incoterm", "Rlocation", "PolCd", "PodCd"];
        //$("#RfqNo").attr('isKey', true);
        //$("#RfqNo").attr('disabled', true);
        var requires = ["QuotNo"];
        var readonlys = ["RfqNo", "RfqDate", "Cur", "FreightTerm", "ServiceMode", "EffectFrom", "Incoterm", "EffectTo", "OutIn", "Status", "Remark", "PolCd", "PodCd", "LspCd", "LspNm", "QuotType", "LoadingFrom", "LoadingTo", "QuotDate", "RfqFrom", "RfqTo"];
        //for (var i = 0; i < disableds.length; i++) {
        //    $("#" + disableds[i]).attr('disabled', true);
        //    $("#" + disableds[i]).attr('isKey', true);
        //}
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
        $("#uploadExcelFile").find(".modal-title").text("@Resources.Locale.L_qtBase_Script_168");
        $("#uploadExcel").bootstrapFileInput();
        $("#EXCEL_UPLOAD_FROM").submit(function () {
            var id = _handler.topData[_handler.key];
            $("#jobNo").val(id);
            var postData = new FormData($(this)[0]);
            $.ajax({
                url: rootPath + "QTManage/FileUpload",
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
                    //alert(data)
                    var d= /CallBack\("[\s\S]*?"\)/.exec(data);
                    CommonFunc.ToogleLoading(false);
                    //if (data.message != "success") {
                    //    CommonFunc.Notify("", "汇入失败" + data.message, 1300, "warning");
                    //    return false;
                    //}
                    var msg = "";
                    if (d.length > 0) {
                        msg = d[0];
                        if (msg&&msg.length > -12)
                            msg = msg.substr(10, msg.length - 12);
                    }
                    if (msg !== "Y") {
                        alert("@Resources.Locale.L_qtBase_Script_169" + msg);
                        return false;
                    }
                    alert("@Resources.Locale.L_QTSetup_IpIqS");
                    $("#uploadExcelFile").modal("hide");
                    MenuBarFuncArr.MBCancel();
                },
                cache: false,
                contentType: false,
                processData: false
            });

            return false;
        });
    }
    catch(e){}
    
    MenuBarFuncArr.MBEdoc = function (thisItem) {
        initEdoc(thisItem, { jobNo: _uid, GROUP_ID: groupId, CMP: cmp, STN: "*" });
    }

    if (_Op !== "N" && type !== 1) {
        MenuBarFuncArr.AddMenu("QuotTypeBtn", "glyphicon", "@Resources.Locale.L_QTSetup_IqBid", function () {
            if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
                CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
                return false;
            }
            if (_checkEdit() === false) return false;
            var truthBeTold = window.confirm("@Resources.Locale.L_QTSetup_BidConf");
            if (!truthBeTold) {
                return;
            }

            try {
                var containerArray = $MainGrid.getDataIDs();
                if (containerArray.length <= 0) {
                    alert("@Resources.Locale.L_qtBase_Script_170");
                    return;
                }
            }
            catch (e) { }

            CommonFunc.ToogleLoading(true);
            if ($("#QuotType").val() === "Q") {
                $("#QuotType").val("N");
                $("#QuotDate").val(getDate());
            }
            $.ajax({
                async: true,
                url: rootPath + "RQManage/SubmitQT",
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
                    //CommonFunc.Notify("", result.message, 3000, "success");
                    if (result.flag) {
                        MenuBarFuncArr.MBCancel();
                    }
                    else
                        CommonFunc.ToogleLoading(false);
                }
            });
        });

        MenuBarFuncArr.AddMenu("VoidBtn", "glyphicon", "@Resources.Locale.L_MenuBar_Audit", function () {
            if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
                CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
                return false;
            }
            if (_checkEdit(1) === false) return false;
            var truthBeTold = window.confirm("@Resources.Locale.L_qtBase_Script_171");
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
                    //CommonFunc.Notify("", result.message, 3000, "success");
                    if (result.flag) {
                        MenuBarFuncArr.MBCancel();
                    }
                    else
                        CommonFunc.ToogleLoading(false);
                }
            });
        });

        //if (type == 2) {
        //    //MenuBarFuncArr.AddMenu("ApproveBtn", "glyphicon", "@Resources.Locale.L_InvCheck_Views_214", function () {
        //    //    if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
        //    //        CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
        //    //        return false;
        //    //    }
        //    //    if (_checkEdit(2) === false) return false;
        //    //    var truthBeTold = window.confirm("@Resources.Locale.L_qtBase_Script_172");
        //    //    if (!truthBeTold) {
        //    //        return;
        //    //    }
        //    //    CommonFunc.ToogleLoading(true);
        //    //    $.ajax({
        //    //        async: true,
        //    //        url: rootPath + "RQManage/ApproveQT",
        //    //        type: 'POST',
        //    //        dataType: "json",
        //    //        data: {
        //    //            "uId": _handler.topData["UId"]
        //    //        },
        //    //        "complete": function (xmlHttpRequest, successMsg) {
                       
        //    //        },
        //    //        "error": function (xmlHttpRequest, errMsg) {
        //    //            CommonFunc.ToogleLoading(false);
        //    //        },
        //    //        success: function (result) {
        //    //            alert(result.message);
        //    //            //CommonFunc.Notify("", result.message, 3000, "success");
        //    //            if (result.flag) {
        //    //                MenuBarFuncArr.MBCancel();
        //    //            }
        //    //            else
        //    //                CommonFunc.ToogleLoading(false);
        //    //        }
        //    //    });
        //    //});

        //    MenuBarFuncArr.AddMenu("ApproveBtn", "glyphicon", "@Resources.Locale.L_ContractQuery_Views_478", function () {
        //        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
        //            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
        //            return false;
        //        }
        //        //if (_checkEdit(2) === false) return false;
        //        //var truthBeTold = window.confirm("@Resources.Locale.L_qtBase_Script_172");
        //        //if (!truthBeTold) {
        //        //    return;
        //        //}
        //        CommonFunc.ToogleLoading(true);
        //        var UId = $("#UId").val();
        //        if (UId) {
        //            $.ajax({
        //                async: true,
        //                url: rootPath + "RQManage/InitiatedCheck",
        //                type: 'POST',
        //                data: { UId: UId },
        //                dataType: "json",
        //                "error": function (xmlHttpRequest, errMsg) {
        //                    CommonFunc.Notify("", "@Resources.Locale.L_ActCheckSetup_Scripts_29", 500, "warning");
        //                    CommonFunc.ToogleLoading(false);
        //                },
        //                success: function (result) {
        //                    //if (!result.flag)
        //                    CommonFunc.Notify("", result.message, 500, "warning");
        //                }
        //            });
        //        }
        //    });
            
        //}
    }

    if (type !== 1 && type !== 2) {
        MenuBarFuncArr.AddMenu("EXCEL_BTN", "glyphicon glyphicon-bell", "@Resources.Locale.L_qtBase_Script_173", function () {
            if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
                CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
                return false;
            }
            if (_checkEdit() === false) return false;
            var id = _handler.topData[_handler.key];
            //$("#FileUploadIframe").contents().find("#jobNo").val(id);
            //$("#ExcelWindow").modal("show");
            $("#uploadExcelFile").modal("show");
            
        });
    }

    
    MenuBarFuncArr.AddMenu("BACK_BTN", "glyphicon glyphicon-log-out", "@Resources.Locale.L_qtBase_Scripts_309", function () {
        if (_Op === "N")
            setNavTabActive({ id: "RQSetup", href: rootPath + "RQManage/RQSetup/" + _RQUid, title: '@Resources.Locale.L_RQQuery_EntInquiry', search: 'uid=' + _RQUid });
        else {
            /*var tranmode =  _handler.topData["TranMode"];
            var _url = "RQSetup";
            if (tranmode == "A") {  //空运
                _url = "AirSetup";
            } else if (tranmode == "F") {   //海运整柜
                if (_handler.topData.Period === "B")
                    _url = "FCLFSFSetup";
                else
                    _url = "FCLFSetup";
            } else if (tranmode === "D") {   //国内快递  D
                _url = "DESetup";
            } else if (tranmode === "E") {   //国际快递
                _url = "IESetup";
            } else if (tranmode === "L") {   //海运散货
                _url = "LCLSetup";
            } else if (tranmode === "T") {   //国内运输 T
                _url = "DTSetup";
            }*/
            setNavTabActive({ id: "QT010", href: rootPath + "QTManage/QTQuery", title: '@Resources.Locale.L_qtBase_Scripts_310' });
        }
    });

    if (type === 1) {
        _checkEdit = function () { }
        //_initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
        _initUI(["MBApply", "MBInvalid", "MBErrMsg"]);//初始化UI工具栏
    }
    else if (type === 2) {
        //_initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
        _initUI(["MBApply", "MBInvalid", "MBErrMsg"]);//初始化UI工具栏
    }
    else
        //_initUI(["MBApply", "MBInvalid", "MBCopy", "MBApprove", "MBErrMsg", "MBAdd", "MBDel", "MBSearch"]);//初始化UI工具栏
        _initUI(["MBApply", "MBInvalid", "MBCopy", "MBErrMsg", "MBAdd", "MBDel", "MBSearch"]);//初始化UI工具栏
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
    if (isa == "Y") {
        MenuBarFuncArr.DelMenu(["MBEdit", "EXCEL_BTN", "MBCopy", "MBSave", "MBCancel", "MBAdd", "MBDel", "VoidBtn", "QuotTypeBtn", "MBPreview"]);
        btnGroup();
    }
}

var _tran = "", _bu = "P";
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
            var trnOptions = data.TTRN || [];
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];
            appendSelectOption($("#TranMode"), trnOptions);

            var pkOptions = data.PK || [];
            appendSelectOption($("#ServiceMode"), pkOptions);
            appendSelectOption($("#LoadingFrom"), pkOptions);
            appendSelectOption($("#LoadingTo"), pkOptions);

            if (_handler.topData) {
                $("#ServiceMode").val(_handler.topData["ServiceMode"]);
                $("#LoadingFrom").val(_handler.topData["LoadingFrom"]);
                $("#LoadingTo").val(_handler.topData["LoadingTo"]);
            }
        }
    });
}

//{ name: 'LspCd', title: 'LspCd', index: 'LspCd', sorttype: 'string', width: 100, editable: true, hidden: true },
//{ name: 'RfqNo', title: 'RfqNo', index: 'RfqNo', sorttype: 'string', width: 100, editable: true, hidden: true },
//{ name: 'Cur', title: 'Cur', index: 'Cur', sorttype: 'string', width: 100, editable: true, hidden: true },
//{ name: 'AllIn', title: 'AllIn', index: 'AllIn', sorttype: 'string', width: 100, editable: true, hidden: true },
//{ name: 'PolCd', title: 'PolCd', index: 'PolCd', sorttype: 'string', width: 100, editable: true, hidden: true },
//{ name: 'PolNm', title: 'PolNm', index: 'PolNm', sorttype: 'string', width: 100, editable: true, hidden: true },
//{ name: 'PodCd', title: 'PodCd', index: 'PodCd', sorttype: 'string', width: 100, editable: true, hidden: true },
//{ name: 'PodNm', title: 'PodNm', index: 'PodNm', sorttype: 'string', width: 100, editable: true, hidden: true },
//{ name: 'Incoterm', title: 'Incoterm', index: 'Incoterm', sorttype: 'string', width: 100, editable: true, hidden: true },
//{ name: 'TranMode', title: 'TranMode', index: 'TranMode', sorttype: 'string', width: 100, editable: true, hidden: true },
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
    //if (lookUps["ServiceMode"])
    //    setGridVal(grid, rowid, "ServiceMode", _handler.topData["ServiceMode"], "lookup");
    //else
    //    setGridVal(grid, rowid, "ServiceMode", _handler.topData["ServiceMode"]);
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
        CommonFunc.Notify("", "@Resources.Locale.L_qtBase_Scripts_311", 500, "warning");
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