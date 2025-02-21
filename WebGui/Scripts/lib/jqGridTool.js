var _dataSource = [];
var _editData = {};

function ds(data, key,control) {
    this._old = [];
    this._control = control;
    this._modify = [];
    this._data = data || [];
    this._index = 0;
    this._key = key || [];
    this._childs = { "__ln": 0 };
    this.addChild = function (name,ds) {
        this._childs[name] = ds;
        this._childs["__ln"]++;
    }

    this.getChild = function (name) {
        return this._childs[name];
    }

    this.getControl = function (name) {
        return this._control;
    }

    this.init = function () {
        var old;
        this._index = -1;
        for (var i = 0; i < this._data.length; i++) {
            this._index = 0;
            this._data[i]["_state"] = "";
            if (this._data[i]["__old"]) continue;
            old = $.extend({}, this._data[i]);
            this._data[i]["__old"] = old;
        }
    };
    this.init();
    this.getCurRd = function () {
        if (this._index >= this._data.length) return null;
        return this._data[this._index]
        //throw "无该栏位";
    }

    this.setData = function (data) {
        this._data = data || [];
        if (this._control && this._control.addJSONData) {
            $(this._control).jqGrid("clearGridData");
            var loadonce = $(this._control).jqGrid("getGridParam", "loadonce");
            //alert(loadonce);
 
            if (!this._loadonce)
                this._loadonce = loadonce !== true ? 1 : 2;
            //ts.p.datatype !== "local" && ts.p.loadonce
            //$(this._control).jqGrid("setGridParam", { data: data.rows || data });
            if (this._loadonce === 1) {
                var datatype = $(this._control).jqGrid("getGridParam", "datatype");
                loadonce = $(this._control).jqGrid("setGridParam", "loadonce");
                $(this._control).jqGrid("setGridParam", { loadonce: true });
                this._control.addJSONData(data);
                $(this._control).jqGrid("setGridParam", { loadonce: loadonce });
                $(this._control).jqGrid("setGridParam", { datatype: "json" });
            }
            else
                this._control.addJSONData(data);
        }
        this.init();
    }

    this.getData = function () {
        return this._data;
    }

    this.endEdit = function () {
        if (this._control && this._control)
            $(this._control).jqGrid('getGridParam', "endEdit")();
        for (var name in this._childs) {
            if ("__ln" === name) continue;
            this._childs[name].endEdit();
        }
    }

    this.getCurVal = function (name) {
        var rd = this.getCurRd();
        if (rd == null || rd[name] === undefined) return null;
        return rd[name];
        //throw "无该栏位";
    }

    this.setCurVal = function (name,val) {
        var rd = this.getCurRd();
        if (rd == null) return false;
        rd[name] = val;
        return true;
    }

    this.setCurIndexByKey = function (rd) {
        var key = _getKey(rd, this._key);
        this._index = -1;
        for (var i = 0; i < this._data.length; i++) {
            if (_getKey(this._data[i], this._key) === key) {
                this._index = i;
                break;
            }
        }
    }

    this.setCurIndexByGrid = function (grid, id) {
        var key = _getKeyByGrid(grid, id, this._key);
        //        var key1; ;
        //        var data = grid.jqGrid("getGridParam", "data");
        //        var rd = null;
        //        for (var i = 0; i < data.length; i++) {
        //            key1 = _getKey(data[i], this._key);
        //            if (key === key1) {
        //                rd = data[i];
        //                break;
        //            }
        //        }
        this._index = -1;
        for (var i = 0; i < this._data.length; i++) {
            if (_getKey(this._data[i], this._key) === key) {
                var rd = this._data[i];
                this._index = i;
                for (var name in this._childs) {
                    if ("__ln" === name) continue;
                    this._childs[name].setData(rd[name]);
                }
                break;
            }
        }
    }

    function _getKeyByGrid(grid,id, key) {
        var val = "";
        for (var i = 0; i < key.length; i++) {
            val += "|" + grid.jqGrid('getCell', id, key[i]);
        }
        return val;
    }

    function _getKey(rd, key) {
        var val = "";
        for (var i = 0; i < key.length; i++) {
            val += "|" + rd[key[i]];
        }
        return val;
    }

    this.setCurIndex = function (index) {
        this._index = index;
    }

    function _setKey(rd,rd1,key) {
        for (var j = 0; j < key.length; j++) {
            //rd[this._key[j]] = rd1[key[j]];
            rd[key[j]] = rd1[key[j]];
        }
    }

    this.getKeyValueByDelete = function () {
        var rds = [];
        var old;
        var rd, rd1, rd2;
        for (var i = 0; i < this._data.length; i++) {
            rd = {};
            rd1 = this._data[i]; //修改后的数据
            rd2 = rd1["__old"]; //原始的数据
            rd["__state"] = "0";
            if (rd["__state"] === "0") {
                _setKey(rd, rd2, this._key);
                rds.push(rd);
            }
            for (var name in this._childs) {
                if ("__ln" === name) continue;
                this._childs[name].setData(rd1[name]);
                var crds = this._childs[name].getKeyValueByDelete();
                if (crds.length > 0) {
                    rd[name] = crds;
                    rds.push(rd);
                }
            }
        }
        return rds;
    }

    this.getChangeValue = function () {
        var rds = [];
        var old;
        var rd, rd1, rd2;
        for (var i = 0; i < this._data.length; i++) {
            rd = {};
            rd1 = this._data[i]; //修改后的数据
            rd2 = rd1["__old"]; //原始的数据
            for (var name in rd2) {
                if (rd1[name] === rd2[name])
                    continue;
                rd[name] = rd1[name];
                rd["__state"] = "2";
                if (name == "_state") {
                    if (rd[name] == "0"){  //删除
                        rd["__state"] = "0";
                        
                    } else if (rd[name] == "1") { //新增
                        rd["__state"] = "1";
                    }
                    delete rd["_state"];
                }
            }
            if (rd["__state"] === "2") {
                _setKey(rd, rd1, this._key);
                rds.push(rd);
            } else if (rd["__state"] === "1") {
                _setKey(rd, rd1, this._key);
                rds.push(rd);
            }
            else if (rd["__state"] === "0") {
                _setKey(rd, rd1, this._key);
                rds.push(rd);
            }

            for (var name in this._childs) {
                if ("__ln" === name) continue;
                this._childs[name].setData(rd1[name]);
                var crds = this._childs[name].getChangeValue();
                if (crds.length > 0) {
                    rd[name] = crds;
                    rds.push(rd);
                }
            }
        }
        console.log(rds);
        return rds;
    }
}

function dm() {
    var _dss = {};
    this.getDs = function (name) {
        if (_dss[name] === undefined || _dss[name] == null)
            return null;
        return _dss[name];
    }

    this.addDs = function (name, data,key,control) {
        _dss[name] = new ds(data, key, control);
    }

    this.getChangeValue = function (rddata) {
        if (rddata == null) {
            var data = {};
            for (var name in _dss) {
                data[name] = _dss[name].getChangeValue();
            }
            return data;
        }
        else {
            //var data = {};
            for (var name in _dss) {
                rddata[name] = _dss[name].getChangeValue();
            }
            return rddata;
        }
    }

    this.getKeyValue = function (rddata) {
        if (rddata == null) {
            var data = {};
            for (var name in _dss) {
                data[name] = _dss[name].getChangeValue();
            }
            return data;
        }
        else {
            //var data = {};
            for (var name in _dss) {
                rddata[name] = _dss[name].getKeyValueByDelete();
            }
            return rddata;
        }
    }

    this.endEdit = function () {
        for (var name in _dss) {
            _dss[name].endEdit();
        }
    }
}

function setToolBtnDisabled(disabled) {
    $("button.btn-default").each(function (i, val) {
        $(this).prop("disabled", disabled);
    });
}

function setdisabled(disabled, isEdit) {
    $("button.btn-default").each(function (i, val) {
        $(this).prop("disabled", disabled);
    });

    $("[fieldname][dt='mt']").each(function (i, val) {
        if ($(this).is("select")) {
            if ($(this).attr("readonly") == "readonly") {
                $(this).prop("readonly", false);
                $(this).prop("disabled", true);
            }
            else {
                if (isEdit && disabled == false && $(this).attr("isKey") == "true") {
                    $(this).prop("disabled", true);
                } else
                    $(this).prop("disabled", disabled);
            }
        }
        else {
            if (isEdit && disabled == false && $(this).attr("isKey") == "true") {
                var btnObj = $(this).parent().find("button.btn-default");
                if (btnObj.length == 1)
                    btnObj.prop("disabled", true);
                $(this).prop("disabled", true);
            } else
                $(this).prop("disabled", disabled);
        }
    });

    $("input[inputType='file']").each(function(i, val){
        $(this).prop("disabled", disabled);
    });
}

function setFieldValue(rows, dt) {
    _dataSource = rows;
    dt = dt || "mt";
    $("[fieldname][dt='" + dt + "']").each(function (index, obj) {
        var jq = $(obj);
        var fieldName = jq.attr("fieldname");
        var value;
        if (!rows || rows.length == 0)
            value = "";
        else
        {
            //value = (rows[0][fieldName]) ? rows[0][fieldName] : "";
            value = (rows[0][fieldName] != null || rows[0][fieldName] == 0) ? rows[0][fieldName] : "";

            if($("#wrapper").attr("InputDep") == "B")
            {
                if($(obj).attr('isNumber') == "true")
                {
                    var d = value.toString().split('.');
                    if(typeof d[1] != "undefined" && d[1].length > 0)
                    {
                        value = AppendComma(value,d[1].length);
                    }
                    else
                    {
                        value = AppendComma(value);
                    }
                }
            }
        }

        switch (obj.tagName.toLocaleLowerCase()) {
            case "select":
                obj.value = value;
                break;
            case "input":
                if ($(obj).attr('fieldType') == "date" || $(obj).attr('fieldType') == "datetime") {
                    if (value && $.trim(value) !="") {
                        /*if (value.length == 10)
                            value = value + " 00:00:00";
                        var dateStr = value, a = dateStr.split(" "), d = a[0].split("-"), t = a[1].split(":");
                        var date = new Date(d[0], (d[1] - 1), d[2], t[0], t[1], t[2]);
                        var dateFormat = $(obj).attr("dateformat");
                        //var datetime = new Date(dateStr);
                        if (dateFormat == null) {
                            if ($(obj).attr('fieldType') == "date") {
                                dateFormat = "yyyy/mm/dd";
                            }
                            else if ($(obj).attr('fieldType') == "datetime") {
                                dateFormat = "yyyy/mm/dd HH:mm";
                            }

                        }*/
                        if (value.length == 10)
                            value = value + " 00:00:00";
                        var t = value.split(/[- :]/);
                        var d = new Date();
                        if($(obj).attr('fieldType') == "date")
                        {
                            d = new Date(t[0], t[1]-1, t[2]);
                        }
                        else
                        {
                            
                            d = new Date(t[0], t[1]-1, t[2], t[3], t[4], t[5]);
                        }

                        if($(obj).attr('fieldType') == "date")
                        {
                            obj.value = formatDate(d);
                        }
                        else
                        {
                            obj.value = formatDatetime(d);
                        }
                        
                    }
                    else {
                        obj.value = value;
                    }

                }
                else {
                    obj.value = value;
                }
                break;
            default:
                $(obj).text(value);
                $(obj).val(value);
                break;
        }
    });
}
function getAllFields() {
    var rd = {};
    var dtArray = new Array();
    $("[dt]").each(function (index, obj) {
        var jq = $(obj);
        var tablename = jq.attr("dt");
        if (dtArray.indexOf(tablename) == "-1") {
            dtArray.push(tablename)
        }
    });
    for (var i = 0; i < dtArray.length; i++) {
        var _attribute = "[dt='" + dtArray[i] + "']";
        var setrd = { __state: _editData["__state"] };
        $(_attribute).each(function (index, _obj) {
            var _jq = $(_obj);
            var fieldName = _jq.attr("fieldname");
            var _key = _jq.attr("isKey");
            var value = _editData;
            if (value != undefined)
                value = value[fieldName];
            else {
                value = "";
            }
            if (value == null || value === undefined) value = "";
            value = value + "";
            var field = fieldName;

            switch (_obj.tagName.toLocaleLowerCase()) {
                case "select":
                case "input":

                    setrd[field] = _obj.value;
                    if (!setrd["__state"])
                        setrd["__state"] = "1";

                    break;
                default:
                    var val = _jq.val() != "" ? _jq.val() : _jq.text();
                    console.log(_jq.val());
                    console.log(_jq.text());
                    console.log(value);
                    //if (value !== val) {
                        setrd[field] = val;
                        if (!setrd["__state"])
                            setrd["__state"] = "1";
                    //}
                    break;
            }
        });
        rd[dtArray[i]] = [setrd];
    }

    return rd;
}
function getAllKeyValue() {
    var rd = {};
    var dtArray = new Array();
    $("[dt]").each(function (index, obj) {
        var jq = $(obj);
        var tablename = jq.attr("dt");
        if (dtArray.indexOf(tablename) == "-1") {
            dtArray.push(tablename)
        }
    });
    for (var i = 0; i < dtArray.length; i++) {
        var _attribute = "[dt='" + dtArray[i] + "']";
        var setrd = { __state: _editData["__state"] };
        $(_attribute).each(function (index, _obj) {
            var _jq = $(_obj);
            var fieldName = _jq.attr("fieldname");
            var _key = _jq.attr("isKey");
            var value = _editData;
            if (value != undefined)
                value = value[fieldName];
            else {
                value = "";
            }
            if (value == null || value === undefined) value = "";
            value = value + "";
            var field = fieldName;

            switch (_obj.tagName.toLocaleLowerCase()) {
                case "select":
                case "input":
                    if (_key == "true" || _key == true) {
                        setrd[field] = _obj.value;
                        if (!setrd["__state"])
                            setrd["__state"] = "1";
                    }
                    break;
                default:
                    if (_key == "true" || _key == true) {
                        setrd[field] = _jq.text();
                        if (!setrd["__state"])
                            setrd["__state"] = "1";
                    }
                    break;
            }
        });
        rd[dtArray[i]] = [setrd];
    }

    //console.log(rd);
    //_dm.getChangeValue(rd);
    return rd;
}

function getChangeValue() {
    var rd = {};
    var dtArray = new Array();


    $("[dt]").each(function (index, obj) {
        var jq = $(obj);
        var tablename = jq.attr("dt");
        if (dtArray.indexOf(tablename) == "-1") {
            dtArray.push(tablename)
        }
    });
    var IsChange = false;
    for (var i = 0; i < dtArray.length; i++) {
        var _attribute = "[dt='" + dtArray[i] + "'][fieldname]";
        //console.log(_attribute);
        var setrd = { __state: _editData["__state"] };
        var keys = {};
        $(_attribute).each(function (index, _obj) {
            var _jq = $(_obj);
            var fieldName = _jq.attr("fieldname");
            var _key = _jq.attr("isKey");
            var value = _editData;
            if (value != undefined) {
                value = value[fieldName];
                //if (value === undefined)
                //    return true;
            } else {
                value = "";
            }
            if (value == null || value === undefined) value = "";
            value = value + "";
            var field = fieldName;

            switch (_obj.tagName.toLocaleLowerCase()) {
                case "select":
                case "input":
                    var inputType = $(_obj).attr("inputType");
                    if (inputType == "chx") {
                        var str = "";
                        $("[dt='mt'][chxName='" + field + "']").each(function (index, el) {
                            var val = $(this).val();

                            if ($(this).prop("checked") == true) {
                                str += val + ";";
                            }

                            if (index == ($("[dt='mt'][chxName='" + field + "']").length - 1)) {
                                str = str.slice(0, -1);
                                $("input[fieldname='" + field + "']").val(str);
                            }

                        });
                    }

                    if (inputType == "rad") {
                        $("[dt='mt'][radName='" + field + "']").each(function (index, el) {
                            var val = $(this).val();
                            if ($(this).prop("checked") == true) {
                                $("input[fieldname='" + field + "']").val(val);
                            }
                        });
                    }
                    if ($(_obj).attr("isnumber") == "true" && _obj.value)
                        _obj.value = _obj.value.replace(/[,'"]/g, "");
                    else if ($(_obj).attr("fieldtype") == "date" && _obj.value) {
                        //_obj.value = _obj.value.replace(/-/g, "/");
                        if (value) {
                            value = value.replace(/-/g, "/").replace(" 00:00:00", "");
                            _obj.value = _obj.value.replace(/-/g, "/").replace(" 00:00:00", "");
                        }
                    }

                    if (value !== _obj.value) {
                        IsChange = true;
                        setrd[field] = _obj.value;
                        if (!setrd["__state"])
                            setrd["__state"] = "2";
                    }
                    break;
                default:
                    //var val = _jq.val() != "" ? _jq.val() : _jq.text();
                    var val = _jq.val();
                    if (value !== val) {
                        setrd[field] = val;
                        IsChange = true;
                        if (!setrd["__state"])
                            setrd["__state"] = "2";
                    }
                    break;
            }
            if (_key == "true" || _key == true) {
                //setrd[field] = _obj.value;
                keys[field] = _obj.value;
            }
        });
        if (setrd["__state"] == 2 || setrd["__state"] == 1) {
            var tempobj = $.extend({}, setrd);
            delete tempobj.__state;
            var isEmpty = $.isEmptyObject(tempobj);
            if (!isEmpty) {
                setrd = $.extend(setrd, keys);
                rd[dtArray[i]] = [setrd];
            }
        } else if (IsChange == true) {
            setrd = $.extend(setrd, keys);
            rd[dtArray[i]] = [setrd];
        }
    }

    //console.log(rd);
    //_dm.getChangeValue(rd);
    return rd;
}

function getChangeValue1() {
    var rd = {};
    $("[dt]").each(function (index, obj) {
        var jq = $(obj);
        var fieldName = jq.attr("fieldname");
        var talbeNmae = jq.attr("dt");
        var setrd = rd;
        if (!setrd[talbeNmae]) {
            setrd[talbeNmae] = [];
        }
        if (setrd[talbeNmae].length == 0) {
            setrd[talbeNmae].push({});
        }
        setrd = setrd[talbeNmae][0];

        var value = _editData;
        if (value != undefined)
            value = value[fieldName];
        else {
            value = "";
        }
        if (value == null || value === undefined) value = "";
        value = value + "";
        var field = fieldName;

        switch (obj.tagName.toLocaleLowerCase()) {
            case "select":
            case "input":
                if (value !== obj.value) {
                    setrd[field] = obj.value;
                    if (!setrd["__state"])
                        setrd["__state"] = "2";
                }
                break;
            default:
                if (value !== jq.text()) {
                    setrd[field] = jq.text();
                    if (!setrd["__state"])
                        setrd["__state"] = "2";
                }
                break;
        }
    });
    console.log(rd);
    //_dm.getChangeValue(rd);
    return rd;
}


function getchangevalue(arrkey,arrvalue) {
    var rd = { __state: _editdata["__state"] };
    $("[fieldname]").each(function (index, obj) {
        var jq = $(obj);
        var fieldname = jq.attr("fieldname");
        var talbenmae = jq.attr("dt");
        var setrd = rd;
        for (var i = 0; i < arrkey.length; i++) {
            setrd[arrkey[i]] = arrvalue[i];
        }
        rd["jobno"] = _editdata["jobno"]||'';
        
                var oldrd = _datasource[0];
        var arr = fieldname.split(".");
        for (var i = 0; i < arr.length - 1; i++) {
            if (!setrd[arr[i]])
                setrd[arr[i]] = {};
            setrd = setrd[arr[i]];
        }
        var value = _editdata;
        for (var i = 0; i < arr.length; i++) {
            if ($.isarray(value)) {
                if (value.length > 0)
                    value = value[0];
                else {
                    value = "";
                    break;
                }
            }
            if (value != undefined)
                value = value[arr[i]];
            else {
                value = "";
            }
        }
        if (value == null || value === undefined) value = "";
        value = value + "";
        var field = arr[arr.length - 1];

        switch (obj.tagname.tolocalelowercase()) {
            case "select":
            case "input":
                if (value !== obj.value) {
                    setrd[field] = obj.value;
                    if (!setrd["__state"])
                    setrd["__state"] = "2";
                }
                break;
            default:
                if (value !== jq.text()) {
                    setrd[field] = jq.text();
                    if (!setrd["__state"])
                    setrd["__state"] = "2";
                }

                break;
        }
        for (var i = 0; i < arrkey.length; i++) {
            setrd[arrkey[i]] = arrvalue[i];
        }
    });
    _dm.getchangevalue(rd);
    return [rd];
}

function getAllFieldValue() {
    var rd = {};
    $("[fieldname]").each(function (index, obj) {
        var jq = $(obj);
        var fieldName = jq.attr("fieldname");
        var setrd = rd;
        var arr = fieldName.split(".");
        for (var i = 0; i < arr.length - 1; i++) {
            if (!setrd[arr[i]])
                setrd[arr[i]] = {};
            setrd = setrd[arr[i]];
        }
        var field = arr[arr.length - 1];
        switch (obj.tagName.toLocaleLowerCase()) {
            case "select":
            case "input":
                setrd[field] = obj.value;
                break;
            default:
                setrd[field] = jq.text();
                break;
        }
    });
    return rd;
}

function formatDate(date) {
    var month = parseInt(date.getMonth());
    var day = parseInt(date.getDate());
    month = month + 1;
    month = month < 10 ? '0'+month : month;
    day = day < 10 ? '0'+day : day;
    return  date.getFullYear() + "-" + month + "-" + day;
}

function formatDatetime(date) {
  var hours = date.getHours();
  var minutes = parseInt(date.getMinutes());
  var month = parseInt(date.getMonth());
  var day = parseInt(date.getDate());
  month = month + 1;
  hours = hours < 10 ? '0'+hours : hours;
  minutes = minutes < 10 ? '0'+minutes : minutes;
  month = month < 10 ? '0'+month : month;
  day = day < 10 ? '0'+day : day;

  var strTime = hours + ':' + minutes;
  return date.getFullYear() + "-" + month + "-" + day + " " + strTime;
}


/////copy from old


