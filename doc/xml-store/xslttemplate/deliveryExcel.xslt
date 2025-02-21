<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>

    <xsl:template match="/">

      <?mso-application progid="Excel.Sheet"?>
      <Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet"
       xmlns:o="urn:schemas-microsoft-com:office:office"
       xmlns:x="urn:schemas-microsoft-com:office:excel"
       xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
       xmlns:html="http://www.w3.org/TR/REC-html40">
        <DocumentProperties xmlns="urn:schemas-microsoft-com:office:office">
          <Created>2006-09-13T11:21:51Z</Created>
          <LastSaved>2011-05-19T07:02:05Z</LastSaved>
          <Version>12.00</Version>
        </DocumentProperties>
        <OfficeDocumentSettings xmlns="urn:schemas-microsoft-com:office:office">
          <RemovePersonalInformation/>
        </OfficeDocumentSettings>
        <ExcelWorkbook xmlns="urn:schemas-microsoft-com:office:excel">
          <WindowHeight>11640</WindowHeight>
          <WindowWidth>19200</WindowWidth>
          <WindowTopX>0</WindowTopX>
          <WindowTopY>90</WindowTopY>
          <ProtectStructure>False</ProtectStructure>
          <ProtectWindows>False</ProtectWindows>
        </ExcelWorkbook>
        <Styles>
          <Style ss:ID="Default" ss:Name="Normal">
            <Alignment ss:Vertical="Center"/>
            <Borders/>
            <Font ss:FontName="宋体" x:CharSet="134" ss:Size="11" ss:Color="#000000"/>
            <Interior/>
            <NumberFormat/>
            <Protection/>
          </Style>
          <Style ss:ID="s39" ss:Name="20% - 强调文字颜色 1">
            <Font ss:FontName="宋体" x:CharSet="134" ss:Size="11" ss:Color="#000000"/>
            <Interior ss:Color="#DBE5F1" ss:Pattern="Solid"/>
          </Style>
          <Style ss:ID="s66">
            <Interior ss:Color="#8DB4E3" ss:Pattern="Solid"/>
          </Style>
          <Style ss:ID="s68">
            <Interior ss:Color="#F2F2F2" ss:Pattern="Solid"/>
          </Style>
          <Style ss:ID="s69" ss:Parent="s39">
            <Alignment ss:Horizontal="Center" ss:Vertical="Center"/>
            <Font ss:FontName="宋体" x:CharSet="134" ss:Size="16" ss:Color="#1F497D"
             ss:Bold="1"/>
          </Style>
        </Styles>
        <Worksheet ss:Name="Sheet1">
          <Table ss:ExpandedColumnCount="22" x:FullColumns="1"
           x:FullRows="1" ss:DefaultColumnWidth="54" ss:DefaultRowHeight="13.5">
            <Column ss:AutoFitWidth="0" ss:Width="30.5"/>
            <Column ss:AutoFitWidth="0" ss:Width="104.25"/>
            <Column ss:AutoFitWidth="0" ss:Width="98.25"/>
            <Column ss:AutoFitWidth="0" ss:Width="92.25"/>
            <Column ss:AutoFitWidth="0" ss:Width="97.5"/>
            <Column ss:AutoFitWidth="0" ss:Width="96"/>
            <Column ss:Index="8" ss:AutoFitWidth="0" ss:Width="100"/>
            <Column ss:AutoFitWidth="0" ss:Width="180"/>
            <Column ss:AutoFitWidth="0" ss:Width="120"/>
            <Column ss:AutoFitWidth="0" ss:Width="66"/>
            <Column ss:AutoFitWidth="0" ss:Width="69"/>
            <Column ss:AutoFitWidth="0" ss:Width="66"/>
            <Column ss:AutoFitWidth="0" ss:Width="63"/>
            <Column ss:AutoFitWidth="0" ss:Width="67.5"/>
            <Column ss:Index="17" ss:AutoFitWidth="0" ss:Width="54.75"/>
            <Column ss:Index="19" ss:AutoFitWidth="0" ss:Width="60.75"/>
            <Column ss:AutoFitWidth="0" ss:Width="104.25"/>
            <Column ss:AutoFitWidth="0" ss:Width="60.75"/>
            <Column ss:AutoFitWidth="0" ss:Width="63"/>
            <Row ss:AutoFitHeight="0">
              <Cell ss:MergeAcross="21" ss:MergeDown="1" ss:StyleID="s69">
                <Data
      ss:Type="String">日通EDI送貨（delivery轉出）</Data>
              </Cell>
            </Row>
            <Row ss:AutoFitHeight="0"/>
            <Row ss:AutoFitHeight="0" ss:StyleID="s66">
              <Cell>
                <Data ss:Type="String">狀態</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">Tracking號碼</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">主提單號碼</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">分提單號碼</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">船名航次/班機</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">開船/班機時間</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">國別</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">寄件人</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">送件地址</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">送件連詻電話</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">送件聯絡人</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">送件日期</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">送件時間</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">通知日期</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">通知時間 </Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">總件數</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">總重量</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">總材積</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">貨品名</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">是否代收帳款(Y/N)</Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">代收幣別 </Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">代收金額 </Data>
              </Cell>
            </Row>

            <xsl:for-each select="DocumentElement/DELIBERY">
              <Row ss:AutoFitHeight="0">
                <xsl:choose>
                  <xsl:when test="position() mod 2 = 1">
                    <xsl:attribute name="ss:StyleID">s68</xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                  </xsl:otherwise>
                </xsl:choose>
                <Cell>
                  <Data ss:Type="Number">
                    <xsl:value-of select="STATUS" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="PCK_ID" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="MAWB_NO" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="AWB_NO" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="FLIGHT_NO" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="ETD_TIME" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="CN" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="SHPR_NAME" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="CNEE_ADDR" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="CNEE_TEL" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="CNEE_ATTN" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="SEND_DATE" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="SEND_TIME" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="SYS_DATE" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="STS_TIME" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="Number">
                    <xsl:value-of select="PKG" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="Number">
                    <xsl:value-of select="GW" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="Number">
                    <xsl:value-of select="CBM" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="CMDTY_NAME" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="String">
                    <xsl:value-of select="ISPAYMENTCRNCY" />
                  </Data>
                </Cell>
                <Cell>
                  <Data ss:Type="Number">
                    <xsl:value-of select="AMOUNT" />
                  </Data>
                </Cell>
              </Row>
            </xsl:for-each>
            
          </Table>
          <WorksheetOptions xmlns="urn:schemas-microsoft-com:office:excel">
            <PageSetup>
              <Header x:Margin="0.3"/>
              <Footer x:Margin="0.3"/>
              <PageMargins x:Bottom="0.75" x:Left="0.7" x:Right="0.7" x:Top="0.75"/>
            </PageSetup>
            <Unsynced/>
            <Print>
              <ValidPrinterInfo/>
              <PaperSizeIndex>9</PaperSizeIndex>
              <HorizontalResolution>200</HorizontalResolution>
              <VerticalResolution>200</VerticalResolution>
            </Print>
            <Selected/>
            <Panes>
              <Pane>
                <Number>3</Number>
                <ActiveRow>4</ActiveRow>
                <ActiveCol>1</ActiveCol>
              </Pane>
            </Panes>
            <ProtectObjects>False</ProtectObjects>
            <ProtectScenarios>False</ProtectScenarios>
          </WorksheetOptions>
        </Worksheet>
      </Workbook>

    </xsl:template>
</xsl:stylesheet>
