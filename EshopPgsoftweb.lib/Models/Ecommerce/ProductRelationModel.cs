using eshoppgsoftweb.lib.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class ProductRelationModel : _BaseModel
    {
        public List<ProductRelationItem> Items { get; set; }

        public static List<ProductRelationItem> LoadItems(Guid productKey)
        {
            List<ProductRelationItem> allItems = new List<ProductRelationItem>();

            // Load all relations
            foreach (EshoppgsoftwebProductRelation relation in new EshoppgsoftwebProductRelationRepository().GetForProduct(productKey))
            {
                ProductRelationItem item = new ProductRelationItem()
                {
                    PkProductMain = productKey,
                    PkProductRelated = relation.PkProductRelated,
                };

                allItems.Add(item);
            }

            // Set related products
            SetRelatedProducts(allItems);

            // Sort result list by product name
            allItems.Sort(new ProductRelationItemComparer());

            return allItems;
        }

        public void SetRelatedProducts()
        {
            SetRelatedProducts(this.Items);
        }

        public static void SetRelatedProducts(List<ProductRelationItem> itemList)
        {
            List<string> relatedProductsKeyList = new List<string>();
            Hashtable htRelated = new Hashtable();
            foreach (ProductRelationItem item in itemList)
            {
                htRelated.Add(item.PkProductRelated, item);
                relatedProductsKeyList.Add(item.PkProductRelated.ToString());
            }


            List<EshoppgsoftwebProduct> dataList = new EshoppgsoftwebProductRepository().GetPage(1, _PagingModel.AllItemsPerPage,
                filter: new EshoppgsoftwebProductFilter()
                {
                    ProductKeyList = relatedProductsKeyList
                }).Items;
            foreach (EshoppgsoftwebProduct relatedProduct in dataList)
            {
                if (htRelated.ContainsKey(relatedProduct.pk))
                {
                    ProductRelationItem item = (ProductRelationItem)htRelated[relatedProduct.pk];
                    item.RelatedProduct = relatedProduct;
                }
            }
        }
    }

    public class ProductRelationItem : _BaseModel
    {
        public Guid PkProductMain { get; set; }
        public Guid PkProductRelated { get; set; }
        public EshoppgsoftwebProduct RelatedProduct { get; set; }

        public ProductRelationItem()
        {
        }

        public void CopyDataFrom(EshoppgsoftwebProductRelation src)
        {
            this.pk = src.pk;
            this.PkProductMain = src.PkProductMain;
            this.PkProductRelated = src.PkProductRelated;
        }

        public void CopyDataTo(EshoppgsoftwebProductRelation trg)
        {
            trg.pk = this.pk;
            trg.PkProductMain = this.PkProductMain;
            trg.PkProductRelated = this.PkProductRelated;
        }

        public static ProductRelationItem CreateCopyFrom(EshoppgsoftwebProductRelation src)
        {
            ProductRelationItem trg = new ProductRelationItem();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static EshoppgsoftwebProductRelation CreateCopyFrom(ProductRelationItem src)
        {
            EshoppgsoftwebProductRelation trg = new EshoppgsoftwebProductRelation();
            src.CopyDataTo(trg);

            return trg;
        }
    }

    public class ProductRelationItemComparer : IComparer<ProductRelationItem>
    {
        public int Compare(ProductRelationItem x, ProductRelationItem y)
        {
            return string.Compare(x.RelatedProduct.ProductName, y.RelatedProduct.ProductName);
        }
    }
}
