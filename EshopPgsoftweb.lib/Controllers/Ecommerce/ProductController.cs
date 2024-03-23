using eshoppgsoftweb.lib.Models;
using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using System;
using System.Collections;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class ProductController : _BaseController
    {
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(int page = 1, string sort = "ProductOrder", string sortDir = "DESC")
        {
            try
            {
                return GetRecordsView(page, sort, sortDir);
            }
            catch
            {
                ProductFilterModel filter = GetEshoppgsoftwebProductFilterForEdit();
                if (filter != null)
                {
                    filter.SearchText = string.Empty;
                    EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                    repository.Save(this.CurrentSessionId, ProductFilterModel.CreateCopyFrom(filter));
                }
                return GetRecordsView(page, sort, sortDir);
            }
        }
        ActionResult GetRecordsView(int page, string sort, string sortDir)
        {
            ProductFilterModel filter = GetEshoppgsoftwebProductFilterForEdit();

            EshoppgsoftwebProductRepository repository = new EshoppgsoftwebProductRepository();
            ProductPagingListModel model = ProductPagingListModel.CreateCopyFrom(
                repository.GetPage(page, _PagingModel.DefaultItemsPerPage, sort, sortDir,
                    new EshoppgsoftwebProductFilter()
                    {
                        ProductCode = filter.ProductCode,
                        SearchText = filter.SearchText
                    }),
                GetEshoppgsoftwebProductDropDowns()
                );

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord()
        {
            return View("EditRecord", GetEshoppgsoftwebProductForEdit());
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            EshoppgsoftwebProductRepository repository = new EshoppgsoftwebProductRepository();
            ProductModel model;
            if (string.IsNullOrEmpty(id))
            {
                model = GetEshoppgsoftwebProductForEdit();
            }
            else
            {
                model = ProductModel.CreateCopyFrom(repository.Get(new Guid(id)), GetEshoppgsoftwebProductDropDowns());
                model.ProductAttributes.pk = model.pk;
                model.ProductAttributes.Items = Product2AttributeModel.LoadItems(model.pk);
                model.ProductRelations.pk = model.pk;
                model.ProductRelations.Items = ProductRelationModel.LoadItems(model.pk);
                model.ProductCategories.LoadCategories(this, model.pk);
            }

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(ProductModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            model.ModelErrors.Clear();

            EshoppgsoftwebProductRepository repository = new EshoppgsoftwebProductRepository();
            // check product CODE
            EshoppgsoftwebProduct dupl = repository.GetForProductCode(model.ProductCode);
            if (dupl != null && dupl.pk != model.pk)
            {
                model.ModelErrors.Add("Zadaný kód produktu už je použitý pre iný produkt.");
            }
            // check product URL
            dupl = repository.GetForProductUrl(model.ProductUrl);
            if (dupl != null && dupl.pk != model.pk)
            {
                model.ModelErrors.Add("Zadané URL už je použité pre iný produkt.");
            }

            if (model.ModelErrors.Count == 0)
            {
                EshoppgsoftwebProduct dataRecord = ProductModel.CreateCopyFrom(model, GetEshoppgsoftwebProductDropDowns());
                if (repository.Save(dataRecord))
                {
                    model.pk = dataRecord.pk;
                }
                else
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
                if (model.ModelErrors.Count == 0)
                {
                    #region Product attributes
                    model.ProductAttributes.pk = dataRecord.pk; // set the current product key

                    EshoppgsoftwebProduct2AttributeRepository repAttr = new EshoppgsoftwebProduct2AttributeRepository();
                    if (!repAttr.DeleteForProduct(model.ProductAttributes.pk))
                    {
                        model.ModelErrors.Add("Nastala chyba pri zápise vlastností produktu systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                    }
                    else
                    {
                        if (model.ProductAttributes.Items != null)
                        {
                            foreach (Product2AttributeItem item in model.ProductAttributes.Items)
                            {
                                item.ProductKey = dataRecord.pk; // set the current product key
                                if (item.IsSelected)
                                {
                                    if (!repAttr.Insert(Product2AttributeItem.CreateCopyFrom(item)))
                                    {
                                        model.ModelErrors.Add("Nastala chyba pri zápise vlastností produktu systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                if (model.ModelErrors.Count == 0)
                {
                    #region Product relations
                    model.ProductRelations.pk = dataRecord.pk; // set the current product key

                    EshoppgsoftwebProductRelationRepository repRel = new EshoppgsoftwebProductRelationRepository();
                    if (!repRel.DeleteForProduct(model.ProductRelations.pk))
                    {
                        model.ModelErrors.Add("Nastala chyba pri zápise súvisiacich produktov do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                    }
                    else
                    {
                        if (model.ProductRelations.Items != null)
                        {
                            Hashtable htDupl = new Hashtable();
                            foreach (ProductRelationItem item in model.ProductRelations.Items)
                            {
                                if (htDupl.ContainsKey(item.PkProductRelated))
                                {
                                    continue;
                                }
                                htDupl.Add(item.PkProductRelated, item);
                                item.PkProductMain = dataRecord.pk; // set the current product key
                                item.PkProductRelated = item.PkProductRelated; // set the related product key
                                if (!repRel.Insert(ProductRelationItem.CreateCopyFrom(item)))
                                {
                                    model.ModelErrors.Add("Nastala chyba pri zápise súvisiacich produktov do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                                    break;
                                }
                            }
                        }
                    }
                    #endregion
                }
                if (model.ModelErrors.Count == 0)
                {
                    #region Product categories
                    Guid pkProduct = model.pk;
                    EshoppgsoftwebProduct2CategoryRepository repCat = new EshoppgsoftwebProduct2CategoryRepository();
                    if (model.ProductCategories.SelectedCategories != null && model.ProductCategories.SelectedCategories.Count > 0)
                    {
                        // Set product to selected categories
                        Hashtable htCatSaved = new Hashtable();
                        foreach (string categoryKey in model.ProductCategories.SelectedCategories)
                        {
                            if (htCatSaved.ContainsKey(categoryKey))
                            {
                                continue;
                            }
                            htCatSaved.Add(categoryKey, categoryKey);

                            Guid pkCategory = new Guid(categoryKey);
                            if (repCat.Get(pkCategory, pkProduct) == null)
                            {
                                // Insert product to category
                                EshoppgsoftwebProduct2Category item = new EshoppgsoftwebProduct2Category();
                                item.PkProduct = pkProduct;
                                item.PkCategory = pkCategory;
                                if (!repCat.Insert(item))
                                {
                                    model.ModelErrors.Add("Nastala chyba pri zápise kategórií produktu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                                    break;
                                }
                            }
                        }

                        // Delete product from not selected categories
                        foreach (EshoppgsoftwebProduct2Category item in repCat.GetForProduct(pkProduct))
                        {
                            if (!htCatSaved.ContainsKey(item.PkCategory.ToString()))
                            {
                                // Delete product from category
                                repCat.Delete(item);
                            }
                        }
                    }
                    else
                    {
                        // Remove product from all categories
                        if (!repCat.DeleteForProduct(model.pk))
                        {
                            model.ModelErrors.Add("Nastala chyba pri zápise kategórií produktu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                        }
                    }
                    #endregion
                }
            }

            if (model.ModelErrors.Count > 0)
            {
                if (model.ProductRelations.Items != null)
                {
                    model.ProductRelations.SetRelatedProducts();
                }
                return RedirectToCurrentUmbracoPageAfterSaveRecord(model);
            }

            ProductFilterModel filter = GetEshoppgsoftwebProductFilterForEdit();
            if (filter != null)
            {
                filter.SearchText = string.Empty;
                filter.ProductCode = string.Empty;
                new EshoppgsoftwebUserPropRepository().Save(this.CurrentSessionId, ProductFilterModel.CreateCopyFrom(filter));
            }


            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceProductsFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            EshoppgsoftwebProductRepository repository = new EshoppgsoftwebProductRepository();
            ProductModel model = string.IsNullOrEmpty(id) ? GetEshoppgsoftwebProductForEdit() : ProductModel.CreateCopyFrom(repository.Get(new Guid(id)), GetEshoppgsoftwebProductDropDowns());

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(ProductModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecord(model);
            }

            model.ModelErrors.Clear();

            if (model.ModelErrors.Count == 0)
            {
                EshoppgsoftwebProductRepository repository = new EshoppgsoftwebProductRepository();
                if (!repository.Delete(ProductModel.CreateCopyFrom(model, GetEshoppgsoftwebProductDropDowns())))
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (model.ModelErrors.Count > 0)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecord(model);
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceProductsFormId);
        }


        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveRecord(ProductModel rec = null)
        {
            SetEshoppgsoftwebProductForEdit(rec);
            return RedirectToCurrentUmbracoPage();
        }
        void SetEshoppgsoftwebProductForEdit(ProductModel rec = null)
        {
            TempData["EshoppgsoftwebProductForEdit"] = rec;
        }
        ProductModel GetEshoppgsoftwebProductForEdit()
        {
            ProductModel model = TempData["EshoppgsoftwebProductForEdit"] == null ? new ProductModel() : (ProductModel)TempData["EshoppgsoftwebProductForEdit"];
            if (model.ProductAttributes.Items == null)
            {
                model.ProductAttributes.pk = model.pk;
                model.ProductAttributes.Items = Product2AttributeModel.LoadItems(model.ProductAttributes.pk);
            }
            if (model.ProductRelations.Items == null)
            {
                model.ProductRelations.pk = model.pk;
                model.ProductRelations.Items = ProductRelationModel.LoadItems(model.ProductRelations.pk);
            }
            if (model.ProductCategories.SelectedCategories == null)
            {
                model.ProductCategories.LoadCategories(this, model.pk);
            }
            if (model.ProductCategories.AllCategories == null)
            {
                model.ProductCategories.UpdateSelectedCategories(this);
            }
            if (model.DropDowns == null)
            {
                model.DropDowns = GetEshoppgsoftwebProductDropDowns();
            }

            return model;
        }
        ProductModelDropDowns GetEshoppgsoftwebProductDropDowns()
        {
            return new ProductModelDropDowns();
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetFilter()
        {
            return View(GetEshoppgsoftwebProductFilterForEdit());
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveFilter(ProductFilterModel model)
        {
            model.ModelErrors.Clear();
            if (model.ModelErrors.Count == 0)
            {
                EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                if (!repository.Save(this.CurrentSessionId, ProductFilterModel.CreateCopyFrom(model)))
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (model.ModelErrors.Count > 0)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecordFilter(model);
            }

            return RedirectToCurrentUmbracoPageAfterSaveRecordFilter();
        }
        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveRecordFilter(ProductFilterModel rec = null)
        {
            SetEshoppgsoftwebProductFilterForEdit(rec);
            return RedirectToCurrentUmbracoPage();
        }
        void SetEshoppgsoftwebProductFilterForEdit(ProductFilterModel rec = null)
        {
            TempData["takfajnProductFilterForEdit"] = rec;
        }
        ProductFilterModel GetEshoppgsoftwebProductFilterForEdit()
        {
            if (TempData["takfajnProductFilterForEdit"] == null)
            {
                EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                TempData["takfajnProductFilterForEdit"] = ProductFilterModel.CreateCopyFrom(repository.Get(this.CurrentSessionId, ConfigurationUtil.PropId_ProductFilterModel));
            }

            return (ProductFilterModel)TempData["takfajnProductFilterForEdit"];
        }



        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditImages(string id)
        {
            ProductImagesModel model = ProductImagesModel.LoadModel(new Guid(id));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveImages(ProductImagesModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            model.Save();

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceProductsFormId);
        }
    }
}
