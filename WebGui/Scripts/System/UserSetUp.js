var _dm = new dm();
var url = "";//  rootPath + "/Shipment/InquiryBootstrapData";
var lGroupId = getCookie("plv3.passport.groupid"),
    lCmp = getCookie("plv3.passport.companyid"),
    lStn = getCookie("plv3.passport.station"),
    lUserId = getCookie("plv3.passport.user");


function initLoadData(UId) {
    if (!UId)
        return;
    var param = "sopt_UId=eq&UId=" + UId;
    //将获取的数据作为条件进行reload数据
    $.ajax({
        async: true,
        url: rootPath + "System/UserDetailQuery?UId=" + UId+"&GroupId="+GroupId+"&Cmp="+Cmp,
        type: 'POST',
        data: {
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
            var $UserWhGrid = $("#UserWhGrid");
            var $SubGrid = null;
            $SubGrid = $("#SubGrid");
            //var maindata = jQuery.parseJSON(result.Content);
            
            var maindata = jQuery.parseJSON(result.mainTable.Content);
            var whData = jQuery.parseJSON(result.whData.Content);
            var LogData =jQuery.parseJSON(result.LogData.Content);
            _dataSource = maindata.rows;
            $('#UPassword').attr("type", "password");
            if (typeof maindata.rows[0].UPassword != "undefined")
            {
                delete maindata.rows[0].UPassword;
            }
            $("#AddRoleBtn").prop("disabled", false);
            $("#showinfo").css("display", "none");
            if ("N" == maindata.rows[0].Md5Info) {
                $("#AddRoleBtn").prop("disabled", true);
                $('#showinfo').css('display', 'block');
                //$('#poltruckbtndiv').css('display', 'block');
            }
            setFieldValue(maindata.rows);
            $UserWhGrid.jqGrid("setGridParam", {
                datatype: 'local',
                sortorder: "asc",
                sortname: "SeqNo",
                data: whData.rows
            }).trigger("reloadGrid");

            $SubGrid.jqGrid("setGridParam", {
                datatype: 'local',
                sortorder: "desc",
                sortname: "CreateDate",
                data: LogData.rows
            }).trigger("reloadGrid");
            

            var UStatus = maindata.rows[0].UStatus;

            if (UStatus == 0) {
                $("#UStatusYes").prop("checked", true);
                $("#UStatusNo").prop("checked", false);
                if (IoFlag == "I") $("#UStatusLeave").prop("checked", false);
            } else if (UStatus == 1) {
                $("#UStatusYes").prop("checked", false);
                $("#UStatusNo").prop("checked", true);
                if (IoFlag == "I") $("#UStatusLeave").prop("checked", false);
            } else{
                $("#UStatusYes").prop("checked", false);
                $("#UStatusNo").prop("checked", false);
                if (IoFlag == "I") $("#UStatusLeave").prop("checked", true);
            }
            if (maindata.rows[0].IE == 'Y') { $("#IEY").prop("checked", true); }
            if (maindata.rows[0].KeyUser == 'Y') { $("#KeyUserY").prop("checked", true); }
            $("#UStatus").val(UStatus);
            if (maindata.rows[0].MailFlag == 'Y') { $("#MailFlagCheckBox").prop("checked", true); }
            if (maindata.rows[0].MsgFlag == 'Y') { $("#MsgFlagCheckBox").prop("checked", true); }
            if (maindata.rows[0].DefaultSite == 'Y') { $("#DefaultSiteY").prop("checked", true); }
            var UserGroupId = maindata.rows[0].GroupId;
            var UserCmp = maindata.rows[0].Cmp;
            var UserStn = maindata.rows[0].Stn;
            var UserDep = maindata.rows[0].Dep;
            $("#GroupId").find("option").each(function (index, el) {
                if ($(this).val() == UserGroupId) {
                    $(this).prop("selected", true);
                }
            });

            var CmpPri = maindata.rows[0].CmpPri;
            if(CmpPri === null)
            {
                CmpPri = "";
            }
            var CmpPriArray = new Array();
            console.log(CmpPri);
            CmpPriArray = CmpPri.split(";");

            $("input[name='CmpPri[]']").each(function(index, el) {
                var cVal = $(this).val();
                var c = $(this);
                $.each(CmpPriArray, function(index, val) {
                    if(cVal === val)
                    {
                        c.prop("checked", true);
                    }
                });
                //if (CmpPri == "" && cVal=="*") {
                //    c.prop("checked", true);
                //}
            });



            var TranType = maindata.rows[0].TranType;
            if (TranType === null) {
                TranType = "";
            }
            var TranTypeArray = new Array();
            console.log(TranType);
            TranTypeArray = TranType.split(";");

            $("input[name='TranType[]']").each(function (index, el) {
                var cVal = $(this).val();
                var c = $(this);
                $.each(TranTypeArray, function (index, val) {
                    if (cVal === val) {
                        c.prop("checked", true);
                    }
                });
            });

            setdisabled(true);
            setToolBtnDisabled(true);
            MenuBarFuncArr.Disabled(["MBSave", "MBCancel","MBResume"]);
            MenuBarFuncArr.Enabled(["MBCopy", "MBDel", "MBEdit", "MBApprove", "MBEdoc", "MBClearFailCount", "MBUpdatePwd", "MBRoleAss", "MBRoleLog", "PermissionCopy"]);
            if (IoFlag == "I" && UStatus == 2) {
                MenuBarFuncArr.Disabled(["MBEdit", "MBClearFailCount", "MBUpdatePwd", "MBRoleAss"]);
                MenuBarFuncArr.Enabled(["MBResume"]);
            }
            var mainrow = maindata.rows[0];
            if (UId == lUserId && Cmp == lCmp) {
                window.parent.umEmail = mainrow.UEmail;
                window.parent.umTel = mainrow.UPhone;
                window.parent.umExt = mainrow.UExt;
                window.parent.umWechat = mainrow.UWechat;
                window.parent.umQq = mainrow.UQq;
                window.parent.umUpdatePriDate = mainrow.UpdatePriDate;
            }
        }
    });
}

$(document).ready(function () {
    //$("input").attr("autocomplete", "new-password");
    var $UserWhGrid = $("#UserWhGrid");
    var $SubGrid =  $("#SubGrid");
    var selStr = "<input type='checkbox' name='CmpPri[]' value='*' disabled>ALL";
    $.each(DeCmpPriData, function(index, val) {
        selStr += "<input type='checkbox' name='CmpPri[]' value='"+DeCmpPriData[index].Cmp+"' disabled>" + DeCmpPriData[index].Name;
    });
    $("#CmpPriArea").append(selStr);

    /*
    var selStr = "<input type='checkbox' name='PlantPri[]' value='*' disabled>ALL";

    console.log(PlantPriData);
    $.each(PlantPriData, function (index, val) {
        selStr += "<input type='checkbox' name='PlantPri[]' value='" + PlantPriData[index].Cd + "' disabled>" + PlantPriData[index].CdDescp;
    });
    $("#PlantPriArea").append(selStr);
    */
    //warehouse type lookup
    WareHouseOPtions = {};
    WareHouseOPtions.gridUrl = rootPath + "TPVCommon/GetWarehouseTypeDataByCmp"
    WareHouseOPtions.registerBtn = $("#WarehouseLookup");
    WareHouseOPtions.isMutiSel = true;
    WareHouseOPtions.focusItem = $("#Whs");
    WareHouseOPtions.columnID = "WsCd";

    WareHouseOPtions.param = "";
    WareHouseOPtions.responseMethod = function (data) {
        var str = "";
        $.each(data, function (index, val) {
            str = str + data[index].WsCd + ";";
        });
        $("#Whs").val(str);
    }
    WareHouseOPtions.lookUpConfig = LookUpConfig.MultWarehouseLookup;
    initLookUp(WareHouseOPtions);

    function getWHop() {
        var city_op = getLookupOp("UserWhGrid",
            {
                url: rootPath + "TPVCommon/GetWarehouseTypeData",
                config: LookUpConfig.WarehouseLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "WsNm", map.WsNm, null);
                    setGridVal($grid, selRowId, "WsCmp", map.Cmp, null);
                    setGridVal($grid, selRowId, "WsUid", map.WsCd, null);
                    return map.WsCd;
                }
            }, LookUpConfig.GetSMWHAuto1(groupId, $UserWhGrid,
                function ($grid, rd, elem, selRowId) {
                    setGridVal($grid, selRowId, "WsNm", rd.WS_NM, null);
                    setGridVal($grid, selRowId, "WsCmp", rd.CMP, null);
                    setGridVal($grid, selRowId, "WsUid", "", null);
                    $(elem).val(rd.WS_CD);
                }, function () {
                    var selRowId = $UserWhGrid.jqGrid('getGridParam', 'selrow');
                    var cmp = getGridVal($UserWhGrid, selRowId, "WsCmp", null);
                    var wsUid = getGridVal($UserWhGrid, selRowId, "WsUid", null);
                    var wsCd = getGridVal($UserWhGrid, selRowId, "WsCd", "lookup");
                    if (cmp != "" && wsUid == wsCd && wsUid!="") {
                        return "&CMP=" + cmp;
                    }
                    return "";
                }, function ($grid, elem, rowid) {
                    setGridVal($grid, rowid, "WsCd", "", null);
                    setGridVal($grid, rowid, "WsNm", "", null);
                    setGridVal($grid, rowid, "WsCmp", "", null);
                    setGridVal($grid, rowid, "WsUid", "", null);
            }), {
            param: "",
            baseConditionFunc: function () {
                return "IO_TYPE IS NULL";
            }
        });
        return city_op;
    }

    var colModel = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', hidden: true },
        { name: 'GroupId', title: 'GroupId', index: 'GroupId', sorttype: 'string', hidden: true },
        { name: 'UserId', title: 'Uesr Id', index: 'UserId', sorttype: 'string', hidden: true },
        { name: 'UCmp', title: 'User Cmp', index: 'UCmp', sorttype: 'string', hidden: true },
        { name: 'WsCd', title: 'WareHouse Code', index: 'WsCd', width: 200, sorttype: 'string', hidden: false, editable: true, editoptions: gridLookup(getWHop()), edittype: 'custom'},
        { name: 'WsNm', title: 'WareHouse Name', index: 'WsNm', width: 200, sorttype: 'string', hidden: false },
        { name: 'WsCmp', title: 'WareHouse Cmp', index: 'WsCmp', width: 200, sorttype: 'string', hidden: false },
        { name: 'WsUid', title: 'Select UId', index: 'WsUid', width: 200, sorttype: 'string', hidden: true }
    ];

    _handler.intiGrid("UserWhGrid", $UserWhGrid, {
        colModel: colModel, caption: 'visible local/other site warehouse table', delKey: ["UId", "UserId", "UCmp","GroupId"],
        onAddRowFunc: function (rowid) {
            var userId = $("#userId").val().trim();
            var groupId = $("#GroupId").val();
            var cmp = $("#Cmp").val();
            //var cmpnmvalue = $("#CmpNm").val();
            $UserWhGrid.jqGrid('setCell', rowid, "UserId", userId);
            $UserWhGrid.jqGrid('setCell', rowid, "GroupId", groupId);
            $UserWhGrid.jqGrid('setCell', rowid, "UCmp", cmp);
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
        }
    });

    if(typeof TranTypeData === "object")
    {
        var TranTypeStr = "<input type='checkbox' name='TranType[]' value='*' disabled>ALL";
        $.each(TranTypeData, function (index, val) {
            TranTypeStr += "<input type='checkbox' name='TranType[]' value='" + TranTypeData[index].Cd + "' disabled>" + TranTypeData[index].CdDescp;
        });
        $("#TranTypeArea").append(TranTypeStr);
    }

    schemas = JSON.parse(decodeHtml(schemas));
    CommonFunc.initField(schemas);
    //var rootPath = $("#rootPath").val();
    url = rootPath + "System/UserSetInquiryData";

    MenuBarFuncArr.DelMenu(["MBApply", "MBInvalid"]);

    var options = {};
    options.gridUrl = rootPath + "/Common/GetGroupData";
    options.registerBtn = $("#groupIdLookup");
    options.isMutiSel = true;
    options.param = "&sopt_Type=eq&Type=0";
    options.gridFunc = function (map) {
        //获取Map中的值，然后将值传递给后台重新去抓取整个画面的url
        var GroupId = map.GroupId;
        $("#GroupId").val(GroupId);
    }
    options.responseMethod = function () { }
    options.lookUpConfig = LookUpConfig.GroupLookup;

    initLookUp(options);


    //CopyAccountLookup
    var CopyAccountOpt = {};
    CopyAccountOpt.gridUrl = rootPath + "TPVCommon/GetUserData";
    CopyAccountOpt.param = "";
    CopyAccountOpt.registerBtn = $("#CopyAccountLookup");
    CopyAccountOpt.focusItem = $("#CopyAccount");
    CopyAccountOpt.gridFunc = function (map) {
        $("#CopyAccount").val(map.UId);
        $("#CopyCmp").val(map.Cmp);
    };
    CopyAccountOpt.lookUpConfig = LookUpConfig.UserLookup;
    initLookUp(CopyAccountOpt);

    //主管放大鏡
    var UManagerOptions = {};
    UManagerOptions.gridUrl = url;
    UManagerOptions.registerBtn = $("#UManagerLookup");
    UManagerOptions.focusItem = $("#UManager");
    UManagerOptions.isMutiSel = true;
    UManagerOptions.gridFunc = function (map) {
        var UId = map.UId;
        $("#UManager").val(UId);
    }
    UManagerOptions.responseMethod = function (data) {
        var str = "", manager = [];
        $.each(data, function(index, val) {
            manager.push(data[index].UId);
            //str = str + data[index].UId + ",";
        });
        $("#UManager").val(manager.join(","));
    }
    UManagerOptions.lookUpConfig = LookUpConfig.MultiUserLookup;
    initLookUp(UManagerOptions);

    //集團放大鏡
    var GroupOpt = {};
    GroupOpt.gridUrl = rootPath + "TPVCommon/GetSiteGroupData";
    GroupOpt.param = "";
    GroupOpt.baseCondition = " TYPE='0'";
    GroupOpt.registerBtn = $("#GroupIdLookup");
    GroupOpt.focusItem = $("#GroupId");
    GroupOpt.gridFunc = function (map) {
        var value = map.Cd;
        $("#GroupId").val(value);
    };
    GroupOpt.lookUpConfig = LookUpConfig.SiteLookup;
    initLookUp(GroupOpt);
    CommonFunc.AutoComplete("#GroupId", 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO=", "PARTY_NO=showValue,PARTY_NO", function (event, ui) {
        $(this).val(ui.item.returnValue.PARTY_NO);
        return false;
    });

    //公司放大鏡
    var CmpOpt = {};
    CmpOpt.gridUrl = rootPath + "TPVCommon/GetSiteCmpData1";
    CmpOpt.param = "";
    CmpOpt.baseCondition = " TYPE='1'";
    CmpOpt.registerBtn = $("#CmpLookup");
    CmpOpt.focusItem = $("#Cmp");
    CmpOpt.baseConditionFunc = function () {
        var GroupId = $("#GroupId").val();

        if(GroupId == "")
        {
            alert(_getLang("L_UserSetUp_PlSelectGroup","请先选择集团"));
            return "";
        }
        else
        {
            return " AND GROUP_ID = '"+GroupId+"'";
        }
    }
    CmpOpt.gridFunc = function (map) {
        var value = map.Cmp;
        $("#Cmp").val(value);
    };
    CmpOpt.lookUpConfig = LookUpConfig.SiteLookup1;
    initLookUp(CmpOpt);
    /*CommonFunc.AutoComplete("#Cmp", 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO=", "PARTY_NO=showValue,PARTY_NO", function (event, ui) {
        $(this).val(ui.item.returnValue.PARTY_NO);
        return false;
    });
    */


    //客戶放大鏡
    var PartyOpt = {};
    PartyOpt.gridUrl = rootPath + "TPVCommon/GetGroupData";
    PartyOpt.param = "";
    PartyOpt.registerBtn = $("#PartyLookup");
    PartyOpt.focusItem = $("#Cmp");
    PartyOpt.gridFunc = function (map) {
        //king 這邊抓錯了
        var value = map.PartyNo;
        $("#Cmp").val(value);
    };
    PartyOpt.lookUpConfig = LookUpConfig.SmptyLookup;
    initLookUp(PartyOpt);
    CommonFunc.AutoComplete("#Cmp", 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO=", "PARTY_NO=showValue,PARTY_NO", function (event, ui) {
        $(this).val(ui.item.returnValue.PARTY_NO);
        return false;
    });

    //basecmp
    var baseCmpOpt = {};
    baseCmpOpt.gridUrl = rootPath + "TPVCommon/GetSiteCmpData1";
    baseCmpOpt.param = "";
    baseCmpOpt.baseCondition = " TYPE='1'";
    baseCmpOpt.registerBtn = $("#BaseCmpLookup");
    baseCmpOpt.focusItem = $("#BaseCmp");
    baseCmpOpt.baseConditionFunc = function () {
        var GroupId = $("#GroupId").val();

        if (GroupId == "") {
            alert(_getLang("L_UserSetUp_PlSelectGroup", "请先选择集团"));
            return "";
        }
        else {
            return " AND GROUP_ID = '" + GroupId + "'";
        }
    }
    baseCmpOpt.gridFunc = function (map) {
        var value = map.Cmp;
        $("#BaseCmp").val(value);
    };
    baseCmpOpt.lookUpConfig = LookUpConfig.SiteLookup1;
    initLookUp(baseCmpOpt);



    //部門放大鏡
    var SiteOpt = {};
    SiteOpt.gridUrl = rootPath + "Common/GetDepData";
    SiteOpt.param = "";
    SiteOpt.registerBtn = $("#DepLookup");
    SiteOpt.focusItem = $("#Dep");
    SiteOpt.gridFunc = function (map) {
        var value = map.Cd;
        $("#Dep").val(value);
    };
    SiteOpt.lookUpConfig = LookUpConfig.SiteLookup;
    initLookUp(SiteOpt);
    CommonFunc.AutoComplete("#Dep", 1, "", "dt=bsc&CD_TYPE=DE&GROUP_ID=" + groupId + "&CD=", "CD=showValue,CD", function (event, ui) {
        $(this).val(ui.item.returnValue.CD);
        return false;
    });

    //TCMP Type lookup
    TcmpOptions = {};
    TcmpOptions.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
    TcmpOptions.registerBtn = $("#TcmpLookup");
    TcmpOptions.isMutiSel = true;
    TcmpOptions.focusItem = $("#Tcmp");
    //mutil select add a columnID help mapping selected data
    TcmpOptions.columnID = "Cd";
    TcmpOptions.param = "&sopt_GroupId=eq&GroupId=" + groupId;
    TcmpOptions.param = "";
    TcmpOptions.baseCondition = " GROUP_ID='"+groupId+"' AND CMP='"+cmp+"' AND CD_TYPE='TCMP'";
    TcmpOptions.responseMethod = function (data) {
        var str = "";
        $.each(data, function(index, val) {
            str = str + data[index].Cd + ";";
        });
        $("#Tcmp").val(str);
    }
    TcmpOptions.lookUpConfig = LookUpConfig.MutiBSCodeLookup;
    initLookUp(TcmpOptions);

    var QtmPriOptions = {};
    QtmPriOptions.gridUrl = rootPath + LookUpConfig.GetCmpUrl;
    QtmPriOptions.registerBtn = $("#QtmPriLookup");
    QtmPriOptions.isMutiSel = true;
    QtmPriOptions.focusItem = $("#QtmPri");
    //mutil select add a columnID help mapping selected data
    QtmPriOptions.columnID = "Cmp";
    QtmPriOptions.param = "&sopt_GroupId=eq&GroupId=" + groupId;
    QtmPriOptions.param = "";
    QtmPriOptions.baseCondition = " GROUP_ID='" + groupId + "' AND TYPE='1'";
    QtmPriOptions.responseMethod = function (data) {
        var str = "";
        $.each(data, function (index, val) {
            str = str + data[index].Cmp + ";";
        });
        $("#QtmPri").val(str);
    }
    QtmPriOptions.lookUpConfig = LookUpConfig.MutiLocationLookup;
    initLookUp(QtmPriOptions);

    var SubBgOptions = {};
    SubBgOptions.gridUrl = rootPath + LookUpConfig.PartyNoUrl;
    SubBgOptions.registerBtn = $("#ViewSubBgLookup");
    SubBgOptions.isMutiSel = true;
    SubBgOptions.focusItem = $("#ViewSubBg");
    //mutil select add a columnID help mapping selected data
    SubBgOptions.columnID = "PartyNo";
    SubBgOptions.param = "&sopt_GroupId=eq&GroupId=" + groupId;
    SubBgOptions.param = "";
    SubBgOptions.baseCondition = " GROUP_ID='" + groupId + "'";
    SubBgOptions.responseMethod = function (data) {
        var str = "";
        $.each(data, function (index, val) {
            str = str + data[index].PartyNo + ";";
        });
        $("#ViewSubBg").val(str);
    }
    SubBgOptions.lookUpConfig = LookUpConfig.MultiPartyNoLookup;
    initLookUp(SubBgOptions);


    //PlantPri Type lookup
    PlantPriOptions = {};
    PlantPriOptions.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
    PlantPriOptions.registerBtn = $("#PlantPriLookup");
    PlantPriOptions.isMutiSel = true;
    PlantPriOptions.focusItem = $("#PlantPri");
    //mutil select add a columnID help mapping selected data
    PlantPriOptions.columnID = "Cd";
    PlantPriOptions.param = "&sopt_GroupId=eq&GroupId=" + groupId;
    PlantPriOptions.param = "";
    PlantPriOptions.baseCondition = " GROUP_ID='" + groupId + "' AND CMP='" + cmp + "' AND CD_TYPE='PLT'";
    PlantPriOptions.responseMethod = function (data) {
        var str = "";
        $.each(data, function (index, val) {
            str = str + data[index].Cd + ";";
        });
        $("#PlantPri").val(str);
    }
    PlantPriOptions.lookUpConfig = LookUpConfig.MutiBSCodeLookup;
    initLookUp(PlantPriOptions);

    


    $("input[name='UStatus']").change(function () {
        if (document.getElementById('UStatusYes').checked == true) {
            $("#UStatus").val("0");
        } else if ($("#UStatusNo").checked == true) {
            $("#UStatus").val("1");
        } else {
            $("#UStatus").val("2");
        }
    })
    
    MenuBarFuncArr.MBAdd = function () {
        $("#UEmail").val("");
        $("#UPassword").val("");
        gridEditableCtrl({ editable: true, gridId: "UserWhGrid" });
        document.getElementById('UStatusYes').checked = true;
        document.getElementById('UStatusNo').checked = false;
        if (IoFlag == "I") {
            $('#UStatusLeave').checked = false;
        }
        $("#UStatus").val($("#UStatusYes").val());
        $("#ModiPwDate").removeAttr('required');
        $("#GroupId").val(groupId);
        $("#ModiPwDate").val(90);
        $("#UType").val("L");

        var now = new Date();
        var newDate = DateAdd("d ", 90, now);
        $("#UpdatePriDate").val(getFormatNewDate(newDate));
        $("#Cmp").prop("readonly", false); 
        $("#SubGrid").jqGrid("clearGridData");
        gridEditableCtrl({ editable: false, gridId: "SubGrid" }); 
    }

    MenuBarFuncArr.MBEdit = function () {
        var createBy = $("#CreateBy").val();
        var modifyBy = $("#ModifyBy").val();
        if ("ITSD" == createBy && modifyBy == "") {
            $("#Cmp").attr("isKey", false);
            $("#Cmp").attr('disabled', false);
            $("#CmpLookup").attr("disabled", false);
            oldCmp = $("#Cmp").val();
        } else {
            $("#Cmp").attr("isKey", true);
            $("#Cmp").attr('disabled', true);
            $("#CmpLookup").attr("disabled", true);
        } 
        if (IoFlag == "I") {
            $("#ItSdModal").modal("show");
        }
        gridEditableCtrl({ editable: true, gridId: "UserWhGrid" });
        $("input[name='TranType[]']").prop("disabled", false);
        if ($("#UPri").val() == "G") {
            $("input[name='CmpPri[]']").prop("disabled", false);
        }
        //$("input[name='PlantPri[]']").prop("disabled", false);
        $("#UPassword").removeAttr('required');
        $("#upw").removeClass('required'); 
        gridEditableCtrl({ editable: false, gridId: "SubGrid" }); 
    }

    MenuBarFuncArr.EndFunc = function () {
        //$("#Cmp").prop("disabled", false);
        //$("#CmpLookup").prop("disabled", false);
    }

    MenuBarFuncArr.MBDel = function () {

        /*var removejson = {};
        removejson[$("#userId").attr("fieldname")] = $("#userId").val();
        removejson[$("#groupId").attr("fieldname")] = $("#groupId").val();
        removejson[$("#cmp").attr("fieldname")] = $("#cmp").val();
        removejson[$("#stn").attr("fieldname")] = $("#stn").val();
        removejson["__state"] = "0";*/
        //$('#GroupId').val($("#GroupId option:selected").val());
        var changeData = getAllKeyValue();
        $.ajax({
            async: true,
            url: rootPath + "System/UserSetUpdateData",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
                if (successMsg != "success") return null;
                else alert("success");
                setdisabled(false);
                setToolBtnDisabled(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                top.topManager.closePage();
                //成功后将页面的数据移除，并设置页面不可编辑
                /*setFieldValue(undefined, "");
                setdisabled(true);
                setToolBtnDisabled(false);*/

            }
        });
    }

    MenuBarFuncArr.MBCancel = function () {
        gridEditableCtrl({ editable: false, gridId: "UserWhGrid" });
        MenuBarFuncArr.Enabled(["MBUpdatePwd", "MBRoleAss", "MBRoleLog", "PermissionCopy"]);
        $("#UPassword").attr('required', "required");
        $("#upw").addClass('required', "required");
        initLoadData(UId);
    }


    var searchColumns = {
        caption: "Shipment Search",
        sortname: "UId",
        refresh: false,
        columns: [{ name: "UId", title: "Country ID", width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: "CntyNm", title: "Country Name", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'UId', title: 'User Id', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'GroupId', title: 'Group Id', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Cmp', title: 'Cmp', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'UName', title: 'User Name', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
    }

    MenuBarFuncArr.MBSearch = function (thisItem) {

        var responseMethod = function (rowId) {
            console.log("thisid=" + rowId);
            //wrtie value process method

        }

        var gridMethod = function (map) {
            //获取Map中的值，然后将值传递给后台重新去抓取整个画面的url
            var uId = map.UId;
            var uName = map.UName;

            var param = "sopt_UId=eq&UId=" + uId;
            param += "&sopt_UName=eq&UName=" + uName;
            //将获取的数据作为条件进行reload数据
            $.ajax({
                async: true,
                url: url,
                type: 'POST',
                data: {
                    model: "SysAcctModel",
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
                    var UserBu = result.rows[0].Bu;
                    if (result.rows[0].UStatus == 0) {
                        $("#UStatusYes").prop("checked", true);
                        $("#UStatusNo").prop("checked", false);
                        if (IoFlag == "I") $("#UStatusLeave").prop("checked", false);
                    } else if (result.rows[0].UStatus == 1) {
                        $("#UStatusYes").prop("checked", false);
                        $("#UStatusNo").prop("checked", true);
                        if (IoFlag == "I") $("#UStatusLeave").prop("checked", false);
                    } else{
                        $("#UStatusYes").prop("checked", false);
                        $("#UStatusNo").prop("checked", false);
                        if (IoFlag == "I") $("#UStatusLeave").prop("checked", true);
                    }
                    $("#UStatus").val(result.rows[0].UStatus);
                    //绑定Grid
                    setdisabled(true);
                    setToolBtnDisabled(true);

                    var from = new Date(result.rows[0].ModiPwDate);
                    var to = new Date();

                    to.setDate(from.getDate() + parseInt(120));
                    $("#NextDate").val(to.getFullYear() + "-" + to.getMonth() + "-" + to.getDate());

                    $("#Dep").find("option").each(function(index, el) {
                        if($(this).val() == UserStn)
                        {
                            $(this).prop("selected", true);
                        }
                    });

                    $("#Bu").find("option").each(function(index, el) {
                        if($(this).val() == UserBu)
                        {
                            $(this).prop("selected", true);
                        }
                    });

                }
            });
        }        
        options = {};
        options.gridUrl = url;
        options.registerBtn = thisItem;
        options.isMutiSel = true;
        options.selfSite = true;
        options.gridFunc = gridMethod;
        options.responseMethod = responseMethod;
        options.lookUpConfig = LookUpConfig.UserLookup;
        initLookUp(options);
        //initLookUp(thisItem, searchColumns, url, myColModel, httpUrlModle, gridNames, true,
       //{ caption: "Shipment Search", sortname: "UId", refresh:false,showcolumns:false,refresh:true,exportexcel:true }, gridMethod, responseMethod);
    }

    initMenuBar(MenuBarFuncArr);

    MenuBarFuncArr.AddMenu("MBClearFailCount", "glyphicon glyphicon-repeat", _getLang("L_UserSetUp_resetFT","重置失败次数"), function () {

        var UId = $("#userId").val();
        var GroupId = $("#GroupId").val();
        var Cmp = $("#Cmp").val();
        var Stn = $("#Stn").val();


        $.ajax({
            async: true,
            url: rootPath + "/Home/UserClearFailCount",
            type: 'POST',
            data: { "UId": UId, "GroupId": GroupId, "Cmp": Cmp, "Stn": Stn },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", _getLang("L_UserSetUp_ResetF","重置失败"), 500, "warning");
            },
            success: function (result) {
                if (result.message != "success") {
                    CommonFunc.Notify("", _getLang("L_UserSetUp_ResetF", "重置失败"), 500, "warning");

                }
                CommonFunc.Notify("", _getLang("L_UserSetUp_ResetS", "Reset succeed"), 500, "success");

            }
        });
    });
    MenuBarFuncArr.AddMenu("MBUpdatePwd", "glyphicon glyphicon-repeat", _getLang("L_UserSetUp_ResetPed", "重置密码"), function () {

        var UId = $("#userId").val();
        var UName = $("#UName").val();
        var PartyNo = $("#Cmp").val();
        var UEmail = $("#UEmail").val();
        var GroupId = $("#GroupId").val();
        var Cmp = $("#Cmp").val();
        var Stn = $("#Stn").val();
        var ModiPwDate = $("#ModiPwDate").val();

        if(UId == "" || UEmail == "")
        {
            alert(_getLang("L_UserSetUp_NoBlank", "帐号或email不能为空"));
            return;
        }

        var postData = { "UId": UId, "GroupId": GroupId, "Cmp": Cmp, "Stn": Stn, UName: UName, PartyNo: PartyNo, UEmail: UEmail, ModiPwDate: ModiPwDate };
        console.log(postData);
        $.ajax({
            async: true,
            url: rootPath + "System/updatePwd",
            type: 'POST',
            data: postData,
            dataType: "json",
            beforeSend: function (xmlHttpRequest, successMsg) {
                CommonFunc.ToogleLoading(true);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", _getLang("L_UserSetUp_ResetF", "重置失败"), 500, "warning");
                CommonFunc.ToogleLoading(false);
            },
            success: function (result) {
                if (result.message != "success") {
                    CommonFunc.Notify("", _getLang("L_UserSetUp_ResetF", "重置失败"), 500, "warning");

                } else {
                    CommonFunc.Notify("", _getLang("L_UserSetUp_ResetS", "Reset succeed"), 500, "success");
                    CommonFunc.ToogleLoading(false);
                    initLoadData(UId);
                }
                /*
                CommonFunc.Notify("", _getLang("L_UserSetUp_ResetS", "Reset succeed"), 500, "success");
                CommonFunc.ToogleLoading(false);
                if (typeof result.userData[0].UPassword != "undefined") {
                    delete result.userData[0].UPassword;
                }
                setFieldValue(result.userData);
                setdisabled(true);
                setToolBtnDisabled(false);
                //CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "保存成功"), 500, "success");
                //MenuBarFuncArr.SaveResult = true;
                console.log(result.userData);
                $("#UStatus").val(result.userData[0].UStatus);
                if (result.userData[0].UStatus == 0) {
                    $("#UStatusYes").prop("checked", true);
                    $("#UStatusNo").prop("checked", false);
                    if (IoFlag == "I") $("#UStatusLeave").prop("checked", false);
                }
                else if (result.userData[0].UStatus == 1) {
                    $("#UStatusYes").prop("checked", false);
                    $("#UStatusNo").prop("checked", true);
                    if (IoFlag == "I") $("#UStatusLeave").prop("checked", false);
                }else{
                    $("#UStatusYes").prop("checked", false);
                    $("#UStatusNo").prop("checked", false);
                    if (IoFlag == "I") $("#UStatusLeave").prop("checked", true);
                }


                $("input[name='CmpPri[]']").prop("disabled", true);
                //$("input[name='PlantPri[]']").prop("disabled", true);
                $("input[name='TranType[]']").prop("disabled", true);
                MenuBarFuncArr.Enabled(["MBUpdatePwd"]);
                dtd.resolve();
                */
            }
        });
        
    });
    //MenuBarFuncArr.Disabled(["MBEdoc"]);
    MenuBarFuncArr.MBSave = function (dtd) {
        //if ($("input[name='UStatus']:checked").val() == "" || typeof ($("input[name='UStatus']:checked")) == undefined) {
        //    alert("请选择")
        //}

        // verify this account by TPV api
        if(IoFlag === "I")
        {
            $("#IoFlag").val("I");
        }
        else
        {
            $("#IoFlag").val("O");
        }
        var UPassword = $("#UPassword").val();
        if (!isEmpty(UPassword) && !validatePassword(UPassword)) {
            alert(_getLang("L_ModifyPwd_Check", "不符合密码设置规则"));
            $("#UPassword").val('');
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve(); 
            return false;
        }
        $('#IE').val($("input[type='checkbox'][name='IE']:checked").val());
        $('#KeyUser').val($("input[type='checkbox'][name='KeyUser']:checked").val());
        $('#UStatus').val($("input[type='radio'][name='UStatus']:checked").val());
        $('#MailFlag').val($("input[type='checkbox'][name='MailFlag']:checked").val());
        $('#MsgFlag').val($("input[type='checkbox'][name='MsgFlag']:checked").val());
        $('#DefaultSite').val($("input[type='checkbox'][name='DefaultSite']:checked").val());
        var CmpPri = "";
        var len = $("input[name='CmpPri[]']").length;
        $("input[name='CmpPri[]']").each(function(index, el) {
            var val = $(this).val();
            var checked = $(this).prop("checked");
            if(checked === true)
            {
                if(val === "*")
                {
                    $("#CmpPri").val(val);
                }
                else
                {
                    if(CmpPri == "")
                    {
                        CmpPri += val;
                    }
                    else
                    {
                        CmpPri += ";" + val;
                    }
                }
            }
        });
        $("#CmpPri").val(CmpPri);

        /*
        var PlantPri = "";
        $("input[name='PlantPri[]']").each(function (index, el) {
            var val = $(this).val();

            var checked = $(this).prop("checked");
            if (checked === true) {
                if (val === "*") {
                    $("#PlantPri").val(val);
                }
                else {
                    if (PlantPri == "") {
                        PlantPri += val;
                    }
                    else {
                        PlantPri += ";" + val;
                    }
                }
            }
        });
        $("#PlantPri").val(PlantPri);
        */
        //alert(CmpPri);
        var TranType = "";
        var len = $("input[name='TranType[]']").length;
        var isAll = false;
        $("input[name='TranType[]']").each(function (index, el) {
            var val = $(this).val();
            var checked = $(this).prop("checked");
            if (checked === true || isAll===true) {
                if (val === "*") {
                    $("#TranType").val(val);
                    isAll=true
                }
                else {
                    if (TranType == "") {
                        TranType += val;
                    }
                    else {
                        TranType += ";" + val;
                    }
                }
            }
        });
        //alert(TranType);
        $("#TranType").val(TranType);
        
        var containerArray = $('#UserWhGrid').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();
        changeData["wh"] = containerArray;
        //var changeData = 
        //表示值沒變
        if ($.isEmptyObject(changeData)) {
            CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "保存成功"), 500, "success");
            MenuBarFuncArr.SaveResult = true;
            dtd.resolve();
            setdisabled(true);
            //MenuBarFuncArr.Disabled(["MBSave"]);
            //MenuBarFuncArr.Enabled(["MBDel", "MBEdit", "MBSummary", "MBComfirm", "MBPrint", "MBComfirm", "MBDelivery"]);
            return;
        }
        $.ajax({
            async: true,
            url: rootPath + "System/UserSetUpdateData",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, groupId: $("#GroupId").val(), cmp: $("#Cmp").val(), ItSd: $("#ItSd").val() },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
                //if (successMsg != "success") return null;
                //else alert("success");
                //setdisabled(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", _getLang("L_UserSetUp_ResetS", "重置成功"), 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                
                if (result.message != "success") 
                {
                    var str=result.message;
                    if (str.indexOf(_getLang("L_ModifyPwd_Dup", "重复键"))>-1)
                        CommonFunc.Notify("", _getLang("L_UserSetUp_IDRepeat", "保存失败"), 500, "warning");
                    else 
                       CommonFunc.Notify("", result.message, 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return false;
                }
                //alert(result.message);
                if (typeof result.userData[0].UPassword != "undefined")
                {
                    delete result.userData[0].UPassword;
                }
                setFieldValue(result.userData);
                $UserWhGrid.jqGrid("setGridParam", {
                    datatype: 'local',
                    sortorder: "asc",
                    sortname: "SeqNo",
                    data: result.userWhData
                }).trigger("reloadGrid");
                setdisabled(true);
                setToolBtnDisabled(false);
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "保存成功"), 500, "success");
                MenuBarFuncArr.SaveResult = true;
                console.log(result.userData[0]);
                $("#UStatus").val(result.userData[0].UStatus);
                if (result.userData[0].UStatus == 0)
                {
                    $("#UStatusYes").prop("checked", true);
                    $("#UStatusNo").prop("checked", false);
                    if (IoFlag == "I") $("#UStatusLeave").prop("checked", false);
                }
                else if (result.userData[0].UStatus == 1) {
                    $("#UStatusYes").prop("checked", false);
                    $("#UStatusNo").prop("checked", true);
                    if (IoFlag == "I") $("#UStatusLeave").prop("checked", false);
                }
                else
                {
                    $("#UStatusYes").prop("checked", false);
                    $("#UStatusNo").prop("checked", false);
                    if (IoFlag == "I") $("#UStatusLeave").prop("checked", true);
                }
               

                $("input[name='CmpPri[]']").prop("disabled", true);
                //$("input[name='PlantPri[]']").prop("disabled", true);
                $("input[name='TranType[]']").prop("disabled", true);
                gridEditableCtrl({ editable: false, gridId: "UserWhGrid" });
                MenuBarFuncArr.Enabled(["MBUpdatePwd", "MBRoleAss", "MBRoleLog", "PermissionCopy"]);
                dtd.resolve();
                Cmp = result.userData[0].Cmp;
                GroupId = result.userData[0].GroupId;
                initLoadData(result.userData[0].UId); 
            }
        });
        return dtd.promise();
    }
    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        var data = {};
        data[_handler.key] = uuid();
        $("#userId").val("");
        $("#UName").val("");
        var now = new Date();
        var newDate = DateAdd("d ", 90, now);
        $("#UpdatePriDate").val(getFormatNewDate(newDate));
        $("#UPassword").val("");
        $("#SapId").val("");
        $("#CardNo").val("");
        $("#CreateBy").val("");
        $("#CreateDate").val("");
        $("#ModifyBy").val("");
        $("#ModifyDate").val("");
    }

    MenuBarFuncArr.DelMenu(["MBEdoc", "MBApprove", "MBErrMsg", "MBSearch"]);
    
    setdisabled(true);
    setToolBtnDisabled(true);
     
    initLoadData(UId);

    if(IoFlag == "I")
    {
        $('#GroupId').change(function (e) {
            var target = $(e.target);
            var groupId = $("#GroupId").val();
            $.ajax({
                async: true,
                url: rootPath + "TPVCommon/GetGroupSelect",
                data:{"gType": 1},
                type: 'POST',
                "error": function (xmlHttpRequest, errMsg) {
                    alert(errMsg);
                },
                success: function (result) {
                    var str = "<option value='' selected>" + _getLang("L_UserSetUp_PlSelect", "请先选择")+"</option>";
                    $.each(result.data, function(index, val) {
                        str += "<option value=\"" + result.data[index].Cmp + "\">" + result.data[index].Cmp + ":" + result.data[index].Name + "</option>";
                    });
                    $("#Cmp").html("");
                    $("#Cmp").html(str);
                }
            });
        });

        MenuBarFuncArr.AddMenu("PermissionCopy", "glyphicon glyphicon-bell", _getLang("L_PermissionCopy", "权限复制"), function () {
            $("#PermissionCopyModal").modal("show");
            $("#CopyAccountLookup").removeAttr("disabled");
        });

        $("#CopyAccountBtn").click(function () {
            var copyItsd = $("#CopyItSd").val();
            var copyAccount = $("#CopyAccount").val();
            var copyCmp = $("#CopyCmp").val();
            if (copyItsd == "" || copyAccount == "") {
                CommonFunc.Notify("", _getLang("L_BookingQuery_Script_14", "不允许为空！"), 500, "warning");
                return false;
            }
            var UId = $("#userId").val();
            var Cmp = $("#Cmp").val();
            CommonFunc.ToogleLoading(true);
            $.ajax({
                async: true,
                url: rootPath + "/System/PermissionCopy",
                type: 'POST',
                data: { "UId": UId, "Cmp": Cmp, "CopyAccount": copyAccount, "CopyCmp": copyCmp, "CopyItsd": copyItsd },
                dataType: "json",
                "complete": function (xmlHttpRequest, successMsg) {
                    CommonFunc.ToogleLoading(false);
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.ToogleLoading(false);
                    alert(errMsg);
                },
                success: function (result) {
                    if (result.message != "success") {
                        alert(result.message);
                    } else {
                        CommonFunc.Notify("", "Success", 500, "success");
                    }
                    initLoadData(UId);
                }
            });
            $("#PermissionCopyModal").modal("hide");
        });
    }
    
    
    $('#UpdatePriDate').removeAttr('required');
   

    $("#userId").on("change", function(){
        var val = $(this).val();

        $(this).val(val.toUpperCase());
    });

    $("#UPri").on("change", function(){
        var val = $(this).val();

        if(val === "G")
        {
            $("input[name='CmpPri[]']").prop("disabled", false);
        }
        else
        {
            $("input[name='CmpPri[]']").prop("disabled", true);
            $("input[name='CmpPri[]']").prop("checked", false);
        }
    });

    $("input[name='CmpPri[]']").click(function(){
        var val = $(this).val();
        var checked = $("input[name='CmpPri[]']").prop("checked");

        if(val === "*")
        {
            if(checked === true)
            {
                $("input[name='CmpPri[]']").each(function(index, el) {
                    val = $(this).val();

                    if(val !== "*")
                    {
                        $(this).prop("checked", false);
                        $(this).prop("disabled", true);
                    }
                });;
            }
            else
            {
                $("input[name='CmpPri[]']").each(function(index, el) {
                    val = $(this).val();

                    if(val !== "*")
                    {
                        $(this).prop("disabled", false);
                    }
                });;
            }
        }
    });

    /*
    $("input[name='PlantPri[]']").click(function () {
        var val = $(this).val();
        var checked = $("input[name='PlantPri[]']").prop("checked");

        if (val === "*") {
            if (checked === true) {
                $("input[name='PlantPri[]']").each(function (index, el) {
                    val = $(this).val();

                    if (val !== "*") {
                        $(this).prop("checked", false);
                        $(this).prop("disabled", true);
                    }
                });;
            }
            else {
                $("input[name='PlantPri[]']").each(function (index, el) {
                    val = $(this).val();

                    if (val !== "*") {
                        $(this).prop("disabled", false);
                    }
                });;
            }
        }
    });
    */
    $("input[name='TranType[]']").click(function () {
        var val = $(this).val();
        var checked = $("input[name='TranType[]']").prop("checked");

        if (val === "*") {
            if (checked === true) {
                $("input[name='TranType[]']").each(function (index, el) {
                    val = $(this).val();

                    if (val !== "*") {
                        $(this).prop("checked", false);
                        $(this).prop("disabled", true);
                    }
                });;
            }
            else {
                $("input[name='TranType[]']").each(function (index, el) {
                    val = $(this).val();

                    if (val !== "*") {
                        $(this).prop("disabled", false);
                    }
                });;
            }
        }
    });

    $("#ModiPwDate").on("change", function(){
        var val = parseInt($(this).val());

        var now = new Date();
        var newDate = DateAdd("d ", val, now);
        $("#UpdatePriDate").val(getFormatNewDate(newDate));
    });

    MenuBarFuncArr.AddMenu("MBRoleAss", "glyphicon glyphicon-repeat", _getLang("L_UserSetup_RoleAss", "角色分配"), function () {
        var allRoles = useRole.split(";");
        var selStr = "";
        var myRoles = userRole.split(";");
        setRoleDetail(userRole, "myModalDetail");
        $.each(allRoles, function (index, val) {
            var str = allRoles[index].split(":");
            if (str.length == 2) {
                if (myRoles.indexOf(str[0]) > -1) {
                    selStr += "<input type='checkbox' name='RoleAss[]' value='" + str[0] + "' checked='checked'><a href = 'javascript: void (0);' onclick =\"setRoleDetail('" + str[0] + "','modalDetail')\">" + str[1] + "</a><br/>";
                } else {
                    selStr += "<input type='checkbox' name='RoleAss[]' value='" + str[0] + "'><a href = 'javascript: void (0);' onclick =\"setRoleDetail('" + str[0] + "','modalDetail')\">" + str[1] + "</a><br/>";
                }
            }
        });
        $("#modalbody").html(selStr);
        ajustamodal('#RoleModal', 100, 120);
        $("#RoleModal").modal("show"); 
        $("input[name='RoleAss[]']").click(function () {
            var newRoles = "";
            $("input[name='RoleAss[]']").each(function (index, el) {
                var val = $(this).val();
                var checked = $(this).prop("checked");
                if (checked === false)
                    return true;
                if (newRoles == "") {
                    newRoles += val;
                }
                else {
                    newRoles += ";" + val;
                }
            });
            setRoleDetail(newRoles, 'myModalDetail');
        });
        //$('.tree li:has(ul)').addClass('parent_li').find(' > span').attr('title', 'Collapse this branch');
    });
        var RoleLogQueryurl = rootPath + "/System/GetRoleLog";
        $RoleLogDialogGrid = $("#RoleLogDialogGrid");

        var RoleLogcolModel = [
            { name: 'RoleAdd', title: _getLang("L_UserSetUp_RoleAdd", "增加角色"), index: 'RoleAdd', width: 180, align: 'left', sorttype: 'string', editable: false },
            { name: 'RoleDel', title: _getLang("L_UserSetUp_RoleDel", "移除角色"), index: 'RoleDel', width: 180, align: 'left', sorttype: 'string', editable: false },
            { name: 'CreateBy', title: _getLang("L_Bsdate_ModifyBy", ""), index: 'CreateBy', width: 100, align: 'left', sorttype: 'string', editable: false },
            {
                name: 'CreateDate', title: _getLang("L_Bsdate_ModifyDate", ""), index: 'CreateDate',
                width: 150, align: 'left', sorttype: 'string',
                hidden: false, editable: true, formatter: 'date',
                editoptions: myEditDateInit,
                formatoptions: {
                    srcformat: 'ISO8601Long',
                    newformat: 'Y-m-d h:i A',
                    defaultValue: null
                }

            }
        ];
        new genGrid($RoleLogDialogGrid, {
            datatype: "json",
            loadonce: true,
            cellEdit: false,//禁用grid编辑功能
            colModel: RoleLogcolModel,
            delKey: [""],
            url: RoleLogQueryurl,
            postData: null,
            ds: _dm.getDs("SubGrid"),
            caption: _getLang("TLB_RoleLog", "角色分配修改Log"),
            sortorder: "Desc",
            sortname: "CreateDate",
            height: "auto",
            refresh: true,
            pginput: false,
            sortable: false,
            pgbuttons: false,
            viewrecords: false,
            exportexcel: false,
            onAddRowFunc: function (rowid) {
            }
        });

        MenuBarFuncArr.AddMenu("MBRoleLog", "glyphicon glyphicon-repeat", _getLang("TLB_RoleLog", "角色分配修改Log"), function () {
            var uid = UId;
            if (uid == "") {
                CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", "请先选择一笔"), 500, "warning");
                return false;
            }
            var cmp = $("#Cmp").val();
            var gridheight = $(window).height() - 300;
            $RoleLogDialogGrid.setGridHeight(gridheight);
            $RoleLogDialogGrid.jqGrid('setGridParam', {
                url: RoleLogQueryurl, datatype: "json",
                postData: { UId: uid, Cmp: cmp },
                loadCompleteFunc: function () {
                    $("#RoleLogDetail").modal("show"); //顯示彈出視窗
                    ajustamodal("#RoleLogDetail");
                }
            }).trigger("reloadGrid", [{ page: 1 }]);
        });

        MenuBarFuncArr.AddMenu("MBResume", "glyphicon glyphicon-bell", _getLang("L_UserSetUp_Resume", "复职"), function () {
            var UId = $("#userId").val();
            var Cmp = $("#Cmp").val();
            CommonFunc.ToogleLoading(true);
            $.ajax({
                async: true,
                url: rootPath + "/System/UserResume",
                type: 'POST',
                data: { "UId": UId, "Cmp": Cmp },
                dataType: "json",
                "complete": function (xmlHttpRequest, successMsg) {
                    CommonFunc.ToogleLoading(false);
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.ToogleLoading(false);
                    alert(errMsg);
                },
                success: function (result) {
                    if (result.message != "success") {
                        alert(result.message);
                    } else {
                        CommonFunc.Notify("", "Success", 500, "success");
                    }
                    initLoadData(UId);
                }
            });
        }); 

    $("#AddRoleBtn").on("click", function () {
        var delRoles = userRole;
        var myRoles = userRole.split(";");
        var newRoles = "";
        $("input[name='RoleAss[]']").each(function (index, el) {
            var val = $(this).val();
            var checked = $(this).prop("checked");
            if (checked === false)
                return true;
            if (myRoles.indexOf(val) < 0) {
                if (newRoles == "") {
                    newRoles += val;
                }
                else {
                    newRoles += ";" + val;
                }
            } else {
                delRoles = delRoles.replace(val, '');
            }
        });
        if (UId == "") {
            UId = $("#userId").val();
            GroupId = $("#GroupId").val();
            Cmp = $("#Cmp").val();
            Stn = $("#Stn").val();
        }
        $.ajax({
            async: false,
            url: rootPath + "System/SaveUserPermission",
            type: 'POST',
            data: { "delRoles": delRoles, "newRoles": newRoles, "UId": UId, "GroupId": GroupId, "Cmp": Cmp, "Stn": Stn },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (result) {
                if (result.message != "success") {
                    CommonFunc.Notify("", result.message, 500, "warning");
                    return false;
                }
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "Save Successful"), 500, "success");
                userRole = result.userRole;
                $("#RoleModal").modal("hide");
            }
        });
    });

        var colModel1 = [
            { name: 'GroupId', title: _getLang("L_UserSetUp_GroupId", ""), index: 'GroupId', width: 80, align: 'left', sorttype: 'string', editable: false },
            { name: 'Cmp', title: _getLang("L_UserSetUp_Cmp", ""), index: 'Cmp', width: 80, align: 'left', sorttype: 'string', editable: false },
            { name: 'UserId', title: _getLang("L_UserSetUp_U_ID", ""), index: 'UserId', width: 80, align: 'left', sorttype: 'string', editable: false },
            { name: 'FieldCode', title: _getLang("L_UserSetUp_FieldCode", "栏位ID"), index: 'FieldCode', width: 120, align: 'left', sorttype: 'string', editable: false },
            { name: 'FieldName', title: _getLang("L_UserSetUp_FieldName", "FIELD NAME"), index: 'FieldName', width: 120, align: 'left', sorttype: 'string', editable: false },
            { name: 'OldValue', title: _getLang("L_UserSetUp_OldValue", "修改前内容"), index: 'OldValue', width: 80, align: 'left', sorttype: 'string', editable: false },
            { name: 'UpdateValue', title: _getLang("L_UserSetUp_UpdateValue", "修改后内容"), index: 'UpdateValue', width: 80, align: 'left', sorttype: 'string', editable: false },
            { name: 'ItSd', title: "ITSD#", index: 'ItSd', width: 80, align: 'left', sorttype: 'string', editable: false, hidden: IoFlag != "I" },
            { name: 'ModifyBy', title: _getLang("L_Bsdate_ModifyBy", ""), index: 'ModifyBy', width: 80, align: 'left', sorttype: 'string', editable: false },
            {
                name: 'ModifyDate', title: _getLang("L_Bsdate_ModifyDate", ""), index: 'ModifyDate',
                width: 150, align: 'left', sorttype: 'string',
                hidden: false, editable: true, formatter: 'date',
                editoptions: myEditDateInit,
                formatoptions: {
                    srcformat: 'ISO8601Long',
                    newformat: 'Y-m-d h:i A',
                    defaultValue: null
                }

            }
        ];
        var titleLog = _getLang("L_UserSetUpO_Log", "外部用户建档修改Log");
        if (IoFlag == "I")
            titleLog = _getLang("L_UserSetUp_Log", "用户建档修改Log");
        _handler.intiGrid("SubGrid", $SubGrid, {
            colModel: colModel1, caption: titleLog, delKey: ["UserId", "Cmp"],
            onAddRowFunc: function (rowid) {
            },
            beforeSelectRowFunc: function (rowid) {
            },
            beforeAddRowFunc: function (rowid) {
            }
        });

        $("#AddItSdBtn").click(function () {
            var ItSdM = $("#ItSdM").val();
            if (ItSdM == "") {
                CommonFunc.Notify("", _getLang("L_BookingQuery_Script_14", "不允许为空！"), 500, "warning");
                return false;
            }
            $("#ItSd").val(ItSdM);
            $("#ItSdModal").modal("hide");
        });

    $("#seepwd").mousedown(function () {
        $("#UPassword").attr("type", "text");
    });
    $("#seepwd").mouseup(function () {
        $("#UPassword").attr("type", "password");
    });

    $("#pwdgroup").focusin(function () {
        $("#seepwd").css("z-index", "100");
    });
    $("#pwdgroup").focusout(function () {
        $("#seepwd").css("z-index", "-100");
    });
});

/*取得冷链質押画面相关的选择选项*/
function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "TPVCommon/GetGroupSelect",
        type: 'POST',
        data:{"gType": 0},
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (result) {
            var str = "<option value='' selected>" + _getLang("L_UserSetUp_PlSelect", "请先选择") + "</option>";
            $.each(result.data, function(index, val) {
                str += "<option value=\"" + result.data[index].GroupId + "\">" + result.data[index].GroupId + ":" +result.data[index].Name + "</option>";
            });
            $("#GroupId").html("");
            $("#GroupId").html(str);
            //appendSelectOption($("#GroupId"), deOptions);
        }
    });

    $.ajax({
        async: true,
        url: rootPath + "Common/GetDepData",
        type: 'POST',
        dataType:"json",
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (result) {
            var str = "<option value='' selected>" + _getLang("L_UserSetUp_PlSelect", "请先选择") + "</option>";
            $.each(result.rows, function(index, val) {
                str += "<option value=\"" + result.rows[index].Cd + "\">" + result.rows[index].Cd + ":" +result.rows[index].CdDescp + "</option>";
            });
            $("#Dep").html("");
            $("#Dep").html(str);
        }
    });


    /*$.ajax({
        async: true,
        url: rootPath + "Common/GetSelectOptions",
        type: 'POST',
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var deOptions = data.De,
            appendSelectOption($("#Dep"), deOptions);
        }
    });*/
}
/*function getGroupIdSelectOptions() {
    $("#GroupId").empty();
    $("#GroupId").append("<option value='' selected>" + "请选择" + "</option>");
}
function getCmpSelectOptions() {
    $("#Cmp").empty();
    $("#Cmp").append("<option value='' selected>" + "请选择" + "</option>");
    $("#Cmp").append("<option value='" + cmp + "'>" + cmp + "</option>");
}*/
function appendUserSelectOption(selectId, options) {
    selectId.empty();
    $.each(options, function (idx, option) {

        selectId.append("<option value=\"" + option.cd + "\">" + option.cdDescp + "</option>");

    });
    selectId.append("<option value='' selected>" + _getLang("L_UserSetUp_PlSelect", "请先选择") + "</option>");
}

/*设置select的选项*/
function appendSelectOption(selectId, options) {
    selectId.empty();
    var str = "<option value='' selected>" + _getLang("L_UserSetUp_PlSelect", "请先选择") + "</option>";
    $.each(options, function (idx, option) {
        str += "<option value=\"" + option.cd + "\">" + option.cdDescp + "</option>";
    });

    selectId.append(str);
}

function getToday () {
    var today = new Date();
    var dd = today.getDate();
    var mm = today.getMonth()+1; //January is 0!
    var yyyy = today.getFullYear();

    if(dd<10) {
        dd='0'+dd
    } 

    if(mm<10) {
        mm='0'+mm
    } 

    today = yyyy+"/"+ (mm<10 ? '0' : '') + mm+"/"+ (dd<10 ? '0' : '') + dd;

    return today;
}

function DateAdd(interval, number, date) {
    switch (interval) {
    case "y ": {
        date.setFullYear(date.getFullYear() + number);
        return date;
        break;
    }
    case "q ": {
        date.setMonth(date.getMonth() + number * 3);
        return date;
        break;
    }
    case "m ": {
        date.setMonth(date.getMonth() + number);
        return date;
        break;
    }
    case "w ": {
        date.setDate(date.getDate() + number * 7);
        return date;
        break;
    }
    case "d ": {
        date.setDate(date.getDate() + number);
        return date;
        break;
    }
    case "h ": {
        date.setHours(date.getHours() + number);
        return date;
        break;
    }
    case "m ": {
        date.setMinutes(date.getMinutes() + number);
        return date;
        break;
    }
    case "s ": {
        date.setSeconds(date.getSeconds() + number);
        return date;
        break;
    }
    default: {
        date.setDate(d.getDate() + number);
        return date;
        break;
    }
    }
}

function getFormatNewDate(newDate)
{
    var dd = newDate.getDate();
    var mm = newDate.getMonth()+1; //January is 0!
    var yyyy = newDate.getFullYear();
    return yyyy+"/"+ (mm<10 ? '0' : '') + mm+"/"+ (dd<10 ? '0' : '') + dd;
}

function setRoleDetail(id, model) {
    var str = "";
    $("#" + model).html(str);
    $.ajax({
        async: false,
        url: rootPath + "System/GetMenuItemByRole",
        type: 'POST',
        data: { "allData": id },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (result) {
            var content = $.parseJSON(result);
            str = "<lable style='font-size:24px'>" + _getLang("L_UserSetup_userPermission", "用户权限") + "</lable>";
            if ("modalDetail" == model)
                str = "<lable style='font-size:24px'>" + _getLang("L_UserSetup_rolePermission", "角色权限") + "</lable>";
            str += "<ul for='allDetail'>";
            $.each(content, function (index, val) {
                var data = content[index];
                if (data.menu.length == 0)
                    return true;

                var permission = "";
                $.each(data.menu, function (idx, obj) {
                    var role = data.menu[idx];
                    if (role.permision.length == 0)
                        return true;
                    permission += "<li for='" + role.roleId + "' class='parent_li'><span title='Collapse this branch'><i class='icon-minus-sign'></i>" + role.roleText + "</span><ul for='" + role.roleId + "'>";

                    $.each(role.permision, function (i, v) {
                        var pms = role.permision[i];
                        permission += "<li for='" + pms.pmsId + "'><span><i class='icon-leaf'></i>" + pms.caption + "</span></li>";
                    });
                    permission += "</ul></li>";
                });
                if (!permission == "")
                    str += "<li for='" + data.hId + "' class='parent_li'><span title='Collapse this branch'><i class='icon-folder-open'></i>" + data.hText + "</span><ul for='" + data.hId + "'>" + permission + "</ul></li>";
            });
            str += "</ul>";
            $("#" + model).html(str);
        }
    });
    $('.tree li.parent_li > span').off('click');
    $('.tree li.parent_li > span').on('click', function (e) {
        var children = $(this).parent('li.parent_li').find(' > ul > li');
        if (children.is(":visible")) {
            children.hide('fast');
            $(this).attr('title', 'Expand this branch').find(' > i').addClass('icon-plus-sign').removeClass('icon-minus-sign');
        } else {
            children.show('fast');
            $(this).attr('title', 'Collapse this branch').find(' > i').addClass('icon-minus-sign').removeClass('icon-plus-sign');
        }
        e.stopPropagation();
    });
}

function getCmpCondition() {
    var isAll = false;
    var CmpPri = "'" + $("#Cmp").val() + "'";
    $("input[name='CmpPri[]']").each(function (index, el) {
        var val = $(this).val();
        var checked = $(this).prop("checked");
        if (checked === true || isAll === true) {
            if (val === "*") {
                $("#CmpPri").val(val);
                isAll = true
            }
            else {
                CmpPri += ",'" + val + "'";
            }
        }
    });
    if (isAll)
        return "";
    return " CMP IN (" + CmpPri + ")";
}

function validatePassword(password) {
    // Rule 1: Password length must be at least 10 characters
    if (password.length < 10) {
        return false;
    }

    // Rule 2: Must contain at least one uppercase letter, one lowercase letter, one digit, and one special character
    const containsUpperCase = /[A-Z]/.test(password);
    const containsLowerCase = /[a-z]/.test(password);
    const containsDigit = /[0-9]/.test(password);
    const containsSpecialChar = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password);

    if (!(containsUpperCase && containsLowerCase && containsDigit && containsSpecialChar)) {
        return false;
    }

    // Rule 3: Prohibit consecutive characters or digits repeated more than 3 times
    if (/(.)\1{2,}/i.test(password.toLowerCase())) {
        return false;
    }

    if (/abc|bcd|cde|def|efg|fgh|ghi|hij|ijk|jkl|klm|lmn|mno|nop|opq|pqr|qrs|rst|stu|tuv|uvw|vwx|wxy|xyz|123|234|345|456|567|678|789/.test(password.toLowerCase())) {
        return false;
    }

    // Rule 4: Prohibit using more than 4 consecutive keyboard characters
    const keyboardSequences = ['1234', '2345', '3456', '4567', '5678', '6789', '7890', '890-', '90-=',
        '!@#$', '@#$%', '#$%^', '$%^&', '%^&*', '^&*(', '&*()', '*()_', '()_+',
        'qwer', 'wert', 'erty', 'rtyu', 'tyui', 'yuio', 'uiop', 'iop[', 'iop{', 'op[]', 'op{}', 'p[]\\', 'p{}|',
        'asdf', 'sdfg', 'dfgh', 'fghj', 'ghkj', 'hjkl', 'jkl;', 'jkl:', 'kl:"',
        'zxcv', 'xcvb', 'cvbn', 'vbnm', 'bnm,', 'bnm<', 'nm,.', 'nm<>', 'm,./', 'm<>?',
        '1qaz', '2wsx', '3edc', '4rfv', '5tgb', '6yhn', '7ujm', '8ik,', '9ol.', '0p;/',
        '!qaz', '@wsx', '#edc', '$rfv', '%tgb', '^yhn', '&ujm', '*ik,', '(ol.', ')p;/'];
    for (let sequence of keyboardSequences) {
        if (password.toLowerCase().includes(sequence)) {
            return false;
        }
    }

    // Rule 5: Prohibit using common or easily guessable passwords
    const commonPasswords = ['admin', 'administrator', 'root', 'password'];
    if (commonPasswords.includes(password.toLowerCase())) {
        return false;
    }

    // If the password passes all rules, return true
    return true;
}