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
            let prevMeal = parseInt(item.prev('li').attr('data-meal'));
            $.ajax({
                url: '/api/planner/' + mealMoved + '/move',
                type: 'PUT',
                data: JSON.stringify({ date: newDate, sortOrderPreviousPlannerMealId: prevMeal == NaN ? null : prevMeal }),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json'
            });
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

    $('textarea.notes').on('change input paste keyup', function() {
        let updatedText = $(this).val();
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