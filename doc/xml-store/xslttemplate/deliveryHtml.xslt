<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>

    <xsl:template match="/">

      <html>
        <body>
          <h3>日通EDI收貨(Delivery轉出)</h3>
          <hr />
          <div style="width:1300px; height:100%; overflow:scroll">
          <table cellpadding="1" cellspacing="0" width="100%" border="0">
            <tr bgcolor="#8DB4E3">
              <td width="80px">
                <b>Tracking號碼</b>
              </td>
              <td width="100px">
                <b>寄件人</b>
              </td>
              <td width="120px">
                <b>送件聯絡人</b>
              </td>
              <td width="200px">
                <b>送件地址</b>
              </td>
              <td width="150px">
                <b>送件聯絡電話</b>
              </td>
              <td width="80px">
                <b>通知日期</b>
              </td>
              <td width="80px">
                <b>通知時間</b>
              </td>
              <td width="60px">
                <b>總件數</b>
              </td>
              <td width="60px">
                <b>總重量</b>
              </td>
              <td width="60px">
                <b>總材積</b>
              </td>
              <td width="100px">
                <b>貨品名</b>
              </td>
            </tr>
            <xsl:for-each select="DocumentElement/DELIBERY">
              <tr>
                <xsl:choose>
                  <xsl:when test="position() mod 2 = 1">
                    <xsl:attribute name="style">background-color:#F2F2F2</xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                  </xsl:otherwise>
                </xsl:choose>
                <td>
                  <xsl:value-of select="AWB_NO" />
                </td>
                <td>
                  <xsl:value-of select="SHPR_NAME" />
                </td>
                <td>
                  <xsl:value-of select="CNEE_ATTN" />
                </td>
                <td>
                  <xsl:value-of select="CNEE_ADDR" />
                </td>
                <td>
                  <xsl:value-of select="CNEE_TEL" />
                </td>
                <td>
                  <xsl:value-of select="SYS_DATE" />
                </td>                
                <td>
                  <xsl:value-of select="STS_TIME" />
                </td>
                <td>
                  <xsl:value-of select="PKG" />
                </td>
                <td>
                  <xsl:value-of select="GW" />
                </td>
                <td>
                  <xsl:value-of select="CBM" />
                </td>
                <td>
                  <xsl:value-of select="CMDTY_NAME" />
                </td>
              </tr>
            </xsl:for-each>
          </table>
          </div>
        </body>
      </html>
      
    </xsl:template>
</xsl:stylesheet>
