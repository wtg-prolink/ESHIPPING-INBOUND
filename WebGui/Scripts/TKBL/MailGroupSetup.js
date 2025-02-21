var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null };

$(function () {
    var _setMyReadonlys = function () {
        var readonlys = ["Cmp"];
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('disabled', true);
            $("#" + readonlys[i]).parent().find("button").attr("disabled", true);
        }
    }

    var searchColumns = {
        caption: "Mail Group",
        sortname: "Cmp",
        refresh: false,
        columns: [{ name: "UId", title: "UId", width: 120, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Name', title: _getLang("", "Name"), width: 100, formatter: "select", editoptions: { value: " :ALL;" + _mailGroup }, classes: "uppercase", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'MailId', title: _getLang("", "E-Mail"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Remark', title: _getLang("", "Remark"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'GroupId', title: _getLang("", "GroupId"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
        { name: 'Cmp', title: _getLang("", "Company"), width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
        ]
    }

    _handler.saveUrl = rootPath + "TKBL/SaveMailGroupData";
    _handler.inquiryUrl = rootPath + "TKBL/MailGroupInquiry";
    _handler.config = searchColumns;


    _handler.addData = function () {
        //初始化新增数据 
        var data = { "GroupId": groupId, "Cmp": cmp, "Stn": stn, "CreateBy": userId };
        data[_handler.key] = uuid();
        setFieldValue([data]);
    }
    MenuBarFuncArr.EndFunc = function () {
        if (_upri === "O") {
            _setMyReadonlys();
        }
    }
    _handler.beforSave = function () {
        var mail_id = $("#MailId").val().split(";");
        var reg = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        for (var i = 0; i < mail_id.length; i++) {
            if (!reg.test(mail_id[i])) {
                alert(_getLang("L_MailGroupSetup_Script_170", "") + mail_id[i] + "xx.com");
                return false;
            }
        }
    }

    _handler.editData = function () {

    }

    _handler.afterEdit = function () {
        if (_upri === "O") {
            _setMyReadonlys();
        }
    }


    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "TKBL/GetMailGroupItem", { UId: map.UId, loading: true },
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
                if (!isEmpty($("#UId").val())) {
                    _uid = $("#UId").val();
                }
            });
    }

    _handler.saveData = function (dtd) {
        var changeData = getChangeValue();//获取所有改变的值

        var uid = $("#UId").val();
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), "uid": uid, autoReturnData: false, loading: true },
            function (result) {
                if (result.message != "success") {
                    alert(result.message);
                    return false;
                } else {
                    _handler.topData = { UId: result.UId };
                    _uid = result.UId;
                    MenuBarFuncArr.MBCancel();
                }
                return true;
            });
    }

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];

        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        MenuBarFuncArr.Enabled(["MBCopy"]);
        MenuBarFuncArr.DelMenu(["MBPreview"]);
    }

    _handler.delData = function () {
        var changeData = getAllKeyValue();//获取所有主键值
        ajaxHttp(_handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false },
            function (result) {
                if (result.message != "success") {
                    alert(result.message);
                    return false
                }
                else if (_handler.setFormData) {
                    setFieldValue(undefined, "");
                    setdisabled(true);
                    setToolBtnDisabled(true);
                    MenuBarFuncArr.SaveResult = true;
                }
                return true;
            });
    }

    _handler.beforLoadView = function () {
    }


    _initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg", "MBPreview", "MBSearch"]);//初始化UI工具栏
    MenuBarFuncArr.DelMenu(["MBEdoc", "MBApprove", "MBErrMsg", "MBPreview", "MBSearch"]);
    setTimeout(500, function () {
        MenuBarFuncArr.DelMenu(["MBPreview"]);
    });

    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit", "MBCopy", "MBDel", "MBSearch"]);
        var _mainCmpData = { UId: _uid }; //_uid
        _handler.loadMainData(_mainCmpData);
        editable = false;
        _subEdit = 0;
    }

    MenuBarFuncArr.MBCopy = function () {
        editable = false;
        $("#CreateBy").val(userId);
        $("#UId").removeAttr('required');
        $("#ModifyDate").val("");
        $("#ModifyBy").val("");
    }
    setMailSelect();

    registBtnLookup($("#CmpLookup"), {
        item: '#Cmp', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#Cmp").val(map.PartyNo);
            _handler.topData = { Cmp: map.PartyNo, Name: map.PartyName };
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Cmp").val(rd.PARTY_NO);
        _handler.topData = { Cmp: rd.PARTY_NO, Name: rd.PARTY_NAME };
    }));

    $("#txt_Cmp").val(cmp);
    if (_upri === "I") {
        $("#txt_Cmp").removeAttr('disabled');
        $("#CmpLookup").removeAttr('disabled');
    }
});

function setMailSelect() {
    $("#Name").append("<option value=\"\">ALL</option>");
    $.each(_mailGroup.split(";"), function (i, value) {
        var cd = value.split(":")[0];
        var cddescp = value.split(":")[1];
        if (cd != "") {
            $("#Name").append("<option value=\"" + cd + "\">" + cddescp + "</option>");
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