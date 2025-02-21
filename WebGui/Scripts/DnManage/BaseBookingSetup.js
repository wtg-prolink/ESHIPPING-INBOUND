/*存储公用的订舱明细页面*/


function returnPartyModel(_partygrid,_$partygrid) {//"SubGrid", $SubGrid
    function getpartyop(_partygrid,_$partygrid, name) {
        var _name = name;
        var city_op = getLookupOp(_partygrid,
            {
                url: rootPath + LookUpConfig.PartyNoUrl,
                config: LookUpConfig.PartyNoLookup,
                returnFn: function (returnObj, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');

                    setGridVal($grid, selRowId, 'PartyName', returnObj.PartyName, null);
                    setGridVal($grid, selRowId, 'PartyMail', returnObj.PartyMail, null);
                    setGridVal($grid, selRowId, 'PartAddr1', returnObj.PartAddr1, null);
                    setGridVal($grid, selRowId, 'PartAddr2', returnObj.PartAddr2, null);
                    setGridVal($grid, selRowId, 'PartAddr3', returnObj.PartAddr3, null);
                    setGridVal($grid, selRowId, 'PartyAttn', returnObj.PartyAttn, null);
                    setGridVal($grid, selRowId, 'State', returnObj.State, null);
                    setGridVal($grid, selRowId, 'Zip', returnObj.Zip, null);
                    setGridVal($grid, selRowId, 'PartyTel', returnObj.PartyTel, null);
                    setGridVal($grid, selRowId, 'DebitTo', returnObj.BillTo, null);
                    setGridVal($grid, selRowId, 'Cnty', returnObj.Cnty, null);
                    setGridVal($grid, selRowId, 'CntyNm', returnObj.CntyNm, null);
                    setGridVal($grid, selRowId, 'City', returnObj.City, null);
                    setGridVal($grid, selRowId, 'CityNm', returnObj.CityNm, null);

                    setGridVal($grid, selRowId, 'PartAddr4', returnObj.PartAddr4, null);
                    setGridVal($grid, selRowId, 'PartAddr5', returnObj.PartAddr5, null);
                    setGridVal($grid, selRowId, 'PartyName2', returnObj.PartyName2, null);
                    setGridVal($grid, selRowId, 'PartyName3', returnObj.PartyName3, null);
                    setGridVal($grid, selRowId, 'PartyName4', returnObj.PartyName4, null);
                    setGridVal($grid, selRowId, 'FaxNo', returnObj.PartyFax, null);
                    setGridVal($grid, selRowId, 'TaxNo', returnObj.TaxNo, null);
                    return returnObj.PartyNo;
                }
            },
            {
                baseConditionFunc: function () {
                    var selRowId = $("#" + _partygrid).jqGrid('getGridParam', 'selrow');
                    var PartyType = $("#" + _partygrid).jqGrid('getCell', selRowId, 'PartyType');
                    return "PARTY_TYPE LIKE '%" + PartyType + "%'";
                    //var selRowId = $("#SubGrid").jqGrid('getGridParam', 'selrow');
                    //var PartyType = $("#SubGrid").jqGrid('getCell', selRowId, 'PartyType');
                    //return "PARTY_TYPE LIKE '%" + PartyType + ";%'";
                }
            }, LookUpConfig.GetPartyNoAuto(groupId, _$partygrid,
            function ($grid, rd, elem, rowid) {
                var selRowId = rowid;
                setGridVal($grid, selRowId, 'PartyName', rd.PARTY_NAME, null);
                setGridVal($grid, selRowId, 'PartyMail', rd.PARTY_MAIL, null);
                setGridVal($grid, selRowId, 'PartAddr1', rd.PART_ADDR1, null);
                setGridVal($grid, selRowId, 'PartAddr2', rd.PART_ADDR2, null); 
                setGridVal($grid, selRowId, 'PartAddr3', rd.PART_ADDR3, null);
                setGridVal($grid, selRowId, 'PartyAttn', rd.PARTY_ATTN, null);
                setGridVal($grid, selRowId, 'State', rd.STATE, null);
                setGridVal($grid, selRowId, 'Zip', rd.ZIP, null);
                setGridVal($grid, selRowId, 'PartyTel', rd.PARTY_TEL, null);
                setGridVal($grid, selRowId, 'PartyNo', rd.PARTY_NO, 'lookup');
                setGridVal($grid, selRowId, 'DebitTo', rd.BILL_TO, null);
                setGridVal($grid, selRowId, 'Cnty', rd.CNTY, null);
                setGridVal($grid, selRowId, 'CntyNm', rd.CNTY_NM, null);
                setGridVal($grid, selRowId, 'City', rd.CITY, null);
                setGridVal($grid, selRowId, 'CityNm', rd.CITY_NM, null);

                setGridVal($grid, selRowId, 'PartAddr4', rd.PART_ADDR4, null);
                setGridVal($grid, selRowId, 'PartAddr5', rd.PART_ADDR5, null);
                setGridVal($grid, selRowId, 'PartyName2', rd.PARTY_NAME2, null);
                setGridVal($grid, selRowId, 'PartyName3', rd.PARTY_NAME3, null);
                setGridVal($grid, selRowId, 'PartyName4', rd.PARTY_NAME4, null);
                setGridVal($grid, selRowId, 'FaxNo', rd.PARTY_FAX, null);
                setGridVal($grid, selRowId, 'TaxNo', rd.TAX_NO, null);
            }, function ($grid, rd, elem, rowid) {
                var selRowId = rowid;
                setGridVal($grid, selRowId, 'PartyName', '', null);
                setGridVal($grid, selRowId, 'PartyMail', '', null);
                setGridVal($grid, selRowId, 'PartAddr1', '', null);
                setGridVal($grid, selRowId, 'PartAddr2', '', null);
                setGridVal($grid, selRowId, 'PartAddr3', '', null);
                setGridVal($grid, selRowId, 'PartyAttn', '', null);
                setGridVal($grid, selRowId, 'State', '', null);
                setGridVal($grid, selRowId, 'Zip', '', null);
                setGridVal($grid, selRowId, 'PartyTel', '', null);
                setGridVal($grid, selRowId, 'PartyNo', '', 'lookup');
                setGridVal($grid, selRowId, 'DebitTo', '', null);
                setGridVal($grid, selRowId, 'Cnty', '', null);
                setGridVal($grid, selRowId, 'CntyNm', '', null);
                setGridVal($grid, selRowId, 'City', '', null);
                setGridVal($grid, selRowId, 'CityNm', '', null);
                setGridVal($grid, selRowId, 'PartAddr4', '', null);
                setGridVal($grid, selRowId, 'PartAddr5', '', null);
                setGridVal($grid, selRowId, 'PartyName2', '', null);
                setGridVal($grid, selRowId, 'PartyName3', '', null);
                setGridVal($grid, selRowId, 'PartyName4', '', null);
                setGridVal($grid, selRowId, 'FaxNo', '', null);
                setGridVal($grid, selRowId, 'TaxNo', '', null);
            }));
        city_op.param = "";
        return city_op;
    }
    function getop(_partygrid, _$partygrid, name) {
        var _name = name;
        var city_op = getLookupOp(_partygrid,
            {
                url: rootPath + LookUpConfig.PartyTypeUrl,
                config: LookUpConfig.PartyTypeLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, 'TypeDescp', map.CdDescp, null);
                    setGridVal($grid, selRowId, 'OrderBy', map.OrderBy, null);
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "PT", _$partygrid, function ($grid, rd, elem, rowid) {
                //$("#PartyType").val(rd.CD);
                var selRowId = rowid;
                setGridVal($grid, selRowId, 'TypeDescp', rd.CD_DESCP, null);
                setGridVal($grid, selRowId, 'OrderBy', rd.ORDER_BY, null);
                setGridVal($grid, selRowId, 'PartyType', rd.CD, 'lookup');
                //$(elem).val(rd.CD);
            }, function ($grid, elem, rowid) {
                //$("#PartyType").val(rd.CD);
                var selRowId = rowid;
                setGridVal($grid, selRowId, 'TypeDescp', "", null);
                setGridVal($grid, selRowId, 'OrderBy', "", null);
                $(elem).val("");
            }), {
            param: "",
            baseConditionFunc: function () {
                return "";
            }
        });
        return city_op;
    }

    var partyColModel = [
	    { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
        { name: 'PartyType', title: '@Resources.Locale.L_DNDetailVeiw_PartyType', index: 'PartyType', editoptions: gridLookup(getop(_partygrid, _$partygrid, "TypeDescp")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'TypeDescp', title: '@Resources.Locale.L_DNDetailVeiw_TypeDescp', index: 'TypeDescp', sorttype: 'string', hidden: false, editable: true },
        { name: 'PartyNo', title: '@Resources.Locale.L_BSCSSetup_PartyNo', index: 'PartyNo', editoptions: gridLookup(getpartyop(_partygrid, _$partygrid, "PartyName")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PartyName', title: '@Resources.Locale.L_DNDetailVeiw_PartyName', index: 'PartyName', sorttype: 'string', width: 200, hidden: false, editoptions: { maxlength: 70 }, editable: true },
        { name: 'PartyName2', title: '@Resources.Locale.L_DNDetailVeiw_PartyName2', index: 'PartyName2', sorttype: 'string', width: 200, hidden: false, editoptions: { maxlength: 70 }, editable: true },
        { name: 'PartyName3', title: '@Resources.Locale.L_DNDetailVeiw_PartyName3', index: 'PartyName3', sorttype: 'string', width: 200, hidden: false, editoptions: { maxlength: 70 }, editable: true },
        { name: 'PartyName4', title: '@Resources.Locale.L_DNDetailVeiw_PartyName4', index: 'PartyName4', sorttype: 'string', width: 200, hidden: false, editoptions: { maxlength: 70 }, editable: true },
        { name: 'DebitTo', title: 'Debit To', index: 'DebitTo', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 10 }, editable: true },
        { name: 'PartyAttn', title: '@Resources.Locale.L_BSCSDataQuery_PartyAttn', index: 'PartyAttn', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 20 }, editable: true },
        { name: 'PartyTel', title: '@Resources.Locale.L_BSCSSetup_CmpTel', index: 'PartyTel', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 50 }, editable: true },
        { name: 'FaxNo', title: '@Resources.Locale.L_BSCSSetup_CmpFax', index: 'FaxNo', sorttype: 'string', width: 100, hidden: false,editoptions: { maxlength: 30 }, editable: true },
        { name: 'PartyMail', title: '@Resources.Locale.L_DNDetailVeiw_Mail', index: 'PartyMail', sorttype: 'string', width: 300, hidden: false, editoptions: { maxlength: 100 }, editable: true },
        { name: 'PartAddr1', title: '@Resources.Locale.L_BSCSDataQuery_PartAddr1', index: 'PartAddr1', sorttype: 'string', width: 100, hidden: false, editoptions: {maxlength: 60}, editable: true },
        { name: 'PartAddr2', title: '@Resources.Locale.L_BSCSDataQuery_PartAddr2', index: 'PartAddr2', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 40 }, editable: true },
        { name: 'PartAddr3', title: '@Resources.Locale.L_BSCSDataQuery_PartAddr3', index: 'PartAddr3', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 40 }, editable: true },
        { name: 'PartAddr4', title: '@Resources.Locale.L_BSCSDataQuery_PartAddr4', index: 'PartAddr4', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 40 }, editable: true },
        { name: 'PartAddr5', title: '@Resources.Locale.L_BSCSDataQuery_PartAddr5', index: 'PartAddr5', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 40 }, editable: true },
        { name: 'Cnty', title: '@Resources.Locale.L_BSCSDataQuery_Cnty', index: 'Cnty', editoptions: { maxlength: 2 }, width: 100, hidden: false, editable: true },
        { name: 'CntyNm', title: '@Resources.Locale.L_BSCSDataQuery_CntyNm', index: 'CntyNm', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'City', title: '@Resources.Locale.L_DNDetailVeiw_City', index: 'City', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'CityNm', title: '@Resources.Locale.L_DNDetailVeiw_CityNm', index: 'CityNm', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'State', title: 'State', index: 'State', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Zip', title: 'Zip', index: 'Zip', sorttype: 'string', width: 100, hidden: false, editoptions: { maxlength: 10 }, editable: true },
         { name: 'TaxNo', title: '@Resources.Locale.L_BSCSDataQuery_TaxNo', index: 'TaxNo', editoptions: { maxlength: 20 }, width: 100, hidden: false, editable: true },
        { name: 'OrderBy', title: '@Resources.Locale.L_DNDetailVeiw_OrderBy', index: 'OrderBy', sorttype: 'string', width: 100, hidden: true, editable: true }
    ];
    return partyColModel;
}

//订舱确认Excel
function Callfunction(trantype) {
    $("#PACKING_EXCEL_UPLOAD_FROM").submit(function () {
        var UId = $("#UId").val();
        $(this).find("input[type='hidden']").remove();
        $(this).append('<input type="hidden" name="UId" value="' + UId + '" />');
        $(this).append('<input type="hidden" name="ConfirmType" value="' + trantype + '" />');
        var postData = new FormData($(this)[0]);

        $.ajax({
            url: rootPath + "DNManage/Upload",
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
                    CommonFunc.Notify("", "@Resources.Locale.L_ActDeatilManage_Views_116" + data.message, 1300, "warning");
                    return false;
                }
                CommonFunc.Notify("", "@Resources.Locale.L_BSTQuery_ImpSuc", 500, "success");
                $("#BookingConfirmUploadWin").modal("hide");
                $("#SummarySearch").trigger("click");
            },
            cache: false,
            contentType: false,
            processData: false
        });

        return false;
    });
}
//订舱party信息导入Excel

function RegisterBookingBtn() {
    registBtnLookup($("#IncotermCdLookup"), {
        item: '#IncotermCd', url: rootPath + LookUpConfig.DlvTermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
            $("#IncotermCd").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TD", undefined, function ($grid, rd, elem) {
        $("#IncotermCd").val(rd.CD);
    }, function ($grid, elem) {
        $("#IncotermCd").val("");
    }));

    registBtnLookup($("#TradeTermLookup"), {
        item: '#TradeTerm', url: rootPath + LookUpConfig.TermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
            $("#TradeTerm").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TINC", undefined, function ($grid, rd, elem) {
        $("#TradeTerm").val(rd.CD);
    }, function ($grid, elem) {
        $("#TradeTerm").val("");
    }));

    registBtnLookup($("#CurLookup"), {
        item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
            $("#Cur").val(map.Cur);
        }
    }, undefined, LookUpConfig.GetCurAuto(groupId, undefined, function ($grid, rd, elem) {
        $(elem).val(rd.CUR);
    }));

    registBtnLookup($("#CostCenterLookup"), {
        item: '#CostCenter', url: rootPath + LookUpConfig.CostCenterUrl, config: LookUpConfig.CostCenterLookup, param: "", selectRowFn: function (map) {
            $("#CostCenter").val(map.CostCenter);
            $("#CostCenterdescp").val(map.Dep);
        }
    }, undefined, LookUpConfig.GetCostCenterAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#CostCenter").val(rd.COST_CENTER);
        $("#CostCenterdescp").val(rd.DEP);
    }));

    registBtnLookup($("#PporCdLookup"), {
        item: '#PporCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PporCd").val(map.CntryCd + map.PortCd);
            $("#PporName").val(map.PortNm);
        }
    }, { focusItem: $("#PporCd") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PporCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PporName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PpolCdLookup"), {
        item: '#PpolCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PpolCd").val(map.CntryCd + map.PortCd);
            $("#PpolName").val(map.PortNm);
        }
    }, { focusItem: $("#PpolCd") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PpolCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PpolName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PpodCdLookup"), {
        item: '#PpodCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PpodCd").val(map.CntryCd + map.PortCd);
            $("#PpodName").val(map.PortNm);
        }
    }, { focusItem: $("#PpodCd") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PpodCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PpodName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PdestCdLookup"), {
        item: '#PdestCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PdestCd").val(map.CntryCd + map.PortCd);
            $("#PdestName").val(map.PortNm);
        }
    }, { focusItem: $("#PdestCd") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PdestCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PdestName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PorCdLookup"), {
        item: '#PorCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PorCd").val(map.CntryCd + map.PortCd);
            $("#PorName").val(map.PortNm);
            ChangeColor();
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PorCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PorName").val(rd.PORT_NM);
        ChangeColor();
    }));

    registBtnLookup($("#PolCdLookup"), {
        item: '#PolCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PolCd").val(map.CntryCd + map.PortCd);
            $("#PolName").val(map.PortNm);
            ChangeColor();
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PolCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PolName").val(rd.PORT_NM);
        ChangeColor();
    }));

    registBtnLookup($("#PodCdLookup"), {
        item: '#PodCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PodCd").val(map.CntryCd + map.PortCd);
            $("#PodName").val(map.PortNm);
            ChangeColor();
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PodCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PodName").val(rd.PORT_NM);
        ChangeColor();
    }));

    //registBtnLookup($("#PortCdLookup"), {
    //    item: '#PortCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
    //        $("#PortCd").val(map.CntryCd + map.PortCd);
    //        $("#PortNm").val(map.PortNm);
    //    }
    //}, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#PortCd").val(rd.CNTRY_CD + rd.PORT_CD);
    //    $("#PortNm").val(rd.PORT_NM);
    //}));
    registBtnLookup($("#PortCdLookup"), {
        item: '#PortCd', url: rootPath + LookUpConfig.PorteUrl, config: LookUpConfig.BSCodeLookup, param: "", selectRowFn: function (map) {
            $("#PortCd").val(map.Cd);
            $("#PortNm").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TVST", undefined, function ($grid, rd, elem) {
        $("#PortCd").val(rd.CD);
        $("#PortNm").val(rd.CD_DESCP);
    }, function ($grid, elem) {
        $("#PortCd").val("");
        $("#PortNm").val("");
    }));

    registBtnLookup($("#DestCdLookup"), {
        item: '#DestCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#DestCd").val(map.CntryCd + map.PortCd);
            $("#DestName").val(map.PortNm);
            ChangeColor();
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#DestCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#DestName").val(rd.PORT_NM);
        ChangeColor();
    }));

    registBtnLookup($("#CarrierLookup"), {
        item: '#Carrier', url: rootPath + LookUpConfig.TCARUrl, config: LookUpConfig.BSCodeLookup, param: "", selectRowFn: function (map) {
            $("#Carrier").val(map.Cd);
            $("#CarrierNm").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TCAR", undefined, function ($grid, rd, elem) {
        $("#Carrier").val(rd.CD);
        $("#CarrierNm").val(rd.CD_DESCP);
    }, function ($grid, elem) {
        $("#Carrier").val("");
        $("#CarrierNm").val("");
    }));

    registBtnLookup($("#QtyuLookup"), {
        item: '#Qtyu', url: rootPath + LookUpConfig.QtyuUrl, config: LookUpConfig.QtyuLookup, param: "", selectRowFn: function (map) {
            $("#Qtyu").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "UB", undefined, function ($grid, rd, elem) {
        $(elem).val(rd.CD);
    }, function ($grid, elem) {
        $(elem).val("");
    }));

    registBtnLookup($("#PkgUnitLookup"), {
        item: '#PkgUnit', url: rootPath + LookUpConfig.QtyuUrl, config: LookUpConfig.QtyuLookup, param: "", selectRowFn: function (map) {
            $("#PkgUnit").val(map.Cd);
            $("#PkgUnitDesc").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "UB", undefined, function ($grid, rd, elem) {
        $("#PkgUnit").val(rd.CD);
        $("#PkgUnitDesc").val(map.CD_DESCP);
    }, function ($grid, elem) {
        $("#PkgUnit").val();
        $("#PkgUnitDesc").val();
    }));

    registBtnLookup($("#GwuLookup"), {
        item: '#Gwu', url: rootPath + LookUpConfig.NwuUrl, config: LookUpConfig.NwuLookup, param: "", selectRowFn: function (map) {
            $("#Gwu").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "UT", undefined, function ($grid, rd, elem) {
        $("#Gwu").val(rd.CD);
    }, function ($grid, elem) {
        $("#Gwu").val("");
    }));

    registBtnLookup($("#OexporterLookup"), {
        item: '#Oexporter', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#Oexporter").val(map.PartyNo);
            $("#OexporterNm").val(map.PartyName);
            $("#OexporterAddr").val(map.PartAddr1);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Oexporter").val(rd.PARTY_NO);
        $("#OexporterNm").val(rd.PARTY_NAME);
        $("#OexporterAddr").val(rd.PART_ADDR1);
    }));

    registBtnLookup($("#OimporterLookup"), {
        item: '#Oimporter', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#Oimporter").val(map.PartyNo);
            $("#OimporterNm").val(map.PartyName);
            $("#OimporterAddr").val(map.PartAddr1);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Oimporter").val(rd.PARTY_NO);
        $("#OimporterNm").val(rd.PARTY_NAME);
        $("#OimporterAddr").val(rd.PART_ADDR1);
    }));

    registBtnLookup($("#IexporterLookup"), {
        item: '#Iexporter', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#Iexporter").val(map.PartyNo);
            $("#IexporterNm").val(map.PartyName);
            $("#IexporterAddr").val(map.PartAddr1);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Iexporter").val(rd.PARTY_NO);
        $("#IexporterNm").val(rd.PARTY_NAME);
        $("#IexporterAddr").val(rd.PART_ADDR1);
    }));

    registBtnLookup($("#IimporterLookup"), {
        item: '#Iimporter', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#Iimporter").val(map.PartyNo);
            $("#IimporterNm").val(map.PartyName);
            $("#IimporterAddr").val(map.PartAddr1);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Iimporter").val(rd.PARTY_NO);
        $("#IimporterNm").val(rd.PARTY_NAME);
        $("#IimporterAddr").val(rd.PART_ADDR1);
    }));

    registBtnLookup($("#PaytermCdLookup"), {
        item: '#PaytermCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PaytermCd").val(map.CntryCd + map.PortCd);
            $("#PaytermNm").val(map.PortNm);
        }
    }, { focusItem: $("#PaytermCd") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PaytermCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PaytermNm").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PaytermCdLookup"), {
        item: '#PaytermCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PaytermCd").val(map.CntryCd + map.PortCd);
            $("#PaytermNm").val(map.PortNm);
        }
    }, { focusItem: $("#PaytermCd") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PaytermCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PaytermNm").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PaytermCdLookup"), {
        item: '#PaytermCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PaytermCd").val(map.CntryCd + map.PortCd);
            $("#PaytermNm").val(map.PortNm);
        }
    }, { focusItem: $("#PaytermCd") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PaytermCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PaytermNm").val(rd.PORT_NM);
    }));

    registBtnLookup($("#RegionLookup"), {
        item: '#Region', url: rootPath + LookUpConfig.TrgnUrl, config: LookUpConfig.TrgnLookup, param: "", selectRowFn: function (map) {
            $("#Region").val(map.Cd);
            //$("#RegionNm").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TRGN", undefined, function ($grid, rd, elem) {
        $("#Region").val(rd.CD);
        //$("#RegionNm").val(rd.CD_DESCP);
    }));

    //registBtnLookup($("#PickupWmsLookup"), {
    //    item: '#PickupWms', url: rootPath + LookUpConfig.WhUrl, config: LookUpConfig.WhLookup, param: "", selectRowFn: function (map) {
    //        $("#PickupWms").val(map.Cd);
    //    }
    //}, { focusItem: $("#PickupWms") }, LookUpConfig.GetCodeTypeAuto(groupId, "WH", function ($grid, rd, elem) {
    //    $("#PickupWms").val(rd.CD);
    //}));

    registBtnLookup($("#PickupWmsLookup"), {
        item: '#PickupWms', url: rootPath + LookUpConfig.SMWHUrl, config: LookUpConfig.SMWHLookup, param: "", selectRowFn: function (map) {
            $("#PickupWms").val(map.WsCd);
        }
    }, { focusItem: $("#PickupWms") }, LookUpConfig.GetSMWHAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PickupWms").val(rd.WS_CD);
    }));
}

function CopyFunction(thisItem, $SubGrid) {
    //初始化新增数据
    var data = { "FreightTerm": _bu, "TranType": _tran };
    data[_handler.key] = uuid();
    //setFieldValue([data]);
    //_handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
    //_handler.loadGridData("SubGrid2", $SubGrid2[0], [], [""]);
    getAutoNo("ShipmentId", "rulecode=SMRV_NO&cmp=" + cmp);

    $("#CreateBy").val(userId);
    $("#CreateDate").val(getDate(0, "-"));
    $("#Cmp").val(cmp);
    $("#Stn").val(stn);
    $("#Dep").val(dep);
    $("#CreateExt").val(ext);
        var voidfields = ["BookingInfo",//"CombineInfo", 
         "PolCd", "PolName", "PorCd", "PorName", "PodCd", "PodName", "DestCd", "DestName",
         "HouseNo", "Voyage1", "Etd", "Eta", "Atd", "Ata", "Ett", "Att", "Dtt", "Aqty", "Dqty",
         "Fqty", "Separate", "Fdqty", "Ndqty", "TruckNo", "Driver", "DriverTel"
        ];
        for (var i = 0; i < voidfields.length; i++) {
            $("#" + voidfields[i]).val('');
        }
        $("#CombineInfo").attr('disabled', true);

        $("#Status").val('A');
        $("#Corder").val('N');
        $("#Border").val('N');
        $("#Torder").val('N');

        var dataRow, addData = [];
        var rowIds = $SubGrid.getDataIDs();
        for (var i = 0; i < rowIds.length; i++) {
            var rowDatas = $SubGrid.jqGrid('getRowData', rowIds[i]);
            dataRow = {
                UId: "",
                PartyType: rowDatas.PartyType,
                TypeDescp: rowDatas.TypeDescp,
                PartyNo: rowDatas.PartyNo,
                PartyName: rowDatas.PartyName,
                PartyAttn: rowDatas.PartyAttn,
                PartyTel: rowDatas.PartyTel,
                PartyMail: rowDatas.PartyMail,
                PartAddr1: rowDatas.PartAddr1,
                PartAddr2: rowDatas.PartAddr2,
                PartAddr3: rowDatas.PartAddr3,
                Cnty: rowDatas.Cnty,
                CntyNm: rowDatas.CntyNm,
                City: rowDatas.City,
                CityNm: rowDatas.CityNm,
                State: rowDatas.State,
                Zip: rowDatas.Zip,
                OrderBy: rowDatas.OrderBy,
            };
            addData.push(dataRow);
        }

    _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
    for (var i = 0; i < addData.length; i++) {
        $("#SubGrid").jqGrid("addRowData", undefined, addData[i], "last");
    }
    gridEditableCtrl({ editable: true, gridId: "SubGrid" });
}

function AddMBPrintFunc() {
    var listBar = [];
    listBar.push({
        menuId: "FCL01", menuName: "Booking Form", menuFunc: function () {
            /*
            var id = $("#UId").val();
            var params = {
                currentCondition: "",
                uid: id,
                rptdescp: "Booking From1",
                rptName: 'FCL01',
                formatType: 'xls',
                //exportType: 'DOWNLOAD'
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
            */
            
            var id = $("#UId").val();
            var data = {
                reportId: "FCL01",
                conditionString: "UId=" + id + "&sopt_UId=eq",
                exportFileType: "pdf",
                reportName: "Booking Form",
                fileType: "BF",
                jobNo: id,
                GroupId: groupId,
                Cmp: cmp,
                Stn: "*",
                combineInfo: $("#CombineInfo").val()
            };

            CommonFunc.ToogleLoading(true);
            if (id) {
                CheckEdoc("BF", data);
            }

        }, menuCss: "glyphicon glyphicon-print"
    });

    listBar.push({
        menuId: "FCL02", menuName: "Draft B/L", menuFunc: function () {
            var id = $("#UId").val();
            var data = {
                reportId: "FCL02",
                conditionString: "UId=" + id + "&sopt_UId=eq",
                exportFileType: "pdf",
                reportName: "Draft BL",
                fileType: "BL",
                jobNo: id,
                GroupId: groupId,
                Cmp: cmp,
                Stn: "*",
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
                        if (data.Success == true) {
                            CommonFunc.Notify("", "@Resources.Locale.L_BaseBookingSetup_Scripts_163", 500, "success");
                        } else {
                            var message = data.Message;
                            if (message) {
                                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_Fail" + "," + message, 500, "warning");
                            } else {
                                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_Fail", 500, "warning");
                            }

                        }
                        CommonFunc.ToogleLoading(false);
                    }
                });
            }
        }, menuCss: "glyphicon glyphicon-print"
    });

    listBar.push({
        menuId: "AIR01", menuName: "Draft B/L - Air", menuFunc: function () {
            var id = $("#UId").val();
            var ShipmentId = $("#ShipmentId").val();
            var data = {
                reportId: "AIR01",
                conditionString: "UId=" + id + "&sopt_UId=eq",
                "arg0": "shipid",
                "val0": ShipmentId,
                "arg_count": 1,
                exportFileType: "pdf",
                reportName: "Draft BL AIR",
                fileType: "BL",
                jobNo: id,
                GroupId: groupId,
                Cmp: cmp,
                Stn: "*",
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
                        if (data.Success == true) {
                            CommonFunc.Notify("", "@Resources.Locale.L_BaseBookingSetup_Scripts_163", 500, "success");
                        } else {
                            var message = data.Message;
                            if (message) {
                                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_Fail" + "," + message, 500, "warning");
                            } else {
                                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_Fail", 500, "warning");
                            }

                        }
                        CommonFunc.ToogleLoading(false);
                    }
                });
            }
        }, menuCss: "glyphicon glyphicon-print"
    });

    listBar.push({
        menuId: "FCL03", menuName: "@Resources.Locale.L_DNManage_BookingformChinese", menuFunc: function () {
            var id = $("#UId").val();
            var data = {
                reportId: "FCL03",
                conditionString: "UId=" + id + "&sopt_UId=eq",
                exportFileType: "pdf",
                reportName: "@Resources.Locale.L_DNManage_BookingformChinese",
                fileType: "BF",
                jobNo: id,
                GroupId: groupId,
                Cmp: cmp,
                Stn: "*",
                combineInfo: $("#CombineInfo").val()
            };

            CommonFunc.ToogleLoading(true);
            if (id) {
                CheckEdoc("BFC", data);
                //$.ajax({
                //    async: true,
                //    cache: false,
                //    dataType: "json",
                //    url: rootPath + "EDOC/CreateNewReport2Edoc",
                //    data: data,
                //    type: 'POST',
                //    "error": function (xmlHttpRequest, errMsg) {
                //        alert(errMsg);
                //        CommonFunc.ToogleLoading(false);
                //    },
                //    success: function (data) {
                //        if (data.Success == true) {
                //            CommonFunc.Notify("", "@Resources.Locale.L_BaseBookingSetup_Scripts_163", 500, "success");
                //        } else {
                //            var message = data.Message;
                //            if (message) {
                //                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_Fail" + "," + message, 500, "warning");
                //            } else {
                //                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_Fail", 500, "warning");
                //            }

                //        }
                //        CommonFunc.ToogleLoading(false);
                //    }
                //});
            }
        }, menuCss: "glyphicon glyphicon-print"
    });
    listBar.push({
        menuId: "ISF01", menuName: "ISF", menuFunc: function () {
            var id = $("#UId").val();
            var ShipmentId = $("#ShipmentId").val();
            var data = {
                reportId: "ISF01",
                uid: id,
                exportFileType: "pdf",
                reportName: "ISF",
                fileType: "ISF",
                jobNo: id,
                "arg0": "shipid",
                "val0": ShipmentId,
                "arg1": "uid",
                "val1": id,
                "arg_count": 2,
                GroupId: groupId,
                Cmp: cmp,
                Stn: "*",
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
                        if (data.Success == true) {
                            CommonFunc.Notify("", "@Resources.Locale.L_BaseBookingSetup_Scripts_163", 500, "success");
                        } else {
                            var message = data.Message;
                            if (message) {
                                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_Fail" + "," + message, 500, "warning");
                            } else {
                                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_Fail", 500, "warning");
                            }

                        }
                        CommonFunc.ToogleLoading(false);
                    }
                });
            }
        }, menuCss: "glyphicon glyphicon-print"
    });

    MenuBarFuncArr.AddDDLMenu("MBPrint", " glyphicon glyphicon-print", "@Resources.Locale.L_DNManage_StatementFile", function () { }, null, listBar);

    var listBar1 = [];
    listBar1.push({
        menuId: "FCL01P", menuName: "Booking Form", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            var conditions = "UId=" + thisuid + "&sopt_UId=eq";
            genReport(thisuid, "FCL01", "Booking Form", conditions);
        }, menuCss: "glyphicon glyphicon-list-alt"
    });

    listBar1.push({
        menuId: "FCL02P", menuName: "Draft B/L", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            var conditions = "UId=" + thisuid + "&sopt_UId=eq";
            genReport(thisuid, "FCL02", "Draft BL", conditions);
        }, menuCss: "glyphicon glyphicon-list-alt"
    });

    listBar1.push({
        menuId: "AIR01P", menuName: "Draft B/L - Air", menuFunc: function () {
            var thisuid = $("#UId").val();
            var shipmentId = $("#ShipmentId").val()
            if (!thisuid)
                return;
            var conditions = "UId=" + thisuid + "&sopt_UId=eq";
            genReport01(thisuid, shipmentId, "AIR01", "Draft BL-Air", conditions);
        }, menuCss: "glyphicon glyphicon-list-alt"
    });

    listBar1.push({
        menuId: "FCL03P", menuName: "@Resources.Locale.L_DNManage_BookingformChinese", menuFunc: function () {
            var thisuid = $("#UId").val();
            if (!thisuid)
                return;
            var conditions = "UId=" + thisuid + "&sopt_UId=eq";
            genReport(thisuid, "FCL03", "@Resources.Locale.L_DNManage_BookingformChinese", conditions);
        }, menuCss: "glyphicon glyphicon-list-alt"
    });

    listBar1.push({
        menuId: "ISF01P", menuName: "ISF", menuFunc: function () {
            var thisuid = $("#UId").val();
            var shipments = $("#ShipmentId").val();
            if (!thisuid)
                return;
            var params = {
                //currentCondition: "",
                //val: dnNo,
                uid: thisuid,
                shipid: shipments,
                rptdescp: "ISF",
                rptName: "ISF01",
                formatType: 'xls',
                exportType: 'PREVIEW',
            };
            showReport(params);
        }, menuCss: "glyphicon glyphicon-list-alt"
    });
    MenuBarFuncArr.AddDDLMenu("MBPreview", " glyphicon glyphicon-print", "@Resources.Locale.L_ActManage_Preview", function () { }, null, listBar1);
}
function CheckEdoc(TYPE,data) {
    $.ajax({
        async: true,
        url: rootPath + "EDOC/FileCheck",
        type: 'POST',
        data: { CMP: cmp, TYPE: TYPE, GROUP_ID: groupId, JOBNO: _uid,smdoc:"Y" },
        dataType: "json",
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
            MenuBarFuncArr.SaveResult = false;
            //dtd.resolve();
        },
        success: function (result) {
            if (result.IsOk == "Y") {
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
                                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_Success", 500, "success");
                            } else {
                                CommonFunc.Notify("", "@Resources.Locale.L_BaseBookingSetup_Scripts_163", 500, "success");
                            }
                        } else {

                            if (message) {
                                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_Fail" + "," + message, 500, "warning");
                            } else {
                                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_Fail", 500, "warning");
                            }

                        }
                        CommonFunc.ToogleLoading(false);
                    }
                });
            }
            else {
                alert(result.message);
                CommonFunc.ToogleLoading(false);
            }

        }
    });
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

function genReport(thisUId, rptId, rptName,conditions) {
    var params = {
        currentCondition: "",
        uid: thisUId,
        rptdescp: rptName,
        rptName: rptId,
        formatType: 'xls',
        exportType: 'PREVIEW',
        conditions: conditions
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

function genReport01(thisUId, shipmentId, rptId, rptName, conditions) {
    var params = {
            currentCondition: "",
            rptdescp: rptName,
            rptName: rptId,
            formatType: 'pdf',
            exportType: 'PREVIEW',
            shipid:shipmentId,
            'conditions': conditions,//jQuery.serialize()已经是进行URL编码过的。
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


function DefaultAllocation() {
    var uid = $("#UId").val();
    if (!uid) {
        CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
        return;
    }
    var status = $("#Status").val();
    if ("A" != status) {
        CommonFunc.Notify("", "@Resources.Locale.L_DNManage_HasBeenBooking", 500, "warning");
        return;
    }
    $.ajax({
        async: true,
        url: rootPath + "BookingAction/DefaultAllocation",
        type: 'POST',
        data: { UId: uid },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            if (data.IsOk == "Y") {
                CommonFunc.Notify("", data.message, 500, "success");
                _handler.topData = { UId: _uid };
                MenuBarFuncArr.MBCancel();
            }
            else {
                CommonFunc.Notify("", data.message, 500, "warning");
            }
        }
    });
}

function DefaultVoidSM(trantype) {
    var cancelreson = "";
    var TranType = $("#TranType").val();
    if (trantype == "T") {
        cancelreson = $("#VoidsmRemark").val();
        if (cancelreson == "") {
            CommonFunc.Notify("", "@Resources.Locale.L_ActManage_EnterReason", 500, "warning");
            return;
        }
    }
    //var cancelreson = "";
    var uid = $("#UId").val();
    var shipments = $("#ShipmentId").val();
    var iscontinue = window.confirm("@Resources.Locale.L_BaseBookingSetup_Script_113" + shipments + "@Resources.Locale.L_BaseBookingSetup_Script_114");
    if (!iscontinue) {
        return;
    }
    $.ajax({
        async: true,
        url: rootPath + "BookingAction/VoidSM",
        type: 'POST',
        data: { UId: uid, CancelReson: cancelreson },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            if (data.IsOk == "Y") {
                CommonFunc.Notify("", data.message, 500, "success");
            }
            else {
                CommonFunc.Notify("", data.message, 500, "warning");
            }
            $("#CloseVoidSMWin").trigger("click");
        }
    });
}

function SetStatusToReadOnly(func, isconfirm,trantype) {
    if (isconfirm) {
        func();
        return;
    }
    var _status = $("#Status").val();

    if (_status == "A" || _status == "B" || isEmpty(_status)) {
        setdisabled(false);
        func();
    } else {
        setdisabled(true);
        _handler.gridEditableCtrl(false);
        if (isconfirm) {
            var readonlys = ["PodCd", "PodName", "HouseNo", "MasterNo", "Etd", "Eta", "Etd1", "Eta1", "Vessel1", "Voyage1",
                "Vessel2", "Voyage2", "Vessel3", "Voyage3", "Vessel4", "Voyage4", "BookingInfo", "PodCdLookup",
                "Etd2", "Eta2", "Etd3", "Eta3", "Etd4", "Eta4", "Carrier", "CarrierNm",
                "CutBlDate", "RlsCntrDate", "SignBack", "CutPortDate", "RcvDate", "CustomsDate", "PortRlsDate", "RcvDocDate"];
            SetArrayDisable(readonlys);
        } else {
            var readonlys = ["DnEtd", "Cw"];
            if (trantype == "L")
                readonlys = ["DnEtd", "Gw", "Cbm"];
            if (trantype == "F")
                readonlys = ["DnEtd", "Gw", "Tcbm"];
            if (trantype == "T")
                readonlys = ["DnEtd", "Gw", "Aqty", "Fqty"];
            SetArrayDisable(readonlys);
        }
    }
    //$("#BlRmk").attr('disabled', false);
}

function SetArrayDisable(arr){
    if(arr.constructor != Array)return;
    for (var i = 0; i < arr.length; i++) {
        $("#" + arr[i]).attr('disabled', false);
        $("#" + arr[i]).parent().find("button").attr("disabled", false);
    }
}

function CheckStatus() {
    var _status = $("#Status").val();
    return true;
    if (_status == "A" || _status == "B" || isEmpty(_status)) {
    } else {
        alert("@Resources.Locale.L_BaseBookingSetup_Script_115");
        return false;
    }
}

function SetEtdEta() {
    $("#Etd1").change(function () {
        getChangeEdt();
    });
    $("#Etd2").change(function () {
        getChangeEdt();
    });
    $("#Etd3").change(function () {
        getChangeEdt();
    });
    $("#Etd4").change(function () {
        getChangeEdt();
    });

    $("#Eta1").change(function () {
        getChangeEda();
    });
    $("#Eta2").change(function () {
        getChangeEda();
    });
    $("#Eta3").change(function () {
        getChangeEda();
    });
    $("#Eta4").change(function () {
        getChangeEda();
    });
}

function getChangeEdt() {
    if (!isEmpty($("#Etd1").val())) {
        $("#Etd").val($("#Etd1").val());
    }
    else if (!isEmpty($("#Etd2").val())) {
        $("#Etd").val($("#Etd2").val());
    }
    else if (!isEmpty($("#Etd3").val())) {
        $("#Etd").val($("#Etd3").val());
    }
    else if (!isEmpty($("#Etd4").val())) {
        $("#Etd").val($("#Etd4").val());
    }
}

function getChangeEda() {
    if (!isEmpty($("#Eta4").val())) {
        $("#Eta").val($("#Eta4").val());
    }
    else if (!isEmpty($("#Eta3").val())) {
        $("#Eta").val($("#Eta3").val());
    }
    else if (!isEmpty($("#Eta2").val())) {
        $("#Eta").val($("#Eta2").val());
    }
    else if (!isEmpty($("#Eta1").val())) {
        $("#Eta").val($("#Eta1").val());
    }
}

function ShowSMStatus(_status) {
    var statsmap = { 'A': '@Resources.Locale.c', 'B': '@Resources.Locale.L_BSTSetup_Book', 'C': '@Resources.Locale.L_UserQuery_ComBA', 'D': '@Resources.Locale.L_UserQuery_Call', 'I': '@Resources.Locale.L_UserQuery_In', 'P': '@Resources.Locale.L_UserQuery_SealCnt', 'O': '@Resources.Locale.L_UserQuery_Out', 'G': '@Resources.Locale.L_UserQuery_Declara', 'H': '@Resources.Locale.L_UserQuery_Release', 'V': '@Resources.Locale.L_BSCSDateQuery_Cancel', 'Z': '@Resources.Locale.L_UserQuery_Return', 'U': '@Resources.Locale.L_DNManage_NtConfBL', 'Y': '@Resources.Locale.L_DNManage_ConfBL' };
    var descp = statsmap[_status] || _status;
    $("#Status_descp").text(descp);
}

function ChangeColor() {
    var _Estimate = ["PpolCd", "PporCd", "PpodCd", "PdestCd", "PpolName", "PporName", "PpodName", "PdestName","Pgw","Pcbm","Pvw"];
    var _Actual = ["PolCd", "PorCd", "PodCd", "DestCd", "PolName", "PorName", "PodName", "DestName","Gw","Cbm","Vw"];
    for (var i = 0; i < _Estimate.length; i++) {
        var _estimateval=$("#" + _Estimate[i]).val();
        var _actualval = $("#" + _Actual[i]).val();
        if (isEmpty(_estimateval) || isEmpty(_actualval)) continue;
        _estimateval = _estimateval.toUpperCase();
        _actualval = _actualval.toUpperCase();
        if (_estimateval != _actualval) {
            $("#" + _Actual[i]).css("color", "red");
        } else {
            $("#" + _Actual[i]).css("color", "black");
        }
    }
}

function PpMonitor() {
    $("#Gw").change(function () {
        ChangeColor();
    });
    $("#Cbm").change(function () {
        ChangeColor();
    });
}

function contrast(comparie, etd) {
    if (isEmpty(comparie)) return false;
    if (isEmpty(etd)) return false;
    comparie = comparie.replace(new  RegExp("/", "gm" ),"-");
    etd = etd.replace(new  RegExp("/", "gm" ),"-");
    //etd = etd.replace("/", "-");
    if (comparie < etd) return false;
    var beginTimes = comparie.substring(0, 10).split('-');
    var endTimes = etd.substring(0, 10).split('-');

    var starttime = new Date(beginTimes[0], beginTimes[1], beginTimes[2]);
    var starttimes = starttime.getTime();

    var lktime = new Date(endTimes[0], endTimes[1], endTimes[2]);
    var lktimes = lktime.getTime();

    if (starttimes >= lktimes) {
        return true;
    }
    else
        return false;
}

String.prototype.replaceAll = function (s1, s2) {
    return this.replace(new RegExp(s1, "gm"), s2);
}

function _initCombineInv()
{
    var $CombineInvDialog = $("#CombineInvDialog");
    var colModel1 = [
            { name: 'DnNo', title: 'DN NO', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
            { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', width: 130, align: 'left', sorttype: 'string', hidden: false },
            { name: 'DnType', title: 'DN Type', index: 'DnType', width: 85, align: 'left', sorttype: 'string', hidden: false }
        ];

    new genGrid(
        $("#CombineInvGrid"),
        {
            datatype: "local",
            loadonce:true,
            colModel: colModel1,
            caption: "Combine DN List",
            height: 300,
            rows: 999999,
            refresh: false,
            cellEdit: false,//禁用grid编辑功能
            pginput: false,
            sortable: true,
            pgbuttons: false,
            exportexcel: false,
            toppager:false,
            multiselect: false,
            sortname: "DnNo"
        }
    );
    $("#CombineInvDialog").on("show.bs.modal", function(){
        var UId =  $("#UId").val();

        if(UId == "" || UId == null)
        {
              CommonFunc.Notify("", "@Resources.Locale.L_DNManage_MayNotSave", 500, "warning");
              return;
        }
        else
        {
            $.post(rootPath + 'Invoice/getCombinDn', {"UId": UId}, function(data, textStatus, xhr) {
                if(data.message == "success")
                {
                    var gridData = $.parseJSON(data.returnData.Content);
                    $("#CombineInvGrid").jqGrid("clearGridData");
                    $("#CombineInvGrid").jqGrid("setGridParam", {
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
        }
    });

    $("#CombineInvBtn").click(function(){
        var selRowId = $("#CombineInvGrid").jqGrid('getGridParam', 'selarrrow');
        var sm_id = $("#ShipmentId").val();
        var c_com = $("#c_com").val();
        var c_io = $("#c_io").val();
        var ids = "";
        var api_url = "";
        var chk_url = "";
        if(c_io == "I")
        {
            api_url = rootPath + "Invoice/CombineInvI";
            chk_url = rootPath + "Invoice/chkCombineI";
            if(c_com == "")
            {
                alert("@Resources.Locale.L_BaseBookingSetup_Script_116");
                return;
            }
        }
        else
        {
            api_url = rootPath + "Invoice/CombineInvO";
            chk_url = rootPath + "Invoice/chkCombineO";
        }

        $.post(chk_url, {sm_id: sm_id, c_com: c_com}, function(data, textStatus, xhr) {
            if(data.msg == "Y")
            {
                if(confirm("@Resources.Locale.L_DNManage_CombinedBefore"))
                {
                    $.ajax({
                        async: true,
                        url: api_url,
                        type: 'POST',
                        data: {sm_id: sm_id, c_com: c_com, cover: "Y"},
                        beforeSend: function (xmlHttpRequest, successMsg) {
                            CommonFunc.ToogleLoading(true);
                        },
                        error: function (xmlHttpRequest, errMsg) {
                            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_combError", 1000, "warning");
                            CommonFunc.ToogleLoading(false);
                        },
                        success: function (result) {
                            if(result.msg == "success")
                            {
                                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_CombS", 1000, "success");
                                //alert(rootPath + "DNManage/InvPkgQuery?ShipmentId=" + sm_id);
                                top.topManager.openPage({
                                    href: rootPath + "DNManage/InvPkgQuery?ShipmentId=" + sm_id,
                                    title: 'Invoice/Packing',
                                    id: 'DN011',
                                    search: "ShipmentId=" + sm_id,
                                    reload: true
                                });
                            }
                            else
                            {
                                CommonFunc.Notify("", result.msg, 1000, "warning");
                            }
                            CommonFunc.ToogleLoading(false);
                        }
                    });
                }
            }
            else
            {
                $.ajax({
                    async: true,
                    url: api_url,
                    type: 'POST',
                    data: {sm_id: sm_id, c_com: c_com},
                    beforeSend: function (xmlHttpRequest, successMsg) {
                        CommonFunc.ToogleLoading(true);
                    },
                    error: function (xmlHttpRequest, errMsg) {
                        CommonFunc.Notify("", "@Resources.Locale.L_DNManage_combError", 1000, "warning");
                        CommonFunc.ToogleLoading(false);
                    },
                    success: function (result) {
                        if(result.msg == "success")
                        {
                            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_CombS", 1000, "success");
                            //alert(rootPath + "DNManage/InvPkgQuery?ShipmentId=" + sm_id);
                            top.topManager.openPage({
                                href: rootPath + "DNManage/InvPkgQuery?ShipmentId=" + sm_id,
                                title: 'Invoice/Packing',
                                id: 'DN011',
                                search: "ShipmentId=" + sm_id,
                                reload: true
                            });
                        }
                        else
                        {
                            CommonFunc.Notify("", result.msg, 1000, "warning");
                        }
                        CommonFunc.ToogleLoading(false);
                    }
                });
            }
        }, "JSON");
    });

    $("#CancelCombineInvBtn").click(function(event) {
        var UId =  $("#UId").val();
        var ShipmentId = $("#ShipmentId").val();
        var c_com = $("#c_com").val();
        var c_io = $("#c_io").val();
        var api_url = "";

        if(c_io == "I")
        {
            api_url = rootPath + "Invoice/CancelCombineInvI";
            if(c_com == "")
            {
                alert("@Resources.Locale.L_BaseBookingSetup_Script_116");
                return;
            }
        }
        else
        {
            api_url = rootPath + "Invoice/CancelCombineInvO";
        }

        if(UId == "" || UId == null)
        {
              CommonFunc.Notify("", "@Resources.Locale.L_DNManage_MayNotSave", 500, "warning");
              return;
        }
        else
        {
            if(confirm("@Resources.Locale.L_DNManage_SureCancelComb"))
            {
                $.post( api_url, {ShipmentId: ShipmentId, c_com:c_com}, function(data, textStatus, xhr) {
                    if(data.msg == "success")
                    {
                        CommonFunc.Notify("", "@Resources.Locale.L_Layout_CanlMer", 1000, "success");
                    }
                    else
                    {
                        CommonFunc.Notify("", data.msg, 1000, "warning");
                    }
                }, "JSON");
            }
        }
    });
}

function setFix() {
    var LspCost = parseFloat($("#LspCost").val());
    var TruckCost = parseFloat($("#TruckCost").val());
    var FreightAmt = parseFloat($("#FreightAmt").val());
    var Gvalue = parseFloat($("#Gvalue").val());

    if (!isNaN(LspCost))
        $("#LspCost").val(LspCost.toFixed(2));
    if (!isNaN(TruckCost))
        $("#TruckCost").val(TruckCost.toFixed(2));
    if (!isNaN(FreightAmt))
        $("#FreightAmt").val(FreightAmt.toFixed(2));
    if (!isNaN(Gvalue))
        $("#Gvalue").val(Gvalue.toFixed(2));
}

var BaseBooking_ScufcolModel = [
	     { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
        { name: 'UFid', title: 'UFid', index: 'UFid', sorttype: 'string', editable: false, hidden: true },
        { name: 'DnNo', title: 'Dn No', index: 'DnNo', sorttype: 'string', width: 120, hidden: false, editable: false },
        //{ name: 'Cuft', title: '类型', index: 'Cuft', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'L', title: '@Resources.Locale.L_AirBookingSetup_Script_87', index: 'L', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'W', title: '@Resources.Locale.L_AirBookingSetup_Script_88', index: 'W', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'H', title: '@Resources.Locale.L_AirBookingSetup_Script_89', index: 'H', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'Pkg', title: '@Resources.Locale.L_DNManage_PkgNum', index: 'Pkg', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'PkgUnit', title: '@Resources.Locale.L_BaseLookup_Unit', index: 'PkgUnit', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Vw', title: '@Resources.Locale.L_AirBookingSetup_Script_90', index: 'Vw', width: 140, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.00' }, editable: false, hidden: false }
];

var BaseBooking_DnModel = [
        { name: 'DnNo', title: 'Dn No', index: 'DnNo', sorttype: 'string', width: 120, editable: false, hidden: false },
        { name: 'BlLevel', title: '@Resources.Locale.L_DNManage_BLType', index: 'BlLevel', sorttype: 'string', editable: false, hidden: false, editable: true },
        { name: 'ExportNo', title: '@Resources.Locale.L_DNApproveManage_ExportNo', index: 'ExportNo', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'Unicode', title: '@Resources.Locale.L_DNApproveManage_Unicode', index: 'Unicode', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'ApproveNo', title: '@Resources.Locale.L_DNApproveManage_ApprovalNo', index: 'ApproveNo', sorttype: 'string', width: 120, hidden: false, editable: false },
       {
           name: 'AskTim', title: '@Resources.Locale.L_DNManage_RqDelaDate', index: 'AskTim', width: 150, align: 'left', hidden: false, editable: true, sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: false,
           editoptions: myEditDateInit,
           formatter: 'date',
           formatoptions: {
               srcformat: 'ISO8601Long',
               newformat: 'Y-m-d',
               defaultValue: ""
           }
       },
        { name: 'EdeclNo', title: '@Resources.Locale.L_DNApproveManage_EdeclNo', index: 'EdeclNo', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'DeclDate', title: '@Resources.Locale.L_BaseLookup_DeclDate', index: 'DeclDate', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'DeclRlsDate', title: '@Resources.Locale.L_DNManage_RelDate', index: 'DeclRlsDate', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'NextNum', title: '@Resources.Locale.L_NEXT_NUMBER', index: 'NextNum', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'Dremark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Dremark', sorttype: 'string', width: 120, hidden: false, editable: false }
];

function BlRemarkModify() {
    //var Status = $("#Status").val();
    //if (Status == "A" || Status == "B" || Status == "C" || Status == "D" || Status == "I" || Status == "P") {
        $("#BlRmk").attr('disabled', false);
        MenuBarFuncArr.DisableAllItem();
        MenuBarFuncArr.Enabled(["MBCancel", "MBSave"]);
    //}
    //else {
    //    var statsmap = { 'O': '离厂', 'G': '报关', 'H': '放行', 'V': '取消', 'Z': '退运', 'U': '未确认提单', 'Y': '已确认提单' };
    //    var descp = statsmap[Status] || Status;
    //    alert("已经" + descp + "，不能修改！");
    //    return;
    //}
}

function ResetValue(val, type) {
    if (type == "A" || (type == "E" & val <= 20)) {
        if (val <= parseInt(val) + 0.5 && val > parseInt(val)) {
            return parseInt(val) + 0.5;
        }
        else if (val > parseInt(val) + 0.5 && val < parseInt(val) + 1) {
            return parseInt(val) + 1;
        }
    }
    else if (type == "E" && val > 20) {
        if (val > parseInt(val)) {
            return parseInt(val) + 1;
        }
    }
    return val;
}