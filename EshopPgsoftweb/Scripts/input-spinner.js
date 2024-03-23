function createInputSpinners() {
    createNumberSpinner();
    createTimeSpinner();
}

/* Number spinner */
function createNumberSpinner() {
    jQuery('<div class="quantity-nav"><div class="quantity-button quantity-up">+</div><div class="quantity-button quantity-down">-</div></div>').insertAfter('.number-spinner input');
    jQuery('.number-spinner').each(function () {
        var spinner = jQuery(this),
            step = spinner.data('step'),
            btnUp = spinner.find('.quantity-up'),
            btnDown = spinner.find('.quantity-down');

        btnUp.click(function () {
            numberSpinnerAddStep(spinner, step);
        });

        btnDown.click(function () {
            numberSpinnerAddStep(spinner, -1 * step);
        });

    });
}
function numberSpinnerAddStep(spinner, step) {
    var input = spinner.find("input");
    var min = spinner.data('min');
    var max = spinner.data('max');
    var val = parseInt(input.val()) || 0;

    val = val + step;
    if (val > max) {
        val = max;
    }
    if (val < min) {
        val = min;
    }

    input.val(val);
    input.trigger("change");
}


/* Time spinner */
function createTimeSpinner() {
    jQuery('<div class="quantity-nav"><div class="quantity-button quantity-up">+</div><div class="quantity-button quantity-down">-</div></div>').insertAfter('.time-spinner input');
    jQuery('.time-spinner').each(function () {
        var spinner = jQuery(this),
            step = spinner.data('step'),
            btnUp = spinner.find('.quantity-up'),
            btnDown = spinner.find('.quantity-down');

        btnUp.click(function () {
            timeSpinnerAddStep(spinner, step);
        });

        btnDown.click(function () {
            timeSpinnerAddStep(spinner, -1 * step);
        });

    });
}
function timeSpinnerAddStep(spinner, step) {
    var input = spinner.find("input");
    var min = spinner.data('min');
    var max = spinner.data('max');
    var val = timeSpinnerDisplayStringToMinutes(input.val());

    val = val + step;
    if (val > max) {
        val = max;
    }
    if (val < min) {
        val = min;
    }

    input.val(timeSpinnerMinutesToDisplayString(val));
    input.trigger("change");
}
function timeSpinnerMinutesToDisplayString(minutes) {
    var dispHour = Math.floor(minutes / 60);
    var dispMinute = minutes - (dispHour * 60);

    return ("0" + dispHour).slice(-2) + ":" + ("0" + dispMinute).slice(-2);
}
function timeSpinnerDisplayStringToMinutes(val) {
    var minutes = 0;

    if (val) {
        var dispHour = 0;
        var dispMinute = 0;
        var items = val.split(":");
        if (items.length > 0) {
            dispHour = parseInt(items[0]) || 0
        }
        if (items.length > 1) {
            dispMinute = parseInt(items[1]) || 0
        }

        minutes = dispHour * 60 + dispMinute;
    }

    return minutes;
}
