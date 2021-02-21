
$(document).on('click', '.plus, .minus', function () {
    var $qty = $(this).closest('.cart__qty').find('.cart__qty-input'),
        currentVal = parseFloat($qty.val()),
        max = parseFloat($qty.attr('max')),
        min = parseFloat($qty.attr('min')),
        step = $qty.attr('step');

    if (!currentVal || currentVal === '' || currentVal === 'NaN') currentVal = 0;
    if (max === '' || max === 'NaN') max = '';
    if (min === '' || min === 'NaN') min = 0;
    if (step === 'any' || step === '' || step === undefined || parseFloat(step) === 'NaN') step = 1;

    if ($(this).is('.plus')) {
        if (max && (currentVal >= max)) {
            $qty.val(max);
        } else {
            $qty.val((currentVal + parseFloat(step)));
        }
    } else {
        if (min && (currentVal <= min)) {
            $qty.val(min);
        } else if (currentVal > 0) {
            $qty.val((currentVal - parseFloat(step)));
        }
    }
    $qty.trigger('change');
});


$(".btn-mobile_vertical_menu").click(function () {
    $("#_mobile_vertical_menu").addClass('active');
    $("#_mobile_sidebarmenu_content").addClass('active');
    $(".sidebar-overlay").addClass('act');
});



function topFunction() {
    document.body.scrollTop = 0; // For Safari
    document.documentElement.scrollTop = 0; // For Chrome, Firefox, IE and Opera
}


$("#back_top").click(function () {
    topFunction();
    return !1
});


$('#cart_block').on("click", function (e) {
    document.getElementById('cart-info').style = 'display:block';
    e.stopPropagation();
});

$(document).on('click', function (event) {
    if ($(event.target).is('#cart_block #cart-info') == !1) {
        document.getElementById('cart-info').style = 'display:none';
    }
});


        $('#show-megamenu').on("click", function () {
            if ($('.canvas-menu').hasClass('active')) {
                $('.canvas-menu').removeClass('active');
                $('body').removeClass('canvasmenu-right');
                $(this).removeClass('close');
            } else {
                $('.canvas-menu').addClass('active');
                $('body').addClass('canvasmenu-right');
                $(this).addClass('close');
            }
            return false;
        });
        
        
        $('.canvas-header-box .close-box, .canvas-overlay').on("click", function () {
            $('.canvas-menu').removeClass('active');
            $('body').removeClass('canvasmenu-right');
            return false;
        });
        
        // vertical dropdown
        if ($(document).width() <= 1199 && $(document).width() >= 992) {
            $(".vertical_dropdown").removeClass('active');
            $("#_desktop_vertical_menu").css('display', 'none')
        }
        if ($(document).width() >= 992) {
            $('.vertical_dropdown').click(function () {
                if ($(this).hasClass('active')) {
                    $(this).removeClass('active');
                    document.getElementById('_desktop_vertical_menu').style = "display:none";
                }
                else {
                    $(this).addClass('active')
                     document.getElementById('_desktop_vertical_menu').style = "display:block";
                    if ($(document).width() < 992) {
                        $(".sidebar-overlay").addClass('act');
                    }
                }
            });
        }

        if ($(document).width() < 992) {
            $(".vertical_dropdown").removeClass('active');
            $('.vertical_dropdown').click(function () {
                $("#_desktop_vertical_menu").addClass('active');
                $(".sidebar-overlay").addClass('act');
            });
        }

        var show_more = $(".vertical_menu").data('count_showmore');
        var show_more_lg = $(".vertical_menu").data('count_showmore_lg') - 1;
        var textshowmore = $(".vertical_menu").data('textshowmore');
        var textless = $(".vertical_menu").data('textless');
        $('.vertical_menu>ul>li:last-child').addClass('last');
        if ($('.vertical_menu>ul>li').length > show_more) {
            $(".vertical_menu .show_more").removeClass('hidden');
        }
        if ($(document).width() < 1200) {
            $('#_desktop_vertical_menu .site-nav>li:gt(' + show_more_lg + ')').addClass('hide');
            if ($('.vertical_menu>ul>li').length > show_more_lg + 1) {
                $(".vertical_menu .show_more").removeClass('hidden hide');
            }
        };
        $('.show_more').click(function () {
            $('.vertical_menu>ul>li:last-child').removeClass('last');
            if ($(this).hasClass('active')) {
                $(this).removeClass('active');
                $(this).text(textshowmore)
            } else {
                $(this).addClass('active');
                $(this).text(textless)
            }
            if ($('.vertical_menu>ul>li').hasClass('hide')) {
                $('.vertical_menu>ul>li.hide').slideToggle(300);
            }
        });

        $(".sidebar-overlay").click(function () {
            $(this).removeClass('act');
            $("#_mobile_vertical_menu").removeClass('active');
            $(".btn_active").css('opacity', '1');
            $('.vertical_dropdown').removeClass('active');
            $("#_desktop_vertical_menu").removeClass('active');
            $('.sidebar-filter').removeClass('active');
        });


        $(function () {
            // Current Ajax request.
            var currentAjaxRequest = null;
            // Grabbing all search forms on the page, and adding a .search-results list to each.
            var searchForms = $('form[action="/search"]').css('position', 'relative').each(function () {
                // Grabbing text input.
                var input = $(this).find('input[name="q"]');
                // Adding a list for showing search results.
                var offSet = input.position().top + input.innerHeight();
                $('<ul class="search-results has-scroll"></ul>').css({ 'position': 'absolute', 'left': '0px', 'top': offSet }).appendTo($(this)).hide();
                // Listening to keyup and change on the text field within these search forms.
                input.attr('autocomplete', 'off').bind('keyup change', function () {
                    // What's the search term?
                    var term = $(this).val();
                    // What's the search form?
                    var form = $(this).closest('form');
                    // What's the search URL?
                    var searchURL = '/search?type=product&q=' + term;
                    // What's the search results list?
                    var resultsList = form.find('.search-results');
                    // If that's a new term and it contains at least 3 characters.
                    if (term.length > 3 && term != $(this).attr('data-old-term')) {
                        // Saving old query.
                        $(this).attr('data-old-term', term);
                        // Killing any Ajax request that's currently being processed.
                        if (currentAjaxRequest != null) currentAjaxRequest.abort();
                        // Pulling results.
                        currentAjaxRequest = $.getJSON(searchURL + '&view=json', function (data) {
                            // Reset results.
                            resultsList.empty();
                            // If we have no results.
                            if (data.results_count == 0) {
                                // resultsList.html('<li><span class="title">No results.</span></li>');
                                // resultsList.fadeIn(200);
                                resultsList.hide();
                            } else {
                                // If we have results.
                                $.each(data.results, function (index, item) {
                                    var link = $('<a class="d-flex"></a>').attr('href', item.url);
                                    link.append('<div class="thumbnail"><img src="' + item.thumbnail + '" /></div>');
                                    link.append('<div class="media-body"><div class="title">' + item.title + '</div><div class="price">' + item.price + '</div></div>');
                                    // link.append('<div class="price">' + item.price + '</div>');
                                    link.wrap('<li></li>');
                                    resultsList.append(link.parent());
                                });
                                // The Ajax request will return at the most 10 results.
                                // If there are more than 10, let's link to the search results page.
                                if (data.results_count > 10) {
                                    resultsList.append('<li><a class="see_all" href="' + searchURL + '">See all results (' + data.results_count + ')</a></li>');
                                }
                                resultsList.fadeIn(200);
                            }
                        });
                    }
                });
            });
            // Clicking outside makes the results disappear.
            $('body').bind('click', function () {
                $('.search-results').hide();
            });
        });

        //Plus & Minus for Quantity product
        $(document).ready(function () {
            var quantity = 1;

            $('.quantity-right-plus').click(function (e) {
                e.preventDefault();
                var quantity = parseInt($('#quantity').val());
                $('#quantity').val(quantity + 1);
            });

            $('.quantity-left-minus').click(function (e) {
                e.preventDefault();
                var quantity = parseInt($('#quantity').val());
                if (quantity > 1) {
                    $('#quantity').val(quantity - 1);
                }
            });

        });

        // Example starter JavaScript for disabling form submissions if there are invalid fields
        (function () {
            'use strict';

            window.addEventListener('load', function () {
                // Fetch all the forms we want to apply custom Bootstrap validation styles to
                var forms = document.getElementsByClassName('needs-validation');

                // Loop over them and prevent submission
                var validation = Array.prototype.filter.call(forms, function (form) {
                    form.addEventListener('submit', function (event) {
                        if (form.checkValidity() === false) {
                            event.preventDefault();
                            event.stopPropagation();
                        }
                        form.classList.add('was-validated');
                    }, false);
                });
            }, false);
        })();