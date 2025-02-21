function btnGroup() {
    MenuBarFuncArr.AddMenu("btn03", "glyphicon glyphicon-log-out", _getLang("L_ActManage_Pass", ""), function () {
        var TranMode = $("#TranMode").val();
        var UId = $("#UId").val();
        
        if (UId.length <= 0) {
            CommonFunc.Notify("", _getLang("L_ActManage_Select1Data", ""), 500, "warning");
            return;
        }
        var _confirm = confirm(_getLang("L_qtBase_Script_172", "confirm to approve or not")); 
        if (!_confirm) {
            return;
        }
        //Checkstatus();
        if (UId) {
            $.ajax({
                async: true,
                url: rootPath + "IRQManage/InitiatedCheck",
                type: 'POST',
                data: { UId: UId, TranMode: TranMode },
                dataType: "json",
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.Notify("", _getLang("L_ActCheckSetup_Scripts_29", ""), 500, "warning");
                    CommonFunc.ToogleLoading(false);
                },
                success: function (result) {
                    //if (!result.flag)
                    MenuBarFuncArr.MBCancel();
                    CommonFunc.Notify("", result.message, 500, "success");
                }
            });
        }
    });
    MenuBarFuncArr.AddMenu("btn04", "glyphicon glyphicon-log-out", _getLang("L_ActManage_Back", ""), function () {
        $("#BackRemark").val("");
        var uid = $("#UId").val();
        if (!uid) {
            CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", ""), 500, "warning"); 
            return;
        }
        $("#ApproveBack").modal("show");
    });
    MenuBarFuncArr.AddMenu("btn06", "glyphicon glyphicon-log-out", _getLang("L_ActManage_ApDetail", ""), function () {
        CheckDetailed();
    });
}
function BackApprove() {
    var backremark = $("#BackRemark").val();
    if (backremark == "") {
        CommonFunc.Notify("", _getLang("L_ActManage_EnterReason", ""), 500, "warning"); 
        return;
    }
    var uid = $("#UId").val();
    if (!uid) {
        CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", ""), 500, "warning"); 
        return;
    }
    $.ajax({
        async: true,
        url: rootPath + "IRQManage/ApproveBackQuot",
        type: 'POST',
        data: {
            "UId": uid,
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
            CommonFunc.Notify("", result.message, 500, "success");
            $("#CloseBackWin").trigger("click");
        }
    });
}
function Checkstatus() {
    var QuotType = $("#QuotType").val();
    var returnMsg = "";
    switch (QuotType) {
        case "P": returnMsg = _getLang("L_BillApproveHelper_Business_2", ""); break; 
        case "V": returnMsg = _getLang("L_BillApproveHelper_Business_3", ""); break; 
        case "A": returnMsg = _getLang("L_BillApproveHelper_Business_35", ""); break; 
    }
    if (returnMsg != "") {
        CommonFunc.Notify("", returnMsg, 500, "warning");
        return false;
    }
}
function CheckDetailed() {

    if ($("#ApproveDetail").length <= 0) {
        $("body").append(_showApproveModal);
    }

    if ($("#ApproveDetailTemplate").length <= 0) {
        $("body").append(_showApproveTemplate);
    }
    var $_templeat = $("#ApproveDetailTemplate");
    var $_DetailTable = $("#ApproveDetailTemplate");
    var $_Detail = $('#ApproveDetail');
    var UId = $("#UId").val();
    var Cmp = $("#Cmp").val();
    if (!UId) {
        CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", ""), 500, "warning"); 
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
                    { "SeqNo": 1, "StatusFlag": "Y", "ApproveName": _getLang("L_SMIPR_CreateBy", ""), "Status": _getLang("L_QTSetup_StatusPass", ""), "NotifyDate": "2015-10-30 15:00", "CreateBy": "Tim", "CreateDate": "2015-10-30 16:00", "ApproveTime": 60, "ActualTime": 60, "Remark": "xxx" }
                ]
            };
            //data = resJson;
            var newResJson = [];
            $.each(resJson, function (index, val) {
                var resJsonItem = val;
                if (resJsonItem.Status == "0") {
                    resJsonItem.Status = _getLang("L_ActManage_No", "");
                } else if (resJsonItem.Status == "1") {
                    resJsonItem.Status = _getLang("L_BookingQuery_Views_288", ""); 
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
        <h4 class="modal-title">' + _getLang("L_ActManage_ApDetail", "") + '</h4>\
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
            <td>' + _getLang("L_DNApproveManage_ApproveType","") + '</td>\
            <td>' + _getLang("L_ActManage_AprOrder", "") + '</td>\
                <td>' + _getLang("L_ApproveGroupSetup_ApproveDep", "") + '</td>\
            <td>' + _getLang("L_DNApproveManage_ApproveTo", "") + '</td>\
            <td>' + _getLang("L_EventSetup_NotifyDate", "") + '</td>\
            <td>' + _getLang("L_ActManage_Aprer", "") + '</td>\
            <td>' + _getLang("L_ActManage_AprTm", "") + '</td>\
            <td>' + _getLang("L_ActManage_EstTm", "") + '</td>\
            <td>' + _getLang("L_ActManage_ActMd", "") + '</td>\
            <td>' + _getLang("L_ActManage_AprCont", "") + '</td>\
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

var altura = $(window).height() - 180; //value corresponding to the modal heading + footer
var modalWidth = $(window).width() - 100;

var _showCargoModal = '<div class="modal fade" id="TKDetail" Sid="">\
<div class="modal-dialog modal-lg" style="width:'+ modalWidth + 'px">\
    <div class="modal-content">\
     <div class="modal-header">\
        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
        <h4 class="modal-title">' + _getLang("L_TKBLQuery_StsDetInf", "") + '</h4>\
        </div>\
      <div class="modal-body" id="TKContent" style="overflow-y:auto; height:'+ altura + 'px">\
        <table class="table table-bordered table-hover" id="TrackingTable">\
        </table>\
      </div>\
      <div class="modal-footer">\
        <button type="button" class="btn btn-default" data-dismiss="modal" id="ModalClose">Close</button>\
      </div>\
      </div>\
  </div>\
</div>';

var _showCargoTemplte = '<script id="trackingTemplate" type="text/x-handlebars-template">\
    <thead>\
        <tr class="info">\
            <td>Status Code</td>\
            <td>Description</td>\
            <td>Even Date</td>\
            <td>Location</td>\
            <td>Remark</td>\
            <td>GMT</td>\
            <td>Sender</td>\
            <td>Sender Time</td>\
        </tr>\
    </thead>\
    <tbody>\
        {{#each rowData}}\
            <tr>\
                <td>{{this.StsCd}}</td>\
                <td>{{this.StsDescp}}</td>\
                <td>{{this.EvenDate}}</td>\
                <td>{{this.Location}}</td>\
                <td>{{this.Remark}}</td>\
                <td>{{this.EvenTmg}}</td>\
                <td>{{this.CreateBy}}</td>\
                <td>{{this.CreateDate}}</td>\
            </tr>\
{{/each}}\
</tbody>\
</script>';
