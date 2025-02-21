function doDownloadExcel($cgrid, VenderCd) {
    var postData = {};
    if(typeof VenderCd !== "undefined")
    {
        postData = {"VenderCd": VenderCd};
    }
    
    $.ajax({
        async: true,
        url: rootPath + "IQTManage/GetTransTypeInfo",
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
                    { name: 'TranType', title: '运输类型||TRAN_TYPE', index: 'TruckType', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    //{ name: 'TruckType', title: '車輛類型||TRUCK_TYPE', index: 'TruckType', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    //{ name: 'ChgCd', title: '費用代碼||CHG_CD', index: 'ChgCd', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'PolNm', title: '启运地名称||POL_NM', index: 'PolNm', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'PolCd', title: '启运地代码||POL_CD', index: 'PolCd', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'PodNm', title: '目的地名称||POD_NM', index: 'PodNm', sorttype: 'string',  width: 100, align: 'left', hidden: false },
                    { name: 'PodCd', title: '目的地代碼||POD_CD', index: 'PodCd', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'State', title: '目的地省份||STATE', index: 'State', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'StateNm', title: '目的地省份代码||STATE_NM', index: 'StateNm', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'Tt', title: '承诺运输交期（小时）||TT', index: 'Tt', sorttype: 'int', align: 'right', formatter: 'integer', width: 100, hidden: false }
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
            var colNames = ["rn", "运输类型", "启运地名称", "启运地代码", "目的地名称","目的地代碼", "目的地省份", "目的地省份代码","承诺运输交期（小时）"];//grid.jqGrid("getGridParam", "colNames");


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
            
            var caption = "Quotation \r\n Location: "+result.stn+"  User : "+result.user+"   Date : "+result.createDate;
            var excelName = "Quotation";


            ExportDataToExcelByParam(url, colModel, colNames, caption, excelName, "", " U_ID IS NULL", "");
        }
    });

}