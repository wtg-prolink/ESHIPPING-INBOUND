var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };

$(function(){

	_handler.saveUrl = rootPath + "SMAL/UpdateData";
	_handler.inquiryUrl = rootPath + "";
	_handler.config = [];

	_handler.beforDel = function () {
		if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
			CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
			return false;
		}
	}

	_handler.addData = function () {
		//初始化新增数据
		var data = {};
		data[_handler.key] = uuid();
		data["IncotermDescp"] = "";
		setFieldValue([data]);
		$("#Cmp").val(cmp);
	}


	MenuBarFuncArr.EndFunc = function(){

	}

	_handler.beforSave = function () {
	    var incotermcd = $("#IncotermCd").val();
	    if (!(incotermcd == "" || incotermcd == null || incotermcd == undefined)) {
	        var descp = $("#IncotermDescp").val();
	        if (descp == "" || descp == null || descp == undefined) {
	            alert("Incoterm Descp is null!");
	            return false;
	        }
	    }
	}


	_handler.saveData = function (dtd) {
		var changeData = getChangeValue();//获取所有改变的值
		var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
		data["u_id"] = encodeURIComponent($("#UId").val());
		//data["rfq_no"] = encodeURIComponent($("#RfqNo").val());
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

	_handler.beforEdit = function () {
		if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
			CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
			return false;
		}
	}

	_handler.beforAdd = function () {//新增前设定
	}

	_handler.setFormData = function (data) {
		if (data["main"])
			_handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
		else
			_handler.topData = [{}];

		setFieldValue(data["main"] || [{}]);
		_handler.beforLoadView();
		setdisabled(true);
		setToolBtnDisabled(true);
		MenuBarFuncArr.Enabled(["MBCopy"]);
		if(data["main"].length > 0)
		{
			var status = data["main"][0]["Status"];
			if(status === "V")
			{
				MenuBarFuncArr.Disabled(["MBEdit", "MBDel"]);
			}
		}
	}

	_handler.loadMainData = function (map) {
		if (!map || !map[_handler.key]) {
			setFieldValue([{}]);
			return;
		}
		ajaxHttp(rootPath + "SMAL/GetDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
			function (data) {
				if (_handler.setFormData)
					_handler.setFormData(data);
		});
	}

	MenuBarFuncArr.MBCopy = function (thisItem) {
		//初始化新增数据
		var data = {};
		data[_handler.key] = uuid();       
		var dataRow, addData = [];
	}



	_initUI(["MBApply", "MBApprove", "MBErrMsg", "MBSearch", "MBEdoc"]);//初始化UI工具栏
	if (!isEmpty(_uid)) {
		_handler.topData = { UId: _uid };
		MenuBarFuncArr.MBCancel();
	}

	
	registBtnLookup($("#CmpLookup"), {
		item: '#Cmp', url: rootPath + LookUpConfig.GetCmpUrl, config: LookUpConfig.CmpLookup, param: "", selectRowFn: function (map) {
			$("#Cmp").val(map.Cmp);
		}
	}, undefined, LookUpConfig.GetCmpAuto(groupId, undefined, function ($grid, rd, elem) {
		$("#Cmp").val(rd.CMP);
	}));

	registBtnLookup($("#ConnCdLookup"), {
		item: '#ConnCd', 
		url: rootPath + LookUpConfig.PartyNoUrl, 
		config: LookUpConfig.PartyNoLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#ConnCd").val(map.PartyNo);
			$("#ConnNm").val(map.PartyName);
		}
	}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, 
		function ($grid, rd, elem) {
			$("#ConnCd").val(rd.PARTY_NO);
			$("#ConnNm").val(rd.PARTY_NAME);
		}, 
		function ($grid, elem) {
			$("#ConnNm").val("");
		}
	));

	registBtnLookup($("#WeCdLookup"), {
		item: '#WeCd',
		url: rootPath + LookUpConfig.PartyNoUrl,
		config: LookUpConfig.PartyNoLookup,
		param: "",
		selectRowFn: function (map) {
			$("#WeCd").val(map.PartyNo);
			$("#WeNm").val(map.PartyName);
		}
	}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined,
		function ($grid, rd, elem) {
			$("#WeCd").val(rd.PARTY_NO);
			$("#WeNm").val(rd.PARTY_NAME);
		},
		function ($grid, elem) {
			$("#WeNm").val("");
		}
	));

	registBtnLookup($("#PodCdLookup"), {
		item: '#PodCd', 
		url: rootPath + LookUpConfig.CityAndTruckPortUrl,
		config: LookUpConfig.CityPortLookup2,
		param: "", 
		selectRowFn: function (map) {
			$("#PodCd").val(map.Pod)
			$("#PodNm").val(map.PortNm);
		}
	}, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, 
		function ($grid, rd, elem) {
			$("#PodCd").val(rd.POD);
			$("#PodNm").val(rd.PORT_NM);
		}, 
		function ($grid, elem) {
			$("#PodNm").val("");
		}
	));

	registBtnLookup($("#LspCdLookup"), {
		item: '#LspCd', 
		url: rootPath + LookUpConfig.PartyNoUrl, 
		config: LookUpConfig.PartyNoLookup, 
		param: "", 
		selectRowFn: function (map) {
			$("#LspCd").val(map.PartyNo);
			$("#LspNm").val(map.PartyName);
		}
	}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, 
		function ($grid, rd, elem) {
			$("#LspCd").val(rd.PARTY_NO);
			$("#LspNm").val(rd.PARTY_NAME);
		}, 
		function ($grid, elem) {
			$("#LspNm").val("");
		}
	));

	registBtnLookup($("#IncotermCdLookup"), {
			item: '#IncotermCd', 
			url: rootPath + LookUpConfig.TermUrl, 
			config: LookUpConfig.TermLookup, 
			param: "", 
			selectRowFn: function (map) {
				$("#IncotermCd").val(map.Cd);
			}
		}, 
		undefined, 
		LookUpConfig.GetCodeTypeAuto(groupId, "TINC", undefined, 
			function ($grid, rd, elem) {
			$("#IncotermCd").val(rd.CD);
			}, 
			function ($grid, elem) {
				$("#IncotermCd").val("");
			}
	));

	registBtnLookup($("#DlvAreaLookup"), {
		item: '#DlvAreaNm',
		url: rootPath + LookUpConfig.TruckPortCdUrl,
		config: LookUpConfig.TruckPortCdLookup,
		param: "",
		selectRowFn: function (map) {
			$("#DlvArea").val(map.PortCd);
			$("#DlvAreaNm").val(map.PortNm);
		}
	}, {
	    basecondition: function () {
	        var Cmp = $("#Cmp").val();
	        return " CMP='" + Cmp + "'";
	    }
	}, LookUpConfig.TruckPortCdAuto(groupId, undefined,
		function ($grid, rd, elem) {
			$("#DlvArea").val(rd.PORT_CD);
			$("#DlvAreaNm").val(rd.PORT_NM);
		}
	));

	registBtnLookup($("#CarrierLookup"), {
		item: '#Carrier',
		url: rootPath + LookUpConfig.TCARUrl,
		config: LookUpConfig.BSCodeLookup,
		param: "",
		selectRowFn: function (map) {
			$("#Carrier").val(map.Cd); 
		}
	}, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TCAR", undefined,
		function ($grid, rd, elem) {
			$("#Carrier").val(rd.CD); 
		},
		function ($grid, elem) {
			$("#Carrier").val(""); 
		}
	));

	//$("#DlvAreaLookup").v3Lookup({
	//    url: rootPath + 'TPVCommon/GetPortData',
	//    focusItem: $("#DlvAreaLookup"),
	//    baseConditionFunc: function () {
	//        var Cmp = $("#Cmp").val();
	//        return " CMP ='" + Cmp + "'";
	//    },
	//    gridFunc: function (map) {
	//        $("#DlvArea").val(map.PortCd);
	//        $("#DlvAreaNm").val(map.PortNm);
	//    },
	//    lookUpConfig: LookUpConfig.TruckPortCdLookup
	//});

	$("#AddrCodeLookup").v3Lookup({
		url: rootPath + 'TPVCommon/GetBsaddrForLookup',
		focusItem: $("#AddrCodeLookup"),
		baseConditionFunc: function(){
		    var DlvArea = $("#DlvArea").val();
		    var Cmp = $("#Cmp").val();
			console.log(DlvArea);
			return " PORT_CD='" + DlvArea + "' AND CMP ='" + Cmp + "'";
		},
		gridFunc: function(map){
			$("#DlvAddr").val(map.Addr);
            $("#AddrCode").val(map.AddrCode);
            $("#WsCd").val(map.WsCd);
            $("#WsNm").val(map.WsNm);
            $("#FinalWh").val(map.FinalWh);
		},
		lookUpConfig: LookUpConfig.TruckPortAddrLookup
	});

	var PartyTypeOptions = {};
	PartyTypeOptions.gridUrl = rootPath + "Common/GetPartyTypeData";
	PartyTypeOptions.registerBtn = $("#PartyTypeLookup");
	PartyTypeOptions.isMutiSel = true;
	PartyTypeOptions.focusItem = $("#PartyType");
	//mutil select add a columnID help mapping selected data
	PartyTypeOptions.columnID = "Cd";
	PartyTypeOptions.param = "";
	PartyTypeOptions.gridFunc = function (map) {
	}
	PartyTypeOptions.responseMethod = function (data) {
	    console.log(data);
	    var str = "";
	    $.each(data, function(index, val) {
	        str = str + data[index].Cd + ";";
		});
		str = str.replace(/;$/, '');
	    $("#PartyType").val(str);
	}
	PartyTypeOptions.lookUpConfig = LookUpConfig.MutiPartyTypeLookup;
	initLookUp(PartyTypeOptions);

	setBscData("TerminalCdLookup", "TerminalCd", "TerminalNm", "TMN")


	function setBscData(lookUp, Cd, Nm, pType)
	{
	    //SMPTY放大鏡
	    options = {};
	    options.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
	    options.registerBtn = $("#"+lookUp);
	    options.focusItem = $("#" + Cd);
	    options.baseCondition = " GROUP_ID='"+groupId+"' AND CMP='"+cmp+"' AND CD_TYPE='"+pType+"'";
	    options.baseConditionFunc = function(){
	    	return " AND AP_CD='"+$("#PodCd").val()+"'";
	    }
	    options.isMutiSel = true;
	    options.gridFunc = function (map) {
	        $("#" + Cd).val(map.Cd);

	        if(Nm != "")
	            $("#" + Nm).val(map.CdDescp);
	    }

	    options.lookUpConfig = LookUpConfig.BSCodeLookup;
	    initLookUp(options);

	    CommonFunc.oAutoComplete("#"+Cd, 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE~"+pType+"&CD=", "CD=showValue,CD,CD_DESCP", function (event, ui) {
	        $(this).val(ui.item.returnValue.CD);

	        if(Nm != "")
	            $("#" + Nm).val(ui.item.returnValue.CD_DESCP);

	        return false;
	    });
	}
});