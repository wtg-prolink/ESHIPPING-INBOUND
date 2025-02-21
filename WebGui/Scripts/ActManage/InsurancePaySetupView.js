var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $MainGrid, $SubGrid, $SubGrid2;
var BlNo;
var IoFlag = getCookie("plv3.passport.ioflag");
$(function () {
    Schemas = JSON.parse(decodeHtml(Schemas));
    CommonFunc.initField(Schemas);
    setdisabled(true);

    if(IoFlag == 'O')
    {
        $("div[name='OutUser']").remove();
    }
    //$("#IpayDate").click(function(event) {
    //    $("#InsFeeWindow").modal("show");
    //});

    $(".flag").click(function(event) {
        var UId = encodeURIComponent($("#UId").val());
        var flag = this.id;
        var JobType = "";
        if (isEmpty(UId))
        {
            alert("@Resources.Locale.L_DNManage_PleSelcData");
            return;
        }      
        switch(flag)
        {
            case "IflagButton":
                JobType = "I";
                $("#JobType").val("I");
                break;
            case "CflagButton":
                JobType = "C";
                $("#JobType").val("C");
                break;
            case "FflagButton":
                JobType = "F";
                $("#JobType").val("F");
                break;
            case "TflagButton":
                JobType = "T";
                $("#JobType").val("T");
                break;
        }
        //$("#HandCarForm")[0].reset();
        //gridEditableCtrl({ editable: true, gridId: "SubGrid" });
        $.ajax({
            url: rootPath + 'ActManage/GetSMIPCDetail',
            type: 'POST',
            dataType: 'JSON',
            data: { UId: UId, JobType: JobType },
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            }
        })
        .done(function (data) {
            CommonFunc.ToogleLoading(false);
            _handler.loadGridData("SubGrid", $SubGrid[0], data["sub2"], [""]);            

        })
        .fail(function () {
            CommonFunc.Notify("", "error", 1000, "danger");
            CommonFunc.ToogleLoading(false);
        })
        .always(function () {
            //console.log("complete");
        });
        $("#InsFeeWindow").modal("show");
        //top.topManager.openPage({
        //    href: rootPath + "ActManage/InsurancePayDetailView/" + UId +"?JobType=" + JobType,
        //    title: '出險費用',
        //    id: 'InsurancePayDetailView',
        //});
    });
    $("[chxName='chxflag']").change(function() {
        var flag = this.name;
        if($(this).prop("checked")) {        
            switch(flag)
            {
                case "Iflag":
                    $("#IflagButton").prop("disabled",false);
                    break;
                case "Cflag":
                    $("#CflagButton").prop("disabled",false);
                    break;
                case "Fflag":
                    $("#FflagButton").prop("disabled",false);
                    break;
                case "Tflag":
                    $("#TflagButton").prop("disabled",false);
                    break;
            }
        }else{
            switch(flag)
            {
                case "Iflag":
                    $("#IflagButton").prop("disabled",true);
                    break;
                case "Cflag":
                    $("#CflagButton").prop("disabled",true);
                    break;
                case "Fflag":
                    $("#FflagButton").prop("disabled",true);
                    break;
                case "Tflag":
                    $("#TflagButton").prop("disabled",true);
                    break;
            }
        }
    });
    $("#ConfrimBtn").click(function (event) {
        var containerArray2 = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub2"] = containerArray2;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        data["Job_No"] = encodeURIComponent($("#JobNo").val());
        data["Job_Type"] = encodeURIComponent($("#JobType").val());
        $.ajax({
            url: rootPath + 'ActManage/SaveInsuranceDetailData',
            type: 'POST',
            dataType: 'JSON',
            data: data,
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            }
        })
        .done(function (data) {
            CommonFunc.ToogleLoading(false);
            //console.log(data);
            if (data.message == "success") {
                CommonFunc.Notify("", "success", 1000, "success");
                $("#InsFeeWindow").modal("hide");
                $("#SummarySearch").trigger("click");
            }
            else {
                CommonFunc.Notify("", data.message, 1000, "danger");
            }
        })
        .fail(function () {
            CommonFunc.Notify("", "error", 1000, "danger");
            CommonFunc.ToogleLoading(false);
        })
        .always(function () {
            //console.log("complete");
        });
    });
    
    $MainGrid = $("#MainGrid");
    $SubGrid = $("#SubGrid");
    
    var BookingLookup = {
        caption: "@Resources.Locale.L_QTSetup_Partial",
        sortname: "CreateDate",
        refresh: false,
        columns: [
            { name: 'UId', title: 'UId', index: 'UId', sorttype: 'string', hidden: true },
            { name: 'ShipmentId', title: '@Resources.Locale.L_QTSetup_ShipNo', index: 'ShipmentId', width: 120, init:true , align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            { name: 'DnNo', title: '@Resources.Locale.L_QTSetup_DeliverNo', index: 'DnNo', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
            //{ name: 'BuyerCd', title: 'Buyer', index: 'BuyerCd', width: 120, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            //{ name: 'SupplierCd', title: 'Supplier', index: 'SupplierCd', width: 120, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            //{ name: 'SupplierNm', title: 'Supplier Name', index: 'SupplierNm', width: 250, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
        ]
    };

    _handler.saveUrl = rootPath + "ActManage/SaveInsuranceData";
    _handler.inquiryUrl = rootPath + "ActManage/GetSMIPMDetail";
    _handler.config = BookingLookup;

    _handler.addData = function () {
        //初始化新增数据
        var dep = getCookie("plv3.passport.dep"),ext = getCookie("plv3.passport.ext");
        var data = {
            "CreateBy": userId, "CreateDate": getDate(0, "-"), "Cmp": cmp , "CreateDep": dep, "CreateExt": ext,"JobDate":getDate(0, "-"),"JobCmp":cmp,"Status":"N", "TpvLcur": "USD"
        };
        data[_handler.key] = uuid();
        setFieldValue([data]);
        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        getAutoNo("JobNo", "rulecode=JOB_NO&cmp=" + cmp);   
        $("[chxName='chxflag']").prop("disabled", false);
        $("#Company").val(cmp);
        $("#CompanyNm").val(CmpNm);
    }

    _handler.afterEdit = function(){
        if(IoFlag == 'O')
        {
            $("#InUser input").prop("disabled", true);
            $("#InUser button").prop("disabled", true);
            $("#InUser select").prop("disabled", true);
            $("#IpayDate").prop("disabled", true);
            $("#IpayDate").siblings("span").find("button").prop("disabled", true);
            $(".refrate").attr("readonly", "readonly");
        } 
    }

    _handler.saveData = function (dtd) {
        var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        data["Job_No"] = encodeURIComponent($("#JobNo").val());        
        $("[chxName='chxflag']").each(function (index, el) {
            if ($(this).prop("checked")){
                data[this.name] = "Y";
            } else {
                data[this.name] = "N";
            }
        });
        ajaxHttpSaveBar(dtd, _handler.saveUrl, data,
            function (result) {
                //_topData = keyData["mt"];
                if (result.message) {
                    alert(result.message);
                    return false;
                }
                else if (_handler.setFormData)
                    _handler.setFormData(result);
                return true;
            });
    }

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];
        if (data["sub"])
            _handler.loadGridData("MainGrid", $MainGrid[0], data["sub"], [""]);
        else
            _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        if (data["sub2"])
            _handler.loadGridData("SubGrid", $SubGrid[0], data["sub2"], [""]);
        else
            _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);

        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);        
        if(IoFlag == 'O')
        {
            MenuBarFuncArr.Disabled(["MBAdd","MBACheck","MBApplication","MBFCheck","MBClosed","MBCopy","MBDel"]);
            MenuBarFuncArr.Enabled(["MBEdoc"]);
        }
        else
        {
            MenuBarFuncArr.Enabled(["MBCopy","MBACheck","MBApplication","MBFCheck","MBClosed", "MBEdoc"]);
        }

        if(typeof data["main"][0] !== "undefined")
        {
            var multiEdocData = [];
            if (data["doc"] != null) {
                $(data["doc"]).each(function (index) {
                    multiEdocData.push({ jobNo: data["doc"][index].UId, 'GROUP_ID': data["doc"][index].GroupId, 'CMP': data["doc"][index].Cmp, 'STN': '*', BaseType: "Insu" });//, TYPE: "PACKI;INVI;BL_confirm;" 
                });
                MenuBarFuncArr.initEdoc($("#UId").val(), groupId, $("#Cmp").val(), "*", multiEdocData);
            }
            else {
                MenuBarFuncArr.initEdoc($("#UId").val(), groupId, $("#Cmp").val(), "*");
            }

            var Iflag = data['main'][0]["Iflag"];
            var Cflag = data['main'][0]["Cflag"];
            var Fflag = data['main'][0]["Fflag"];
            var Tflag = data['main'][0]["Tflag"];
            if (Iflag == "Y") {
                $("[chxName='chxflag'][Name='Iflag']").prop("checked",true);
                $("#IflagButton").prop("disabled",false);
            };
            if (Cflag == "Y") {
                $("[chxName='chxflag'][Name='Cflag']").prop("checked",true);
                $("#CflagButton").prop("disabled",false);
            };
            if (Fflag == "Y") {
                $("[chxName='chxflag'][Name='Fflag']").prop("checked",true);
                $("#FflagButton").prop("disabled",false);
            };
            if (Tflag == "Y") {
                $("[chxName='chxflag'][Name='Tflag']").prop("checked",true);
                $("#TflagButton").prop("disabled",false);
            };

            if(IoFlag == "O")
            {
                
                if(data["main"][0]["Status"] == "F" || data["main"][0]["Status"] == "A")
                {
                    MenuBarFuncArr.Disabled(["MBEdit", "MBDel"]);
                }

                if(data["main"][0]["Status"] == "C")
                {
                    MenuBarFuncArr.Disabled(["MBEdit", "MBDel"]);
                }
            }
            var Status = data['main'][0]["Status"];
            if (Status != "F") {
                $('#LossRate').removeAttr("readonly");
                $('#IuRate').removeAttr("readonly");
                $('#CuRate').removeAttr("readonly");
                $('#FuRate').removeAttr("readonly");
                $('#TuRate').removeAttr("readonly");
            }
            else {
                $('#LossRate').removeAttr("readonly", "readonly");
                $('#IuRate').removeAttr("readonly", "readonly");
                $('#CuRate').removeAttr("readonly", "readonly");
                $('#FuRate').removeAttr("readonly", "readonly");
                $('#TuRate').removeAttr("readonly", "readonly");
                $(".refrate").attr("readonly", "readonly");
                MenuBarFuncArr.Disabled(["MBAdd", "MBEdit", "MBACheck", "MBApplication", "MBFCheck", "MBClosed", "MBCopy", "MBDel"]);
                MenuBarFuncArr.Enabled(["MBEdoc"]);
            }
        }

        

    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "ActManage/GetSMIPMDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
        
    }
    _handler.beforEdit = function(map)
    {
        $("[chxName='chxflag']").prop("disabled", false);
    }

    var colModel = [
       { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'UFid', title: 'U FID', index: 'UFid', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'SeqNo', title: language["L_SMIPR_SeqNo"], index: 'SeqNo', sorttype: 'string', width: 60, editable: false, hidden: true },
       { name: 'JobNo', title: language["L_SMIPR_JobNo"], index: 'JobNo', sorttype: 'string', width: 100, editable: false, hidden: true },
       { name: "JobDate", title: language["L_SMIPR_JobDate"], index: "JobDate", width: 150, align: "left", sorttype: "string", formatter: "date", formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d" }, hidden: false },
       { name: 'JobCmp', title: language["L_SMIPR_Location"], index: 'JobCmp', sorttype: 'string', width: 100, editable: false, hidden: true },
       { name: 'CreateBy', title: language["L_SMIPR_CreateBy"], index: 'CreateBy', sorttype: 'string', width: 100, editable: false, hidden: false },
       { name: 'Remark', title: language["L_SMIPR_Remark"], index: 'Remark', sorttype: 'string', width: 300, editable: true, hidden: false }
    ];


    _handler.intiGrid("MainGrid", $MainGrid, {
        colModel: colModel, caption: '@Resources.Locale.L_InsurancePaySetupView_Scripts_83', delKey: ["UId"], height: 100,
        onAddRowFunc: function (rowid) {
            var dep = getCookie("plv3.passport.dep")
            var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            setGridVal($MainGrid, rowid, "SeqNo", maxSeqNo + 1);            
            setGridVal($MainGrid, rowid, "JobNo", $("#JobNo").val());
            setGridVal($MainGrid, rowid, "JobDate", getDate(0, "-"));
            setGridVal($MainGrid, rowid, "JobCmp", cmp);
            setGridVal($MainGrid, rowid, "CreateBy", userId);
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
        }
    });

    var colModel2 = [
       { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'UFid', title: 'U FID', index: 'UFid', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'SeqNo', title: language["L_SMIPC_SeqNo"], index: 'SeqNo', sorttype: 'string', width: 60, editable: false, hidden: false },
       { name: 'JobNo', title: language["L_SMIPC_JobNo"], index: 'JobNo', sorttype: 'string', width: 100, editable: false, hidden: false },
       { name: 'JobType', title: language["L_SMIPC_JobType"], index: 'JobType', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: '@Resources.Locale.L_InsurancePayDetailView_Script_75' }, edittype: 'select' },
       { name: 'InsCd', title: language["L_SMIPC_InsCd"], index: 'InsCd', sorttype: 'string', width: 100, editable: true, hidden: false, editoptions: gridLookup(getop("LspCd")), edittype: 'custom' },
       { name: 'ChgDescp', title: language["L_SMIPC_ChgDescp"], index: 'ChgDescp', width: 80, align: 'left',formatter:"select", editoptions: { value: vChgDescp }, edittype: 'select', editable: true },
       { name: 'IpAmt', title: language["L_SMIPC_IpAmt"], index: 'IpAmt', sorttype: 'string', width: 100, editable: true, hidden: false,  formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true},
       { name: 'Remark', title: language["L_SMIPC_Remark"], index: 'Remark', sorttype: 'string', width: 300, editable: true, hidden: false }
    ];


    _handler.intiGrid("SubGrid", $SubGrid, {
        colModel: colModel2, caption: '@Resources.Locale.L_InsurancePaySetupView_Views_211', delKey: ["UId"], height: 150,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            setGridVal($SubGrid, rowid, "SeqNo", maxSeqNo + 1);            
            setGridVal($SubGrid, rowid, "JobNo", $("#JobNo").val(), null);
            setGridVal($SubGrid, rowid, "InsCd", $("#InsCd").val(), "lookup");
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
        }
    });

    $("#HandCarWindow").on("show.bs.modal", function(){
        
    });


    //提單
    $("#BlNoLookup").v3Lookup({

        url: rootPath + "ActManage/GetSmsmIForSMIPMLookup",// LookUpConfig.Smsm4SMIRMUrl,
        gridFunc: function (map) {
            if (!isEmpty(map.HouseNo)) {
                BlNo = GetInvNo(map.HouseNo, map.ShipmentId);
                $("#BlNo").val(map.HouseNo);
            } else {
                BlNo = GetInvNo(map.MasterNo, map.ShipmentId);
                $("#BlNo").val(map.MasterNo);
            }
            $("#TranType").val(map.TranType);
            $("#Cur").val(map.Cur);
            $("#Amt").val(map.Gvalue);
            $("#Term").val(map.TradeTerm);
            $("#Region").val(map.Region);
            $("#Pol").val(map.PolCd);
            $("#PolNm").val(map.PolName);
            $("#Pod").val(map.PodCd);
            $("#PodNm").val(map.PodName);
            $("#Dest").val(map.DestCd);
            $("#DestNm").val(map.DestName);
            $("#Goods").val(map.Goods);
            $("#ShipperCd").val(map.ShCd);
            $("#ShipperNm").val(map.ShNm);
            $("#FiCd").val(map.FcCd);
            $("#FiNm").val(map.FcNm);
            $("#CneeCd").val(map.CsCd);
            $("#CneeNm").val(map.CsNm);
            $("#Carrier").val(map.Carrier);
            $("#CarrierNm").val(map.CarrierNm);
            $("#LspNo").val(map.LspNo);
            $("#LspNm").val(map.LspNm);
            $("#TermDescp").val(map.TradetermDescp);
            //$("#CntNo").val(map.CntrNo);
            $("#ShipmentId").val(map.ShipmentId);
        },
        lookUpConfig: LookUpConfig.Smsm4SMIRMLookup,
        baseConditionFunc: function () { return ""; },
        responseMethod: function () { return ""; }
    });

    $("#BlNo").v3AutoComplete({
        params: "dt=smsm&ISNULL(HOUSE_NO,'')+ISNULL(MASTER_NO,'')~",
        keyinNum: "1",
        returnValue: "HOUSE_NO&MASTER_NO=showValue,HOUSE_NO,MASTER_NO,TRAN_TYPE,CUR,GVALUE,TRADE_TERM,REGION,POL_CD,POL_NAME,POD_CD,POD_NAME,DEST_CD,DEST_NAME,COMBINE_INFO,GOODS,SH_CD,SH_NM,FC_CD,FC_NM,CS_CD,CS_NM,CARRIER,CARRIER_NM,LSP_NO,LSP_NM,TRADETERM_DESCP,SHIPMENT_ID",
        callBack: function (event, ui) {
            var map = ui.item.returnValue;
             if($(this).val() == map.MASTER_NO)
            {
                
                $(this).val(map.MASTER_NO);
                GetInvNo(map.MASTER_NO, map.SHIPMENT_ID);
            }
            else
            {
                if(ui.item.returnValue.HOUSE_NO != "")
                {
                    $(this).val(map.HOUSE_NO);
                    GetInvNo(map.HOUSE_NO, map.SHIPMENT_ID);
                }
                else
                {
                    $(this).val(map.MASTER_NO);
                    GetInvNo(map.MASTER_NO, map.SHIPMENT_ID);
                }
            }


            $("#TranType").val(map.TRAN_TYPE);
            $("#Cur").val(map.CUR);
            $("#Amt").val(map.GVALUE);
            $("#Term").val(map.TRADE_TERM);
            $("#Region").val(map.REGION);
            $("#Pol").val(map.POL_CD);
            $("#PolNm").val(map.POL_NAME);
            $("#Pod").val(map.POD_CD);
            $("#PodNm").val(map.POD_NAME);
            $("#Dest").val(map.DEST_CD);
            $("#DestNm").val(map.DEST_NAME);
            $("#Goods").val(map.GOODS);
            $("#ShipperCd").val(map.SH_CD);
            $("#ShipperNm").val(map.SH_NM);
            $("#FiCd").val(map.FC_CD);
            $("#FiNm").val(map.FC_NM);
            $("#CneeCd").val(map.CS_CD);
            $("#CneeNm").val(map.CS_NM);
            $("#Carrier").val(map.CARRIER);
            $("#CarrierNm").val(map.CARRIER_NM);
            $("#LspNo").val(map.LSP_NO);
            $("#LspNm").val(map.LSP_NM);
            $("#TradetermDescp").val(map.TRADETERM_DESCP);
            //$("#CntNo").val(map.CNTR_NO);
            return false;
        },
        dymcFunc: function () {
            return "";
        },
        clearFunc: function () {
            $("#BlNo").val("");
        },
        closeFunc: function (data) {
            if (data.length > 0) {
                var map = data[0].returnValue;
                if (map.HOUSE_NO != "") {
                    $("#BlNo").val(map.HOUSE_NO);
                    GetInvNo(map.HOUSE_NO, map.SHIPMENT_ID);
                }
                else if (map.MASTER_NO!="") {

                    $("#BlNo").val(map.MASTER_NO);
                    GetInvNo(map.MASTER_NO, map.SHIPMENT_ID);
                }


                $("#TranType").val(map.TRAN_TYPE);
                $("#Cur").val(map.CUR);
                $("#Amt").val(map.GVALUE);
                $("#Term").val(map.TRADE_TERM);
                $("#Region").val(map.REGION);
                $("#Pol").val(map.POL_CD);
                $("#PolNm").val(map.POL_NAME);
                $("#Pod").val(map.POD_CD);
                $("#PodNm").val(map.POD_NAME);
                $("#Dest").val(map.DEST_CD);
                $("#DestNm").val(map.DEST_NAME);
                $("#Goods").val(map.GOODS);
                $("#ShipperCd").val(map.SH_CD);
                $("#ShipperNm").val(map.SH_NM);
                $("#FiCd").val(map.FC_CD);
                $("#FiNm").val(map.FC_NM);
                $("#CneeCd").val(map.CS_CD);
                $("#CneeNm").val(map.CS_NM);
                $("#Carrier").val(map.CARRIER);
                $("#CarrierNm").val(map.CARRIER_NM);
                $("#LspNo").val(map.LSP_NO);
                $("#LspNm").val(map.LSP_NM);
                $("#TradetermDescp").val(map.TRADETERM_DESCP);
            }
        }
    });

    setSmptyData("ShipperCdLookup", "ShipperCd", "ShprNm", "SH", {});
    setSmptyData("CneeCdLookup", "CneeCd", "CneeNm", "CS", {});
    setSmptyData("CarrierLookup", "Carrier", "CarrierNm", "FS", {});
    setSmptyData("LspNoLookup", "LspNo", "LspNoLookup", "", {});
    setSmptyData("InsCdLookup", "InsCd", "InsNm", "", {});
    setSmptyData("FiCdLookup", "FiCd", "FiNm", "FC", {});

    $("#JobCmpLookup").v3Lookup({
        url: rootPath + "Common/GetPartyNoData",
        gridFunc: function (map) {
            $("#JobCmp").val(map.PartyNo);
        },
        lookUpConfig: LookUpConfig.SmptyLookup,
        baseConditionFunc: function () { return " GROUP_ID='" + groupId + "' AND PARTY_TYPE LIKE '%LC%'"; },
        responseMethod: function () { return ""; }
    });

    $("#JobCmp").v3AutoComplete({
        params: "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=",
        keyinNum: "1",
        returnValue: "CMP=showValue,CMP,NAME",
        callBack: function (event, ui) {
            $(this).val(ui.item.returnValue.CMP);
            return false;
        },
        dymcFunc: function () {
            return "";
        },
        clearFunc: function () {
            $("#JobCmp").val("");
        }
    });

    $("#CompanyLookup").v3Lookup({
        url: rootPath + "TPVCommon/GetSiteCmpData",
        gridFunc: function (map) {
            $("#Company").val(map.Cd);
            $("#CompanyNm").val(map.CdDescp);
        },
        lookUpConfig: LookUpConfig.SiteLookup,
        baseConditionFunc: function () { return " GROUP_ID='" + groupId + "' AND TYPE='1'"; },
        responseMethod: function () { return ""; }
    });

    $("#Company").v3AutoComplete({
        params: "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=",
        keyinNum: "1",
        returnValue: "CMP=showValue,CMP,NAME",
        callBack: function (event, ui) {
            $(this).val(ui.item.returnValue.CMP);
            $("#CompanyNm").val(ui.item.returnValue.NAME);
            return false;
        },
        dymcFunc: function () {
            return "";
        },
        clearFunc: function () {
            $("#Company").val("");
            $("#CompanyNm").val("");
        },
        closeFunc: function (data) {
            if (data.length > 0) {
                var map = data[0].returnValue;
                $("#Company").val(map.CMP);
                $("#CompanyNm").val(map.NAME);
            }
        }
    });
    //幣別
    $("#CurLookup").v3Lookup({
        url: rootPath + LookUpConfig.CurUrl,
        gridFunc: function (map) {
            $("#Cur").val(map.Cur);
        },
        lookUpConfig: LookUpConfig.CurLookup,
        baseConditionFunc: function () { return ""; },
        responseMethod: function () { return ""; }
    });

    $("#Cur").v3AutoComplete({
        params: "dt=crn&GROUP_ID=" + groupId + "&CUR%",
        keyinNum: "1",
        returnValue: "CUR&CUR_DESCP=showValue,CUR,CUR_DESCP",
        callBack: function (event, ui) {
            var map = ui.item.returnValue;
            $(this).val(map.CUR);
            return false;
        },
        dymcFunc: function () {
            return "";
        },
        clearFunc: function () {
            $("#Cur").val("");
        }
    });

    $("#LossCurLookup").v3Lookup({
        url: rootPath + LookUpConfig.CurUrl,
        gridFunc: function (map) {
            $("#LossCur").val(map.Cur);
        },
        lookUpConfig: LookUpConfig.CurLookup,
        baseConditionFunc: function () { return ""; },
        responseMethod: function () { return ""; }
    });

    $("#LossCur").v3AutoComplete({
        params: "dt=crn&GROUP_ID=" + groupId + "&CUR%",
        keyinNum: "1",
        returnValue: "CUR&CUR_DESCP=showValue,CUR,CUR_DESCP",
        callBack: function (event, ui) {
            var map = ui.item.returnValue;
            $(this).val(map.CUR);
            return false;
        },
        dymcFunc: function () {
            return "";
        },
        clearFunc: function () {
            $("#LossCur").val("");
        }
    });

    $("#IcurLookup").v3Lookup({
        url: rootPath + LookUpConfig.CurUrl,
        gridFunc: function (map) {
            $("#Icur").val(map.Cur);
        },
        lookUpConfig: LookUpConfig.CurLookup,
        baseConditionFunc: function () { return ""; },
        responseMethod: function () { return ""; }
    });

    $("#Icur").v3AutoComplete({
        params: "dt=crn&GROUP_ID=" + groupId + "&CUR%",
        keyinNum: "1",
        returnValue: "CUR&CUR_DESCP=showValue,CUR,CUR_DESCP",
        callBack: function (event, ui) {
            var map = ui.item.returnValue;
            $(this).val(map.CUR);
            return false;
        },
        dymcFunc: function () {
            return "";
        },
        clearFunc: function () {
            $("#Icur").val("");
        }
    });

    $("#CcurLookup").v3Lookup({
        url: rootPath + LookUpConfig.CurUrl,
        gridFunc: function (map) {
            $("#Ccur").val(map.Cur);
        },
        lookUpConfig: LookUpConfig.CurLookup,
        baseConditionFunc: function () { return ""; },
        responseMethod: function () { return ""; }
    });

    $("#Ccur").v3AutoComplete({
        params: "dt=crn&GROUP_ID=" + groupId + "&CUR%",
        keyinNum: "1",
        returnValue: "CUR&CUR_DESCP=showValue,CUR,CUR_DESCP",
        callBack: function (event, ui) {
            var map = ui.item.returnValue;
            $(this).val(map.CUR);
            return false;
        },
        dymcFunc: function () {
            return "";
        },
        clearFunc: function () {
            $("#Ccur").val("");
        }
    });

    $("#FcurLookup").v3Lookup({
        url: rootPath + LookUpConfig.CurUrl,
        gridFunc: function (map) {
            $("#Fcur").val(map.Cur);
        },
        lookUpConfig: LookUpConfig.CurLookup,
        baseConditionFunc: function () { return ""; },
        responseMethod: function () { return ""; }
    });

    $("#Fcur").v3AutoComplete({
        params: "dt=crn&GROUP_ID=" + groupId + "&CUR%",
        keyinNum: "1",
        returnValue: "CUR&CUR_DESCP=showValue,CUR,CUR_DESCP",
        callBack: function (event, ui) {
            var map = ui.item.returnValue;
            $(this).val(map.CUR);
            return false;
        },
        dymcFunc: function () {
            return "";
        },
        clearFunc: function () {
            $("#Fcur").val("");
        }
    });

    $("#TcurLookup").v3Lookup({
        url: rootPath + LookUpConfig.CurUrl,
        gridFunc: function (map) {
            $("#Tcur").val(map.Cur);
        },
        lookUpConfig: LookUpConfig.CurLookup,
        baseConditionFunc: function () { return ""; },
        responseMethod: function () { return ""; }
    });

    $("#Tcur").v3AutoComplete({
        params: "dt=crn&GROUP_ID=" + groupId + "&CUR%",
        keyinNum: "1",
        returnValue: "CUR&CUR_DESCP=showValue,CUR,CUR_DESCP",
        callBack: function (event, ui) {
            var map = ui.item.returnValue;
            $(this).val(map.CUR);
            return false;
        },
        dymcFunc: function () {
            return "";
        },
        clearFunc: function () {
            $("#Tcur").val("");
        }
    });

    $("#PolLookup").v3Lookup({
        url: rootPath + LookUpConfig.CityPortUrl,
        gridFunc: function (map) {
            $("#Pol").val(map.CntryCd + map.PortCd);
            $("#PolNm").val(map.PortNm);
        },
        lookUpConfig: LookUpConfig.CityPortLookup,
        baseConditionFunc: function () { return ""; },
        responseMethod: function () { return ""; }
    });

    $("#Pol").v3AutoComplete({
        params: "dt=port1&GROUP_ID=" + groupId + "&CNTRY_CD+PORT_CD%",
        keyinNum: "1",
        returnValue: "CNTRY_CD&PORT_CD=showValue,CNTRY_CD,PORT_CD,PORT_NM",
        callBack: function (event, ui) {
            var map = ui.item.returnValue;
            $("#Pol").val(map.CNTRY_CD + map.PORT_CD);
            $("#PolNm").val(map.PORT_NM);
            return false;
        },
        dymcFunc: function () {
            return "";
        },
        clearFunc: function () {
            $("#Pol").val("");
        }
    });

    $("#PodLookup").v3Lookup({
        url: rootPath + LookUpConfig.CityPortUrl,
        gridFunc: function (map) {
            $("#Pod").val(map.CntryCd + map.PortCd);
            $("#PodNm").val(map.PortNm);
        },
        lookUpConfig: LookUpConfig.CityPortLookup,
        baseConditionFunc: function () { return ""; },
        responseMethod: function () { return ""; }
    });

    $("#Pod").v3AutoComplete({
        params: "dt=port1&GROUP_ID=" + groupId + "&CNTRY_CD+PORT_CD%",
        keyinNum: "1",
        returnValue: "CNTRY_CD&PORT_CD=showValue,CNTRY_CD,PORT_CD,PORT_NM",
        callBack: function (event, ui) {
            var map = ui.item.returnValue;
            $("#Pod").val(map.CNTRY_CD + map.PORT_CD);
            $("#PodNm").val(map.PORT_NM);
            return false;
        },
        dymcFunc: function () {
            return "";
        },
        clearFunc: function () {
            $("#Pod").val("");
        }
    });

    $("#DestLookup").v3Lookup({
        url: rootPath + LookUpConfig.CityPortUrl,
        gridFunc: function (map) {
            $("#Dest").val(map.CntryCd + map.PortCd);
            $("#DestNm").val(map.PortNm);
        },
        lookUpConfig: LookUpConfig.CityPortLookup,
        baseConditionFunc: function () { return ""; },
        responseMethod: function () { return ""; }
    });

    $("#Dest").v3AutoComplete({
        params: "dt=port1&GROUP_ID=" + groupId + "&CNTRY_CD+PORT_CD%",
        keyinNum: "1",
        returnValue: "CNTRY_CD&PORT_CD=showValue,CNTRY_CD,PORT_CD,PORT_NM",
        callBack: function (event, ui) {
            var map = ui.item.returnValue;
            $("#Dest").val(map.CNTRY_CD + map.PORT_CD);
            $("#DestNm").val(map.PORT_NM);
            return false;
        },
        dymcFunc: function () {
            return "";
        },
        clearFunc: function () {
            $("#Dest").val("");
        }
    });

    registBtnLookup($("#TpvLcurLookup"), {
        item: '#TpvLcur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
            $("#TpvLcur").val(map.Cur);
            //$("#Amt").val(map.PartyName);
        }
    }, undefined, LookUpConfig.GetCurAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#TpvLcur").val(rd.CUR);
    }));

    //貿易條件
    registBtnLookup($("#TermLookup"), {
        item: '#Term', url: rootPath + LookUpConfig.TermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
            $("#Term").val(map.Cd);
            $("#TermDescp").val(map.Name);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TD", undefined, function ($grid, rd, elem) {
        $("#Term").val(rd.CD);
    }));
    
    registBtnLookup($("#LspNoLookup"), {
        item: '#LspNo', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#LspNo").val(map.PartyNo);
            $("#LspNm").val(map.PartyName);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#LspNm").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#LocationLookup"), {
        item: '#Location', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#Location").val(map.PartyNo);
            $("#LocName").val(map.PartyName);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#LocName").val(rd.PARTY_NAME);
    }));
    
    registBtnLookup($("#RegionLookup"), {
        item: '#Region', url: rootPath + LookUpConfig.ProvinceUrl, config: LookUpConfig.ProvinceLookup, param: "", selectRowFn: function (map) {
            $("#Region").val(map.RegionCd);
        }
    }, undefined, LookUpConfig.GetProvinceAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Region").val(rd.REGION_CD);
    }, function ($grid, elem) {
        $("#Region").val("");
    }));

    registBtnLookup($("#CntyLookup"), {
        item: '#Cnty', url: rootPath + LookUpConfig.CountryUrl, config: LookUpConfig.CountryLookup, param: "", selectRowFn: function (map) {
            $("#Cnty").val(map.CntryCd);
        }
    }, undefined, LookUpConfig.GetCountryAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Cnty").val(rd.CNTRY_CD);
    }, function ($grid, elem) {
        $("#Cnty").val("");
    }));
    

    // MenuBarFuncArr.MBEdoc = function (thisItem) {
    //     initEdoc(thisItem, { jobNo: $("#UId").val(), GROUP_ID: groupId, CMP: cmp, STN: "*" });
    // }

    _handler.beforLoadView = function () {
        var keys = ["Uid"];
        for (var i = 0; i < keys.length; i++) {
            $("#" + keys[i]).attr('isKey', true);
        }

        var requires = [];
        for (var i = 0; i < requires.length; i++) {
            $("#" + requires[i]).attr('required', true);
            $("[for=" + requires[i] + "]").css("color", "rgb(255, 0, 0)");
        }
        var readonlys = ["JobNo", "Status"];
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('readonly', true);
        } 
    }
    MenuBarFuncArr.AddMenu("MBApplication", "glyphicon glyphicon-bell", "@Resources.Locale.L_SYS_APPLY", function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
        if (_handler.topData["Status"] != "N") {
            var status = _handler.topData["Status"];
            var tip;
            switch (status) {
                case "Y": tip = "@Resources.Locale.L_InsuranceManage_StatusY"; break;
                case "C": tip = "@Resources.Locale.L_InsuranceManage_StatusC"; break;
                case "A": tip = "@Resources.Locale.L_InsuranceManage_StatusA"; break;
                case "F": tip = "@Resources.Locale.L_SMIPM_EndingBy"; break;
                case "I": tip = "@Resources.Locale.L_MenuBar_Problem"; break;
            }
            CommonFunc.Notify("", "@Resources.Locale.L_InsurancePaySetup_Tip1" + tip + "@Resources.Locale.L_InsurancePaySetup_Tip2", 500, "warning");
            return false;
        }
        if (!confirm("@Resources.Locale.L_InsurancePaySetup_confirm"))
        {
            return false;
        }
        changeStatus("Y");
    });
    MenuBarFuncArr.AddMenu("MBACheck", "glyphicon glyphicon-bell", "@Resources.Locale.L_SMIPM_ConfirmBy", function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
        if (_handler.topData["Status"] === "C") {
            CommonFunc.Notify("", "@Resources.Locale.L_InsurancePaySetupView_Scripts_84", 500, "warning");
            return false;
        }
        if (_handler.topData["Status"] === "A") {
            CommonFunc.Notify("", "@Resources.Locale.L_InsurancePaySetupView_Scripts_85", 500, "warning");
            return false;
        }
        if (_handler.topData["Status"] === "F") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasClosed", 500, "warning");
            return false;
        }
        if (!confirm("@Resources.Locale.L_SMIPC_IpAmtTip1")) {
            return false;
        }
        changeStatus("C");
    });

    
    MenuBarFuncArr.AddMenu("MBFCheck", "glyphicon glyphicon-bell", "@Resources.Locale.L_SMIPM_AcBy", function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }

        if(dep !== "SFI")
        {
            CommonFunc.Notify("", "@Resources.Locale.L_InsurancePaySetupView_Script_76", 500, "warning");
            return false;
        }
        if (_handler.topData["Status"] === "A") {
            CommonFunc.Notify("", "@Resources.Locale.L_InsurancePaySetupView_Scripts_85", 500, "warning");
            return false;
        }
        if (_handler.topData["Status"] === "F") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasClosed", 500, "warning");
            return false;
        }

        if($("input[chxName='chxflag']:eq(0)").prop("checked") == true && ($("#IpayDate").val() == "" || $("#IpayDate").val() == null))
        {
            CommonFunc.Notify("", "@Resources.Locale.L_InsurancePaySetupView_Scripts_87", 500, "warning");
            return false;
        }

        if($("input[chxName='chxflag']:eq(1)").prop("checked") == true && ($("#CpayDate").val() == "" || $("#CpayDate").val() == null))
        {
            CommonFunc.Notify("", "@Resources.Locale.L_InsurancePaySetupView_Scripts_88", 500, "warning");
            return false;
        }

        if($("input[chxName='chxflag']:eq(2)").prop("checked") == true && ($("#FpayDate").val() == "" || $("#FpayDate").val() == null))
        {
            CommonFunc.Notify("", "@Resources.Locale.L_InsurancePaySetupView_Scripts_89", 500, "warning");
            return false;
        }

        if($("input[chxName='chxflag']:eq(1)").prop("checked") == true && ($("#TpayDate").val() == "" || $("#TpayDate").val() == null))
        {
            CommonFunc.Notify("", "@Resources.Locale.L_InsurancePaySetupView_Scripts_90", 500, "warning");
            return false;
        }
        if (!confirm("@Resources.Locale.L_SMIPC_IpAmtTip2")) {
            return false;
        }
        changeStatus("A");
    });

    MenuBarFuncArr.AddMenu("MBClosed", "glyphicon glyphicon-bell", "@Resources.Locale.L_SMIPM_EndingBy", function () {
        if (!confirm("@Resources.Locale.L_SMIPC_IpAmtTip3")) {
            return false;
        }
        var TpvLamt = RemoveComma($("#TpvLamt").val());
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;;
        }

        if (_handler.topData["Status"] === "F") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasClosed", 500, "warning");
            return false;
        }

        if(TpvLamt <= 5000 && $("#CreateBy").val() == userId)
        {
            changeStatus("F");
            return;
        }
        else
        {
            if (_handler.topData["Status"] !== "A")
             {
                CommonFunc.Notify("", "@Resources.Locale.L_InsurancePaySetupView_Script_77", 500, "warning");
                return false;
            }
        }
        
        changeStatus("F");
    });

    MenuBarFuncArr.MBCopy = function (thisItem) {
        getAutoNo("JobNo", "rulecode=JOB_NO&cmp=" + cmp);   
        MenuBarFuncArr.Disabled(["MBFCheck","MBClosed","MBACheck","MBApplication"]);
        $("#UId").removeAttr('required');
        $("#UId").val("");
        $("#ConfirmBy").val("");
        $("#ConfirmDate").val("");
        $("#AcBy").val("");
        $("#AcDate").val("");
        $("#EndingBy").val("");
        $("#EndingDate").val("");
        $("#Status").val("Y");
        $("#ModifyBy").val("");
        $("#ModifyDate").val("");
        $("#ApplicationDate").val("");
        $MainGrid.jqGrid("clearGridData");
        $("[chxName='chxflag']").prop("disabled", false);
    }
    
    _initUI(["MBApply", "MBApprove", "MBErrMsg"]);//初始化UI工具栏

    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    function changeStatus(Status) {
        var Uid = encodeURIComponent($("#UId").val());
        var nowStatus = encodeURIComponent($("#Status").val());
        
        $.ajax({
            async: true,
            url: rootPath + "ActManage/ChangeInsuranceStatus",
            type: 'POST',
            data: {
                "Uid": Uid,
                "Status": Status
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
                if (result.message == "success" && Status == "C") {
                    CommonFunc.Notify("", "@Resources.Locale.L_InsurancePaySetupView_Scripts_93", 500, "success");
                }
                else if (result.message == "success" && Status == "A") {
                    CommonFunc.Notify("", "@Resources.Locale.L_InsurancePaySetupView_Scripts_94", 500, "success");
                    MenuBarFuncArr.Disabled(["MBEdit", "MBDel"]);
                }
                else if (result.message == "success" && Status == "F") {
                    CommonFunc.Notify("", "@Resources.Locale.L_InsurancePaySetupView_Scripts_95", 500, "success");
                    MenuBarFuncArr.Disabled(["MBEdit", "MBDel"]);
                }
                else {
                    CommonFunc.Notify("", result.message, 500, "warning");
                }
                var formData = $.parseJSON(result.returnData.Content);
                _handler.setFormData(formData);
            }
        });

    }
    function GetInvNo(BlNo, ShipmentId)
    {
        $.ajax({
            async: true,
            url: rootPath + "ActManage/GetInvNo",
            type: 'POST',
            data: {
                "BlNo": BlNo,
                "ShipmentId": ShipmentId
            },
            success: function (result) {
                $("#InvNo").val(result.InvNoList);
                $("#CntNo").val(result.CntNoList);
                //return result;
            }
        });        
    }
    
    function getop(Uid) {
        var city_op = getLookupOp("SubGrid",
            {
                url: rootPath + LookUpConfig.InsInfoUrl,
                config: LookUpConfig.InsInfoLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, 'InsCd', map.CreateBy, "lookup");
                    return map.CreateBy;

                }
            }, LookUpConfig.GetInsInfoAuto(Uid, SubGrid,
            function ($grid, rd, elem, selRowId) {
                setGridVal($grid, selRowId, 'InsCd', rd.CREATE_BY, "lookup");
                $(elem).val(rd.CREATE_BY);
            }), {
                param: "",
                baseConditionFunc: function () {
                    return "";
                }
            });
        return city_op;
    }

    changeExrate();
});

function changeExrate()
{
    $("#Iamt").on("change", function () {
        var IuRate  = $("#IuRate").val()
        if (IuRate == 0 || IuRate == undefined) {
            var Icur = $("#Icur").val();
            var val = $(this).val();
            $.post(rootPath + 'ActManage/changeExrate', { 'Fcur': Icur, 'Tcur': 'USD', 'Famt': val }, function (data, textStatus, xhr) {
                $("#Iuamt").val(data['amt']);
                $("#IuRate").val(data['exRate']);
                sumTpvLamt();
            });
        }
        else {
            HandRateCalculation("Iuamt", $(this).val(), IuRate);
        }
    });

    $("#Camt").on("change", function () {
        var CuRate = $("#CuRate").val()
        if (CuRate == 0 || CuRate == undefined) {
            var Ccur = $("#Ccur").val();
            var val = $(this).val();
            $.post(rootPath + 'ActManage/changeExrate', { 'Fcur': Ccur, 'Tcur': 'USD', 'Famt': val }, function (data, textStatus, xhr) {
                $("#Cuamt").val(data['amt']);
                $("#CuRate").val(data['exRate']);
                sumTpvLamt();
            });
        }
        else {
            HandRateCalculation("Cuamt", $(this).val(), CuRate);
        }
    });

    $("#Tamt").on("change", function () {
        var TuRate = $("#TuRate").val()
        if (TuRate == 0 || TuRate == undefined) {
            var Tcur = $("#Tcur").val();
            var val = $(this).val();
            $.post(rootPath + 'ActManage/changeExrate', { 'Fcur': Tcur, 'Tcur': 'USD', 'Famt': val }, function (data, textStatus, xhr) {
                $("#Tuamt").val(data['amt']);
                $("#TuRate").val(data['exRate']);
                sumTpvLamt();
            });
        }
        else {
            HandRateCalculation("Tuamt", $(this).val(), TuRate);
        }
    });

    $("#Famt").on("change", function(){
        var FuRate = $("#FuRate").val()
        if (FuRate == 0 || FuRate == undefined) {
            var Fcur = $("#Fcur").val();
            var val = $(this).val();
            $.post(rootPath + 'ActManage/changeExrate', { 'Fcur': Fcur, 'Tcur': 'USD', 'Famt': val }, function (data, textStatus, xhr) {
                $("#Fuamt").val(data['amt']);
                $("#FuRate").val(data['exRate']);
                sumTpvLamt();
            });
        }
        else {
            HandRateCalculation("Fuamt", $(this).val(), FuRate);
        }
    });

    $("#LossAmt").on("change", function () {
        var LossRate = $("#LossRate").val()
        if (LossRate == 0 || LossRate == undefined) {
            var LossCur = $("#LossCur").val();
            var val = $(this).val();
            $.post(rootPath + 'ActManage/changeExrate', { 'Fcur': LossCur, 'Tcur': 'USD', 'Famt': val }, function (data, textStatus, xhr) {
                $("#LossUamt").val(data['amt']);
                $("#LossRate").val(data['exRate']);
                sumTpvLamt();
            });
        }
        else {
            HandRateCalculation("LossUamt", $(this).val(), LossRate);
        }
    });

    $(".refrate").on("change", function () {
        if ($(this).length > 0)
        {
            var id = $(this)[0].id;
            var Rate = $(this).val();
            switch (id)
            {
                case "LossRate": HandRateCalculation("LossUamt", $("#LossAmt").val(), Rate); break;
                case "IuRate": HandRateCalculation("Iuamt", $("#Iamt").val(), Rate); break;
                case "CuRate": HandRateCalculation("Cuamt", $("#Camt").val(), Rate); break;
                case "FuRate": HandRateCalculation("Fuamt", $("#Famt").val(), Rate); break;
                case "TuRate": HandRateCalculation("Tuamt", $("#Tamt").val(), Rate); break;
            }

        }
    });
}
function HandRateCalculation(id,amt,rate)
{
    var re = new RegExp(",", "g");
    amt = amt.replace(re, "");
    var _amt = parseFloat(amt);
    //rate = parseFloat(rate);
    _amt = _amt * rate;
    _amt = _amt.toFixed(2);
    $("#" + id).val(_amt);
    sumTpvLamt();
}

function setSmptyData(lookUp, Cd, Nm, pType, opt)
{
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetSmptyDataForLookup";
    options.registerBtn = $("#"+lookUp);
    options.focusItem = $("#" + Cd);
    options.param = "";
    options.baseCondition = " PARTY_TYPE LIKE '%"+pType+"%'";
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        var addr_str = (map.PartAddr1 || "") + " " + (map.PartAddr2 || "") + " " + (map.PartAddr3 || "") + " " + (map.PartAddr4 || "") + " " + (map.PartAddr5 || "");
        $("#" + Cd).val(map.PartyNo);

        if(Nm != "")
            $("#" + Nm).val(map.PartyName);

        if(typeof opt.Addr !== "undefined")
        {
            $(opt.Addr).val(addr_str);
        }

        if(typeof opt.Attn !== "undefined")
        {
            $(opt.Attn).val(map.PartyAttn);
        }

        if(typeof opt.Tel !== "undefined")
        {
            $(opt.Tel).val(map.PartyTel);
        }

        if(typeof opt.Fax !== "undefined")
        {
            $(opt.Fax).val(map.PartyFax);
        }

        
    }

    options.lookUpConfig = LookUpConfig.SmptyLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#"+Cd, 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO=", "PARTY_NO=showValue,PARTY_NO,PARTY_NAME,PART_ADDR1,PART_ADDR2,PART_ADDR3,PART_ADDR4,PART_ADDR5,PARTY_ATTN,PARTY_TEL,PARTY_FAX", function (event, ui) {
        var addr_str = (ui.item.returnValue.PART_ADDR1 || "") + " " + (ui.item.returnValue.PART_ADDR2 || "") + " " + (ui.item.returnValue.PART_ADDR3 || "") + " " + (ui.item.returnValue.PART_ADDR4 || "") + " " + (ui.item.returnValue.PART_ADDR5 || "");


        $(this).val(ui.item.returnValue.PARTY_NO);

        if(Nm != "")
            $("#" + Nm).val(ui.item.returnValue.PARTY_NAME);

        if(typeof opt.Addr !== "undefined")
        {
            $(opt.Addr).val(addr_str);
        }

        if(typeof opt.Attn !== "undefined")
        {
            $(opt.Attn).val(ui.item.returnValue.PARTY_ATTN);
        }

        if(typeof opt.Tel !== "undefined")
        {
            $(opt.Tel).val(ui.item.returnValue.PARTY_TEL);
        }

        if(typeof opt.Fax !== "undefined")
        {
            $(opt.Fax).val(ui.item.returnValue.PARTY_FAX);
        }
        return false;
    });
}


function sumTpvLamt()
{
    var amt1 = parseFloat(RemoveComma($("#Iuamt").val()));
    var amt2 = parseFloat(RemoveComma($("#Fuamt").val()));
    var amt3 = parseFloat(RemoveComma($("#Tuamt").val()));
    var amt4 = parseFloat(RemoveComma($("#Cuamt").val()));
    var amt5 = parseFloat(RemoveComma($("#LossUamt").val()));
    var sum = CommonFunc.formatFloat(amt5-(amt1+amt2+amt3+amt4),2);
    $("#TpvLamt").val(sum);
}
