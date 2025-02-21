var docHeight = 0;
var rolelistA, rolelistB;
jQuery(document).ready(function ($) {
    docHeight = $(document).height();


    MenuBarFuncArr.DelMenu(["MBEdoc", "MBApply", "MBApprove", "MBInvalid", "MBCopy"]);
    var RoleLi = '';
    var UserLi = '';
    var uStatus = '';
    for (var i = 0; i < RoleData.length; i++) {
        RoleLi += '<li class="list-group-item rolelist " id="' + RoleData[i].RoleID + '" GroupId="' + RoleData[i].GroupId + '" Cmp="' + RoleData[i].Cmp + '" Stn="' + RoleData[i].Stn + '" ><p class="rolename">' + RoleData[i].RoleName + '</p></li>';
    }

    for (var j = 0; j < UserData.length; j++) {
        var userdisable = "userdisable='true'";
        uStatus = _getLang("L_UserSetUp_Disable", "停用");
        if (UserData[j].UStatus == "0") {
            uStatus = _getLang("L_UserSetUp_Enable", "启用");
            userdisable = "";
        }
        UserLi += '<li class="list-group-item checkbox userlist" id="' + UserData[j].UserID + '" GroupId="' + UserData[j].GroupId + '" Cmp="' + UserData[j].Cmp + '" Stn="' + UserData[j].Stn + '" ><label><input type="checkbox" value="' + UserData[j].UserID + '" disabled ' + userdisable +'><p class="username">' + UserData[j].UserID + ":" + UserData[j].UserName + '</p></label><label style="float: right;text-align: right">' + uStatus + '</label></li>';
    }

    $(".PERMISION-ROLE").append(RoleLi);
    var options = {
        valueNames: ['rolename'],
        page: 9999
    };

    rolelistA = new List('PA', options);



    $(".PERMISSION-USER").append(UserLi);
    var options = {
        valueNames: ['username'],
        page:99999
    };

    rolelistB = new List('PB', options);


    $('#PERMISSION-TREE').treeview({
        color: "#333",
        selectedColor: "#666",
        selectedBackColor: "#eee",
        state: {
            expanded: true
        },
        //enableLinks: true,
        data: MenuData
    });

    $("#PERMISION-SELECT a").on("click", function (e) {
        e.preventDefault()
        $(this).tab('show')
    });

    /*******新增某角色下的用戶********/
    $(".PERMISSION-USER").on("click", "input", function () {
        var UserID = $(this).val();
        rolelistB.search();
        if (typeof localData.RoleData[RoleID] === "undefined" ) {
            localData.RoleData[RoleID] = {};
            localData.RoleData[RoleID]["users"] = new Array();
            if ($(".PERMISSION-USER input:checked").length > 0) {
                $(".PERMISSION-USER input:checked").each(function (i, val) {
                    console.log($(this).val());
                    localData.RoleData[RoleID]["users"].push($(this).val());
                });
            }
            else {
                if ($(this).prop("checked") == true) {
                    localData.RoleData[RoleID]["users"].push(UserID);
                }
                else {
                    var index = localData.RoleData[RoleID]["users"].indexOf(UserID);
                    if (index > -1) {
                        localData.RoleData[RoleID]["users"].splice(index, 1);
                    }
                }
            }
        }
        else {
            if (typeof localData.RoleData[RoleID]["users"] == "undefined") {
                localData.RoleData[RoleID]["users"] = new Array();
                if ($(".PERMISSION-USER input:checked").length > 0) {
                    $(".PERMISSION-USER input:checked").each(function (i, val) {
                        console.log($(this).val());
                        localData.RoleData[RoleID]["users"].push($(this).val());
                    });
                }
                else {
                    if ($(this).prop("checked") == true) {
                        localData.RoleData[RoleID]["users"].push(UserID);
                    }
                    else {
                        var index = localData.RoleData[RoleID]["users"].indexOf(UserID);
                        if (index > -1) {
                            localData.RoleData[RoleID]["users"].splice(index, 1);
                        }
                    }
                }
            } else {
                if ($(this).prop("checked") == true) {
                    localData.RoleData[RoleID]["users"].push(UserID);
                }
                else {
                    var index = localData.RoleData[RoleID]["users"].indexOf(UserID);
                    if (index > -1) {
                        localData.RoleData[RoleID]["users"].splice(index, 1);
                    }
                }
            }
        }
        rolelistB.search($("#PB input").val());
        //console.log(localData.RoleData[RoleID]["users"]);
    });
/*******新增某角色下的用戶 END********/

    var showalluser = false;
    $("#ShowPBUserInfo").click(function (event) {
        $(".PERMISSION-USER input").each(function (i, val) {
            if ($(this).prop("checked") != true || $(this).attr("userdisable")) {
                $(this).closest("li").css('display', 'none');
            }
        });
        if (showalluser) {
            $(".PERMISSION-USER input").each(function (i, val) {
                $(this).closest("li").css('display', 'block');
            });
            showalluser = false;
        } else {
            showalluser = true;
        }

    });

    /******* 取得角色權限 ********/
    $("#PERMISSION-TREE").on("click", "li", function (e) {
        $("#CHECKBOX-GROUP").children().remove();
        $("#CHECKBOX-GROUP").attr('pmsId', $(this).attr('pmsid'));

        $("#RPT-CHECKBOX-GROUP").children().remove();
        $("#RPT-CHECKBOX-GROUP").attr('pmsId', $(this).attr('pmsid'));

        if ($(e.target).is(".list-group-item") && $(e.target).children("span").hasClass("glyphicon-tag")) {
            if (localData.RoleData[RoleID] == null || typeof localData.RoleData[RoleID]["permission"] == "undefined" || typeof localData.RoleData[RoleID]["permission"][$(this).attr('pmsid')] == "undefined") {
                if ($("ul.PERMISION-ROLE li").hasClass("active") == true) {
                    $.get(getPermissionUrl, { roleId: RoleID, GroupId: $(".rolelist[ID='" + RoleID + "']").attr('groupid'), CompanyId: $(".rolelist[ID='" + RoleID + "']").attr('cmp'), Station: $(".rolelist[ID='" + RoleID + "']").attr('stn'), pmsId: $(e.target).attr('pmsid') }, function (data) {

                        for (var i = 0; i < data.permision.length; i++) {
                            var str = '';
                            str += '<li class="list-group-item checkbox">';
                            if (data.permision[i].checked == 0) {
                                str += '<label><input type="checkbox" value="1" id="' + data.permision[i].pmsId + '" showname="' + data.permision[i].caption + '" >' + data.permision[i].caption + '</label>';
                            }
                            else if (data.permision[i].checked == 1) {
                                str += '<label><input type="checkbox" value="1" id="' + data.permision[i].pmsId + '" showname="' + data.permision[i].caption + '" CHECKED>' + data.permision[i].caption + '</label>';
                            }
                            str += '</li>';

                            if (data.permision[i].pmsId.indexOf("_RPT_") > -1) {
                                $("#RPT-CHECKBOX-GROUP").append(str);
                            } else {
                                $("#CHECKBOX-GROUP").append(str);
                            }

                        }


                    }, 'json');
                }
                else {
                    //alert("@Resources.Locale.L_UserPermission_SelcRole");
                    alert(_getLang("L_UserPermission_SelcRole", "请先选择角色"));
                }
            }
            else {

                var data = Object.keys(localData.RoleData[RoleID]["permission"][$(this).attr('pmsId')]);
                //var dataName = Object.keys(localData.RoleData[RoleID]["permission"][$(this).attr('pmsId')+"_NAME"]);

                for (var i = 0; i < data.length; i++) {
                    var str = '';
                    str += '<li class="list-group-item checkbox">';
                    if (localData.RoleData[RoleID]["permission"][$(this).attr('pmsId')][data[i]] == 0) {
                        str += '<label><input type="checkbox" value="1" id="' + data[i] + '" showname = "' + localData.RoleData[RoleID]["permission"][$(this).attr('pmsId') + "_NAME"][data[i]] + '">' + localData.RoleData[RoleID]["permission"][$(this).attr('pmsId') + "_NAME"][data[i]] + '</label>';
                    }
                    else if (localData.RoleData[RoleID]["permission"][$(this).attr('pmsId')][data[i]] == 1) {
                        str += '<label><input type="checkbox" value="1" id="' + data[i] + '" showname = "' + localData.RoleData[RoleID]["permission"][$(this).attr('pmsId') + "_NAME"][data[i]] + '" CHECKED>' + localData.RoleData[RoleID]["permission"][$(this).attr('pmsId') + "_NAME"][data[i]] + '</label>';
                    }
                    str += '</li>';

                    if (data[i].indexOf("_RPT_") > -1) {
                        $("#RPT-CHECKBOX-GROUP").append(str);
                    } else {
                        $("#CHECKBOX-GROUP").append(str);
                    }
                }

            }
        }
    });
    /******* 取得角色權限 END********/

    /******* 取得角色用戶列表 ********/
    $(".PERMISION-ROLE li").on("click", function () {
        $("#CHECKBOX-GROUP").find("li").remove();
        RoleID = $(this).attr('id');
        $(this).siblings('li').removeClass('active');
        $(this).addClass('active');
        if (typeof localData.RoleData[RoleID] === "undefined" || typeof localData.RoleData[RoleID]["users"] === "undefined") {
            $.ajax({
                url: getRoleUsersUrl + RoleID + "&cmp=" + $(this).attr('cmp') + "&stn=" + $(this).attr('stn'),
                type: 'GET',
                dataType: 'json',
            })
            .done(function (data) {
                console.log(data);
                rolelistB.search("");
                //$(".PERMISSION-USER input").prop("disabled", false);
                $('.PERMISSION-USER li').each(function (i, val) {

                    for (var j = 0; j < data.length; j++) {

                        if ($(this).attr('id') == data[j].UserID) {
                            $(this).find('input[type=checkbox]').prop('checked', true);
                            return;
                        }
                        else {
                            $(this).find('input[type=checkbox]').prop('checked', false);
                        }
                    }

                    if (data.length == 0) {
                        $(this).find('input[type=checkbox]').prop('checked', false);
                    }

                });
                rolelistB.search($("#PB input").val());
            })
            .fail(function () {
                console.log("error");
            });
        }
        else {
            rolelistB.search();
            $('.PERMISSION-USER li').each(function (i, val) {

                if ($.inArray($(this).attr('id'), localData.RoleData[RoleID]["users"]) != -1) {
                    $(this).find('input[type=checkbox]').prop('checked', true);
                }
                else {
                    $(this).find('input[type=checkbox]').prop('checked', false);
                }

                if (localData.RoleData[RoleID].length == 0) {
                    $(this).find('input[type=checkbox]').prop('checked', false);
                }

            });
            rolelistB.search($("#PB input").val());
        }
    });
    /******* 取得角色用戶列表 END********/

    /******* 設至功能報表權限 ********/
    $("#CHECKBOX-GROUP,#RPT-CHECKBOX-GROUP").on("click", "input", function () {
        var thisPMSId = $("#CHECKBOX-GROUP").attr('pmsId');

        if (typeof localData.RoleData[RoleID] == "undefined") {
            localData.RoleData[RoleID] = {};
            localData.RoleData[RoleID]["permission"] = {};
            localData.RoleData[RoleID]["permission"][thisPMSId] = {};
            localData.RoleData[RoleID]["permission"][thisPMSId + "_NAME"] = {};
            //設置功能
            $("#CHECKBOX-GROUP input").each(function (i, el) {
                if ($(this).prop("checked") == true) {
                    localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 1;
                }
                else {
                    localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 0;
                }
                localData.RoleData[RoleID]["permission"][thisPMSId + "_NAME"][$(this).attr('id')] = $(this).attr('showname');
            });
            //設置報表
            $("#RPT-CHECKBOX-GROUP input").each(function (i, el) {
                if ($(this).prop("checked") == true) {
                    localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 1;
                }
                else {
                    localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 0;
                }
                localData.RoleData[RoleID]["permission"][thisPMSId + "_NAME"][$(this).attr('id')] = $(this).attr('showname');
            });
        }
        else {
            if (typeof localData.RoleData[RoleID]["permission"] == "undefined") {
                localData.RoleData[RoleID]["permission"] = {}
                localData.RoleData[RoleID]["permission"][thisPMSId] = {};
                localData.RoleData[RoleID]["permission"][thisPMSId + "_NAME"] = {};

                //設置功能
                $("#CHECKBOX-GROUP input").each(function (i, el) {
                    if ($(this).prop("checked") == true) {
                        localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 1;
                    }
                    else {
                        localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 0;
                    }
                    localData.RoleData[RoleID]["permission"][thisPMSId + "_NAME"][$(this).attr('id')] = $(this).attr('showname');
                });

                //設置報表
                $("#RPT-CHECKBOX-GROUP input").each(function (i, el) {
                    if ($(this).prop("checked") == true) {
                        localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 1;
                    }
                    else {
                        localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 0;
                    }
                    localData.RoleData[RoleID]["permission"][thisPMSId + "_NAME"][$(this).attr('id')] = $(this).attr('showname');
                });
            }
            else if (typeof localData.RoleData[RoleID]["permission"][thisPMSId] == "undefined") {
                localData.RoleData[RoleID]["permission"][thisPMSId] = {};
                localData.RoleData[RoleID]["permission"][thisPMSId + "_NAME"] = {};

                //設置功能
                $("#CHECKBOX-GROUP input").each(function (i, el) {
                    if ($(this).prop("checked") == true) {
                        localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 1;
                    }
                    else {
                        localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 0;
                    }
                    localData.RoleData[RoleID]["permission"][thisPMSId + "_NAME"][$(this).attr('id')] = $(this).attr('showname');
                });

                //設置報表
                $("#RPT-CHECKBOX-GROUP input").each(function (i, el) {
                    if ($(this).prop("checked") == true) {
                        localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 1;
                    }
                    else {
                        localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 0;
                    }
                    localData.RoleData[RoleID]["permission"][thisPMSId + "_NAME"][$(this).attr('id')] = $(this).attr('showname');
                });
            }
            else {
                //設置功能
                $("#CHECKBOX-GROUP input").each(function (i, el) {
                    if ($(this).prop("checked") == true) {
                        localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 1;
                    }
                    else {
                        localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 0;
                    }
                    localData.RoleData[RoleID]["permission"][thisPMSId + "_NAME"][$(this).attr('id')] = $(this).attr('showname');
                });

                //設置報表
                $("#RPT-CHECKBOX-GROUP input").each(function (i, el) {
                    if ($(this).prop("checked") == true) {
                        localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 1;
                    }
                    else {
                        localData.RoleData[RoleID]["permission"][thisPMSId][$(this).attr('id')] = 0;
                    }
                    localData.RoleData[RoleID]["permission"][thisPMSId + "_NAME"][$(this).attr('id')] = $(this).attr('showname');
                });
            }
        }
        //console.log(localData.RoleData[RoleID]["permission"]);
    });


    /******* 儲存 ********/
    MenuBarFuncArr.MBSave = function (dtd) {
        $("#PA input").val("");
        rolelistA.search();

        $("#PB input").val("");
        rolelistB.search();

        $.each(localData.RoleData, function (index, val) {
            var tempData = {};
            tempData.roleID = index;
            tempData.groupID = $(".rolelist[ID='" + index + "']").attr('groupid');
            tempData.cmp = $(".rolelist[ID='" + index + "']").attr('cmp');
            tempData.stn = $(".rolelist[ID='" + index + "']").attr('stn');
            //tempData.users = new Array();
            //if (typeof localData.RoleData[index].users != "undefined") {
            //    tempData.users = localData.RoleData[index].users;
            //    //var userArr = tempData.users.split(",");
            //    for (var i = 0; i < tempData.users.length; i++) {


            //        if (typeof tempData.ugroupID == "undefined") {
            //            tempData.ugroupID = $(".userlist[ID='" + tempData.users[i] + "']").attr('groupid');
            //        } else {
            //            tempData.ugroupID += "," + $(".userlist[ID='" + tempData.users[i] + "']").attr('groupid');
            //        }

            //        if (typeof tempData.ucmp == "undefined") {
            //            tempData.ucmp = $(".userlist[ID='" + tempData.users[i] + "']").attr('cmp');
            //        } else {
            //            tempData.ucmp += "," + $(".userlist[ID='" + tempData.users[i] + "']").attr('cmp');
            //        }

            //        if (typeof tempData.ustn == "undefined") {
            //            tempData.ustn = $(".userlist[ID='" + tempData.users[i] + "']").attr('stn');
            //        } else {
            //            tempData.ustn += "," + $(".userlist[ID='" + tempData.users[i] + "']").attr('stn');
            //        }

            //    }
            //}
            tempData.permission = new Array();

            if (typeof localData.RoleData[index].permission != "undefined") {
                $.each(localData.RoleData[index].permission, function (index, val) {

                    if (index.indexOf("_NAME") == -1) {
                        var tempData2 = {};
                        tempData2["pms-id"] = index;
                        tempData2["psm"] = new Array();

                        $.each(localData.RoleData[tempData.roleID].permission[index], function (index, val) {
                            var tempData3 = {};
                            tempData3["pms-id"] = index;
                            tempData3["checked"] = val;
                            tempData2["psm"].push(tempData3)
                        });
                        tempData.permission.push(tempData2);
                    }
                });
            }
            postData.push(tempData);
        });
        console.log(postData);

        $.ajax({
            async: false,
            url: saveUrl,
            type: 'POST',
            data: { "permision": _tobase64String(JSON.stringify(postData)) },
            "complete": function (xmlHttpRequest, successMsg) {
                return null;
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", errMsg, 500, "danger");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },         
            success: function (result) {
                var reslut2 = JSON.parse(result);
                if (reslut2.result) {
                    CommonFunc.Notify(_getLang("L_UserPermission_SData", "保存信息"), _getLang("L_MailFormatSetup_SaveS", "Save Successful"), 500, "success");
                    dtd.resolve();
                    location.reload();
                } else {
                    CommonFunc.Notify("", "Failed to save. Please contact RD!", 500, "danger");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                }

            }
        });

    }
    /******* 儲存 ********/

    MenuBarFuncArr.MBCancel = function () {
        location.reload();
    }

    MenuBarFuncArr.DelMenu(["MBSearch", "MBAdd", "MBDel", "MBEdit", "MBErrMsg", "MBCancel"]);

    initMenuBar(MenuBarFuncArr);
    MenuBarFuncArr.Enabled(["MBSave", "MBCancel"]);

    $("#PA").height((docHeight - 165) / 2);
    $("#PB").height((docHeight - 165) / 2);
    $("#PC").height(docHeight - 100);
    $("#PD").height(docHeight - 100);
});

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}