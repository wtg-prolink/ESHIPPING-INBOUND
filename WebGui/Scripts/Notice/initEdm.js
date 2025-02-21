var _dm = new dm();
var groupId = getCookie("plv3.passport.groupid"),
    cmp = getCookie("plv3.passport.companyid"),
    stn = getCookie("plv3.passport.station"),
    userId = getCookie("plv3.passport.user");
var IsAdd = false;
var Result = false;

function InitSubFormField(disabled) {
    $("#subForm :input").not("button").prop("disabled", disabled);
    $("#subForm .input-group").find("button").prop("disabled", disabled);
    //setTimeout(function(){$("#subForm .input-group").find("button").prop("disabled", true);}, 200);

}
jQuery(document).ready(function ($) {

    var rootPath = $("#rootPath").val() + "zh-CN";
    $('#navRoot').hide();
    $(".btn-link").remove();

    BindLookUp();
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
        $("#NOTICEDEL").prop("disabled", true);
        $("#NOTICECANCEL").prop("disabled", false);
        $("#NOTICESAVES").prop("disabled", false);
        $("#INSERTEDM").prop("disabled", false);
        InitSubFormField(false);
        IsAdd = true;
      //  CKEDITOR.instances['TpltContent'].setReadOnly(false);
    });

    //add data to grid
    $("#NOTICESAVES").click(function () {
        //get content order set to field "TPLT_SORT" and main content's id is "content"
        var sort = "";
        $.each($("#sortable li"), function (index, val) {
            console.log(val);
            sort += (val.id === "") ? "content" : val.id;
            sort += ";";
        });
        $("#TpltSort").val(sort);

    //    CKEDITOR.instances['TpltContent'].setReadOnly(true);
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
            url: rootPath + "/NOTICE/EDMSetUpdateData",
            type: 'POST',
            data: { "changedData": JSON.stringify(changeData), autoReturnData: true },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (result) {
                console.log(result.message);
                if (result.message !== "success") {
                    CommonFunc.Notify("", "保存失败", 500, "warning");
                    return;
                }
           
                IsAdd = false;
                $("#containerInfoGrid").jqGrid("setGridParam", {
                    datatype: 'json',
                    data: result.noticeData
                }).trigger("reloadGrid");

                $('#SearchArea1').collapse('show');
                $('#AddArea').collapse('hide');
                CommonFunc.Notify("", "保存成功", 500, "success");
                return false;
            }
        });
 
    });

    $("#NOTICEEDIT").click(function () {
        $("#subForm").focusFirst();
        $("#NOTICEADD").prop("disabled", true);
        $("#NOTICEEDIT").prop("disabled", true);
        $("#NOTICEDEL").prop("disabled", true);
        $("#NOTICECANCEL").prop("disabled", false);
        $("#NOTICESAVES").prop("disabled", false);
        InitSubFormField(false);
        $("#INSERTEDM").prop("disabled", false);
      //  CKEDITOR.instances['TpltContent'].setReadOnly(false);
    });



    $("#NOTICECANCEL").click(function(){
        $("#NOTICEADD").prop("disabled", false);
        $("#NOTICEEDIT").prop("disabled", false);
        $("#NOTICEDEL").prop("disabled", true);
        $("#NOTICECANCEL").prop("disabled", true);
        $("#NOTICESAVES").prop("disabled", true);
        $("#INSERTEDM").prop("disabled", true);
        InitSubFormField(true);
    //    CKEDITOR.instances['TpltContent'].setReadOnly(true);
    });

    $("#NOTICEDEL").click(function () {
        if (!confirm("确定要清空数据吗？")) {
            return;
        }
        StatusBarArr.nowStatus("正在清空数据...");
        var changeData = getAllKeyValue();
        changeData.mt[0].__state = "0";
    //    CKEDITOR.instances['TpltContent'].setReadOnly(true);
        $.ajax({
            async: true,
            url: rootPath + "/NOTICE/EDMSetUpdateData",
            type: 'POST',
            data: {
                "changedData": JSON.stringify(changeData), autoReturnData: true
            },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
                if (successMsg != "success")
                    return null;
                else
                    CommonFunc.Notify("", "删除成功", 500, "success");
                setdisabled(false);
                setToolBtnDisabled(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", "删除失败", 500, "warning");
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
});

function BindLookUp() {
    //EDM
    var buoptions = {};
    buoptions.gridUrl = rootPath + "Notice/EDMSetupInquiryData";
    buoptions.registerBtn = $("#INSERTEDM");
    // buoptions.focusItem = $("#Role");
    buoptions.selfSite = true;
    buoptions.multiselect = true;
    buoptions.gridFunc = function (map) {
        //获取Map中的值，然后将值传递给后台重新去抓取整个画面的url
       
    }
    buoptions.responseMethod = function (data) {
        console.log(data);
        var str = "";
        $.each(data, function (index, val) {
            $("#sortable").append('<li class="ui-state-default" id="' + data[index].UId + '"><span class="ui-icon ui-icon-arrowthick-2-n-s " style="float: left;"></span>' + data[index].TpltName + '<br / ><div style="background-color: white;">' + data[index].TpltContent + '</div></li>');
        });
    }
    buoptions.lookUpConfig = LookUpConfig.EDMLookup;
    initLookUp(buoptions);
}
