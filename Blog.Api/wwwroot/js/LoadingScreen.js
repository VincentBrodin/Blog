document.addEventListener('htmx:configRequest', function () {
    ShowLoading();
});

document.addEventListener('htmx:afterRequest', function (event) {
    HideLoading();
});

function ShowLoading() {
    const div = document.getElementById("loading");
    div.classList.add("overlay")
    div.classList.remove("noOverlay")
}

function HideLoading() {
    const div = document.getElementById("loading");
    div.classList.add("noOverlay")
    div.classList.remove("overlay")
}