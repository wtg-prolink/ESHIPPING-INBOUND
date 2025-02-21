$Sub1Grid = $("#Sub1Grid");
$Sub2Grid = $("#Sub2Grid");

function DataSummary()
{
    //init Search
    var gop = {};
    var numberTemplate = "2";
    gop.gridColModel = [
         { name: 'UId', showname: 'ID', sorttype: 'string', hidden: true, viewable: false },
	     { name: 'TranMode', title: '@Resources.Locale.L_DNApproveManage_TranMode', index: 'TranMode', width: 70, align: 'left', sorttype: 'string', hidden: false },
	     { name: 'Year', title: '@Resources.Locale.L_ContainUsage_Year', index: 'Year', width: 60, align: 'left', sorttype: 'string', hidden: false },
         { name: 'Week', title: '@Resources.Locale.L_ContainUsage_Week', index: 'Week', width: 60, align: 'left', sorttype: 'string', hidden: false },
	     //{ name: 'Term', title: '公司', index: 'Term', width: 90, align: 'left', sorttype: 'string', hidden: false },
	     { name: 'Pol', title: '@Resources.Locale.L_ForecastQueryData_PolName', index: 'Pol', width: 150, align: 'left', sorttype: 'string', hidden: false },
	     { name: 'Region', title: '@Resources.Locale.L_BaseLookup_Region', index: 'Region', width: 100, align: 'left', sorttype: 'string', hidden: false },
	     { name: 'Pod', title: '@Resources.Locale.L_ForecastQueryData_PodName', index: 'Pod', width: 150, align: 'left', sorttype: 'string', hidden: false },
	     { name: 'FreightTerm', title: '@Resources.Locale.L_PartyDocSetup_FreightTerm', index: 'FreightTerm', width: 150, align: 'left', sorttype: 'string', hidden: false },
         { name: 'Carrier', title: '@Resources.Locale.L_ContainUsage_Carrier', index: 'Carrier', width: 150, align: 'left', sorttype: 'string', hidden: false },
         { name: 'LspCd', title: 'Lsp Cd', index: 'LspCd', width: 150, align: 'left', sorttype: 'string', hidden: false },
	     { name: 'Ffeu', title: '@Resources.Locale.L_DM_FFEU', index: 'Ffeu', width: 100, align: 'right', sorttype: 'number', hidden: false },
	     { name: 'Afeu', title: '@Resources.Locale.L_DM_AFEU', index: 'Afeu', width: 100, align: 'right', sorttype: 'number', hidden: false },
	     { name: 'Bfeu', title: '@Resources.Locale.L_DM_BFEU', index: 'Bfeu', width: 100, align: 'right', sorttype: 'number', hidden: true },
         { name: 'Rfeu', title: '@Resources.Locale.L_DM_RFEU', index: 'Rfeu', width: 100, align: 'right', sorttype: 'number', hidden: true },
         { name: 'OrderBy', title: 'Order By', index: 'OrderBy', width: 70, align: 'left', sorttype: 'string', hidden: false }
    ];
    gop.AddUrl = false;
    gop.gridId = "containerInfoGrid";
    gop.gridAttr = { caption: "@Resources.Locale.L_DNManage_AssList", height: "auto", refresh: true, exportexcel: true, conditions: encodeURI(loadCondition) };
    gop.gridSearchUrl = rootPath + "DistManage/DisTributInquiryData";
    gop.searchColumns = getSelectColumn(gop.gridColModel);

    //SAVE CONDITION 為避免以後須調整畫面，ID都要傳到元件
    gop.searchFormId = "ConditionArea";
    gop.searchDivId = "SearchArea"; 
    gop.StatusAreaId = "StatusArea";
    gop.BtnGroupId = "BtnGroupArea";

    gop.gridFunc = function (map) {
    }

    gop.searchFormId = "ConditionArea";
    gop.searchDivId = "SearchArea";

    //initSearch button group {id:"Unique",name:"Show Name",func:"Click Event"}
    gop.btnGroup = [{
            id: "btn01",
            name: "@Resources.Locale.L_DNManage_BkDetail",
            func: function () {
                alert("@Resources.Locale.L_DNManage_BkDetail");
            }
        }
    ]

    initSearch(gop);
}

$(document).ready(function ($) {
    DataSummary();
    
});