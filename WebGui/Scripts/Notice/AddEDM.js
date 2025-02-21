
var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
$(function(){
    _handler.saveUrl = rootPath + "Notice/EDMSetUpdateData";
    _handler.inquiryUrl = rootPath + "Notice/EDMSetupInquiryData";
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
        CKEDITOR.instances['TpltContent'].setReadOnly(false);

    }


    MenuBarFuncArr.EndFunc = function(){

    }

    _handler.beforSave = function(){
        CKEDITOR.instances['TpltContent'].setReadOnly(true);
    }

    _handler.beforDel = function(){
        CKEDITOR.instances['TpltContent'].setData("");
    }

    _handler.saveData = function (dtd) {

        //get content order set to field "TPLT_SORT" and main content's id is "content"
        var sort = "";
        $.each($("#sortable li"), function (index, val) {
            console.log(val);
            sort += (val.id === "") ? "content" : val.id;
            sort += ";";
        });
        $("#TpltSort").val(sort);

        CKEDITOR.instances['TpltContent'].setReadOnly(true);

        var changeData = getChangeValue();//获取所有改变的值
        var data = { "changedData": JSON.stringify(changeData), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
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
        CKEDITOR.instances['TpltContent'].setReadOnly(false);
    }

    _handler.beforAdd = function () {//新增前设定
    }

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];

        setFieldValue(data["main"] || [{}]);
        //_handler.beforLoadView();
        setdisabled(true);
        setToolBtnDisabled(true);
     /*   CKEDITOR.replace('TpltContent');
        CKEDITOR.instances.TpltContent.on('change', function () {
            CKEDITOR.instances.TpltContent.updateElement()
            $('textarea[fieldname=TpltContent]').text(CKEDITOR.instances.TpltContent.getData());
        });*/
        var editor = CKEDITOR.instances["TpltContent"];
        if (editor) { editor.setData(data["main"][0].TpltContent); }

        //process to init sort content
        var sort = "";
        var tempContent = $("#TpltContent").val();
        if (data["main"][0].TpltSort != "") {
            sort = data["main"][0].TpltSort.split(";");
            var dataCount = 0;
            $.ajax({
                async: true,
                url: rootPath + "/NOTICE/GetTPLTDetail",
                type: 'POST',
                data: {
                    "uIds": data["main"][0].TpltSort, autoReturnData: true
                },
                dataType: "json",
                success: function (result) {
                   
                    if (editor) { editor.destroy(true); }
                    $("#sortable").children().remove();
                    
                    $.each(sort, function (index, value) {
                        if (value === "content") {
                            $("#sortable").append(' <li class="ui-state-default"><span class="ui-icon ui-icon-arrowthick-2-n-s" style="float: left;"></span>內文<textarea class="form-control" id="TpltContent" dt="mt" name="TpltContent" fieldname="TpltContent"></textarea></li>');
                           
                            CKEDITOR.replace('TpltContent');
                            $('textarea[fieldname=TpltContent]').text(tempContent);
                            CKEDITOR.instances.TpltContent.on('change', function () {
                                CKEDITOR.instances.TpltContent.updateElement()
                                $('textarea[fieldname=TpltContent]').text(CKEDITOR.instances.TpltContent.getData());
                            });
                     
                        } else {
                            $.each($.parseJSON(result.data.Content).rows, function (idx, val) {
                                if (val.UId == value) {
                                    $("#sortable").append('<li class="ui-state-default" id="' + value + '"><span class="ui-icon ui-icon-arrowthick-2-n-s " style="float: left;"></span>' + val.TpltName + '<br / ><div style="background-color: white;">' + val.TpltContent + '</div></li>');
                                }
                            });
                        }
                    });


                    
                }
            });
        }
       
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }

        $.ajax({
            async: false,
            url: rootPath + "Notice/GetEDMDetail",
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

    _initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg", "MBCopy", "MBSearch", "MBEdoc"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }




});