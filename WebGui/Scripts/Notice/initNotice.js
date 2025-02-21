var _dm = new dm();
var groupId = getCookie("plv3.passport.groupid"),
    cmp = getCookie("plv3.passport.companyid"),
    stn = getCookie("plv3.passport.station"),
    userId = getCookie("plv3.passport.user");
var IsAdd = false;
var Result = false;
jQuery(document).ready(function ($) {

    var rootPath = $("#rootPath").val() + "zh-CN";
    $('#navRoot').hide();
    //CommonFunc.initField(schemas);
    $(".btn-link").remove();
   

    $('#DocRmk').multiselect({
        buttonWidth: '100%'
    });

    BindLookUp();
    BindAutoComplete();

  
   
    //getSelectOptions();

    //add data to grid
    $("#NOTICEADD").click(function () {
        subStatus = "add";
        $("#subForm")[0].reset();
        $("#Cmp").val(cmp);
        $("#Stn").val(stn);
        $("#GroupId").val(groupId);
        $("#subForm").focusFirst();
        $("#NOTICEADD").prop("disabled", true);
        $("#NOTICEEDIT").prop("disabled", true);
        $("#NOTICECOPY").prop("disabled", true);
        $("#NOTICEDEL").prop("disabled", true);
        $("#NOTICECANCEL").prop("disabled", false);
        $("#NOTICESAVES").prop("disabled", false);
        InitSubFormField(false);
        IsAdd = true;

        /*if (!checkNoAllowNullFields())
            return false;
        var changeData = getChangeValue();
        changeData.mt[0].__state = "1";
        console.log(changeData.mt);
        var a = 1;
        $.ajax({
            async: true,
            url: rootPath+"/IPCHM/CHMSetUpdateData",
            type: 'POST',
            data: { "changedData": JSON.stringify(changeData), autoReturnData: true },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (result) {
                if (result.message !== "success") {
                    CommonFunc.Notify("", "新增失败", 500, "warning");
                    return;
                }
                //將明細資料帶到grid
                var $grid = $("#containerInfoGrid"),
                    ids = $grid.jqGrid('getDataIDs'),
                    rowid = ids.length + 1;
                var dataRow = $("#subForm").serializeObject();
                console.log(dataRow);
                //$grid.jqGrid("addRowData", rowid, dataRow, "last");
                $grid.jqGrid("addRowData", undefined, dataRow, "last");
                $('#SearchArea1').collapse('show');
                $('#AddArea').collapse('hide');
                CommonFunc.Notify("", "新增成功", 500, "success");
                return false;
            }
        });*/
       
    });

    //add data to grid
    $("#NOTICESAVES").click(function () {
        if (!checkNoAllowNullFields())
            return false;
        var changeData = getChangeValue();
        if(IsAdd === true)
        {
            changeData.mt[0].__state = "1";
        }
        console.log(changeData);
        var a = 1;
        $.ajax({
            async: true,
            url: rootPath + "/NOTICE/NoticeSetUpdateData",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (result) {
                console.log(result.message);
                if (result.message !== "success") {
                    CommonFunc.Notify("", _getLang("L_BSCSSetup_SFail", "保存失败"), 500, "warning");
                    return;
                }
                /*
                //將明細資料帶到grid
                var $grid = $("#containerInfoGrid"),
                    ids = $grid.jqGrid('getDataIDs'),
                    rowid = ids.length + 1;
                //_dm.addDs("containerInfoGrid", [], ["UId"], $grid[0]);
                //$grid.setGridParam({ ds: _dm.getDs("containerInfoGrid") });
                var dataRow = $("#subForm").serializeObject();
                var id = $grid.jqGrid('getGridParam', 'selrow');
                var gridRowRecord = $grid.jqGrid('getRowData', id);
                var dirtyRecord = getChangedCell(gridRowRecord, dataRow);
                //var record = $.extend(gridRowRecord, dataRow);
                //$grid.jqGrid("setRowData", id, record, 'edited');
                //var jqColModel = $grid.jqGrid('getGridParam', 'colModel');
                var modifyFlag = false;
                ds = $grid.jqGrid("getGridParam", "ds");
                for (var key in dirtyRecord) {
                    $grid.jqGrid('setCell', id, key, dirtyRecord[key], 'edit-cell dirty-cell');
                    modifyFlag = true;

                    if (ds && ds.setCurVal) {
                        //alert(val);
                        ds.setCurIndexByKey($grid.jqGrid("getRowData", id));
                        ds.setCurVal(key, dirtyRecord[key]);
                    }
                }
                if (modifyFlag) {
                    var sleectedTr = $grid.jqGrid("getInd", id, true);
                    $(sleectedTr).addClass("edited");
                    //$grid.saveRow(id, false, 'clientArray');

                }
                $grid.jqGrid("addRowData", undefined, dataRow, "last");
                
                */
                IsAdd = false;
                $("#containerInfoGrid").jqGrid("setGridParam", {
                    datatype: 'json',
                    data: result.noticeData
                }).trigger("reloadGrid");

                $('#SearchArea1').collapse('show');
                $('#AddArea').collapse('hide');
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "保存成功"), 500, "success");
                return false;
            }
        });
 
    });

    $("#NOTICEEDIT").click(function () {
        $("#subForm").focusFirst();
        $("#NOTICEADD").prop("disabled", true);
        $("#NOTICEEDIT").prop("disabled", true);
        $("#NOTICECOPY").prop("disabled", true);
        $("#NOTICEDEL").prop("disabled", true);
        $("#NOTICECANCEL").prop("disabled", false);
        $("#NOTICESAVES").prop("disabled", false);
        InitSubFormField(false);
        $("#Id").prop("disabled", true);
    });

    $("#NOTICECOPY").click(function () {
        $("#subForm").focusFirst();
        $("#NOTICEADD").prop("disabled", true);
        $("#NOTICEEDIT").prop("disabled", true);
        $("#NOTICECOPY").prop("disabled", true);
        $("#NOTICEDEL").prop("disabled", true);
        $("#NOTICECANCEL").prop("disabled", false);
        $("#NOTICESAVES").prop("disabled", false);
        InitSubFormField(false);
        $("#Id").val("");
        $("#SysCode").val("");
        $("#ProgCode").val("");
        IsAdd = true;
    });

    $("#NOTICECANCEL").click(function(){
        $("#NOTICEADD").prop("disabled", false);
        $("#NOTICEEDIT").prop("disabled", false);
        $("#NOTICECOPY").prop("disabled", true);
        $("#NOTICEDEL").prop("disabled", true);
        $("#NOTICECANCEL").prop("disabled", true);
        $("#NOTICESAVES").prop("disabled", true);
        InitSubFormField(true);
    });

    //initLoadData(PoNo);
    
    $("#NOTICEDEL").click(function () {
        if (!confirm(_getLang("L_initNotice_CleanData", "确定要清空数据吗"))) {
            return;
        }
        StatusBarArr.nowStatus(_getLang("L_initNotice_Cleaning", "正在清空数据..."));
        var changeData = getAllKeyValue();
        changeData.mt[0].__state = "0";
        $.ajax({
            async: true,
            url: rootPath + "/NOTICE/NoticeSetUpdateData",
            type: 'POST',
            data: {
                "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true
            },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
                if (successMsg != "success")
                    return null;
                else
                    CommonFunc.Notify("", _getLang("L_MailFormatSetup_DelS", "删除成功"), 500, "success"); 
                setdisabled(false);
                setToolBtnDisabled(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_DelF", "删除失败"), 500, "warning"); 
                console.log(errMsg);
            },
            success: function (result) {
                console.log(result.ipchmData);
                //成功后将页面的数据移除，并设置页面不可编辑
                //_dataSource = result.ipchmData ;
                $("#containerInfoGrid").jqGrid("setGridParam", {
                    datatype: 'json',
                    data: result.noticeData
                }).trigger("reloadGrid");

                $('#SearchArea1').collapse('show');
                $('#AddArea').collapse('hide');
                StatusBarArr.nowStatus("");
                setFieldValue(undefined, "");
            }
        });
    });

    $("#Id").bind("blur", function () {
        var Id = $(this).val();
        if (Id)
            checkId(Id);
    });
});

function BindLookUp() {
    //ROLE
    var buoptions = {};
    buoptions.gridUrl = rootPath + "System/RoleSetInquiryData";
    buoptions.registerBtn = $("#RoleLookup");
    buoptions.focusItem = $("#Role");
    buoptions.isMutiSel = true;
    buoptions.gridFunc = function (map) {
        //获取Map中的值，然后将值传递给后台重新去抓取整个画面的url
       
    }
    buoptions.responseMethod = function (data) {
        console.log(data);
        var str = "";
        $.each(data, function (index, val) {
            str = str + data[index].Fid + ";";
        });
        $("#Role").val(str);
    }
    buoptions.lookUpConfig = LookUpConfig.NoticeRoleLookup;
    initLookUp(buoptions);
}

function BindAutoComplete() {

    //ROLE
    CommonFunc.AutoComplete("#Role", 1, "", "dt=role&GROUP_ID=" + groupId + "&Fid%", "Fid=showValue,Fid", function (event, ui) {
        $(this).val(ui.item.returnValue.Fid);
        return false;
    });
}


function initLoadData(PoNo) {
    if (!PoNo)
        return;
    var param = "sopt_PoNo=eq&PoNo=" + PoNo;
    //将获取的数据作为条件进行reload数据
    $.ajax({
        async: true,
        url: rootPath + "IPCHM/CHMSetupPoNoInquiryData",
        type: 'POST',
        data: {
            sidx: 'PoNo',
            'conditions': encodeURI(param),
            'PoNo': PoNo,
            page: 1,
            rows: 20
        },
        dataType: "json",
        beforeSend: function () {
            //getSelectOptions();
            CommonFunc.ToogleLoading(true);
        },
        "complete": function (xmlHttpRequest, successMsg) {
            if (successMsg != "success") return null;
        },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (result) {
            console.log(result);
            _dataSource = result.rows;
            setFieldValue(result.rows);
            var $grid = $("#containerInfoGrid");
            _dm.addDs("containerInfoGrid", [], ["UId"], $grid[0]);
            _dm.getDs("containerInfoGrid").setData(result.rows);
            CommonFunc.ToogleLoading(false);
            if (result.rows.length == 0) {
                $("#SearchArea").hide();
                $("#AddNewData").show();
                $("#LabelPoNo").text(PoNo);
                
            }
        }
    });
}

function InitSubFormField(disabled)
{
    $("#subForm :input").not("button").prop("disabled", disabled);
    $("#subForm .input-group").find("button").prop("disabled", disabled);
    //setTimeout(function(){$("#subForm .input-group").find("button").prop("disabled", true);}, 200);
    
}
function checkId(id) {

    $.ajax({
        async: true,
        cache: false,
        data: { 'id': id },
        url: rootPath + "NOTICE/CheckId",
        type: 'POST',
        dataType: "json",
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            if (data) {
                CommonFunc.Notify("", _getLang("L_initNotice_NotifyCdExi", "通知代码已存在请重新输入"), 500, "success"); 
                $("#Id").val('');
            }
        }
    });
}

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return id || caption;
}