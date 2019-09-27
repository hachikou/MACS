/// DateSelector: 日付入力ウィジェット.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Web;
using System.Text;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   日付入力ウィジェット
/// </summary>
/// <remarks>
///   <para>
///     年／月／日入力のプルダウンを表示します
///   </para>
/// </remarks>
public class DateSelector : TranslatableWebControl {

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public DateSelector() : base() {}

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public DateSelector(string name, string id) : base(name, id) {}

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public DateSelector(string name) : base(name) {}

    /// <summary>
    ///   翻訳機指定コンストラクタ
    /// </summary>
    public DateSelector(Translatable tr) : base(tr) {}

    /// <summary>
    ///   翻訳機指定コンストラクタ
    /// </summary>
    public DateSelector(string name, string id, Translatable tr) : base(name, id, tr) {}

    /// <summary>
    ///   翻訳機指定コンストラクタ
    /// </summary>
    public DateSelector(string name, Translatable tr) : base(name, tr) {}

    /// <summary>
    ///   選択された値。Valueと同じだが、DateTimeにキャストされている
    /// </summary>
    public DateTime Selected {
        get {
            if(Value == null)
                return new DateTime(0);
            return (DateTime)Value; }
        set { Value = value; }
    }

    /// <summary>
    ///   選択された値の表示文字列
    /// </summary>
    public string Text {
        get {
            string yearFormat = YearFormat??_("DateSelector.YearFormat");
            string yearSeparator = "";
            if(!yearFormat.Contains("{0}")) {
                yearFormat = "{0}";
                //月、または日を表示する場合、セパレータ文字を設定する
                if(ShowMonth || ShowDay) {
                    yearSeparator = "/";
                }
            }
            string monthFormat = MonthFormat??_("DateSelector.MonthFormat");
            string monthSeparator = "";
            if(!monthFormat.Contains("{0}")) {
                monthFormat = "{0}";
                //日を表示する場合、セパレータ文字を設定する
                if(ShowDay) {
                    monthSeparator = "/";
                }
            }
            string dayFormat = DayFormat??_("DateSelector.DayFormat");
            if(!dayFormat.Contains("{0}")) {
                dayFormat = "{0}";
            }
            StringBuilder sb = new StringBuilder();
            if(ShowYear) {
                sb.AppendFormat(yearFormat, Selected.Year);
                if(!String.IsNullOrEmpty(yearSeparator))
                    sb.Append(yearSeparator);
            }
            if(ShowMonth) {
                sb.AppendFormat(monthFormat, Selected.Month);
                if(!String.IsNullOrEmpty(monthSeparator))
                    sb.Append(monthSeparator);
            }
            if(ShowDay) {
                sb.AppendFormat(dayFormat, Selected.Day);
            }
            return sb.ToString();
        }
    }

    /// <summary>
    ///   年の選択範囲（始め）
    /// </summary>
    public int YearFrom = DateTime.Today.Year-10;

    /// <summary>
    ///   年の選択範囲（終わり）
    /// </summary>
    public int YearTo = DateTime.Today.Year+10;

    /// <summary>
    ///   変更時に呼び出されるJavaScript
    /// </summary>
    public string OnChange;
    
    /// <summary>
    ///   年の選択表示
    ///   <remarks>
    ///     true:プルダウン表示 false:プルダウン非表示
    ///   </remarks>
    /// </summary>
    public bool ShowYear= true; 

    /// <summary>
    ///   月の選択表示
    ///   <remarks>
    ///     true:プルダウン表示 false:プルダウン非表示
    ///   </remarks>
    /// </summary>
    public bool ShowMonth = true; 

    /// <summary>
    ///   日の選択表示
    ///   <remarks>
    ///     true:プルダウン表示 false:プルダウン非表示
    ///   </remarks>
    /// </summary>
    public bool ShowDay = true; 
    
    /// <summary>
    ///   年のフォーマット
    /// </summary>
    public string YearFormat = null;
    
    /// <summary>
    ///   月のフォーマット
    /// </summary>
    public string MonthFormat = null;
    
    /// <summary>
    ///   日のフォーマット
    /// </summary>
    public string DayFormat = null;

    /// <summary>
    ///   レンダリング
    /// </summary>
    public override StringBuilder Render(StringBuilder sb) {
        if(String.IsNullOrEmpty(Name)) {
            Name = "DateSelector";
        }
        if(!Visible) {
            sb.Append("<input type=\"hidden\" name=\"");
            sb.Append(Name+"Year");
            sb.Append("\" value=\"");
            sb.Append(Selected.Year.ToString());
            sb.Append("\"");
            if(!String.IsNullOrEmpty(ID)) {
                sb.Append(" id=\"");
                sb.Append(ID+"Year");
                sb.Append("\"");
            }
            sb.Append("/>");
            sb.Append("<input type=\"hidden\" name=\"");
            sb.Append(Name+"Month");
            sb.Append("\" value=\"");
            sb.Append(Selected.Month.ToString());
            sb.Append("\"");
            if(!String.IsNullOrEmpty(ID)) {
                sb.Append(" id=\"");
                sb.Append(ID+"Month");
                sb.Append("\"");
            }
            sb.Append("/>");
            sb.Append("<input type=\"hidden\" name=\"");
            sb.Append(Name+"Day");
            sb.Append("\" value=\"");
            sb.Append(Selected.Day.ToString());
            sb.Append("\"");
            if(!String.IsNullOrEmpty(ID)) {
                sb.Append(" id=\"");
                sb.Append(ID+"Day");
                sb.Append("\"");
            }
            sb.Append("/>");
            return sb;
        }
        sb.Append("<span class=\"dateselector");
        if(CssClass != null){
            sb.Append(" ");
            sb.Append(CssClass);
        }
        sb.Append("\"");
        if(!String.IsNullOrEmpty(ID)) {
            sb.Append(" id=\"");
            sb.Append(ID);
            sb.Append("\"");
        }
        sb.Append(">");

        string yearFormat = YearFormat??_("DateSelector.YearFormat");
        string yearSeparator = "";
        if(!yearFormat.Contains("{0}")) {
            yearFormat = "{0}";
            
            //月、または日を表示する場合、セパレータ文字を設定する
            if(ShowMonth || ShowDay) {
                yearSeparator = "/";
            }
        }
        string monthFormat = MonthFormat??_("DateSelector.MonthFormat");
        string monthSeparator = "";
        if(!monthFormat.Contains("{0}")) {
            monthFormat = "{0}";
            
            //日を表示する場合、セパレータ文字を設定する
            if(ShowDay) {
                monthSeparator = "/";
            }
        }
        string dayFormat = DayFormat??_("DateSelector.DayFormat");
        if(!dayFormat.Contains("{0}")) {
            dayFormat = "{0}";
        }

        if(ShowYear) {
            sb.Append("<select name=\"");
            sb.Append(Name+"Year");
            sb.Append("\"");
            if(!String.IsNullOrEmpty(OnChange)) {
                sb.Append(" onchange=\"");
                sb.Append(OnChange);
                sb.Append("\"");
            }
            sb.Append(">");
            for(int i = YearFrom; i <= YearTo; i++) {
                sb.Append("<option value=\"");
                sb.Append(i.ToString());
                sb.Append("\"");
                if(Selected.Year == i)
                    sb.Append(" selected=\"selected\"");
                sb.Append(">");
                sb.Append(HE(String.Format(yearFormat,i)));
                sb.Append("</option>");
            }
            sb.Append("</select>");
            if(!String.IsNullOrEmpty(yearSeparator)) {
                sb.Append(" "+yearSeparator+" ");
            }
        } else {
            sb.Append("<input type=\"hidden\" name=\"");
            sb.Append(Name+"Year");
            sb.Append("\" value=\"");
            sb.Append(Selected.Year.ToString());
            sb.Append("\"");
            if(!String.IsNullOrEmpty(ID)) {
                sb.Append(" id=\"");
                sb.Append(ID+"Year");
                sb.Append("\"");
            }
            sb.Append("/>");
        }

        if(ShowMonth) {
            sb.Append("<select name=\"");
            sb.Append(Name+"Month");
            sb.Append("\"");
            if(!String.IsNullOrEmpty(OnChange)) {
                sb.Append(" onchange=\"");
                sb.Append(OnChange);
                sb.Append("\"");
            }
            sb.Append(">");
            for(int i = 1; i <= 12; i++) {
                sb.Append("<option value=\"");
                sb.Append(i.ToString());
                sb.Append("\"");
                if(Selected.Month == i)
                    sb.Append(" selected=\"selected\"");
                sb.Append(">");
                sb.Append(HE(String.Format(monthFormat,i)));
                sb.Append("</option>");
            }
            sb.Append("</select>");
            if(!String.IsNullOrEmpty(monthSeparator)) {
                sb.Append(" "+monthSeparator+" ");
            }
        } else {
            sb.Append("<input type=\"hidden\" name=\"");
            sb.Append(Name+"Month");
            sb.Append("\" value=\"");
            sb.Append(Selected.Month.ToString());
            sb.Append("\"");
            if(!String.IsNullOrEmpty(ID)) {
                sb.Append(" id=\"");
                sb.Append(ID+"Month");
                sb.Append("\"");
            }
            sb.Append("/>");
        }

        if(ShowDay) {
            sb.Append("<select name=\"");
            sb.Append(Name+"Day");
            sb.Append("\"");
            if(!String.IsNullOrEmpty(OnChange)) {
                sb.Append(" onchange=\"");
                sb.Append(OnChange);
                sb.Append("\"");
            }
            sb.Append(">");
            for(int i = 1; i <= 31; i++) {
                sb.Append("<option value=\"");
                sb.Append(i.ToString());
                sb.Append("\"");
                if(Selected.Day == i)
                    sb.Append(" selected=\"selected\"");
                sb.Append(">");
                sb.Append(HE(String.Format(dayFormat,i)));
                sb.Append("</option>");
            }
            sb.Append("</select>");
        } else {
            sb.Append("<input type=\"hidden\" name=\"");
            sb.Append(Name+"Day");
            sb.Append("\" value=\"");
            sb.Append(Selected.Day.ToString());
            sb.Append("\"");
            if(!String.IsNullOrEmpty(ID)) {
                sb.Append(" id=\"");
                sb.Append(ID+"Day");
                sb.Append("\"");
            }
            sb.Append("/>");
        }

        sb.Append("</span>");
        RenderInLineError(sb);
        return sb;
    }

    public override void Fetch(HttpPage page, object defaultValue=null) {
        int yy,mm,dd;
        if(defaultValue is DateTime) {
            DateTime d = (DateTime)defaultValue;
            yy = d.Year;
            mm = d.Month;
            dd = d.Day;
        } else {
            yy = 0;
            mm = 0;
            dd = 0;
        }
        Value = StringUtil.ToDateTime(String.Format("{0}/{1}/{2}",
                                                    page.Fetch(Name+"Year",yy),
                                                    page.Fetch(Name+"Month",mm),
                                                    page.Fetch(Name+"Day",dd)));
    }
}

} // End of namespace
