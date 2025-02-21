var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null };

var url = "";
$SubGrid1 = $("#SubGrid1");
$SubGrid2 = $("#SubGrid2");
$SubGrid3 = $("#SubGrid3");
var _oldDeatiArray = {};
var upload = "";

$(function () {
    schemas = JSON.parse(decodeHtml(schemas));
    CommonFunc.initField(schemas);
    setdisabled(true);

    $('#PackingUploadExcel').bootstrapFileInput();

    _initMenu();

    var listBar1 = [];
    var listBar2 = [];
    listBar2.push({
        menuId: "IPQ05", menuName: _getLang("L_DNManage_EpDecla", "出口报关") + " Packing List", menuFunc: function () {
            var BatchFlagp = $("#BatchFlagp").val();
            if (BatchFlagp == "B") {
                CommonFunc.Notify("", _getLang("L_InvPkgQuery_BATCHFLAGPP", "此票出口报关Packing报表导入排程，请等候！"), 500, "success");
                return;
            }
            CheckEdoc("PACKO")

        }, menuCss: "glyphicon glyphicon-print"
    });

    listBar2.push({
        menuId: "IPQ06", menuName: _getLang("L_DNManage_EpDecla", "出口报关") + " INVOICE", menuFunc: function () {
            var BatchFlagi = $("#BatchFlagi").val();
            if (BatchFlagi == "B") {
                CommonFunc.Notify("", _getLang("L_InvPkgQuery_BATCHFLAGIP", "此票出口报关Invoice报表导入排程，请等候！"), 500, "success");
                return;
            }
            CheckEdoc("INVO")
        }, menuCss: "glyphicon glyphicon-print"
    });

    listBar2.push({
        menuId: "IPQ03", menuName: "CKD-SKD Packing List", menuFunc: function () {

            var id = $("#DnUid").val();
            var shipmentid = $("#ShipmentId").val();
            saveReport2Shipment(id, shipmentid, "IPQ03", "CKD-SKD Packing List");

        }, menuCss: "glyphicon glyphicon-print"
    });
    listBar2.push({
        menuId: "IPQ03C", menuName: "NEW CKD-SKD Packing List", menuFunc: function () {
            var UId = $("#UId").val();
            var shipmentid = $("#ShipmentId").val();
            if (!UId) {
                alert(_getLang("L_InvPkgSetup_Script_141", "没有资料！"));
                return;
            }
            saveReport("IPQ03C", "NEW CKD-SKD Packing List", UId, shipmentid, "FOC_IB_INV_Inc")
        }, menuCss: "glyphicon glyphicon-print"
    });
    listBar2.push({
        menuId: "IPQ03PL", menuName: "CKD-SKD Packing Inbound PL", menuFunc: function () {
            var UId = $("#UId").val();
            var shipmentid = $("#ShipmentId").val();
	    var reportname=$("#InvNo").val();
            if(reportname==""||reportname==null||reportname==undefined){
                reportname="CKD-SKD Packing Inbound PL";
            }
            if (!UId) {
                alert(_getLang("L_InvPkgSetup_Script_141", "没有资料！"));
                return;
            }
            saveReport("IPQ03PL", reportname, UId, shipmentid, "PACKI", "xls")
        }, menuCss: "glyphicon glyphicon-print"
    });
    listBar2.push({
        menuId: "IPQ03BR", menuName: "CKD-SKD Packing Inbound BR", menuFunc: function () {
            var UId = $("#UId").val();
            var shipmentid = $("#ShipmentId").val();
            if (!UId) {
                alert(_getLang("L_InvPkgSetup_Script_141", "没有资料！"));
                return;
            }
            saveReport("IPQ03C", "CKD-SKD Packing Inbound BR", UId, shipmentid, "PACKI", "xls")
        }, menuCss: "glyphicon glyphicon-print"
    });
    listBar2.push({
        menuId: "IPQ05C", menuName: "CKD-SKD TTL PACKING", menuFunc: function () {
            var id = $("#DnUid").val();
            var shipmentid = $("#ShipmentId").val();
            saveReport2Shipment(id, shipmentid, "IPQ05CNEW", "CKD-SKD TTL PACKING");

        }, menuCss: "glyphicon glyphicon-print"
    });

    listBar2.push({
        menuId: "IPQ0TTLPKG", menuName: "INBOUND CKD-SKD TTL PACKING", menuFunc: function () {
            var id = $("#DnUid").val();
            var shipmentid = $("#ShipmentId").val();
            saveReport2Shipment(id, shipmentid, "IPQ0TTLPKG", "INBOUND CKD-SKD TTL PACKING");

        }, menuCss: "glyphicon glyphicon-print"
    });

    listBar2.push({
        menuId: "IPQ04", menuName: "CKD-SKD INVOICE", menuFunc: function () {

            var id = $("#DnUid").val();
            var shipmentid = $("#ShipmentId").val();
            saveReport2Shipment(id, shipmentid, "IPQ04", "CKD-SKD Invoice");

        }, menuCss: "glyphicon glyphicon-print"
    });
    listBar2.push({
        menuId: "IPQ01", menuName: _getLang("L_DNManage_IpCla", "进口清关") + " Packing List", menuFunc: function () {

            var id = $("#DnUid").val();
            var shipmentid = $("#ShipmentId").val();
            saveReport2Shipment(id, shipmentid, "IPQ01", _getLang("L_DNManage_IpCla", "进口清关") + " Packing List");

        }, menuCss: "glyphicon glyphicon-print"
    });
    listBar2.push({
        menuId: "IPQ02", menuName: _getLang("L_DNManage_IpCla", "进口清关") + " INVOICE", menuFunc: function () {

            var id = $("#DnUid").val();
            var shipmentid = $("#ShipmentId").val();
            saveReport2Shipment(id, shipmentid, "IPQ02", _getLang("L_DNManage_IpCla", "进口清关") + " Invoice");

        }, menuCss: "glyphicon glyphicon-print"
    });
    listBar2.push({
        menuId: "IPQ06C", menuName: _getLang("L_QTSetup_Cntr", "合约"), menuFunc: function () {

            var BatchFlagc = $("#BatchFlagc").val();
            if (BatchFlagc == "B") {
                CommonFunc.Notify("", _getLang("L_InvPkgQuery_BATCHFLAGCP", "此票合同已导入排程，请等候！"), 500, "success");
                return;
            }
            CheckEdoc("CONTRACT")

            //var id = $("#DnUid").val();
            //var shipmentid = $("#ShipmentId").val();
            //saveReport2Shipment(id, shipmentid, "IPQ06C", _getLang("L_QTSetup_Cntr", "合约"));

        }, menuCss: "glyphicon glyphicon-print"
    });

    listBar2.push({
        menuId: "IPQ07", menuName: "Packing-Incoterm", menuFunc: function () {
            var id = $("#UId").val();
            var shipmentid = $("#ShipmentId").val();
            saveReport("IPQ07", "Packing-Incoterm", id, shipmentid, "FOC_IB_PKL_Inc")
        }, menuCss: "glyphicon glyphicon-print"
    });
    listBar2.push({
        menuId: "IPQ08", menuName: "Invoice-Incoterm", menuFunc: function () {
            var id = $("#UId").val();
            var shipmentid = $("#ShipmentId").val();
            saveReport("IPQ08", "Invoice-Incoterm", id, shipmentid, "FOC_IB_INV_Inc")
        }, menuCss: "glyphicon glyphicon-print"
    });
    listBar2.push({
        menuId: "IPQ09", menuName: "Packing-DLV Term", menuFunc: function () {
            var id = $("#UId").val();
            var shipmentid = $("#ShipmentId").val();
            saveReport("IPQ09", "Packing-DLV Term", id, shipmentid, "FOC_IB_PKL_DLV");

        }, menuCss: "glyphicon glyphicon-print"
    });
    listBar2.push({
        menuId: "IPQ10", menuName: "Invoice-DLV Term", menuFunc: function () {
            var id = $("#UId").val();
            var shipmentid = $("#ShipmentId").val();
            saveReport("IPQ10", "Invoice-DLV Term", id, shipmentid, "FOC_IB_INV_DLV");

        }, menuCss: "glyphicon glyphicon-print"
    });


    listBar1.push({
        menuId: "IPQ05P", menuName: _getLang("L_DNManage_EpDecla", "出口报关") + " Packing List", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            genReport(thisuid, "IPQ05", _getLang("L_DNManage_EpDecla", "出口报关") + " Packing List");
        }, menuCss: "glyphicon glyphicon-list-alt"
    });

    listBar1.push({
        menuId: "IPQ06P", menuName: _getLang("L_DNManage_EpDecla", "出口报关") + " Invoice", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            genReport(thisuid, "IPQ06", _getLang("L_DNManage_EpDecla", "出口报关") + " Invoice");
        }, menuCss: "glyphicon glyphicon-list-alt"
    });

    listBar1.push({
        menuId: "IPQ03P", menuName: "CKD-SKD Packing List", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            genReport(thisuid, "IPQ03", "CKD-SKD Packing List");
        }, menuCss: "glyphicon glyphicon-list-alt"
    });
    listBar1.push({
        menuId: "IPQ03CP", menuName: "NEW CKD-SKD Packing List", menuFunc: function () {
            var UId = $("#UId").val();
            //var shipments = $("#ShipmentId").val();
            if (!UId) {
                alert(_getLang("L_InvPkgSetup_Script_141", "没有资料！"));
                return;
            }
            var params = {
                //currentCondition: "",
                //val: dnNo,
                UId: UId,
                rptdescp: "NEW CKD-SKD Packing List",
                rptName: "IPQ03C",
                formatType: 'xls',
                exportType: 'PREVIEW',
            };
            showReport(params);
        }, menuCss: "glyphicon glyphicon-list-alt"
    });
    listBar1.push({
        menuId: "IPQ03PLP", menuName: "CKD-SKD Packing Inbound PL", menuFunc: function () {
            var UId = $("#UId").val();
 	    var reportname=$("#InvNo").val();
            if(reportname==""||reportname==null||reportname==undefined){
                reportname="CKD-SKD Packing Inbound PL";
            }
            //var shipments = $("#ShipmentId").val();
            if (!UId) {
                alert(_getLang("L_InvPkgSetup_Script_141", "没有资料！"));
                return;
            }
            var params = {
                //currentCondition: "",
                //val: dnNo,
                UId: UId,
                rptdescp: reportname,
                rptName: "IPQ03PL",
                formatType: 'xls',
                exportType: 'PREVIEW',
            };
            showReport(params);
        }, menuCss: "glyphicon glyphicon-list-alt"
    });
    listBar1.push({
        menuId: "IPQ03BRP", menuName: "CKD-SKD Packing Inbound BR", menuFunc: function () {
            var UId = $("#UId").val();
            //var shipments = $("#ShipmentId").val();
            if (!UId) {
                alert(_getLang("L_InvPkgSetup_Script_141", "没有资料！"));
                return;
            }
            var params = {
                //currentCondition: "",
                //val: dnNo,
                UId: UId,
                rptdescp: "CKD-SKD Packing Inbound BR",
                rptName: "IPQ03C",
                formatType: 'xls',
                exportType: 'PREVIEW',
            };
            showReport(params);
        }, menuCss: "glyphicon glyphicon-list-alt"
    });
    listBar1.push({
        menuId: "IPQ05CP", menuName: "CKD-SKD TTL PACKING", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            genReport(thisuid, "IPQ05CNEW", "CKD-SKD TTL PACKING");
        }, menuCss: "glyphicon glyphicon-list-alt"
    });

    listBar1.push({
        menuId: "IPQ0TTLPKGP", menuName: "INBOUND CKD-SKD TTL PACKING", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            genReport(thisuid, "IPQ0TTLPKG", "INBOUND CKD-SKD TTL PACKING");
        }, menuCss: "glyphicon glyphicon-list-alt"
    });

    listBar1.push({
        menuId: "IPQ04P", menuName: "CKD-SKD Invoice", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            genReport(thisuid, "IPQ04", " CKD-SKD Invoice");
        }, menuCss: "glyphicon glyphicon-list-alt"
    });

    listBar1.push({
        menuId: "IPQ01P", menuName: _getLang("L_DNManage_IpCla", "进口清关") + " Packing List", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            genReport(thisuid, "IPQ01", _getLang("L_DNManage_IpCla", "进口清关") + " Packing List");
        }, menuCss: "glyphicon glyphicon-list-alt"
    });

    listBar1.push({
        menuId: "IPQ02P", menuName: _getLang("L_DNManage_IpCla", "进口清关") + " Invoice", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            genReport(thisuid, "IPQ02", _getLang("L_DNManage_IpCla", "进口清关") + " Invoice");
        }, menuCss: "glyphicon glyphicon-list-alt"
    });

    listBar1.push({
        menuId: "IPQ06CP", menuName: _getLang("L_QTSetup_Cntr", "合约"), menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            genReport(thisuid, "IPQ06C", _getLang("L_QTSetup_Cntr", "合约"));
        }, menuCss: "glyphicon glyphicon-list-alt"
    });
    listBar1.push({
        menuId: "IPQ07P", menuName: "Packing-Incoterm", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            genReport(thisuid, "IPQ07", "Packing-Incoterm");
        }, menuCss: "glyphicon glyphicon-list-alt"
    });
    listBar1.push({
        menuId: "IPQ08P", menuName: "Invoice-Incoterm", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            genReport(thisuid, "IPQ08", "Invoice-Incoterm");
        }, menuCss: "glyphicon glyphicon-list-alt"
    });
    listBar1.push({
        menuId: "IPQ09P", menuName: "Packing-DLV Term", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            genReport(thisuid, "IPQ09", "Packing-DLV Term");
        }, menuCss: "glyphicon glyphicon-list-alt"
    });
    listBar1.push({
        menuId: "IPQ10P", menuName: "Invoice-DLV Term", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            genReport(thisuid, "IPQ10", "Invoice-DLV Term");
        }, menuCss: "glyphicon glyphicon-list-alt"
    });
    MenuBarFuncArr.AddDDLMenu("MBPreview", " glyphicon glyphicon-print", _getLang("L_ActManage_Preview", "报表预览"), function () { }, null, listBar1);

    MenuBarFuncArr.AddDDLMenu("MBPrint", " glyphicon glyphicon-print", _getLang("L_DNManage_StatementFile", "报表归档"), function () { }, null, listBar2);

    MenuBarFuncArr.AddMenu("MBReload", "glyphicon glyphicon-repeat", "Reload Invoice", function () {
        var ShipmentId = $("#ShipmentId").val();
        var DnNo = $("#DnNo").val();
        var InvoiceType = $("#InvoiceType").val();

        if (DnNo != "") {
            $("#InvNo").val(DnNo.substr(4, DnNo.length));
        }


        GetInvByDn(DnNo);
    });

    MenuBarFuncArr.AddMenu("MBTosap", "glyphicon glyphicon-repeat", _getLang("L_DNManage_FSAP", "运费抛SAP"), function () {
        var ShipmentId = $("#ShipmentId").val();
        var UId = $("#UId").val();
        $.ajax({
            async: true,
            url: rootPath + "Invoice/Fee2Sap",
            type: 'POST',
            dataType: 'JSON',
            data: {
                ShipmentId: ShipmentId,
                UId: UId
            },
            "beforeSend": function (xmlHttpRequest, successMsg) {
                CommonFunc.ToogleLoading(true);
                StatusBarArr.nowStatus(_getLang("L_InvPkgSetup_Script_138", "抛转中…"));
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.ToogleLoading(false);
                var resJson = $.parseJSON(errMsg)
                CommonFunc.Notify("", resJson.message, 500, "warning");
            },
            success: function (result) {
                CommonFunc.ToogleLoading(false);
                StatusBarArr.nowStatus("");
                console.log(result);
                if (result.IsSucceed)
                    CommonFunc.Notify("", result.message, 500, "success");
                else
                    alert(result.message);
                initLoadData(UId);
            }
        });
    });

    MenuBarFuncArr.AddMenu("MBSEND", " glyphicon glyphicon-usd", _getLang("L_DNManage_NotiDecla", "通知报关"), function () {
        var shipmentid = $("#ShipmentId").val();
        if (shipmentid === undefined || shipmentid === "" || shipmentid == null) {
            alert(_getLang("L_InvPkgSetup_Script_136", "Shipment信息为空，无法通知报关"));
            return false;
        }
        $.ajax({
            async: true,
            url: rootPath + "BookingAction/DECLBookActionInvPkg",
            type: 'POST',
            data: {
                "ShipmentId": shipmentid
            },
            "complete": function (xmlHttpRequest, successMsg) {

            },
            "error": function (xmlHttpRequest, errMsg) {
                var resJson = $.parseJSON(errMsg)
                CommonFunc.Notify("", resJson.message, 500, "warning");
            },
            success: function (result) {
                if (result.IsOk == "Y") {
                    CommonFunc.Notify("", result.message, 500, "success");
                } else {
                    CommonFunc.Notify("", result.message, 500, "warning");
                }
                $("#SummarySearch").trigger("click");
            }
        });
    });

    MenuBarFuncArr.AddMenu("btnPackingSendToSAP", " glyphicon glyphicon-usd", _getLang("TLB_PackingSendToSAP", "发送Packing Item"), function () {
        var uid = $("#UId").val();
        if (uid === undefined || uid === "" || uid == null) {
            alert(_getLang("L_InvPkgSetup_Script_NoData", "No Data！"));
            return false;
        }
        var iscontinue = window.confirm("确认要发送Packing Item 到SAP吗？");
        if (!iscontinue) {
            return;
        }
        $.ajax({
            async: true,
            url: rootPath + "GateManage/HandSendPackingToSAP",
            type: 'POST',
            data: {
                "uids": uid
            },
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                var resJson = $.parseJSON(errMsg)
                CommonFunc.Notify("", resJson.message, 500, "warning");
            },
            success: function (result) {
                if (result.IsOk == "Y") {
                    CommonFunc.Notify("", result.message, 500, "success");
                } else {
                    CommonFunc.Notify("", result.message, 500, "warning");
                }
            }
        });
    });

    _initLookup();
    _initGenGrid();
    initLoadData(_uid);


    $("#PACKING_EXCEL_UPLOAD_FROM").submit(function () {
        var UId = $("#UId").val();
        var InvoiceType = $("#InvoiceType").val();
        var ShipmentId = $("#ShipmentId").val();
        var DnNo = $("#DnNo").val();
        var InvNo = $("#InvNo").val();
        $(this).find("input[type='hidden']").remove();
        $(this).append('<input type="hidden" name="UId" value="' + UId + '" />');
        $(this).append('<input type="hidden" name="InvoiceType" value="' + InvoiceType + '" />');
        $(this).append('<input type="hidden" name="ShipmentId" value="' + ShipmentId + '" />');
        $(this).append('<input type="hidden" name="DnNo" value="' + DnNo + '" />');
        $(this).append('<input type="hidden" name="InvNo" value="' + InvNo + '" />');
        var postData = new FormData($(this)[0]);

        $.ajax({
            url: rootPath + "GateManage/UploadPackingNew",
            type: 'POST',
            data: postData,
            async: false,
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            },
            complete: function () {
            },
            success: function (data) {
                //alert(data)
                CommonFunc.ToogleLoading(false);
                if (data.message != "success") {
                    CommonFunc.Notify("", _getLang("L_ActDeatilManage_Views_116", "汇入失败") + data.message, 1300, "warning");
                    return false;
                }
                CommonFunc.Notify("", _getLang("L_BSTQuery_ImpSuc", "汇入成功"), 500, "success");
                $("#PkgUnitDesc").val(data.PkgUnitDesc);
                $("#Pltu").val(data.Pltu);
                $("#TtlPlt").val(data.TtlPlt);
                $("#UploadBy").val(data.UploadBy);
                $("#Marks").val(data.Marks);
                $("#SupplierInvNo").val(data.supplierNo);
                upload = "success";
                $("#uploadExcelFile").modal("hide");
                $("#PackingFrom").val("P");

                $SubGrid1.jqGrid("clearGridData");
                $SubGrid1.jqGrid("setGridParam", {
                    url: rootPath + "DNManage/GetIndGridData",
                    rowNum: 999,
                    postData: { UId: $("#UId").val() },
                    page: 1,
                    datatype: "json"
                }).trigger("reloadGrid");

                $SubGrid2.jqGrid("clearGridData");
                $SubGrid2.jqGrid("setGridParam", {
                    url: rootPath + "DNManage/GetPkgGridData",
                    rowNum: 999,
                    postData: { UId: $("#UId").val() },
                    page: 1,
                    datatype: "json"
                }).trigger("reloadGrid");
                updateSmrv();
            },
            cache: false,
            contentType: false,
            processData: false
        });

        return false;
    });

    $("#TradeTerm").on("change", function () {
        setTtlValue();
    });

    $("#FreightFee").on("change", function () {
        caluFobValue();
        setFix();
    });

    $("#IssueFee").on("change", function () {
        caluFobValue();
        setFix();
    });

    $("#TtlValue").on("change", function () {
        setTtlValue();
        setFix();
    });

    $("#Etd").on("change", function () {
        var Etd = $(this).val();
        if (Etd) {
            Etd = Etd.substr(0, 4);

            $.post(rootPath + 'Invoice/getTir', { Etd: Etd }, function (data, textStatus, xhr) {
                tir = isNaN(parseFloat(data.tir)) ? 0 : parseFloat(data.tir);
                setTtlValue();
            });
        }
    });

    $("#transfer2Inv").click(function (event) {
        var ShipmentId = $("#ShipmentId").val(), InvoiceType = $("#InvoiceType").val(), DnNo = $("#DnNo").val(), InvNo = $("#InvNo").val();
        /*
    	if(ShipmentId == "" || InvoiceType == "" || DnNo == "" || InvNo == "")
    	{
    		alert("请检查Shipment ID, Invoice Type, DN NO, Invoice No. 都有输入");
    		return false;
    	}
		*/
        if (!confirm(_getLang("L_DNManage_EntIvClear", "转入Invoice时，会把原来的Invoice明细清掉，确定要转入吗？"))) {
            return false;
        }
        var allData2 = $SubGrid2.find("tr");
        $.each(allData2, function (i, val) {
            if (typeof val.id != "undefined" && val.id != "") {
                setGridVal($SubGrid2, val.id, 'SeqNo', i, null);
                if (val.id.indexOf("jqg") == -1) {
                    //console.log(val.id);
                    setGridVal($SubGrid2, val.id, 'rn', val.id, null);
                }
            }
        });

        var changeData = {};
        var SubGridChageArray2 = $SubGrid2.jqGrid('getGridParam', "arrangeGrid")();
        changeData["st2"] = SubGridChageArray2;
        $.post(rootPath + 'Invoice/transfer2Inv', { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val(), ShipmentId: ShipmentId, InvoiceType: InvoiceType, DnNo: DnNo, InvNo: InvNo }, function (data, textStatus, xhr) {
            if (data.message != "success") {
                CommonFunc.Notify("", _getLang("L_DNManage_EnterF", "转入失败"), 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
                return false;
            }
            $SubGrid1.jqGrid("clearGridData");
            $SubGrid1.jqGrid("setGridParam", {
                datatype: 'local',
                data: data.subData1
            }).trigger("reloadGrid");

            $SubGrid2.jqGrid("clearGridData");
            $SubGrid2.jqGrid("setGridParam", {
                datatype: 'local',
                data: data.subData2
            }).trigger("reloadGrid");

            CommonFunc.Notify("", _getLang("L_DNManage_EntS", "转入成功"), 500, "success");
        }, "JSON");
    });

    $("#copyFirstDn2Pkg").click(function () {
        var DnNo = $("#DnNo").val();
        var UId = $("#UId").val();
        if (DnNo == "") {
            alert(_getLang("L_InvPkgSetup_Script_139", "请先输入Dn") + " No");
            return;
        }

        if (UId == "") {
            alert(_getLang("L_DNManage_SaveandCopy", "请先保存，再复制"));
            return;
        }

        if (confirm(_getLang("L_DNManage_CopyfirstPacking", "复制首段Packing，确定要执行吗？"))) {
            $.ajax({
                async: true,
                url: rootPath + "Invoice/copyFirstDn2Packing",
                type: 'POST',
                data: { UId: UId, DnNo: DnNo },
                dataType: "json",
                beforeSend: function () {
                    CommonFunc.ToogleLoading(true);
                },
                "error": function (xmlHttpRequest, errMsg) {
                },
                success: function (result) {

                    if (result.message != "success") {
                        CommonFunc.Notify("", result.message, 500, "warning");
                        CommonFunc.ToogleLoading(false);
                        return;
                    }

                    $SubGrid1.jqGrid("clearGridData");
                    $SubGrid1.jqGrid("setGridParam", {
                        datatype: 'local',
                        data: result.subData1
                    }).trigger("reloadGrid");

                    $SubGrid2.jqGrid("clearGridData");
                    $SubGrid2.jqGrid("setGridParam", {
                        datatype: 'local',
                        data: result.subData2
                    }).trigger("reloadGrid");

                    CommonFunc.Notify("", _getLang("L_DNManage_CopyS", "复制成功"), 500, "success");
                    CommonFunc.ToogleLoading(false);
                }
            });
        }
    });
    getSelectOptionsN();
});

function initLoadData(Uid) {
    if (!Uid)
        return;
    $.ajax({
        async: true,
        url: rootPath + "DNManage/GetInvDetail",
        type: 'POST',
        data: {
            UId: Uid
        },
        dataType: "json",
        beforeSend: function () {
            CommonFunc.ToogleLoading(true);
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            var maindata = jQuery.parseJSON(result.mainTable.Content);
            var inData = jQuery.parseJSON(result.inData.Content);
            var pkgData = jQuery.parseJSON(result.pkgData.Content);
            var ipoData = jQuery.parseJSON(result.ipoData.Content);
            console.log(result);
            setFieldValue(maindata.rows);

            $SubGrid1.jqGrid("setGridParam", {
                datatype: 'local',
                sortorder: "asc",
                sortname: "SeqNo",
                data: inData.rows
            }).trigger("reloadGrid");

            $SubGrid2.jqGrid("setGridParam", {
                datatype: 'local',
                sortorder: "asc",
                sortname: "SeqNo",
                data: pkgData.rows
            }).trigger("reloadGrid");

            $SubGrid3.jqGrid("setGridParam", {
                datatype: 'local',
                sortorder: "asc",
                sortname: "PoNo",
                data: ipoData.rows
            }).trigger("reloadGrid");

            setdisabled(true);
            setToolBtnDisabled(true);
            var multiEdocData = [];
            ajaxHttp(rootPath + "DNManage/GetSMAndDnData", { SmNo: $("#ShipmentId").val(), loading: true, DnNo: $("#DnNo").val() },
            function (data) {
                if (data == null) {
                    MenuBarFuncArr.initEdocCus($("#UId").val(), groupId, $("#Cmp").val(), "*", $("#Atd").val(), undefined, callBackFunc);
                } else {
                    $(data.sm).each(function (index) {
                        multiEdocData.push({ jobNo: data.sm[index].UId, 'GROUP_ID': data.sm[index].GroupId, 'CMP': data.sm[index].Cmp, 'STN': '*', 'atd': $("#Atd").val() });
                    });
                    MenuBarFuncArr.initEdocCus($("#UId").val(), groupId, $("#Cmp").val(), "*", $("#Atd").val(), multiEdocData, callBackFunc);
                }
            });
            MenuBarFuncArr.Disabled(["MBSave", "MBCancel", "MBAdd", "MBReload"]);
            MenuBarFuncArr.Enabled(["MBEdoc", "MBDel", "MBEdit", "MBCopy", "MBPrint"]);

            if (status == "add") {
                MenuBarFuncArr.Disabled(["MBEdit"]);
                MenuBarFuncArr.Enabled(["MBAdd"]);
            }

            $("#DnUid").val(result.dn_uid);
            CommonFunc.ToogleLoading(false);
            setUnit();
            setFix();
        }
    });
}

function _initMenu() {
    MenuBarFuncArr.DelMenu(["MBApprove", "MBPrint", "MBErrMsg", "MBInvalid", "MBSearch"]);

    MenuBarFuncArr.MBAdd = function () {
        $("#UId").removeAttr('required');
        $("#uploadPackingBtn").prop("disabled", false);
        gridEditableCtrl({ editable: true, gridId: "SubGrid1" });
        gridEditableCtrl({ editable: true, gridId: "SubGrid2" });
        gridEditableCtrl({ editable: true, gridId: "SubGrid3" });
        MenuBarFuncArr.Disabled(["MBTosap", "MBSEND","btnPackingSendToSAP"]);
    };

    MenuBarFuncArr.MBCopy = function (thisItem) {
        MenuBarFuncArr.Disabled(["MBTosap", "MBSEND","btnPackingSendToSAP"]);
        $("#UId").removeAttr('required');
        $("#UId").val("");
        $("#uploadPackingBtn").prop("disabled", false); 
        gridEditableCtrl({ editable: true, gridId: "SubGrid1" });
        gridEditableCtrl({ editable: true, gridId: "SubGrid2" });
        gridEditableCtrl({ editable: true, gridId: "SubGrid3" });
        var allData1 = $('#SubGrid1').jqGrid("getGridParam", "data");
        $.each(allData1, function (i, val) { 
            val["_id_"] = "jqg" + i; 
        });

        var allData2 = $('#SubGrid2').jqGrid("getGridParam", "data");
        $.each(allData2, function (i, val) { 
            val["_id_"] = "jqg" + i; 
        });
        var allData3 = $('#SubGrid3').jqGrid("getGridParam", "data");
        $.each(allData3, function (i, val) { 
            val["_id_"] = "jqg" + i; 
        });
    }

    MenuBarFuncArr.MBEdit = function () {
        $("#uploadPackingBtn").prop("disabled", false);
        gridEditableCtrl({ editable: true, gridId: "SubGrid1" });
        gridEditableCtrl({ editable: true, gridId: "SubGrid2" });
        gridEditableCtrl({ editable: true, gridId: "SubGrid3" });
        MenuBarFuncArr.Disabled(["MBTosap", "MBSEND","btnPackingSendToSAP"]);

    };

    MenuBarFuncArr.EndFunc = function () {
        MenuBarFuncArr.Enabled(["MBReload"]);
    }

    MenuBarFuncArr.MBCancel = function () {
        $("#uploadPackingBtn").prop("disabled", true);
        gridEditableCtrl({ editable: false, gridId: "SubGrid1" });
        gridEditableCtrl({ editable: false, gridId: "SubGrid2" });
        gridEditableCtrl({ editable: false, gridId: "SubGrid3" });

        $SubGrid1.jqGrid("setGridParam", {
            url: rootPath + "DNManage/GetIndGridData",
            rowNum: 999,
            postData: { UId: $("#UId").val() },
            page: 1,
            datatype: "json"
        }).trigger("reloadGrid");

        $SubGrid2.jqGrid("setGridParam", {
            url: rootPath + "DNManage/GetPkgGridData",
            rowNum: 999,
            postData: { UId: $("#UId").val() },
            page: 1,
            datatype: "json"
        }).trigger("reloadGrid");

        $SubGrid3.jqGrid("setGridParam", {
            url: rootPath + "DNManage/GetInpoGridData",
            rowNum: 999,
            postData: { UId: $("#UId").val() },
            page: 1,
            datatype: "json"
        }).trigger("reloadGrid");

        MenuBarFuncArr.Disabled(["MBReload"]);
        MenuBarFuncArr.Enabled(["MBPrint", "MBPreview", "MBTosap", "MBSEND", "MBEdoc","btnPackingSendToSAP"]);
    };

    MenuBarFuncArr.MBSave = function (dtd) {
        caluSum();
        var allData1 = $SubGrid1.find("tr");
        var allData2 = $SubGrid2.find("tr");
        var TtlQty = $("#TtlQty").val();
        var TtlPqty = $("#TtlPqty").val();

        if (TtlPqty != TtlQty) {
            if (!confirm(_getLang("L_DNManage_CautionIvPacking", "注意：Invoice数量「不等于」Packing数量，要继续吗？") + " ")) {
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
                return;
            }
        }
        var ShipmentId = $("#ShipmentId").val();
        if (ShipmentId == "")
            if (!confirm(_getLang("L_InvPkgSetup_Script_152", "注意：没有Shipment  id 将不会进行货柜更新，要继续吗？"))) {
                return;
            }
            else {
                upload = "false";
            }
        $.each(allData1, function (i, val) {
            if (typeof val.id != "undefined" && val.id != "") {
                setGridVal($SubGrid1, val.id, 'SeqNo', i, null);
                if (val.id.indexOf("jqg") == -1) {
                    //console.log(val.id);
                    setGridVal($SubGrid1, val.id, 'rn', val.id, null);
                }
            }
        });
        $.each(allData2, function (i, val) {
            if (typeof val.id != "undefined" && val.id != "") {
                setGridVal($SubGrid2, val.id, 'SeqNo', i, null);
                if (val.id.indexOf("jqg") == -1) {
                    //console.log(val.id);
                    setGridVal($SubGrid2, val.id, 'rn', val.id, null);
                }
            }
        });

        //MenuBarFuncArr.SaveResult = false;
        //dtd.resolve();
        //return false;
        var changeData = getChangeValue();
        var SubGridChageArray1 = $SubGrid1.jqGrid('getGridParam', "arrangeGrid")();
        var SubGridChageArray2 = $SubGrid2.jqGrid('getGridParam', "arrangeGrid")();
        var SubGridChageArray3 = $SubGrid3.jqGrid('getGridParam', "arrangeGrid")();
        //return;
        //表示值沒變
        if ($.isEmptyObject(changeData)) {
            CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "保存成功"), 500, "success");
            MenuBarFuncArr.SaveResult = true;
            dtd.resolve();
            setdisabled(true);
            return;
        }

        changeData["st1"] = SubGridChageArray1;
        changeData["st2"] = SubGridChageArray2;
        changeData["st3"] = SubGridChageArray3;

        console.log(changeData);
        $.ajax({
            async: true,
            url: rootPath + "DNManage/InvSetupUpdate",
            type: 'POST',
            data: {
                "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val()
            },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveF", "保存失败"), 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") {
                    CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveF", "保存失败")+"," + result.message, 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return false;
                }

                setFieldValue(result.mainData);
                console.log(result);
                $SubGrid1.jqGrid("clearGridData");
                $SubGrid1.jqGrid("setGridParam", {
                    datatype: 'local',
                    data: result.subData1
                }).trigger("reloadGrid");

                $SubGrid2.jqGrid("clearGridData");
                $SubGrid2.jqGrid("setGridParam", {
                    datatype: 'local',
                    data: result.subData2
                }).trigger("reloadGrid");

                $SubGrid3.jqGrid("clearGridData");
                $SubGrid3.jqGrid("setGridParam", {
                    datatype: 'local',
                    data: result.subData3
                }).trigger("reloadGrid");


                gridEditableCtrl({ editable: false, gridId: "SubGrid1" });
                gridEditableCtrl({ editable: false, gridId: "SubGrid2" });
                gridEditableCtrl({ editable: false, gridId: "SubGrid3" });
                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "保存成功"), 500, "success");
                MenuBarFuncArr.SaveResult = true;
                $("#uploadPackingBtn").prop("disabled", true);
                MenuBarFuncArr.Disabled(["MBReload"]);
                MenuBarFuncArr.Enabled(["MBPrint", "MBPreview", "MBTosap", "MBSEND", "MBEdoc","btnPackingSendToSAP"]);
                dtd.resolve();
                setFix();

                //if (upload == "success")
                //    updateSmrv();
                var uid = $("#UId").val();
                initLoadData(uid);
            }
        });
        return dtd.promise();
    }

    MenuBarFuncArr.MBDel = function () {
        var changeData = getChangeValue();
        //表示值沒變
        $.ajax({
            async: true,
            url: rootPath + "DNManage/InvSetupUpdate",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val() },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveF", "保存失败"), 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                //dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") {
                    CommonFunc.Notify("", _getLang("L_MailFormatSetup_DelF", "删除失败"), 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    //dtd.resolve();
                    return false;
                }

                setFieldValue(result.mainData);
                $SubGrid1.jqGrid("clearGridData");
                $SubGrid2.jqGrid("clearGridData");
                $SubGrid3.jqGrid("clearGridData");
                gridEditableCtrl({ editable: false, gridId: "SubGrid1" });
                gridEditableCtrl({ editable: false, gridId: "SubGrid2" });
                gridEditableCtrl({ editable: false, gridId: "SubGrid3" });

                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_DelS", "删除成功"), 500, "success");
                MenuBarFuncArr.SaveResult = true;
                $("#uploadPackingBtn").prop("disabled", true);
            }
        });

    }

    initMenuBar(MenuBarFuncArr);
}
function CheckEdoc(TYPE) {
    $.ajax({
        async: true,
        url: rootPath + "EDOC/FileCheck",
        type: 'POST',
        data: { CMP: $("#Cmp").val(), TYPE: TYPE, GROUP_ID: groupId, JOBNO: _uid },
        dataType: "json",
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveF", "保存失败"), 500, "warning");
            MenuBarFuncArr.SaveResult = false;
            //dtd.resolve();
        },
        success: function (result) {
            if (result.IsOk == "Y") {
                var id = $("#DnUid").val();
                var shipmentid = $("#ShipmentId").val();
                if (TYPE == "PACKO") {
                    saveReport2Shipment(id, shipmentid, "IPQ05", _getLang("L_DNManage_EpDecla", "出口报关") + " Packing List");
                }
                if (TYPE == "INVO") {
                    saveReport2Shipment(id, shipmentid, "IPQ06", _getLang("L_DNManage_EpDecla", "出口报关") + " Invoice");
                }
                if (TYPE == "CONTRACT") {
                    saveReport2Shipment(id, shipmentid, "IPQ06C", _getLang("L_DNManage_EpDecla", "出口报关") + " " + _getLang("L_QTSetup_Cntr", "合约"));
                }
            }
            else {
                alert(result.message);
            }

        }
    });
}
function _initLookup() {
    //Shipper放大鏡
    setSmptyData("ShprCdLookup", "ShprCd", "ShprNm", "SH", { Addr: "#ShprAddr", Attn: "#ShprAttn", Tel: "#ShprTel", Fax: "#ShprFax" });

    //BILL TO放大鏡
    setSmptyData("BillToLookup", "BillTo", "BillNm", "RE", { Addr: "#BillAddr", Attn: "#BillAttn", Tel: "#BillTel", Fax: "#BillFax" });

    //Consignee放大鏡
    setSmptyData("CneeCdLookup", "CneeCd", "CneeNm", "CS", { Addr: "#CneeAddr", Attn: "#CneeAttn", Tel: "#CneeTel", Fax: "#CneeFax" });

    //Ship to放大鏡
    setSmptyData("ShipToLookup", "ShipTo", "ShipNm", "WE", { Addr: "#ShipAddr", Attn: "#ShipAttn", Tel: "#ShipTel", Fax: "#ShipFax" });

    //CUST to放大鏡
    setSmptyData("CustCdLookup", "CustCd", "CustNm", "WE", { Addr: "#CustAddr", Attn: "#CustAttn", Tel: "#CustTel", Fax: "#CustFax" });

    //Notify放大鏡
    setSmptyData("NotifyNoLookup", "NotifyNo", "NotifyNm", "NT", {});

    //PaymentTerm Lookup
    setBscData("TradeTermLookup", "TradeTerm", "", "TINC");

    //Price Term Lookup
    setBscData("IncotermLookup", "Incoterm", "", "TINC");

    //PLT unit Lookup
    setBscData("PltuLookup", "Pltu", "", "UB");

    //QTYU unit Lookup
    setBscData("QtyuLookup", "Qtyu", "", "UB");

    //NWU unit Lookup
    setBscData("NwuLookup", "Nwu", "", "UT");

    //NWU unit Lookup
    setBscData("CbmuLookup", "Cbmu", "", "UT");

    //From Lookup
    setCityData("FromCdLookup", "FromCd", "FromDescp", "");

    //To Lookup
    setCityData("ToCdLookup", "ToCd", "ToDescp", "");

    //DN放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetDnForLookup";
    options.param = "";
    options.registerBtn = $("#DnNoLookup");
    options.focusItem = $("#DnNo");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        var DnNo = map.DnNo;
        var ShipmentId = map.ShipmentId;
        $("#DnNo").val(map.DnNo);
        $("#ShipmentId").val(ShipmentId);
        //GetDnAndShipmentData(DnNo);
    }

    options.lookUpConfig = LookUpConfig.dnLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#DnNo", 1, "", "dt=smdn&GROUP_ID=" + groupId + "&DN_NO=", "DN_NO=showValue,DN_NO,SHIPMENT_ID", function (event, ui) {
        $(this).val(ui.item.returnValue.DN_NO);
        $("#ShipmentId").val(ui.item.returnValue.SHIPMENT_ID);
        //GetDnAndShipmentData(ui.item.returnValue.DN_NO);
        return false;
    });

    //國家放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetCountryDataForLookup";
    options.registerBtn = $("#CntryOrnLookup");
    options.focusItem = $("#CntryOrn");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#CntryOrn").val(map.CntryCd);
        $("#CntryDescp").val(map.CntryNm);
    }

    options.lookUpConfig = LookUpConfig.CntryLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#CntryOrn", 1, "", "dt=country&GROUP_ID=" + groupId + "&CNTRY_CD=", "CNTRY_CD=showValue,CNTRY_CD,CNTRY_NM", function (event, ui) {
        $(this).val(ui.item.returnValue.PARTY_NO);
        $("#CntryDescp").val(ui.item.returnValue.CNTRY_NM);
        return false;
    });

    //幣別放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetBsCurDataForLookup";
    options.registerBtn = $("#CurLookup");
    options.focusItem = $("#CntryOrn");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#Cur").val(map.Cur);
    }

    options.lookUpConfig = LookUpConfig.CurLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#Cur", 1, "", "dt=crn&GROUP_ID=" + groupId + "&CUR=", "CUR=showValue,CUR", function (event, ui) {
        $(this).val(ui.item.returnValue.CUR);
        return false;
    });
}

function _initGenGrid() {
    //user lookup for grid
    var qtyuOpt = {};
    qtyuOpt.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
    qtyuOpt.param = "G";
    qtyuOpt.baseCondition = " CD_TYPE='UB'";
    qtyuOpt.gridReturnFunc = function (map) {
        var value = map.Cd
        return value;
    }
    qtyuOpt.selfSite = true;
    qtyuOpt.lookUpConfig = LookUpConfig.BSCodeLookup;
    qtyuOpt.autoCompKeyinNum = 1;
    qtyuOpt.gridId = "SubGrid1";
    qtyuOpt.autoCompUrl = "";
    qtyuOpt.autoCompDt = "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE~UB&CD=";
    qtyuOpt.autoCompParams = "CD=showValue,CD,CD_DESCP";
    qtyuOpt.autoCompFunc = function (elem, event, ui, rowid) {
        $(elem).val(ui.item.returnValue.CD);
    }

    var curOpt = {};
    curOpt.gridUrl = rootPath + "TPVCommon/GetBsCurDataForLookup";
    curOpt.param = "";
    curOpt.gridReturnFunc = function (map) {
        var value = map.Cur
        return value;
    }
    curOpt.selfSite = true;
    curOpt.lookUpConfig = LookUpConfig.CurLookup;
    curOpt.autoCompKeyinNum = 1;
    curOpt.gridId = "SubGrid1";
    curOpt.autoCompUrl = "";
    curOpt.autoCompDt = "dt=crn&GROUP_ID=" + groupId + "&CUR=";
    curOpt.autoCompParams = "CUR=showValue,CUR";
    curOpt.autoCompFunc = function (elem, event, ui, rowid) {
        $(elem).val(ui.item.returnValue.CUR);
    }

    var IpartOpt = {};
    IpartOpt.gridUrl = rootPath + "TPVCommon/GetIpartNoForLookup";
    IpartOpt.param = "";
    IpartOpt.baseConditionFunc = function () {
        var DnNo = $("#DnNo").val();

        return "DnNo=" + DnNo + "&sopt_DnNo=eq";
    }
    IpartOpt.gridReturnFunc = function (map) {
        var selRowId = $SubGrid2.jqGrid('getGridParam', 'selrow');
        var value = map.IpartNo
        var GoodsDescp = map.GoodsDescp;
        var Qty = map.Qty;
        var Qtyu = map.Qtyu;
        var Gw = map.Gw;
        var Cbm = map.Cbm;
        var OhsCode = map.OhsCode;
        var IhsCode = map.IhsCode;
        var PartNo = map.PartNo;
        var OpartNo = map.OpartNo;

        setGridVal($SubGrid2, selRowId, "GoodsDescp", GoodsDescp, null);
        setGridVal($SubGrid2, selRowId, "Qty", Qty, null);
        setGridVal($SubGrid2, selRowId, "Qtyu", Qtyu, null);
        setGridVal($SubGrid2, selRowId, "Gw", Gw, null);
        setGridVal($SubGrid2, selRowId, "Cbm", Cbm, null);
        setGridVal($SubGrid2, selRowId, "OhsCode", OhsCode, null);
        setGridVal($SubGrid2, selRowId, "IhsCode", IhsCode, null);
        setGridVal($SubGrid2, selRowId, "PartNo", PartNo, null);
        setGridVal($SubGrid2, selRowId, "OpartNo", OpartNo, null);

        return value;
    }
    IpartOpt.selfSite = true;
    IpartOpt.lookUpConfig = LookUpConfig.SmdnpLookup;
    IpartOpt.autoCompKeyinNum = 1;
    IpartOpt.gridId = "SubGrid2";
    IpartOpt.autoCompUrl = "";
    IpartOpt.autoCompDt = "dt=smdnp&IPART_NO=";
    IpartOpt.autoCompParams = "IPART_NO=showValue,IPART_NO,GOODS_DESCP,QTY,QTYU,GW,CBM,OHS_CODE,IHS_CODE,PART_NO,OPART_NO";
    IpartOpt.baseConditionFunc = function () {
        var DnNo = $("#DnNo").val();

        return "DnNo=" + DnNo + "&sopt_DnNo=eq";
    }
    IpartOpt.autoCompFunc = function (elem, event, ui, rowid) {
        var GoodsDescp = ui.item.returnValue.GOODS_DESCP;
        var Qty = ui.item.returnValue.QTY;
        var Qtyu = ui.item.returnValue.QTYU
        var Gw = ui.item.returnValue.GW;
        var Cbm = ui.item.returnValue.CBM;
        var OhsCode = ui.item.returnValue.OHS_CODE;
        var IhsCode = ui.item.returnValue.IHS_CODE;
        var PartNo = ui.item.returnValue.PART_NO;
        var OpartNo = ui.item.returnValue.OPART_NO;

        setGridVal($SubGrid2, rowid, "GoodsDescp", GoodsDescp, null);
        setGridVal($SubGrid2, rowid, "Qty", Qty, null);
        setGridVal($SubGrid2, rowid, "Qtyu", Qtyu, null);
        setGridVal($SubGrid2, rowid, "Gw", Gw, null);
        setGridVal($SubGrid2, rowid, "Cbm", Cbm, null);
        setGridVal($SubGrid2, rowid, "OhsCode", OhsCode, null);
        setGridVal($SubGrid2, rowid, "IhsCode", IhsCode, null);
        setGridVal($SubGrid2, rowid, "PartNo", PartNo, null);
        setGridVal($SubGrid2, rowid, "OpartNo", OpartNo, null);
        $(elem).val(ui.item.returnValue.IPART_NO);
    }

    var colModel1 = [
		{ name: 'UId', title: 'UId', index: 'UId', width: 100, sorttype: 'string', hidden: true },
		{ name: 'SeqNo', title: 'SeqNo', index: 'SeqNo', width: 100, align: 'right', sorttype: 'float', hidden: true },
		{ name: 'InvoiceType', title: _getLang("L_InvPkgQuery_InvoiceType", "Inv.类型"), index: 'InvoiceType', width: 70, sorttype: 'string', editable: false, hidden: true },
	    { name: 'ShipmentId', title: _getLang("L_DNApproveManage_ShipmentId", "Shipment ID"), index: 'ShipmentId', width: 100, sorttype: 'string', editable: false, hidden: true },
        { name: 'DnNo', title: _getLang("L_DNApproveManage_DnNo", "DN NO"), index: 'DnNo', width: 100, sorttype: 'string', editable: false, hidden: false },
	    { name: 'InvNo', title: _getLang("L_DNApproveManage_InvNo", "Invoice No"), index: 'InvNo', width: 100, sorttype: 'string', editable: false, hidden: true },
	    { name: 'IpartNo', title: _getLang("L_DNApproveManage_IpartNo", "对内机种名"), index: 'IpartNo', width: 200, sorttype: 'string', editable: true },
        { name: 'GoodsDescp', title: _getLang("L_DNApproveManage_GoodsDescp", "商品名称"), index: 'GoodsDescp', width: 200, sorttype: 'string', editable: true },
	    { name: 'Qty', title: _getLang("L_BaseLookup_Qty", "数量"), index: 'Qty', width: 60, sorttype: 'int', align: 'right', editable: true, formatter: 'integer' },
	    { name: 'Qtyu', title: _getLang("L_BaseLookup_Qtyu", "数量单位"), index: 'Qtyu', width: 100, sorttype: 'string', editable: true, edittype: 'custom', editoptions: gridLookup(qtyuOpt) },
        { name: 'Cur1', title: _getLang("L_IpPart_Crncy", "币别"), index: 'Cur1', width: 70, sorttype: 'string', editable: true, edittype: 'custom', editoptions: gridLookup(curOpt) },
        {
            name: 'UnitPrice1', title: _getLang("L_DNApproveManage_UnitPrice", "单价"), index: 'UnitPrice1', align: 'right', width: 100, sorttype: 'float', editable: true,
            formatter: 'number',
            formatoptions: {
                decimalSeparator: ".",
                thousandsSeparator: ",",
                decimalPlaces: 6,
                defaultValue: '0.000000'
            }
        },
        {
            name: 'Amt', title: _getLang("L_DNDetailVeiw_Value", "金额"), index: 'Amt', width: 100, align: 'right', sorttype: 'string', editable: true,
            formatter: 'number',
            formatoptions: {
                decimalSeparator: ".",
                thousandsSeparator: ",",
                decimalPlaces: 2,
                defaultValue: '0.00'
            }
        },
        { name: 'Category', title: 'Category', index: 'Category', width: 100, sorttype: 'string', editable: true },
        { name: 'NewCategory', title: 'NewCategory', index: 'NewCategory', width: 100, sorttype: 'string', hidden: true, editable: true },
	    { name: 'PoNo', title: _getLang("L_DNApproveManage_PoNo", "订单号"), index: 'PoNo', width: 150, sorttype: 'string', editable: true },
	    { name: 'SoNo', title: _getLang("L_BaseLookup_SoNo", "SO NO"), index: 'SoNo', width: 150, sorttype: 'string', editable: true },
	    { name: 'OpartNo', title: _getLang("L_DNApproveManage_OpartNo", "对外机种名"), index: 'OpartNo', width: 100, sorttype: 'string', editable: true },
	    { name: 'PartNo', title: _getLang("L_DNApproveManage_PartNo", "客户物料号"), index: 'PartNo', width: 200, sorttype: 'string', editable: true },
	    { name: 'IhsCode', title: _getLang("L_DNApproveManage_HisCode", "目的国商品编码"), index: 'IhsCode', width: 100, sorttype: 'string', editable: true },
	    { name: 'OhsCode', title: _getLang("L_DNApproveManage_OhsCode", "出口国商品编码"), index: 'OhsCode', width: 100, sorttype: 'string', editable: true },
	    { name: 'ProdDescp', title: _getLang("L_DNApproveManage_ProdDescp", "商品品名"), index: 'ProdDescp', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
	    { name: 'Brand', title: _getLang("L_DNApproveManage_Brand", "品牌"), index: 'Brand', width: 100, sorttype: 'string', editable: true },
	    { name: 'Remark', title: _getLang("L_BSCSSetup_Remark", "备注"), index: 'Remark', width: 200, sorttype: 'string', editable: true },
	    {
	        name: 'TtlNw', title: _getLang("L_BaseLookup_Nw", "净重"), index: 'TtlNw', align: 'right', width: 70, sorttype: 'float', editable: true,
	        formatter: 'number',
	        formatoptions: {
	            decimalSeparator: ".",
	            thousandsSeparator: ",",
	            decimalPlaces: 3,
	            defaultValue: '0.000'
	        }
	    },
	    {
	        name: 'TtlGw', title: _getLang("L_BaseLookup_Gw", "毛重"), index: 'TtlGw', align: 'right', width: 70, sorttype: 'float', editable: true,
	        formatter: 'number',
	        formatoptions: {
	            decimalSeparator: ".",
	            thousandsSeparator: ",",
	            decimalPlaces: 3,
	            defaultValue: '0.000'
	        }
	    },
	    {
	        name: 'TtlCbm', title: 'CBM', index: 'TtlCbm', width: 100, align: 'right', sorttype: 'float', editable: true,
	        formatter: 'number',
	        formatoptions: {
	            decimalSeparator: ".",
	            thousandsSeparator: ",",
	            decimalPlaces: 4,
	            defaultValue: '0.0000'
	        }
	    }
    ];

    new genGrid(
    	$SubGrid1,
    	{
    	    datatype: "local",
    	    loadonce: true,
    	    colModel: colModel1,
    	    caption: _getLang("L_DNManage_InvoiceDetail", "Invioce 明细"),
    	    height: 400,
    	    rows: 9999,
    	    refresh: true,
    	    cellEdit: false,//禁用grid编辑功能
    	    pginput: false,
    	    sortable: true,
    	    pgbuttons: false,
    	    exportexcel: false,
    	    delKey: ["UId"],
    	    loadComplete: function (data) {
    	        var Amt = $SubGrid1.jqGrid("getCol", "Amt", false, "sum");
    	        //var TtlQty = $SubGrid1.jqGrid("getCol", "Qty", false, "sum");
    	        var TtlQty = 0;
    	        var allData1 = $SubGrid1.find("tr");
    	        $.each(allData1, function (i, val) {
    	            if (typeof val.id != "undefined" && val.id != "") {
    	                var Qty = getGridVal($SubGrid1, val.id, "Qty", null);
                        var Category = getGridVal($SubGrid1, val.id, "NewCategory", null);
    	                if (Category != "TANN") {
    	                    TtlQty = TtlQty + parseInt(Qty);
    	                }
    	            }
    	        });

    	        $("#TtlValue").val(CommonFunc.formatFloat(Amt, 2));
    	        $("#TtlQty").val(CommonFunc.formatFloat(TtlQty, 0));
    	    },
    	    onAddRowFunc: function (rowid) {
    	        var DnNo = $("#DnNo").val();
    	        setGridVal($SubGrid1, rowid, "DnNo", DnNo, null);
    	    },
    	    afterSaveCellWithIdFunc: function (rowid, name, val, iRow, iCol, toolId) {
    	        var Qty, UnitPrice1;
    	        switch (name) {
    	            case "Qty":
    	                Qty = val;
    	                UnitPrice1 = getGridVal($SubGrid1, rowid, "UnitPrice1", null);
    	                break;
    	            case "UnitPrice1":
    	                UnitPrice1 = val;
    	                Qty = getGridVal($SubGrid1, rowid, "Qty", null);
    	                break;
    	            default:
    	                return true;
    	                break;
    	        }

    	        var Amt = Qty * UnitPrice1;
    	        setGridVal($SubGrid1, rowid, "Amt", Amt, null);

    	        var TtlValue = $SubGrid1.jqGrid("getCol", "Amt", false, "sum");
                $("#TtlValue").val(CommonFunc.formatFloat(TtlValue, 2));

                var category = getGridVal($SubGrid1, rowid, "Category", null);
                setGridVal($SubGrid1, rowid, "NewCategory", category, null);

    	        var allData1 = $SubGrid1.find("tr");
    	        var TtlQty = 0;
    	        $.each(allData1, function (i, val) {
    	            if (typeof val.id != "undefined" && val.id != "") {
    	                var Qty = getGridVal($SubGrid1, val.id, "Qty", null);
                        var Category = getGridVal($SubGrid1, val.id, "NewCategory", null);
    	                if (Category != "TANN") {
    	                    TtlQty = TtlQty + parseInt(Qty);
    	                }
    	            }
    	        });
    	        $("#TtlQty").val(CommonFunc.formatFloat(TtlQty, 0));
    	    }
    	}
    ); 

    //user lookup for grid
    var qtyuOpt1 = {};
    qtyuOpt1.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
    qtyuOpt1.param = "";
    qtyuOpt1.baseCondition = " CD_TYPE='UB'";
    qtyuOpt1.gridReturnFunc = function (map) {
        var value = map.Cd
        return value;
    }
    qtyuOpt1.selfSite = true;
    qtyuOpt1.lookUpConfig = LookUpConfig.BSCodeLookup;
    qtyuOpt1.autoCompKeyinNum = 1;
    qtyuOpt1.gridId = "SubGrid2";
    qtyuOpt1.autoCompUrl = "";
    qtyuOpt1.autoCompDt = "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE~UB&CD=";
    qtyuOpt1.autoCompParams = "CD=showValue,CD,CD_DESCP";
    qtyuOpt1.autoCompFunc = function (elem, event, ui, rowid) {
        $(elem).val(ui.item.returnValue.CD);
    }

    var colModel2 = [
			{ name: 'UId', title: 'UId', index: 'UId', width: 100, sorttype: 'string', hidden: true },
    		{ name: 'SeqNo', title: 'SeqNo', index: 'SeqNo', width: 60, sorttype: 'float', align: 'right', hidden: true },
    		{ name: 'InvoiceType', title: 'Dn No', index: 'InvoiceType', width: 70, sorttype: 'string', editable: false, hidden: true },
    	    { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', width: 100, sorttype: 'string', editable: false, hidden: true },
            { name: 'DnNo', title: 'Dn No', index: 'DnNo', width: 100, sorttype: 'string', editable: false, hidden: false },
    	    { name: 'InvNo', title: 'Invoice No', index: 'InvNo', width: 100, sorttype: 'string', editable: false, hidden: true },
    	    { name: 'PlaNo', title: 'Pallet No', index: 'PlaNo', width: 70, sorttype: 'string', editable: true, hidden: false },
    	    { name: 'PlaSize', title: 'Pallet Size', index: 'PlaSize', width: 70, sorttype: 'string', editable: true, hidden: false },
    	    { name: 'CaseNo', title: _getLang("L_InvPkgSetup_CaseNo", "Case No"), index: 'CaseNo', width: 70, sorttype: 'string', editable: true, hidden: false },
    	    { name: 'CaseNum', title: _getLang("L_InvPkgSetup_CaseNum", "Case #"), index: 'CaseNum', align: 'right', width: 70, sorttype: 'int', editable: true, hidden: false, formatter: 'integer' },
    	    { name: 'IpartNo', title: _getLang("L_DNApproveManage_IpartNo", "对内机种名"), index: 'IpartNo', width: 150, sorttype: 'string', editable: true, edittype: 'custom', editoptions: gridLookup(IpartOpt) },
			{ name: 'GoodsDescp', title: _getLang("L_DNApproveManage_GoodsDescp", "商品名称"), index: 'GoodsDescp', width: 200, sorttype: 'string', editable: true },
            { name: 'LgoodsDescp', title: _getLang("L_DNApproveManage_LgoodsDescp", "中文商品名称"), index: 'LgoodsDescp', width: 200, sorttype: 'string', editable: true },
			{ name: 'Qty', title: _getLang("L_BaseLookup_Qty", "数量"), index: 'Qty', width: 70, align: 'right', sorttype: 'int', editable: true, formatter: 'integer' },
			{ name: 'Qtyu', title: _getLang("L_BaseLookup_Qtyu", "数量单位"), index: 'Qtyu', width: 70, sorttype: 'string', editable: true, edittype: 'custom', editoptions: gridLookup(qtyuOpt1) },
			{ name: 'TtlQty', title: _getLang("L_InvPkgSetup_TtlQty", "总数量"), index: 'TtlQty', width: 70, align: 'right', sorttype: 'int', editable: true, formatter: 'integer' },
            {
                name: 'Nw', title: _getLang("L_BaseLookup_Nw", "净重"), index: 'Nw', width: 70, align: 'right', sorttype: 'float', editable: true,
                formatter: 'number',
                formatoptions: {
                    decimalSeparator: ".",
                    thousandsSeparator: ",",
                    decimalPlaces: 6,
                    defaultValue: '0.000000'
                }
            },
			{
			    name: 'TtlNw', title: _getLang("L_InvPkgSetup_TtlNw", "总净重"), index: 'TtlNw', align: 'right', width: 70, sorttype: 'float', editable: true,
			    formatter: 'number',
			    formatoptions: {
			        decimalSeparator: ".",
			        thousandsSeparator: ",",
			        decimalPlaces: 6,
			        defaultValue: '0.000000'
			    }
			},
			{
			    name: 'Gw', title: _getLang("L_BaseLookup_Gw", "毛重"), index: 'Gw', width: 70, align: 'right', sorttype: 'float', editable: true,
			    formatter: 'number',
			    formatoptions: {
			        decimalSeparator: ".",
			        thousandsSeparator: ",",
			        decimalPlaces: 6,
			        defaultValue: '0.000000'
			    }
			},
			//{ name: 'Gwu', title: 'Unit', index: 'Gwu', width: 60, sorttype: 'string', editable: true, hidden: true },
			{
			    name: 'TtlGw', title: _getLang("L_InvPkgSetup_TtlGw", "总毛重"), index: 'TtlGw', align: 'right', width: 70, sorttype: 'float', editable: true,
			    formatter: 'number',
			    formatoptions: {
			        decimalSeparator: ".",
			        thousandsSeparator: ",",
			        decimalPlaces: 6,
			        defaultValue: '0.000000'
			    }
			},
			{
			    name: 'Cbm', title: _getLang("L_BaseLookup_Cbm", "CBM"), index: 'Cbm', width: 70, align: 'right', sorttype: 'float', editable: true,
			    formatter: 'number',
			    formatoptions: {
			        decimalSeparator: ".",
			        thousandsSeparator: ",",
			        decimalPlaces: 6,
			        defaultValue: '0.000000'
			    }
			},
			//{ name: 'Cbmu', title: 'Unit', index: 'Cbmu', width: 60, sorttype: 'string', editable: true, hidden: true },
			{
			    name: 'TtlCbm', title: _getLang("L_InvPkgSetup_TtlCbm", "总CBM"), index: 'TtlCbm', width: 100, align: 'right', sorttype: 'float', editable: true,
			    formatter: 'number',
			    formatoptions: {
			        decimalSeparator: ".",
			        thousandsSeparator: ",",
			        decimalPlaces: 6,
			        defaultValue: '0.000000'
			    }
			},
			{ name: 'IhsCode', title: _getLang("L_DNApproveManage_HisCode", "目的国商品编码"), index: 'IhsCode', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
			{ name: 'OpartNo', title: _getLang("L_DNApproveManage_OpartNo", "对外机种名"), index: 'OpartNo', width: 150, sorttype: 'string', editable: true },
			{ name: 'NcmNo', title: 'NCM NO', index: 'NcmNo', width: 70, sorttype: 'string', editable: true },
			{ name: 'CntryOrn', title: 'Original country', index: 'CntryOrn', width: 70, sorttype: 'string', editable: true },
			{ name: 'PartNo', title: _getLang("L_DNApproveManage_PartNo", "客户物料号"), index: 'PartNo', width: 200, sorttype: 'string', editable: true },
			{ name: 'VenCd', title: 'Vendor Code', index: 'VenCd', width: 300, sorttype: 'string', editable: true },
			{ name: 'VenNm', title: 'Vendor Name', index: 'VenNm', width: 300, sorttype: 'string', editable: true },
			{ name: 'VenAddr', title: 'Vendor Address', index: 'VenAddr', width: 300, sorttype: 'string', editable: true },
            { name: 'CntrNo', title: 'Container no#', index: 'CntrNo', width: 300, sorttype: 'string', editable: false },
			//{ name: 'Dimension', title: _getLang("L_BaseLookup_Dimension", "尺寸"), index: 'Dimension', width: 300, sorttype: 'string', editable: true },
			{ name: 'Remark', title: _getLang("L_BSCSSetup_Remark", "备注"), index: 'Remark', width: 300, sorttype: 'string', editable: true },
            {
                name: 'GwByPn', title: 'Gross weightByPN', index: 'GwByPn', width: 100, align: 'right', sorttype: 'float', editable: true,
                formatter: 'number',
                formatoptions: {
                    decimalSeparator: ".",
                    thousandsSeparator: ",",
                    decimalPlaces: 6,
                    defaultValue: '0.000000'
                }
            },
             { name: 'CnCode', title: 'CN Code', index: 'CnCode', width: 100, sorttype: 'string', editable: false }
    ];

    new genGrid(
    	$SubGrid2,
    	{
    	    datatype: "local",
    	    loadonce: true,
    	    colModel: colModel2,
    	    caption: _getLang("L_DNManage_PackingDetail", "Packing 明细"),
    	    height: 400,
    	    rows: 9999,
    	    refresh: true,
    	    cellEdit: false,//禁用grid编辑功能
    	    pginput: false,
    	    sortable: true,
    	    pgbuttons: false,
    	    exportexcel: false,
    	    delKey: ["UId"],
    	    loadComplete: function (data) {
    	        var TtlNw = $SubGrid2.jqGrid("getCol", "TtlNw", false, "sum");
    	        var TtlGw = $SubGrid2.jqGrid("getCol", "TtlGw", false, "sum");
    	        var TtlCbm = $SubGrid2.jqGrid("getCol", "TtlCbm", false, "sum");
    	        var TtlPqty = $SubGrid2.jqGrid("getCol", "TtlQty", false, "sum");

    	        $("#TtlNw").val(CommonFunc.formatFloat(TtlNw, 3));
    	        $("#TtlGw").val(CommonFunc.formatFloat(TtlGw, 3));
    	        $("#TtlCbm").val(CommonFunc.formatFloat(TtlCbm, 4));
    	        $("#TtlPqty").val(CommonFunc.formatFloat(TtlPqty, 0));

    	        //if(data.rows.length > 0)
    	        //{
    	        //	$("#Qtyu").val(data.rows[0].Qtyu);
    	        //}
    	    },
    	    onAddRowFunc: function (rowid) {
    	        var DnNo = $("#DnNo").val();

    	        setGridVal($SubGrid2, rowid, "DnNo", DnNo, null);
    	    },
    	    afterSaveCellWithIdFunc: function (rowid, name, val, iRow, iCol, toolId) {

    	        var CaseNo, Qty, Nw, Gw, Cbm;
    	        switch (name) {
    	            case "CaseNo":
    	                CaseNo = val;
    	                Qty = getGridVal($SubGrid2, rowid, "Qty", null);
    	                Nw = getGridVal($SubGrid2, rowid, "Nw", null);
    	                Gw = getGridVal($SubGrid2, rowid, "Gw", null);
    	                Cbm = getGridVal($SubGrid2, rowid, "Cbm", null);
    	                break;
    	            case "Qty":
    	                Qty = val;
    	                CaseNo = getGridVal($SubGrid2, rowid, "CaseNo", null);
    	                Nw = getGridVal($SubGrid2, rowid, "Nw", null);
    	                Gw = getGridVal($SubGrid2, rowid, "Gw", null);
    	                Cbm = getGridVal($SubGrid2, rowid, "Cbm", null);
    	                break;
    	            case "Nw":
    	                Nw = val;
    	                CaseNo = getGridVal($SubGrid2, rowid, "CaseNo", null);
    	                Qty = getGridVal($SubGrid2, rowid, "Qty", null);
    	                Gw = getGridVal($SubGrid2, rowid, "Gw", null);
    	                Cbm = getGridVal($SubGrid2, rowid, "Cbm", null);
    	                break
    	            case "Gw":
    	                Gw = val;
    	                CaseNo = getGridVal($SubGrid2, rowid, "CaseNo", null);
    	                Qty = getGridVal($SubGrid2, rowid, "Qty", null);
    	                Nw = getGridVal($SubGrid2, rowid, "Nw", null);
    	                Cbm = getGridVal($SubGrid2, rowid, "Cbm", null);
    	                break
    	            case "Cbm":
    	                Cbm = val;
    	                CaseNo = getGridVal($SubGrid2, rowid, "CaseNo", null);
    	                Qty = getGridVal($SubGrid2, rowid, "Qty", null);
    	                Nw = getGridVal($SubGrid2, rowid, "Nw", null);
    	                Gw = getGridVal($SubGrid2, rowid, "Gw", null);
    	                break;
    	            case "IpartNo":
    	                Cbm = getGridVal($SubGrid2, rowid, "Cbm", null);
    	                CaseNo = getGridVal($SubGrid2, rowid, "CaseNo", null);
    	                Qty = getGridVal($SubGrid2, rowid, "Qty", null);
    	                Nw = getGridVal($SubGrid2, rowid, "Nw", null);
    	                Gw = getGridVal($SubGrid2, rowid, "Gw", null);
    	                break;
    	            default:
    	                return true;
    	                break;
    	        }

    	        var regex = /[a-zA-Z\s]/gi;
    	        var CaseArr = [];
    	        var n = 1;

    	        if (CaseNo != "") {
    	            CaseNo = CaseNo.replace(regex, "");
    	            CaseArr = CaseNo.split("-");
    	            var CaseNum = 1;

    	            if (typeof CaseArr[1] !== "undefined") {
    	                CaseNum = parseInt(CaseArr[1]) - parseInt(CaseArr[0]) + 1;
    	                n = CaseNum;
    	            }
    	            else {
    	                n = CaseNum;
    	            }

    	        }
    	        else {
    	            CaseNum = 0;
    	        }

    	        setGridVal($SubGrid2, rowid, "CaseNum", CaseNum, null);

    	        if (Qty > 0) {
    	            var TtlQty = floatMul(n, Qty);
    	            setGridVal($SubGrid2, rowid, "TtlQty", TtlQty, null);
    	        }

    	        if (Nw > 0) {
    	            var TtlNw = floatMul(n, Nw);
    	            setGridVal($SubGrid2, rowid, "TtlNw", TtlNw, null);
    	        }

    	        if (Gw > 0) {
    	            var TtlGw = floatMul(n, Gw);
    	            setGridVal($SubGrid2, rowid, "TtlGw", TtlGw, null);
    	        }

    	        if (Cbm > 0) {
    	            var TtlCbm = floatMul(n, Cbm);
    	            setGridVal($SubGrid2, rowid, "TtlCbm", TtlCbm, null);
    	        }

    	        var sumTtlNw = $SubGrid2.jqGrid("getCol", "TtlNw", false, "sum");
    	        var sumTtlGw = $SubGrid2.jqGrid("getCol", "TtlGw", false, "sum");
    	        var sumTtlCbm = $SubGrid2.jqGrid("getCol", "TtlCbm", false, "sum");
    	        var sumTtlQty = $SubGrid2.jqGrid("getCol", "Qty", false, "sum");
    	        var sumTtlPlt = $SubGrid2.jqGrid("getCol", "CaseNum", false, "sum");

    	        $("#TtlNw").val(CommonFunc.formatFloat(sumTtlNw, 3));
    	        $("#TtlGw").val(CommonFunc.formatFloat(sumTtlGw, 3));
    	        $("#TtlCbm").val(CommonFunc.formatFloat(sumTtlCbm, 4));
    	        $("#TtlPqty").val(CommonFunc.formatFloat(sumTtlQty, 0));
    	        $("#TtlPlt").val(CommonFunc.formatFloat(sumTtlPlt, 0));
    	    }
    	}
    );

    var colModel3 = [
        { name: 'UId', title: 'UId', index: 'UId', width: 100, sorttype: 'string', hidden: true }, 
        { name: 'DnNo', title: 'DN NO', index: 'DnNo', width: 100, sorttype: 'string', editable: false, hidden: false },
        { name: 'InvNo', title: 'Invoice NO', index: 'InvNo', width: 100, sorttype: 'string', editable: false, hidden: false },
        { name: 'PoNo', title: _getLang("L_DNApproveManage_PoNo", "订单号"), index: 'PoNo', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'PoType', title: _getLang("L_SMIPO_PoType", "PO类型"), index: 'PoType', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'SoNo', title: 'SO NO', index: 'SoNo', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'OpartNo', title: _getLang("L_SMIDNP_OpartNo", "对外机种名"), index: 'OpartNo', width: 150, sorttype: 'string', editable: true, hidden: false },
        { name: 'PartNo', title: _getLang("L_DNApproveManage_PartNo", "L_DNApproveManage_PartNo"), index: 'PartNo', width: 150, sorttype: 'string', editable: true, hidden: false },
        { name: 'IpartNo', title: _getLang("L_SMIDNP_IpartNo", "对内机种名"), index: 'IpartNo', width: 150, sorttype: 'string', editable: true, hidden: false },
        { name: 'IhsCode', title: _getLang("L_DNApproveManage_HisCode", "目的国商品编码"), index: 'IhsCode', width: 220, sorttype: 'string', editable: true, hidden: false },
        { name: 'GoodsDescp', title: _getLang("L_DNApproveManage_GoodsDescp", "商品名称"), index: 'GoodsDescp', width: 150, sorttype: 'string', editable: true, hidden: false },
        { name: 'Brand', title: _getLang("L_DNApproveManage_Brand", "品牌"), index: 'Brand', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'Qty', title: _getLang("L_BaseLookup_Qty", "数量"), index: 'Qty', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'Qtyu', title: _getLang("L_BaseLookup_Qtyu", "数量单位"), index: 'Qtyu', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'Cur1', title: _getLang("L_IpPart_Crncy", "币别"), index: 'Cur1', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'UnitPrice1', title: _getLang("L_IpPart_UnitPrice", "单价"), index: 'UnitPrice1', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'Amt', title: _getLang("L_IQTManage_Amt", "金额"), index: 'Amt', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'Remark', title: _getLang("L_BSCSSetup_Remark", "备注"), index: 'Remark', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'InvoiceType', title: 'Invoice Type', index: 'InvoiceType', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'VenCd', title: 'Vendor Code', index: 'VenCd', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'VenNm', title: 'Vender Name', index: 'VenNm', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'TtlNw', title: _getLang("L_ShipmentID_Nw", "净重"), index: 'TtlNw', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'TtlGw', title: _getLang("L_BaseLookup_Gw", "毛重"), index: 'TtlGw', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'TtlCbm', title: "CBM", index: 'TtlCbm', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'ProdDescp', title: _getLang("L_DNApproveManage_ProdDescp", "商品品名"), index: 'ProdDescp', width: 150, sorttype: 'string', editable: true, hidden: false },
        { name: 'OhsCode', title: _getLang("L_DNApproveManage_OhsCode", "出口国商品编码"), index: 'OhsCode', width: 150, sorttype: 'string', editable: true, hidden: false },
        { name: 'DeliveryItem', title: 'PO Item', index: 'DeliveryItem', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'Category', title: 'Category', index: 'Category', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'SafetyModel', title: _getLang("L_DNApproveManage_SafetyModel", "旧物料号"), index: 'SafetyModel', width: 150, sorttype: 'string', editable: true, hidden: false },
        { name: 'ChineseDescription', title: _getLang("L_SMIDNP_ProdDescp", "中文品名"), index: 'ChineseDescription', width: 150, sorttype: 'string', editable: true, hidden: false },
        { name: 'ChineseModel', title: _getLang("L_SMIPO_ChineseModel", "中文规格型号"), index: 'ChineseModel', width: 150, sorttype: 'string', editable: true, hidden: false },
        { name: 'Incoterm', title: 'Incoterms', index: 'Incoterm', width: 100, sorttype: 'string', editable: true, hidden: false },
        { name: 'Plant', title: 'Plant', index: 'Plant', width: 100, sorttype: 'string', editable: true, hidden: false }
    ];

    new genGrid(
        $SubGrid3,
        {
            datatype: "local",
            loadonce: true,
            colModel: colModel3,
            caption: _getLang("L_INPO_Detail", "Inbound Invoice明细（PO）"),
            height: 400,
            rows: 9999,
            refresh: true,
            cellEdit: false,//禁用grid编辑功能
            pginput: false,
            sortable: true,
            pgbuttons: false,
            exportexcel: false,
            delKey: ["UId"],
            loadComplete: function (data) { 
            },
            onAddRowFunc: function (rowid) {
                var DnNo = $("#DnNo").val();
                setGridVal($SubGrid1, rowid, "DnNo", DnNo, null);
            },
            afterSaveCellWithIdFunc: function (rowid, name, val, iRow, iCol, toolId) {
                var Qty, UnitPrice1;
                switch (name) {
                    case "Qty":
                        Qty = val;
                        UnitPrice1 = getGridVal($SubGrid3, rowid, "UnitPrice1", null);
                        break;
                    case "UnitPrice1":
                        UnitPrice1 = val;
                        Qty = getGridVal($SubGrid3, rowid, "Qty", null);
                        break;
                    default:
                        return true;
                        break;
                } 
                var Amt = Qty * UnitPrice1;
                setGridVal($SubGrid3, rowid, "Amt", Amt, null); 
            }
        }
    );
}

function setSmptyData(lookUp, Cd, Nm, pType, opt) {
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetSmptyDataForLookup";
    options.registerBtn = $("#" + lookUp);
    options.focusItem = $("#" + Cd);
    options.param = "";
    options.baseCondition = " PARTY_TYPE LIKE '%" + pType + "%'";
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        var addr_str = (map.PartAddr1 || "") + " " + (map.PartAddr2 || "") + " " + (map.PartAddr3 || "") + " " + (map.PartAddr4 || "") + " " + (map.PartAddr5 || "");
        $("#" + Cd).val(map.PartyNo);

        if (Nm != "")
            $("#" + Nm).val(map.PartyName);

        if (typeof opt.Addr !== "undefined") {
            $(opt.Addr).val(addr_str);
        }

        if (typeof opt.Attn !== "undefined") {
            $(opt.Attn).val(map.PartyAttn);
        }

        if (typeof opt.Tel !== "undefined") {
            $(opt.Tel).val(map.PartyTel);
        }

        if (typeof opt.Fax !== "undefined") {
            $(opt.Fax).val(map.PartyFax);
        }


    }

    options.lookUpConfig = LookUpConfig.SmptyLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#" + Cd, 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO=", "PARTY_NO=showValue,PARTY_NO,PARTY_NAME,PART_ADDR1,PART_ADDR2,PART_ADDR3,PART_ADDR4,PART_ADDR5,PARTY_ATTN,PARTY_TEL,PARTY_FAX", function (event, ui) {
        var addr_str = (ui.item.returnValue.PART_ADDR1 || "") + " " + (ui.item.returnValue.PART_ADDR2 || "") + " " + (ui.item.returnValue.PART_ADDR3 || "") + " " + (ui.item.returnValue.PART_ADDR4 || "") + " " + (ui.item.returnValue.PART_ADDR5 || "");


        $(this).val(ui.item.returnValue.PARTY_NO);

        if (Nm != "")
            $("#" + Nm).val(ui.item.returnValue.PARTY_NAME);

        if (typeof opt.Addr !== "undefined") {
            $(opt.Addr).val(addr_str);
        }

        if (typeof opt.Attn !== "undefined") {
            $(opt.Attn).val(ui.item.returnValue.PARTY_ATTN);
        }

        if (typeof opt.Tel !== "undefined") {
            $(opt.Tel).val(ui.item.returnValue.PARTY_TEL);
        }

        if (typeof opt.Fax !== "undefined") {
            $(opt.Fax).val(ui.item.returnValue.PARTY_FAX);
        }
        return false;
    });
}

function setBscData(lookUp, Cd, Nm, pType) {
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
    options.registerBtn = $("#" + lookUp);
    options.focusItem = $("#" + Cd);
    options.params = "";
    options.param = "";
    options.baseCondition = " GROUP_ID='" + groupId + "' AND CD_TYPE='" + pType + "'";
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#" + Cd).val(map.Cd);

        if (Nm != "")
            $("#" + Nm).val(map.CdDescp);

        if (lookUp == "IncotermLookup") {
            $("#Incoterm").trigger('change');
        }
    }

    options.lookUpConfig = LookUpConfig.BSCodeLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#" + Cd, 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE~" + pType + "&CD=", "CD=showValue,CD,CD_DESCP", function (event, ui) {
        $(this).val(ui.item.returnValue.CD);

        if (Nm != "")
            $("#" + Nm).val(ui.item.returnValue.CD_DESCP);
        if (lookUp == "IncotermLookup") {
            $("#Incoterm").trigger('change');
        }

        return false;
    });


}

function setCityData(lookUp, Cd, Nm, pType) {
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetBsCityDataForLookup";
    options.registerBtn = $("#" + lookUp);
    options.focusItem = $("#" + Cd);
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#" + Cd).val(map.CntryCd + map.PortCd);

        if (Nm != "")
            $("#" + Nm).val(map.PortNm);
    }

    options.lookUpConfig = LookUpConfig.CityPortLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#" + Cd, 1, "", "dt=port&GROUP_ID=" + groupId + "&CNTRY_CD+PORT_CD=", "CNTRY_CD&PORT_CD=showValue,CNTRY_CD,PORT_CD,PORT_NM", function (event, ui) {
        $(this).val(ui.item.returnValue.CNTRY_CD + ui.item.returnValue.PORT_CD);

        if (Nm != "")
            $("#" + Nm).val(ui.item.returnValue.PORT_NM);

        return false;
    });
}

function GetDnAndShipmentData(DnNo) {
    $.post(rootPath + 'Invoice/GetDnptAndShipmentData', { "DnNo": DnNo }, function (data, textStatus, xhr) {

        var DnData = $.parseJSON(data.dnData.Content);
        var shipmentData = $.parseJSON(data.shipmentData.Content);

        var sData = shipmentData.rows[0];
        console.log(DnData.rows);
        $.each(DnData.rows, function (i, val) {
            if (DnData.rows[i].PartyType == "SH") {
                $("#ShprCd").val(DnData.rows[i].PartyNo);
                $("#ShprNm").val(DnData.rows[i].PartyName);
                $("#ShprAddr").val(DnData.rows[i].PartyAddr1);
            }

            if (DnData.rows[i].PartyType == "CS") {
                $("#CneeCd").val(DnData.rows[i].PartyNo);
                $("#CneeNm").val(DnData.rows[i].PartyName);
                $("#CneeAddr").val(DnData.rows[i].PartyAddr1);
            }

            if (DnData.rows[i].PartyType == "NT") {
                $("#NotifyNo").val(DnData.rows[i].PartyNo);
                $("#NotifyNm").val(DnData.rows[i].PartyName);
            }

            if (DnData.rows[i].PartyType == "RE") {
                $("#BillToCd").val(DnData.rows[i].PartyNo);
                $("#BillToNm").val(DnData.rows[i].PartyName);
                $("#BillAddr").val(DnData.rows[i].PartyAddr1);
            }
        });
        console.log(shipmentData);
        $("#SoNo").val(sData.SoNo);
        $("#Etd").val(sData.Etd);
        $("#Eta").val(sData.Eta);
        $("#Vessel").val(sData.Vessel1);
        $("#BlNo").val(sData.BlNo);
        $("#RefNo").val(sData.RefNo);
    }, "JSON");

}

function GetInvByDn(DnNo) {

    var ShipmentId = $("#ShipmentId").val();
    var UId = $("#UId").val();
    if (UId == "") {
        alert(_getLang("L_InvPkgSetup_Script_151", "请先保存后，再次Reload Invoice"));
        return;
    }
    CommonFunc.ToogleLoading(true);
    $.post(rootPath + 'Invoice/InboundReloadInvoice', { "UId": UId }, function (data, textStatus, xhr) {

        console.log(data);
        try {
            var partyData = $.parseJSON(data.partyData.Content);
            var shipmentData = $.parseJSON(data.shipmentData.Content);
            var dnData = $.parseJSON(data.dnData.Content);
            var DnNo = $("#DnNo").val();
            var fee = $("#FreightFee").val();

            var dData = {}, sData = {};
            if (dnData.rows != null) {
                dData = dnData.rows[0];
            }

            if (shipmentData.rows != null) {
                sData = shipmentData.rows[0];
            }

            //console.log(DnData.rows);
            $.each(partyData.rows, function (i, val) {
                if (partyData.rows[i].PartyType == "SH")//== "SH"
                {
                    $("#ShprCd").val(partyData.rows[i].PartyNo);
                    $("#ShprNm").val(partyData.rows[i].PartyName);
                    $("#ShprAddr").val(partyData.rows[i].PartAddr);
                    $("#ShprAttn").val(partyData.rows[i].PartyAttn);
                    $("#ShprTel").val(partyData.rows[i].PartyTel);
                    $("#ShprFax").val(partyData.rows[i].FaxNo);
                }

                if (partyData.rows[i].PartyType == "CS") {
                    $("#CneeCd").val(partyData.rows[i].PartyNo);
                    $("#CneeNm").val(partyData.rows[i].PartyName);
                    $("#CneeAddr").val(partyData.rows[i].PartAddr);
                    $("#CneeAttn").val(partyData.rows[i].PartyAttn);
                    $("#CneeTel").val(partyData.rows[i].PartyTel);
                    $("#CneeFax").val(partyData.rows[i].FaxNo);
                }

                if (partyData.rows[i].PartyType == "AG") {
                    $("#CustCd").val(partyData.rows[i].PartyNo);
                    $("#CustNm").val(partyData.rows[i].PartyName);
                    $("#CustAddr").val(partyData.rows[i].PartAddr);
                    $("#CustAttn").val(partyData.rows[i].PartyAttn);
                    $("#CustTel").val(partyData.rows[i].PartyTel);
                    $("#CustFax").val(partyData.rows[i].FaxNo);
                }

                if (partyData.rows[i].PartyType == "NT") {
                    $("#NotifyNo").val(partyData.rows[i].PartyNo);
                    $("#NotifyNm").val(partyData.rows[i].PartyName);
                }

                if (partyData.rows[i].PartyType == "RE") {
                    $("#BillTo").val(partyData.rows[i].PartyNo);
                    $("#BillNm").val(partyData.rows[i].PartyName);
                    $("#BillAddr").val(partyData.rows[i].PartAddr);
                    $("#BillAttn").val(partyData.rows[i].PartyAttn);
                    $("#BillTel").val(partyData.rows[i].PartyTel);
                    $("#BillFax").val(partyData.rows[i].FaxNo);
                }

                if (partyData.rows[i].PartyType == "WE") {
                    $("#ShipTo").val(partyData.rows[i].PartyNo);
                    $("#ShipNm").val(partyData.rows[i].PartyName);
                    $("#ShipAddr").val(partyData.rows[i].PartAddr);
                    $("#ShipAttn").val(partyData.rows[i].PartyAttn);
                    $("#ShipTel").val(partyData.rows[i].PartyTel);
                    $("#ShipFax").val(partyData.rows[i].FaxNo);
                }
            });

            if (!$.isEmptyObject(sData)) {
                console.log(sData);
                $("#Etd").val(sData.Etd);
                $("#Eta").val(sData.Eta);
                if (sData.Vessel1 != null) {
                    $("#VesselNm").val(sData.Vessel1);
                }

                if (sData.Voyage1 != null) {
                    $("#VesselNm").val(sData.Voyage1);
                }

                if (sData.Vessel1 != null && sData.Voyage1 != null) {
                    $("#VesselNm").val(sData.Vessel1 + "/" + sData.Voyage1);
                }
                $("#BlNo").val(sData.BlNo);
                $("#RefNo").val(sData.RefNo);
                $("#FromCd").val(sData.PolCd);
                $("#FromDescp").val(sData.PolName);
                $("#ToCd").val(sData.DestCd);
                $("#ToDescp").val(sData.DestName);
                $("#Incoterm").val(sData.IncotermCd);
                $("#IncotermDescp").val(sData.IncotermDescp);
                $("#CombineInfo").val(sData.CombineInfo);
                $("#ShippingDate").val(sData.Etd);
                //$("#Etd").val(sData.DnEtd);
                $("#VatNo").val(sData.VatNo);
                $("#ShipmentId").val(sData.ShipmentId);
                $("#CmdtyCd").val(sData.Goods);
                $("#Lgoods").val(sData.Lgoods);
                $("#TradeTerm").val(sData.TradeTerm);
                $("#TradetermDescp").val(sData.TradetermDescp);
                $("#Qtyu").val(sData.Qtyu);

                if ($("#UploadBy").val() == "" || $("#UploadBy").val() == null || $("#UploadBy").val() == undefined) {
                    $("#Marks").val(dData.Marks);
                    $("#TtlPlt").val(sData.PkgNum);
                    $("#Pltu").val(sData.PkgUnit);
                    $("#PkgUnitDesc").val(sData.PkgUnitDesc);
                    if (DnNo == "") {
                        $("#Marks").val(sData.Marks);
                    }
                }

                $("#CntryOrn").val(sData.CntryOrn);
                $("#CntryDescp").val(sData.CntryDescp);
                $("#BankMsg").val(sData.Memo);
                $("#TransacteMode").val(sData.TransacteMode);
               
                var InvDate = sData.ActPostDate;
                if (InvDate != null) {
                    $("#InvDate").val(InvDate.substr(0, 10));
                }

                //var RefNo = sData.DnNoCmpRef;
                //if(RefNo != "" && RefNo != null)
                //{
                //	$("#RefNo").val(RefNo.substr(4, RefNo.length));
                //}


                if (sData.HouseNo == null || sData.HouseNo == "") {
                    $("#BlNo").val(sData.MasterNo);
                }
                else {
                    $("#BlNo").val(sData.HouseNo);
                }

                if (sData.TranType == "F" || sData.TranType == "L") {
                    $("#DlvWay").val("By Sea");
                }
                else if (sData.TranType == "A") {
                    $("#DlvWay").val("By Air");
                }
                else if (sData.TranType == "D" || sData.TranType == "E") {
                    $("#DlvWay").val("By Express");
                }
                else if (sData.TranType == "R") {
                    $("#DlvWay").val("By Railroad");
                }

                var subData = $("#SubGrid1").jqGrid("getRowData", 1);

                if (subData.SoNo != "" && subData.SoNo != null) {
                    $("#SoNo").val(subData.SoNo);
                }

                var f_fee = getFreightFee(sData.FreightAmt || 0);
                $("#FreightFee").val(f_fee);
                setTtlValue();
            }

            if (!$.isEmptyObject(dData)) {
                $("#VatNo").val(dData.VatNo);
                var RefNo = dData.DnNoCmpRef;
                if (RefNo != "" && RefNo != null) {
                    $("#RefNo").val(RefNo.substr(4, RefNo.length));
                }
                var InvDate = dData.ActPostDate;
                if (InvDate != null) {
                    $("#InvDate").val(InvDate.substr(0, 10));
                }
                $("#BankMsg").val(dData.Memo);
                $("#Lgoods").val(dData.Lgoods);
                //$("#Etd").val(dData.DnEtd);
                $("#Incoterm").val(dData.Incoterm);
                $("#IncotermDescp").val(dData.IncotermDescp);
                $("#TradeTerm").val(dData.TradeTerm);
                $("#TradetermDescp").val(dData.TradetermDescp);
                $("#TransacteMode").val(dData.TransacteMode);
                if (DnNo != "") {
                    if ($("#UploadBy").val() == "" || $("#UploadBy").val() == null || $("#UploadBy").val() == undefined) {
                        $("#Marks").val(dData.Marks);
                        $("#TtlPlt").val(dData.PkgNum);
                        $("#PkgUnitDesc").val(dData.PkgUnitDesc);
                    }
                    $("#CmdtyCd").val(dData.Goods);
                    var f_fee = getFreightFee(dData.FreightAmt || 0);
                    $("#FreightFee").val(f_fee);
                    setTtlValue();
                }
            }

            if ($("#InvNo").val() != "" && $("#DnNo").val() != "") {
                $("#PackingNo").val(parseInt($("#InvNo").val()));
            }

            $("#InvoiceRmk").val(data.InvRemark);
            $("#InvIntru").val(data.InvIntro);
            $("#PackingRmk").val(data.PkgRemark);
            $("#PkgIntru").val(data.PkgIntro);

            var TtlValue = $SubGrid1.jqGrid("getCol", "Amt", false, "sum");
            $("#TtlValue").val(TtlValue);
            var SoNo = $("#SubGrid1").jqGrid('getCell', 1, 'SoNo');
            var Cur = null;
            var i = $("#SubGrid1").jqGrid('getGridParam', 'records');
            for (var j = 1; j < i + 1; j++) {
                var category = $("#SubGrid1").jqGrid('getCell', j, 'NewCategory');
                if (category === 'TANN')
                    continue;
                if (category === 'TAN') {
                    Cur = $("#SubGrid1").jqGrid('getCell', j, 'Cur1');
                    break;
                }
                if (Cur == null || Cur === "" || Cur == undefined)
                    Cur = $("#SubGrid1").jqGrid('getCell', j, 'Cur1');
            }

            if (SoNo) {
                $("#SoNo").val(SoNo);
            }

            if (Cur) {
                $("#Cur").val(Cur)
            }

            if ($("#TtlValue").val() != "") {
                setTtlValue();
            }
            var InvDateetd = data.etd;
            if (InvDateetd != null) {
                $("#InvDate").val(InvDateetd.substr(0, 10));
            } else {
                $("#InvDate").val("");
            }
            caluFobValue();
            setUnit();
        }
        catch (err) {
            CommonFunc.ToogleLoading(false);
        }
        CommonFunc.ToogleLoading(false);
    }, "JSON")
    .fail(function () {
        CommonFunc.ToogleLoading(false);
    });
}

function genReport(thisUId, rptId, rptName) {
    var params = {
        currentCondition: "",
        uid: thisUId,
        rptdescp: rptName,
        rptName: rptId,
        formatType: 'xls',
        exportType: 'PREVIEW',
    };

    $.ajax({
        async: true,
        url: rootPath + "Report/CreateNewReport",
        type: 'POST',
        data: params,
        "complete": function (xmlHttpRequest, successMsg) {
            if (successMsg != "success") return null;
            var xx = xmlHttpRequest.responseText;
            window.open(xmlHttpRequest.responseText);
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {

        }
    });
}

function saveReport2Shipment(id, shipmentid, rptId, rptName) {
    var fileType = "";
    var CombineFlag = $("#CombineFlag").val();
    var rmk = "DN NO: " + $("#DnNo").val();
    var mainId = $("#UId").val();


    var condiStr = "IuFid=" + mainId + "&sopt_IuFid=eq";
    if (id == "") {
        //alert("没有Dn号，不能归档");
        //return;
    }

    if (CombineFlag == "C" || id == "") {
        id = $("#UId").val();
        rmk = "Shipment ID: " + $("#ShipmentId").val();
    }
    if (rptId == "IPQ04" || rptId == "IPQ06") {
        fileType = "INVO";
    }
    else if (rptId == "IPQ05" || rptId == "IPQ03" || rptId == "IPQ05CNEW") {
        fileType = "PACKO";


    }
    else if (rptId == "IPQ01" || rptId == "IPQ0TTLPKG") {
        fileType = "PACKI";
    }
    else if (rptId == "IPQ02") {
        fileType = "INVI";
    }
    else if (rptId == "IPQ06C") {
        fileType = "CONTRACT";
    }

    if (rptId == "IPQ01" || rptId == "IPQ03" || rptId == "IPQ05") {
        condiStr = "PuFid=" + mainId + "&sopt_PuFid=eq";
    }

    var data = {
        reportId: rptId,
        conditionString: condiStr,
        exportFileType: "pdf",
        reportName: rptName,
        jobNo: id,
        GroupId: groupId,
        Cmp: $("#Cmp").val(),
        Stn: "*",
        fileType: fileType,//歸檔類型
        remark: rmk,//remark 預設放INVNO
        uid: mainId,
        "arg0": "uid",
        "val0": mainId,
        "arg_count": 1,
        combineInfo: $("#CombineInfo").val()
    };

    CommonFunc.ToogleLoading(true);
    if (id) {
        $.ajax({
            async: true,
            cache: false,
            dataType: "json",
            url: rootPath + "EDOC/CreateNewReport2Edoc",
            data: data,
            type: 'POST',
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
                CommonFunc.ToogleLoading(false);
            },
            success: function (data) {
                var message = data.Message;
                if (data.Success == true) {
                    if (message == "thread") {
                        CommonFunc.Notify("", _getLang("L_DNManage_Success", "报表归档已排程，请等候3~5分钟"), 500, "success");
                    } else {
                        CommonFunc.Notify("", _getLang("L_BaseBookingSetup_Scripts_163", "报表归档成功"), 500, "success");
                    }


                    /*$.post(rootPath + 'Invoice/updateShipment', {UId:$("#UId").val()}, function(data, textStatus, xhr) {
	                	if(data.message != "success")
	                	{
	                		CommonFunc.Notify("", data.message, 500, "warning");
	                	}
	                }, "JSON");*/
                } else {

                    if (message) {
                        CommonFunc.Notify("", _getLang("L_DNManage_Fail", "报表归档失败") + "," + message, 500, "warning");
                    } else {
                        CommonFunc.Notify("", _getLang("L_DNManage_Fail", "报表归档失败"), 500, "warning");
                    }

                }
                CommonFunc.ToogleLoading(false);
            }
        });
    }
}

function setTtlValue() {
    var TtlValue = isNaN(parseFloat($("#TtlValue").val())) ? 0 : parseFloat(RemoveComma($("#TtlValue").val()));
    //var IssueFee = TtlValue * 1.1 * tir;
    var IssueFee = TtlValue * 1.1 * tir;
    var TradeTerm = $("#TradeTerm").val();
    TradeTerm = TradeTerm.toUpperCase();
    //if(TradeTerm != "FOB" && TradeTerm != "FCA" && TradeTerm != "CFR" && TradeTerm != "EXW")
    //{
    if (IssueFee > 0 && IssueFee <= 1) {
        IssueFee = 1;
    }
    else if (IssueFee > 1) {
        IssueFee = Math.round(IssueFee);
    }
    //$("#IssueFee").val(CommonFunc.formatFloat(IssueFee, 2));
    //}
    //else
    //{
    //IssueFee = 0;
    //$("#IssueFee").val(CommonFunc.formatFloat(IssueFee, 2));
    //}
    var i_fee = getIssueFee(IssueFee || 0);
    $("#IssueFee").val(CommonFunc.formatFloat(i_fee, 2));
    caluFobValue();
}

function caluFobValue() {
    var val = $("#TradeTerm").val();
    var FreightFee = isNaN(parseFloat($("#FreightFee").val())) ? 0 : parseFloat(RemoveComma($("#FreightFee").val()));
    var IssueFee = isNaN(parseFloat($("#IssueFee").val())) ? 0 : parseFloat(RemoveComma($("#IssueFee").val()));
    var TtlValue = isNaN(parseFloat($("#TtlValue").val())) ? 0 : parseFloat(RemoveComma($("#TtlValue").val()));
    val = val.toUpperCase();
    var FobValue = 0;
    FobValue = TtlValue - FreightFee - IssueFee;
    $("#FobValue").val(CommonFunc.formatFloat(FobValue, 2));
    setFix();
    /*
	switch(val)
	{
		case "FOB":
			$("#FobValue").val(CommonFunc.formatFloat(TtlValue, 2));
		break;
		case "EXW":
			$("#FobValue").val(CommonFunc.formatFloat(TtlValue, 2));
		break;
		case "CIF":
			FobValue = TtlValue - FreightFee - IssueFee;
			$("#FobValue").val(CommonFunc.formatFloat(FobValue, 2));
		break;
		case "C&F":
			FobValue = TtlValue - FreightFee;
			$("#FobValue").val(CommonFunc.formatFloat(FobValue, 2));
		break;
		case "CNF":
			FobValue = TtlValue - FreightFee;
			$("#FobValue").val(CommonFunc.formatFloat(FobValue, 2));
		break;
		case "C&I":
			FobValue = TtlValue - IssueFee;
			$("#FobValue").val(CommonFunc.formatFloat(FobValue, 2));
		break;
		case "CNI":
			FobValue = TtlValue - IssueFee;
			$("#FobValue").val(CommonFunc.formatFloat(FobValue, 2));
		break;
	}
	*/
}

function caluSum() {
    var Amt = $SubGrid1.jqGrid("getCol", "Amt", false, "sum");
    //var TtlQty = $SubGrid1.jqGrid("getCol", "Qty", false, "sum");
    var TtlQty = 0;
    var allData1 = $SubGrid1.find("tr");
    $.each(allData1, function (i, val) {
        if (typeof val.id != "undefined" && val.id != "") {
            var Qty = getGridVal($SubGrid1, val.id, "Qty", null);
            var Category = getGridVal($SubGrid1, val.id, "NewCategory", null);
            if (Category != "TANN") {
                TtlQty = TtlQty + parseInt(Qty);
            }
        }
    });

    $("#TtlValue").val(CommonFunc.formatFloat(Amt, 2));
    $("#TtlQty").val(CommonFunc.formatFloat(TtlQty, 0));

    var TtlNw = $SubGrid2.jqGrid("getCol", "TtlNw", false, "sum");
    var TtlGw = $SubGrid2.jqGrid("getCol", "TtlGw", false, "sum");
    var TtlCbm = $SubGrid2.jqGrid("getCol", "TtlCbm", false, "sum");
    var TtlPqty = $SubGrid2.jqGrid("getCol", "TtlQty", false, "sum");

    $("#TtlNw").val(CommonFunc.formatFloat(TtlNw, 3));
    $("#TtlGw").val(CommonFunc.formatFloat(TtlGw, 3));
    $("#TtlCbm").val(CommonFunc.formatFloat(TtlCbm, 4));
    $("#TtlPqty").val(CommonFunc.formatFloat(TtlPqty, 0));

}

function setUnit() {
    var Nwu = $("#Nwu").val();
    var Pltu = $("#Pltu").val();
    var Qtyu = $("#Qtyu").val();

    if (Nwu == "") {
        $("#Nwu").val("KG");
    }

    if (Pltu == "") {
        $("#Pltu").val("PLT");
    }

    if (Qtyu == "") {
        $("#Qtyu").val("CTN");
    }
}

function setFix() {
    var TtlValue = parseFloat(RemoveComma($("#TtlValue").val()) || 0);
    var FreightFee = parseFloat(RemoveComma($("#FreightFee").val()) || 0);
    var IssueFee = parseFloat(RemoveComma($("#IssueFee").val()) || 0);
    var FobValue = parseFloat(RemoveComma($("#FobValue").val()) || 0);

    $("#TtlValue").val(CommonFunc.formatFloat(TtlValue.toFixed(2), 2));
    $("#FreightFee").val(CommonFunc.formatFloat(FreightFee.toFixed(2), 2));
    $("#IssueFee").val(CommonFunc.formatFloat(IssueFee.toFixed(2), 2));
    $("#FobValue").val(CommonFunc.formatFloat(FobValue.toFixed(2), 2));
}


function getFreightFee(FrieghtFee) {
    var TradeTerm = $("#TradeTerm").val();

    switch (TradeTerm) {
        case "C&I":
            return 0;
            break;
        case "EXW":
            return 0;
            break;
        case "FAS":
            return 0;
            break;
        case "FCA":
            return 0;
            break;
        case "FH":
            return 0;
            break;
        case "FOB":
            return 0;
        default:
            return FrieghtFee;
            break;
    }

    return;
}

function getIssueFee(IssueFee) {
    var TradeTerm = $("#TradeTerm").val();

    switch (TradeTerm) {
        case "CFR":
            return 0;
            break;
        case "CPT":
            return 0;
            break;
        case "EXW":
            return 0;
            break;
        case "FCA":
            return 0;
            break;
        case "FAS":
            return 0;
            break;
        case "FH":
            return 0;
            break;
        case "FOB":
            return 0;
            break;
        default:
            return IssueFee;
            break;
    }

    return;
}
function saveReport(reportId, reportName, id, ShipmentId, fileType,exportfileType) {
    if (exportfileType == "" || exportfileType == null || exportfileType == undefined) {
        exportfileType = "pdf";
    }
    var data = { 
        reportId: reportId,
        conditionString: "",
        "arg0": "uid",
        "val0": id,
        "arg_count": 1,
        exportFileType: exportfileType,
        ShipmentId: ShipmentId,
        reportName: reportName,
        fileType: fileType,
        jobNo: id,
        GroupId: groupId,
        Cmp: $("#Cmp").val(),
        Stn: "*"
    };

    CommonFunc.ToogleLoading(true);
    if (id) {
        $.ajax({
            async: true,
            cache: false,
            dataType: "json",
            url: rootPath + "EDOC/CreateNewReport2Edoc",
            data: data,
            type: 'POST',
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
                CommonFunc.ToogleLoading(false);
            },
            success: function (data) {
                if (data.Success == true) {
                    CommonFunc.Notify("", _getLang("L_BaseBookingSetup_Scripts_163", "报表归档成功"), 500, "success");
                } else {
                    var message = data.Message;
                    if (message) {
                        CommonFunc.Notify("", _getLang("L_DNManage_Fail", "报表归档失败") + "," + message, 500, "warning");
                    } else {
                        CommonFunc.Notify("", _getLang("L_DNManage_Fail", "报表归档失败"), 500, "warning");
                    }

                }
                CommonFunc.ToogleLoading(false);
            }
        });
    }
}

function ajaxHttp(url, data, successFn, completeFn, errorFn) {
    var loading = data.loading;
    if (loading == true) {
        CommonFunc.ToogleLoading(true);
    }
    $.ajax({
        async: true,
        url: url,
        type: 'POST',
        data: data,
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
            if (loading == true) {
                CommonFunc.ToogleLoading(false);
            }
            if (completeFn) completeFn(xmlHttpRequest, successMsg);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.Notify("", errMsg, 500, "danger");
            if (errorFn) errorFn();
        },
        success: function (result) {
            if (successFn) successFn(result);
        }
    });
}

function updateSmrv() {
    var UId = $("#UId").val();
    var InvoiceType = $("#InvoiceType").val();
    var ShipmentId = $("#ShipmentId").val();
    var DnNo = $("#DnNo").val();
    var InvNo = $("#InvNo").val();
    var postData = {
        DnNo: DnNo,
        UId: UId,
        InvNo: InvNo,
        ShipmentId: ShipmentId
    };

    $.ajax({
        url: rootPath + "DNManage/ExeclToUpdateSmrv",
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
                CommonFunc.Notify("", _getLang("L_InvPkgSetup_Script_145", "货柜资讯设定失败，") + data.message, 1300, "warning");
            }
            CommonFunc.Notify("", _getLang("L_InvPkgSetup_Scripts_243", "货柜资讯设定成功"), 500, "success");
        }
    });
    upload == "false";
}

function showReport(params) {
    $.ajax({
        async: true,
        url: rootPath + "Report/CreateNewReport",
        type: 'POST',
        data: params,
        "complete": function (xmlHttpRequest, successMsg) {
            if (successMsg != "success") return null;
            var xx = xmlHttpRequest.responseText;
            window.open(xmlHttpRequest.responseText);
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {

        }
    });
}
/*Edoc上傳後或更新類型後 更新燈號*/
var callBackFunc = function (jobNo, edocType) {
    if (edocType == null) {
        return true;
    }

    $.ajax({
        url: rootPath + 'SMSMI/setLight',
        type: 'POST',
        dataType: 'json',
        data: { "OUid": $("#OUid").val(), "UId": $("#UId").val() || jobNo, "Io": "O" },
        beforeSend: function () {
            //StatusBarArr.nowStatus(language["L_OrderManage_ComfirmNow"]);
            CommonFunc.ToogleLoading(true);
        },
        success: function (result) {
            if (result.message == "success") {
                //CommonFunc.Notify("", language["L_OrderManage_ComfirmSuccess"], 1000, "success");
                //$("#SummarySearch").trigger("click");
            }
            else {
                //CommonFunc.Notify("", result.message, 1000, "warning");
                //alert(result.message);
            }
            CommonFunc.ToogleLoading(false);
        },
        error: function () {
            //CommonFunc.Notify("", "", 1000, "danger");
            CommonFunc.ToogleLoading(false);
        }
    });
}
/*Edoc上傳後或更新類型後 更新燈號 end*/

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return id || caption;
}

function getSelectOptionsN() {
    var data = JSON.parse(decodeHtml(optionlist));
    var _shownull = { cd: '', cdDescp: '' };

    var TModOptions = data.TMOD || [];
    if (TModOptions.length > 0)
        _mt = TModOptions[0]["cd"];
    TModOptions.unshift(_shownull);
    appendSelectOption($("#TransacteMode"), TModOptions);

    if (_handler.topData) {
        $("#TransacteMode").val(_handler.topData["TransacteMode"]);
    }
}

