using dufeksoft.lib.Model.Grid;
using dufeksoft.lib.ParamSet;
using dufeksoft.lib.UI;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using NPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class ProducerModel : _BaseModel
    {
        [Required(ErrorMessage = "Názov výrobcu musí byť zadaný")]
        [Display(Name = "Názov výrobcu")]
        public string ProducerName { get; set; }

        [AllowHtml]
        [Display(Name = "Popis výrobcu")]
        public string ProducerDescription { get; set; }

        [Display(Name = "Webová stránka")]
        public string ProducerWeb { get; set; }

        public void CopyDataFrom(EshoppgsoftwebProducer src)
        {
            this.pk = src.pk;
            this.ProducerName = src.ProducerName;
            this.ProducerDescription = src.ProducerDescription;
            this.ProducerWeb = src.ProducerWeb;
        }

        public void CopyDataTo(EshoppgsoftwebProducer trg)
        {
            trg.pk = this.pk;
            trg.ProducerName = this.ProducerName;
            trg.ProducerDescription = this.ProducerDescription;
            trg.ProducerWeb = this.ProducerWeb;
        }

        public static ProducerModel CreateCopyFrom(EshoppgsoftwebProducer src)
        {
            ProducerModel trg = new ProducerModel();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static EshoppgsoftwebProducer CreateCopyFrom(ProducerModel src)
        {
            EshoppgsoftwebProducer trg = new EshoppgsoftwebProducer();
            src.CopyDataTo(trg);

            return trg;
        }
    }

    public class ProducerListModel : List<ProducerModel>
    {
        public HttpRequest CurrentRequest { get; private set; }
        public string SessionId { get; set; }
        public int PageSize { get; private set; }

        private GridPagerModel currentPager;
        public GridPagerModel Pager
        {
            get
            {
                return GetPager();
            }
        }

        public ProducerListModel(HttpRequest request, int pageSize = 25)
        {
            this.CurrentRequest = request;
            this.PageSize = pageSize;
        }

        public List<ProducerModel> GetPageItems()
        {
            GridPageInfo cpi = this.Pager.GetCurrentPageInfo();

            List<ProducerModel> resultList = new List<ProducerModel>();
            for (int i = cpi.FirsItemIndex; i < this.Count && i < cpi.LastItemIndex + 1; i++)
            {
                resultList.Add(this[i]);
            }

            return resultList;
        }

        GridPagerModel GetPager()
        {
            if (this.currentPager == null || this.currentPager.ItemCnt != this.Count)
            {
                this.currentPager = new GridPagerModel(this.CurrentRequest, this.Count, this.PageSize);
            }

            return this.currentPager;
        }
    }

    public class ProducerPagingListModel : _PagingModel
    {
        public List<ProducerModel> Items { get; set; }

        public static ProducerPagingListModel CreateCopyFrom(Page<EshoppgsoftwebProducer> srcArray)
        {
            ProducerPagingListModel trgArray = new ProducerPagingListModel();
            trgArray.ItemsPerPage = (int)srcArray.ItemsPerPage;
            trgArray.TotalItems = (int)srcArray.TotalItems;
            trgArray.Items = new List<ProducerModel>(srcArray.Items.Count + 1);

            foreach (EshoppgsoftwebProducer src in srcArray.Items)
            {
                trgArray.Items.Add(ProducerModel.CreateCopyFrom(src));
            }

            return trgArray;
        }
    }

    public class ProducerFilterModel : _BaseUserPropModel
    {

        [Display(Name = "Vyhľadávanie (názov, popis, web ...)")]
        public string SearchText { get; set; }


        public ProducerFilterModel()
        {
            this.PropId = ConfigurationUtil.PropId_ProducerFilterModel;
        }

        public static ProducerFilterModel CreateCopyFrom(EshoppgsoftwebUserProp src)
        {
            ProducerFilterModel trg = new ProducerFilterModel();
            if (src != null)
            {
                trg.CopyDataFrom(src);
            }
            trg.UpdateBeforeEdit();

            return trg;
        }

        public static EshoppgsoftwebUserProp CreateCopyFrom(ProducerFilterModel src)
        {
            src.UpdateAfterEdit();
            EshoppgsoftwebUserProp trg = new EshoppgsoftwebUserProp();
            src.CopyDataTo(trg);

            return trg;
        }


        public void UpdateBeforeEdit()
        {
            LoadPropValue(this.PropValue);
        }

        public void UpdateAfterEdit()
        {
            this.PropValue = SavePropValue();
        }

        private string SavePropValue()
        {
            // Create XML document
            XmlDocument doc = new XmlDocument();
            // Create main element
            XmlElement mainNode = doc.CreateElement("ProducerFilterModel");
            mainNode.SetAttribute("version", "1.0");
            doc.AppendChild(mainNode);

            // Search text
            XmlParamSet.SaveItem(doc, mainNode, "SearchText", this.SearchText);

            return doc.InnerXml;
        }

        private void LoadPropValue(string propValue)
        {
            XmlDocument doc = new XmlDocument();
            if (!string.IsNullOrEmpty(propValue))
            {
                doc.LoadXml(propValue);

                string fullParent = "ProducerFilterModel";

                // Search text
                this.SearchText = XmlParamSet.LoadItem(doc, fullParent, "SearchText", string.Empty);
            }
        }
    }

    public class ProducerPagerModel
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class ProducerDropDown : CmpDropDown
    {
        public ProducerDropDown()
        {
        }

        public static ProducerDropDown CreateFromRepository(bool allowNull, string emptyText = "[ nezadané ]")
        {
            EshoppgsoftwebProducerRepository repository = new EshoppgsoftwebProducerRepository();
            return ProducerDropDown.CreateCopyFrom(repository.GetPage(1, _PagingModel.AllItemsPerPage), allowNull, emptyText);
        }

        public static ProducerDropDown CreateCopyFrom(Page<EshoppgsoftwebProducer> dataList, bool allowNull, string emptyText)
        {
            ProducerDropDown ret = new ProducerDropDown();

            if (allowNull)
            {
                ret.AddItem(emptyText, Guid.Empty.ToString(), null);
            }
            foreach (EshoppgsoftwebProducer dataItem in dataList.Items)
            {
                ProducerModel dataModel = ProducerModel.CreateCopyFrom(dataItem);
                ret.AddItem(dataModel.ProducerName, dataModel.pk.ToString(), dataModel);
            }

            return ret;
        }
    }
}
