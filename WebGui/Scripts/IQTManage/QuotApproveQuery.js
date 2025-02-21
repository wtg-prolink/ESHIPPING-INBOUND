var mygrid = $("#containerInfoGrid");
function btnGroup(gop) {
    gop.btnGroup = [{
        id: "btn02",
        name: _getLang("L_ActManage_Pass","通过"),
        func: function () {
            var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow'); 
            var responseData = []; 
            $.each(selRowId, function (index, val) {
                responseData.push(mygrid.getRowData(selRowId[index]));
            });
            if (responseData.length < 1) {
                CommonFunc.Notify("", _getLang("L_ActManage_Select1Data", "请至少选择一笔资料"), 500, "warning");
                return; 
            }

            var idList = "";
            for (var i = 0; i < responseData.length; i++) {
                idList += responseData[i].UId + ",";
                var approveto = responseData[i].ApproveTo;
                if (approveto == "Finish") {
                    CommonFunc.Notify("", "" + " " + _getLang("L_QuotApproveQuery_tip1", "签核已完成，请确认。"), 500, "warning");
                    return;
                }
            }
            var _confirm = confirm(_getLang("L_qtBase_Script_172", "confirm to approve or not"));
            if (!_confirm) {
                return;
            }
            
            $.ajax({
                async: true,
                url: rootPath + "IRQManage/InitiatedCheck",
                type: 'POST',
                data: { UId: idList },
                dataType: "json",
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.Notify("", _getLang("L_ActCheckSetup_Scripts_29","系统错误"), 500, "warning");
                    CommonFunc.ToogleLoading(false);
                },
                success: function (result) { 
                    CommonFunc.Notify("", result.message, 500, "success");
                    reloadStatus();
                }
            });

        }
    },
       {
           id: "btn09",
           name: _getLang("L_ActManage_Back","回上层"),
           func: function () {
               var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
               var TranMode = mygrid.jqGrid('getCell', selRowId, 'TranMode');
               var UId = mygrid.jqGrid('getCell', selRowId, 'UId');
               $("#BackRemark").val("");
               var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
               var uid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'UId');

               if (!uid) {
                   CommonFunc.Notify("", _getLang("L_TKBLQuery_Select","请先选择一笔记录"), 500, "success");
                   return;
               }

               $("#ApproveBack").modal("show");
           }
       },
       {
           id: "btn05",
           name: _getLang("L_ActManage_ApDetail","签核明细"),
           func: function () {
               CheckDetailed();
           }
       }];
}
function BackApprove() {
    var backremark = $("#BackRemark").val();
    if (backremark == "") {
        CommonFunc.Notify("", _getLang("L_ActManage_EnterReason","请输入退回原因"), 500, "warning");
        return;
    }
    var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
    var uid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'UId');
    var ApproveType = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ApproveType');
    var approveTo = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ApproveTo');

    if (!uid) {
        CommonFunc.Notify("", _getLang("L_TKBLQuery_Select","请先选择一笔记录"), 500, "warning");
        return;
    }
    $.ajax({
        async: true,
        url: rootPath + "IRQManage/ApproveBackQuot",
        type: 'POST',
        data: {
            "UId": uid,
            "ApproveType": ApproveType,
            "ApproveTo": approveTo,
            "BackRemark": backremark
        },
        "complete": function (xmlHttpRequest, successMsg) {

        },
        "error": function (xmlHttpRequest, errMsg) {
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
            $("#CloseBackWin").trigger("click");
        },
        success: function (result) {
            //var resJson = $.parseJSON(result)
            CommonFunc.Notify("", result.message, 500, "success");
            $("#CloseBackWin").trigger("click");
            //$("#SummarySearch").trigger("click");
            reloadStatus();
        }
    });
}
function reloadStatus() {
    $.ajax({
        async: true,
        url: rootPath + "QTManage/QTCheckQueryData",
        type: 'POST',
        data: {
            "resultType": "count",
            "statusField": "ApproveTo"//,
            //"baseCondition": " STATUS='D'" + tranTypeCondition
        },
        "complete": function (xmlHttpRequest, successMsg) {

        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            var resJson = $.parseJSON(result);
            $(".statusCount").each(function (index, el) {
                $(this).html(0);
            });
            for (var i = 0 ; i < resJson.rows.length ; i++) {
                $("#statusCount_" + resJson.rows[i].PoStatus).html(resJson.rows[i].Count);
            };
            $("#SummarySearch").click();
        }
    });
}
function CheckDetailed() {

    if ($("#ApproveDetail").length <= 0) {
        $("body").append(_showApproveModal);
    }

    if ($("#ApproveDetailTemplate").length <= 0) {
        $("body").append(_showApproveTemplate);
    }

    var $_grid = $("#containerInfoGrid");
    var $_templeat = $("#ApproveDetailTemplate");
    var $_DetailTable = $("#ApproveDetailTemplate");
    var $_Detail = $('#ApproveDetail');
    var selRowId = $_grid.jqGrid('getGridParam', 'selrow');
    var UId = $_grid.jqGrid('getCell', selRowId, 'UId');
    var Cmp = $_grid.jqGrid('getCell', selRowId, 'Cmp');
    if (!UId) {
        CommonFunc.Notify("", _getLang("L_TKBLQuery_Select","请先选择一笔记录"), 500, "warning");
        return;
    }

    $.ajax({
        async: true,
        url: rootPath + "ActManage/GetApproveInfo",
        type: 'POST',
        data: {
            "UId": UId,
            "Cmp": Cmp
        },
        "complete": function (xmlHttpRequest, successMsg) {
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            console.log(result);
            var resJson = $.parseJSON(result)
            var source = $_templeat.html();
            var data = {
                rowData: [
                    { "SeqNo": 1, "StatusFlag": "Y", "ApproveName": _getLang("L_SMIPR_CreateBy","申请者"), "Status": _getLang("L_QTSetup_StatusPass","审核通过"), "NotifyDate": "2015-10-30 15:00", "CreateBy": "Tim", "CreateDate": "2015-10-30 16:00", "ApproveTime": 60, "ActualTime": 60, "Remark": "xxx" }
                ]
            };
            //data = resJson;
            var newResJson = [];
            $.each(resJson, function (index, val) {
                var resJsonItem = val;
                if (resJsonItem.Status == "0") {
                    resJsonItem.Status = _getLang("L_ActManage_No","否");
                } else if (resJsonItem.Status == "1") {
                    resJsonItem.Status = _getLang("L_ApproveGroupSetup_Approved","批准");
                }
                newResJson.push(resJsonItem);
            });
            var data = { rowData: newResJson, maxNum: 1 };
            var template = Handlebars.compile(source);
            var html = template(data);
            $("#ApproveDetailTable").html(html);

            $('#ApproveDetail').modal('show'); //顯示彈出視窗
            ajustamodal("#ApproveDetail");
        }
    });
};

var _showApproveModal = '<div class="modal fade" id="ApproveDetail" Sid="">\
  <div class="modal-dialog modal-lg">\
    <div class="modal-content">\
      <div class="modal-header">\
        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
        <h4 class="modal-title">' + _getLang("L_ActManage_ApDetail", "签核明细") + '</h4>\
      </div>\
      <div class="modal-body" id="ApproveContent">\
        <table class="table table-bordered table-hover" id="ApproveDetailTable">\
        </table>\
      </div>\
      <div class="modal-footer">\
        <button type="button" class="btn btn-default" data-dismiss="modal" id="ModalClose">Close</button>\
      </div>\
    </div>\
  </div>\
</div>';

var _showApproveTemplate = '<script id="ApproveDetailTemplate" type="text/x-handlebars-template">\
    <thead>\
        <tr class="info">\
            <td>' + _getLang("L_DNApproveManage_ApproveType","审核类别") + '</td>\
            <td>' + _getLang("L_ActManage_AprOrder","签核顺序") + '</td>\
            <td>' + _getLang("L_ApproveGroupSetup_ApproveDep","签核部门") + '</td>\
            <td>' + _getLang("L_DNApproveManage_ApproveTo","签核状态") + '</td>\
            <td>' + _getLang("L_EventSetup_NotifyDate","通知时间") + '</td>\
            <td>' + _getLang("L_ActManage_Aprer","签核人") + '</td>\
            <td>' + _getLang("L_ActManage_AprTm","签核时间") + '</td>\
            <td>' + _getLang("L_ActManage_EstTm","预定秏时") + '</td>\
            <td>' + _getLang("L_ActManage_ActMd","实际秏时") + '</td>\
            <td>' + _getLang("L_ActManage_AprCont","签核内容") + '</td>\
            <td>' + _getLang("L_ActManage_ApprovePeople", "Next Approve Person") + '</td>\
        </tr>\
    </thead>\
    <tbody>\
        {{#each rowData}}\
            {{#ifbig this.ActualTime ../maxNum }}\
            <tr class="danger">\
            {{else}}\
            <tr>\
            {{/ifbig}}\
                <td>{{this.ApproveCodename}}</td>\
                <td>{{this.ApproveLevel}}</td>\
                <td>{{this.ApproveRole}}</td>\
                <td>{{this.Status}}</td>\
                <td>{{this.NotifyDate}}</td>\
                <td>{{this.ApproveBy}}</td>\
                <td>{{this.ApproveDate}}</td>\
                <td>{{this.ApproveTime}}</td>\
                <td>{{this.ActualTime}}</td>\
                <td>{{this.Remark}}</td>\
                <td>{{this.NoticeTo}}</td>\
            </tr>\
{{/each}}\
</tbody>\
</script>';


function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return id || caption;
}