var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $MainGrid = null;
$(function(){
	$MainGrid = $("#MainGrid");
	_handler.saveUrl = rootPath + "BSLIGHT/UpdateData";
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
		setFieldValue([data]);
	}


	MenuBarFuncArr.EndFunc = function(){

	}

	_handler.saveData = function (dtd) {
		var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
		var changeData = getChangeValue();//获取所有改变的值
		changeData["sub"] = containerArray;
		var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
		data["u_id"] = encodeURIComponent($("#UId").val());
		data["CustCd"] = encodeURIComponent($("#CustCd").val());

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
		console.log(data);
		if (data["main"])
			_handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
		else
			_handler.topData = [{}];

		if (data["sub"])
			_handler.loadGridData("MainGrid", $MainGrid[0], data["sub"], [""]);
		else
			_handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);

		setFieldValue(data["main"] || [{}]);
		_handler.beforLoadView();
		setdisabled(true);
		setToolBtnDisabled(true);
		MenuBarFuncArr.Enabled(["MBCopy"]);
		
	}

	_handler.loadMainData = function (map) {
		if (!map || !map[_handler.key]) {
			setFieldValue([{}]);
			return;
		}
		ajaxHttp(rootPath + "BSLIGHT/GetDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
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

		gridEditableCtrl({ editable: true, gridId: "MainGrid" });

		var obj = _dm.getDs("MainGrid")._data;
		_handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);

		for (var i = 0; i < obj.length; i++) {
			dataRow = {
				'UId': '',
				'UFid': '',
				'Io': obj[i]['Io'],
				'TranType': obj[i]['TranType'],
				'L1': obj[i]['L1'],
				'L2': obj[i]['L2'],
				'L3': obj[i]['L3'],
				'L4': obj[i]['L4'],
				'L5': obj[i]['L5'],
				'L6': obj[i]['L6'],
				'L7': obj[i]['L7'],
				'L8': obj[i]['L8'],
				'L9': obj[i]['L9'],
				'L10': obj[i]['L10']
			}
			$("#MainGrid").jqGrid("addRowData", undefined, dataRow, "last");
		}
		
	}



	_initUI(["MBApply", "MBApprove", "MBErrMsg", "MBSearch", "MBEdoc"]);//初始化UI工具栏
	if (!isEmpty(_uid)) {
		_handler.topData = { UId: _uid };

		_handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
		MenuBarFuncArr.MBCancel();
	}

	registBtnLookup(
		$("#CustCdLookup"), 
		{
			item: "#CustCd", 
			url: rootPath + LookUpConfig.PartyNoUrl, 
			config: LookUpConfig.PartyNoLookup, 
			param: "", 
			selectRowFn: function (map) {
				$("#CustCd").val(map.PartyNo);
				$("#CustNm").val(map.PartyName);
			}
		}, 
		{}, 
		LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
			$("#CustCd").val(rd.PARTY_NO);
			$("#CustNm").val(rd.PARTY_NAME);
		})
	);

	genMainGrid();
});

function genMainGrid()
{
	var ColModel = [
		{ name: 'UId', title: 'null', index: 'UId', sorttype: 'string', editable: false, hidden: true },
		{ name: 'UFid', title: 'null', index: 'UFid', sorttype: 'string', editable: false, hidden: true },
		{ name: 'Io', title: 'null', index: 'Io', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'O:Outbound;I:Inbound', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'TranType', title: 'null', index: 'TranType', sorttype: 'string', hidden: false, editable: true, formatter: "select", edittype: 'select', editoptions: { value: TranSel, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'L1', title: 'null', index: 'L1', sorttype: 'string', width: 150, hidden: false, editoptions: { maxlength: 20 }, editable: true, formatter: "select", edittype: 'select', editoptions: { value: EdtSel, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'L2', title: 'null', index: 'L2', sorttype: 'string', width: 150, hidden: false, editoptions: { maxlength: 20 }, editable: true, formatter: "select", edittype: 'select', editoptions: { value: EdtSel, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'L3', title: 'null', index: 'L3', sorttype: 'string', width: 150, hidden: false, editoptions: { maxlength: 20 }, editable: true, formatter: "select", edittype: 'select', editoptions: { value: EdtSel, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'L4', title: 'null', index: 'L4', sorttype: 'string', width: 150, hidden: false, editoptions: { maxlength: 20 }, editable: true, formatter: "select", edittype: 'select', editoptions: { value: EdtSel, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'L5', title: 'null', index: 'L5', sorttype: 'string', width: 150, hidden: false, editoptions: { maxlength: 20 }, editable: true, formatter: "select", edittype: 'select', editoptions: { value: EdtSel, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'L6', title: 'null', index: 'L6', sorttype: 'string', width: 150, hidden: false, editoptions: { maxlength: 20 }, editable: true, formatter: "select", edittype: 'select', editoptions: { value: EdtSel, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'L7', title: 'null', index: 'L7', sorttype: 'string', width: 150, hidden: false, editoptions: { maxlength: 20 }, editable: true, formatter: "select", edittype: 'select', editoptions: { value: EdtSel, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'L8', title: 'null', index: 'L8', sorttype: 'string', width: 150, hidden: false, editoptions: { maxlength: 20 }, editable: true, formatter: "select", edittype: 'select', editoptions: { value: EdtSel, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'L9', title: 'null', index: 'L9', sorttype: 'string', width: 150, hidden: false, editoptions: { maxlength: 20 }, editable: true, formatter: "select", edittype: 'select', editoptions: { value: EdtSel, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'L10', title: 'null', index:'L10', sorttype: 'string', width: 150, hidden: false, editoptions: { maxlength: 20 }, editable: true, formatter: "select", edittype: 'select', editoptions: { value: EdtSel, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
	];

	for (var i = 0; i < ColModel.length; i++) {
		ColModel[i]['title'] = ColModelLang[i];
	}

	_handler.intiGrid("MainGrid", $MainGrid, 
	{
		colModel: ColModel, 
		caption: _getLangCaption("SetLight","灯号设置"), 
		delKey: ["UId"],
		onAddRowFunc: function (rowid) {

		},
		beforeSelectRowFunc: function (rowid) {

		},
		onAddRowFunc: function (rowid) {

		},
		beforeAddRowFunc: function (rowid) {

		}
	});
}

function _getLangCaption(langId, caption) {
    try {
        return GetLangCaption(langId,caption)
    } catch (e) {
        return caption || langId;
    }
}