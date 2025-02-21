function initLookUp(options) {
    //registerBtn, searchColumns, gridUrl, gridColModel, httpUrlModle, gridNames, multiselect, gop, gridFunc, responseMethod
    var groupId = getCookie("plv3.passport.groupid"),
    cmp = getCookie("plv3.passport.companyid"),
    stn = getCookie("plv3.passport.station"),
    userId = getCookie("plv3.passport.user");

    if (typeof options.lookUpConfig == "undefined") {
        return;
    }

    var searchColumns = options.lookUpConfig.columns;
    var registerBtn = options.registerBtn;

    var gop = {};
    gop.caption = options.lookUpConfig.caption;
    gop.sortname = options.lookUpConfig.sortname;
    gop.sortorder = options.lookUpConfig.sortorder;
    gop.refresh = options.lookUpConfig.refresh;
    gop.multiSort = options.lookUpConfig.multiSort;
    gop.multlSelectFunc = options.lookUpConfig.multlSelectFunc;
    gop.loadCompleteFunc = options.lookUpConfig.loadCompleteFunc;
    gop.columnID = options.columnID;
    //bind afterDelRowFunc & afterAddRowFunc
    gop.cellCalFunc = options.lookUpConfig.cellCalFunc;
    gop.afterSaveCellWithIdFunc = options.lookUpConfig.afterSaveCellWithIdFunc;
    gop.openclick = options.openclick;
    //gop.editable = options.lookUpConfig.editable;

    if (typeof options.lookUpConfig.editable != "undefined") {
        gop.editable = options.lookUpConfig.editable;
    }
    else {
        gop.editable = false;
    }

    gop.rows = options.lookUpConfig.rows;

    if (typeof options.lookUpConfig.multiselect != "undefined") {
        gop.multiselect = options.lookUpConfig.multiselect;
    }
    if (typeof options.lookUpConfig.footerrow != "undefined") {
        gop.footerrow = options.lookUpConfig.footerrow;
    }



    //var gridColModel = [],
    //    gridNames = [];
    //$.each(searchColumns, function (index, item) {
    //    var gridColumns = {};
    //    gridColumns.name = item.fieldName;
    //    gridColumns.index = item.fieldName;
    //    gridColumns.width = item.width;
    //    gridColumns.sorttype = item.sorttype;
    //    gridColumns.hidden = item.hidden;
    //    gridColumns.viewable = item.viewable;
    //    gridColModel.push(gridColumns);
    //    gridNames.push(item.title);
    //});
    gop.data = [];
    //gop.colModel = gridColModel;
    gop.colModel = searchColumns;

    //gop.colNames = gridNames;
    //condiArray这里所有对应的操作和Server端是一致的，请对比ModelFoctory.cs中关于查询时生成条件关于switch那一段判断逻辑959行
    var condiArray = { 'eq': "equal", 'ne': "not equal", 'lt': "less than", 'le': "less equal than", 'gt': "greater than", 'ge': "greater equal than", 'nu': "is null", 'nn': "is not null", 'in': "is in", 'ni': "is not in", 'cn': "like", 'nc': "not like", 'bw': "begin with", 'bn': "not begin with", 'ew': "end with", 'en': "not end with", 'bt': "between" };
    var mygrid = null;
    var myId = typeof registerBtn.attr("id") == "undefined" ? options.customId + "_" + options.gridId : registerBtn.attr("id") + "_" + options.gridId;
    //console.log(myId);
    var DocumentPosition = 0;
    registerBtn.attr("data-target", "lookupDialog_" + myId);

    if ($("#lookupDialog_" + myId).html() == null) {

        var temp = '<div class="modal fade" id="lookupDialog_{initID}">\
                            <div class="modal-dialog modal-lg">\
                                <div class="modal-content">\
                                    <div class="modal-header">\
                                        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
                                        <h4 class="modal-title">Search</h4>\
                                    </div>\
                                    <div class="modal-body">\
                                        <form id="searchForm_{initID}">\
                                        <div id="searchFilterDiv_{initID}">\
                                        </div>\
                                        </form>\
                                        <div class="pure-g" id="searchDiv_{initID}">\
                                            <div class="pure-u-sm-3-24">\
                                                <label class="control-label">Search Item</label>\
                                            </div>\
                                            <div class="pure-u-sm-4-24">\
                                                <select id="addFilterSel_{initID}" class="form-control input-sm"></select>\
                                            </div>\
                                            <div class="pure-u-sm-4-24">\
                                                <button type="button" id="addFilterBtn_{initID}" class="btn btn-sm btn-info">\
                                                    <i class="glyphicon glyphicon-plus"></i>\
                                                </button>\
                                                <button type="button" class="btn btn-sm btn-success" id="searchFilterBtn_{initID}">Search</button>\
                                            </div>\
                                        </div>\
                                        <div class="pure-g" style="margin-top: 10px;">\
                                            <div class="pure-u-sm-24-24">\
                                                <div id="lookupGridPage_{initID}"></div>\
                                                <button type="button" class="btn btn-sm btn-primary" id="mutiSelBtn_{initID}" style="margin-bottom: 5px; display:none">Select</button>\
                                                <table id="lookupGrid_{initID}"><tr><td /></tr></table>\
                                            </div>\
                                        </div>\
                                    </div>\
                                </div>\
                            </div>\
                        </div>';


        temp = temp.replace(/{initID}/g, myId);
        $('body').append(temp);
    }
    var realHeight = parent.document.body.clientHeight;
    if (realHeight <= 0) {
        realHeight = $(window).height();
    }
    gop.height = realHeight - 340;
    gop.rowList = [20, 50, 100];

    if (gop.multiselect == true) {
        $("#mutiSelBtn_" + myId).css("display", "block");
    } else {
        gop.multiselect = false;
        $("#mutiSelBtn_" + myId).css("display", "none");
    }


    registerBtn.off().on("click", function () {
        //alert(myId);
        var isInit = $("#lookupStatus_" + myId).attr("isInit");
        if (options.onClickRegiBtnFunc != null) {
            if (!options.onClickRegiBtnFunc()) {
                return false;
            }
        }
        if (isInit != "false") {

            //查詢條件
            for (var key in searchColumns) {
                //console.log(key);
                if (searchColumns.hasOwnProperty(key)) {


                    if (searchColumns[key].init != true) {
                        $('#addFilterSel_' + myId).append("<option value='" + searchColumns[key].name + "'>" + searchColumns[key].title + "</option>");
                    } else {

                        /*if (condiTemp != null) {
                            break;
                        }*/


                        var thisSopt = [];
                        var ItemID = (new Date).getTime();
                        for (var soptKey in searchColumns[key].sopt) {
                            thisSopt[soptKey] = { sopt: searchColumns[key].sopt[soptKey], showsopt: condiArray[searchColumns[key].sopt[soptKey]] };
                        }

                        var dt = "";
                        var dv = "";
                        //alert(typeof searchColumns[key].dt);
                        if (typeof searchColumns[key].dt != "undefined") {
                            dt = searchColumns[key].dt;
                        }
                        if (typeof searchColumns[key].dv != "undefined") {
                            dv = searchColumns[key].dv;
                        }


                        var data, source, template, html;
                        //下拉选择
                        if (searchColumns[key].formatter === "select") {
                            var selectOptions = [],
                                optiontemp,
                                selectstr = searchColumns[key].editoptions.value,
                                ary = selectstr.split(";");
                            for (var i = 0; i < ary.length; i++) {
                                optiontemp = ary[i].split(":");
                                if (optiontemp.length < 2)
                                    continue;
                                selectOptions.push({ val: optiontemp[0], text: optiontemp[1], dV: dv });
                            }
                            source = $("#ConditionSelectTemplate").html();

                        } else {
                            source = $("#ConditionLookupTemplate").html();
                        }

                        data = { fieldName: searchColumns[key].name, label: searchColumns[key].title, ItemID: ItemID, sopt: thisSopt, sortType: searchColumns[key].sorttype, dT: dt, selectoptions: selectOptions, dV: dv };
                        template = Handlebars.compile(source);
                        html = template(data);

                        $("#" + "searchFilterDiv_" + myId).append(html);

                        if (searchColumns[key].sorttype == "date" || searchColumns[key].sorttype == "datetime") {
                            var data = { ItemID: ItemID, fieldName: searchColumns[key].name, sortType: searchColumns[key].sorttype, dV: dv };
                            var source = $("#BetweenTemplate").html();
                            var template = Handlebars.compile(source);
                            var html = template(data);

                            $(".tmp[ItemID='" + ItemID + "']").remove();
                            $("[tempId='temp_" + ItemID + "']:last-child").append(html);
                            //alert("#sel_" + searchColumns[key].name + "S");
                            //setTimeout(function () {
                            //alert("start");

                            if (searchColumns[key].sorttype == "date") {
                                $("#sel_" + searchColumns[key].name + "S").datepicker({
                                    dateFormat: 'yy/mm/dd',
                                    beforeShow: function () {
                                        //alert("111");
                                        setTimeout(function () {
                                            $('.ui-datepicker').css('z-index', 99999999999999);
                                        }, 0);
                                    }
                                });

                                $("#sel_" + searchColumns[key].name + "E").datepicker({
                                    dateFormat: 'yy/mm/dd',
                                    beforeShow: function () {
                                        setTimeout(function () {
                                            $('.ui-datepicker').css('z-index', 99999999999999);
                                        }, 0);
                                    }
                                });
                            }

                            if (searchColumns[key].sorttype == "datetime") {
                                $("#sel_" + searchColumns[key].name + "S").datetimepicker({
                                    dateFormat: 'yy/mm/dd',
                                    beforeShow: function () {
                                        //alert("111");
                                        setTimeout(function () {
                                            $('.ui-datepicker').css('z-index', 99999999999999);
                                        }, 0);
                                    }
                                });

                                $("#sel_" + searchColumns[key].name + "E").datetimepicker({
                                    dateFormat: 'yy/mm/dd',
                                    beforeShow: function () {
                                        setTimeout(function () {
                                            $('.ui-datepicker').css('z-index', 99999999999999);
                                        }, 0);
                                    }
                                });
                            }

                            $('#sopt_' + searchColumns[key].name).val("bt");
                        }

                        if (searchColumns[key].sorttype == "string") {
                            $('#sopt_' + searchColumns[key].name).val("cn");
                        }

                    }
                }
            }

            $('body').append('<div id="' + "lookupStatus_" + myId + '"></div>');
            $("#lookupStatus_" + myId).attr("isInit", "false");
            //alert("123");
            //初始化GRID
            //塞Url與Model
            $.extend(gop, {
                ondblClickRow: function (rowid, iRow, iCol, e) {
                    //用于回调函数，例如赋值操作等
                    //alert("123");
                    var jsonMap = $(this).jqGrid("getRowData", iRow);
                    if (options.gridFunc != null) {
                        //alert("123");
                        //遍历selectRowsData
                        options.gridFunc(jsonMap);
                        //$("#lookupDialog_" + myId).dialog("close");
                        $("#lookupDialog_" + myId).modal("hide");
                    }
                }
            });
            if (gop.multiselect) {
                //muti select loadcomplete event
                gop.setSelectedFunc = function () {

                    if (typeof options.focusItem != "undefined") {
                        var allData = $("#lookupGrid_" + myId).jqGrid("getRowData");
                        var selData = $("#lookupGrid_" + myId).jqGrid('getGridParam', 'selarrrow');
                        var selectData = options.focusItem.val();
                        var checkId = options.columnID;
                        var rowid = 1;
                        var selectDataList = selectData.split(';');
                        $.each(allData, function (i, val) {
                            $.each(selectDataList, function (index, invalue) {
                                if (invalue == val[checkId] && $.inArray(rowid + "", selData) == -1) {
                                    $("#lookupGrid_" + myId).jqGrid('setSelection', rowid, true);
                                }
                            });
                            rowid++;
                        });
                    }
                };

            }


            mygrid =
            genGrid($("#lookupGrid_" + myId), //{ data: [], colModel: gridColModel, colNames: gridNames },
                gop, "lookupGridPage_" + myId);

            if (gop.openclick)
                $("#searchFilterBtn_" + myId).trigger("click");

            if (gop.multiselect) {
                //muti select button event
                $("#mutiSelBtn_" + myId).click(function () {
                    var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
                    var responseData = [];
                    var colId = gop.columnID;
                    var tempColIdData = [];

                    $.each(selRowId, function (index, val) {
                        responseData.push(mygrid.getRowData(selRowId[index]));
                        if (typeof colId != "undefined") {
                            tempColIdData.push(mygrid.getRowData(selRowId[index])[colId])
                        }
                    });

                    if (typeof colId != "undefined") {
                        $.each(idsOfSelectedRows["lookupGrid_" + myId], function (index, val) {
                            var obj = {};
                            obj[colId] = idsOfSelectedRows["lookupGrid_" + myId][index];
                            if ($.inArray(val, tempColIdData) == -1) {
                                responseData.push(obj);
                            }
                        });
                    }

                  
                    options.responseMethod(responseData);
                    //$( "#lookupDialog_"+myId ).dialog( "close" );
                    $("#lookupDialog_" + myId).modal("hide");

                });

            }
            /*if (typeof gop.editable != "undefined") {
                gridEditableCtrl({ editable: gop.editable, gridId: "lookupGrid_" + myId });
            }*/
        }

        //$( "#lookupDialog_"+myId ).dialog( "open" );
        $("#lookupDialog_" + myId).modal({
            backdrop: 'static',
            keyboard: false
        });
        if (gop.openclick && "false" == isInit) {
            $("#lookupGrid_" + myId).jqGrid("clearGridData");
            $("#searchFilterBtn_" + myId).trigger("click");
        }
    });

    $("#" + "searchFilterDiv_" + myId).on("click", ".RemoveSearch", function () {
        var ItemID = $(this).attr("ItemID");

        var selname = $("input[ItemID = '" + ItemID + "']").attr("oname");
        if (typeof selname == "undefined" || selname.indexOf("sopt_") !== -1)//选择项
            selname = $("select[ItemID = '" + ItemID + "']").attr("oname");

        $(this).parents("[ItemID='" + ItemID + "']").remove();
        //console.log(searchColumns);
        //console.log(selname);
        for (var key in searchColumns) {
            //alert(searchColumns.hasOwnProperty(key));
            //alert(searchColumns.hasOwnProperty(key));
            if (searchColumns.hasOwnProperty(key) && selname == searchColumns[key].name) {
                //alert(searchColumns[key].name);
                $('#addFilterSel_' + myId).append("<option value='" + searchColumns[key].name + "'>" + searchColumns[key].title + "</option>");
                searchColumns[key].init = false;
                break;
            }

        }
    });

    $("#searchForm_"+myId).on("change", "select", function () {
        if (typeof ($(this).attr("sletype")) == "undefined")
            return;
        var ItemID = $(this).attr('ItemID');
        var fieldName = $("input[ItemID = '" + ItemID + "']").attr("oname");
        var sortType = $("input[ItemID = '" + ItemID + "']").attr("sortType");
        var defVal = $("input[ItemID = '" + ItemID + "']").val();

        console.log(sortType);

        if ($(this).val() == "bt") {
            var data = { ItemID: ItemID, fieldName: fieldName, sortType: sortType };
            var source = $("#BetweenTemplate").html();
            var template = Handlebars.compile(source);
            var html = template(data);

            $(this).parent().siblings("[ItemID='" + ItemID + "']").remove();
            $(this).parent().after(html);

            if (sortType == "date") {

                $("#sel_" + fieldName + "S").datepicker({
                    dateFormat: 'yy/mm/dd',
                    beforeShow: function () {
                        setTimeout(function () {
                            //alert("111");
                            $('.ui-datepicker').css('z-index', 99999999999999);
                        }, 0);
                    }
                });
                $("#sel_" + fieldName + "E").datepicker({
                    dateFormat: 'yy/mm/dd',
                    beforeShow: function () {
                        setTimeout(function () {
                            $('.ui-datepicker').css('z-index', 99999999999999);
                        }, 0);
                    }
                });
            }
            if (sortType == "datetime") {

                $("#sel_" + fieldName + "S").datetimepicker({
                    dateFormat: 'yy/mm/dd',
                    beforeShow: function () {
                        setTimeout(function () {
                            //alert("111");
                            $('.ui-datepicker').css('z-index', 99999999999999);
                        }, 0);
                    }
                });
                $("#sel_" + fieldName + "E").datetimepicker({
                    dateFormat: 'yy/mm/dd',
                    beforeShow: function () {
                        setTimeout(function () {
                            $('.ui-datepicker').css('z-index', 99999999999999);
                        }, 0);
                    }
                });
            }
        }
        else {
            var data = { ItemID: ItemID, fieldName: fieldName, sortType: sortType, DefVal: defVal };
            var source = $("#CommonTemplate").html();
            var template = Handlebars.compile(source);
            var html = template(data);

            $(this).parent().siblings("[ItemID='" + ItemID + "']").remove();
            $(this).parent().after(html);

            if (sortType == "date") {
                $("#sel_" + fieldName).datepicker({
                    dateFormat: 'yy/mm/dd',
                    beforeShow: function () {
                        setTimeout(function () {
                            $('.ui-datepicker').css('z-index', 99999999999999);
                        }, 0);
                    }
                });
            }
            if (sortType == "datetime") {
                $("#sel_" + fieldName).datetimepicker({
                    dateFormat: 'yy/mm/dd',
                    beforeShow: function () {
                        setTimeout(function () {
                            $('.ui-datepicker').css('z-index', 99999999999999);
                        }, 0);
                    }
                });
            }
        }


    });

    $("#addFilterBtn_" + myId).unbind('click').click(function () {

        var selname = $("#addFilterSel_" + myId).val();
        $("#addFilterSel_" + myId).find(":selected").remove();


        for (var key in searchColumns) {
            if (searchColumns.hasOwnProperty(key) && selname == searchColumns[key].name) {
                var ItemID = (new Date).getTime();
                var sopt = "<div class='pure-u-sm-8-60'><select sletype='sopt' ItemID='" + ItemID + "' name='sopt_" + searchColumns[key].name + "' class='form-control input-sm'>";
                var isDefault = false;
                if (typeof searchColumns[key].sopt == 'undefined') {
                    searchColumns[key].sopt = condiArray;
                    isDefault = true;
                }
                var remark = "";
                if (typeof searchColumns[key].remark != 'undefined')
                    remark = "<div style='text-align:center;color:red'><label class='control-label'>" + searchColumns[key].remark + "</label></div>";

                for (var opt_key in searchColumns[key].sopt) {

                    if (isDefault) {
                        sopt += "<option value='" + opt_key + "'>" + condiArray[opt_key] + "</option>";
                    } else {
                        if (opt_key != 'contains' && opt_key != 'duplicated')
                            sopt += "<option value='" + searchColumns[key].sopt[opt_key] + "'>" + condiArray[searchColumns[key].sopt[opt_key]] + "</option>";
                    }
                }
                sopt += "</select></div>";

                var inputText = '<div class="input-group" ItemID="' + ItemID + '" ><input sorttype="' + searchColumns[key].sorttype + '" oname="' + searchColumns[key].name + '" ItemID="' + ItemID + '" type="text" id="sel_' + searchColumns[key].name + '" class="form-control input-sm" name="' + searchColumns[key].name + '" searchItem="' + searchColumns[key].name + '"/><span class="input-group-btn"><button class="btn btn-sm btn-default btn-danger" type="button" id="delFilterBtn_' + searchColumns[key].name + '" data="' + searchColumns[key].name + '"><span class="glyphicon glyphicon-remove"></span></button></span></div>';


                if (typeof searchColumns[key].dt != "undefined") {
                    var dt = searchColumns[key].dt;
                    //alert(dt);
                    inputText += '<input sorttype="' + searchColumns[key].sorttype + '" oname="'+ searchColumns[key].name +'" ItemID="' + ItemID + '" type="hidden" class="form-control input-sm" name="dt_' + searchColumns[key].name + '" id="dt_' + searchColumns[key].name + '"  value="' + dt + '">';
                }
                $('#searchFilterDiv_' + myId).append("<div id='filterRow_" + searchColumns[key].name + "' ItemID='" + ItemID + "' class='pure-g'><div class='pure-u-sm-8-60'><label for='" + searchColumns[key].name + "' class='control-label'>" + searchColumns[key].title + "</label></div>" + sopt + "<div class='pure-u-sm-25-60' ItemID='" + ItemID + "' >" + inputText + "</div>" + remark + "</div>");

                //option default to string
                $("select[name='sopt_" + searchColumns[key].name + "'] option[value='eq']").attr("selected", "selected");


                //若型態為Date就加入DatePicker
                if (searchColumns[key].sorttype == "date") {
                    $("#sel_" + searchColumns[key].name).datepicker({
                        dateFormat: 'yy/mm/dd',
                        beforeShow: function() {
                            setTimeout(function(){
                                $('.ui-datepicker').css('z-index', 99999999999999);
                            }, 0);
                        }
                    });
                }
                if (searchColumns[key].sorttype == "datetime") {
                    $("#sel_" + searchColumns[key].name).datetimepicker();
                }

                $("#searchFilterDiv_" + myId + " #delFilterBtn_" + searchColumns[key].name).click(function () {

                    var selname = $(this).attr("data");
                    $("#searchFilterDiv_" + myId + " #filterRow_" + selname).remove();


                    for (var key in searchColumns) {
                        console.log(key);
                        if (searchColumns.hasOwnProperty(key) && selname == searchColumns[key].name) {

                            $('#addFilterSel_' + myId).append("<option value='" + searchColumns[key].name + "'>" + searchColumns[key].title + "</option>");
                        }
                    }
                });
                setTimeout(function () { $("input[searchItem='" + searchColumns[key].name + "']").focus(); }, 200);
            }
        }
    });

    $("#searchFilterBtn_" + myId).unbind('click').click(function () {


        $.each($('#searchForm_' + myId + " input "), function (key, value) {
            $(value).val($(value).val().replace('&', '(@nd)'));
        });
        var param = $('#searchForm_' + myId).serialize();

        $.each($('#searchForm_' + myId + " input "), function (key, value) {
            $(value).val($(value).val().replace('(@nd)', '&'));
        });

        //若LOOKUP是多表關聯的狀況下，會發生模擬兩可的狀況，故此時就要自訂PARAM
        if (options.param != null) {
            param += options.param;
        } else {
            //目前先設定集團、站別、公司資料不可共享，未來可能會根據登入者個權限決定是否跨站別、公司
            if (param) {
                param += "&";
            }
            if (options.selfSite) {
                param += "sopt_GroupId=eq&GroupId=" + groupId + "&sopt_Cmp=eq&Cmp=" + cmp;// + "&sopt_Stn=eq&Stn=" + stn;
            } else {
                param += "sopt_GroupId=eq&GroupId=" + groupId + "&sopt_Cmp=eq&Cmp=" + basecmp;// + "&sopt_Stn=eq&Stn=" + basestn;
            }

        }
        console.log(param);
        mygrid = $("#lookupGrid_" + myId);
        //console.log(param);


        var baseCondition = "";

        if (options.baseCondition != null) {
            if (options.baseCondition) {
                baseCondition += options.baseCondition;
            }
        }

        if (typeof options.baseConditionFunc != "undefined") {
            var filterStr = options.baseConditionFunc()
            if (filterStr.indexOf("&sopt_") > -1) {
                param += filterStr;
            } else {
                baseCondition += filterStr;
            }
            //baseCondition += options.baseConditionFunc();
        }
        var gridUrl = options.gridUrl;
        if (typeof options.baseUrParamUrlFunc != "undefined") {
            var filterStr = options.baseUrParamUrlFunc()
            gridUrl = options.gridUrl + filterStr;
            //baseCondition += options.baseConditionFunc();
        }

        //把查詢條件放到參數內，重新跟SERVER端要資料，並RELOAD GRID  gridModel
        mygrid.jqGrid('setGridParam', {
            url: gridUrl, datatype: "json",
            postData: {
                //'conditions': encodeURI(param),
                'conditions': param,//jQuery.serialize()已经是进行URL编码过的。
                'baseCondition': initLookup_tobase64String(baseCondition)
                //model: "TrackingHubMvc.Models.Tracking.SiBlModel"
            }
        }).trigger("reloadGrid", [{ page: 1 }]);
    });

    ajustamodal("#lookupDialog_" + myId, 100, 50);

    jQuery(document).ready(function ($) {
        $(document).on("show.bs.modal", "#lookupDialog_" + myId, function () {
            DocumentPosition = $("body").scrollTop();
            //console.log(DocumentPosition);
            setTimeout(function () { $("#searchFilterBtn_" + myId).focus(); }, 200);
        });

        $(document).on("hide.bs.modal", "#lookupDialog_" + myId, function () {
            if (typeof options.focusItem != "undefined") {
                var focusItem = options.focusItem;
                setTimeout(function () { focusItem.focus() }, 200);
            }
            setTimeout(function () {
                $('html, body').animate({
                    scrollTop: DocumentPosition
                }, 200);
            }, 500);

        });
    });
}


/*function ajustamodal(myModal) 
{
    var altura = $(window).height() - 50; //value corresponding to the modal heading + footer
    var modalWidth = $(window).width() - 100;
    $(myModal + " .modal-body").css({"height":altura,"overflow-y":"auto"});
    $(myModal + " .modal-lg").css({"width": modalWidth});
}*/


function initLookup_tobase64String(str) {
    try {
        return _tobase64String(str);
    }
    catch (e) { }
    return str;
}