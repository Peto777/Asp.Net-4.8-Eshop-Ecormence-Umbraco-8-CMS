using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Util;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Web;
using System.Xml;

namespace eshoppgsoftweb.lib.Tasks.Ecommerce
{
    public class QuoteToXml
    {
        public static string FtpDefaultPath = "\\App_Data\\MkSoft";

        public XmlDocument Xml { get; private set; }
        public QuoteModel Quote { get; private set; }


        public QuoteToXml(Guid quoteKey)
        {
            this.Quote = QuoteModel.GetCompleteModel(quoteKey);

            // Create XML document
            this.Xml = new XmlDocument();
            XmlDeclaration xmlDeclaration = this.Xml.CreateXmlDeclaration("1.0", "windows-1250", null);
            this.Xml.AppendChild(xmlDeclaration);

            // Create main element
            XmlElement mainNode = this.Xml.CreateElement("data");
            mainNode.SetAttribute("popis", "export z gloziksoft.sk");
            mainNode.SetAttribute("kontakt", "");
            mainNode.SetAttribute("verzia", "2");
            this.Xml.AppendChild(mainNode);

            Hashtable ht = new Hashtable();
            XmlElement priceListNode = this.Xml.CreateElement("cennik");
            mainNode.AppendChild(priceListNode);
            foreach (Product2QuoteModel item in this.Quote.Items)
            {
                if (item.BasePriceNoVat > 0M && item.IsProductItem)
                {
                    if (!ht.ContainsKey(item.ItemCode))
                    {
                        XmlElement itemNode = this.Xml.CreateElement("polozka");
                        priceListNode.AppendChild(itemNode);

                        //AddItem(itemNode, "plu", item.ItemCode);
                        AddItem(itemNode, "kod", item.ItemCode);
                        AddItem(itemNode, "nazov", item.ItemName);
                        AddItem(itemNode, "skupina", item.BasePriceVatPerc == 10M ? "8" : "1");
                        AddItem(itemNode, "mj", item.UnitName);

                        ht.Add(item.ItemCode, null);
                    }
                }
            }

            XmlElement quoteNode = this.Xml.CreateElement("objednavka");
            mainNode.AppendChild(quoteNode);

            AddItem(quoteNode, "typ", "OP");
            AddItem(quoteNode, "cislo", this.Quote.QuoteId);
            AddItem(quoteNode, "datum", DateTimeUtil.GetDisplayDate(this.Quote.DateFinished.Value));
            AddItem(quoteNode, "typceny", "2");
            //AddItem(quoteNode, "OPVybavTypV", "VPH");
            //AddItem(quoteNode, "sposobdopravy", this.Quote.GetTransportTypeName());
            //AddItem(quoteNode, "sposobuhrady", this.Quote.GetPaymentTypeName());

            XmlElement addressNode = this.Xml.CreateElement("firma");
            quoteNode.AppendChild(addressNode);

            if (this.Quote.User.IsCompanyInvoice)
            {
                AddItem(addressNode, "ico", this.Quote.User.CompanyIco);
                AddItem(addressNode, "dic", this.Quote.User.CompanyDic);
                if (!string.IsNullOrEmpty(this.Quote.User.CompanyIcdph))
                {
                    AddItem(addressNode, "icdph", this.Quote.User.CompanyIcdph);
                }
                AddItem(addressNode, "nazov1", this.Quote.User.CompanyName);
                AddItem(addressNode, "nazov2", this.Quote.User.InvName);
            }
            else
            {
                AddItem(addressNode, "nazov1", this.Quote.User.InvName);
            }
            AddItem(addressNode, "ulica", this.Quote.User.InvStreet);
            AddItem(addressNode, "psc", this.Quote.User.InvZip);
            AddItem(addressNode, "obec", this.Quote.User.InvCity);
            AddItem(addressNode, "stat", this.Quote.User.InvCountry);
            AddItem(addressNode, "kodstatu", this.Quote.User.InvCountryCode);
            AddItem(addressNode, "telefon", this.Quote.User.QuotePhone);
            AddItem(addressNode, "email", this.Quote.User.QuoteEmail);

            NumberFormatInfo numfi = NumberFormatInfo.InvariantInfo;

            foreach (Product2QuoteModel item in this.Quote.Items)
            {
                if (item.BasePriceWithVat > 0M)
                {
                    XmlElement itemNode = this.Xml.CreateElement("pohyb");
                    quoteNode.AppendChild(itemNode);

                    AddItem(itemNode, "oznacenie", item.ItemCode);
                    AddItem(itemNode, "nazov", item.ItemName);
                    AddItem(itemNode, "pocet", PriceUtil.NumberFromEditorString(item.ItemPcs).ToString(numfi));
                    AddItem(itemNode, "cena", item.BasePriceWithVat.ToString(numfi));
                    //AddItem(itemNode, "pdph", item.BasePriceVatPerc.ToString(numfi));
                }
            }
        }

        void AddItem(XmlElement parent, string name, string value)
        {
            XmlElement item = this.Xml.CreateElement(name);
            item.InnerXml = value;
            parent.AppendChild(item);
        }

        public static void ExportToFtp(Guid quoteKey)
        {
            QuoteToXml export = new QuoteToXml(quoteKey);
            byte[] data = XmlUtil.ToWin1250(export.Xml);
            string name = string.Format("Objednavka{0}.xml", export.Quote.QuoteId);

            string fileFullName = string.Format("{0}{1}\\{2}",
                HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath),
                FtpDefaultPath,
                name);

            using (var fs = new FileStream(fileFullName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();
            }
        }
    }
}
