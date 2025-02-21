<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<!DOCTYPE html>

<html>
<head id="Head1" runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
<link rel="stylesheet" href="Scripts/css/WHBoard.css" type="text/css">
<link rel="stylesheet" href="Scripts/css/ui_theme/jquery-ui.min.css" type="text/css">
    <link rel="stylesheet" type="text/css" href="Scripts/css/lib/bootstrap-select.css">
    <link rel="stylesheet" href="Scripts/Core/bui/css/bootstrap.min.css" type="text/css">
</head>
    <script src="Scripts/Core/bui/js/jquery-1.11.1.min.js"></script>
    <script src="Scripts/Core/bui/js/jquery-ui.min.js"></script>
    <script src="Scripts/lib/bootstrap-select.js"></script>
    <script src="Scripts/Core/bui/js/lib/bootstrap.min.js"></script>

    <script type="text/javascript">
        fei = { jsimg: {} }
        fei.jsimg.histogram = function (x, y, data) {
            this.width = 800;                                       //图片宽度
            this.height = 350;                                     //图片高度
            this.barwidth = 36;                                     //柱子宽度
            this.x = x || [1, 2, 3, 4, 5, 6];                                       //x轴刻度
            this.y = y || ["ZB1", "ZB2", "ZB3", "ZB4", "ZB5", "ZB6"];               //y轴刻度
            this.data = data || [0, 0, 0, 0, 0, 0];                                 //数据
            this.dataformat = "{0}%";                       //x轴和数据的格式：如 可表示为 "{0}%"
            this.vbarBg = "";      //柱子背景图路径
        }

        fei.jsimg.histogram.prototype.draw = function () {
            this._set_max_value();
            this._draw_container();
            this._draw_container_rows();
            this._draw_container_cols();
        }

        fei.jsimg.histogram.prototype._set_max_value = function () {
            this.max_x = 0;
            this.max_data = 0;
            for (var i = 0; i < this.x.length; i++) {
                if (this.x[i] >= this.max_x) {
                    this.max_x = this.x[i];
                }
            }
            for (var i = 0; i < this.data.length; i++) {
                if (this.data[i] >= this.max_data) {
                    this.max_data = this.data[i];
                }
            }
        }

        fei.jsimg.histogram.prototype._draw_container = function () {
            this.histogramimg = document.createElement("ul"); //容器：ul
            this.addclass(this.histogramimg, "fei_histogram_container_style");
            this.histogramimg.style.cssText = "height: " + this.height + "px !important;" + "width: " + this.width + "px;";
        }

        fei.jsimg.histogram.prototype._draw_container_rows = function () {
            this.rows = document.createElement("li"); //行的集合：结构为<li>｛div集合｝</li>
            this.addclass(this.rows, "fei_histogram_container_rows_style");
            this.rows.style.cssText = "height:" + this.height + "px";

            var xheigth = this.height / this.x.length; //行之间的间隔
            for (var i = this.x.length - 1; i >= 0; i--) {
                var row = document.createElement("div"); //单行：div
                row.style.cssText = "height:" + (xheigth - 1) + "px";

                var row_p = document.createElement("p"); //行的刻度文字：x轴刻度
                if (this.dataformat == "" || !this.dataformat) {
                    row_p.innerHTML = this.x[i].toString();
                }
                else {
                    row_p.innerHTML = this.dataformat.replace("{0}", this.x[i].toString());
                }
                row.appendChild(row_p);

                this.rows.appendChild(row);
            }
            this.histogramimg.appendChild(this.rows);
        }

        fei.jsimg.histogram.prototype._draw_container_cols = function () {
            var ywidth = this.width / this.data.length; //算出列宽
            var col_ul_li_left = (ywidth - this.barwidth) / 2;
            var color = [ "/INBOUND/Images/Columnar1.png", "/INBOUND/Images/Columnar.png"];
            for (var i = 0; i < this.data.length; i++) {
                var col = document.createElement("li"); //列：li
                var left = i * ywidth; //列到x轴的距离
                this.addclass(col, "fei_histogram_container_col_style");
                col.style.cssText = "left: " + left + "px;height:" + (this.height + 1) + "px;width: " + ywidth + "px;";
                col.innerHTML = this.y[i].toString() + "<br/>";
                /*---柱子---*/
                var barul = document.createElement("ul");
                var barli = document.createElement("li");
                if (this.dataformat == "" || !this.dataformat) {
                    barli.innerHTML = this.data[i].toString();
                }
                else {
                    barli.innerHTML = this.dataformat.replace("{0}", this.data[i].toString());
                }
                barli.style.cssText = "height: " + this.data[i] * this.height / this.max_x + "px;" +
                    "left: " + col_ul_li_left + "px;width: " + this.barwidth + "px;" +
                    "background: url('" + color[i % 2] + "') no-repeat scroll 0 0 #00B0F0;";
                    //"background-image: linear-gradient(to right, #ff4500 0%, #ff4500 47%, #cf3a02 50%, #cf3a02 100%)";
                barul.appendChild(barli);
                col.appendChild(barul);
                this.histogramimg.appendChild(col);
            }
        }

        fei.jsimg.histogram.prototype.addclass = function (elements, value) {
            if (!elements.className) {
                elements.className = value;
            }
            else {
                newClass = elements.className;
                newClass += " ";
                newClass += value;
                elements.className = newClass;
            }
        }

        fei.jsimg.histogram.prototype.render = function (id) {
            this.container = document.getElementById(id);
            this.draw();
            this.container.replaceChild(this.histogramimg, this.container.firstChild);
            //this.container.appendChild(this.histogramimg);
        }
    </script>
    <script type="text/javascript">
        var t = null;
        t = setTimeout(time, 0);
        var timelist = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23];
        var vertical = ["SEA", "RAIL", "LCL", "AIR", "TRUCK", "EXPRESS"];
        var Divisions = ["PANEL", "SKD", "Spare parts", "OTHERS"];
        function time()
        {
            clearTimeout(t);
            var dateObj = new Date();
            var hours = dateObj.getHours();
            var minutes = dateObj.getMinutes();
            var date = dateObj.toLocaleDateString()
            document.getElementById("NowTime").innerHTML = hours + ":" + minutes;
            document.getElementById("NowDate").innerHTML = date;
            t = setTimeout(time, 10000);
        }

        function getFormatDate(i) {
            var date = new Date();
            date.setTime(date.getTime() + i * (24 * 60 * 60 * 1000));
            var tDate = date.getFullYear() + "/" + (date.getMonth() + 1) + "/" + date.getDate();
            return tDate;
        }

        $(document).ready(function () {
            Target();
            $("#target").val(90);
            initLoadData();
        });

        function initLoadData()
        {
            var dateObj = new Date();
            var hours = dateObj.getHours();
            var minutes = dateObj.getMinutes();
            $("#lastRefre").html(hours + ":" + minutes);
            var vbar = new fei.jsimg.histogram();
            vbar.x = [20, 40, 60, 80, 100];
            var lines = ["MSL", "CSWH", "PACK", "FGSL"];
            $("#beginTime").datepicker({
                dateFormat: 'yy-mm-dd',
                showOtherMonths: true,
                selectOtherMonths: true
            });
            $("#endTime").datepicker({
                dateFormat: 'yy-mm-dd',
                showOtherMonths: true,
                selectOtherMonths: true

            });
            $('.selectpicker').selectpicker({
                'selectedText': 'cat'
            });
            getWHcode(lines);
            getTrucker();
            var data = [];
            var y = [];
            getEfficiency(data, y);
            vbar.y = y;
            vbar.data = data;
            vbar.render("vbarTest");
            $("#Query").click(function (vbar) {
                QueryBtn(vbar);
            });
            $("#Button1").click(function (vbar) {
                QueryBtn(vbar);
            });
            var select = document.getElementById("WsCd");
            var value = select.value;
            var selectedText = select.options[value].text;
            select.onchange = function () {
                value = select.value;
                selectedText = select.options[value].text;
                WHBoard(selectedText);
                getWHSummary(selectedText);
                getPodSum(selectedText);
            };
            var target = document.getElementById("target");
            target.onchange =function() {
                var height = 350;
                var OnTime = $("#target").val();
                height = 474 + height * (100 - OnTime) / 100;
                var TargetLine = document.getElementById("TargetLine");
                TargetLine.style.cssText = "width:750px;height:3px;border:none;border-top:3px solid red;position:absolute; left:1100px; top:" + height + "px;z-index:3;"

            }
            WHBoard(selectedText);
            getWHSummary(selectedText);
            getPodSum(selectedText);
            setTimeout(initLoadData, 300000);

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
                        for (var i in timelist) {
                            var n = timelist[i];
                            var m = parseInt(n);
                            var r = n - m;
                            var num = 0;
                            str += "<tr>";
                            if (r == 0)
                                str += "<td class='auto-style40 size12p'>" + m + ":00</td>";
                            else
                                str += "<td class='auto-style40 size12p'>" + m + ":30</td>";
                            str += "</tr>";
                            strTb += "<tr>";
                            $.each(result.data, function (j, val) {
                                if (result.data[j].H == m) {
                                    for (var l = 0; l < result.data[j].Coun; l++) {
                                        if (num == 9)
                                            break;
                                        strTb += "<td class='" + result.data[j].Status + " pad1'>&nbsp;</td>"
                                        num++;
                                    }
                                }
                            });
                            for (; num < 9; num++)
                                strTb += "<td class='pad1'>&nbsp;</td>";
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
                        Cstr += "<tr><td class='cred TTL'>" + sum + "</td><td class='cred TTL'>100%</td></tr>";
                        for (var i = 0; i < cTd.length; i++) {
                            if (sum == 0)
                                Cstr += "<tr><td class='cwhite center'>" + cTd[i] + "</td><td class='cwhite center'>0%</td></tr>";
                            else
                                Cstr += "<tr><td class='cwhite center'>" + cTd[i] + "</td><td class='cwhite center'>" + (Math.round(cTd[i] / sum * 100) + "%") + "</td></tr>";
                        }
                        document.getElementById("TimeList").innerHTML = str;
                        document.getElementById("SubTable").innerHTML = strTb;
                        document.getElementById("CTb").innerHTML = Cstr;
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
                    if (result.message == "success")
                    {
                        lines = result.top5WH.split(';');
                        var db3 = "";
                        var sub3 = result.db3.split(';');
                        var sum = result.sum.split(';');
                        var k = 0;
                        for (var i = 0; i < vertical.length; i++) {
                            db3 += "<tr>";
                            for (var j = 0; j < lines.length; j++) {
                                if (vertical[i] == selectedText) {
                                    if (sub3[k] == 0)
                                        db3 += "<td class='db3'>&nbsp;</td>"
                                    else
                                        db3 += "<td class='red12 db3'>" + sub3[k] + "</td>"
                                }
                                else {
                                    if (sub3[k] == 0)
                                        db3 += "<td class='db3'>&nbsp;</td>"
                                    else
                                        db3 += "<td class='size12 db3'>" + sub3[k] + "</td>"
                                }
                                k++;
                            }
                            db3 += "</tr>"
                        }
                        var dSum = "<div class='auto-div41'>&nbsp;</div>";
                        var WHlines = "<div class='auto-div41'>&nbsp;</div>";
                        for (var i = 0; i < lines.length; i++) {
                            if (lines[i] != "EMPTY") {
                                dSum += "<div class='auto-div5'>" + sum[i] + "</div>";
                                WHlines += "<div class='auto-div5'>" + lines[i] + "</div>";
                            } else {
                                dSum += "<div class='auto-div5'>&nbsp;</div>";
                                WHlines += "<div class='auto-div5'>&nbsp;</div>";
                            }
                        }
                        document.getElementById("db3").innerHTML = db3;
                        document.getElementById("sum").innerHTML = dSum;
                        document.getElementById("WHlines").innerHTML = WHlines;
                        var Divis = result.Division.split(';');
                        var Division = "";
                        k = 0;
                        for (var i = 0; i < Divisions.length; i++) {
                            Division += "<tr>";
                            for (var j = 0; j < lines.length; j++) {
                                if (Divis[k] == 0)
                                    Division += "<td class='db3'>&nbsp;</td>"
                                else
                                    Division += "<td class='red12 db3'>" + Divis[k] + "</td>"
                                k++;
                            }
                            Division += "</tr>";
                        }
                        document.getElementById("Division").innerHTML = Division;
                    }
                }
            });
        }

        function getPodSum(selectedText) {
            $.ajax({
                async: true,
                type: "POST",
                url: "en-US/Api/PodSum",
                data: {"WsCd": selectedText },
                dataType: "json",
                success: function (result) {
                    if (result.message == "success") {
                        var pod = result.podSum.split(';');
                        var pods = "";
                        var thisWeeks = "";
                        k = 0;
                        for (var i = 0; i < 7; i++) {
                            thisWeeks += "<div class='auto-style43 size12p'>" + getFormatDate(i) + "</div>";
                            pods += "<tr>";
                            for (var j = 0; j < pod.length / 7; j++) {
                                if (pod[k] == 0)
                                    pods += "<td class='tbpod'>&nbsp;</td>";
                                else
                                    pods += "<td class='size12p tbpod'>" + pod[k] + "</td>";
                                k++;
                            }
                            pods += "</tr>";
                        }
                        document.getElementById("thisWeeks").innerHTML = thisWeeks;
                        document.getElementById("pods").innerHTML = pods;

                        var datetime = result.nowdate;
                        var Pods = result.Pods.split(';');
                        var k = 0;
                        var Ppods = "<div class='auto-width100'>&nbsp;</div>";
                        for (var i = 0; i < 7; i++) {
                            for (var j = 0; j < 4; j++) {
                                if (Pods[k] === "EMPTY" || Pods[k] === "" || (Pods[k]=="Oths" && j==0))
                                    Ppods += "<div class='auto-width16 color" + i + "'>&nbsp;</div>";
                                else
                                    Ppods += "<div class='auto-width16 color" + i + "'>" + Pods[k] + "</div>";
                                k++;
                            }
                        }
                        Ppods += "<div class='auto-width16'>target</div>";
                        Ppods += "<div class='auto-width16'>&nbsp;</div>";
                        document.getElementById("Ppods").innerHTML = Ppods;
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
                    var line = "";
                    for (var i = 0; i < lines.length; i++)
                        line += "<option value='" + i + "'>" + lines[i] + "</option>";
                    $("#WsCd").html(line);
                }
            });
        }

        function Target()
        {
            var Target = "";
            for (var i = 0; i < 100; i++)
                Target += "<option value='" + i + "'>" + i + "%</option>";
            $("#target").html(Target);
        }

        function getTrucker() {
            var beginTime = $("#beginTime").val();
            var endTime = $("#endTime").val();
            var selectedTrucker = "";
            for (var i = 0; i < $("#TruckerList option:selected").length; i++)
                selectedTrucker += $("#TruckerList option:selected").eq(i).val() + ",";
            $.ajax({
                async: false,
                type: "POST",
                url: "en-US/Api/getTrucker",
                data: {"beginTime":beginTime,"endTime":endTime},
                dataType: "json",
                success: function (result) {
                    var TruckerList = "";
                    var Truckers = "";
                    var TruckerLists = [];
                    if (TruckerLists.length <= 0 && !isEmpty(selectedTrucker))
                        TruckerLists = selectedTrucker.split(',');
                    $.each(result.TruckData, function (j, val) {
                        var value = "";
                        if (isEmpty(selectedTrucker) && j < 3)
                            value = "selected";
                        else if (!isEmpty(selectedTrucker) && $.inArray(result.TruckData[j].Trucker, TruckerLists) >= 0)
                            value = "selected";
                        TruckerList += "<option value='" + result.TruckData[j].Trucker + "' " + value + ">" + result.TruckData[j].PartyName + "</option>";
                    });
                    $("#TruckerList").html(TruckerList);
                    $('#TruckerList').selectpicker('refresh');
                    $('#beginTime').val(result.beginTime);
                    $('#endTime').val(result.endTime);

                }
            });
        }

        function getEfficiency(EffiData,y) {
            var beginTime = $("#beginTime").val();
            var endTime = $("#endTime").val();
            var selectedTrucker = "";
            var Truckers = "";
            for (var i = 0; i < $("#TruckerList option:selected").length; i++) {
                selectedTrucker += $("#TruckerList option:selected").eq(i).val() + ",";
                Truckers += "<div style='width:" + 100 / $("#TruckerList option:selected").length + "%' class='left size12 center'><label>" + $("#TruckerList option:selected").eq(i).text() + "</label></div>";
            }
            $("#Trucker").html(Truckers);
            var delay = $("#delay").val();
            $.ajax({
                async: false,
                type: "POST",
                url: "en-US/Api/getEfficiency",
                data: { "beginTime": beginTime, "endTime": endTime, "selectedTrucker": selectedTrucker,"delay":delay },
                dataType: "json",
                success: function (result) {
                    var Efficiency = result.Efficiency.split(';');
                    Efficiency.forEach(function (data, index, arr) {
                        y.push("");
                        EffiData.push(+data);
                    });
                }
            });

        }

        function isEmpty(val){
            if (val === undefined || val === "" || val == null)
                return true;
            return false;
        }

        function QueryBtn(vbar) {
            var vbar = new fei.jsimg.histogram();
            vbar.x = [20, 40, 60, 80, 100];
            var data = [];
            var y = [];
            getTrucker();
            getEfficiency(data, y);
            vbar.y = y;
            vbar.data = data;
            vbar.render("vbarTest");
        }

        function getRootPath() {
            var curPageUrl = window.document.location.href;
            var rootPath = curPageUrl.split("//")[0] + curPageUrl.split("//")[1].split("/")[0]
                    + curPageUrl.split("//")[1].split("/")[1];
            return rootPath;
        }

    </script>
<body style="height:490px; width: 1885px; background:black">
    <form id="form1" runat="server">
        <div style="width:1885px;height:55px;">
			<div class="nowdate" id="NowDate">
            	2018/3/28
            </div>
            <div class="nowtime" id="NowTime">
            	
            </div>
            <div class="nowdate">
            	WH board
            </div>
            <div class="WsCd">
            	<select id="WsCd">
                </select>
            </div>
            <div class="nowdate">
            	last refre
            </div>
            <div class="nowdate" id="lastRefre">
            </div>
            <button class="right" type="button" id="Query">
                Query
            </button>
            <div class="right" style="margin-right:10px">
                <label class="size12">End Date：</label>
                <input type="text" id="endTime">
            </div>
            <div class="right" style="margin-right:10px">
                <label class="size12">Start Date：</label>
                <input type="text" id="beginTime">
            </div>

        </div>
        <div style="width:1885px;height:870px;">
            <div class="div1">
                <div class="auto-style41">
        
                </div>
                <div style="float:left;width:433px;height:25px">
                    <div class="nowtime">
                        1
                    </div>
                    <div class="nowtime">
                        2
                    </div>
                    <div class="nowtime">
                        3
                    </div>
                    <div class="nowtime">
                        4
                    </div>
                    <div class="nowtime">
                        5
                    </div>
                    <div class="nowtime">
                        6
                    </div>
                    <div class="nowtime">
                        7
                    </div>
                    <div  class="nowtime">
                        8
                    </div>
                    <div class="nowtime">
                        More
                    </div>
                </div>
                <div class="auto-style42" id="TimeList">
                </div>
                <div class="div-table1">
                    <table class="auto-style100" id="SubTable"> 
                    </table>
                    </div>
            </div>
            <div class="auto-style3">
                <div class="auto-style8">
                <div class="auto-handldiv">
                    <label class="cred auto-headlabel">
                        TTL Delivery
                    </label>
                </div>
                <div class="auto-style6">
                    <table class="auto-style100" id="CTb">
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                    </table>
                </div>
                <div>
                        <div class="auto-style2 SLOTTIME">
                            <label class="auto-label1">
                                SlotTime Booked
                            </label>
                        </div>
                        <div class="auto-style2 ARRIVAL">
                            <label class="auto-label1">
                                Arrival
                            </label>
                        </div>
                        <div class="auto-style2 IN">
                            <label class="auto-label1">
                                Gate In
                            </label>
                        </div>
                        <div class="auto-style2 POD">
                            <label class="auto-label1">
                                POD
                            </label>
                        </div>
                        <div class="auto-style2 OUT">
                            <label class="auto-label1">
                                Gate Out
                            </label>
                        </div>
                </div>
                </div>
                <div class="auto-style7">
                    <div class="WhLines" id="WHlines">
                    </div>
                    <div style="height:276px;">
                        <div class="auto-div54">
                            <div class="auto-div4">
                                SEA
                            </div>
                            <div class="auto-div4">
                                RAIL
                            </div>
                            <div class="auto-div4">
                                LCL
                            </div>
                            <div class="auto-div4">
                                AIR
                            </div>
                            <div class="auto-div4">
                                Truck
                            </div>
                            <div class="auto-div4">
                                Express
                            </div>
                        </div>
                        <div class="auto-table1">
                           <table class="auto-style100" id="db3">
                           </table>
                        </div>

                    </div>
                    <div><hr class="auto-hr" /></div>
                    <div id="sum">

                    </div>
                    <div>
                        <div class="auto-div55">
                            <div class="auto-div8">
                                Panel
                            </div>
                            <div class="auto-div8">
                                SKD
                            </div>
                            <div class="auto-div81">
                                Spare parts
                            </div>
                            <div class="auto-div8">
                                OTHERS
                            </div>
                        </div>
                        <div class="auto-table2">
                            <table style="width:100%;height:100%" id="Division">
                            </table>
                        </div>

                    </div>
                </div>
            </div>
            <div class="auto-div43">
                <div class="auto-style100">
                    <div class="auto-height50">
                        <div class="auto-height210 left">
                            &nbsp;
                        </div>
                        <div class="auto-height224 color0">
                            Containers SEA
                        </div>
                        <div class="auto-height224 color1">
                            Containers RAIL
                        </div>
                        <div class="auto-height224 color2">
                            LCL
                        </div>
                        <div class="auto-height224 color3">
                            AIR
                        </div>
                        <div class="auto-height224 color4">
                            Express
                        </div>
                        <div class="auto-height224 color5">
                            Others
                        </div>
                        <div class="auto-height224 color6">
                            Out of E-sp
                        </div>
                        <div class="auto-height26">
                            WH target
                        </div>
                        <div class="auto-height26">
                            &nbsp;
                        </div>
                        <div style="height:19px" id="Ppods">

                        </div>
                        <div class="auto-710" id="thisWeeks">
                        </div>
                        <div class="auto-790">
                            <table class="auto-style100" id="pods">
                            </table>
                        </div>
                    </div>
                    <div class="auto-height51">
                        <div id="head" style="height:60px">
                            &nbsp;
                            <div style="margin-right:10px; float:right">
                                <select class="right" style="margin-right:10px" id="delay">
                                      <option value="15">15min</option>
                                      <option value="10">10min</option>
                                      <option value="5">5min</option>
                                      <option value="0">0min</option>
                                </select>
                                <label class="size12 right" style="margin-right:10px">Delay buffer</label>
                            </div>
                            <div style="margin-right:10px; float:right">
                                <label class="size12" style="margin-right:10px">On time Target</label>
                                <select id="target">
                                </select>
                            </div>
                            <div class="right" style="margin-right:10px">
                                <button type="button" id="Button1">
                                        Query
                                </button>
                            </div>
                            <div style="float:right; margin-right:10px">
                                <label class="size12" style="margin-right:10px">Trucker</label>
                                <select id="TruckerList" class="selectpicker bla bla bli" multiple data-live-search="true">
                                  </select>
                            </div>
                        </div>
                        <div style="height:350px">
                            <div style="width:125px;float:left;height:300px;text-align:center">
                                <label style="transform:rotate(270deg);color:white;line-height:300px;">Efficiency</label>
                            </div>
                            <hr class="target" style="position:absolute; left:1100px; top:509px;z-index:3;" id="TargetLine"/>
                            <div id="vbarTest" class="right">
                    	    </div>
                        </div>
                        <div id="Trucker" class="Trucker">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
