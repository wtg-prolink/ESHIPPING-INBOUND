//function InitSealMode()
//{
//    var _showApproveModal = '<div class="modal fade" id="SealDetail" sid="">\
//    <div class="modal-dialog modal-lg">\
//        <div class="modal-content">\
//            <div class="modal-header">\
//                <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
//                <h4 class="modal-title">@Resources.Locale.L_DNManage_SlRl</h4>\
//            </div>\
//            <div class="modal-body">\
//                <div class="pure-g">\
//                    <div class="pure-u-sm-60-40" id="SealList">\
//                    </div>\
//                </div>\
//            </div>\
//            <div class="modal-footer">\
//                <button type="button" class="btn btn-default" data-dismiss="modal" id="CloseBackWin">Close</button>\
//                <button type="button" class="btn btn-primary" onclick="SealDo()" id="BackConfirm">@Resources.Locale.L_BSCSDateQuery_Confirm</button>\
//            </div>\
//        </div>\
//    </div>\
//</div>';
//}
function InitSealMode(dndata)
{
    var allbool = true;
    $("#SealList").empty();
    var selStr = '<ul style="">';
    if (dndata.length <= 1) {
        $.each(dndata, function (index, val) {
            selStr += "<li><input type='checkbox' BeDone='N' value='" + dndata[index].DnNo + "' checked='checked' />" + dndata[index].DnNo + "</li>";
            all = "<div style='padding-left: 40px;'><input type='checkbox' value='*' id='checkall' checked='checked'>ALL</div>";
        });
    }
    else {
        $.each(dndata, function (index, val) {
            if (dndata[index].SealSataue == 'Y') {
                selStr += "<li><input type='checkbox' BeDone='Y' value='" + dndata[index].DnNo + "' disabled checked/>" + dndata[index].DnNo + "</li>";
            } else {
                selStr += "<li><input type='checkbox' BeDone='N' value='" + dndata[index].DnNo + "' />" + dndata[index].DnNo + "</li>";
                allbool = false;
            }
        });
    }
    selStr += '</ul>';
    var all = "<div style='padding-left: 40px;'><input type='checkbox' value='*' id='checkall' disabled checked>ALL</div>";
    if(!allbool)
        var all = "<div style='padding-left: 40px;'><input type='checkbox' value='*' id='checkall'>ALL</div>";
    $("#SealList").append(all + selStr);

    $("#checkall").change(function () {
        //alert("checked");

        var val = $(this).is(':checked');

        if (val) {
            $("input[BeDone='N']").prop("checked", true);
        }
        else {
            $("input[BeDone='N']").prop("checked", false);
            //$("input[BeDone='N']").attr("checked", false);
            //$("input[BeDone='N']").removeAttr("checked");
            //$("input[BeDone='N']").attr("checked", false);
        }
    });

    $("input[BeDone='N']").change(function () {
        var check = false;
        $("input[BeDone='N']").each(function () {
            if (!$(this).is(':checked')) {
                check = false;
                return false; 
            }
            else
                check = true;
        })
        if (check) {
            $("#checkall").prop("checked", true);
        }
        else
            $("#checkall").prop("checked", false);
    })
    $("#CloseBackWin").removeAttr("disabled");
}
function SealDo() {
    var list = "";
    
    $("input[BeDone='N']").each(function () {
        var val = $(this).is(':checked');
        if (val)
            list += $(this).val() + ";";
    });

    var all = $("#checkall").is(':checked');
    if (list != "") {
        list = list.substring(0, list.length - 1);
    }
    else if (all)
    {

    }
    else {
        alert("请至少选择一笔DN");
        return;
    }
    $.ajax({
        async: true,
        url: rootPath + "Api/CheckQty",
        type: 'POST',
        data: { id: $("#UId").val() ,dnlist:list},
        dataType: "json",
        beforeSend: function () {
            CommonFunc.ToogleLoading(true);
        },
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.Notify("", errMsg, 1000, "danger");
            CommonFunc.ToogleLoading(false);
        },
        success: function (result) {
            CommonFunc.ToogleLoading(false);
            
            if (result.flag === true) {
                if (list != "") {
                    //list = list.substring(0, list.length - 1);
                    $.ajax({
                        async: true,
                        url: rootPath + "Api/InFactoryConfirmDN",
                        type: 'POST',
                        data: { id: $("#UId").val(), dnlist: list },
                        dataType: "json",
                        beforeSend: function () {
                            CommonFunc.ToogleLoading(true);
                        },
                        "error": function (xmlHttpRequest, errMsg) {
                            CommonFunc.Notify("", errMsg, 1000, "danger");
                            CommonFunc.ToogleLoading(false);
                        },
                        success: function (result) {
                            CommonFunc.ToogleLoading(false);
                            if (result.flag) {
                                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_SealSDN", 1000, "success");
                                if (all)
                                    InFactoryConfirm();
                            }
                            else {
                                CommonFunc.Notify("", result.message, 1000, "warning");
                            }
                        }
                    });
                    initLoadData($("#UId").val());
                }
                else if (all)
                {
                    InFactoryConfirm();
                }

            }
            else {
                var truthBeTold = window.confirm(result.message + ",@Resources.Locale.L_DNManage_SealorNot");
                if (!truthBeTold) {
                    return;
                }
                if (list != "") {
                    //list = list.substring(0, list.length - 1);
                    $.ajax({
                        async: true,
                        url: rootPath + "Api/InFactoryConfirmDN",
                        type: 'POST',
                        data: { id: $("#UId").val(), dnlist: list },
                        dataType: "json",
                        beforeSend: function () {
                            CommonFunc.ToogleLoading(true);
                        },
                        "error": function (xmlHttpRequest, errMsg) {
                            CommonFunc.Notify("", errMsg, 1000, "danger");
                            CommonFunc.ToogleLoading(false);
                        },
                        success: function (result) {
                            CommonFunc.ToogleLoading(false);
                            if (result.flag) {
                                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_SealSDN", 1000, "success");

                                if (all)
                                    InFactoryConfirm();
                            }
                            else {
                                CommonFunc.Notify("", result.message, 1000, "warning");
                            }
                        }
                    });
                    initLoadData($("#UId").val());
                }
                else if (all) {
                    InFactoryConfirm();
                }
            }
        }
    });

    function InFactoryConfirm() {
            $.ajax({
                async: true,
                url: rootPath + "Api/InFactoryConfirm",
                type: 'POST',
                data: { id: $("#UId").val(), status: $("#Status").val(), LtruckNo: $("#LtruckNo").val(), Ldriver: $("#Ldriver").val(), tranType: $("#TranType").val(), cntr_no: $("#CntrNo").val(), seal_no1: $("#SealNo1").val() },
                dataType: "json",
                beforeSend: function () {
                    CommonFunc.ToogleLoading(true);
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.Notify("", errMsg, 1000, "danger");
                    CommonFunc.ToogleLoading(false);
                },
                success: function (result) {
                    CommonFunc.ToogleLoading(false);
                    if (result.message == "@Resources.Locale.L_ApiController_Controllers_28") {
                        CommonFunc.Notify("", "@Resources.Locale.L_DNManage_SealS", 1000, "success");
                        initLoadData($("#UId").val());
                    }
                    else {
                        CommonFunc.Notify("", result.message, 1000, "warning");
                    }
                    initLoadData($("#UId").val());
                }
            }
            );
        initLoadData($("#UId").val());
        $("#CloseBackWin").trigger("click");
    }
}