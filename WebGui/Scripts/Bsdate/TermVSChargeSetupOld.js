var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var mainKeyValue = {};
var groupId = getCookie("plv3.passport.groupid");
var gridSetting = {};

jQuery(document).ready(function ($) {
    var editable = false;
    var docHeight = $(document).height();
    gridHeight = docHeight - 150;
    
    var funcOpt = {};
    funcOpt.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
    funcOpt.param = "";
    funcOpt.baseCondition = " CD_TYPE='TD' AND GROUP_ID='" + groupId + "' AND (CMP='" + cmp + "' OR CMP='*')";
    funcOpt.gridReturnFunc = function (map) {
        var value = map.Cd
        return value;
    }
    funcOpt.selfSite = true;
    funcOpt.lookUpConfig = LookUpConfig.BSCodeLookup;
    funcOpt.autoCompKeyinNum = 1;
    funcOpt.gridId = "TermvschgGrid";
    funcOpt.autoCompUrl = "";
    funcOpt.autoCompDt = "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE~TD&CD=";
    funcOpt.autoCompParams = "CD=showValue,CD,CD_DESCP";
    funcOpt.autoCompFunc = function (elem, event, ui, rowid) {
        $(elem).val(ui.item.returnValue.CD);
    }

    var dlvterOpt = {};
    dlvterOpt.gridUrl = rootPath + LookUpConfig.CityAndTruckPortUrl;
    dlvterOpt.param = "";
    dlvterOpt.baseCondition = " 1=1";
    dlvterOpt.gridReturnFunc = function (map, rowid) {
        var podcd = map.Pod;
        //setGridVal($grid, rowid, 'PodName', map.PortNm, 'lookup');
        var value = podcd
        return value;
    }
    dlvterOpt.selfSite = true;
    dlvterOpt.lookUpConfig = LookUpConfig.CityPortLookup2;
    dlvterOpt.autoCompKeyinNum = 1;
    dlvterOpt.gridId = "TermvschgGrid";
    dlvterOpt.autoCompUrl = "";
    dlvterOpt.autoCompDt = "dt=cityport&GROUP_ID=" + groupId + "&POD=";
    dlvterOpt.autoCompParams = "CNTRY_CD&PORT_CD=showValue,CNTRY_CD,PORT_CD,PORT_NM";
    dlvterOpt.autoCompFunc = function (elem, event, ui, rowid) {
        $(elem).val(ui.item.returnValue.POD);
        setGridVal($grid, rowid, 'PodName', ui.item.returnValue.PORT_NM, 'lookup');
    }

   
    var _NeedChgOption = { "FC": "Freight Charge", "BC": "Broker Charge", "TC": "Truck Charge", "LC": "Local Charge" };
    //创建控件
    function _create_element_NeedChg(value, options) {
        var el = document.createElement("div");
        for (var name in _NeedChgOption) {
            $(el).append('<input name="' + name + '" value="' + name + '" type="checkbox"/>' + _NeedChgOption[name] + '<br />');
        }

        $(el).click(function (event) {
            event.stopPropagation();
        });

        //var vals = (value || '').split(";");
        //for (var i = 0; i < vals.length; i++) {
        //    for (var name in _NeedChgOption) {
        //        if (_NeedChgOption[name] === vals[i]) {
        //            $(el).find("input[name='" + name + "']").attr("checked", true);
        //            break;
        //        }
        //    }
        //}
        _setNeedChg(value, $(el).find("input"));
        return el;
    }

    //获取和设置值
    function _value_NeedChg(elem, operation, value) {
        if (operation === 'get') {
            return _unformatNeedChg(value,operation,elem);
        } else if (operation === 'set') {
            _setNeedChg(value, inputs);
        }
    };

    //设置grid编辑框的值
    function _setNeedChg(value, inputs) {
        var val = ',';
        var vals = (value || '').split(",");
        if (vals.length <= 0)
            return;
        for (var i = 0; i < vals.length; i++) {
            var v = _myTrim(vals[i]);
            val += v + ',';
        }
        $.each(inputs, function () {
            var jq = $(this);
            $(this).attr("checked", val.indexOf("," + jq.attr('name') + ",") > -1);
        });
    }

    //格式化显示grid编辑框的值
    function _formatNeedChg(cellValue, options, rowObject) {
        //return cellValue;
        var val = '';
        var vals = (cellValue || '').split(",");
        for (var i = 0; i < vals.length; i++) {
            var v = _myTrim(vals[i]);
            if (v === "")
                continue;
            val += (_NeedChgOption[v] || v) + ';';
        }
        val = val.substring(0, val.lastIndexOf(';'));
        return val;
    }

    //当数据要保存到db时 需要反格式化grid编辑框的值
    function _unformatNeedChg(cellvalue, options, cell) {
        var inputs = $(cell).find("input");
        var val = "";
        if (inputs.length > 0) {//当已创建控件时
            $.each(inputs, function () {
                var jq = $(this);
                if (jq.is(':checked'))
                    val += jq.val() + ",";
                //$(this).attr("checked", val.indexOf(jq.name) > -1);
            });
            val = val.substring(0, val.lastIndexOf(','));
            return val;
        }

        //未创建控件
        var vals = (cellvalue || '').split(";");
        for (var i = 0; i < vals.length; i++) {
            for (var name in _NeedChgOption) {
                if (_NeedChgOption[name] === vals[i]) {
                    val += name + ",";
                    break;
                }
            }
        }
        val = val.substring(0, val.lastIndexOf(','));
        return val;
    }

    function _myTrim(x) {
        return (x || '').replace(/^\s+|\s+$/gm, '');
    }

    function getCmpop(name) {
        var Cmp_op = getLookupOp("TermvschgGrid",
            {
                url: rootPath + LookUpConfig.GetCmpUrl,
                config: LookUpConfig.CmpLookup,
                returnFn: function (map, $grid, selRowId) {
                    selRowId = selRowId || $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "Cmp", map.Cd, "lookup");
                    return map.Cmp;
                }
            }, LookUpConfig.GetCmpAuto(groupId, $("#TermvschgGrid"), function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, "Cmp", rd.CMP, "lookup");
                $(elem).val(rd.CMP);
            },
            function () {
                return "GROUP_ID=" + groupId + " AND TYPE='1'";
            }), {
                param: "",
                baseConditionFunc: function () {
                    return "GROUP_ID='" + groupId + "' AND TYPE='1'";
                }
            });
        return Cmp_op;
    }


    var colModel = [
        { name: 'UId', title: '', index: 'UId', sorttype: 'string', editable: false, hidden: true },
	    { name: 'GroupId', title: '', index: 'GroupId', sorttype: 'string', editable: false, hidden: true },
	    { name: 'Cmp', title: 'Location', index: 'Cmp', sorttype: 'string', width: 80, hidden: false, editable: true, editoptions: gridLookup(getCmpop("Cmp")), edittype: 'custom' },
        { name: 'IoFlag', title: 'I/O Bound', index: 'IoFlag', width: 100, sorttype: 'string', editable: true, formatter: 'select', edittype: 'select', editoptions: { value: 'I:InBound;O:OutBound' }, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } },
        { name: 'FrtTerm', title: 'Freight Term', index: 'FrtTerm', width: 100, sorttype: 'string', editable: true, formatter: 'select', edittype: 'select', editoptions: { value: 'P:Prepaid;C:Collect' }, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } },
        { name: 'IncotermCd', title: 'DLV TERM', index: 'IncotermCd', edittype: 'custom', sorttype: 'string', width: 120, hidden: false, editable: true, editoptions: gridLookup(funcOpt) },
	    { name: 'IncotermDescp', title: 'DLV Description', index: 'IncotermDescp', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'PodCd', title: 'POD', index: 'PodCd', edittype: 'custom', sorttype: 'string', width: 120, hidden: false, editable: true, editoptions: gridLookup(dlvterOpt) },
		{ name: 'PodName', title: 'POD Description', index: 'PodName', sorttype: 'string', editable: false },
        {
            name: 'NeedChg', title: 'Need Charge', index: 'NeedChg', width: 400, sorttype: 'string', editable: true, edittype: 'custom', editoptions: { custom_element: _create_element_NeedChg, custom_value: _value_NeedChg }, formatter: _formatNeedChg, unformat: _unformatNeedChg
        }
    ];

    $grid = $("#TermvschgGrid");
    _dm.addDs("TermvschgGrid", [], ["Cur"], $grid[0]);
    new genGrid(
    	$grid,
    	{
    	    //datatype:  "json",
    	    //loadonce: true,
    	    colModel: colModel,
    	    datatype: "json",
    	    url: rootPath + "Bsdate/TermVSChargeQuery",
    	    cellEdit: false,//禁用grid编辑功能
    	    caption: "Term vs Charges",
    	    height: gridHeight,
    	    rownumWidth: 50,
    	    refresh: true,
    	    rows: 9999,
    	    exportexcel: false,
    	    pginput: false,
    	    pgbuttons: false,
    	    ds: _dm.getDs("TermvschgGrid"),
    	    sortorder: "asc",
    	    sortname: "CreateDate",
    	    delKey: "UId",
    	    beforeSelectRowFunc: function (rowid) {
    	    },
    	    onAddRowFunc: function (rowid) {
                
    	    },
            afterSaveCellWithIdFunc: function (rowid, name, val, iRow, iCol, toolId) {

            },
    	    beforeAddRowFunc: function (rowid) {
    	        //add row 時要可以編輯main key
    	        // $("#TermvschgGrid").setColProp('UId', { editable: true });
    	    }
    	}
    );

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit"]);
        location.reload();
        gridEditableCtrl({ editable: false, gridId: "TermvschgGrid" });
        editable = false;
    }

    MenuBarFuncArr.MBEdit = function () {
        gridEditableCtrl({ editable: true, gridId: "TermvschgGrid" });
        editable = true;
    }

    MenuBarFuncArr.MBSave = function (dtd) {

        editable = false;
        var containerArray = $("#TermvschgGrid").jqGrid('getGridParam', "arrangeGrid")();

        for(var i=0; i<containerArray.length; i++) {
            var __state = containerArray[i]["__state"];
            var NeedChg = containerArray[i]["NeedChg"];

            if(__state == 1) {
                if(typeof NeedChg == "object") {
                    containerArray[i]["NeedChg"] = NeedChg.join();
                }
            }
        }

        var changeData = {};
        changeData["mt"] = containerArray;
        $.ajax({
            async: true,
            url: rootPath + "Bsdate/TermVSChargeUpdate",
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
                console.log(result.message);
                if (result.message !== "success") {
                    CommonFunc.Notify("", "@Resources.Locale.L_BSCSSetup_SFail", 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return;
                }

                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_TKSetup_Success", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                gridEditableCtrl({ editable: false, gridId: "TermvschgGrid" });
                editable = false;

                dtd.resolve();
                //location.reload();
            }
        });
        return dtd.promise();
    }


    initMenuBar(MenuBarFuncArr);

    MenuBarFuncArr.DelMenu(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);
    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBEdit"]);

});

