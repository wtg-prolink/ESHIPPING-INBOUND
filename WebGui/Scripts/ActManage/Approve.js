function CheckDetailed() {

    if ($("#ApproveDetail").length <= 0) {
        $("body").append(_showApproveModal);
    }

    if ($("#ApproveDetailTemplate").length <= 0) {
        $("body").append(_showApproveTemplate);
    }

    var $_grid=$("#containerInfoGrid");
    var $_templeat=$("#ApproveDetailTemplate");
    var $_DetailTable=$("#ApproveDetailTemplate");
    var $_Detail=$('#ApproveDetail');
    var selRowId = $_grid.jqGrid('getGridParam', 'selrow');
    var UId = $_grid.jqGrid('getCell', selRowId, 'UId');
    var Cmp = $_grid.jqGrid('getCell', selRowId, 'Cmp');
    if (!UId) {
        CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
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
                    { "SeqNo": 1, "StatusFlag": "Y", "ApproveName": "@Resources.Locale.L_SMIPR_CreateBy", "Status": "@Resources.Locale.L_QTSetup_StatusPass", "NotifyDate": "2015-10-30 15:00", "CreateBy": "Tim", "CreateDate": "2015-10-30 16:00", "ApproveTime": 60, "ActualTime": 60, "Remark": "xxx" }
                ]
            };
            //data = resJson;
            var newResJson = [];
            $.each(resJson, function (index, val) {
                var resJsonItem = val;
                if (resJsonItem.Status == "0") {
                    resJsonItem.Status = '@Resources.Locale.L_ActManage_No';
                } else if (resJsonItem.Status == "1") {
                    resJsonItem.Status = 'Yes';
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

var _showApproveModal='<div class="modal fade" id="ApproveDetail" Sid="">\
  <div class="modal-dialog modal-lg">\
    <div class="modal-content">\
      <div class="modal-header">\
        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
        <h4 class="modal-title">'+'@Resources.Locale.L_ActManage_ApDetail'+'</h4>\
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

var _showApproveTemplate='<script id="ApproveDetailTemplate" type="text/x-handlebars-template">\
    <thead>\
        <tr class="info">\
            <td>' + '@Resources.Locale.L_DNApproveManage_ApproveType' + '</td>\
            <td>' + '@Resources.Locale.L_ActManage_AprOrder' + '</td>\
            <td>' + '@Resources.Locale.L_ApproveGroupSetup_ApproveDep' + '</td>\
            <td>' + '@Resources.Locale.L_DNApproveManage_ApproveTo' + '</td>\
            <td>' + '@Resources.Locale.L_EventSetup_NotifyDate' + '</td>\
            <td>' + '@Resources.Locale.L_ActManage_Aprer' + '</td>\
            <td>' + '@Resources.Locale.L_ActManage_AprTm' + '</td>\
            <td>' + '@Resources.Locale.L_ActManage_EstTm' + '</td>\
            <td>' + '@Resources.Locale.L_ActManage_ActMd' + '</td>\
            <td>' + '@Resources.Locale.L_ActManage_AprCont' + '</td>\
            <td>' + 'Next Approve User' + '</td>\
            <td>' + '@Resources.Locale.L_ActDeatilManage_Views_70' + '</td>\
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
                <td>{{this.NotifyDateL}}</td>\
                <td>{{this.ApproveBy}}</td>\
                <td>{{this.ApproveDateL}}</td>\
                <td>{{this.ApproveTime}}</td>\
                <td>{{this.ActualTime}}</td>\
                <td>{{this.Remark}}</td>\
                <td>{{this.NoticeTo}}</td>\
                <td>{{this.ApproveRemark}}</td>\
            </tr>\
{{/each}}\
</tbody>\
</script>';

var altura = $(window).height() - 180; //value corresponding to the modal heading + footer
var modalWidth = $(window).width() - 100;

var _showCargoModal = '<div class="modal fade" id="TKDetail" Sid="">\
<div class="modal-dialog modal-lg" style="width:'+modalWidth+'px">\
    <div class="modal-content">\
     <div class="modal-header">\
        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
        <h4 class="modal-title">' + '@GetLangText("L_TKBLQuery_StsDetInf")' + '</h4>\
        </div>\
      <div class="modal-body" id="TKContent" style="overflow-y:auto; height:'+altura+'px">\
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
            <td>Event Date</td>\
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


function CheckCargoDetailed() {
    if ($("#TKDetail").length <= 0) {
        $("body").append(_showCargoModal);
    }

    if ($("#trackingTemplate").length <= 0) {
        $("body").append(_showCargoTemplte);
    }
    var id = $("#containerInfoGrid").jqGrid('getGridParam', "selrow");
    var map = $("#containerInfoGrid").jqGrid('getRowData', id);
    var ShipmentId;
    if (map) ShipmentId = map.ShipmentId;

    if (isEmpty(ShipmentId)) {
        CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
        return false;
    }

    getData(rootPath + "BookingAction/GetStatus", { ShipmentId: ShipmentId }, function (result) {
        var source = $("#trackingTemplate").html();
        var cols = ["StsCd", "StsDescp", "EvenDate", "Location", "EvenTmg", "Remark", "CreateBy", "CreateDate"];
        var data = { rowData: result };
        var template = Handlebars.compile(source);
        var html = template(data);
        $("#TrackingTable").html(html);

        $('#TKDetail').modal('show');
    });
}

function isEmpty(val) {
    if (val === undefined || val === "" || val == null)
        return true;
    return false;
}

function getData(url, data, callBackFn) {
    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: true,
        url: url,
        type: 'POST',
        data: data,
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            console.log(result);
            var resJson = $.parseJSON(result);
            callBackFn(resJson);
        }
    });
}