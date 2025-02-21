 var _UpdateAction = {};
//DischargeDate
_UpdateAction._showDischargeDialog = '<div class="modal fade" id="UpdateDischargeWindow">\
    <div class="modal-dialog modal-lg">\
        <div class="modal-content">\
            <div class="modal-header">\
                <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
                <h4 class="modal-title">Batch Upload Discharge Date</h4>\
            </div>\
            <div class="modal-body">\
                <button class="btn btn-sm btn-info" id="SetSameAddr2">As above</button>\
                <div class="pure-g">\
                    <div class="pure-u-sm-60-60">\
                        <p class="help-block tooltips" style="color:red">Note: please input valid date, ex: YYYY-MM-DD, YYYYMMDD, YYYY/MM/DD.</p>\
                    </div>\
                </div>\
                <table class="table">\
                </table>\
            </div>\
            <div class="modal-footer">\
                <button type="submit" class="btn btn-md btn-info" id="AddAddrBtn2">Insert</button>\
                <button type="button" class="btn btn-md btn-danger" data-dismiss="modal" id="ModalClose">Close</button>\
            </div>\
        </div>\
    </div>\
</div>';

function DateCheck(value, id) {
    var reg = /^[2-9]\d{3}(-|\/)(0[1-9]|1[0-2])(-|\/)(0[1-9]|[1-2][0-9]|3[0-1])$/;
    if (value.length == 8)
        reg = /^[2-9]\d{3}(0[1-9]|1[0-2])(0[1-9]|[1-2][0-9]|3[0-1])$/;
    if (!reg.test(value) && value.length > 0) {
        $("#" + id).val("");
        return "false";
    }
    return "";
}

_UpdateAction.RegisterUploadDischargeBtn = function () {
    if ($("#UpdateDischargeWindow").length <= 0) {
        $("body").append($(this._showDischargeDialog));
        ajustamodal("#UpdateDischargeWindow");
    }

    $("#SetSameAddr2").on("click", function () {
        var discharge = $("input[name='DischargeDate']:eq(0)").val();
        $("input[name='DischargeDate']").val(discharge);
    });
    
    $("#AddAddrBtn2").on("click", function () {
        var DischargeDatelist = $("input[name='DischargeDate']")
        var paramsdis = "";
        var checkFlag = "";
        $.each(DischargeDatelist, function (index, val) {
            var idname = val.id;
            var discharge = val.value; 
            checkFlag += DateCheck(discharge, idname);
            paramsdis += discharge + idname + ",";
        });

        if (checkFlag!="") {
            CommonFunc.Notify("", "Please Input valid date!", 500, "warning");
            return;
        }

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
        var shpFRlist = "";
        var shpOTHlist = "";
        
        $.each(selRowId, function (index, val) {
            responseData.push(mygrid.getRowData(selRowId[index]));
        });
        if (responseData.length < 1) {
            CommonFunc.Notify("", "Please Select Data", 500, "warning");
            return false;
        } 
        for (var i = 0; i < responseData.length; i++) {
            var tranType = responseData[i].TranType;
            if ("F" == tranType || "R" == tranType) { 
                shpFRlist += responseData[i].ShipmentId + ",";
            } else {
                shpOTHlist += responseData[i].ShipmentId + ",";
            }
            console.log(tranType + "id"+ responseData[i].ShipmentId);
        }
        var data1 = { "shpFRlist": shpFRlist, "shpOTHlist": shpOTHlist };
        console.log(data1);
        $.ajax({
            async: true,
            url: rootPath + "IbGateManage/DoQueryCntrOrDN",
            type: 'POST',
            data: {
                "shpFRlist": shpFRlist, "shpOTHlist": shpOTHlist,
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
        var dischargeDate = map.DischargeDate;
        if (dischargeDate == "null" || dischargeDate == null || dischargeDate == undefined || dischargeDate == "undefined") dischargeDate = "";
        dateids.push(dischargeid);
        str += '<tbody>';
        str += "<tr id='tr" + map.UId + "'>";
        str += "<td>" + map.CntrNo + "</td>";
        str += "<td style='word-break:break-all; word-wrap:break-word;'>" + map.DnNo + "</td>";
        str += "<td style='word-break:break-all; word-wrap:break-word;'>" + map.InvNo + "</td>";
        str += '<td>\
				<div class="input-group">\
					<input type="text" class="form-control input-sm" id="' + dischargeid + '"  name="DischargeDate" value="' + dischargeDate + '" />\
				</div></td>';
        str += "</tr>";
        str += '</tbody>';
    }

    $("#UpdateDischargeWindow").find(".table").html(str);

    $.each(dateids, function (index, val) {
        $("#" + val + "").wrap('<div class="input-group">').datepicker({
            showOn: "button",
            changeYear: true,
            dateFormat: "yy/mm/dd",
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

//Update ATA
_UpdateAction._showATADialog = '<div class="modal fade" id="UpdateATAWindow">\
    <div class="modal-dialog modal-lg">\
        <div class="modal-content">\
            <div class="modal-header">\
                <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
                <h4 class="modal-title">Batch Upload ATA Date</h4>\
            </div>\
            <div class="modal-body">\
                <button class="btn btn-sm btn-info" id="SetSameAddr1">As above</button>\
                <div class="pure-g">\
                    <div class="pure-u-sm-60-60">\
                        <p class="help-block tooltips" style="color:red">Note: please input valid date, ex: YYYY-MM-DD, YYYYMMDD, YYYY/MM/DD.</p>\
                    </div>\
                </div>\
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

_UpdateAction.RegisterATABtn = function () {
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
        var checkFlag = "";
        $.each(ATADatelist, function (index, val) {
            var idname = val.id;
            var ata = val.value;
            checkFlag += DateCheck(ata, idname);
            paramsdis += ata + idname + ",";
        });

        if (checkFlag != "") {
            CommonFunc.Notify("", "Please Input valid date!", 500, "warning");
            return;
        }
        console.log(paramsdis);
        $.ajax({
            async: true,
            url: rootPath + "IbGateManage/UpdateAtaInfo",
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
        var shmlist = [];
        $.each(selRowId, function (index, val) {
            responseData.push(mygrid.getRowData(selRowId[index]));
        });
        if (responseData.length < 1) {
            CommonFunc.Notify("", "Please Select Data", 500, "warning");
            return false;
        } 
        for (var i = 0; i < responseData.length; i++) {
            if (!shmlist.includes(responseData[i].ShipmentId))
                shmlist.push(responseData[i].ShipmentId);
        }
        var parm = { "shmlist": shmlist.toString() };
        console.log(parm);
        $.ajax({
            async: true,
            url: rootPath + "IbGateManage/DoQuerySMORD",
            type: 'POST',
            data: parm,
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
        var ataid = 'ATA' + map.ShipmentId;
        var ata = map.Ata;
        if (ata == "null" || ata == null || ata == undefined || ata == "undefined") ata = "";

        dateids.push(ataid);
        str += '<tbody>';
        str += "<tr id='tr" + map.ShipmentId + "'>";
        str += "<td>" + map.MasterNo + "</td>";
        str += "<td style='word-break:break-all; word-wrap:break-word;'>" + map.HouseNo + "</td>";
        str += '<td>\
				<div class="input-group">\
					<input type="text" class="form-control input-sm" id="' + ataid + '"  name="ATA" value="' + ata + '" />\
				</div></td>';
        str += "</tr>";
        str += '</tbody>';
    }

    $("#UpdateATAWindow").find(".table").html(str);

    $.each(dateids, function (index, val) {
        $("#" + val + "").wrap('<div class="input-group">').datepicker({
            showOn: "button",
            changeYear: true,
            dateFormat: "yy/mm/dd",
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

 
