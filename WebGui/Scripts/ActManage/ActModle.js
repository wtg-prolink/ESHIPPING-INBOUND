function _getColModel(transType, haveCur, hasTT, isOld) {
    function _getLang(id, caption) {
        try {
            return GetLangCaption(id, caption);
        }
        catch (e) { }
        return caption || id;
    }

    var colModel = [
        { name: 'rn', title: 'rn', index: 'rn', sorttype: 'string', width: 100, align: 'left', hidden: false },
        { name: 'Location', title: _getLang('L_ActModle_Script_6', 'LOCATION||LOCATION'), index: 'Location', sorttype: 'string', align: 'left', width: 120, hidden: false },
        { name: 'ShipmentId', title: _getLang('L_ActModle_Script_5', 'Shipment ID||SHIPMENT_ID'), index: 'ShipmentId', sorttype: 'string', align: 'left', width: 120, hidden: false },
        { name: 'CombineInfo', title: _getLang('L_ActModle_Script_4', 'DN INFO||COMBINE_INFO'), index: 'COMBINE_INFO', sorttype: 'string', align: 'left', width: 120, hidden: false },
        { name: 'HouseNo', title: _getLang('L_ActModle_Script_10', 'BL NO||HOUSE_NO'), index: 'HOUSE_NO', sorttype: 'string', width: 120, align: 'left', hidden: false },
        { name: 'DebitDate', title: _getLang('L_ActModle_Script_0', 'DEBIT DATE||DEBIT_DATE'), index: 'DebitDate', sorttype: 'string', width: 100, align: 'left', formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
        { name: 'DebitNo', title: _getLang('L_ActModle_Script_1', 'DEBIT NO||DEBIT_NO'), index: 'DebitNo', sorttype: 'string', align: 'left', width: 120, hidden: false }
    ];

    switch (transType) {
        case "F":
            if (!isOld) {
                colModel = [
                    { name: 'rn', title: 'rn', index: 'rn', sorttype: 'string', width: 100, align: 'left', hidden: false },
                    { name: 'Location', title: _getLang('L_ActModle_Script_6', 'LOCATION||LOCATION'), index: 'Location', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'ShipmentId', title: _getLang('L_ActModle_Script_5', 'Shipment ID||SHIPMENT_ID'), index: 'ShipmentId', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'CombineInfo1', title: _getLang('L_ActModle_Script_8', 'DN||COMBINE_INFO1'), index: 'CombineInfo1', sorttype: 'string', align: 'left', width: 160, hidden: false },
                    { name: 'ExCntrNos', title: _getLang('L_ActModle_Script_12', 'Container number ||EX_CNTR_NOS'), index: 'ExCntrNos', sorttype: 'string', align: 'left', width: 160, hidden: false },
                    { name: 'EdeclNo', title: _getLang('L_ActModle_Script_13', 'Custom clerance number ||EDECL_NO'), index: 'EdeclNo', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'MasterNo', title: _getLang('L_ActModle_Script_9', 'MBL NO||MASTER_NO'), index: 'MasterNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'HouseNo', title: _getLang('L_ActModle_Script_11', 'House NO||HOUSE_NO'), index: 'HOUSE_NO', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'SoNo', title: _getLang('L_ActModle_Script_14', 'BOOKING Bill of lading No. ||SO_NO'), index: 'SoNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'FcNm', title: _getLang('L_ActModle_Script_15', 'FC Financial customers  ||FC_NM'), index: 'FcNm', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'IncotermCd', title: 'DLV TERM||INCOTERM_CD', index: 'IncotermCd', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'DestName', title: _getLang('L_ActModle_Script_25', 'Destination(Port of destination) ||DEST_NAME'), index: 'DestName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'PolName', title: 'POL||POL_NAME', index: 'PolName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'Carrier', title: 'CARRIER||CARRIER', index: 'Carrier', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'Pcnt20', title: '20GP||PCNT20', index: 'Pcnt20', sorttype: 'string', width: 60, align: 'left', hidden: false },
                    { name: 'Pcnt40', title: '40GP||PCNT40', index: 'Pcnt40', sorttype: 'string', width: 60, align: 'left', hidden: false },
                    { name: 'Pcnt40hq', title: 'HQ||PCNT40HQ', index: 'Pcnt40hq', sorttype: 'string', width: 60, align: 'left', hidden: false },
                    { name: 'Lcl', title: 'LCL||LCL', index: 'Lcl', sorttype: 'string', width: 60, align: 'left', hidden: false },

                    { name: 'CallDate', title: 'Call Date||CALL_DATE', index: 'CallDate', sorttype: 'string', width: 140, align: 'left', hidden: false },
                    { name: 'InDate', title: 'Gate In Date||IN_DATE', index: 'InDate', sorttype: 'string', width: 140, align: 'left', hidden: false },
                    { name: 'OutDate', title: 'Gate Out Date||OUT_DATE', index: 'OutDate', sorttype: 'string', width: 140, align: 'left', hidden: false },

                    { name: 'CrNm', title: 'Truck Name||CR_NM', index: 'CrNm', sorttype: 'string', width: 100, align: 'left', hidden: false },
                    { name: 'LspNm', title: 'LSP Name||LSP_NM', index: 'LspNm', sorttype: 'string', width: 100, align: 'left', hidden: false },


                    { name: 'Cbm', title: _getLang('L_ActModle_Script_16', 'TPV Volume ||CBM'), index: 'Cbm', sorttype: 'string', width: 60, align: 'left', hidden: false },
                    { name: 'Tcbm', title: _getLang('L_ActModle_Script_17', 'Total volume ||TCBM'), index: 'Tcbm', sorttype: 'string', width: 60, align: 'left', hidden: false },
                    { name: 'Etd', title: _getLang('L_ActModle_Script_18', 'Sailing ETD||ETD'), index: 'Etd', sorttype: 'string', width: 120, align: 'left', hidden: false, formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                    { name: 'Vv', title: _getLang('L_ActModle_Script_19', 'Vessel name ||VV'), index: 'Vv', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'SalesWin', title: _getLang('L_ActModle_Script_20', 'Sales Window ||SALES_WIN'), index: 'SalesWin', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'BlWin', title: _getLang('L_ActModle_Script_21', 'Booking Window ||BL_WIN'), index: 'BlWin', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'Remark', title: _getLang('L_ActModle_Script_22', 'Remark ||REMARK'), index: 'Remark', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'DebitDate', title: _getLang('L_ActModle_Script_0', 'DEBIT DATE||DEBIT_DATE'), index: 'DebitDate', sorttype: 'string', width: 100, align: 'left', formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                    { name: 'DebitNo', title: _getLang('L_ActModle_Script_1', 'DEBIT NO||DEBIT_NO'), index: 'DebitNo', sorttype: 'string', align: 'left', width: 120, hidden: false }
                ];
            }
            else {
                colModel = [
                    { name: 'rn', title: 'rn', index: 'rn', sorttype: 'string', width: 100, align: 'left', hidden: false },
                    { name: 'Location', title: _getLang('L_ActModle_Script_6', 'LOCATION||LOCATION'), index: 'Location', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'ShipmentId', title: _getLang('L_ActModle_Script_5', 'Shipment ID||SHIPMENT_ID'), index: 'ShipmentId', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'CombineInfo1', title: _getLang('L_ActModle_Script_8', 'DN||COMBINE_INFO1'), index: 'CombineInfo1', sorttype: 'string', align: 'left', width: 160, hidden: false },
                    { name: 'ExCntrNos', title: _getLang('L_ActModle_Script_12', 'Container number ||EX_CNTR_NOS'), index: 'ExCntrNos', sorttype: 'string', align: 'left', width: 160, hidden: false },
                    { name: 'EdeclNo', title: _getLang('L_ActModle_Script_13', 'Custom clerance number ||EDECL_NO'), index: 'EdeclNo', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'MasterNo', title: _getLang('L_ActModle_Script_9', 'MBL NO||MASTER_NO'), index: 'MasterNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'HouseNo', title: _getLang('L_ActModle_Script_11', 'House NO||HOUSE_NO'), index: 'HOUSE_NO', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'SoNo', title: _getLang('L_ActModle_Script_14', 'BOOKING Bill of lading No. ||SO_NO'), index: 'SoNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'FcNm', title: _getLang('L_ActModle_Script_15', 'FC Financial customers  ||FC_NM'), index: 'FcNm', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'IncotermCd', title: 'DLV TERM||INCOTERM_CD', index: 'IncotermCd', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'DestName', title: _getLang('L_ActModle_Script_25', 'Destination(Port of destination) ||DEST_NAME'), index: 'DestName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'PolName', title: 'POL||POL_NAME', index: 'PolName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'Carrier', title: 'CARRIER||CARRIER', index: 'Carrier', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'Pcnt20', title: '20GP||PCNT20', index: 'Pcnt20', sorttype: 'string', width: 60, align: 'left', hidden: false },
                    { name: 'Pcnt40', title: '40GP||PCNT40', index: 'Pcnt40', sorttype: 'string', width: 60, align: 'left', hidden: false },
                    { name: 'Pcnt40hq', title: 'HQ||PCNT40HQ', index: 'Pcnt40hq', sorttype: 'string', width: 60, align: 'left', hidden: false },


                    { name: 'Cbm', title: _getLang('L_ActModle_Script_16', 'TPV Volume ||CBM'), index: 'Cbm', sorttype: 'string', width: 60, align: 'left', hidden: false },
                    { name: 'Tcbm', title: _getLang('L_ActModle_Script_17', 'Total volume ||TCBM'), index: 'Tcbm', sorttype: 'string', width: 60, align: 'left', hidden: false },
                    { name: 'Etd', title: _getLang('L_ActModle_Script_18', 'Sailing ETD||ETD'), index: 'Etd', sorttype: 'string', width: 120, align: 'left', hidden: false, formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                    { name: 'Vv', title: _getLang('L_ActModle_Script_19', 'Vessel name ||VV'), index: 'Vv', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'SalesWin', title: _getLang('L_ActModle_Script_20', 'Sales Window ||SALES_WIN'), index: 'SalesWin', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'BlWin', title: _getLang('L_ActModle_Script_21', 'Booking Window ||BL_WIN'), index: 'BlWin', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'Remark', title: _getLang('L_ActModle_Script_22', 'Remark ||REMARK'), index: 'Remark', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'DebitDate', title: _getLang('L_ActModle_Script_0', 'DEBIT DATE||DEBIT_DATE'), index: 'DebitDate', sorttype: 'string', width: 100, align: 'left', formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                    { name: 'DebitNo', title: _getLang('L_ActModle_Script_1', 'DEBIT NO||DEBIT_NO'), index: 'DebitNo', sorttype: 'string', align: 'left', width: 120, hidden: false }
                ];
            }
            break;
        case "L":
            if (!isOld) {
                colModel = [
                    { name: 'rn', title: 'rn', index: 'rn', sorttype: 'string', width: 100, align: 'left', hidden: false },
                    { name: 'Location', title: _getLang('L_ActModle_Script_6', 'LOCATION||LOCATION'), index: 'Location', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'ShipmentId', title: _getLang('L_ActModle_Script_5', 'Shipment ID||SHIPMENT_ID'), index: 'ShipmentId', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'CombineInfo1', title: _getLang('L_ActModle_Script_8', 'DN||COMBINE_INFO1'), index: 'CombineInfo1', sorttype: 'string', align: 'left', width: 160, hidden: false },
                    { name: 'ExCntrNos', title: _getLang('L_ActModle_Script_12', 'Container number ||EX_CNTR_NOS'), index: 'ExCntrNos', sorttype: 'string', align: 'left', width: 160, hidden: false },
                    { name: 'EdeclNo', title: _getLang('L_ActModle_Script_13', 'Custom clerance number ||EDECL_NO'), index: 'EdeclNo', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'MasterNo', title: _getLang('L_ActModle_Script_9', 'MBL NO||MASTER_NO'), index: 'MasterNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'HouseNo', title: _getLang('L_ActModle_Script_11', 'House NO||HOUSE_NO'), index: 'HOUSE_NO', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'SoNo', title: _getLang('L_ActModle_Script_14', 'BOOKING Bill of lading No. ||SO_NO'), index: 'SoNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'FcNm', title: _getLang('L_ActModle_Script_15', 'FC Financial customers  ||FC_NM'), index: 'FcNm', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'IncotermCd', title: 'DLV TERM||INCOTERM_CD', index: 'IncotermCd', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'DestName', title: _getLang('L_ActModle_Script_25', 'Destination(Port of destination) ||DEST_NAME'), index: 'DestName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'PolName', title: 'POL||POL_NAME', index: 'PolName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'Tcbm', title: _getLang('L_ActModle_Script_23', 'TPV Volume ||TCBM'), index: 'Tcbm', sorttype: 'string', width: 60, align: 'left', hidden: false },
                    { name: 'Cbm', title: _getLang('L_ActModle_Script_24', 'Total volume ||CBM'), index: 'Cbm', sorttype: 'string', width: 60, align: 'left', hidden: false },

                    { name: 'CallDate', title: 'Call Date||CALL_DATE', index: 'CallDate', sorttype: 'string', width: 140, align: 'left', hidden: false },
                    { name: 'InDate', title: 'Gate In Date||IN_DATE', index: 'InDate', sorttype: 'string', width: 140, align: 'left', hidden: false },
                    { name: 'OutDate', title: 'Gate Out Date||OUT_DATE', index: 'OutDate', sorttype: 'string', width: 140, align: 'left', hidden: false },

                    { name: 'CrNm', title: 'Truck Name||CR_NM', index: 'CrNm', sorttype: 'string', width: 100, align: 'left', hidden: false },
                    { name: 'LspNm', title: 'LSP Name||LSP_NM', index: 'LspNm', sorttype: 'string', width: 100, align: 'left', hidden: false },
                    { name: 'Lcl', title: 'LCL||LCL', index: 'Lcl', sorttype: 'string', width: 60, align: 'left', hidden: false },

                    { name: 'Etd', title: _getLang('L_ActModle_Script_18', 'Sailing ETD||ETD'), index: 'Etd', sorttype: 'string', width: 120, align: 'left', hidden: false, formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                    { name: 'Vv', title: _getLang('L_ActModle_Script_19', 'Vessel name ||VV'), index: 'Vv', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'SalesWin', title: _getLang('L_ActModle_Script_20', 'Sales Window ||SALES_WIN'), index: 'SalesWin', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'BlWin', title: _getLang('L_ActModle_Script_21', 'Booking Window ||BL_WIN'), index: 'BlWin', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'Remark', title: _getLang('L_ActModle_Script_22', 'Remark ||REMARK'), index: 'Remark', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'DebitDate', title: _getLang('L_ActModle_Script_0', 'DEBIT DATE||DEBIT_DATE'), index: 'DebitDate', sorttype: 'string', width: 100, align: 'left', formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                    { name: 'DebitNo', title: _getLang('L_ActModle_Script_1', 'DEBIT NO||DEBIT_NO'), index: 'DebitNo', sorttype: 'string', align: 'left', width: 120, hidden: false }
                ];
            }
            else {
                colModel = [
                    { name: 'rn', title: 'rn', index: 'rn', sorttype: 'string', width: 100, align: 'left', hidden: false },
                    { name: 'Location', title: _getLang('L_ActModle_Script_6', 'LOCATION||LOCATION'), index: 'Location', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'ShipmentId', title: _getLang('L_ActModle_Script_5', 'Shipment ID||SHIPMENT_ID'), index: 'ShipmentId', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'CombineInfo1', title: _getLang('L_ActModle_Script_8', 'DN||COMBINE_INFO1'), index: 'CombineInfo1', sorttype: 'string', align: 'left', width: 160, hidden: false },
                    { name: 'ExCntrNos', title: _getLang('L_ActModle_Script_12', 'Container number ||EX_CNTR_NOS'), index: 'ExCntrNos', sorttype: 'string', align: 'left', width: 160, hidden: false },
                    { name: 'EdeclNo', title: _getLang('L_ActModle_Script_13', 'Custom clerance number ||EDECL_NO'), index: 'EdeclNo', sorttype: 'string', align: 'left', width: 120, hidden: false },
                    { name: 'MasterNo', title: _getLang('L_ActModle_Script_9', 'MBL NO||MASTER_NO'), index: 'MasterNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'HouseNo', title: _getLang('L_ActModle_Script_11', 'House NO||HOUSE_NO'), index: 'HOUSE_NO', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'SoNo', title: _getLang('L_ActModle_Script_14', 'BOOKING Bill of lading No. ||SO_NO'), index: 'SoNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'FcNm', title: _getLang('L_ActModle_Script_15', 'FC Financial customers  ||FC_NM'), index: 'FcNm', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'IncotermCd', title: 'DLV TERM||INCOTERM_CD', index: 'IncotermCd', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'DestName', title: _getLang('L_ActModle_Script_25', 'Destination(Port of destination) ||DEST_NAME'), index: 'DestName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'PolName', title: 'POL||POL_NAME', index: 'PolName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'Tcbm', title: _getLang('L_ActModle_Script_23', 'TPV Volume ||TCBM'), index: 'Tcbm', sorttype: 'string', width: 60, align: 'left', hidden: false },
                    { name: 'Cbm', title: _getLang('L_ActModle_Script_24', 'Total volume ||CBM'), index: 'Cbm', sorttype: 'string', width: 60, align: 'left', hidden: false },

                    { name: 'Etd', title: _getLang('L_ActModle_Script_18', 'Sailing ETD||ETD'), index: 'Etd', sorttype: 'string', width: 120, align: 'left', hidden: false, formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                    { name: 'Vv', title: _getLang('L_ActModle_Script_19', 'Vessel name ||VV'), index: 'Vv', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'SalesWin', title: _getLang('L_ActModle_Script_20', 'Sales Window ||SALES_WIN'), index: 'SalesWin', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'BlWin', title: _getLang('L_ActModle_Script_21', 'Booking Window ||BL_WIN'), index: 'BlWin', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'Remark', title: _getLang('L_ActModle_Script_22', 'Remark ||REMARK'), index: 'Remark', sorttype: 'string', width: 120, align: 'left', hidden: false },
                    { name: 'DebitDate', title: _getLang('L_ActModle_Script_0', 'DEBIT DATE||DEBIT_DATE'), index: 'DebitDate', sorttype: 'string', width: 100, align: 'left', formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                    { name: 'DebitNo', title: _getLang('L_ActModle_Script_1', 'DEBIT NO||DEBIT_NO'), index: 'DebitNo', sorttype: 'string', align: 'left', width: 120, hidden: false }
                ];
            }
            break;
        case "A":
            colModel = [
                { name: 'rn', title: 'rn', index: 'rn', sorttype: 'string', width: 100, align: 'left', hidden: false },
                { name: 'Location', title: _getLang('L_ActModle_Script_6', 'LOCATION||LOCATION'), index: 'Location', sorttype: 'string', align: 'left', width: 120, hidden: false },
                { name: 'ShipmentId', title: _getLang('L_ActModle_Script_5', 'Shipment ID||SHIPMENT_ID'), index: 'ShipmentId', sorttype: 'string', align: 'left', width: 120, hidden: false },
                { name: 'CombineInfo', title: _getLang('L_ActModle_Script_4', 'DN INFO||COMBINE_INFO'), index: 'CombineInfo', sorttype: 'string', align: 'left', width: 160, hidden: false },
                { name: 'Etd', title: _getLang('L_ActModle_Script_18', 'Sailing ETD||ETD'), index: 'Etd', sorttype: 'string', width: 120, align: 'left', hidden: false, formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                { name: 'EdeclNo', title: _getLang('L_ActModle_Script_13', 'Custom clerance number ||EDECL_NO'), index: 'EdeclNo', sorttype: 'string', align: 'left', width: 120, hidden: false },
                { name: 'MasterNo', title: 'MAWB||MASTER_NO', index: 'MasterNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'HouseNo', title: 'HAWB||HOUSE_NO', index: 'HouseNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'FcNm', title: _getLang('L_ActModle_Script_15', 'FC Financial customers  ||FC_NM'), index: 'FcNm', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'PolName', title: 'POL||POL_NAME', index: 'PolName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'DestName', title: _getLang('L_ActModle_Script_25', 'Destination(Port of destination) ||DEST_NAME'), index: 'DestName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'IncotermCd', title: 'DLV TERM||INCOTERM_CD', index: 'IncotermCd', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'Qty', title: _getLang('L_ActModle_Script_26', 'Quantity ||QTY'), index: 'Qty', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'Cw', title: _getLang('L_ActModle_Script_27', 'Chargeable Weight ||CW'), index: 'Cw', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'DebitDate', title: _getLang('L_ActModle_Script_0', 'DEBIT DATE||DEBIT_DATE'), index: 'DebitDate', sorttype: 'string', width: 100, align: 'left', formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                { name: 'DebitNo', title: _getLang('L_ActModle_Script_1', 'DEBIT NO||DEBIT_NO'), index: 'DebitNo', sorttype: 'string', align: 'left', width: 120, hidden: false }
            ];
            break;
        case "E":
            colModel = [
                { name: 'rn', title: 'rn', index: 'rn', sorttype: 'string', width: 100, align: 'left', hidden: false },
                { name: 'Location', title: _getLang('L_ActModle_Script_6', 'LOCATION||LOCATION'), index: 'Location', sorttype: 'string', align: 'left', width: 120, hidden: false },
                { name: 'ShipmentId', title: _getLang('L_ActModle_Script_5', 'Shipment ID||SHIPMENT_ID'), index: 'ShipmentId', sorttype: 'string', align: 'left', width: 120, hidden: false },
                { name: 'CombineInfo', title: _getLang('L_ActModle_Script_4', 'DN INFO||COMBINE_INFO'), index: 'CombineInfo', sorttype: 'string', align: 'left', width: 160, hidden: false },
                { name: 'LspNm', title: _getLang('L_ActModle_Script_28', 'Express company (logistics Provider) ||LSP_NM'), index: 'LspNm', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'HouseNo', title: _getLang('L_ActModle_Script_29', 'Express tracking number ||HOUSE_NO'), index: 'HouseNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'Etd', title: 'ETD||ETD', index: 'Etd', sorttype: 'string', width: 120, align: 'left', hidden: false, formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                { name: 'FcNm', title: _getLang('L_ActModle_Script_15', 'FC Financial customers  ||FC_NM'), index: 'FcNm', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'PolName', title: 'region||POL_NAME', index: 'PolName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'ExPregion', title: _getLang('L_ActModle_Script_30', 'Booking destination country ||EX_PREGION'), index: 'ExPregion', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'PpodName', title: _getLang('L_ActModle_Script_31', 'Booking destination ||PPOD_NAME'), index: 'PpodName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'IncotermCd', title: 'DLV TERM||INCOTERM_CD', index: 'IncotermCd', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'ExCostCenters', title: _getLang('L_ActModle_Script_32', 'Cost center ||EX_COST_CENTERS'), index: 'ExCostCenters', sorttype: 'string', width: 220, align: 'left', hidden: false },
                { name: 'ExCostCenterdescps', title: _getLang('L_ActModle_Script_33', 'Cost belongs to ||EX_COST_CENTERDESCPS'), index: 'ExCostCenterdescps', sorttype: 'string', width: 220, align: 'left', hidden: false },

                { name: 'Gw', title: _getLang('L_ActModle_Script_34', 'Weight (KG) ||GW'), index: 'Gw', sorttype: 'string', width: 60, align: 'left', hidden: false },
                { name: 'Cbm', title: _getLang('L_ActModle_Script_35', 'Volume (M3) ||CBM'), index: 'Cbm', sorttype: 'string', width: 60, align: 'left', hidden: false },
                { name: 'Cw', title: _getLang('L_ActModle_Script_27', 'Chargeable Weight ||CW'), index: 'Cw', sorttype: 'string', width: 60, align: 'left', hidden: false },
                { name: 'DebitDate', title: _getLang('L_ActModle_Script_0', 'DEBIT DATE||DEBIT_DATE'), index: 'DebitDate', sorttype: 'string', width: 100, align: 'left', formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                { name: 'DebitNo', title: _getLang('L_ActModle_Script_1', 'DEBIT NO||DEBIT_NO'), index: 'DebitNo', sorttype: 'string', align: 'left', width: 120, hidden: false }
            ];
            break;
        case "R":
            colModel = [
                { name: 'rn', title: 'rn', index: 'rn', sorttype: 'string', width: 100, align: 'left', hidden: false },
                { name: 'Location', title: _getLang('L_ActModle_Script_6', 'LOCATION||LOCATION'), index: 'Location', sorttype: 'string', align: 'left', width: 120, hidden: false },
                { name: 'ShipmentId', title: _getLang('L_ActModle_Script_5', 'Shipment ID||SHIPMENT_ID'), index: 'ShipmentId', sorttype: 'string', align: 'left', width: 120, hidden: false },
                { name: 'CombineInfo', title: _getLang('L_ActModle_Script_36', 'DN+container number+container size||COMBINE_INFO'), index: 'CombineInfo', sorttype: 'string', align: 'left', width: 160, hidden: false },
                { name: 'EdeclNo', title: _getLang('L_ActModle_Script_13', 'Custom clerance number ||EDECL_NO'), index: 'EdeclNo', sorttype: 'string', align: 'left', width: 120, hidden: false },
                { name: 'MasterNo', title: _getLang('L_ActModle_Script_9', 'MBL NO||MASTER_NO'), index: 'MasterNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'HouseNo', title: _getLang('L_ActModle_Script_37', 'BOOKING Bill of lading No. ||HOUSE_NO'), index: 'HouseNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'FcNm', title: _getLang('L_ActModle_Script_15', 'FC Financial customers  ||FC_NM'), index: 'FcNm', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'IncotermCd', title: 'DLV TERM||INCOTERM_CD', index: 'IncotermCd', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'PodName', title: 'POD||POD_NAME', index: 'PodName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'PolName', title: 'POL||POL_NAME', index: 'PolName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'Carrier', title: 'CARRIER||CARRIER', index: 'Carrier', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'Pcnt20', title: '20GP||PCNT20', index: 'Pcnt20', sorttype: 'string', width: 60, align: 'left', hidden: false },
                { name: 'Pcnt40', title: '40GP||PCNT40', index: 'Pcnt40', sorttype: 'string', width: 60, align: 'left', hidden: false },
                { name: 'Pcnt40hq', title: 'HQ||PCNT40HQ', index: 'Pcnt40hq', sorttype: 'string', width: 60, align: 'left', hidden: false },
                { name: 'Tcbm', title: _getLang('L_ActModle_Script_23', 'TPV Volume ||TCBM'), index: 'Tcbm', sorttype: 'string', width: 60, align: 'left', hidden: false },
                { name: 'Cbm', title: _getLang('L_ActModle_Script_24', 'Total volume ||CBM'), index: 'Cbm', sorttype: 'string', width: 60, align: 'left', hidden: false },
                { name: 'Etd', title: _getLang('L_ActModle_Script_18', 'Sailing ETD||ETD'), index: 'Etd', sorttype: 'string', width: 120, align: 'left', hidden: false, formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                { name: 'Vv', title: _getLang('L_ActModle_Script_38', 'Flight ||VV'), index: 'Vv', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'SalesWin', title: _getLang('L_ActModle_Script_20', 'Sales Window ||SALES_WIN'), index: 'SalesWin', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'BlWin', title: _getLang('L_ActModle_Script_21', 'Booking Window ||BL_WIN'), index: 'BlWin', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'Remark', title: _getLang('L_ActModle_Script_22', 'Remark ||REMARK'), index: 'Remark', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'DebitDate', title: _getLang('L_ActModle_Script_0', 'DEBIT DATE||DEBIT_DATE'), index: 'DebitDate', sorttype: 'string', width: 100, align: 'left', formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                { name: 'DebitNo', title: _getLang('L_ActModle_Script_1', 'DEBIT NO||DEBIT_NO'), index: 'DebitNo', sorttype: 'string', align: 'left', width: 120, hidden: false }
            ];
            break;
        case "D":
        case "T":
        case "TT":
            colModel = [
                { name: 'rn', title: 'rn', index: 'rn', sorttype: 'string', width: 100, align: 'left', hidden: false },
                { name: 'Location', title: _getLang('L_ActModle_Script_6', 'LOCATION||LOCATION'), index: 'Location', sorttype: 'string', align: 'left', width: 120, hidden: false },
                { name: 'DnCmp', title: _getLang('L_ActModle_Script_39', 'Company code ||DN_CMP'), index: 'DnCmp', sorttype: 'string', align: 'left', width: 120, hidden: false },
                { name: 'ShipmentId', title: _getLang('L_ActModle_Script_5', 'Shipment ID||SHIPMENT_ID'), index: 'ShipmentId', sorttype: 'string', align: 'left', width: 120, hidden: false },
                { name: 'CombineInfo', title: _getLang('L_ActModle_Script_4', 'DN INFO||COMBINE_INFO'), index: 'CombineInfo', sorttype: 'string', align: 'left', width: 160, hidden: false },
                { name: 'DnType', title: _getLang('L_ActModle_Script_40', 'Order type ||DN_TYPE'), index: 'DnType', sorttype: 'string', align: 'left', width: 120, hidden: false },
                { name: 'RgCd', title: _getLang('L_ActModle_Script_41', 'DN PARTY RG Code ||RG_CD'), index: 'RgCd', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'RgNm', title: _getLang('L_ActModle_Script_42', 'RG Name ||RG_NM'), index: 'RgNm', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'FcNm', title: _getLang('L_ActModle_Script_15', 'FC Financial customers  ||FC_NM'), index: 'FcNm', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'PolName', title: _getLang('L_ActModle_Script_43', 'Place of Origin ||POL_CD'), index: 'PolName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'PodName', title: _getLang('L_ActModle_Script_44', 'Destination ||POD_NAME'), index: 'PodName', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'Etd', title: _getLang('L_ActModle_Script_45', 'Delivery date (Leaving factory date) ||ETD'), index: 'Etd', sorttype: 'string', width: 120, align: 'left', hidden: false, formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                { name: 'DnNo', title: _getLang('L_ActModle_Script_2', 'DN NO||DN_NO'), index: 'DnNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'IpartNo', title: _getLang('L_ActModle_Script_46', 'Model / part number ||IPART_NO'), index: 'IpartNo', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'DnQty', title: _getLang('L_ActModle_Script_47', 'Chargeable quantity ||DN_QTY'), index: 'DnQty', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'DnQtyu', title: _getLang('L_ActModle_Script_48', 'Unit ||DN_QTYU'), index: 'DnQtyu', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'CntrStdQty', title: _getLang('L_ActModle_Script_49', 'Standard Loading Q\'tys||CNTR_STD_QTY'), index: 'CntrStdQty', sorttype: 'string', width: 120, align: 'left', hidden: false },


                { name: 'SalesWin', title: _getLang('L_ActModle_Script_50', 'Sales Window ||SALES_WIN'), index: 'SalesWin', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'BlWin', title: _getLang('L_ActModle_Script_51', 'LST Window ||BL_WIN'), index: 'BlWin', sorttype: 'string', width: 120, align: 'left', hidden: false },

                { name: 'Carrier', title: _getLang('L_ActModle_Script_3', 'CARRIER||CARRIER'), index: 'Carrier', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'CarType', title: _getLang('L_ActModle_Script_52', 'Truck type ||Truck_TYPE'), index: 'CarType', sorttype: 'string', width: 60, align: 'left', hidden: false },
                { name: 'CarQty', title: _getLang('L_ActModle_Script_53', 'Truck Quantity ||CAR_QTY'), index: 'CarQty', sorttype: 'string', width: 60, align: 'left', hidden: false },

                { name: 'TrackWay', title: _getLang('L_ActModle_Script_54', 'Delivery type ||Delivery_WAY'), index: 'TrackWay', sorttype: 'string', width: 60, align: 'left', hidden: false },


                { name: 'EdeclNo', title: _getLang('L_ActModle_Script_13', 'Custom clerance number ||EDECL_NO'), index: 'EdeclNo', sorttype: 'string', align: 'left', width: 120, hidden: false },
                { name: 'Amount1', title: _getLang('L_ActModle_Script_55', 'Cargo value($)||AMOUNT1)'), index: 'Amount1', sorttype: 'string', width: 120, align: 'left', hidden: false },
                { name: 'Cbm', title: _getLang('L_ActModle_Script_56', 'CargoVolume ||CBM'), index: 'Cbm', sorttype: 'string', width: 60, align: 'left', hidden: false },
                { name: 'Gw', title: _getLang('L_ActModle_Script_57', 'Cargo weight ||GW'), index: 'Gw', sorttype: 'string', width: 60, align: 'left', hidden: false },
                { name: 'DebitDate', title: _getLang('L_ActModle_Script_0', 'DEBIT DATE||DEBIT_DATE'), index: 'DebitDate', sorttype: 'string', width: 100, align: 'left', formatter: "date", formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: "" } },
                { name: 'DebitNo', title: _getLang('L_ActModle_Script_1', 'DEBIT NO||DEBIT_NO'), index: 'DebitNo', sorttype: 'string', align: 'left', width: 120, hidden: false }
            ];

            if (hasTT) {
                colModel.push({ name: "TtShipmentid", title: "By SHIPMENT ID", index: "TtShipmentid", sorttype: 'string', width: 100, align: 'right', hidden: false });
                colModel.push({ name: "TtDnno", title: "By DN", index: "TtDnno", sorttype: 'string', width: 100, align: 'right', hidden: false });
            }
            break;
    }
    if (haveCur !== false)
        colModel.push({ name: 'Cur', title: _getLang('L_ActModle_Script_7', 'Currency||CUR'), index: 'Cur', sorttype: 'string', align: 'left', width: 100, hidden: false });
    return colModel;
}
