var url = "";
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
$(function () {
    setdisabled(true);

    _initMenu();
    MenuBarFuncArr.Enabled(["MBEdit"]);

    pwdmouse("#seeOpwd", "#Opwd");
    pwdmouse("#seeUpwd", "#UPassword");
    pwdmouse("#seeCpwd", "#ConfirmPwd");
    seepwd("#pwdOgroup", "#seeOpwd");
    seepwd("#pwdUgroup", "#seeUpwd");
    seepwd("#pwdCgroup", "#seeCpwd");
});

function pwdmouse(id1, id2) {
    $(id1).mousedown(function () {
        $(id2).attr("type", "text");
    });
    $(id1).mouseup(function () {
        $(id2).attr("type", "password");
    });
}

function seepwd(id1, id2) {
    $(id1).focusin(function () {
        $(id2).css("z-index", "100");
    });
    $(id1).focusout(function () {
        $(id2).css("z-index", "-100");
    });
}

function _initMenu() {
    MenuBarFuncArr.DelMenu(["MBApprove", "MBPrint", "MBErrMsg", "MBEdoc", "MBInvalid", "MBSearch", "MBAdd", "MBDel", "MBCopy"]);

    MenuBarFuncArr.MBEdit = function () {

        $("#ConfirmPwd").prop("disabled", false);
    }
    MenuBarFuncArr.MBCancel = function () {
        $("#ConfirmPwd").prop("disabled", true);
        MenuBarFuncArr.Enabled(["MBEdit"]);
    }
    MenuBarFuncArr.MBSave = function (dtd) {
        var UPassword = $("#UPassword").val();
        var ConfirmPwd = $("#ConfirmPwd").val();
        var Opwd = $("#Opwd").val();
        if (Opwd == "") {
            alert(_getLang("L_ModifyExpridPwd_Opwd", "请填写原始密码"));
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve(); 
            return false;
        }
        if (UPassword == "") {
            alert(_getLang("L_ModifyExpridPwd_Npwd", "请填写新密码"));
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve(); 
            return false;
        }

        if (!validatePassword(UPassword)) {
            alert(_getLang("L_ModifyPwd_Check", "不符合密码设置规则"));
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve(); 
            return false;
        }

        if (ConfirmPwd == "") {
            alert(_getLang("L_ModifyExpridPwd_Cpwd", "请填写确认密码"));
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve(); 
            return false;
        }
        if (Opwd == UPassword) {
            $("#UPassword").val("");
            $("#password").focus();
            $("#ConfirmPwd").val("");
            alert(_getLang("L_ModifyExpridPwd_Opwdcompare", "原始密码不能与新密码相同，请重新填写"));
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve(); 
            return false;
        }
        if (UPassword !== ConfirmPwd) {
            CommonFunc.Notify("", _getLang("L_ModifyPwd_NoMatch", "确认密码与密码不符"), 500, "warning");
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve();
            return dtd.promise();
        }

        //var changeData = getChangeValue();

        $.ajax({
            async: true,
            url: rootPath + "/Common/CheckUserPmsForTPV",
            type: 'POST',
            data: { "account": userId, "password": UPassword, "isDetail": "checky" },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {

            },
            "error": function (xmlHttpRequest, errMsg) {

            },
            success: function (result) {
                if (result.message == "passwordFailed") {
                    CommonFunc.Notify("", _getLang("L_ModifyPwd_VerF", "TPV ACCOUNT验证失败密码错误"), 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                } else {
                    if (result.message == "success") {
                        CommonFunc.Notify("", _getLang("L_ModifyPwd_VerS", "TPV ACCOUNT验证成功"), 500, "success");
                    } else {
                        CommonFunc.Notify("", _getLang("L_ModifyPwd_VerSN", "TPV ACCOUNT验证通过建立新使用者"), 500, "success");
                    }

                    var changeData = getChangeValue();
                    //表示值沒變
                    if ($.isEmptyObject(changeData)) {
                        CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "Save Successful"), 500, "success");
                        MenuBarFuncArr.SaveResult = true;
                        dtd.resolve();
                        setdisabled(true);
                        return;
                    }
                    $.ajax({
                        async: true,
                        url: rootPath + "/System/UserSetUpdatePwd",
                        type: 'POST',
                        data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, "Opwd": Opwd },
                        dataType: "json",
                        "complete": function (xmlHttpRequest, successMsg) {
                            //if (successMsg != "success") return null;
                            //else alert("success");
                            //setdisabled(false);
                        },
                        "error": function (xmlHttpRequest, errMsg) {
                            CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveF", "保存失敗"), 500, "warning");
                            MenuBarFuncArr.SaveResult = false;
                            dtd.resolve();
                        },
                        success: function (result) {
                            if (result.message != "success") {
                                var str = result.message;
                                if (str.indexOf(_getLang("L_ModifyPwd_Dup", "重複按鍵")) > -1)
                                    CommonFunc.Notify("", _getLang("L_UserSetUp_IDRepeat", "保存失败，用户ID重复"), 500, "warning");
                                else
                                    CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveF", "Save Failed"), 500, "warning");
                                MenuBarFuncArr.SaveResult = false;
                                dtd.resolve();
                                return false;
                            }
                            //alert(result.message);
                            setFieldValue(result.userData);
                            setdisabled(true);
                            setToolBtnDisabled(true);
                            CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "Save Successful"), 500, "success");
                            MenuBarFuncArr.SaveResult = true;
                            $("#Opwd").val("");
                            $("#ConfirmPwd").val("");

                            dtd.resolve();
                        }
                    });
                }
            }
        });
        return dtd.promise();
    }

    initMenuBar(MenuBarFuncArr);
}

function validatePassword(password) {
    // Rule 1: Password length must be at least 10 characters
    if (password.length < 10) {
        return false;
    }

    // Rule 2: Must contain at least one uppercase letter, one lowercase letter, one digit, and one special character
    const containsUpperCase = /[A-Z]/.test(password);
    const containsLowerCase = /[a-z]/.test(password);
    const containsDigit = /[0-9]/.test(password);
    const containsSpecialChar = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password);

    if (!(containsUpperCase && containsLowerCase && containsDigit && containsSpecialChar)) {
        return false;
    }

    // Rule 3: Prohibit consecutive characters or digits repeated more than 3 times
    if (/(.)\1{2,}/i.test(password.toLowerCase())) {
        return false;
    }

    if (/abc|bcd|cde|def|efg|fgh|ghi|hij|ijk|jkl|klm|lmn|mno|nop|opq|pqr|qrs|rst|stu|tuv|uvw|vwx|wxy|xyz|123|234|345|456|567|678|789/.test(password.toLowerCase())) {
        return false;
    }

    // Rule 4: Prohibit using more than 4 consecutive keyboard characters
    const keyboardSequences = ['1234', '2345', '3456', '4567', '5678', '6789', '7890', '890-', '90-=',
        '!@#$', '@#$%', '#$%^', '$%^&', '%^&*', '^&*(', '&*()', '*()_', '()_+',
        'qwer', 'wert', 'erty', 'rtyu', 'tyui', 'yuio', 'uiop', 'iop[', 'iop{', 'op[]', 'op{}', 'p[]\\', 'p{}|',
        'asdf', 'sdfg', 'dfgh', 'fghj', 'ghkj', 'hjkl', 'jkl;', 'jkl:', 'kl:"',
        'zxcv', 'xcvb', 'cvbn', 'vbnm', 'bnm,', 'bnm<', 'nm,.', 'nm<>', 'm,./', 'm<>?',
        '1qaz', '2wsx', '3edc', '4rfv', '5tgb', '6yhn', '7ujm', '8ik,', '9ol.', '0p;/',
        '!qaz', '@wsx', '#edc', '$rfv', '%tgb', '^yhn', '&ujm', '*ik,', '(ol.', ')p;/'];
    for (let sequence of keyboardSequences) {
        if (password.toLowerCase().includes(sequence)) {
            return false;
        }
    }

    // Rule 5: Prohibit using common or easily guessable passwords
    const commonPasswords = ['admin', 'administrator', 'root', 'password'];
    if (commonPasswords.includes(password.toLowerCase())) {
        return false;
    }

    // If the password passes all rules, return true
    return true;
}