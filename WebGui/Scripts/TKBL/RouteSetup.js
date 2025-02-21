var $MainGrid = $("#MainGrid");
var $SubGrid = $("#SubGrid");
var colModel1, colModel2;

var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var _mainCmpData = {},//由放大镜查询出来的公司数据
    mainKeyValue = {},
    gridSetting = {};

var groupId = getCookie("plv3.passport.groupid"),
    cmp = getCookie("plv3.passport.companyid"),
    stn = getCookie("plv3.passport.station"),
    userId = getCookie("plv3.passport.user");

var _loader = {};
var _subEdit = 0;//是否开启编辑子表 0不开启  1准备开启  >1开启
$(function () {
    setdisabled(true);
    setToolBtnDisabled(true);
    _initModel();//初始化列实体

    gridSetting.gridId = "SubGrid";
    gridSetting.colModel = colModel2;

    _intiGrid($MainGrid, {
        colModel: colModel1, caption: '@Resources.Locale.L_RouteSetup_TranList', delKey: ["UId"],
        onSelectRowFunc: function (map) {
            if (mainKeyValue.UId) {
                gridSetting.keyData = { UId: mainKeyValue.UId };
                if (_dm.getDs("SubGrid"))
                    getGridChangeDataDS(gridSetting);
            }

            mainKeyValue.UId = map.UId;
            var UId = map.UId;

            if (!isEmpty(UId)) {
                if (_subEdit == 1) {
                    _subEdit++;
                    gridEditableCtrl({ editable: true, gridId: "SubGrid" });
                }
                //此处增加判断，如果tempDeatiArray存在该笔数据的资料，则加载缓存，如果没有则通过请求来更新
                if (_oldDeatiArray[UId] != undefined || _oldDeatiArray[UId] != null) {
                    //将json设置给GoodsCInfoGrid
                    //移除_state状态为0的数据，，因为_state的数据是删除的数据
                    $.each(_oldDeatiArray[UId], function (i, val) {
                        if (val._state == "0") {
                            _oldDeatiArray[UId].splice(i, 1);
                        }
                    });
                    _dm.getDs("SubGrid").setData(_oldDeatiArray[UId]);
                    return;
                }
                if (!_loader[UId]) {
                    _loader[UId] = true;
                    loadSubData(UId);
                }
            }
        },
        delRowFunc: function (rowid) {
            _dm.getDs("SubGrid").setData([]);
            return true;
        },
        onAddRowFunc: function (rowid) {
            //var maxSeqNo = $('#IpbtmGrid').jqGrid("getCol", "SeqNo", false, "max");
            //if (typeof maxSeqNo === "undefined")
            //    maxSeqNo = 0;
            //$MainGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
            $MainGrid.jqGrid('setCell', rowid, "UId", uuid());
            $MainGrid.jqGrid('setCell', rowid, "Cmp", _mainCmpData.Cmp);
        }
    });
    _intiGrid($SubGrid, {
        colModel: colModel2, caption: "Detail", delKey: ["UId", "SeqNo"],
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            $SubGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
        }
    });
    _dm.addDs("MainGrid", [], ["UId"], $MainGrid[0]);
    _dm.addDs("SubGrid", [], ["UId", "SeqNo"], $SubGrid[0]);
    $MainGrid.jqGrid('setGridParam', {
        ds: _dm.getDs("MainGrid")
    });

    $SubGrid.jqGrid('setGridParam', {
        ds: _dm.getDs("SubGrid")
    });

    _initBar();//初始化工具栏

    function loadSubData(UId) {
        $.ajax({
            async: true,
            url: rootPath + "TKBL/GetSubRoutData",
            type: 'POST',
            data: {
                uid: UId
            },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
                if (successMsg != "success") return null;
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                var mainTable = $.parseJSON(result.mainTable.Content);
                var $grid = $SubGrid;
                _oldDeatiArray[UId] = mainTable.rows;
                if (_dm.getDs("SubGrid") == null || _dm.getDs("SubGrid") == undefined) {
                    _dm.addDs("SubGrid", mainTable.rows, ["UId", "SeqNo"], $grid[0]);
                } else {
                    _dm.getDs("SubGrid").setData(mainTable.rows);
                }
            }
        });
    }

    function loadMainData(map) {
        _loader = {};
        _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
        _changeDatArray = [];
        mainKeyValue = {};

        _dm.getDs("SubGrid").setData([]);
        if (map) {
            $("#txt_Cmp").val(map.Cmp);
            $("#txt_CmpName").val(map.Name);
        }
        _mainCmpData = map;
        $MainGrid.jqGrid('setGridParam', {
            url: rootPath + "TKBL/GetRoutData", datatype: "json",
            postData: {
                'conditions': "&sopt_Cmp=eq&Cmp=" + map.Cmp || ''//jQuery.serialize()已经是进行URL编码过的。
            }
        }).trigger("reloadGrid");
    }

    function _intiGrid(jqGrid, op) {
        //{ colModel: colModel2,caption: "",delKey: ["UId"]};
        op = $.extend({
            datatype: "local",
            loadonce: true,
            data: [],
            height: "auto",
            refresh: true,//是否允许刷新
            cellEdit: false,//禁用grid编辑功能
            exportexcel: false,//是否可导出excel
            pginput: false,//禁止bar上有输入框
            sortable: false,//禁止排序
            pgbuttons: false,//是否有分页按钮
            toppager: true,//是否有头上那个分页的bar
            rows: 200
        }, op);
        return new genGrid(jqGrid, op);
    }

    function _initModel() {
        function getTermop(name) {
            var _name = name;
            var term_op = getLookupOp("MainGrid",
                {
                    url: rootPath + LookUpConfig.TermUrl,
                    config: LookUpConfig.TermLookup,
                    returnFn: function (map, $grid) {
                        return map.Cd;
                    }
                }, LookUpConfig.GetCodeTypeAuto(groupId, "TD", $MainGrid,
                function ($grid, rd, elem) {
                }));
            return term_op;
        }

        function getTranModeop(name) {
            var _name = name;
            var tranmode_op = getLookupOp("MainGrid",
                {
                    url: rootPath + LookUpConfig.TrackingTranModeUrl,
                    config: LookUpConfig.TranModeLookup,
                    returnFn: function (map, $grid) {
                        return map.Cd;
                    }
                }, LookUpConfig.GetCodeTypeAuto(groupId, "TNT", $MainGrid,
                function ($grid, rd, elem) {
                }));
            return tranmode_op;
        }

        function getcust(name) {
            var _name = name;
            var type;
            switch (_name) {
                case "ShipTo":
                    type = "ST";
                    break;
                case "Carrier":
                    type = "CA";
                    break;
            }
            var cust_op = getLookupOp("MainGrid",
                {
                    url: rootPath + LookUpConfig.PartyNoUrl,
                    config: LookUpConfig.PartyNoLookup,
                    returnFn: function (map, $grid) {
                        var rowid = $grid.jqGrid('getGridParam', 'selrow');
                        //$grid.jqGrid('setCell', selRowId, _name, map.PartyName);
                        setGridVal($grid, rowid, _name, map.PartyNo, "lookup");
                        return map.PartyNo;
                    }
                }, LookUpConfig.GetPartyNoAuto(groupId, $MainGrid,
                function ($grid, rd, elem) {
                    //var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    //$grid.jqGrid('setCell', selRowId, _name, rd.PARTY_NAME);
                }), {
                    //baseConditionFunc: function () {
                    //    //var selRowId = $("#ApproveGroup").jqGrid('getGridParam', 'selrow');
                    //    //var AuId = $("#ApproveGroup").jqGrid("getCell", selRowId, "AuId");
                    //    //return " U_FID = '"+AuId+"'";
                    //    switch (_name) {
                    //        case "ShipTo":
                    //            return "PARTY_TYPE LIKE '%WE%'";
                    //        case "Carrier":
                    //            return "PARTY_TYPE LIKE '%CA%'";
                    //    }
                    //      return "";
                    //}
                });
            return cust_op;
        }

        function getop(name) {
            var _name = name;
            var city_op = getLookupOp("MainGrid",
                {
                    url: rootPath + LookUpConfig.CityPortUrl,
                    config: LookUpConfig.CityPortLookup,
                    returnFn: function (map, $grid) {
                        //var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                        //$grid.jqGrid('setCell', selRowId, _name, map.CntyCd + map.CityCd);
                        return map.CntryCd + map.PortCd;
                    }
                }, LookUpConfig.GetCityPortAuto(groupId, $MainGrid,
                function ($grid, rd, elem) {
                    $(elem).val(rd.CNTRY_CD + rd.PORT_CD);
                }));
            return city_op;
        }

        function getStatusop(name) {
            var _name = name;
            var status_op = getLookupOp("SubGrid",
                {
                    url: rootPath + LookUpConfig.StatusUrl,
                    config: LookUpConfig.StatusLookup,
                    returnFn: function (map, $grid) {
                        var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                        //$grid.jqGrid('setCell', selRowId, _name, map.Ldescp);
                        //setGridCurVal($grid, selRowId, _name, map.Ldescp);
                        setGridVal($grid, selRowId, _name, map.Edescp, null);
                        return map.StsCd;
                    }
                }, LookUpConfig.GetStatusAuto(groupId, $SubGrid,
                function ($grid, rd, elem, rowid) {
                    //var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    //$grid.jqGrid('setCell', selRowId, _name, rd.LDESCP);
                    //setGridCurVal($grid, selRowId, _name, rd.LDESCP);
                    //alert(rowid);
                    setGridVal($grid, rowid, _name, rd.EDESCP, null);
                    $(elem).val(rd.STS_CD);
                }), { param:""});
            return status_op;
        }

        colModel1 = [
	    { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
        { name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', editable: false, hidden: true },
        //{ name: 'TranMode', title: '運輸類別', index: 'TranMode', sorttype: 'string', hidden: false, editable: true, width: 80, formatter: "select", edittype: 'select', editoptions: { value: select_tranmode, defaultValue: default_tranmode } },
        { name: 'TranMode', title: '@Resources.Locale.L_DNApproveManage_TranMode', index: 'TranMode', sorttype: 'string', hidden: false, editable: true, width: 80, editoptions: gridLookup(getTranModeop("Pol")), edittype: 'custom' },
        //{ name: 'Term', title: '貿易條款', index: 'Term', sorttype: 'string', hidden: false, editable: true, width: 90, formatter: "select", edittype: 'select', editoptions: { value: select_term, defaultValue: default_term } },
        { name: 'Term', title: '@Resources.Locale.L_PartyDocSetup_Term', index: 'Term', sorttype: 'string', hidden: false, editable: true, width: 90, editoptions: gridLookup(getTermop("Pol")), edittype: 'custom' },
        { name: 'Pol', title: '@Resources.Locale.L_BaseLookup_PolCd', index: 'Pol', editoptions: gridLookup(getop("Pol")), width: 120, sorttype: 'string', hidden: false, edittype: 'custom', editable: true },
        { name: 'Via', title: '@Resources.Locale.L_BaseLookup_ViaCd', index: 'Via', editoptions: gridLookup(getop("Via")), width: 120, sorttype: 'string', hidden: false, edittype: 'custom', editable: true },
        { name: 'Pod', title: '@Resources.Locale.L_BaseLookup_PodCd', index: 'Pod', editoptions: gridLookup(getop("Pod")), width: 120, sorttype: 'string', hidden: false, edittype: 'custom', editable: true },
        { name: 'Dlv', title: '@Resources.Locale.L_RouteSetup_Dlv', index: 'Dlv', editoptions: gridLookup(getop("Dlv")), width: 120, sorttype: 'string', hidden: false, edittype: 'custom', editable: true },
        { name: 'Carrier', title: '@Resources.Locale.L_DNApproveManage_CaCd', index: 'Carrier', editoptions: gridLookup(getcust("Carrier")), sorttype: 'string', hidden: false, edittype: 'custom', editable: true, width: 120 },
        { name: 'ShipTo', title: '@Resources.Locale.L_DNApproveManage_ShipTo', index: 'ShipTo', editoptions: gridLookup(getcust("ShipTo")), sorttype: 'string', hidden: false, edittype: 'custom', editable: true, width: 120 },
        { name: 'TT', title: '@Resources.Locale.L_RouteSetup_Tt', index: 'TT', sorttype: 'string', hidden: false, editable: true, width: 60 },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', sorttype: 'string', hidden: false, editable: true, width: 150 }
        ];

        colModel2 = [
            { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
            { name: 'SeqNo', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'SeqNo', sorttype: 'string', hidden: true, editable: false },
            { name: 'StsCd', title: '@Resources.Locale.L_AirTransport_StsCd', index: 'StsCd', editoptions: gridLookup(getStatusop("StsDescp")), sorttype: 'string', hidden: false, edittype: 'custom', editable: true, width: 90 },
            { name: 'StsDescp', title: '@Resources.Locale.L_AirTransport_StsDescp', index: 'StsDescp', sorttype: 'string', hidden: false, editable: false, width: 190 },
            { name: 'NstsCd', title: '@Resources.Locale.L_RouteSetup_NstsCd', index: 'NstsCd', editoptions: gridLookup(getStatusop("NstsDescp")), sorttype: 'string', hidden: false, edittype: 'custom', editable: true, width: 90 },
            { name: 'NstsDescp', title: '@Resources.Locale.L_AirTransport_StsDescp', index: 'NstsDescp', sorttype: 'string', hidden: false, editable: false, width: 190 },
            { name: 'WorkingHour', title: '@Resources.Locale.L_RouteSetup_WorkingHour', index: 'WorkingHour', width: 60, align: 'right', editable: true, formatter: 'integer', hidden: false },
            { name: 'Location', title: '@Resources.Locale.L_AirTransport_Location', index: 'Location', sorttype: 'string', hidden: false, editable: true, width: 150 },
            { name: 'SendType', title: '@Resources.Locale.L_RouteSetup_SendType', index: 'SendType', sorttype: 'string', hidden: false, editable: true, width: 80 },
            { name: 'SendDescp', title: '@Resources.Locale.L_RouteSetup_SendDescp', index: 'SendDescp', sorttype: 'string', hidden: false, editable: true, width: 190 },
            { name: 'MailTo', title: '@Resources.Locale.L_RouteSetup_MailTo', index: 'MailTo', sorttype: 'string', hidden: false, editable: true, width: 200 }
        ];
    }

    function _endGrid($grid) {//结束grid的编辑状态
        var selRowId = $grid.jqGrid('getGridParam', 'selrow');
        $grid.jqGrid('saveRow', selRowId, false, 'clientArray');
        $grid.jqGrid('getGridParam', "endEdit")();
    }

    function _initBar() {

        /*按下上方修改後，要做的事*/
        MenuBarFuncArr.MBEdit = function () {
            if (isEmpty(_mainCmpData.Cmp)) {
                alert('@Resources.Locale.L_RouteSetup_SelectCom');
                return false;
            }

            gridEditableCtrl({ editable: true, gridId: "MainGrid" }); //使Grid出现「Add Row」button, 按下add row为新增新的一行
            if (!isEmpty(mainKeyValue.UId)) {
                _subEdit = 2;
                gridEditableCtrl({ editable: true, gridId: "SubGrid" }); //使Grid出现「Add Row」button, 按下add row为新增新的一行
            }
            else
                _subEdit = 1;
            editable = true;
        }

        MenuBarFuncArr.EndFunc = function () {
            $("#CmpLookup").attr('disabled', true);
            $("#txt_Cmp").attr('disabled', true);
        }

        /*按下上方Menu撤消後，要做的事*/
        MenuBarFuncArr.MBCancel = function () {
            MenuBarFuncArr.Enabled(["MBEdit"]);
            _endGrid($MainGrid);
            _endGrid($SubGrid);

            gridEditableCtrl({ editable: false, gridId: "MainGrid" });
            gridEditableCtrl({ editable: false, gridId: "SubGrid" });
            loadMainData(_mainCmpData);
            editable = false;
            _subEdit = 0;
        }

        MenuBarFuncArr.CancelStatus = function () {
            //console.log(_dataSource);
            setFieldValue(_dataSource || []);
            setdisabled(true);
            //setToolBtnDisabled(true);
            StatusBarArr.nowStatus('@Resources.Locale.L_RouteSetup_Browse');
            MenuBarFuncArr.Disabled(["MBSave", "MBCancel"]);
            if (_dataSource.length == 0) {
                MenuBarFuncArr.Enabled(["MBAdd", "MBSearch"]);
            }
            else {
                MenuBarFuncArr.Enabled(["MBAdd", "MBEdit", "MBDel", "MBCopy", "MBEdoc", "MBSearch", "MBApprove", "MBInvalid", "MBSummary"]);
            }
            $("#txt_Cmp").removeAttr('disabled');
            $("#CmpLookup").removeAttr('disabled');
        }

        MenuBarFuncArr.MBSearch = function (thisItem) {
            var gridMethod = function (map) {
                console.log(map);
                loadMainData(map);
            }

            var options = {};
            options.gridUrl = rootPath + "/TKBL/GetRoutGroupData"; //查询的url, rootPath为网站根目录
            options.registerBtn = thisItem;
            options.isMutiSel = true;
            options.selfSite = true;
            options.gridFunc = gridMethod;
            //options.param = "&sopt_Type=eq&Type=1";
            options.param = "";
            options.lookUpConfig = LookUpConfig.CmpLookup; //查询条件在Scripts/BaseLookup.js设定
            initLookUp(options);
        }

        function beforSave($grid, nullCols) {
            var rowIds = $grid.getDataIDs();
            for (var i = 0; i < rowIds.length; i++) {
                var rowDatas = $grid.jqGrid('getRowData', rowIds[i]);
                for (var j = 0; j < nullCols.length; j++) {
                    var col = nullCols[j];
                    if (isEmpty($.trim(rowDatas[col.name]))) {
                        $grid.jqGrid("editCell", rowIds[i], col.index, true);
                        CommonFunc.Notify("", col.text + "@Resources.Locale.L_RouteSetup_NoEmp", 2000, "warning");
                        return false;
                    }
                }
            }
        }

        MenuBarFuncArr.MBSave = function (dtd) {
            _endGrid($MainGrid);
            _endGrid($SubGrid);

            var nullCols = [];
            nullCols.push({ name: "TranMode", index: 3, text: "@Resources.Locale.L_RouteSetup_Tran_mode" });
            nullCols.push({ name: "Term", index: 4, text: "@Resources.Locale.L_RouteSetup_Term" });
            if (beforSave($MainGrid, nullCols) === false) {
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
                return false;
            }

            //获取子表的changeValue
            gridSetting.keyData = { UId: mainKeyValue.UId };
            getGridChangeDataDS(gridSetting);

            var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
            var changeData = {};
            changeData["mt"] = containerArray;
            changeData["sub"] = _changeDatArray;
            console.log(_changeDatArray);
            $.ajax({
                async: true,
                url: rootPath + "TKBL/SaveRoutData",
                type: 'POST',
                data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true },
                dataType: "json",
                "complete": function (xmlHttpRequest, successMsg) {
                },
                "error": function (xmlHttpRequest, errMsg) {
                    alert(errMsg);
                    CommonFunc.Notify("", errMsg, 500, "danger");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                },
                success: function (result) {
                    if (result.message !== "success") {
                        CommonFunc.Notify("", result.message, 1000, "danger");
                        MenuBarFuncArr.SaveResult = false;
                        dtd.resolve();
                        return;
                    }
                    //alert(result.message);
                    setdisabled(true);
                    setToolBtnDisabled(true);
                    CommonFunc.Notify("", "@Resources.Locale.L_RouteSetup_SaveSuccess", 500, "success");
                    MenuBarFuncArr.SaveResult = true;
                    dtd.resolve();

                    $("#txt_Cmp").removeAttr('disabled');
                    $("#CmpLookup").removeAttr('disabled');

                    MenuBarFuncArr.MBCancel();
                }
            });

            return dtd.promise();
        }

        initMenuBar(MenuBarFuncArr);

        MenuBarFuncArr.DelMenu(["MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid", "MBErrMsg"]);  //Delete不需要的Menu
        MenuBarFuncArr.Disabled(["MBSave"]); //Disable Menu
        MenuBarFuncArr.Enabled(["MBEdit"]);  //Enabled Menu

        registBtnLookup($("#CmpLookup"), {
            item: '#txt_Cmp', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
                $("#txt_Cmp").val(map.PartyNo);
                $("#txt_CmpName").val(map.PartyName);
                _mainCmpData = { Cmp: map.PartyNo, Name: map.PartyName };
                MenuBarFuncArr.MBCancel();
            }
        }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
            $("#txt_Cmp").val(rd.PARTY_NO);
            $("#txt_CmpName").val(rd.PARTY_NAME);
            _mainCmpData = { Cmp: rd.PARTY_NO, Name: rd.PARTY_NAME };
            MenuBarFuncArr.MBCancel();
        }));

        $("#txt_Cmp").lookuptrig = true;
        $("#txt_Cmp").val(cmp);
        $("#txt_Cmp").blur();

        $("#txt_Cmp").removeAttr('disabled');
        $("#CmpLookup").removeAttr('disabled');
        //$("#txt_CmpName").hide();
        //_mainCmpData = { Cmp: cmp };
        //MenuBarFuncArr.MBCancel();
    }
});

function createColItem(name, title, op) {
    var opt = { name: name, title: title, index: name, sorttype: 'string', editable: true, hidden: false };
    return $.extend(opt, op);
}

function isEmpty(val) {
    if (val === undefined || val === "" || val == null)
        return true;
    return false;
}

function getLookupOp(gridId, op, op1, op2) {
    //op = op || { url: rootPath + "Common/GetCountryData", config: config, returnFn: function () { }, autoFn: function () { } };
    var $grid = $("#" + gridId);
    var opt = {};
    opt.gridUrl = op.url;
    opt.selfSite = false;
    opt.param = "";
    opt.gridReturnFunc = function (map) {
        //var selRowId = $grid.jqGrid('getGridParam', 'selrow');
        //$grid.jqGrid('setCell', selRowId, 'CntryNm', map.CntyNm);
        if (op.returnFn)
            return op.returnFn(map, $grid);
    };
    opt.lookUpConfig = op.config;
    opt.gridId = gridId;

    //自动带入
    opt.autoCompKeyinNum = 1;
    opt.autoCompUrl = "";
    //opt.autoCompDt = "dt=country&GROUP_ID=" + groupId + "&CNTY_CD%";
    //opt.autoCompParams = "CNTY_NM&CNTY_CD=showValue,CNTY_CD,CNTY_NM";

    //opt.autoCompFunc = function (elem, event, ui) {
    //    $(elem).val(ui.item.returnValue.CNTY_CD);
    //    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
    //    $grid.jqGrid('setCell', selRowId, 'CntryNm', ui.item.returnValue.CNTY_NM);
    //}
    opt = $.extend(opt, op1);
    opt = $.extend(opt, op2);
    return opt;
}

//function registLookup(id, op, callbackFn) {
//    var options = {};
//    options.gridUrl = op.url;
//    options.registerBtn = $("#" + id);
//    if (op.isMutiSel) options.isMutiSel = true;
//    options.param = op.param;
//    options.gridFunc = function (map) {
//        //获取Map中的值，然后将值传递给后台重新去抓取整个画面的url
//        callbackFn(map);
//    }
//    options.responseMethod = function () { }
//    options.lookUpConfig = op.config;
//    initLookUp(options);
//}

function registBtnLookup(item, op, op1, op2) {
    //{url:"",config:""}
    var options = {};
    options.gridUrl = op.url;
    options.registerBtn = item;
    if (op.isMutiSel) options.isMutiSel = true;
    options.param = op.param;
    options.gridFunc = function (map) {
        //获取Map中的值，然后将值传递给后台重新去抓取整个画面的url
        if (op.selectRowFn)
            op.selectRowFn(map);
    }
    options.responseMethod = function () { }
    options.lookUpConfig = op.config;
    if (op1)
        options = $.extend(options, op1);
    initLookUp(options);

    if (op2 && op.item) {
        CommonFunc.AutoComplete(op.item, 1, "", op2.autoCompDt, op2.autoCompParams,
            function (event, ui) {
                if (op2.autoCompFunc) {
                    op2.autoCompFunc($(op.item), event, ui);
                }
                return false;
            });
    }
}

function uuid() {
    var s = [];
    var hexDigits = "0123456789abcdef";
    for (var i = 0; i < 36; i++) {
        s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1);
    }
    s[14] = "4";  // bits 12-15 of the time_hi_and_version field to 0010
    s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1);  // bits 6-7 of the clock_seq_hi_and_reserved to 01
    s[8] = s[13] = s[18] = s[23] = "-";

    var uuid = s.join("");
    return uuid.replace(/-/g, "");
}
// 用不到了
function setGridCurVal($grid, selRowId, name, val) {
    var ds = $grid.jqGrid("getGridParam", "ds");
    if (ds && ds.setCurVal) {
        //alert(val);
        ds.setCurIndexByKey($grid.jqGrid("getRowData", selRowId));
        ds.setCurVal(name, val);
    }
}