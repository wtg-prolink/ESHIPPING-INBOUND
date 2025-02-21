var _handler = { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, afterEdit: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, loadGridData: null, grids: [], gridEditableCtrl: null, endGrid: null, endGrids: null };
_handler.lang = { isNotNull: "Do not allow null values", tip1: "Please select a record", tip2: "There is the same record", saveTip1: "Saved Successful", saveTip2: "Saved Fail", delTip1: "Delete Successful", delTip2: "Delete Fail", maxTip: "The maximum value is：", minTip: "The minimum value is：" };

var groupId = getCookie("plv3.passport.groupid"),
    cmp = getCookie("plv3.passport.companyid"),
    stn = getCookie("plv3.passport.station"),
    userId = getCookie("plv3.passport.user");
var _dm = new dm();
var _changeDatArray = [];

//_handler.grids.push({ id: "", $grid: $("#") });
_handler.search = function (item) {
    registBtnLookup(item, {
        url: _handler.inquiryUrl, config: _handler.config, param: "", selectRowFn: function (map) {
            if (_handler.loadMainData)
                _handler.loadMainData(map);
        }
    });
}

_handler.checkData = function checkData($grid, nullCols, sameCols) {
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
                    try{
                        $grid.jqGrid("editCell", rowIds[i], col.index, true);
                    }
                    catch (e) {
                    }
                    CommonFunc.Notify("", _handler.lang.tip2 + col.text + ":" + $.trim(rowDatas1[col.name]), 2000, "warning");
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
                    try{
                        $grid.jqGrid("editCell", rowIds[i], col.index, true);
                    }
                    catch (e) {
                    }
                    CommonFunc.Notify("", col.text + _handler.lang.isNotNull, 2000, "warning");
                    return false;
                }
            }
        }
        for (var j = 0; j < nullCols.length; j++) {
            var col = nullCols[j];
            if (col.checkData) {
                var msg = col.checkData(col, $.trim(rowDatas[col.name]));
                if (msg) {
                    try{
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

_handler.beforDel = function () {
    if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
        CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
        return false;
    }
}

_handler.delData = function () {
    var changeData = getAllKeyValue();//获取所有主键值
    ajaxHttp(_handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false },
        function (result) {
            if (result.message) {
                CommonFunc.Notify("", result.message, 1000, "warning");
                return false;
            }
            else if (_handler.setFormData) {
                _handler.setFormData([{}]);
            }
            return true;
        });
}

_handler.saveData = function (dtd) {
    //var keyData = getAllKeyValue();
    var changeData = getChangeValue();//获取所有改变的值
    var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
    if (_handler.topData && !isEmpty(_handler.topData[_handler.key]))
        data[_handler.key] = encodeURIComponent(_handler.topData[_handler.key]);
    ajaxHttpSaveBar(dtd, _handler.saveUrl, data,
        function (result) {
            //_handler.topData = keyData["mt"];
            if (result.message) {
                alert(result.message);
                return false;
            }
            else if (_handler.setFormData)
                _handler.setFormData(result);
            return true;
        });
}

_handler.addData = function () {
    //初始化新增数据
    var data = {};
    data[_handler.key] = uuid();
    setFieldValue([data]);
}

_handler.cancelData = function () {
    if (_handler.loadMainData)
        _handler.loadMainData(_handler.topData);
}

_handler.beforEdit = function () {
    if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
        CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
        return false;
    }
}

_handler.setFormData = function (data) {
    _handler.topData = data[0] || {};
    setFieldValue(data);
    setdisabled(true);
    setToolBtnDisabled(true);
}

_handler.loadGridData = function (dsNm, grid, data, keys) {
    if (_dm.getDs(dsNm) == null || _dm.getDs(dsNm) == undefined) {
        _dm.addDs(dsNm, data.rows, keys, grid);// ["UId", , "SeqNo"]
    } else {
        _dm.getDs(dsNm).setData(data);//
    }
}

_handler.endSave = function (dtd) {
    if (_handler.gridEditableCtrl) _handler.gridEditableCtrl(false);
    MenuBarFuncArr.SaveResult = true;
    dtd.resolve();
    return true;
}

_handler.endGrid = function ($grid) {//结束grid的编辑状态
    try {
        var selRowId = $grid.jqGrid('getGridParam', 'selrow');
        $grid.jqGrid('saveRow', selRowId, false, 'clientArray');
        $grid.jqGrid('getGridParam', "endEdit")();
    }
    catch (e) {
    }
}

_handler.endGrids = function () {//结束grid的编辑状态
    var grids = _handler.grids;
    for (var i = 0; i < grids.length; i++) {
        _handler.endGrid(grids[i].$grid);
    }
}

_handler.gridEditableCtrl = function (editable) {//结束grid的编辑状态
    var grids = _handler.grids;
    for (var i = 0; i < grids.length; i++) {
        try {
            if (grids[i].editable) {
                gridEditableCtrl({ editable: editable, gridId: grids[i].id });
            }
        }
        catch (e) {
        }
    }
}

function _intiGrid(jqGrid, op) {
    //{ colModel: colModel2,caption: "",delKey: ["UId"]};
    op.baseColModel = $.extend(true, {}, op.colModel);
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
        rows: 999999
    }, op);
    if (op.savelayout)
        return GetLayout(jqGrid, op);
    return new genGrid(jqGrid, op);
}

_handler.intiGrid = function (id, $grid, op, keys, editable) {
    op = op || {};
    editable = (editable==false)?false:true;
    var gg = _intiGrid($grid, op);
    _dm.addDs(id, [], op.delKey || keys, $grid[0]);
    $grid.jqGrid('setGridParam', {
        ds: _dm.getDs(id)
    });
    _handler.grids.push({ id: id, $grid: $grid,editable:editable });
    return gg;
}

_handler.beforLoadView = function () {
}

function _initUI(delMenu, disabledMenu, enabledMenu) {
    var initLang = true;
    try {
        GetLangCaption("test", "test");
    }
    catch (e) { initLang = false; }
    if (initLang) {
        $("[langId]").each(function () {
            var jq = $(this);
            var caption = jq.text() || '';
            var langId = jq.attr("langId");
            if (langId == undefined || langId === "" || langId == null)
                return;
            caption = GetLangCaption(langId, caption);
            if (caption == undefined || caption === "" || caption == null)
                return;
            jq.text(caption);
        });
    }

    //InitSubFormField(true);
    try {
        schemas = JSON.parse(decodeHtml(schemas));
        var columns = '';
        for (var i in schemas) {
            columns += i + ',';
        }
        CommonFunc.initField(schemas);
    }
    catch (e) { }
    $("input, textarea").not("[type=submit]").jqBootstrapValidation();
    if (_handler.beforLoadView)
        _handler.beforLoadView();
    setdisabled(true);
    setToolBtnDisabled(true);
    MenuBarFuncArr.MBDel = function () {
        if (_handler.beforDel && _handler.beforDel() === false) {
            return false;
        }
        if (_handler.delData && _handler.delData() !== false) {
            editable = true;
            CommonFunc.Notify("", _handler.lang.delTip1, 500, "success");
        }
    }

    MenuBarFuncArr.MBAdd = function () {
        if (_handler.beforAdd && _handler.beforAdd() === false) {
            return false;
        }
        _handler.gridEditableCtrl(true);
        if (_handler.addData) _handler.addData();
        editable = true;
        //document.getElementById('UStatusYes').checked = true;
        //document.getElementById('UStatusNo').checked = false;
        //$("#UStatus").val($("#UStatusYes").val());
        //$("#ModiPwDate").removeAttr('required');
    }


    /*按下上方修改後，要做的事*/
    MenuBarFuncArr.MBEdit = function () {
        if (_handler.beforEdit && _handler.beforEdit() === false) {
            return false;
        }

        if (_handler.gridEditableCtrl) _handler.gridEditableCtrl(true);

        if (_handler.editData) _handler.editData();
        editable = true;
        return true;
    }


    /*按下上方Menu撤消後，要做的事*/
    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit"]);
        MenuBarFuncArr.Enabled(["MBDel"]);

        if (_handler.endGrids) _handler.endGrids();
        if (_handler.gridEditableCtrl) _handler.gridEditableCtrl(false);
        if (_handler.cancelData) _handler.cancelData();
        editable = false;
    }

    bindNumber();

    MenuBarFuncArr.MBSearch = function (thisItem) {
        if (_handler.search && _handler.search(thisItem) === false)
            return false;
    }

    MenuBarFuncArr.MBSave = function (dtd) {
        _handler.endGrids();
        if (_handler.beforSave && _handler.beforSave() === false) {
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve();
            return false;
        }
        _handler.saveData(dtd);
        return dtd.promise();
    }

    MenuBarFuncArr.MBBeforeSave = function (dtd) {
        if (checkNoAllowNullFields() == false) {
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve();
            return false;
        }
        MenuBarFuncArr.SaveResult = true;
        dtd.resolve();
        return dtd.promise();
    }

    initMenuBar(MenuBarFuncArr);
    $('#MBEdit').unbind("click");
    $("#MBEdit").click(function () {//自定义编辑
        var className = $(this).attr('class');
        if (className == "nav-disabled") {
            return false;
        }
        _editData = _dataSource[0];
        var result = MenuBarFuncArr.MBEdit();
        if (result === false) return;
        MenuBarFuncArr.EditStatus(result);
        if (_handler.afterEdit) _handler.afterEdit();
        $("#wrapper").focusFirst();

    });

    $("#MBAdd").unbind("click").click(function () {
        var className = $(this).attr('class');
        if (className == "nav-disabled") {
            return false;
        }
        //setFieldValue(undefined, "");
        if (MenuBarFuncArr.MBAdd() === false)
            return;
        MenuBarFuncArr.AddStatus();
        $("#wrapper").focusFirst();
    });

    $("#MBCopy").unbind("click").click(function () {
        var className = $(this).attr('class');
        if (className == "nav-disabled") {
            return false;
        }
        if (MenuBarFuncArr.MBCopy() === false)
            return;
        MenuBarFuncArr.AddStatus();
    });

    if (delMenu)
        MenuBarFuncArr.DelMenu(delMenu);  //Delete不需要的Menu
    if (disabledMenu)
        MenuBarFuncArr.Disabled(disabledMenu); //Disable Menu
    if (enabledMenu)
        MenuBarFuncArr.Enabled(enabledMenu);  //Enabled Menu
    
}

//带有dtd的post数据
function ajaxHttpSaveBar(dtd, url, data, successFn, completeFn, errorFn) {
    ajaxHttp(url, data, function (result) {
        if (!successFn(result)) {
            CommonFunc.Notify("", _handler.lang.saveTip2, 500, "warning");
            MenuBarFuncArr.SaveResult = false;
            if (dtd) dtd.resolve();
            return false;
        }
        if (_handler.gridEditableCtrl) _handler.gridEditableCtrl(false);
        setdisabled(true);
        setToolBtnDisabled(true);
        CommonFunc.Notify("", _handler.lang.saveTip1, 500, "success");
        MenuBarFuncArr.SaveResult = true;
        if (dtd) dtd.resolve();
        //MenuBarFuncArr.MBCancel();

    }, function (xmlHttpRequest, successMsg) {
        if (completeFn) completeFn(xmlHttpRequest, successMsg);
    }, function () {
        MenuBarFuncArr.SaveResult = false;
        if (dtd) dtd.resolve();
    });
}

//post数据
function ajaxHttp(url, data, successFn, completeFn, errorFn) {
    var loading = data.loading;
    if (loading == true) {
        CommonFunc.ToogleLoading(true);
    }
    $.ajax({
        async: true,
        url: url,
        type: 'POST',
        data: data,
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
            if (loading == true) {
                CommonFunc.ToogleLoading(false);
            }
            if (completeFn) completeFn(xmlHttpRequest, successMsg);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.Notify("", errMsg, 500, "danger");
            if (errorFn) errorFn();
        },
        success: function (result) {
            if (successFn) successFn(result);
        }
    });
}

//判断文字是否为空
function isEmpty(val) {
    if (val === undefined || val === "" || val == null)
        return true;
    return false;
}

//注册grid型的放大镜
function getLookupOp(gridId, op, op1, op2) {
    //op = op || { url: rootPath + "Common/GetCountryData", config: config, returnFn: function () { }, autoFn: function () { } };
    var $grid = $("#" + gridId);
    var opt = {};
    opt.gridUrl = op.url;
    opt.selfSite = false;
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
    //opt.baseConditionFunc = op1.baseConditionFunc;
    if (typeof (op.autoCompGetValueFunc) != "undefined")
        opt.autoCompGetValueFunc = op1.autoCompGetValueFunc;
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

//注册按钮型的放大镜  op=》{url:"",config:"",param:""} op基本格式 item是jq对象  url是api  config是colom数组 param防止带入底层的siti那个乱七八糟的东西
function registBtnLookup(item, op, op1, op2) {
    //{url:"",config:""}
    var options = {};
    options.gridUrl = op.url;
    options.registerBtn = item;
    options.param="";
    if (op.isMutiSel) options.isMutiSel = true;
    options.param = op.param;
    options.gridFunc = function (map) {
        //获取Map中的值，然后将值传递给后台重新去抓取整个画面的url
        if (op.selectRowFn)
            op.selectRowFn(map);
    }
    options.responseMethod = function () { }
    options.lookUpConfig = op.config;
    options.openclick = op.openclick;
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
            },function () {
                if (op2.dymcFunc) {
                   return op2.dymcFunc();
                }
                return "";
            }, function () {
                if (op2.autoClearFunc) {
                    op2.autoClearFunc($(op.item));
                }
                return false;
            });
    }
}

function getLookupOpNew(gridId, op, op1, op2) {
    //op = op || { url: rootPath + "Common/GetCountryData", config: config, returnFn: function () { }, autoFn: function () { } };
    var $grid = $("#" + gridId);
    var opt = {};
    opt.gridUrl = op.url;
    opt.selfSite = false;
    var config = op.config;
    opt.gridReturnFunc = function (map) {
        //var selRowId = $grid.jqGrid('getGridParam', 'selrow');
        //$grid.jqGrid('setCell', selRowId, 'CntryNm', map.CntyNm);
        if (op.selectRowFn)
            return op.selectRowFn(map, $grid);
    };

    opt.gridId = gridId;
    opt.IsAutoC = true;
    if (typeof (op.IsAutoC) != "undefined")
        opt.IsAutoC = op.IsAutoC;
    //自动带入
    opt.autoCompKeyinNum = 1;
    opt.autoCompUrl = "";
    if (typeof (op.baseConditionFunc) != "undefined")
        opt.baseConditionFunc = op.baseConditionFunc;

    if (typeof (op.baseCondition) != "undefined")
        opt.baseCondition = op.baseCondition;
    //opt.baseConditionFunc = op1.baseConditionFunc;
    if (typeof (op.autoCompGetValueFunc) != "undefined")
        opt.autoCompGetValueFunc = op.autoCompGetValueFunc;
    opt.defaultCase = false;
    opt.param = "";
    if (typeof (op.param) != "undefined")
        opt.param = op.param;
    if (typeof (op.autoClearFunc) != "undefined")
        opt.autoClearFunc = op.autoClearFunc;
    if (typeof (op.autoCompDt) != "undefined")
        opt.autoCompDt = op.autoCompDt;
    if (typeof (op.responseMethod) != "undefined")
        opt.responseMethod = op.responseMethod;
    if (typeof (op.useSelectMode) != "undefined")
        opt.useSelectMode = op.useSelectMode;
    if (typeof (op.OtherParamFunc) != "undefined")
        opt.OtherParamFunc = op.OtherParamFunc;
    if (op.isMutiSel) {
        opt.isMutiSel = true;
        config = $.extend({ multiselect: true }, op.config);
    }
    if (typeof (op.autoCompParams) != "undefined")
        opt.autoCompParams = op.autoCompParams;
    opt.lookUpConfig = config;
    opt = $.extend(opt, op1);
    opt = $.extend(opt, op2);
    return opt;
}

function registBtnLookupNew(item, op) {
    var options = {};
    options.gridUrl = op.url;
    options.registerBtn = item;
    options.param = "";
    var config = op.config;
    if (op.isMutiSel) {
        options.isMutiSel = true;
        config = $.extend({ multiselect: true }, op.config);
    }
    options.gridFunc = function (map) {
        //获取Map中的值，然后将值传递给后台重新去抓取整个画面的url
        if (op.selectRowFn)
            op.selectRowFn(map);
        $(op.item).focus();
    }
    options.responseMethod = op.responseMethod
    options.lookUpConfig = config;
    options.baseConditionFunc = op.baseConditionFunc;
    if (typeof (op.OtherParamFunc) != "undefined")
        options.OtherParamFunc = op.OtherParamFunc;
    initLookUp(options);
    if (typeof (op.IsAutoC) == "undefined")
        op.IsAutoC = true;
    if (!op.autoCompDt && op.item.length >= 1)
        op.autoCompDt = ConditionParam("", op.item.substr(0, 1) == "#" ? op.item.substr(1, op.item.length - 1) : op.item, "", "bw");
    if (op.item && op.IsAutoC) {
        CommonFunc.AutoComplete(op.item, 1, op.url, op.autoCompDt, "",
            function (event, ui) {
                if (op.selectRowFn) {
                    op.selectRowFn(ui.item);
                }
                return false;
            }, function () {
                if (op.baseConditionFunc) {
                    return op.baseConditionFunc();
                }
                return "";
            }, function () {
                if (op.autoClearFunc) {
                    op.autoClearFunc();
                }
                return false;
            }, null, op.OtherParamFunc);
    }
}


//生成GUID
function uuid(N) {
    var s = [];
    var hexDigits = "0123456789abcdef";
    for (var i = 0; i < 36; i++) {
        s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1);
    }
    s[14] = "4";  // bits 12-15 of the time_hi_and_version field to 0010
    s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1);  // bits 6-7 of the clock_seq_hi_and_reserved to 01
    s[8] = s[13] = s[18] = s[23] = "-";

    var uuid = s.join("");
    if (!N)
        return uuid.replace(/-/g, "");
    return uuid;
}

function setGridCurVal($grid, selRowId, name, val) {
    var ds = $grid.jqGrid("getGridParam", "ds");
    if (ds && ds.setCurVal) {
        //alert(val);
        ds.setCurIndexByKey($grid.jqGrid("getRowData", selRowId));
        ds.setCurVal(name, val);
    }
}

//绑定number类型的input  number="0" 代表保留0位小数  maxnum="9" 最大值为9  minnum="0" 最小值为0
function bindNumber() {
    // alert(/^\d+\.\d+$/.test(this.value));
    $("[number]").unbind(".number").bind("keypress.number", function (e) {

        if (e.which == 45) {
            var min = parseFloat($(this).attr("minnum") + "");
            if (isNaN(min) || min < 0) {
                if ($(this).val().indexOf("-") == -1 && $(this).val().length <= 0) {
                    return true;
                } else {
                    return false;
                }
            }
            else
                return false;
        }
        if (e.which == 46) {
            var pos = parseInt($(this).attr("number") + "" || "0");
            if (pos > 0) {
                if ($(this).val().indexOf(".") == -1) {
                    return true;
                } else {
                    return false;
                }
            }
            else
                return false;
        } else {
            if ((e.which >= 48 && e.which <= 57
                    && e.ctrlKey == false && e.shiftKey == false)
                    || e.which == 0 || e.which == 8) {
                return true;
            } else {
                if (e.ctrlKey == true
                        && (e.which == 99 || e.which == 118)) {
                    return true;
                } else {
                    return false;
                }
            }
        }
    }).bind("paste.number", function () {
        $(this).val($(this).val().replace(/[^0-9.-]/g, ''));
    }).bind("dragenter.number", function () {
        return false;
    }).bind("blur.number", function () {
        var max = parseFloat($(this).attr("maxnum") + "");
        var min = parseFloat($(this).attr("minnum") + "");
        var val = parseFloat($(this).val() + "" || "0");
        if (!isNaN(max) && val > max) {
            CommonFunc.Notify("", _handler.lang.maxTip + max, 500, "danger");
            val = max;
        }
        if (!isNaN(min) && val < min) {
            CommonFunc.Notify("", _handler.lang.minTip + min, 500, "danger");
            val = min;
        }

        var pos = parseInt($(this).attr("number") + "" || "0");
        var val1 = fomatFloat(val, pos);
        if (val1 !== val && pos)
            CommonFunc.Notify("", _getLang("L_initView_Scripts_385", "自动保留") + pos + _getLang("L_initView_Scripts_386", "位小数"), 500, "danger");
        $(this).val(val1);
    }).css("ime-mode", "disabled");
};

//四舍五入数据  
function fomatFloat(f, pos) {
    return Math.round(f * Math.pow(10, pos)) / Math.pow(10, pos);
}

var now = new Date(),
      _nowDayOfWeek = now.getDay(),
      _nowDay = now.getDate(),
      _nowMonth = now.getMonth(),
      _nowYear = now.getFullYear();
function getDate(day, split) {
    split = split || "/";
    if (day === undefined || day == null)
        day = 0;
    var weekEndDate = new Date(_nowYear, _nowMonth, _nowDay + day);
    var y = weekEndDate.getFullYear(), m = (weekEndDate.getMonth() + 1), d = weekEndDate.getDate();
    if (m < 10)
        m = "0" + m;
    if (d < 10)
        d = "0" + d;
    return weekEndDate.getFullYear() + split + m + split + d;
}

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}


