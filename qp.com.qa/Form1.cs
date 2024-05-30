using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Sql;
using System.Data.SqlTypes;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Net.Mime;
using System.Net;
using static System.Windows.Forms.LinkLabel;
using System.Xml.Linq;

namespace qp.com.qa
{
    public partial class Form1 : Form
    {
        //CancelEventHandler webBrowser1NewWindow = null;
        ScrapClass.Class1 MainClass = new ScrapClass.Class1();
        HtmlDocument theDoc = null;
        HtmlElement temp_Doc = null; string attachment = "";
        string Step = "Step_1"; string MyUrl = ""; string lastpage = string.Empty; string maindata = string.Empty; string TranslateTable = string.Empty;
        int MyReturnCode = 0; int a = 0; int pno = 2; int b = 0; int t = 0; int lno = 2;
        int m = 0;
        List<string> TempLink = new List<string>(); int ID = 1; string intro = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Add in first form Form_Load event // and check productname and others are same as source name

            System.Diagnostics.Process[] procss = System.Diagnostics.Process.GetProcessesByName(Application.ProductName);
            if (procss.Count() > 1)
            {
                MessageBox.Show("Same Product Name Exe is Already Running....!\n\n\n\n\n", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
                Environment.Exit(0);
            }
            string dayname = DateTime.Now.DayOfWeek.ToString();
            if (dayname == "Monday")
            {
                dateTimePicker1From.Text = DateTime.Today.AddDays(-2).ToString();
            }
            else
            {
                dateTimePicker1From.Text = DateTime.Today.AddDays(-1).ToString();
            }
            dateTimePicker2To.Text = DateTime.Today.AddDays(0).ToString();
            radioButton1live.Checked = true;
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.StatusTextChanged += new EventHandler(webBrowser1_StatusTextChanged);
        }

        private void button2go_Click(object sender, EventArgs e)
        {
            GlobalLevel.Global.Fromdate = dateTimePicker1From.Text;
            GlobalLevel.Global.Todate = dateTimePicker2To.Text;

            panel1.Enabled = false;
            string link = "https://zakupki.rostelecom.ru/procedure/?PUBLISHED=&arrFilter_ff%5BNAME%5D=&arrFilter_pf%5BRECEIVER_ORG_CODE%5D=&arrFilter_pf%5BCREATION_DATE_FROM%5D=" + GlobalLevel.Global.Fromdate + "&arrFilter_pf%5BCREATION_DATE_TO%5D=" + GlobalLevel.Global.Todate + "&arrFilter_pf%5BAPPLY_END_DATE_FROM%5D=&arrFilter_pf%5BAPPLY_END_DATE_TO%5D=&set_filter=Y&set_filter=Apply+";
            webBrowser1.Navigate(link);
            //timer1.Enabled = true;
            Step = "Step_1";
        }



        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.ReadyState == WebBrowserReadyState.Complete)
            {
                theDoc = webBrowser1.Document;
                string MyDocType = webBrowser1.DocumentType.ToString();
                if (theDoc.Body.InnerText.Contains("Navigation to the webpage was canceled") || theDoc.Body.InnerText.Contains("This program cannot display the webpage"))
                {
                    MessageBox.Show(new Form() { TopMost = true }, "Network Connection Failed.\r\n    OR\r\nNavigation to the webpage was canceled!!", "Network Problem!!", MessageBoxButtons.OK, MessageBoxIcon.None);
                    webBrowser1.Refresh();
                }
                else
                {
                    switch (Step)
                    {
                        case "Step_1":
                            CollectLinks();
                            NextPage();

                            break;

                        case "Step_2":
                            GetData();
                            NavigateLinks();
                            break;

                        case "Step_3":

                            break;
                    }
                }
            }
            else
            {
                Application.DoEvents();
            }
        }

        private void CollectLinks()
        {
            try
            {
                HtmlElementCollection tbcol = theDoc.GetElementsByTagName("table");
                foreach (HtmlElement ele in tbcol)
                {
                    if (ele.OuterHtml.Contains("₽"))
                    {
                        HtmlElementCollection trcol = ele.GetElementsByTagName("TR");
                        foreach (HtmlElement ele1 in trcol)
                        {
                            if (ele1.OuterHtml.Contains("href"))
                            {
                                HtmlElementCollection tdcol = ele1.GetElementsByTagName("TD");

                                string Publish_date = "";
                                string Tender_no = "";
                                string Title = "";
                                string Organizer = "";
                                string ClosingDate = "";
                                string Price = "";
                                string link = "";

                                Publish_date = tdcol[0].InnerText;
                                Publish_date = Publish_date.Replace("\r\n", "").Trim();

                                String tdcol1 = tdcol[1].InnerText;
                                Tender_no = tdcol1.Remove(tdcol1.IndexOf("\r\n")).Trim();

                                Title = tdcol1.Substring(tdcol1.IndexOf("\r\n") + "\r\n".Length);

                                Organizer = tdcol[2].InnerText;

                                ClosingDate = tdcol[3].InnerText;

                                Price = tdcol[4].InnerText;

                                link = tdcol[1].InnerHtml;
                                link = link.Substring(link.IndexOf("href=\"") + "href=\"".Length);
                                link = link.Remove(link.IndexOf("\">")).Trim();
                                if (!link.Contains("https://zakupki.rostelecom.ru/"))
                                {
                                    link = "https://zakupki.rostelecom.ru" + link;
                                }

                                string total = Publish_date + "☻" + Tender_no + "♥" + Title + "♦" + Organizer + "♣" + ClosingDate + "♠" + Price + "◘" + link;
                                GlobalLevel.Global.Doc_Links.Add(total);
                                label2linkcoll.Visible = true;
                                label2linkcoll.Text = "Links Collected : " + GlobalLevel.Global.Doc_Links.Count;
                                label2linkcoll.Refresh();
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("Error in (collectlink) method...Give it for maintenance \n\n" + ee.Message, Application.ProductName);
                Application.Exit();
            }
        }

        public void NextPage()
        {
            try
            {
                if (GlobalLevel.Global.nextpage == true)
                {
                    if (theDoc.Body.InnerHtml.Contains("\">" + pno + "</A>"))
                    {
                        label1pno.Visible = true;
                        label1pno.Text = "Page No. : " + pno;
                        label1pno.Refresh();

                        HtmlElementCollection atag = theDoc.GetElementsByTagName("a");
                        foreach (HtmlElement tag in atag)
                        {
                            if (tag.OuterHtml.Contains(">" + pno + "</A>"))
                            {
                                tag.InvokeMember("click");
                                Application.DoEvents();
                                pno++;
                                // timer1.Enabled = true;
                                Step = "Step_1";
                                break;
                            }

                        }

                    }
                    else
                    {
                        NavigateLinks();
                    }
                }
                else
                {
                    NavigateLinks();
                }

            }
            catch (Exception)
            {


            }
        }

        public void NavigateLinks()
        {
            if (a < GlobalLevel.Global.Doc_Links.Count)
            {
                try

                {
                    string Link = GlobalLevel.Global.Doc_Links[a];
                    Link = Link.Substring(Link.IndexOf("◘") + 1);
                    webBrowser1.Navigate(Link);
                    //timer2.Enabled = true;
                    Step = "Step_2";
                }
                catch
                {

                }
            }
            else
            {

                MessageBox.Show("All Data Inserted Successfully....!!\n\nTotal Data : " + GlobalLevel.Global.Doc_Links.Count + "\n\nTotal Inserted : " + GlobalLevel.Global.inserted + "\n\nDuplicate Data : " + GlobalLevel.Global.duplicate + "\n\nExpired Data : " + GlobalLevel.Global.expired + "\n\nSkipped Data : " + GlobalLevel.Global.skipped, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
                Environment.Exit(0);
            }
        }

        private void CollectData()
        {
            if (a < GlobalLevel.Global.Doc_Links.Count - 1)
            {
                a++;
                label3datacoll.Visible = true;
                label3datacoll.Text = "Data Collected : " + a + "/" + GlobalLevel.Global.Doc_Links.Count;
                label3datacoll.Refresh();
                string maindata = theDoc.Body.InnerHtml;
                try
                {
                    maindata = maindata.Substring(maindata.IndexOf("<TABLE cellSpacing=0 cellPadding=0 width=\"100%\" border=0>") + "<TABLE cellSpacing=0 cellPadding=0 width=\"100%\" border=0>".Length);
                    maindata = maindata.Substring(maindata.IndexOf("<TABLE cellSpacing=0 cellPadding=0 width=\"100%\" border=0>"));
                    maindata = maindata.Remove(maindata.IndexOf("<DIV role=contentinfo class=footer-copy>"));
                    GlobalLevel.Global.Doc_Data.Add(maindata);
                }
                catch
                {

                }
                try
                {
                    //NavigateLinks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in CollectData() method\n\nError are : " + ex, "des.wa.gov");
                }
            }
            else
            {
                Step = "Step_5";
                a++;
                label3datacoll.Visible = true;
                label3datacoll.Text = "Data Collected : " + a + "/" + GlobalLevel.Global.Doc_Links.Count;
                label3datacoll.Refresh();
                string maindata = theDoc.Body.InnerHtml;
                try
                {
                    maindata = maindata.Substring(maindata.IndexOf("<TABLE cellSpacing=0 cellPadding=0 width=\"100%\" border=0>") + "<TABLE cellSpacing=0 cellPadding=0 width=\"100%\" border=0>".Length);
                    maindata = maindata.Substring(maindata.IndexOf("<TABLE cellSpacing=0 cellPadding=0 width=\"100%\" border=0>"));
                    maindata = maindata.Remove(maindata.IndexOf("<DIV role=contentinfo class=footer-copy>"));
                    GlobalLevel.Global.Doc_Data.Add(maindata);
                }
                catch
                {
                    maindata = MainClass.GetRqdStr(maindata, "<div class=\"tender-preview wrap-preview\">", "<div class=\"centered row\">", "", "");
                    GlobalLevel.Global.Doc_Data.Add(maindata);
                }
                GetData();
            }
        }

        private void GetData()
        {
            try
            {
                string htmldata = "";
                HtmlElement maindoc = theDoc.GetElementById("content");
                htmldata = maindoc.OuterHtml;
                htmldata = htmldata.Remove(htmldata.LastIndexOf("<TABLE")).Trim();
                htmldata = htmldata.Replace("/download", "https://zakupki.rostelecom.ru/download");

                if (htmldata.Contains("id=form_review_block"))
                {
                    string toremove = htmldata.Substring(htmldata.IndexOf("<DIV id=form_review_block"));
                    toremove = toremove.Remove(toremove.IndexOf("<DIV class=popup_header>"));
                    htmldata = htmldata.Replace(toremove, "");
                }
                

                while (htmldata.Contains("<BUTTON"))
                {
                    string remove = htmldata.Substring(htmldata.IndexOf("<BUTTON"));
                    remove = remove.Substring(0, remove.IndexOf(">") + 1);
                    htmldata = htmldata.Replace(remove, "");
                }

                #region Get additional attach_docs
                string attach_link_withname = string.Empty;
                string tdcol = htmldata;


                while (tdcol.Contains("href=\""))
                {
                    // Extract the link
                    int hrefIndex = tdcol.IndexOf("href=\"") + "href=\"".Length;
                    string doclink = tdcol.Substring(hrefIndex);
                    doclink = doclink.Substring(0, doclink.IndexOf("\"")).Trim();

                    if (doclink.Contains("download"))
                    {
                        // Replace the relative download URL with the absolute URL
                        string absoluteDoclink = doclink.Replace("/download", "https://zakupki.rostelecom.ru/download");

                        // Prepare to extract the document name
                        string docname = string.Empty;

                        // Find the next "loadItem_desc" after the current link
                        int loadItemDescIndex = tdcol.IndexOf("<DIV class=loadItem_desc>", hrefIndex);
                        if (loadItemDescIndex != -1)
                        {
                            loadItemDescIndex += "<DIV class=loadItem_desc>".Length;
                            docname = tdcol.Substring(loadItemDescIndex);
                            docname = docname.Remove(docname.IndexOf("</DIV>")).Trim();
                            docname = Regex.Replace(docname, @"<[^>]*>", String.Empty).Trim();
                        }
                        if (docname != "")
                        {
                            if (!docname.Contains(".zip"))
                            {
                                // Append the link and name to the result string
                                string fullurldocnam = docname + "~" + absoluteDoclink;
                                if (string.IsNullOrEmpty(attach_link_withname))
                                {
                                    attach_link_withname = fullurldocnam;
                                }
                                else
                                {
                                    attach_link_withname += "," + fullurldocnam;
                                }
                            }


                        }


                        // Move past this link to avoid infinite loop
                        tdcol = tdcol.Substring(hrefIndex);
                    }
                    else
                    {
                        // Move past this href to avoid infinite loop
                        tdcol = tdcol.Substring(hrefIndex);
                    }
                    doclink = "href=\"" + doclink + "";
                    tdcol = tdcol.Replace(doclink, "");

                }

                #endregion

                GlobalLevel.Global.DataHtmlDoc = htmldata;
                GlobalLevel.Global.DocPath = GlobalLevel.Global.Doc_Links[a];
                MyReturnCode = MainClass.SegDoc(GlobalLevel.Global.DataHtmlDoc, GlobalLevel.Global.DocPath, attach_link_withname);
                a++;
                label4datainserted.Visible = true;
                label4datainserted.Text = "Inserted : " + GlobalLevel.Global.inserted + "\nDuplicate : " + GlobalLevel.Global.duplicate + "\nExpired : " + GlobalLevel.Global.expired;
                label4datainserted.Refresh();
                label3datacoll.Visible = true;
                label3datacoll.Text = "Completed : " + a + " / " + GlobalLevel.Global.Doc_Links.Count();
                label3datacoll.Refresh();
                this.Text = Application.ProductName + "[" + a + " / " + GlobalLevel.Global.Doc_Links.Count() + "]";

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GetData() method\n\nError are : " + ex, Application.ProductName);
            }

        }

        private string getFilename1(string url)
        {
            Uri uri = new Uri(url);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);
            filename = filenameReplace(filename);
            if (!filename.Contains("."))
            {
                filename = "";
            }
            return filename;
        }

        public string filenameReplace(string filename)
        {
            filename = Regex.Replace(filename, @"[^0-9a-zA-Z .\-_]+", String.Empty).Trim();
            filename = Regex.Replace(filename, @"\s+", " ");
            filename = filename.Replace(" ", "-");
            filename = filename.Replace("---", "-");
            filename = filename.Replace("--", "-");
            return filename;
        }

        private void button1exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {

        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            txtUrlName.Text = e.Url.ToString();
            MyUrl = txtUrlName.Text;
        }

        private void webBrowser1_NewWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void webBrowser1_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            try
            {
                //toolStripProgressBar1.Maximum = (int)e.MaximumProgress;
                //toolStripProgressBar1.Value = (int)e.CurrentProgress;
            }
            catch (Exception ex)
            { }
        }

        private void radioButton1live_CheckedChanged(object sender, EventArgs e)
        {
            GlobalLevel.Global.flagServer = true;
            GlobalLevel.Global.server = "Live";
            this.Text = "zakupki.rostelecom.ru [Live Server Selected]";
            label5server.Text = "Live Server Selected";
            label5server.Refresh();
        }

        private void radioButton2test_CheckedChanged(object sender, EventArgs e)
        {
            GlobalLevel.Global.flagServer = false;
            GlobalLevel.Global.server = "Test";
            this.Text = "zakupki.rostelecom.ru [Test Server Selected]";
            label5server.Text = "Test Server Selected";
            label5server.Refresh();
        }

        private void webBrowser1_StatusTextChanged(object sender, EventArgs e)
        {
            //toolStripStatusLabel1.Text = webBrowser1.StatusText;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dateTimePicker1From_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label1timer_Click(object sender, EventArgs e)
        {

        }



        private void timer1_Tick_1(object sender, EventArgs e)
        {
            if (t >= 8)
            {
                theDoc = webBrowser1.Document;
                t = 0;
                timer1.Enabled = false;
                if (theDoc != null)
                {
                    if (theDoc.Body != null)
                    {

                        if (!theDoc.Body.InnerHtml.Contains("blockUI blockOverlay"))
                        {
                            CollectLinks();
                            NextPage();
                        }
                        else
                        {
                            timer1.Enabled = true;
                        }

                    }
                    else
                    {
                        timer1.Enabled = true;
                    }
                }
                else
                {
                    timer1.Enabled = true;
                }
            }
            else
            {
                t++;
                label1timer.Visible = true;
                label1timer.Text = (t * 2) + "% Loading...";
                label1timer.Refresh();
            }
        }
    }
}
