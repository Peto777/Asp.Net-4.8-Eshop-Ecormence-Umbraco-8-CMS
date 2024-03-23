using eshoppgsoftweb.lib.Controllers;
using eshoppgsoftweb.lib.Controllers.Ecommerce;
using eshoppgsoftweb.lib.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class CategoryModel : _BaseModel
    {
        public const string EmptyImgUrl = "/Styles/Images/img-add.png";

        [Display(Name = "Zobrazovať")]
        public bool CategoryIsVisible { get; set; }
        public string CategoryIsVisibleText { get; set; }

        public Guid ParentCategoryKey { get; set; }
        [Display(Name = "Poradie")]
        public int CategoryOrder { get; set; }
        [Required(ErrorMessage = "Kód kategórie musí byť zadaný")]
        [Display(Name = "Kód kategórie")]
        public string CategoryCode { get; set; }
        [Required(ErrorMessage = "Názov kategórie musí byť zadaný")]
        [Display(Name = "Názov kategórie")]
        public string CategoryName { get; set; }
        [AllowHtml]
        [Display(Name = "Popis kategórie")]
        public string CategoryDescription { get; set; }
        [Display(Name = "Popis pre ponukový list")]
        public string CategoryOfferText { get; set; }

        [Display(Name = "Obrázok")]
        public string CategoryImg { get; set; }

        public string AdminImgUrl
        {
            get
            {
                return string.IsNullOrEmpty(this.CategoryImg) ? CategoryModel.EmptyImgUrl : this.CategoryImg;
            }
        }

        [Required(ErrorMessage = "URL musí byť zadané")]
        [Display(Name = "URL")]
        public string CategoryUrl { get; set; }
        [Required(ErrorMessage = "META TITLE musí byť zadané")]
        [Display(Name = "META TITLE")]
        public string CategoryMetaTitle { get; set; }
        [Required(ErrorMessage = "META KEYWORDS musí byť zadané")]
        [Display(Name = "META KEYWORDS")]
        public string CategoryMetaKeywords { get; set; }
        [Required(ErrorMessage = "META DESCRIPTION musí byť zadané")]
        [Display(Name = "META DESCRIPTION")]
        public string CategoryMetaDescription { get; set; }


        public List<CategoryModel> Parents { get; set; }
        public List<CategoryModel> Children { get; set; }
        public CategoryModel Parent { get; set; }

        public bool HasChildren
        {
            get
            {
                return this.Children != null && this.Children.Count > 0;
            }
        }

        public bool IsRoot
        {
            get
            {
                return this.pk == Guid.Empty;
            }
        }

        public string Url
        {
            get
            {
                return string.Format("{0}{1}", CategoryContentFinder.CategoryPath, this.CategoryUrl);
            }
        }

        public string TabId { get; set; }
        public string TabSubcategActiveCss
        {
            get
            {
                return string.IsNullOrEmpty(this.TabId) || this.TabId == _BaseCategoryController.ReturnToTabVal_Subcateg ? "active" : string.Empty;
            }
        }
        public string TabProdInCatActiveCss
        {
            get
            {
                return this.TabId == _BaseCategoryController.ReturnToTabVal_ProdInCat ? "active" : string.Empty;
            }
        }
        public string TabProdNotInCatActiveCss
        {
            get
            {
                return this.TabId == _BaseCategoryController.ReturnToTabVal_ProdNotInCat ? "active" : string.Empty;
            }
        }

        public void CopyDataFrom(EshoppgsoftwebCategory src)
        {
            if (src == null)
            {
                return;
            }
            this.pk = src.pk;
            this.CategoryIsVisible = src.CategoryIsVisible;
            this.CategoryIsVisibleText = this.CategoryIsVisible ? "ÁNO" : "NIE";
            this.ParentCategoryKey = src.ParentCategoryKey;
            this.CategoryOrder = src.CategoryOrder;
            this.CategoryCode = src.CategoryCode;
            this.CategoryName = src.CategoryName;
            this.CategoryDescription = src.CategoryDescription;
            this.CategoryImg = src.CategoryImg;
            this.CategoryOfferText = src.CategoryOfferText;

            this.CategoryUrl = src.CategoryUrl;
            this.CategoryMetaTitle = src.CategoryMetaTitle;
            this.CategoryMetaKeywords = src.CategoryMetaKeywords;
            this.CategoryMetaDescription = src.CategoryMetaDescription;
        }

        public void CopyDataTo(EshoppgsoftwebCategory trg)
        {
            trg.pk = this.pk;
            trg.CategoryIsVisible = this.CategoryIsVisible;
            trg.ParentCategoryKey = this.ParentCategoryKey;
            trg.CategoryOrder = this.CategoryOrder;
            trg.CategoryCode = this.CategoryCode;
            trg.CategoryName = this.CategoryName;
            trg.CategoryDescription = this.CategoryDescription;
            trg.CategoryImg = this.CategoryImg;
            trg.CategoryOfferText = this.CategoryOfferText;

            trg.CategoryUrl = this.CategoryUrl;
            trg.CategoryMetaTitle = this.CategoryMetaTitle;
            trg.CategoryMetaKeywords = this.CategoryMetaKeywords;
            trg.CategoryMetaDescription = this.CategoryMetaDescription;
        }

        public static CategoryModel CreateCopyFrom(EshoppgsoftwebCategory src)
        {
            CategoryModel trg = new CategoryModel();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static EshoppgsoftwebCategory CreateCopyFrom(CategoryModel src)
        {
            EshoppgsoftwebCategory trg = new EshoppgsoftwebCategory();
            src.CopyDataTo(trg);

            return trg;
        }

        public List<List<CategoryModel>> GetChildRows(int colCnt)
        {
            List<List<CategoryModel>> rows = new List<List<CategoryModel>>();
            List<CategoryModel> row = null;

            int idx = 0;
            foreach (CategoryModel child in this.Children)
            {
                if (idx % colCnt == 0)
                {
                    row = new List<CategoryModel>();
                    rows.Add(row);
                }
                idx++;

                row.Add(child);
            }

            return rows;
        }

        public void LoadRelatives(EshoppgsoftwebCategoryRepository repository)
        {
            this.Children = GetChildren(repository, this.pk);
            this.Parents = GetParents(repository, this.pk);
        }

        public static List<CategoryModel> GetChildren(EshoppgsoftwebCategoryRepository repository, Guid categoryKey)
        {
            List<CategoryModel> dataList = new List<CategoryModel>();
            foreach (EshoppgsoftwebCategory category in repository.GetRecordsForParent(categoryKey))
            {
                dataList.Add(CategoryModel.CreateCopyFrom(category));
            }

            return dataList;
        }

        public static List<CategoryModel> GetParents(EshoppgsoftwebCategoryRepository repository, Guid categoryKey)
        {
            List<CategoryModel> dataList = new List<CategoryModel>();

            Hashtable htKey = new Hashtable();
            while (categoryKey != null && categoryKey != Guid.Empty)
            {
                if (htKey.ContainsKey(categoryKey))
                {
                    // This category was already reached. Possible infite loop recursion!!!
                    break;
                }
                else
                {
                    CategoryModel model = CategoryModel.CreateCopyFrom(repository.Get(categoryKey));
                    dataList.Add(model);
                    htKey.Add(categoryKey, model);

                    // Move to the upper parent
                    categoryKey = model.ParentCategoryKey;
                }
            }

            return dataList;
        }
    }

    public class CategoryTree
    {
        public CategoryModel Root { get; private set; }
        Hashtable htTree;
        Hashtable htTreeCode;

        public CategoryTree()
        {
            this.Root = new CategoryModel();
            this.Root.pk = Guid.Empty;

            EshoppgsoftwebCategoryRepository repository = new EshoppgsoftwebCategoryRepository();
            List<EshoppgsoftwebCategory> dataList = repository.GetPage(1, _PagingModel.AllItemsPerPage, Guid.Empty).Items;
            this.htTree = new Hashtable(dataList.Count + 1);
            this.htTreeCode = new Hashtable(dataList.Count + 1);
            ResolveTreeChildren(this.Root, dataList);
        }

        void ResolveTreeChildren(CategoryModel parent, List<EshoppgsoftwebCategory> dataList)
        {
            parent.Children = new List<CategoryModel>();
            foreach (EshoppgsoftwebCategory category in dataList)
            {
                if (!this.htTree.ContainsKey(category.pk) && category.ParentCategoryKey == parent.pk)
                {
                    CategoryModel categoryModel = CategoryModel.CreateCopyFrom(category);
                    this.htTree.Add(category.pk, categoryModel);
                    if (!this.htTreeCode.ContainsKey(category.CategoryCode))
                    {
                        this.htTreeCode.Add(category.CategoryCode, categoryModel);
                    }
                    parent.Children.Add(categoryModel);
                    categoryModel.Parent = parent;
                }
            }

            foreach (CategoryModel child in parent.Children)
            {
                ResolveTreeChildren(child, dataList);
            }
        }

        public CategoryModel GetCategoryNode(Guid categoryKey)
        {
            if (categoryKey == Guid.Empty)
            {
                return this.Root;
            }
            return this.htTree.ContainsKey(categoryKey) ? (CategoryModel)this.htTree[categoryKey] : null;
        }
        public CategoryModel GetCategoryNode(string categoryCode)
        {
            return this.htTreeCode.ContainsKey(categoryCode) ? (CategoryModel)this.htTreeCode[categoryCode] : null;
        }

        public List<CategoryModel> GetParents(Guid categoryKey)
        {
            List<CategoryModel> parents = new List<CategoryModel>();
            CategoryModel category = GetCategoryNode(categoryKey);
            if (category != null)
            {
                CategoryModel parent = category.Parent;
                while (parent != null)
                {
                    parents.Add(parent);
                    parent = parent.Parent;
                }
            }

            return parents;
        }

        public static List<string> GetDeepCategoryList(_BaseController ctrl, string categoryKey, bool onlyVisible = false)
        {
            CategoryTree tree = ctrl.GetCurrentEshopModel().CategoryTreeData;
            CategoryModel root = tree.GetCategoryNode(new Guid(categoryKey));
            if (root == null)
            {
                return null;
            }
            if (onlyVisible)
            {
                if (!root.CategoryIsVisible)
                {
                    return null;
                }
            }

            List<string> ret = new List<string>();
            ret.Add(root.pk.ToString());
            AddChildrenToDeepCategoryList(root, ret, onlyVisible);

            return ret;
        }

        private static void AddChildrenToDeepCategoryList(CategoryModel parent, List<string> ret, bool onlyVisible)
        {
            if (parent != null && parent.Children.Count > 0)
            {
                foreach (CategoryModel child in parent.Children)
                {
                    if (onlyVisible)
                    {
                        if (!child.CategoryIsVisible)
                        {
                            continue;
                        }
                    }
                    ret.Add(child.pk.ToString());
                    AddChildrenToDeepCategoryList(child, ret, onlyVisible);
                }
            }
        }
    }

    public class CategoryBannerModel
    {
        public CategoryModel Category { get; private set; }
        public List<CategoryModel> Children { get; private set; }

        public CategoryBannerModel(string categoryCode)
        {
            EshoppgsoftwebCategoryRepository rep = new EshoppgsoftwebCategoryRepository();
            this.Category = CategoryModel.CreateCopyFrom(rep.GetForCategoryCode(categoryCode));
            this.Children = CategoryModel.GetChildren(rep, this.Category.pk);
        }
    }
}
