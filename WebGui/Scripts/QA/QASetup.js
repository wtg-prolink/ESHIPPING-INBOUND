var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
$(function(){
    _handler.saveUrl = rootPath + "QA/UpdateData";
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

    registBtnLookup($("#QaTypeLookup"), {
        isMutiSel: true,
        item: "#QaType", url: rootPath + LookUpConfig.MutiQATypeUrl, config: LookUpConfig.MutiQATypeLookup, param: "", selectRowFn: function (map) {
        }
    }, {
        focusItem: $("#QaType"), columnID: "Cd", responseMethod: function (data) {
            console.log(data);
            var str = "";
            $.each(data, function (index, val) {
                str = str + data[index].Cd + ";";
            });
            $("#QaType").val(str);
        }
    }, {});

    MenuBarFuncArr.EndFunc = function(){

    }

    _handler.beforSave = function(){
        //CKEDITOR.instances['QaAnswer'].setReadOnly(true);
    }

    _handler.beforDel = function(){
        //CKEDITOR.instances['QaAnswer'].setData("");
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
        data["QaAnswer"] = encodeURIComponent(ue.getContent());
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
        //CKEDITOR.instances['QaAnswer'].setReadOnly(false);
    }

    _handler.beforAdd = function () {//新增前设定
        ue.setEnabled();
        //CKEDITOR.instances['QaAnswer'].setReadOnly(false);
    }

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];

        
        setFieldValue(data["main"] || [{}]);
        MenuBarFuncArr.Enabled(["MBEdoc","MBCopy"]);
        MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
        //_handler.beforLoadView();
        setdisabled(true);
        setToolBtnDisabled(true);
        //CKEDITOR.instances['QaAnswer'].setData(data["main"][0].QaAnswer);
        //CKEDITOR.instances['QaAnswer'].setReadOnly(true);
        ue.ready(function (editor) {
            ue.setDisabled();
        });
        MenuBarFuncArr.Enabled(["MBEdoc"]);
        

    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }

        $.ajax({
            async: false,
            url: rootPath + "QA/GetDetail",
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
        //CKEDITOR.instances['QaAnswer'].setReadOnly(false);
    }

    _initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg", "MBSearch"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    ue.ready(function (editor) {
        ue.setDisabled();
    });
});

function setUEditorDisabled(except) {
    ue = UE.getEditor('QaAnswer', {
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