function smpInitialise() {
    smpInitialiseNav();

    $('ul.smp-planner-list').sortable({
        handle: '.smp-planner-list-meal-drag-handle',
        isValidTarget: function(item, container) {
            return container.el[0].className.includes('smp-planner-list-meal');
        },
        onDrop: function(item, container, _super, event) {
            _super(item, container, event);

            let mealMoved = item.attr('data-meal');
            let newDate = item.parent('ul').attr('data-day');
            if (typeof mealMoved !== 'undefined' && typeof newDate !== 'undefined') {
                let prevMeal = parseInt(item.prev('li').attr('data-meal'));

                $.ajax({
                    url: '/api/planner/' + mealMoved + '/move',
                    type: 'PUT',
                    data: JSON.stringify({ date: newDate, sortOrderPreviousPlannerMealId: prevMeal == NaN ? null : prevMeal }),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json'
                });

                return;
            }

            let shoppingListItemMoved = item.attr('data-shoppinglistitem');
            if (typeof shoppingListItemMoved !== 'undefined') {
                let prevShoppingListItem = parseInt(item.prev('li').attr('data-shoppinglistitem'));

                $.ajax({
                    url: '/api/shoppinglist/' + shoppingListItemMoved + '/move',
                    type: 'PUT',
                    data: JSON.stringify({ sortOrderPreviousShoppingListItemId: prevShoppingListItem == NaN ? null : prevShoppingListItem }),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json'
                });

                return;
            }
        }
    });

    $('.smp-planner-list-meal-oc').click(function() {
        $(this).parent('div').next().find('.smp-planner-list-meal-details').toggle();
    });

    $('.smp-planner-add-header > span').click(function() {
        if ($(this).hasClass('selected'))
            return;
        $(this).parent().children('span.selected').each(function() {
            $(this).removeClass('selected');
            let selectedEl = $(this).attr('data-select');
            $(selectedEl).hide();
        });
        $(this).addClass('selected');
        let selectEl = $(this).attr('data-select');
        $(selectEl).show();
    });

    $('button[data-depends]').each(function() {
        let btnWithDependency = $(this);
        let dependentFormObject = $(btnWithDependency.attr('data-depends'));
        dependentFormObject.on('keypress', function(e) {
            if (btnWithDependency.attr('disabled') && (e.keyCode || e.which) === 13) {
                e.preventDefault();
                return false;
            }
        });
        dependentFormObject.on('change input paste keyup', function() {
            let dependentValue = $(this).val();
            btnWithDependency.prop('disabled', dependentValue === null || dependentValue.match(/^\s*$/) !== null);
        });
        dependentFormObject.trigger('change');
    });

    $('textarea.notes').on('change input paste keyup', function() {
        let updatedText = $(this).val();
        if (updatedText === $(this).data('saved-value'))
            return;

        $('div.note-info').text('Saving...').addClass('note-unsaved');
        let timeout = $(this).data('throttle');
        if (typeof timeout !== 'undefined')
            clearTimeout(timeout);
        $(this).data('throttle', setTimeout(function() {
            $.ajax({
                    url: '/api/note',
                    type: 'PUT',
                    data: JSON.stringify({ noteText: updatedText }),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json'
                })
                .done(function() {
                    $('textarea.notes').data('saved-value', updatedText);
                    $('div.note-info').text('Up to date').removeClass('note-unsaved');
                })
                .fail(function() {
                    $('div.note-info').text('Error saving note!');
                });
        }, 1000));
    });

    $('form[data-confirm]').submit(function(event) {
        if (!confirm($(this).attr('data-confirm'))) {
            event.preventDefault();
            return false;
        }
    });
}

function smpInitialiseNav() {
    $(window).resize(function() {
        $('aside').css('display', '');
        if ($('.nav-close:visible').length > 0) {
            $('.nav-close').css('display', '');
            $('.nav-show').css('display', '');
        }
    });
    $('.nav-show').click(function() {
        $('aside').fadeToggle('fast');
        $(this).hide();
        $('.nav-close').show();
    });
    $('.nav-close').click(function() {
        $('aside').hide();
        $(this).hide();
        $('.nav-show').show();
    });
    $('[data-href]').click(function() {
        window.location.href = $(this).attr('data-href');
    });
}

coerceToArrayBuffer = function (thing, name) {
    if (typeof thing === "string") {
        // base64url to base64
        thing = thing.replace(/-/g, "+").replace(/_/g, "/");

        // base64 to Uint8Array
        var str = window.atob(thing);
        var bytes = new Uint8Array(str.length);
        for (var i = 0; i < str.length; i++) {
            bytes[i] = str.charCodeAt(i);
        }
        thing = bytes;
    }

    // Array to Uint8Array
    if (Array.isArray(thing)) {
        thing = new Uint8Array(thing);
    }

    // Uint8Array to ArrayBuffer
    if (thing instanceof Uint8Array) {
        thing = thing.buffer;
    }

    // error if none of the above worked
    if (!(thing instanceof ArrayBuffer)) {
        throw new TypeError("could not coerce '" + name + "' to ArrayBuffer");
    }

    return thing;
};

coerceToBase64Url = function (thing) {
    // Array or ArrayBuffer to Uint8Array
    if (Array.isArray(thing)) {
        thing = Uint8Array.from(thing);
    }

    if (thing instanceof ArrayBuffer) {
        thing = new Uint8Array(thing);
    }

    // Uint8Array to base64
    if (thing instanceof Uint8Array) {
        var str = "";
        var len = thing.byteLength;

        for (var i = 0; i < len; i++) {
            str += String.fromCharCode(thing[i]);
        }
        thing = window.btoa(str);
    }

    if (typeof thing !== "string") {
        throw new Error("could not coerce to string");
    }

    // base64 to base64url
    // NOTE: "=" at the end of challenge is optional, strip it off here
    thing = thing.replace(/\+/g, "-").replace(/\//g, "_").replace(/=*$/g, "");

    return thing;
};
