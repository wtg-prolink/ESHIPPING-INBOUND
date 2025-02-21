var approveFunc = {process:function(MaxLv, NowLv){}};
var approveCode = "";
var RefNo		= "";
var myId = "";
var $table = "";
var postUrl = "";
var CountApprove = 0;
var dd = {};
function DropDown(el) {
	this.dd = el;
	this.opts = this.dd.find('ul.myDropDownMenu > li');
	this.val = [];
	this.valNm = [];
	this.index = [];
	this.initEvents();

	
}


function initEapprove(registerBtn, keyData)
{
	CommonFunc.consoleMsg("Init E-Approve start");
	var obj = this;
	approveCode = keyData.approveCode;
	RefNo		= keyData.RefNo;
	postUrl = rootPath + "EApproved/getApprovedData";
	myId = registerBtn.attr("id");
    registerBtn.attr("data-target","approveDialog_" + myId);

	//init SearchTemplate
	var data = {'initID': myId};
	var source = $("#EapproveTemplate").html();
	var template = Handlebars.compile(source);
	var html = template(data);
	$("body").append(html);

	registerBtn.click(function(event) {
		$( "#approveDialog_"+myId ).modal( {
			backdrop: 'static',
			keyboard: false
		});
		ajustamodal("#approveDialog_" + myId, 100, 180);
	});

	$table = $("#approveTable_" + myId);
	$("#approveDialog_" + myId).on("show.bs.modal", function(){
		$(this).find("#approveClose").prop("disabled", false);
		genApproveTable($table, postUrl, approveCode, RefNo);
	});
}

function genApproveTable($table, postUrl, approveCode, RefNo)
{
		console.log($table);
		$.ajax({
		    async: true,
		    url: postUrl,
		    type: 'POST',
		    data: {
		        ApproveCode: approveCode,
		        RefNo: RefNo
		    },
		    dataType: "json",
		    "error": function (xmlHttpRequest, errMsg) {
		        console.log(errMsg);
		    },
		    success: function (result) {
		        console.log(result);
		        var str = "";
		        var MaxLv = parseInt(result.Max);
		        var btnStr = "";
		        var nextLv = 0;
		        CountApprove = result.returnData.length;
		        $.each(result.returnData, function(i, val) {
		        	var ButtonDisabled = "disabled";
		        	

		        	if(result.returnData[i].CreateBy == null)
		        	{
		        		result.returnData[i].CreateBy = "";
		        		result.returnData[i].CreateDate = "";
		        	}

		        	if(MaxLv == 0)
		        	{
		        		if($.inArray(result.returnData[i].Fid, roles) != -1)
		        		{
		        			ButtonDisabled = "";
		        		}

		        		if(result.returnData[i].ApproveLevel == 1)
		        		{
			        		if(result.returnData[i].Fdescp == null || result.returnData[i].Fdescp == "")
			        		{
			        			result.returnData[i].Fdescp = "";
			        			ButtonDisabled = "";
			        		}
			        		str += '<tr class="primary">\
			        	                <td>' + result.returnData[i].ApproveLevel + '</td>\
			        	                <td>'+result.returnData[i].ApproveName+'</td>\
			        	                <td>'+result.returnData[i].Fdescp+'</td>\
			        	                <td></td>\
			        	                <td></td>\
			        	                <td>待申请</td>\
			        	                <td>\
			        	                    <button class="btn btn-sm btn-success iApprove" status="1" nowLv="'+result.returnData[i].ApproveLevel+'" maxLv="'+MaxLv+'" ApproveCode="'+result.returnData[i].ApproveCode+'" approveName="'+result.returnData[i].ApproveName+'" RefNo="'+RefNo+'" Role="'+result.returnData[i].Fid+'" '+ButtonDisabled+' >申請</button>\
			        	                </td>\
			        	            </tr>';
		        	    }
		        	    else
		        	    {
		        	    	if(result.returnData[i].ApproveLevel == 2)
		        	    	{
		        	    		var OpentionStr = "";
		        	    		
		        	    		if(result.RoleResult.length > 0)
		        	    		{

		        	    			$.each(result.RoleResult, function(index, val) {
		        	    				var checked = "";
		        	    				var disabled = "";
		        	    				if(result.returnData[i].Role == result.RoleResult[index].Fid)
		        	    				{
		        	    					checked = "checked";
		        	    					disabled = "disabled";
		        	    				}
		        	    				OpentionStr += '<li><input type="checkbox" id="el-'+index+'" name="el-1" value="'+result.RoleResult[index].Fid+'" '+checked+' '+disabled+' /><label for="el-'+index+'">'+result.RoleResult[index].Fdescp+'</label></li>';
		        	    			});
		        	    		}

		        	    		if(result.returnData[i].Fdescp == null)
		        	    		{
		        	    			result.returnData[i].Fdescp = "";
		        	    		}
		        	    		btnStr = '<div class="myDropDown" id="dd"><span>'+result.returnData[i].Fdescp+'</span><ul class="myDropDownMenu">'+OpentionStr+'</ul></li>';
		        	    	}
		        	    	else
		        	    	{
		        	    		btnStr = result.returnData[i].Fdescp;
		        	    	}
		        	    	str += '<tr class="warning">\
		        	                    <td>' + result.returnData[i].ApproveLevel + '</td>\
		        	                    <td>'+result.returnData[i].ApproveName+'</td>\
		        	                    <td>'+btnStr+'</td>\
		        	                    <td></td>\
		        	                    <td></td>\
		        	                    <td>待審核</td>\
		        	                    <td>\
		        	                        <button class="btn btn-sm btn-success iApprove" status="1" disabled>通过</button>\
		        	                        <button class="btn btn-sm btn-danger iApprove" status="0" disabled>退回</button>\
		        	                    </td>\
		        	                </tr>';
		        	    }
		        	}
		        	else
		        	{
		        		var disabled = "";
		        		var ApproveStatus = "warning";
		        		var statusMsg = "";
		        		var roleArray, managerArray;
		        		var iApproveInfo = "";
		        		var btnStr = "";
		        		var OpentionStr = "";
		        		

		        		if(result.returnData[i].Status == 1)
		        		{
		        			disabled = 'disabled';
		        			ApproveStatus = "success";
		        			if(result.returnData[i].ApproveLevel == 1)
		        			{
		        				statusMsg = "已申请";
		        			}
		        			else
		        			{
		        				statusMsg = "已审核";
		        				btnStr = result.returnData[i].RoleNm;
		        			}
		        		}
		        		else if(result.returnData[i].Status == 2)
		        		{
		        			disabled = "disabled";
		        			ApproveStatus = "primary";
		        			statusMsg = "待审核";

		        			if(result.returnData[i].Role != null)
		        			{
		        				roleArray = result.returnData[i].Role.split(",");
		        				$.each(roleArray, function(index, val) {
		        					 if($.inArray(roleArray[index], roles) != -1)
		        					 {
		        					     disabled = "";
		        					     iApproveInfo = ' nowLv="' + result.returnData[i].ApproveLevel + '" maxLv="' + MaxLv + '" ApproveCode="' + result.returnData[i].ApproveCode + '" approveName="' + result.returnData[i].ApproveName + '" RefNo="' + RefNo + '" Boss="' + result.returnData[i].UManager + '"';
		        					    return false;
		        					 }
		        					 else
		        					 {
		        					 	if(result.returnData[i].UManager != null)
		        					 	{
		        					 		managerArray = result.returnData[i].UManager.split(",");

		        					 		if($.inArray(userId,managerArray) != -1)
		        					 		{
		        					 		    disabled = "";
		        					 		    iApproveInfo = ' nowLv="' + result.returnData[i].ApproveLevel + '" maxLv="' + MaxLv + '" ApproveCode="' + result.returnData[i].ApproveCode + '" approveName="' + result.returnData[i].ApproveName + '" RefNo="' + RefNo + '" Boss="' + result.returnData[i].UManager + '"';
		        					 		}
		        					 	}
		        					 }
		        				});
		        			}

		        			

		        			nextLv = parseInt(result.returnData[i].ApproveLevel) + 1;
		        			btnStr = result.returnData[i].RoleNm;
		        		}
		        		else if(result.returnData[i].Status == 0 && result.returnData[i].ApproveLevel == nextLv )
		        		{
		        			if(result.RoleResult.length > 0)
		        			{

		        				$.each(result.RoleResult, function(index, val) {
		        					var checked = "";
		        					var disabled = "";

		        					if(result.returnData[i].Fid == result.RoleResult[index].Fid)
		        					{
		        						checked = "checked";
		        						disabled = "disabled";
		        					}
		        					OpentionStr += '<li><input type="checkbox" id="el-'+index+'" name="el-1" value="'+result.RoleResult[index].Fid+'" '+checked+' '+disabled+' /><label>'+result.RoleResult[index].Fdescp+'</label></li>';
		        				});
		        			}
		        			btnStr = '<div class="myDropDown" id="dd"><span>'+result.returnData[i].Fdescp+'</span><ul class="myDropDownMenu">'+OpentionStr+'</ul></li>';
		        			statusMsg = "待审核";
		        			disabled = 'disabled';
		        		}
		        		else
		        		{
		        			disabled = 'disabled';
		        			ApproveStatus = "warning";
		        			statusMsg = "待审核";
		        			btnStr = '';
		        		}

		        		if(result.returnData[i].ApproveLevel == 1)
		        		{
	        				if(result.returnData[i].Fdescp == null || result.returnData[i].Fdescp == "")
	        				{
	        					result.returnData[i].Fdescp = "";
	        				}
	        				str += '<tr class="'+ApproveStatus+'">\
	        			                <td>' + result.returnData[i].ApproveLevel + '</td>\
	        			                <td>'+result.returnData[i].ApproveName+'</td>\
	        			                <td>'+result.returnData[i].Fdescp+'</td>\
	        			                <td>'+result.returnData[i].CreateBy+'</td>\
	        			                <td>'+result.returnData[i].CreateDate+'</td>\
	        			                <td>'+statusMsg+'</td>\
	        			                <td>\
	        			                    <button class="btn btn-sm btn-success iApprove" '+disabled+'>申請</button>\
	        			                </td>\
	        			            </tr>';
		        		}
		        		else
		        		{
	        				str += '<tr class="'+ApproveStatus+'">\
	        			                <td>' + result.returnData[i].ApproveLevel + '</td>\
	        			                <td>'+result.returnData[i].ApproveName+'</td>\
	        			                <td>'+btnStr+'</td>\
	        			                <td>'+result.returnData[i].CreateBy+'</td>\
	        			                <td>'+result.returnData[i].CreateDate+'</td>\
	        			                <td>'+statusMsg+'</td>\
	        			                <td>\
	        			                    <button class="btn btn-sm btn-success iApprove" status="2" ' + iApproveInfo + ' ' + disabled + '>通过</button>\
	        			                    <button class="btn btn-sm btn-danger iApprove" status="0"  ' + iApproveInfo + ' ' + disabled + '>退回</button>\
	        			                </td>\
	        			            </tr>';
		        		}
		        		/*if(result.returnData[i].Alevel < MaxLv + 1 )
		        		{
			        		if(result.returnData[i].Alevel == 1)
			        		{
				        		str += '<tr class="success">\
				        	                <td>' + result.returnData[i].Alevel + '</td>\
				        	                <td>'+result.returnData[i].Aname+'</td>\
				        	                <td>'+RoleNm+'</td>\
				        	                <td>'+result.returnData[i].CreateBy+'</td>\
				        	                <td>'+result.returnData[i].CreateDate+'</td>\
				        	                <td>已審核</td>\
				        	                <td>\
				        	                    <button class="btn btn-sm btn-success iApprove" disabled>申請</button>\
				        	                </td>\
				        	            </tr>';
			        	    }
			        	    else
			        	    {
			        	    	str += '<tr class="success">\
			        	                    <td>' + result.returnData[i].Alevel + '</td>\
			        	                    <td>'+result.returnData[i].Aname+'</td>\
			        	                    <td>'+RoleNm+'</td>\
			        	                    <td>'+result.returnData[i].CreateBy+'</td>\
			        	                    <td>'+result.returnData[i].CreateDate+'</td>\
			        	                    <td>已審核</td>\
			        	                    <td>\
			        	                        <button class="btn btn-sm btn-success iApprove" disabled>通过</button>\
			        	                        <button class="btn btn-sm btn-danger iApprove" disabled>退回</button>\
			        	                    </td>\
			        	                </tr>';
			        	    }
		        		}
		        		else if(result.returnData[i].Alevel == MaxLv + 1 )
		        		{
	        				str += '<tr class="primary">\
	        			                <td>' + result.returnData[i].Alevel + '</td>\
	        			                <td>'+result.returnData[i].Aname+'</td>\
	        			                <td>'+RoleNm+'</td>\
	        			                <td>'+result.returnData[i].CreateBy+'</td>\
	        			                <td>'+result.returnData[i].CreateDate+'</td>\
	        			                <td>待審核</td>\
	        			                <td>\
	        			                    <button class="btn btn-sm btn-success iApprove" status="2" nowLv="'+result.returnData[i].Alevel+'" maxLv="'+MaxLv+'" ApproveCode="'+result.returnData[i].Acode+'" approveName="'+result.returnData[i].Aname+'" RefNo="'+RefNo+'" Role="'+result.returnData[i].Arole+'" '+ButtonDisabled+'>通过</button>\
	        			                    <button class="btn btn-sm btn-danger iApprove" status="0" nowLv="'+result.returnData[i].Alevel+'" maxLv="'+MaxLv+'" ApproveCode="'+result.returnData[i].Acode+'" approveName="'+result.returnData[i].Aname+'" RefNo="'+RefNo+'" Role="'+result.returnData[i].Arole+'" '+ButtonDisabled+'>退回</button>\
	        			                </td>\
	        			            </tr>';
		        		}
		        		else
		        		{
			        		
			        	    	str += '<tr class="warning">\
			        	                    <td>' + result.returnData[i].Alevel + '</td>\
			        	                    <td>'+result.returnData[i].Aname+'</td>\
			        	                    <td>'+RoleNm+'</td>\
			        	                    <td>'+result.returnData[i].CreateBy+'</td>\
			        	                    <td>'+result.returnData[i].CreateDate+'</td>\
			        	                    <td>待審核</td>\
			        	                    <td>\
			        	                        <button class="btn btn-sm btn-success iApprove" disabled>通过</button>\
			        	                        <button class="btn btn-sm btn-danger iApprove" disabled>退回</button>\
			        	                    </td>\
			        	                </tr>';
		        		}*/
		        	}
		        	
		        });
		        
		        $table.find("tbody").html(str);

		        DropDown.prototype = {
		            initEvents: function () {
		                var obj = this;
		                var str = "";
		                var strNm = "";
		                var defaultRoleNm = "", defaultRole = [], defaultRoleNmArray = [];
		                $.each(obj.opts.children('input'), function (index, val) {
		                    if ($(this).prop("checked")) {
		                        obj.valNm.push($(this).siblings('label').text());
		                        obj.val.push($(this).val());
		                        defaultRole.push($(this).val());
		                        defaultRoleNmArray.push($(this).siblings('label').text());
		                    }
		                });

		                defaultRoleNm = obj.valNm.join();

		                obj.dd.on('click', function (event) {
		                    if ($(event.target).attr("id") == "dd")
		                        $(this).toggleClass('active');
		                    event.stopPropagation();
		                });
		                obj.opts.children('label').on('click', function (event) {
		                    $(this).siblings('input').click();
		                });

		                obj.opts.children('input').on('click', function (event) {
		                    var opt = $(this).parent(),
                                chbox = $(this),
                                val = chbox.val(),
                                valNm = chbox.siblings('label').text();
		                    idx = opt.index();

		                    if (chbox.prop("disabled") === false) {
		                        //(chbox.prop("checked") === true) ? chbox.prop("checked", false) : chbox.prop("checked", true);
		                        /*($.inArray(val, obj.val) !== -1) ? obj.val.splice( $.inArray(val, obj.val), 1 ) : obj.val.push( val );
                                ($.inArray(idx, obj.index) !== -1) ? obj.index.splice( $.inArray(idx, obj.index), 1 ) : obj.index.push( idx );
                                ($.inArray(valNm, obj.valNm) !== -1) ? obj.valNm.splice( $.inArray(valNm, obj.valNm), 1 ) : obj.valNm.push( valNm );*/
		                        if (chbox.prop("checked") === true) {
		                            obj.val.push(val);
		                            obj.index.push(idx);
		                            obj.valNm.push(valNm);
		                        }
		                        else {
		                            if ($.inArray(val, obj.val) !== -1) {
		                                obj.val.splice($.inArray(val, obj.val), 1);
		                                obj.index.splice($.inArray(idx, obj.index), 1);
		                                obj.valNm.splice($.inArray(valNm, obj.valNm), 1);
		                            }
		                        }
		                    }


		                    str = obj.val.join();
		                    strNm = obj.valNm.join();

		                    if (obj.valNm.length > 3) {
		                        obj.dd.find("span").text("已选择" + obj.valNm.length + "项目");
		                    }
		                    else if (obj.valNm.length == 0) {
		                        obj.dd.find("span").text(defaultRoleNm);
		                        $.each(defaultRole, function (index, val) {
		                            obj.val.push(defaultRole[index]);
		                        });

		                        $.each(defaultRoleNmArray, function (index, val) {
		                            obj.valNm.push(defaultRoleNmArray[index]);
		                        });
		                    }
		                    else {
		                        obj.dd.find("span").text(strNm);
		                    }
		                });
		            },
		            getValue: function () {
		                return this.val;
		            },
		            getIndex: function () {
		                return this.index;
		            }
		        }


		        dd = new DropDown( $('#dd') );
		    }
		});
}

jQuery(document).ready(function($) {
	$(document).click(function(){
		$(".myDropDown").removeClass('active');
	});
	$("body").on("click", "button.iApprove", function(){

		var status = $(this).attr("status");
		var NowLv = parseInt($(this).attr("NowLv"));
		var returnNowLv = NowLv;
		if(status == 0)
		{
			//NowLv = NowLv - 1;
			returnNowLv = returnNowLv - 1 ;
		}
		else
		{
			returnNowLv = returnNowLv + 1;
		}
		
		var MaxLv = $(this).attr("MaxLv");
		var Boss = $(this).attr("Boss");
		var ApproveCode = $(this).attr("ApproveCode");
		var ApproveName = $(this).attr("ApproveName");
		var RefNo = $(this).attr("RefNo");
		var Role = $(this).attr("Role");
		var iApprovedUrl = rootPath + "EAPPROVED/iApproved";
		var postData = {
		        'status': status,
		        'ApproveCode': ApproveCode,
		        'ApproveName': ApproveName,
		        'ApproveLevel': NowLv,
		        'RefNo': RefNo,
		        'Role': dd.val.join(),
		        'RoleNm': dd.valNm.join()
		    };
		CommonFunc.consoleMsg(postData);
		$.ajax({
		    async: true,
		    url: iApprovedUrl,
		    type: 'POST',
		    data: postData,
		    dataType: "json",
		    "error": function (xmlHttpRequest, errMsg) {
		    },
		    success: function (result) {
		    	if(result.message == "success")
		    	{
		    		var status = approveFunc.process(CountApprove, returnNowLv, ApproveCode, Boss);
		    		console.log(status);
		    		if(status === true)
		    		{
		    			genApproveTable($table, postUrl, approveCode, RefNo);
		    			CommonFunc.Notify("", "已进入下一流程", 500, "success");
		    		}
		    		else
		    		{
		    			$.ajax({
		    			    async: true,
		    			    url: iApprovedUrl,
		    			    type: 'POST',
		    			    data: {
						        'status': 0,
						        'ApproveCode': ApproveCode,
						        'ApproveName': ApproveName,
						        'ApproveLevel': NowLv,
						        'RefNo': RefNo,
						        'Role': Role
						    },
		    			    dataType: "json",
		    			    "error": function (xmlHttpRequest, errMsg) {
		    			    	return false;
		    			    },
		    			    success: function (result) {
		    			    	genApproveTable($table, postUrl, approveCode, RefNo);
		    			    	return false;
		    			    }

		    			});
		    		}
		    		
		    	}
		    	else
		    	{
		    		CommonFunc.Notify("", "写入数据库有误", 500, "warning");
		    	}
		    }

		});	
	});
});