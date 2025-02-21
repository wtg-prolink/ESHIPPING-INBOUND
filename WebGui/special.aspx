<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="special.aspx.cs" Inherits="WebGui.special" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <script src="Scripts/Core/bui/js/jquery-1.11.1.min.js"></script>
    <script type="text/javascript">
        var handle = { Url: null, Data: null, DataType: "json", Success: null };
        function copybase()
        {
            var fromcmp = $("#fromcmp").val();
            var arrivecmp = $("#arrivecmp").val();
            var basetable = $("#basetable").val();
            if (fromcmp == "")
                if (!confirm("没有填写公司别，将默认选择福清"))
                    return;
            if (basetable == "")
            {
                alert("请先选择建档"); return;
            }
            handle.Url = "SomeSpecial.ashx";
            handle.Data = { action: "copybase", basetable: basetable, fromcmp: fromcmp, arrivecmp: arrivecmp };
            handle.Success = function (result) {
                if (result.IsOk == "Y") {
                    alert("success");
                }
            }
            ajaxput();
        }
        function layout() {
            var column = $("#column").val();
            if (column == "")
                if (!confirm("请填写列名"))
                    return;
            handle.Url = "SomeSpecial.ashx";
            handle.Data = { action: "resetlayout", column: column };
            handle.Success = function (result) {
                if (result.IsOk == "Y") {
                    alert("success");
                }
                else {
                    alert(result.message);
                }
            }
            ajaxput();
        }
        function ajaxput() {
            $.ajax({
                url: handle.Url,
                data: handle.Data,
                dataType: "json",
                success:handle.Success
            }
            );
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div style="border-style: dotted;border-width: 1px; padding: 10px;">
    <input type="text" id="column"  name="column" runat="server"/>
        <p>添加订舱默认栏位，多个请以逗号隔开</p>
        <input type="button" onclick="layout()" value="提交" />
    </div>
    <div style="border-style: dotted;border-width: 1px; padding: 10px;">
        <b>基本建档复制：</b>
        <div><select id="basetable" >
            <option></option>
            <option value ="客户建档" >客户建档</option>
            <option value ="客户交易建档" >客户交易建档</option>
            <option value ="币别建档" >币别建档</option>
            <option value ="汇率建档" >汇率建档</option>
            <option value ="费用建档" >费用建档</option>
            <option value ="过账节点建档" >过账节点建档</option>
                </select></div>
        <div><b>从公司别</b><input type ="text" id="fromcmp" /><b>复制到</b><input type="text" id="arrivecmp"/></div>
        <input type="button" onclick="copybase()" value="提交" />
    </div>
    </form>
</body>
</html>
