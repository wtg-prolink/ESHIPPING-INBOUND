jQuery(document).ready(function($) {
	MenuBarFuncArr.DelMenu(["MBEdoc", "MBApply", "MBApprove", "MBInvalid", "MBCopy", "MBEdit", "MBAdd", "MBDel", "MBSearch", "MBErrMsg"]);

	/***** 按集團叫出公司 *****/
	$('#GROUP').on('click', 'li',function(e){
	    var target = $(e.target);
	    if(target.hasClass('list-group-item'))
	    {
	        $(this).siblings('li').removeClass('active');
	        $(this).addClass('active');

	        apiUrl = getSiteUrl + "/1?groupId=" + target.attr('groupID');
	        console.log(apiUrl);
	        $.post(apiUrl, function(data, textStatus, xhr) {
	            var tempData = [];
	            $('#COMPANY').children('li').remove();
	            $('#STATION').children('li').remove();
	            $('#DEP').children('li').remove();
	            var source    = $("#com-template").html();
	            var template  = Handlebars.compile(source);
	            
	            if(data.length > 0)
	            {
	                var html      = template(data);
	            }
	            else
	            {
	                $.each(localData.CMP, function(index, val) {
	                    if(localData.CMP[index].GroupId == target.attr('groupID'))
	                        tempData.push(localData.CMP[index]);
	                });

	                var html = template(tempData);
	            }

	            $('#COMPANY').html(html);

	        }, "JSON");
	    }
	});
	/***** 按集團叫出公司 END *****/

	/***** 按公司叫出站別 *****/
	$('#COMPANY').on('click', 'li', function(e){
	    var target = $(e.target);
	    if(target.hasClass('list-group-item'))
	    {
	        $(this).siblings('li').removeClass('active');
	        $(this).addClass('active');

	        apiUrl = getSiteUrl + "/2?groupId=" + target.attr('groupID') + "&companyId=" + target.attr('comID');
	        $.post(apiUrl, function(data, textStatus, xhr) {
	            var tempData = [];
	            $('#STATION').children('li').remove();
	            $('#DEP').children('li').remove();
	            var source    = $("#stn-template").html();
	            var template  = Handlebars.compile(source);
	            if(data.length > 0)
	            {
	                $.each(localData.STN, function(index, val) {
	                    if(localData.STN[index].GroupId == target.attr('groupID') && localData.STN[index].Cmp == target.attr('comID') && localData.STN[index].__state == 1)
	                        data.push(localData.STN[index]);
	                });
	                var html      = template(data);
	            }
	            else
	            {
	                $.each(localData.STN, function(index, val) {
	                    if(localData.STN[index].GroupId == target.attr('groupID') && localData.STN[index].Cmp == target.attr('comID'))
	                        tempData.push(localData.STN[index]);
	                });

	                var html = template(tempData);
	            }

	            $('#STATION').html(html);

	        }, "JSON");
	    }
	});
	/***** 按公司叫出站別 END *****/

	/***** 按站別叫出部門 *****/
	$('#STATION').on('click', 'li', function(e){
	    var target = $(e.target);
	    if(target.hasClass('list-group-item'))
	    {
	        $(this).siblings('li').removeClass('active');
	        $(this).addClass('active');

	        apiUrl = getSiteUrl + "/3?groupId=" + target.attr('groupID') + "&companyId=" + target.attr('comID') + "&stnId=" + target.attr('stnID');
	        $.post(apiUrl, function(data, textStatus, xhr) {
	            var tempData = [];
	            $('#DEP').children('li').remove();
	            var source    = $("#dep-template").html();
	            var template  = Handlebars.compile(source);

	            if(data.length > 0)
	            {
	                $.each(localData.DEP, function(index, val) {
	                    if(localData.DEP[index].GroupId == target.attr('groupID') && localData.DEP[index].Cmp == target.attr('comID') && localData.DEP[index].Stn == target.attr('stnID') && localData.DEP[index].__state == 1)
	                        data.push(localData.DEP[index]);
	                });
	                var html      = template(data);
	            }
	            else
	            {
	                $.each(localData.DEP, function(index, val) {
	                    if(localData.DEP[index].GroupId == target.attr('groupID') && localData.DEP[index].Cmp == target.attr('comID') && localData.DEP[index].Stn == target.attr('stnID'))
	                        tempData.push(localData.DEP[index]);
	                });

	                var html = template(tempData);
	            }
	            
	            $('#DEP').html(html);

	        }, "JSON");
	    }
	});
	/***** 按站別叫出部門 END *****/


	$('.GROUP-ADD').on('click', function(){

	    if($('#GROUP').find('div.form-group').length == 0)
	    {
	        var lookupStr = '<div class="input-group"><input type="text" class="form-control input-sm" id="groupName" name="groupName" fieldname="groupName" placeholder="@Resources.Locale.L_BaseLookup_GroupName" readonly /><span class="input-group-btn"><button class="btn btn-sm btn-default" type="button" id="groupIDLookup"><span class="glyphicon glyphicon-search"></span></button></span></div><input type="hidden" id="groupID" name="groupID">';
	        var $newLi = $('<li class="list-group-item" style="display:none"><div class="row"><div class="col-xs-12"><div class="form-group">'+lookupStr+'</div></div></div><div class="row" style="text-align:center;"><button type="button" class="btn btn-xs btn-success GROUP-OK">@Resources.Locale.L_BSCSDateQuery_Confirm</button><button type="button" class="btn btn-xs btn-danger GROUP-CANCEL">@Resources.Locale.L_BSCSDateQuery_Cancel</button></div></li>');
	        $('#GROUP').append($newLi)
	        $newLi.show(200).children('selector');
	        $("input[name='groupID']").setfocus();

	        genLookup("groupID", "groupName", "groupIDLookup", "GP");
	    }
	});

	$('.COMPANY-ADD').on('click', function(){

	    if($('#GROUP li').hasClass('active'))
	    {
	        if($('#COMPANY').find('div.form-group').length == 0)
	        {
	            //var $newLi = $('<li class="list-group-item" style="display:none"><div class="input-group"><input type="text" class="form-control" name="TITLE"><span class="input-group-btn"><button class="btn btn-danger REMOVE-INPUT" type="button"><span class="glyphicon glyphicon-remove"></span></button></span></div></li>');
    	        var lookupStr = '<div class="input-group"><input type="text" class="form-control input-sm" id="companyName" name="companyName" fieldname="companyName" placeholder="Loaction Name" readonly /><span class="input-group-btn"><button class="btn btn-sm btn-default" type="button" id="cmpIDLookup"><span class="glyphicon glyphicon-search"></span></button></span></div><input type="hidden" id="companyID" name="companyID">';
	            var $newLi = $('<li class="list-group-item" style="display:none"><div class="row"><div class="col-xs-12"><div class="form-group">'+lookupStr+'</div></div></div><div class="row" style="text-align:center;"><button type="button" class="btn btn-xs btn-success COMPANY-OK">@Resources.Locale.L_BSCSDateQuery_Confirm</button><button type="button" class="btn btn-xs btn-danger COMPANY-CANCEL">@Resources.Locale.L_BSCSDateQuery_Cancel</button></div></li>');
	            
	            $('#COMPANY').append($newLi)
	            $newLi.show(200);
	            $("input[name='companyID']").setfocus();

	            genLookup("companyID", "companyName", "cmpIDLookup", "LC");
	        }                    
	    }
	    else
	    {
	        alert("@Resources.Locale.L_GroupRelation_PSelcG");
	    }

	});

	$('.STATION-ADD').on('click', function(){

	    if($('#COMPANY li').hasClass('active'))
	    {
	        if($('#STATION').find('div.form-group').length == 0)
	        {
	            //var $newLi = $('<li class="list-group-item" style="display:none"><div class="input-group"><input type="text" class="form-control" name="TITLE"><span class="input-group-btn"><button class="btn btn-danger REMOVE-INPUT" type="button"><span class="glyphicon glyphicon-remove"></span></button></span></div></li>');
	            var lookupStr = '<div class="input-group"><input type="text" class="form-control input-sm" id="stationName" name="stationName" fieldname="stationName" placeholder="@Resources.Locale.L_GroupRelation_StnNm" readonly /><span class="input-group-btn"><button class="btn btn-sm btn-default" type="button" id="stnIDLookup"><span class="glyphicon glyphicon-search"></span></button></span></div><input type="hidden" id="stationID" name="stationID">';
	            var $newLi = $('<li class="list-group-item" style="display:none"><div class="row"><div class="col-xs-12"><div class="form-group">'+lookupStr+'</div></div></div><div class="row" style="text-align:center;"><button type="button" class="btn btn-xs btn-success STATION-OK">@Resources.Locale.L_BSCSDateQuery_Confirm</button><button type="button" class="btn btn-xs btn-danger STATION-CANCEL">@Resources.Locale.L_BSCSDateQuery_Cancel</button></div></li>');
	            $('#STATION').append($newLi)
	            $newLi.show(200);
	            $("input[name='stationID']").setfocus();

	            genLookup("stationID", "stationName", "stnIDLookup", "LC");
	        }
	    }
	    else
	    {
	        alert("@Resources.Locale.L_GroupRelation_PSelcC");
	    }

	});

	$('.DEP-ADD').on('click', function(){

	    if($('#STATION li').hasClass('active'))
	    {
	        if($('#STATION').find('div.form-group').length == 0)
	        {
	            //var $newLi = $('<li class="list-group-item" style="display:none"><div class="input-group"><input type="text" class="form-control" name="TITLE"><span class="input-group-btn"><button class="btn btn-danger REMOVE-INPUT" type="button"><span class="glyphicon glyphicon-remove"></span></button></span></div></li>');
	            var lookupStr = '<div class="input-group"><input type="text" class="form-control input-sm" id="depName" name="depName" fieldname="depName" placeholder="@Resources.Locale.L_GroupRelation_DepNm" readonly /><span class="input-group-btn"><button class="btn btn-sm btn-default" type="button" id="depIDLookup"><span class="glyphicon glyphicon-search"></span></button></span></div><input type="hidden" id="depID" name="depID">';
				var $newLi = $('<li class="list-group-item" style="display:none"><div class="row"><div class="col-xs-12"><div class="form-group">'+lookupStr+'</div></div></div><div class="row" style="text-align:center;"><button type="button" class="btn btn-xs btn-success DEP-OK">@Resources.Locale.L_BSCSDateQuery_Confirm</button><button type="button" class="btn btn-xs btn-danger DEP-CANCEL">@Resources.Locale.L_BSCSDateQuery_Cancel</button></div></li>');
	            $('#DEP').append($newLi)
	            $newLi.show(200);
	            $("input[name='depID']").setfocus();

	            genLookup("depID", "depName", "depIDLookup", "DE");
	        }
	    }
	    else
	    {
	        alert("@Resources.Locale.L_GroupRelation_PSelc");
	    }

	});

	$("#GROUP").on('click', '.GROUP-OK', function(){
		var oldGroupId = $
	    var groupID = $('li.list-group-item input[name="groupID"]').val();
	    var groupName = $('#groupName').val();
	    if(groupID != '' || groupName != '')
	    {
	        var $reproduce = $('<li class="list-group-item style="display:none" groupID="'+groupID+'" newItem="1"><span>'+groupName+'</span><button class="btn btn-xs btn-default REMOVE-OLD" type="button" groupID="'+groupID+'"><span class="glyphicon glyphicon-remove"></span></button></li>');
	        

	        $(this).parents('li').hide(200, function(){
	            $(this).remove();
	            $("#GROUP").append($reproduce);
	            $reproduce.show(200);
	        });

	        if(typeof localData.GROUP[groupID] === "undefined")
	        {
	            localData.GROUP[groupID]                = {};
	            localData.GROUP[groupID].GroupId        = groupID;
	            localData.GROUP[groupID].Name           = groupName;
	            localData.GROUP[groupID].Cmp            = "*";
	            localData.GROUP[groupID].Stn            = "*";
	            localData.GROUP[groupID].Dep            = "*";
	            localData.GROUP[groupID].Type           = 0;
	            localData.GROUP[groupID].__state        = 1;
	        }
	        else
	        {
	            delete localData.GROUP[groupID]
	        }                        
	    }
	    else
	    {
	        alert("@Resources.Locale.L_GroupRelation_PEntCdNm");
	    }
	});

	$("#COMPANY").on('click', '.COMPANY-OK', function(){
	    var groupID = $('#GROUP').find(".active").attr("groupID");
	    var companyID = $('li.list-group-item input[name="companyID"]').val();
	    var companyName = $('#companyName').val();

	    if(companyID != '' || companyName != '')
	    {
			var $reproduce = $('<li class="list-group-item style="display:none" groupID="' + groupID + '" comID="' + companyID + '" newItem="1"><span>' + companyID + ':' + companyName + '</span><button class="btn btn-xs btn-default REMOVE-OLD" type="button" groupID="' + groupID + '" comID="' + companyID + '"><span class="glyphicon glyphicon-remove"></span></button></li>');

	        $(this).parents('li').hide(200, function(){
	            $(this).remove();
	            $("#COMPANY").append($reproduce);
	            $reproduce.show(200);
	        });

	        if(typeof localData.CMP[companyID] === "undefined")
	        {
	            localData.CMP[companyID]                = {};
	            localData.CMP[companyID].GroupId        = groupID;
	            localData.CMP[companyID].Cmp            = companyID;
	            localData.CMP[companyID].Stn            = "*";
	            localData.CMP[companyID].Dep            = "*";
	            localData.CMP[companyID].Type           = 1;
	            localData.CMP[companyID].Name           = companyName;
	            localData.CMP[companyID].__state   		= 1;
	        }
	        else
	        {
	            delete localData.CMP[companyID]
	        }
	    }
	    else
	    {
	        alert("@Resources.Locale.L_GroupRelation_PEntCdNm");
	    }

	});

	$("#STATION").on('click', '.STATION-OK', function(){
	    var groupID     = $('#GROUP').find(".active").attr("groupID");
	    var companyID   = $('#COMPANY').find(".active").attr("comID");
	    var stationID   = $('li.list-group-item input[name="stationID"]').val();
	    var stationName = $('#stationName').val();

	    if(stationID != '' || stationName != '')
	    {
	        var $reproduce = $('<li class="list-group-item style="display:none" groupID="'+groupID+'" comID="'+companyID+'" stnID="'+stationID+'" newItem="1"><span>'+stationName+'</span><button class="btn btn-xs btn-default REMOVE-OLD" type="button" groupID="'+groupID+'" comID="'+companyID+'" stnID="'+stationID+'"><span class="glyphicon glyphicon-remove"></span></button></li>');

	        $(this).parents('li').hide(200, function(){
	            $(this).remove();
	            $("#STATION").append($reproduce);
	            $reproduce.show(200);
	        });

	        if(typeof localData.STN[stationID] === "undefined")
	        {
	            localData.STN[stationID]                = {};
	            localData.STN[stationID].GroupId        = groupID;
	            localData.STN[stationID].Cmp            = companyID;
	            localData.STN[stationID].Stn            = stationID;
	            localData.STN[stationID].Dep            = "*";
	            localData.STN[stationID].Type           = 2;
	            localData.STN[stationID].Name           = stationName;
	            localData.STN[stationID].__state   		= 1;
	        }
	        else
	        {
	            delete localData.STN[stationID]
	        }
	    }
	    else
	    {
	        alert("@Resources.Locale.L_GroupRelation_PEntCdNm");
	    }
	});

	$("#DEP").on('click', '.DEP-OK', function(){
	    var groupID     = $('#GROUP').find(".active").attr("groupID");
	    var companyID   = $('#COMPANY').find(".active").attr("comID");
	    var stationID   = $('#STATION').find(".active").attr("stnID");
	    var depID       = $('li.list-group-item input[name="depID"]').val();
	    var depName     = $('#depName').val();

	    if(depID != '' || depName != '')
	    {
	        var $reproduce = $('<li class="list-group-item style="display:none" groupID="'+groupID+'" comID="'+companyID+'" stnID="'+stationID+'" depID="'+depID+'" newItem="1"><span>'+depName+'</span><button class="btn btn-xs btn-default REMOVE-OLD" type="button" groupID="'+groupID+'" comID="'+companyID+'" stnID="'+stationID+'" depID="'+depID+'"><span class="glyphicon glyphicon-remove"></span></button></li>');

	        $(this).parents('li').hide(200, function(){
	            $(this).remove();
	            $("#DEP").append($reproduce);
	            $reproduce.show(200);
	        });

	        if(typeof localData.DEP[depID] === "undefined")
	        {
	            localData.DEP[depID]                = {};
	            localData.DEP[depID].GroupId       = groupID;
	            localData.DEP[depID].Cmp            = companyID;
	            localData.DEP[depID].Stn            = stationID;
	            localData.DEP[depID].Dep            = depID;
	            localData.DEP[depID].Type           = 3;
	            localData.DEP[depID].Name           = depName;
	            localData.DEP[depID].__state   = 1;
	        }
	        else
	        {
	            delete localData.DEP[depID]
	        }
	    }
	    else
	    {
	        alert("@Resources.Locale.L_GroupRelation_PEntCdNm");
	    }
	});

	$("#GROUP").on('click', '.REMOVE-OLD', function(e){
	    var target = $(e.target);
	    if(target.hasClass('REMOVE-OLD') || target.hasClass('glyphicon-remove'))
	    {
	        var groupID = $(this).attr("groupID");
	        if(typeof localData.GROUP[groupID] === "undefined")
	        {
	            localData.GROUP[groupID]                = {};
	            localData.GROUP[groupID].GroupId       = groupID;
	            localData.GROUP[groupID].__state   = 0;
	        }
	        else
	        {
	            localData.GROUP[groupID].__state   = 0;
	        }
	        $('#COMPANY').children('li').remove();
	        $('#STATION').children('li').remove();
	        $(this).parent('li').hide(200, function(){
	            $(this).remove();
	        });
	    }

	});

	$("#COMPANY").on('click', '.REMOVE-OLD', function(e){
	    var target = $(e.target);

	    if(target.hasClass('REMOVE-OLD') || target.hasClass('glyphicon-remove'))
	    {
	        var comID = $(this).attr("comID");
	        var groupID = $(this).attr("groupID");
	        $('#STATION').children('li').remove();
	        $(this).parent('li').hide(200, function(){
	            $(this).remove();
	        });

	        if(typeof localData.CMP[comID] === "undefined")
	        {
	            localData.CMP[comID]              = {};
	            localData.CMP[comID].GroupId     = groupID;
	            localData.CMP[comID].Cmp          = comID;
	            localData.CMP[comID].__state      = 0;
	        }
	        else
	        {
	            localData.CMP[comID].__state   = 0;
	        }
	    }
	    

	});

	$("#STATION").on('click', '.REMOVE-OLD', function(e){
	    var target = $(e.target);

	    if(target.hasClass('REMOVE-OLD') || target.hasClass('glyphicon-remove'))
	    {
	        var stnID = $(this).attr("stnID");
	        var comID = $(this).attr("comID");
	        var groupID = $(this).attr("groupID");
	        $('#DEP').children('li').remove();
	        $(this).parent('li').hide(200, function(){
	            $(this).remove();
	        });

	        if(typeof localData.STN[stnID] === "undefined")
	        {
	            localData.STN[stnID]                = {};
	            localData.STN[stnID].GroupId       = groupID;
	            localData.STN[stnID].Cmp            = comID;
	            localData.STN[stnID].Stn            = stnID;
	            localData.STN[stnID].__state   = 0;
	        }
	        else
	        {
	            localData.STN[stnID].__state   = 0;
	        }
	    }

	});

	$("#DEP").on('click', '.REMOVE-OLD', function(e){
	    var target = $(e.target);

	    if(target.hasClass('REMOVE-OLD') || target.hasClass('glyphicon-remove'))
	    {
	        var depID = $(this).attr("depID");
	        var stnID = $(this).attr("stnID");
	        var comID = $(this).attr("comID");
	        var groupID = $(this).attr("groupID");
	        $(this).parent('li').hide(200, function(){
	            $(this).remove();
	        });

	        if(typeof localData.DEP[depID] === "undefined")
	        {
	            localData.DEP[depID]                = {};
	            localData.DEP[depID].GroupId       = groupID;
	            localData.DEP[depID].Cmp            = comID;
	            localData.DEP[depID].Stn            = stnID;
	            localData.DEP[depID].Dep            = depID;
	            localData.DEP[depID].__state        = 0;
	        }
	        else
	        {
	            localData.DEP[depID].__state   = 0;
	        }
	    }

	});

	$("#GROUP").on('click', '.GROUP-CANCEL', function(){
	    $(this).parents('li').hide(200, function(){
	        $(this).remove();
	    });

	});

	$("#COMPANY").on('click', '.COMPANY-CANCEL', function(){
	    $(this).parents('li').hide(200, function(){
	        $(this).remove();
	    });

	});

	$("#STATION").on('click', '.STATION-CANCEL', function(){
	    $(this).parents('li').hide(200, function(){
	        $(this).remove();
	    });

	});

	$("#DEP").on('click', '.DEP-CANCEL', function(){
	    $(this).parents('li').hide(200, function(){
	        $(this).remove();
	    });

	});

	
	MenuBarFuncArr.MBSave = function (dtd) {
	    var postData = [];
	    $.each(localData.GROUP, function(index, val) {
	        postData.push(localData.GROUP[index]);
	    });

	    $.each(localData.CMP, function(index, val) {
	        postData.push(localData.CMP[index]);
	    });

	    $.each(localData.STN, function(index, val) {
	        postData.push(localData.STN[index]);
	    });

	    $.each(localData.DEP, function(index, val) {
	        postData.push(localData.DEP[index]);
	    });

	    console.log(postData);

	    $.post(postUrl, { changedData: JSON.stringify(postData) }, function (data, textStatus, xhr) {
	        if(data.status == 1)
	        {
	            CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
	            MenuBarFuncArr.SaveResult = true;
	            dtd.resolve();

	            MenuBarFuncArr.Enabled(["MBEdit"]);
	            ClearData();
	            setTimeout(function(){location.reload();}, 1000);
	        }
	        else
	        {
	            //notice ajax warning 一定要放入下面三行
	            CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
	            MenuBarFuncArr.SaveResult = false;
	            dtd.resolve();

	            return;
	        }
	    }, "json");

	    return;
	}

	MenuBarFuncArr.MBCancel = function (){
	    location.reload();
	}

	initMenuBar(MenuBarFuncArr);
	MenuBarFuncArr.Enabled(["MBSave", "MBCancel"]);


	var groupData = JSON.parse(groupStr.replace(/&quot;/g, '"'));
	var source    = $("#group-template").html();
	var template  = Handlebars.compile(source);
	var html      = template(groupData);
	$('#GROUP').html(html);
});


function genLookup(id, name, lookup, PartyType)
{
	if(PartyType != "DE")
	{
		var options = {};
		options.gridUrl = rootPath + "TPVCommon/GetGroupData";
		options.registerBtn = $("#" + lookup);
		options.isMutiSel = true;
		options.param = "";
		options.baseCondition = " PARTY_TYPE LIKE '%" + PartyType + "%'";
		options.gridFunc = function (map) {
		    var PartyNo = map.PartyNo,
		        PartyName = map.PartyName;
		    console.log(map);
		    $("#" + id).val(PartyNo);
		    $("#" + name).val(PartyName);
		}

		options.lookUpConfig = LookUpConfig.SmptyLookup;
		initLookUp(options);
	}
	else
	{
		var options = {};
		options.gridUrl = rootPath + "Common/GetDepData";
		options.registerBtn = $("#" + lookup);
		options.isMutiSel = true;
		options.param = "";
		options.gridFunc = function (map) {
		    var Cd = map.Cd,
		        CdDescp = map.CdDescp;
		    console.log(map);
		    $("#" + id).val(Cd);
		    $("#" + name).val(CdDescp);
		}

		options.lookUpConfig = LookUpConfig.DepLookup;
		initLookUp(options);
	}
	
}

function ClearData()
{
    postData = [];
    localData.GROUP = {};
    localData.CMP   = {};
    localData.STN   = {};
    localData.DEP   = {};
}