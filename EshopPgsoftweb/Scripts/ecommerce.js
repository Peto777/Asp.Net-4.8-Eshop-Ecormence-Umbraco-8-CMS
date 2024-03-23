$(document).ready(function () {
    ecClearSessionData();
    ecInitDeliveryAddress();
    ecInitCompanyData();

    // Ecommerce category detail product view type change
    $('.eccd-pvt').change(function () {
        ecCategoryPublicFilterModel_ProductViewType(this);
    });
    // Ecommerce category detail product sort type change
    $('.eccd-pst').change(function () {
        ecCategoryPublicFilterModel_ProductSortType(this);
    });
    // Ecommerce category detail product producer selection change
    $('.eccd-prodsel').change(function () {
        ecCategoryPublicFilterModel_ProducerIsSelected(this);
    });
    // Ecommerce category detail product attribute selection change
    $('.eccd-attrsel').change(function () {
        ecCategoryPublicFilterModel_ProductAttributeIsSelected(this);
    });
    // Price change
    $(".calc-vat").on("keyup", function () {
        ecCalculatePriceWithoutVat(this);
    });
    $(".calc-vatrate").on("keyup", function () {
        $(".calc-vat").each(function () {
            ecCalculatePriceWithoutVat(this);
        })
    });
    // Pcs change
    ecInitToBasketPcsChange();
    $(".basket-product-item .product-pcs .inc").click(function (evt) {
        evt.preventDefault();
        ecSetInBasketPcs(this, $(this).data('cnt').toString());
    });
    $(".basket-product-item .product-pcs .dec").click(function (evt) {
        evt.preventDefault();
        ecSetInBasketPcs(this, $(this).data('cnt').toString());
    });
    // Drop related product
    $(".related-product a.drop-relation-btn").click(function (evt) {
        evt.preventDefault();
        ecDropRelatedProduct(this);
    });
    // Add related product
    // https://goodies.pixabay.com/jquery/auto-complete/demo.html
    ecRelatedProductAutocomplete('input[name="relatedProductSearch"]', '#spinRelatedProductSearch');
    // Search product for price edit
    // https://goodies.pixabay.com/jquery/auto-complete/demo.html
    ecProductPriceAutocomplete('input[name="productPriceSearch"]', '#spinProductPriceSearch', 'changeProductPriceLink');
    // Add product to existing quote
    // https://goodies.pixabay.com/jquery/auto-complete/demo.html
    ecQuoteProductAutocomplete('input[name="quoteProductSearch"]', '#spinQuoteProductSearch');
    // Product category change
    $(".pct-cb").change(function () {
        acProductToCategoryChange(this);
    });
    // Category tree
    $(".category-tree .fa").click(function (evt) {
        evt.preventDefault();
        ecCategoryTreeClick(this);
    });
});

function ecClearSessionData() {
    $.ajax('/Umbraco/Ecommerce/EcommerceApi/ClearSessionData',
        {
            type: 'POST',
            success: function (data) {
                //alert(data);
            }
        });
}
function ecInitDeliveryAddress() {
    if ($(".cb-delivery-address").is(":checked")) {
        $('.delivery-address').removeClass('hidden');
    }
    $(".cb-delivery-address").change(function () {
        if (this.checked) {
            $('.delivery-address').removeClass('hidden');
        }
        else {
            $('.delivery-address').addClass('hidden');
        }
    });
}
function ecInitCompanyData() {
    if ($(".cb-company-data").is(":checked")) {
        $('.company-data').removeClass('hidden');
    }
    $(".cb-company-data").change(function () {
        if (this.checked) {
            $('.company-data').removeClass('hidden');
        }
        else {
            $('.company-data').addClass('hidden');
        }
    });
}
function ecInitToBasketPcsChange() {
    $(".to-basket-pcs .inc").off('click');
    $(".to-basket-pcs .inc").on('click', function (evt) {
        evt.preventDefault();
        ecSetToBasketPcs(this, $(this).data('cnt').toString());
    });
    $(".to-basket-pcs .dec").off('click');
    $(".to-basket-pcs .dec").on('click',function (evt) {
        evt.preventDefault();
        ecSetToBasketPcs(this, $(this).data('cnt').toString());
    });
}



/*
 * Product2Quote
 */
function ecSetToBasketPcs(el, pcs) {
    if (pcs.search(',') > 0) {
        var target = $('#' + $(el).data('target'));
        var addpcs = ec100IntFromPcs(pcs);
        var oldpcs = ec100IntFromPcs(target.html());
        var newpcs = oldpcs + addpcs;
        if (newpcs > 0) {
            target.html(ec100IntToPcs(newpcs));
        }
    }
    else {
        var target = $('#' + $(el).data('target'));
        var addpcs = parseInt(pcs);
        var oldpcs = parseInt(target.html());

        if (addpcs > 0 || oldpcs > 1) {
            var newpcs = oldpcs + addpcs;
            target.html(newpcs);
        }
    }
}
function ec100IntFromPcs(pcs) {
    return parseInt(pcs.replace(',', ''));
}
function ec100IntToPcs(num) {
    var str = num.toString();
    if (num < 100) {
        str = '0' + str;
    }
    if (num < 10) {
        str = '0' + str;
    }

    return str.substr(0, str.length - 2) + ',' + str.substr(str.length - 2, 2);
}
function ecSetInBasketPcs(el, pcs) {
    ecBeforeReloadPage();
    ecSetToBasketPcs(el, pcs);
    var target = $('#' + $(el).data('target'));
    var quoteId = target.data('quote');
    var productId = target.attr('id');
    var infoId = target.data('info');
    ecSaveProduct2Quote(quoteId, productId, infoId);
}

function ecAddProduct2Quote(quoteId, valueId, productId, memberId, infoId, modalId) {
    var quoteVal = $('#' + valueId).html();
    //var cnt = parseInt(quoteVal) || 0;

    //if (cnt < 1) {
    //    ecMessageWarning(infoId, "Produkt nebolo možné pridať, pretože ste zadali nesprávne množstvo pre pridanie do košíka.");
    //}
    //else {
    //    ecAddProductCnt2Quote(quoteId, productId, memberId, infoId, modalId, cnt);
    //}
    ecAddProductCnt2Quote(quoteId, productId, memberId, infoId, modalId, quoteVal);
}
function ecAddProductCnt2Quote(quoteId, productId, memberId, infoId, modalId, cnt) {
    var param = quoteId + '|' + productId + '|' + memberId + '|' + cnt;
    $.ajax('/Umbraco/Ecommerce/QuoteApi/AddProductToQuote/?id=' + param, {
        type: 'POST',
        success: function (data) {
            if (data.Result == "OK") {
                ecAfter_AddProductCnt2Quote(quoteId, memberId, modalId, data);
            }
            else {
                ecMessageError(infoId, data);
            }
        },
        error: function () {
            ecMessageError(infoId, "Vznikla chyba pri pridávaní produktu do košíka. Skontrolujte prosím stav košíka.");
        }
    });
}
function ecAddProductContainers2Quote(infoId, modalId) {
    $('#' + modalId + ' .container-detail .cd-data').each(function () {
        var containerData = $(this);
        var quoteId = containerData.data('quoteid');
        var productId = containerData.data('productid');
        var memberId = containerData.data('memberid');
        var cnt = containerData.data('cnt');

        ecAddProductCnt2Quote(quoteId, productId, memberId, infoId, modalId, cnt);
    });
}
function ecAfter_AddProductCnt2Quote(quoteId, memberId, modalId, data) {
    ecModalAddToBasketShow(quoteId, memberId, modalId, data);
    ecUpdateQuoteCounter(quoteId, memberId);
}
function ecSaveProduct2Quote(quoteId, productId, infoId) {
    var quoteVal = $('#' + productId).html();
    var param = quoteId + '|' + productId + '|' + quoteVal;
    $.ajax('/Umbraco/Ecommerce/QuoteApi/SaveProductToQuote/?id=' + param, {
        type: 'POST',
        success: function (data) {
            if (data.Result == "OK") {
                ecReloadPage();
            }
            else {
                ecMessageError(infoId, data.Result);
            }
        },
        error: function () {
            ecMessageError(infoId, "Vznikla chyba pri ukladaní produktu do košíka.");
        }
    });
}
function ecRemoveProduct2Quote(quoteId, productId, infoId) {
    var param = quoteId + '|' + productId;
    $.ajax('/Umbraco/Ecommerce/QuoteApi/RemoveProductToQuote/?id=' + param, {
        type: 'POST',
        success: function (data) {
            if (data.Result == "OK") {
                ecRemoveElement("pc-" + productId);
                ecBeforeReloadPage();
                ecReloadPage();
            }
            else {
                ecMessageError(infoId, data.Result);
            }
        },
        error: function () {
            ecMessageError(infoId, "Vznikla chyba pri odstraňovaní produktu z košíka.");
        }
    });
}
function ecRemoveElement(id) {
    $('#' + id).remove();
}
function ecUpdateQuoteCounter(quoteId, memberId) {
    var param = quoteId;
    $.ajax('/Umbraco/Ecommerce/QuoteApi/BasketInfo/?id=' + param, {
        type: 'POST',
        success: function (data) {
            if (data.Result == "OK") {
                ecSetQuoteCounterValues(data);
                ecUpdateBasketQuickView(quoteId, memberId);
            }
        },
        error: function () {
        }
    });
}
function ecSetQuoteCounterValues(data) {
    //$('.header-basket-info .basket-price').html(data.TotalPriceWithVatTrimZeros);
    $('.header-basket-pcs').html(data.ItemCnt);
    //$('.basket-total-price.with-vat .price.autoupdate').html(data.TotalPriceWithVat);
    //$('.basket-total-price.no-vat .price.autoupdate').html(data.TotalPriceNoVat);
}
function ecUpdateBasketQuickView(quoteId, memberId) {
    //$('.basket-qv .loading').show();
    //$('.basket-qv .empty').hide();
    //$('.basket-qv .detail').hide();
    //var param = quoteId + "|" + memberId;
    //$.ajax('/Umbraco/Ecommerce/QuoteApi/BasketQuickViewInfo/?id=' + param, {
    //    type: 'POST',
    //    success: function (data) {
    //        if (data.Result == "OK") {
    //            $('.basket-qv .detail .total-price .no-vat .price').html(data.TotalPriceNoVat);
    //            $('.basket-qv .detail .total-price .with-vat .price').html(data.TotalPriceWithVat);
    //            $('.basket-qv .detail .free-transport span').html(data.FreeTransport);
    //            var itemsHtml = "";
    //            for (i = 0; i < data.QuoteItems.length; i++) {
    //                itemsHtml += ecUpdateBasketQuickViewItem(data.QuoteItems[i]);
    //            }
    //            $('.basket-qv .detail .items').html(itemsHtml);
    //            $('.basket-qv .detail').show();
    //            $('.basket-qv .loading').hide();
    //        }
    //        if (data.Result == "EMPTY") {
    //            $('.basket-qv .empty').show();
    //            $('.basket-qv .loading').hide();
    //        }
    //    },
    //    error: function () {
    //        //$('.basket-qv .loading').hide();
    //    }
    //});
}
function ecUpdateBasketQuickViewItem(quoteItem) {
    var html = "<div class='item'>"
        + "<div class='img'>";
    if (quoteItem.Img) {
        html += "<img src='" + quoteItem.Img + "' />";
    }
    html += "</div>"
        + "<div class='txt'>"
        + "<div class='name'>" + quoteItem.Name + "</div>"
        + "<div class='with-vat'>" + quoteItem.PriceWithVat + "<span class='cnt'>" + quoteItem.Cnt + "x</span></div>"
        + "<div class='no-vat'>" + quoteItem.PriceNoVat + " bez DPH</div>"
        + "</div></div>";

    return html;
}

/*
 * Message
 */
function ecMessage(infoId, msgHtml)
{
    ecAfterReloadPage();

    var einf = $('#' + infoId);
    einf.stop();
    einf.hide();
    einf.html(msgHtml);
    einf.fadeIn('slow').delay(3000).fadeOut('slow');
}
function ecMessageSuccess(infoId, msg)
{
    ecMessage(infoId, "<div class='alert alert-success fade in'><strong>Informácia</strong><br/>" + msg + "</div>");
}
function ecMessageWarning(infoId, msg) {
    ecMessage(infoId, "<div class='alert alert-warning fade in'><strong>Upozornenie</strong><br/>" + msg + "</div>");
}
function ecMessageError(infoId, msg) {
    ecMessage(infoId, "<div class='alert alert-danger fade in'><strong>Chyba</strong><br/>" + msg + "</div>");
}
function ecModalAddToBasketShow(quoteId, memberId, modalId, data) {
    var needsContainer = $('#' + modalId + ' .needs-container');
    var knownContainer = $('#' + modalId + ' .known-container');
    var unknownContainer = $('#' + modalId + ' .unknown-container');

    $('#' + modalId + ' .product-name').html(data.ProductName);
    needsContainer.hide();
    if (data.NeedsContainer == '1') {
        needsContainer.show();
        knownContainer.hide();
        unknownContainer.hide();
        if (data.IsContainerInfo == '1') {
            ecModalAddToBasketShow_ContainersToAdd(quoteId, memberId, knownContainer, data.ContainersToAdd);
            knownContainer.show();
        }
        else {
            unknownContainer.show();
        }
    }
    $('#' + modalId + ' .free-transport .cert-vert span').html(data.FreeTransport);
    ecModalShow(modalId);
}
function ecModalAddToBasketShow_ContainersToAdd(quoteId, memberId, knownContainer, containersToAdd) {
    var html = "";
    for (idx = 0; idx < containersToAdd.length; idx++) {
        var containerItemToAdd = containersToAdd[idx];

        html += "<div class='cd-name'>"+containerItemToAdd.Name+"</div>" +
            "<div class='cd-img'><img src='" + containerItemToAdd.Img + "' /></div>" +
            "<div class='cd-pcs'><span>Počet ks: </span><span class='cd-pcs-cnt'>"+containerItemToAdd.Cnt+"</span></div>" +
            "<div class='cd-price'><span>Cena za ks bez DPH: </span><span class='cd-price-novat'>"+containerItemToAdd.PriceNoVat+"</span></div>" +
            "<div class='cd-price'><span>Cena za ks s DPH: </span><span class='cd-price-withvat'>"+containerItemToAdd.PriceWithVat+"</span></div>" +
            "<div class='cd-data' data-memberid='" + memberId + "' data-quoteid='" + quoteId + "' data-productid='" + containerItemToAdd.Id + "' data-cnt='"+containerItemToAdd.Cnt+"'></div>";
    }
    $('.container-detail').html(html);
}
function ecModalShow(modalId) {
    $('#' + modalId).modal('show');
}
function ecModalHide(modalId) {
    $('#' + modalId).modal('hide');
}


/*
 * Product2Category
 */
function ecAddProduct2Category(catid, prodid) {
    ecBeforeReloadPage();

    var param = catid + '|' + prodid;
    $.ajax('/Umbraco/Ecommerce/Product2CategoryApi/AddProductToCategory/?id=' + param, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}
function ecRemoveProduct2Category(catid, prodid) {
    ecBeforeReloadPage();

    var param = catid + '|' + prodid;
    $.ajax('/Umbraco/Ecommerce/Product2CategoryApi/RemoveProductToCategory/?id=' + param, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}
function ecRemoveProduct2Category(catid, prodid) {
    ecBeforeReloadPage();

    var param = catid + '|' + prodid;
    $.ajax('/Umbraco/Ecommerce/Product2CategoryApi/RemoveProductToCategory/?id=' + param, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}
function ecMoveUpProduct2Category(catid, prodid) {
    ecBeforeReloadPage();

    var param = catid + '|' + prodid;
    $.ajax('/Umbraco/Ecommerce/Product2CategoryApi/MoveUpProductToCategory/?id=' + param, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}
function ecMoveDownProduct2Category(catid, prodid) {
    ecBeforeReloadPage();

    var param = catid + '|' + prodid;
    $.ajax('/Umbraco/Ecommerce/Product2CategoryApi/MoveDownProductToCategory/?id=' + param, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}


/*
 * Category
 */
function ecMoveUpCategory(parentid, catid) {
    ecBeforeReloadPage();

    var param = parentid + '|' + catid;
    $.ajax('/Umbraco/Ecommerce/CategoryApi/MoveUpCategory/?id=' + param, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}
function ecMoveDownCategory(parentid, catid) {
    ecBeforeReloadPage();

    var param = parentid + '|' + catid;
    $.ajax('/Umbraco/Ecommerce/CategoryApi/MoveDownCategory/?id=' + param, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}


/*
 * Product
 */
function ecMoveUpProduct(id) {
    ecBeforeReloadPage();

    $.ajax('/Umbraco/Ecommerce/ProductApi/MoveUpProduct/?id=' + id, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}
function ecMoveDownProduct(id) {
    ecBeforeReloadPage();

    $.ajax('/Umbraco/Ecommerce/ProductApi/MoveDownProduct/?id=' + id, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}

/*
 * Product attribute
 */
function ecMoveUpProductAttribute(id) {
    ecBeforeReloadPage();

    $.ajax('/Umbraco/Ecommerce/ProductApi/MoveUpProductAttribute/?id=' + id, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}
function ecMoveDownProductAttribute(id) {
    ecBeforeReloadPage();

    $.ajax('/Umbraco/Ecommerce/ProductApi/MoveDownProductAttribute/?id=' + id, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}

/*
 * Product price
 */
function ecCalculatePriceWithoutVat(el) {
    var self = $(el);
    var target = $('#' + self.data('vattarget'));
    var param = self.val() + '|' + $('#' + self.data('vatrate')).val();

    $.ajax('/Umbraco/Ecommerce/ProductApi/CalculatePriceWithoutVat/?id=' + param, {
        type: 'POST',
        success: function (data) {
            if (data.Status == 'OK') {
                target.val(data.PriceWithoutVat);
            }
            else {
                target.val('');
            }
        },
        error: function () {
            target.val('');
        }
    });
}


/* Category public */
function ecCategoryPublicFilterModel_PageSize(sessionId, pageSize) {
    ecBeforeReloadPage();

    var param = sessionId + '|' + pageSize;

    $.ajax('/Umbraco/Ecommerce/CategoryApi/CategoryPublicFilterModel_PageSize_Set/?id=' + param, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}
function ecCategoryPublicFilterModel_ProductViewType(sessionId, viewType) {
    ecBeforeReloadPage();

    var param = sessionId + '|' + viewType;

    $.ajax('/Umbraco/Ecommerce/CategoryApi/CategoryPublicFilterModel_ProductView_Set/?id=' + param, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}
//function ecCategoryPublicFilterModel_ProductViewType(el) {
//    ecBeforeReloadPage();

//    var self = $(el);
//    var param = self.data("sessionid") + '|' + self.val();

//    $.ajax('/Umbraco/Ecommerce/CategoryApi/CategoryPublicFilterModel_ProductView_Set/?id=' + param, {
//        type: 'POST',
//        success: function (data) {
//            ecReloadPage();
//        },
//        error: function () {
//            ecReloadPage();
//        }
//    });
//}
function ecCategoryPublicFilterModel_ProductSortType(el) {
    ecBeforeReloadPage();

    var self = $(el);
    var param = self.data("sessionid") + '|' + self.val();

    $.ajax('/Umbraco/Ecommerce/CategoryApi/CategoryPublicFilterModel_ProductSort_Set/?id=' + param, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}
function ecCategoryPublicFilterModel_ProducerIsSelected(el) {
    ecBeforeReloadPage();

    var self = $(el);
    var param = self.data("sessionid") + '|' + self.data("producer") + '|' + (self.is(":checked") ? '1' : '0');

    $.ajax('/Umbraco/Ecommerce/CategoryApi/CategoryPublicFilterModel_ProducerIsSelected_Set/?id=' + param, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}
function ecCategoryPublicFilterModel_AllProducers(sessionid, setSelected) {
    ecBeforeReloadPage();

    var param = sessionid + '|' + (setSelected ? '1' : '0');

    $.ajax('/Umbraco/Ecommerce/CategoryApi/CategoryPublicFilterModel_ProducerIsSelected_All/?id=' + param, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}
function ecCategoryPublicFilterModel_ProductAttributeIsSelected(el) {
    ecBeforeReloadPage();

    var self = $(el);
    var param = self.data("sessionid") + '|' + self.data("attribute") + '|' + (self.is(":checked") ? '1' : '0');

    $.ajax('/Umbraco/Ecommerce/CategoryApi/CategoryPublicFilterModel_ProductAttributeIsSelected_Set/?id=' + param, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}
function ecCategoryPublicFilterModel_AllProdAttrs(sessionid, setSelected) {
    ecBeforeReloadPage();

    var param = sessionid + '|' + (setSelected ? '1' : '0');

    $.ajax('/Umbraco/Ecommerce/CategoryApi/CategoryPublicFilterModel_ProductAttributeIsSelected_All/?id=' + param, {
        type: 'POST',
        success: function (data) {
            ecReloadPage();
        },
        error: function () {
            ecReloadPage();
        }
    });
}

/* Page reload */
function ecBeforeReloadPage() {
    $('html body').addClass('noscroll');
    $('html body').prepend("<div class='pending-reload'><div class='reload-msg'><i class='fas fa-spinner fa-spin fa-2x'></i></div></div>");
}
function ecAfterReloadPage() {
    $('html body').removeClass('noscroll');
    $('.pending-reload').remove();
}
function ecReloadPage() {
    location.reload(true);
}


/* Related products */
function ecDropRelatedProduct(el) {
    $(el).parent().remove();
    ecReindexRelatedProducts();
}
function ecReindexRelatedProducts() {
    $('.related-products .related-product').each(function (index, element) {
        var pkrelated = $(element).find('.pkrelated');
        pkrelated.attr('id', 'ProductRelations_Items_' + index + '__PkProductRelated');
        pkrelated.attr('name', 'ProductRelations.Items[' + index + '].PkProductRelated');
    });
}

var ec_relatedprodsearch_xhr;
function ecRelatedProductAutocomplete(selector, spinner) {
    $(selector).autoComplete({
        minChars: 1,
        cache: false,
        delay: 200,
        source: function (term, response) {
            $(spinner).removeClass('hidden');
            try { ec_relatedprodsearch_xhr.abort(); } catch (e) { }
            ec_relatedprodsearch_xhr = $.ajax('/Umbraco/Ecommerce/ProductApi/SearchProduct?id=' + term,
                {
                    type: 'POST',
                    success: function (data) {
                        response(data);
                        $(spinner).addClass('hidden');
                    }
                });
        },
        renderItem: function (item, search) {
            return '<div class="autocomplete-suggestion" data-key="' + item.Key + '" data-code="' + item.Code + '" data-name="' + item.Name + '">' + ecRelatedProductSearchResultItem(item) + '</div>';
        },
        onSelect: function (e, term, item) {
            ecAddRelatedProduct(item);
        }
    });
}
function ecRelatedProductSearchResultItem(item) {
    if (item.Msg) {
        return "<div class='msg'>" + item.Msg + "</div>";
    }

    return "<div class='title'>" + item.Code + " " + item.Name + "</div>";
}
function ecAddRelatedProduct(item) {
    if (item.Msg) {
        return;
    }

    var key = item.data("key");
    var code = item.data("code");
    var name = item.data("name");
    var htmlToAdd = "<div class='related-product'><input class='hidden pkrelated' id='pkrelatednewid' name='pkrelatednewname' readonly='readonly' type='text' value='" + key + "' /><a href='#' onclick='ecDropRelatedProduct(this);return false;' class='drop-relation-btn' title='Odstrániť súvisiaci produkt'><i class='fa fa-trash-alt'></i></a><span class='code'>" + code + "</span><span class='name'>" + name + "</span></div>";
    $(".related-product-add").before(htmlToAdd);
    ecReindexRelatedProducts();
}


/* Quote product */
var ec_quoteprodsearch_xhr;
function ecQuoteProductAutocomplete(selector, spinner) {
    $(selector).autoComplete({
        minChars: 1,
        cache: false,
        delay: 200,
        source: function (term, response) {
            $(spinner).removeClass('hidden');
            try { ec_relatedprodsearch_xhr.abort(); } catch (e) { }
            ec_relatedprodsearch_xhr = $.ajax('/Umbraco/Ecommerce/ProductApi/SearchProduct?id=' + term,
                {
                    type: 'POST',
                    success: function (data) {
                        response(data);
                        $(spinner).addClass('hidden');
                    }
                });
        },
        renderItem: function (item, search) {
            return '<div class="autocomplete-suggestion" data-key="' + item.Key + '" data-code="' + item.Code + '" data-name="' + item.Name + '" data-unitname="' + item.UnitName + '">' + ecQuoteProductSearchResultItem(item) + '</div>';
        },
        onSelect: function (e, term, item) {
            ecAddQuoteProduct(item);
        }
    });
}
function ecQuoteProductSearchResultItem(item) {
    if (item.Msg) {
        return "<div class='msg'>" + item.Msg + "</div>";
    }

    return "<div class='title'>" + item.Code + " " + item.Name + "</div>";
}
function ecAddQuoteProduct(item) {
    if (item.Msg) {
        return;
    }

    $('#PkProduct').val(item.data("key"));
    $('#ItemCode').val(item.data("code"));
    $('#ItemName').val(item.data("name"));

    $('.form-item.item-pcs label').html(item.data("unitname"));
    $('.form-item.item-pcs').removeClass('hidden');
}

/* Product price */
var ec_prodpricesearch_xhr;
function ecProductPriceAutocomplete(selector, spinner, linkid) {
    $(selector).autoComplete({
        minChars: 1,
        cache: false,
        delay: 200,
        source: function (term, response) {
            $(spinner).removeClass('hidden');
            try { ec_prodpricesearch_xhr.abort(); } catch (e) { }
            ec_prodpricesearch_xhr = $.ajax('/Umbraco/Ecommerce/ProductApi/SearchProduct?id=' + term,
                {
                    type: 'POST',
                    success: function (data) {
                        response(data);
                        $(spinner).addClass('hidden');
                    }
                });
        },
        renderItem: function (item, search) {
            return '<div class="autocomplete-suggestion" data-key="' + item.Key + '" data-code="' + item.Code + '" data-name="' + item.Name + '">' + ecProductPriceSearchResultItem(item) + '</div>';
        },
        onSelect: function (e, term, item) {
            $('#' + linkid).attr('href', '/moj-ucet/produkty/ceny?productId=' + item.data('key'));
            document.getElementById(linkid).click();
        }
    });
}
function ecProductPriceSearchResultItem(item) {
    if (item.Msg) {
        return "<div class='msg'>" + item.Msg + "</div>";
    }

    return "<div class='title'>" + item.Code + " " + item.Name + "</div>";
}

/* Product category */
function acProductToCategoryChange(el) {
    if (el.checked) {
        ecAddProductToCategory($(el).data('key'));
    }
    else {
        ecRemoveProductFromCategory($(el).data('key'));
    }
}
function ecAddProductToCategory(catid) {
    var htmlToAdd = "<div class='product-in-category'><input class='hidden pkcategory' id='pkcategorynewid' name='pkcategorynewid' readonly='readonly' type='text' value='" + catid + "' /></div>";
    $(".product-in-categories .category-tree").before(htmlToAdd);
    ecReindexProductCategories();
}
function ecRemoveProductFromCategory(catid) {
    $('.pkcategory:input[value="' + catid + '"]').parent().remove();
    ecReindexProductCategories();
}
function ecReindexProductCategories() {
    $('.product-in-categories .product-in-category').each(function (index, element) {
        var pkcategory = $(element).find('.pkcategory');
        pkcategory.attr('id', 'ProductCategories_SelectedCategories_' + index + '_');
        pkcategory.attr('name', 'ProductCategories.SelectedCategories[' + index + ']');
    });
}
function ecCategoryTreeClick(el) {
    var node = $(el).parent().parent();
    if (node.hasClass('closed')) {
        node.removeClass('closed');
    }
    else {
        node.addClass('closed');
    }
}

/* Submit quote */
function ecSubmitBasketDeliveryData(submitBtnId, transportGroupName, transportInputId, paymentGroupName, paymentInputId) {
    $('#' + transportInputId).val($('input[name=' + transportGroupName + ']:checked').val());
    $('#' + paymentInputId).val($('input[name=' + paymentGroupName + ']:checked').val());

    document.getElementById(submitBtnId).click();
}

/* Grid pager */
function naplnspajzuTogglePageNbInput_CategoryDetailProductsPager(el) {
    var self = $(el);
    var pageNb = self.next();
    if (pageNb.hasClass('hidden')) {
        pageNb.removeClass('hidden');
        pageNb.find('input').focus();
    }
    else {
        pageNb.addClass('hidden');
    }
}
function naplnspajzuLoadPageNb_CategoryDetailProductsPager(el, replacePgNb) {
    var self = $(el);
    var pgnb = self.prev().val();
    var href = self.attr('href').replace(replacePgNb, pgnb);
    self.attr('href', href);
}

/* Admin quote state edit */
var quoteAdminGetRecords_CurrentQuoteStateLink;

function naplnspajzuQuoteStateChange(el) {
    quoteAdminGetRecords_CurrentQuoteStateLink = $(el);

    $('#modalQuoteStateChangeId_QuoteId').html(quoteAdminGetRecords_CurrentQuoteStateLink.data('quoteid'));
    $('#modalQuoteStateChangeId_Date').html(quoteAdminGetRecords_CurrentQuoteStateLink.data('quotedate'));
    $('#modalQuoteStateChangeId_Price').html(quoteAdminGetRecords_CurrentQuoteStateLink.data('quoteprice'));

    var stateName = quoteAdminGetRecords_CurrentQuoteStateLink.html();

    $('.btn-quote-state').removeClass('selected');
    $('.btn-quote-state').each(function () {
        var btn = $(this);
        if (btn.html() == stateName) {
            btn.addClass('selected');
        }
    });

    ecModalShow('modalQuoteStateChangeId');
}
function naplnspajzuQuoteSetState(el) {
    var quoteId = quoteAdminGetRecords_CurrentQuoteStateLink.data('quotepk');
    var quoteState = $(el).data('statekey');

    var param = quoteId + '|' + quoteState;

    $.ajax('/Umbraco/Ecommerce/QuoteApi/ChangeQuoteState/?id=' + param, {
        type: 'POST',
        success: function (data) {
            console.log(data);
            if (data.Result == 'OK') {
                quoteAdminGetRecords_CurrentQuoteStateLink.html(data.StateName);
            }
            ecModalHide('modalQuoteStateChangeId');
        },
        error: function () {
            ecModalHide('modalQuoteStateChangeId');
        }
    });
}


/* Product search */
var ecProductSearchManager = (function () {
    var requests = [];
    var url = '';

    return {
        saved: {},
        addReq: function (opt) {
            requests.push(opt);
        },
        removeReq: function (opt) {
            if ($.inArray(opt, requests) > -1)
                requests.splice($.inArray(opt, requests), 1);
        },
        run: function () {
            var self = this,
                oriSuc;

            if (requests.length) {
                oriSuc = requests[0].complete;

                requests[0].complete = function () {
                    if (typeof (oriSuc) === 'function') oriSuc();
                    requests.shift();
                    self.run.apply(self, []);
                };

                $.ajax(requests[0]);
            } else {
                self.tid = setTimeout(function () {
                    self.run.apply(self, []);
                }, 1000);
            }
        },
        stop: function () {
            requests = [];
            clearTimeout(this.tid);
        }
    };
}());

function ecInitProductSearchUI(url) {
    ecProductSearchManager.url = url;
    ecProductSearchManager.run();

    var searchInput = $('.product-search .search-request input');

    if (searchInput.length > 0) {
        searchInput.on("keyup", function (e) {
            //if (e.keyCode === 13) {
            //    ecProductSearch();
            //}
            ecProductSearch();
        });
        searchInput.focus();
    }


    if ($('.category-detail-search').length > 0) {
        $('a.menu-item-search').on('click', function () {
            ecProductSearchOpen();
            return false;
        });
        if ($.cookie("naplnspajzu_productsearch_open") == '1') {
            // set cookie to not open search control after next page reload
            $.cookie("naplnspajzu_productsearch_open", '0', { expires: 40000, path: '/' });
            // open search control now
            ecProductSearchOpen();
        }
    }
    else {
        $('a.menu-item-search').on('click', function () {
            // set cookie to open search control after next page reload
            $.cookie("naplnspajzu_productsearch_open", '1', { expires: 40000, path: '/' });
            return true;
        });
    }
}


function ecProductSearch() {
    var search = $('.product-search .search-request input').val();
    if (search && search.length > 0) {
        var formData = new FormData();
        formData.append('id', search);
        ecProductSearchManager.addReq({
            url: ecProductSearchManager.url,
            type: 'POST',
            contentType: false,
            processData: false,
            data: formData,
            beforeSend: function () {
                ecBeforeStartProductSearch();
            },
            success: function (data) {
                $('.product-search .search-result').html(data);
                ecInitToBasketPcsChange();
                ecAfterFinishProductSearch();
            },
            error: function () {
                ecAfterFinishProductSearch();
            }
        });
        //$.ajax(ecProductSearchUrl,
        //    {
        //        type: 'POST',
        //        contentType: false,
        //        processData: false,
        //        data: formData,
        //        success: function (data) {
        //            $('.product-search .search-result').html(data);
        //            ecInitToBasketPcsChange();
        //            ecAfterFinishProductSearch();
        //        },
        //        error: function () {
        //            ecAfterFinishProductSearch();
        //        }
        //    });
    }
    else {
        $('.product-search .search-result').html('');
    }
}
function ecBeforeStartProductSearch() {
    $('.product-search .search-result').addClass('hidden');
    $('.product-search .search-pending').removeClass('hidden');
}
function ecAfterFinishProductSearch() {
    $('.product-search .search-result').removeClass('hidden');
    $('.product-search .search-pending').addClass('hidden');
}

function ecProductSearchOpen() {
    $('.category-detail-products').addClass('hidden');
    $('.category-detail-search').removeClass('hidden');
    var input = $('.category-detail-search .search-request input');
    input.val('');
    input.focus();
}
function ecProductSearchClose() {
    $('.category-detail-search').addClass('hidden');
    $('.category-detail-search .search-result').html('');
    $('.category-detail-products').removeClass('hidden');
}

/* Product promo scroll */
function ecMoveProductPromoInit(id) {
    $('#' + id).scroll(function () {
        var pos = $(this).scrollLeft();
        var maxpos = this.scrollWidth - this.clientWidth;
        if (pos > 10) {
            $('#' + id + '-move-prev').removeClass('hidden');
        }
        else {
            $('#' + id + '-move-prev').addClass('hidden');
        }
        if (pos < maxpos - 10) {
            $('#' + id + '-move-next').removeClass('hidden');
        }
        else {
            $('#' + id + '-move-next').addClass('hidden');
        }
    });
}
function ecMoveProductPromo(el, direction) {
    var self = $(el);
    var container = $('#' + self.attr('data-target'));
    var pos = container.scrollLeft();
    var box = $('#' + self.attr('data-target') + ' .col-md-3').first();
    var movepos = box.width() + 34;


    var newpos = pos + (direction * movepos);
    container.animate({ scrollLeft: newpos }, 500);
}

function ecMoveProductPromoNext(el) {
    ecMoveProductPromo(el, 1);
}
function ecMoveProductPromoPrev(el) {
    ecMoveProductPromo(el, -1);
}

/* Favorite products */
function ecAddProduct2Favorite(productId, memberId, infoId) {
    var param = productId + '|' + memberId;
    $.ajax('/Umbraco/Ecommerce/FavoriteProductApi/AddProductToFavorite/?id=' + param, {
        type: 'POST',
        success: function (data) {
            if (data.Result == "OK") {
                ecMessageSuccess(infoId, "Produkt pridaný medzi obľúbené.");
            }
            else {
                ecMessageError(infoId, data.Result);
            }
        },
        error: function () {
            ecMessageError(infoId, "Vznikla chyba pri pridávaní produktu medzi obľúbené.");
        }
    });
}
function ecRemoveProduct2Favorite(productId, memberId, infoId) {
    ecBeforeReloadPage();
    var param = productId + '|' + memberId;
    $.ajax('/Umbraco/Ecommerce/FavoriteProductApi/RemoveProductToFavorite/?id=' + param, {
        type: 'POST',
        success: function (data) {
            if (data.Result == "OK") {
                ecReloadPage();
            }
            else {
                ecAfterReloadPage();
                ecMessageError(infoId, data.Result);
            }
        },
        error: function () {
            ecAfterReloadPage();
            ecMessageError(infoId, "Vznikla chyba pri odstraňovaní produktu z obľúbených.");
        }
    });
}
