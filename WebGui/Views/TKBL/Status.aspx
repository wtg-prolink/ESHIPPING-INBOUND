<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Status.aspx.cs" Inherits="WebGui.View.Status" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
    <style type="text/css">
        body
        {
            text-align: center;
        }
        .FieldCol
        {
            position: relative;
            text-align: left;
            float: left;
            width: 80px;
            padding-left: 5px;
        }
        .FieldCol0
        {
            position: relative;
            text-align: left;
            float: left;
            width: 150px;
            padding-left: 5px;
        }
        .FieldColr
        {
            position: relative;
            text-align: right;
            float: left;
            width: 100px;
            padding-right: 5px;
        }
        .DataRow
        {
            width: 1000px;
            height: 22px;
        }
        table.gridtable
        {
            font-family: verdana,arial,sans-serif;
            font-size: 14px;
            color: #333333;
            border-width: 1px;
            border-color: #666666;
            border-collapse: collapse;
        }
        table.gridtable th
        {
            border-width: 1px;
            padding: 8px;
            border-style: solid;
            border-color: #666666;
            background-color: #8ECFF5;
        }
        table.gridtable td
        {
            border-width: 1px;
            padding: 3px;
            border-style: solid;
            border-color: #666666;
            background-color: #ffffff;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <div class="DataRow">
            <div class="FieldCol">
                <span>DN NO:</span></div>
            <div class="FieldCol0">
                <%=HouseNo%>&nbsp;
            </div>
          <%--  <div class="FieldColr">
                <span>MBL NO:</span></div>
            <div class="FieldCol0">
                <%=MasterNo%>&nbsp;
            </div>
            <div class="FieldCol" style="width:auto;">
                <span>Shipment Id:</span></div>
            <div class="FieldCol0">
                <%=ShipmentId%>&nbsp;
            </div>--%>
           <%--  <div class="FieldCol0">
                <a href="javascript:see()" style="font-weight: bold;text-decoration: initial;">@Resources.Locale.L_Status_Views_535</a>
            </div>--%>
        </div>
        <div style="width: 1000px;">
            <table class="gridtable">
                <tr>
                    <th style="text-align: left;" width="200px">
                        Status Description
                    </th>
                    <th style="text-align: left;" width="150px">
                        <%if (DetailDT.Columns.Contains("HOUSE_NO")){%>
                        @Resources.Locale.L_Status_Views_536
                        <%}else{ %>
                        Container No
                        <%} %>
                    </th>
                    <th style="text-align: left;" width="200px">
                        Event Date
                    </th>
                    <th style="text-align: left;" width="150px">
                        Location
                    </th>
                    <th style="text-align: left;" width="250px">
                        Remark
                    </th>
                </tr>
                <%foreach (System.Data.DataRow dr in DetailDT.Rows)
                  { %>
                <tr>
                    <td style="text-align: left;">
                        <%=dr["STS_DESCP"] %>
                    </td>
                    <td style="text-align: left;">
                        <%if(DetailDT.Columns.Contains("HOUSE_NO")){%><%=dr["HOUSE_NO"]%><%}else{ %><%=dr["CNTR_NO"] %><%} %>
                    </td>
                    <td style="text-align: left;">
                        <%=dr["EVEN_DATE_STR"] %>
                    </td>
                    <td style="text-align: left;">
                        <%=dr["LOCATION_DESCP"] %>
                    </td>
                    <td style="text-align: left;">
                        <%=dr["REMARK"] %>
                    </td>
                </tr>
                <%} %>
            </table>
        </div>
    </div>
    </form>
    <script src="Scripts/jquery.min.js" type="text/javascript"></script>
    <script type="text/javascript" language="javascript">
        function see() {
            var mailto = window.prompt("@Resources.Locale.L_Status_Views_537", "")
            function isEmpty(val) {
                if (val === undefined || val == null || val === "")
                    return true
                return false;
            }

            if (!isEmpty(mailto)) {
                if (mailto.indexOf("@") < 0) {
                    alert("@Resources.Locale.L_Status_Views_538");
                    return;
                }

                $.ajax({
                    async: true,
                    url: "StatusNotice.aspx",
                    type: 'POST',
                    data: { content: "<%=HouseNo%>", jobNos: "<%=jobno%>", cmp: "", groupId: "<%=GroupId%>", createby: encodeURIComponent("@Resources.Locale.L_Status_Views_539"), "mailTo":encodeURIComponent(mailto||"fish@pllink.com") },
                    dataType: "json",
                    "complete": function (xmlHttpRequest, successMsg) {

                    },
                    "error": function (xmlHttpRequest, errMsg) {
                        alert("@Resources.Locale.L_Status_Views_540");
                    },
                    success: function (result) {
                        if (result && result.success)
                            alert("@Resources.Locale.L_Status_Views_541");
                        else
                            alert("@Resources.Locale.L_Status_Views_540");
                    }
                });
            }
        }
    </script>
</body>
</html>
