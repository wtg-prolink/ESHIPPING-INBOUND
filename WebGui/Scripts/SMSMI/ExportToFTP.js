
var _ExportToFTP = {};
_ExportToFTP.lang = {
    ScmIsOk: "Send successfully, please wait 2 minutes to view FTP ",
    IsOk: "Send successfully",
    BeforSend: "Uploading",
    failed: "Uploading is failed",
    title: "Batch Upload",
    file: "select the file",
    submitbtn: "Uplaod",
    NoData:"Please Select Data"
};
_ExportToFTP.ExportToEdocInit = function () {
    if ($("#uploadToEdocDailog").length <= 0) {
        $("body").append(this._showApproveModal);
    }
    $("#SMSMIUploadEDOC").bootstrapFileInput();
    $("#BATCH_UPLOAD_FROM").submit(function () {
        var postData = new FormData($(this)[0]);
        $.ajax({
            url: rootPath + "SMSMI/BatchUploadEdoc",
            type: 'POST',
            data: postData,
            async: true,
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            },
            error: function (xmlHttpRequest, errMsg) {
                resetFileInput($("#SMSMIUploadEDOC"));
                CommonFunc.Notify("", "error", 500, "warning");
                CommonFunc.ToogleLoading(false);
            },
            success: function (data) {
                //alert(data)
                resetFileInput($("#SMSMIUploadEDOC"));
                CommonFunc.ToogleLoading(false);
                if (data.IsOk == "Y") {
                    CommonFunc.Notify("", "Successfully!", 500, "success");
                } else {
                    alert(_ExportToFTP.lang.failed);
                    return false;
                }
                //CommonFunc.Notify("", _ExportToFTP.lang.success, 500, "success");
                $("#uploadToEdocDailog").modal("hide");
                $("#SummarySearch").click();
            },
            cache: false,
            contentType: false,
            processData: false
        });

        return false;
    });
    $.ajax({
        async: false,
        url: rootPath + "EDOC/GetSelectOptions",
        type: 'POST',
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var options = data.Edt;
            $("#EdocType").empty();
            $("#EdocType").append("<option value=\"\"></option>");
            $.each(options, function (idx, option) {
                $("#EdocType").append("<option value=\"" + option.cd + "\">" + option.cdDescp + "</option>");
            });
        }
    });
    this.InitShowDailoghandle();
};
_ExportToFTP.InitShowDailoghandle = function () {
    $('#uploadToEdocDailog').on('show.bs.modal', function () {
        var mygrid = $("#containerInfoGrid");
        var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
        var responseData = [];
        var uidlist = "";
        var olocation = "";
        $.each(selRowId, function (index, val) {
            responseData.push(mygrid.getRowData(selRowId[index]));
        });
        if (responseData.length < 1) {
            CommonFunc.Notify("", _ExportToFTP.lang.NoData, 500, "warning");
            return false;
        }
        var shipments = "";
        for (var i = 0; i < responseData.length; i++) {
            uidlist += responseData[i].OUid + ",";
            olocation += responseData[i].OLocation + ",";
            if (shipments.length > 0)
                shipments += ",";
            shipments += responseData[i].ShipmentId;
            if (responseData[i].Status == "E") {
                CommonFunc.Notify("", "This status is in E-Alert,So you cann't operate this Action!", 500, "warning");
                return false;
            }
        }
        $("#uidlist").val(uidlist);
        $("#olocation").val(olocation);
    });

    $('#uploadToEdocDailog').on('hidden.bs.modal', function () {
        $("#uidlist").val("");
        $("#olocation").val("");
    });
};
_ExportToFTP.InitButtonHandle = function () {
    var mygrid = $("#containerInfoGrid");
    var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
    var responseData = [];
    var uidlist = "";
    $.each(selRowId, function (index, val) {
        responseData.push(mygrid.getRowData(selRowId[index]));
    });
    if (responseData.length < 1) {
        CommonFunc.Notify("", _ExportToFTP.lang.NoData, 500, "warning");
        return false;
    }
    $("#uploadToEdocDailog").modal("show");
    resetFileInput($("#SMSMIUploadEDOC"));
}

_ExportToFTP._showApproveModal = '<div class="modal fade" id="uploadToEdocDailog" role="dialog">\
    <div class="modal-dialog">\
        <div class="modal-content">\
            <div class="modal-header">\
                <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
                <h4 class="modal-title">'+_ExportToFTP.lang.title+'</h4>\
            </div>\
           <form name="EXCEL_UPLOAD_FROM" id="BATCH_UPLOAD_FROM" method="post" enctype="multipart/form-data">\
                <div class="modal-body">\
                    <div class="pure-g">\
                        <div class="pure-u-sm-20-60">\
                        <label for="OrderCarCntType" class="control-label">EDOC Type</label>\
                    </div>\
                    <div class="pure-u-sm-33-60 control-group">\
                       <select class="form-control input-sm" name="EdocType" id="EdocType">\
                        </select>\
                    </div>\
                    </div>\
                    <div class="pure-g">\
                        <div class="pure-u-sm-60-60">\
                            <input type="file" title="' + _ExportToFTP.lang.file + '" id="SMSMIUploadEDOC" name="file" />\
                            <input type="hidden" name="uidlist" id="uidlist" />\
                            <input type="hidden" name="olocation" id="olocation" />\
                       </div>\
                    </div>\
                </div>\
                <div class="modal-footer">\
                    <button type="submit" class="btn btn-sm btn-info" id="uploadEDOCBtn">' + _ExportToFTP.lang.submitbtn + '</button>\
                    <button type="button" class="btn btn-sm btn-danger" data-dismiss="modal">Close</button>\
                </div>\
            </form>\
        </div>\
    </div>\
</div>';


_ExportToFTP.SCMInitDownHandle = function (Ismodel) {
    var mygrid = $("#containerInfoGrid");
    var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
    var responseData = [];
    var uidlist = "";
    $.each(selRowId, function (index, val) {
        responseData.push(mygrid.getRowData(selRowId[index]));
    });

    if (Ismodel == "Y") {
        //url = rootPath + 'SMSMI/DownLoadSCMRequestModel';
        var conditions = mygrid.jqGrid("getGridParam", "postData").conditions;
        CommonFunc.ToogleLoading(true);
        $.ajax({
            async: true,
            url: rootPath + "SMSMI/DownLoadSCMRequestModelToFTP",
            type: 'POST',
            data: {
                conditions: conditions
            },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
                CommonFunc.ToogleLoading(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.ToogleLoading(false);
                var resJson = $.parseJSON(errMsg)
                CommonFunc.Notify("", resJson.message, 500, "warning");
            },
            success: function (data) {
                CommonFunc.ToogleLoading(false);
                if (data.IsOk != 'Y')
                    CommonFunc.Notify("", data.message, 500, "warning");
                else {
                    CommonFunc.Notify("", _ExportToFTP.lang.ScmIsOk, 500, "success");
                }
            }
        });
        return;
    } 

    if (responseData.length < 1) {
        CommonFunc.Notify("", _ExportToFTP.lang.NoData, 500, "warning");
        return false;
    }
    var fclshipmentids = "";
    var normalshipmentids = "";
    var trantype = "";
    for (var i = 0; i < responseData.length; i++) {
        trantype = responseData[i].TranType;
        if (trantype == "R" || trantype == "F") {
            fclshipmentids += responseData[i].ShipmentId + ";";
        } else {
            normalshipmentids += responseData[i].ShipmentId + ";";
        }
    }
    var url = rootPath + 'SMSMI/DownLoadSCMRequest';
    postAndRedirect(url, { "fclshipmentids": fclshipmentids, "normalshipmentids": normalshipmentids, "Ismodel": Ismodel });

}

_ExportToFTP._showSCMDialog='<div class="modal fade" id="SCMUploadWin" role="dialog">\
    <div class="modal-dialog">\
        <div class="modal-content">\
            <div class="modal-header">\
                <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
                <h4 class="modal-title">Upload SCM Info</h4>\
            </div>\
            <form name="SCM_UPLOAD_FROM" id="SCM_UPLOAD_FROM"  method="post" enctype="multipart/form-data">\
                <div class="modal-body">\
                    <div class="pure-g">\
                        <div class="pure-u-sm-60-60">\
                            <input type="file" title="SelectFile" id="PackingUploadExcel" name="file"/>\
                            <input type="hidden" id="uploadKeyId" />\
                        </div>\
                    </div>\
                </div>\
                <div class="modal-footer">\
                    <button type="submit" class="btn btn-sm btn-info" id="modalUploadBtn">Upload</button>\
                    <button type="button" class="btn btn-sm btn-danger" data-dismiss="modal" id="ModalClose">Close</button>\
                </div>\
            </form>\
        </div>\
    </div>\
</div>'

_ExportToFTP.SCMInitUploadHandle = function (ismodel) {
    if ($("#SCMUploadWin").length <= 0) {
        $("body").append($(this._showSCMDialog));
        $("#SCM_UPLOAD_FROM").submit(function () {
            var UId = $("#UId").val();
            var isModel = $("#myModel").val();
            $(this).find("input[type='hidden']").remove();
            $(this).append('<input type="hidden" name="UId" value="' + UId + '" />');
            var uploadurl = rootPath + "SMSMI/UploadSCMAction";
            if (isModel == "Model") {
                uploadurl = rootPath + "SMSMI/UploadSCMActionByModel";
            }
            if (isModel == "ASN" || isModel == "GR") {
                $(this).append('<input type="hidden" name="type" value="' + isModel + '" />');
                uploadurl = rootPath + "SMSMI/UploadSCMActionASN";
            }
            var postData = new FormData($(this)[0]);
            $.ajax({
                url: uploadurl,
                type: 'POST',
                data: postData,
                async: false,
                beforeSend: function () {
                    CommonFunc.ToogleLoading(true);
                },
                success: function (data) {
                    //alert(data)
                    resetFileInput($("#PackingUploadExcel"));
                    CommonFunc.ToogleLoading(false);
                    if (data.message != "success") {
                        CommonFunc.Notify("", _ExportToFTP.lang.failed + data.message, 1300, "warning");
                        return false;
                    }
                    CommonFunc.Notify("", "Send successfully!", 500, "success");
                    $("#SCMUploadWin").modal("hide");
                },
                cache: false,
                contentType: false,
                processData: false
            });

            return false;
        });
    }
    $("#SCMUploadWin").modal("show");
    resetFileInput($("#PackingUploadExcel"));
    $("#SCM_UPLOAD_FROM").append('<input type="hidden" id="myModel" name="type" value="' + ismodel + '" />');
}

_ExportToFTP._showBatchSentToFtpDialog = '<div class="modal fade" id = "ConfirmBatchFtPathDialog" style="height: 250px;">\
        <div class="modal-dialog modal-sm">\
            <div class="modal-content">\
        <div class="modal-header" >\
            <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
            <h4 class="modal-title">Setting Folder Name</h4>\
                </div >\
        <div class="modal-body">\
        <div class="pure-g">\
            <button class="btn btn-sm btn-info" id="BatchFTPByDate">Set By Date</button>\
                    </div>\
         <div class="pure-g" >\
            <button class="btn btn-sm btn-info" id="BatchFTPByShipment">Set By Shipment ID</button>\
                    </div>\
        <div class="pure-g">\
            <button class="btn btn-sm btn-info" id="BatchFTPByBillNo">Set By Bill No</button>\
                    </div>\
        </div>\
<div class="modal-footer">\
    < button type = "submit" class="btn btn-sm btn-info" id = "Pdbtn" >@Resources.Locale.L_Layout_Confirm</button >\
        <button type="button" class="btn btn-sm btn-danger" data-dismiss="modal" id="ModalClose">@Resources.Locale.L_BSCSDateQuery_Cancel</button>\
			</div >\
            </div >\
        </div >\
</div>';



_ExportToFTP.initBathSendToFTP = function () {
    //EDOC Type lookup
    EdocTypeOptions = {};
    EdocTypeOptions.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookupPri";
    EdocTypeOptions.registerBtn = $("#SendToFTP");
    EdocTypeOptions.isMutiSel = true;
    EdocTypeOptions.focusItem = $("#SendToFTP");
    //mutil select add a columnID help mapping selected data
    EdocTypeOptions.columnID = "Cd";
    EdocTypeOptions.param = "&sopt_GroupId=eq&GroupId=" + groupId;
    EdocTypeOptions.param = "";
    EdocTypeOptions.baseCondition = " GROUP_ID='" + groupId + "' AND (CMP='" + basecmp + "' OR CMP='*') AND CD_TYPE='EDT'";
    var that = this;
    EdocTypeOptions.responseMethod = function (type) {
        if ($("#ConfirmBatchFtPathDialog").length <= 0) {
            $("body").append($(that._showBatchSentToFtpDialog));
            ajustamodal("#ConfirmBatchFtPathDialog");
        }
        var typeStr = "";
        var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
        var ShipmentId = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ShipmentId');
        if (ShipmentId == "" || ShipmentId == null) {
            alert(_ExportToFTP.lang.NoData);
            return;
        }

        $(type).each(function (index) {
            typeStr += type[index].Cd + ";";
        });
        var selRowIds = $("#containerInfoGrid").jqGrid('getGridParam', 'selarrrow') + "";
        var keyData = "";
        var shipmentId = "";
        selRowIds = selRowIds.split(",");
        for (var i = 0; i < selRowIds.length; i++) {
            shipmentId = $("#containerInfoGrid").jqGrid('getCell', selRowIds[i], 'ShipmentId');
            keyData += shipmentId + ",";
        }
        if (typeStr == "" || typeStr == null || typeStr == ";") {
            CommonFunc.Notify("", "Please Select the edoc type!", 500, "warning");
            return false;
        }
        var asndata = {
            ShipmentId: keyData,
            loading: true,
            TYPE: typeStr,
        };

        $("#BatchFTPByDate").unbind("click").on("click", function () {
            BatchDownFtp("Date", asndata);
        });
        $("#BatchFTPByShipment").unbind("click").on("click", function () {
            BatchDownFtp("Shipment", asndata);
        });
        $("#BatchFTPByBillNo").unbind("click").on("click", function () {
            BatchDownFtp("BillNo", asndata);
        });

        $("#ConfirmBatchFtPathDialog").modal("show");
    }
    EdocTypeOptions.lookUpConfig = LookUpConfig.EdocTypeLookup;
    initLookUp(EdocTypeOptions);
}

function BatchDownFtp(type, asndata) {
    asndata.BatchType = type;

    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: true,
        url: rootPath + "SMSMI/GetExportData1",
        type: 'POST',
        data: asndata,
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.ToogleLoading(false);
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
        },
        success: function (data) {
            CommonFunc.ToogleLoading(false);
            CommonFunc.Notify("", _ExportToFTP.lang.IsOk, 500, "success");
        }
    });

    $("#ConfirmBatchFtPathDialog").modal("hide");
}




_ExportToFTP._showPoNoDialog = '<div class="modal fade" id="UpdatePoNoWindow">\
    <div class="modal-dialog modal-lg">\
        <div class="modal-content">\
            <div class="modal-header">\
                <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
                <h4 class="modal-title">Batch Upload PO No</h4>\
            </div>\
            <div class="modal-body">\
                <button class="btn btn-sm btn-info" id="SetSameAddr">As above</button>\
                <table class="table">\
                </table>\
            </div>\
            <div class="modal-footer">\
                <button type="submit" class="btn btn-md btn-info" id="AddAddrBtn">Insert</button>\
                <button type="button" class="btn btn-md btn-danger" data-dismiss="modal" id="ModalClose">Close</button>\
            </div>\
        </div>\
    </div>\
</div>';

_ExportToFTP.RegisterUploadPoNoBtn = function () {
    if ($("#UpdatePoNoWindow").length <= 0) {
        $("body").append($(this._showPoNoDialog));
        ajustamodal("#UpdatePoNoWindow");
    }

    $("#SetSameAddr").on("click", function () {
        var discharge = $("input[name='PoNo']:eq(0)").val();
        $("input[name='PoNo']").val(discharge);
        var diswo = $("input[name='Wo']:eq(0)").val();
        $("input[name='Wo']").val(diswo);
    });

    $("#AddAddrBtn").on("click", function () {
        var PoNolist = $("input[name='PoNo']")
        var paramsdis = "";
        $.each(PoNolist, function (index, val) {
            var idname = val.id;
            var pono = val.value;
            if (!isEmpty(pono))
                paramsdis += pono + idname + ",";
        });
        var WoNolist = $("input[name='Wo']")
        $.each(WoNolist, function (index, val) {
            var idname = val.id;
            var wono = val.value;
            if (!isEmpty(wono))
                paramsdis += wono + idname + ",";
        });
        console.log(paramsdis);
        $.ajax({
            async: true,
            url: rootPath + "SMSMI/UpdatePoNoInfo",
            type: 'POST',
            data: {
                "changedData": paramsdis
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
                if (result.message == "success") {
                    CommonFunc.Notify("", result.message, 500, "success");
                }
                else {
                    alert(result.message);
                }
                $("#UpdatePoNoWindow").modal("hide");
            }
        });

        $("#AddrModal").modal("hide");
    });

    $('#UpdatePoNoWindow').on('show.bs.modal', function () {
        $("#UpdatePoNoWindow").find(".table").html('');
        var mygrid = $("#containerInfoGrid");
        var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
        var responseData = [];
        var uidlist = "";
        $.each(selRowId, function (index, val) {
            responseData.push(mygrid.getRowData(selRowId[index]));
        });
        if (responseData.length < 1) {
            CommonFunc.Notify("", _ExportToFTP.lang.NoData, 500, "warning");
            return false;
        }
        var shipments = "";
        for (var i = 0; i < responseData.length; i++) {
            uidlist += responseData[i].UId + ",";
            shipments += responseData[i].ShipmentId + ",";
            if (responseData[i].Status == "E") {
                CommonFunc.Notify("", "This status is in E-Alert,So you cann't operate this Action!", 500, "warning");
                return false;
            }
        }
        var data1 = { "Uids": uidlist };
        console.log(data1);
        $.ajax({
            async: true,
            url: rootPath + "SMSMI/DoQueryCntrOrDN_PoNo",
            type: 'POST',
            data: {
                "Uids": uidlist
            },
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            },
            success: function (data) {
                console.log(data);
                //alert(data)
                CommonFunc.ToogleLoading(false);
                var datajson = $.parseJSON(data)
                if (datajson.rows.length > 0) {
                    SetPoNo_func(datajson);
                }
            }
        });
    });
};

function SetPoNo_func(data) {
    var str = "";

    str += '<thead><tr><th>Container No</th><th style="word-break:break-all; word-wrap:break-word;">DN NO</th><th style="word-break:break-all; word-wrap:break-word;">Inv NO</th><th>PO NO</th><th>WO</th></tr></thead>';
    //var dateids = [];
    for (var i = 0; i < data.rows.length; i++) {
        var map = data.rows[i];
        var ponoid = 'PoNo' + map.UId;
        var woid='Wo'+map.UId;
        //dateids.push(ponoid);
        str += '<tbody>';
        str += "<tr id='tr" + map.UId + "'>";
        str += "<td>" + map.CntrNo + "</td>";
        str += "<td style='word-break:break-all; word-wrap:break-word;'>" + map.DnNo + "</td>";
        str += "<td style='word-break:break-all; word-wrap:break-word;'>" + (map.InvNo||'') + "</td>";
        if (map.Status == "P")
        {
            str += '<td>\
				<div class="input-group">\
					<input type="text" class="form-control input-sm" id="' + ponoid + '"  name="PoNo" value="' + (map.PoNo||'') + '" readonly />\
				</div></td>';
        }
        else
        {
            str += '<td>\
				<div class="input-group">\
					<input type="text" class="form-control input-sm" id="' + ponoid + '"  name="PoNo" value="' + (map.PoNo||'') + '" />\
				</div></td>';
        }
        str += '<td>\
            <div class="input-group">\
                <input type="text" class="form-control input-sm" id="'+ woid + '" name="Wo" value="' + (map.Wo||'') + '"/>\
            </div></td>';
        str += "</tr>";
        str += '</tbody>';
    }

    $("#UpdatePoNoWindow").find(".table").html(str);
}

_ExportToFTP._showDischargeDialog = '<div class="modal fade" id="UpdateDischargeWindow">\
    <div class="modal-dialog modal-lg">\
        <div class="modal-content">\
            <div class="modal-header">\
                <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
                <h4 class="modal-title">Batch Upload Discharge Date</h4>\
            </div>\
            <div class="modal-body">\
                <button class="btn btn-sm btn-info" id="SetSameAddr">As above</button>\
                <table class="table">\
                </table>\
            </div>\
            <div class="modal-footer">\
                <button type="submit" class="btn btn-md btn-info" id="AddAddrBtn">Insert</button>\
                <button type="button" class="btn btn-md btn-danger" data-dismiss="modal" id="ModalClose">Close</button>\
            </div>\
        </div>\
    </div>\
</div>';

_ExportToFTP.RegisterUploadDischargeBtn = function () {
    if ($("#UpdateDischargeWindow").length <= 0) {
        $("body").append($(this._showDischargeDialog));
        ajustamodal("#UpdateDischargeWindow");
    }

    $("#SetSameAddr").on("click", function () {
        var discharge = $("input[name='DischargeDate']:eq(0)").val();
        $("input[name='DischargeDate']").val(discharge);
    });

    $("#AddAddrBtn").on("click", function () {
        var DischargeDatelist = $("input[name='DischargeDate']")
        var paramsdis = "";
        $.each(DischargeDatelist, function (index, val) {
            var idname=val.id;
            var discharge = val.value;
            paramsdis += discharge + idname + ",";
        });
        console.log(paramsdis);
        $.ajax({
            async: true,
            url: rootPath + "SMSMI/UpdateDischargeInfo",
            type: 'POST',
            data: {
                "changedData": paramsdis
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
                if (result.message == "success") {
                    CommonFunc.Notify("", result.message, 500, "success");
                }
                else {
                    alert(result.message);
                }
                $("#UpdateDischargeWindow").modal("hide");
            }
        });

        $("#AddrModal").modal("hide");
    });

    $('#UpdateDischargeWindow').on('show.bs.modal', function () {
        $("#UpdateDischargeWindow").find(".table").html('');
        var mygrid = $("#containerInfoGrid");
        var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
        var responseData = [];
        var uidlist = "";
        $.each(selRowId, function (index, val) {
            responseData.push(mygrid.getRowData(selRowId[index]));
        });
        if (responseData.length < 1) {
            CommonFunc.Notify("", _ExportToFTP.lang.NoData, 500, "warning");
            return false;
        }
        var shipments = "";
        for (var i = 0; i < responseData.length; i++) {
            uidlist += responseData[i].UId + ",";
            shipments += responseData[i].ShipmentId + ",";
            if (responseData[i].Status == "E") {
                CommonFunc.Notify("", "This status is in E-Alert,So you cann't operate this Action!", 500, "warning");
                return false;
            }
        }
        var data1 = { "Uids": uidlist };
        console.log(data1);
        $.ajax({
            async: true,
            url: rootPath + "SMSMI/DoQueryCntrOrDN",
            type: 'POST',
            data: {
                "Uids": uidlist
            },
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            },
            success: function (data) {
                console.log(data);
                //alert(data)
                CommonFunc.ToogleLoading(false);
                var datajson=$.parseJSON(data)
                if (datajson.rows.length > 0) {
                    SetDischarge_func(datajson);
                }
            }
        });
    });
};

function SetDischarge_func(data) {
    var str = "";
    
    str += '<thead><tr><th>Container No</th><th style="word-break:break-all; word-wrap:break-word;">DN NO</th><th style="word-break:break-all; word-wrap:break-word;">Inv NO</th><th>Discharge Date</th></tr></thead>';
    var dateids = [];
    for (var i = 0; i < data.rows.length; i++) {
        var map = data.rows[i];
        var dischargeid = 'DgDate' + map.UId + 'DgDate' + map.TranType;
        dateids.push(dischargeid);
        var DischargeDate = map.DischargeDate;
        if (DischargeDate == "null" || DischargeDate == null || DischargeDate == undefined || DischargeDate == "undefined")
            DischargeDate = "";
        str += '<tbody>';
        str += "<tr id='tr" + map.UId + "'>";
        str += "<td>" + map.CntrNo + "</td>";
        str += "<td style='word-break:break-all; word-wrap:break-word;'>" + map.DnNo + "</td>";
        str += "<td style='word-break:break-all; word-wrap:break-word;'>" + map.InvNo + "</td>";
        str += '<td>\
				<div class="input-group">\
					<input type="text" class="form-control input-sm" id="' + dischargeid + '"  name="DischargeDate" value="' + DischargeDate + '" />\
				</div></td>';
        str += "</tr>";
        str += '</tbody>';
    }

    $("#UpdateDischargeWindow").find(".table").html(str);

    $.each(dateids, function (index, val) {
        $("#" + val + "").wrap('<div class="input-group">').datetimepicker({
            showOn: "button",
            changeYear: true,
            dateFormat: "yy/mm/dd",
            timeFormat: 'HH:mm:ss',
            beforeShow: function () {
                setTimeout(function () {
                    $('.ui-datepicker').css('z-index', 99999999999999);
                }, 0);
            },
            onClose: function (text, inst) {
                $(this).focus();
            }
        }).next("button").button({
            icons: { primary: "ui-icon-calendar" },
            label: "Select a date",
            text: false
        }).addClass("btn btn-sm btn-default").html("<span class='glyphicon glyphicon-calendar'></sapn>")
          .wrap('<span class="input-group-btn">')
          .find('.ui-button-text')
          .css({
              'visibility': 'hidden',
              'display': 'inline'
          });
    });
}

_ExportToFTP._showATADialog = '<div class="modal fade" id="UpdateATAWindow">\
    <div class="modal-dialog modal-lg">\
        <div class="modal-content">\
            <div class="modal-header">\
                <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
                <h4 class="modal-title">Batch Upload ATA Date</h4>\
            </div>\
            <div class="modal-body">\
                <button class="btn btn-sm btn-info" id="SetSameAddr1">As above</button>\
                <table class="table">\
                </table>\
            </div>\
            <div class="modal-footer">\
                <button type="submit" class="btn btn-md btn-info" id="AddAddrBtn1">Insert</button>\
                <button type="button" class="btn btn-md btn-danger" data-dismiss="modal" id="ModalClose1">Close</button>\
            </div>\
        </div>\
    </div>\
</div>';

_ExportToFTP.RegisterATABtn = function () {
    if ($("#UpdateATAWindow").length <= 0) {
        $("body").append($(this._showATADialog));
        ajustamodal("#UpdateATAWindow");
    }

    $("#SetSameAddr1").on("click", function () {
        var ATA = $("input[name='ATA']:eq(0)").val();
        $("input[name='ATA']").val(ATA);
    });

    $("#AddAddrBtn1").on("click", function () {
        var ATADatelist = $("input[name='ATA']")
        var paramsdis = "";
        $.each(ATADatelist, function (index, val) {
            var idname = val.id;
            var ata = val.value;
            paramsdis += ata + idname + ",";
        });
        console.log(paramsdis);
        $.ajax({
            async: true,
            url: rootPath + "SMSMI/UpdateAtaInfo",
            type: 'POST',
            data: {
                "changedData": paramsdis
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
                if (result.message == "success") {
                    CommonFunc.Notify("", result.message, 500, "success");
                }
                else {
                    alert(result.message);
                }
                $("#UpdateATAWindow").modal("hide");
            }
        });

        $("#AddrModal").modal("hide");
    });

    $('#UpdateATAWindow').on('show.bs.modal', function () {
        $("#UpdateATAWindow").find(".table").html('');
        var mygrid = $("#containerInfoGrid");
        var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
        var responseData = [];
        var uidlist = "";
        $.each(selRowId, function (index, val) {
            responseData.push(mygrid.getRowData(selRowId[index]));
        });
        if (responseData.length < 1) {
            CommonFunc.Notify("", _ExportToFTP.lang.NoData, 500, "warning");
            return false;
        }
        var shipments = "";
        for (var i = 0; i < responseData.length; i++) {
            uidlist += responseData[i].UId + ",";
            shipments += responseData[i].ShipmentId + ",";
            if (responseData[i].Status == "E") {
                CommonFunc.Notify("", "This status is in E-Alert,So you cann't operate this Action!", 500, "warning");
                return false;
            }
        }
        var data1 = { "Uids": uidlist };
        console.log(data1);
        $.ajax({
            async: true,
            url: rootPath + "SMSMI/DoQuerySMSMI",
            type: 'POST',
            data: {
                "Uids": uidlist
            },
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            },
            success: function (data) {
                console.log(data);
                //alert(data)
                CommonFunc.ToogleLoading(false);
                var datajson = $.parseJSON(data)
                if (datajson.rows.length > 0) {
                    SetAta_func(datajson);
                }
            }
        });
    });
};

function SetAta_func(data) {
    var str = "";

    str += '<thead><tr><th>Master No</th><th style="word-break:break-all; word-wrap:break-word;">House No</th><th>ATA Date</th></tr></thead>';
    var dateids = [];
    for (var i = 0; i < data.rows.length; i++) {
        var map = data.rows[i];
        var ataid = 'ATA' + map.UId;
        dateids.push(ataid);
        var Ata = map.Ata;
        if (Ata == "null" || Ata == null || Ata == undefined || Ata == "undefined")
            Ata = "";
        str += '<tbody>';
        str += "<tr id='tr" + map.UId + "'>";
        str += "<td>" + map.MasterNo + "</td>";
        str += "<td style='word-break:break-all; word-wrap:break-word;'>" + map.HouseNo + "</td>";
        str += '<td>\
				<div class="input-group">\
					<input type="text" class="form-control input-sm" id="' + ataid + '"  name="ATA" value="' + Ata + '" />\
				</div></td>';
        str += "</tr>";
        str += '</tbody>';
    }

    $("#UpdateATAWindow").find(".table").html(str);

    $.each(dateids, function (index, val) {
        $("#" + val + "").wrap('<div class="input-group">').datetimepicker({
            showOn: "button",
            changeYear: true,
            dateFormat: "yy/mm/dd",
            timeFormat: 'HH:mm:ss',
            beforeShow: function () {
                setTimeout(function () {
                    $('.ui-datepicker').css('z-index', 99999999999999);
                }, 0);
            },
            onClose: function (text, inst) {
                $(this).focus();
            }
        }).next("button").button({
            icons: { primary: "ui-icon-calendar" },
            label: "Select a date",
            text: false
        }).addClass("btn btn-sm btn-default").html("<span class='glyphicon glyphicon-calendar'></sapn>")
          .wrap('<span class="input-group-btn">')
          .find('.ui-button-text')
          .css({
              'visibility': 'hidden',
              'display': 'inline'
          });
    });
}


_ExportToFTP.DirectlyNBHandle = function (type) {
    var mygrid = $("#containerInfoGrid");
    var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
    var responseData = [];
    var uids = "";
    $.each(selRowId, function (index, val) {
        responseData.push(mygrid.getRowData(selRowId[index]));
    });
    if (responseData.length < 1) {
        CommonFunc.Notify("", "Please Select data!", 500, "warning");
        return;
    }
    var shipments = "";
    for (var i = 0; i < responseData.length; i++) {
        if (responseData[i].Status == "S") {
            CommonFunc.Notify("", "This status is in ISF Sending,So you cann't operate this Action!", 500, "warning");
            return false;
        }
        if (responseData[i].Status == "E") {
            CommonFunc.Notify("", "This status is in E-Alert,So you cann't operate this Action!", 500, "warning");
            return false;
        }
        uids += responseData[i].UId + ",";
    }
    var onlynotifybroker = false;

    var iscontinue = false;
    if (!isEmpty(type) && "IBTC" === type) {
        iscontinue = window.confirm("Are You Sure To Send This Shipment：" + shipments + "To Directly Notify Transit Broker？");
    }
    else if (!isEmpty(type) && "IBBR" === type) {
        iscontinue = window.confirm("Are You Sure To Send This Shipment：" + shipments + "To Directly Notify Broker？");
    }
    if (!iscontinue && !isEmpty(type)) {
        return;
    }
    var types = type;
    if (isEmpty(type)) {
        onlynotifybroker = true;
        types = "IBBR";
    }


    
    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: true,
        url: rootPath + "SMSMI/DoDirectlyNB",
        type: 'POST',
        data: {
            "Uids": uids,
            "Type": types,
            "onlynotifybroker": onlynotifybroker
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
            if (result.message == "success") {
                CommonFunc.Notify("", result.message, 500, "success");
            }
            else {
                alert(result.message);
            }
            $("#UpdateDischargeWindow").modal("hide");
        }
    });
}

_ExportToFTP.InboundTracking = function () {
    if ($("#CargoDetail").length <= 0) {
        var _showCargoDetailDialog = '<div class="modal fade" id="CargoDetail" Sid="">\
                      <div class="modal-dialog modal-lg">\
	                    <div class="modal-content">\
	                      <div class="modal-header">\
		                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
		                    <h4 class="modal-title">Tracking Status</h4>\
	                      </div>\
	                      <div class="modal-body">\
			                    <div class="pure-g">\
				                    <div class="pure-u-sm-60-60">\
                                            <table id="CargoGrid" class="_tableGrid" style="width: 100%">\
									                    <tr></tr>\
								                    </table>\
				                    </div>\
			                    </div>\
	                      </div>\
	                      <div class="modal-footer">\
		                    <button type="button" class="btn btn-default" data-dismiss="modal" id="CloseCargoDetail" >Close</button>\
	                      </div>\
	                    </div>\
                      </div>\
                    </div>';
        $("body").append($(_showCargoDetailDialog));

        var _CargoGridcolModel = [
                { name: 'StsCd', title: 'Status Code', index: 'StsCd', sorttype: 'string', width: 100, hidden: false, search: false },
                { name: 'StsDescp', title: 'Description', index: 'StsDescp', sorttype: 'string', width: 300, hidden: false, search: false, frozen: true },
                { name: 'EvenDate', title: 'Event Date', index: 'EvenDate', sorttype: 'string', width: 130, hidden: false, search: false, frozen: true },
                { name: 'Location', title: 'Location', index: 'Location', sorttype: 'string', width: 100, hidden: false, search: false, frozen: true },
                { name: 'Remark', title: 'Remark', index: 'Remark', sorttype: 'string', width: 300, hidden: false, search: false, frozen: true },
                { name: 'EvenTmg', title: 'GMT', index: 'EvenTmg', sorttype: 'string', width: 170, hidden: false, search: true, searchoptions: { sopt: ['cn'] }, frozen: true },
                { name: 'CreateBy', title: 'Sender', index: 'CreateBy', sorttype: 'string', width: 100, hidden: false, search: true, searchoptions: { sopt: ['cn'] }, frozen: true },
                { name: 'CreateDate', title: 'Sender Time', index: 'CreateDate', sorttype: 'string', width: 130, hidden: false, search: false, frozen: true }
        ];
        var altura = $(window).height() - 280;
        if (altura < 300) {
            altura = 300;
        }       
        new initGrid(
            $("#CargoGrid"),
            {
                data: [],
                colModel: _CargoGridcolModel,
                beforeSelectRowFunc: function (rowid) {

                }
            },
            {
                
                loadonce: true,
                cellEdit: false,//禁用grid编辑功能
                caption: "Tracking Status",
                height: altura,
                refresh: true,
                rows: 999999,
                exportexcel: false,
                pginput: false,
                pgbuttons: false,
                sortorder: "Desc",
                sortname: "EvenDate",
                savelayout: true,
                showcolumns: true,
                footerrow: false,
                dblClickFunc: function (map) {
                },
                loadComplete: function (data) {

                }

            });
    }

    //CheckCargoDetailed();
    var id = $("#containerInfoGrid").jqGrid('getGridParam', "selrow");
    var map = $("#containerInfoGrid").jqGrid('getRowData', id);
    var ShipmentId;
    if (map) ShipmentId = map.ShipmentId;

    if (ShipmentId==""||ShipmentId==null||ShipmentId==undefined) {
        CommonFunc.Notify("", _ExportToFTP.lang.NoData, 500, "warning");
        return false;
    }
    $.post(rootPath + 'BookingAction/GetStatus', { "ShipmentId": ShipmentId, "Pagesize": 100 }, function (data, textStatus, xhr) {
        CommonFunc.ToogleLoading(false);
        $("#CargoGrid").jqGrid("clearGridData");
        $("#CargoGrid").jqGrid("setGridParam", {
            datatype: 'local',
            sortorder: "desc",
            sortname: "EvenDate",
            data: data,
        }).trigger("reloadGrid");
        $('#CargoDetail').modal('show');
        ajustamodal("#CargoDetail");
    }, "JSON");

};

_ExportToFTP.InboundLSPFeeHandler = function () {
    var id = $("#containerInfoGrid").jqGrid('getGridParam', "selrow");
    var map = $("#containerInfoGrid").jqGrid('getRowData', id);
    var UId;
    if (map) UId = map.UId;

    if (UId === undefined || UId === "" || UId == null) {
        CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", "Please Select a Record"), 500, "warning");
        return false;
    }

    this.getData(rootPath + "IQTManage/CostDetail", { uid: UId, shipmentId: map.ShipmentId }, function (result) {
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

_ExportToFTP.getData=function(url, data, callBackFn) {
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

//To Door Delivery
_ExportToFTP.ToDoorDeliverHandler = function () {
    var mygrid = $("#containerInfoGrid");
    var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
    var responseData = [];
    var uids = "";
    $.each(selRowId, function (index, val) {
        responseData.push(mygrid.getRowData(selRowId[index]));
    });
    //DLV TERM=DDP,CIP,DAP的才可以案;只能在Unreach才能案
    if (responseData.length < 1) {
        CommonFunc.Notify("", "Please Select data!", 500, "warning");
        return;
    }
    var shipments = "";
    for (var i = 0; i < responseData.length; i++) {
        if (responseData[i].Status == "S") {
            CommonFunc.Notify("", "This status is in ISF Sending,So you cann't operate this Action!", 500, "warning");
            return false;
        }
        if (responseData[i].Status == "E") {
            CommonFunc.Notify("", "This status is in E-Alert,So you cann't operate this Action!", 500, "warning");
            return false;
        }
        uids += responseData[i].UId + ",";
        shipments += responseData[i].ShipmentId + ",";
    }
    var iscontinue = window.confirm("Are You Sure To Send This Shipment：" + shipments + "To Door Deliver？");
    if (!iscontinue) {
        return;
    }
    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: true,
        url: rootPath + "SMSMI/DoToDoorDeliver",
        type: 'POST',
        data: {
            "Uids": uids
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
            if (result.message == "success") {
                CommonFunc.Notify("", result.message, 500, "success");
            }
            else {
                alert(result.message);
            }
            $("#UpdateDischargeWindow").modal("hide");
        }
    });
}


function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}

function resetFileInput(file) {
    file.after(file.clone().val(""));
    file.remove();
    $(".file-input-name").html("");
}