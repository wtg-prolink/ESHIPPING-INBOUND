--测试机新增台北嘉捷 inbound站点，Location code=OBMTPIB。基本建档资料请copy自inbound TP。

/*****????***********/

 INSERT INTO [dbo].[SYS_ROLE]
           ([FID]
           ,[GROUP_ID]
           ,[CMP]
           ,[STN]
           ,[FDESCP]
           ,[SUB_ROLES])
SELECT [FID]
      ,[GROUP_ID]
      ,'OBMTPIB'
      ,[STN]
      ,[FDESCP]
      ,[SUB_ROLES]
  FROM [dbo].[SYS_ROLE]
 where FID in ('SDADMIN','ADMINISTRATOR' )and cmp='TP' 

/*****??????***********/
INSERT INTO [dbo].[SYS_ROLE_OBJ_PMS]
           ([FROLE_ID]
           ,[FOBJ_ID]
           ,[GROUP_ID]
           ,[CMP]
           ,[STN]
           ,[FPMLIST])
SELECT [FROLE_ID]
      ,[FOBJ_ID]
      ,[GROUP_ID]
      ,'OBMTPIB'
      ,[STN]
      ,[FPMLIST]
  FROM [dbo].[SYS_ROLE_OBJ_PMS] WHERE FROLE_ID in ('SDADMIN','ADMINISTRATOR') and cmp='TP' 

  /*****????***************/
INSERT INTO [dbo].[SYS_ACCT_ROLE]
           ([FACCT_ID]
			  ,[FROLE_ID]
			  ,[GROUP_ID]
			  ,[CMP]
			  ,[STN]
			  ,[FDESCP]
			  ,[RCMP]
			  ,[RSTN])
SELECT [FACCT_ID]
      ,[FROLE_ID]
      ,[GROUP_ID]
      ,'OBMTPIB'
      ,[STN]
      ,[FDESCP]
      ,'OBMTPIB'
      ,[RSTN]
  FROM  [dbo].[SYS_ACCT_ROLE] WHERE CMP = 'TP' and [FACCT_ID] in( 'ADMIN','MANA10')

INSERT INTO [dbo].[SYS_ACCT]
           ([U_ID]
      ,[GROUP_ID],[CMP],[STN],[DEP]
      ,[U_NAME],[U_PASSWORD],[U_STATUS]
      ,[UPDATE_PRI_DATE],[MD5_INFO],[U_PHONE]
      ,[U_EMAIL],[U_MANAGER],[BU]
      ,[U_WECHAT],[U_QQ],[MAIL_FLAG]
      ,[MSG_FLAG],[WECHAT_FLAG],[QQ_FLAG],[U_EXT]
      ,[U_PRI],[IO_FLAG],[CMP_PRI],[MODI_PW_DATE]
      ,[SAP_ID],[CARD_NO],[TRAN_TYPE]
      ,[FAIL_COUNT],[CREATE_BY]
      ,[CREATE_DATE],[RC]
      ,[IS_MD5]
      ,[TCMP]
      ,[PLANT_PRI])
SELECT [U_ID]
      ,[GROUP_ID]
      ,'OBMTPIB'
      ,[STN],[DEP],[U_NAME]
      ,[U_PASSWORD],[U_STATUS]
      ,[UPDATE_PRI_DATE],[MD5_INFO]
      ,[U_PHONE],[U_EMAIL]
      ,[U_MANAGER],[BU]
      ,[U_WECHAT],[U_QQ]
      ,[MAIL_FLAG],[MSG_FLAG]
      ,[WECHAT_FLAG]
      ,[QQ_FLAG],[U_EXT]
      ,[U_PRI],[IO_FLAG]
      ,[CMP_PRI],[MODI_PW_DATE]
      ,[SAP_ID],[CARD_NO]
      ,[TRAN_TYPE],[FAIL_COUNT]
      ,[CREATE_BY],[CREATE_DATE],[RC],[IS_MD5]
      ,[TCMP],[PLANT_PRI]
  FROM [dbo].[SYS_ACCT] WHERE CMP = 'TP' AND U_ID IN ('MANA10','ADMIN')


/**********??????*****************/
INSERT INTO [dbo].[BSCODE_KIND]
           ([GROUP_ID]
           ,[CD_TYPE]
           ,[CD_DESCP]
           ,[CMP]
           ,[STN]
           ,[CREATE_BY]
           ,[CREATE_DATE]
           ,[MODIFY_BY]
           ,[DEP]
           ,[MODIFY_DATE])
SELECT TOP 1000 [GROUP_ID]
      ,[CD_TYPE]
      ,[CD_DESCP]
      ,'OBMTPIB'
      ,[STN]
      ,[CREATE_BY]
      ,[CREATE_DATE]
      ,[MODIFY_BY]
      ,[DEP]
      ,[MODIFY_DATE]
  FROM  [dbo].[BSCODE_KIND] WHERE CMP = 'TP'

/**********??????*****************/
INSERT INTO [dbo].[BSCODE]
           ([GROUP_ID]
           ,[CD_TYPE]
           ,[CD],[CD_DESCP],[CMP]
           ,[STN],[CREATE_BY]
           ,[CREATE_DATE],[MODIFY_BY]
           ,[AMS_CODE],[INTTRA]
           ,[AR_CD],[AP_CD]
           ,[DEP],[APPLY]
           ,[LOCATION]
           ,[ORDER_BY])
SELECT [GROUP_ID]
      ,[CD_TYPE]
      ,[CD]
      ,[CD_DESCP]
      ,'OBMTPIB'
      ,[STN]
      ,[CREATE_BY],[CREATE_DATE]
      ,[MODIFY_BY],[AMS_CODE]
      ,[INTTRA],[AR_CD],[AP_CD]
      ,[DEP]
      ,[APPLY]
      ,[LOCATION]
      ,[ORDER_BY]
  FROM [dbo].[BSCODE] WHERE CMP = 'TP'

  --UPDATE [dbo].[APPROVE_FLOW_M] SET
  --SITE_ID = '*' 
/*****???????????*****************/
INSERT INTO [dbo].[APPROVE_FLOW_M]
           ([APPROVE_CODE]
           ,[APPROVE_NAME]
           ,[GROUP_ID]
           ,[CMP_ID]
           ,[SITE_ID]
           ,[APPROVE_STATUS]
           ,[CREATE_BY]
           ,[CREATE_DATE]
           ,[MODIFY_BY]
           ,[MODIFY_DATE]
           ,[AU_ID])
SELECT [APPROVE_CODE]
      ,[APPROVE_NAME]
      ,[GROUP_ID]
      ,'OBMTPIB'
      ,[SITE_ID]
      ,[APPROVE_STATUS]
      ,[CREATE_BY]
      ,[CREATE_DATE]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
      ,[AU_ID]
  FROM [dbo].[APPROVE_FLOW_M] WHERE CMP_ID ='TP'

/***********????**************/
INSERT INTO [dbo].[BSSTATE]
           ([U_ID]
           ,[GROUP_ID]
           ,[CMP]
           ,[CNTRY_CD]
           ,[CNTRY_NM]
           ,[STATE_CD]
           ,[STATE_NM]
           ,[REGION_CD]
           ,[REGION_NM])
SELECT NewID()
      ,[GROUP_ID]
      ,'OBMTPIB'
      ,[CNTRY_CD]
      ,[CNTRY_NM]
      ,[STATE_CD]
      ,[STATE_NM]
      ,[REGION_CD]
      ,[REGION_NM]
  FROM [dbo].[BSSTATE] WHERE CMP = 'TP'
  
--??SMCC  ??????
INSERT INTO [dbo].[SMCC]
           ([U_ID]
           ,[GROUP_ID]
           ,[CMP]
           ,[DEP]
           ,[COMPANY]
           ,[COST_CENTER]
           ,[SHORT_DESCP]
           ,[DESCP]
           ,[PRINCIPAL]
           ,[CREATE_BY]
           ,[CREATE_DATE]
           ,[MODIFY_BY]
           ,[MODIFY_DATE])
SELECT NewID()
      ,[GROUP_ID]
      ,'OBMTPIB'
      ,[DEP]
      ,[COMPANY]
      ,[COST_CENTER]
      ,[SHORT_DESCP]
      ,[DESCP]
      ,[PRINCIPAL]
      ,[CREATE_BY]
      ,[CREATE_DATE]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
  FROM [dbo].[SMCC] WHERE CMP = 'TP'

/*********??????*****************************************************************?????*/

INSERT INTO [dbo].[APPROVE_ATTRIBUTE]
           ([U_ID]
      ,[GROUP_ID]
      ,[CMP]
      ,[STN]
      ,[APPROVE_ATTR]
      ,[MODIFY_BY]
      ,[MODIFY_DATE])
SELECT NewID()
      ,[GROUP_ID]
      ,'OBMTPIB'
      ,[STN]
      ,[APPROVE_ATTR]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
  FROM [dbo].[APPROVE_ATTRIBUTE] WHERE CMP = 'TP'

INSERT INTO [dbo].[APPROVE_ATTR_D]
           ([U_ID]
           ,[U_FID]
           ,[APPROVE_GROUP]
           ,[GROUP_ID]
           ,[CMP]
           ,[STN]
           ,[APPROVE_ATTR]
           ,[GROUP_DESCP]
           ,[MODIFY_BY]
           ,[MODIFY_DATE])
SELECT [U_ID]
      ,[U_FID]
      ,[APPROVE_GROUP]
      ,[GROUP_ID]
      ,'OBMTPIB'
      ,[STN]
      ,[APPROVE_ATTR]
      ,[GROUP_DESCP]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
  FROM [dbo].[APPROVE_ATTR_D] WHERE CMP = 'TP'


INSERT INTO [dbo].[APPROVE_ATTR_DP]
           ([U_ID]
           ,[U_FID]
           ,[U_FFID]
           ,[GROUP_ID],[CMP]
           ,[STN],[USER_ID]
           ,[U_EMAIL]
           ,[BY_EMAIL],[BY_MSG]
           ,[BY_WECHAT],[BY_APP]
           ,[REMARK],[MODIFY_BY]
           ,[MODIFY_DATE])
SELECT [U_ID]
      ,[U_FID]
      ,[U_FFID]
      ,[GROUP_ID]
      ,'OBMTPIB'
      ,[STN]
      ,[USER_ID]
      ,[U_EMAIL],[BY_EMAIL]
      ,[BY_MSG],[BY_WECHAT]
      ,[BY_APP],[REMARK]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
  FROM [dbo].[APPROVE_ATTR_DP] WHERE CMP = 'TP'

INSERT INTO [dbo].[TKPDM]
           ([U_ID]
           ,[TRAN_MODE],[TERM]
           ,[FREIGHT_TERM],[PARTY_TYPE]
           ,[PARTY_DESCP]
           ,[STS_CD],[STS_DESCP],[REMARK]
           ,[CREATE_DATE],[CREATE_BY]
           ,[MODIFY_DATE]
           ,[MODIFY_BY]
           ,[GROUP_ID]
           ,[CMP],[STN]
           ,[DEP]
           ,[PARTY_NO]
           ,[PARTY_NM])
SELECT  [U_ID]
      ,[TRAN_MODE]
      ,[TERM]
      ,[FREIGHT_TERM]
      ,[PARTY_TYPE],[PARTY_DESCP]
      ,[STS_CD],[STS_DESCP]
      ,[REMARK]
      ,[CREATE_DATE],[CREATE_BY]
      ,[MODIFY_DATE],[MODIFY_BY]
      ,[GROUP_ID]
      ,'OBMTPIB'
      ,[STN],[DEP],[PARTY_NO]
      ,[PARTY_NM]
  FROM  [dbo].[TKPDM] WHERE CMP = 'TP'


--??????????ApproveGroup??????Attribute?????????????????
 
update APPROVE_ATTR_D set u_fid=(
SELECT U_ID FROM APPROVE_ATTRIBUTE WHERE CMP='OBMTPIB' and approve_attr='DN') where
 CMP='OBMTPIB' AND APPROVE_ATTR='DN'  
   
update APPROVE_ATTR_D set u_fid=(
SELECT U_ID FROM APPROVE_ATTRIBUTE WHERE CMP='OBMTPIB' and approve_attr='INVALID') where
 CMP='OBMTPIB' AND APPROVE_ATTR='INVALID'  

 update APPROVE_ATTR_D set u_fid=(
SELECT U_ID FROM APPROVE_ATTRIBUTE WHERE CMP='OBMTPIB' and approve_attr='BILLING') where
 CMP='OBMTPIB' AND APPROVE_ATTR='BILLING'  

 update APPROVE_ATTR_D set u_fid=(
SELECT U_ID FROM APPROVE_ATTRIBUTE WHERE CMP='OBMTPIB' and approve_attr='QUOT') where
 CMP='OBMTPIB' AND APPROVE_ATTR='QUOT'  
   
   
--????
INSERT INTO [dbo].[TKPMT]
           ([U_ID]
		,[GROUP_ID]
      ,[CMP]
      ,[STN]
      ,[DEP]
           ,[SEQ]
           ,[MT_TYPE]
           ,[MT_NAME]
           ,[MT_CONTENT]
           ,[REMARK])
SELECT TOP 1000 
		NewID() ,
		[GROUP_ID]
      ,'OBMTPIB'
      ,[STN]
      ,[DEP]
           ,[SEQ]
           ,[MT_TYPE]
           ,[MT_NAME]
           ,[MT_CONTENT]
           ,[REMARK]
  FROM [dbo].[TKPMT] WHERE CMP = 'TP';

  UPDATE TKPMT SET MT_CONTENT=replace( CAST(MT_CONTENT as varchar(8000)),'TP','OBMTPIB')  WHERE MT_CONTENT LIKE '%TP%' AND CMP = 'OBMTPIB'



INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('RFQ_NO', 'OBMTPIB', '*', 'TPV', '2015-12-15 14:35', ';OBMTPIB;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('RFQ_NO',1,'20151214','CMP','公司',2,'OBMTPIB','OBMTPIB','*','TPV'),
('RFQ_NO',2,'20151214','CONST','常量',1,'A','OBMTPIB','*','TPV'),
('RFQ_NO',3,'20151214','YY','年度',2,'YY','OBMTPIB','*','TPV'),
('RFQ_NO',4,'20151214','MM','月份',2,'MM','OBMTPIB','*','TPV'),
('RFQ_NO',5,'20151214','NO','流水號',6,NULL,'OBMTPIB','*','TPV');

 

INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('RFQ_NO', '询价自动编号', '20151214', 'OBMTPIB', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('QUOT_NO', 'OBMTPIB', '*', 'TPV', '2016-01-01 14:35', ';OBMTPIB;15;12' )



INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('QUOT_NO',1,'20160101','CMP','公司',2,'OBMTPIB','OBMTPIB','*','TPV'),
('QUOT_NO',2,'20160101','CONST','常量',1,'Q','OBMTPIB','*','TPV'),
('QUOT_NO',3,'20160101','YY','年度',2,'YY','OBMTPIB','*','TPV'),
('QUOT_NO',4,'20160101','MM','月份',2,'MM','OBMTPIB','*','TPV'),
('QUOT_NO',5,'20160101','NO','流水號',6,NULL,'OBMTPIB','*','TPV');




INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('QUOT_NO', '询价自动编号', '20160101', 'OBMTPIB', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('BAT_NO', 'OBMTPIB', '*', 'TPV', '2016-01-01 14:35', ';OBMTPIB;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('BAT_NO',1,'20160101','CMP','公司',2,'OBMTPIB','OBMTPIB','*','TPV'),
('BAT_NO',2,'20160101','CONST','常量',2,'LT','OBMTPIB','*','TPV'),
('BAT_NO',3,'20160101','YY','年度',2,'YY','OBMTPIB','*','TPV'),
('BAT_NO',4,'20160101','MM','月份',2,'MM','OBMTPIB','*','TPV'),
('BAT_NO',5,'20160101','NO','流水號',6,NULL,'OBMTPIB','*','TPV');

 

INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('BAT_NO', '預約批次號', '20160101', 'OBMTPIB', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('TK_NO', 'OBMTPIB', '*', 'TPV', '2016-01-01 14:35', ';OBMTPIB;15;12' )


INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('TK_NO',1,'20160101','CMP','公司',2,'OBMTPIB','OBMTPIB','*','TPV'),
('TK_NO',2,'20160101','CONST','常量',2,'ET','OBMTPIB','*','TPV'),
('TK_NO',3,'20160101','YY','年度',2,'YY','OBMTPIB','*','TPV'),
('TK_NO',4,'20160101','MM','月份',2,'MM','OBMTPIB','*','TPV'),
('TK_NO',5,'20160101','NO','流水號',6,NULL,'OBMTPIB','*','TPV');


INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('TK_NO', '預約批次號', '20160101', 'OBMTPIB', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('RV_NO', 'OBMTPIB', '*', 'TPV', '2016-01-01 14:35', ';OBMTPIB;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('RV_NO',1,'20160101','CMP','公司',2,'OBMTPIB','OBMTPIB','*','TPV'),
('RV_NO',2,'20160101','CONST','常量',2,'R','OBMTPIB','*','TPV'),
('RV_NO',3,'20160101','YY','年度',2,'YY','OBMTPIB','*','TPV'),
('RV_NO',4,'20160101','MM','月份',2,'MM','OBMTPIB','*','TPV'),
('RV_NO',5,'20160101','NO','流水號',6,NULL,'OBMTPIB','*','TPV');

SELECT * FROM SCS_AUTONO_FLOWNO WHERE RULE_CODE='SMRV_NO' AND CMP='OBMTPIB'

INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('RV_NO', '預約批次號', '20160101', 'OBMTPIB', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('SMRV_NO', 'OBMTPIB', '*', 'TPV', '2016-01-01 14:35', ';OBMTPIB;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('SMRV_NO',1,'20160101','CMP','公司',2,'OBMTPIB','OBMTPIB','*','TPV'),
('SMRV_NO',2,'20160101','CONST','常量',2,'B','OBMTPIB','*','TPV'),
('SMRV_NO',3,'20160101','YY','年度',2,'YY','OBMTPIB','*','TPV'),
('SMRV_NO',4,'20160101','MM','月份',2,'MM','OBMTPIB','*','TPV'),
('SMRV_NO',5,'20160101','NO','流水號',6,NULL,'OBMTPIB','*','TPV');


SELECT * FROM SCS_AUTONO_RULE WHERE RULE_CODE='SMRV_NO' AND CMP='OBMTPIB'

INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('SMRV_NO', '預約批次號', '20160101', 'OBMTPIB', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('ORD_NO', 'OBMTPIB', '*', 'TPV', '2016-01-01 14:35', ';OBMTPIB;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('ORD_NO',1,'20160101','CMP','公司',2,'OBMTPIB','OBMTPIB','*','TPV'),
('ORD_NO',2,'20160101','CONST','常量',2,'O','OBMTPIB','*','TPV'),
('ORD_NO',3,'20160101','YY','年度',2,'YY','OBMTPIB','*','TPV'),
('ORD_NO',4,'20160101','MM','月份',2,'MM','OBMTPIB','*','TPV'),
('ORD_NO',5,'20160101','NO','流水號',6,NULL,'OBMTPIB','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('ORD_NO', 'Transport Order No.', '20160101', 'OBMTPIB', '*', 'TPV')


INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('SHIB_NO', 'OBMTPIB', '*', 'TPV', '2016-01-01 14:35', ';OBMTPIB;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('SHIB_NO',1,'20160101','CMP','公司',3,'OBMTPIB','OBMTPIB','*','TPV'),
('SHIB_NO',2,'20160101','YY','常量',2,'YY','OBMTPIB','*','TPV'),
('SHIB_NO',3,'20160101','MM','年度',2,'MM','OBMTPIB','*','TPV'),
('SHIB_NO',4,'20160101','DD','Day',2,'DD','OBMTPIB','*','TPV'),
('SHIB_NO',5,'20160101','NO','流水號',6,NULL,'OBMTPIB','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('SHIB_NO', 'Inbound Shipment ID', '20160101', 'OBMTPIB', '*', 'TPV')
 

--??????
INSERT INTO [dbo].[BSCNTY]
           ([GROUP_ID]
           ,[CNTRY_CD]
           ,[CNTRY_NM]
           ,[CMP]
           ,[STN]
           ,[CREATE_BY]
           ,[CREATE_DATE]
           ,[MODIFY_BY]
           ,[MODIFY_DATE]
           ,[DEP]
           ,[SHIPPING_INSTRUCTION])
SELECT [GROUP_ID]
           ,[CNTRY_CD]
           ,[CNTRY_NM]
           ,'OBMTPIB'
           ,[STN]
           ,[CREATE_BY]
           ,[CREATE_DATE]
           ,[MODIFY_BY]
           ,[MODIFY_DATE]
           ,[DEP]
           ,[SHIPPING_INSTRUCTION]
  FROM  [dbo].[BSCNTY] WHERE CMP ='TP' 
   
  --??MANA10???????????????
  --  UPDATE SYS_ACCT_ROLE SET STN='*' WHERE CMP='OBMTPIB' AND FROLE_ID='SDADMIN'

  select * from SYS_ACCT where U_ID='MANA10' AND CMP='OBMTPIB'

  

INSERT INTO SMPTY([U_ID]
      ,[GROUP_ID]
      ,[CMP]
      ,[STN]
      ,[DEP]
      ,[PARTY_NO]
      ,[PARTY_TYPE]
      ,[STATUS]
      ,[ABBR]
      ,[HEAD_OFFICE]
      ,[PARTY_NAME]
      ,[PART_ADDR1]
      ,[PART_ADDR2]
      ,[PART_ADDR3]
      ,[CNTY]
      ,[CNTY_NM]
      ,[CITY]
      ,[CITY_NM]
      ,[STATE]
      ,[ZIP]
      ,[PARTY_ATTN]
      ,[PARTY_TEL]
      ,[PARTY_MAIL]
      ,[TAX_NO]
      ,[BILL_TO]
      ,[REMARK]
      ,[DUE_DAY]
      ,[CREATE_BY]
      ,[CREATE_DATE]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
      ,[PARTY_FAX]
      ,[PIC]
      ,[PARTY_NAME2]
      ,[PARTY_NAME3]
      ,[PARTY_NAME4]
      ,[PART_ADDR4]
      ,[PART_ADDR5]
      ,[IM_RECORD]
      ,[EDI_CUST_NM]
      ,[BANK_NM]
      ,[BANK_ACCOUNT]
      ,[IDENT]
      ,[BOND_TYPE]
      ,[BOND_ACT]
      ,[SOURCE_CODE]
      ,[IS_DFL]
      ,[FILTER]
      ,[Regon]
      ,[GLN]
      ,[SAP_ID]
      ,[KRS]
      ,[TRANSACTION_TYPE]
      ,[SCAC]
      ,[SHIPTO_FAC_ID]
      ,[VENDOR_ID]
      ,[VENDOR_LOC]
      ,[CONTRACT]
      ,[PAY_TERM]
      ,[PAY_METHOD]
      ,[MOBILE_TEL]
      ,[TRADING_PARTER]
      ,[IS_OUTBOUND]
      ,[PICKUP_POINT]
      ,[PICKUP_NAME])

SELECT REPLACE(CONVERT(NVARCHAR(64),NEWID()),'-','')
      ,[GROUP_ID]
      ,'OBMTPIB'
      ,[STN]
      ,[DEP]
      ,'OBMTPIB'
      ,[PARTY_TYPE]
      ,[STATUS]
      ,[ABBR]
      ,[HEAD_OFFICE]
      ,[PARTY_NAME]
      ,[PART_ADDR1]
      ,[PART_ADDR2]
      ,[PART_ADDR3]
      ,[CNTY]
      ,[CNTY_NM]
      ,[CITY]
      ,[CITY_NM]
      ,[STATE]
      ,[ZIP]
      ,[PARTY_ATTN]
      ,[PARTY_TEL]
      ,[PARTY_MAIL]
      ,[TAX_NO]
      ,[BILL_TO]
      ,[REMARK]
      ,[DUE_DAY]
      ,[CREATE_BY]
      ,[CREATE_DATE]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
      ,[PARTY_FAX]
      ,[PIC]
      ,[PARTY_NAME2]
      ,[PARTY_NAME3]
      ,[PARTY_NAME4]
      ,[PART_ADDR4]
      ,[PART_ADDR5]
      ,[IM_RECORD]
      ,[EDI_CUST_NM]
      ,[BANK_NM]
      ,[BANK_ACCOUNT]
      ,[IDENT]
      ,[BOND_TYPE]
      ,[BOND_ACT]
      ,[SOURCE_CODE]
      ,[IS_DFL]
      ,[FILTER]
      ,[Regon]
      ,[GLN]
      ,[SAP_ID]
      ,[KRS]
      ,[TRANSACTION_TYPE]
      ,[SCAC]
      ,[SHIPTO_FAC_ID]
      ,[VENDOR_ID]
      ,[VENDOR_LOC]
      ,[CONTRACT]
      ,[PAY_TERM]
      ,[PAY_METHOD]
      ,[MOBILE_TEL]
      ,[TRADING_PARTER]
      ,[IS_OUTBOUND]
      ,[PICKUP_POINT]
      ,[PICKUP_NAME]
  FROM [dbo].[SMPTY] WHERE PARTY_NO='TP' AND STATUS='U'








