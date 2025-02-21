var _selectIdName = "";
var myEditDateInit = {
    size: 12,
    maxlengh: 20,
    dataInit: function (element) {
        $(element).removeAttr('class').addClass('form-control input-sm');
        $(element).wrap('<div class="input-group">')
        .datepicker({
            showOn: "button",
            changeYear: true,
            dateFormat: "yy-mm-dd",
            beforeShow: function () {
                setTimeout(function () {
                    $('.ui-datepicker').css('z-index', 99999999999999);
                }, 0);
            },
            onClose: function (text, inst) {
                $(this).focus();
            }
        })
        .next("button").button({
            icons: { primary: "ui-icon-calendar" },
            label: "Select a date",
            text: false
        })
        .addClass("btn btn-sm btn-default").prop("disabled", false).html("<span class='glyphicon glyphicon-calendar'></sapn>")
        .wrap('<span class="input-group-btn">')
        .find('.ui-button-text')
        .css({
            'visibility': 'hidden',
            'display': 'inline'
        });
        $(element).on('change', function () {
            $(this).setfocus();
        });
    }
};
var myEditDateTimeInit = {
    size: 12,
    maxlengh: 20,
    dataInit: function (element) {
        $(element).removeAttr('class').addClass('form-control input-sm');
        $(element).wrap('<div class="input-group">')
        .datetimepicker({
            showOn: "button",
            changeYear: true,
            dateFormat: "yy-mm-dd",
            timeFormat: 'HH:mm',
            beforeShow: function () {
                setTimeout(function () {
                    $('.ui-datepicker').css('z-index', 99999999999999);
                }, 0);
            },
            onClose: function (text, inst) {
                $(this).focus();
            }
        })
        .next("button").button({
            icons: { primary: "ui-icon-calendar" },
            label: "Select a date",
            text: false
        })
        .addClass("btn btn-sm btn-default").prop("disabled", false).html("<span class='glyphicon glyphicon-calendar'></sapn>")
        .wrap('<span class="input-group-btn">')
        .find('.ui-button-text')
        .css({
            'visibility': 'hidden',
            'display': 'inline'
        });
        $(element).on('change', function () {
            $(this).setfocus();
        });
    }
};

var numFormat = { decimalSeparator: ",", thousandsSeparator: ",", decimalPlaces: 2, prefix: "$ " };
var oriSelRow = 0;
var oriSelCol = 0;
var setGridChange = function (opt) {
    var ds = $(opt.gridId).jqGrid("getGridParam", "ds");
    if (typeof ds != "undefined" && ds != null) {
        ds.setCurIndexByKey($(opt.gridId).jqGrid("getRowData", opt.rowId));
        ds.setCurVal(opt.cellKey, opt.cellValue);
    }
    $(opt.gridId).jqGrid('setCell', opt.rowId, opt.cellKey, opt.cellValue, 'edit-cell dirty-cell');

    if (typeof opt.selColIndex != 'undefined' && opt.editable && (oriSelRow != opt.rowId || oriSelCol != opt.selColIndex)) {

        oriSelRow = opt.rowId;
        oriSelCol = opt.selColIndex;

        try {
            //$(opt.gridId).jqGrid('editCell', opt.rowId, opt.selColIndex, true);
        } catch (e) {

        }


    }

    $($(opt.gridId)[0].rows.namedItem(opt.rowId)).addClass("edited");
}
var idsOfSelectedRows = [];
var updateIdsOfSelectedRows = function (id, isSelected, gridId) {
    var contains = idsOfSelectedRows[gridId].contains(id);
    if (!isSelected && contains) {
        for (var i = 0; i < idsOfSelectedRows[gridId].length; i++) {
            if (idsOfSelectedRows[gridId][i] == id) {
                idsOfSelectedRows[gridId].splice(i, 1);
                break;
            }
        }
    }
    else if (isSelected && !contains) {
        idsOfSelectedRows[gridId].push(id);
    }
    console.log(idsOfSelectedRows[gridId]);
};
//params: string grid id,string rowid,string cell name,string value,string 'lookup' or null
var setGridVal = function ($thisGrid, rowid, cellName, value, type) {
    $thisGrid.jqGrid('getGridParam', "setGridCellValueCustom")($thisGrid.attr("id"), rowid, cellName, type, (value == "") ? null : value);
}
//params: string grid id,string rowid,string cell name,string 'lookup' or null
var getGridVal = function ($thisGrid, rowid, cellName, type) {
    return $thisGrid.jqGrid('getGridParam', "getGridCellValueCustom")($thisGrid, rowid, cellName, type);
}

var gridEditableCtrl = function (opt) {
    $('#' + opt.gridId).jqGrid("editCell", 0, 0, true);
    var editOnly = $('#' + opt.gridId).jqGrid("getGridParam", "editOnly");
    if (opt.editable) {
        $('#' + opt.gridId).jqGrid('setGridParam', { cellEdit: true, cmTemplate: { sortable: false }, cellsubmit: 'clientArray' });
        if (!editOnly) {
            $('#' + opt.gridId).jqGrid('getGridParam', "appendAddRowButton")();
            $('#' + opt.gridId).jqGrid('getGridParam', "appendDelRowButton")();
            if (opt.copy) {
                $('#' + opt.gridId).jqGrid('getGridParam', "appendCopyRowButton")();
            }
        }
    } else {
        $('#' + opt.gridId).jqGrid('setGridParam', { cellEdit: false, cmTemplate: { sortable: true } });
        if (!editOnly) {
            $('#' + opt.gridId).jqGrid('getGridParam', "removeAddRowButton")(opt.gridId);
            $('#' + opt.gridId).jqGrid('getGridParam', "removeDelRowButton")(opt.gridId);
            if (opt.copy) {
                $('#' + opt.gridId).jqGrid('getGridParam', "removeCopyRowButton")(opt.gridId);
            }
        }
    }
    $('#' + opt.gridId).jqGrid('resetSelection');

}
var gridLookup = function (opt) {
    return {
        size: 10,
        maxlengh: 20,
        readonly: (typeof opt.readonly == "undefined") ? "" : opt.readonly,
        custom_element: myelem,
        custom_value: myvalue,
        dataInit: function (elem, options) {
            var thisId = options.rowId + "_" + options.id.split("_")[1];
            var gridOption = {};

            gridOption.gridUrl = opt.gridUrl;
            gridOption.columnID = opt.columnID;
            gridOption.registerBtn = $("#" + thisId + "Lookup");
            gridOption.isMutiSel = false;
            gridOption.gridFunc = function (map) {
                //获取Map中的值，然后将值传递给后台重新去抓取整个画面的url
                var value = opt.gridReturnFunc(map, options);
                $("#" + thisId + "Input").val(value);
                $("#" + thisId + "Input").setfocus();
                $(elem).val(value);
                $(elem).setfocus();
            }
            gridOption.responseMethod = opt.responseMethod;
            //if (opt.columnID2 != "undefined") {
            //    gridOption.responseMethod = function (data) {
            //        var str = "";
            //        $.each(data, function (index, val) {
            //            str = str + data[index].CntryCd + data[index].PortCd + "/";
            //        });
            //        str=str.substr(0, str.length - 1);
            //        $("#" + thisId + "Input").val(str);
            //        $("#" + thisId + "Input").setfocus();
            //        $(elem).val(str);
            //        $(elem).setfocus();
            //    }
            //}
            gridOption.lookUpConfig = opt.lookUpConfig;
            gridOption.param = opt.param;
            gridOption.selfSite = opt.selfSite;
            gridOption.baseConditionFunc = opt.baseConditionFunc;
            gridOption.onClickRegiBtnFunc = opt.onClickRegiBtnFunc;

            if (opt.baseCondition != null) {
                if (opt.baseCondition) {
                    gridOption.baseCondition = opt.baseCondition;
                }
            }

            initLookUp(gridOption);

            if (typeof opt.useSelectMode == "undefined" || opt.useSelectMode == false) {
                CommonFunc.AutoComplete("#" + thisId + "Input", opt.autoCompKeyinNum, opt.autoCompUrl, opt.autoCompDt, opt.autoCompParams, function (event, ui) {

                    if (ui == null) {
                        var rowid = options.rowId;
                        $("#" + thisId + "Input").val("");
                        //return false;
                        var cellData = thisId.split("_");
                        setGridChange({ "gridId": "#" + opt.gridId, "rowId": rowid, "cellKey": cellData[1], "cellValue": null });
                        if (opt.autoClearFunc != null) {
                            if (opt.autoClearFunc) {
                                opt.autoClearFunc(this, event, options.rowId);
                            }
                        }
                    } else {
                        opt.autoCompFunc(this, event, ui, options.rowId);
                    }
                    return false;
                }, opt.autoCompGetValueFunc, opt.autoClearFunc);
            } else {
                CommonFunc.oAutoComplete("#" + thisId + "Input", opt.autoCompKeyinNum, opt.autoCompUrl, opt.autoCompDt, opt.autoCompParams, function (event, ui) {

                    if (ui == null) {
                        var rowid = options.rowId;
                        $("#" + thisId + "Input").val("");
                        //return false;
                        var cellData = thisId.split("_");
                        setGridChange({ "gridId": "#" + opt.gridId, "rowId": rowid, "cellKey": cellData[1], "cellValue": null });
                        if (opt.autoClearFunc != null) {
                            if (opt.autoClearFunc) {
                                opt.autoClearFunc(this, event, options.rowId);
                            }
                        }
                    } else {
                        opt.autoCompFunc(this, event, ui, options.rowId);
                    }
                    return false;
                }, opt.autoCompGetValueFunc, opt.autoClearFunc);
            }


            $("#" + thisId + "Input").setfocus();
        }
    }

};

function getGridChangeDataDS(opt) {

    var selRowId = $("#" + opt.gridId).jqGrid('getGridParam', 'selrow');

    $("#" + opt.gridId).jqGrid('saveRow', selRowId, false, 'clientArray');
    $("#" + opt.gridId).jqGrid('getGridParam', "endEdit")();

    //获取change后的值;将change后的值存储到_changeDatArray
    if (_dm.getDs(opt.gridId) == null || _dm.getDs(opt.gridId) == undefined) {

    } else {
        var detailvalues = _dm.getDs(opt.gridId).getChangeValue();
        $.each(detailvalues, function (i, detail) {
            if (typeof _changeDatArray[opt.gridId] == "undefined") {
                _changeDatArray.push(detail);
            } else {
                _changeDatArray[opt.gridId].push(detail);
            }
        });
    }

    var dsData = _dm.getDs(opt.gridId).getData();

    //获取子表中的所有数据遍历查询数据中是否有add的资料
    var allData = $("#" + opt.gridId).jqGrid("getGridParam", "data");


    var seq = 0;
    $.each(allData, function (i, val) {
        if (val._id_ != undefined || val.id != undefined) {

            if (val[opt.seqKey] != null && val[opt.seqKey] != "") {
                seq = val[opt.seqKey];
            } else {
                seq += 1;
            }
            if ((val._id_ != null && val._id_.indexOf("jqg") >= 0) || (val.id != null && val.id.indexOf("jqg") >= 0)) {
                //如果不等，说明是insert的数据
                val["__state"] = "1";

                $.each(opt.keyData, function (key, value) {
                    val[key] = value;
                });

                delete val["id"];
                delete val["_id_"];
                delete val["rn"];
                if (typeof _changeDatArray[opt.gridId] == "undefined") {
                    _changeDatArray.push(val);
                } else {
                    _changeDatArray[opt.gridId].push(val);
                }

                var addRowData = {};

                $.each(opt.colModel, function (key, value) {
                    addRowData[value.name] = val[value.name];
                });



                dsData.push(addRowData);
            }
        }
    });
    return;
}

function myelem(value, options) {
    console.log(options);

    if (typeof options.readonly == "undefined") {
        options.readonly = "";
    }

    var thisId = options.rowId + "_" + options.id.split("_")[1];
    var tag = ' <div class="input-group">\
                        <input type="text" class="form-control input-sm grid-autocomp" role="textbox" id="' + thisId + 'Input"  value="' + value + '" ' + options.readonly + '>\
                        <span class="input-group-btn">\
                            <button class="btn btn-sm btn-default" role="lookup" type="button" id="' + thisId + 'Lookup">\
                                <span  role="lookup" class="glyphicon glyphicon-search"></span>\
                            </button>\
                        </span>\
                    </div>';
    return tag;
}


function myvalue(elem, operation, value) {
    //console.log($(elem)[0]);
    if ($(elem).context.id != "")
        var thisId = $(elem).context.id + "_" + $(elem)[0].id.split("_")[1];
    else
        var thisId = $(elem)[0].id;

    if (operation === 'get') {
        return $("#" + thisId + "Input").val();
    } else if (operation === 'set') {
        $("#" + thisId + "Input").val(value);
    }
}




function responsive_jqgrid(jqgrid) {
    jqgrid.find('.ui-jqgrid').addClass('clear-margin span12').css('width', '');
    jqgrid.find('.ui-jqgrid-view').addClass('clear-margin span12').css('width', '');
    jqgrid.find('.ui-jqgrid-view > div').eq(1).addClass('clear-margin span12').css('width', '').css('min-height', '0');
    jqgrid.find('.ui-jqgrid-view > div').eq(2).addClass('clear-margin span12').css('width', '').css('min-height', '0');
    jqgrid.find('.ui-jqgrid-sdiv').addClass('clear-margin span12').css('width', '');
    jqgrid.find('.ui-jqgrid-pager').addClass('clear-margin span12').css('width', '');
}
//執行儲存LAYOUT的動作 注意:layoutid為grid的CAPTION，故不要重複
function DoSaveLayout(grid, captionname) {
    var colModel = $.extend(true, [], grid.jqGrid("getGridParam", "colModel").slice());
    var caption = grid.jqGrid("getGridParam", "caption");
    var resultModel = new Array();

    var delCol = 0;
    for (var i = 0; i < colModel.length; i++) {

        if (colModel[i].name == "rn") {
            delete colModel[i];
            delCol++;
            continue;
        }
        if (colModel[i].name == "cb") {
            delete colModel[i];
            delCol++;
            continue;
        }
        $.each(colModel[i], function (key, value) {

            if (key != "hidden" && key != "width" && key != "name" && key != "formatter" && key != "editoptions" && key != "editoptions" && key != "classes") {
                delete colModel[i][key];
            }
        });
        resultModel.push(colModel[i]);
    }

    $.ajax({
        async: true,
        url: rootPath + "Common/SetLayout",
        type: 'POST',
        data: { "layout": JSON.stringify(resultModel), "layoutid": caption, "layouttype": "GRID", layoutName: captionname },
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
        },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        "success": function (result) {
            if (result) {
                //alert("Save Grid Layout Success!");
            }

        }
    });
}
function DeleteLayout(grid, captionname) {
    var caption = grid.jqGrid("getGridParam", "caption");
    $.ajax({
        async: true,
        url: rootPath + "Common/DeleteLayout",
        type: 'POST',
        data: { "layoutid": caption, "layouttype": "GRID", layoutName: captionname },
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
        },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        "success": function (result) {
            if (result) {
                if (result[0].LAYOUT != null) {
                    var myModel = jQuery.parseJSON(result[0].LAYOUT);
                    grid.jqGrid('getGridParam', "ResetGridColmodel")(myModel);
                }
            }
        }
    });
}
function DoResetLayout(grid, captionname, alertconfirm) {
    var caption = grid.jqGrid("getGridParam", "caption");
    var isconfirm = true;
    if (alertconfirm == true) {
        isconfirm = confirm("Are you sure reset this layout? This page will reloaded!");
    }
    if (isconfirm == true) {
        $.ajax({
            async: true,
            url: rootPath + "Common/ResetLayout",
            type: 'POST',
            data: { "layoutid": caption, "layouttype": "GRID", layoutName: captionname },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            "success": function (result) {
                if (result) {
                    var myModel = jQuery.parseJSON(result[0].LAYOUT);
                    grid.jqGrid('getGridParam', "ResetGridColmodel")(myModel);
                }
            }
        });
    }
}
//取得LAYOUT 透過CAPTION名稱當作layoutid 取得完成後會產生GRID
//TODO 目前是每次LOAD都會去抓一次，未來可以執行暫存
function GetLayout($grid, gop, toolId, captionname) {
    //console.log(gop.caption);
    $.ajax({
        async: false,
        url: rootPath + "Common/GetLayout",
        type: 'POST',
        data: { "layoutid": gop.caption, "layouttype": "GRID", "captionname": captionname},
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        "success": function (result) {
            if (result[0] == null) { 
                genGrid($grid, gop, toolId);
                return true;
            } 
            var myModel = jQuery.parseJSON(result[0].LAYOUT);
            var resultModel = null;

            if (myModel != null && myModel != "") {
                for (var i = 0; i < myModel.length; i++) {
                    var objKey = jQuery.grep(Object.keys(gop.colModel), function (key, value) {
                        return gop.colModel[key].name == myModel[i].name;
                    });
                    if (typeof gop.colModel[objKey] != 'undefined') {
                        gop.colModel[objKey].hidden = myModel[i].hidden;
                        gop.colModel[objKey].name = myModel[i].name;
                        gop.colModel[objKey].width = myModel[i].width;
                        $.extend(true, myModel[i], gop.colModel[objKey]);
                    }
                }
                var _colModel = gop.colModel;
                for (var i = 0; i < _colModel.length; i++) {
                    var objKey = jQuery.grep(Object.keys(myModel), function (key, value) {
                        return myModel[key].name === _colModel[i].name;
                    });
                    if (objKey === undefined || (objKey && objKey.length <= 0)) {
                        if (_colModel[i].name && _colModel[i].index) {
                            var col = $.extend(true, {}, _colModel[i]);
                            //col.hidden = true;
                            myModel.push(col);
                        }
                    }
                }
                gop.colModel = myModel;

            }
            var __myModel = [];
            for (var i = 0; i < gop.colModel.length; i++) {
                if ((gop.colModel[i] && gop.colModel[i].name && gop.colModel[i].index) || (gop.colModel[i].name == "UId" && !gop.colModel[i].index)) {
                    __myModel.push(gop.colModel[i]);
                }
            }
            gop.colModel = __myModel;
            if (result[1] != null) {
                gop.layoutlist = result[1].LAYOUT_ID_NAME;
            }
            if (result[2] != null)
            {
                _selectIdName = result[1].SELECT_ID_NAME;
            }
            genGrid($grid, gop, toolId);
        }
    });
}


function GetGridModel(caption, colModel) {
    $.ajax({
        async: false,
        url: rootPath + "Common/GetGridModel",
        type: 'POST',
        data: { "layoutid": caption, "layouttype": "GRID", "ColumnList": JSON.stringify(colModel).replace(new RegExp('"', "gm"), "'") },
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        "success": function (result) {
            if (result.isok) {
                $.each(colModel, function (index, colmodel) {
                    var collist = jQuery.parseJSON(result.collist);
                    $.each(collist, function (rindex, rcolmodel) {
                        if (colmodel.name == rcolmodel.name) {
                            colmodel = $.extend(colmodel, rcolmodel);
                            return;
                        }
                    });
                });
            }
        }
    });
    return colModel;
}

function ExportDataToExcel(grid) {
    var url = grid.jqGrid("getGridParam", "url");
    if (url == null || url == "") {
        return false;
    }
    var colModel = grid.jqGrid("getGridParam", "colModel").slice();

    var colNames = grid.jqGrid("getGridParam", "colNames");
    var caption = grid.jqGrid("getGridParam", "caption");
    var excelName = grid.jqGrid("getGridParam", "excelName");
    var conditions = grid.jqGrid("getGridParam", "postData").conditions;
    var baseCondition = grid.jqGrid("getGridParam", "postData").baseCondition;
    var virConditions = grid.jqGrid("getGridParam", "postData").virConditions

    if (typeof baseCondition == "undefined") {
        baseCondition = "";
    }

    for (var i = 0; i < colModel.length; i++) {
        if (i == 0) {
            delete colModel[0];

            continue;
        }

        if (colModel[i].hidden == true || typeof colModel[i] == "undefined" || typeof colNames[i] == "undefined") {
            delete colModel[i];
            continue;
        }
        if (colModel[i].name == "cb") {
            delete colModel[i];
            continue;
        }


        colModel[i].caption = colNames[i];

        if (colModel[i].sorttype == "int") {
            colModel[i].sorttype = 1;
        } else if (colModel[i].sorttype == "float") {
            colModel[i].sorttype = 2;
        } else {
            colModel[i].sorttype = 0;
        }
    }
    console.log(JSON.stringify(colModel).replace(new RegExp('"', "gm"), "'"));
    //alert(url);
    postAndRedirect(url, { "ColumnList": JSON.stringify(colModel).replace(new RegExp("&#39;", "gm"), "ft").replace(new RegExp('"', "gm"), "'"), "conditions": conditions, "baseCondition": baseCondition,"virConditions":virConditions, "resultType": "excel", "ReportTitle": caption, "excelName": excelName, });
}
function ExportDataToExcelByParam(url, colModel, colNames, caption, excelName, conditions, baseCondition, virtualCol,vircondition) {
    var url = url;
    var colModel = colModel;
    var colNames = colNames;
    var caption = caption;
    var excelName = excelName;
    var conditions = conditions;
    var virconditions = vircondition;
    var baseCondition = baseCondition

    for (var i = 0; i < colModel.length; i++) {
        if (i == 0) {
            delete colModel[0];
            continue;
        }

        if (colModel[i].hidden == true || typeof colModel[i] == "undefined" || typeof colNames[i] == "undefined") {
            delete colModel[i];
            continue;
        }
        if (colModel[i].name == "cb") {
            delete colModel[i];
            continue;
        }
        colModel[i].caption = colNames[i];
        if (colModel[i].sorttype == "int") {
            colModel[i].sorttype = 1;
        } else if (colModel[i].sorttype == "float") {
            colModel[i].sorttype = 2;
        } else {
            colModel[i].sorttype = 0;
        }
    }
    postAndRedirect(url, { "ColumnList": JSON.stringify(colModel).replace(new RegExp("&#39;", "gm"), "ft").replace(new RegExp('"', "gm"), "'"), "virConditions": virconditions, "conditions": conditions, "baseCondition": baseCondition, "resultType": "excel", "ReportTitle": caption, "excelName": excelName, "virtualCol": virtualCol });
}

function initGrid($grid, param, gop, toolId) {

    gop.data = param.data;
    gop.colModel = param.colModel;
    gop.baseColModel = $.extend(true, {}, param.colModel);
    gop.gridFunc = param.gridFunc;
    if (param.dblClickFunc != null)
        gop.dblClickFunc = param.dblClickFunc;
    if (param.beforeSelectRowFunc != null)
        gop.beforeSelectRowFunc = param.beforeSelectRowFunc;
    if (param.onSelectRowFunc != null)
        gop.onSelectRowFunc = param.onSelectRowFunc;


    GetLayout($grid, gop, toolId);
}

function genGrid($grid, gop, toolId) { 
    if (gop.isModel) {
        gop.colModel = GetGridModel(gop.caption, gop.colModel);
    }

    idsOfSelectedRows[$grid[0].id] = [];

    if ($.isArray(gop.delKey)) {
        var delRowKeys = [];
    } else {
        var delRowKeys = "";
    }
    var oldValue = "";
    var selICol; //iCol of selected cell
    var selIRow; //iRow of selected cell
    var editable = false;
    //若使用外部FORM填打資料，此項為TRUE
    if (gop.outEdit) {
        editable = true;
    }
    var functions = {};


    function _getLang(id) {
        try {
            return GetLangCaption(id);
        }
        catch (e) { }
        return id;
    }

    if ($("#gridMenu").html() == null) {
        var contextmenu = '<div class="contextMenu" id="gridMenu" style="display:none">\
            <ul style="width: 200px">\
                 <li id="add">\
                    <span class="ui-icon ui-icon-plus" style="float:left"></span>\
                    <span style="font-size:11px; font-family:Verdana">' + _getLang("插入一行") + '</span>\
                </li>\
                <li id="del">\
                    <span class="ui-icon ui-icon-trash" style="float:left"></span>\
                    <span style="font-size:11px; font-family:Verdana">' + _getLang("删除一行") + '</span>\
                </li>\
            </ul>\
        </div>';
        $("body").append(contextmenu);
    }

    var _options = [];
    var layoutlist = gop.layoutlist;
    if (layoutlist !== null && layoutlist != undefined && layoutlist != "") {
        var layoutArr = layoutlist.split(';');
        for (var i = 0; i < layoutArr.length; i++) {
            var val = {};
            if (layoutArr[i] != null && layoutArr[i] != "") {
                _options.push(layoutArr[i])
            }
        }
    }

    if ($("#LayoutWindow_" + $grid[0].id).html() == null) {
        var layoutWindow = '<div class="modal fade" id="LayoutWindow_' + $grid[0].id + '" role="dialog"  >\
            <div class="modal-dialog" class="modal-dialog modal-lg" style="width:300px;" >\
                <div class="modal-content">\
                    <div class="modal-header">\
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
                        <h4 class="modal-title">Set Layout</h4>\
                    </div>\
                    <div class="modal-body">\
                        <div class="pure-g">\
                             <div class="pure-u-sm-18-60">\
                                 <label class= "control-label" >Layout Name</label >\
                             </div>\
                            <div class="pure-u-sm-4-60">\
                                  <select class="form-control input-sm" id="sel_layoutId_'+ $grid[0].id + '"></select>\
                             </div>\
                             <div class="pure-u-sm-30-60">\
                                  <input type="text" class="form-control input-sm" id="savelayoutId_'+ $grid[0].id + '"/>\
                             </div>\
                          </div>\
                     </div >\
                </div >\
           </div>\
        </div> ';
        $("body").append(layoutWindow);
        //<button class="btn btn-sm btn-info" type="button" id="win_selectLayout">Select</button>\
        $("#LayoutWindow_" + $grid[0].id).find(".modal-content").append('<div class="modal-footer">\
                         <button class="btn btn-sm btn-success" type="button" id="win_saveLayout_'+ $grid[0].id + '">Save</button>\
                         <button class="btn btn-sm btn-danger" type="button" id="win_deleteLayout_'+ $grid[0].id + '">Delete</button>\
                    </div >');
        $("#win_deleteLayout_" + $grid[0].id).click(function () {
            var val = $("#sel_layoutId_" + $grid[0].id).find("option:selected").text();
            if ((val || "") === "") {
                alert("please select layout first");
                $("#sel_layoutId_" + $grid[0].id).focus();
                return;
            }

            if (val === "default") {
                alert("default layout can not delete!");
                $("#sel_layoutId_" + $grid[0].id).focus();
                return;
            }
            if (confirm(_getLang("L_DeleteLayout") + val + "?") == true) {
                DeleteLayout($grid, val);
                $("#sel_layoutId_" + $grid[0].id).find("option:selected").remove();
                $("#LayoutWindow_" + $grid[0].id).modal("hide");
                alert(_getLang("L_BSCSSetup_DelS"));//"删除布局成功");
            }

        });

        $("#win_selectLayout_" + $grid[0].id).click(function () {
            var val = $("#sel_layoutId_" + $grid[0].id).find("option:selected").text();
            if ((val || "") === "") {
                alert("please select layout first");
                $("#sel_layoutId_" + $grid[0].id).focus();
                return;
            }
            $("#savelayoutId_" + $grid[0].id).val(val);
            DoResetLayout($grid, val);
        });

        $("#win_saveLayout_" + $grid[0].id).click(function () {
            var val = $("#savelayoutId_" + $grid[0].id).val();
            if ((val || "") === "") {
                alert("please input layout first");
                return;
            }

            if (val.indexOf(" ") >= 0) {
                alert("The layout name cannot contain spaces")
                return;
            }

            if (val === "default" && "ADMIN" != userId) {
                alert("default layout can not modify!");
                return;
            }

            var options = $("#sel_layoutId_" + $grid[0].id).find("option");
            for (var i = 0; i < options.length; i++) {
                if ($(options[i]).text() == val) {
                    if (confirm(_getLang("L_RecoverLayout") + val + "?") == true) {
                        DoSaveLayout($grid, val);
                        $("#LayoutWindow_" + $grid[0].id).modal("hide");
                    }
                    return;
                }
            }
            _options.push(val);
            $("#sel_layoutId_" + $grid[0].id).append("<option>" + val + "</option>");
            $("#LayoutWindow_" + $grid[0].id).modal("hide");
            DoSaveLayout($grid, val);
            alert(_getLang("L_NewLayout"));
        });
        $("#sel_layoutId_" + $grid[0].id).change(function () {

            var val = $("#sel_layoutId_" + $grid[0].id).find("option:selected").text();
            if ((val || "") === "") {
                alert("please select layout first");
                $("#sel_layoutId_" + $grid[0].id).focus();
                return;
            }
            $("#savelayoutId_" + $grid[0].id).val(val);
            DoResetLayout($grid, val);
        });
    }

    var AddRowOptions = {
        keys: true,

        oneditfunc: function (rowid) {
            if (gop.afterAddRowFunc != null) {
                if (gop.afterAddRowFunc) {
                    gop.afterAddRowFunc(rowid);
                }
            }
            if (gop.cellCalFunc != null) {
                if (gop.cellCalFunc) {
                    gop.cellCalFunc(rowid);
                }
            }
            var jsonMap = $(this).jqGrid("getRowData", rowid);

            console.log(jsonMap);
            if (gop.gridFunc != null) {
                if (gop.gridFunc) {
                    //遍历selectRowsData
                    gop.gridFunc(jsonMap);
                }
            }
        },
        aftersavefunc: function (rowid, response, options) {
            if (gop.cellCalFunc != null) {
                if (gop.cellCalFunc) {
                    gop.cellCalFunc(rowid);
                }
            }
            if (gop.afterAddRowWithIdFunc != null) {
                if (gop.afterAddRowWithIdFunc) {
                    gop.afterAddRowWithIdFunc(rowid, toolId);
                }
            }
        }
    };

    var appendSettings = {
        rowID: undefined,
        initdata: { name: "1", id: undefined },
        position: "afterSelected",
        useDefValues: false,
        useFormatter: false,
        addRowParams: AddRowOptions
    },
       addSettings = {
           rowID: undefined,
           initdata: { name: "1", id: undefined },
           position: "last",
           useDefValues: false,
           useFormatter: false,
           addRowParams: AddRowOptions
       },
       delSettings = {
           reloadAfterSubmit: false,
           afterShowForm: function (form) {
               form.closest('div.ui-jqdialog').center();
           },
           onclickSubmit: function (options) {
               var grid_id = $grid[0].id,
                   grid_p = $grid[0].p,
                   newPage = $grid[0].p.page;

               $grid.jqGrid("editCell", 0, 0, true);
               $grid.jqGrid("saveCell", lastSel, selICol);
               $grid.jqGrid('saveRow', lastSel, false);

               if (gop.delRowFunc != null) {
                   if (gop.delRowFunc) {
                       if (!gop.delRowFunc(lastSel)) {
                           return true;
                       }
                   }
               }

               var ds = $grid.jqGrid("getGridParam", "ds");
               if (ds != null) {
                   //delete 操作设置_state
                   ds.setCurIndexByGrid($grid, lastSel);
                   console.log(lastSel);
                   ds.setCurVal("__state", "0");
                   ds.setCurVal("_state", "0");
                   console.log(ds);
               }

               if ($.isArray(gop.delKey)) {
                   var val = {};
                   for (var k = 0; k < gop.delKey.length; k++) {
                       var key = gop.delKey[k];
                       val[key] = $grid.jqGrid('getCell', lastSel, gop.delKey[k]);

                   }
                   delRowKeys.push(val);
               } else {
                   var data_id = $grid.jqGrid('getCell', lastSel, gop.delKey);
                   delRowKeys += data_id + "|";
               }

               $grid.jqGrid('delRowData', lastSel);
               $.jgrid.hideModal("#delmod" + grid_id,
                                 { gb: "#gbox_" + grid_id, jqm: options.jqModal, onClose: options.onClose });

               if (grid_p.lastpage > 1) {// on the multipage grid reload the grid
                   if (grid_p.reccount === 0 && newPage === grid_p.lastpage) {
                       // if after deliting there are no rows on the current page
                       // which is the last page of the grid
                       newPage -= 1; // go to the previous page
                   }
                   // reload grid to make the row from the next page visable.
                   $grid.trigger("reloadGrid", [{ page: newPage }]);
               }

               if (gop.afterDelRowFunc != null) {
                   if (gop.afterDelRowFunc) {
                       gop.afterDelRowFunc(lastSel);
                   }
               }
               if (gop.cellCalFunc != null) {
                   if (gop.cellCalFunc) {
                       gop.cellCalFunc(lastSel);
                   }
               }

               return true;
           },
           processing: true
       };
    var lastSel,
    oldAddRowData = $.fn.jqGrid.addRowData;
    $.jgrid.extend({
        addRowData: function (rowid, rdata, pos, src) {
            if (pos === 'afterSelected' || pos === 'beforeSelected') {
                if (typeof src === 'undefined' && this[0].p.selrow !== null) {
                    src = this[0].p.selrow;
                    pos = (pos === "afterSelected") ? 'after' : 'before';
                } else {
                    pos = (pos === "afterSelected") ? 'last' : 'first';
                }
            }
            return oldAddRowData.call(this, rowid, rdata, pos, src);
        }
    });

    var doAddRowFunc = function () {
        if (gop.beforeAddRowFunc != null) {
            if (gop.beforeAddRowFunc) {
                //遍历selectRowsData
                if (gop.beforeAddRowFunc() == false) {
                    $grid.trigger("reloadGrid");
                    $grid.jqGrid("clearGridData");
                    return false;
                }
            }
        }
        /*var selRowId = $grid.jqGrid('getGridParam', 'selrow');

        if (selRowId != null && selRowId != "") {
            $grid.jqGrid('saveRow', selRowId, false, 'clientArray');
            $grid.jqGrid('editCell', selRowId, 0, true);
        }*/
        $grid.jqGrid("addRow", addSettings);
        var selRowId = $grid.jqGrid('getGridParam', 'selrow');


        if (gop.onAddRowFunc != null) {
            if (gop.onAddRowFunc) {
                gop.onAddRowFunc(selRowId);
            }
        }
        if (gop.onAddRowWithIdFunc != null) {
            if (gop.onAddRowWithIdFunc) {
                gop.onAddRowWithIdFunc(selRowId, toolId);
            }
        }


        var colindex = 0;
        var colModel = $grid.jqGrid('getGridParam', 'colModel');
        var colName = "";
        for (var idx in colModel) {
            if ($grid.jqGrid('getColProp', colModel[idx].name).editable) {
                colName = colModel[idx].name;
                break;
            }
            if (colindex + 1 == colModel.length) {
                break;
            }
            colindex++;
        }
        $grid.jqGrid('saveRow', selRowId, false, 'clientArray');
        $grid.jqGrid('editCell', selRowId, colindex, true);
        $("#" + selRowId + "_" + colName).select();
        editable = true;
    };

    var doAppendRowFunc = function () {

        var selRowId = $grid.jqGrid('getGridParam', 'selrow');
        lastSel = selRowId;
        if (gop.beforeAddRowFunc != null) {
            if (gop.beforeAddRowFunc) {
                //遍历selectRowsData
                if (gop.beforeAddRowFunc() == false) {
                    $grid.trigger("reloadGrid");
                    $grid.jqGrid("clearGridData");
                    return false;
                }
            }
        }

        $grid.jqGrid('saveRow', selRowId, false, 'clientArray');
        $grid.jqGrid('getGridParam', "endEdit")();
        $grid.jqGrid('setSelection', lastSel, true);
        $grid.jqGrid("addRow", appendSettings);

        var selRowId = $grid.jqGrid('getGridParam', 'selrow');
        $grid.jqGrid('saveRow', selRowId, false, 'clientArray');
        $grid.jqGrid('getGridParam', "endEdit")();

        if (gop.onAddRowFunc != null) {
            if (gop.onAddRowFunc) {
                gop.onAddRowFunc(selRowId);
            }
        }
        if (gop.onAddRowWithIdFunc != null) {
            if (gop.onAddRowWithIdFunc) {
                gop.onAddRowWithIdFunc(selRowId, toolId);
            }
        }
        var colindex = 0;
        var colModel = $grid.jqGrid('getGridParam', 'colModel');
        var colName = "";
        for (var colindex in colModel) {
            if ($grid.jqGrid('getColProp', colModel[colindex].name).editable) {
                colName = colModel[colindex].name;
                break;
            }
            colindex++;
        }
        $grid.jqGrid('saveRow', selRowId, false, 'clientArray');
        $grid.jqGrid('editCell', selRowId, colindex, true);

        $("#" + selRowId + "_" + colName).select();
        editable = true;
    };

    var doDelRowFunc = function () {

        if (gop.beforeDelRowFunc != null) {
            if (gop.beforeDelRowFunc) {
                if (!gop.beforeDelRowFunc(lastSel)) {
                    return false;
                }
            }
        }

        $grid.jqGrid('delGridRow', lastSel, delSettings);
        editable = true;
    };
    gop.editOnly = (gop.editOnly == null) ? false : gop.editOnly
    gop.lockRKey = (gop.lockRKey == null) ? false : gop.lockRKey
    var grid_options = {
        cmTemplate: { sortable: gop.sortable },

        //hidedlg: true,
        //設定資料取得方法
        datatype: 'local',
        //data: mydata,
        rowNum: gop.rows || 20,
        //colModel: myColModel,
        //rowNum: 20,
        sortorder: gop.sortorder,
        //多欄位排序需要,隔開 且 multiSort:true
        sortname: gop.sortname,
        //rowList: [20, 50, 100],
        rownumWidth: 50,
        minHeight: "100px",
        //pager: targetPager,
        gridview: true,
        rownumbers: true,
        cloneToTop: true,
        toppager: true,
        viewrecords: true,
        //同時指定以下兩個參數，畫面可縮小，且不會擠在一塊
        shrinkToFit: false,
        width: null,
        editOnly: gop.editOnly,
        //height: null,
        caption: "GridTable",
        multiselect: gop.multiselect,
        //sortable=true欄位允許調動 PS：注意，不得與setFrozenColumns共用
        sortable: gop.sortable,
        multiSort: gop.multiSort,
        cellEdit: (gop.cellEdit == null) ? false : gop.cellEdit,
        baseCondition: gop.baseCondition,
        //cellsubmit: 'clientArray',
        scrollerbar: gop.scrollerbar,
        height: gop.height,
        editurl: 'clientArray',
        addurl: 'clientArray',
        //toolbar: [true,"top"],
        beforeRequest: function (rowid, iRow, iCol, e) {
            responsive_jqgrid($(".jqGrid"));
        },
        ondblClickRow: function (rowid, iRow, iCol, e) {
            var jsonMap = $(this).jqGrid("getRowData", rowid);
            //console.log(jsonMap);
            if (gop.gridFunc != null) {
                if (gop.gridFunc) {
                    //遍历selectRowsData
                    gop.gridFunc(jsonMap);
                }
            }
            if (gop.dblClickFunc != null) {
                if (gop.dblClickFunc) {
                    //遍历selectRowsData
                    gop.dblClickFunc(jsonMap);
                }
            }
        },
        onSelectRow: function (id, status) {


            var jsonMap = $(this).jqGrid("getRowData", id);
            var selectedRow = $(this).jqGrid('getGridParam', 'selarrrow');

            if (gop.gridFunc != null) {
                if (gop.gridFunc) {
                    //遍历selectRowsData
                    gop.gridFunc(jsonMap);
                }
            }
            if (gop.onSelectRowFunc != null) {
                if (gop.onSelectRowFunc) {
                    //遍历selectRowsData
                    jsonMap.gridId = id;
                    gop.onSelectRowFunc(jsonMap);
                }
            }
            if (gop.multlSelectFunc != null) {
                if (gop.multlSelectFunc) {
                    //遍历selectRowsData
                    gop.multlSelectFunc($(this), jsonMap, selectedRow);
                }
            }

            if (typeof gop.columnID != "undefined") {
                id = $(this).jqGrid('getCell', id, gop.columnID);
            }
            if ($(this).jqGrid('getGridParam', 'multiselect')) {
                updateIdsOfSelectedRows(id, status, $grid[0].id);
            }

            var ds = $grid.jqGrid("getGridParam", "ds");
            if (ds != null) {
                ds.setCurIndexByGrid($grid, id);
            }
        },
        onSelectAll: function (aRowids, status) {

            var i, count, id;
            for (i = 0, count = aRowids.length; i < count; i++) {
                id = aRowids[i];

                if (typeof gop.columnID != "undefined") {
                    id = $(this).jqGrid('getCell', id, gop.columnID);
                }

                updateIdsOfSelectedRows(id, status, $grid[0].id);
            }
            var jsonMap = $(this).jqGrid("getRowData", aRowids);
            var selectedRow = $(this).jqGrid('getGridParam', 'selarrrow');
            if (gop.multlSelectFunc != null) {
                if (gop.multlSelectFunc) {
                    //遍历selectRowsData
                    gop.multlSelectFunc($(this), jsonMap, selectedRow);
                }
            }
        },
        endEdit: function () {
            try {
                $grid.jqGrid("editCell", 0, 0, true);
            } catch (e) {

            }

            $grid.jqGrid('saveRow', selIRow, false)
            $grid.jqGrid("saveCell", selIRow, selICol);
            if (gop.afterAddRowWithIdFunc != null) {
                if (gop.afterAddRowWithIdFunc) {
                    gop.afterAddRowWithIdFunc(lastSel, toolId);
                }
            }


            return true;
        },
        beforeSelectRow: function (rowid, e) {
            if (rowid != lastSel) {
                $(this).jqGrid('saveRow', lastSel);
                lastSel = rowid;

                if (gop.beforeSelectRowFunc != null) {
                    if (gop.beforeSelectRowFunc) {
                        //遍历selectRowsData
                        gop.beforeSelectRowFunc(rowid, e);
                    }
                }

                var jsonMap = $(this).jqGrid("getRowData", rowid);
                if (gop.gridFunc != null) {
                    if (gop.gridFunc) {
                        //遍历selectRowsData
                        gop.gridFunc(jsonMap);
                    }
                }
            }


            return true;
        },
        afterEditCell: function (id, cellname, val, iRow, iCol) {

            var nowColumns = $grid.jqGrid('getGridParam', 'colNames');
            var lastCol = nowColumns[nowColumns.length];
            //check if this is last column
            if (iCol + 1 == nowColumns.length) {
                var inputControl = $('#' + (iRow) + '_' + cellname);
                var fristEditableCol = 1;
                // Listen for tab (and shift-tab)
                inputControl.bind("keydown", function (e) {
                    if (e.keyCode === 9) {  // TAB key:
                        var increment = e.shiftKey ? -1 : 1;
                        // Do not go out of bounds
                        var lastRowInd = $grid.jqGrid("getGridParam", "reccount")

                        if (iRow + increment == 0 || iRow + increment == lastRowInd + 1) {

                        } else {
                            var colindex = 0;
                            var colModel = $grid.jqGrid('getGridParam', 'colModel');
                            for (var colindex in colModel) {
                                if ($grid.jqGrid('getColProp', colModel[colindex].name).editable) {
                                    break;
                                }
                                colindex++;
                            }
                            $grid.jqGrid("editCell", iRow + increment, colindex, true);

                        }
                    }
                }); // End keydown binding
            } else {

                var inputControl = $('#' + (iRow) + '_' + cellname);
                // Listen for enter
                inputControl.bind("keydown", function (e) {

                    if (e.ctrlKey && e.keyCode === 13) {//ctrl+enter
                        doAddRowFunc();
                    } else if (e.keyCode == 13)//enter
                    {
                        if (gop.enterdown) {
                            $grid.jqGrid("editCell", iRow + 1, iCol, true);
                            $("input").focus(function () {
                                //alert("Handler for .focus() called.");
                                this.select();
                            });
                        }
                    }

                }); // End keydown binding
            }

            $("input").focus(function () {
                //alert("Handler for .focus() called.");
                this.select();
            });
        },
        beforeEditCell: function (rowid, cellname, value, iRow, iCol) {
            editable = true;
            var jsonMap = $(this).jqGrid("getRowData", rowid);
            if (gop.gridFunc != null && selIRow != iRow) {
                if (gop.gridFunc) {
                    //遍历selectRowsData
                    gop.gridFunc(jsonMap);
                }
            }

            if (gop.onSelectRowFunc != null) {
                if (gop.onSelectRowFunc) {
                    jsonMap.gridId = rowid;
                    //遍历selectRowsData
                    gop.onSelectRowFunc(jsonMap);
                }
            }
            if (gop.beforeEditCellFunc != null) {
                if (gop.beforeEditCellFunc) {
                    //遍历selectRowsData
                    gop.beforeEditCellFunc(jsonMap);
                }
            }
            selICol = iCol;
            selIRow = iRow;
            //save old cell value
            oldValue = $grid.jqGrid('getCell', rowid, iCol);


        },
        onCellSelect: function (rowid, cellname, value, iRow, iCol) {
            var cm = $grid.jqGrid("getGridParam", "colModel");
            var jsonMap = $(this).jqGrid("getRowData", rowid);

            if (gop.onCellSelFunc != null) {
                if (gop.onCellSelFunc) {
                    gop.onCellSelFunc(rowid, cm[cellname].name, value, jsonMap);
                }
            }
            if (gop.onSelectRowFunc != null) {
                if (gop.onSelectRowFunc) {
                    //遍历selectRowsData
                    jsonMap.gridId = rowid;
                    gop.onSelectRowFunc(jsonMap);
                }
            }

            $("input").focus(function () {
                //alert("Handler for .focus() called.");
                this.select();
            });
        },
        beforeSaveCell: function (rowid, name, val, iRow, iCol) {
            focused = $grid.find(".grid-autocomp");
            var thisKey = rowid + "_" + name + "_" + val;
            if (focused.length > 0) {
                var veriKey = focused.attr("id").replace("Input", "") + "_" + focused.val();
                if (thisKey == veriKey)
                    focused.trigger("autocompletechangefunc", val);
                if (typeof val == "undefined") {
                    val = "";
                }
                return val.toUpperCase();
            }
            var upperFieldArr = $('.uppercase>input');
            var upperFieldName = upperFieldArr.attr("name");
            if (name == upperFieldName) {

                return val.toUpperCase();
            }
        },
        afterSaveCell: function (rowid, name, val, iRow, iCol) {

            var ds = $grid.jqGrid("getGridParam", "ds");
            if (ds && ds.setCurVal) {
                ds.setCurIndexByKey($grid.jqGrid("getRowData", rowid));
                ds.setCurVal(name, val);
            }
            if (gop.afterSaveCellFunc != null) {
                if (gop.afterSaveCellFunc) {
                    //遍历selectRowsData
                    gop.afterSaveCellFunc(rowid, name, val, iRow, iCol);
                }
            }
            if (gop.afterSaveCellWithIdFunc != null) {
                if (gop.afterSaveCellWithIdFunc) {
                    //遍历selectRowsData
                    gop.afterSaveCellWithIdFunc(rowid, name, val, iRow, iCol, toolId);
                }
            }
        },
        afterSaveRow: function (rowid, name, val, iRow) {
            var ds = $grid.jqGrid("getGridParam", "ds");
            if (ds && ds.setCurVal) {
                ds.setCurIndexByKey($grid.jqGrid("getRowData", iRow));
                ds.setCurVal(name, val);
            }
        },
        loadComplete: function (data) {
            if (!gop.editOnly && !gop.lockRKey) {
                //右鍵MENU
                $grid.contextMenu("gridMenu", {
                    bindings: {
                        'add': function (trigger) {
                            doAppendRowFunc();
                        },
                        'del': function (trigger) {
                            if ($('#del').hasClass('ui-state-disabled') === false) {
                                doDelRowFunc();
                            }
                        }
                    },
                    onContextMenu: function (event) {
                        if (editable) {
                            lastSel = $(event.target).closest("tr.jqgrow").attr("id");
                            return true;
                        } else {
                            return false;
                        }
                    }
                });
            }

            //when grid focus out 
            $("#wrapper").click(function (e) {
                if (typeof $(e.target).attr("role") == "undefined" && editable) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    if (typeof selRowId != "undefined") {
                        try {
                            $grid.jqGrid('saveRow', selRowId);
                            $grid.jqGrid("editCell", selRowId, 0, true);

                            if (gop.afterAddRowFunc != null) {
                                if (gop.afterAddRowFunc) {
                                    gop.afterAddRowFunc(selRowId);
                                }
                            }
                            if (gop.cellCalFunc != null) {
                                if (gop.cellCalFunc) {
                                    gop.cellCalFunc(selRowId);
                                }
                            }
                            if (gop.afterAddRowWithIdFunc != null) {
                                if (gop.afterAddRowWithIdFunc) {
                                    gop.afterAddRowWithIdFunc(selRowId, toolId);
                                }
                            }
                        }
                        catch (err) {

                        }
                    }
                    editable = false;
                }
            });

            //设置值
            var ds = $grid.jqGrid("getGridParam", "ds");
            if (ds != null) {
                var __gridData = data.rows || data;
                if (__gridData.length <= 0)
                    __gridData = $grid.jqGrid("getGridParam", "data");
                ds.setData(__gridData);
            }

            $grid.jqGrid('getGridParam', "loadCompleteFunc")($grid);

            if (functions.loadComplete)
                functions.loadComplete(data);


            var allData = $grid.jqGrid("getRowData");
            var selData = $grid.jqGrid('getGridParam', 'selarrrow');
            var checkId = gop.columnID;
            var rowid = 1;
            $.each(allData, function (i, val) {
                if ($.inArray(val[checkId], idsOfSelectedRows[$grid[0].id]) > -1 && $.inArray(rowid + "", selData) == -1) {
                    $grid.jqGrid('setSelection', rowid, true);
                }
                rowid++;
            });

            if (gop.setSelectedFunc != null) {
                if (gop.setSelectedFunc) {
                    gop.setSelectedFunc();
                }
            }
        },
        serializeGridData: function (postData) {
            var myPostData = $.extend({}, postData); // make a copy of the input parameter
            if (myPostData.sidx != "") {
                var colModel = $grid.jqGrid('getGridParam', 'colModel');
                $.each(colModel, function (key, value) {
                    if (value.name != "rn" && typeof value.dt == "undefined") {
                        return false;
                    }
                    if (value.name == myPostData.sidx) {
                        myPostData.sidx = value.dt + "." + myPostData.sidx.substring(0, 1).toLowerCase() + myPostData.sidx.substring(1, myPostData.sidx.length);
                        return myPostData;
                    }

                });
            }
            return myPostData;
        },
        appendAddRowButton: function () {
            editable = true;
            //欄位選擇功能 上方
            $grid.jqGrid('navButtonAdd', '#' + $grid[0].id + '_toppager_left', { // "#list_toppager_left"
                caption: "",
                title: "Add Row",
                buttonicon: ' ui-icon-plusthick',
                onClickButton: function () {
                    doAddRowFunc();
                }
            });
            $grid.jqGrid('navButtonAdd', '#' + $grid[0].id + '_toppager_left', { // "#list_toppager_left"
                caption: "",
                title: "Append Row",
                buttonicon: 'ui-icon-arrowreturnthick-1-e',
                onClickButton: function () {
                    doAppendRowFunc()
                }
            });
        },
        ResetGridColmodel: function (postData, msgAlert) {
            var oldcolModel = $grid.jqGrid('getGridParam', 'colModel');
            var perm = [];
            var perm2 = [];
            for (var i = 0; i < oldcolModel.length; i++) {
                var isnew = true;
                if (oldcolModel[i].name === "rn" || oldcolModel[i].name === "cb") {
                    perm.push(i);
                    isnew = false;
                }
                for (var j = 0; j < postData.length; j++) {
                    if (typeof postData[j] == "undefined")
                        continue;
                    if (postData[j].name === oldcolModel[i].name) {
                        isnew = false;
                    }
                }
                if (isnew)
                    perm2.push(i);
            }
            for (var i = 0; i < postData.length; i++) {
                for (var j = 0; j < oldcolModel.length; j++) {
                    if (typeof oldcolModel[j] == "undefined")
                        continue;
                    if (postData[i].name === oldcolModel[j].name) {
                        if (perm.includes(j))
                            continue;
                        perm.push(j);
                        break;
                    }
                }
            }
            var perm3 = perm.concat(perm2);
            $grid.jqGrid('remapColumns', perm3, true);
            var colModel = $.extend({}, postData);
            $.each(colModel, function (key, value) {
                if (value.name != "rn" && typeof value.dt == "undefined" && value.hidden == false) {
                    $grid.jqGrid('showCol', value.name);
                }
                else {
                    $grid.jqGrid('hideCol', value.name);
                }
            });
            if (msgAlert) {
                setTimeout(function () {
                    DoSaveLayout($grid, "layout1");
                    alert("Reset Grid Layout Success!");
                }, 500);
            }
        },
        appendDelRowButton: function () {
            editable = true;
            //欄位選擇功能 上方
            $grid.jqGrid('navButtonAdd', '#' + $grid[0].id + '_toppager_left', { // "#list_toppager_left"
                caption: "",
                title: "Delete Row",
                buttonicon: 'ui-icon-closethick',
                onClickButton: function () {
                    doDelRowFunc();
                },
                position: "last"
            });
        },
        appendCopyRowButton: function () {

            //欄位選擇功能 上方
            $grid.jqGrid('navButtonAdd', '#' + $grid[0].id + '_toppager_left', { // "#list_toppager_left"
                caption: "",
                title: "Copy Row",
                buttonicon: 'ui-icon-copy',
                onClickButton: function () {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    //$SubGrid.jqGrid("addRowData", undefined, null, "last", selRowId);
                    if (selRowId) {
                        $grid.jqGrid('saveRow', selRowId, false, 'clientArray');
                        $grid.jqGrid('editCell', selRowId, 0, true);
                        //alert(selRowId);
                        if (gop.beforeCopyRowFunc != null) {
                            if (gop.beforeCopyRowFunc) {
                                gop.beforeCopyRowFunc(selRowId);
                            }
                        }
                        var row = $grid.jqGrid('getRowData', selRowId);
                        $grid.jqGrid("addRowData", undefined, row, "last", selRowId);
                        newRowId = $grid.jqGrid('getDataIDs')[$grid.jqGrid('getDataIDs').length - 1];
                        //alert(newRowId);
                        if (gop.afterCopyRowFunc != null) {
                            if (gop.afterCopyRowFunc) {
                                gop.afterCopyRowFunc(newRowId);
                            }
                        }

                    } else {
                        alert("Please selected a row!")
                    }

                }
            });

        },
        removeAddRowButton: function (gridId) {
            $("#" + gridId + "_toppager_left td[title='Add Row']").remove();
            $("#" + gridId + "_toppager_left td[title='Append Row']").remove();
            editable = false;
        },
        removeDelRowButton: function (gridId) {
            $("#" + gridId + "_toppager_left td[title='Delete Row']").remove();
            editable = false;
        },
        removeCopyRowButton: function (gridId) {
            $("#" + gridId + "_toppager_left td[title='Copy Row']").remove();
            editable = false;
        },
        onSelectCell: function () {
            //alert ("edited"); 
        },
        getGridCellValueCustom: function ($thisGrid, rowid, key, type) {
            //when get select type ,the type have to input "select",and lookup type input "lookup"
            if (type == "lookup")
                return (typeof $("#" + rowid + "_" + key + "Input").val() == "undefined") ? $thisGrid.jqGrid('getCell', rowid, key) : $("#" + rowid + "_" + key + "Input").val();
            else
                return (typeof $("#" + rowid + "_" + key).val() == "undefined") ? $thisGrid.jqGrid('getCell', rowid, key) : $("#" + rowid + "_" + key).val();
        },
        setGridCellValueCustom: function ($thisGrid, rowid, key, type, value) {
            //when get select type ,the type have to input "select",and lookup type input "lookup"
            if (type == "lookup") {
                var rn = rowid.replace("jqg", "");
                return (typeof $("#" + rowid + "_" + key + "Input").val() == "undefined") ? setGridChange({ "gridId": "#" + $thisGrid, "rowId": rowid, "cellKey": key, "cellValue": value, "selColIndex": selICol, "editable": editable }) : $("#" + rowid + "_" + key + "Input").val(value);
            } else {
                var rn = $("#" + $thisGrid).jqGrid('getCell', rowid, "rn");
                // var maxCount = $("#" + $thisGrid).jqGrid('getGridParam', 'records');
                if (rn == rowid || (typeof $("#" + rowid + "_" + key).val() != "undefined")) {
                    return (typeof $("#" + rowid + "_" + key).val() == "undefined") ? setGridChange({ "gridId": "#" + $thisGrid, "rowId": rowid, "cellKey": key, "cellValue": value, "selColIndex": selICol, "editable": editable }) : $("#" + rowid + "_" + key).val(value);
                } else {
                    return (typeof $("#" + rn + "_" + key).val() == "undefined") ? setGridChange({ "gridId": "#" + $thisGrid, "rowId": rowid, "cellKey": key, "cellValue": value, "selColIndex": selICol, "editable": editable }) : $("#" + rn + "_" + key).val(value);
                }
            }
        },
        getDelData: function () {
            return delRowKeys;
        },
        bindKeys: {
            onEnter: function () {
                //alert("123");
            },
            onSpace: null,
            onLeftKey: null, onRightKey: null,
            scrollingRows: true
        },
        arrangeGrid: function () {
            //$grid.jqGrid("editCell", 0, 0, true);
            var chkValue = true;
            var selRowId = $grid.jqGrid('getGridParam', 'selrow');

            $grid.jqGrid('saveRow', selRowId, false, 'clientArray')
            if (!$grid.jqGrid('getGridParam', "endEdit")()) {
                return false;
            }

            selIRow = "";

            var containerArray = $.extend(true, [], $grid.getChangedCells().slice());
            var tempArray = containerArray.slice();
            var delCount = 0;
            for (var i = 0; i < tempArray.length; i++) {
                //新增的id和rn是不相等的
                if (tempArray[i].id == tempArray[i].rn || tempArray[i].id.indexOf("jqg") < 0) {
                    //如果相等，说明是Update
                    containerArray[i - delCount]["__state"] = "2";
                    delete containerArray[i - delCount]["id"];
                    delete containerArray[i - delCount]["rn"];
                }
                if (tempArray[i].id != null && tempArray[i].id.indexOf("jqg") >= 0) {

                    delete containerArray.splice(i - delCount++, 1);

                }
            }
            var allData = $grid.jqGrid("getGridParam", "data");
            $.each(allData, function (i, val) {

                if ((val._id_ != null && val._id_.indexOf("jqg") >= 0) || (val.id != null && val.id.indexOf("jqg") >= 0)) {
                    if (typeof gop.chkRequire !== "undefined") {
                        if (gop.chkRequire(i, val) == false) {
                            chkValue = false;
                        }
                        else {
                            //如果不等，说明是insert的数据
                            val["__state"] = "1";
                            delete val["_id_"];
                            delete val["id"];
                            delete val["rn"];
                            containerArray.push(val);
                        }
                    }
                    else {
                        //如果不等，说明是insert的数据
                        val["__state"] = "1";
                        delete val["_id_"];
                        delete val["id"];
                        delete val["rn"];
                        containerArray.push(val);
                    }

                }
            });

            if ($.isArray(gop.delKey)) {
                var delArr = delRowKeys;
                for (var i = 0; i < delArr.length; i++) {
                    var val = {};
                    if (delArr[i] != null && delArr[i] != "") {

                        for (var k = 0; k < gop.delKey.length; k++) {
                            val[gop.delKey[k]] = delArr[i][gop.delKey[k]];
                        }

                        val["__state"] = "0";
                        containerArray.push(val);
                    }

                }
                delRowKeys = [];
            } else {
                var delArr = delRowKeys.split('|');
                for (var i = 0; i < delArr.length; i++) {
                    var val = {};
                    if (delArr[i] != null && delArr[i] != "") {

                        val[gop.delKey] = delArr[i];
                        val["__state"] = "0";
                        containerArray.push(val);
                    }

                }
                delRowKeys = "";
            }



            if (chkValue == false) {
                return false;
            }
            editable = false;

            return containerArray;
        },
        loadCompleteFunc: function () {
        }

    }
    if (!gop.rows) {
        delete grid_options.rowNum;
        delete grid_options.rowList;
    }
    if (gop.loadComplete) {
        functions.loadComplete = gop.loadComplete;
        delete gop.loadComplete;
    }
    $.extend(grid_options, gop || {});


    grid_options.data = gop.data;

    if (!grid_options.colModel) {
        grid_options.colModel = gop.colModel;
    }

    //处理gop里的colModel产生出colNames 
    var __colModel = grid_options.colModel;
    var __gridNames = [];
    $.each(__colModel, function (index, item) {
        if (item != null) {
            __gridNames.push(item.title);
        } else {
            //GET LAYOUT第一個欄位會為NULL 必須要去除
            __colModel.splice(0, 1);;
        }
    });
    grid_options.colNames = __gridNames;
    $grid.jqGrid(grid_options);

    if (gop.refresh) {
        //凍結欄位設定 colModel Frozen:true
        //$grid.jqGrid("setFrozenColumns");
        //下方MENU BAR      
        $grid.jqGrid('navGrid', '#gridPage', {
            cloneToTop: true, add: false, edit: false, del: false, search: false, refresh: false,
            refreshtext: ""
        });
    }
    //上方 定義inline edit的功能 PS：如果欄位太小就不要給text只顯示圖片就好
    $grid.jqGrid('inlineNav', '#list_toppager', {
        edit: false,
        save: false,
        cancel: false,
        addtext: "Add",
        restoreAfterSelect: false,
        addParams: { position: "last" }
    });

    //下方 定義inline edit的功能 PS：如果欄位太小就不要給text只顯示圖片就好
    $grid.jqGrid('inlineNav', '#gridPage', {
        edit: false,
        add: false,
        del: false,
        save: false,
        cancel: false,
        restoreAfterSelect: false,
        addtext: "Add",
        addParams: { position: "last" }
    });
    //欄位過濾工具是否啟用 PS：注意，不得與setFrozenColumns共用
    //$grid.jqGrid('filterToolbar',{searchOperators : true});
    if (gop.addRow) {
        //欄位選擇功能 上方
        $grid.jqGrid('navButtonAdd', '#' + $grid[0].id + '_toppager_left', { // "#list_toppager_left"
            caption: "Add",
            buttonicon: 'ui-icon-plus',
            onClickButton: function () {
                if (gop.beforeAddRowFunc != null) {
                    if (gop.beforeAddRowFunc) {
                        //遍历selectRowsData
                        if (gop.beforeAddRowFunc() == false) {
                            $grid.trigger("reloadGrid");
                            $grid.jqGrid("clearGridData");
                            return false;
                        }
                    }
                }
                $grid.jqGrid("addRow", addSettings);
            }
        });
        $grid.jqGrid('navButtonAdd', '#' + $grid[0].id + '_toppager_left', { // "#list_toppager_left"
            caption: "Delete",
            buttonicon: 'ui-icon-close',
            onClickButton: function () {
                if (gop.beforeAddRowFunc != null) {
                    if (gop.beforeAddRowFunc) {
                        //遍历selectRowsData
                        if (gop.beforeAddRowFunc() == false) {
                            $grid.trigger("reloadGrid");
                            $grid.jqGrid("clearGridData");
                            return false;
                        }
                    }
                }
                $grid.jqGrid("addRow", addSettings);
            }
        });
    }

    if (gop.showcolumns) {
        //欄位選擇功能 上方
        $grid.jqGrid('navButtonAdd', '#' + $grid[0].id + '_toppager_left', { // "#list_toppager_left"
            caption: "Column Control",
            buttonicon: 'ui-icon-wrench',
            onClickButton: function () {
                $grid.jqGrid('columnChooser', {
                    done: function (perm) {
                        if (!perm) { return false; }
                        //console.log(perm);
                        this.jqGrid('remapColumns', perm, true);
                        var $t = $grid;
                        var windowwidth = this.parents('div.ui-jqgrid-bdiv').width();
                        $t.jqGrid('setGridWidth', windowwidth);
                    }
                });

                var altura = $(window).height() - 100;
                var modalWidth = $(window).width() - 100;

                if (modalWidth >= 470 && altura >= 370) {
                    $("div[aria-describedby='colchooser_" + $grid[0].id + "']").css({ "height": altura });
                    $("div[aria-describedby='colchooser_" + $grid[0].id + "']").css({ "width": modalWidth });
                    $("div[aria-describedby='colchooser_" + $grid[0].id + "']").css("top", ($(window).height() - altura) / 2 + $(window).scrollTop() + "px");
                    $("div[aria-describedby='colchooser_" + $grid[0].id + "']").css("left", ($(window).width() - modalWidth) / 2 + $(window).scrollLeft() + "px");
                    $("#colchooser_" + $grid[0].id + ">div").css('margin', '');
                    $('#colchooser_' + $grid[0].id + '>div').css('height', altura - 150);
                    $('#colchooser_' + $grid[0].id + '>div').css('width', modalWidth - 50);
                    $('#colchooser_' + $grid[0].id + '>div>div').css('width', modalWidth - 50);
                    $('#colchooser_' + $grid[0].id + '>div>div>div[class="selected"]').css('width', (modalWidth - 50) * 0.6);
                    $('#colchooser_' + $grid[0].id + '>div>div>div[class="available"]').css('width', modalWidth - (modalWidth - 50) * 0.6 - 53);
                    $('#colchooser_' + $grid[0].id + '>div>div>div[class="selected"]>ul').css('height', altura - 180);
                    $('#colchooser_' + $grid[0].id + '>div>div>div[class="available"]>ul').css('height', altura - 180);
                    $("div[aria-describedby='colchooser_" + $grid[0].id + "']>div[class*='ui-dialog-titlebar']>button[class*='ui-dialog-titlebar-diag']>span").removeClass("ui-icon-arrow-4-diag");
                    $("div[aria-describedby='colchooser_" + $grid[0].id + "']>div[class*='ui-dialog-titlebar']>button[class*='ui-dialog-titlebar-diag']>span").addClass("ui-icon-arrow-4");
                }
                var spanid = $("#colchooser_" + $grid[0].id).parent().attr("aria-labelledby");
                $("#" + spanid).dblclick(function () {
                    var height = $("#colchooser_" + $grid[0].id + ">div").height();
                    if (modalWidth < 470 || altura < 370)
                        return;
                    if (height == 240) {
                        $("div[aria-describedby='colchooser_" + $grid[0].id + "']").css({ "height": altura });
                        $("div[aria-describedby='colchooser_" + $grid[0].id + "']").css({ "width": modalWidth });
                        $("div[aria-describedby='colchooser_" + $grid[0].id + "']").css("top", ($(window).height() - altura) / 2 + $(window).scrollTop() + "px");
                        $("div[aria-describedby='colchooser_" + $grid[0].id + "']").css("left", ($(window).width() - modalWidth) / 2 + $(window).scrollLeft() + "px");
                        $("#colchooser_" + $grid[0].id + ">div").css('margin', '');
                        $('#colchooser_' + $grid[0].id + '>div').css('height', altura - 150);
                        $('#colchooser_' + $grid[0].id + '>div').css('width', modalWidth - 50);
                        $('#colchooser_' + $grid[0].id + '>div>div').css('width', modalWidth - 50);
                        $('#colchooser_' + $grid[0].id + '>div>div>div[class="selected"]').css('width', (modalWidth - 50) * 0.6);
                        $('#colchooser_' + $grid[0].id + '>div>div>div[class="available"]').css('width', modalWidth - (modalWidth - 50) * 0.6 - 53);
                        $('#colchooser_' + $grid[0].id + '>div>div>div[class="selected"]>ul').css('height', altura - 180);
                        $('#colchooser_' + $grid[0].id + '>div>div>div[class="available"]>ul').css('height', altura - 180);
                        $("div[aria-describedby='colchooser_" + $grid[0].id + "']>div[class*='ui-dialog-titlebar']>button[class*='ui-dialog-titlebar-diag']>span").removeClass("ui-icon-arrow-4-diag");
                        $("div[aria-describedby='colchooser_" + $grid[0].id + "']>div[class*='ui-dialog-titlebar']>button[class*='ui-dialog-titlebar-diag']>span").addClass("ui-icon-arrow-4");
                    } else {
                        $("div[aria-describedby='colchooser_" + $grid[0].id + "']").css({ "height": "auto" });
                        $("div[aria-describedby='colchooser_" + $grid[0].id + "']").css({ "width": "470px" });
                        $("div[aria-describedby='colchooser_" + $grid[0].id + "']").css("top", ($(window).height() - 370) / 2 + $(window).scrollTop() + "px");
                        $("div[aria-describedby='colchooser_" + $grid[0].id + "']").css("left", ($(window).width() - 470) / 2 + $(window).scrollLeft() + "px");
                        $('#colchooser_' + $grid[0].id + '>div').css('height', "240px");
                        $('#colchooser_' + $grid[0].id + '>div').css('width', "420px");
                        $('#colchooser_' + $grid[0].id + '>div>div').css('width', "421px");
                        $('#colchooser_' + $grid[0].id + '>div>div>div[class="selected"]').css('width', "251px");
                        $('#colchooser_' + $grid[0].id + '>div>div>div[class="available"]').css('width', "167px");
                        $('#colchooser_' + $grid[0].id + '>div>div>div[class="selected"]>ul').css('height', "198.4px");
                        $('#colchooser_' + $grid[0].id + '>div>div>div[class="available"]>ul').css('height', "198.4px");
                        $("div[aria-describedby='colchooser_" + $grid[0].id + "']>div[class*='ui-dialog-titlebar']>button[class*='ui-dialog-titlebar-diag']>span").removeClass("ui-icon-arrow-4");
                        $("div[aria-describedby='colchooser_" + $grid[0].id + "']>div[class*='ui-dialog-titlebar']>button[class*='ui-dialog-titlebar-diag']>span").addClass("ui-icon-arrow-4-diag");
                    }
                });
            }
        });
    }

    if (gop.exportexcel) {
        //欄位選擇功能 上方
        $grid.jqGrid('navButtonAdd', '#' + $grid[0].id + '_toppager_left', {
            caption: "Export to Excel",
            buttonicon: "ui-icon-print",
            onClickButton: function () {
                //console.log($grid.jqGrid("getGridParam", "data"));
                ExportDataToExcel($grid);
            },
            position: "last"
        });
    }
    if (gop.savelayout) {
        //欄位選擇功能 上方
        $grid.jqGrid('navButtonAdd', '#' + $grid[0].id + '_toppager_left', {
            caption: "Set Layout",
            buttonicon: "ui-icon-clipboard",
            onClickButton: function () {
                $.ajax({
                    async: false,
                    url: rootPath + "Common/GetLayoutName",
                    type: 'POST',
                    data: { "layoutid": gop.caption, "layouttype": "GRID" },
                    dataType: "json",
                    "complete": function (xmlHttpRequest, successMsg) {
                    },
                    "error": function (xmlHttpRequest, errMsg) {
                    },
                    "success": function (result) {
                        if (result.LAYOUT_ID_NAME != null) {
                            var _options = [];
                            var layoutlist = result.LAYOUT_ID_NAME;
                            if (layoutlist !== null && layoutlist != undefined && layoutlist != "") {
                                var layoutArr = layoutlist.split(';');
                                for (var i = 0; i < layoutArr.length; i++) {
                                    var val = {};
                                    if (layoutArr[i] != null && layoutArr[i] != "") {
                                        _options.push(layoutArr[i])
                                    }
                                }
                                if (result.LAYOUT_DEFAULT_NAME != null && result.LAYOUT_DEFAULT_NAME != "") {
                                    $("#savelayoutId_" + $grid[0].id).val(result.LAYOUT_DEFAULT_NAME);
                                }
                                $("#sel_layoutId_" + $grid[0].id).find("option").remove();
                                $("#sel_layoutId_" + $grid[0].id).append("<option value=''></option>");
                                for (var i = 0; i < _options.length; i++) {
                                    $("#sel_layoutId_" + $grid[0].id).append("<option value='" + _options[i] + "'>" + _options[i] + "</option>");
                                }
                                //$("#sel_layoutId_" + $grid[0].id + " option[value=" + result.LAYOUT_DEFAULT_NAME + "]").attr("selected", "selected");
                            } else {
                                $("#savelayoutId_" + $grid[0].id).val(result.LAYOUT_DEFAULT_NAME);
                                $("#sel_layoutId_" + $grid[0].id).find("option").remove();
                            }
                            $("#LayoutWindow_" + $grid[0].id).modal("show");
                        }
                    }
                });
            },
            position: "last"
        });
        //欄位選擇功能 上方
        $grid.jqGrid('navButtonAdd', '#' + $grid[0].id + '_toppager_left', {
            caption: "Reset Layout",
            buttonicon: "ui-icon-arrowreturnthick-1-w",
            onClickButton: function () {
                if (confirm("Are you sure reset this layout?")) {
                    $grid.jqGrid('getGridParam', "ResetGridColmodel")(gop.baseColModel, true);
                    $("#sel_layoutId_" + $grid[0].id).find("option:selected").remove();
                    //alert("Reset Grid Layout Success!");
                }
            },
            position: "last"
        });
    }

    $grid.keydown(function (e) {

        var selectedRow = $grid.getGridParam('selrow');
        if (selectedRow == null) return;

        var ids = $grid.getDataIDs();
        var index = $grid.getInd(selectedRow);
        if (e.keyCode === $.ui.keyCode.UP) { // 38
            index--;
            if (index > ids.length)
                index = 1;
            $grid.setSelection(ids[index - 1], true);
        }
        if (e.keyCode === $.ui.keyCode.DOWN) { // 40
            index++;
            if (index > ids.length)
                index = 1;

            $grid.setSelection(ids[index - 1], true);
        }
        if (e.ctrlKey && e.keyCode === 13) {//ctrl+enter
            doAddRowFunc();
        }
    });

    $grid.jqGrid("setLabel", "rn", "Seq.");
    return $grid;
}

