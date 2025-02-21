
var _UpdateDetail = {};
_UpdateDetail.lang = {
    IsOk: "Send successfully",
    BeforSend: "Uploading",
    failed: "Uploading is failed",
    title: "Batch Upload",
    file: "select the file",
    submitbtn: "Uplaod",
    NoData:"Please Select Data"
};

_UpdateDetail._showPoNumberInputDialog = '<div class="modal fade" id="PoNumberInput">\
    <div class="modal-dialog modal-lg">\
        <div class="modal-content">\
            <div class="modal-header">\
                <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
                <h4 class="modal-title">Batch Upload Discharge Date</h4>\
            </div>\
            <div class="modal-body">\
                <button class="btn btn-sm btn-info" id="SetSameAddr">As above</button>\
                <table class="table">\
                </table>\
            </div>\
            <div class="modal-footer">\
                <button type="submit" class="btn btn-md btn-info" id="AddAddrBtn">Insert</button>\
                <button type="button" class="btn btn-md btn-danger" data-dismiss="modal" id="ModalClose">Close</button>\
            </div>\
        </div>\
    </div>\
</div>';

_UpdateDetail.RegisterPoNumberInputBtn = function () {
    if ($("#PoNumberInput").length <= 0) {
        $("body").append($(this._showPoNumberInputDialog));
    }

};