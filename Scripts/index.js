document.querySelectorAll('.gallery').forEach(function (g) {
	lightGallery(g, {
		selector: 'a[href*=".jpg"], a[href*=".png"], [data-src]'
	});
});