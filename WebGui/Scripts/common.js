var CommonFunc = {};
var DEBUGGERMOD = true;
var commmonlang = "zh-CN";
if (document.location && document.location.pathname && document.location.pathname.toLowerCase) {
    var hostpath = document.location.pathname.toLowerCase();
    if (hostpath.indexOf('/zh-tw/') > -1)
        commmonlang = 'zh-TW';
    else if (hostpath.indexOf('/zh-cn/') > -1)
        commmonlang = 'zh-CN';
    else if (hostpath.indexOf('/en-us/') > -1)
        commmonlang = 'en-US';
    else if (hostpath.indexOf('/ru-ru/') > -1)
        commmonlang = 'ru-RU';
}

var langDefaul = GetLang(commmonlang);


CommonFunc.Notify = function (Title, Msg, delay, type) {

    var icon = "";
    switch (type) {
        case "success":
            icon = "glyphicon glyphicon-ok-sign";
            break;
        case "danger":
            icon = "glyphicon glyphicon-remove-sign";
            break;
        case "warning":
            icon = "glyphicon glyphicon-warning-sign";
            break;
        default:
            icon = "glyphicon glyphicon-warning-sign";
            break;

    }
    var setting = {
        type: type,
        delay: delay,
        z_index: 1060,
        animate: {
            enter: 'animated fadeInDown',
            exit: 'animated fadeOutUp'
        },
        placement: {
            from: "top",
            align: "center"
        }
    };

    if (type == "danger" || type == "warning") {
        alert(Msg);
    }
    else {
        $.notify({
            icon: icon,
            title: Title,
            message: Msg,
        }, setting);
    }


}

CommonFunc.IndexNotify = function (Title, Msg, delay, type) {

    var icon = "";
    switch (type) {
        case "success":
            icon = "glyphicon glyphicon-ok-sign";
            break;
        case "danger":
            icon = "glyphicon glyphicon-remove-sign";
            break;
        case "warning":
            icon = "glyphicon glyphicon-warning-sign";
            break;
        case "info":
            icon = "glyphicon glyphicon-envelope";
            break;
        default:
            icon = "glyphicon glyphicon-warning-sign";
            break;

    }
    var setting = {
        type: type,
        delay: delay,
        z_index: 1031,
        offset: 30,
        mouse_over: "pause",
        animate: {
            enter: 'animated fadeInUp',
            exit: 'animated fadeOutDown'
        },
        placement: {
            from: "bottom",
            align: "right"
        }
    };

    $.notify({
        icon: icon,
        title: Title,
        message: Msg,
    }, setting);
}

CommonFunc.initField = function (schemas, dt) {
    //console.log(schemas);
    var fields;
    var attr;
    //schemas = JSON.parse(schemas.replace(/&quot;/g, '"'));
    dt = dt || "mt";
    var $fieldObj;
    $(schemas).each(function (index, item) {
        for (var field in item) {
            var fieldsName = field;
            var fieldsAttrs = item[field];
            $fieldObj = $("[fieldname=" + fieldsName + "][dt=" + dt + "]");
            if (fieldsAttrs.isKey == true) {
                $fieldObj.attr("isKey", "true");
            }
            if (fieldsAttrs.fieldType == "string") {
                $fieldObj.attr("maxlength", fieldsAttrs.maxLength);
            }
            if (fieldsAttrs.fieldType == "decimal") {
                $fieldObj.addClass('input-num-right');
                var Precision = fieldsAttrs.numericPrecision;
                var Scale = fieldsAttrs.numericScale;
                var regStr = "";
                var regMsg = "";
                //if(fieldsAttrs.isKey == true)
                //{
                //    $("[fieldname="+fieldsName+"]").attr("isKey", "true");
                //}
                if (Scale == 0) {
                    //regStr = "\\d{1," + Precision + "}(\\.[0-9]{0," + Scale + "})?";
                    regStr = "^(\\$|[-]|)([0-9]\\d{0," + Precision + "}(\\,\\d{3})*|([0-9]\\d{0," + Precision + "}))?$";
                    regMsg = langDefaul.common_integer + Precision + commmonlang.common_position;
                }
                else if (Scale > 0) {
                    //regStr = "\\d{1," + Precision + "}(\\.[0-9]{1," + Scale + "})?";
                    regStr = "^(\\$|[-]|)([0-9]\\d{0," + Precision + "}(\\,\\d{3})*|([0-9]\\d{0," + Precision + "}))(\\.\\d{1," + Scale + "})?$";
                    regMsg = commmonlang.common_btdp + Precision + commmonlang.common_batdp + Scale + commmonlang.common_position;
                }

                $fieldObj.attr("data-validation-regex-regex", regStr);
                $fieldObj.attr("data-validation-regex-message", regMsg);
                $fieldObj.attr("Precision", Precision);
                $fieldObj.attr("Scale", Scale);

                $fieldObj.attr("isNumber", "true");
            }

            if (fieldsAttrs.fieldType == "int") {
                $fieldObj.addClass('input-num-right');

                var Precision = fieldsAttrs.numericPrecision;

                var regStr = "";
                var regMsg = "";
                //if(fieldsAttrs.isKey == true)
                //{
                //    $("[fieldname="+fieldsName+"]").attr("isKey", "true");
                //}
                regStr = "^[0-9]{1," + Precision + "}$";
                regMsg = commmonlang.common_integer + Precision + commmonlang.common_position;
                $fieldObj.attr("data-validation-regex-regex", regStr);
                $fieldObj.attr("data-validation-regex-message", regMsg);

                $fieldObj.attr("isNumber", "true");
            }

            if (fieldsAttrs.allowDBNull == false) {
                $fieldObj.attr("required", "true");
                $fieldObj.prop("required", "true");
                $("label[for='" + fieldsName + "']").css("color", "red");
            }

            if (fieldsAttrs.fieldType == "date" && $fieldObj.is("input")) {

                if ($("input[fieldname=" + fieldsName + "][dt=" + dt + "]").prop("readonly") === false) {
                    $fieldObj.attr("fieldType", "date");
                    var NewDateFormat = $fieldObj.attr("dateformat");
                    if (NewDateFormat == null) {
                        NewDateFormat = "yy/mm/dd";
                    }
                    /*$("input[fieldname=" + fieldsName + "][dt=" + dt + "]").datepicker({
                        showOn: "button",
                        buttonImage: '../../Images/datepicker.png',
                        buttonImageOnly: true,
                        dateFormat: NewDateFormat,
                        beforeShow: function() {
                            setTimeout(function(){
                                $('.ui-datepicker').css('z-index', 99999999999999);
                            }, 0);
                        },
                        onClose: function (text, inst) {
                            $(this).focus();
                        }
                    });*/


                    var beforeShowFunc = function () {
                        var showmsg = $(this).attr("showmsg");
                        if (showmsg == null || typeof (showmsg) == "undefined") {
                            setTimeout(function () {
                                $('.ui-datepicker').css('z-index', 99999999999999);
                            }, 0);
                            return true;
                        }
                        if (confirm(_getLang(showmsg, ""))) {
                            setTimeout(function () {
                                $('.ui-datepicker').css('z-index', 99999999999999);
                            }, 0);
                            return true;
                        } else {
                            return false;
                        }
                    }

                    $("input[fieldname=" + fieldsName + "][dt=" + dt + "]").wrap('<div class="input-group">')
                        .datepicker({
                            showOn: "button",
                            changeYear: true,
                            dateFormat: NewDateFormat,
                            beforeShow: beforeShowFunc,
                            onClose: function (text, inst) {
                                $(this).focus();
                            }
                        })
                        .next("button").button({
                            icons: { primary: "ui-icon-calendar" },
                            label: "Select a date",
                            text: false
                        })
                        .addClass("btn btn-sm btn-default").prop("disabled", true).html("<span class='glyphicon glyphicon-calendar'></sapn>")
                        .wrap('<span class="input-group-btn">')
                        .find('.ui-button-text')
                        .css({
                            'visibility': 'hidden',
                            'display': 'inline'
                        });

                    $("input[fieldname=" + fieldsName + "][dt=" + dt + "]").blur(function () {
                        var timeRegex1 = /(([0-9]{3}[1-9]|[0-9]{2}[1-9][0-9]{1}|[0-9]{1}[1-9][0-9]{2}|[1-9][0-9]{3})[-|\/](((0[13578]|1[02])[-|\/](0[1-9]|[12][0-9]|3[01]))|((0[469]|11)[-|\/](0[1-9]|[12][0-9]|30))|(02-(0[1-9]|[1][0-9]|2[0-8]))))|((([0-9]{2})(0[48]|[2468][048]|[13579][26])|((0[48]|[2468][048]|[3579][26])00))-02-29)$/;
                        var dateinput = $(this).val();
                        if (dateinput.indexOf("/") < 0 && dateinput.indexOf("-") < 0 && dateinput.length == 8) {
                            dateinput = dateinput.substring(0, 4) + "/" + dateinput.substring(4, 6) + "/" + dateinput.substring(6, 8);
                            $(this).val(dateinput);
                        }

                        if (dateinput.indexOf("/") < 0 && dateinput.indexOf("-") < 0 && dateinput.length >= 12) {
                            dateinput = dateinput.substring(0, 4) + "/" + dateinput.substring(4, 6) + "/" + dateinput.substring(6, 8) + " " + dateinput.substring(8, 10) + ":" + dateinput.substring(10, 12);
                            $(this).val(dateinput);
                        }

                        if (dateinput != "" && (!isValidDate(dateinput) || dateinput.length > 10)) {
                            CommonFunc.Notify("", _getLang("L_DateError1", "日期格式错误，请将日期更正为YYYY-MM-DD"), 1300, "warning");
                            $(this).val("");
                        }
                    })
                }

            }

            if (fieldsAttrs.fieldType == "datetime" && $fieldObj.is("input")) {
                if ($("input[fieldname=" + fieldsName + "][dt=" + dt + "]").prop("readonly") === false) {
                    $("[fieldname=" + fieldsName + "]").attr("fieldType", "datetime");
                    var NewDateFormat = $fieldObj.attr("dateformat");
                    if (NewDateFormat == null) {
                        NewDateFormat = "yy/mm/dd";
                    }

                    var beforeShowFunc = function () {
                        var showmsg = $(this).attr("showmsg");
                        if (showmsg == null || typeof (showmsg) == "undefined") {
                            setTimeout(function () {
                                $('.ui-datepicker').css('z-index', 99999999999999);
                            }, 0);
                            return true;
                        }
                        if (confirm(_getLang(showmsg, ""))) {
                            setTimeout(function () {
                                $('.ui-datepicker').css('z-index', 99999999999999);
                            }, 0);
                            return true;
                        } else {
                            return false;
                        }
                    }

                    $("input[fieldname=" + fieldsName + "][dt=" + dt + "]").wrap('<div class="input-group">')
                        .datetimepicker({
                            showOn: "button",
                            changeYear: true,
                            dateFormat: NewDateFormat,
                            timeFormat: 'HH:mm',
                            beforeShow: beforeShowFunc,
                            onClose: function (text, inst) {
                                $(this).focus();
                            }
                        })
                        .next("button").button({
                            icons: { primary: "ui-icon-calendar" },
                            label: "Select a date",
                            text: false
                        })
                        .addClass("btn btn-sm btn-default").prop("disabled", true).html("<span class='glyphicon glyphicon-calendar'></sapn>")
                        .wrap('<span class="input-group-btn">')
                        .find('.ui-button-text')
                        .css({
                            'visibility': 'hidden',
                            'display': 'inline'
                        });

                    $("input[fieldname=" + fieldsName + "][dt=" + dt + "]").blur(function () {
                        var timeRegex1 = /(([0-9]{3}[1-9]|[0-9]{2}[1-9][0-9]{1}|[0-9]{1}[1-9][0-9]{2}|[1-9][0-9]{3})[-|\/](((0[13578]|1[02])[-|\/](0[1-9]|[12][0-9]|3[01]))|((0[469]|11)[-|\/](0[1-9]|[12][0-9]|30))|(02-(0[1-9]|[1][0-9]|2[0-8]))))|((([0-9]{2})(0[48]|[2468][048]|[13579][26])|((0[48]|[2468][048]|[3579][26])00))-02-29)\\s+([0-1]?[0-9]|2[0-3]):([0-5][0-9])$/;
                        var dateinput = $(this).val();
                        if (dateinput.indexOf("/") < 0 && dateinput.indexOf("-") < 0 && dateinput.length == 8) {
                            dateinput = dateinput.substring(0, 4) + "/" + dateinput.substring(4, 6) + "/" + dateinput.substring(6, 8);
                            dateinput += " 00:00";
                            $(this).val(dateinput);
                        }

                        if (dateinput.indexOf("/") < 0 && dateinput.indexOf("-") < 0 && dateinput.length >= 12) {
                            dateinput = dateinput.substring(0, 4) + "/" + dateinput.substring(4, 6) + "/" + dateinput.substring(6, 8) + " " + dateinput.substring(8, 10) + ":" + dateinput.substring(10, 12);
                            $(this).val(dateinput);
                        }
                        if (dateinput != "" && (!isValidDateTime(dateinput) || dateinput.length > 16)) {
                            CommonFunc.Notify("", _getLang("L_DateError2", "日期格式错误，请将日期更正为YYYY-MM-DD HH-MM"), 1300, "warning");
                            $(this).val("");
                        }
                    })
                }

            }

            for (var attrName in fieldsAttrs) {
                attr = attrName;
            }
        }
    });
}

CommonFunc.ToogleLoading = function (LoadingSwitch) {
    if (LoadingSwitch === true) {
        if ($("body").height() < $(document).height()) {

            $("body").addClass('noscroll');
        }

        $("#MainOverlay").fadeIn(10);
    }
    else {
        $("body").removeClass('noscroll');
        $("#MainOverlay").fadeOut(10);
    }
}

CommonFunc.formatFloat = function (num, pos) {
    var size = Math.pow(10, pos);
    var val = (Math.round(num * size) / size).toFixed(pos);
    return AppendComma(val, pos);
}

CommonFunc.formatFloatNoComma = function (num, pos) {
    var size = Math.pow(10, pos);
    return (Math.round(num * size) / size).toFixed(pos);
}

var substringMatcher = function (strs) {
    return function findMatches(q, cb) {
        var matches, substrRegex;

        // an array that will be populated with substring matches
        matches = [];

        // regex used to determine if a string contains the substring `q`
        substrRegex = new RegExp(q, 'i');

        // iterate through the pool of strings and for any string that
        // contains the substring `q`, add it to the `matches` array
        $.each(strs, function (i, str) {
            if (substrRegex.test(str)) {
                // the typeahead jQuery plugin expects suggestions to a
                // JavaScript object, refer to typeahead docs for more info
                matches.push({ value: str });
            }
        });

        cb(matches);
    };
};

CommonFunc.AutoComplete = function (elementID, keyinNum, url, params, returnValue, callBack) {
    CommonFunc.AutoComplete(elementID, keyinNum, url, params, returnValue, callBack, null, null, false);
}

CommonFunc.AutoComplete = function (elementID, keyinNum, url, params, returnValue, callBack, dymcFunc) {
    CommonFunc.AutoComplete(elementID, keyinNum, url, params, returnValue, callBack, dymcFunc, null, false);
}
//elementID=>regust Target
//keyinNum=>number of key in trigger autoCompelete
//url=>ajax post url(default Common/GetAutoCompData reutrn json)
//params=>dt=BSCODE&CD_TYPE=RC&APPLY=Y&CD~ (dt is table name,num is show list default 10, last param is like column in elementID)
//returnValue=>CD_DESCP&CD=showValue,CD,CD_DESCP (it have a showvalue , the '&' will repalce to +'-'+ ,other colnum using ',')
//callBack=> function of process selected method,if overwrite elementCD val using return false
CommonFunc.AutoComplete = function (elementID, keyinNum, url, params, returnValue, callBack, dymcFunc, clearFunc, defaultCase, OtherParamFunc) {

    if (url == "") {
        url = rootPath + "Common/GetAutoCompData"
    }
    //alert(url);


    var thisData = {};


    //console.log($(elementID));
    if ($(elementID).data('ui-autocomplete') != undefined) {
        $(elementID).autocomplete("destroy");
    }

    $(elementID).autocomplete({
        minLength: keyinNum,
        autoFocus: true,
        source: function (request, response) {
        },
        change: function (event, ui) {
            if ($(this).val() != "") {
                StatusBarArr.colMsg("Processing...");
                var paramStr = params + $(elementID).val();
                if (dymcFunc != null) {
                    if (dymcFunc) {
                        paramStr = dymcFunc() + "&" + paramStr;
                    }
                }
                //alert(paramStr);
                var __val = paramStr;
                var conditions = encodeURIComponent(paramStr || '');
                if (url.indexOf("Common/GetAutoCompData") > 0)
                    __val = encodeURIComponent(__val || '');
                var postData = { "params": __val, "returnValue": returnValue, "conditions": conditions };
                if (typeof OtherParamFunc != "undefined") {
                    var otherparm = OtherParamFunc()
                    if (otherparm != "undefined") {
                        postData = $.extend(postData, otherparm);
                    }
                }
                var _jqpostxhr = $.post(url, postData, function (data, textStatus, xhr) {
                    thisData = data;
                    var ui = {};
                    if (data.rows) {
                        ui.item = data.rows[0];
                    } else {
                        ui.item = data[0];
                    }

                    if (ui.item) {
                        callBack(null, ui);
                        if (typeof $(elementID).val() != "undefined") {
                            if (defaultCase) {
                                $(elementID).val($(elementID).val());
                            } else {
                                $(elementID).val($(elementID).val().toUpperCase());
                            }
                        }

                    } else {
                        $(elementID).val("");
                        $("input[ref='" + elementID.substring(1) + "']").val("");
                        CommonFunc.Notify("", langDefaul.common_cdne, 500, "warning");
                        if (clearFunc != null) {
                            clearFunc();
                        }
                    }
                    setTimeout(function () { StatusBarArr.colMsg(""); }, 200);
                }, "json");
                //_jqpostxhr.complete(function () { StatusBarArr.nowStatus(""); StatusBarArr.colMsg(""); });
            } else {
                if (clearFunc != null) {
                    clearFunc();
                }
                $(elementID).val("");
                $("input[ref='" + elementID.substring(1) + "']").val("");
            }
            $(elementID).attr("blur-type", "false");
        },
    });
    $(elementID).on("autocompletechangefunc", function (event, val) {
        if ($(this).val() != "") {
            var paramStr = params + val;

            if (dymcFunc != null) {
                if (dymcFunc) {
                    paramStr = dymcFunc() + "&" + paramStr;

                    //alert(paramStr);
                }
            }
            //alert(elementID);
            var __val = paramStr;
            var conditions = encodeURIComponent(__val || '');
            if (url.indexOf("Common/GetAutoCompData") > 0)
                __val = encodeURIComponent(__val || '');

            var postData = { "params": __val, "returnValue": returnValue, "conditions": conditions };
            if (typeof OtherParamFunc != "undefined") {
                var otherparm = OtherParamFunc()
                if (otherparm != "undefined") {
                    postData = $.extend(postData, otherparm);
                }
            }
            StatusBarArr.colMsg("Processing...");
            var _jqpostxhr = $.post(url, postData, function (data, textStatus, xhr) {
                thisData = data;
                var ui = {};
                if (data.rows) {
                    ui.item = data.rows[0];
                } else {
                    ui.item = data[0];
                }
                if (ui.item || params.indexOf("clearevent=false") > -1) {
                    if (params.indexOf("clearevent=false") == -1) {
                        callBack(null, ui, this);
                    }
                    if (typeof val != "undefined") {
                        val = val.toUpperCase();
                    }

                } else {
                    var data = elementID.substring(1).split('_');
                    callBack(null, null)
                    val = "";

                    CommonFunc.Notify("", langDefaul.common_cdne, 500, "warning");
                    if (clearFunc != null) {
                        clearFunc();
                    }
                    //}
                }
                setTimeout(function () { StatusBarArr.colMsg(""); }, 200);
            }, "json");
            //_jqpostxhr.complete(function () { });
        } else {
            if (clearFunc != null) {
                clearFunc();
            }
            callBack(null, null);
        }


        return val;
    });
}

CommonFunc.oAutoComplete = function (elementID, keyinNum, url, params, returnValue, callBack) {
    CommonFunc.oAutoComplete(elementID, keyinNum, url, params, returnValue, callBack, null, null);
}

CommonFunc.oAutoComplete = function (elementID, keyinNum, url, params, returnValue, callBack, dymcFunc) {
    CommonFunc.oAutoComplete(elementID, keyinNum, url, params, returnValue, callBack, dymcFunc, null);
}

CommonFunc.oAutoComplete = function (elementID, keyinNum, url, params, returnValue, callBack, dymcFunc, clearFunc, closeFunc, OtherParamFunc) {

    if (url == "") {
        url = rootPath + "Common/GetAutoCompData"
    }

    var oAuto_data = null;
    $(elementID).autocomplete({
        minLength: keyinNum,
        autoFocus: true,
        source: function (request, response) {
            var paramStr = params;
            if (dymcFunc != null) {
                if (dymcFunc) {
                    paramStr += dymcFunc();
                }
            }

            var __val = paramStr + $(elementID).val();
            var conditions = encodeURIComponent(__val || '');
            if (url.indexOf("Common/GetAutoCompData") > 0)
                __val = encodeURIComponent(__val || '');

            var postData = { "params": __val, "returnValue": returnValue, "conditions": conditions };
            if (typeof OtherParamFunc != "undefined") {
                var otherparm = OtherParamFunc()
                if (otherparm != "undefined") {
                    postData = $.extend(postData, otherparm);
                }
            }
            $.post(url, postData, function (data, textStatus, xhr) {
                //console.log(postData);
                var auto_data = [];
                if (data.rows) {
                    var vals = returnValue.replace("=showvalue", "").split("&");
                    $.each(data.rows, function (i, o) {
                        var str = "";
                        $.each(vals, function (idx, obj) {
                            str += data.rows[i][obj] + ":";
                        });
                        if (str.length > 0)
                            str = str.substring(0, str.length - 1);
                        auto_data.push({ "label": str, "returnValue": o });
                    });
                } else {
                    auto_data = data;
                }
                oAuto_data = auto_data;
                response(auto_data);
            }, "json");
        },
        select: callBack,
        change: function (event, ui) {
            //console.log(event);
            // console.log(ui);
            //console.log($(elementID).autocomplete("search", $(this).val()));
            //callBack(event, ui)

            if (ui.item || params.indexOf("clearevent=false") > -1) {
                //console.log("ui.item.value: " + ui.item.value);
            } else {
                if (clearFunc != null) {
                    clearFunc();
                }
                $(this).val("");

                if (closeFunc != null && oAuto_data && oAuto_data.length > 0) {
                    for (var i = 0; i < oAuto_data.length; i++) {
                        if (oAuto_data[i].label != "" && oAuto_data[i].label != undefined) {
                            closeFunc(oAuto_data);
                            break;
                        }
                    }
                }
            }
            //console.log("this.value: " + this.value);
        }
    });
}

CommonFunc.GetGridEditOption = function (url, params, returnValue) {
    var thisData = {};
    var thisOption = "";
    if (url == "") {
        url = rootPath + "Common/GetAutoCompData"
    }
    var postData = { "params": params + "&num=1000", "returnValue": returnValue };
    /*  $.post(url, postData, function (data, textStatus, xhr) {
          $.each(data, function (idx, obj) {
              thisOption += obj.label + ";";
              console.log(obj.label);
        });
        return thisOption;
      }, "json");*/


    $.ajax({
        async: false,
        url: url,
        type: 'POST',
        data: postData,
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
        },
        "error": function (xmlHttpRequest, errMsg) {

        },
        success: function (data) {
            $.each(data, function (idx, obj) {
                thisOption += obj.label + ";";
                console.log(obj.label);
            });
            return thisOption;
        }
    });
    return thisOption;
}

CommonFunc.formatNumber = function (number) {
    var number = number.toFixed(2) + '';
    var x = number.split('.');
    var x1 = x[0];
    var x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    return x1 + x2;
}

CommonFunc.consoleMsg = function (msg) {
    if (DEBUGGERMOD === true) {
        console.log(msg);
    }

    return false;
}

CommonFunc.ToogleSearch = function (show) {
    if (show === true) {
        $("#DataSearch").show();
        $("#MainArea").hide();
        $("#navRoot").hide();
        $("#wrapper").addClass('False');
    }
    else {
        $("#DataSearch").hide();
        $("#MainArea").show();
        $("#navRoot").show();
        $("#wrapper").removeClass('False');
    }
}

function checkNoAllowNullFields() {
    var noAllowNullFields = $("[required]"),
        result = true;
    $.each(noAllowNullFields, function (index, item) {
        item = $(item);
        if (item.attr("required") === "required") {
            var val = item.val(),
                title = item.attr("placeholder") || "";
            if (!val || $.trim(val) === '') {
                console.log(item);
                var showName = $("Label[for='" + item[0].name + "']").html();
                if (showName == "" || typeof showName == "undefined") {
                    showName = item[0].name
                }

                alert(showName + " Can't be empty");
                item.focus();
                result = false;
                return false;
            }
        }
    });
    return result;
}

function getAutoNo(id, params) {
    $.ajax({
        async: true,
        url: rootPath + "COMMON/GetAutoNo",
        type: 'POST',
        data: { "params": params },
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {

        },
        "error": function (xmlHttpRequest, errMsg) {
            return "";
        },
        success: function (result) {
            //console.log(result);
            $("#" + id).val(result[0].autoNo);
        }
    });
}

function recoverAutoNo(params) {
    $.ajax({
        async: true,
        url: rootPath + "COMMON/RecoverAutoNo",
        type: 'POST',
        data: { "params": params },
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {

        },
        "error": function (xmlHttpRequest, errMsg) {
            return "";
        },
        success: function (result) {
            return result[0].autoNo;
        }
    });
}

(function ($) {
    jQuery.fn.setfocus = function () {
        return this.each(function () {
            var dom = this;
            setTimeout(function () {
                try { dom.focus(); dom.select(); } catch (e) { }
            }, 0);
        });
    };

    $.fn.v3Lookup = function (jsonMap) {
        var options = {};
        if (typeof jsonMap.url !== "undefined")
            options.gridUrl = jsonMap.url;

        if (typeof jsonMap.param !== "undefined")
            options.param = jsonMap.param;
        else
            options.param = "";

        if (typeof jsonMap.registerBtn !== "undefined")
            options.registerBtn = jsonMap.registerBtn;
        else
            options.registerBtn = $(this);

        if (typeof jsonMap.focusItem !== "undefined")
            options.focusItem = jsonMap.focusItem;
        else
            options.focusItem = $(this).parent().siblings('input');

        if (typeof jsonMap.gridFunc !== "undefined")
            options.gridFunc = jsonMap.gridFunc;

        if (typeof jsonMap.lookUpConfig !== "undefined")
            options.lookUpConfig = jsonMap.lookUpConfig;

        if (typeof jsonMap.isMutiSel !== "undefined")
            options.isMutiSel = jsonMap.isMutiSel;
        else
            options.isMutiSel = true;

        if (typeof jsonMap.baseConditionFunc !== "undefined")
            options.baseConditionFunc = jsonMap.baseConditionFunc;

        if (typeof jsonMap.baseCondition !== "undefined")
            options.baseCondition = jsonMap.baseCondition;
        else
            options.baseCondition = "";

        if (typeof jsonMap.responseMethod !== "undefined")
            options.responseMethod = jsonMap.responseMethod;

        if (typeof jsonMap.openclick !== "undefined")
            options.openclick = jsonMap.openclick;

        initLookUp(options);
    }

    $.fn.v3AutoComplete = function (jsonMap) {
        var opt = {};
        if (typeof jsonMap.url !== "undefined")
            opt.url = jsonMap.url;
        else
            opt.url = "";

        if (typeof jsonMap.keyinNum !== "undefined")
            opt.keyinNum = jsonMap.keyinNum;
        else
            opt.keyinNum = 1;

        if (typeof jsonMap.params !== "undefined")
            opt.params = jsonMap.params;
        else
            opt.params = "";

        if (typeof jsonMap.returnValue !== "undefined")
            opt.returnValue = jsonMap.returnValue;
        else
            opt.returnValue = "";

        if (typeof jsonMap.callBack !== "undefined")
            opt.callBack = jsonMap.callBack;
        else
            opt.callBack = function (event, ui) { return false };

        if (typeof jsonMap.dymcFunc !== "undefined")
            opt.dymcFunc = jsonMap.dymcFunc;
        else
            opt.dymcFunc = function () { return "" };

        if (typeof jsonMap.clearFunc !== "undefined")
            opt.clearFunc = jsonMap.clearFunc;
        else
            opt.clearFunc = function () { return "" };
        if (typeof jsonMap.closeFunc !== "undefined")
            opt.closeFunc = jsonMap.closeFunc;
        else
            opt.closeFunc = function () { return "" };
        CommonFunc.oAutoComplete($(this), opt.keyinNum, opt.url, opt.params, opt.returnValue, opt.callBack, opt.dymcFunc, opt.clearFunc, opt.closeFunc);
    }
})(jQuery);


//by milo 20150417
$.fn.setFormByJson = function (jsonMap) {
    var form = this;
    $.each(jsonMap, function (name, val) {
        var $oinput = form.find("input [name=" + name + "]");
        if ($oinput.attr("type") == "radio" || $oinput.attr("type") == "checkbox") {
            $oinput.each(function () {
                if (Object.prototype.toString.apply(val) == '[object Array]') {
                    for (var i = 0; i < val.length; i++) {
                        if ($(this).val() == val[i])
                            $(this).attr("checked", "checked");
                    }
                } else {
                    if ($(this).val() == val)
                        $(this).attr("checked", "checked");
                }
            });
        } else if ($oinput.attr("type") == "") {
            form.find("[name=" + name + "]").html(val);
        } else {
            form.find("[name=" + name + "]").val(val);
        }

    });
};

//by milo 20150417
$.fn.serializeObject = function () {
    var o = {
    };
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name] !== undefined) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};

var REGX_HTML_ENCODE = /"|&|'|<|>|[\x00-\x20]|[\x7F-\xFF]|[\u0100-\u2700]/g;
var REGX_HTML_DECODE = /&\w+;|&#(\d+);/g;
var REGX_TRIM = /(^\s*)|(\s*$)/g;
var HTML_DECODE = {
    "&lt;": "<",
    "&gt;": ">",
    "&amp;": "&",
    "&nbsp;": " ",
    "&quot;": "\"",
    "&copy;": ""
};

function encodeHtml(s) {
    s = s ? s : this.toString();
    return (typeof s != "string") ? s : s.replace(REGX_HTML_ENCODE,
        function ($0) {
            var c = $0.charCodeAt(0), r = ["&#"];
            c = (c == 0x20) ? 0xA0 : c;
            r.push(c); r.push(";");
            return r.join("");
        });
}

function decodeHtml(s) {
    s = s ? s : this.toString();
    return (typeof s != "string") ? s : s.replace(REGX_HTML_DECODE,
                 function ($0, $1) {
                     var c = HTML_DECODE[$0];
                     if (c == undefined) {
                         if (!isNaN($1)) {
                             c = String.fromCharCode(($1 == 160) ? 32 : $1);
                         } else {
                             c = $0;
                         }
                     }
                     return c;
                 });
}


//by Tim 20150507
$.fn.focusFirst = function () {
    var elem = $(this).find(".form-control:visible").eq(0);
    //var elem = $('input:visible', this).get(0);
    /*var select = $('select:visible', this).get(0);
    if (select && elem) {
        if (select.offsetTop > elem.offsetTop) {
          elem = select;
        }
    }
    var textarea = $('textarea:visible', this).get(0);
    if (textarea && elem) {
        if (textarea.offsetTop < elem.offsetTop) {
            elem = textarea;
        }
    }*/

    if (elem) {
        setTimeout(function () { elem.focus(); }, 0);
    }
    return this;
}

$.fn.center = function () {
    this.css("position", "absolute");
    this.css("top", ($(window).height() - this.height()) / 2 + $(window).scrollTop() + "px");
    this.css("left", ($(window).width() - this.width()) / 2 + $(window).scrollLeft() + "px");
    return this;
}

jQuery(document).ready(function ($) {
    var $SearchStatus = $("#SearchStatus");
    $SearchStatus.on("click", ".status-box", function () {
        $(this).addClass('active');
        $(this).siblings().removeClass('active');
    });

    //按Tab時略過disable or readonly的input
    $("#wrapper").on("keydown", "input, textarea", function (e) {

        var keyCode = e.keyCode || e.which;

        if (keyCode == 9) {
            CommonFunc.consoleMsg("#Wrapper Keydown tab Event");
            if ($(this).hasClass('lastInput')) {
                $("#wrapper").focusFirst();
                return false;
            }
            else if (typeof $(this).attr("TargetEl") !== "undefined") {
                var el = $(this).attr("TargetEl");
                $(el).setfocus();
                return false;
            }
            else {
                if ($(this).prop("disabled") == true || $(this).attr("readonly") == "readonly") {
                    $(':input:eq(' + ($(':input').index(this) + 1) + ')').focus(function () { $(this).select(); });
                }
            }
        }
    });

    $(document).on("keydown", function (e) {
        if (e.which === 8 && !$(e.target).is("input, textarea")) {
            CommonFunc.consoleMsg("#Wrapper Keydown BackSpace Event");
            e.preventDefault();
            return false;
        }
        if (e.which === 13 && $(e.target).is("input, textarea") && e.target.id.indexOf("sel_")==0) {
            $("#SummarySearch").trigger("click");
            e.preventDefault();
            return false;
        }
    });

    //input on foucus時，如果this為disable or readonle則略過跳下一個
    $("#wrapper").on("focusin", "input", function (e) {
        if ($(this).prop("disabled") == true || $(this).attr("readonly") == "readonly") {
            CommonFunc.consoleMsg("#Wrapper Focusin Event");
            $(':input:eq(' + ($(':input').index(this) + 1) + ')').setfocus();
            //$(this).select();
        }

        var InputDep = $("#wrapper").attr("InputDep");
        //有InputDep=B的才加上comma
        if (InputDep == "B") {
            if ($(this).attr("isNumber") === "true") {
                $(this).val(RemoveComma($(this).val()));
            }
        }

    });


    $("#wrapper").on("focusout", "input", function (e) {
        var InputDep = $("#wrapper").attr("InputDep");

        if (InputDep == "B") {
            if ($(this).attr("isNumber") === "true") {
                var value = $(this).val();
                var d = value.split('.');
                if (typeof d[1] != "undefined" && d[1].length > 0) {
                    var c = AppendComma(value, d[1].length);
                    $(this).val(c);
                }
                else {
                    $(this).val(AppendComma(value));
                }
            }
        }

    });

    //將input有enterType=number者，只能輸入數字
    $(document).on("keydown", "input[isNumber='true']", function (e) {
        // Allow: backspace, delete, tab, escape, enter and .
        if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190, 189, 109]) !== -1 ||
            // Allow: Ctrl+A
            (e.keyCode == 65 && e.ctrlKey === true) ||
            // Allow: home, end, left, right, down, up
            (e.keyCode >= 35 && e.keyCode <= 40)) {
            // let it happen, don't do anything
            return;
        }
        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
            e.preventDefault();
        }
    });

});

function floatAdd(arg1, arg2) {
    if (typeof (arg1) == "string") {
        arg1 = arg1.replace(/[,'"]/g, "");
    }
    if (typeof (arg2) == "string") {
        arg2 = arg2.replace(/[,'"]/g, "");
    }
    var r1, r2, m;
    try { r1 = arg1.toString().split(".")[1].length } catch (e) { r1 = 0 }
    try { r2 = arg2.toString().split(".")[1].length } catch (e) { r2 = 0 }
    m = Math.pow(10, Math.max(r1, r2));
    return (arg1 * m + arg2 * m) / m;
}

function floatSub(arg1, arg2) {
    if (typeof (arg1) == "string") {
        arg1 = arg1.replace(/[,'"]/g, "");
    }
    if (typeof (arg2) == "string") {
        arg2 = arg2.replace(/[,'"]/g, "");
    }
    var r1, r2, m, n;
    try { r1 = arg1.toString().split(".")[1].length } catch (e) { r1 = 0 }
    try { r2 = arg2.toString().split(".")[1].length } catch (e) { r2 = 0 }
    m = Math.pow(10, Math.max(r1, r2));
    //动态控制精度长度    
    n = (r1 >= r2) ? r1 : r2;
    return ((arg1 * m - arg2 * m) / m).toFixed(n);
}

function floatMul(arg1, arg2) {
    if (typeof (arg1) == "string") {
        arg1 = arg1.replace(/[,'"]/g, "");
    }
    if (typeof (arg2) == "string") {
        arg2 = arg2.replace(/[,'"]/g, "");
    }
    var m = 0, s1 = arg1.toString(), s2 = arg2.toString();
    try { m += s1.split(".")[1].length } catch (e) { }
    try { m += s2.split(".")[1].length } catch (e) { }
    return Number(s1.replace(".", "")) * Number(s2.replace(".", "")) / Math.pow(10, m);
}

function floatDiv(arg1, arg2) {
    if (typeof (arg1) == "string") {
        arg1 = arg1.replace(/[,'"]/g, "");
    }
    if (typeof (arg2) == "string") {
        arg2 = arg2.replace(/[,'"]/g, "");
    }
    var t1 = 0, t2 = 0, r1, r2;
    try { t1 = arg1.toString().split(".")[1].length } catch (e) { }
    try { t2 = arg2.toString().split(".")[1].length } catch (e) { }

    r1 = Number(arg1.toString().replace(".", ""));

    r2 = Number(arg2.toString().replace(".", ""));
    return (r1 / r2) * Math.pow(10, t2 - t1);
}

//单位换算 by milo 20150610
function ConvertToInt(obj) {
    if (typeof (obj) == "string")
        obj = obj.replace(/[,'"]/g, "");
    if (isNaN(parseInt(obj)))
        return 0;
    else return parseInt(obj);
}

function ConvertToFloat(obj) {
    if (typeof (obj) == "string") {
        obj = obj.replace(/[,'"]/g, "");
    }
    //obj = obj.replace(",", "");
    if (isNaN(parseFloat(obj)))
        return 0;
    else return parseFloat(obj);
}


//单位换算 by milo 20150610
function unitConvert(oriunit, destunit) {
    oriunit = oriunit ? oriunit.toUpperCase() : oriunit;
    destunit = destunit ? destunit.toUpperCase() : destunit;
    //目标单位
    if (destunit == "G") {
        if (oriunit == "OZ")
            return 31.1035;
        else if (oriunit == "KGM" || oriunit == "KGS" || oriunit == "KG")
            return 1000;
        else if (oriunit == "TON" || oriunit == "MT" || oriunit == "DMT" || oriunit == "WMT")
            return 1000000;
        else if (oriunit == "LB")
            return 453.59237;

    } else if (destunit == "KGM" || destunit == "KGS" || destunit == "KG") {
        if (oriunit == "OZ")
            return 0.0311035;
        else if (oriunit == "TON" || oriunit == "MT" || oriunit == "DMT" || oriunit == "WMT")
            return 1000;
        else if (oriunit == "LB")
            return 0.4535924;
        else if (oriunit == "G" || oriunit == "G/DMT")
            return 0.001;
    } else if (destunit == "OZ") {
        if (oriunit == "KGM" || oriunit == "KGS" || oriunit == "KG")
            return 32.1507;
        else if (oriunit == "TON" || oriunit == "MT" || oriunit == "DMT" || oriunit == "WMT")
            return 32150.7226;
        else if (oriunit == "LB")
            return 14.5833;
        else if (oriunit == "G" || oriunit == "G/DMT")
            return 0.0321507;
    } else if (destunit == "TON" || destunit == "MT" || destunit == "DMT" || destunit == "WMT" || destunit == "T") {
        if (oriunit == "KGM" || oriunit == "KGS" || oriunit == "KG")
            return 0.001;
        else if (oriunit == "OZ")
            return 0.0000311035;
        else if (oriunit == "LB")
            return 0.0004536;
        else if (oriunit == "G" || oriunit == "G/DMT")
            return 0.000001;
    } else if (destunit == "LB") {
        if (oriunit == "KGM" || oriunit == "KGS" || oriunit == "KG")
            return 2.2046226;
        else if (oriunit == "OZ")
            return 0.06857;
        else if (oriunit == "TON" || oriunit == "MT" || oriunit == "DMT" || oriunit == "WMT")
            return 2204.6;
        else if (oriunit == "G" || oriunit == "G/DMT")
            return 0.0022046;
    }
    return 1;
}
//新增NOTICE_RECORD
function AddNotice(id, refNo, allFields, noticeDate, approveCode, boss, otherFields) {

    $.ajax({
        async: false,
        url: rootPath + "NOTICE/ChangeNotice",
        type: 'POST',
        data: { "allFields": JSON.stringify(allFields), autoReturnData: true, "id": id, "refNo": refNo, "noticeDate": noticeDate, "approveCode": approveCode, "otherFields": otherFields, "boss": boss },
        dataType: "json",
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {

            console.log(data);


        }
    });
}
// 对Date的扩展，将 Date 转化为指定格式的String
// 月(M)、日(d)、小时(h)、分(m)、秒(s)、季度(q) 可以用 1-2 个占位符， 
// 年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字) 
// 例子： 
// (new Date()).Format("yyyy-MM-dd hh:mm:ss.S") ==> 2006-07-02 08:09:04.423 
// (new Date()).Format("yyyy-M-d h:m:s.S")      ==> 2006-7-2 8:9:4.18 
Date.prototype.Format = function (fmt) { //author: meizz 
    var o = {
        "M+": this.getMonth() + 1, //月份 
        "d+": this.getDate(), //日 
        "h+": this.getHours(), //小时 
        "m+": this.getMinutes(), //分 
        "s+": this.getSeconds(), //秒 
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
        "S": this.getMilliseconds() //毫秒 
    };
    if (/(y+)/.test(fmt)) fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt)) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
}

//數字處理為有千分位
function AppendComma(n, pos) {
    if (typeof pos == "undefined") {
        pos = 0;
    }
    if (typeof (n) == "string" && n.indexOf(",") > -1)
        return n;
    //if (!/^[0-9]+(\.[0-9]+)?/.test(n))
    if (!/^(-|\+)?[0-9]+(\.[0-9]+)?/.test(n)) {
        var newValue = /^[0-9]+(\.[0-9]+)?/.exec(n);
        if (newValue != null) {
            if (parseFloat(newValue)) {
                n = newValue;
            }
            else {
                n = '0';
            }
        }
        else {
            n = '0';
        }
    }
    if (parseFloat(n) == 0) {
        n = '0';
    }
    else {
        n = parseFloat(n).toFixed(pos).toString();
    }

    n += '';
    var arr = n.split('.');
    var re = /(\d{1,3})(?=(\d{3})+$)/g;
    return arr[0].replace(re, '$1,') + (arr.length == 2 ? '.' + arr[1] : '');
}
//將有千分位的數值轉為一般數字
function RemoveComma(n) {
    return n.replace(/[,]+/g, '');
}
//調整千分位
function AdjustComma(item, length) {
    var originalValue = $.trim($(item).val()).length > length
        ? $.trim($(item).val()).substr(0, length)
        : $.trim($(item).val());

    $(item).val(AppendComma(originalValue));
}

function ippombaseCondition() {
    return " (GROUP_ID='" + groupId + "' AND CMP='" + cmp + "' AND STN='" + stn + "' OR AGENT_CD='" + cmp + stn + "')";
}
function ipctmbaseCondition() {
    return " (GROUP_ID='" + groupId + "' AND CMP='" + cmp + "' AND STN='" + stn + "' OR CONTACT_CD='" + cmp + stn + "')";
}
function ipbtmbaseCondition() {
    return " (GROUP_ID='" + groupId + "' AND CMP='" + cmp + "' AND STN='" + stn + "')";
}
function empidbaseCondition() {
    return " GROUP_ID='" + groupId + "' AND (CMP='" + cmp + "' AND STN='" + stn + "' OR EMP_ISSHARE ='Y')";
}
function setDisabledByName($obj, disabled) {
    if ($obj.is("select")) {
        if ($obj.attr("readonly") == "readonly") {
            $obj.prop("readonly", false);
            $obj.prop("disabled", true);
        }
        else {
            $obj.prop("disabled", disabled);
        }
    }
    else {
        $obj.prop("disabled", disabled);
    }
}

function getChangedCell(originData, data) {
    var dirty = {};
    for (var key in originData) {
        if (typeof data[key] != 'undefined' && originData[key] !== data[key]) {
            dirty[key] = data[key];
        }
    }
    return dirty;
}

/*
判断数组内是否包含element
create by milo 20150826
*/
Array.prototype.contains = function (element) {
    var i = 0,
        n = this.length;
    for (; i < n; i++) {
        if (this[i] === element)
            return true;
    }
    return false;
}

/*
判断某个数组内是否有重复
create by milo 20150826
*/
Array.prototype.duplicated = function () {
    return /(\x0f[^\x0f]+)\x0f[\s\S]*\1/.test("\x0f" + this.join("\x0f\x0f") + "\x0f");
}

/*
 判斷自訂日期格試
*/
function dateValidationCheck(str, reg) {

    var re = new RegExp(reg);
    var strDataValue;
    var infoValidation = true;
    if ((strDataValue = re.exec(str)) != null) {
        var i;
        i = parseFloat(strDataValue[1]);
        if (i <= 0 || i > 9999) { /*年*/
            infoValidation = false;
        }
        i = parseFloat(strDataValue[2]);
        if (i <= 0 || i > 12) { /*月*/
            infoValidation = false;
        }
    } else {
        infoValidation = false;
    }

    if (!infoValidation) {
        CommonFunc.Notify("", commmonlang.common_petdf, 1000, "warning");
    }
    return infoValidation;
}

function ajustamodal(myModal, w, h) {
    var altura = $(window).height() - 180; //value corresponding to the modal heading + footer
    var modalWidth = $(window).width() - 100;
    $(myModal + " .modal-body").css({ "height": altura, "overflow-y": "auto" });
    $(myModal + " .modal-lg").css({ "width": modalWidth });
}

//註冊 TEMPLATE有IF判斷的功能
Handlebars.registerHelper('ifCond', function (v1, v2, options) {
    if (v1 === v2) {
        return options.fn(this);
    }
    return options.inverse(this);
});

//註冊 TEMPLATE有IF判斷大於的功能
Handlebars.registerHelper('ifbig', function (v1, v2, options) {
    if (v1 > v2) {
        return options.fn(this);
    }
    return options.inverse(this);
});



$.fn.SearchStatus = function (options) {
    $SearchStatus = $(this);
    $SearchStatus.show();
    $PanelBody = $SearchStatus.find(".panel-body");
    var searchStatusWidth = $SearchStatus.width();


    $PanelBody.width("auto");

    var settings = $.extend({
        data: [],
        defaultId: "",
        thisTempId: "",
        postUrl: "",
        postData: {},
        loadComplete: function () { },
        beforeSelectFunc: function () { },
        afterSelectFunc: function () { }
    }, options);

    $.each(settings.data, function (index, val) {
        var actived = "";
        if (settings.defaultId == val["id"]) {
            actived = "active";
        }

        if (typeof val["amount"] == 'undefined') {
            val["amount"] = "";
        }

        var el = "<div class='status-box " + actived + "' id='searchStatus_" + val["id"] + "' statusid='" + val["id"] + "'>";
        el += "<p>" + val["label"];
        el += "(<span id='statusCount_" + val["id"] + "' class='statusCount' >" + val["amount"] + "</span>)</p>";
        el += "</div>";
        $PanelBody.append(el);
    });

    $SearchStatus.on("click", ".status-box", function () {
        var statusId = $(this).attr("statusid");
        $(this).addClass('active');
        $(this).siblings().removeClass('active');

        //以下做click後的事
        settings.beforeSelectFunc(statusId);
    });

    $SearchStatus.on("scroll", ".panel-body", function (event) {
        alert("aaa");
    });

    var SearchStatus = document.getElementById("SearchStatus_containerInfoGrid");
    if (SearchStatus.addEventListener) {
        // IE9, Chrome, Safari, Opera
        SearchStatus.addEventListener("mousewheel", MouseWheelHandler, false);
        // Firefox
        SearchStatus.addEventListener("DOMMouseScroll", MouseWheelHandler, false);
    }

    function MouseWheelHandler() {
        var e = window.event || e; // old IE support
        var delta = Math.max(-1, Math.min(1, (e.wheelDelta || -e.detail)));

        if (delta < 0) {
            $PanelBody.scrollLeft($PanelBody.scrollLeft() + 25);
        }
        else {
            $PanelBody.scrollLeft($PanelBody.scrollLeft() - 25);
        }
    }

    $(window).resize(function (event) {
        searchStatusWidth = $SearchStatus.width();
        $PanelBody.width("auto");
    });
}

$.fn.TrackingStatus = function (Cstatus) {
    /*
    
    var settings = $.extend({
        data: [],
        defaultId: "",
        thisTempId: ""
    }, options);

    
    
    
    var lastTracking = {};
    var dataLength = settings.data.length;
    
    if(dataLength > 0)
    {
        lastTracking = settings.data[dataLength - 1];
    }
    console.log(lastTracking);*/
    var status = [];
    $TrackingStatus = $(this);
    $PanelBody = $TrackingStatus.find(".panel-body");
    var TrackingStatusWidth = $TrackingStatus.width();

    $PanelBody.width(TrackingStatusWidth - 10);
    $PanelBody.html("");
    var statusArray = ["Pickup", "Receive", "Onboard", "Arrival", "Delivery"];
    var linkStr = '<div class="tracking-link"></div>';
    switch (Cstatus) {
        case 1:
            status = ["success", "danger", "danger", "danger", "danger"];
            break;
        case 2:
            status = ["success", "success", "danger", "danger", "danger"];
            break;
        case 3:
            status = ["success", "success", "success", "danger", "danger"];
            break;
        case 4:
            status = ["success", "success", "success", "success", "danger"];
            break;
        case 5:
            status = ["success", "success", "success", "success", "success"];
            break;
        default:
            status = ["success", "success", "success", "success", "success"];
            break;
    }

    for (var i = 0; i < 5; i++) {
        var el = "";

        if (i != 4) {
            el += '<div class="tracking-box ' + status[i] + '"><span class="status">' + statusArray[i] + '</span><br /><span class="datetime"></span><br /></div>' + linkStr;
        }
        else {
            el += '<div class="tracking-box ' + status[i] + '"><span class="status">' + statusArray[i] + '</span><br /><span class="datetime"></span><br /></div>'
        }
        $PanelBody.append(el);
    }
}


function genTrackingTable(rows) {
    var source = $("#trackingTemplate").html();
    data = { rowData: rows };
    template = Handlebars.compile(source);
    html = template(data);
    console.log(html);
    $("#TrackingTable").append(html);
}

//生成查询条件
function loadCondition(formId) {
    var param = $('#' + formId).serialize();
    return param.trim('&');
}
function convFormData2Json(formId) {

    var $form = $('#' + formId);
    var unindexed_array = $form.serializeArray();
    var indexed_array = {};

    $.map(unindexed_array, function (n, i) {
        if (typeof indexed_array[n['name']] != "undefiend")
            indexed_array[n['name']] = n['value'] + "," + indexed_array[n['name']];
        else
            indexed_array[n['name']] = n['value'];
    });

    var data = JSON.stringify(indexed_array);

    return data;
}

function checkInvalid() {
    var result = false;
    $("input[dt='mt']").each(function (index, item) {

        var invalid = $(this).attr("aria-invalid");
        if (invalid === "true") {

            alert(commmonlang.common_teddncttrof);
            item.focus();
            result = true;
            return true;
        }
    });
    return result;
}

function genUid(uuid) {
    return uuid.substr(0, 8) + "-" + uuid.substr(8, 4) + "-" + uuid.substr(12, 4) + "-" + uuid.substr(16, 4) + "-" + uuid.substr(20, 12)
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

function commonSetBscData(lookUp, Cd, Nm, pType, callBack) {
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
    options.registerBtn = $("#" + lookUp);
    options.focusItem = $("#" + Cd);
    options.param = "";
    options.baseCondition = " GROUP_ID='" + groupId + "' AND CMP='" + cmp + "' AND CD_TYPE='" + pType + "'";
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#" + Cd).val(map.Cd);

        if (Nm != "")
            $("#" + Nm).val(map.CdDescp);

        if (typeof callBack === "function") {
            callBack(map);
        }
    }

    options.lookUpConfig = LookUpConfig.BSCodeLookup;
    initLookUp(options);
}

function commonBscAuto(lookUp, Cd, Nm, pType, callBack) {
    CommonFunc.AutoComplete("#" + Cd, 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE=" + pType + "&CD=", "CD=showValue,CD,CD_DESCP", function (event, ui) {
        $(this).val(ui.item.returnValue.CD);

        if (Nm != "")
            $("#" + Nm).val(ui.item.returnValue.CD_DESCP);
        console.log(callBack);
        if (typeof callBack === "function") {
            callBack(ui.item.returnValue);
        }

        return false;
    });
}

function commonSetSmptyData(lookUp, Cd, Nm, pType, callBack) {
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetSmptyDataForLookup";
    options.registerBtn = $("#" + lookUp);
    options.focusItem = $("#" + Cd);
    options.param = "";
    options.baseCondition = " PARTY_TYPE LIKE '%" + pType + "%'";
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#" + Cd).val(map.PartyNo);

        if (Nm != "")
            $("#" + Nm).val(map.PartyName);

        if (typeof callBack === "function") {
            callBack(map);
        }
    }

    options.lookUpConfig = LookUpConfig.SmptyLookup;
    initLookUp(options);
}

function commonSmptyAuto(lookUp, Cd, Nm, pType, callBack) {
    CommonFunc.AutoComplete("#" + Cd, 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_TYPE~" + pType + "&PARTY_NO=", "PARTY_NO=showValue,PARTY_NO,PARTY_NAME", function (event, ui) {
        var map = ui.item.returnValue;
        $(this).val(ui.item.returnValue.PARTY_NO);

        if (Nm != "")
            $("#" + Nm).val(ui.item.returnValue.PARTY_NAME);

        if (typeof callBack === "function") {
            callBack(ui.item.returnValue);
        }
        return false;
    });
}

function setSTdisabled(disabled, isEdit) {
    $("button.btn-stBtn").not(".menuBar").each(function (i, val) {
        $(this).prop("disabled", disabled);
    });

    $("[fieldname][dt='st']").each(function (i, val) {
        if ($(this).is("select")) {
            if ($(this).attr("readonly") == "readonly") {
                $(this).prop("readonly", false);
                $(this).prop("disabled", true);
            }
            else {
                if (isEdit && disabled == false && $(this).attr("isKey") == "true") {
                    $(this).prop("disabled", true);
                } else
                    $(this).prop("disabled", disabled);
            }
        }
        else {
            if (isEdit && disabled == false && $(this).attr("isKey") == "true") {
                var btnObj = $(this).parent().find("button.btn-stBtn");
                if (btnObj.length == 1)
                    btnObj.prop("disabled", true);
                $(this).prop("disabled", true);
            } else
                $(this).prop("disabled", disabled);
        }
    });
}
function setNavTabActive(nid) {

    var isSetPage = top.topManager.operatePage(null, nid.id, "check", null);

    if (!isSetPage) {
        top.topManager.openPage({
            id: nid.id,
            reload: false,
            href: nid.href,
            title: nid.title,
            isClose: true,
            search: nid.search
        });
    } else {
        top.topManager.openPage({
            id: nid.id,
            reload: false,
            isClose: true
        });
    }
}

function pmsBtnCheck(btnId) {
    if (pmsList != "") {
        pmsList = pmsList + "|";
        if (pmsList.indexOf(btnId + "|") == -1) {
            $("#" + btnId).remove();
            return false;
        }
    }

    return true;
}

function formatCur(type, point, num) {
    switch (type) {
        case 0:
            num = Math.floor(num);
            break;
        case 1:
            num = Math.ceil(num);
            break;
        case 2:
            num = CommonFunc.formatFloatNoComma(num, point);
            break;
        default:
            num = Math.floor(num);
            break;
    }
}

function checkCtnNo(str) {
    if (str.length < 11)
        return false;
    var total_no = 0;
    var n = 0;
    for (var i = 0; i < 10; i++) {
        var s = str.substring(i, i + 1);
        if (s >= "0" && s <= "9") {
            n = getAscii(s) - getAscii("0");
            n = n * Math.pow(2, i);
        }
        else if (s >= "A" && s <= "Z") {
            switch (getAscii(s)) {
                case 65: n = 10; break;
                case 66: n = 12; break;
                case 67: n = 13; break;
                case 68: n = 14; break;
                case 69: n = 15; break;
                case 70: n = 16; break;
                case 71: n = 17; break;
                case 72: n = 18; break;
                case 73: n = 19; break;
                case 74: n = 20; break;
                case 75: n = 21; break;
                case 76: n = 23; break;
                case 77: n = 24; break;
                case 78: n = 25; break;
                case 79: n = 26; break;
                case 80: n = 27; break;
                case 81: n = 28; break;
                case 82: n = 29; break;
                case 83: n = 30; break;
                case 84: n = 31; break;
                case 85: n = 32; break;
                case 86: n = 34; break;
                case 87: n = 35; break;
                case 88: n = 36; break;
                case 89: n = 37; break;
                case 90: n = 38; break;
            }
            n = n * Math.pow(2, i);
        }
        else
            return false;
        total_no += n;
    }
    var m = total_no % 11;
    if (m == 10)
        m = 0;
    if (getAscii(str.substring(10, 11)) - getAscii("0") != m)
        return false;
    return true;
}
function getAscii(str) {
    return str.charCodeAt();
}
function getSiteHeight() {
    var realHeight = parent.document.body.clientHeight;
    if (realHeight <= 400) {
        realHeight = $(window).height();
    }
    return realHeight;
}

function getColModel(table, keys) {
    $.post(rootPath + 'GateManage/genColModel', { table: _tobase64String(table), name: _tobase64String(table), keys: 'U_ID' }, function (data, textStatus, xhr) {
        var content = $.parseJSON(data.Content);
        var colModel = [];
        //console.log(content);
        $.each(content, function (index, val) {
            var obj = {};
            //console.log(content[index].fieldType);
            if (content[index].fieldType == "uniqueidentifier") {
                obj.name = index;
                obj.index = index;
                obj.title = index;
                obj.width = 80;
                obj.align = "left";
                obj.sorttype = "string";
                obj.hidden = true;
            }

            if (content[index].fieldType == "string") {
                var width = content[index].maxLength + 60;
                if (width > 200) {
                    width = 200;
                }
                obj.name = index;
                obj.index = index;
                obj.title = index;
                obj.width = width;
                obj.align = "left";
                obj.sorttype = "string";
                obj.hidden = false;
            }

            if (content[index].fieldType == "date") {
                obj.name = index;
                obj.index = index;
                obj.title = index;
                obj.width = 100;
                obj.align = "left";
                obj.sorttype = "string";
                obj.formatter = "date";
                obj.formatoptions = { srcformat: "ISO8601Long", newformat: "Y-m-d" };
                obj.hidden = false;
            }

            if (content[index].fieldType == "datetime") {
                obj.name = index;
                obj.index = index;
                obj.title = index;
                obj.width = 120;
                obj.align = "left";
                obj.sorttype = "string";
                obj.formatter = "date";
                obj.formatoptions = { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" };
                obj.hidden = false;
            }

            if (content[index].fieldType == "decimal" || content[index].fieldType == "int") {
                var value = "0.";
                for (var i = 0; i < content[index].numericScale; i++) {
                    value += "0";
                }
                obj.name = index;
                obj.index = index;
                obj.title = index;
                obj.width = 120;
                obj.align = "right";
                obj.sorttype = "float";
                obj.formatter = "number";
                obj.formatoptions = {
                    decimalSeparator: ".",
                    thousandsSeparator: ",",
                    decimalPlaces: content[index].numericScale,
                    defaultValue: value
                };
                obj.hidden = false;
            }

            colModel.push(obj);
        });
        //console.log(content);
        var text = JSON.stringify(colModel);
        //console.log(text);
    }, "JSON");
}

var actFormatter = function (cellvalue, options, rawObject) {
    var val = "";
    if (cellvalue == null || cellvalue === undefined || cellvalue == 0)
        val = "";
    else {
        val = cellvalue.replace(/,/g, '，');
    }
    return val;
};


function genColModel(table, keys, prefix, colSetting) {

    var _dfr;
    _dfr = $.Deferred();


    $.when($.ajax({
        async: false,
        url: rootPath + 'Common/genColModel',
        type: 'POST',
        data: { table: _tobase64String(table), name:_tobase64String(table), keys: keys, prefix: prefix },
        dataType: "json"
    })
    ).then(function (data, textStatus, xhr) {
        var content = $.parseJSON(data.Content);
        var colModel = [];
        //console.log(content);
        $.each(content, function (index, val) {
            var obj = {};
            //console.log(content[index].fieldType);
            if (content[index].fieldType == "uniqueidentifier") {
                obj.name = index;
                obj.index = index;
                obj.title = content[index].langText;
                obj.width = 80;
                obj.order = 999;
                obj.align = "left";
                obj.sorttype = "string";
                obj.hidden = true;
            }

            if (content[index].fieldType == "string" || content[index].fieldType == "text" || content[index].fieldType == "ntext") {
                var width = content[index].maxLength + 60;
                if (width > 200) {
                    width = 200;
                }
                obj.name = index;
                obj.index = index;
                obj.title = content[index].langText;
                obj.width = width;
                obj.order = 999;
                obj.align = "left";
                obj.sorttype = "string";
                obj.hidden = false;
                if (index == "CombineInfo" || index == "CntrInfo" || index == "InvoiceInfo" || index == "PostBillDateInfo" || index == "SoTwoInfo" || index == "DnTwoInfo" || index == "OhscodeInfo" || index == "ExrernalInfo" || index == "SlipSheetInfo") {
                    obj.classes = "normal-white-space";
                    obj.formatter = actFormatter;
                }
            }

            if (content[index].fieldType == "date") {
                obj.name = index;
                obj.index = index;
                obj.title = content[index].langText;
                obj.width = 100;
                obj.order = 999;
                obj.align = "left";
                obj.sorttype = "string";
                obj.formatter = "date";
                obj.formatoptions = { srcformat: "ISO8601Long", newformat: "Y-m-d" };
                obj.hidden = false;
            }

            if (content[index].fieldType == "datetime") {
                obj.name = index;
                obj.index = index;
                obj.title = content[index].langText;
                obj.width = 120;
                obj.order = 999;
                obj.align = "left";
                obj.sorttype = "string";
                obj.formatter = "date";
                obj.formatoptions = { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" };
                obj.hidden = false;
            }

            if (content[index].fieldType == "decimal" || content[index].fieldType == "int") {
                var value = "0.";
                for (var i = 0; i < content[index].numericScale; i++) {
                    value += "0";
                }
                obj.name = index;
                obj.index = index;
                obj.title = content[index].langText;
                obj.width = 120;
                obj.order = 999;
                obj.align = "right";
                obj.sorttype = "float";
                obj.formatter = "number";
                if (content[index].fieldType == "int") {
                    if (index == "Year") {
                        obj.width = 60;
                        obj.formatoptions = {
                            decimalSeparator: null,
                            thousandsSeparator: null,
                            decimalPlaces: 0,
                            defaultValue: 0
                        };
                    }else if (index == "Priority") {
                        obj.formatoptions = {
                            decimalSeparator: ".",
                            thousandsSeparator: ",",
                            decimalPlaces: 0,
                            defaultValue: ""
                        };
                    } else {
                        obj.formatoptions = {
                            decimalSeparator: ".",
                            thousandsSeparator: ",",
                            decimalPlaces: 0,
                            defaultValue: 0
                        };
                    }
                } else {
                    if (index == "Year") {
                        obj.width = 60;
                        obj.formatoptions = {
                            decimalSeparator: null,
                            thousandsSeparator: null,
                            decimalPlaces: 0,
                            defaultValue: 0
                        };
                    } else {
                        obj.formatoptions = {
                            decimalSeparator: ".",
                            thousandsSeparator: ",",
                            decimalPlaces: content[index].numericScale,
                            defaultValue: value
                        };
                    }
                }

                obj.hidden = false;
            }

            $.each(colSetting, function (i, v) {
                if (colSetting[i].name == index) {
                    if (i == 0) {
                        //console.log(obj);
                        //console.log(colSetting[i]);
                    }
                    var setting = $.extend({}, obj, colSetting[i]);
                    obj = setting;
                    return false;
                }

            });


            colModel.push(obj);
        });
        colModel.sort(sort_by('order', false, parseInt));
        //var text = JSON.stringify(colModel);
        //console.log(text);
        return _dfr.resolve(colModel);
    });
    return _dfr.promise();

}

var sort_by = function (field, reverse, primer) {

    var key = primer ?
        function (x) { return primer(x[field]) } :
        function (x) { return x[field] };

    reverse = !reverse ? 1 : -1;

    return function (a, b) {
        return a = key(a), b = key(b), reverse * ((a > b) - (b > a));
    }
}


var _unitSelect = ":"
var _units = {};
//_units["20GP"] = "20GP";
//_units["40GP"] = "40GP";
//_units["40HQ"] = "40HQ";
//_units["CBM"] = "CBM";
//_units["SHT"] = "Shipment";
//_units["TRK"] = "Truck";
//_units["CTN"] = "CARTON";
//_units["CTR"] = "CONTAINER";
//_units["PLT"] = "PALLET";
//_units["WM"] = "Weight or Measurement";
//_units["AC"] = "At Cost";
//_units["KGS"] = "KILOGRAMS";
//_units["TNE"] = "TONNE (1000KG)";
//_units["BL"] = "BL";
//_units["DOC"] = "Document";
//_units["HAD"] = "Handling";
//_units["DAY"] = "Day";
//_units["HR"] = "Hour";
//_units["SET"] = "SET";
//_units["PCS"] = "Pieces";
//_units["PKG"] = "Packages ";
//_units["BLE"] = "Bill of Entry";
//_units["CW"] = "Charge Weight";
//_units["%"] = "%";
////_units["CNT"] = "櫃數";
//_units["DN"] = "DN";
//_units["DEC"] = "Declaration No.";
//_units["DECA"] = "Declaration No.";
//_units["HS"] = "HS Code Qty.";
//_units["ITEM"] = "Item";
//_units["PHK"] = "Per 100 KG";
//_units["INVO"] = "Invioce";
//_units["PLT"] = "PLT";

//for (var name in _units) {
//    if (_unitSelect.length > 0)
//        _unitSelect += ";";
//    _unitSelect += name + ":" + _units[name];
//}

function SetCntUnit() {
    var isok = $.ajax({
        async: false,
        url: rootPath + "Common/GetCntTypeData",
        type: 'POST',
        data: { uid: "" },
        "error": function(xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function(data) {
            if (data.message != "") {
                var cnttypes = data.message.split(';');
                $.each(cnttypes, function(idx, value) {
                    _units[value] = value;
                });
                _units["20GP"] = "20GP";
                _units["40GP"] = "40GP";
                _units["40HQ"] = "40HQ";
                _units["CBM"] = "CBM";
                _units["SHT"] = "Shipment";
                _units["TRK"] = "Truck";
                _units["CTN"] = "CARTON";
                _units["CTR"] = "CONTAINER";
                _units["PLT"] = "PALLET";
                _units["WM"] = "Weight or Measurement";
                _units["AC"] = "At Cost";
                _units["KGS"] = "KILOGRAMS";
                _units["TNE"] = "TONNE (1000KG)";
                _units["BL"] = "BL";
                _units["DOC"] = "Document";
                _units["HAD"] = "Handling";
                _units["DAY"] = "Day";
                _units["HR"] = "Hour";
                _units["SET"] = "SET";
                _units["PCS"] = "Pieces";
                _units["PKG"] = "Packages ";
                _units["BLE"] = "Bill of Entry";
                _units["CW"] = "Charge Weight";
                _units["%"] = "%";
                //_units["CNT"] = "櫃數";
                _units["DN"] = "DN";
                _units["DEC"] = "Declaration No.";
                _units["DECA"] = "DEC. No";
                _units["HS"] = "HS Code Qty.";
                _units["ITEM"] = "Item";
                _units["PHK"] = "Per 100 KG";
                _units["INVO"] = "Invioce";
                for (var name in _units) {
                    if (_unitSelect.length > 0)
                        _unitSelect += ";";
                    _unitSelect += name + ":" + _units[name];
                }
            }
        }
    });
}

function getNowFormatDate() {

    var date = new Date();

    var seperator1 = "-";

    var seperator2 = ":";

    var month = date.getMonth() + 1;

    var strDate = date.getDate();

    if (month >= 1 && month <= 9) {

        month = "0" + month;

    }

    if (strDate >= 0 && strDate <= 9) {

        strDate = "0" + strDate;

    }

    var currentdate = date.getFullYear() + seperator1 + month + seperator1 + strDate

            + " " + date.getHours() + seperator2 + date.getMinutes()

            + seperator2 + date.getSeconds();

    return currentdate;

}

var _tobase64String = function (str) {
    try {
        $.base64.utf8encode = true;
        var code = $.base64.btoa(str);
        if (code === "" || code == undefined || code == null)
            return str;
        return encodeURIComponent(code);
    }
    catch (e) {

    }
    return str;
}


function isValidDate(dateString) {
    // 定義日期格式為YYYY-MM-DD or YYYY/MM/DD
    const regex = /^(?:19|20)\d\d[-/](0[1-9]|1[0-2])[-/](0[1-9]|[12][0-9]|3[01])$/;

    if (!regex.test(dateString)) {
        return false; // 日期格式不正確
    }

    // 將日期字串拆分為年、月、日
    const [year, month, day] = dateString.split(/[-/]/).map(Number);

    // 檢查月份範圍
    if (month < 1 || month > 12) {
        return false;
    }

    // 檢查日的範圍
    if (day < 1 || day > 31) {
        return false;
    }

    // 檢查大小月
    const daysInMonth = new Date(year, month, 0).getDate();
    if (day > daysInMonth) {
        return false;
    }

    // 檢查閏年
    if (month === 2 && day === 29) {
        if (!((year % 4 === 0 && year % 100 !== 0) || (year % 400 === 0))) {
            return false; // 非閏年
        }
    }

    return true; // 符合所有條件
}

function isValidDateTime(dateTimeString) {
    // 定義日期時間格式為YYYY-MM-DD HH:mm 或 YYYY/MM/DD HH:mm
    const regex = /^(?:19|20)\d\d[-/](0[1-9]|1[0-2])[-/](0[1-9]|[12][0-9]|3[01]) (?:[01]\d|2[0-3]):[0-5]\d$/;

    if (!regex.test(dateTimeString)) {
        return false; // 日期時間格式不正確
    }

    // 將日期時間字串拆分為年、月、日、時、分
    const [datePart, timePart] = dateTimeString.split(" ");
    const [year, month, day] = datePart.split(/[-/]/).map(Number);
    const [hour, minute] = timePart.split(":").map(Number);

    // 檢查月份範圍
    if (month < 1 || month > 12) {
        return false;
    }

    // 檢查日的範圍
    if (day < 1 || day > 31) {
        return false;
    }

    // 檢查大小月
    const daysInMonth = new Date(year, month, 0).getDate();
    if (day > daysInMonth) {
        return false;
    }

    // 檢查閏年
    if (month === 2 && day === 29) {
        if (!((year % 4 === 0 && year % 100 !== 0) || (year % 400 === 0))) {
            return false; // 非閏年
        }
    }

    // 檢查時間範圍
    if (hour < 0 || hour > 23 || minute < 0 || minute > 59) {
        return false;
    }

    return true; // 符合所有條件
}

CommonFunc.stringToJson = function (obj) {
    if (obj == "" || obj == null || obj == undefined) return [];
    var val;
    try {
        return JSON.parse(decodeHtml(obj));
    }
    catch (e) {
        return [];
    }
}

function ConditionParam(params, Field, FieldValue, type) {
    params += "&";// params == "" ? "" : "&";
    if (type == "bt") {
        params += "sopt_" + Field + "=bt&" + Field + "$S=" + FieldValue[0] + "&" + Field + "$E=" + FieldValue;
    }
    else {
        params += "sopt_" + Field + "=" + type + "&" + Field + "=" + FieldValue;
    }
    return params;
}