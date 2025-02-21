
var searchColumns;

//生成查询条件
function loadCondition(formId) {
    $.each($(":not(input[name*='VT_'])", '#' + formId), function (key, value) {
        //$(value).val($(value).val().replace('&', '(@nd)'));
        if ($(value).attr("multi_sel") != undefined && $(value).attr("multi_sel") != "undefined") {
            var mutiid = "multi_" + $(value).attr("id");
            var mutival = "";
            $('#' + mutiid + ' :selected').each(function (i, selected) {
                if (mutival != "") mutival += ";";
                mutival += $(selected).val();
            });
            $(value).val(mutival);
        }
        if ($(value).attr("multiple") == undefined || $(value).attr("multiple") == "undefined") {
            if ($(value).val() != "" && $(value).val() != null) {
                $(value).val($(value).val().replace('&', '(@nd)'));
            }
        }
    });

    var param = $(":not([name*='VT_'])", '#' + formId).serialize();

    $.each($(":not(input[name*='VT_'])", '#' + formId), function (key, value) {
        //$(value).val($(value).val().replace('(@nd)', '&'));
        if ($(value).attr("multiple") == undefined || $(value).attr("multiple") == "undefined") {
            if ($(value).val() != "" && $(value).val() != null) {
                $(value).val($(value).val().replace('(@nd)', '&'));
            }
        }
    });

    return param.trim('&');
}

function loadConditionCol(formId, colModel) {
    var dic = [];
    $.each(colModel, function (key, value) {
        if (value["sorttype"]) {
            var sorttype = value["sorttype"];
            if (value["name"] == "UId" && !dic[value["name"]] && value["title"])
                dic[value["name"]] = value["title"];
            switch (sorttype.toUpperCase()) {
                case "DATE":
                case "DATETIME":
                    if (!dic[value["name"]] && value["title"])
                        dic[value["name"]] = value["title"];
                    break;
            }
        }
    });
    //var dateFlag = true;
    var param = loadCondition(formId);
    var params = decodeURIComponent(param).split('&');
    $.each(params, function (key, value) {
        var vals = value.split('=');
        var col = vals[0].replace("$E", "").replace("$S", "");
        if (dic[col]) {
            var dateVal = vals[1];
            
            if (dateVal != "") {
                if (col == "UId") {
                    if (dic[col].toUpperCase().indexOf("UID") >= 0 || dic[col].toUpperCase().indexOf("U_ID") >= 0 || dic[col].toUpperCase().indexOf("U ID") >= 0) {
                        var regex = /^([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12})$/;
                        if (!regex.test(dateVal))
                            param = dic[col] + ": GUID Format Error";
                    }
                }
                else {
                    if (moment(dateVal).isValid() && dateVal.length>=10) {
                        var year = parseInt(moment(dateVal).format("YYYY"));
                        if (year > 9999)
                            param = dic[col] + ": DateTime Format Error";
                    } else {
                        param = dic[col] + ": DateTime Format Error";
                    }
                }
            }
        }
    });
    return param;
}
function loadVIRCondition(formId) {
    $.each($("input[name*='VT_']", '#' + formId), function (key, value) {
        $(value).val($(value).val().replace('&', '(@nd)'));
    });

    var param = $("[name*='VT_']", '#' + formId).serialize();

    $.each($("input[name*='VT_']", '#' + formId), function (key, value) {
        $(value).val($(value).val().replace('(@nd)', '&'));
    });

    return param.trim('&').replace(/VT_/g, "");
}


function getSearchColumns() {
    return searchColumns;
}

function htmlEncode(value) {
    //create a in-memory div, set it's inner text(which jQuery automatically encodes)
    //then grab the encoded contents back out.  The div never exists on the page.
    return $('<div/>').text(value).html().trim();
}

function htmlDecode(value) {
    return $('<div/>').html(value).text();
}

function GetCondiLayout(myId) {
    //console.log(gop.caption);
    console.log(myId);
    var myModel = null;
    $.ajax({
        async: false,
        url: rootPath + "Common/GetLayout",
        type: 'POST',
        data: { "layoutid": myId, "layouttype": "CONDITION" },
        dataType: "json",
        "success": function (result) {
            myModel = jQuery.parseJSON(result[0].LAYOUT);
        }
    });

    return myModel;
}


function getSelectColumn(colModel) {
    var SelColModel = colModel.slice(0);
    var resultModel = new Array();

    var delCol = 0;
    for (var i = 0; i < SelColModel.length; i++) {

        $.each(SelColModel[i], function (key, value) {
            if (key == "title") {
                SelColModel[i]["showname"] = value;
                SelColModel[i]["sorttype"] = "string";
                SelColModel[i]["sopt"] = ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'];
            }

            if (key == "formatter" && value == "date") {
                SelColModel[i]["sorttype"] = "date";
                SelColModel[i]["sopt"] = ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'bt'];
            }
            if (key == "formatter" && value == "datetime") {
                SelColModel[i]["sorttype"] = "datetime";
                SelColModel[i]["sopt"] = ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni', 'bt'];
            }

            /*if (key != "sorttype" && key != "init" && key != "name" && key != "dt" ) {
                delete SelColModel[i][key];
            }*/
        });
        //console.log(colModel[i]);
        resultModel.push(SelColModel[i]);
    }
    //console.log(SelColModel);
    //console.log(colModel);
    return resultModel;
}

function isEmpty(val) {
    if (val === undefined || val === "" || val == null)
        return true;
    return false;
}

function initSearch(gop) {

    $grid = $("#" + gop.gridId);
    //避免gridFunc重複被使用，資料彙總改為dblClickFunc
    params = loadCondition(gop.searchFormId);
    if (typeof gop.SearchFilter !== 'undefined') {
        params = params + gop.SearchFilter;
    }

    if (gop.statusDefaultId != '' && typeof gop.statusDefaultId !== "undefined") {
        params += "&" + gop.statusField + "=" + gop.statusDefaultId + "&sopt_" + gop.statusField + "=eq";
    }

    var baseCondition = "";
    if (typeof gop.baseCondition != "undefined") {
        baseCondition = gop.baseCondition;
    }

    if (typeof gop.baseConditionFunc != "undefined") {
        var filterStr = gop.baseConditionFunc()
        if (filterStr.indexOf("&sopt_") > -1) {
            params += filterStr;
        } else {
            baseCondition += filterStr;
        }
    }

    var vir_params = loadVIRCondition(gop.searchFormId);
    new initGrid($grid,
        {
            data: [], colModel: gop.gridColModel, dblClickFunc: gop.gridFunc, beforeSelectRowFunc: gop.beforeSelectRowFunc, onSelectRowFunc: gop.onSelectRowFunc
        },
        {
            datatype: gop.statusPreLoad ? "json" : "local",
            data: [],
            url: gop.gridSearchUrl,
            postData: {
                'conditions': params,//jQuery.serialize()已经是进行URL编码过的。
                'baseCondition': initSearch_tobase64String(baseCondition),
                'virConditions': vir_params
            },
            caption: gop.gridAttr.caption,
            excelName: gop.gridAttr.excelName,
            height: gop.gridAttr.height,
            rowList: [15, 50, 100],
            sortname: gop.gridAttr.sortname,
            multiSort: gop.gridAttr.multiSort,
            sortable: gop.gridAttr.sortable,
            footerrow: gop.footerrow,
            multiselect: gop.multiselect,
            cellEdit: false,
            rows: 15,
            refresh: true,
            exportexcel: true,
            footerrow: gop.footerrow,
            savelayout: true,
            colModel: gop.gridColModel,
            showcolumns: true,
            loadCompleteFunc: function () {

                if (typeof gop.loadCompleteFunc != 'undefined') {
                    gop.loadCompleteFunc();
                }
                if (typeof gop.statusDefaultId != 'undefined') {
                    mygrid = $("#" + gop.gridId);
                    $("#statusCount_" + gop.statusDefaultId).html(mygrid.jqGrid('getGridParam', 'records'));
                }
            },
            gridCompleteFunc: function () { },
            scrollerbar: gop.gridAttr.scrollerbar,
            multiboxonly: (typeof gop.multiboxonly === "undefined") ? false : gop.multiboxonly,
            // height:"300px"
        }, gop.gridAttr);


    var myId = gop.gridId;
    var ShowChart = gop.ShowChart;

    if (typeof gop.reportItem == "undefined") {
        gop.reportItem = [];
    }

    //init SearchTemplate
    var data = { myId: myId, AddUrl: gop.AddUrl, ReportItem: gop.reportItem };
    var source = $("#SearchTemplate").html();
    var template = Handlebars.compile(source);
    var html = template(data);
    $("#" + gop.searchDivId).append(html);

    //$(".condition-layout").after("<div class='topDiv' id='SearchAreaCtrl' >=</div>");



    var condiArray = { 'eq': "equal", 'ne': "not equal", 'lt': "less than", 'le': "less equal than", 'gt': "greater than", 'ge': "greater equal than", 'nu': "is null", 'nn': "is not null", 'in': "is in", 'ni': "is not in", 'cn': "like", 'nc': "not like", 'bw': "begin with", 'bn': "not begin with", 'ew': "end with", 'en': "not end with", 'bt': "between" };
    var addFilterSelID = "#addFilterSel_" + myId;
    searchColumns = gop.searchColumns;

    function _initAutocomplete() {
        $(addFilterSelID).parent().attr("class", "pure-u-sm-14-60");
        $(addFilterSelID).parent().after('<div class="pure-u-sm-7-60"><input id="addFilterText_' + myId + '" class="form-control input-sm"></input></div>');
        var _names = [];
        for (var key in searchColumns) {
            if (searchColumns.hasOwnProperty(key)) {
                if (searchColumns[key].name && searchColumns[key].showname) {
                    _names.push({ id: searchColumns[key].name, value: searchColumns[key].showname });
                }
            }
        }

        $("#addFilterText_" + myId).autocomplete({
            source: function (request, response) {
                var matcher = new RegExp($.ui.autocomplete.escapeRegex(request.term), "i");
                var _count = 0;
                response($.grep(_names, function (value) {
                    var val = value.label || value.value || value;
                    var name = value.id;
                    var result = false;
                    if ($(addFilterSelID).find("[value='" + name + "']").length > 0 && _count < 10) {
                        result = matcher.test(val) || matcher.test(name);
                    }
                    if (result)
                        _count++;
                    return result;
                }));
            },
            select: function (event, ui) {
                if ($(addFilterSelID).find("[value='" + ui.item.id + "']").length > 0)
                    $(addFilterSelID).val(ui.item.id);
            }
        });
    };
    _initAutocomplete();

    //console.log(searchColumns);
    $(".condition-layout").parent().parent().append('<div class="searchOpenClose" id="searchOpenClose">Close<span class="glyphicon glyphicon-chevron-up"></span></div>');
    var condiTemp = GetCondiLayout(gop.gridAttr.caption);

    function __initDate(name) {
        $("#sel_" + name).datepicker({
            dateFormat: 'yy/mm/dd',
            beforeShow: function () {
                //alert("111");
                setTimeout(function () {
                    $('.ui-datepicker').css('z-index', 99999999999999);
                }, 0);
            }
        });
    }

    function __initTime(name) {
        $("#sel_" + name).datetimepicker({
            dateFormat: 'yy/mm/dd',
            beforeShow: function () {
                setTimeout(function () {
                    $('.ui-datepicker').css('z-index', 99999999999999);
                }, 0);
            }
        });
    }

    function _initDateUI(searchColumns, key) {
        if (searchColumns[key].sorttype === "date" || searchColumns[key].sorttype === "datetime") {
            $("#sel_" + searchColumns[key].name).removeClass("hasDatepicker");
            $("#sel_" + searchColumns[key].name + "S").removeClass("hasDatepicker");
            $("#sel_" + searchColumns[key].name + "E").removeClass("hasDatepicker");
        }

        if (searchColumns[key].sorttype === "date") {
            __initDate(searchColumns[key].name);
            __initDate(searchColumns[key].name + "S");
            __initDate(searchColumns[key].name + "E");
        }
        if (searchColumns[key].sorttype === "datetime") {
            __initTime(searchColumns[key].name);
            __initTime(searchColumns[key].name + "S");
            __initTime(searchColumns[key].name + "E");
        }
    }
    var multi_selid = [];
    if (condiTemp != null) {
        multi_selid = [];
        $("#" + gop.searchFormId).html(htmlDecode(condiTemp));
        for (var key in searchColumns) {
            if (searchColumns.hasOwnProperty(key)) {
                if (typeof $("#sel_" + searchColumns[key].name).val() == "undefined") {
                    $(addFilterSelID).append("<option value='" + searchColumns[key].name + "'>" + searchColumns[key].showname + "</option>");
                }
                else if (typeof $("#sel_" + searchColumns[key].name).attr("multi_sel") != "undefined") {
                    var multiid = searchColumns[key].name;
                    var dataarray = $("#sel_" + multiid).val().split(";");
                    $("#multi_sel_" + multiid).val(dataarray);
                    $("#multi_sel_" + multiid).selectpicker('refresh');

                    //multi_selid.push(searchColumns[key].name);
                    //setTimeout(function () {
                    //    $.each(multi_selid, function (i, id) {
                    //        var dataarray = IssueType.split(";");
                    //        $("#multi_sel_" + id).val($("#sel_" + id).val());
                    //        $("#multi_sel_" + id).selectpicker('refresh');
                    //    });
                    //}, 6000);
                }
            }
            _initDateUI(searchColumns, key);
        }
    } else {
        //查詢條件
        for (var key in searchColumns) {
            if (searchColumns.hasOwnProperty(key)) {
                if (searchColumns[key].init != true) {
                    $(addFilterSelID).append("<option value='" + searchColumns[key].name + "'>" + searchColumns[key].showname + "</option>");
                } else {

                    if (condiTemp != null) {
                        break;
                    }


                    var thisSopt = [];
                    var ItemID = (new Date).getTime();
                    for (var soptKey in searchColumns[key].sopt) {
                        thisSopt[soptKey] = { sopt: searchColumns[key].sopt[soptKey], showsopt: condiArray[searchColumns[key].sopt[soptKey]], dfsopt: searchColumns[key].dfsopt };
                    }

                    var dt = "";
                    var dv = "";
                    var remark = "";
                    //alert(typeof searchColumns[key].dt);
                    if (typeof searchColumns[key].dt != "undefined") {
                        dt = searchColumns[key].dt;
                    }
                    if (typeof searchColumns[key].dv != "undefined") {
                        dv = searchColumns[key].dv;
                    }
                    if (typeof searchColumns[key].remark != "undefined") {
                        remark = searchColumns[key].remark;
                    }


                    var data, source, template, html;
                    //下拉选择
                    if (searchColumns[key].formatter === "select" && typeof searchColumns[key].remark == "undefined") {
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
                        if (searchColumns[key].multiselect == "true") {
                            source = $("#ConditionMultiSelectTemplate").html();
                        } else {
                            source = $("#ConditionSelectTemplate").html();
                        }
                    } else {
                        source = $("#ConditionSearchTemplate").html();
                    }

                    data = { fieldName: searchColumns[key].name, label: searchColumns[key].showname, ItemID: ItemID, sopt: thisSopt, sortType: searchColumns[key].sorttype, dT: dt, selectoptions: selectOptions, dV: dv, remark: remark };
                    template = Handlebars.compile(source);
                    html = template(data);

                    $("#" + gop.searchFormId).append(html);

                    if (searchColumns[key].sorttype == "date" || searchColumns[key].sorttype == "datetime") {
                        var data = { ItemID: ItemID, fieldName: searchColumns[key].name, sortType: searchColumns[key].sorttype, dV: dv };
                        var source = $("#BetweenTemplate").html();
                        var template = Handlebars.compile(source);
                        var html = template(data);

                        $(".tmp[ItemID='" + ItemID + "']").remove();
                        $("[tempId='temp_" + ItemID + "']:last-child").append(html);

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
                                dateFormat: "yy/mm/dd",
                                timeFormat: 'HH:mm',
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

                    if (searchColumns[key].sorttype == "string" && typeof searchColumns[key].dfsopt == "undefined") {
                        $('#sopt_' + searchColumns[key].name).val("cn");
                    }

                    if (searchColumns[key].multiselect == "true") {
                        $("#multi_sel_" + searchColumns[key].name).multiselect({
                            enableFiltering: true,
                            filterPlaceholder: 'Search for something...',
                            maxWidth: '500px',
                            buttonWidth: '100%'
                        });
                        $("#multi_sel_" + searchColumns[key].name).multiselect("destroy");
                        $("#multi_sel_" + searchColumns[key].name).selectpicker('refresh');
                    }

                }



            }
        }
    }


    //init summary add
    if (typeof gop.AddUrl === "object") {
        $("#SearchArea").on("click", "#SummaryInput", function () {
            //alert(gop.AddUrl.url);
            top.topManager.openPage({
                href: gop.AddUrl.url,
                title: gop.AddUrl.title,
                id: gop.AddUrl.id
            });
        });
    }
    else {
        if (gop.AddUrl === false) {
            $("#SummaryInput").remove();
        }

        if (gop.AddUrl == '#') {
            $("#SearchArea").on("click", "#SummaryInput", function () {
                CommonFunc.ToogleSearch(false); //资料汇总切换
            });
        }
    }


    if (gop.ShowChart === true) {
        $("#SummaryShowChart").show();
    }

    //init status Group
    if (typeof gop.statusGroup != "undefined" && gop.statusGroup.length > 0) {
        data = { ID: myId };
        source = $("#SearchStatusGroupTemplate").html();
        template = Handlebars.compile(source);
        html = template(data);
        $("#" + gop.StatusAreaId).after(html);


        $("#SearchStatus_" + myId).SearchStatus({
            data: gop.statusGroup,
            defaultId: gop.statusDefaultId,
            thisTempId: "SearchStatus_" + myId
        });

        $.each(gop.statusGroup, function (key, status) {
            $("#searchStatus_" + status.id).on("click", function () {

                if (typeof status.func != "undefined" && status.func != null) {
                    status.func();
                }


                params = loadCondition(gop.searchFormId);
                if (typeof gop.SearchFilter !== 'undefined') {
                    params = params + gop.SearchFilter;
                }

                params += "&" + gop.statusField + "=" + status.id + "&sopt_" + gop.statusField + "=eq";

                var baseCondition = "";
                if (typeof gop.baseCondition != "undefined") {
                    baseCondition = gop.baseCondition;
                }

                if (typeof gop.baseConditionFunc != "undefined") {
                    var filterStr = gop.baseConditionFunc()
                    if (filterStr.indexOf("&sopt_") > -1) {
                        params += filterStr;
                    } else {
                        baseCondition += filterStr;
                    }
                }

                if (typeof status.BeforeClick != "undefined") {
                    if (!status.BeforeClick())
                        return false;
                }

                mygrid = $("#" + gop.gridId);
                console.log(mygrid.jqGrid('setGridParam', {
                    url: gop.gridSearchUrl, datatype: "json",
                    postData: {
                        'conditions': params,//jQuery.serialize()已经是进行URL编码过的。
                        'baseCondition': initSearch_tobase64String(baseCondition)
                    },
                    loadCompleteFunc: function () {
                        //king 20170208 增加自動計數
                        $("#statusCount_" + status.id).html(mygrid.jqGrid('getGridParam', 'records'));
                        if (typeof gop.loadCompleteFunc != 'undefined') {
                            gop.loadCompleteFunc();
                        }
                    }
                }).trigger("reloadGrid", [{ page: 1 }]));
            });
        });
    }

    if (SearchReportData.length > 0) {
        if (pmsList != "") {
            var thisViewId = pmsList.split("|");
            $.each(SearchReportData, function (i, val) {
                var str = "";
                if (thisViewId[0] == SearchReportData[i]["ViewId"]) {
                    str = '<li item="' + SearchReportData[i]["RptId"] + '" RptCondition="' + SearchReportData[i]["RptCondition"] + '" RptParameter="' + SearchReportData[i]["RptParameter"] + '" RptWay="' + SearchReportData[i]["RptWay"] + '" RptType="' + SearchReportData[i]["RptType"] + '"><a href="#">' + SearchReportData[i]["RptNm"] + '</a></li>';
                    $("#ReportList").append(str);
                }
            });
        }

    }

    //init btn Group
    if (typeof gop.btnGroup != "undefined" && gop.btnGroup.length > 0) {
        data = { btnArr: gop.btnGroup };
        source = $("#SearchBtnGroupTemplate").html();
        template = Handlebars.compile(source);
        html = template(data);
        $("#" + gop.BtnGroupId).after(html);
        if (pmsList != "") {
            var pmsBtns = $(".pms-btn");
            //alert(pmsList);
            $.each(pmsBtns, function (index, value) {

                if (pmsList.indexOf($(pmsBtns[index]).attr("id")) == -1) {
                    $(pmsBtns[index]).parent("li").remove();
                }

            });
        }


        $.each(gop.btnGroup, function (key, btn) {
            $("#" + btn.id).unbind("click").on("click", function () {
                $("#" + btn.id).prop('disabled', true);
                btn.func();
                $("#" + btn.id).prop('disabled', false);
            });
        });
    }

    $(".searchOpenClose").on("click", function () {
        if ($(".condition-layout").is(":visible")) {
            $(".condition-layout").parent().hide();
            $(".searchOpenClose span").attr("class", "glyphicon glyphicon-chevron-down");
        } else {
            $(".condition-layout").parent().show();
            $(".searchOpenClose span").attr("class", "glyphicon glyphicon-chevron-up");
        }

    });
    $("#SummaryAdd").on("click", function () {
        var selname = $("#addFilterSel_" + myId).val();

        for (var key in searchColumns) {
            if (searchColumns.hasOwnProperty(key) && selname == searchColumns[key].name) {
                var thisSopt = [];
                for (var soptKey in searchColumns[key].sopt) {
                    thisSopt[soptKey] = { sopt: searchColumns[key].sopt[soptKey], showsopt: condiArray[searchColumns[key].sopt[soptKey]], dfsopt: searchColumns[key].dfsopt };
                }
                var dt = "";
                var dv = "";
                var remark = "";
                //alert(typeof searchColumns[key].dt);
                if (typeof searchColumns[key].dt != "undefined") {
                    dt = searchColumns[key].dt;
                }
                if (typeof searchColumns[key].dv != "undefined") {
                    dv = searchColumns[key].dv;
                }


                var data, source, template, html;
                //下拉选择
                if (searchColumns[key].formatter === "select" && typeof searchColumns[key].remark == "undefined") {
                    var selectOptions = [],
                        optiontemp,
                        selectstr = searchColumns[key].editoptions.value,
                        ary = selectstr.split(";");
                    for (var i = 0; i < ary.length; i++) {
                        optiontemp = ary[i].split(":");
                        if (optiontemp.length < 2)
                            continue;
                        selectOptions.push({ val: optiontemp[0], text: optiontemp[1] });
                    }
                    if (searchColumns[key].multiselect == "true") {
                        source = $("#ConditionMultiSelectTemplate").html();
                    } else {
                        source = $("#ConditionSelectTemplate").html();
                    }

                } else {
                    source = $("#ConditionSearchTemplate").html();
                    if (typeof searchColumns[key].remark != "undefined") {
                        remark = searchColumns[key].remark;
                    }
                }
                data = { fieldName: $(addFilterSelID).val(), label: searchColumns[key].showname, ItemID: (new Date).getTime(), sopt: thisSopt, sortType: searchColumns[key].sorttype, dT: dt, selectoptions: selectOptions, remark: remark, dV: dv };
                template = Handlebars.compile(source);
                html = template(data);
                $("#" + gop.searchFormId).append(html);

                if (searchColumns[key].sorttype == "date") {
                    $("#sel_" + searchColumns[key].name).datepicker({
                        dateFormat: 'yy/mm/dd',
                        beforeShow: function () {
                            setTimeout(function () {
                                $('.ui-datepicker').css('z-index', 99999999999999);
                            }, 0);
                        }
                    });
                }

                if (searchColumns[key].sorttype == "datetime") {
                    $("#sel_" + searchColumns[key].name).datetimepicker({
                        dateFormat: 'yy/mm/dd',
                        beforeShow: function () {
                            setTimeout(function () {
                                $('.ui-datepicker').css('z-index', 99999999999999);
                            }, 0);
                        }
                    });
                }

                if (searchColumns[key].multiselect == "true") {
                    $("#multi_sel_" + searchColumns[key].name).multiselect({
                        enableFiltering: true,
                        filterPlaceholder: 'Search for something...',
                        maxWidth: '500px',
                        buttonWidth: '100%'
                    });
                    $("#multi_sel_" + searchColumns[key].name).multiselect("destroy");
                    $("#multi_sel_" + searchColumns[key].name).selectpicker('refresh');
                }

                searchColumns[key].init = true;
                break;
            }
        }
        $("#addFilterSel_" + myId).find(":selected").remove();
    });

    $("#" + gop.searchFormId).on("change", "select", function () {
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

    $("#" + gop.searchFormId).on("click", ".RemoveSearch", function () {
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
                $('#addFilterSel_' + myId).append("<option value='" + searchColumns[key].name + "'>" + searchColumns[key].showname + "</option>");
                searchColumns[key].init = false;
                break;
            }

        }
    });
    $("#SummarySave").on("click", function () {


        var clone = $("#" + gop.searchFormId).clone();
        var selects = $("#" + gop.searchFormId).find("select");
        var inputs = $("#" + gop.searchFormId).find("input");
        $(selects).each(function (i) {
            var select = this;
            if ($(select).val() == "" || $(select).val() == undefined) {
                return;
            }
            if ($(select).attr("multiple") != undefined && $(select).attr("multiple") != "undefined") {
                var mutiid = $(select).attr("id");
                var mutiinputid = mutiid.replace("multi_", "");
                var mutival = "";
                $('#' + mutiid + ' :selected').each(function (i, selected) {
                    if (mutival != "") mutival += ";";
                    mutival += $(selected).val();
                });
                $("#" + mutiinputid).val(mutival);
                $(clone).find("select").eq(i).next().remove();
                return;
            }
            $(clone).find("select").eq(i).val($(select).val());
            $(clone).find("select").eq(i).find('option').removeAttr('selected');
            if (!isEmpty($(select).val()))
                $(clone).find("select").eq(i).find('option[value=' + $(select).val() + ']').attr('selected', true);
        });

        $(inputs).each(function (i) {
            var input = this;
            $(clone).find("input").eq(i).attr('value', $(input).val());
        });

        var resultModel = htmlEncode(clone.html());

        $.ajax({
            async: true,
            url: rootPath + "Common/SetLayout",
            type: 'POST',
            data: { "layout": JSON.stringify(resultModel), "layoutid": gop.gridAttr.caption, "layouttype": "CONDITION" },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            "success": function (result) {
                if (result) {
                    alert("Save Grid Layout Success!");
                }

            }
        });
    });

    $("#SummaryReset").on("click", function () {
        var r = confirm("Are you sure reset this layout? This page will reloaded!");
        if (r == true) {
            $.ajax({
                async: true,
                url: rootPath + "Common/ResetLayout",
                type: 'POST',
                data: { "layoutid": gop.gridAttr.caption, "layouttype": "CONDITION" },
                dataType: "json",
                "complete": function (xmlHttpRequest, successMsg) {
                },
                "error": function (xmlHttpRequest, errMsg) {
                    alert(errMsg);
                },
                "success": function (result) {
                    if (result) {
                        alert("Reset Grid Layout Success! This page will reloaded!");
                        location.reload();
                    }

                }
            });
        }
    });

    $("#SummarySearch").on("click", function () {
        params = loadConditionCol(gop.searchFormId, gop.searchColumns);
        if (params.indexOf("Format Error") > 0) {
            alert(params);
            return false;
        }
        var vir_params = loadVIRCondition(gop.searchFormId);
        if (typeof gop.SearchFilter !== 'undefined') {
            params = params + gop.SearchFilter;
        }
        /*
        if (gop.statusDefaultId != '') {
            params += "&" + gop.statusField + "=" + gop.statusDefaultId + "&sopt_" + gop.statusField + "=eq";
        }
        */

        if (typeof gop.BeforeSearch != "undefined") {
            if (!gop.BeforeSearch())
                return false;
        }
        var baseCondition = "";

        if (typeof gop.baseCondition != "undefined") {
            baseCondition = gop.baseCondition;
        }
        if (typeof gop.baseConditionFunc != "undefined") {
            var filterStr = gop.baseConditionFunc()
            if (filterStr.indexOf("&sopt_") > -1) {
                params += filterStr;
            } else {
                baseCondition += filterStr;
            }
        }



        mygrid = $("#" + gop.gridId);
        mygrid.jqGrid('setGridParam', {
            url: gop.gridSearchUrl, datatype: "json",
            mtype: 'POST',
            postData: {
                'conditions': params,//jQuery.serialize()已经是进行URL编码过的。
                'baseCondition': initSearch_tobase64String(baseCondition),
                'virConditions': vir_params
            },
            loadCompleteFunc: function () {
                if (typeof gop.loadCompleteFunc != 'undefined') {
                    gop.loadCompleteFunc();
                }
                //king 20170208 增加自動計數
                /*
                $("#statusCount_" + gop.statusDefaultId).html(mygrid.jqGrid('getGridParam', 'records'));
                $("#statusCount_" + gop.statusDefaultId).trigger("click");*/
            }
        }).trigger("reloadGrid", [{ page: 1 }]);

        if (typeof gop.statusGroup != "undefined" && gop.statusGroup.length > 0) {
            //alert("111");
            //params.push({ "resultType": "count", "statusField": gop.statusField });
            $(".statusCount").each(function (k, v) {
                $(v).html("0");
            });

            console.log(params);
            $.ajax({
                async: true,
                url: gop.gridSearchUrl,
                type: 'POST',
                data: {
                    'conditions': params,//jQuery.serialize()已经是进行URL编码过的。
                    'baseCondition': initSearch_tobase64String(baseCondition),
                    "resultType": "count",
                    "statusField": gop.statusField,
                    'virConditions': vir_params
                },
                "complete": function (xmlHttpRequest, successMsg) {

                },
                "error": function (xmlHttpRequest, errMsg) {
                },
                success: function (result) {
                    console.log(result);
                    var resJson = $.parseJSON(result)
                    for (var i = 0; i < resJson.rows.length; i++) {
                        $("#statusCount_" + resJson.rows[i].PoStatus).html(resJson.rows[i].Count);
                    }

                }
            });
        }


    });


    $("#ReportList").on("click", "li", function () {
        var t = $(this);
        var itemId = $(t).select("item").attr("item");
        var rptName = $(t).select("item").text();

        var RptCondition = $(this).attr("RptCondition");
        var RptWay = $(this).attr("RptWay");
        var RptType = $(this).attr("RptType");
        var RptParameter = $(this).attr("RptParameter");

        if (typeof RptCondition != "undefined") {
            var c = RptCondition.match(/(\{[A-z]{0,30}\})/g);
            var rBasecondition = "";
            for (var i = 0; i < c.length; i++) {
                var fieldName = c[i].replace("{", "").replace("}", "");
                var val = $("#" + fieldName).val();
                if (typeof val == "undefined") {
                    val = "";
                }

                val = "'" + val + "'";
                RptCondition = RptCondition.replace(c[i], val);
            }
        }



        var searchConditions = loadCondition(gop.searchFormId);
        if (typeof gop.SearchFilter !== 'undefined') {
            searchConditions = searchConditions + gop.SearchFilter;
        }

        var baseCondition = gop.baseCondition;
        if (typeof baseCondition == "undefined") {
            baseCondition = "";
        }
        if (typeof gop.baseConditionFunc != "undefined") {
            var filterStr = gop.baseConditionFunc()
            if (filterStr.indexOf("&sopt_") > -1) {
                searchConditions += filterStr
            } else {
                baseCondition += filterStr;
            }
        }


        if (typeof gop.reportFunc != "undefined") {
            gop.reportFunc(itemId, rptName, searchConditions, baseCondition);
            return;
        }

        if (RptCondition != "") {
            if (baseCondition == "") {
                baseCondition = RptCondition;
            }
            else {
                baseCondition = baseCondition + " AND " + RptCondition;
            }
        }
        var params = {};
        if (RptType == "PREVIEW") {
            params = {
                currentCondition: "",
                rptdescp: rptName,
                rptName: itemId,
                exportType: RptType,
                'conditions': searchConditions,//jQuery.serialize()已经是进行URL编码过的。
                'baseCondition': initSearch_tobase64String(baseCondition),
            };
        }
        else {
            params = {
                currentCondition: "",
                rptdescp: rptName,
                rptName: itemId,
                formatType: RptType,
                'conditions': searchConditions,//jQuery.serialize()已经是进行URL编码过的。
                'baseCondition': initSearch_tobase64String(baseCondition),
            };
        }

        var pmts = [];
        if (RptParameter != "") {
            pmts = RptParameter.split(",");
        }

        for (var j = 0; j < pmts.length; j++) {
            var val = $("#" + pmts[j]).val();
            if (typeof val == "undefined") {
                val = "";
            }
            var obj = {};
            obj[pmts[j]] = val;
            $.extend(params, obj);
        }

        $.ajax({
            async: true,
            url: rootPath + "Report/CreateNewReport",
            type: 'POST',
            data: params,
            "complete": function (xmlHttpRequest, successMsg) {
                if (successMsg != "success") return null;
                var xx = xmlHttpRequest.responseText;
                window.open(xmlHttpRequest.responseText);
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {

            }
        });
    });

    /*$("form#ConditionArea").submit(function (event) {
        $("#SummarySearch").click();
        return false;
    });*/
    /*
    if (typeof gop.statusGroup != "undefined" && gop.statusGroup.length > 0) {
        if (typeof gop.statusPreLoad != "undefined" && gop.statusPreLoad) {
            $("#SummarySearch").trigger("click");
        }
    }
    */
    $("#SummarySearch").setfocus();

}

function conditionEmptyCheck(gop) {
    var params = loadCondition(gop.searchFormId);
    var virParams = loadVIRCondition(gop.searchFormId);
    var allParams = params + "&" + virParams;
    var comParams = allParams.split('&');
    var hasCondition = false;
    $.each(comParams, function (index, val) {
        var vals = val.split('=');
        if (vals.length > 1) {
            if (vals[0].indexOf("sopt_") !== -1) {
                if (vals[1] == "nu" || vals[1] == "nn")
                    hasCondition = true;
            } else if (!hasCondition) {
                hasCondition = vals[1] != "";
            }
        }
    });
    return hasCondition;
}

function getCreateDateParams(colName, gop) {
    if (!conditionEmptyCheck(gop)) {
        var today = new Date();
        var ms = 180 * (1000 * 60 * 60 * 24);
        var starday = new Date(today.getTime() - ms);
        var yyyy = starday.getFullYear();
        var mm = starday.getMonth() + 1;
        var dd = starday.getDate();
        return "&" + colName + "=" + yyyy + "-" + mm + "-" + dd + "&sopt_" + colName + "=gt";
    }
    return "";
}


function initSearch_tobase64String(str) {
    try {
        return _tobase64String(str);
    }
    catch (e) { }
    return str;
}
