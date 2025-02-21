var navData, config, tempNav = Array;

BUI.use('common/main', function () {
    $.ajax({
        url: '@Url.Content("~/zh-CN/Common/Menu")',
        type: 'POST',
        dataType: "json",
        crossDomain: true
    })
	.done(function (data) {
	    data[0].menu[0].items[0].href = "form/user_add.html";
	    data[0].menu[0].items[1].href = "form/user_add.html";
	    data[0].menu[0].items[2].href = "form/permision.html";
	    data[0].menu[0].items[3].href = "form/user_add_three.html";
	    data[0].homePage = "USER";
	    console.log(data);
	    navData = data;
	    //navData[0].menu = Array;

	    for(var i=0; i<data[0].menu.length; i++)
	    {
	    	var temp, temp1 = "";
	    	var text;
	    
	    	text = data[0].menu[i].text.split('@');

	    	temp = text[0];
	    	temp1 = text[1];

	    	console.log(text);
	    }
	    new PageUtil.MainPage({
	        modulesConfig: data
	    });
	})
	.fail(function (xhr, status, error) {
	    alert(error);
	});
});

$(document).ready(function ($) {
    userStatus("MAN10", "AAA", "BBB", "Browse");
});