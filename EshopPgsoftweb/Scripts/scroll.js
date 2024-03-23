
$(document).ready(function () {

    //Check to see if the window is top if not then display button
    $(window).scroll(function () {
        if ($(this).scrollTop() > 200) {
            $('.header').addClass('scroll');
            $('.page-body').addClass('scroll');
        } else {
            $('.header').removeClass('scroll');
            $('.page-body').removeClass('scroll');
        }
        if ($(this).scrollTop() > 200) {
            $('.scrollToTop').fadeIn();
            $('.scrollDown').fadeOut();
        } else {
            $('.scrollToTop').fadeOut();
            $('.scrollDown').fadeIn();
        }
    });

    //Click event to scroll to top
    $('.scrollToTop').click(function () {
        $('html, body').animate({ scrollTop: 0 }, 800);
        return false;
    });
    //Click event to scroll down
    $('.scrollDown').click(function () {
        $('html, body').animate({ scrollTop: $(window).height()}, 800);
        return false;
    });
});
