using System.Configuration;

namespace eshoppgsoftweb.lib.Util
{
    public abstract class ConfigurationUtil
    {
        public const string Ecommerce_Document_Contract_1 = "Platba";
        public const string Ecommerce_Document_Contract_2 = "Platba";
        public const string GetCfgValue = "Platba";
        


        public const string Ecommerce_Quote_TransportItemCode = "DOPRAVA";
        public const string Ecommerce_Quote_PaymentItemCode = "PLATBA";
        public const string Ecommerce_Quote_DiscountItemCode = "ZĽAVA";

        public const string Ecommerce_Quote_InfMsgId = "quote-info-msg";
        public const string Ecommerce_Quote_ModalMsgId = "quote-modal-msg";

        public const string Ecommerce_Quote_InitialState = "eshoppgsoftweb.Ecommerce.Quote.InitialState";
        public const string Ecommerce_Quote_PaidPriceState = "eshoppgsoftweb.Ecommerce.Quote.PaidPriceState";


        public const string EcommerceAfterLoginFormId = "eshoppgsoftweb.Ecommerce.AfterLoginFormId";
        public const string EcommerceRegistrationOkFormId = "eshoppgsoftweb.Ecommerce.RegistrationOkFormId";
        public const string EcommerceMembersFormId = "eshoppgsoftweb.Ecommerce.MembersFormId";
        public const string EcommerceCustomersFormId = "eshoppgsoftweb.Ecommerce.CustomersFormId";
        public const string EcommerceCountriesFormId = "eshoppgsoftweb.Ecommerce.CountriesFormId";
        public const string EcommerceProducersFormId = "eshoppgsoftweb.Ecommerce.ProducersFormId";
        public const string EcommerceAvailabilitiesFormId = "eshoppgsoftweb.Ecommerce.AvailabilitiesFormId";
        public const string EcommerceTransportTypesFormId = "eshoppgsoftweb.Ecommerce.TransportTypesFormId";
        public const string EcommercePaymentTypesFormId = "eshoppgsoftweb.Ecommerce.PaymentTypesFormId";
        public const string EcommerceSimpleStringsFormId = "eshoppgsoftweb.Ecommerce.SimpleStringsFormId";
        public const string EcommercePaymentStatesFormId = "eshoppgsoftweb.Ecommerce.PaymentStatesFormId";
        public const string EcommerceQuoteStatesFormId = "eshoppgsoftweb.Ecommerce.QuoteStatesFormId";
        public const string EcommerceCategoriesFormId = "eshoppgsoftweb.Ecommerce.CategoriesFormId";
        public const string EcommerceProductAttributesFormId = "eshoppgsoftweb.Ecommerce.ProductAttributesFormId";
        public const string EcommerceProductsFormId = "eshoppgsoftweb.Ecommerce.ProductsFormId";
        public const string EcommerceProductPricesFormId = "eshoppgsoftweb.Ecommerce.ProductPricesFormId";
        public const string EcommerceQuotesFormId = "eshoppgsoftweb.Ecommerce.QuotesFormId";
        public const string EcommerceQuotesEditFormId = "eshoppgsoftweb.Ecommerce.QuotesEditFormId";

        public const string Ecommerce_ProductPublic_DetailPageId = "eshoppgsoftweb.Ecommerce.ProductPublic_DetailPageId";
        public const string Ecommerce_ProductPublic_CategoryPageId = "eshoppgsoftweb.Ecommerce.ProductPublic_CategoryPageId";

        public const string Ecommerce_Basket_DeliveryDataPageId = "eshoppgsoftweb.Ecommerce.Basket_DeliveryDataPageId";
        public const string Ecommerce_Basket_ReviewAndSendPageId = "eshoppgsoftweb.Ecommerce.Basket_ReviewAndSendPageId";
        public const string Ecommerce_Basket_FinishedPageId = "eshoppgsoftweb.Ecommerce.Basket_FinishedPageId";

        public const string PropId_CustomerFilterModel = "PropId_CustomerFilterModel";
        public const string PropId_ProducerFilterModel = "PropId_ProducerFilterModel";
        public const string PropId_ProductFilterModel = "PropId_ProductFilterModel";
        public const string PropId_ProductInCategoryFilterModel = "PropId_ProductInCategoryFilterModel";
        public const string PropId_ProductNotInCategoryFilterModel = "PropId_ProductNotInCategoryFilterModel";
        public const string PropId_CategoryPublicFilterModel_PageSize = "PropId_CategoryPublicFilterModel_PageSize";
        public const string PropId_CategoryPublicFilterModel_ProductView = "PropId_CategoryPublicFilterModel_ProductView";
        public const string PropId_CategoryPublicFilterModel_ProductSort = "PropId_CategoryPublicFilterModel_ProductSort";
        public const string PropId_CategoryPublicFilterModel_CurrentCategory = "PropId_CategoryPublicFilterModel_CurrentCategory";
        public const string PropId_CategoryPublicFilterModel_Producer = "PropId_CategoryPublicFilterModel_Producer";
        public const string PropId_CategoryPublicFilterModel_ProductAttribute = "PropId_CategoryPublicFilterModel_ProductAttribute";
        public const string PropId_ProductAttributeFilterModel = "PropId_ProductAttributeFilterModel";
        public const string PropId_QuoteListFilterModel = "PropId_QuoteListFilterModel";

        public static string InitialQuoteState()
        {
            return ConfigurationManager.AppSettings[ConfigurationUtil.Ecommerce_Quote_InitialState];
        }
        public static string PaiedQuotePriceState()
        {
            return ConfigurationManager.AppSettings[ConfigurationUtil.Ecommerce_Quote_PaidPriceState];
        }

        public static int GetPageId(string pageKey)
        {
            return int.Parse(ConfigurationManager.AppSettings[pageKey]);
        }

        public static string EshopRootUrl
        {
            get
            {
                return "/eshop/kategoria/-vsetko-";
            }
        }
        public static string QuoteViewUrl
        {
            get
            {
                return "/e-shop/detail-objednavky";
            }
        }
    }
}
