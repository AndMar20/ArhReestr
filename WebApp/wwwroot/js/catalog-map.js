window.catalogMap = (() => {
    let map;
    let markers = [];

    function ensureMap(elementId) {
        const element = document.getElementById(elementId);
        if (!element || !window.L) {
            return null;
        }

        if (map && !map.getContainer().isConnected) {
            map.remove();
            map = null;
            markers = [];
        }

        if (!map) {
            map = L.map(elementId, {
                scrollWheelZoom: true
            });

            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
                maxZoom: 19,
                attribution: "&copy; OpenStreetMap"
            }).addTo(map);
        }

        setTimeout(() => map.invalidateSize(), 0);
        return map;
    }

    function clearMarkers() {
        markers.forEach(marker => marker.remove());
        markers = [];
    }

    function render(elementId, items) {
        const currentMap = ensureMap(elementId);
        if (!currentMap) {
            return;
        }

        clearMarkers();
        const bounds = [];
        const locationCounts = new Map();

        (items || []).forEach(item => {
            if (item.latitude == null || item.longitude == null) {
                return;
            }

            const point = spreadPoint(item, locationCounts);
            const marker = L.marker(point).addTo(currentMap);
            marker.bindPopup(`
                <strong>${escapeHtml(item.price)}</strong><br>
                <a href="${escapeAttribute(item.url || `/real-estate/${item.id}`)}">${escapeHtml(item.address)}</a><br>
                <span>${escapeHtml(item.details)}</span>
            `);
            marker.on("click", () => marker.openPopup());
            markers.push(marker);
            bounds.push(point);
        });

        if (bounds.length === 1) {
            currentMap.setView(bounds[0], 16);
        } else if (bounds.length > 1) {
            currentMap.fitBounds(bounds, { padding: [28, 28], maxZoom: 16 });
        } else {
            currentMap.setView([55.751244, 37.618423], 11);
        }
    }

    function spreadPoint(item, locationCounts) {
        const latitude = Number(item.latitude);
        const longitude = Number(item.longitude);
        const key = `${latitude.toFixed(7)}:${longitude.toFixed(7)}`;
        const index = locationCounts.get(key) || 0;
        locationCounts.set(key, index + 1);

        if (index === 0) {
            return [latitude, longitude];
        }

        const radius = 0.00008 + Math.floor(index / 8) * 0.00005;
        const angle = (index - 1) * (Math.PI * 2 / 8);
        return [
            latitude + Math.sin(angle) * radius,
            longitude + Math.cos(angle) * radius
        ];
    }

    function escapeHtml(value) {
        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }

    function escapeAttribute(value) {
        return escapeHtml(value).replaceAll("`", "&#096;");
    }

    return { render };
})();
