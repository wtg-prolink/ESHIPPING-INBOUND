
var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
$(function(){
    _handler.saveUrl = rootPath + "Dashboard/AddBulletinUpdate";
    _handler.inquiryUrl = rootPath + "Dashboard/AddBulletinInquiry";
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
        //CKEDITOR.instances['BullContent'].setReadOnly(false);
        $("#Cmp").removeAttr('required');
    }


    MenuBarFuncArr.EndFunc = function(){

    }

    _handler.beforSave = function () {
        var Cmp = $("#Cmp").val();
        if (Cmp == "" || Cmp == null || Cmp == undefined)
            $("#Cmp").val(baseCmp);
        //CKEDITOR.instances['BullContent'].setReadOnly(true);
    }

    _handler.beforDel = function(){
        //CKEDITOR.instances['BullContent'].setData("");
    }

    $("#MBCancel").click(function () {//自定义编辑
        ue.setDisabled();
        var className = $(this).attr('class');
        if (className == "nav-disabled") {
            return false;
        }
        MenuBarFuncArr.MBCancel();
        MenuBarFuncArr.CancelStatus();
    });

    _handler.saveData = function (dtd) {
        var changeData = getChangeValue();//获取所有改变的值
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        data["bullContent"] = encodeURIComponent(ue.getContent());
        data["BullType"] = encodeURIComponent($("#BullType").val());
        data["Cmp"] = encodeURIComponent($("#Cmp").val());
        data["Dep"] = encodeURIComponent($("#Dep").val());
        console.log(changeData);
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
        ue.setEnabled();
        //CKEDITOR.instances['BullContent'].setReadOnly(false);
    }


    _handler.afterEdit = function () {
        CmpDepDisabled();
    }

    _handler.beforAdd = function () {//新增前设定
        ue.setEnabled();
    }

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];

        setFieldValue(data["main"] || [{}]);
        $("#Cmp").removeAttr('required');
        MenuBarFuncArr.Enabled(["MBAttachment"]);
        //_handler.beforLoadView();
        setdisabled(true);
        ue.ready(function (editor) {
            ue.setDisabled();
        });
        //setToolBtnDisabled(true);
        //CKEDITOR.instances['BullContent'].setData(data["main"][0].BullContent);
        //CKEDITOR.instances['BullContent'].setReadOnly(true);
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }

        $.ajax({
            async: false,
            url: rootPath + "Dashboard/GetDetail",
            type: 'POST',
            data: { uId: map.UId, loading: true },
            dataType: "json",
            "beforeSend": function (xmlHttpRequest, successMsg) {
                CommonFunc.ToogleLoading(true);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", errMsg, 500, "danger");
                CommonFunc.ToogleLoading(false);
            },
            success: function (result) {
                CommonFunc.ToogleLoading(false);
                if (_handler.setFormData)
                        _handler.setFormData(result);
            }
        });
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        var data = {};
        data[_handler.key] = uuid();
        var dataRow, addData = [];
    }
    MenuBarFuncArr.AddMenu("MBAttachment", "glyphicon glyphicon-bell", "Attachment", function () {
        var uid = $("#UId").val();
        if (!isEmpty(uid)) {
            initAttach($("#MBAttachment"), { jobNo: uid, 'GROUP_ID': "TPV", 'CMP': "FQ", 'STN': '*', "REMARK": "Bulletin", "FileDel": true }, null, null, true);
        } else {
            alert("Uid is empty, please re-enter the interface！");
        }
        
    });

    _initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg", "MBCopy", "MBSearch", "MBEdoc"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    ue.ready(function (editor) {
        ue.setDisabled();
    });

    registBtnLookup($("#CmpLookup"), {
        isMutiSel: true,
        item: "#Cmp", url: rootPath + LookUpConfig.GetCmpUrl, config: LookUpConfig.MutiLocationLookup, param: "", selectRowFn: function (map) {
            $("#Cmp").val(map.Cmp);
        }
    }, {
        baseCondition: " GROUP_ID='" + groupId + "' AND TYPE='1'",
        responseMethod: function (data) {
            console.log(data);
            var str = "";
            $.each(data, function (index, val) {
                str = str + data[index].Cmp + ";";
            });
            $("#Cmp").val(str);
        }
    });

    registBtnLookup($("#DepLookup"), {
        isMutiSel: true,
        item: "#Dep", url: rootPath + "Common/GetDisDepData", config: LookUpConfig.MutiBSCodeLookup, param: "", selectRowFn: function (map) {
            $("#Dep").val(map.Cd);
        }
    }, {
        baseCondition: " 1=1 ",
        responseMethod: function (data) {
            console.log(data);
            var str = "";
            $.each(data, function (index, val) {
                str = str + data[index].Cd + ";";
            });
            $("#Dep").val(str);
        }
    });


});

function setUEditorDisabled(except) {
    ue = UE.getEditor('BullContent', {
        toolbars: [
            ['fullscreen', 'source', '|', 'undo', 'redo', '|',
        'bold', 'italic', 'underline', 'fontborder', 'strikethrough', 'superscript', 'subscript', 'removeformat', 'formatmatch', 'autotypeset', 'blockquote', 'pasteplain', '|', 'forecolor', 'backcolor', 'insertorderedlist', 'insertunorderedlist', 'selectall', 'cleardoc', '|',
        'rowspacingtop', 'rowspacingbottom', 'lineheight', '|',
        'customstyle', 'paragraph', 'fontfamily', 'fontsize', '|',
        'directionalityltr', 'directionalityrtl', 'indent', '|',
        'justifyleft', 'justifycenter', 'justifyright', 'justifyjustify', '|', 'touppercase', 'tolowercase', '|',
        'link', 'unlink', 'anchor', '|', 'imagenone', 'imageleft', 'imageright', 'imagecenter', '|',
        'simpleupload', 'insertimage', 'emotion', 'scrawl', 'insertframe', 'insertcode', 'pagebreak', 'template', 'background', '|',
        'horizontal', 'date', 'time', 'spechars', 'snapscreen', 'wordimage', '|',
        'inserttable', 'deletetable', 'insertparagraphbeforetable', 'insertrow', 'deleterow', 'insertcol', 'deletecol', 'mergecells', 'mergeright', 'mergedown', 'splittocells', 'splittorows', 'splittocols', 'charts', '|',
        'print', 'preview', 'searchreplace', 'help', 'drafts']
        ],
        allHtmlEnabled: false,//提交到后台的数据是否包含整个html字符串  
        autoHeightEnabled: false,
        autoFloatEnabled: true,
        allowDivTransToP: false,//阻止div标签自动转换为p标签 
        readonly: false//界面是否可编辑
    });
}

$("#BullType").change(function () {
    CmpDepDisabled();
});

function CmpDepDisabled() {
    var bullType = $("#BullType").val();
    if (bullType == 2) {
        $("#Dep").val('');
        $("#DepLookup").attr("disabled", true);
    } else {
        $("#DepLookup").attr("disabled", false);
    }
    //if (bullType == 4 || bullType == 5) {
    //    $("#Cmp").val("");
    //    $("#CmpLookup").attr("disabled", true);
    //} else {
    //    $("#CmpLookup").attr("disabled", false);
    //}
}