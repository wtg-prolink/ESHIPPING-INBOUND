var edtOptions;
var _emptySelectFileId = [];
var _fileIDArry = [];
var delJobNo;
var whiteFlag = false;
function initEdoc(registerBtn, keyData, multiData, callBackFunc, sendMail) {
    getOpt();
    var __jobno = keyData.jobNo;
    var __dnNo = keyData.dnNo;
    delJobNo = keyData.jobNo;
    if (__dnNo == undefined || __dnNo == null) {
        __dnNo = "";
    }
    else {
        __dnNo = "(" + __dnNo + ")";
    }
    if (__jobno !== undefined && __jobno !== null)
        __jobno = __jobno.replace('.', '_')
    var myId = registerBtn.attr("id") + "_" + __jobno;
    //var myId = registerBtn.attr("id") + "_" + keyData.jobNo;
    //alert(myId);
    _fileIDArry = [];
    registerBtn.attr("data-target", "edocDialog_" + myId);
    if (typeof $("#edocDialog_" + myId) != "undefined") {
        var temp = '<div class="modal fade" id="edocDialog_{initID}">\
                    <div class="modal-dialog modal-lg">\
                        <div class="modal-content">\
                            <div class="modal-header">\
                                <button type="button" class="close" id="edocClose_{initID}"><span aria-hidden="true">&times;</span></button>\
                                <h4 class="modal-title">E Document ' + __dnNo + ' </h4>' +
                        '</div>\
                            <div class="modal-body">\
								<div class="row">\
								    <div class="col-md-12">\
								        <form enctype="multipart/form-data">\
								        <div class="form-group">\
								            <input id="uploadFile_{initID}" class="file" type="file" name="uploadFile_{initID}" multiple data-preview-file-type="text">\
								        </div>\
								        </form>\
								    </div>\
								</div>\
								<div class="row">\
								    <div class="col-md-12">\
                                        <form id="edocForm_{initID}" action="' + rootPath + 'EDOC/GetSelectOptions" >\
								            <table class="table table-striped table-hover">\
								                <thead>\
								                    <tr>\
								                        <th>#</th>\
								                        <th>File Name</th>\
								                        <th>File Type</th>\
                                                        <th>EDOC Type</th>\
								                        <th>Uploader</th>\
								                        <th>Date</th>\
                                                        <th>Remark</th>\
								                        <th>Action</th>\
								                    </tr>\
								                </thead>\
                                                    <button type="button" class="btn btn-sm btn-success" id="UPDATEEDOC_{initID}">update uploading type</button>&nbsp;\
                                                    <div style="margin: 0;padding: 0;position: relative;left:150px;top:-20px;color: red;">\
                                                    <div>Support DOC Type:jpg,png,jpeg,gif,tif,tiff,doc,docx,xls,xlsx,csv,ppt,pptx,msg,pdf,xps,txt</div>\
								                    <div>File Limit Size:10MB/file</div></div>\
                                                    <tbody></tbody>\
								            </table>\
                                         </form>\
								    </div>\
								</div>\
                                <div class="row" id="EdocMail_{initID}" style="display:none;">\
                                    <div class="form-group">\
                                        <label for="inputEmail3" class="col-sm-2 control-label">Email</label>\
                                        <div class="col-sm-12">\
                                            <input type="email" class="form-control" id="EdocMailTo" placeholder="Email">\
                                        </div>\
                                      </div>\
                                      <div class="form-group">\
                                        <label for="inputPassword3" class="col-sm-2 control-label">Content</label>\
                                        <div class="col-sm-12">\
                                          <textarea class="form-control" id="EdocMailContent" rows="5"></textarea>\
                                        </div>\
                                        <div class="col-sm-12">\
                                           <button type="button" class="btn btn-sm btn-success" id="EDOCSEND_{initID}">convey</button><>\
                                        </div>\
                                      </div>\
                                </div>\
                            </div>\
                        </div>\
                    </div>\
                </div>';
        temp = temp.replace(/{initID}/g, myId);
        $("body").append(temp);
    }


    registerBtn.unbind("click").click(function (event) {

        _fileIDArry = [];
        _emptySelectFileId = [];
        $("#edocDialog_" + myId).modal({
            backdrop: 'static',
            keyboard: false
        });
        ajustamodal("#edocDialog_" + myId, 100, 120);
        //alert("init");

        doFileQuery(keyData, myId, edtOptions, true, sendMail, callBackFunc);
        if (typeof multiData != "undefined" && multiData != null && multiData.length > 0) {
            multiQuery(registerBtn, multiData, myId, edtOptions, sendMail, callBackFunc);
        }
        setEmptySelectFiledId();
    });

    $("#UPDATEEDOC_" + myId).unbind("click").click(function (event) {
        console.log($("#edocForm_" + myId).serialize());
        $.ajax({
            async: false,
            url: rootPath + "EDOC/UpdateEdoc",
            data: $("#edocForm_" + myId).serialize(),
            type: 'POST',
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (data) {
                if (callBackFunc != null) {
                    if (callBackFunc) {
                        var edocTypes = "";
                        $.each($("#edocForm_" + myId).serialize().split("&"), function (i, value) {
                            edocTypes += value.split("=")[1] + ","
                        });
                        var __jobno = keyData.jobNo;
                        if (__jobno !== undefined && __jobno !== null)
                            __jobno = __jobno.replace('.', '_')
                        callBackFunc(__jobno, edocTypes);
                    }
                }
                CommonFunc.Notify("", "Update sucessful", 500, "success");
            }
        });
        _emptySelectFileId = [];
        setEmptySelectFiledId();
    });

    $("#EDOCSEND_" + myId).unbind("click").click(function (event) {
        var mailTo = $("#EdocMailTo").val();
        var mailContent = $("#EdocMailContent").val();
        $.each($(".Chk_" + myId), function (i, value) {
            if (value.checked) {
                mailContent += "<hr /><a href=" + $(value).attr("data") + ">" + $(value).attr("filename") + "</a><br />"
            }
        });

        var postData = { mailTo: mailTo, mailContent: mailContent };
        $.ajax({
            async: false,
            url: rootPath + "EDOC/SendMail",
            data: postData,
            type: 'POST',
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (data) {
                CommonFunc.Notify("", "Success", 500, "success");
            }
        });
    });
    //console.log(pmsList);
    var isUpload = true;
    if (pmsList.indexOf("EDOC_UP") == -1 && pmsList != "") {
        isUpload = false;
        $("#uploadFile_" + myId).hide();
    } else {
        $("#uploadFile_" + myId).fileinput({
            uploadUrl: BaseUrl + lang + "EDOC/FileUpload",
            minFileCount: 1,
            uploadAsync: isUpload,
            showUpload: isUpload,
            allowedFileExtensions: ["jpg", "png", "gif", "doc", "docx", "xls", "xlsx", "txt", "msg", "jpeg", "csv", "pdf", "ppt", "tiff", "pptx", "xps", "tif"],
            showRemove: true,
            showPreview: false,
            dropZoneEnabled: false,
            uploadExtraData: keyData
        });
    }


    $("#uploadFile_" + myId).unbind("filebatchuploadsuccess").on('filebatchuploadsuccess', function (event, result) {
        if (result.response.message === undefined || result.response.message === "" || result.response.message == null) {
            whiteFlag = false;
        } else {
            whiteFlag = true;
            alert(result.response.message);
        }
    });

    $("#uploadFile_" + myId).unbind("filebatchuploadcomplete").on('filebatchuploadcomplete', function (event, files, extra) {
        if (whiteFlag) return;
        _fileIDArry = [];
        _emptySelectFileId = [];
        //alert("do");
        CommonFunc.Notify("", "Upload successfully!", 500, "success");
        doFileQuery(keyData, myId, edtOptions, true, sendMail, callBackFunc);
        if (typeof multiData != "undefined") {
            multiQuery(registerBtn, multiData, myId, edtOptions, sendMail, callBackFunc);
        }
        $("#edocClose_" + myId).attr("aria-label", "")
        $("#edocClose_" + myId).attr("data-dismiss", "");
    });

    $("#edocClose_" + myId).unbind("click").click(function (event) {
        if (_emptySelectFileId.length > 0) {
            CommonFunc.Notify("", _getLang("L_Edoc_noEdocType",""), 1000, "warning");
        } else {
            $("#edocClose_" + myId).attr("aria-label", "Close")
            $("#edocClose_" + myId).attr("data-dismiss", "modal");
        }
    });

}

function multiQuery(registerBtn, multiData, myId, opt, sendMail, callBackFunc) {
    $(multiData).each(function (k, v) {
        doFileQuery(multiData[k], myId, opt, false, sendMail, callBackFunc);
    });

}

function doFileQuery(keyData, myId, opt, remove, sendMail, callBackFunc) {
    $("#edocDialog_" + myId).find("tbody").html("");

    var count = 1;
    if (remove) {
        $(".fileItems").remove();
    }
    $.ajax({
        async: true,
        url: BaseUrl + lang + "EDOC/FileQuery",
        type: 'POST',
        data: keyData,
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
            //if (successMsg != "success") return null;
            //else alert("success");
            //setdisabled(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        "success": function (result) {
            console.log(result);

            count = $(".fileItems").length + 1;

            var lastFid = "";

            $.each(result, function (i, value) {
                var isEditable = true;
                if (_fileIDArry.indexOf(result[i].FileID) > -1)
                    return true;
                _fileIDArry.push(result[i].FileID);
                var spamType = ",PO,INVO,PACKO,BF,DECL_BK";
                if (typeof keyData.ata != "undefined" && keyData.ata != "") {
                    isEditable = false;
                }
//TEST
                
                var downloadStr = "";
                if (pmsList.indexOf("EDOC_DOWN") > -1 || pmsList == "") {
                    if (pmsList.indexOf("EDOC_EDT_V_" + result[i].EdocType) > -1 || pmsList == "") {
                        downloadStr = '<button type="button" class="btn btn-xs btn-success downloadBtn" itemid="' + result[i].FileID + '" title="Download"><span class="glyphicon glyphicon-circle-arrow-down"></span></button>';
                        downloadStr += ' <button type="button" class="btn btn-xs btn-warning edocCopyBtn" title="Copy" data="' + parent.edocUrl + 'apis/apilaunchfile.ashx?token=' + result[i].Token + '&i=' + result[i].FileID + '"  itemid="' + result[i].FileID + '" ><span class="glyphicon glyphicon-book"></span></button>';
                    }
                }

                var deleteStr = "";
                if (pmsList.indexOf("EDOC_ALLDEL") > -1) {
                    deleteStr = '<button type="button" class="btn btn-xs btn-danger deleteBtn" itemid="' + result[i].FileID + '" title="Remove"><span class="glyphicon glyphicon-remove"></span></button>';
                } else {
                    if (pmsList.indexOf("EDOC_DEL") > -1 || pmsList == "") {
                        if ((userId.toUpperCase() == result[i].Uploader.toUpperCase() && (pmsList.indexOf("EDOC_EDT_D_" + result[i].EdocType) > -1 || result[i].EdocType == "")) || pmsList == "") {
                            if (isEditable || result[i].EdocType == '') {
                                deleteStr = '<button type="button" class="btn btn-xs btn-danger deleteBtn" itemid="' + result[i].FileID + '" title="Remove"><span class="glyphicon glyphicon-remove"></span></button>';
                            } else {
                                if (spamType.indexOf(result[i].EdocType) == -1) {
                                    deleteStr = '<button type="button" class="btn btn-xs btn-danger deleteBtn" itemid="' + result[i].FileID + '" title="Remove"><span class="glyphicon glyphicon-remove"></span></button>';
                                }
                            }
                        }
                    }
                }

                var selectStr = "";

                if (userId.toUpperCase() == result[i].Uploader.toUpperCase() || pmsList == "") {
                    if (isEditable) {
                        selectStr = '<select id="' + result[i].FileID + '_Edt"' + ' name="' + result[i].FileID + '_Edt"><select>';
                    } else {
                        if(spamType.indexOf(result[i].EdocType) > -1 && result[i].EdocType!=''){
                            selectStr = '<select id="' + result[i].FileID + '_Edt"' + ' name="' + result[i].FileID + '_Edt" disabled="disabled"><select>';
                        }else{
                            selectStr = '<select id="' + result[i].FileID + '_Edt"' + ' name="' + result[i].FileID + '_Edt"><select>';
                        }
                    }      
                } else {
                        selectStr = '<select id="' + result[i].FileID + '_Edt"' + ' name="' + result[i].FileID + '_Edt" disabled="disabled"><select>';
                }

                var checkboxStr = "";

                if (typeof sendMail != "undefined" && sendMail) {
                    checkboxStr = "<input type='checkbox' fileName='" + result[i].DummyName + "' data='" + "{EDOC_URL}apis/apilaunchfile.ashx?token=" + result[i].Token + "&i=" + result[i].FileID + "' class='Chk_" + myId + "' name='Chk_" + myId + "' id='" + result[i].FileID + '_Chk"' + "' />"
                }

                lastFid = result[i].FileID;

                var newRow = '<tr class="item_' + result[i].FileID + ' fileItems ">\
                            <td>' + (i + count) + " " + checkboxStr + '</td>\
                            <td>'+ result[i].DummyName + '</td>\
                            <td>'+ result[i].Ext + '</td>\
                            <td>' + selectStr + '</td>\
                            <td>'+ result[i].Uploader + '</td>\
                            <td>'+ result[i].SendTime + '</td>\
                            <td>' + result[i].Remark + '</td>\
                            <td>\
                            ' + downloadStr + '\
                            ' + deleteStr + '\
                            </td>\
                            </tr>';

                $("#edocDialog_" + myId).find("tbody").append(newRow);

                console.log(opt);
                setOpt($("#" + result[i].FileID + "_Edt"), opt, result[i].EdocType);
                if (result[i].EdocType == "" && !(_emptySelectFileId.indexOf(result[i].FileID) > -1))
                    _emptySelectFileId.push(result[i].FileID);
            });

            new Clipboard('.edocCopyBtn', {
                text: function (trigger) {
                    return trigger.getAttribute('data');
                }
            });

            //king when edoc types was null, send last fid into callBackFunc
            if (remove) {
                if (callBackFunc != null) {
                    if (callBackFunc) {
                        callBackFunc(lastFid, null);
                    }
                }
            }
            $(".downloadBtn").unbind("click").click(function (event) {
                var itemId = $(this).attr("itemid");

                $.ajax({
                    async: true,
                    url: BaseUrl + lang + "EDOC/GetEdocServer",
                    type: 'POST',
                    data: { "itemId": itemId },
                    dataType: "json",
                    "complete": function (xmlHttpRequest, successMsg) {
                    },
                    "error": function (xmlHttpRequest, errMsg) {
                        alert(errMsg);
                    },
                    "success": function (result) {
                        // console.log(result);
                        postAndRedirect(result[0].url + "apis/apilaunchfile.ashx", { "sguid": result[0].softID, "act": result[0].account, "pws": result[0].password, "i": itemId, "ct": "url" });
                    }
                });


            });

            $(".deleteBtn").unbind("click").click(function (event) {


                var txt;
                var r = confirm("Are you sure delete this file?");
                if (r == true) {
                    var itemId = $(this).attr("itemid");
                    $.ajax({
                        async: true,
                        url: BaseUrl + lang + "EDOC/FileDelete",
                        type: 'POST',
                        data: { "itemId": itemId, "jobNo": delJobNo },
                        dataType: "json",
                        "complete": function (xmlHttpRequest, successMsg) {
                        },
                        "error": function (xmlHttpRequest, errMsg) {
                            alert(errMsg);
                        },
                        "success": function (result) {
                            var index = _emptySelectFileId.indexOf(itemId)
                            if (index > -1)
                                _emptySelectFileId.splice(index, 1);
                            $(".item_" + itemId).remove();
                            if (callBackFunc != null) {
                                if (callBackFunc) {
                                    var edocTypes = "";
                                    $.each($("#edocForm_" + myId).serialize().split("&"), function (i, value) {
                                        edocTypes += value.split("=")[1] + ","
                                    });
                                    var __jobno = keyData.jobNo;
                                    if (__jobno !== undefined && __jobno !== null)
                                        __jobno = __jobno.replace('.', '_')
                                    callBackFunc(__jobno, edocTypes);
                                }
                            }

                        }
                    });

                }
            });


            $(".Chk_" + myId).unbind("change").change(function () {
                var showable = false;
                $.each($(".Chk_" + myId), function (i, value) {
                    if (value.checked) {
                        showable = true;
                    }
                });

                if (showable) {
                    $("#EdocMail_" + myId).show();
                } else {
                    $("#EdocMail_" + myId).hide();
                }
            });
        }
    });
}
/*取得选项*/
function getOpt() {

    $.ajax({
        async: false,
        url: rootPath + "EDOC/GetSelectOptions",
        type: 'POST',
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            edtOptions = data.Edt;
        }
    });
}
/*设置select的选项*/
function setOpt(selectId, options, value) {
    selectId.empty();
    if (value == "") {
        selectId.append("<option value='' selected='selected'></option>");
    }
    $.each(options, function (idx, option) {

        if (option.cd == value) {
            selectId.append("<option value=\"" + option.cd + "\" selected='selected'>" + option.cdDescp + "</option>");
        }
        else {
            selectId.append("<option value=\"" + option.cd + "\">" + option.cdDescp + "</option>");
        }
    });
}


/*function ajustamodal(myModal) 
{
    var altura = $(window).height() - 120; //value corresponding to the modal heading + footer
    var modalWidth = $(window).width() - 100;
    $(myModal + " .modal-body").css({"height":altura,"overflow-y":"auto"});
    $(myModal + " .modal-lg").css({"width": modalWidth});
}*/

function setEmptySelectFiledId() {
    $.each($("#edocForm_" + myId).serialize().split("&"), function (i, value) {
        if (value.split("=")[1] == "")
            _emptySelectFileId.push(value.split("_Edt=")[0]);
    });
}


function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}

function resetFileInput(file) {
    file.after(file.clone().val(""));
    file.remove();
    $(".file-input-name").html("");
}