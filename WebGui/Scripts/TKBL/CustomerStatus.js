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
        colModel: colModel1, caption: 'Party Type', delKey: ["UId"],
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
            setGridVal($MainGrid, rowid, "UId", uuid());
            setGridVal($MainGrid, rowid, "Cmp", _mainCmpData.Cmp);
        }
    });
    _intiGrid($SubGrid, {
        colModel: colModel2, caption: '@GetLangText("L_CusStatus_ShipmentStatus")', delKey: ["UId", "UFid"],
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            setGridVal($SubGrid, rowid, "SeqNo", maxSeqNo + 1);
            setGridVal($SubGrid, rowid, "UFid", mainKeyValue.UId);
            setGridVal($SubGrid, rowid, "ViewOp", "Y");
            setGridVal($SubGrid, rowid, "UploadOp", "N");
        }
    });
    _dm.addDs("MainGrid", [], ["UId"], $MainGrid[0]);
    _dm.addDs("SubGrid", [], ["UId", "UFid"], $SubGrid[0]);
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
            url: rootPath + "TKBL/GetSubCustomerStatus",
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
                //var mainTable = $.parseJSON(result.mainTable.Content);
                var mainTable = result;
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
            url: rootPath + "TKBL/GetCustomerStatus", datatype: "json",
            postData: {
                cmp: map.Cmp || ''
                //'conditions': "&sopt_Cmp=eq&Cmp=" + map.Cmp || ''//jQuery.serialize()已经是进行URL编码过的。
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
        function checkSameValue($grid, rowId, sameCols) {
            var rowIds = $grid.getDataIDs();
            for (var i = 0; i < rowIds.length; i++) {
                if (rowIds[i] === rowId)
                    continue;
                var rowDatas1 = $grid.jqGrid('getRowData', rowIds[i]);
                for (var x = 0; x < sameCols.length; x++) {
                    var col = sameCols[x];
                    if ($.trim(col.val) === $.trim(rowDatas1[col.name])) {
                        try {
                            //$grid.jqGrid("editCell", rowIds[i], col.index, true);
                        }
                        catch (e) {
                        }
                        CommonFunc.Notify("", "@Resources.Locale.L_SystemController_Same" + col.text + ":" + $.trim(rowDatas1[col.name]), 2000, "warning");
                        return false;
                    }
                }
            }
        }

        function getop(_partygrid, _$partygrid, name) {
            var _name = name;
            var op = getLookupOp(_partygrid,
                {
                    url: rootPath + LookUpConfig.PartyTypeUrl,
                    config: LookUpConfig.PartyTypeLookup,
                    returnFn: function (map, $grid) {
                        var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                        setGridVal($grid, selRowId, 'PartyDescp', map.CdDescp, null);
                        setGridVal($grid, selRowId, 'PartyType', map.Cd, 'lookup');
                        return map.Cd;
                    }
                }, LookUpConfig.GetCodeTypeAuto(groupId, "PT", _$partygrid, function ($grid, rd, elem, rowid) {
                    var selRowId = rowid;
                    setGridVal($grid, selRowId, 'PartyDescp', rd.CD_DESCP, null);
                    setGridVal($grid, selRowId, 'PartyType', rd.CD, 'lookup');
                    //$(elem).val(rd.CD);
                }, function ($grid, elem, rowid) {
                    var selRowId = rowid;
                    setGridVal($grid, selRowId, 'PartyDescp', "", null);
                    setGridVal($grid, selRowId, 'PartyType', "", 'lookup');
                    $(elem).val("");
                }), {
                param: "",
                baseConditionFunc: function () {
                    return "";
                }
            });
            return op;
        }

        function getStatusop(name) {
            var _name = name;
            var status_op = getLookupOp("SubGrid",
                {
                    url: rootPath + LookUpConfig.StatusUrl,
                    config: LookUpConfig.StatusLookup,
                    returnFn: function (map, $grid) {
                        var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                        var sameCols = [];
                        sameCols.push({ name: "StsCd", val: map.StsCd, index: 5, text: 'Status' });
                        setGridVal($grid, selRowId, "StsDescp", "", null);
                        setGridVal($grid, selRowId, "StsCd", "", 'lookup');
                        if (checkSameValue($grid, selRowId, sameCols) !== false) {
                            setGridVal($grid, selRowId, "StsDescp", map.Ldescp, null);
                            setGridVal($grid, selRowId, "StsCd", map.StsCd, 'lookup');
                            return map.StsCd;
                        }
                        return "";
                    }
                }, LookUpConfig.GetStatusAuto(groupId, $SubGrid,
                function ($grid, rd, elem, rowid) {
                    var sameCols = [];
                    sameCols.push({ name: "StsCd", val: rd.STS_CD, index: 5, text: 'Status' });
                    setGridVal($grid, rowid, "StsCd", "", 'lookup');
                    setGridVal($grid, rowid, "StsDescp", "", null);
                    if (checkSameValue($grid, rowid, sameCols) !== false) {
                        $(elem).val(rd.STS_CD);
                        setGridVal($grid, rowid, "StsCd", rd.STS_CD, 'lookup');
                        setGridVal($grid, rowid, "StsDescp", rd.LDESCP, null);
                    }
                  
                }), { param: "" });
            return status_op;
        }
        
        colModel1 = [
           { name: 'UId', title: 'id', index: 'UId', sorttype: 'string', hidden: true, editable: false },
           { name: 'GroupId', title: 'GroupId', index: 'GroupId', sorttype: 'string', hidden: true, editable: false },
           { name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', hidden: true, editable: false },
           { name: 'PartyType', title: 'Party Type', index: 'PartyType', sorttype: 'string', width: 80, hidden: false, editable: true, editoptions: gridLookup(getop("MainGrid", $MainGrid, "StsCd")), edittype: 'custom' },
           { name: 'PartyDescp', title: 'Description', index: 'PartyDescp', sorttype: 'string', width: 180, hidden: false, editable: true }
        ];

        colModel2 = [
          { name: 'UId', title: 'id', index: 'UId', sorttype: 'string', hidden: true, editable: false },
          { name: 'UFid', title: 'UFID', index: 'UFid', sorttype: 'string', hidden: true, editable: false },
          { name: 'GroupId', title: 'GroupId', index: 'GroupId', sorttype: 'string', hidden: true, editable: false },
          { name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', hidden: true, editable: false },
          { name: 'StsCd', title: 'Status', index: 'StsCd', sorttype: 'string', width: 80, hidden: false, editable: true, editoptions: gridLookup(getStatusop("StsCd")), edittype: 'custom' },
          { name: 'StsDescp', title: 'Description', index: 'StsDescp', sorttype: 'string', width: 200, hidden: false, editable: true },
          { name: 'ViewOp', title: 'View', index: 'ViewOp', sorttype: 'string', width: 80, hidden: false, editable: true, edittype: 'select', editoptions: { value: "N:N;Y:Y", defaultValue: 'N' } },
          { name: 'UploadOp', title: 'Upload', index: 'UploadOp', sorttype: 'string', width: 80, hidden: false, editable: true, edittype: 'select', editoptions: { value: "N:N;Y:Y", defaultValue: 'N' } }
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
                alert("@Resources.Locale.L_RouteSetup_SelectCom");
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
            StatusBarArr.nowStatus("@Resources.Locale.L_RouteSetup_Browse");
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
            options.gridUrl = rootPath + "/TKBL/GetCustomerStatus"; //查询的url, rootPath为网站根目录
            options.registerBtn = thisItem;
            options.isMutiSel = true;
            options.selfSite = true;
            options.gridFunc = gridMethod;
            //options.param = "&sopt_Type=eq&Type=1";
            options.param = "";
            options.lookUpConfig = LookUpConfig.CmpLookup; //查询条件在Scripts/BaseLookup.js设定
            initLookUp(options);
        }

        function checkData($grid, nullCols, sameCols) {
            var rowIds = $grid.getDataIDs();
            for (var i = 0; i < rowIds.length; i++) {
                if (rowIds[i].indexOf("jqg") < 0)
                    continue;
                var rowDatas = $grid.jqGrid('getRowData', rowIds[i]);
                for (var j = 0; j < rowIds.length; j++) {
                    if (rowIds[i] === rowIds[j])
                        continue;
                    var rowDatas1 = $grid.jqGrid('getRowData', rowIds[j]);
                    for (var x = 0; x < sameCols.length; x++) {
                        var col = sameCols[x];
                        if ($.trim(rowDatas[col.name]) === $.trim(rowDatas1[col.name])) {
                            try {
                                $grid.jqGrid("editCell", rowIds[i], col.index, true);
                            }
                            catch (e) {
                            }
                            CommonFunc.Notify("", "@Resources.Locale.L_SystemController_Same" + col.text + ":" + $.trim(rowDatas1[col.name]), 2000, "warning");
                            return false;
                        }
                    }
                }
            }

            for (var i = 0; i < rowIds.length; i++) {
                var rowDatas = $grid.jqGrid('getRowData', rowIds[i]);
                for (var j = 0; j < nullCols.length; j++) {
                    var col = nullCols[j];
                    if (!col.canNull) {
                        if (isEmpty($.trim(rowDatas[col.name]))) {
                            try {
                                $grid.jqGrid("editCell", rowIds[i], col.index, true);
                            }
                            catch (e) {
                            }
                            CommonFunc.Notify("", col.text + "@Resources.Locale.L_RouteSetup_NoEmp", 2000, "warning");
                            return false;
                        }
                    }
                }
                for (var j = 0; j < nullCols.length; j++) {
                    var col = nullCols[j];
                    if (col.checkData) {
                        var msg = col.checkData(col, $.trim(rowDatas[col.name]));
                        if (msg) {
                            try {
                                $grid.jqGrid("editCell", rowIds[i], col.index, true);
                            }
                            catch (e) {
                            }
                            CommonFunc.Notify("", msg, 2000, "warning");
                            return false;
                        }
                    }
                }
            }
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

            var nullCols = [], sameCols=[];
            nullCols.push({ name: "PartyType", index: 4, text: 'Party Type' });
            nullCols.push({ name: "PartyDescp", index: 5, text: 'Party Description' });
            sameCols.push({ name: "PartyType", index: 4, text: 'Party Type' });
            if (checkData($MainGrid, nullCols, sameCols) === false) {
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
                return false;
            }
            //if (beforSave($MainGrid, nullCols) === false) {
            //    MenuBarFuncArr.SaveResult = false;
            //    dtd.resolve();
            //    return false;
            //}

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
                url: rootPath + "TKBL/SaveCustomerStatus",
                type: 'POST',
                data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), cmp: $("#txt_Cmp").val(), autoReturnData: true },
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
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
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

        MenuBarFuncArr.DelMenu(["MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid", "MBSearch", "MBErrMsg"]);  //Delete不需要的Menu
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