﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.34014.
// 
#pragma warning disable 1591

namespace Task.DN {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.33440")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="DNServicePortBinding", Namespace="http://ws.wct.com/")]
    public partial class DNService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback ackOperationCompleted;
        
        private System.Threading.SendOrPostCallback getOperationCompleted;
        
        private System.Threading.SendOrPostCallback setOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public DNService() {
            this.Url = global::Task.Properties.Settings.Default.AsusDNPush_DN_DNService;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event ackCompletedEventHandler ackCompleted;
        
        /// <remarks/>
        public event getCompletedEventHandler getCompleted;
        
        /// <remarks/>
        public event setCompletedEventHandler setCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", RequestNamespace="http://ws.wct.com/", ResponseNamespace="http://ws.wct.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void ack([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string dc_id, [System.Xml.Serialization.XmlElementAttribute("dn_ack", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] dnAck[] dn_ack) {
            this.Invoke("ack", new object[] {
                        dc_id,
                        dn_ack});
        }
        
        /// <remarks/>
        public void ackAsync(string dc_id, dnAck[] dn_ack) {
            this.ackAsync(dc_id, dn_ack, null);
        }
        
        /// <remarks/>
        public void ackAsync(string dc_id, dnAck[] dn_ack, object userState) {
            if ((this.ackOperationCompleted == null)) {
                this.ackOperationCompleted = new System.Threading.SendOrPostCallback(this.OnackOperationCompleted);
            }
            this.InvokeAsync("ack", new object[] {
                        dc_id,
                        dn_ack}, this.ackOperationCompleted, userState);
        }
        
        private void OnackOperationCompleted(object arg) {
            if ((this.ackCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ackCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", RequestNamespace="http://ws.wct.com/", ResponseNamespace="http://ws.wct.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("dn_content", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public dnContent[] get([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string dc_id) {
            object[] results = this.Invoke("get", new object[] {
                        dc_id});
            return ((dnContent[])(results[0]));
        }
        
        /// <remarks/>
        public void getAsync(string dc_id) {
            this.getAsync(dc_id, null);
        }
        
        /// <remarks/>
        public void getAsync(string dc_id, object userState) {
            if ((this.getOperationCompleted == null)) {
                this.getOperationCompleted = new System.Threading.SendOrPostCallback(this.OngetOperationCompleted);
            }
            this.InvokeAsync("get", new object[] {
                        dc_id}, this.getOperationCompleted, userState);
        }
        
        private void OngetOperationCompleted(object arg) {
            if ((this.getCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.getCompleted(this, new getCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", RequestNamespace="http://ws.wct.com/", ResponseNamespace="http://ws.wct.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("dn_message", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public dnMessage[] set([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string dc_id, [System.Xml.Serialization.XmlElementAttribute("dn_content", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] dnContent[] dn_content) {
            object[] results = this.Invoke("set", new object[] {
                        dc_id,
                        dn_content});
            return ((dnMessage[])(results[0]));
        }
        
        /// <remarks/>
        public void setAsync(string dc_id, dnContent[] dn_content) {
            this.setAsync(dc_id, dn_content, null);
        }
        
        /// <remarks/>
        public void setAsync(string dc_id, dnContent[] dn_content, object userState) {
            if ((this.setOperationCompleted == null)) {
                this.setOperationCompleted = new System.Threading.SendOrPostCallback(this.OnsetOperationCompleted);
            }
            this.InvokeAsync("set", new object[] {
                        dc_id,
                        dn_content}, this.setOperationCompleted, userState);
        }
        
        private void OnsetOperationCompleted(object arg) {
            if ((this.setCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.setCompleted(this, new setCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34230")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.wct.com/")]
    public partial class dnAck {
        
        private string delivery_noField;
        
        private string event_idField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string delivery_no {
            get {
                return this.delivery_noField;
            }
            set {
                this.delivery_noField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string event_id {
            get {
                return this.event_idField;
            }
            set {
                this.event_idField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34230")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.wct.com/")]
    public partial class dnMessage {
        
        private string delivery_noField;
        
        private int error_codeField;
        
        private string[] error_messagesField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string delivery_no {
            get {
                return this.delivery_noField;
            }
            set {
                this.delivery_noField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int error_code {
            get {
                return this.error_codeField;
            }
            set {
                this.error_codeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("error_messages", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
        public string[] error_messages {
            get {
                return this.error_messagesField;
            }
            set {
                this.error_messagesField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34230")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.wct.com/")]
    public partial class dnContent {
        
        private string delivery_noField;
        
        private string ship_fromField;
        
        private string ship_toField;
        
        private string ship_wayField;
        
        private string conditionField;
        
        private string shipperField;
        
        private string forwarderField;
        
        private string incotermField;
        
        private string ems_vendor_site_codeField;
        
        private string bill_toField;
        
        private string ship_to_customerField;
        
        private string ship_to_locationField;
        
        private string so_lineField;
        
        private string customer_poField;
        
        private string shipment_priorityField;
        
        private string product_typeField;
        
        private string model_nameField;
        
        private string saField;
        
        private string mawbField;
        
        private string hawbField;
        
        private string vesselField;
        
        private string container_noField;
        
        private string container_typeField;
        
        private string container_sizeField;
        
        private int pcsField;
        
        private bool pcsFieldSpecified;
        
        private int palletField;
        
        private bool palletFieldSpecified;
        
        private int cartonField;
        
        private bool cartonFieldSpecified;
        
        private double chargable_weightField;
        
        private bool chargable_weightFieldSpecified;
        
        private double gross_weightField;
        
        private bool gross_weightFieldSpecified;
        
        private double volume_weightField;
        
        private bool volume_weightFieldSpecified;
        
        private string to_cdField;
        
        private string transfer_dateField;
        
        private string cradField;
        
        private string ship_confirm_dateField;
        
        private string ex_factory_dateField;
        
        private string pickup_dateField;
        
        private string telex_releasedField;
        
        private string telex_released_dateField;
        
        private string etdField;
        
        private string atdField;
        
        private string etaField;
        
        private string ataField;
        
        private string customs_declaration_dateField;
        
        private string customs_clearance_dateField;
        
        private string nldc_in_dateField;
        
        private string nldc_expected_out_dateField;
        
        private string nldc_out_dateField;
        
        private string slot_dateField;
        
        private string requested_cdc_in_dateField;
        
        private string cdc_in_dateField;
        
        private string cdc_expected_out_dateField;
        
        private string cdc_out_dateField;
        
        private string pod_dateField;
        
        private string event_idField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string delivery_no {
            get {
                return this.delivery_noField;
            }
            set {
                this.delivery_noField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ship_from {
            get {
                return this.ship_fromField;
            }
            set {
                this.ship_fromField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ship_to {
            get {
                return this.ship_toField;
            }
            set {
                this.ship_toField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ship_way {
            get {
                return this.ship_wayField;
            }
            set {
                this.ship_wayField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string condition {
            get {
                return this.conditionField;
            }
            set {
                this.conditionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string shipper {
            get {
                return this.shipperField;
            }
            set {
                this.shipperField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string forwarder {
            get {
                return this.forwarderField;
            }
            set {
                this.forwarderField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string incoterm {
            get {
                return this.incotermField;
            }
            set {
                this.incotermField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ems_vendor_site_code {
            get {
                return this.ems_vendor_site_codeField;
            }
            set {
                this.ems_vendor_site_codeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string bill_to {
            get {
                return this.bill_toField;
            }
            set {
                this.bill_toField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ship_to_customer {
            get {
                return this.ship_to_customerField;
            }
            set {
                this.ship_to_customerField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ship_to_location {
            get {
                return this.ship_to_locationField;
            }
            set {
                this.ship_to_locationField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string so_line {
            get {
                return this.so_lineField;
            }
            set {
                this.so_lineField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string customer_po {
            get {
                return this.customer_poField;
            }
            set {
                this.customer_poField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string shipment_priority {
            get {
                return this.shipment_priorityField;
            }
            set {
                this.shipment_priorityField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string product_type {
            get {
                return this.product_typeField;
            }
            set {
                this.product_typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string model_name {
            get {
                return this.model_nameField;
            }
            set {
                this.model_nameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sa {
            get {
                return this.saField;
            }
            set {
                this.saField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string mawb {
            get {
                return this.mawbField;
            }
            set {
                this.mawbField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string hawb {
            get {
                return this.hawbField;
            }
            set {
                this.hawbField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string vessel {
            get {
                return this.vesselField;
            }
            set {
                this.vesselField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string container_no {
            get {
                return this.container_noField;
            }
            set {
                this.container_noField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string container_type {
            get {
                return this.container_typeField;
            }
            set {
                this.container_typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string container_size {
            get {
                return this.container_sizeField;
            }
            set {
                this.container_sizeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int pcs {
            get {
                return this.pcsField;
            }
            set {
                this.pcsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool pcsSpecified {
            get {
                return this.pcsFieldSpecified;
            }
            set {
                this.pcsFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int pallet {
            get {
                return this.palletField;
            }
            set {
                this.palletField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool palletSpecified {
            get {
                return this.palletFieldSpecified;
            }
            set {
                this.palletFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int carton {
            get {
                return this.cartonField;
            }
            set {
                this.cartonField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cartonSpecified {
            get {
                return this.cartonFieldSpecified;
            }
            set {
                this.cartonFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double chargable_weight {
            get {
                return this.chargable_weightField;
            }
            set {
                this.chargable_weightField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool chargable_weightSpecified {
            get {
                return this.chargable_weightFieldSpecified;
            }
            set {
                this.chargable_weightFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double gross_weight {
            get {
                return this.gross_weightField;
            }
            set {
                this.gross_weightField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool gross_weightSpecified {
            get {
                return this.gross_weightFieldSpecified;
            }
            set {
                this.gross_weightFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double volume_weight {
            get {
                return this.volume_weightField;
            }
            set {
                this.volume_weightField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool volume_weightSpecified {
            get {
                return this.volume_weightFieldSpecified;
            }
            set {
                this.volume_weightFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string to_cd {
            get {
                return this.to_cdField;
            }
            set {
                this.to_cdField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string transfer_date {
            get {
                return this.transfer_dateField;
            }
            set {
                this.transfer_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string crad {
            get {
                return this.cradField;
            }
            set {
                this.cradField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ship_confirm_date {
            get {
                return this.ship_confirm_dateField;
            }
            set {
                this.ship_confirm_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ex_factory_date {
            get {
                return this.ex_factory_dateField;
            }
            set {
                this.ex_factory_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string pickup_date {
            get {
                return this.pickup_dateField;
            }
            set {
                this.pickup_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string telex_released {
            get {
                return this.telex_releasedField;
            }
            set {
                this.telex_releasedField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string telex_released_date {
            get {
                return this.telex_released_dateField;
            }
            set {
                this.telex_released_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string etd {
            get {
                return this.etdField;
            }
            set {
                this.etdField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string atd {
            get {
                return this.atdField;
            }
            set {
                this.atdField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string eta {
            get {
                return this.etaField;
            }
            set {
                this.etaField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ata {
            get {
                return this.ataField;
            }
            set {
                this.ataField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string customs_declaration_date {
            get {
                return this.customs_declaration_dateField;
            }
            set {
                this.customs_declaration_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string customs_clearance_date {
            get {
                return this.customs_clearance_dateField;
            }
            set {
                this.customs_clearance_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string nldc_in_date {
            get {
                return this.nldc_in_dateField;
            }
            set {
                this.nldc_in_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string nldc_expected_out_date {
            get {
                return this.nldc_expected_out_dateField;
            }
            set {
                this.nldc_expected_out_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string nldc_out_date {
            get {
                return this.nldc_out_dateField;
            }
            set {
                this.nldc_out_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string slot_date {
            get {
                return this.slot_dateField;
            }
            set {
                this.slot_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string requested_cdc_in_date {
            get {
                return this.requested_cdc_in_dateField;
            }
            set {
                this.requested_cdc_in_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string cdc_in_date {
            get {
                return this.cdc_in_dateField;
            }
            set {
                this.cdc_in_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string cdc_expected_out_date {
            get {
                return this.cdc_expected_out_dateField;
            }
            set {
                this.cdc_expected_out_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string cdc_out_date {
            get {
                return this.cdc_out_dateField;
            }
            set {
                this.cdc_out_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string pod_date {
            get {
                return this.pod_dateField;
            }
            set {
                this.pod_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string event_id {
            get {
                return this.event_idField;
            }
            set {
                this.event_idField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.33440")]
    public delegate void ackCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.33440")]
    public delegate void getCompletedEventHandler(object sender, getCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.33440")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class getCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal getCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public dnContent[] Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((dnContent[])(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.33440")]
    public delegate void setCompletedEventHandler(object sender, setCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.33440")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class setCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal setCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public dnMessage[] Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((dnMessage[])(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591