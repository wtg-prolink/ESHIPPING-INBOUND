function doDownloadExcel($cgrid, VenderCd) {

    /*var selRowId = $cgrid.jqGrid('getGridParam', 'selrow');
    if (typeof selRowId == "undefined") {
        CommonFunc.Notify("", "请选择一笔数据", 500, "warning");
        return;
    }

    var ioFlag = getCookie("plv3.passport.ioflag");
    var TargetCmp = "";
    if (ioFlag == "I") {
        TargetCmp = prompt("請輸入物流業者號碼", "");
        if (TargetCmp === null) {
            CommonFunc.Notify("", "取消下載", 500, "warning");
            return; //break out of the function early
        }

    }
    
    if(TargetCmp==null||TargetCmp==="")
        TargetCmp = getCookie("plv3.passport.companyid");

    var selRowId = $cgrid.jqGrid('getGridParam', 'selrow');
    var OrderCt = $cgrid.jqGrid('getCell', selRowId, 'CtId');
    var selRowIds = $cgrid.jqGrid('getGridParam', 'selarrrow');
    $.each(selRowIds, function (index, val) {
        if (OrderCt != $cgrid.jqGrid('getCell', selRowIds[index], 'CtId')) {
            CommonFunc.Notify("", "帳單Control Tower必須一致，請取消勾選 訂單號 :" + $cgrid.jqGrid('getCell', selRowIds[index], 'OrdNo') + " 再下載帳單 ", 500, "warning");
            return;
        }
    });*/

    var postData = {};
    if(typeof VenderCd !== "undefined")
    {
        postData = {"VenderCd": VenderCd};
    }
    
    $.ajax({
        async: true,
        url: rootPath + "OrderManage/GetTransTypeInfo",
        type: 'POST',
        data: postData,
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.ToogleLoading(false);
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
        },
        success: function (result) {

            if(result.message != "success"){
                CommonFunc.Notify("", result.message, 500, "warning");
                return;
            }

            var url = $cgrid.jqGrid("getGridParam", "url");
            if (url == null || url == "") {
                return false;
            }
            var colModel = [
               { name: 'rn', title: 'rn', index: 'rn', sorttype: 'string', width: 100, align: 'left', hidden: false },
                            { name: 'TruckType', title: '車輛類型||TRUCK_TYPE', index: 'TruckType', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            //{ name: 'ChgCd', title: '費用代碼||CHG_CD', index: 'ChgCd', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'PolNm', title: '启运地名称||POL_NM', index: 'PolNm', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            //{ name: 'PolCd', title: '启运地代码||POL_CD', index: 'PolCd', sorttype: 'string', width: 120, align: 'left', hidden: false },
                            { name: 'PodNm', title: '目的地名称||POD_NM', index: 'PodNm', sorttype: 'string',  width: 100, align: 'left', hidden: false },
                            //{ name: 'PodCd', title: '目的地代碼||POD_CD', index: 'PodCd', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'Tt', title: '承诺运输交期（小时）||TT', index: 'Tt', sorttype: 'int', align: 'right', formatter: 'integer', width: 100, hidden: false },
                            { name: 'PickFee', title: '提貨費||PICK_FEE', index: 'PickFee', sorttype: 'float', align: 'right', formatter: 'number', width: 100, hidden: false, formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' } },
                            { name: 'DlvFee', title: '送貨費||DLV_FEE', index: 'DlvFee', sorttype: 'float', align: 'right', formatter: 'number', width: 100, hidden: false, formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' } },
                            { name: 'MinAmt', title: '最低收費||MIN_AMT', index: 'MinAmt', sorttype: 'float', align: 'right', formatter: 'number', width: 100, hidden: false, formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' } }
                        ];

            //var qAmtSql = "  SELECT SUM(ISNULL(SMBID.Qamt,0)) FROM SMBID WHERE SMBID.SHIPMENT_ID = S.SHIPMENT_ID AND SMBID.LSP_NO = '" + TargetCmp + "' ";
            //grid.jqGrid("getGridParam", "colModel").slice();
            
            var transTypeCols=[], gwTypeCols=[], cbmTypeCols=[];
            if(result.chgTypeStr != "")
                transTypeCols = result.chgTypeStr.split(";");
            if(result.gwTypeStr != "")
                gwTypeCols = result.gwTypeStr.split(";");
            if(result.cbmTypeStr != "")
                cbmTypeCols = result.cbmTypeStr.split(";");

            var transTypeColsN = result.chgTypeColsStr.split(";");
            //var virtualCol = " ,TRAN_TYPE, CHG_CD,POL_NM,POL_CD,POD_NM,POD_CD,TT,MIN_AMT ";
            var colNames = ["rn", "車輛類型", "启运地名称", "目的地名称", "承诺运输交期（小时）","提貨費", "送貨費", "最低收費"];//grid.jqGrid("getGridParam", "colNames");
            $.each(transTypeCols, function (index, val) {
                colModel.push({ name: transTypeColsN[index], title: val, index: transTypeColsN[index], sorttype: 'float', width: 40, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false });
                colNames.push(val);
                //virtualCol += " ,(" + qAmtSql + " AND CHG_CD = '" + val.split("-")[0] + "' ) AS '" + val.split("-")[0] + "'";
            });
            $.each(gwTypeCols, function (index, val) {
                colModel.push({ name: transTypeColsN[index], title: val, index: transTypeColsN[index], sorttype: 'float', width: 40, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false });
                colNames.push(val);
                //virtualCol += " ,(" + qAmtSql + " AND CHG_CD = '" + val.split("-")[0] + "' ) AS '" + val.split("-")[0] + "'";
            });
            $.each(cbmTypeCols, function (index, val) {
                colModel.push({ name: transTypeColsN[index], title: val, index: transTypeColsN[index], sorttype: 'float', width: 40, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false });
                colNames.push(val);
                //virtualCol += " ,(" + qAmtSql + " AND CHG_CD = '" + val.split("-")[0] + "' ) AS '" + val.split("-")[0] + "'";
            });
            console.log(colModel);
            console.log(colNames);
            
            var caption = "Quotation \r\n Control Tower: "+result.stn+"  User : "+result.user+"   Date : "+result.createDate;
            var excelName = "Quotation";

            /*var selRowId = $cgrid.jqGrid('getGridParam', 'selarrrow');
            var conditions = "1=1 AND " + $cgrid.jqGrid("getGridParam", "postData").conditions;
            var selKey = "";
            $.each(selRowId, function (index, val) {
                selKey += $cgrid.jqGrid('getCell', selRowId[index], 'UId') + ";";
            });

            if (selKey != "") {
                conditions += "&sopt_UId=in&UId=" + selKey.slice(0, -1) + "";
            }
            var baseCondition = $cgrid.jqGrid("getGridParam", "postData").baseCondition;
            if (typeof baseCondition == "undefined") {
                baseCondition = "";
            }*/
            ExportDataToExcelByParam(url, colModel, colNames, caption, excelName, "", " U_ID IS NULL", "");
        }
    });

}

function downloadDebitExcel(grid, OrdNo, ArAp)
{
    $.ajax({
        async: true,
        url: rootPath + "ActManage/GetTransTypeInfo",
        type: 'POST',
        data: {
            "ids": OrdNo,
            "ArAp": ArAp
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

            if(result.message != "success"){
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
                            //{ name: 'Stn', title: 'CONTROL TOWER||STN', index: 'Stn', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'OrdNo', title: 'Order No||ORD_NO', index: 'OrdNo', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'ArAp', title: '應收/應付||AR_AP', index: 'ArAp', sorttype: 'string', align: 'left', width: 120, hidden: false, formatter: "select", editoptions: { value: 'R:Receivable;P:Payable' } },
                            { name: 'Atd', title: 'PICKUP DATE||ATD', index: 'Atd', sorttype: 'string',  width: 100, align: 'left', formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                            { name: 'RefNo1', title: 'Reference No1||REF_NO1', index: 'RefNo1', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'RefNo2', title: 'Reference No2||REF_NO1', index: 'RefNo2', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'TruckNo', title: 'Truck No||TRUCK_NO', index: 'TruckNo', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'Hqid', title: 'Customer||HQID', index: 'Hqid', sorttype: 'string', align: 'left', width: 80, hidden: false },
                            { name: 'CustName', title: 'Customer Name||CUST_NAME', index: 'CustName', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'VenderHqid', title: 'Vender||VENDER_HQID', index: 'VenderHqid', sorttype: 'string', align: 'left', width: 80, hidden: false },
                            { name: 'VenderCd', title: 'Vender Code||VENDER_CD', index: 'VenderCd', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'VenderNm', title: 'Vender Name||VENDER_NM', index: 'VenderNm', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'DlvAreaNm', title: 'DEST||DLV_AREA_NM', index: 'DlvAreaNm', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'PickAreaNm', title: 'ORG||PICK_AREA_NM', index: 'PickAreaNm', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'Gw', title: 'GW||GW', index: 'Gw', sorttype: 'float', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false },
                            { name: 'PkgNum', title: 'CTNS||PKG_NUM', index: 'PkgNum', sorttype: 'float', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false },
                            { name: 'PickDlvRemark', title: 'REMARK||PICK_DLV_REMARK', index: 'PickDlvRemark', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'DebitDate', title: 'DEBIT DATE||DEBIT_DATE', index: 'DebitDate', sorttype: 'string',  width: 100, align: 'left', formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                            { name: 'DebitNo', title: 'DEBIT NO||DEBIT_NO', index: 'DebitNo', sorttype: 'string', align: 'left', width: 120, hidden: false },
                            { name: 'Cur', title: 'Currency||CUR', index: 'Cur', sorttype: 'string', align: 'left', width: 100, hidden: false }
                           ];

            var qAmtSql = "  SELECT SUM(ISNULL(SMBID.Qamt,0)) FROM SMBID WHERE SMBID.ORD_NO = S.ORD_NO AND SMBID.AR_AP='" + ArAp + "'";
            var qCurSql = "  SELECT TOP 1 QCUR FROM SMBID WHERE SMBID.ORD_NO = S.ORD_NO AND SMBID.AR_AP='" + ArAp + "' AND QCUR IS NOT NULL";
            var transTypeCols = result.chgTypeStr.split(";");
            var transTypeColsN = result.chgTypeColsStr.split(";");
            var virtualCol = ",'"+ArAp+"' AS AR_AP, ISNULL(ATD,ETD) AS DEBIT_DATE, '' AS DEBIT_NO, (REMARK+REMARK1) AS PICK_DLV_REMARK, (SELECT HQID FROM SMPTY S WHERE S.PARTY_NO=CUST_CD) AS HQID,(CASE  WHEN (TK_CD IS NULL OR TK_CD = '') THEN (SELECT HQID FROM SMPTY S WHERE S.PARTY_NO=CT_ID) ELSE   (SELECT HQID FROM SMPTY S WHERE S.PARTY_NO=TK_CD) END) AS VENDER_HQID, (CASE  WHEN TK_CD IS NULL OR TK_CD = '' THEN CT_ID WHEN TK_CD IS NOT NULL THEN TK_CD END) AS VENDER_CD, ISNULL(TK_NM, CT_NM) AS VENDER_NM";
            var colNames = ["rn", "Order No", "應收/應付", "PICKUP DATE", "REFERENCE NO1", "REFERENCE NO2", "Truck No", "CUSTOMER", "CUSTOMER NAME","VENDER","VENDER CODE","VENDER NAME","DEST", "ORG", "GW", "CTNS", "REMARK", "DEBIT DATE", "DEBIT NO", "DEBIT CURRENCY"];//grid.jqGrid("getGridParam", "colNames");
            var i=0;
            $.each(transTypeCols, function (index, val) {
                var thisChgCd = val.split("-")[0];
                colModel.push({name: 'Cur'+index, title:'Currency', index: 'Cur'+index, sorttype: 'string', width: 70, hidden: false});
                colNames.push("CUR");

                colModel.push({ name: transTypeColsN[index], title: val, index: transTypeColsN[index], sorttype: 'string', width: 70, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false });
                colNames.push(val);
                //virtualCol += " ,'' AS '" + thisChgCd+"'";
                virtualCol += " ,("+qCurSql+" AND CHG_CD='"+thisChgCd+"') AS 'CUR"+index+"', (" + qAmtSql + " AND CHG_CD = '" + thisChgCd + "' ) AS '" + thisChgCd + "'";
                i = index;
            });
            /*colModel.push({name: 'Cur'+(i+1), title:'Currency', index: 'Cur'+(i+1), sorttype: 'string', width: 70, hidden: false});
            colNames.push("CUR");

            colModel.push({ name: "TotalSales", title: "Total Sales", index: "TotalSales", sorttype: 'string', width: 70, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false });
            colNames.push("Total Sales");*/

            virtualCol += " ,'' AS 'CUR"+(i+1)+"', '' AS 'TOTAL_SALES'";

            console.log(colModel);
            console.log(colNames);
            
            var caption = "Statement Of Account \r\n Region : "+result.stb+"  User : "+result.user+"   Date : "+result.createDate;
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