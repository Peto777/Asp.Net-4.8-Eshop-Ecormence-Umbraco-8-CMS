$(document).ready(function () {
    // Chosen drop down
    $(".chosen-select").chosen({
        placeholder_text: "Vyberte z ponuky",
        no_results_text: "Nič sa nenašlo: ",
        width: "100%"
    });
    // Autocomplete off
    $(".nplspz-form .form-item input").attr("autocomplete", "off");
    $("input.no-autocomplete").attr("autocomplete", "off");
    // Date picker
    $("input:text.skdate").datepicker({ firstDay: 1 });
    $("div.skdate").find("input").datepicker({ firstDay: 1 });
    // Mobile menu
    $('#mobile-menu-button').click(function () {
        $('#mobile-menu-button .navbar-hamburger').toggleClass('hidden');
        $('#mobile-menu-button .navbar-close').toggleClass('hidden');
    });
    // Submenu
    $('ul.nav li.dropdown').hover(
        function () { $(this).addClass('open'); },
        function () { $(this).removeClass('open'); }
    );
});
