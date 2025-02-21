$(function(){
	var $SiDailogGrid = $("#SiDailogGrid");
	var colModel1 = [
            { name: 'UId', showname: 'ID', sorttype: 'string', hidden: true, viewable: false },
            { name: 'Profile', showname: 'ID', index: 'Profile', width: 200, sorttype: 'string', hidden: false }
        ];

	new genGrid(
	    $SiDailogGrid,
	    {
	        datatype: "local",
	        loadonce:true,
	        colModel: colModel1,
	        caption: "Profile ID(@Resources.Locale.L_GetSiInfo_Script_135",
	        height: "auto",
	        rows: 999999,
	        refresh: false,
	        cellEdit: false,//禁用grid编辑功能
	        pginput: false,
	        sortable: true,
	        pgbuttons: false,
	        exportexcel: false,
	        toppager:false,
	        dblClickFunc: function(map){
	            var UId = map.UId;
	            top.topManager.openPage({
	                href: rootPath + "System/BSTDataSetup/" + UId+"?MenuBarPermiss=Y",
	                title: '@Resources.Locale.L_BSTQuery_CustTranSet',
	                id: 'BSTSetup',
	                search: 'uid=' + UId
	            });
	        }
	    }
	);

	$('#SiQueryDialog').on('show.bs.modal', function (e) {
		var ShipmentId = $("#ShipmentId").val();
		$.post(rootPath + 'DnManage/getSiData', {"ShipmentId": ShipmentId}, function(data, textStatus, xhr) {
			if(data.message == "success")
			{
				var gridData = $.parseJSON(data.returnData.Content);
				$SiDailogGrid.jqGrid("clearGridData");
				$SiDailogGrid.jqGrid("setGridParam", {
	                datatype: 'local',
	                data: gridData.rows
	            }).trigger("reloadGrid");
			}
			else
			{
				CommonFunc.Notify("", data.message, 500, "warning");
				return;
			}
		}, "JSON");
	});
});


var _ChangePOD = {};
_ChangePOD.lang = {
    IsOk: "Send successfully",
    BeforSend: "Uploading",
    failed: "Uploading is failed",
    title: "Change Pod Modal",
    file: "select the file",
    submitbtn: "Uplaod",
    NoData: "Please Select Data"
};
_ChangePOD.ExportToEdocInit = function () {
    if ($("#changePodDailog").length <= 0) {
        $("body").append(this._showApproveModal);
    }
};

function changepodFuc() {
    var cancelreson = $("#ChangeRemark").val();
    if (cancelreson == "") {
        CommonFunc.Notify("", "@Resources.Locale.L_ActManage_EnterReason", 500, "warning");
        return;
    }
    var torder = $("#Torder").val();
    if (torder == "C" && !confirm("@Resources.Locale.L_FCLBooking_Script_22")) {
        return;
    }
    var shipments = $("#ShipmentId").val();
    ajaxHttp(rootPath + "BookingAction/ChangeToPOD", { "shipmentid": shipments, "cancelreson": cancelreson, autoReturnData: false },
   function (result) {
       if (result.IsOk == "Y") {
           CommonFunc.Notify("", result.message, 500, "success");
           $("#CloseChangePodWin").trigger("click");
       }
       else {
           CommonFunc.Notify("", result.message, 500, "warning");
           $("#CloseChangePodWin").trigger("click");
       }
       MenuBarFuncArr.MBCancel();
       return true;
   });
}

_ChangePOD.InitButtonHandle = function () {
    $("#changePodDailog").modal("show");
}

_ChangePOD._showApproveModal = '<div class="modal fade" id="changePodDailog" Sid="">\
  <div class="modal-dialog modal-lg">\
    <div class="modal-content">\
      <div class="modal-header">\
        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
        <h4 class="modal-title">'+ _ChangePOD.lang.title + '</h4>\
      </div>\
      <div class="modal-body">\
            <div class="pure-g">\
                <div class="pure-u-sm-60-60">\
                    <div class="form-group">\
                        <label for="exampleInputEmail1">@Resources.Locale.L_DNManage_ChgReason</label>\
                        <textarea class="form-control" id="ChangeRemark" name="ChangeRemark" fieldname="ChangeRemark"></textarea>\
                    </div>\
                </div>\
            </div>\
      </div>\
      <div class="modal-footer">\
        <button type="button" class="btn " data-dismiss="modal" id="CloseChangePodWin" >Close</button>\
        <button type="button" class="btn btn-primary" onclick="changepodFuc()" id="BackConfirm">@Resources.Locale.L_Layout_Confirm</button>\
      </div>\
    </div>\
  </div>\
</div>';
