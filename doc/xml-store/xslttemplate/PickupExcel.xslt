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
          <LastSaved>2011-05-15T07:34:21Z</LastSaved>
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
          <Style ss:ID="s66">
            <Font ss:FontName="宋体" x:CharSet="134" ss:Size="12" ss:Color="#000000"
             ss:Bold="1"/>
            <Interior ss:Color="#DBE5F1" ss:Pattern="Solid"/>
          </Style>
          <Style ss:ID="s78">
            <Alignment ss:Horizontal="Center" ss:Vertical="Center"/>
            <Font ss:FontName="宋体" x:CharSet="134" ss:Size="14" ss:Color="#000000"
             ss:Bold="1"/>
          </Style>
          <Style ss:ID="s84">
            <Font ss:FontName="宋体" x:CharSet="134" ss:Size="11" ss:Color="#000000"/>
          </Style>
        </Styles>
        <Worksheet ss:Name="Sheet1">
          <Table ss:ExpandedColumnCount="18" x:FullColumns="1"
           x:FullRows="1" ss:DefaultColumnWidth="54" ss:DefaultRowHeight="13.5">
            <Column ss:AutoFitWidth="0" ss:Width="39.75"/>
            <Column ss:AutoFitWidth="0" ss:Width="99.75" ss:Span="1"/>
            <Column ss:Index="4" ss:AutoFitWidth="0" ss:Width="93.75"/>
            <Column ss:AutoFitWidth="0" ss:Width="213.75"/>
            <Column ss:AutoFitWidth="0" ss:Width="97.5"/>
            <Column ss:AutoFitWidth="0" ss:Width="81"/>
            <Column ss:Index="9" ss:AutoFitWidth="0" ss:Width="69.75"/>
            <Column ss:AutoFitWidth="0" ss:Width="39"/>
            <Column ss:AutoFitWidth="0" ss:Width="42.75"/>
            <Column ss:Index="13" ss:AutoFitWidth="0" ss:Width="63.75" ss:Span="1"/>
            <Column ss:Index="16" ss:AutoFitWidth="0" ss:Width="89.25"/>
            <Column ss:AutoFitWidth="0" ss:Width="56.25"/>
            <Column ss:AutoFitWidth="0" ss:Width="63"/>
            <Row ss:AutoFitHeight="0">
              <Cell ss:MergeAcross="17" ss:MergeDown="1" ss:StyleID="s78">
                <Data
      ss:Type="String">日通EDI取貨(Pickup轉出)</Data>
              </Cell>
            </Row>
            <Row ss:Index="3" ss:Height="14.25">
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">狀態</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">Tracking號碼</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">日通託運單號</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">客戶代碼</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">取件地址</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">取件連詻電話</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">取件聯絡人</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">備註</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">時間</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">件數</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">重量</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">材積</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">叫件日期</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">叫件時間</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">貨品名</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">代收帳款(Y/N)</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">代收幣別</Data>
              </Cell>
              <Cell ss:StyleID="s66">
                <Data ss:Type="String">代收金額</Data>
              </Cell>
            </Row>
            <xsl:for-each select="DocumentElement/PICKUP">
              <Row>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="STATUS" /></Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="TRACKING_NO" /></Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="THIRDPL_NO" /></Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="CUSTOM_NO" /></Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="PICKUP_ADDR" />
                </Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="PICKUP_TEL" /></Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="PICKUP_ATTN" />
                </Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="REMARK" /></Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="SYS_TIME" />
                </Data>
              </Cell>
              <Cell>
                <Data ss:Type="String">
                  <xsl:value-of select="QTY" />
                </Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="GW" /></Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="CBM" />
                </Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="CALL_DATE" />
                </Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="CALL_TIME" /></Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="GOODS" /></Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="ISPAYMENT" /></Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
                  <xsl:value-of select="CRNCY" /></Data>
              </Cell>
              <Cell ss:StyleID="s84">
                <Data ss:Type="String">
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
                <ActiveCol>5</ActiveCol>
              </Pane>
            </Panes>
            <ProtectObjects>False</ProtectObjects>
            <ProtectScenarios>False</ProtectScenarios>
          </WorksheetOptions>
        </Worksheet>
        <Worksheet ss:Name="Sheet2">
          <Table ss:ExpandedColumnCount="1" ss:ExpandedRowCount="1" x:FullColumns="1"
           x:FullRows="1" ss:DefaultColumnWidth="54" ss:DefaultRowHeight="13.5">
          </Table>
          <WorksheetOptions xmlns="urn:schemas-microsoft-com:office:excel">
            <PageSetup>
              <Header x:Margin="0.3"/>
              <Footer x:Margin="0.3"/>
              <PageMargins x:Bottom="0.75" x:Left="0.7" x:Right="0.7" x:Top="0.75"/>
            </PageSetup>
            <Print>
              <ValidPrinterInfo/>
              <PaperSizeIndex>9</PaperSizeIndex>
              <HorizontalResolution>200</HorizontalResolution>
              <VerticalResolution>200</VerticalResolution>
            </Print>
            <ProtectObjects>False</ProtectObjects>
            <ProtectScenarios>False</ProtectScenarios>
          </WorksheetOptions>
        </Worksheet>
        <Worksheet ss:Name="Sheet3">
          <Table ss:ExpandedColumnCount="1" ss:ExpandedRowCount="1" x:FullColumns="1"
           x:FullRows="1" ss:DefaultColumnWidth="54" ss:DefaultRowHeight="13.5">
          </Table>
          <WorksheetOptions xmlns="urn:schemas-microsoft-com:office:excel">
            <PageSetup>
              <Header x:Margin="0.3"/>
              <Footer x:Margin="0.3"/>
              <PageMargins x:Bottom="0.75" x:Left="0.7" x:Right="0.7" x:Top="0.75"/>
            </PageSetup>
            <Print>
              <ValidPrinterInfo/>
              <PaperSizeIndex>9</PaperSizeIndex>
              <HorizontalResolution>200</HorizontalResolution>
              <VerticalResolution>200</VerticalResolution>
            </Print>
            <ProtectObjects>False</ProtectObjects>
            <ProtectScenarios>False</ProtectScenarios>
          </WorksheetOptions>
        </Worksheet>
      </Workbook>

    </xsl:template>
</xsl:stylesheet>
