$(window).on('load', function (event) {
	$('body').removeClass('preloading');
	$('.load').delay(6000).fadeOut('slow');
});