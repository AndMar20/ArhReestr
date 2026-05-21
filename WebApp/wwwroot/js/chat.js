window.arhChat = {
    scrollToBottom: function (element) {
        if (!element) {
            return;
        }

        requestAnimationFrame(function () {
            element.scrollTop = element.scrollHeight;
        });
    }
};