using dufeksoft.lib.ParamSet;
using eshoppgsoftweb.lib.Controllers;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using NPoco;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class Product2CategoryModel : _BaseModel
    {
        public string ID
        {
            get
            {
                return string.Format("{0}|{1}", this.PkCategory, this.PkProduct);
            }
        }

        public Guid PkCategory { get; set; }
        public Guid PkProduct { get; set; }
        public int ProductOrder { get; set; }

        public ProductModel Product { get; set; }
        public string ProductCode
        {
            get
            {
                return this.Product != null ? this.Product.ProductCode : string.Empty;
            }
        }
        public string ProductName
        {
            get
            {
                return this.Product != null ? this.Product.ProductName : string.Empty;
            }
        }
        public string ProductDescription
        {
            get
            {
                return this.Product != null ? this.Product.ProductDescription : string.Empty;
            }
        }


        public void CopyDataFrom(EshoppgsoftwebProduct2Category src)
        {
            this.pk = src.pk;
            this.PkCategory = src.PkCategory;
            this.PkProduct = src.PkProduct;
            this.ProductOrder = src.ProductOrder;
        }

        public static Product2CategoryModel CreateCopyFrom(EshoppgsoftwebProduct2Category src)
        {
            Product2CategoryModel trg = new Product2CategoryModel();
            trg.CopyDataFrom(src);

            return trg;
        }
    }

    public class Product2CategoryItemsModel : _PagingModel
    {
        public Guid CategoryKey { get; set; }
        public List<Product2CategoryModel> Items { get; set; }

        public static Product2CategoryItemsModel CreateCopyFrom(Page<EshoppgsoftwebProduct2Category> src)
        {
            Product2CategoryItemsModel trg = new Product2CategoryItemsModel();
            trg.Items = new List<Product2CategoryModel>();
            foreach (EshoppgsoftwebProduct2Category srcItem in src.Items)
            {
                trg.Items.Add(Product2CategoryModel.CreateCopyFrom(srcItem));
            }
            trg.ItemsPerPage = (int)src.ItemsPerPage;
            trg.TotalItems = (int)src.TotalItems;

            return trg;
        }

        public static Product2CategoryItemsModel CreateCopyFrom(Guid categoryKey, Page<EshoppgsoftwebProduct> src, ProductModelDropDowns productDropDowns)
        {
            Product2CategoryItemsModel trg = new Product2CategoryItemsModel();
            trg.Items = new List<Product2CategoryModel>();
            int i = 0;
            foreach (EshoppgsoftwebProduct srcItem in src.Items)
            {
                trg.Items.Add(
                        new Product2CategoryModel()
                        {
                            PkCategory = categoryKey,
                            PkProduct = srcItem.pk,
                            ProductOrder = ++i,
                            Product = ProductModel.CreateCopyFrom(srcItem, productDropDowns)
                        }
                    );
            }
            trg.ItemsPerPage = (int)src.ItemsPerPage;
            trg.TotalItems = (int)src.TotalItems;

            return trg;
        }

        public void BindProducts(List<EshoppgsoftwebProduct> products, ProductModelDropDowns productDropDowns)
        {
            Hashtable htProduct = new Hashtable(products.Count + 1);
            foreach (EshoppgsoftwebProduct product in products)
            {
                htProduct.Add(product.pk, ProductModel.CreateCopyFrom(product, productDropDowns));
            }

            foreach (Product2CategoryModel item in this.Items)
            {
                if (htProduct.ContainsKey(item.PkProduct))
                {
                    item.Product = (ProductModel)htProduct[item.PkProduct];
                }
            }
        }
    }

    public class ProductInCategoryFilterModel : _BaseUserPropModel
    {
        public Guid PkCategory { get; set; }

        [Display(Name = "Kód produktu")]
        public string ProductCode { get; set; }
        [Display(Name = "Vyhľadávanie (ID, kód, názov, popis ...)")]
        public string SearchText { get; set; }


        public ProductInCategoryFilterModel()
        {
            this.PropId = ConfigurationUtil.PropId_ProductInCategoryFilterModel;
        }

        public static ProductInCategoryFilterModel CreateCopyFrom(EshoppgsoftwebUserProp src)
        {
            ProductInCategoryFilterModel trg = new ProductInCategoryFilterModel();
            if (src != null)
            {
                trg.CopyDataFrom(src);
            }
            trg.UpdateBeforeEdit();

            return trg;
        }

        public static EshoppgsoftwebUserProp CreateCopyFrom(ProductInCategoryFilterModel src)
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
            XmlElement mainNode = doc.CreateElement("ProductInCategoryFilterModel");
            mainNode.SetAttribute("version", "1.0");
            doc.AppendChild(mainNode);

            // Product code
            XmlParamSet.SaveItem(doc, mainNode, "ProductCode", this.ProductCode);
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

                string fullParent = "ProductInCategoryFilterModel";

                // Product code
                this.ProductCode = XmlParamSet.LoadItem(doc, fullParent, "ProductCode", string.Empty);
                // Search text
                this.SearchText = XmlParamSet.LoadItem(doc, fullParent, "SearchText", string.Empty);
            }
        }
    }
    public class ProductNotInCategoryFilterModel : _BaseUserPropModel
    {
        public Guid PkCategory { get; set; }

        [Display(Name = "Kód produktu")]
        public string ProductCode { get; set; }
        [Display(Name = "Vyhľadávanie (ID, kód, názov, popis ...)")]
        public string SearchText { get; set; }


        public ProductNotInCategoryFilterModel()
        {
            this.PropId = ConfigurationUtil.PropId_ProductNotInCategoryFilterModel;
        }

        public static ProductNotInCategoryFilterModel CreateCopyFrom(EshoppgsoftwebUserProp src)
        {
            ProductNotInCategoryFilterModel trg = new ProductNotInCategoryFilterModel();
            if (src != null)
            {
                trg.CopyDataFrom(src);
            }
            trg.UpdateBeforeEdit();

            return trg;
        }

        public static EshoppgsoftwebUserProp CreateCopyFrom(ProductNotInCategoryFilterModel src)
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
            XmlElement mainNode = doc.CreateElement("ProductNotInCategoryFilterModel");
            mainNode.SetAttribute("version", "1.0");
            doc.AppendChild(mainNode);

            // Product code
            XmlParamSet.SaveItem(doc, mainNode, "ProductCode", this.ProductCode);
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

                string fullParent = "ProductNotInCategoryFilterModel";

                // Product code
                this.ProductCode = XmlParamSet.LoadItem(doc, fullParent, "ProductCode", string.Empty);
                // Search text
                this.SearchText = XmlParamSet.LoadItem(doc, fullParent, "SearchText", string.Empty);
            }
        }
    }

    public class Product2CategoryEditModel
    {
        public CategoryTree AllCategories { get; private set; }
        public List<string> SelectedCategories { get; set; }
        Hashtable htSelected;
        Hashtable htChildSelected;

        public void LoadCategories(_BaseController ctrl, Guid productKey)
        {
            this.SelectedCategories = new List<string>();

            foreach (EshoppgsoftwebProduct2Category dataRec in new EshoppgsoftwebProduct2CategoryRepository().GetForProduct(productKey))
            {
                this.SelectedCategories.Add(dataRec.PkCategory.ToString());
            }

            UpdateSelectedCategories(ctrl);
        }

        public void UpdateSelectedCategories(_BaseController ctrl)
        {
            this.AllCategories = ctrl.GetCurrentEshopModel().CategoryTreeData;
            this.htSelected = new Hashtable();
            this.htChildSelected = new Hashtable();
            foreach (string key in this.SelectedCategories)
            {
                if (!this.htSelected.ContainsKey(key))
                {
                    this.htSelected.Add(key, key);
                    SetChildSelected(key);
                }
            }
        }
        void SetChildSelected(string childKey)
        {
            CategoryModel child = this.AllCategories.GetCategoryNode(new Guid(childKey));
            if (child != null && child.Parent != null)
            {
                string key = child.Parent.pk.ToString();
                if (!this.htChildSelected.ContainsKey(key))
                {
                    this.htChildSelected.Add(key, key);
                }
                SetChildSelected(key);
            }
        }

        public bool IsSelected(string key)
        {
            return this.htSelected.ContainsKey(key);
        }
        public bool IsChildSelected(string key)
        {
            return this.htChildSelected.ContainsKey(key);
        }
    }
}
