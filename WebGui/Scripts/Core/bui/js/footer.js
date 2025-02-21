

var StatusBarArr = {
    userName: function (str) { $(".STATUS-USER",window.parent.document).find("span").html(str) },
    msgTip: function (str) {
        $(".STATUS-TIP", window.parent.document).find("span").html(str)
        $("#msgTipVal").text(str);
    },
    colMsg: function (str) {
        $(".STATUS-MSG", window.parent.document).find("span").html(str)
        $("#colMsgVal").text(str);
    },
    nowStatus: function (str) {
        $(".STATUS-NOW", window.parent.document).find("span").html(str)
        $("#nowStatusVal").text(str);
    }
};