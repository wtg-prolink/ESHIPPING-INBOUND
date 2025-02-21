<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <link rel="stylesheet" href="Scripts/css/WHBoard.css" type="text/css">
    <link rel="stylesheet" href="Scripts/css/ui_theme/jquery-ui.min.css" type="text/css">
    <link rel="stylesheet" type="text/css" href="Scripts/css/lib/bootstrap-select.css">
    <link rel="stylesheet" href="Scripts/Core/bui/css/bootstrap.min.css" type="text/css">
    <link rel="stylesheet" href="Scripts/Core/bui/css/main.css" type="text/css">
    <title></title>
</head>
    <script src="Scripts/Core/bui/js/jquery-1.11.1.min.js"></script>
    <script src="Scripts/Core/bui/js/jquery-ui.min.js"></script>
    <script src="Scripts/lib/bootstrap-select.js"></script>
    <script src="Scripts/Core/bui/js/lib/bootstrap.min.js"></script>
    <script>
        var t = null;
        t = setTimeout(time, 0);
        var timelist = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23];
        var vertical = ["SEA", "RAIL", "LCL", "AIR", "TRUCK", "EXPRESS"];
        var Divisions = ["PANEL", "SKD", "Spare parts", "OTHERS"];

        function time() {
            clearTimeout(t);
            var dateObj = new Date();
            var hours = dateObj.getHours();
            var minutes = dateObj.getMinutes();
            var date = dateObj.toLocaleDateString()
            document.getElementById("NowTime").innerHTML = hours + ":" + minutes;
            document.getElementById("NowDate").innerHTML = date;
            t = setTimeout(time, 10000);
        }

        $(document).ready(function () {
            initLoadData();
        });

        function initLoadData()
        {
            setFontSize("V2bhead1",0.6);
            setFontSize("V2bhead2",0.6);
            setFontSize("V2bhead3",0.6);
            setfFontSize("More", "lineList", 0.3, 1.5);
            setfFontSize("SpareParts", "V22Text", 0.5, 2);
            setfFontSize("Express", "V23Text", 0.5, 1.5);
            var dateObj = new Date();
            var hours = dateObj.getHours();
            var minutes = dateObj.getMinutes();
            var lines = ["MSL", "CSWH", "PACK", "FGSL"];
            $("#lastRefre").html(hours + ":" + minutes);
            $('.selectpicker').selectpicker({
                'selectedText': 'cat'
            });
            getWHcode(lines);
            var select = document.getElementById("WsCd");
            var value = select.value;
            var selectedText = select.options[value].text;
            select.onchange = function () {
                value = select.value;
                selectedText = select.options[value].text;
                WHBoard(selectedText);
                getWHSummary(selectedText);
            };
            WHBoard(selectedText);
            getWHSummary(selectedText);
            
            setTimeout(initLoadData, 300000);
        }

        function time() {
            clearTimeout(t);
            var dateObj = new Date();
            var hours = dateObj.getHours();
            var minutes = dateObj.getMinutes();
            var date = dateObj.toLocaleDateString()
            document.getElementById("NowTime").innerHTML = hours + ":" + minutes;
            document.getElementById("NowDate").innerHTML = date;
            setFontSize("NowDate", 0.4);
            setFontSize("NowTime", 0.4);
            setFontSize("lastRefre", 0.4);
            setFontSize("nowDate", 0.4);
            t = setTimeout(time, 10000);
        }

        function WHBoard(selectedText) {
            $.ajax({
                async: true,
                type: "POST",
                url: "en-US/Api/WHBoard",
                data: { "FirstTime": timelist[0], "EndTime": timelist[timelist.length - 1], "WsCd": selectedText },
                dataType: "json",
                success: function (result) {
                    if (result.message == "success") {
                        var str = "";
                        var strTb = "";
                        str += "<table class='auto-style100'>";
                        for (var t = 0; t < 9; t++) {
                            if (t==0)
                                str += "<tr><td class='V2Time' id='tTime'>&nbsp;</td>";
                            for (var i in timelist) {
                                var n = timelist[i];
                                var m = parseInt(n);
                                var r = n - m;
                                var num = 0;
                                if (t == 0)
                                    str += "<td class='V2Time Cout'>" + m + ":00</td>";
                                $.each(result.data, function (j, val) {
                                    if (result.data[j].H == m && result.data[j].Coun > 0) {
                                        strTb += "<td class='" + result.data[j].Status + " pad1'>&nbsp;</td>"
                                        result.data[j].Coun = result.data[j].Coun - 1;
                                        num = 1;
                                        return false;
                                    }
                                });
                                if (num == 0)
                                    strTb += "<td class='pad1'>&nbsp;</td>";
                            }
                            if (t == 0)
                                str += "</tr>";
                            strTb += "</tr>";
                        }
                        str += "</table>";
                        var sum = 0;
                        var Cstr = ""
                        var cTd = new Array();
                        var n = 0;
                        var status = ["SLOTTIME", "ARRIVAL", "IN", "POD", "OUT"];
                        for (var i = 0; i < status.length; i++) {
                            $.each(result.cdata, function (j, val) {
                                if (result.cdata[j].Status == status[i]) {
                                    sum += result.cdata[j].Count;
                                    cTd[n] = result.cdata[j].Count;
                                    n++;
                                }
                            });
                            if (n < i + 1) {
                                cTd[n] = 0;
                                n++;
                            }
                        }
                        for (var i = 0; i < cTd.length; i++) {
                            if (sum == 0)
                                Cstr += "<tr><td class='cwhite center'>" + cTd[i] + "</td><td class='cwhite center'>0%</td></tr>";
                            else
                                Cstr += "<tr><td class='cwhite center'>" + cTd[i] + "</td><td class='cwhite center'>" + (Math.round(cTd[i] / sum * 100) + "%") + "</td></tr>";
                        }
                        document.getElementById("TimeList").innerHTML = str;
                        document.getElementById("SubTable").innerHTML = strTb;
                        document.getElementById("V2Status").innerHTML = Cstr;
                        Cstr = "<tr><td class='cred center'>" + sum + "</td><td class='cred center'>100%</td></tr>";
                        document.getElementById("V2StatusSum").innerHTML = Cstr;
                        setfFontSize("tTime", "TimeList", 0.3, 1.5);
                        setfFontSize("cBook", "V2Up", 0.5, 1.5);
                        setfFontSize("ttlD", "V2Down", 0.6, 1.5);
                    }
                }
            });
        }

        function getWHSummary(selectedText) {
            $.ajax({
                async: true,
                type: "POST",
                url: "en-US/Api/getWHSummary",
                data: { "FirstTime": timelist[0], "EndTime": timelist[timelist.length - 1], "WsCd": selectedText },
                dataType: "json",
                success: function (result) {
                    if (result.message == "success") {
                        lines = result.top5WH.split(';');
                        var db3 = "";
                        var sub3 = result.db3.split(';');
                        var sum = result.sum.split(';');
                        var k = 0;
                        var maxLines = 6;
                        for (var i = 0; i < vertical.length; i++) {
                            db3 += "<tr>";
                            for (var j = 0; j < lines.length; j++) {
                                var line = lines[j];
                                if (line.length > maxLines)
                                    maxLines = line.length;
                                if (vertical[i] == selectedText) {
                                    if (sub3[k] == 0)
                                        db3 += "<td class='V2Tb3'>&nbsp;</td>"
                                    else
                                        db3 += "<td class='red12 V2Tb3'>" + sub3[k] + "</td>"
                                }
                                else {
                                    if (sub3[k] == 0)
                                        db3 += "<td class='V2Tb3'>&nbsp;</td>"
                                    else
                                        db3 += "<td class='size12 V2Tb3'>" + sub3[k] + "</td>"
                                }
                                k++;
                            }
                            db3 += "</tr>"
                        }
                        var dSum = "";
                        var WHlines = "";
                        var whline = "";
                        for (var i = 0; i < lines.length; i++) {
                            if (lines[i] != "EMPTY" && i==0) {
                                dSum += "<div class='V2lines cred'>" + sum[i] + "</div>";
                                if (maxLines == lines[i].length && maxLines > -1) {
                                    WHlines += "<div class='V2lines cred' id='MaxLines'>" + lines[i] + "</div>";
                                    whline += "<div class='V2lines cred' id='MaxLines2'>" + lines[i] + "</div>";
                                    maxLines = -1;
                                } else {
                                    WHlines += "<div class='V2lines cred'>" + lines[i] + "</div>";
                                    whline += "<div class='V2lines cred'>" + lines[i] + "</div>";
                                }
                            } else if (lines[i] != "EMPTY" && i!=0) {
                                dSum += "<div class='V2lines Cout'>" + sum[i] + "</div>";
                                if (maxLines == lines[i].length && maxLines > -1) {
                                    WHlines += "<div class='V2lines Cout' id='MaxLines'>" + lines[i] + "</div>";
                                    whline += "<div class='V2lines Cout' id='MaxLines2'>" + lines[i] + "</div>";
                                    maxLines = -1;
                                } else {
                                    WHlines += "<div class='V2lines Cout'>" + lines[i] + "</div>";
                                    whline += "<div class='V2lines Cout'>" + lines[i] + "</div>";
                                }
                            } else if (lines[i] == "EMPTY")
                            {
                                dSum += "<div class='V2lines'>&nbsp;</div>";
                                
                                if (maxLines <= 6 && maxLines > -1) {
                                    WHlines += "<div class='V2lines' id='MaxLines'>&nbsp;</div>";
                                    whline += "<div class='V2lines' id='MaxLines2'>&nbsp;</div>";
                                    maxLines = -1;
                                } else {
                                    WHlines += "<div class='V2lines'>&nbsp;</div>";
                                    whline += "<div class='V2lines'>&nbsp;</div>";
                                }
                            }
                        }
                        document.getElementById("V2byType").innerHTML = db3;
                        document.getElementById("V2Sum").innerHTML = dSum;  
                        document.getElementById("V2Typesum").innerHTML = dSum;
                        document.getElementById("V2WHline").innerHTML = WHlines;
                        document.getElementById("V2whline").innerHTML = whline;
                        var Divis = result.Division.split(';');
                        var Division = "";
                        k = 0;
                        for (var i = 0; i < Divisions.length; i++) {
                            Division += "<tr>";
                            for (var j = 0; j < lines.length; j++) {
                                if (Divis[k] == 0)
                                    Division += "<td class='db3'>&nbsp;</td>"
                                else
                                    Division += "<td class='yellow12 db3'>" + Divis[k] + "</td>"
                                k++;
                            }
                            Division += "</tr>";
                        }
                        document.getElementById("V2byMaterial").innerHTML = Division;
                        //setfFontSize("MaxLines", "V22Right", 0.5, 1.3);
                        //setfFontSize("MaxLines2", "V23Right", 0.5, 1.3);                      
                        //setfFontSize("MaxLines", "V22Down", 0.5, 2);
                        //setfFontSize("MaxLines2", "V23Down", 0.5, 2);
                        newSetFontSize("V22Right");
                        newSetFontSize("V23Right");
                        newSetFontSize("V22Down");
                        newSetFontSize("V23Down");
                    }
                }
            });
        }

        function getWHcode(lines) {
            $.ajax({
                async: false,
                type: "POST",
                url: "en-US/Api/getWHCode",
                data: {},
                dataType: "json",
                success: function (result) {
                    lines = result.WHCodes.split(';');
                    var wscd = result.SelectWsCd;
                    var line = "";
                    for (var i = 0; i < lines.length; i++) {
                        var selected = "";
                        if (lines[i] == wscd)
                            selected = "selected";
                        line += "<option value='" + i + "' " + selected + ">" + lines[i] + "</option>";
                    }
                    $("#WsCd").html(line);
                    setFontSize("V2WsCd", 0.4);
                }
            });
        }

        function setFontSize(id, multiple) {
            var str = document.getElementById(id);
            var height = str.offsetHeight * multiple;
            var width = str.offsetWidth;
            str.style.fontSize = height  + "px";
            if (height > width)
                str.style.fontSize = width/10 + "px";
        }
        function setfFontSize(id, fid, multiple,wmultiple) {
            var fstr = document.getElementById(fid);
            fstr.style.fontSize = 1 + "px";

            var str = document.getElementById(id);
            var lenstr = str.innerHTML.length;//DIV里文字长度
            var height = str.offsetHeight * multiple;
            var width = str.offsetWidth / lenstr * wmultiple;
            fstr.style.fontSize = height + "px";
            if (height > width)
                fstr.style.fontSize = width + "px";
        }
        function newSetFontSize(fid)
        {
            var fstr = document.getElementById(fid);
            fstr.style.fontSize = 13.65 + "px";
        }

    </script>
<body style="background:black";>
    <div class="V2Container">
        <div class="V2header">
			<div class="V2nowDate" id="NowDate">
            	2018/3/28
            </div>
            <div class="V2nowtime" id="NowTime">
            	
            </div>
            <div class="V2WsCd" id="V2WsCd">
            	<select id="WsCd">
                </select>
            </div>
            <div class="V2nowDate" id="nowDate">
            	last refre
            </div>
            <div class="V2nowtime" id="lastRefre">
            </div>
        </div>
        <div class="V2Content">
        <div class="V2Box1">
                <div class="V2timeList" id="TimeList">
                </div>
                <div style="float:left;width:4%;height:90%" id="lineList">
                    <table class="auto-style100">
                        <tr><td class="Cout center">1</td></tr>
                        <tr><td class="Cout center">2</td></tr>
                        <tr><td class="Cout center">3</td></tr>
                        <tr><td class="Cout center">4</td></tr>
                        <tr><td class="Cout center">5</td></tr>
                        <tr><td class="Cout center">6</td></tr>
                        <tr><td class="Cout center">7</td></tr>
                        <tr><td class="Cout center">8</td></tr>
                        <tr><td class="Cout center">9</td></tr>
                        <tr><td class="Cout center" id="More">More</td></tr>
                    </table>
                </div>

                <div class="V2Tb1">
                    <table class="auto-style100" id="SubTable"> 
                    </table>
                </div>
            </div>
        <div class="V2Box2">
            <div class="V2Box21">
                <div class="V2Bhead" id="V2bhead1">
                    Daily Status Summary
                </div>
                <div id="Ctb" class="V2Text">
                    <div class="V2Up" id="V2Up">
                        <div class="V2Status" >
                            <table class="auto-style100">
                                <tr><td class="Cbook" id="cBook">SlotTime Booked</td></tr>
                                <tr><td class="cred">Arrival</td></tr>
                                <tr><td class="Cin">Gate In</td></tr>
                                <tr><td class="Cpod">POD</td></tr>
                                <tr><td class="Cout">Gate Out</td></tr>
                            </table>
                        </div>
                        <div class="V2Status">
                            <table class="auto-style100" id="V2Status">
                            </table>
                        </div>
                    </div>
                    <div class="V2Down" id="V2Down">
                        <div class="V2Status">
                            <div style="height:100%;width:100%"><label class="cred" style="height:100%;width:100%" id="ttlD">TTL Delivery</label></div>
                        </div>
                        <div class="V2Status">
                            <table class="auto-style100" id="V2StatusSum">
                            </table>
                        </div>
                    </div>
                </div>
            </div>
            <div class="V2Box22">
                <div class="V2Bhead" id="V2bhead2">
                    Material Split
                </div>
                <div class="V2Text" id="V22Text">
                    <div class="V2Up">
                        <div class="V2left" id="V22Left">
                            <div class="V2WHline">&nbsp;</div>
                            <div class="V2WHSum">
                                <table class="auto-style100">
                                    <tr><td class="Cout">Panel</td></tr>
                                    <tr><td class="Cout">SKD</td></tr>
                                    <tr><td class="Cout" id="SpareParts">Spare parts</td></tr>
                                    <tr><td class="Cout">OTHERS</td></tr>
                                </table>
                            </div>
                        </div>
                        <div class="V2Right" id="V22Right">
                            <div class="V2WHline" id="V2WHline">

                            </div>
                            <div class="V2WHSum">
                                <table class="auto-style100" id="V2byMaterial">
                                </table>
                            </div>
                        </div>
                    </div>
                    <div class="V2Down" id="V22Down">
                        <div class="V2left">
                            &nbsp;
                        </div>
                        <div class="V2Right" id="V2Sum">
                            
                        </div>
                    </div>
                </div>
            </div>
            <div class="V2Box23">
                <div class="V2Bhead" id="V2bhead3">
                    Trans Mode Split
                </div>
                <div class="V23Text" id="V23Text">
                    <div class="V2Up">
                        <div class="V2left" id="V23Left">
                            <div class="V2WHline">&nbsp;</div>
                            <div class="V2WHSum">
                                <table class="auto-style100">
                                    <tr><td class="Cout">SEA</td></tr>
                                    <tr><td class="Cout">RAIL</td></tr>
                                    <tr><td class="Cout">LCL</td></tr>
                                    <tr><td class="Cout">AIR</td></tr>
                                    <tr><td class="Cout">Truck</td></tr>
                                    <tr><td class="Cout" id="Express">Express</td></tr>
                                </table>
                            </div>
                        </div>
                        <div class="V2Right" id="V23Right">
                            <div class="V2WHline" id="V2whline">

                            </div>
                            <div class="V2WHSum">
                                <table class="auto-style100" id="V2byType">
                                </table>
                            </div>
                        </div>
                    </div>
                    <div class="V2Down" id="V23Down">
                        <div class="V2left">
                            &nbsp;
                        </div>
                        <div class="V2Right" id="V2Typesum">
                            
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    </div>
</body>
</html>
