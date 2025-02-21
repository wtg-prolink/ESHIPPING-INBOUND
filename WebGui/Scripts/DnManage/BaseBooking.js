//物流费用所需事件
$("#ShipFeeQueryView").click(function(event) {
    var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
    var ShipmentId = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ShipmentId');
    if (!ShipmentId) {
        CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
        return;
    }
    top.topManager.openPage({
        href: rootPath + "QtManage/ShipFeeQueryView?ShipmentId=" + ShipmentId,
        title: '@Resources.Locale.L_ActManage_LspChg',
        id: 'ShipFeeQueryView',
        reload: true
    });
});
var ShipFeeColModel = [
    { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: false, hidden: true },
    { name: 'RfqNo', title: '@Resources.Locale.L_ActManage_IqNo', index: 'RfqNo', sorttype: 'string', width: 100, editable: true, hidden: false },
    { name: 'QuotNo', title: '@Resources.Locale.L_QTQuery_QuotNo', index: 'QuotNo', sorttype: 'string', width: 100, editable: true, hidden: false },
    { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', sorttype: 'string', width: 100, editable: true, hidden: false },
    { name: 'BlNo', title: '@Resources.Locale.L_ChgApproveManage_Views_190', index: 'BlNo', sorttype: 'string', width: 100, editable: true, hidden: false },
    { name: 'DebitNo', title: '@Resources.Locale.L_ActQuery_InvNo', index: 'DebitNo', width: 120, align: 'left', sorttype: 'string', hidden: false },
    { name: 'DebitDate', title: '@Resources.Locale.L_ActQuery_DebitDate', index: 'DebitDate', width: 120, align: 'left', sorttype: 'string', hidden: false }, 
    { name: 'LspNo', title: '@Resources.Locale.L_DRule_LspNo', index: 'LspNo', sorttype: 'string', width: 120, editable: true, hidden: false},
    { name: 'LspNm', title: '@Resources.Locale.L_AirQuery_LspNm', index: 'LspNm', sorttype: 'string', width: 120, editable: true, hidden: false },
    { name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', sorttype: 'string', width: 80, editable: true, hidden: false},
    { name: 'ChgDescp', title: '@Resources.Locale.L_SMCHGSetup_ChgDescp', index: 'ChgDescp', sorttype: 'string', width: 120, editable: true, hidden: false },
    { name: 'ChgType', title: '@Resources.Locale.L_SMCHGSetup_ChgRepay', index: 'ChgType', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: 'F:F.Freight Charge;O:O.Original Fee;D:D.Destination Fee' }, edittype: 'select' },
    { name: 'Qcur', title: '@Resources.Locale.L_BaseLookup_WithholdCur', index: 'Qcur', sorttype: 'string', width: 80, hidden: false, editable: true },
    { name: 'QchgUnit', title: '@Resources.Locale.L_BaseLookup_Unit', index: 'QchgUnit', sorttype: 'string', width: 80, hidden: false, editable: true },
    {
        name: 'QunitPrice', title: '@Resources.Locale.L_BaseLookup_WithholdPrice', index: 'QunitPrice',
        width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'float',
        hidden: false
   , editable: true },
    { name: 'Qqty', title: '@Resources.Locale.L_BaseLookup_WithholdQty', index: 'Qqty', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, sorttype: 'string', width: 100, hidden: false, editable: true },
    { name: 'Qamt', title: '@Resources.Locale.L_ActSetup_Qamt', index: 'Qamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, editable: true },
    {
        name: 'QexRate', title: '@Resources.Locale.L_ActSetup_ExRate', index: 'QexRate',
        width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'float',
        hidden: false
   , editable: true },
    { name: 'Qlamt', title: '@Resources.Locale.L_ActManage_LocWithAmt', index: 'Qlamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00', editable: true }, width: 100, editable: true },
    { name: 'Qtax', title: '@Resources.Locale.L_ActManage_WithTax', index: 'Qtax', sorttype: 'float', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 80, hidden: false, editable: true },
    { name: 'CheckDescp', title: '@Resources.Locale.L_ActManage_AuditReason', index: 'CheckDescp', sorttype: 'string', width: 150, hidden: false, classes: "normal-white-space", editable: true },
    { name: 'Cur', title: '@Resources.Locale.L_ActQuery_Cur', index: 'Cur', sorttype: 'string', width: 80, hidden: false, editable: true },
    {
        name: 'UnitPrice', title: '@Resources.Locale.L_QTManage_IvUP', index: 'UnitPrice',
        width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'float',
        hidden: false
   , editable: true },
    { name: 'Qty', title: '@Resources.Locale.L_QTManage_IvQty', index: 'Qty', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000', editable: true }, sorttype: 'string', width: 100, hidden: false },
    { name: 'Bamt', title: '@Resources.Locale.L_ActSetup_Amt', index: 'Bamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00'}, width: 100, editable: true },
    {
        name: 'ExRate', title: '@Resources.Locale.L_QTManage_ExRate', index: 'ExRate',
        width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'float',
        hidden: false
   , editable: true },
    { name: 'Lamt', title: '@Resources.Locale.L_ActManage_LocIvAmt', index: 'Lamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00'}, width: 100, editable: true },
    { name: 'Tax', title: '@Resources.Locale.L_QTManage_IvTax', index: 'Tax', sorttype: 'float', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00', editable: true }, width: 80, hidden: false},
    { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', sorttype: 'string', width: 200, hidden: false, editable: true },
    { name: 'CostCenter', title: '@Resources.Locale.L_SMCHGSetup_ChgLevel', index: 'CostCenter', sorttype: 'string', width: 200, hidden: false, editable: true },
    { name: 'ProfitCenter', title: '@Resources.Locale.L_ActManage_ProCenter', index: 'ProfitCenter', sorttype: 'string', width: 200, hidden: false, editable: true  }
];
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
    var dnNo = $_grid.jqGrid('getCell', selRowId, 'DnNo');
    if (!dnNo) {
        CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
        return;
    }

    $.ajax({
        async: true,
        url: rootPath + "DNManage/GetApproveInfo",
        type: 'POST',
        data: {
            "DnNo": dnNo
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
                    resJsonItem.Status = '@Resources.Locale.L_BookingQuery_Views_288';
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
        <h4 class="modal-title">@Resources.Locale.L_ActManage_ApDetail</h4>\
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
            <td>@Resources.Locale.L_DNApproveManage_ApproveType</td>\
            <td>@Resources.Locale.L_ActManage_AprOrder</td>\
            <td>@Resources.Locale.L_ApproveGroupSetup_ApproveDep</td>\
            <td>@Resources.Locale.L_DNApproveManage_ApproveTo</td>\
            <td>@Resources.Locale.L_EventSetup_NotifyDate</td>\
            <td>@Resources.Locale.L_ActManage_Aprer</td>\
            <td>@Resources.Locale.L_ContractQuery_Views_491</td>\
            <td>@Resources.Locale.L_ContractQuery_Views_492</td>\
            <td>@Resources.Locale.L_ContractQuery_Views_493</td>\
            <td>@Resources.Locale.L_ContractQuery_Views_494</td>\
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
            </tr>\
{{/each}}\
</tbody><\/script>';


function EdocSet(id, fieldId, notice, callBackFunc) {

    if (notice === null) {
        notice = true;
    }

    var $_docstn = $("#" + id);
    var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
    var shipmentid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ShipmentId');
    var shipmentUid = "";
    var multiEdocData = [];
    var _cmp = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'Cmp');
    var _groupid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'GroupId');
    var dnUid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'UId');

    if (!dnUid) {
        if (notice) {
            CommonFunc.Notify("", "@Resources.Locale.L_ChgApproveManage_Views_183", 500, "warning");
        }

        return;
    }

    shipmentid = $("#containerInfoGrid").jqGrid('getCell', selRowId, fieldId);
    var dnno = $("#containerInfoGrid").jqGrid('getCell', selRowId, "DnNo");
    var ata = $("#containerInfoGrid").jqGrid('getCell', selRowId, "Ata");
    
    if (shipmentid != "") {
        $.ajax({
            async: true,
            url: rootPath + "DNManage/GetSMData",
            type: 'POST',
            dataType: 'json',
            data: { SmNo: shipmentid },
            "complete": function (xmlHttpRequest, successMsg) {

            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (data) {

                if (data == null) {
                    initEdoc($_docstn, { dnNo: dnno, jobNo: dnUid, 'GROUP_ID': _groupid, 'CMP': _cmp, 'STN': '*', ata: ata }, null, callBackFunc);
                    if (notice) {
                        $_docstn.trigger("click");
                    }
                } else {
                    $(data.sm).each(function (index) {
                        if (data.sm[index].UId !== dnUid)
                        {
                            multiEdocData.push({ jobNo: data.sm[index].UId, 'GROUP_ID': data.sm[index].GroupId, 'CMP': data.sm[index].Cmp, 'STN': '*' });
                        }
                    });
                    initEdoc($_docstn, { dnNo: dnno, jobNo: dnUid, 'GROUP_ID': _groupid, 'CMP': _cmp, 'STN': '*', ata: ata }, multiEdocData, callBackFunc);
                    if (notice) {
                        $_docstn.trigger("click");
                    }
                }

            }
        });
    } else {
        initEdoc($_docstn, { dnNo: dnno, jobNo: dnUid, 'GROUP_ID': _groupid, 'CMP': _cmp, 'STN': '*', ata: ata }, null, callBackFunc);
        if (notice) {
            $_docstn.trigger("click");
        }
    }

}
   
    
var altura = $(window).height() - 180; //value corresponding to the modal heading + footer
var modalWidth = $(window).width() - 100;


var _showCargoModal = '<div class="modal fade" id="TKDetail" Sid="">\
<div class="modal-dialog modal-lg" style="width:'+modalWidth+'px">\
    <div class="modal-content">\
     <div class="modal-header">\
        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
        <h4 class="modal-title">@Resources.Locale.L_TKBLQuery_StsDetInf</h4>\
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
        CommonFunc.Notify("", "@Resources.Locale.L_ChgApproveManage_Views_183", 500, "warning");
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

function DownLoadLogisticExcel() {
    window.open(rootPath + "DNManage/DownLoadXls?TranType=Logistics");
}

function DownLoadChangePodExcel() {
    window.open(rootPath + "DNManage/DownLoadXls?TranType=POD");
}

function CallLogisticsFunc() {
    $("#PARTY_EXCEL_UPLOAD_FROM").submit(function () {
        var UId = $("#UId").val();
        $(this).find("input[type='hidden']").remove();
        $(this).append('<input type="hidden" name="UId" value="' + UId + '" />');
        var postData = new FormData($(this)[0]);

        $.ajax({
            url: rootPath + "BookingAction/LogisticUpload",
            type: 'POST',
            data: postData,
            async: false,
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            },
            success: function (data) {
                //alert(data)
                CommonFunc.ToogleLoading(false);
                if (data.message != "success") {
                    CommonFunc.Notify("", "@Resources.Locale.L_BSTQuery_ImpFail" + data.message, 1300, "warning");
                    return false;
                }
                CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Scripts_126", 500, "success");
                $("#LogisticsUploadWin").modal("hide");
                $("#SummarySearch").trigger("click");
            },
            cache: false,
            contentType: false,
            processData: false
        });

        return false;
    });
}

function CallChangePriceFunc() {
    $("#DN_PRICE_UPLOAD_FROM").submit(function () {
        var UId = $("#UId").val();
        $(this).find("input[type='hidden']").remove();
        $(this).append('<input type="hidden" name="UId" value="' + UId + '" />');
        var postData = new FormData($(this)[0]);

        $.ajax({
            url: rootPath + "DNManage/DnPriceUpload",
            type: 'POST',
            data: postData,
            async: false,
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            },
            success: function (data) {
                //alert(data)
                CommonFunc.ToogleLoading(false);
                CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Scripts_126" + data.message, 1300, "warning");
                $("#DNPirceUploadWin").modal("hide");
                return true;
            },
            cache: false,
            contentType: false,
            processData: false
        });

        return false;
    });
}

function CallChangeGwCbmFunc() {
    $("#GWCBM_EXCEL_UPLOAD_FROM").submit(function () {
        var UId = $("#UId").val();
        var trantype = $("#trantypeforinput").val();
        $(this).find("input[type='hidden']").remove();
        $(this).append('<input type="hidden" name="UId" value="' + UId + '" />');
        if (trantype != "")
            $(this).append('<input type="hidden" name="type" value="' + UId + '" />');
        var postData = new FormData($(this)[0]);
        var url = rootPath + "BookingAction/CBMAndGwUpload";
        if (trantype != "") {
            url += "?type=" + trantype;
        }
        $.ajax({
            url: url,
            type: 'POST',
        data: postData,
        async: false,
        beforeSend: function () {
        CommonFunc.ToogleLoading(true);
        },
        success: function (data) {
            //alert(data)
                CommonFunc.ToogleLoading(false);
                CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Scripts_126" + data.message, 1300, "warning");
                $("#BatchGwCbmUploadWin").modal("hide");
        return true;
        },
        cache: false,
        contentType: false,
        processData: false
    });

        return false;
    });
}


//以下是订舱menu公用的部分
var BookingFirstMenu = [
     {
         id: "btnPartyImport",
         name: "Import Party",
         func: function () {
             $("#LogisticsUploadWin").modal("show");
         }
     },
    {
        id: "btn01",
        name: "@Resources.Locale.L_DNManage_ForeBk",
        func: function () {
            var mygrid = $("#containerInfoGrid");
            var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
            var responseData = [];
            var dnitems = "";
            $.each(selRowId, function (index, val) {
                responseData.push(mygrid.getRowData(selRowId[index]));
            });
            if (responseData.length < 1) {
                CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Scripts_127", 500, "warning");
                return;
            }
            var shipments = "";
            for (var i = 0; i < responseData.length; i++) {
                dnitems += responseData[i].UId + ",";
                if (shipments.length > 0)
                    shipments += ",";
                shipments += responseData[i].ShipmentId;
            }

            var iscontinue = window.confirm("@Resources.Locale.L_DNManage_Is1" + shipments + "@Resources.Locale.L_DNManage_Launch");
            if (!iscontinue) {
                return;
            }
            CommonFunc.ToogleLoading(true);
            $.ajax({
                async: true,
                url: rootPath + "BookingAction/FCLBookAction",
                type: 'POST',
                data: {
                    "Uid": dnitems
                },
                "complete": function (xmlHttpRequest, successMsg) {
                    CommonFunc.ToogleLoading(false);
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.ToogleLoading(false);
                    var resJson = $.parseJSON(errMsg)
                    CommonFunc.Notify("", resJson.message, 500, "warning");
                },
                success: function (result) {
                    //var resJson = $.parseJSON(result)
                    if (result.IsOk == "Y") {
                        CommonFunc.Notify("", result.message, 500, "success");
                    }
                    else {
                        alert(result.message);
                    }
                    $("#SummarySearch").trigger("click");
                }
            });
        }
    },
    {
        id: "btn02",
        name: "@Resources.Locale.L_DNManage_CancelBk",
        func: function () {
            var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
            var uid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'UId');
            if (!uid) {
                CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                return;
            }
            var combine = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'COMBINE_INFO');
            if (combine) {
                CommonFunc.Notify("", "@Resources.Locale.L_DNApproveManage_Views_312", 500, "warning");
                return;
            }
            var dnType = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'Status');
            if (dnType == "A") {
                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_NoForeBk", 500, "warning");
                return;
            }
            $("#BackRemark").val('');
            $("#BookingCancel").modal("show");
        }
    },
    {
        id: "btn03",
        name: "@Resources.Locale.L_DNManage_NotiDecla",
        func: function () {
            var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
            var uid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'UId');
            if (!uid) {
                CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                return;
            }
            top.topManager.openPage({
                href: rootPath + "DNManage/CustomsBooking/" + uid,
                title: '@Resources.Locale.L_DNManage_DeclaBk',
                id: 'CustomsBooking'
            });
        }
    },
    {
        id: "combineBL",
        name: "@Resources.Locale.L_DNManage_MerBL",
        func: function () {
            var mygrid = $("#containerInfoGrid");
            var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
            var responseData = [];
            var shipmentids = "";
            $.each(selRowId, function (index, val) {
                responseData.push(mygrid.getRowData(selRowId[index]));
            });
            if (responseData.length < 2) {
                CommonFunc.Notify("", "@Resources.Locale.L_DNFlowManage_Views_343", 500, "warning");
                return;
            }
            for (var i = 0; i < responseData.length; i++) {
                //if (!isEmpty(responseData[i].ShipmentInfo)) {
                //    CommonFunc.Notify("", "该笔资料：" + responseData[i].ShipmentId + "已经合併过资料", 500, "warning");
                //    return;
                //}
                shipmentids += responseData[i].ShipmentId + ";";
            }
            var iscontinue = window.confirm("@Resources.Locale.L_DNManage_Is1" + shipmentids + "@Resources.Locale.L_DNManage_Is3");
            if (!iscontinue) {
                return;
            }

            CommonFunc.ToogleLoading(true);
            $.ajax({
                async: true,
                url: rootPath + "BookingAction/CombineBill",
                type: 'POST',
                data: {
                    "Shipmentid": shipmentids
                },
                "complete": function (xmlHttpRequest, successMsg) {
                    CommonFunc.ToogleLoading(false);
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.ToogleLoading(false);
                    var resJson = $.parseJSON(errMsg)
                    CommonFunc.Notify("", resJson.message, 500, "warning");
                },
                success: function (result) {
                    CommonFunc.ToogleLoading(false);
                    if (result.IsOk == "Y") {
                        CommonFunc.Notify("", result.message, 500, "success");
                    } else {
                        CommonFunc.Notify("", result.message, 500, "warning");
                    }
                    $("#SummarySearch").trigger("click");
                }
            });
        }
    },
    {
        id: "SpellcombineBL",
        name: "@Resources.Locale.L_BaseBooking_Scripts_129",
        func: function () {
            var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
            var uid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'UId');
            if (!uid) {
                CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                return;
            }
            var status = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'Status');
            //if (status != "A") {
            //    CommonFunc.Notify("", "已经发起对外订舱，不能解除合并！", 500, "warning");
            //    return;
            //}
            $("#SpellCombineBillModel").modal("show");
        }
    },
    {
        id: "combineSM",
        name: "@Resources.Locale.L_BaseBooking_Script_94",
        func: function () {
            var mygrid = $("#containerInfoGrid");
            var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
            var responseData = [];
            var shipmentids = "";
            $.each(selRowId, function (index, val) {
                responseData.push(mygrid.getRowData(selRowId[index]));
            });
            if (responseData.length < 2) {
                CommonFunc.Notify("", "@Resources.Locale.L_DNFlowManage_Views_343", 500, "warning");
                return;
            }
            for (var i = 0; i < responseData.length; i++) {
                shipmentids += responseData[i].ShipmentId + ";";
            }
            var iscontinue = window.confirm("@Resources.Locale.L_DNManage_Is1" + shipmentids + "@Resources.Locale.L_BaseBooking_Script_95");
            if (!iscontinue) {
                return;
            }

            CommonFunc.ToogleLoading(true);
            $.ajax({
                async: true,
                url: rootPath + "BookingAction/CombineShipment",
                type: 'POST',
                data: {
                    "Shipmentid": shipmentids
                },
                "complete": function (xmlHttpRequest, successMsg) {
                    CommonFunc.ToogleLoading(false);
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.ToogleLoading(false);
                    var resJson = $.parseJSON(errMsg)
                    CommonFunc.Notify("", resJson.message, 500, "warning");
                },
                success: function (result) {
                    CommonFunc.ToogleLoading(false);
                    if (result.IsOk == "Y") {
                        CommonFunc.Notify("", result.message, 500, "success");
                    } else {
                        CommonFunc.Notify("", result.message, 500, "warning");
                    }
                    $("#SummarySearch").trigger("click");
                }
            });
        }
    },

    {
        id: "SpellCombineSM",
        name: "@Resources.Locale.L_BaseBooking_Script_96",
        func: function () {
            var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
            var uid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'UId');
            if (!uid) {
                CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                return;
            }

            var status = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'Status');
            //if (status != "A") {
            //    CommonFunc.Notify("", "已经发起对外订舱，不能解除合并！", 500, "warning");
            //    return;
            //}
            $("#SpellCombineSModel").modal("show");
        }
    },
    {
        id: "btn04",
        name: "@Resources.Locale.L_UserQuery_Return",
        func: function () {
            var r = confirm("@Resources.Locale.L_BaseBooking_Script_97")
            if (r == false) {
                return;
            }
            var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
            var responseData = [];
            var dnitems = "";
            var shipments = "";
            $.each(selRowId, function (index, val) {
                responseData.push(mygrid.getRowData(selRowId[index]));
            });
            if (responseData.length < 1) {
                CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Scripts_132", 500, "warning");
                return;
            }
            for (var i = 0; i < responseData.length; i++) {
                var lspno = responseData[i].Status;
                if (lspno != "O") {
                    CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Script_98", 500, "warning");
                    return;
                }

                dnitems += responseData[i].DnNo + ",";
                shipments += responseData[i].ShipmentId + ",";
            }
            CommonFunc.ToogleLoading(true);
            $.ajax({
                async: true,
                url: rootPath + "BookingAction/BackTransport",
                type: 'POST',
                data: {
                    "Dnno": dnitems,
                    "Shipmentid": shipments
                },
                "complete": function (xmlHttpRequest, successMsg) {
                    CommonFunc.ToogleLoading(false);
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.ToogleLoading(false);
                    var resJson = $.parseJSON(errMsg)
                    CommonFunc.Notify("", resJson.message, 500, "warning");
                },
                success: function (result) {
                    CommonFunc.ToogleLoading(false);
                    if (result.IsOk == "Y") {
                        CommonFunc.Notify("", result.message, 500, "success");
                        $("#SummarySearch").trigger("click");
                    } else {
                        CommonFunc.Notify("", result.message, 500, "warning");
                        $("#SummarySearch").trigger("click");
                    }
                }
            });
        }
    },
    {
        id: "Hand",
        name: "Hand",
        func: function () {
            var r = confirm("@Resources.Locale.L_BaseBooking_Script_99")
            if (r == false) {
                return;
            }

            var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
            var responseData = [];
            var dnitems = "";
            var shipments = "";
            $.each(selRowId, function (index, val) {
                responseData.push(mygrid.getRowData(selRowId[index]));
            });
            if (responseData.length < 1) {
                CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Scripts_132", 500, "warning");
                return;
            }
            for (var i = 0; i < responseData.length; i++) {
                shipments += responseData[i].ShipmentId + ";";
            }

            CommonFunc.ToogleLoading(true);
            $.ajax({
                async: true,
                url: rootPath + "BookingAction/HandIn",
                type: 'POST',
                data: {
                    "Shipmentid": shipments
                },
                "complete": function (xmlHttpRequest, successMsg) {
                    CommonFunc.ToogleLoading(false);
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.ToogleLoading(false);
                    var resJson = $.parseJSON(errMsg)
                    CommonFunc.Notify("", resJson.message, 500, "warning");
                },
                success: function (result) {
                    CommonFunc.ToogleLoading(false);
                    //var resJson = $.parseJSON(result)
                    CommonFunc.Notify("", result.message, 500, "success");
                    $("#SummarySearch").trigger("click");
                }
            });
        }
    }
];

var isfBtn = [
    {
        id: "send2isf",
        name: "@Resources.Locale.L_BaseBooking_Script_100",
        func: function () {
            var selRowId =  $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
            var ShipmentId = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ShipmentId');
            var ScacCd = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ScacCd');
            var MasterNo = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'MasterNo');
            var HouseNo = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'HouseNo');

            if (ISFAcct == "" || ISFKey == "") {
                CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Script_101", 500, "warning");
                return false;
            }

            if (isEmpty(ShipmentId)) {
                CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                return false;
            }

            var r = confirm("Are you sure send to ISF server?");
            if (r == false) {
                return;
            }

            //if (isEmpty(ScacCd))
            //{
            //    CommonFunc.Notify("", "SCAC不可为空", 500, "warning");
            //    return false;
            //}
            //if (isEmpty(MasterNo) && isEmpty(HouseNo))
            //{
            //    CommonFunc.Notify("", "Master bl no/ house bl no不可同时为空", 500, "warning");
            //    return false;
            //}
            //ScacCd   MasterNo  HouseNo
            $.ajax({
                url: rootPath + 'DNManage/Send2ISF',
                type: 'POST',
                dataType: 'json',
                data: { ShipmentId: ShipmentId, ISFAcct: ISFAcct, ISFPWD: ISFPWD },
                beforeSend: function(){
                    StatusBarArr.nowStatus("@Resources.Locale.L_ActManage_Views_137");
                    CommonFunc.ToogleLoading(true);
                },
                success: function (result) {

                    xmlDoc = $.parseXML(result),
                    $xml = $(xmlDoc);
                    var res = $xml.find("result");
                    var msg = $xml.find("msgData");
                    if (res.text() == "True")
                    {
                        CommonFunc.Notify("", "@Resources.Locale.L_SYS_SUCCESS", 1000, "success");
                    }
                    else if (result.message == "fail") {
                        CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Script_122", 1000, "warning");
                    }
                    else {
                        //CommonFunc.Notify("", "成功", 1000, "success");
                        if (msg.text() == "")
                            CommonFunc.Notify("", result.message, 1000, "warning");
                        else CommonFunc.Notify("", msg.text(), 1000, "warning");
                        //alert(result.message);
                    }

                    StatusBarArr.nowStatus("");
                    CommonFunc.ToogleLoading(false);
                },
                error: function(){
                    CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Script_123", 1000, "danger");
                    CommonFunc.ToogleLoading(false);
                }
            });
        }
    },
    {
        id: "openIsf",
        name: "@Resources.Locale.L_BaseBooking_Script_124",
        func: function () {
            var selRowId =  $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
            var ShipmentId = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ShipmentId');
            //console.log(ISFAcct);
            if (ISFAcct == "" || ISFKey == "") {
                CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Script_101", 500, "warning");
                return false;
            }

            if (isEmpty(ShipmentId)) {
                CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                return false;
            }
            var blno = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'HouseNo');
            if(isEmpty(blno)){
                blno = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'MasterNo');
            }
           // alert(ISFUrl+"ISF_CBP_Input.aspx?plDataNo=" + ShipmentId + "&user=TPVFQ_CBP&key=6d2ddd165a52397780de27dc2950663a");
            var isSend = "";
            if (pmsList.indexOf("ISFSEND") > -1) {
                isSend = '&is_send=Y';
            } else {
                isSend = '&is_send=N';
            }
            //alert(ISFUrl + "ISF_CBP_Input.aspx?plDataNo=" + ShipmentId + "&user=ADMIN&key=73acd9a5972130b75066c82595a1fae3" + isSend);
            top.topManager.openPage({
                href: ISFUrl + "ISF_CBP_Input.aspx?&user=" + ISFAcct + "&key=" + ISFKey + "&plDataNo=" + ShipmentId + isSend + "&blNo" + blno,
                title: '@Resources.Locale.L_DNManage_ImISF',
                reload: true,
                id: 'ISFManage'
            });

        }
    }
];


var BookingLastMenu = [
    {
        id: "CalCulCost",
        name: "@Resources.Locale.L_Layout_AutoAmount",
        func: function () {
            var mygrid = $("#containerInfoGrid");
            var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
            var responseData = [];
            var uids = "";
            $.each(selRowId, function (index, val) {
                responseData.push(mygrid.getRowData(selRowId[index]));
            });
            if (responseData.length < 1) {
                CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                return;
            }
            for (var i = 0; i < responseData.length; i++) {
                uids += responseData[i].UId + ";";
            }
            CommonFunc.ToogleLoading(true);
            $.ajax({
                async: true,
                url: rootPath + "QTManage/CreateBill",
                type: 'POST',
                data: {
                    "uid": uids,
                    shipmentId: uids
                },
                "complete": function (xmlHttpRequest, successMsg) {
                    CommonFunc.ToogleLoading(false);
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.ToogleLoading(false);
                    var resJson = $.parseJSON(errMsg)
                    CommonFunc.Notify("", resJson.message, 500, "warning");
                },
                success: function (result) {
                    CommonFunc.ToogleLoading(false);
                    //var resJson = $.parseJSON(result)
                    CommonFunc.Notify("", result.message, 500, "success");
                    //$("#SummarySearch").trigger("click");
                }
            });
        }
    },
    {
        id: "ShipFee",
        name: "@Resources.Locale.L_ActManage_LspChg",
        func: function () {
            var id = $("#containerInfoGrid").jqGrid('getGridParam', "selrow");
            var map = $("#containerInfoGrid").jqGrid('getRowData', id);
            var UId;
            if (map) UId = map.UId;

            if (isEmpty(UId)) {
                CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                return false;
            }

            getData(rootPath + "QTManage/CostDetail", { uid: UId, shipmentId: map.ShipmentId }, function (result) {
                /*var source = $("#costTemplate").html();
                var cols = ["ChgCd", "ChgDescp", "Cur", "UnitPrice", "ChgUnit", "Qty", "Qamt", "Bamt", "Tax"];
                var data = { rowData: result };
                var template = Handlebars.compile(source);
                var html = template(data);
                $("#costTable").html(html);*/
                //console.log(result);
                $("#ShipFeeDialogGrid").jqGrid("clearGridData");
                $("#ShipFeeDialogGrid").jqGrid("setGridParam", {
                    datatype: 'local',
                    sortorder: "asc",
                    sortname: "LspNo",
                    data: result,
                }).trigger("reloadGrid");

                $('#costDetail').modal('show'); //顯示彈出視窗
                ajustamodal("#costDetail");
            });
        }
    },
    {
        id: "btn06",
        name: "DN",
        func: function () {
            var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
            var shipmentid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ShipmentId');
            if (!shipmentid) {
                CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                return;
            }

            top.topManager.openPage({
                href: rootPath + "DNManage/ShipmentDN/" + shipmentid,
                title: 'ShipmentDN',
                id: 'ShipmentDN'
            });
        }
    },
    //{
    //    id: "btn07",
    //    name: "電子文檔",
    //    func: function () {
    //        EdocSet("btn07");
    //    }
    //},
    {
        id: "btn08",
        name: "@Resources.Locale.L_BookingQuery_Views_266",
        func: function () {
            var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
            var uid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'UId');
            var shipmentid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ShipmentId');
            if (!uid) {
                CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                return;
            }
            initErrMsg($("#btn08"), { 'GROUP_ID': groupId, 'CMP': cmp, 'STN': stn, 'UId': uid, 'JobNo': shipmentid }, true, $("#containerInfoGrid"));

        }
    },

    {
        id: "btn09",
        name: "@Resources.Locale.L_BookingQuery_Views_267",
        func: function () {
            CheckCargoDetailed();
        }
    },
    {
        id: "Inv",
        name: "Invoice",
        func: function () {
            var selarrrowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
            var ShipmentId = getGridVal($("#containerInfoGrid"), selarrrowId, "ShipmentId", null);

            if (!ShipmentId) {
                CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                return;
            }
            top.topManager.openPage({
                href: rootPath + "DNManage/InvPkgQuery?ShipmentId=" + ShipmentId,
                title: 'Invoice/Packing',
                id: 'DN011',
                search: 'ShipmentId=' + ShipmentId
            });
        }
    }, {
        id: "CallCar",
        name: "@Resources.Locale.L_BaseBooking_Script_105",
        func: function () {
            CallCarDetailed();
        }
    }, {
        id: "MBNoticeBill",
        name: "@Resources.Locale.L_BaseBooking_Scripts_149",
        func: function () {
            NoticeBillFunc();
        }
    }, {
        id: "MBConfirmBill",
        name: "@Resources.Locale.L_BaseBooking_Scripts_150",
        func: function () {
            ConfirmBill();
        }
    }, {
        id: "MBChangePod",
        name: "@Resources.Locale.TLB_ChangePod",
        func: function () {
            $("#podUploadWin").modal("show");
        }
    }
];

function BookingCancel() {
    var backremark = $("#BackRemark").val();
    if (backremark == "") {
        CommonFunc.Notify("", "@Resources.Locale.L_ActManage_EnterReason", 500, "warning");
        return;
    }
    var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
    var uid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'UId');
    if (!uid) {
        CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
        return;

    }

    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: true,
        url: rootPath + "BookingAction/FCLBookCancel",
        type: 'POST',
        data: {
            "UId": uid,
            "BackRemark": backremark
        },
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.ToogleLoading(false);
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
            $("#CloseBookingWin").trigger("click");
        },
        success: function (result) {
            CommonFunc.ToogleLoading(false);
            if (result.IsOk = "Y") {
                $("#CloseBookingWin").trigger("click");
                CommonFunc.Notify("", result.message, 500, "success");
            } else {
                $("#CloseBookingWin").trigger("click");
                CommonFunc.Notify("", result.message, 500, "warning");
            }
            $("#SummarySearch").trigger("click");
        }
    });
}

var _showCallCarModal = '<div class="modal fade" id="CallCarDetail" Sid="">\
<div class="modal-dialog modal-lg">\
    <div class="modal-content">\
     <div class="modal-header">\
        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
        <h4 class="modal-title">@Resources.Locale.L_DNManage_CntrCarInfo</h4>\
        </div>\
      <div class="modal-body" id="TKContent">\
        <table class="table table-bordered table-hover" id="CallCarTable">\
        </table>\
      </div>\
      <div class="modal-footer">\
        <button type="button" class="btn btn-default" data-dismiss="modal" id="ModalClose">Close</button>\
      </div>\
      </div>\
  </div>\
</div>';

var _showCallCarTemplte = '<script id="CallCarTemplate" type="text/x-handlebars-template">\
    <thead>\
        <tr class="info">\
            <td>SEQ NO</td>\
            <td>@Resources.Locale.L_DNManage_AcSealNo</td>\
            <td>@Resources.Locale.L_BaseBooking_Script_106</td>\
            <td>@Resources.Locale.L_BaseBooking_Script_107</td>\
            <td>@Resources.Locale.L_GateAnalysis_ReserveNo</td>\
            <td>@Resources.Locale.L_GateReserve_TruckCntrno</td>\
            <td>@Resources.Locale.L_DNManage_EstSeal</td>\
            <td>@Resources.Locale.L_GateReserveSetup_CallDate</td>\
            <td>@Resources.Locale.L_BaseLookup_PickupCdate</td>\
            <td>@Resources.Locale.L_GateReserve_WsCd</td>\
            <td>@Resources.Locale.L_GateAnalysis_MoveNumber</td>\
            <td>@Resources.Locale.L_ContainerManage_InBy</td>\
            <td>@Resources.Locale.L_GateAnalysis_GateNo</td>\
            <td>@Resources.Locale.L_BaseLookup_TruckNo</td>\
            <td>@Resources.Locale.L_GateReserve_Driver</td>\
        </tr>\
    </thead>\
    <tbody>\
        {{#each rowData}}\
            <tr>\
                <td>{{this.__seq}}</td>\
                <td>{{this.CntrNo}}</td>\
                <td>{{this.SealNo1}}</td>\
                <td>{{this.SealNo2}}</td>\
                <td>{{this.ReserveNo}}</td>\
                <td>{{this.TruckCntrno}}</td>\
                <td>{{this.TruckSealno}}</td>\
                <td>{{this.CallDate}}</td>\
                <td>{{this.UseDate}}</td>\
                <td>{{this.WsCd}}</td>\
                <td>{{this.MoveNumber}}</td>\
                <td>{{this.InDate}}</td>\
                <td>{{this.GateNo}}</td>\
                <td>{{this.TruckNo}}</td>\
                <td>{{this.Driver}}</td>\
            </tr>\
    {{/each}}\
</tbody>\
</script>';

function CallCarDetailed() {
    if ($("#CallCarDetail").length <= 0) {
        $("body").append(_showCallCarModal);
    }

    if ($("#CallCarTemplate").length <= 0) {
        $("body").append(_showCallCarTemplte);
    }
    var id = $("#containerInfoGrid").jqGrid('getGridParam', "selrow");
    var map = $("#containerInfoGrid").jqGrid('getRowData', id);
    var ShipmentId;
    if (map) ShipmentId = map.ShipmentId;

    if (isEmpty(ShipmentId)) {
        CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
        return false;
    }

    getData(rootPath + "BookingAction/GetContainerInfo", { ShipmentId: ShipmentId }, function (result) {
        var source = $("#CallCarTemplate").html();
        var cols = ["__seq", "CntrNo", "SealNo1", "SealNo2", "ReserveNo", "TruckCntrno", "TruckSealno", "CallDate", "UseDate", "WsCd", "MoveNumber","InDate","WsCd","GateNo","TruckNo","Driver"];
        var data = { rowData: result };
        var template = Handlebars.compile(source);
        var html = template(data);
        $("#CallCarTable").html(html);
        $('#CallCarDetail').modal('show');
        ajustamodal("#CallCarDetail");
    });
}

function NoticeBillFunc(){
    //1.多一個提單通知按鈕，可以mail通知forwarder及夾憱draft B/L 
    var mygrid = $("#containerInfoGrid");
    var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
    var responseData = [];
    var dnitems = "";
    $.each(selRowId, function (index, val) {
        responseData.push(mygrid.getRowData(selRowId[index]));
    });
    if (responseData.length < 1) {
        CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Scripts_127", 500, "warning");
        return;
    }
    var shipments = "";
    for (var i = 0; i < responseData.length; i++) {
        dnitems += responseData[i].UId + ",";
        if (shipments.length > 0)
            shipments += ",";
        shipments += responseData[i].ShipmentId;
    }

    var iscontinue = window.confirm("@Resources.Locale.L_DNManage_Is1" + shipments + "@Resources.Locale.L_BaseBooking_Script_108");
    if (!iscontinue) {
        return;
    }
    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: true,
        url: rootPath + "BookingAction/NoticeBillFreight",
        type: 'POST',
        data: {
            "Uid": dnitems
        },
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.ToogleLoading(false);
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
        },
        success: function (result) {
            if (result.IsOk == "Y") {
                CommonFunc.Notify("", result.message, 500, "success");
            }
            else {
                alert(result.message);
            }
            $("#SummarySearch").trigger("click");
        }
    });
}

function ConfirmBill(){
    var mygrid = $("#containerInfoGrid");
    var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
    var responseData = [];
    var dnitems = "";
    $.each(selRowId, function (index, val) {
        responseData.push(mygrid.getRowData(selRowId[index]));
    });
    if (responseData.length < 1) {
        CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Scripts_127", 500, "warning");
        return;
    }
    var shipments = "";
    for (var i = 0; i < responseData.length; i++) {
        dnitems += responseData[i].UId + ",";
        if (shipments.length > 0)
            shipments += ",";
        shipments += responseData[i].ShipmentId;
    }

    var iscontinue = window.confirm("@Resources.Locale.L_DNManage_Is1" + shipments + "@Resources.Locale.L_BaseBooking_Script_109");
    if (!iscontinue) {
        return;
    }
    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: true,
        url: rootPath + "BookingAction/BlCheckConfirm",
        type: 'POST',
        data: {
            "Uid": dnitems
        },
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.ToogleLoading(false);
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
        },
        success: function (result) {
            if (result.IsOk == "Y") {
                CommonFunc.Notify("", result.message, 500, "success");
            }
            else {
                CommonFunc.Notify("", result.message, 500, "warning");
            }
            $("#SummarySearch").trigger("click");
        }
    });
}

function ConfirmSpellSM() {
    var spDnTemp = "";
    $('input:checkbox[name=spellDn]:checked').each(function (i) {
        if(0==i){
            spDnTemp = $(this).val();
        }else{
            spDnTemp += (","+$(this).val());
        }
    });

    if (isEmpty(spDnTemp))
    {
        return alert("@Resources.Locale.L_BaseBooking_Script_110");
    }
    var shipmentid = $("#hid_shipmentid").val();
    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: true,
        url: rootPath + "BookingAction/SPellCombineShipment",
        type: 'POST',
        data: {
            "ShipmentId": shipmentid,
            "RemoveDn": spDnTemp
        },
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.ToogleLoading(false);
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
        },
        success: function (result) {
            $("#CloseSpellWin").trigger("click");
            if (result.IsOk == "Y") {
                CommonFunc.Notify("", result.message, 500, "success");
            }
            else {
                CommonFunc.Notify("", result.message, 500, "warning");
            }
            $("#SummarySearch").trigger("click");
        }
    });
}

function ConfirmSpellBill() {
    var spDnTemp = "";
    var count = 0;
    $('input:checkbox[name=spellSm]:checked').each(function (i) {
        if (0 == i) {
            spDnTemp = $(this).val();
        } else {
            spDnTemp += ("," + $(this).val());
        }
        count++;
    });
    var allcount = $('input:checkbox[name=spellSm]').length;
    if (count + 1 == allcount)
    {
        if (!confirm("@Resources.Locale.L_BaseBooking_Script_111"))
            return false;
        //return alert("请至少要保留兩筆或者全部解除合并");
    }
    if (isEmpty(spDnTemp)) {
        return alert("@Resources.Locale.L_DNManage_ShipRemove");
    }
    var shipmentid = $("#hid_billid").val();
    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: true,
        url: rootPath + "BookingAction/SPellCombineBill",
        type: 'POST',
        data: {
            "ShipmentId": shipmentid,
            "RemoveSm": spDnTemp
        },
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.ToogleLoading(false);
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
        },
        success: function (result) {
            $("#CloseSpellBillWin").trigger("click");
            if (result.IsOk == "Y") {
                CommonFunc.Notify("", result.message, 500, "success");
            }
            else {
                CommonFunc.Notify("", result.message, 500, "warning");
            }
            $("#SummarySearch").trigger("click");
        }
    });
}

var _showSpellBillTemplte = '<div class="modal fade" id="SpellCombineBillModel" Sid="">\
  <div class="modal-dialog modal-lg">\
    <div class="modal-content">\
      <div class="modal-header">\
        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
        <h4 class="modal-title">@Resources.Locale.L_DNManage_LiftCombBL</h4>\
      </div>\
      <div class="modal-body">\
            <div class="pure-g">\
                <div class="pure-u-sm-60-60">\
                    <div class="form-group">\
                        <label for="exampleInputEmail1">@Resources.Locale.L_DNManage_LiftShipInfo</label>\
                        <input type="hidden" id="hid_billid" />\
                        <div id="SMCheckList">\
                        </div>\
                    </div>\
                </div>\
            </div>\
      </div>\
      <div class="modal-footer">\
        <button type="button" class="btn btn-default" data-dismiss="modal" id="CloseSpellBillWin" >Close</button>\
        <button type="button" class="btn btn-primary" onclick="ConfirmSpellBill()" id="SpellConfirmBill">@Resources.Locale.L_Layout_Confirm</button>\
      </div>\
    </div>\
  </div>\
</div>';

function RegisterSpellBill() {
    if ($("#SpellCombineBillModel").length <= 0) {
        $("body").append(_showSpellBillTemplte);
    }

    $('#SpellCombineBillModel').on('show.bs.modal', function () {
        var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
        var CombineInfo = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ShipmentInfo');
        var shipmentid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ShipmentId');
        $("#hid_billid").val(shipmentid);
        var CombineInfolist = CombineInfo.split(',');
        var spllhtml = "";
        if (CombineInfolist.length <= 1) {
            spllhtml += '<div>@Resources.Locale.L_DNManage_NoData</div>';
        } else {
            $.each(CombineInfolist, function (index, value) {
                spllhtml += '<div><input type="checkbox" name="spellSm" value="' + value + '" />' + value + '</div>';
            });
        }
        $("#SMCheckList").html(spllhtml);
    });
}


function doDownloadExcel(grid, transType, btnType) {

    var selRowId = grid.jqGrid('getGridParam', 'selrow');
    if (typeof selRowId == "undefined") {
        CommonFunc.Notify("", "@Resources.Locale.L_DNManage_PleSelcData", 500, "warning");
        return;
    }

    var ioFlag = getCookie("plv3.passport.ioflag");
    var TargetCmp = "";
    if (ioFlag == "I" && !(transType == "F" && btnType == "F") && !(transType == "R" && btnType == "F")) {
        TargetCmp = prompt("@Resources.Locale.Alert_DebitNo_notice", "");
        if (TargetCmp === null) {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_CancelDownload", 500, "warning");
            return; //break out of the function early
        }

    }

    if (TargetCmp == null || TargetCmp === "")
        TargetCmp = getCookie("plv3.passport.companyid");

    var selRowId = grid.jqGrid('getGridParam', 'selrow');
    var shipmentCmp = grid.jqGrid('getCell', selRowId, 'Cmp');
    var selRowIds = grid.jqGrid('getGridParam', 'selarrrow');
    $.each(selRowIds, function (index, val) {
        if (shipmentCmp != grid.jqGrid('getCell', selRowIds[index], 'Cmp')) {
            CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Script_112" + grid.jqGrid('getCell', selRowIds[index], 'ShipmentId') + "@Resources.Locale.L_ActDeatilManage_Views_115 ", 500, "warning");
            return;
        }
    });

    $.ajax({
        async: true,
        url: rootPath + "DNManage/GetTransTypeInfo",
        type: 'POST',
        data: {
            "TransType": transType,
            "Cmp": TargetCmp,
            "Type": btnType
        },
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.ToogleLoading(false);
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
        },
        success: function (result) {

            if (result.message != "success") {
                CommonFunc.Notify("", result.message, 500, "warning");
                return;
            }

            var url = grid.jqGrid("getGridParam", "url");
            if (url == null || url == "") {
                return false;
            }
            // { message = returnMsg, chgTypeStr = chgTypeStr.Substring(-1, chgTypeStr.Length), cmp = CompanyId, user = UserId, group = GroupId, createDate = DateTime.Now.ToString("yyyy/MM/dd HH:ss")
            var colModel = [
               { name: 'rn', title: 'rn', index: 'rn', sorttype: 'string', width: 100, align: 'left', hidden: false },
                            { name: 'Location', title: 'LOCATION||LOCATION', index: 'Location', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'ShipmentId', title: 'Shipment ID||SHIPMENT_ID', index: 'ShipmentId', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'DnInfo', title: 'DN INFO||DN_INFO', index: 'DnInfo', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'BlNo', title: 'BL NO||BL_NO', index: 'BlNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                            { name: 'DebitDate', title: 'DEBIT DATE||DEBIT_DATE', index: 'DebitDate', sorttype: 'string', width: 100, align: 'left', formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                            { name: 'DebitNo', title: 'DEBIT NO||DEBIT_NO', index: 'DebitNo', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'Cur', title: 'Currency||CUR', index: 'Cur', sorttype: 'string', align: 'left', width: 100, hidden: false }
            ];

            var qAmtSql = "  SELECT SUM(ISNULL(SMBID.Qamt,0)) FROM SMBID WHERE SMBID.SHIPMENT_ID = S.SHIPMENT_ID AND SMBID.LSP_NO = '" + TargetCmp + "' ";
            //grid.jqGrid("getGridParam", "colModel").slice();
            var transTypeCols = result.chgTypeStr.split(";");
            var transTypeColsN = result.chgTypeColsStr.split(";");
            var virtualCol = " ,CMP AS LOCATION, COMBINE_INFO AS DN_INFO,ISNULL(MASTER_NO,HOUSE_NO) AS BL_NO,ISNULL(ATD,ETD) AS DEBIT_DATE ";
            var colNames = ["rn", "LOCATION", "Shipment ID", "DN INFO", "BL NO", "DEBIT DATE", "DEBIT NO", "CURRENCY"];//grid.jqGrid("getGridParam", "colNames");
            $.each(transTypeCols, function (index, val) {
                colModel.push({ name: transTypeColsN[index], title: val, index: transTypeColsN[index], sorttype: 'string', width: 40, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false });
                colNames.push(val);
                virtualCol += " ,(" + qAmtSql + " AND CHG_CD = '" + val.split("-")[0] + "' ) AS '" + val.split("-")[0] + "'";
            });
            console.log(colModel);
            console.log(colNames);

            var caption = "Statement Of Account \r\n Location : " + result.cmp + "  User : " + result.user + "   Date : " + result.createDate;
            var excelName = "Statement Of Account";

            var selRowId = grid.jqGrid('getGridParam', 'selarrrow');
            var conditions = "1=1 AND " + grid.jqGrid("getGridParam", "postData").conditions;
            var selKey = "";
            $.each(selRowId, function (index, val) {
                selKey += grid.jqGrid('getCell', selRowId[index], 'UId') + ";";
            });

            if (selKey != "") {
                conditions += "&sopt_UId=in&UId=" + selKey.slice(0, -1) + "";
            }
            var baseCondition = grid.jqGrid("getGridParam", "postData").baseCondition;
            if (typeof baseCondition == "undefined") {
                baseCondition = "";
            }
            ExportDataToExcelByParam(url, colModel, colNames, caption, excelName, conditions, baseCondition, virtualCol);
        }
    });

}

function ChangePod() {
    var mygrid = $("#containerInfoGrid");
    var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
    var responseData = [];
    var dnitems = "";
    $.each(selRowId, function (index, val) {
        responseData.push(mygrid.getRowData(selRowId[index]));
    });
    if (responseData.length < 1) {
        CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Scripts_127", 500, "warning");
        return;
    }
    var shipments = "";
    for (var i = 0; i < responseData.length; i++) {
        dnitems += responseData[i].UId + ",";
        if (shipments.length > 0)
            shipments += ",";
        shipments += responseData[i].ShipmentId;
    }

    var iscontinue = window.confirm("@Resources.Locale.L_DNManage_Is1" + shipments + "改港");
    if (!iscontinue) {
        return;
    }
    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: true,
        url: rootPath + "BookingAction/ChangeToPOD",
        type: 'POST',
        data: {
            "Uid": dnitems
        },
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.ToogleLoading(false);
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
        },
        success: function (result) {
            if (result.IsOk == "Y") {
                CommonFunc.Notify("", result.message, 500, "success");
            }
            else {
                alert(result.message);
            }
            $("#SummarySearch").trigger("click");
        }
    });
}

var _showChangePodTemplte = '<div class="modal fade" id="ChangePodModel" Sid="">\
  <div class="modal-dialog modal-lg">\
    <div class="modal-content">\
      <div class="modal-header">\
        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
        <h4 class="modal-title">@Resources.Locale.L_DNManage_LiftCombBL</h4>\
      </div>\
      <div class="modal-body">\
            <div class="pure-g">\
                <div class="pure-u-sm-60-60">\
                    <div class="form-group">\
                        <label for="exampleInputEmail1">@Resources.Locale.L_DNManage_LiftShipInfo</label>\
                        <input type="hidden" id="hid_billid" />\
                        <div id="SMCheckList">\
                        </div>\
                    </div>\
                </div>\
            </div>\
      </div>\
      <div class="modal-footer">\
        <button type="button" class="btn btn-default" data-dismiss="modal" id="CloseSpellBillWin" >Close</button>\
        <button type="button" class="btn btn-primary" onclick="ConfirmSpellBill()" id="SpellConfirmBill">@Resources.Locale.L_Layout_Confirm</button>\
      </div>\
    </div>\
  </div>\
</div>';


function RegisterChangePodWin() {
    if ($("#ChangePodModel").length <= 0) {
        $("body").append(_showChangePodTemplte);
    }

    $('#SpellCombineBillModel').on('show.bs.modal', function () {
        var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
        var CombineInfo = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ShipmentInfo');
        var shipmentid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ShipmentId');
        $("#hid_billid").val(shipmentid);
        var CombineInfolist = CombineInfo.split(',');
        var spllhtml = "";
        if (CombineInfolist.length <= 1) {
            spllhtml += '<div>@Resources.Locale.L_DNManage_NoData</div>';
        } else {
            $.each(CombineInfolist, function (index, value) {
                spllhtml += '<div><input type="checkbox" name="spellSm" value="' + value + '" />' + value + '</div>';
            });
        }
        $("#SMCheckList").html(spllhtml);
    });
}

function _baseInitChangePodWin(){
    if ($("#podUploadWin").length <= 0) {
        $("body").append(this._showChangePodWin);
    }
    $("#POD_EXCEL_UPLOAD_FROM").submit(function () {
        var UId = $("#UId").val();
        $(this).find("input[type='hidden']").remove();
        $(this).append('<input type="hidden" name="UId" value="' + UId + '" />');
        var postData = new FormData($(this)[0]);

        $.ajax({
            url: rootPath + "BookingAction/BatchChangePOD",
            type: 'POST',
            data: postData,
            async: false,
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            },
            success: function (data) {
                //alert(data)
                CommonFunc.ToogleLoading(false);
                if (data.message != "success") {
                    CommonFunc.Notify("", "@Resources.Locale.L_BSTQuery_ImpFail" + data.message, 1300, "warning");
                    return false;
                }
                CommonFunc.Notify("", "@Resources.Locale.L_BaseBooking_Scripts_126", 500, "success");
                $("#podUploadWin").modal("hide");
                $("#SummarySearch").trigger("click");
            },
            cache: false,
            contentType: false,
            processData: false
        });

        return false;
    });
}

var _showChangePodWin = '<div class="modal fade" id="podUploadWin" role="dialog">\
    <div class="modal-dialog">\
        <div class="modal-content">\
            <div class="modal-header">\
                <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
                <h4 class="modal-title">@Resources.Locale.L_AirBooking_Script_Excel6</h4>\
            </div>\
            <form name="PARTY_EXCEL_UPLOAD_FROM" id="POD_EXCEL_UPLOAD_FROM"  method="post" enctype="multipart/form-data">\
                <div class="modal-body">\
                    <div class="pure-g">\
                        <div class="pure-u-sm-60-60">\
                            <input type="file" title="@Resources.Locale.L_BSTQuery_SelectFile" id="PackingUploadExcel" name="file"/>\
                            <input type="hidden" id="uploadKeyId" />\
                        </div>\
                    </div>\
                     <div class="pure-g">\
                        <div class="pure-u-sm-60-60">\
                             <a href="#" onclick="DownLoadChangePodExcel()" >@Resources.Locale.L_DNManage_ClickDl</a>\
                        </div>\
                    </div>\
                </div>\
                <div class="modal-footer">\
                    <button type="submit" class="btn btn-sm btn-info" id="podUploadBtn">@Resources.Locale.L_BSTQuery_Upload</button>\
                    <button type="button" class="btn btn-sm btn-danger" data-dismiss="modal" id="ModalClose">Close</button>\
                </div>\
            </form>\
        </div>\
    </div>\
</div>';