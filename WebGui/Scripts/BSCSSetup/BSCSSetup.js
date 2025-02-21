var _dm = new dm();
var url = "";
var groupId = getCookie("plv3.passport.groupid");
//var cmp = getCookie["plv3.passport.basecompanyid"];
//var stn = getCookie["plv3.passport.basestation"]; 
//var dep = getCookie["plv3.passport.dep"];
var userId = getCookie("plv3.passport.user");
var UId = "";
function initLoadData(UId) {
    if (!UId)
        return;
    //getSMPTYCGridData();

    //getSMPTYSGridData();

    //getSMPTYDGridData();

    var param = "sopt_UId=eq&UId=" + UId;
    //将获取的数据作为条件进行reload数据
    $.ajax({
        async: true,
        url: rootPath + "/System/BSCSDetailQuery?UId="+UId,
        type: 'POST',
        data: {
            model: "SmptyModel",
            sidx: 'UId',
            'conditions': encodeURI(param),
            page: 1,
            rows: 20
        },
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
            if (successMsg != "success") return null;
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            var maindata = jQuery.parseJSON(result.mainTable.Content);
            _dataSource = maindata.rows;
            setFieldValue(maindata.rows);
            var resultSmptyc = jQuery.parseJSON(result.resultSmptyc.Content);
            var resultSmptys = jQuery.parseJSON(result.resultSmptys.Content);
            var resultSmptyd = jQuery.parseJSON(result.resultSmptyd.Content);
            var resultTkcsts = jQuery.parseJSON(result.resultTkcsts.Content);
            var resultSmbkInfo = jQuery.parseJSON(result.resultSmbkInfo.Content);
            _dm.getDs("SMPTYCGrid").setData(resultSmptyc.rows);
            $('#SMPTYCGrid').jqGrid("setGridParam", { data: resultSmptyc.rows });

            _dm.getDs("SMPTYSGrid").setData(resultSmptys.rows);
            $('#SMPTYSGrid').jqGrid("setGridParam", { data: resultSmptys.rows });

            _dm.getDs("SMPTYDGrid").setData(resultSmptyd.rows);
            $('#SMPTYDGrid').jqGrid("setGridParam", { data: resultSmptyd.rows });

            _dm.getDs("TKCSTSGrid").setData(resultTkcsts.rows);
            $('#TKCSTSGrid').jqGrid("setGridParam", { data: resultTkcsts.rows });

            _dm.getDs("SMBKINFOGrid").setData(resultSmbkInfo.rows);
            $('#SMBKINFOGrid').jqGrid("setGridParam", { data: resultSmbkInfo.rows });

            if(maindata.rows[0].Pic != "" && maindata.rows[0].Pic != null)
            {
                var imgPath = rootPath + "System/Image/" + maindata.rows[0].UId;
                var link = '<img src="'+imgPath+'" alt="" class="img-thumbnail" height="300" />';
                $("#picInfo").html(link);
                
            }

            setdisabled(true);
            setToolBtnDisabled(true);
            MenuBarFuncArr.Disabled(["MBSave", "MBSearch"]);
            MenuBarFuncArr.Enabled(["MBAdd", "MBCopy", "MBDel", "MBEdit", "MBApprove", "MBEdoc"]);
            MenuBarFuncArr.initEdoc($("#UId").val(), groupId, cmp, "*");
        }
    });
}
function fill() {
    var partyNo = $("#PartyNo").val();
    if (!$("#HeadOffice").val()) $("#HeadOffice").val(partyNo);
    if (!$("#BillTo").val()) $("#BillTo").val(partyNo);
}
function selectPartyTypeOption(selectId, options) {
    selectId.empty();
    $.each(options, function (idx, option) {

        selectId.append("<option value=\"" + option.cd + "\">" + option.cdDescp + "</option>");

    });
}

$(document).ready(function () {
    ChangeLabelVal();
    schemas = JSON.parse(decodeHtml(schemas));
    CommonFunc.initField(schemas);
    UId = $("#UId").val();
    url = rootPath + "/System/BSCSQuery";

    getSMPTYCGridData();

    getSMPTYSGridData();

    getSMPTYDGridData();

    getTKCSTSGridData();

    getSMBKINFOGridData();

    MenuBarFuncArr.DelMenu(["MBApply", "MBInvalid"]);

    MenuBarFuncArr.MBCancel = function () {
        var UId = $("#UId").val();
        if (!UId) return;
        var param = "sopt_UId=eq&UId=" + UId;
        //将获取的数据作为条件进行reload数据
        $.ajax({
            async: true,
            url: rootPath + "/System/BSCSQuery",
            type: 'POST',
            data: {
                model: "SmptyModel",
                sidx: 'UId',
                'conditions': encodeURI(param),
                page: 1,
                rows: 20
            },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
                if (successMsg != "success") return null;
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                _dataSource = result.rows;
                setFieldValue(result.rows);
                var UserStn = result.rows[0].Stn;
                //var UId = result.rows[0].UId;
                var UId = $('#UId').val();
                var postdata = { "conditions": "sopt_1=ne&1=0" };
                if (UId) {
                    postdata = { "conditions": "sopt_UFid=eq&UFid=" + UId };
                }
                $("#SMPTYCGrid").jqGrid("setGridParam", {
                    model: "SmptycModel",
                    url: rootPath + "System/SmptycQuery?UFid=" + UId,
                    postData: postdata,
                    page: 1,
                    datatype: "json"
                }).trigger("reloadGrid");
                $("#SMPTYSGrid").jqGrid("setGridParam", {
                    model: "SmptysModel",
                    url: rootPath + "System/SmptysQuery?UFid=" + UId,
                    postData: postdata,
                    page: 1,
                    datatype: "json"
                }).trigger("reloadGrid");

                $("#SMPTYDGrid").jqGrid("setGridParam", {
                    model: "SmptydModel",
                    url: rootPath + "System/SmptydQuery?UFid=" + UId,
                    postData: postdata,
                    page: 1,
                    datatype: "json"
                }).trigger("reloadGrid");
                $("#TKCSTSGrid").jqGrid("setGridParam", {
                    model: "TkcstsModel",
                    url: rootPath + "System/TkcstsQuery?UFid=" + UId,
                    postData: postdata,
                    page: 1,
                    datatype: "json"
                }).trigger("reloadGrid");

                $("#SMBKINFOGrid").jqGrid("setGridParam", {
                    model: "SmbkinfoModel",
                    url: rootPath + "System/SmbkinfoQuery?UFid=" + UId,
                    postData: postdata,
                    page: 1,
                    datatype: "json"
                }).trigger("reloadGrid");

                gridEditableCtrl({ editable: false, gridId: "SMPTYCGrid" });
                gridEditableCtrl({ editable: false, gridId: "SMPTYSGrid" });
                gridEditableCtrl({ editable: false, gridId: "SMPTYDGrid" });
                gridEditableCtrl({ editable: false, gridId: "TKCSTSGrid" });
                gridEditableCtrl({ editable: false, gridId: "SMBKINFOGrid" });
            }
        });

        //var postdata = { "conditions": "sopt_1=ne&1=0" };
        //if (UId) {
        //    postdata = { "conditions": "sopt_UId=eq&UId=" + UId };
        //}
        //$("#SMPTYCGrid").jqGrid("clearGridData");
        //gridEditableCtrl({ editable: false, gridId: "SMPTYCGrid" });
        //$("#SMPTYCGrid").jqGrid("setGridParam", {
        //    url: rootPath + "System/SmptycQuery",
        //    postData: postdata,
        //    page: 1,
        //    datatype: "json"
        //}).trigger("reloadGrid");

        //$("#SMPTYSGrid").jqGrid("clearGridData");
        //gridEditableCtrl({ editable: false, gridId: "SMPTYSGrid" });
        //$("#SMPTYSGrid").jqGrid("setGridParam", {
        //    url: rootPath + "System/SmptysQuery",
        //    postData: postdata,
        //    page: 1,
        //    datatype: "json"
        //}).trigger("reloadGrid");

        //$("#SMPTYDGrid").jqGrid("clearGridData");
        //gridEditableCtrl({ editable: false, gridId: "SMPTYDGrid" });
        //$("#SMPTYDGrid").jqGrid("setGridParam", {
        //    url: rootPath + "System/SmptydQuery",
        //    postData: postdata,
        //    page: 1,
        //    datatype: "json"
        //}).trigger("reloadGrid");

    }

    MenuBarFuncArr.MBAdd = function () {
        editable = true;
        getAutoPartyNo("PartyNo");
        $("#CreateBy").val(userId);
        $("#Status").val("U");
        $("#Cmp").val(cmp);
        $("#Dep").val(dep);
        $("#SMPTYCGrid").jqGrid("clearGridData");
        gridEditableCtrl({ editable: true, gridId: "SMPTYCGrid" });
        $("#SMPTYSGrid").jqGrid("clearGridData");
        gridEditableCtrl({ editable: true, gridId: "SMPTYSGrid" });
        $("#SMPTYDGrid").jqGrid("clearGridData");
        gridEditableCtrl({ editable: true, gridId: "SMPTYDGrid" });
        $("#TKCSTSGrid").jqGrid("clearGridData");
        gridEditableCtrl({ editable: true, gridId: "TKCSTSGrid" });
        $("#SMBKINFOGrid").jqGrid("clearGridData");
        gridEditableCtrl({ editable: true, gridId: "SMBKINFOGrid" });
    }

    MenuBarFuncArr.MBDel = function () {
        var changeData = getAllKeyValue();
        var SmptycGridArray = $('#SMPTYCGrid').jqGrid('getGridParam', "arrangeGrid")();
        var SmptysGridArray = $('#SMPTYSGrid').jqGrid('getGridParam', "arrangeGrid")();
        var SmptydGridArray = $('#SMPTYDGrid').jqGrid('getGridParam', "arrangeGrid")();
        var TkcstsGridArray = $('#TKCSTSGrid').jqGrid('getGridParam', "arrangeGrid")();
        var SmbkinfoGridArray = $('#SMBKINFOGrid').jqGrid('getGridParam', "arrangeGrid")();
        changeData["ct"] = SmptycGridArray;
        changeData["st"] = SmptysGridArray;
        changeData["dt"] = SmptydGridArray;
        changeData["tk"] = TkcstsGridArray;
        changeData["bk"] = SmbkinfoGridArray;
        console.log(changeData);

        $.ajax({
            async: true,
            url: rootPath + "/System/BSCSSUpdateData",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                if (result.message != "success") {
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelF", 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    return false;
                }
                //成功后将页面的数据移除，并设置页面不可编辑
                setFieldValue(undefined, "");
                setdisabled(true);
                setToolBtnDisabled(true);
                $("#SMPTYCGrid").jqGrid("clearGridData");
                $("#SMPTYSGrid").jqGrid("clearGridData");
                $("#SMPTYDGrid").jqGrid("clearGridData");
                $("#TKCSTSGrid").jqGrid("clearGridData");
                $("#SMBKINFOGrid").jqGrid("clearGridData");
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
            }
        });
    }
    
    //MenuBarFuncArr.MBSearch = function (thisItem) {

    //    var gridMethod = function (map) {
    //        var UId = map.UId;
    //        var param = "sopt_UId=eq&UId=" + UId;

    //        //将获取的数据作为条件进行reload数据
    //        //$.ajax({
    //        //    async: true,
    //        //    url: url,
    //        //    type: 'POST',
    //        //    data: {
    //        //        model: "SmptyModel",
    //        //        sidx: 'UId',
    //        //        'conditions': encodeURI(param),
    //        //        page: 1,
    //        //        rows: 20
    //        //    },
    //        //    dataType: "json",
    //        //    "complete": function (xmlHttpRequest, successMsg) {
    //        //        if (successMsg != "success") return null;
    //        //    },
    //        //    "error": function (xmlHttpRequest, errMsg) {
    //        //    },
    //        //    success: function (result) {
    //        //        _dataSource = result.rows;
    //        //        setFieldValue(result.rows);
    //        //        var UserStn = result.rows[0].Stn;
    //        //        //绑定Grid
    //        //        setdisabled(true);
    //        //        setToolBtnDisabled(true);
    //        //    }
    //        //});
    //        $.ajax({
    //            async: true,
    //            url: rootPath + "/System/BSCSQuery",
    //            type: 'POST',
    //            data: {
    //                model: "SmptyModel",
    //                sidx: 'UId',
    //                'conditions': encodeURI(param),
    //                page: 1,
    //                rows: 20
    //            },
    //            dataType: "json",
    //            "complete": function (xmlHttpRequest, successMsg) {
    //                if (successMsg != "success") return null;
    //            },
    //            "error": function (xmlHttpRequest, errMsg) {
    //            },
    //            success: function (result) {
    //                _dataSource = result.rows;
    //                setFieldValue(result.rows);
    //                //var UserStn = result.rows[0].Stn;
    //                //var UId = result.rows[0].UId;
    //                var UId = $('#UId').val();
    //                var postdata = { "conditions": "sopt_1=ne&1=0" };
    //                if (UId) {
    //                    postdata = { "conditions": "sopt_UFid=eq&UFid=" + UId };
    //                }
    //                $("#SMPTYCGrid").jqGrid("setGridParam", {
    //                    model: "SmptycModel",
    //                    url: rootPath + "System/SmptycQuery?UId=" + UId,
    //                    postData: postdata,
    //                    page: 1,
    //                    datatype: "json"
    //                }).trigger("reloadGrid");

    //                $("#SMPTYSGrid").jqGrid("setGridParam", {
    //                    model: "SmptysModel",
    //                    url: rootPath + "System/SmptysQuery?UId=" + UId,
    //                    postData: postdata,
    //                    page: 1,
    //                    datatype: "json"
    //                }).trigger("reloadGrid");

    //                $("#SMPTYDGrid").jqGrid("setGridParam", {
    //                    model: "SmptydModel",
    //                    url: rootPath + "System/SmptydQuery?UId=" + UId,
    //                    postData: postdata,
    //                    page: 1,
    //                    datatype: "json"
    //                }).trigger("reloadGrid");
    //            }
    //        });
    //    }
    //    bscsOptions = {};
    //    bscsOptions.gridUrl = url;
    //    bscsOptions.registerBtn = thisItem;
    //    bscsOptions.isMutiSel = true;
    //    bscsOptions.param = '';
    //    bscsOptions.gridFunc = gridMethod;
    //    bscsOptions.responseMethod = function () { };
    //    bscsOptions.lookUpConfig = LookUpConfig.BSCSSetupLookup;
    //    initLookUp(bscsOptions);
    //}

    MenuBarFuncArr.MBEdit = function () {
        editable = true;
        gridEditableCtrl({ editable: true, gridId: "SMPTYCGrid" });
        gridEditableCtrl({ editable: true, gridId: "SMPTYSGrid" });
        gridEditableCtrl({ editable: true, gridId: "SMPTYDGrid" });
        gridEditableCtrl({ editable: true, gridId: "TKCSTSGrid" });
        gridEditableCtrl({ editable: true, gridId: "SMBKINFOGrid" });
    }
    
    MenuBarFuncArr.MBSave = function (dtd) {
        if (checkNoAllowNullFields() == false) {
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve();
            return false;
        }
        var UId = $("#UId").val();
        var partyNo = $("#PartyNo").val();

        var SmptycGridArray = $('#SMPTYCGrid').jqGrid('getGridParam', "arrangeGrid")();
        var SmptysGridArray = $('#SMPTYSGrid').jqGrid('getGridParam', "arrangeGrid")();
        var SmptydGridArray = $('#SMPTYDGrid').jqGrid('getGridParam', "arrangeGrid")();
        var TkcstsGridArray = $('#TKCSTSGrid').jqGrid('getGridParam', "arrangeGrid")();
        var SmbkinfoGridArray = $('#SMBKINFOGrid').jqGrid('getGridParam', "arrangeGrid")();

        var changeData = getChangeValue();
        changeData["ct"] = SmptycGridArray;
        changeData["st"] = SmptysGridArray;
        changeData["dt"] = SmptydGridArray;
        changeData["tk"] = TkcstsGridArray;
        changeData["bk"] = SmbkinfoGridArray;
        console.log(changeData);
       
        $.ajax({
            //notice async一定要為false
            //async: true,
            url: rootPath + "System/BSCSSUpdateData",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), "UId": UId, "partyNo":partyNo,autoReturnData: true },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
                gridEditableCtrl({ editable: false, gridId: "SMPTYCGrid" });
                gridEditableCtrl({ editable: false, gridId: "SMPTYSGrid" });
                gridEditableCtrl({ editable: false, gridId: "SMPTYDGrid" });
                gridEditableCtrl({ editable: false, gridId: "TKCSTSGrid" });
                gridEditableCtrl({ editable: false, gridId: "SMBKINFOGrid" });

            },
            "error": function (xmlHttpRequest, errMsg) {
                //notice ajax error 一定要放入下面三行
                CommonFunc.Notify("", errMsg, 500, "danger");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {

                if (result.message != "success") {
                    //notice ajax warning 一定要放入下面三行
                    CommonFunc.Notify("", "@Resources.Locale.L_BSCSSetup_SFail" + "   " + result.message, 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();

                    console.log(result.message);
                    return;
                }

                fileUpload();
                setdisabled(true);
                setToolBtnDisabled(true);
                initLoadData(result.UId);
                
               /* setFieldValue(result.smptyData);

                $("#SMPTYCGrid").jqGrid("setGridParam", {
                    datatype: 'local',
                    sortorder: "asc",
                    sortname: "UId",
                    data: result.smptycData
                }).trigger("reloadGrid");
                _dm.getDs("SMPTYCGrid").setData(result.ippodData);

                $("#SMPTYSGrid").jqGrid("setGridParam", {
                    datatype: 'local',
                    sortorder: "asc",
                    sortname: "UId",
                    data: result.smptysData
                }).trigger("reloadGrid");
                _dm.getDs("SMPTYSGrid").setData(result.smptysData);

                $("#SMPTYDGrid").jqGrid("setGridParam", {
                    datatype: 'local',
                    sortorder: "asc",
                    sortname: "UId",
                    data: result.smptydData
                }).trigger("reloadGrid");
                _dm.getDs("SMPTYDGrid").setData(result.smptydData);
                */
                //_dm.getDs("SMPTYCGrid").setData(result.smptycData);
                //$("#SMPTYCGrid").jqGrid("setGridParam", {
                //    datatype: 'json',
                //    data: result.smptycData
                //}).trigger("reloadGrid");

                //_dm.getDs("SMPTYSGrid").setData(result.smptysData);
                //$("#SMPTYSGrid").jqGrid("setGridParam", {
                //    datatype: 'json',
                //    data: result.smptysData
                //}).trigger("reloadGrid");

                //_dm.getDs("SMPTYDGrid").setData(result.smptydData);
                //$("#SMPTYDGrid").jqGrid("setGridParam", {
                //    datatype: 'json',
                //    data: result.smptydData
                //}).trigger("reloadGrid");


                //notice ajax success 一定要放入下面三行
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                dtd.resolve();
            }
        });
        //notice ajax 最後一定要 return promise
        return dtd.promise();
    }

    initMenuBar(MenuBarFuncArr);
    MenuBarFuncArr.DelMenu(["MBApprove", "MBCopy", "MBErrMsg", "MBSearch"]);
    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBAdd", "MBApprove"]);

    setdisabled(true);
    setToolBtnDisabled(true);

    //headoffice
    headOfficeOptions = {};
    headOfficeOptions.gridUrl = rootPath + "Common/GetPartyNoData";
    headOfficeOptions.registerBtn = $("#HeadOfficeLookup");
    headOfficeOptions.isMutiSel = true;
    headOfficeOptions.focusItem = $("#HeadOffice");
    headOfficeOptions.param = "&sopt_GroupId=eq&GroupId=" + groupId;
    //cntryOptions.baseCondition = " (BU='C' OR BU='D' OR BU='E')";
    headOfficeOptions.gridFunc = function (map) {
        var PartyNo = map.PartyNo;
        $("#HeadOffice").val(PartyNo);
    }
    headOfficeOptions.responseMethod = function () { }
    headOfficeOptions.lookUpConfig = LookUpConfig.PartyNoLookup;
    initLookUp(headOfficeOptions);
    CommonFunc.AutoComplete("#HeadOffice", 2, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO=", "PARTY_NO=showValue,PARTY_NO", function (event, ui) {
        $(this).val(ui.item.returnValue.PARTY_NO);
        return false;
    });


    //Party Type lookup
    PartyTypeOptions = {};
    PartyTypeOptions.gridUrl = rootPath + "Common/GetPartyTypeData";
    PartyTypeOptions.registerBtn = $("#PartyTypeLookup");
    PartyTypeOptions.isMutiSel = true;
    PartyTypeOptions.focusItem = $("#PartyType");
    //mutil select add a columnID help mapping selected data
    PartyTypeOptions.columnID = "Cd";
    PartyTypeOptions.param = "";
    PartyTypeOptions.gridFunc = function (map) {
    }
    PartyTypeOptions.responseMethod = function (data) {
        console.log(data);
        var str = "";
        $.each(data, function(index, val) {
            str = str + data[index].Cd + ";";
        });
        $("#PartyType").val(str);
    }
    PartyTypeOptions.lookUpConfig = LookUpConfig.MutiPartyTypeLookup;
    initLookUp(PartyTypeOptions);

    //billto
    billToOptions = {};
    billToOptions.gridUrl = rootPath + "Common/GetPartyNoData";
    billToOptions.registerBtn = $("#BillToLookup");
    billToOptions.isMutiSel = true;
    billToOptions.focusItem = $("#BillTo");
    billToOptions.param = "&sopt_GroupId=eq&GroupId=" + groupId;
    //cntryOptions.baseCondition = " (BU='C' OR BU='D' OR BU='E')";
    billToOptions.gridFunc = function (map) {
        var PartyNo = map.PartyNo;
        $("#BillTo").val(PartyNo);
    }
    billToOptions.responseMethod = function () { }
    billToOptions.lookUpConfig = LookUpConfig.PartyNoLookup;
    initLookUp(billToOptions);
    CommonFunc.AutoComplete("#BillTo", 2, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO=", "PARTY_NO=showValue,PARTY_NO", function (event, ui) {
        $(this).val(ui.item.returnValue.PARTY_NO);
        return false;
    });

    //国家代码
    cntryOptions = {};
    cntryOptions.gridUrl = rootPath + "Common/GetCntryCdData";
    cntryOptions.registerBtn = $("#CntyLookup");
    cntryOptions.isMutiSel = true;
    cntryOptions.focusItem = $("#Cnty");
    cntryOptions.param = "&sopt_Cmp=eq&Cmp=" + cmp + "&sopt_GroupId=eq&GroupId=" + groupId;
    //cntryOptions.baseCondition = " (BU='C' OR BU='D' OR BU='E')";
    cntryOptions.gridFunc = function (map) {
        var cntryCd = map.CntryCd,
            cntryNm = map.CntryNm;
        $("#Cnty").val(cntryCd);
        $("#CntyNm").val(cntryNm);
    }
    cntryOptions.responseMethod = function () { }
    cntryOptions.lookUpConfig = LookUpConfig.CntyCdLookup;
    initLookUp(cntryOptions);
    CommonFunc.AutoComplete("#Cnty", 2, "", "dt=country&GROUP_ID=" + groupId+ "&CNTRY_CD=", "CNTRY_CD=showValue,CNTRY_CD,CNTRY_NM", function (event, ui) {
        $("input[name='CntyNm']").val(ui.item.returnValue.CNTRY_NM);
        $(this).val(ui.item.returnValue.CNTRY_CD);
        return false;
    });


    ////城市港口
    cityOptions = {};
    cityOptions.gridUrl = rootPath + "Common/GetCityCdData";
    cityOptions.registerBtn = $("#CityLookup");
    cityOptions.isMutiSel = true;
    cityOptions.focusItem = $("#City");
    cityOptions.param = "&sopt_GroupId=eq&GroupId=" + groupId;
    //options.baseCondition = " (BU='C' OR BU='D' OR BU='E')";
    cityOptions.gridFunc = function (map) {
        var cd = map.PortCd,
            nm = map.PortNm;
        $("#City").val(cd);
        $("#CityNm").val(nm);
    }
    cityOptions.responseMethod = function () { }
    cityOptions.lookUpConfig = LookUpConfig.CityPortLookup;
    initLookUp(cityOptions);
    CommonFunc.AutoComplete("#City", 2, "", "dt=port&GROUP_ID=" + groupId + "&PORT_CD=", "PORT_CD=showValue,PORT_CD,PORT_NM", function (event, ui) {
        $("input[name='CityNm']").val(ui.item.returnValue.PORT_NM);
        $(this).val(ui.item.returnValue.PORT_CD);
        return false;
    });

    //State Lookup
    var options = {};
    options.gridUrl = rootPath + "TPVCommon/GetStateDataForLookup";
    options.registerBtn = $("#StateLookup");
    options.focusItem = $("#State");
    options.param = "";
    options.isMutiSel = true;
    options.baseConditionFunc = function(){
        var CntryCd = $("#Cnty").val();
	    var Cmp= $("#Cmp").val();
        var con="";
        if(Cmp != "")
        {
            con = "&Cmp=" + Cmp + "&sopt_Cmp=eq";
        }
        if (CntryCd != "") {
            //return " CNTRY_CD='" + CntryCd + "'"; 
            con = con == "" ? con : con + "&";
            return con + "CntryCd=" + CntryCd + "&sopt_CntryCd=eq";
        }
        else {
            return con;
        }
    }
    options.gridFunc = function (map) {
        $("#State").val(map.StateCd);
    }

    registBtnLookupNew($("#PickupPointLookup"), {
        item: '#PickupPoint',
        url: rootPath + LookUpConfig.TruckPortCdUrl,
        config: LookUpConfig.TruckPortCdLookup,
        autoCompDt: ConditionParam("", "PortCd", "", "bw"),
        selectRowFn: function (map) {
            $("#PickupPoint").val(map.PortCd);
            $("#PickupName").val(map.PortNm);
        },
        autoClearFunc: function () {
            $("#PickupPoint").val("");
            $("#PickupName").text("");
        },
        baseConditionFunc: function () {
            return ""
        }
    });

    options.lookUpConfig = LookUpConfig.StateLookup;
    initLookUp(options);
    CommonFunc.AutoComplete("#State", 1, "", "dt=state&GROUP_ID=" + groupId + "&STATE_CD=", "STATE_CD=showValue,STATE_CD,STATE_NM", function (event, ui) {
        var map = ui.item.returnValue;
        $(this).val(ui.item.returnValue.STATE_CD);
        return false;
    }, function(){
        var CntryCd = $("#Cnty").val();

        if(CntryCd != "")
        {
            return " CNTRY_CD=" + CntryCd; 
        }
        else
        {
            return "";
        }
    });


    $("#UId").removeAttr('required');

    initLoadData(_UId);
    //getSelectOptions();
    

    $("#delPic").click(function(event) {
        if (confirm("@Resources.Locale.L_BSCSSetup_SureDelStamp"))
        {
            var UId = $("#UId").val();
            if(UId == "" || UId == null)
            {
                alert("@Resources.Locale.L_BSCSSetup_NoNeedToDel");
                return;
            }
            var postData = {UId: UId};
            console.log(postData);
            $.ajax({
                url: rootPath + "System/delPic",
                type: 'POST',
                data: postData,
                async: false,
                error: function(){
                    CommonFunc.ToogleLoading(false);
                    alert("error");
                },
                beforeSend: function(){
                    CommonFunc.ToogleLoading(true);
                },
                success: function (data) {
                    if(data.message == "success")
                    {
                        CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelS", 500, "success");
                        $("#picInfo").find("img").remove();
                    }
                    else
                    {
                        CommonFunc.Notify("", data.message, 500, "warning");
                    }
                    CommonFunc.ToogleLoading(false);
                },
                cache: false
            });
        }

        return;
    });

    $("#ImRecord").on("change", function(){
        var value = $("#Ident").val();
        var idNo = $(this).val();
        switch (value) 
        {
            case "CU":
               strFormat = "NNNNNN-NNNNN";
               strReg = "^\\d{6}-\\d{5}$";

               break;
            case "34":
               strFormat = "NNN-NN-NNNN";
               strReg = "^\\d{3}-\\d{2}-\\d{4}$";
               caption = "SSN";
               break;
            case "EI":
               strFormat = "NN-NNNNNNNXX";
               strReg = "^\\d{2}-\\d{7}[A-Z0-9]{2}$";
               caption = "IRS#";
               break;
            case "1":
               strFormat = "NNNNNNNNN";
               strReg = "^\\d{9}$";
               break;
            case "9":
               strFormat = "NNNNNNNNNNNNN";
               strReg = "^\\d{6}-\\d{5}$";
               break;
            case "CW":
               strFormat = "YYDDPP-NNNNN";
               strReg = "^\\d{6}-\\d{5}$";
               caption = "CBP#";
               break;
            case "PN":
               strFormat = "XXXXXXXXX";
               strReg = "^[A-Z0-9.]{9}$";
               caption = "Passport#";
               break;
        }
        var reg = new RegExp(strReg);
        if (!reg.test(idNo)) 
        {
            alert(caption + " is not valid format of " + strFormat);
            $(this).val("");
            return false;
        }
    });
});


function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "System/GetPartyTypeSelects",
        type: 'POST',
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (result) {
            var deOptions = result.De;
            appendSelectOption($("#PartyType"), deOptions);
        }
    });
}


//function SetSelectOptions() {
//    $.ajax({
//        async: true,
//        url: rootPath + "Common/GetSelectOptions",
//        type: 'POST',
//        "error": function (xmlHttpRequest, errMsg) {
//            alert(errMsg);
//        },
//        success: function (data) {
//            var buOptions = data.Bu;
//            appendSelectOption($("#PartyType"), buOptions);
//        }
//    });
//}

//客户合约档
function getSMPTYCGridData() {
    var RegionOption = {};
    RegionOption.gridUrl = rootPath + "Common/GetBscsBsCodeData";
    RegionOption.param = "sopt_CdType=eq&CdType=TRGN"+"&sopt_GroupId=eq&GroupId=" + groupId;
    RegionOption.gridReturnFunc = function (map) {
        //var $SMPTYCGrid = $("#SMPTYCGrid");
        var value = map.Cd;
        //var selRowId = $SMPTYCGrid.jqGrid('getGridParam', 'selrow');
        //$SMPTYCGrid.jqGrid('setCell', selRowId, "Region", map.Cd, 'edit-cell dirty-cell');
        return value;
    };
    RegionOption.lookUpConfig = LookUpConfig.TrgnLookup;
    RegionOption.autoCompKeyinNum = 1;
    RegionOption.gridId = "SMPTYCGrid";
    RegionOption.autoCompUrl = "";
    RegionOption.autoCompDt = "dt=bsc&CD=";
    RegionOption.autoCompParams = "CD=showValue,CD";
    RegionOption.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.CD);
        //var $SMPTYCGrid = $("#SMPTYCGrid");
        //var selRowId = $SMPTYCGrid.jqGrid('getGridParam', 'selrow');
        //$SMPTYCGrid.jqGrid('setCell', selRowId, "Region", ui.item.returnValue.Cd, 'edit-cell dirty-cell');
    }

    var colModel = [
        { name: 'UId', showname: 'ID', sorttype: 'string', hidden: true, viewable: false },
        { name: 'UFid', showname: 'UFID', sorttype: 'string', hidden: true, viewable: false },
        { name: 'Region', title: 'Region', index: 'Region', width: 150, align: 'left', sorttype: 'string', editable: true, edittype: 'custom', editoptions: gridLookup(RegionOption) },
        { name: 'ContactNo', title: '@Resources.Locale.L_BSCSSetup_ContactNo', index: 'ContactNo', width: 150, align: 'left', sorttype: 'string', editable: true },
        { name: 'PartyNo', title: '@Resources.Locale.L_BSCSSetup_PartyNo', index: 'PartyNo', width: 150, align: 'left', sorttype: 'string', editable: true },
        { name: 'Status', title: '@Resources.Locale.L_BSCSSetup_Status', index: 'Status', width: 150, align: 'left', sorttype: 'string', editable: true, formatter: "select", edittype: 'select', editoptions: { value: '@Resources.Locale.L_BSCSSetup_Script_78' } },
        { name: 'VenderContact', title: '@Resources.Locale.L_BSCSSetup_VenderContact', index: 'VenderContact', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        {
            name: 'ContactDate', title: '@Resources.Locale.L_BSCSSetup_ContactDate', index: 'ContactDate',
            width: 150, align: 'left', sorttype: 'string',
            hidden: false, editable: true, formatter: 'date',
            editoptions: myEditDateInit,
            formatoptions: {
                srcformat: 'ISO8601Long',
                newformat: 'Y/m/d',
                defaultValue: null
            }
        },
        {
            name: 'ClosingDate', title: '@Resources.Locale.L_BSCSSetup_ClosingDate', index: 'ClosingDate',
            width: 150, align: 'left', sorttype: 'string',
            hidden: false, editable: true, formatter: 'date',
            editoptions: myEditDateInit,
            formatoptions: {
                srcformat: 'ISO8601Long',
                newformat: 'Y/m/d',
                defaultValue: null
            }

        },
        { name: 'CntrNum', title: '@Resources.Locale.L_BSCSSetup_CntrNum', index: 'CntrNum', width: 150, align: 'right', sorttype: 'number', hidden: false, editable: true },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true }
    ];

   

    $grid = $("#SMPTYCGrid");
    _dm.addDs("SMPTYCGrid", [], ["UId", "UFid"], $grid[0]);
    UId = $("#UId").val();
    var postdata = { "conditions": "sopt_1=ne&1=0" };
    if (UId) {
        postdata = { "conditions": "sopt_UFid=eq&UFid=" + UId };
    }
    new genGrid($grid, {
        datatype: "json",
        loadonce: true,
        cellEdit: false,//禁用grid编辑功能
        colModel: colModel,
        delKey: ["UId","UFid"],
        url: rootPath + "/System/SmptycQuery",
        postData: postdata,
        ds: _dm.getDs("SMPTYCGrid"),
        isModel:true,
        caption: "@Resources.Locale.L_BSCSSetup_VenderContract",
        height: "auto",
        refresh: true,
        pginput: false,
        sortable: false,
        pgbuttons: false,
        viewrecords: false,
        exportexcel: false,
        onAddRowFunc: function (rowid) {
            var UFid = $("#UId").val();
            $('#SMPTYCGrid').jqGrid('setCell', rowid, "UFid", UFid);
        },
    });
}

//客户停权档
function getSMPTYSGridData() {
    var RegionOption1 = {};
    RegionOption1.gridUrl = rootPath + "Common/GetBscsBsCodeData";
    RegionOption1.param = "sopt_CdType=eq&CdType=TRGN"+"&sopt_GroupId=eq&GroupId=" + groupId;
    RegionOption1.gridReturnFunc = function (map) {
        //var $SMPTYSGrid = $("#SMPTYSGrid");
        var value = map.Cd;
        //var selRowId = $SMPTYSGrid.jqGrid('getGridParam', 'selrow');
        //$SMPTYSGrid.jqGrid('setCell', selRowId, "Region", map.Cd, 'edit-cell dirty-cell');
        return value;
    };
    RegionOption1.lookUpConfig = LookUpConfig.TrgnLookup;
    RegionOption1.autoCompKeyinNum = 1;
    RegionOption1.gridId = "SMPTYSGrid";
    RegionOption1.autoCompUrl = "";
    RegionOption1.autoCompDt = "dt=bsc&CD=";
    RegionOption1.autoCompParams = "CD=showValue,CD";
    RegionOption1.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.CD);
        //var $SMPTYSGrid = $("#SMPTYSGrid");
        //var selRowId = $SMPTYSGrid.jqGrid('getGridParam', 'selrow');
        //$SMPTYSGrid.jqGrid('setCell', selRowId, "Region", ui.item.returnValue.Cd, 'edit-cell dirty-cell');
    }

    var PolOption = {};
    PolOption.gridUrl = rootPath + "Common/GetCityCdData";
    PolOption.param = 'sopt_GroupId=eq&GroupId=' + groupId;
    PolOption.gridReturnFunc = function (map) {
        //var $SMPTYSGrid = $("#SMPTYSGrid");
        var value = map.PortCd;
        //var selRowId = $SMPTYSGrid.jqGrid('getGridParam', 'selrow');
        //$SMPTYSGrid.jqGrid('setCell', selRowId, "Pol", map.PortCd, 'edit-cell dirty-cell');
        return value;
    };
    PolOption.lookUpConfig = LookUpConfig.CityPortLookup;
    PolOption.autoCompKeyinNum = 1;
    PolOption.gridId = "SMPTYSGrid";
    PolOption.autoCompUrl = "";
    PolOption.autoCompDt = "dt=port&PORT_CD=";
    PolOption.autoCompParams = "PORT_CD=showValue,PORT_CD";
    PolOption.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.PORT_CD);
        //var $SMPTYSGrid = $("#SMPTYSGrid");
        //var selRowId = $SMPTYSGrid.jqGrid('getGridParam', 'selrow');
        //$SMPTYSGrid.jqGrid('setCell', selRowId, "Pol", ui.item.returnValue.PortCd, 'edit-cell dirty-cell');
    }

    var PodOption = {};
    PodOption.gridUrl = rootPath + "Common/GetCityCdData";
    PodOption.param = 'sopt_GroupId=eq&GroupId=' + groupId;
    PodOption.gridReturnFunc = function (map) {
        //var $SMPTYSGrid = $("#SMPTYSGrid");
        var value = map.PortCd;
        //var selRowId = $SMPTYSGrid.jqGrid('getGridParam', 'selrow');
        //$SMPTYSGrid.jqGrid('setCell', selRowId, "Region", map.PortCd, 'edit-cell dirty-cell');
        return value;
    };
    PodOption.lookUpConfig = LookUpConfig.CityPortLookup;
    PodOption.autoCompKeyinNum = 1;
    PodOption.gridId = "SMPTYSGrid";
    PodOption.autoCompUrl = "";
    PodOption.autoCompDt = "dt=port&PORT_CD=";
    PodOption.autoCompParams = "PORT_CD=showValue,PORT_CD";
    PodOption.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.PORT_CD);
        // var $SMPTYSGrid = $("#SMPTYSGrid");
        //var selRowId = $SMPTYSGrid.jqGrid('getGridParam', 'selrow');
        //$SMPTYSGrid.jqGrid('setCell', selRowId, "Pod", ui.item.returnValue.PortCd, 'edit-cell dirty-cell');
    }

    var LocationOption = {};
    LocationOption.gridUrl = rootPath + "Common/GetBscsLocationData";
    LocationOption.param = "sopt_CdType=eq&CdType=LOC" + "&sopt_GroupId=eq&GroupId=" + groupId;
    LocationOption.gridReturnFunc = function (map) {
       // var $SMPTYSGrid = $("#SMPTYSGrid");
        var value = map.CD;
        return value;
    };
    LocationOption.lookUpConfig = LookUpConfig.LocationLookup;
    LocationOption.autoCompKeyinNum = 1;
    LocationOption.gridId = "SMPTYSGrid";
    LocationOption.autoCompUrl = "";
    LocationOption.autoCompDt = "dt=bsc&CD=";
    LocationOption.autoCompParams = "CD=showValue,CD";
    LocationOption.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.CD);
        //var $SMPTYSGrid = $("#SMPTYSGrid");
    }

    var colModel1 = [
        { name: 'UId', showname: 'ID', sorttype: 'string', hidden: true, viewable: false },
        { name: 'UFid', showname: 'UFID', sorttype: 'string', hidden: true, viewable: false },
        {
            name: 'StartDate', title: '@Resources.Locale.L_BSCSSetup_StartDate', index: 'StartDate',
            width: 150, align: 'left', sorttype: 'string',
            editable: true, formatter: 'date',
            editoptions: myEditDateInit,
            formatoptions: {
                srcformat: 'ISO8601Long',
                newformat: 'Y/m/d',
                defaultValue: null
            }

        },
        {
            name: 'EndDate', title: '@Resources.Locale.L_BSCSSetup_EndDate', index: 'EndDate',
            width: 150, align: 'left', sorttype: 'string',
            editable: true, formatter: 'date',
            editoptions: myEditDateInit,
            formatoptions: {
                srcformat: 'ISO8601Long',
                newformat: 'Y/m/d',
                defaultValue: null
            }

        },
        { name: 'PartyNo', title: '@Resources.Locale.L_BSCSSetup_PartyNum', index: 'PartyNo', width: 150, align: 'left', sorttype: 'string', editable: true },
        { name: 'Location', title: 'Location', index: 'Location', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, edittype: 'custom', editoptions: gridLookup(LocationOption) },
        { name: 'Region', title: 'Region', index: 'Region', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, edittype: 'custom', editoptions: gridLookup(RegionOption1) },
        { name: 'Pol', title: 'Pol', index: 'Pol', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, edittype: 'custom', editoptions: gridLookup(PolOption) },
        { name: 'Pod', title: 'Pod', index: 'Pod', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, edittype: 'custom', editoptions: gridLookup(PodOption) },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true }
    ];

    $grid1 = $("#SMPTYSGrid");
    _dm.addDs("SMPTYSGrid", [], ["UId", "UFid"], $grid1[0]);
    UId = $("#UId").val();
    var postdata = { "conditions": "sopt_1=ne&1=0" };
    if (UId) {
        postdata = { "conditions": "sopt_UFid=eq&UFid=" + UId };
    }
    new genGrid($grid1, {
        datatype: "json",
        loadonce: true,
        cellEdit: false,//禁用grid编辑功能
        colModel: colModel1,
        delKey: ["UId","UFid"],
        url: rootPath + "/System/SmptysQuery",
        postData: postdata,
        ds: _dm.getDs("SMPTYSGrid"),
        isModel:true,
        caption: "@Resources.Locale.L_BSCSSetup_SuspendSetup",
        height: "auto",
        refresh: true,
        pginput: false,
        sortable: false,
        pgbuttons: false,
        viewrecords: false,
        exportexcel: false,
        onAddRowFunc: function (rowid) {
            var UFid = $("#UId").val();
            $('#SMPTYSGrid').jqGrid('setCell', rowid, "UFid", UFid);
        },
    });
}

//客户送货档
function getSMPTYDGridData() {
    var RegionOption1 = {};
    RegionOption1.gridUrl = rootPath + "Common/GetBscsBsCodeData";
    RegionOption1.param = "sopt_CdType=eq&CdType=TRGN"+"&sopt_GroupId=eq&GroupId=" + groupId;
    RegionOption1.gridReturnFunc = function (map) {
        var $SMPTYDGrid = $("#SMPTYDGrid");
        var value = map.Cd;
        return value;
    };
    RegionOption1.lookUpConfig = LookUpConfig.TrgnLookup;
    RegionOption1.autoCompKeyinNum = 1;
    RegionOption1.gridId = "SMPTYDGrid";
    RegionOption1.autoCompUrl = "";
    RegionOption1.autoCompDt = "dt=bsc&CD=";
    RegionOption1.autoCompParams = "CD=showValue,CD";
    RegionOption1.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.CD);
        //var $SMPTYDGrid = $("#SMPTYDGrid");
    }
    //state
    var StateOption = {};
    StateOption.gridUrl = rootPath + "TPVCommon/GetStateDataForLookup";
    StateOption.param = "sopt_GroupId=eq&GroupId=" + groupId;
    StateOption.gridReturnFunc = function (map) {
        var $SMPTYDGrid = $("#SMPTYDGrid");
        var value = map.StateCd;
        return value;
    };
    StateOption.lookUpConfig = LookUpConfig.StateLookup;
    StateOption.autoCompKeyinNum = 1;
    StateOption.gridId = "SMPTYDGrid";
    StateOption.autoCompUrl = "";
    StateOption.autoCompDt = "dt=bsstate&STATE_CD=";
    StateOption.autoCompParams = "STATE_CD=showValue,STATE_CD";
    StateOption.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.STATE_CD);
        //var $SMPTYDGrid = $("#SMPTYDGrid");
    }

    //卡车地点
    var PortOption = {};
    PortOption.gridUrl = rootPath + "Common/GetTruckPortCdData";
    PortOption.param = "sopt_GroupId=eq&GroupId=" + groupId;
    PortOption.gridReturnFunc = function (map) {
        var $SMPTYDGrid = $("#SMPTYDGrid");
        var value = map.PortCd;
        return value;
    };
    PortOption.lookUpConfig = LookUpConfig.TruckPortCdLookup;
    PortOption.autoCompKeyinNum = 1;
    PortOption.gridId = "SMPTYDGrid";
    PortOption.autoCompUrl = "";
    PortOption.autoCompDt = "dt=bstport&PORT_CD=";
    PortOption.autoCompParams = "PORT_CD=showValue,PORT_CD";
    PortOption.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.PORT_CD);
        //var $SMPTYDGrid = $("#SMPTYDGrid");
    }


    var colModel2 = [
        { name: 'UId', showname: 'ID', sorttype: 'string', hidden: true, viewable: false },
        { name: 'UFid', showname: 'UFID', sorttype: 'string', hidden: true, viewable: false },
        { name: 'SeqNo', showname: 'SEQNO', sorttype: 'string', hidden: true, viewable: false },
        { name: 'Region', title: '@Resources.Locale.L_BSCSSetup_Region', index: 'Region', width: 150, align: 'left', sorttype: 'string', editable: true, edittype: 'custom', editoptions: gridLookup(RegionOption1) },
        { name: 'State', title: '@Resources.Locale.L_BaseLookup_State', index: 'State', width: 150, align: 'left', sorttype: 'string', editable: true, edittype: 'custom', editoptions: gridLookup(StateOption) },
        { name: 'Port', title: '@Resources.Locale.L_BSCSSetup_Port', index: 'Port', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, edittype: 'custom', editoptions: gridLookup(PortOption) },
        { name: 'PartyNo', title: '@Resources.Locale.L_BSCSSetup_PartyNo', index: 'PartyNo', width: 150, align: 'left', sorttype: 'string', editable: true },
        { name: 'ZipCd', title: '@Resources.Locale.L_BSCSDataQuery_Zip', index: 'ZipCd', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'Name', title: '@Resources.Locale.L_BSCSSetup_CmpName', index: 'Name', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'Addr', title: '@Resources.Locale.L_BSCSSetup_CmpAddr', index: 'Addr', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'Attn', title: '@Resources.Locale.L_BSCSSetup_CmpAttn', index: 'Attn', width: 150, align: 'left', sorttype: 'string', editable: true },
        { name: 'Tel', title: '@Resources.Locale.L_BSCSSetup_CmpTel', index: 'Tel', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'MailId', title: '@Resources.Locale.L_BSCSSetup_CmpEmail', index: 'MailId', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true }
    ];

    $grid2 = $("#SMPTYDGrid");
    _dm.addDs("SMPTYDGrid", [], ["UId", "UFid"], $grid2[0]);
    UId = $("#UId").val();
    var postdata = { "conditions": "sopt_1=ne&1=0" };
    if (UId) {
        postdata = { "conditions": "sopt_UFid=eq&UFid=" + UId };
    }
    new genGrid($grid2, {
        datatype: "json",
        loadonce: true,
        cellEdit: false,//禁用grid编辑功能
        colModel: colModel2,
        delKey: ["UId","UFid","SeqNo"],
        url: rootPath + "System/SmptydQuery",
        postData: postdata,
        ds: _dm.getDs("SMPTYDGrid"),
        isModel:true,
        caption: "@Resources.Locale.L_BSCSSetup_DomAdressSetup",
        height: "auto",
        refresh: true,
        pginput: false,
        sortable: false,
        pgbuttons: false,
        viewrecords: false,
        exportexcel: false,
        onAddRowFunc: function (rowid) {
            var UFid = $("#UId").val();
            $('#SMPTYSGrid').jqGrid('setCell', rowid, "UFid", UFid);
        },
    });
}

//状态建档
function getTKCSTSGridData() {

    var colModel2 = [
        { name: 'UId', showname: 'ID', sorttype: 'string', hidden: true, viewable: false },
        { name: 'UFid', showname: 'UFID', sorttype: 'string', hidden: true, viewable: false },
        { name: 'SeqNo', showname: 'SEQNO', sorttype: 'string', hidden: true, viewable: false },
        { name: 'PartyNo', title: '@Resources.Locale.L_BSCSSetup_PartyNum', index: 'PartyNo', width: 200, align: 'left', sorttype: 'string', editable: true, hidden: false },
        { name: 'StsCd', title: '@Resources.Locale.L_TKQuery_StsCd', index: 'StsCd', width: 150, align: 'left', sorttype: 'string', editable: true, hidden: false },
        { name: 'StsDescp', title: '@Resources.Locale.L_BSCSSetup_Descp', index: 'StsDescp', width: 200, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'LightSeq', title: '@Resources.Locale.L_BSCSSetup_LightSeq', index: 'LightSeq', width: 150, align: 'left', sorttype: 'string', editable: true },
        { name: 'InputView', title: '@Resources.Locale.L_BSCSSetup_InputView', index: 'InputView', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 200, align: 'left', sorttype: 'string', hidden: false, editable: true }
    ];

    $grid2 = $("#TKCSTSGrid");
    _dm.addDs("TKCSTSGrid", [], ["UId", "UFid"], $grid2[0]);
    UId = $("#UId").val();
    var postdata = { "conditions": "sopt_1=ne&1=0" };
    if (UId) {
        postdata = { "conditions": "sopt_UFid=eq&UFid=" + UId };
    }
    new genGrid($grid2, {
        datatype: "json",
        loadonce: true,
        cellEdit: false,//禁用grid编辑功能
        colModel: colModel2,
        delKey: ["UId", "UFid", "SeqNo"],
        url: rootPath + "System/TkcstsQuery",
        postData: postdata,
        ds: _dm.getDs("TKCSTSGrid"),
        isModel:true,
        caption: "@Resources.Locale.L_BSCSSetup_StsSet",
        height: "auto",
        refresh: true,
        pginput: false,
        sortable: false,
        pgbuttons: false,
        viewrecords: false,
        exportexcel: false,
        onAddRowFunc: function (rowid) {
            var UFid = $("#UId").val();
            $('#TKCSTSGrid').jqGrid('setCell', rowid, "UFid", UFid);
        },
    });
}

//银行账号建档
function getSMBKINFOGridData() {
    var $SMBKINFOGrid = $("#SMBKINFOGrid");
    function getCur() {
        var cur_op = getLookupOp("SMBKINFOGrid",
            {
                url: rootPath + LookUpConfig.CurUrl,
                config: LookUpConfig.CurLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "Crncy", map.Cur, "lookup");
                    return map.Cur;
                }
            }, LookUpConfig.GetCurAuto(groupId, "", $SMBKINFOGrid,
                function ($grid, rd, elem, rowid) {
                    setGridVal($grid, rowid, "Crncy", rd.CUR, "lookup");
                }), { param: "" });
        return cur_op;
    }

    function getCntry() {
        var cntry_op = getLookupOp("SMBKINFOGrid",
            {
                url: rootPath + LookUpConfig.CountryUrl,
                config: LookUpConfig.CountryLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "BankCountry", map.CntryCd, "lookup");
                    return map.CntryCd;
                }
            }, LookUpConfig.GetCountryAuto(groupId, $SMBKINFOGrid,
                function ($grid, rd, elem, selRowId) {
                    setGridVal($grid, selRowId, "BankCountry", rd.CNTRY_CD, "lookup");
                }), { param: "" });
        return cntry_op;
    }

    function getCity() {
        var city_op = getLookupOp("SMBKINFOGrid",
            {
                url: rootPath + LookUpConfig.CityPortUrl,
                config: LookUpConfig.CityPortLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, 'City', map.PortCd, null);
                    return map.PortCd;
                }
            }, LookUpConfig.GetCityPortAuto2(groupId, $SMBKINFOGrid,
                function ($grid, rd, elem, selRowId) {
                    setGridVal($grid, selRowId, 'City', rd.PORT_CD, "lookup");
                }), { param: "" });
        return city_op;
    }

    var colModel3 = [
        { name: 'UId', showname: 'ID', sorttype: 'string', hidden: true, viewable: false },
        { name: 'UFid', showname: 'UFID', sorttype: 'string', hidden: true, viewable: false },
        { name: 'Crncy', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Crncy', sorttype: 'string', editoptions: gridLookup(getCur()), edittype: 'custom', width: 100, align: 'left', hidden: false, editable: true },
        { name: 'BankCountry', title: '@Resources.Locale.L_BankInfo_BankCountry', index: 'BankCountry', width: 100, sorttype: 'string', edittype: 'custom', editable: true, editoptions: gridLookup(getCntry()) },
        { name: 'BankRef', title: '@Resources.Locale.L_BankInfo_BankRef', index: 'BankRef', width: 100, align: 'left', sorttype: 'string', editable: true, hidden: false },
        { name: 'BankInfo', title: '@Resources.Locale.L_DNManage_BkAc', index: 'BankInfo', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'RefNo', title: '@Resources.Locale.L_BankInfo_BankRefNo', index: 'RefNo', sorttype: 'string', width: 150, hidden: false, editable: true },
        { name: 'AccountName', title: '@Resources.Locale.L_BankInfo_AccountName', index: 'AccountName', sorttype: 'string', width: 150, hidden: false, editable: true },
        { name: 'IbankNo', title: 'IBAN', index: 'IbankNo', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'BankType', title: '@Resources.Locale.L_BankInfo_BankType', index: 'BankType', sorttype: 'string', width: 150, hidden: false, editable: true },
        { name: 'CollectBank', title: '@Resources.Locale.L_BankInfo_CollectBank', index: 'CollectBank', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Streets', title: '@Resources.Locale.L_BankInfo_Streets', index: 'Streets', width: 200, align: 'left', sorttype: 'string', editable: true, hidden: false },
        { name: 'City', title: '@Resources.Locale.L_BSCSSetup_City', index: 'City', width: 100, sorttype: 'string', edittype: 'custom', editable: true, editoptions: gridLookup(getCity()) },
        { name: 'SwiftCode', title: 'Swift/BIC', index: 'SwiftCode', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'BankNo', title: '@Resources.Locale.L_BankInfo_BankNo', index: 'BankNo', width: 150, align: 'left', sorttype: 'string', editable: true, hidden: false }
    ];

    $grid3 = $("#SMBKINFOGrid");
    _dm.addDs("SMBKINFOGrid", [], ["UId", "UFid"], $grid3[0]);
    var postdata = { "conditions": "sopt_1=ne&1=0" };
    UId = $("#UId").val();
    if (UId) {
        postdata = { "conditions": "sopt_UFid=eq&UFid=" + UId };
    }
    console.log(postdata);
    new genGrid($grid3, {
        datatype: "json",
        loadonce: true,
        cellEdit: false,//禁用grid编辑功能
        colModel: colModel3,
        delKey: ["UId", "UFid", "SeqNo"],
        url: rootPath + "System/SmbkinfoQuery",
        postData: postdata,
        ds: _dm.getDs("SMBKINFOGrid"),
        isModel: true,
        caption: "@Resources.Locale.L_BSCSSetup_BankInfo",
        height: "auto",
        refresh: true,
        pginput: false,
        sortable: false,
        pgbuttons: false,
        viewrecords: false,
        exportexcel: false,
        onAddRowFunc: function (rowid) {
            var UFid = $("#UId").val();
            $('#SMBKINFOGrid').jqGrid('setCell', rowid, "UFid", UFid);
        },
    });
}


function fileUpload() {
    var UId = $("#UId").val();
    if(UId == "" || UId == null || typeof UId == "undefined")
    {
        return;
    }
    var fd = new FormData(document.forms.namedItem("fileinfo"));
    fd.append("UId", $("#UId").val());
    $.ajax({
        url: rootPath + "System/picFileUpload",
        type: "POST",
        data: fd,
        dataType:　"JSON",
        processData: false,  // tell jQuery not to process the data
        contentType: false,   // tell jQuery not to set contentType
        beforeSend: function(){
            CommonFunc.ToogleLoading(true);
        },
        success: function(result){
            //if(result.message != "success")
            //{
            //    CommonFunc.Notify("", result.message, 500, "warning");
            //}
            if (result.message == "success") {
                CommonFunc.Notify("", "@Resources.Locale.L_BSCSSetup_UploadSuc", 500, "success");
                var imgPath = rootPath + "System/Image/" + $("#UId").val();
                var link = '<img src="' + imgPath + '" alt="" class="img-thumbnail" height="300" />';
                $("#picInfo").html(link);
            }
            CommonFunc.ToogleLoading(false);
        }
    });
}

function getAutoPartyNo(id) {
    $.ajax({
        async: true,
        url: rootPath + "System/GetPartyNo",
        type: 'POST',
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {

        },
        "error": function (xmlHttpRequest, errMsg) {
            return "";
        },
        success: function (result) {
            console.log(result);
            console.log(result[0].partyNo);
            $("#" + id).val(result[0].partyNo);
            fill();
        }
    });
}

function ChangeLabelVal() {
    var keys = ["PartyNo", "Abbr", "PartyName",
        "PartyName2", "PartyName3", "PartyName4", "PartAddr1", "PartAddr2", "PartAddr3", "PartAddr4", "PartAddr5",
        "Cnty", "City", "State", "Zip", "PartyAttn", "PartyTel", "PartyMail", "PartyFax", "TaxNo", "PayTerm", "PayMethod", "MobileTel", "TradingPartner", "SourceCode"];
    for (var i = 0; i < keys.length; i++) {
        var keyid = keys[i];
        var desp = $("label[for='" + keyid + "']").html();
        $("label[for='" + keyid + "']").html('*' + desp);
    }
}
