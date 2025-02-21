 

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
      ,'AVAIB'
      ,[STN]
      ,[FDESCP]
      ,[SUB_ROLES]
  FROM [dbo].[SYS_ROLE]
 where FID in ('SDADMIN','ADMINISTRATOR' )and cmp='MN' 

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
      ,'AVAIB'
      ,[STN]
      ,[FPMLIST]
  FROM [dbo].[SYS_ROLE_OBJ_PMS] WHERE FROLE_ID in ('SDADMIN','ADMINISTRATOR') and cmp='MN' 

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
      ,'AVAIB'
      ,[STN]
      ,[FDESCP]
      ,'AVAIB'
      ,[RSTN]
  FROM  [dbo].[SYS_ACCT_ROLE] WHERE CMP = 'MN' and [FACCT_ID] in( 'ADMIN','MANA10')

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
      ,'AVAIB'
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
  FROM [dbo].[SYS_ACCT] WHERE CMP = 'MN' AND U_ID IN ('MANA10','ADMIN')


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
      ,'AVAIB'
      ,[STN]
      ,[CREATE_BY]
      ,[CREATE_DATE]
      ,[MODIFY_BY]
      ,[DEP]
      ,[MODIFY_DATE]
  FROM  [dbo].[BSCODE_KIND] WHERE CMP = 'MN'

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
      ,'AVAIB'
      ,[STN]
      ,[CREATE_BY],[CREATE_DATE]
      ,[MODIFY_BY],[AMS_CODE]
      ,[INTTRA],[AR_CD],[AP_CD]
      ,[DEP]
      ,[APPLY]
      ,[LOCATION]
      ,[ORDER_BY]
  FROM [dbo].[BSCODE] WHERE CMP = 'MN'

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
      ,'AVAIB'
      ,[SITE_ID]
      ,[APPROVE_STATUS]
      ,[CREATE_BY]
      ,[CREATE_DATE]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
      ,[AU_ID]
  FROM [dbo].[APPROVE_FLOW_M] WHERE CMP_ID ='MN'

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
      ,'AVAIB'
      ,[CNTRY_CD]
      ,[CNTRY_NM]
      ,[STATE_CD]
      ,[STATE_NM]
      ,[REGION_CD]
      ,[REGION_NM]
  FROM [dbo].[BSSTATE] WHERE CMP = 'MN'
  
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
      ,'AVAIB'
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
  FROM [dbo].[SMCC] WHERE CMP = 'MN'

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
      ,'AVAIB'
      ,[STN]
      ,[APPROVE_ATTR]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
  FROM [dbo].[APPROVE_ATTRIBUTE] WHERE CMP = 'MN'

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
      ,'AVAIB'
      ,[STN]
      ,[APPROVE_ATTR]
      ,[GROUP_DESCP]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
  FROM [dbo].[APPROVE_ATTR_D] WHERE CMP = 'MN'


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
      ,'AVAIB'
      ,[STN]
      ,[USER_ID]
      ,[U_EMAIL],[BY_EMAIL]
      ,[BY_MSG],[BY_WECHAT]
      ,[BY_APP],[REMARK]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
  FROM [dbo].[APPROVE_ATTR_DP] WHERE CMP = 'MN'

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
      ,'AVAIB'
      ,[STN],[DEP],[PARTY_NO]
      ,[PARTY_NM]
  FROM  [dbo].[TKPDM] WHERE CMP = 'MN'


--??????????ApproveGroup??????Attribute?????????????????

update APPROVE_ATTR_D set u_fid=(
SELECT U_ID FROM APPROVE_ATTRIBUTE WHERE CMP='AVAIB' and approve_attr='DN') where
 CMP='AVAIB' AND APPROVE_ATTR='DN'

 update APPROVE_ATTR_D set u_fid=(
SELECT U_ID FROM APPROVE_ATTRIBUTE WHERE CMP='AVAIB' and approve_attr='BILLING') where
 CMP='AVAIB' AND APPROVE_ATTR='BILLING'

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
      ,'AVAIB'
      ,[STN]
      ,[DEP]
           ,[SEQ]
           ,[MT_TYPE]
           ,[MT_NAME]
           ,[MT_CONTENT]
           ,[REMARK]
  FROM [dbo].[TKPMT] WHERE CMP = 'MN';

  UPDATE TKPMT SET MT_CONTENT=replace( CAST(MT_CONTENT as varchar(8000)),'MN','AVAIB')  WHERE MT_CONTENT LIKE '%MN%' AND CMP = 'AVAIB'



INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('RQD_NO', 'AVAIB', '*', 'TPV', '2015-12-15 14:35', ';AVAIB;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('RQD_NO',1,'20151214','CMP','??',2,'AVAIB','AVAIB','*','TPV'),
('RQD_NO',2,'20151214','CONST','??',1,'A','AVAIB','*','TPV'),
('RQD_NO',3,'20151214','YY','??',2,'YY','AVAIB','*','TPV'),
('RQD_NO',4,'20151214','MM','??',2,'MM','AVAIB','*','TPV'),
('RQD_NO',5,'20151214','NO','???',6,NULL,'AVAIB','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('RQD_NO', '??????', '20151214', 'AVAIB', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('QUOT_NO', 'AVAIB', '*', 'TPV', '2016-01-01 14:35', ';AVAIB;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('QUOT_NO',1,'20160101','CMP','??',2,'AVAIB','AVAIB','*','TPV'),
('QUOT_NO',2,'20160101','CONST','??',1,'Q','AVAIB','*','TPV'),
('QUOT_NO',3,'20160101','YY','??',2,'YY','AVAIB','*','TPV'),
('QUOT_NO',4,'20160101','MM','??',2,'MM','AVAIB','*','TPV'),
('QUOT_NO',5,'20160101','NO','???',6,NULL,'AVAIB','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('QUOT_NO', '??????', '20160101', 'AVAIB', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('BAT_NO', 'AVAIB', '*', 'TPV', '2016-01-01 14:35', ';AVAIB;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('BAT_NO',1,'20160101','CMP','??',2,'AVAIB','AVAIB','*','TPV'),
('BAT_NO',2,'20160101','CONST','??',2,'LT','AVAIB','*','TPV'),
('BAT_NO',3,'20160101','YY','??',2,'YY','AVAIB','*','TPV'),
('BAT_NO',4,'20160101','MM','??',2,'MM','AVAIB','*','TPV'),
('BAT_NO',5,'20160101','NO','???',6,NULL,'AVAIB','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('BAT_NO', '?????', '20160101', 'AVAIB', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('TK_NO', 'AVAIB', '*', 'TPV', '2016-01-01 14:35', ';AVAIB;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('TK_NO',1,'20160101','CMP','??',2,'AVAIB','AVAIB','*','TPV'),
('TK_NO',2,'20160101','CONST','??',2,'ET','AVAIB','*','TPV'),
('TK_NO',3,'20160101','YY','??',2,'YY','AVAIB','*','TPV'),
('TK_NO',4,'20160101','MM','??',2,'MM','AVAIB','*','TPV'),
('TK_NO',5,'20160101','NO','???',6,NULL,'AVAIB','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('TK_NO', '?????', '20160101', 'AVAIB', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('RV_NO', 'AVAIB', '*', 'TPV', '2016-01-01 14:35', ';AVAIB;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('RV_NO',1,'20160101','CMP','??',2,'AVAIB','AVAIB','*','TPV'),
('RV_NO',2,'20160101','CONST','??',2,'R','AVAIB','*','TPV'),
('RV_NO',3,'20160101','YY','??',2,'YY','AVAIB','*','TPV'),
('RV_NO',4,'20160101','MM','??',2,'MM','AVAIB','*','TPV'),
('RV_NO',5,'20160101','NO','???',6,NULL,'AVAIB','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('RV_NO', '?????', '20160101', 'AVAIB', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('SMRV_NO', 'AVAIB', '*', 'TPV', '2016-01-01 14:35', ';AVAIB;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('SMRV_NO',1,'20160101','CMP','??',2,'AVAIB','AVAIB','*','TPV'),
('SMRV_NO',2,'20160101','CONST','??',2,'B','AVAIB','*','TPV'),
('SMRV_NO',3,'20160101','YY','??',2,'YY','AVAIB','*','TPV'),
('SMRV_NO',4,'20160101','MM','??',2,'MM','AVAIB','*','TPV'),
('SMRV_NO',5,'20160101','NO','???',6,NULL,'AVAIB','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('SMRV_NO', '?????', '20160101', 'AVAIB', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('ORD_NO', 'AVAIB', '*', 'TPV', '2016-01-01 14:35', ';AVAIB;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('ORD_NO',1,'20160101','CMP','??',2,'AVAIB','AVAIB','*','TPV'),
('ORD_NO',2,'20160101','CONST','??',2,'O','AVAIB','*','TPV'),
('ORD_NO',3,'20160101','YY','??',2,'YY','AVAIB','*','TPV'),
('ORD_NO',4,'20160101','MM','??',2,'MM','AVAIB','*','TPV'),
('ORD_NO',5,'20160101','NO','???',6,NULL,'AVAIB','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('ORD_NO', 'Transport Order No.', '20160101', 'AVAIB', '*', 'TPV')


INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('SHIB_NO', 'AVAIB', '*', 'TPV', '2016-01-01 14:35', ';AVAIB;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('SHIB_NO',1,'20160101','CMP','??',3,'AVAIB','AVAIB','*','TPV'),
('SHIB_NO',2,'20160101','YY','??',2,'YY','AVAIB','*','TPV'),
('SHIB_NO',3,'20160101','MM','??',2,'MM','AVAIB','*','TPV'),
('SHIB_NO',4,'20160101','DD','Day',2,'DD','AVAIB','*','TPV'),
('SHIB_NO',5,'20160101','NO','???',6,NULL,'AVAIB','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('SHIB_NO', 'Inbound Shipment ID', '20160101', 'AVAIB', '*', 'TPV')
 

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
           ,'AVAIB'
           ,[STN]
           ,[CREATE_BY]
           ,[CREATE_DATE]
           ,[MODIFY_BY]
           ,[MODIFY_DATE]
           ,[DEP]
           ,[SHIPPING_INSTRUCTION]
  FROM  [dbo].[BSCNTY] WHERE CMP ='MN' 
   
  --??MANA10???????????????
  --  UPDATE SYS_ACCT_ROLE SET STN='*' WHERE CMP='AVAIB' AND FROLE_ID='SDADMIN'