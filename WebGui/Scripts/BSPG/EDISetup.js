var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $MainGrid;
function initQTGrid() {
    $MainGrid = $("#MainGrid");
    var colModel = [
        { name: 'EdiId', title: 'EDI ID', index: 'EdiId', sorttype: 'string', width: 100, viewable: true, hidden: false },
        { name: 'EdiNm', title: 'EDI Name', index: 'EdiNm', sorttype: 'string', width: 120, viewable: true, hidden: false },
        { name: 'FuncCode', title: '@Resources.Locale.L_EDISetup_Name', index: 'FuncCode', sorttype: 'string', width: 200, viewable: true, hidden: false },
        {
            name: 'EdiModel', title: '@Resources.Locale.L_EDISetup_Script_EDI79', index: 'EdiModel', sorttype: 'string', width: 100, viewable: true, hidden: false,
        	formatter: "select", 
        	edittype: 'select', 
        	editoptions: { value: '@Resources.Locale.L_EDISetup_Script_80', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } }
        },
        { name: 'SourceType', title: '@Resources.Locale.L_EDIDetailSetupView_Use', index: 'SourceType', sorttype: 'string', width: 100, viewable: true, hidden: false,
        	formatter: "select", 
        	edittype: 'select', 
        	editoptions: { value: 'F:FTP;S:Web Service;M:Mail;L:Local file;C:Crewler site', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } }
        },
        { name: 'TriggerType', title: '@Resources.Locale.L_EDISetup_StartPer', index: 'TriggerType', sorttype: 'string', width: 100, viewable: true, hidden: false,
        	formatter: "select", 
        	edittype: 'select', 
        	editoptions: { value: 'MINUTE:MINUTE;HOUR:HOUR;WEEK:WEEK;DAY:DAY;MONTH:MONTH;YEAR:YEAR', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } }
        },
        { name: 'CycleData', title: '@Resources.Locale.L_EDISetup_Scripts_96', index: 'CycleData', sorttype: 'string', width: 200, viewable: true, hidden: false },
        { name: 'ServicePath', title: '@Resources.Locale.L_EDIDetailSetupView_SevAddr', index: 'ServicePath', sorttype: 'string', width: 200, viewable: true, hidden: false },
        { name: 'SourcePath', title: '@Resources.Locale.L_EDISetup_Scripts_97', index: 'SourcePath', sorttype: 'string', width: 200, viewable: true, hidden: false },
        { name: 'SourceAccount', title: '@Resources.Locale.L_EDISetup_Scripts_98', index: 'SourceAccount', sorttype: 'string', width: 100, viewable: true, hidden: false },
        { name: 'SourcePwd', title: '@Resources.Locale.L_EDISetup_Scripts_99', index: 'SourcePwd', sorttype: 'string', width: 100, viewable: true, hidden: false },
        { name: 'SourceFormat', title: '@Resources.Locale.L_EDISetup_Scripts_100', index: 'SourceFormat', sorttype: 'string', width: 200, viewable: true, hidden: false },
        { name: 'SourcePort', title: '@Resources.Locale.L_EDISetup_Script_81', index: 'SourcePort', sorttype: 'string', width: 100, viewable: true, hidden: false },
        { name: 'CourceFolder', title: '@Resources.Locale.L_EDISetup_Scripts_102', index: 'CourceFolder', sorttype: 'string', width: 200, viewable: true, hidden: false },
        { name: 'EdiNamespace', title: 'Namespace', index: 'EdiNamespace', sorttype: 'string', width: 200, viewable: true, hidden: false },
        { name: 'EdiProgramer', title: '@Resources.Locale.L_EDIDetailSetupView_Desiger', index: 'EdiProgramer', sorttype: 'string', width: 200, viewable: true, hidden: false },
        { name: 'GroupId', title: 'GroupId', index: 'GroupId', sorttype: 'string', width: 100, viewable: true, hidden: true },
        { name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', width: 100, viewable: true, hidden: true },
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, viewable: true, hidden: true },
        { name: 'UFid', title: 'U FID', index: 'UFid', sorttype: 'string', width: 100, viewable: true, hidden: true }
    ];


    _handler.intiGrid("MainGrid", $MainGrid, {
        colModel: colModel, caption: '@Resources.Locale.L_EDISetup_Scripts_103', delKey: ["UId"], height: 400,
        onAddRowFunc: function (rowid) {
        },
        beforeSelectRowFunc: function (rowid) {
        },
        dblClickFunc: function(map)
        {
        	var duid = map.UId;
        	top.topManager.openPage({
        	    href: rootPath + "BSPG/EDIDetialSetupView/" + duid,
        	    title: 'EDI Detail View',
        	    id: 'EDIDetialSetupView',
        	    reload: true
        	});
        },
        beforeAddRowFunc: function (rowid) {
        }
    },["UId"], false);
}
$(function(){
	initQTGrid();
	_handler.saveUrl = rootPath + "BSPG/EdiUpdateData";
	_handler.inquiryUrl = rootPath + "BSPG/EdiGetDetail";
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
	    $MainGrid.jqGrid("clearGridData");
	}


	MenuBarFuncArr.EndFunc = function(){

	}

	_handler.saveData = function (dtd) {
	    var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
	    var changeData = getChangeValue();//获取所有改变的值
	    changeData["sub"] = containerArray;
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

	    if (data["sub"])
	        _handler.loadGridData("MainGrid", $MainGrid[0], data["sub"], [""]);
	    else
	        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);

	    setFieldValue(data["main"] || [{}]);
	    _handler.beforLoadView();
	    setdisabled(true);
	    setToolBtnDisabled(true);
	    
	    MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
	    MenuBarFuncArr.Enabled(["MBEdoc"]);
	    MenuBarFuncArr.Enabled(["MBCopy"]);
	}

	_handler.loadMainData = function (map) {
	    if (!map || !map[_handler.key]) {
	        setFieldValue([{}]);
	        return;
	    }
	    ajaxHttp(rootPath + "BSPG/EdiGetDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
	        function (data) {
	            if (_handler.setFormData)
	                _handler.setFormData(data);
        });
	}

	MenuBarFuncArr.MBCopy = function (thisItem) {
	    //初始化新增数据
	    var data = {};
	    data[_handler.key] = uuid();	   
	    $("#EffectDate").val("");
	    $("#Area").val("");
	    var dataRow, addData = [];
	    $("#CreateBy").val("");
	    $("#CreateDate").val("");
	}

	_initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
	if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    MenuBarFuncArr.Enabled(["MBEdoc"]);

    $("#addDetailBtn").click(function(event) {
    	if(_uid == "")
    	{
    		alert("@Resources.Locale.L_EDISetup_MainS");
    		return;
    	}
    	top.topManager.openPage({
    	    href: rootPath + "BSPG/EDIDetialSetupView?muid=" + _uid,
    	    title: 'EDI Detail Setup',
    	    reload: true,
    	    id: 'EDIDetialSetupView'
    	});
    });

    $("#reloadBtn").click(function(event) {
    	CommonFunc.ToogleLoading(true);
    	$.post(rootPath + 'BSPG/getEdiList', {uid: _uid}, function(data, textStatus, xhr) {
    		CommonFunc.ToogleLoading(false);
    		$MainGrid.jqGrid("clearGridData");
    		$MainGrid.jqGrid("setGridParam", {
    		    datatype: 'local',
    		    data: data["sub"]
    		}).trigger("reloadGrid");
    	},"JSON");
    });

});

function setSmptyData(lookUp, Cd, Nm, pType)
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
	    $("#" + Cd).val(map.PartyNo);

	    if(Nm != "")
	    	$("#" + Nm).val(map.PartyName);
	}

	options.lookUpConfig = LookUpConfig.SmptyLookup;
	initLookUp(options);

	CommonFunc.AutoComplete("#"+Cd, 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_TYPE~"+pType+"&PARTY_NO=", "PARTY_NO=showValue,PARTY_NO,PARTY_NAME", function (event, ui) {
	    $(this).val(ui.item.returnValue.PARTY_NO);

	    if(Nm != "")
	    	$("#" + Nm).val(ui.item.returnValue.PARTY_NAME);
	    return false;
	});
}