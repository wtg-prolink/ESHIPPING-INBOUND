var $MainGrid;
function initQTGrid() {
	$MainGrid = $("#MainGrid");
	function get_cityop(name) {
		var _name = name;
		var city_op = getLookupOp("MainGrid",
			{
				url: rootPath + LookUpConfig.CityPortUrl,
				config: LookUpConfig.CityPortLookup,
				returnFn: function (map, $grid) {
					var selRowId = $grid.jqGrid('getGridParam', 'selrow');
					//$grid.jqGrid('setCell', selRowId, _name, map.CntyCd + map.CityCd);
					if ("PodCd" === _name)
						setGridVal($grid, selRowId, "PodNm", map.PortNm, null);
					else if ("PolCd" === _name)
						setGridVal($grid, selRowId, "PolNm", map.PortNm, null);
					setGridVal($grid, selRowId, _name, map.CntryCd + map.PortCd, "lookup");
					return map.CntryCd + map.PortCd;
				}
			}, LookUpConfig.GetCityPortAuto(groupId, $MainGrid,
			function ($grid, rd, elem, selRowId) {
				if ("PodCd" === _name)
					setGridVal($grid, selRowId, "PodNm", rd.PORT_NM, null);
				else if ("PolCd" === _name)
					setGridVal($grid, selRowId, "PolNm", rd.PORT_NM, null);
				$(elem).val(rd.CNTRY_CD + rd.PORT_CD);
				setGridVal($grid, selRowId, _name, rd.CNTRY_CD + rd.PORT_CD, "lookup");
			}));
		return city_op;
	}

	function get_cntryop(name) {
		var _name = name;
		var cntry_op = getLookupOp("MainGrid",
			{
				url: rootPath + LookUpConfig.CountryUrl,
				config: LookUpConfig.CountryLookup,
				returnFn: function (map, $grid) {
					//var selRowId = $grid.jqGrid('getGridParam', 'selrow');
					//$grid.jqGrid('setCell', selRowId, _name, map.CntyCd + map.CityCd);
					return map.CntryCd;
				}
			}, LookUpConfig.GetCountryAuto(groupId, $MainGrid,
			function ($grid, rd, elem) {
				$(elem).val(rd.CNTRY_CD);
			}), { param: "" });
		return cntry_op;
	}

	function getcust(name) {
		var _name = name;
		var unit_op = getLookupOp("MainGrid",
			{
				url: rootPath + LookUpConfig.TCARUrl,
				config: LookUpConfig.BSCodeLookup,
				returnFn: function (map, $grid) {  
					return map.Cd;
				}
			}, LookUpConfig.GetCodeTypeAuto(groupId, "TCAR", $MainGrid,
			function ($grid, rd, elem, rowid) {
				setGridVal($grid, rowid, _name, rd.CD, "lookup");
				$(elem).val(rd.CD);
			}));
		return unit_op;
	}

	//function getcust(name) {
	//    var _name = name;
	//    var cust_op = getLookupOp("MainGrid",
	//        {
	//            url: rootPath + LookUpConfig.PartyNoUrl,
	//            config: LookUpConfig.PartyNoLookup,
	//            returnFn: function (map, $grid) {
	//                return map.PartyNo;
	//            }
	//        }, LookUpConfig.GetPartyTypeNoAuto(groupId, undefined, $MainGrid,
	//        function ($grid, rd, elem, rowid) {
	//            setGridVal($grid, rowid, _name, rd.PARTY_NO,"lookup");
	//            $(elem).val(rd.PARTY_NO);
	//        }), {
	//            param: ""
	//            //baseConditionFunc: function () {
	//            //    return "PARTY_TYPE='CA'";
	//            //}
	//        });
	//    return cust_op;
	//}

	function getTermop(name) {
		var _name = name;
		var term_op = getLookupOp("MainGrid",
			{
				url: rootPath + LookUpConfig.TermUrl,
				config: LookUpConfig.TermLookup,
				returnFn: function (map, $grid) {
					return map.Cd;
				}
			}, LookUpConfig.GetCodeTypeAuto(groupId, "TD", $MainGrid,
			function ($grid, rd, elem) {
				$(elem).val(rd.CD);
			}), { param: "" });
		return term_op;
	}

	function get_regionop(name) {
		var _name = name;
		var region_op = getLookupOp("MainGrid",
			{
				url: rootPath + LookUpConfig.TrgnUrl,
				config: LookUpConfig.TrgnLookup,
				returnFn: function (map, $grid) {
					//var selRowId = $grid.jqGrid('getGridParam', 'selrow');
					//$grid.jqGrid('setCell', selRowId, _name, map.CntyCd + map.CityCd);
					return map.Cd;
				}
			}, LookUpConfig.GetCodeTypeAuto(groupId, "TRGN", $MainGrid,
			function ($grid, rd, elem,rowid) {
				$(elem).val(rd.CD);
			}), { param: "" });
		return region_op;
	}

	function getcur(name) {
		var _name = name;
		var cur_op = getLookupOp("MainGrid",
			{
				url: rootPath + LookUpConfig.CurUrl,
				config: LookUpConfig.CurLookup,
				returnFn: function (map, $grid) {
					var selRowId = $grid.jqGrid('getGridParam', 'selrow');
					setGridVal($grid, selRowId, "Cur", map.Cur, "lookup");
					return map.Cur;
				}
			}, LookUpConfig.GetCurAuto(groupId,undefined, $MainGrid,
			function ($grid, rd, elem, rowid) {
				$(elem).val(rd.CUR);
				setGridVal($grid, rowid, "Cur", rd.CUR, "lookup");
			}), { param: "" });
		return cur_op;
	}

	var colModel = [
		{ name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: true, hidden: true },
		{ name: 'RfqNo', title: '@Resources.Locale.L_RQQuery_RfqNo', index: 'RfqNo', sorttype: 'string', width: 100, editable: true, hidden: true },
		{ name: 'AllIn', title: '@Resources.Locale.L_AirQuery_AllIn', index: 'AllIn', sorttype: 'string', width: 100, editable: true, hidden: true },
		{ name: 'TranMode', title: '@Resources.Locale.L_RQQuery_TranMode', index: 'TranMode', sorttype: 'string', width: 100, editable: true, hidden: true },
		{ name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', sorttype: 'string', width: 100, editable: true, hidden: true },
		{ name: 'Region', title: '@Resources.Locale.L_BaseLookup_Region', index: 'Region', sorttype: 'string', edittype: 'custom', editoptions: gridLookup(get_regionop("Region")), width: 80, hidden: false, editable: true },
		{ name: 'PolCd', title: '@Resources.Locale.L_BaseLookup_PolCd', index: 'PolCd', sorttype: 'string', edittype: 'custom', editoptions: gridLookup(get_cityop("PolCd")), width: 80, hidden: false, editable: true },
		{ name: 'PolNm', title: '@Resources.Locale.L_BaseLookup_PolName', index: 'PolNm', sorttype: 'string', width: 120, hidden: false, editable: false },
		{ name: 'ViaCd', title: 'Via', index: 'ViaCd', sorttype: 'string', width: 80, sorttype: 'string', width: 120, hidden: false, editable: false },
		//{ name: 'ViaCd', title: 'Via', index: 'ViaCd', sorttype: 'string', width: 80, hidden: false, editable: true, formatter: "select", editoptions: { value: ':;MLB:MLB;AWR:AWR;NORTH:NORTH;WEST:WEST;SOUTH:SOUTH;New port:New port;Catlai:Catlai;VICT:VICT' } },
		{ name: 'PodCd', title: '@Resources.Locale.L_BaseLookup_DestCd', index: 'PodCd', sorttype: 'string', edittype: 'custom', editoptions: gridLookup(get_cityop("PodCd")), width: 80, hidden: false, editable: true },
		{ name: 'PodNm', title: '@Resources.Locale.L_BaseLookup_DestName', index: 'PodNm', sorttype: 'string', width: 120, hidden: false, editable: false },
		{ name: 'Carrier', title: '@Resources.Locale.L_DNApproveManage_CaCd', index: 'Carrier', edittype: 'custom', editoptions: gridLookup(getcust("Carrier")), sorttype: 'string', width: 90, hidden: false, editable: true },
		{ name: 'LoadingFrom', title: 'Loading From', index: 'LoadingFrom', sorttype: 'string', width: 100, hidden: false, editable: false },
		{ name: 'LoadingTo', title: 'Loading To', index: 'LoadingTo', sorttype: 'string', width: 100, hidden: false, editable: false },
		{ name: 'ContractNo', title: '@Resources.Locale.L_QtManage_ContractNo', index: 'ContractNo', sorttype: 'string', width: 100, hidden: false, editable: true },
		//{ name: 'ServiceMode', title: '@Resources.Locale.L_RQQuery_ServiceMode', index: 'ServiceMode', edittype: 'custom', editoptions: gridLookup(getTermop("ServiceMode")), sorttype: 'string', width: 100, hidden: false, editable: false },
		{ name: 'Cur', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Cur', editoptions: gridLookup(getcur("Cur")), edittype: 'custom', sorttype: 'string', width: 90, hidden: false, editable: false },
		{ name: 'F2', title: "@Resources.Locale.L_SFCLQuery_F2", index: 'F2', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
		{ name: 'F3', title: "@Resources.Locale.L_SFCLQuery_F3", index: 'F3', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
		{ name: 'F4', title: "@Resources.Locale.L_SFCLQuery_F4", index: 'F4', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
		{ name: 'F12', title: "@Resources.Locale.L_SFCLQuery_F12", index: 'F12', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
		{ name: 'F13', title: "@Resources.Locale.L_SFCLQuery_F13", index: 'F13', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
		{ name: 'F14', title: "@Resources.Locale.L_SFCLQuery_F14", index: 'F14', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
		{ name: 'SailingDay', title: '@Resources.Locale.L_SFCLQuery_SailingDay', index: 'SailingDay', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'FreeOdt', title: '@Resources.Locale.L_SFCLQuery_FreeOdt', index: 'FreeOdt', width: 200, align: 'right', formatter: 'integer', hidden: false, editable: true },
		{ name: 'FreeOdm', title: '@Resources.Locale.L_SFCLQuery_FreeOdm', index: 'FreeOdm', width: 200, align: 'right', formatter: 'integer', hidden: false, editable: true },
		{ name: 'FreeDdt', title: '@Resources.Locale.L_SFCLQuery_FreeDdt', index: 'FreeDdt', width: 200, align: 'right', formatter: 'integer', hidden: false, editable: true },
		{ name: 'FreeDdm', title: '@Resources.Locale.L_SFCLQuery_FreeDdm', index: 'FreeDdm', width: 200, align: 'right', formatter: 'integer', hidden: false, editable: true },
		{ name: 'Tt', title: '@Resources.Locale.L_RouteSetup_Tt', index: 'Tt', width: 60, align: 'right', formatter: 'integer', hidden: false, editable: true },
		{ name: 'ViaNm', title: '@Resources.Locale.L_SFCLQuery_ViaNm', index: 'ViaNm', sorttype: 'string', width: 200, hidden: false, editable: true, editable: true },
		{ name: 'Note', title: '@Resources.Locale.L_SFCLQuery_Note', index: 'Note', sorttype: 'string', width: 100, hidden: false, editable: true, editable: true },
		{ name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', sorttype: 'string', width: 200, hidden: false, editable: true, editable: true, editoptions: { size: 500, maxlength: 500 } },
		{ name: 'SeqNo', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, editable: false },
	   { name: 'QuotNo', title: '@Resources.Locale.L_QTQuery_QuotNo', index: 'QuotNo', sorttype: 'string', width: 100, editable: false, hidden: true }
	];

	colModel = SetColModel(FMOption, colModel, 18);
	_handler.intiGrid("MainGrid", $MainGrid, {
		colModel: colModel, caption: 'FCL', delKey: ["UId"], sortorder: "asc", sortname: "CreateDate", height: 200,
		onAddRowFunc: function (rowid) {
			var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
			if (typeof maxSeqNo === "undefined")
				maxSeqNo = 0;
			setGridVal($MainGrid, rowid, "SeqNo", maxSeqNo + 1);
			setDefutltGridData($MainGrid, rowid, { "PodCd": true, "PolCd": true, "Incoterm": true });
			setGridVal($MainGrid, rowid, "ChgCd", "FRT");
		},
		beforeSelectRowFunc: function (rowid) {
			//main key 修改時不允與修改
			$MainGrid.setColProp('Cur', { editable: true });
		},
		beforeAddRowFunc: function (rowid) {
			//add row 時要可以編輯main key
			//$SubGrid.setColProp('DocType', { editable: true });
		},
		afterSaveCellFunc: function (rowid) {
			var contractNo = $("#MainGrid").jqGrid('getCell', rowid, "ContractNo");
			$('#MainGrid').jqGrid('setCell', rowid, "ContractNo", $.trim(contractNo));//合约号去除空格
		}
	});
}

$(function () {
	$('#FreightTermSelect').multiselect({
		enableFiltering: true,
		filterPlaceholder: 'Search for something...',
		maxWidth: '400px',
		buttonWidth: '100%'
	});
	$("#FreightTermSelect").multiselect("destroy");

	$('#OutInSelect').multiselect({
		enableFiltering: true,
		filterPlaceholder: 'Search for something...',
		maxWidth: '400px',
		buttonWidth: '100%'
	});
	$("#OutInSelect").multiselect("destroy");

	intQtView();
	initQTGrid();
	_handler.beforSave = function () {
		SetFrtOutIn();
		var nullCols = [], sameCols = [];
		nullCols.push({ name: "Region", index: 6, text: 'Region' });
		nullCols.push({ name: "PolCd", index: 7, text: 'POL' });
		nullCols.push({ name: "PodCd", index: 9, text: 'POD' });
	   
		return _handler.checkData($MainGrid, nullCols, sameCols);
	}
	function SetFrtOutIn() {
		var FreightTerm = "";
		$('#FreightTermSelect :selected').each(function (i, selected) {
			if (FreightTerm != "") FreightTerm += ";";
			FreightTerm += $(selected).val();
		});
		$("#FreightTerm").val(FreightTerm);

		var OutIn = "";
		$('#OutInSelect :selected').each(function (i, selected) {
			if (OutIn != "") OutIn += ";";
			OutIn += $(selected).val();
		});
		$("#OutIn").val(OutIn);
	}

	_handler.saveData = function (dtd) {
		var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
		var changeData = getChangeValue();//获取所有改变的值
		changeData["sub"] = containerArray;
		var tranMode = $("#TranMode").val();
		if ("F" == tranMode && changeData["mt"] != undefined) {
			for (var i = 0; i < changeData["mt"].length; i++) {

				{
					if (changeData["mt"][i]["FreightTerm"] != undefined) {
						var FreightTerm = "";
						$('#FreightTermSelect :selected').each(function (i, selected) {
							if (FreightTerm != "") FreightTerm += ";";
							FreightTerm += $(selected).val();
						});
						changeData["mt"][i]["FreightTerm"] = FreightTerm;
					}
					if (changeData["mt"][i]["OutIn"] != undefined) {
						var OutIn = "";
						$('#OutInSelect :selected').each(function (i, selected) {
							if (OutIn != "") OutIn += ";";
							OutIn += $(selected).val();
						});
						changeData["mt"][i]["OutIn"] = OutIn;
					}
				}
			}
		}
		var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
		data["u_id"] = encodeURIComponent($("#UId").val());
		data["rfq_no"] = encodeURIComponent($("#RfqNo").val());
		data["quot_no"] = encodeURIComponent($("#QuotNo").val());
		data["contractType"] = encodeURIComponent($("#ContractType").val());
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
		setContractHtml();
		setFieldValue(data["main"] || [{}]);
		setdisabled(true);
		setToolBtnDisabled(true);
		if (_handler.topData.Period === "B") {
			$("#EXCEL_BTN").show();
		}
		else
			$("#EXCEL_BTN").hide();
		MenuBarFuncArr.Enabled(["EXCEL_BTN"]);
		MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");


		var multiEdocData = [
			{ jobNo: _handler.topData["RqUid"], 'GROUP_ID': _handler.topData["RqGroupid"], 'CMP': _handler.topData["RqCmp"], 'STN': '*' }
		]; 
		MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*", multiEdocData);

		MenuBarFuncArr.Enabled(["MBEdoc"]);
		MenuBarFuncArr.Enabled(["QuotTypeBtn"]);
		MenuBarFuncArr.Enabled(["VoidBtn"]);
		MenuBarFuncArr.Enabled(["BACK_BTN"]);
		setRQData(data);

		if (data["main"][0].FreightTerm != null) {
			var FreightTerm = data["main"][0].FreightTerm;
			var dataarray = FreightTerm.split(";");
			$("#FreightTermSelect").val(dataarray);
			$("#FreightTermSelect").selectpicker('refresh');
		}
		if (data["main"][0].OutIn != null) {
			var OutIn = data["main"][0].OutIn;
			var dataarray = OutIn.split(";");
			$("#OutInSelect").val(dataarray);
			$("#OutInSelect").selectpicker('refresh');
		}
		_handler.beforLoadView();
	} 

	loadQtView();
	MenuBarFuncArr.Enabled(["MBEdoc"]);
});



