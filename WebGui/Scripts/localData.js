var localDB = {};

localDB.createTable = function(tableName, pk, oldData){
	localDB[tableName] = {};
	localDB[tableName]["pk"] = pk;
	localDB[tableName]["newData"] = {};
	localDB[tableName]["oldData"] = {};

	$.each(oldData, function(i, val) {
		localDB[tableName]["oldData"][oldData[i][pk]] = oldData[i];
	});
}

localDB.actionFunc = function(tableName, data, action){
	var pk = localDB[tableName]["pk"];
	var thisUid = data[pk];
	if(action == "INSERT")
	{
		data["__state"] = "1";
		localDB[tableName]["newData"][thisUid] = {};
		localDB[tableName]["newData"][thisUid] = data;
		return;

	}
	else if(action === "UPDATE")
	{
		if(typeof localDB[tableName]["oldData"][thisUid] === "object")
		{
			var oldData = localDB[tableName]["oldData"][data[pk]];
			if(typeof localDB[tableName]["newData"][thisUid] !== "object")
			{
				localDB[tableName]["newData"][thisUid] = {};
			}

			localDB[tableName]["newData"][thisUid]["__state"] = "2";
			localDB[tableName]["newData"][thisUid][pk] = thisUid;
			$.each(data, function(index, val) {
				if(!oldData[index])
				{
					oldData[index] = "";
				}
				if(!data[index])
				{
					oldData[index] = "";
				}
				if(data[index] != oldData[index])
				{
					localDB[tableName]["newData"][thisUid][index] = data[index];
				}
			});

		}
		else
		{
			localDB[tableName]["newData"][thisUid] = {};
			data["__state"] = "1";
			localDB[tableName]["newData"][thisUid] = data;
			
		}
		return;

	}
	else if(action === "DELETE")
	{
		if(typeof localDB[tableName]["newData"][thisUid] === "object")
		{
			data["__state"] = "0";
			localDB[tableName]["newData"][thisUid] = {};
			localDB[tableName]["newData"][thisUid][pk] = data[pk];
			localDB[tableName]["newData"][thisUid]["__state"] = data["__state"];
		}
		else
		{
			localDB[tableName]["newData"][thisUid] = {};
			localDB[tableName]["newData"][thisUid][pk] = thisUid;
			localDB[tableName]["newData"][thisUid]["__state"] = "0";
		}

		return;
	}
}

localDB.getChangeData = function(tableName) {
	var newData = localDB[tableName]["newData"];
	var returnData = [];

	$.each(newData, function(index, val) {
		returnData.push(newData[index]);
	});
	return returnData;
}

var stMenuBarFunc = {
    STAdd: function(){},
    STEdit: function(){},
    STCopy: function(){},
    STDel: function(){},
    STCancel: function(){},
    STSave: function(){}
}

stMenuBarFunc.DelMenu = function (menuIdArr) {

    $.each(menuIdArr, function (key, value) {
        $("#" + value).parent().addClass("nav-hidden");
    });
}

stMenuBarFunc.Disabled = function (menuIdArr) {

    $.each(menuIdArr, function (key, value) {
        $("#" + value).addClass("nav-disabled");
        $("#" + value).prop("disabled", true);
    });
}

stMenuBarFunc.Enabled = function (menuIdArr) {

    $.each(menuIdArr, function (key, value) {
        $("#" + value).removeClass("nav-disabled");
        $("#" + value).prop("disabled", false);
    });
}

stMenuBarFunc.DisabledAll = function () {
    $("button.menuBar").prop("disabled", true);
}

function _initSubMenubar(stMenuBarFunc)
{
    $("#STAdd").click(function(){
        $("#subFormData")[0].reset();
        subStatus = "add";
        stMenuBarFunc.Enabled(["STSave", "STCancel"]);
        stMenuBarFunc.Disabled(["STAdd", "STEdit", "STCopy"]);
        setSTdisabled(false);
        stMenuBarFunc.STAdd();
    });

    $("#STCopy").click(function(){
        var s = stMenuBarFunc.STCopy();
        if(s === false)
        {
        	return;
        }
        else
        {
        	subStatus = "copy";
        	stMenuBarFunc.Enabled(["STSave", "STCancel"]);
        	stMenuBarFunc.Disabled(["STAdd", "STEdit", "STCopy"]);
        	setSTdisabled(false);
        }
    });

    $("#STEdit").click(function(){
        var s = stMenuBarFunc.STEdit();
        if(s === false)
        {
        	return;
        }
        else
        {
        	subStatus = "edit";
        	stMenuBarFunc.Enabled(["STSave", "STCancel"]);
        	stMenuBarFunc.Disabled(["STAdd", "STEdit", "STCopy"]);
        	setSTdisabled(false);
        }
    });

    $("#STDel").click(function(){
        subStatus = "del";
        stMenuBarFunc.STDel();
        $("#subFormData")[0].reset();
    });

    $("#STCancel").click(function(){
        stMenuBarFunc.Disabled(["STSave", "STCancel"]);
        stMenuBarFunc.Enabled(["STAdd", "STEdit", "STCopy"]);
        setSTdisabled(true);
        stMenuBarFunc.STCancel();
        subStatus = "";
    });

    $("#STSave").click(function(){
        stMenuBarFunc.Disabled(["STSave", "STCancel"]);
        stMenuBarFunc.Enabled(["STAdd", "STEdit", "STCopy"]);
        stMenuBarFunc.STSave();
        subStatus = "";
    });
}