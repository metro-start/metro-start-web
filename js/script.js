$(function() {
	$('.color').each(function(index, element) {
		if ($(element).prop('id') == 'title-color') {
			$(element).on('input', function(e) {
				$('.' + $(this).prop('id')).css('color', $(this).val());
			});
		} else {
			$(element).on('input', function(e) {
				$('.' + $(this).prop('id')).css('background-color', $(this).val());
			});
		}
		$(element).trigger('input');
	});
});
