using eshoppgsoftweb.lib.Models;
using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class QuoteApiController : _BaseApiController
    {
        public const string ProductToQuoteOk = "OK";
        public const string AddProductToQuoteError = "Vznikla chyba pri pridávaní produktu do košíka. Skontrolujte prosím stav košíka.";
        public const string SaveProductToQuoteError = "Vznikla chyba pri ukladaní produktu do košíka.";
        public const string RemoveProductToQuoteError = "Vznikla chyba pri odstraňovaní produktu z košíka.";
        public const string BasketInfoError = "Vznikla chyba pri čítaní obsahu košíka.";
        public const string BasketInfoEmpty = "EMPTY";
        public const string QuoteStateInfoError = "Vznikla chyba pri zmene stavu objednávky.";

        public Api_AddToBasketInfo AddProductToQuote(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string quoteId = items[0];
                string pkProduct = items[1];
                string memberId = items[2];
                decimal cnt = PriceUtil.NumberFromEditorString(items[3]);

                // Get quote
                QuoteRepository quoteRep = new QuoteRepository();
                Quote quote = quoteRep.GetForSession(quoteId);
                // Add product to quote
                EshoppgsoftwebProduct product = InsertProduct2QuoteModel.AddProductToQuote(quote.pk, new Guid(pkProduct), cnt);

                Api_AddToBasketInfo ret = new Api_AddToBasketInfo();
                ret.ProductName = product.ProductName;

                // Recalc quote discount
                QuoteModel.RecalcQuoteDiscountForSession(quoteId, memberId);
                // Reload quote and calculate free transport
                quote = quoteRep.GetForSession(quoteId);
                List<Product2Quote> quoteItems = new Product2QuoteRepository().GetQuoteItems(quote.pk);
                decimal freeTransportPrice = SysConstUtil.GetFreeTransportPrice(GetQuotePriceForFreeTransport(quoteItems));
                if (freeTransportPrice > 0M)
                {
                    ret.FreeTransport = string.Format("K DOPRAVE ZADARMO VÁM CHÝBA {0}", PriceUtil.GetPriceString(freeTransportPrice, trimDecZeros: true));
                }
                else
                {
                    ret.FreeTransport = "DOPRAVU MÁTE ZADARMO";
                }


                ret.Result = QuoteApiController.ProductToQuoteOk;
                return ret;
            }
            catch (Exception exc)
            {
                this.Logger.Error(typeof(QuoteApiController), "AddProductToQuote error", exc);
                return new Api_AddToBasketInfo()
                {
                    Result = string.Format("{0}. {1}", QuoteApiController.AddProductToQuoteError, exc.ToString())
                };
            }
        }

        public Api_BasketInfo SaveProductToQuote(string id)
        {
            try
            {
                string[] items = id.Replace("|", ";").Split(';');
                string quoteId = items[0];
                string pkProduct = items[1];
                decimal cnt = PriceUtil.NumberFromEditorString(items[2]);

                // Get quote
                QuoteRepository quoteRep = new QuoteRepository();
                Quote quote = quoteRep.GetForSession(quoteId);
                // Save product to quote
                Product2QuoteRepository rep = new Product2QuoteRepository();
                Product2Quote dataRec = rep.Get(quote.pk, new Guid(pkProduct));
                if (dataRec != null)
                {
                    dataRec.ItemPcs = cnt;
                    rep.Save(dataRec);
                }

                Product2QuoteModel model = Product2QuoteModel.CreateCopyFrom(dataRec);
                Api_BasketInfo ret = new Api_BasketInfo();
                ret.Set(0, model.TotalPriceNoVat, model.TotalPriceWithVat);
                ret.Result = QuoteApiController.ProductToQuoteOk;

                return ret;
            }
            catch (Exception exc)
            {
                this.Logger.Error(typeof(QuoteApiController), "SaveProductToQuote error", exc);
                return new Api_BasketInfo()
                {
                    Result = string.Format("{0}. {1}", QuoteApiController.SaveProductToQuoteError, exc.ToString())
                };
            }
        }

        public Api_BasketInfo RemoveProductToQuote(string id)
        {
            try
            {
                string[] items = id.Replace("|", ";").Split(';');
                string quoteId = items[0];
                string pkProduct = items[1];

                // Get quote
                QuoteRepository quoteRep = new QuoteRepository();
                Quote quote = quoteRep.GetForSession(quoteId);
                // Delete product from quote
                Product2QuoteRepository rep = new Product2QuoteRepository();
                Product2Quote dataRec = rep.Get(quote.pk, new Guid(pkProduct));
                if (dataRec != null)
                {
                    rep.Delete(dataRec);
                }
                return BasketInfo(quoteId);
            }
            catch (Exception exc)
            {
                this.Logger.Error(typeof(QuoteApiController), "RemoveProductToQuote error", exc);
                return new Api_BasketInfo()
                {
                    Result = string.Format("{0}. {1}", QuoteApiController.RemoveProductToQuoteError, exc.ToString())
                };
            }
        }

        public Api_BasketInfo AddDiscountCouponToQuote(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string quoteId = items[0];
                string couponCode = items[1];

                if (QuoteModel.UseQuoteDiscountCouponForSession(quoteId, couponCode))
                {
                    return new Api_BasketInfo()
                    {
                        Result = QuoteApiController.ProductToQuoteOk
                    };
                }
                else
                {
                    return new Api_BasketInfo()
                    {
                        Result = "Zadaný zľavový kupón nie je možné uplatniť."
                    };
                }
            }
            catch (Exception exc)
            {
                this.Logger.Error(typeof(QuoteApiController), "AddDiscountCouponToQuote error", exc);
                return new Api_BasketInfo()
                {
                    Result = string.Format("{0}. {1}", QuoteApiController.AddProductToQuoteError, exc.ToString())
                };
            }
        }
        public Api_BasketInfo RemoveDiscountCouponToQuote(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string quoteId = items[0];

                QuoteRepository quoteRep = new QuoteRepository();
                Quote quote = quoteRep.GetForSession(quoteId);
                new Product2QuoteRepository().DeleteNonProductItem(quote.pk, ConfigurationUtil.Ecommerce_Quote_DiscountItemCode);

                return new Api_BasketInfo()
                {
                    Result = QuoteApiController.ProductToQuoteOk
                };
            }
            catch (Exception exc)
            {
                this.Logger.Error(typeof(QuoteApiController), "RemoveDiscountCouponToQuote error", exc);
                return new Api_BasketInfo()
                {
                    Result = string.Format("Vznikla chyba pri odoberaní zľavy z košíka. {0}", exc.ToString())
                };
            }
        }


        public Api_BasketInfo ChangeQuoteTransportAndPayment(string id)
        {
            try
            {
                string[] items = id.Replace("|", ";").Split(';');
                string quoteId = items[0];
                string pkTransport = items[1];
                string pkPayment = items[2];

                // Load quote
                QuoteModel quote = QuoteModel.CreateCopyFrom(new QuoteRepository().Get(new Guid(quoteId)));
                quote.LoadProductItems(new ProductModelDropDowns(), clearNonProductItems: false);

                // Update transport and payment
                quote.UpdateTransportAndPayment(new Guid(pkTransport), new Guid(pkPayment));

                return new Api_BasketInfo()
                {
                    Result = QuoteApiController.ProductToQuoteOk
                };
            }
            catch (Exception exc)
            {
                this.Logger.Error(typeof(QuoteApiController), "RemoveProductToQuote error", exc);
                return new Api_BasketInfo()
                {
                    Result = string.Format("{0}. {1}", QuoteApiController.RemoveProductToQuoteError, exc.ToString())
                };
            }
        }

        public Api_PriceInfo PriceInfo(string id)
        {
            try
            {
                string[] items = id.Split('|');
                int cnt;
                decimal price, totalPrice = 0;

                for (int i = 1; i < items.Length; i = i + 2)
                {
                    cnt = int.Parse(items[i]);
                    price = PriceUtil.NumberFromEditorString(items[i + 1]);

                    totalPrice += ((decimal)cnt * price);
                }

                decimal freeTransportPrice = SysConstUtil.GetFreeTransportPrice(totalPrice);

                return new Api_PriceInfo()
                {
                    Result = "OK",
                    TotalPrice = PriceUtil.GetPriceString(totalPrice),
                    FreeTransportPrice = freeTransportPrice > 0 ? PriceUtil.GetPriceString(freeTransportPrice, trimDecZeros: true) : "0",
                    FreeTransportPriceStatus = SysConstUtil.IsFreeTransportPriceAvailable ? "ENABLED" : "DISABLED",
                };
            }
            catch (Exception exc)
            {
                this.Logger.Error(typeof(QuoteApiController), "PriceInfo error", exc);
                return new Api_PriceInfo() { Result = "ERR" };
            }
        }

        public Api_BasketInfo BasketInfo(string id)
        {
            Api_BasketInfo ret = new Api_BasketInfo();
            int count = 0;
            decimal priceNoVat = 0;
            decimal priceWithVat = 0;
            try
            {
                QuoteRepository quoteRep = new QuoteRepository();
                Guid quoteKey = quoteRep.GetQuoteIdForSessionId(id);
                if (quoteKey != null && quoteKey != Guid.Empty)
                {
                    // Don't show price in basket info for naplnspajzu.sk 
                    //Quote quote = quoteRep.Get(quoteKey);
                    //priceNoVat = quote.QuotePriceNoVat;
                    //priceWithVat = quote.QuotePriceWithVat;
                    count = new Product2QuoteRepository().GetQuoteItemsCnt(quoteKey);
                }

                ret.Set(count, count > 0 ? priceNoVat : 0, count > 0 ? priceWithVat : 0);
                ret.Result = QuoteApiController.ProductToQuoteOk;
            }
            catch (Exception exc)
            {
                this.Logger.Error(typeof(QuoteApiController), "BasketInfo error", exc);
                ret.Result = string.Format("{0}. {1}", QuoteApiController.BasketInfoError, exc.ToString());
            }

            return ret;
        }

        public Api_BasketQuickViewInfo BasketQuickViewInfo(string id)
        {
            try
            {
                Api_BasketQuickViewInfo ret = null;
                QuoteRepository quoteRep = new QuoteRepository();
                Guid quoteKey = quoteRep.GetQuoteIdForSessionId(id);
                if (quoteKey != null && quoteKey != Guid.Empty)
                {
                    Quote quote = quoteRep.Get(quoteKey);
                    Product2QuoteRepository p2qRep = new Product2QuoteRepository();
                    List<Product2Quote> quoteItems = p2qRep.GetQuoteItems(quote.pk);
                    if (quoteItems.Count > 0)
                    {
                        // Create quote info
                        ret = new Api_BasketQuickViewInfo();
                        // Quote price
                        ret.TotalPriceNoVat = PriceUtil.GetPriceString(quote.QuotePriceNoVat);
                        ret.TotalPriceWithVat = PriceUtil.GetPriceString(quote.QuotePriceWithVat);
                        ret.TotalPriceWithVatTrimZeros = PriceUtil.GetPriceString(quote.QuotePriceWithVat, trimDecZeros: true);
                        decimal freeTransportPrice = SysConstUtil.GetFreeTransportPrice(GetQuotePriceForFreeTransport(quoteItems));
                        if (freeTransportPrice > 0M)
                        {
                            ret.FreeTransport = string.Format("K DOPRAVE ZADARMO VÁM CHÝBA {0}", PriceUtil.GetPriceString(freeTransportPrice, trimDecZeros: true));
                        }
                        else
                        {
                            ret.FreeTransport = "DOPRAVU MÁTE ZADARMO";
                        }
                        // Quote items
                        ret.QuoteItems = new List<Api_BasketQuickViewProductInfo>();
                        Hashtable htItems = new Hashtable();
                        foreach (Product2Quote item in quoteItems)
                        {
                            Api_BasketQuickViewProductInfo info = new Api_BasketQuickViewProductInfo()
                            {
                                Cnt = item.ItemPcs.ToString(),
                                Name = item.ItemName,
                                PriceNoVat = PriceUtil.GetPriceString(item.UnitPriceNoVat),
                                PriceWithVat = PriceUtil.GetPriceString(item.UnitPriceWithVat),
                            };
                            ret.QuoteItems.Add(info);
                            if (!htItems.ContainsKey(item.PkProduct))
                            {
                                htItems.Add(item.PkProduct, info);
                            }
                        }
                        // Images
                        EshoppgsoftwebProductRepository prodRep = new EshoppgsoftwebProductRepository();
                        List<EshoppgsoftwebProduct> productList = prodRep.GetPageForQuote(1, _PagingModel.AllItemsPerPage, quote.pk).Items;
                        foreach (EshoppgsoftwebProduct product in productList)
                        {
                            if (!string.IsNullOrEmpty(product.ProductImg) && htItems.ContainsKey(product.pk))
                            {
                                ((Api_BasketQuickViewProductInfo)htItems[product.pk]).Img = product.ProductImg;
                            }
                        }
                        // Final result
                        ret.Result = QuoteApiController.ProductToQuoteOk;
                    }
                }

                if (ret == null)
                {
                    return new Api_BasketQuickViewInfo()
                    {
                        Result = QuoteApiController.BasketInfoEmpty
                    };
                }

                return ret;
            }
            catch (Exception exc)
            {
                this.Logger.Error(typeof(QuoteApiController), "Api_BasketQuickViewInfo error", exc);
                return new Api_BasketQuickViewInfo()
                {
                    Result = string.Format("{0}. {1}", QuoteApiController.BasketInfoError, exc.ToString())
                };
            }
        }

        decimal GetQuotePriceForFreeTransport(List<Product2Quote> quoteItems)
        {
            decimal totalPrice = 0M;

            foreach (Product2Quote item in quoteItems)
            {
                if (item.ItemCode != ConfigurationUtil.Ecommerce_Quote_PaymentItemCode && item.ItemCode != ConfigurationUtil.Ecommerce_Quote_TransportItemCode)
                {
                    totalPrice += item.TotalItemPriceWithVat();
                }
            }

            return totalPrice;
        }

        public Api_QuoteState ChangeQuoteState(string id)
        {
            try
            {
                Api_QuoteState ret = new Api_QuoteState();

                string[] items = id.Split('|');
                string quoteId = items[0];
                string stateId = items[1];

                QuoteState state = new QuoteStateRepository().Get(new Guid(stateId));
                QuoteRepository rep = new QuoteRepository();
                Quote quote = rep.Get(new Guid(quoteId));
                quote.QuoteState = state.Title;
                rep.Save(quote);

                ret.StateKey = state.pk.ToString();
                ret.StateName = state.Title;
                ret.Result = QuoteApiController.ProductToQuoteOk;
                return ret;
            }
            catch (Exception exc)
            {
                this.Logger.Error(typeof(QuoteApiController), "ChangeQuoteState error", exc);
                return new Api_QuoteState()
                {
                    Result = string.Format("{0}. {1}", QuoteApiController.QuoteStateInfoError, exc.ToString())
                };
            }
        }

        public Api_BasketRefreshInfo BasketRefreshInfo(string id)
        {
            Api_BasketRefreshInfo ret = new Api_BasketRefreshInfo();
            try
            {
                QuoteRepository quoteRep = new QuoteRepository();
                Guid quoteKey = quoteRep.GetQuoteIdForSessionId(id);
                if (quoteKey != null && quoteKey != Guid.Empty)
                {
                    QuoteModel quote = QuoteModel.CreateCopyFrom(quoteRep.Get(quoteKey));
                    quote.LoadRefreshedProductItems();

                    ret.TotalPriceNoVat = quote.TotalPriceNoVatWithCurrency;
                    ret.TotalPriceWithVat = quote.TotalPriceWithVatWithCurrency;

                    // Product info
                    int pcsCnt = 0;
                    ret.Products = new List<Api_BasketRefreshInfo_Product>();
                    foreach (Product2QuoteModel item in quote.Items)
                    {
                        if (!item.IsProductItem)
                        {
                            continue;
                        }
                        Api_BasketRefreshInfo_Product product = new Api_BasketRefreshInfo_Product();
                        product.Id = string.Format("pc-{0}", item.PkProduct.ToString());
                        product.Cnt = item.ItemPcs.ToString();
                        product.TotalPriceNoVat = item.TotalPriceWithCurrencyNoVat;
                        product.TotalPriceWithVat = item.TotalPriceWithCurrencyWithVat;
                        ret.Products.Add(product);

                        pcsCnt++;
                    }
                    ret.PcsCnt = pcsCnt.ToString();

                    // Transport info
                    ret.Transports = new List<Api_BasketRefreshInfo_Transport>();
                    TransportTypeListModel transportDataList = TransportTypeListModel.CreateCopyFrom(new TransportTypeRepository().GetRecordsForBasket(), quote.QuoteTotalWeight);
                    foreach (TransportTypeModel transportDataRec in transportDataList.Items)
                    {
                        Api_BasketRefreshInfo_Transport transport = new Api_BasketRefreshInfo_Transport();
                        transport.Id = transportDataRec.pk.ToString();
                        transport.Price = string.Format("+ {0}", transportDataRec.PriceWithCurrency);
                        if (quote.IsFreeTransportPrice)
                        {
                            transport.IsFree = true;
                        }
                        ret.Transports.Add(transport);
                    }

                    // Overall result
                    ret.Result = QuoteApiController.ProductToQuoteOk;
                }
            }
            catch (Exception exc)
            {
                this.Logger.Error(typeof(QuoteApiController), "BasketRefreshInfo error", exc);
                ret.Result = string.Format("{0}. {1}", QuoteApiController.BasketInfoError, exc.ToString());
            }

            return ret;
        }
    }

    public class Api_AddToBasketInfo
    {
        public string Result { get; set; }

        public string ProductName { get; set; }
        public string FreeTransport { get; set; }
    }


    public class Api_PriceInfo
    {
        public string Result { get; set; }

        public string TotalPrice { get; set; }
        public string FreeTransportPrice { get; set; }
        public string FreeTransportPriceStatus { get; set; }
    }


    public class Api_BasketInfo
    {
        public string Result { get; set; }

        public string TotalPriceNoVat { get; set; }
        public string TotalPriceWithVat { get; set; }
        public string TotalPriceWithVatTrimZeros { get; set; }
        public string ItemCnt { get; set; }

        public Api_BasketInfo()
        {
            Set(0, 0, 0);
        }

        public void Set(int itemCnt, decimal priceNoVat, decimal priceWithVat)
        {
            this.ItemCnt = itemCnt.ToString();
            this.TotalPriceNoVat = PriceUtil.GetPriceString(priceNoVat);
            this.TotalPriceWithVat = PriceUtil.GetPriceString(priceWithVat);
            this.TotalPriceWithVatTrimZeros = PriceUtil.GetPriceString(priceWithVat, trimDecZeros: true);
        }
    }

    public class Api_BasketQuickViewInfo
    {
        public string Result { get; set; }

        public string TotalPriceNoVat { get; set; }
        public string TotalPriceWithVat { get; set; }
        public string TotalPriceWithVatTrimZeros { get; set; }
        public string FreeTransport { get; set; }
        public List<Api_BasketQuickViewProductInfo> QuoteItems { get; set; }
    }
    public class Api_BasketQuickViewProductInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Img { get; set; }
        public string PriceNoVat { get; set; }
        public string PriceWithVat { get; set; }
        public string Cnt { get; set; }
    }

    public class Api_QuoteState
    {
        public string Result { get; set; }

        public string StateKey { get; set; }
        public string StateName { get; set; }
    }

    public class Api_BasketRefreshInfo
    {
        public string Result { get; set; }

        public string PcsCnt { get; set; }
        public string TotalPriceNoVat { get; set; }
        public string TotalPriceWithVat { get; set; }

        public List<Api_BasketRefreshInfo_Product> Products { get; set; }
        public List<Api_BasketRefreshInfo_Transport> Transports { get; set; }
    }
    public class Api_BasketRefreshInfo_Product
    {
        public string Id { get; set; }
        public string Cnt { get; set; }
        public string TotalPriceNoVat { get; set; }
        public string TotalPriceWithVat { get; set; }
    }
    public class Api_BasketRefreshInfo_Transport
    {
        public string Id { get; set; }
        public string Price { get; set; }
        public bool IsFree { get; set; }
    }
}
