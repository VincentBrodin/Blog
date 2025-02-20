document.addEventListener("DOMContentLoaded", () => {
	let openedAt = Date.now();

	document.addEventListener('htmx:configRequest', function() {
		ShowLoading();
	});

	document.addEventListener('htmx:beforeSwap', function(event) {
		_HideLoading();
		//HideLoading(event);
	});

	function ShowLoading() {
		openedAt = Date.now();
		const div = document.getElementById("loading");
		div.classList.add("overlay")
		div.classList.remove("noOverlay")
	}

	function HideLoading(event) {
		event.detail.isError = false;

		const minLoading = 1000;//ms
		const openedFor = Date.now() - openedAt; // Diff in ms

		if (openedFor < minLoading) {
			const timeout = minLoading - openedFor; // Timeout for atleast 250ms
			setTimeout(() => {
				if (event.detail.target.getAttribute("hx-swap") === "outerHTML") {
					event.detail.target.outerHTML = event.detail.serverResponse;
				} else {
					event.detail.target.innerHTML = event.detail.serverResponse;
				}
				//htmx.process(event.detail.target);
				_HideLoading();
			}, timeout);
			event.detail.shouldSwap = false;
		}
		else {
			_HideLoading();
		}
	}

	function _HideLoading() {
		const div = document.getElementById("loading");
		div.classList.add("noOverlay")
		div.classList.remove("overlay")
	}
});
