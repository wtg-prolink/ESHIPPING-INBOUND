
function initChart(opt, gop)
{

	var myId = $("#SummaryShowChart").attr("id");
	var myModal = $("#ChartDialog");
	var chartTitle  	= opt.chartTitle;
	var chartTarget 	= opt.chartTarget;
	var chartDataSource = opt.chartDataSource;
	var chartType 		= opt.chartType;
	var useBasecondition = (typeof opt.useBasecondition === "undefined") ? true : opt.useBasecondition;

	$.each(chartType, function(index, item) {
		var str = '<option value="'+item.val+'">'+item.label+'</option>';
		$("#chartType").append(str);
	});

	$.each(chartTarget, function(index, item) {
		var str = '<option value="'+item.val+'">'+item.label+'</option>';
		$("#chartTarget").append(str);
	});

	$.each(chartDataSource, function(index, item) {
		var str = '<option value="'+item.val+'">'+item.label+'</option>';
		$("#chartDataSource").append(str);
	});


	$("#ChartDialog").on("show.bs.modal", function(){
		$("#ChartArea").hide();
		$("#ChartConArea").show();
	})
	
	$(document).on("click", "#SummaryShowChart", function(){
		myModal.find(".modal-title").html(chartTitle);
		myModal.modal("show");
		ajustamodal("#ChartDialog");

		var ModalWidth = $("#ChartDialog .modal-lg").width();
		var Modalheigh = $("#ChartDialog .modal-body").height();

		$("#MyChart").width(ModalWidth - 50);
		$("#MyChart").height(Modalheigh);

		//opt.getChartFunc();
	});

	$("#ChartOk").click(function(event) {
		CommonFunc.ToogleLoading(true);
		var formObj = $("#chartForm").serializeObject();
		opt.getChartFunc(formObj);
	});

	$("#ChartOpt").click(function(){
		$("#ChartArea").hide();
		$("#ChartConArea").show();
	});

	opt.getChartFunc = function(obj){
		$("#ChartArea").show();
		$("#ChartConArea").hide();
		var params = loadCondition(gop.searchFormId);
		var url = "";
		var baseCondition = gop.baseCondition;
		if(useBasecondition == false)
		{
			baseCondition = "";
		}
		var postData = {
			'conditions': params,//jQuery.serialize()已经是进行URL编码过的。
			'baseCondition': baseCondition
		};
		$.each(chartType, function(index, item) {
			if(obj.chartType == item.val)
			{
				url = item.url;
				callBack = item.callBack;
			}
		});

		$.each(obj, function(index, val) {
			postData[index] = val;
		});

	    $.getJSON(url,postData,
	    	function (data) {
	    		var success = true;
		        // Create the chart
		        console.log(typeof opt.beforeShowChart);
		        if(typeof opt.beforeShowChart == "function")
		        {
		        	success = opt.beforeShowChart(data.data);
		        }

		        if(success === true)
		        {
		        	callBack(data.data);
		        }
		        else
		        {
		        	$("#ChartArea").hide();
		        	$("#ChartConArea").show();
		        }
		        
		        CommonFunc.ToogleLoading(false);
	    	}
	    );
	}

}