 document.body.addEventListener("htmx:beforeOnLoad", function(evt) {
    evt.detail.shouldSwap = true;
    evt.detail.isError = false;
  });