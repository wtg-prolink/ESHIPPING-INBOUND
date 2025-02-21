var mygrid = $("#containerInfoGrid");
function btnGroup(gop) {
    gop.btnGroup = [{
        id: "btn02",
        name: "@Resources.Locale.L_ActManage_Pass",
        func: function () {
            var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
            if (selRowId.length <= 0) {
                CommonFunc.Notify("", "@Resources.Locale.L_ActManage_Select1Data", 500, "warning");
                return;
            }
            var TranMode = mygrid.jqGrid('getCell', selRowId, 'TranMode');
            var approveto = mygrid.jqGrid('getCell', selRowId, 'APPROVE_TO');
            if (approveto == "Finish")
            {
                CommonFunc.Notify("", "@Resources.Locale.L_QuotApproveQuery_tip1", 500, "warning");
                return;
            }
            var UId = mygrid.jqGrid('getCell', selRowId, 'UId');
            var _confirm = confirm("@Resources.Locale.L_QuotApproveQuery_ISAPPROVE");
            if (!_confirm) {
                return;
            }
            if (UId) {
                $.ajax({
                    async: true,
                    url: rootPath + "RQManage/InitiatedCheck",
                    type: 'POST',
                    data: { UId: UId, TranMode: TranMode },
                    dataType: "json",
                    "error": function (xmlHttpRequest, errMsg) {
                        CommonFunc.Notify("", "@Resources.Locale.L_ActCheckSetup_Scripts_29", 500, "warning");
                        CommonFunc.ToogleLoading(false);
                    },
                    success: function (result) {
                        //if (!result.flag)
                        CommonFunc.Notify("", result.message, 500, "success");
                        reloadStatus();
                    }
                });
            }
        }
    },
       {
           id: "btn09",
           name: '@Resources.Locale.L_ActManage_Back',
           func: function () {
               var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
               var TranMode = mygrid.jqGrid('getCell', selRowId, 'TranMode');
               var UId = mygrid.jqGrid('getCell', selRowId, 'UId');
               $("#BackRemark").val("");
               var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
               var uid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'UId');

               if (!uid) {
                   CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "success");
                   return;
               }

               $("#ApproveBack").modal("show");
           }
       },
       {
           id: "btn05",
           name: "@Resources.Locale.L_ActManage_ApDetail",
           func: function () {
               CheckDetailed();
           }
       }];
}
function BackApprove() {
    var backremark = $("#BackRemark").val();
    if (backremark == "") {
        CommonFunc.Notify("", "@Resources.Locale.L_ActManage_EnterReason", 500, "warning");
        return;
    }
    var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
    var uid = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'UId');
    var ApproveType = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ApproveType');
    var approveTo = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ApproveTo');

    if (!uid) {
        CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
        return;
    }
    $.ajax({
        async: true,
        url: rootPath + "RQManage/ApproveBackQuot",
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