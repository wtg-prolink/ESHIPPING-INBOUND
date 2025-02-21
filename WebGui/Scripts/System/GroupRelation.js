jQuery(document).ready(function($) {
    //var editable = false;
    MenuBarFuncArr.DelMenu(["MBEdoc", "MBApply", "MBApprove", "MBInvalid", "MBCopy", "MBEdit"]);
    var $newLi = $('<li class="list-group-item" style="display:none"><div class="input-group"><input type="text" class="form-control" name="TITLE"><span class="input-group-btn"><button class="btn btn-default REMOVE-INPUT" type="button"><span class="glyphicon glyphicon-remove"></span></button></span></div></li>');

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
            var $newLi = $('<li class="list-group-item" style="display:none"><div class="row"><div class="col-xs-12"><div class="form-group"><input type="text" class="form-control" id="groupID" name="groupID" placeholder="@Resources.Locale.L_GroupRelation_groupID"></div></div></div><div class="row"><div class="col-xs-12"><div class="form-group"><input type="text" class="form-control" id="groupName" name="groupName" placeholder="@Resources.Locale.L_BaseLookup_GroupName"></div></div></div><div class="row" style="text-align:center;"><button type="button" class="btn btn-xs btn-success GROUP-OK">@Resources.Locale.L_BSCSDateQuery_Confirm</button><button type="button" class="btn btn-xs btn-danger GROUP-CANCEL">@Resources.Locale.L_BSCSDateQuery_Cancel</button></div></li>');
            $('#GROUP').append($newLi)
            $newLi.show(200).children('selector');
            $("input[name='groupID']").setfocus();            
        }
    });

    $('.COMPANY-ADD').on('click', function(){

        if($('#GROUP li').hasClass('active'))
        {
            if($('#COMPANY').find('div.form-group').length == 0)
            {
                //var $newLi = $('<li class="list-group-item" style="display:none"><div class="input-group"><input type="text" class="form-control" name="TITLE"><span class="input-group-btn"><button class="btn btn-danger REMOVE-INPUT" type="button"><span class="glyphicon glyphicon-remove"></span></button></span></div></li>');
                var $newLi = $('<li class="list-group-item" style="display:none"><div class="row"><div class="col-xs-12"><div class="form-group"><input type="text" class="form-control" id="companyID" name="companyID" placeholder="@Resources.Locale.L_GroupRelation_comID"></div></div></div><div class="row"><div class="col-xs-12"><div class="form-group"><input type="text" class="form-control" id="companyName" name="companyName" placeholder="@Resources.Locale.L_SMIPM_CompanyNm"></div></div></div><div class="row" style="text-align:center;"><button type="button" class="btn btn-xs btn-success COMPANY-OK">@Resources.Locale.L_BSCSDateQuery_Confirm</button><button type="button" class="btn btn-xs btn-danger COMPANY-CANCEL">@Resources.Locale.L_BSCSDateQuery_Cancel</button></div></li>');
                
                $('#COMPANY').append($newLi)
                $newLi.show(200);
                $("input[name='companyID']").setfocus();
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
                var $newLi = $('<li class="list-group-item" style="display:none"><div class="row"><div class="col-xs-12"><div class="form-group"><input type="text" class="form-control" id="stationID" name="stationID" placeholder="@Resources.Locale.L_GroupRelation_Scripts_353"></div></div></div><div class="row"><div class="col-xs-12"><div class="form-group"><input type="text" class="form-control" id="stationName" name="stationName" placeholder="@Resources.Locale.L_GroupRelation_StnNm"></div></div></div><div class="row" style="text-align:center;"><button type="button" class="btn btn-xs btn-success STATION-OK">@Resources.Locale.L_BSCSDateQuery_Confirm</button><button type="button" class="btn btn-xs btn-danger STATION-CANCEL">@Resources.Locale.L_BSCSDateQuery_Cancel</button></div></li>');
                $('#STATION').append($newLi)
                $newLi.show(200);
                $("input[name='stationID']").setfocus();
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
                var $newLi = $('<li class="list-group-item" style="display:none"><div class="row"><div class="col-xs-12"><div class="form-group"><input type="text" class="form-control" id="depID" name="depID" placeholder="@Resources.Locale.L_GroupRelation_Scripts_354"></div></div></div><div class="row"><div class="col-xs-12"><div class="form-group"><input type="text" class="form-control" id="depName" name="depName" placeholder="@Resources.Locale.L_GroupRelation_DepNm"></div></div></div><div class="row" style="text-align:center;"><button type="button" class="btn btn-xs btn-success DEP-OK">@Resources.Locale.L_BSCSDateQuery_Confirm</button><button type="button" class="btn btn-xs btn-danger DEP-CANCEL">@Resources.Locale.L_BSCSDateQuery_Cancel</button></div></li>');
                $('#DEP').append($newLi)
                $newLi.show(200);
                $("input[name='depID']").setfocus();
            }
        }
        else
        {
            alert("@Resources.Locale.L_GroupRelation_PSelc");
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

    $("#GROUP").on('click', '.GROUP-OK', function(){
        var groupID = $('li.list-group-item input[name="groupID"]').val();
        var groupName = $('#groupName').val();
        if(groupID != '' || groupName != '')
        {
            var $reproduce = $('<li class="list-group-item style="display:none" groupID="'+groupID+'" newItem="1"><span>'+groupName+'</span><button class="btn btn-xs btn-default REMOVE-OLD" type="button" groupID="'+groupID+'"><span class="glyphicon glyphicon-remove"></span></button><button class="btn btn-xs btn-default EDIT-OLD" type="button"  data-toggle="modal" data-target="#groupModal" groupID="'+groupID+'" groupName="'+groupName+'"><span class="glyphicon glyphicon-pencil"></span></button></li>');
            

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
                localData.GROUP[groupID].__state        = 1;
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
            var $reproduce = $('<li class="list-group-item style="display:none" groupID="'+groupID+'" comID="'+companyID+'" newItem="1"><span>'+companyName+'</span><button class="btn btn-xs btn-default REMOVE-OLD" type="button" groupID="'+groupID+'" comID="'+companyID+'"><span class="glyphicon glyphicon-remove"></span></button><button class="btn btn-xs btn-default EDIT-OLD" type="button" data-toggle="modal" data-target="#comModal" comID="'+companyID+'" comName="'+companyName+'"><span class="glyphicon glyphicon-pencil"></span></button></li>');

            $(this).parents('li').hide(200, function(){
                $(this).remove();
                $("#COMPANY").append($reproduce);
                $reproduce.show(200);
            });

            if(typeof localData.CMP[companyID] === "undefined")
            {
                localData.CMP[companyID]                = {};
                localData.CMP[companyID].GroupId       = groupID;
                localData.CMP[companyID].Cmp            = companyID;
                localData.CMP[companyID].Stn            = "*";
                localData.CMP[companyID].Dep            = "*";
                localData.CMP[companyID].Type           = 1;
                localData.CMP[companyID].Name           = companyName;
                localData.CMP[companyID].__state   = 1;
            }
            else
            {
                localData.CMP[companyID].__state   = 1;
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
            var $reproduce = $('<li class="list-group-item style="display:none" groupID="'+groupID+'" comID="'+companyID+'" stnID="'+stationID+'" newItem="1"><span>'+stationName+'</span><button class="btn btn-xs btn-default REMOVE-OLD" type="button" groupID="'+groupID+'" comID="'+companyID+'" stnID="'+stationID+'"><span class="glyphicon glyphicon-remove"></span></button><button class="btn btn-xs btn-default EDIT-OLD" type="button" data-toggle="modal" data-target="#stnModal" stationID="'+stationID+'" stationName="'+stationName+'"><span class="glyphicon glyphicon-pencil"></span></button></li>');

            $(this).parents('li').hide(200, function(){
                $(this).remove();
                $("#STATION").append($reproduce);
                $reproduce.show(200);
            });

            if(typeof localData.STN[stationID] === "undefined")
            {
                localData.STN[stationID]                = {};
                localData.STN[stationID].GroupId       = groupID;
                localData.STN[stationID].Cmp            = companyID;
                localData.STN[stationID].Stn            = stationID;
                localData.STN[stationID].Dep            = "*";
                localData.STN[stationID].Type            = 2;
                localData.STN[stationID].Name           = stationName;
                localData.STN[stationID].__state   = 1;
            }
            else
            {
                localData.STN[stationID].__state   = 1;
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
            var $reproduce = $('<li class="list-group-item style="display:none" groupID="'+groupID+'" comID="'+companyID+'" stnID="'+stationID+'" depID="'+depID+'" newItem="1"><span>'+depName+'</span><button class="btn btn-xs btn-default REMOVE-OLD" type="button" groupID="'+groupID+'" comID="'+companyID+'" stnID="'+stationID+'" depID="'+depID+'"><span class="glyphicon glyphicon-remove"></span></button><button class="btn btn-xs btn-default EDIT-OLD" type="button" data-toggle="modal" data-target="#depModal" depID="'+depID+'" depName="'+depName+'"><span class="glyphicon glyphicon-pencil"></span></button></li>');

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
                localData.DEP[depID].__state   = 1;
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

    $('#groupModal').on('shown.bs.modal', function (e) {
        groupEditbtn = $(e.relatedTarget);
        var GroupID = groupEditbtn.attr('groupID');
        $(this).find("input").eq(0).setfocus();

        if(typeof localData.GROUP[GroupID] === "undefined")
        {
            $(this).find("input[type='text']").val('');
            $(this).find('#groupID').val(groupEditbtn.attr('groupID'));
            $(this).find('#groupCName').val(groupEditbtn.attr('groupName'));
            $(this).find('#groupEName').val(groupEditbtn.parent("li").attr('Ename'));
            $(this).find('#groupContact').val(groupEditbtn.parent("li").attr('Attn'));
            $(this).find('#groupTel').val(groupEditbtn.parent("li").attr('AttnTel'));
            $(this).find('#groupEmail').val(groupEditbtn.parent("li").attr('Email'));
            $(this).find('#groupWeb').val(groupEditbtn.parent("li").attr('Url'));
            $(this).find('#groupCaddr').val(groupEditbtn.parent("li").attr('Addr'));
            $(this).find('#groupEaddr').val(groupEditbtn.parent("li").attr('Eaddr'));

            if(groupEditbtn.parent("li").attr('Status') == 0)
            {
                $("#groupModal").find("input[value='0']").prop('checked');
            }
            else
            {
                $("#groupModal").find("input[value='1']").prop('checked');
            }

            $(this).find('#groupDesc').val(groupEditbtn.parent("li").attr('Descp'));
        }
        else
        {
            $(this).find('#groupID').val(localData.GROUP[GroupID].GroupId);
            $(this).find('#groupCName').val(localData.GROUP[GroupID].Name);
            $(this).find('#groupEName').val(localData.GROUP[GroupID].Ename);
            $(this).find('#groupContact').val(localData.GROUP[GroupID].Attn);
            $(this).find('#groupTel').val(localData.GROUP[GroupID].AttnTel);
            $(this).find('#groupEmail').val(localData.GROUP[GroupID].Email);
            $(this).find('#groupWeb').val(localData.GROUP[GroupID].Url);
            $(this).find('#groupCaddr').val(localData.GROUP[GroupID].Addr);
            $(this).find('#groupEaddr').val(localData.GROUP[GroupID].Eaddr);
            
            if(localData.GROUP[GroupID].STATUS == 0)
            {
                $("#groupModal").find("input[value='0']").prop('checked');
            }
            else
            {
                $("#groupModal").find("input[value='1']").prop('checked');
            }
            $(this).find('#groupDesc').val(localData.GROUP[GroupID].Descp);
        }
    });

    $('#comModal').on('shown.bs.modal', function (e) {
        comEditbtn = $(e.relatedTarget);
        var CMPID = comEditbtn.attr('comID');
        $(this).find("input").eq(0).setfocus();

        if(typeof localData.CMP[CMPID] === "undefined")
        {
            $(this).find("input[type='text']").val('');
            $(this).find('#comID').val(comEditbtn.attr('comID'));
            $(this).find('#comCName').val(comEditbtn.attr('comName'));
            $(this).find('#comEName').val(comEditbtn.parent("li").attr('Ename'));
            $(this).find('#comContact').val(comEditbtn.parent("li").attr('Attn'));
            $(this).find('#comTel').val(comEditbtn.parent("li").attr('AttnTel'));
            $(this).find('#comEmail').val(comEditbtn.parent("li").attr('Email'));
            $(this).find('#comWeb').val(comEditbtn.parent("li").attr('Url'));
            $(this).find('#comCaddr').val(comEditbtn.parent("li").attr('Addr'));
            $(this).find('#comEaddr').val(comEditbtn.parent("li").attr('Eaddr'));
        }
        else
        {
            $(this).find('#comID').val(localData.CMP[CMPID].Cmp);
            $(this).find('#comCName').val(localData.CMP[CMPID].Name);
            $(this).find('#comContact').val(localData.CMP[CMPID].Attn);
            $(this).find('#comTel').val(localData.CMP[CMPID].AttnTel);
            $(this).find('#comEmail').val(localData.CMP[CMPID].Email);
            $(this).find('#comWeb').val(localData.CMP[CMPID].Url);
            $(this).find('#comCaddr').val(localData.CMP[CMPID].Addr);
            $(this).find('#comEaddr').val(localData.CMP[CMPID].Eaddr);
            
            if(localData.CMP[CMPID].STATUS == 0)
            {
                $("#comModal").find("input[value='0']").prop('checked');
            }
            else
            {
                $("#comModal").find("input[value='1']").prop('checked');
            }
            $(this).find('#comDesc').val(localData.CMP[CMPID].Descp);
        }
    });

    $('#stnModal').on('shown.bs.modal', function (e) {
        stnEditbtn = $(e.relatedTarget);
        var STNID = stnEditbtn.attr('stnID');
        $(this).find("input").eq(0).setfocus();

        if(typeof localData.STN[STNID] === "undefined")
        {
            $(this).find("input[type='text']").val('');
            $(this).find('#stnID').val(stnEditbtn.attr('stnID'));
            $(this).find('#stnCName').val(stnEditbtn.attr('stnName'));
            $(this).find('#stnEName').val(stnEditbtn.parent("li").attr('Ename'));
            $(this).find('#stnContact').val(stnEditbtn.parent("li").attr('Attn'));
            $(this).find('#stnTel').val(stnEditbtn.parent("li").attr('AttnTel'));
            $(this).find('#stnEmail').val(stnEditbtn.parent("li").attr('Email'));
            $(this).find('#stnWeb').val(stnEditbtn.parent("li").attr('Url'));
            $(this).find('#stnCaddr').val(stnEditbtn.parent("li").attr('Addr'));
            $(this).find('#stnEaddr').val(stnEditbtn.parent("li").attr('Eaddr'));
        }
        else
        {
            $(this).find('#stnID').val(localData.STN[STNID].Stn);
            $(this).find('#stnCName').val(localData.STN[STNID].Name);
            $(this).find('#stnContact').val(localData.STN[STNID].Attn);
            $(this).find('#stnTel').val(localData.STN[STNID].AttnTel);
            $(this).find('#stnEmail').val(localData.STN[STNID].Email);
            $(this).find('#stnWeb').val(localData.STN[STNID].Url);
            $(this).find('#stnCaddr').val(localData.STN[STNID].Addr);
            $(this).find('#stnEaddr').val(localData.STN[STNID].Eaddr);
            
            if(localData.STN[STNID].STATUS == 0)
            {
                $("#stnModal").find("input[value='0']").prop('checked');
            }
            else
            {
                $("#stnModal").find("input[value='1']").prop('checked');
            }
            $(this).find('#stnDesc').val(localData.STN[STNID].Descp);
        }
    });

    $('#depModal').on('shown.bs.modal', function (e) {
        depEditbtn = $(e.relatedTarget);
        var DEPID = depEditbtn.attr('depID');
        $(this).find("input").eq(0).setfocus();
        if(typeof localData.DEP[DEPID] === "undefined")
        {
            $(this).find("input[type='text']").val('');
            $(this).find('#depID').val(depEditbtn.attr('depID'));
            $(this).find('#depCName').val(depEditbtn.attr('depName'));
            $(this).find('#depEName').val(depEditbtn.parent("li").attr('Ename'));
            $(this).find('#depContact').val(depEditbtn.parent("li").attr('Attn'));
            $(this).find('#depTel').val(depEditbtn.parent("li").attr('AttnTel'));
            $(this).find('#depEmail').val(depEditbtn.parent("li").attr('Email'));
            $(this).find('#depWeb').val(depEditbtn.parent("li").attr('Url'));
            $(this).find('#depCaddr').val(depEditbtn.parent("li").attr('Addr'));
            $(this).find('#depEaddr').val(depEditbtn.parent("li").attr('Eaddr'));
        }
        else
        {
            $(this).find('#depID').val(localData.DEP[DEPID].Dep);
            $(this).find('#depCName').val(localData.DEP[DEPID].Name);
            $(this).find('#depContact').val(localData.DEP[DEPID].Attn);
            $(this).find('#depTel').val(localData.DEP[DEPID].AttnTel);
            $(this).find('#depEmail').val(localData.DEP[DEPID].Email);
            $(this).find('#depWeb').val(localData.DEP[DEPID].Url);
            $(this).find('#depCaddr').val(localData.DEP[DEPID].Addr);
            $(this).find('#depEaddr').val(localData.DEP[DEPID].Eaddr);
            
            if(localData.DEP[DEPID].STATUS == 0)
            {
                $("#depModal").find("input[value='0']").prop('checked');
            }
            else
            {
                $("#depModal").find("input[value='1']").prop('checked');
            }
            $(this).find('#depDesc').val(localData.DEP[DEPID].Descp);
        }
    });

    $(".groupSave").on("click", function(){
        var tempData = new Object();
        //處理資料暫存
        tempData.GROUP_ID                                 = $("#groupModal").find('#groupID').val();
        localData.GROUP[tempData.GROUP_ID]                = {};
        localData.GROUP[tempData.GROUP_ID].GroupId        = $("#groupModal").find('#groupID').val();
        localData.GROUP[tempData.GROUP_ID].Cmp            = "*";
        localData.GROUP[tempData.GROUP_ID].Stn            = "*";
        localData.GROUP[tempData.GROUP_ID].Dep            = "*";
        localData.GROUP[tempData.GROUP_ID].Name           = $("#groupModal").find('#groupCName').val();
        localData.GROUP[tempData.GROUP_ID].Ename          = $("#groupModal").find('#groupEName').val();
        localData.GROUP[tempData.GROUP_ID].Type           = 0;
        localData.GROUP[tempData.GROUP_ID].Attn           = $("#groupModal").find("#groupContact").val();
        localData.GROUP[tempData.GROUP_ID].AttnTel        = $("#groupModal").find("#groupTel").val();
        localData.GROUP[tempData.GROUP_ID].Email          = $("#groupModal").find("#groupEmail").val();
        localData.GROUP[tempData.GROUP_ID].Url            = $("#groupModal").find("#groupWeb").val();
        localData.GROUP[tempData.GROUP_ID].Addr           = $("#groupModal").find("#groupCaddr").val();
        localData.GROUP[tempData.GROUP_ID].Eaddr          = $("#groupModal").find("#groupEaddr").val();
        localData.GROUP[tempData.GROUP_ID].Status         = $("#groupModal").find("input[name='groupStatus']:checked").val();
        localData.GROUP[tempData.GROUP_ID].Descp          = $("#groupModal").find("#groupDesc").val();
        localData.GROUP[tempData.GROUP_ID].Findex          = $("#GROUP li[groupID='"+tempData.GROUP_ID+"']").attr("FINDEX");
        if($("#GROUP li[groupID='"+tempData.GROUP_ID+"']").attr("newItem") == 1)
        {
            localData.GROUP[tempData.GROUP_ID].__state   = 1;
        }
        else
        {
            localData.GROUP[tempData.GROUP_ID].__state   = 2;
        }
        

        groupEditbtn.attr('groupName', localData.GROUP[tempData.GROUP_ID].NAME);
        groupEditbtn.parent("li span").html(localData.GROUP[tempData.GROUP_ID].NAME);

        $("#groupModal").find("input[name='text']").val("");

        $('#groupModal').modal('hide');
    });

    $(".comSave").on("click", function(){
        var tempData = new Object();
        //處理資料暫存
        tempData.CMP_ID                               = $("#comModal").find('#comID').val();
        localData.CMP[tempData.CMP_ID]                = {};
        localData.CMP[tempData.CMP_ID].GroupId        = $("#COMPANY li[comID='"+tempData.CMP_ID+"']").attr("groupID");
        localData.CMP[tempData.CMP_ID].Cmp            = $("#comModal").find('#comID').val();
        localData.CMP[tempData.CMP_ID].Stn            = "*";
        localData.CMP[tempData.CMP_ID].Name           = $("#comModal").find('#comCName').val();
        localData.CMP[tempData.CMP_ID].Ename          = $("#comModal").find('#comEName').val();
        localData.CMP[tempData.CMP_ID].Type           = 1;
        localData.CMP[tempData.CMP_ID].Attn           = $("#comModal").find("#comContact").val();
        localData.CMP[tempData.CMP_ID].AttnTel        = $("#comModal").find("#comTel").val();
        localData.CMP[tempData.CMP_ID].Email          = $("#comModal").find("#comEmail").val();
        localData.CMP[tempData.CMP_ID].Url            = $("#comModal").find("#comWeb").val();
        localData.CMP[tempData.CMP_ID].Addr           = $("#comModal").find("#comCaddr").val();
        localData.CMP[tempData.CMP_ID].Eaddr          = $("#comModal").find("#comEaddr").val();
        localData.CMP[tempData.CMP_ID].Status         = $("#comModal").find("input[name='comStatus']:checked").val();
        localData.CMP[tempData.CMP_ID].Descp          = $("#comModal").find("#comDesc").val();
        localData.CMP[tempData.CMP_ID].Findex          = $("#COMPANY li[comID='"+tempData.CMP_ID+"']").attr("FINDEX");
        if($("#COMPANY li[comID='"+tempData.CMP_ID+"']").attr("newItem") == 1)
        {
            localData.CMP[tempData.CMP_ID].__state   = 1;
        }
        else
        {
            localData.CMP[tempData.CMP_ID].__state   = 2;
        }

        comEditbtn.attr('comName', localData.CMP[tempData.CMP_ID].NAME);
        comEditbtn.parent("li span").html(localData.CMP[tempData.CMP_ID].NAME);

        $("#comModal").find("input[name='text']").val("");

        $('#compModal').modal('hide');
    });

    $(".stnSave").on("click", function(){
        var tempData = new Object();
        //處理資料暫存
        tempData.STN_ID                               = $("#stnModal").find('#stnID').val();
        localData.STN[tempData.STN_ID]                = {};
        localData.STN[tempData.STN_ID].GroupId        = $("#STATION li[stnID='"+tempData.STN_ID+"']").attr("groupID");
        localData.STN[tempData.STN_ID].Cmp            = $("#STATION li[stnID='"+tempData.STN_ID+"']").attr("comID");
        localData.STN[tempData.STN_ID].Stn            = $("#stnModal").find('#stnID').val();
        localData.STN[tempData.STN_ID].Dep            = "*";
        localData.STN[tempData.STN_ID].Name           = $("#stnModal").find('#stnCName').val();
        localData.STN[tempData.STN_ID].Ename          = $("#stnModal").find('#stnEName').val();
        localData.STN[tempData.STN_ID].Type           = 2;
        localData.STN[tempData.STN_ID].Attn           = $("#stnModal").find("#stnContact").val();
        localData.STN[tempData.STN_ID].AttnTel        = $("#stnModal").find("#stnTel").val();
        localData.STN[tempData.STN_ID].Email          = $("#stnModal").find("#stnEmail").val();
        localData.STN[tempData.STN_ID].Url            = $("#stnModal").find("#stnWeb").val();
        localData.STN[tempData.STN_ID].Addr           = $("#stnModal").find("#stnCaddr").val();
        localData.STN[tempData.STN_ID].Eaddr          = $("#stnModal").find("#stnEaddr").val();
        localData.STN[tempData.STN_ID].Status         = $("#stnModal").find("input[name='stnStatus']:checked").val();
        localData.STN[tempData.STN_ID].Descp          = $("#stnModal").find("#stnDesc").val();
        localData.STN[tempData.STN_ID].Findex         = $("#STATION li[stnID='"+tempData.STN_ID+"']").attr("FINDEX");
        if($("#STATION li[stnID='"+tempData.STN_ID+"']").attr("newItem"))
        {
            localData.STN[tempData.STN_ID].__state = 1;
        }
        else
        {
            localData.STN[tempData.STN_ID].__state = 2;
        }

        stnEditbtn.attr('stnName', localData.STN[tempData.STN_ID].NAME);
        stnEditbtn.parent("li span").html(localData.STN[tempData.STN_ID].NAME)

        $("#stnModal").find("input[name='text']").val("");

        $('#stnModal').modal('hide');
    });

    $(".depSave").on("click", function(){
        var tempData = new Object();
        //處理資料暫存
        tempData.DEP_ID                               = $("#depModal").find('#depID').val();
        localData.DEP[tempData.DEP_ID]                = {};
        localData.DEP[tempData.DEP_ID].GroupId        = $("#DEP li[depID='"+tempData.DEP_ID+"']").attr("groupID");
        localData.DEP[tempData.DEP_ID].Cmp            = $("#DEP li[depID='"+tempData.DEP_ID+"']").attr("comID");
        localData.DEP[tempData.DEP_ID].Stn            = $("#DEP li[depID='"+tempData.DEP_ID+"']").attr("stnID");
        localData.DEP[tempData.DEP_ID].Dep            = $("#depModal").find('#depID').val();
        localData.DEP[tempData.DEP_ID].Name           = $("#depModal").find('#depCName').val();
        localData.DEP[tempData.DEP_ID].Ename          = $("#depModal").find('#depEName').val();
        localData.DEP[tempData.DEP_ID].Type           = 3;
        localData.DEP[tempData.DEP_ID].Attn           = $("#depModal").find("#depContact").val();
        localData.DEP[tempData.DEP_ID].AttnTel        = $("#depModal").find("#depTel").val();
        localData.DEP[tempData.DEP_ID].Email          = $("#depModal").find("#depEmail").val();
        localData.DEP[tempData.DEP_ID].Url            = $("#depModal").find("#depWeb").val();
        localData.DEP[tempData.DEP_ID].Addr           = $("#depModal").find("#depCaddr").val();
        localData.DEP[tempData.DEP_ID].Eaddr          = $("#depModal").find("#depEaddr").val();
        localData.DEP[tempData.DEP_ID].Status         = $("#depModal").find("input[name='depStatus']:checked").val();
        localData.DEP[tempData.DEP_ID].Descp          = $("#depModal").find("#depDesc").val();
        localData.DEP[tempData.DEP_ID].Findex          = $("#DEP li[depID='"+tempData.DEP_ID+"']").attr("FINDEX");
        if($("#DEP li[depID='"+tempData.DEP_ID+"']").attr("newItem") == 1)
        {
            localData.DEP[tempData.DEP_ID].__state = 1;
        }
        else
        {
             localData.DEP[tempData.DEP_ID].__state = 2;
        }

        depEditbtn.attr('depName', localData.DEP[tempData.DEP_ID].NAME);
        depEditbtn.parent("li span").html(localData.DEP[tempData.DEP_ID].NAME);

        $("#depModal").find("input[name='text']").val("");

        $('#depModal').modal('hide');
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
                //editable = false;
                //setdisabled(true);
                //setToolBtnDisabled(true);
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

    MenuBarFuncArr.MBEdit = function (){
        //editable = true;
        MenuBarFuncArr.Enabled(["MBSave", "MBCancel"]);
        MenuBarFuncArr.Disabled(["MBEdit"]);
    }


    MenuBarFuncArr.DelMenu(["MBSearch","MBAdd","MBDel"]);
    
    initMenuBar(MenuBarFuncArr);
    MenuBarFuncArr.Enabled(["MBSave", "MBCancel"]);
    //setdisabled(true);
    //setToolBtnDisabled(true);
    //MenuBarFuncArr.Enabled(["MBEdit"]);
});

function ClearData()
{
    postData = [];
    localData.GROUP = {};
    localData.CMP   = {};
    localData.STN   = {};
    localData.DEP   = {};
}