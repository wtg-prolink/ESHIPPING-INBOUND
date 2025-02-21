$(function () {
    var $SubGrid = $("#SubGrid");
    var $SubGrid1 = $("#SubGrid1");
    _handler.saveUrl = rootPath + "TKBL/SaveProcessStatus";
    _handler.inquiryUrl = rootPath + "TKBL/GetProcessStatusData";
    _handler.config = LookUpConfig.ShipmentUrl;

    _handler.addData = function () {
        _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
    }

    _handler.checkData = function checkData($grid, nullCols, sameCols) {
        var rowIds = $grid.getDataIDs();
        for (var i = 0; i < rowIds.length; i++) {
            var rowDatas = $grid.jqGrid('getRowData', rowIds[i]);
            if (isEmpty($.trim(rowDatas["Location"])) && isEmpty($.trim(rowDatas["EvenDate"])))
                continue;
            for (var j = 0; j < nullCols.length; j++) {
                var col = nullCols[j];
                if (isEmpty($.trim(rowDatas[col.name]))) {
                    try {
                        $grid.jqGrid("editCell", rowIds[i], col.index, true);
                    }
                    catch (e) {
                    }
                    CommonFunc.Notify("", col.text + _handler.lang.isNotNull, 2000, "warning");
                    return false;
                }
            }
        }
    }

    //_handler.beforSave = function () {
    //    var nullCols = [], sameCols = [];
    //    //nullCols.push({ name: "StsCd", index: 2, text: 'Status Code' });
    //    nullCols.push({ name: "Location", index: 4, text: 'Location' });
    //    nullCols.push({ name: "EvenDate", index: 5, text: 'Even Date' });
    //    sameCols.push({ name: "StsCd", index: 2, text: 'Status Code' });
    //    return _handler.checkData($SubGrid, nullCols, sameCols);
    //}

    _handler.saveData = function (dtd) {
        var containerArray = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};//获取所有改变的值
        changeData["sub"] = containerArray;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        data["EvenNo"] = _evenNo;
        data["ShipmentId"] = $("#ShipmentId").val()
        
        ajaxHttpSaveBar(dtd, _handler.saveUrl, data,
            function (result) {
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
        //_handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
        if (data["allStatus"] === true) {
            $SubGrid.setColProp('StsDescp', { editable: true });
            $SubGrid.setColProp('StsCd', { editable: true });
        }
        else {
            $SubGrid.setColProp('StsDescp', { editable: false });
            $SubGrid.setColProp('StsCd', { editable: false });
        }
        //if (data["status"]) {
        //    if (data["status"].length <= 0)
        //        $SubGrid.setColProp('StsCd', { editable: true });
        //}
        //else
        //    $SubGrid.setColProp('StsCd', { editable: true });

        if (data["status"])
            _handler.loadGridData("SubGrid", $SubGrid[0], data["status"], [""]);
        else
            _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);

        if (data["sub"])
            _handler.loadGridData("SubGrid1", $SubGrid1[0], data["sub"], [""]);
        else
            _handler.loadGridData("SubGrid1", $SubGrid1[0], [], [""]);

        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
    }

    _handler.afterEdit = function () {
        gridEditableCtrl({ editable: false, gridId: "SubGrid1" });
    }

    _handler.loadMainData = function (map) {
        if (isEmpty(_evenNo) && isEmpty(_blNo)) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "TKBL/GetProcessStatusData", { EvenNo: _evenNo, blNo: _blNo, loading: true },
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    function getStatus(name) {
        var _name = name;
        var status_op = getLookupOp("SubGrid",
            {
                url: rootPath + LookUpConfig.StatusUrl,
                config: LookUpConfig.StatusLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    var stsDescp = map.Edescp;
                    if (_tranType == "D" || _tranType == "T")
                        stsDescp = map.Edescp + "/" + map.Ldescp;
                    setGridVal($grid, selRowId, 'StsDescp', stsDescp, null);
                    setGridVal($grid, selRowId, _name, map.StsCd, "lookup");
                    return map.StsCd;

                }
            }, LookUpConfig.GetStatusAuto(groupId, $SubGrid,
            function ($grid, rd, elem, selRowId) {
                var stsDescp = rd.EDESCP;
                if (_tranType == "D" || _tranType == "T")
                    stsDescp = rd.EDESCP + "/" + rd.LDESCP;
                setGridVal($grid, selRowId, 'StsDescp', stsDescp, null);
                setGridVal($grid, selRowId, _name, rd.STS_CD, "lookup");
                $(elem).val(rd.STS_CD);
            }), { param: "" });
        return status_op;
    }

    function getcityop(name) {
        var _name = name;
        var city_op = getLookupOp("SubGrid",
            {
                url: rootPath + LookUpConfig.CityPortUrl,
                config: LookUpConfig.CityPortLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, _name, map.CntryCd + map.PortCd, "lookup");
                    setGridVal($grid, selRowId, "LocationDescp", map.PortNm, null);
                    return map.CntryCd + map.PortCd;
                }
            }, LookUpConfig.GetCityPortAuto(groupId, $SubGrid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, _name, rd.CNTRY_CD + rd.PORT_CD, "lookup");
                setGridVal($grid, rowid, "LocationDescp", rd.PORT_NM, null);
                $(elem).val(rd.CNTRY_CD + rd.PORT_CD);
            }), { param: "" });
        return city_op;
    }
    if (_tranType === "D" || _tranType === "T") {
        getcityop = function (name) {
            var _name = name;
            var city_op = getLookupOp("SubGrid",
                {
                    url: rootPath + LookUpConfig.TruckPortCdUrl,
                    config: LookUpConfig.TruckPortCdLookup,
                    returnFn: function (map, $grid) {
                        var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                        setGridVal($grid, selRowId, "LocationDescp", map.PortNm, null);
                        setGridVal($grid, selRowId, _name, map.PortCd, "lookup");
                        return map.PortCd;
                    }
                }, LookUpConfig.TruckPortCdAuto(groupId, $SubGrid,
                function ($grid, rd, elem, rowid) {
                    setGridVal($grid, rowid, "LocationDescp", rd.PORT_NM, null);
                    setGridVal($grid, rowid, _name, rd.PORT_CD, "lookup");
                }), { param: "" });
            return city_op;
        }
    }

    var colModel = [
     { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
     { name: 'StsCd', title: 'Status Code', index: 'StsCd', editoptions: gridLookup(getStatus("StsCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: false },
     { name: 'StsDescp', title: 'Description', index: 'StsDescp', sorttype: 'string', width: 200, hidden: false, editable: false },
     { name: 'Location', title: 'Location', index: 'Location', editoptions: gridLookup(getcityop("Location")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
     { name: 'LocationDescp', title: 'Location Name', index: 'LocationDescp', sorttype: 'string', width: 120, hidden: false, editable: true },
     {
         name: 'EvenDate', title: 'Event Date', index: 'EvenDate', sorttype: 'string',
         hidden: false, editable: true, 
         formatter: 'date', formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d H:i', defaultValue: "" }, editoptions: myEditDateTimeInit,
         width: 140, hidden: false
     },
     { name: 'Remark', title: 'Remark', index: 'Remark', sorttype: 'string', width: 400, hidden: false, editable: true }];

    _handler.intiGrid("SubGrid", $SubGrid, {
        colModel: colModel, caption: '@Resources.Locale.L_ProSts_EntCSts', delKey: ["UId"], cellEdit: true, addRow: false
    });

    _handler.intiGrid("SubGrid1", $SubGrid1, {
        colModel: colModel, caption: '@Resources.Locale.L_ProSts_OwnCSts', delKey: ["UId"], cellEdit: true, addRow: false
    });


    _initUI(["MBAdd", "MBDel", "MBCopy", "MBApply", "MBApprove", "MBInvalid"]);//初始化UI工具栏

    MenuBarFuncArr.DelMenu(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBApprove", "MBInvalid", "MBErrMsg", "MBEdoc"]);
    getSelectOptions();
    if (!isEmpty(_evenNo) || !isEmpty(_blNo)){
        MenuBarFuncArr.MBCancel();
    }
});

function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("Shipment") },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var options = data.TKLC || [];
            appendSelectOption($("#Cstatus"), options);
            options = data.TNT || [];
            appendSelectOption($("#TranType"), options);
            
            if (_handler.topData) {
                $("#Cstatus").val(_handler.topData["Cstatus"]);
                $("#NotifyFormat").val(_handler.topData["TranType"]);
            }
        }
    });
}