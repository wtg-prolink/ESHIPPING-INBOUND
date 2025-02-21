/*****增加角色***********/
 INSERT INTO [dbo].[SYS_ROLE]
           ([FID]
           ,[GROUP_ID]
           ,[CMP]
           ,[STN]
           ,[FDESCP]
           ,[SUB_ROLES])
SELECT [FID]
      ,[GROUP_ID]
      ,'TP'
      ,[STN]
      ,[FDESCP]
      ,[SUB_ROLES]
  FROM [dbo].[SYS_ROLE]
 where FID in ('SDADMIN','ADMINISTRATOR' )and cmp='FQ' 

/*****增加角色权限***********/
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
      ,'TP'
      ,[STN]
      ,[FPMLIST]
  FROM [ESHIPPING].[dbo].[SYS_ROLE_OBJ_PMS] WHERE FROLE_ID in ('SDADMIN','ADMINISTRATOR') and cmp='FQ' 

  /*****角色权限***************/
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
      ,'TP'
      ,[STN]
      ,[FDESCP]
      ,'TP'
      ,[RSTN]
  FROM [ESHIPPING].[dbo].[SYS_ACCT_ROLE] WHERE CMP = 'FQ' and [FACCT_ID] in( 'ADMIN','MANA10')

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
      ,'TP'
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
  FROM [dbo].[SYS_ACCT] WHERE CMP = 'FQ' AND U_ID IN ('MANA10','ADMIN')


/**********代码建档主档*****************/
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
      ,'TP'
      ,[STN]
      ,[CREATE_BY]
      ,[CREATE_DATE]
      ,[MODIFY_BY]
      ,[DEP]
      ,[MODIFY_DATE]
  FROM [ESHIPPING].[dbo].[BSCODE_KIND] WHERE CMP = 'FQ'


/**********代码建档细档*****************/
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
      ,'TP'
      ,[STN]
      ,[CREATE_BY],[CREATE_DATE]
      ,[MODIFY_BY],[AMS_CODE]
      ,[INTTRA],[AR_CD],[AP_CD]
      ,[DEP]
      ,[APPLY]
      ,[LOCATION]
      ,[ORDER_BY]
  FROM [dbo].[BSCODE] WHERE CMP = 'FQ'

  --UPDATE [ESHIPPING].[dbo].[APPROVE_FLOW_M] SET
  --SITE_ID = '*'

/*****签核流程和签核群组设定*****************/
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
      ,'TP'
      ,[SITE_ID]
      ,[APPROVE_STATUS]
      ,[CREATE_BY]
      ,[CREATE_DATE]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
      ,[AU_ID]
  FROM [ESHIPPING].[dbo].[APPROVE_FLOW_M] WHERE CMP_ID ='FQ'

/***********区域设置**************/
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
      ,'TP'
      ,[CNTRY_CD]
      ,[CNTRY_NM]
      ,[STATE_CD]
      ,[STATE_NM]
      ,[REGION_CD]
      ,[REGION_NM]
  FROM [ESHIPPING].[dbo].[BSSTATE] WHERE CMP = 'FQ'

--新增SMCC  成本中心建档
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
      ,'TP'
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
  FROM [dbo].[SMCC] WHERE CMP = 'FQ'

/*********签核群组设定*****************************************************************华丽分割线*/

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
      ,'TP'
      ,[STN]
      ,[APPROVE_ATTR]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
  FROM [dbo].[APPROVE_ATTRIBUTE] WHERE CMP = 'FQ'

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
      ,'TP'
      ,[STN]
      ,[APPROVE_ATTR]
      ,[GROUP_DESCP]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
  FROM [dbo].[APPROVE_ATTR_D] WHERE CMP = 'FQ'


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
      ,'TP'
      ,[STN]
      ,[USER_ID]
      ,[U_EMAIL],[BY_EMAIL]
      ,[BY_MSG],[BY_WECHAT]
      ,[BY_APP],[REMARK]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
  FROM [dbo].[APPROVE_ATTR_DP] WHERE CMP = 'FQ'

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
      ,'TP'
      ,[STN],[DEP],[PARTY_NO]
      ,[PARTY_NM]
  FROM [ESHIPPING].[dbo].[TKPDM] WHERE CMP = 'FQ'


--以上设定完成后，记得ApproveGroup那边需要设定Attribute如果为空，后面的查询是查不出资料的

update APPROVE_ATTR_D set u_fid=(
SELECT U_ID FROM APPROVE_ATTRIBUTE WHERE CMP='TP' and approve_attr='DN') where
 CMP='TP' and u_fid='0F3C2DFC-9286-474B-80A4-204C0E155016'

 update APPROVE_ATTR_D set u_fid=(
SELECT U_ID FROM APPROVE_ATTRIBUTE WHERE CMP='TP' and approve_attr='BILLING') where
 CMP='TP' and u_fid='7a975e0e-a1cb-4630-8604-89e47b8ca9e5'

--邮件格式
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
      ,'TP'
      ,[STN]
      ,[DEP]
           ,[SEQ]
           ,[MT_TYPE]
           ,[MT_NAME]
           ,[MT_CONTENT]
           ,[REMARK]
  FROM [ESHIPPING].[dbo].[TKPMT] WHERE CMP = 'FQ';

  UPDATE TKPMT SET MT_CONTENT=replace( CAST(MT_CONTENT as varchar(8000)),' FQ ',' BJ ')  WHERE MT_CONTENT LIKE '% FQ %' AND CMP = 'TP'


INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('RQD_NO', 'TP', '*', 'TPV', '2015-12-15 14:35', ';BJ;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('RQD_NO',1,'20151214','CMP','公司',2,'TP','TP','*','TPV'),
('RQD_NO',2,'20151214','CONST','常量',1,'A','TP','*','TPV'),
('RQD_NO',3,'20151214','YY','年度',2,'YY','TP','*','TPV'),
('RQD_NO',4,'20151214','MM','月份',2,'MM','TP','*','TPV'),
('RQD_NO',5,'20151214','NO','流水號',6,NULL,'TP','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('RQD_NO', '询价自动编号', '20151214', 'TP', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('QUOT_NO', 'TP', '*', 'TPV', '2016-01-01 14:35', ';BJ;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('QUOT_NO',1,'20160101','CMP','公司',2,'TP','TP','*','TPV'),
('QUOT_NO',2,'20160101','CONST','常量',1,'Q','TP','*','TPV'),
('QUOT_NO',3,'20160101','YY','年度',2,'YY','TP','*','TPV'),
('QUOT_NO',4,'20160101','MM','月份',2,'MM','TP','*','TPV'),
('QUOT_NO',5,'20160101','NO','流水號',6,NULL,'TP','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('QUOT_NO', '询价自动编号', '20160101', 'TP', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('BAT_NO', 'TP', '*', 'TPV', '2016-01-01 14:35', ';BJ;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('BAT_NO',1,'20160101','CMP','公司',2,'TP','TP','*','TPV'),
('BAT_NO',2,'20160101','CONST','常量',2,'LT','TP','*','TPV'),
('BAT_NO',3,'20160101','YY','年度',2,'YY','TP','*','TPV'),
('BAT_NO',4,'20160101','MM','月份',2,'MM','TP','*','TPV'),
('BAT_NO',5,'20160101','NO','流水號',6,NULL,'TP','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('BAT_NO', '預約批次號', '20160101', 'TP', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('TK_NO', 'TP', '*', 'TPV', '2016-01-01 14:35', ';BJ;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('TK_NO',1,'20160101','CMP','公司',2,'TP','TP','*','TPV'),
('TK_NO',2,'20160101','CONST','常量',2,'ET','TP','*','TPV'),
('TK_NO',3,'20160101','YY','年度',2,'YY','TP','*','TPV'),
('TK_NO',4,'20160101','MM','月份',2,'MM','TP','*','TPV'),
('TK_NO',5,'20160101','NO','流水號',6,NULL,'TP','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('TK_NO', '預約批次號', '20160101', 'TP', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('RV_NO', 'TP', '*', 'TPV', '2016-01-01 14:35', ';BJ;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('RV_NO',1,'20160101','CMP','公司',2,'TP','TP','*','TPV'),
('RV_NO',2,'20160101','CONST','常量',2,'R','TP','*','TPV'),
('RV_NO',3,'20160101','YY','年度',2,'YY','TP','*','TPV'),
('RV_NO',4,'20160101','MM','月份',2,'MM','TP','*','TPV'),
('RV_NO',5,'20160101','NO','流水號',6,NULL,'TP','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('RV_NO', '預約批次號', '20160101', 'TP', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('SMRV_NO', 'TP', '*', 'TPV', '2016-01-01 14:35', ';BJ;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('SMRV_NO',1,'20160101','CMP','公司',2,'TP','TP','*','TPV'),
('SMRV_NO',2,'20160101','CONST','常量',2,'B','TP','*','TPV'),
('SMRV_NO',3,'20160101','YY','年度',2,'YY','TP','*','TPV'),
('SMRV_NO',4,'20160101','MM','月份',2,'MM','TP','*','TPV'),
('SMRV_NO',5,'20160101','NO','流水號',6,NULL,'TP','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('SMRV_NO', '預約批次號', '20160101', 'TP', '*', 'TPV')

--国建代码建档
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
           ,'TP'
           ,[STN]
           ,[CREATE_BY]
           ,[CREATE_DATE]
           ,[MODIFY_BY]
           ,[MODIFY_DATE]
           ,[DEP]
           ,[SHIPPING_INSTRUCTION]
  FROM [ESHIPPING].[dbo].[BSCNTY] WHERE CMP ='FQ' 


  --检查MANA10是否有值，若没有的话就直接替换
  --  UPDATE SYS_ACCT_ROLE SET STN='*' WHERE CMP='TP' AND FROLE_ID='SDADMIN'