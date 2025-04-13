function triggerFlashMessage() {
    const flashDiv = document.getElementById("flashMessage");
    if (!flashDiv) return;

    flashDiv.classList.add("show");

    setTimeout(() => {
        flashDiv.classList.remove("show");
    }, 1000);
}