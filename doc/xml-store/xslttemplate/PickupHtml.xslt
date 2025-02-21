<?xml version="1.0"?>
<xsl:stylesheet  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template match="/">
    <html>
      <body>
        <h1>日通EDI取貨(Pickup轉出)</h1>
        <hr />
        <table width="100%" border="0">
          <tr bgcolor="gainsboro">
            <td>
              <b>Tracking號碼</b>
            </td>
            <td>
              <b>客戶代碼</b>
            </td>
            <td>
              <b>取件地址</b>
            </td>
            <td>
              <b>取件連詻電話</b>
            </td>
            <td>
              <b>取件聯絡人</b>
            </td>
            <td>
              <b>備註</b>
            </td>
            <td>
              <b>叫件日期</b>
            </td>
            <td>
              <b>叫件時間</b>
            </td>
          </tr>
          <xsl:for-each select="DocumentElement/PICKUP">
            <tr>
              <td>
                <xsl:value-of select="TRACKING_NO" />
              </td>
              <td>
                <xsl:value-of select="CUSTOM_NO" />
              </td>
              <td>
                <xsl:value-of select="PICKUP_ADDR" />
              </td>
              <td>
                <xsl:value-of select="PICKUP_TEL" />
              </td>
              <td>
                <xsl:value-of select="PICKUP_ATTN" />
              </td>
              <td>
                <xsl:value-of select="REMARK" />
              </td>
              <td>
                <xsl:value-of select="CALL_DATE" />
              </td>
              <td>
                <xsl:value-of select="CALL_TIME" />
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>