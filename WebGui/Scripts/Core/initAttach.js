var edtOptions;
var seletedCount = 0;
var _emptySelectFileId = [];
var _fileIDArry = [];
var _fileDel = true;
var first = 1;
var whiteFlag = false;
function initAttach(registerBtn, keyData, multiData, callBackFunc,attachFlag, sendMail) {
    var __jobno = keyData.jobNo;
    _fileDel = keyData.FileDel;
    if (__jobno !== undefined && __jobno !== null)
        __jobno = __jobno.replace('.', '_')
    var myId = registerBtn.attr("id") + "_" + __jobno;
    _fileIDArry = [];
    registerBtn.attr("data-target", "edocDialog_" + myId);
    if (typeof $("#edocDialog_" + myId) != "undefined") {
        var temp = '<div class="modal fade" id="edocDialog_{initID}">\
                    <div class="modal-dialog modal-lg">\
                        <div class="modal-content">\
                            <div class="modal-header">\
                                <button type="button" class="close" id="edocClose_{initID}"><span aria-hidden="true">&times;</span></button>\
                                <h4 class="modal-title">Document</h4>' +
            '</div>\
                            <div class="modal-body">';

        if (attachFlag) {
            temp += '<div class="row">\
								    <div class="col-md-12">\
								        <form enctype="multipart/form-data">\
								        <div class="form-group">\
								            <input id="uploadFile_{initID}" class="file" type="file" name="uploadFile_{initID}" multiple data-preview-file-type="text">\
								        </div>\
								        </form>\
								    </div>\
								</div>';
        }
        temp += '<div class="row">\
								    <div class="col-md-12">\
                                        <form id="edocForm_{initID}" action="' + rootPath + 'EDOC/GetSelectOptions" >\
								            <table class="table table-striped table-hover">\
								                <thead>\
								                    <tr>\
								                        <th>#</th>\
								                        <th>File Name</th>\
								                        <th>File Type</th>\
								                        <th>Uploader</th>\
								                        <th>Date</th>\
                                                        <th>Remark</th>\
								                        <th>Action</th>\
								                    </tr>\
								                </thead>';
        if (attachFlag) {
            temp += '<div style="margin: 0;padding: 0;position: relative;left:10px;top:10px;color: red;">\
                                                    <div>' + _getLang("L_Initedoc_FormatAttention", "目前仅支持的格式有：") + 'jpg,png,jpeg,gif,tif,tiff,doc,docx,xls,xlsx,csv,ppt,pptx,msg,pdf,xps,txt</div>\
								                    <div>' + _getLang("L_Initedoc_FileSizeAttention", "最大文件size:") + '10MB/file</div></div>';
        }

        temp += '<tbody></tbody>\
								            </table>\
                                         </form>\
								    </div>\
								</div>\
                            </div>\
                        </div>\
                    </div>\
                </div>';
        temp = temp.replace(/{initID}/g, myId);
        $("body").append(temp);
    }

    //.unbind("click")
    registerBtn.unbind("click").click(function (event) {
        first = 2;
        $("#edocDialog_" + myId).modal({
            backdrop: 'static',
            keyboard: false
        });
        _emptySelectFileId = [];
        ajustamodal("#edocDialog_" + myId, 100, 120);
        //alert("init");
        _fileIDArry = [];
        doFileQuery(keyData, myId, edtOptions, true, sendMail, callBackFunc);
        if (typeof multiData != "undefined" && multiData != null && multiData.length > 0) {
            multiQuery(registerBtn, multiData, myId, edtOptions, sendMail, callBackFunc, keyData);
        }

        setEmptySelectFiledId();
    });

    //console.log(pmsList);
    var isUpload = true;
    if (pmsList.indexOf("EDOC_UP") == -1 && pmsList != "") {
        isUpload = false;
        $("#uploadFile_" + myId).hide();
    } else {
        $("#uploadFile_" + myId).fileinput({
            uploadUrl: BaseUrl + lang + "EDOC/AttachFileUpload",
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
        CommonFunc.Notify("", _getLang("L_BSCSSetup_UploadSuc", "档案上传成功"), 500, "success")
        _emptySelectFileId = [];
        _fileIDArry = [];
        doFileQuery(keyData, myId, edtOptions, true, sendMail, callBackFunc);
        if (typeof multiData != "undefined" && multiData != null && multiData.length > 0) {
            multiQuery(registerBtn, multiData, myId, edtOptions, sendMail, callBackFunc, keyData);
        }

        $("#edocClose_" + myId).attr("aria-label", "")
        $("#edocClose_" + myId).attr("data-dismiss", "");
    });

    $("#edocClose_" + myId).unbind("click").click(function (event) {
        $("#edocClose_" + myId).attr("aria-label", "Close")
        $("#edocClose_" + myId).attr("data-dismiss", "modal");
    });
    if (first == 1) {
        registerBtn.trigger("click");
        first = 1;
    }
    
}

function multiQuery(registerBtn, multiData, myId, opt, sendMail, callBackFunc, keyData) {

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
        url: BaseUrl + lang + "EDOC/AttachFileQuery",
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
                //var spamType = ",PO,INVO,PACKO,BF,DECL_BK";
                //if (typeof keyData.ata != "undefined" && keyData.ata != "") {
                //    isEditable = false;
                //}
                //TEST
                if (_fileIDArry.indexOf(result[i].FileID) > -1)
                    return true;
                _fileIDArry.push(result[i].FileID);
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
                            //if (result[i].ATID != 'Y' || result[i].EdocType == '') {
                            //    deleteStr = '<button type="button" class="btn btn-xs btn-danger deleteBtn" itemid="' + result[i].FileID + '" title="Remove"><span class="glyphicon glyphicon-remove"></span></button>';
                            //} else {
                            //}
                            if (_fileDel) {
                                deleteStr = '<button type="button" class="btn btn-xs btn-danger deleteBtn" itemid="' + result[i].FileID + '" title="Remove"><span class="glyphicon glyphicon-remove"></span></button>';
                            }
                        }
                    }
                }

                lastFid = result[i].FileID;

                var newRow = '<tr class="item_' + result[i].FileID + ' fileItems ">\
                            <td>' + (i + count) + '</td>\
                            <td>'+ result[i].DummyName + '</td>\
                            <td>'+ result[i].Ext + '</td>\
                            <td>'+ result[i].Uploader + '</td>\
                            <td>'+ result[i].SendTime + '</td>\
                            <td><input style="border-style:none;background-color:transparent;outline:none" name="'+ result[i].FileID + '_Rmk" type="text" value="' + result[i].Remark + '" readonly \></td>\
                            <td>\
                            ' + downloadStr + '\
                            ' + deleteStr + '\
                            </td>\
                            </tr>';

                $("#edocDialog_" + myId).find("tbody").append(newRow);

                console.log(opt);
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

            $(".downloadBtn").unbind("click").bind("click", function (event) {
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
                        data: { "itemId": itemId },
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
                            doFileQuery(keyData, myId, edtOptions, true, sendMail, callBackFunc);
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

            //setEmptySelectFiledId();
        }
    });
}


function setEmptySelectFiledId()
{
    $.each($("#edocForm_" + myId).serialize().split("&"), function (i, value) {
        if (value.split("=")[1] == "")
            _emptySelectFileId.push(value.split("_Edt=")[0]);
    });
}

function CC(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}