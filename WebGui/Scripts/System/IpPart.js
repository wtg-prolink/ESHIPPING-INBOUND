var url = ""
var groupId = getCookie("plv3.passport.groupid"),
    cmp = getCookie("plv3.passport.companyid"),
    stn = getCookie("plv3.passport.station"),
    userId = getCookie("plv3.passport.user"),
    Supplier = "",
    PartNo = "";

var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];

function initLoadData(SupplierCd, PartNo) {

    if (!SupplierCd || !PartNo)
        return;
    var param = "sopt_PartNo=eq&PartNo=" + PartNo + "&sopt_SupplierCd=eq&SupplierCd="+SupplierCd+"&IM.GROUP_ID=SD&IM.CMP=SD&IM.STN=DLC";
    $.ajax({
        async: true,
        url: rootPath + "System/IpPartInquiryData",
        type: 'POST',
        data: {
            sidx: 'PartNo',
            'conditions': encodeURI(param),
            page: 1,
            rows: 20
        },
        dataType: "json",
        beforeSend: function () {
            CommonFunc.ToogleLoading(true);
        },
        "complete": function (xmlHttpRequest, successMsg) {
            if (successMsg != "success") return null;
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {

            _dataSource = result.rows;
            setFieldValue(result.rows);
            var Supplier = result.rows[0].SupplierCd;
            var PartNo = result.rows[0].PartNo;
            var stn = result.rows[0].Stn;
            var groupId = result.rows[0].GroupId;
            var postdata = { "conditions": "sopt_1=ne&1=1" };
            if (groupId && Supplier && PartNo && stn) {
                postdata = { "conditions": "sopt_GroupId=eq&GroupId=" + groupId + "&sopt_SupplierCd=eq&Supplier=" + Supplier + "&sopt_PartNo=eq&PartNo=" + PartNo + "&sopt_Stn=eq&Stn=" + stn };
            } 

            $("#PartdGrid").jqGrid("setGridParam", {
                url: rootPath + "System/PartdInquiryData",
                postData: postdata,
                page: 1,
                datatype: "json"
            }).trigger("reloadGrid");

            gridEditableCtrl({ editable: false, gridId: "PartdGrid" });
            MenuBarFuncArr.initEdoc($("#SupplierCd").val() + $("#PartNo").val());
            setdisabled(true);
            setToolBtnDisabled(true);
            MenuBarFuncArr.Disabled(["MBSave"]);
            MenuBarFuncArr.Enabled(["MBAdd", "MBCopy", "MBDel", "MBEdit", "MBApprove", "MBEdoc"]);
            CommonFunc.ToogleSearch(false); //资料汇总切换
            CommonFunc.ToogleLoading(false);
            var Cntry = result.rows[0].Cntry;
            console.log(result.rows);
            $.ajax({
                async: true,
                url: rootPath + "IPBTM/GetGoodsCnty",
                type: 'POST',
                data: {
                    Goods:result.rows[0].Goods
                },
                "error": function (xmlHttpRequest, errMsg) {
                    //alert(errMsg);
                },
                success: function (result) {
                    var results = result.message;
                   
                    if (results.length > 0) {
                        appendCntyrOption($("#Cntry"), results, Cntry);
                    } else {
                        CommonFunc.Notify("", "@Resources.Locale.L_IpPart_Script_187", 1000, "warning");
                        $("#Cntry").empty();
                        $("#CntryNm").val('');
                    }
                }
            });
        }
    });
}


jQuery(document).ready(function ($) {

    url = rootPath + "System/IpPartInquiryData";
    getSelectOptions();

    getGridData();

    


	MenuBarFuncArr.MBCancel = function (){
        $("#PartdGrid").jqGrid("clearGridData");
        gridEditableCtrl({ editable: false, gridId: "PartdGrid" });
        var NowSupplier = $("#SupplierCd").val();
        var NowPartNo   = $("#PartNo").val();
	    var postdata = { "conditions": "sopt_1=ne&1=1" };
        if (groupId && NowSupplier && NowPartNo && stn) {
            postdata = { "conditions": "sopt_GroupId=eq&GroupId=" + groupId + "&sopt_SupplierCd=eq&Supplier=" + NowSupplier + "&sopt_PartNo=eq&PartNo=" + NowPartNo + "&sopt_Stn=eq&Stn=" + stn };
        } 

        $("#PartdGrid").jqGrid("setGridParam", {
            url: rootPath + "System/PartdInquiryData",
            postData: postdata,
            page: 1,
            datatype: "json"
        }).trigger("reloadGrid");
	}

	MenuBarFuncArr.MBDel = function () {
	    var changeData = getAllKeyValue();
	    $.ajax({
	        async: true,
	        url: rootPath + "System/IpPartUpdateData",
	        type: 'POST',
	        data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false },
	        dataType: "json",
	        "complete": function (xmlHttpRequest, successMsg) {
	            if (successMsg != "success")
	                return null;
	            else
	                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelS", 500, "success");
	            setdisabled(false);
	            setToolBtnDisabled(false);
	        },
	        "error": function (xmlHttpRequest, errMsg) {
	        },
	        success: function (result) {
                if (result.message !== "success") {
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                    return;
                }	            
	            //成功后将页面的数据移除，并设置页面不可编辑
	            setFieldValue();
	            setdisabled(true);
	            setToolBtnDisabled(false);
                $("#PartdGrid").jqGrid("clearGridData");
	        }
	    });
	}

	MenuBarFuncArr.MBAdd = function (thisItem) {
	    $("#CreateBy").val(userId);
	    $("#Cmp").val(cmp);
	    $("#Stn").val(stn);
	    $("#Bound").val("N");
	    $("#CheckFlag").val("N");
	    $("#PaperBox").val("N");
        $("#PartdGrid").jqGrid("clearGridData");

        gridEditableCtrl({ editable: true, gridId: "PartdGrid" });
	}

    MenuBarFuncArr.MBEdit = function () {
        $("#PartNo").prop("readonly", true);
        gridEditableCtrl({ editable: true, gridId: "PartdGrid" });
    }

	MenuBarFuncArr.MBCopy = function (thisItem) {
	    //$("#CreateBy").val(userId);
	    //$("#Cmp").val(cmp);
	    //$("#Stn").val(stn);
	    //$("#Bound").val("N");
	    //$("#CheckFlag").val("N");
	    //$("#PaperBox").val("N");
	    $("#PartNo").val("");
	    //$("#Descp").val("");
	    //$("#Cdescp").val("");
        $("#PartdGrid").jqGrid("clearGridData");
        gridEditableCtrl({ editable: true, gridId: "PartdGrid" });
	}

    //放大镜
	var ipPartSearchOptions = {};
	ipPartSearchOptions.gridUrl = url;
	ipPartSearchOptions.registerBtn = $("#MBSearch");
	ipPartSearchOptions.isMutiSel = true;
	ipPartSearchOptions.gridFunc = function (map) {
	    var supplierCd = map.SupplierCd,
                partNo = map.PartNo,
                groupId = map.GroupId;

	    var param = "sopt_SupplierCd=eq&SupplierCd=" + supplierCd;
	    param += "&sopt_PartNo=eq&PartNo=" + partNo;
	    param += "&sopt_GroupId=eq&GroupId=" + groupId;

        
	    //将获取的数据作为条件进行reload数据
	    $.ajax({
	        async: true,
	        url: url,
	        type: 'POST',
	        data: {
	            model: "IpPartModel",
	            sidx: 'PartNo',
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
                $("#PartdGrid").jqGrid("clearGridData");
	            _dataSource = result.rows;
	            setFieldValue(result.rows);
                Supplier = result.rows[0].SupplierCd;
                PartNo = result.rows[0].PartNo;
                var Cntry = result.rows[0].Cntry;
                var postdata = { "conditions": "sopt_1=ne&1=1" };
                if (groupId && Supplier && PartNo && stn) {
                    postdata = { "conditions": "sopt_GroupId=eq&GroupId=" + groupId + "&sopt_SupplierCd=eq&Supplier=" + Supplier + "&sopt_PartNo=eq&PartNo=" + PartNo + "&sopt_Stn=eq&Stn=" + stn };
                } 
                $("#PartdGrid").jqGrid("setGridParam", {
                    url: rootPath + "System/PartdInquiryData",
                    postData: postdata,
                    page: 1,
                    datatype: "json"
                }).trigger("reloadGrid");

                $.ajax({
                    async: true,
                    url: rootPath + "IPBTM/GetGoodsCnty",
                    type: 'POST',
                    data: {
                        Goods:result.rows[0].Goods
                    },
                    "error": function (xmlHttpRequest, errMsg) {
                        //alert(errMsg);
                    },
                    success: function (result) {
                        var results = result.message;
                        Cuntry = results;
                        if (results.length > 0) {
                            appendCntyrOption($("#Cntry"), results, Cntry);
                        } else {
                            CommonFunc.Notify("", "@Resources.Locale.L_IpPart_Script_187", 1000, "warning");
                            $("#Cntry").empty();
                            $("#CntryNm").val('');
                        }
                    }
                });
	        }
	    });
	}
	ipPartSearchOptions.responseMethod = function () { }
	ipPartSearchOptions.lookUpConfig = LookUpConfig.PartLookup;
	MenuBarFuncArr.MBSearch = function (thisItem) {
	    initLookUp(ipPartSearchOptions);
	}

    //notice MBSave一定要傳入dtd
	MenuBarFuncArr.MBSave = function (dtd) {
	    if (checkNoAllowNullFields() == false) {
	        MenuBarFuncArr.SaveResult = false;
	        dtd.resolve();
	        return false;
	    }
	    var SupplierCd = $("#SupplierCd").val();
	    var PartNo = $("#PartNo").val();
	    var Cntry = $("#Cntry").val();
        var containerArray = $('#PartdGrid').jqGrid('getGridParam', "arrangeGrid")();

	    var changeData = getChangeValue();
        changeData["st"] = containerArray;
        console.log(changeData);

	    $.ajax({
	        //notice async一定要為false
	        //async: true,
	        url: rootPath + "System/IpPartUpdateData",
	        type: 'POST',
	        data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), "SupplierCd": SupplierCd, "PartNo": PartNo, "Cntry": Cntry, autoReturnData: true },
	        dataType: "json",
	        "complete": function (xmlHttpRequest, successMsg) {
	        },
	        "error": function (xmlHttpRequest, errMsg) {
	            //notice ajax error 一定要放入下面三行
	            CommonFunc.Notify("", errMsg, 500, "danger");
	            MenuBarFuncArr.SaveResult = false;
	            dtd.resolve();
	        },
	        success: function (result) {
	            if (result.message != "success")
	            {
	                //notice ajax warning 一定要放入下面三行
	                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
	                MenuBarFuncArr.SaveResult = false;
	                dtd.resolve();

	                console.log(result.message);
                    return;
                }
	            setdisabled(true);
	            setToolBtnDisabled(true);

                $("#containerInfoGrid").jqGrid("setGridParam", {
                    datatype: 'json',
                    data: result.ippartData
                }).trigger("reloadGrid");

	            //notice ajax success 一定要放入下面三行
	            CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
	            MenuBarFuncArr.SaveResult = true;
	            dtd.resolve();
	        }
	    });
	    //notice ajax 最後一定要 return promise
	    return dtd.promise();
	}

    MenuBarFuncArr.AddMenu("MBSummary", "glyphicon glyphicon-search", "@Resources.Locale.L_IpPart_Scripts_364", function () {
        CommonFunc.ToogleSearch(true); //资料汇总切换
    });
	
	initMenuBar(MenuBarFuncArr);
	MenuBarFuncArr.DelMenu(["MBApply", "MBApprove", "MBInvalid", "MBErrMsg"]);

    setdisabled(true);
    setToolBtnDisabled(true);


    //品号放大镜
    options = {};
    options.gridUrl = rootPath + "Common/GetCoData";
    options.registerBtn = $("#GoodsLookup");
    options.isMutiSel = true;
    options.focusItem = $("#Goods");
    options.param = "&sopt_GroupId=eq&GroupId=" + groupId;
    options.baseCondition = " (BU='C' OR BU='D' OR BU='E')";
    options.gridFunc = function (map) {
        var cd = map.Goods,
            descp = map.GoodsDescp;
        $("#Goods").val(cd);
        $("#GoodsDescp").val(descp);

        //根据取得的goods去抓取对应的国别
        $.ajax({
            async: true,
            url: rootPath + "IPBTM/GetGoodsCnty",
            type: 'POST',
            data: {
                Goods:cd
            },
            "error": function (xmlHttpRequest, errMsg) {
                //alert(errMsg);
            },
            success: function (result) {
                var results = result.message;
                Cuntry = results;
                if (results.length > 0) {
                    appendCntyrOption($("#Cntry"), results);
                } else {
                    CommonFunc.Notify("", "@Resources.Locale.L_IpPart_Script_187", 1000, "warning");
                    $("#Cntry").empty();
                    $("#CntryNm").val('');
                }
            }
        });

    }
    options.responseMethod = function () { }
    options.lookUpConfig = LookUpConfig.GoodsLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#Goods", 2, "", "dt=igd&GROUP_ID=" + groupId + "&GOODS=", "GOODS=showValue,GOODS,GOODS_DESCP", function (event, ui) {
        //set return value to other field
        $("input[name='GoodsDescp']").val(ui.item.returnValue.GOODS_DESCP);
        //set return to register field 
        $(this).val(ui.item.returnValue.GOODS);
        $.ajax({
            async: true,
            url: rootPath + "IPBTM/GetGoodsCnty",
            type: 'POST',
            data: {
                Goods:ui.item.returnValue.GOODS
            },
            "error": function (xmlHttpRequest, errMsg) {
                //alert(errMsg);
            },
            success: function (result) {
                var results = result.message;
                Cuntry = results;
                if (results.length > 0) {
                    appendCntyrOption($("#Cntry"), results);
                } else {
                    CommonFunc.Notify("", "@Resources.Locale.L_IpPart_Script_187", 1000, "warning");
                    $("#Cntry").empty();
                    $("#CntryNm").val('');
                }
            }
        });
        return false;
    });

    //供应商放大镜
    options = {};
    options.gridUrl = rootPath + "Common/GetSupplierData";
    options.registerBtn = $("#SupplierCdLookup");
    options.isMutiSel = true;
    options.selfSite = true;
    options.focusItem = $("#SupplierCd");
    options.gridFunc = function (map) {
        var custCd = map.CustCd,
            localNm = map.LocalNm;
        $("#SupplierCd").val(custCd);
        $("#SupplierNm").val(localNm);
    }
    options.responseMethod = function () { }
    options.lookUpConfig = LookUpConfig.BSCSVLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#SupplierCd", 2, "", "dt=cus&GROUP_ID=" + groupId + "&CMP=" + cmp + "&STN=" + stn + "&CUST_TYPE~V&CUST_CD=", "CUST_CD=showValue,CUST_CD,LOCAL_NM", function (event, ui) {
        //set return value to other field
        $("input[name='SupplierNm']").val(ui.item.returnValue.LOCAL_NM);
        //set return to register field 
        $(this).val(ui.item.returnValue.CUST_CD);
        return false;
    });

    //币别放大镜
    options = {};
    options.gridUrl = rootPath + "Common/GetCrncyData";
    options.registerBtn = $("#CrncyLookup");
    options.isMutiSel = true;
    options.selfSite = true;
    options.focusItem = $("#Crncy");
    options.gridFunc = function (map) {
        var cur = map.Cur;
        $("#Crncy").val(cur);
    }
    options.responseMethod = function () { }
    options.lookUpConfig = LookUpConfig.CurLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#Crncy", 1, "", "dt=crn&GROUP_ID=" + groupId + "&CUR=", "CUR=showValue,CUR,CUR_DESCP", function (event, ui) {
        $(this).val(ui.item.returnValue.CUR);
        return false;
    });

    //类别放大镜
    options = {};
    options.gridUrl = rootPath + "Common/GetPartTypeData";
    options.registerBtn = $("#PartTypeLookup");
    options.focusItem = $("#PartType");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        console.log(map);
        var Cd = map.Cd;
        $("#PartType").val(Cd);
    }
    options.responseMethod = function () { }
    options.lookUpConfig = LookUpConfig.PartTypeLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#PartType", 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE=S01&CD=", "CD_DESCP&CD=showValue,CD,CD_DESCP", function (event, ui) {
        $(this).val(ui.item.returnValue.CD);
        return false;
    });

    //杂别别放大镜
    options = {};
    options.gridUrl = rootPath + "Common/GetPartOthData";
    options.registerBtn = $("#PartOthLookup");
    options.focusItem = $("#PartOth");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        var Cd = map.Cd;
        $("#PartOth").val(Cd);
    }
    options.responseMethod = function () { }
    options.lookUpConfig = LookUpConfig.PartOthLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#PartOth", 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE=S02&CD=", "CD_DESCP&CD=showValue,CD,CD_DESCP", function (event, ui) {
        $(this).val(ui.item.returnValue.CD);
        return false;
    });

    //编号放大镜
    options = {};
    options.gridUrl = rootPath + "Common/GetPartCdData";
    options.registerBtn = $("#PartCdLookup");
    options.focusItem = $("#PartCd");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        var Cd = map.Cd;
        $("#PartCd").val(Cd);
    }
    options.responseMethod = function () { }
    options.lookUpConfig = LookUpConfig.PartCdLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#PartCd", 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE=S03&CD=", "CD_DESCP&CD=showValue,CD,CD_DESCP", function (event, ui) {
        $(this).val(ui.item.returnValue.CD);
        return false;
    });

    //包装放大镜
    options = {};
    options.gridUrl = rootPath + "Common/GetPartPkgData";
    options.registerBtn = $("#PartPkgLookup");
    options.focusItem = $("#PartPkg");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        var Cd = map.Cd;
        $("#PartPkg").val(Cd);
    }
    options.responseMethod = function () { }
    options.lookUpConfig = LookUpConfig.PartPkgLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#PartPkg", 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE=S04&CD=", "CD_DESCP&CD=showValue,CD,CD_DESCP", function (event, ui) {
        $(this).val(ui.item.returnValue.CD);
        return false;
    });

    //工厂放大镜
    options = {};
    options.gridUrl = rootPath + "Common/GetMafNoData";
    options.registerBtn = $("#MafNoLookup");
    options.focusItem = $("#MafNo");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        var Cd = map.Cd;
        $("#MafNo").val(Cd);
    }
    options.responseMethod = function () { }
    options.lookUpConfig = LookUpConfig.MafNoLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#MafNo", 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE=S05&CD=", "CD_DESCP&CD=showValue,CD,CD_DESCP", function (event, ui) {
        $(this).val(ui.item.returnValue.CD);
        return false;
    });
});

/*取得冷链采购画面相关的选择选项*/
function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "Common/GetSelectOptions",
        type: 'POST',
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var deOptions = data.De,
                buOptions = data.Bu,
                luOptions = data.Lu;
            appendSelectOption($("#Dep"), deOptions);
            appendSelectOption($("#Bu"), buOptions);
        }
    });
}

/*设置select的选项*/
function appendSelectOption(selectId, options) {
    selectId.empty();
    $.each(options, function (idx, option) {
        selectId.append("<option value=\"" + option.cd + "\">" + option.cdDescp + "</option>");
    });
}

function appendCntyrOption(selectId, options, Cntry) {
    selectId.empty();
    $.each(options, function (idx, option) {
        if(typeof Cntry == "undefined")
        {
            /*if (idx == 0) {
                selectId.append("<option value=\"" + option.CntryCd + "\" selected >" + option.CntryCd + "</option>");
            } else {
                selectId.append("<option value=\"" + option.CntryCd + "\">" + option.CntryCd + "</option>");
            }*/
            selectId.append("<option value=\"" + option.CntryCd + "\">" + option.CntryCd + "</option>");
        }
        else
        {
            var selected = "";
            if(option.CntryCd == Cntry)
            {
                selected = "selected";
            }

            selectId.append("<option value=\"" + option.CntryCd + "\" "+selected+" >" + option.CntryCd + "</option>");
        }
        
    });

    if(typeof Cntry == "undefined")
    {
        $("#Cntry").val("");
    }
}

function getGridData()
{
    var StnOpt = {};
    StnOpt.gridUrl = rootPath + "Common/GetStnData";
    StnOpt.gridReturnFunc = function (map) {
        var value = map.Stn;

        var col = $("#PartdGrid").jqGrid('getCol', 'Stn', false);//获取批文号码列的值
        var isexist = false;
        $.each(col, function (index, colname) {
            if (colname == value) {
                CommonFunc.Notify("", "@Resources.Locale.L_IpPart_Script_188", 700, "warning");
                isexist = true;
            }
        });
        if (isexist) {
            return "";
        }
        
        return value;
    };
    StnOpt.lookUpConfig = LookUpConfig.StnLookup;
    StnOpt.autoCompKeyinNum = 1;
    StnOpt.gridId = "PartdGrid";
    StnOpt.autoCompUrl = "";
    StnOpt.autoCompDt = "dt=stn&GROUP_ID=" + groupId + "&CMP=" + cmp + "&STN%";
    StnOpt.autoCompParams = "STN=showValue,STN";
    StnOpt.autoCompFunc = function (elem, event, ui) {
        console.log(ui);
        $(elem).val(ui.item.returnValue.Stn);
    }

    var colModel=  [
        { name: 'Stn', title: '@Resources.Locale.L_IpPart_Stn', index: 'Stn', width: 200, align: 'left', sorttype: 'string', editable: true, edittype:'custom', editoptions: gridLookup(StnOpt)},
        { name: 'DeclDescp', title: '@Resources.Locale.L_IpPart_PartdGrid', index: 'DeclDescp', width: 200, align: 'left', sorttype: 'string', editable: true},
        { name: 'GroupId', title: 'Group ID', index: 'GroupId', sorttype: 'string', hidden: true },
        { name: 'Stn', title: 'Station', index: 'Stn', sorttype: 'string', hidden: true },
        { name: 'PartNo', title: 'Part No', index: 'PartNo', sorttype: 'string', hidden: true },
        { name: 'Supplier', title: 'Supplier', index: 'Supplier', sorttype: 'string', hidden: true }
    ];

    $grid = $("#PartdGrid");
    _dm.addDs("PartdGrid", [], ["GroupId", "Stn", "PartNo", "Supplier"], $grid[0]);
    var postdata = { "conditions": "sopt_1=ne&1=1" };
    if (groupId && Supplier && PartNo && stn) {
        postdata = { "conditions": "sopt_GroupId=eq&GroupId=" + groupId + "&sopt_SupplierCd=eq&Supplier=" + Supplier + "&sopt_PartNo=eq&PartNo=" + PartNo + "&sopt_Stn=eq&Stn=" + stn };
    } 
    new genGrid($grid, {
        datatype: "json",
        loadonce: true,
        cellEdit: false,//禁用grid编辑功能
        colModel: colModel,
        delKey: ["GroupId", "Stn", "PartNo", "SupplierCd"],
        url: rootPath + "System/PartdInquiryData",
        postData: postdata,
        ds: _dm.getDs("PartdGrid"),
        isModel:true,
        caption: "@Resources.Locale.L_IpPart_PartdGrid",
        height: "auto",
        refresh: true,
        pginput: false,
        sortable: false,
        pgbuttons: false,
        viewrecords: false,
        exportexcel: false,
        onAddRowFunc: function (rowid) {
            var Supplier = $("#SupplierCd").val(), PartNo = $("#PartNo").val();

            if(Supplier == "" || PartNo == "")
            {
                CommonFunc.Notify("", "@Resources.Locale.L_IpPart_Scripts_368", 500, "warning");
                $("#PartdGrid").jqGrid("clearGridData");
                return false;
            }

            $('#PartdGrid').jqGrid('setCell', rowid, "PartNo", PartNo);
            $('#PartdGrid').jqGrid('setCell', rowid, "Supplier", Supplier);
        },
    });
}