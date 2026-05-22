window.arhChat = {
    getValue: function (element) {
        return element ? element.value : "";
    },

    clearValue: function (element) {
        if (element) {
            element.value = "";
        }
    },

    scrollToBottom: function (element) {
        if (!element) {
            return;
        }

        requestAnimationFrame(function () {
            element.scrollTop = element.scrollHeight;
        });
    }
};