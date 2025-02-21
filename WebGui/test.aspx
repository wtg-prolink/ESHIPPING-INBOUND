<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="test.aspx.cs" Inherits="WebGui.test" %>


<!DOCTYPE html>
<script type="text/javascript">
    function onSelected(x) {
        if (x.value == "L" || x.value == "M" || x.value == "N") {
            document.getElementById("txt_ID").style.display = "";
        }
        else {
            document.getElementById("txt_ID").style.display = "none";
        }
    }
</script>
<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    
</head>
<body>
    <form id="form1" runat="server">
    <div>

  <select id="import_mode" runat="server">
                                <option></option>
                                <option selected="selected" value="A">类别</option>
                                <option value="B">货柜尺寸</option>
                                <option value="C">产品组</option>
                                <option value="D">OrderType</option>
                                <option value="E">港口(启运港)</option>
                                <option value="F">销售渠道</option>
                                <option value="G">销售组织</option>
                                <option value="H">贸易条款</option>
                                <option value="I">汇率</option>
                                <option value="J">工厂</option>
                                <option value="K">缺货港</option>
                                <option value="L">Get DN</option>
                                <option value="M">导入序列号数</option>
                                <option value="N">Get Profile</option>
                                <option value="O">回抛运保费</option>
  </select>       
        <button id="btn_import" runat="server" >导入</button>
         <div id="txt_ID"  ><span>NO:</span><input runat="server" id="txt_DNNO" type="text"  /></div>
    </div>
        <div>
            <label id="result" runat="server"></label>
        </div>
    </form>
</body>
</html>
