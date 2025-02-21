<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExecuteReport.aspx.cs" Inherits="Prolink.OEC.WebGui.ExecuteReport" %>

<%@ Register assembly="Stimulsoft.Report.Web, Version=2016.2.0.0, Culture=neutral, PublicKeyToken=ebe6666cba19647a" namespace="Stimulsoft.Report.Web" tagprefix="cc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <cc1:StiWebViewer ID="StiWebViewer1" runat="server" Visible="false" />
    
    </div>
    </form>
</body>
<script type="text/javascript">
    /*
    * modify by fish   2013-6-3     需求by：coco 
    * 兆鵬在列印報價單時,這個print有三個選項,第三個選項能否拿掉,我不想讓user看到
    * 因為第三個按鈕印出來時是整個字放大,怎麼調報表都沒用,還請協助一下,謝謝
    */
    var dom = document.getElementById("viewer1_Print_child_PrintWithoutPreview_currItem");
    dom.style.display = "none";
</script>
</html>
