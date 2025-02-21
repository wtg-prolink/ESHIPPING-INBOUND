var MaxSeqNo = "";
var $tableErr = "";
var postErrUrl = "";
var errExpType = "";


function initErrMsg(registerBtn, keyData, autoOpen,containerTable)
{
    var UId = "";
    var JobNo = "";
    var myId = "";
    var fieldName = "";

    CommonFunc.consoleMsg("Init ErrMsg start");
    
    UId = keyData.UId;
    JobNo = keyData.JobNo;
    fieldName = "ShipmentId";
    if (keyData.hasOwnProperty("fieldName")) {
        fieldName = KeyData.fieldName;
    }
    postErrUrl = rootPath + "ERRMSG/getErrMsgData";
	myId = registerBtn.attr("id");
	registerBtn.attr("data-target","ErrMsgDialog_" + myId);
	//$("#ErrMsgDialog_" + myId).empty();
	//init SearchTemplate
	var data = {'initID': myId,"initJobNo":JobNo};
	var source = $("#ErrMsgTemplate").html();
	var template = Handlebars.compile(source);
	var html = template(data);
	$("body").append(html);

	registerBtn.unbind("click").click(function (event) {
	    $("#ErrMsgAdd").prop("disabled", false);
	    $("#ErrMsgSave").prop("disabled", true);
	    
	    $("#ErrMsgDialog_" + myId).modal({
	        backdrop: 'static',
	        keyboard: false
	    });
	    ajustamodal("#ErrMsgDialog_" + myId);
	});

	$tableErr = $("#ErrMsgTable_" + myId);
	$("#ErrMsgDialog_" + myId).on("show.bs.modal", function () {
	    var selRowId = containerTable.jqGrid('getGridParam', 'selrow');
	    var uid = containerTable.jqGrid('getCell', selRowId, 'UId');
	    var shipmentid = containerTable.jqGrid('getCell', selRowId, fieldName);
	    if (!uid) {
	        CommonFunc.Notify("", "请先选择一笔记录", 500, "warning");
	        return;
	    }
	    UId = uid;
	    JobNo = shipmentid;
	    $("#ErrorJobNo").html(shipmentid);
	    genErrMsgTable($tableErr, postErrUrl, uid, shipmentid);
	    //genErrMsgTable($tableErr, postErrUrl, UId, JobNo);
	});

    $("#ErrMsgAdd").unbind("click").click(function(event) {
        $(this).prop("disabled", true);
        $("#ErrMsgSave").prop("disabled", false);
        if (MaxSeqNo == null || MaxSeqNo == "") {
            MaxSeqNo = 1;
        }
        var str = '<tr >\
                <td id="SeqNo">' + MaxSeqNo + '</td>\
                <td width="150">\
                    <div class="input-group">\
                        <input type="text" class="form-control input-sm" dt="mt" id="ExpObj" name="ExpObj" fieldname="ExpObj"/>\
                        <span class="input-group-btn">\
                            <button class="btn btn-sm btn-default" type="button" id="ExpObjLookup">\
                                <span class="glyphicon glyphicon-search"></span>\
                            </button>\
                        </span>\
                    </div>\
                    <div>\
                        <input type="hidden" dt="mt" class="form-control input-sm" id="ExpCd" name="ExpCd" fieldname="ExpCd" hidden="true"/>\
                    </div>\
                </td>\
                <td><select class="form-control input-sm" name="ExpType" id="ExpType"></select></td>\
                <td><select class="form-control input-sm" name="ExpReason" id="ExpReason"></select></td>\
                <td><input class="form-control input-sm" name="ExpText" /></td>\
                <td id="WrId"></td>\
                <td id="WrDate"></td>\
                <td></td>\
                <td></td>\
                <td></td>\
                </tr>';
        $("#ErrMsgTable_" + myId).find("tbody").append(str);
        getExpTypeSelect();
        getExpObj()
    });

    $("#ErrMsgSave").unbind("click").click(function(event) {
        $("#ErrMsgAdd").prop("disabled", false);
        var ExpObj = $("input[name='ExpObj']").val();
        var ExpCd = $("input[name='ExpCd']").val();
        var ExpType = $("select[name='ExpType']").val();
        var ExpReason = $("#ExpReason").val();
        var ExpText = $("input[name='ExpText']").val();       
        var Dep = $("#Dep").val();

        var selRowId = containerTable.jqGrid('getGridParam', 'selrow');
        var uid = containerTable.jqGrid('getCell', selRowId, 'UId');
        var shipmentid = containerTable.jqGrid('getCell', selRowId, fieldName);
        if (!uid) {
            CommonFunc.Notify("", "请先选择一笔记录", 500, "warning");
            return;
        }
        JobNo = shipmentid;
        UId = uid;
        $.ajax({
            async: true,
            url: rootPath + "ERRMSG/InsrtErrMsg",
            type: 'POST',
            data: {
                UFid: UId,
                JobNo: JobNo,
                ExpObj: ExpObj,
                ExpCd:ExpCd,
                ExpType: ExpType,
                ExpReason: ExpReason,
                ExpText: ExpText,
                SeqNo: MaxSeqNo,
                Dep: Dep
            },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                if (result.message == "success") {
                    genErrMsgTable($tableErr, postErrUrl, UId, JobNo);
                    $("#ErrMsgSave").prop("disabled", true);
                    CommonFunc.Notify("", "新增異常訊息成功", 500, "success");
                }
                else {
                    CommonFunc.Notify("", "写入数据库有误", 500, "warning");
                }

            }
        });
    });

    if(autoOpen === true)
    {
        $("#ErrMsgDialog_" + myId).modal("show");
    }
}

function getExpObj() {
    var options = {};
    options.gridUrl = rootPath + "ERRMSG/ErrGetSmptyData";
    options.param = "";
    options.registerBtn = $("#ExpObjLookup");
    options.focusItem = $("#ExpObj");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#ExpObj").val(map.PartyName);
        $("#ExpCd").val(map.PartyNo);
    }

    options.lookUpConfig = LookUpConfig.SmptyLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#ExpObj", 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO=", "PARTY_NAME=showValue,PARTY_NO,PARTY_NAME", function (event, ui) {
        $(this).val(ui.item.returnValue.PARTY_NAME);
        $("#ExpCd").val(ui.item.returnValue.PARTY_NO);
        return false;
    });
}

function genErrMsgTable($tableErr, postErrUrl, UId, JobNo)
{
    console.log($tableErr);
    console.log(UId + "#" + JobNo);
    $tableErr.find("tbody").html("");
    $.ajax({
        async: true,
        url: postErrUrl,
        type: 'POST',
        data: {
            UId: UId,
            JobNo: JobNo
        },
        dataType: "json",
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            CommonFunc.consoleMsg(result);
            var str = "";
            $.each(result.returnData, function(i, val) {
                var ButtonDisabled = "";
                if (result.returnData[i].CancelBy == null || result.returnData[i].CancelBy == "") {
                    ButtonDisabled = '<button class="btn btn-sm btn-success iErrMsg " SeqNo="' + result.returnData[i].SeqNo + '"UFid="' + UId + '" JobNo="' + JobNo + '" ' + ButtonDisabled + '>解除</button>';
                }
                if (result.returnData[i].CancelBy == null) {
                    result.returnData[i].CancelBy = "";
                    result.returnData[i].CancelDate = "";
                }
                if (result.returnData[i].IsRelieve == "N" || result.returnData[i].IsRelieve == null) {
                    ButtonDisabled = "";
                    //var seqno = result.returnData[i].SeqNo;
                    //$("button[seqno='" + seqno + "']").prop("disabled", true);
                    //alert("OK");
                }
                str += '<tr >\
                <td>' + result.returnData[i].SeqNo + '</td>\
                <td>' + result.returnData[i].ExpObj + '</td>\
			    <td>' + result.returnData[i].CdDescp + '</td>\
                <td>' + result.returnData[i].ExpDescp1 + '</td>\
			    <td>' + result.returnData[i].ExpText + '</td>\
			    <td>' + result.returnData[i].WrId + '</td>\
			    <td>' + result.returnData[i].WrDate + '</td>\
			    <td>'+ result.returnData[i].CancelBy + '</td>\
                <td>' + result.returnData[i].CancelDate + '</td>\
                <td> ' + ButtonDisabled + '</td>\
			    </tr>';
                var no = parseInt(result.returnData[i].SeqNo);
                if (no>=0)
                    MaxSeqNo = no + 1;
                else
                    MaxSeqNo = parseInt(result.returnData.length) + 1;
            });
            $tableErr.find("tbody").html(str);
        }
    });
}



function getExpTypeSelect() {
    
    
    $.ajax({
        async: true,
        url: rootPath + "ERRMSG/GetSelectOptions",
        type: 'POST',
        //data:Options,
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (data) {
            var exOptions = data.Ex;
            //var exOptions2 = data.Ex2;
            var _shownull = { cd: '', cdDescp: '' };
            exOptions.unshift(_shownull);
            appendSelectOption($("#ExpType"), exOptions);
            //appendSelectOption($("#ExpReason"), exOptions2);
        }
    });
}

function getExpReasonSelect(val) {


    $.ajax({
        async: true,
        url: rootPath + "ERRMSG/GetErrReasonOptions",
        type: 'POST',
        data: {ExpType: val},
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (data) {           
            var exOptions2 = data.Ex2;           
            appendSelectOption($("#ExpReason"), exOptions2);
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

jQuery(document).ready(function($) {
    
    $("body").on("click", "button.iErrMsg", function(){

        var SeqNo = $(this).attr("SeqNo");
        var UFid = $(this).attr("UFid");
        var JobNo = $(this).attr("JobNo");
        var iErrMsgUrl = rootPath + "ERRMSG/iErrMsg";
        var postData = {
            'SeqNo': SeqNo,
            'UFid': UFid,
            'JobNo': JobNo
        };
        CommonFunc.consoleMsg(postData);
        $.ajax({
            async: true,
            url: iErrMsgUrl,
            type: 'POST',
            data: postData,
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                if (result.message == "success") {
                    genErrMsgTable($tableErr, postErrUrl, UFid, JobNo);
                    CommonFunc.Notify("", "解除成功", 500, "success");
                }
                else {
                    CommonFunc.Notify("", "写入数据库有误", 500, "warning");
                }
            }
        });
    });

    $("body").on("change", "#ExpType", function () {
        //var SeqNo = $(this).attr("SeqNo");
        var Options = $(this).val();
        //alert(Options);
        getExpReasonSelect(Options);
    });
});


function initErrMsgForSummary(registerBtn, UId, JobNo)
{
    MaxSeqNo = 0;
    var myId = "";
    console.log(UId + "#" + JobNo);
    postErrUrl = rootPath + "ERRMSG/getErrMsgData";
    myId = registerBtn.attr("id");
    registerBtn.attr("data-target","ErrMsgDialog_" + myId);
   

    if ($("#ErrMsgDialog_" + myId).length == 0) {
        //init SearchTemplate
        var data = { 'initID': myId, "initJobNo": JobNo };
        var source = $("#ErrMsgTemplate").html();
        var template = Handlebars.compile(source);
        var html = template(data);
        $("body").append(html);
        ajustamodal("#ErrMsgDialog_" + myId);
    } else {
        $("#ErrorJobNo").html(JobNo);
    }


    $tableErr = $("#ErrMsgTable_" + myId);
    $("#ErrMsgDialog_" + myId).unbind('show.bs.modal');
    $("#ErrMsgDialog_" + myId).on("show.bs.modal", function () {
        genErrMsgTable($tableErr, postErrUrl, UId, JobNo);
    });

    $("#ErrMsgDialog_" + myId).unbind('hide.bs.modal');
    $("#ErrMsgDialog_" + myId).on("hide.bs.modal", function () {
        $("#ErrMsgAdd").prop("disabled", false);
        $("#ErrMsgSave").prop("disabled", true);
    });

    $("#ErrMsgAdd").unbind("click").click(function(event) {
        $(this).prop("disabled", true);
        $("#ErrMsgSave").prop("disabled", false);
        if (MaxSeqNo == null || MaxSeqNo == "") {
            MaxSeqNo = 1;
        }
        var str = '<tr >\
                <td id="SeqNo">' + MaxSeqNo + '</td>\
                <td width="150">\
                    <div class="input-group">\
                        <input type="text" class="form-control input-sm" dt="mt" id="ExpObj" name="ExpObj" fieldname="ExpObj"/>\
                        <span class="input-group-btn">\
                            <button class="btn btn-sm btn-default" type="button" id="ExpObjLookup">\
                                <span class="glyphicon glyphicon-search"></span>\
                            </button>\
                        </span>\
                    </div>\
                    <div>\
                        <input type="hidden" dt="mt" class="form-control input-sm" id="ExpCd" name="ExpCd" fieldname="ExpCd"/>\
                    </div>\
                </td>\
                <td><select class="form-control input-sm" name="ExpType" id="ExpType"></select></td>\
                <td><select class="form-control input-sm" name="ExpReason" id="ExpReason"></select></td>\
                <td><input class="form-control input-sm" name="ExpText" /></td>\
                <td id="WrId"></td>\
                <td id="WrDate"></td>\
                <td></td>\
                <td></td>\
                <td></td>\
                </tr>';
        $("#ErrMsgTable_" + myId).find("tbody").append(str);
        getExpTypeSelect();
        getExpObj()
    });

    $("#ErrMsgSave").unbind("click").click(function(event) {
        $("#ErrMsgAdd").prop("disabled", false);
        var ExpObj = $("input[name='ExpObj']").val();
        var ExpCd = $("input[name='ExpCd']").val();
        var ExpType = $("select[name='ExpType']").val();
        var ExpReason = $("#ExpReason").val();
        var ExpText = $("input[name='ExpText']").val();       
        var Dep = $("#Dep").val();
        $.ajax({
            async: true,
            url: rootPath + "ERRMSG/InsrtErrMsg",
            type: 'POST',
            data: {
                UFid: UId,
                JobNo: JobNo,
                ExpObj: ExpObj,
                ExpCd:ExpCd,
                ExpType: ExpType,
                ExpReason: ExpReason,
                ExpText: ExpText,
                SeqNo: MaxSeqNo,
                Dep: Dep
            },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                if (result.message == "success") {
                    genErrMsgTable($tableErr, postErrUrl, UId, JobNo);
                    $("#ErrMsgSave").prop("disabled", true);
                    CommonFunc.Notify("", "新增異常訊息成功", 500, "success");
                }
                else {
                    CommonFunc.Notify("", "写入数据库有误", 500, "warning");
                }

            }
        });
    });
    $("#ErrMsgSave").prop("disabled", true);
    $("#ErrMsgDialog_" + myId).modal("show");
}