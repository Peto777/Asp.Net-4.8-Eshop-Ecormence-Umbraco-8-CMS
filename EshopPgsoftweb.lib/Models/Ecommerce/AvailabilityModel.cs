using dufeksoft.lib.Model.Grid;
using dufeksoft.lib.UI;
using eshoppgsoftweb.lib.Repositories;
using NPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class AvailabilityModel : _BaseModel
    {
        [Required(ErrorMessage = "Názov dostupnosti produktu musí byť zadaný")]
        [Display(Name = "Názov dostupnosti produktu")]
        public string AvailabilityName { get; set; }

        public void CopyDataFrom(EshoppgsoftwebAvailability src)
        {
            this.pk = src.pk;
            this.AvailabilityName = src.AvailabilityName;
        }

        public void CopyDataTo(EshoppgsoftwebAvailability trg)
        {
            trg.pk = this.pk;
            trg.AvailabilityName = this.AvailabilityName;
        }

        public static AvailabilityModel CreateCopyFrom(EshoppgsoftwebAvailability src)
        {
            AvailabilityModel trg = new AvailabilityModel();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static EshoppgsoftwebAvailability CreateCopyFrom(AvailabilityModel src)
        {
            EshoppgsoftwebAvailability trg = new EshoppgsoftwebAvailability();
            src.CopyDataTo(trg);

            return trg;
        }
    }

    public class AvailabilityListModel : List<AvailabilityModel>
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

        public AvailabilityListModel(HttpRequest request, int pageSize = 25)
        {
            this.CurrentRequest = request;
            this.PageSize = pageSize;
        }

        public List<AvailabilityModel> GetPageItems()
        {
            GridPageInfo cpi = this.Pager.GetCurrentPageInfo();

            List<AvailabilityModel> resultList = new List<AvailabilityModel>();
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

    public class AvailabilityPagingListModel : _PagingModel
    {
        public List<AvailabilityModel> Items { get; set; }

        public static AvailabilityPagingListModel CreateCopyFrom(Page<EshoppgsoftwebAvailability> srcArray)
        {
            AvailabilityPagingListModel trgArray = new AvailabilityPagingListModel();
            trgArray.ItemsPerPage = (int)srcArray.ItemsPerPage;
            trgArray.TotalItems = (int)srcArray.TotalItems;
            trgArray.Items = new List<AvailabilityModel>(srcArray.Items.Count + 1);

            foreach (EshoppgsoftwebAvailability src in srcArray.Items)
            {
                trgArray.Items.Add(AvailabilityModel.CreateCopyFrom(src));
            }

            return trgArray;
        }
    }

    public class AvailabilityDropDown : CmpDropDown
    {
        public AvailabilityDropDown()
        {
        }

        public static AvailabilityDropDown CreateFromRepository(bool allowNull, string emptyText = "[ nezadané ]")
        {
            EshoppgsoftwebAvailabilityRepository repository = new EshoppgsoftwebAvailabilityRepository();
            return AvailabilityDropDown.CreateCopyFrom(repository.GetPage(1, _PagingModel.AllItemsPerPage), allowNull, emptyText);
        }

        public static AvailabilityDropDown CreateCopyFrom(Page<EshoppgsoftwebAvailability> dataList, bool allowNull, string emptyText)
        {
            AvailabilityDropDown ret = new AvailabilityDropDown();

            if (allowNull)
            {
                ret.AddItem(emptyText, Guid.Empty.ToString(), null);
            }
            foreach (EshoppgsoftwebAvailability dataItem in dataList.Items)
            {
                AvailabilityModel dataModel = AvailabilityModel.CreateCopyFrom(dataItem);
                ret.AddItem(dataModel.AvailabilityName, dataModel.pk.ToString(), dataModel);
            }

            return ret;
        }
    }
}
