function smpInitialise() {
    smpInitialiseNav();

    $("ul.smp-planner-list").sortable({
        handle: '.smp-planner-list-meal-drag-handle',
        isValidTarget: function($item, container) {
            return container.el[0].className.includes('smp-planner-list-meal');
        }
    });

    $(".smp-planner-list-meal-oc").click(function() {
        $(this).parent("div").next().find(".smp-planner-list-meal-details").toggle();
    });

    $(".smp-planner-add-header > span").click(function() {
        if ($(this).hasClass("selected"))
            return;
        $(this).parent().children("span.selected").each(function() {
            $(this).removeClass("selected");
            let selectedEl = $(this).attr("data-select");
            $(selectedEl).hide();
        });
        $(this).addClass("selected");
        let selectEl = $(this).attr("data-select");
        $(selectEl).show();
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