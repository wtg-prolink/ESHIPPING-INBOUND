/*****���ӽ�ɫ***********/
 INSERT INTO [dbo].[SYS_ROLE]
           ([FID]
           ,[GROUP_ID]
           ,[CMP]
           ,[STN]
           ,[FDESCP]
           ,[SUB_ROLES])
SELECT [FID]
      ,[GROUP_ID]
      ,'CNI'
      ,[STN]
      ,[FDESCP]
      ,[SUB_ROLES]
  FROM [dbo].[SYS_ROLE]
 where FID in ('SDADMIN','ADMINISTRATOR' )and cmp='PL' 

/*****���ӽ�ɫȨ��***********/
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
      ,'CNI'
      ,[STN]
      ,[FPMLIST]
  FROM [dbo].[SYS_ROLE_OBJ_PMS] WHERE FROLE_ID in ('SDADMIN','ADMINISTRATOR') and cmp='PL' 

  /*****��ɫȨ��***************/
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
      ,'CNI'
      ,[STN]
      ,[FDESCP]
      ,'CNI'
      ,[RSTN]
  FROM [ESHIPPING].[dbo].[SYS_ACCT_ROLE] WHERE CMP = 'PL' and [FACCT_ID] in( 'ADMIN','MANA10')

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
      ,'CNI'
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
  FROM [dbo].[SYS_ACCT] WHERE CMP = 'PL' AND U_ID IN ('MANA10','ADMIN')


/**********���뽨������*****************/
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
      ,'CNI'
      ,[STN]
      ,[CREATE_BY]
      ,[CREATE_DATE]
      ,[MODIFY_BY]
      ,[DEP]
      ,[MODIFY_DATE]
  FROM [ESHIPPING].[dbo].[BSCODE_KIND] WHERE CMP = 'PL'


/**********���뽨��ϸ��*****************/
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
      ,'CNI'
      ,[STN]
      ,[CREATE_BY],[CREATE_DATE]
      ,[MODIFY_BY],[AMS_CODE]
      ,[INTTRA],[AR_CD],[AP_CD]
      ,[DEP]
      ,[APPLY]
      ,[LOCATION]
      ,[ORDER_BY]
  FROM [dbo].[BSCODE] WHERE CMP = 'PL'

  --UPDATE [ESHIPPING].[dbo].[APPROVE_FLOW_M] SET
  --SITE_ID = '*'

/*****ǩ�����̺�ǩ��Ⱥ���趨*****************/
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
      ,'CNI'
      ,[SITE_ID]
      ,[APPROVE_STATUS]
      ,[CREATE_BY]
      ,[CREATE_DATE]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
      ,[AU_ID]
  FROM [ESHIPPING].[dbo].[APPROVE_FLOW_M] WHERE CMP_ID ='PL'

/***********��������**************/
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
      ,'CNI'
      ,[CNTRY_CD]
      ,[CNTRY_NM]
      ,[STATE_CD]
      ,[STATE_NM]
      ,[REGION_CD]
      ,[REGION_NM]
  FROM [ESHIPPING].[dbo].[BSSTATE] WHERE CMP = 'PL'

--����SMCC  �ɱ����Ľ���
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
      ,'CNI'
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
  FROM [dbo].[SMCC] WHERE CMP = 'PL'

/*********ǩ��Ⱥ���趨*****************************************************************�����ָ���*/

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
      ,'CNI'
      ,[STN]
      ,[APPROVE_ATTR]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
  FROM [dbo].[APPROVE_ATTRIBUTE] WHERE CMP = 'PL'

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
      ,'CNI'
      ,[STN]
      ,[APPROVE_ATTR]
      ,[GROUP_DESCP]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
  FROM [dbo].[APPROVE_ATTR_D] WHERE CMP = 'PL'


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
      ,'CNI'
      ,[STN]
      ,[USER_ID]
      ,[U_EMAIL],[BY_EMAIL]
      ,[BY_MSG],[BY_WECHAT]
      ,[BY_APP],[REMARK]
      ,[MODIFY_BY]
      ,[MODIFY_DATE]
  FROM [dbo].[APPROVE_ATTR_DP] WHERE CMP = 'PL'

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
      ,'CNI'
      ,[STN],[DEP],[PARTY_NO]
      ,[PARTY_NM]
  FROM [ESHIPPING].[dbo].[TKPDM] WHERE CMP = 'PL'


--�����趨��ɺ󣬼ǵ�ApproveGroup�Ǳ���Ҫ�趨Attribute���Ϊ�գ�����Ĳ�ѯ�ǲ鲻�����ϵ�


update APPROVE_ATTR_D set u_fid=(
SELECT U_ID FROM APPROVE_ATTRIBUTE WHERE CMP='CNI' and approve_attr='DN') where
 CMP='CNI' AND APPROVE_ATTR='DN'

 update APPROVE_ATTR_D set u_fid=(
SELECT U_ID FROM APPROVE_ATTRIBUTE WHERE CMP='CNI' and approve_attr='BILLING') where
 CMP='CNI' AND APPROVE_ATTR='BILLING'

--�ʼ���ʽ
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
      ,'CNI'
      ,[STN]
      ,[DEP]
           ,[SEQ]
           ,[MT_TYPE]
           ,[MT_NAME]
           ,[MT_CONTENT]
           ,[REMARK]
  FROM [ESHIPPING].[dbo].[TKPMT] WHERE CMP = 'PL';

  UPDATE TKPMT SET MT_CONTENT=replace( CAST(MT_CONTENT as varchar(8000)),' PL ',' CNI')  WHERE MT_CONTENT LIKE '% PL%' AND CMP = 'CNI'



INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('RQD_NO', 'CNI', '*', 'TPV', '2015-12-15 14:35', ';CNI;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('RQD_NO',1,'20151214','CMP','��˾',2,'CNI','CNI','*','TPV'),
('RQD_NO',2,'20151214','CONST','����',1,'A','CNI','*','TPV'),
('RQD_NO',3,'20151214','YY','���',2,'YY','CNI','*','TPV'),
('RQD_NO',4,'20151214','MM','�·�',2,'MM','CNI','*','TPV'),
('RQD_NO',5,'20151214','NO','��ˮ̖',6,NULL,'CNI','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('RQD_NO', 'ѯ���Զ����', '20151214', 'CNI', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('QUOT_NO', 'CNI', '*', 'TPV', '2016-01-01 14:35', ';CNI;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('QUOT_NO',1,'20160101','CMP','��˾',2,'CNI','CNI','*','TPV'),
('QUOT_NO',2,'20160101','CONST','����',1,'Q','CNI','*','TPV'),
('QUOT_NO',3,'20160101','YY','���',2,'YY','CNI','*','TPV'),
('QUOT_NO',4,'20160101','MM','�·�',2,'MM','CNI','*','TPV'),
('QUOT_NO',5,'20160101','NO','��ˮ̖',6,NULL,'CNI','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('QUOT_NO', 'ѯ���Զ����', '20160101', 'CNI', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('BAT_NO', 'CNI', '*', 'TPV', '2016-01-01 14:35', ';CNI;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('BAT_NO',1,'20160101','CMP','��˾',2,'CNI','CNI','*','TPV'),
('BAT_NO',2,'20160101','CONST','����',2,'LT','CNI','*','TPV'),
('BAT_NO',3,'20160101','YY','���',2,'YY','CNI','*','TPV'),
('BAT_NO',4,'20160101','MM','�·�',2,'MM','CNI','*','TPV'),
('BAT_NO',5,'20160101','NO','��ˮ̖',6,NULL,'CNI','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('BAT_NO', '�A�s����̖', '20160101', 'CNI', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('TK_NO', 'CNI', '*', 'TPV', '2016-01-01 14:35', ';CNI;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('TK_NO',1,'20160101','CMP','��˾',2,'CNI','CNI','*','TPV'),
('TK_NO',2,'20160101','CONST','����',2,'ET','CNI','*','TPV'),
('TK_NO',3,'20160101','YY','���',2,'YY','CNI','*','TPV'),
('TK_NO',4,'20160101','MM','�·�',2,'MM','CNI','*','TPV'),
('TK_NO',5,'20160101','NO','��ˮ̖',6,NULL,'CNI','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('TK_NO', '�A�s����̖', '20160101', 'CNI', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('RV_NO', 'CNI', '*', 'TPV', '2016-01-01 14:35', ';CNI;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('RV_NO',1,'20160101','CMP','��˾',2,'CNI','CNI','*','TPV'),
('RV_NO',2,'20160101','CONST','����',2,'R','CNI','*','TPV'),
('RV_NO',3,'20160101','YY','���',2,'YY','CNI','*','TPV'),
('RV_NO',4,'20160101','MM','�·�',2,'MM','CNI','*','TPV'),
('RV_NO',5,'20160101','NO','��ˮ̖',6,NULL,'CNI','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('RV_NO', '�A�s����̖', '20160101', 'CNI', '*', 'TPV')

INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('SMRV_NO', 'CNI', '*', 'TPV', '2016-01-01 14:35', ';CNI;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('SMRV_NO',1,'20160101','CMP','��˾',2,'CNI','CNI','*','TPV'),
('SMRV_NO',2,'20160101','CONST','����',2,'B','CNI','*','TPV'),
('SMRV_NO',3,'20160101','YY','���',2,'YY','CNI','*','TPV'),
('SMRV_NO',4,'20160101','MM','�·�',2,'MM','CNI','*','TPV'),
('SMRV_NO',5,'20160101','NO','��ˮ̖',6,NULL,'CNI','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('SMRV_NO', '�A�s����̖', '20160101', 'CNI', '*', 'TPV')


INSERT INTO SCS_AUTONO_FLOWNO (RULE_CODE, CMP, STN, GROUP_ID, UPDATE_DATE, FLOW_COND) VALUES('ORD_NO', 'CNI', '*', 'TPV', '2016-01-01 14:35', ';CNI;15;12' )
INSERT INTO SCS_AUTONO_ITEM (RULE_CODE, SEQ_NO, EFFECT_DATE, CODE, CODE_NAME, CODE_LEN, CODE_CONTENT, CMP, STN, GROUP_ID) VALUES
('ORD_NO',1,'20160101','CMP','��˾',3,'CNI','CNI','*','TPV'),
('ORD_NO',2,'20160101','CONST','����',2,'O','CNI','*','TPV'),
('ORD_NO',3,'20160101','YY','���',2,'YY','CNI','*','TPV'),
('ORD_NO',4,'20160101','MM','�·�',2,'MM','CNI','*','TPV'),
('ORD_NO',5,'20160101','NO','��ˮ̖',6,NULL,'CNI','*','TPV');
INSERT INTO SCS_AUTONO_RULE (RULE_CODE, RULE_NAME, EFFECT_DATE, CMP, STN, GROUP_ID) VALUES ('ORD_NO', 'Transport Order No.', '20160101', 'CNI', '*', 'TPV')



--�������뽨��
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
           ,'CNI'
           ,[STN]
           ,[CREATE_BY]
           ,[CREATE_DATE]
           ,[MODIFY_BY]
           ,[MODIFY_DATE]
           ,[DEP]
           ,[SHIPPING_INSTRUCTION]
  FROM [ESHIPPING].[dbo].[BSCNTY] WHERE CMP ='PL' 


  --���MANA10�Ƿ���ֵ����û�еĻ���ֱ���滻
  --  UPDATE SYS_ACCT_ROLE SET STN='*' WHERE CMP='CNI' AND FROLE_ID='SDADMIN'


--***********--�����ǵ�������Ȩ�޵ĺͽ�ɫ�Ĳ���********

DELETE FROM [SYS_ROLE] WHERE CMP='CNI'
DELETE FROM [SYS_ROLE_OBJ_PMS] WHERE CMP='CNI'

 INSERT INTO [dbo].[SYS_ROLE]
           ([FID]
           ,[GROUP_ID]
           ,[CMP]
           ,[STN]
           ,[FDESCP]
           ,[SUB_ROLES])
SELECT [FID]
      ,[GROUP_ID]
      ,'CNI'
      ,[STN]
      ,[FDESCP]
      ,[SUB_ROLES]
  FROM [dbo].[SYS_ROLE]
 where 1= 1and cmp='PL' 

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
      ,'CNI'
      ,[STN]
      ,[FPMLIST]
  FROM [dbo].[SYS_ROLE_OBJ_PMS] WHERE 1=1 and cmp='PL' 


