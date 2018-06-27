/// HtmlCalender: カレンダーをHTMLで生成するためのクラス.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Web;
//using System.Web.UI;
using System.Text;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   カレンダーをHTMLで生成するためのクラス。
/// </summary>
public class HtmlCalender {

    private string m_tagname;
    private string m_inputtagname;

    /// <summary>
    ///   カレンダー生成器を作る。
    /// </summary>
    /// <param name="tagname">カレンダになるテーブルタグのID名</param>
    /// <param name="inputtagname">カレンダの値の入力先になるINPUTタグのID名</param>
    public HtmlCalender(string tagname, string inputtagname) {
        m_tagname = tagname;
        m_inputtagname = inputtagname;
    }

    /// <summary>
    ///   カレンダーHTMLを生成する。
    /// </summary>
    public string Render(HttpTemplatePage page) {
        StringBuilder sb = new StringBuilder();
        Render(sb, page);
        return sb.ToString();
    }

    /// <summary>
    ///   カレンダーHTMLをStringBuilderに書き込む
    /// </summary>
    public void Render(StringBuilder sb, HttpTemplatePage page){
        sb.Append("<table id=\"");
        sb.Append(m_tagname);
        sb.Append("\" class=\"calender\" style=\"display:none;\">\n");
        sb.Append("  <tr class=\"year\">\n");
        sb.Append("    <td class=\"prev\">\n");
        sb.Append("      <a href=\"javascript:void(0);\" onclick=\"cal_prev_year('");
        sb.Append(m_tagname);
        sb.Append("','");
        sb.Append(m_inputtagname);
        sb.Append("');return false;\">&lt;&lt;</a>&nbsp;\n");
        sb.Append("      <a href=\"javascript:void(0);\" onclick=\"cal_prev_month('");
        sb.Append(m_tagname);
        sb.Append("','");
        sb.Append(m_inputtagname);
        sb.Append("');return false;\">&lt;</a>\n");
        sb.Append("    </td>\n");
        sb.Append("    <td colspan=\"5\">\n");
        sb.Append("      <select id=\"");
        sb.Append(m_tagname);
        sb.Append("_year\" onchange=\"cal_setup('");
        sb.Append(m_tagname);
        sb.Append("','");
        sb.Append(m_inputtagname);
        sb.Append("');\">\n");
        sb.Append("      </select>\n");
        sb.Append("      <select id=\"");
        sb.Append(m_tagname);
        sb.Append("_month\" onchange=\"cal_setup('");
        sb.Append(m_tagname);
        sb.Append("','");
        sb.Append(m_inputtagname);
        sb.Append("');\">\n");
        string [] MONTH_NAME = {page._("1月"), page._("2月"), page._("3月"), page._("4月"),
                                page._("5月"), page._("6月"), page._("7月"), page._("8月"),
                                page._("9月"), page._("10月"), page._("11月"), page._("12月")};
        for(int i = 1; i <= 12; i++){
            sb.Append("        <option value=\"");
            sb.Append(i.ToString());
            sb.Append("\">");
            sb.Append(MONTH_NAME[i-1]);
            sb.Append("</option>\n");
        }
        sb.Append("      </select>\n");
        sb.Append("    </td>\n");
        sb.Append("    <td class=\"next\" colspan=\"2\">\n");
        sb.Append("      <a href=\"javascript:void(0);\" onclick=\"cal_next_month('");
        sb.Append(m_tagname);
        sb.Append("','");
        sb.Append(m_inputtagname);
        sb.Append("');return false;\">&gt;</a>&nbsp;\n");
        sb.Append("      <a href=\"javascript:void(0);\" onclick=\"cal_next_year('");
        sb.Append(m_tagname);
        sb.Append("','");
        sb.Append(m_inputtagname);
        sb.Append("');return false;\">&gt;&gt;</a>\n");
        sb.Append("    </td>\n");
        sb.Append("  </tr>\n");
        for(int i = 0; i < 6; i++){
            sb.Append("  <tr class=\"day\">\n");
            sb.Append("    <td class=\"sun\"></td><td class=\"mon\"></td><td class=\"tue\"></td><td class=\"wed\"></td><td class=\"thu\"></td><td class=\"fri\"></td><td class=\"sat\"></td>\n");
            sb.Append("  </tr>\n");
        }
        sb.Append("</table>\n");
        //sb.Append("<script type=\"text/javascript\">\n");
        //sb.Append("<!--\n");
        //sb.Append("cal_setup(\"");
        //sb.Append(m_tagname);
        //sb.Append("\",\"");
        //sb.Append(m_inputtagname);
        //sb.Append("\");\n");
        //sb.Append("// -->\n");
        //sb.Append("</script>\n");
    }

}

} // End of namespace
