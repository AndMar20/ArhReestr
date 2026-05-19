(function () {
    const alertSelector = ".alert";
    const managedClass = "toast-alert";
    const hiddenClass = "toast-hidden";
    const timers = new WeakMap();

    function getVisibleAlerts() {
        return Array.from(document.querySelectorAll(alertSelector))
            .filter((alert) => !alert.classList.contains(hiddenClass));
    }

    function timeoutFor(alert) {
        if (alert.classList.contains("error")) {
            return 10000;
        }

        if (alert.classList.contains("success")) {
            return 6000;
        }

        return 8000;
    }

    function hideAlert(alert) {
        if (alert.classList.contains(hiddenClass)) {
            return;
        }

        const timer = timers.get(alert);
        if (timer) {
            window.clearTimeout(timer);
            timers.delete(alert);
        }

        alert.classList.add("toast-hiding");

        window.setTimeout(() => {
            alert.classList.add(hiddenClass);
            alert.classList.remove("toast-hiding");
            updatePositions();
        }, 180);
    }

    function getAlertMessage(alert) {
        return Array.from(alert.childNodes)
            .filter((node) => !(node.classList && node.classList.contains("toast-close")))
            .map((node) => node.textContent || "")
            .join(" ")
            .replace(/\s+/g, " ")
            .trim();
    }

    function ensureCloseButton(alert) {
        if (alert.querySelector(":scope > .toast-close")) {
            return;
        }

        const closeButton = document.createElement("button");
        closeButton.type = "button";
        closeButton.className = "toast-close";
        closeButton.setAttribute("aria-label", "Close notification");
        closeButton.textContent = "x";
        closeButton.addEventListener("click", () => hideAlert(alert));
        alert.appendChild(closeButton);
    }

    function scheduleHide(alert) {
        if (timers.has(alert)) {
            return;
        }

        const timer = window.setTimeout(() => hideAlert(alert), timeoutFor(alert));
        timers.set(alert, timer);
    }

    function enhanceAlert(alert) {
        if (!(alert instanceof HTMLElement)) {
            return;
        }

        const message = getAlertMessage(alert);
        if (alert.dataset.toastMessage !== message) {
            const timer = timers.get(alert);
            if (timer) {
                window.clearTimeout(timer);
                timers.delete(alert);
            }

            alert.dataset.toastMessage = message;
            alert.classList.remove(hiddenClass, "toast-hiding");
        }

        alert.classList.add(managedClass);
        alert.setAttribute("role", alert.classList.contains("error") ? "alert" : "status");
        ensureCloseButton(alert);
        scheduleHide(alert);
    }

    function updatePositions() {
        const gap = 12;
        const baseBottom = window.innerWidth <= 720 ? 12 : Math.max(16, Math.min(window.innerWidth * 0.04, 32));
        let bottom = baseBottom;

        getVisibleAlerts().reverse().forEach((alert) => {
            enhanceAlert(alert);
            alert.style.bottom = `${bottom}px`;
            bottom += alert.offsetHeight + gap;
        });
    }

    function refresh() {
        document.querySelectorAll(alertSelector).forEach(enhanceAlert);
        updatePositions();
    }

    const observer = new MutationObserver(() => {
        window.requestAnimationFrame(refresh);
    });

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", refresh, { once: true });
    } else {
        refresh();
    }

    observer.observe(document.body, {
        childList: true,
        subtree: true
    });

    window.addEventListener("resize", updatePositions);
})();
