var OpenEugene = OpenEugene || {};
OpenEugene.LittleHelpBook = OpenEugene.LittleHelpBook || {};
OpenEugene.LittleHelpBook.Providers = OpenEugene.LittleHelpBook.Providers || {};
OpenEugene.LittleHelpBook.Providers.AddressAutofill = (function () {
    const initialized = new WeakSet();
    const stateCodes = {
        alabama: "AL", alaska: "AK", arizona: "AZ", arkansas: "AR", california: "CA",
        colorado: "CO", connecticut: "CT", delaware: "DE", florida: "FL", georgia: "GA",
        hawaii: "HI", idaho: "ID", illinois: "IL", indiana: "IN", iowa: "IA",
        kansas: "KS", kentucky: "KY", louisiana: "LA", maine: "ME", maryland: "MD",
        massachusetts: "MA", michigan: "MI", minnesota: "MN", mississippi: "MS", missouri: "MO",
        montana: "MT", nebraska: "NE", nevada: "NV", "new hampshire": "NH", "new jersey": "NJ",
        "new mexico": "NM", "new york": "NY", "north carolina": "NC", "north dakota": "ND",
        ohio: "OH", oklahoma: "OK", oregon: "OR", pennsylvania: "PA", "rhode island": "RI",
        "south carolina": "SC", "south dakota": "SD", tennessee: "TN", texas: "TX", utah: "UT",
        vermont: "VT", virginia: "VA", washington: "WA", "west virginia": "WV", wisconsin: "WI",
        wyoming: "WY", "district of columbia": "DC"
    };

    function setField(form, field, value) {
        const input = form.querySelector(`[data-address-field="${field}"]`);
        if (!input) return;
        const nextValue = value == null ? "" : String(value);
        if (input.value === nextValue) return;
        input.value = nextValue;
        input.dispatchEvent(new Event("input", { bubbles: true }));
        input.dispatchEvent(new Event("change", { bubbles: true }));
    }

    function normalizeState(value) {
        if (!value) return "";
        const trimmed = value.trim();
        if (trimmed.length <= 2) return trimmed.toUpperCase();
        return stateCodes[trimmed.toLowerCase()] || trimmed.substring(0, 2).toUpperCase();
    }

    function clearCoordinates(form) {
        setField(form, "longitude", "");
        setField(form, "latitude", "");
        setField(form, "geocoding", "");
    }

    function applyFeature(form, feature) {
        if (!form || !feature) return;
        const properties = feature.properties || {};
        const context = properties.context || {};
        const coordinates = feature.geometry && Array.isArray(feature.geometry.coordinates)
            ? feature.geometry.coordinates
            : [];

        setField(form, "address1", properties.address_line1 || properties.name || properties.feature_name || "");
        setField(form, "address2", properties.address_line2 || "");
        setField(form, "city", properties.address_level2 || context.place?.name || "");
        setField(form, "state", normalizeState(properties.address_level1 || context.region?.name));
        setField(form, "postalCode", properties.postcode || context.postcode?.name || "");
        setField(form, "longitude", coordinates.length > 1 ? coordinates[0] : "");
        setField(form, "latitude", coordinates.length > 1 ? coordinates[1] : "");
        setField(form, "geocoding", (properties.mapbox_id || "").substring(0, 64));
    }

    function createSessionToken() {
        if (crypto.randomUUID) return crypto.randomUUID();
        return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
    }

    function hideSuggestions(state) {
        state.container.hidden = true;
        state.list.replaceChildren();
        state.input.setAttribute("aria-expanded", "false");
    }

    function suggestionLabel(suggestion) {
        return [suggestion.name, suggestion.place_formatted].filter(Boolean).join(", ");
    }

    async function retrieveSuggestion(state, suggestion) {
        state.controller?.abort();
        state.controller = new AbortController();
        const id = encodeURIComponent(suggestion.mapbox_id);
        const parameters = new URLSearchParams({
            session_token: state.sessionToken,
            access_token: state.token
        });
        const response = await fetch(`https://api.mapbox.com/search/searchbox/v1/retrieve/${id}?${parameters}`, {
            signal: state.controller.signal
        });
        if (!response.ok) throw new Error(`Mapbox retrieve failed with status ${response.status}.`);

        const result = await response.json();
        state.applying = true;
        applyFeature(state.form, result.features && result.features[0]);
        state.applying = false;
        state.sessionToken = createSessionToken();
        hideSuggestions(state);
        state.input.focus();
    }

    function showSuggestions(state, suggestions) {
        state.list.replaceChildren();
        suggestions.forEach(function (suggestion, index) {
            const item = document.createElement("li");
            item.setAttribute("role", "presentation");
            const button = document.createElement("button");
            button.type = "button";
            button.id = `${state.container.id}-option-${index}`;
            button.className = "lhb-address-suggestion";
            button.setAttribute("role", "option");
            button.textContent = suggestionLabel(suggestion);
            button.addEventListener("click", function () {
                retrieveSuggestion(state, suggestion).catch(function (error) {
                    if (error.name !== "AbortError") console.error(error);
                });
            });
            item.appendChild(button);
            state.list.appendChild(item);
        });
        state.container.hidden = suggestions.length === 0;
        state.input.setAttribute("aria-expanded", suggestions.length ? "true" : "false");
        state.status.textContent = suggestions.length ? "" : "No matching addresses found.";
    }

    async function requestSuggestions(state) {
        const query = state.input.value.trim();
        if (query.length < 3) {
            hideSuggestions(state);
            return;
        }

        state.controller?.abort();
        state.controller = new AbortController();
        const parameters = new URLSearchParams({
            q: query,
            country: "us",
            language: "en",
            types: "address",
            session_token: state.sessionToken,
            access_token: state.token
        });

        try {
            const response = await fetch(`https://api.mapbox.com/search/searchbox/v1/suggest?${parameters}`, {
                signal: state.controller.signal
            });
            if (!response.ok) throw new Error(`Mapbox suggest failed with status ${response.status}.`);
            const result = await response.json();
            state.status.textContent = "";
            showSuggestions(state, result.suggestions || []);
        } catch (error) {
            if (error.name !== "AbortError") {
                hideSuggestions(state);
                state.status.textContent = error.message || "Address suggestions are temporarily unavailable.";
                console.error(error);
            }
        }
    }

    function initializeForm(form) {
        if (initialized.has(form)) return;
        initialized.add(form);
        const token = form.dataset.lhbMapboxToken;
        const input = form.querySelector('[data-address-field="address1"]');
        const container = form.querySelector("[data-lhb-address-suggestions]");
        const status = form.querySelector("[data-lhb-address-status]");
        if (!token || !input || !container || !status) return;

        const state = {
            applying: false,
            container,
            controller: null,
            debounce: null,
            form,
            input,
            list: container.querySelector('[role="listbox"]'),
            sessionToken: createSessionToken(),
            status,
            token
        };

        input.setAttribute("aria-autocomplete", "list");
        input.setAttribute("aria-controls", container.id);
        input.setAttribute("aria-expanded", "false");
        status.textContent = "Address suggestions ready.";
        input.addEventListener("input", function () {
            clearTimeout(state.debounce);
            state.debounce = setTimeout(function () { requestSuggestions(state); }, 250);
        });
        input.addEventListener("change", function () {
            if (!state.applying) clearCoordinates(form);
        });
        input.addEventListener("keydown", function (event) {
            if (event.key === "Escape") {
                hideSuggestions(state);
            } else if (event.key === "ArrowDown" && !container.hidden) {
                const firstOption = state.list.querySelector("button");
                if (firstOption) {
                    event.preventDefault();
                    firstOption.focus();
                }
            }
        });
        container.addEventListener("keydown", function (event) {
            const options = Array.from(state.list.querySelectorAll("button"));
            const index = options.indexOf(document.activeElement);
            if (event.key === "ArrowDown" && index < options.length - 1) {
                event.preventDefault();
                options[index + 1].focus();
            } else if (event.key === "ArrowUp") {
                event.preventDefault();
                if (index > 0) options[index - 1].focus();
                else input.focus();
            } else if (event.key === "Escape") {
                hideSuggestions(state);
                input.focus();
            }
        });
    }

    function initializeAll(root) {
        const scope = root && root.querySelectorAll ? root : document;
        scope.querySelectorAll("[data-lhb-address-form]").forEach(initializeForm);
    }

    function start() {
        initializeAll(document);
        const observer = new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType !== Node.ELEMENT_NODE) return;
                    if (node.matches && node.matches("[data-lhb-address-form]")) initializeForm(node);
                    initializeAll(node);
                });
            });
        });
        observer.observe(document.body, { childList: true, subtree: true });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", start, { once: true });
    } else {
        start();
    }

    return { initializeAll: initializeAll };
})();

OpenEugene.LittleHelpBook.Providers.ProviderMap = (function () {
    const mapStates = new WeakMap();
    const pending = new WeakSet();
    const fallbackCenter = [-123.0868, 44.0521];
    const locationStorageKey = "littleHelpBook.providerLocation.v1";
    const locationMaxAge = 30 * 24 * 60 * 60 * 1000;
    const markerThemes = [
        { keywords: ["emergency", "crisis", "call center"], icon: "☎", color: "#b3261e", label: "Crisis help" },
        { keywords: ["food", "meal", "pantr"], icon: "🍲", color: "#a85d00", label: "Food" },
        { keywords: ["shelter", "housing", "overnight", "parking"], icon: "⌂", color: "#6a4c93", label: "Shelter" },
        { keywords: ["dental"], icon: "🦷", color: "#00796b", label: "Dental care" },
        { keywords: ["vision", "hearing"], icon: "◉", color: "#00695c", label: "Vision or hearing care" },
        { keywords: ["health", "medical", "prescription", "reproductive", "needle"], icon: "✚", color: "#007c91", label: "Health care" },
        { keywords: ["bathroom", "shower"], icon: "🚿", color: "#0277bd", label: "Bathrooms or showers" },
        { keywords: ["laundry", "clothing", "bedding"], icon: "♨", color: "#5c6bc0", label: "Clothing or laundry" },
        { keywords: ["phone", "internet", "library", "mail"], icon: "⌕", color: "#3949ab", label: "Communication" },
        { keywords: ["transport"], icon: "🚌", color: "#455a64", label: "Transportation" },
        { keywords: ["legal", "rights"], icon: "⚖", color: "#4e342e", label: "Legal help" },
        { keywords: ["money", "financial", "rent", "utility"], icon: "$", color: "#2e7d32", label: "Financial help" },
        { keywords: ["veteran"], icon: "★", color: "#546e7a", label: "Veterans" },
        { keywords: ["senior"], icon: "♿", color: "#7b1fa2", label: "Seniors" },
        { keywords: ["youth", "school", "education", "college"], icon: "🎓", color: "#1565c0", label: "Education or youth" },
        { keywords: ["work", "employment", "job"], icon: "▣", color: "#5d4037", label: "Employment" },
        { keywords: ["pet"], icon: "🐾", color: "#8d6e63", label: "Pet care" },
        { keywords: ["wildfire", "evacuation", "smoke"], icon: "🔥", color: "#d84315", label: "Wildfire support" },
        { keywords: ["cultur", "latino", "latinx", "lgbt", "jewish"], icon: "●", color: "#ad1457", label: "Culturally specific services" },
        { keywords: ["post-incarceration"], icon: "↗", color: "#616161", label: "Post-incarceration support" },
        { keywords: ["care", "family"], icon: "♥", color: "#c2185b", label: "Care services" },
        { keywords: ["basic", "camp support", "day storage"], icon: "◆", color: "#37474f", label: "Basic needs" }
    ];

    function parsePins(element) {
        try {
            const pins = JSON.parse(element.dataset.lhbProviderPins || "[]");
            return pins.filter(function (pin) {
                return Number.isFinite(pin.longitude) && Number.isFinite(pin.latitude) &&
                    pin.longitude >= -180 && pin.longitude <= 180 && pin.latitude >= -90 && pin.latitude <= 90;
            });
        } catch (error) {
            console.error("Provider map data could not be parsed.", error);
            return [];
        }
    }

    function markerTheme(pin) {
        const labels = (pin.subcategories || []).concat(pin.categories || []).join(" ").toLowerCase();
        return markerThemes.find(function (theme) {
            return theme.keywords.some(function (keyword) { return labels.includes(keyword); });
        }) || { icon: "•", color: "#2e7d52", label: "Community service" };
    }

    function distanceMiles(from, to) {
        const radians = Math.PI / 180;
        const latitudeDelta = (to[1] - from[1]) * radians;
        const longitudeDelta = (to[0] - from[0]) * radians;
        const a = Math.sin(latitudeDelta / 2) ** 2 +
            Math.cos(from[1] * radians) * Math.cos(to[1] * radians) * Math.sin(longitudeDelta / 2) ** 2;
        return 3958.8 * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    }

    function validCoordinates(coordinates) {
        return Array.isArray(coordinates) && coordinates.length === 2 &&
            Number.isFinite(coordinates[0]) && Number.isFinite(coordinates[1]) &&
            coordinates[0] >= -180 && coordinates[0] <= 180 &&
            coordinates[1] >= -90 && coordinates[1] <= 90;
    }

    function saveUserLocation(location) {
        try {
            localStorage.setItem(locationStorageKey, JSON.stringify({
                coordinates: location.coordinates,
                label: location.label,
                savedAt: Date.now(),
                source: location.source
            }));
        } catch (error) {
            console.warn("The selected location could not be saved in this browser.", error);
        }
    }

    function loadUserLocation() {
        try {
            const location = JSON.parse(localStorage.getItem(locationStorageKey) || "null");
            if (!location || !validCoordinates(location.coordinates) ||
                typeof location.label !== "string" || !location.label.trim() ||
                !Number.isFinite(location.savedAt) || Date.now() - location.savedAt > locationMaxAge) {
                localStorage.removeItem(locationStorageKey);
                return null;
            }
            return location;
        } catch (error) {
            try { localStorage.removeItem(locationStorageKey); } catch { }
            return null;
        }
    }

    function removeSavedUserLocation() {
        try {
            localStorage.removeItem(locationStorageKey);
        } catch (error) {
            console.warn("The saved location could not be removed from this browser.", error);
        }
    }

    function createPopup(state, pin, theme) {
        const popup = document.createElement("div");
        const name = document.createElement("strong");
        name.textContent = pin.name || "Provider";
        popup.appendChild(name);

        if (pin.address) {
            const address = document.createElement("div");
            address.className = "lhb-provider-map-address";
            address.textContent = pin.address;
            popup.appendChild(address);
        }

        const services = (pin.subcategories || []).length ? pin.subcategories : (pin.categories || []);
        if (services.length) {
            const serviceList = document.createElement("div");
            serviceList.className = "lhb-provider-map-services";
            serviceList.textContent = `${theme.label}: ${services.join(", ")}`;
            popup.appendChild(serviceList);
        }

        if (state.userLocation) {
            const distance = document.createElement("div");
            distance.className = "lhb-provider-map-distance";
            distance.textContent = `About ${distanceMiles(state.userLocation.coordinates, [pin.longitude, pin.latitude]).toFixed(1)} miles away`;
            popup.appendChild(distance);
        }

        const link = document.createElement("a");
        link.href = pin.url;
        link.textContent = "View provider details";
        popup.appendChild(link);
        return popup;
    }

    function createProviderMarker(pin, theme) {
        const marker = document.createElement("button");
        marker.type = "button";
        marker.className = "lhb-provider-map-marker";
        marker.setAttribute("aria-label", `${theme.label}: ${pin.name || "Provider"}, ${pin.address || "mapped location"}`);
        marker.title = `${pin.name || "Provider"} — ${theme.label}`;

        const pinShape = document.createElement("span");
        pinShape.className = "lhb-provider-map-marker-shape";
        pinShape.style.backgroundColor = theme.color;
        const icon = document.createElement("span");
        icon.className = "lhb-provider-map-marker-icon";
        icon.setAttribute("aria-hidden", "true");
        icon.textContent = theme.icon;
        pinShape.appendChild(icon);
        marker.appendChild(pinShape);
        return marker;
    }

    function formatDistance(miles) {
        if (miles < 0.1) return "Less than 0.1 miles away";
        return `${miles.toFixed(1)} ${miles < 1.05 ? "mile" : "miles"} away`;
    }

    function nearbyProviders(state, pins) {
        const ranked = pins.map(function (pin) {
            return {
                distance: state.userLocation
                    ? distanceMiles(state.userLocation.coordinates, [pin.longitude, pin.latitude])
                    : null,
                pin: pin
            };
        });
        ranked.sort(function (left, right) {
            if (left.distance !== null && right.distance !== null) return left.distance - right.distance;
            return (left.pin.name || "").localeCompare(right.pin.name || "");
        });

        const providerIds = new Set();
        return ranked.filter(function (item) {
            const providerId = item.pin.providerId || item.pin.name;
            if (providerIds.has(providerId)) return false;
            providerIds.add(providerId);
            return true;
        });
    }

    function focusProvider(state, pin) {
        const entry = state.markerEntries.find(function (candidate) { return candidate.pin === pin; });
        if (!entry) return;
        state.markerEntries.forEach(function (candidate) {
            const popup = candidate.marker.getPopup();
            if (candidate !== entry && popup?.isOpen()) candidate.marker.togglePopup();
        });
        state.map.easeTo({ center: [pin.longitude, pin.latitude], zoom: Math.max(state.map.getZoom(), 14) });
        if (!entry.marker.getPopup()?.isOpen()) entry.marker.togglePopup();
        entry.element.focus({ preventScroll: true });
    }

    function renderNearbyPage(state) {
        if (!state.nearby?.list) return;
        const providers = state.nearby.providers || [];
        const total = providers.length;
        const pageCount = Math.max(1, Math.ceil(total / state.nearby.pageSize));
        state.nearby.page = Math.min(Math.max(state.nearby.page, 0), pageCount - 1);
        const start = state.nearby.page * state.nearby.pageSize;
        const end = Math.min(start + state.nearby.pageSize, total);
        state.nearby.list.replaceChildren();
        if (state.nearby.status) {
            state.nearby.status.textContent = total === 0 ? "0 providers" : `${start + 1}-${end} of ${total}`;
        }
        if (state.nearby.previous) state.nearby.previous.disabled = state.nearby.page === 0;
        if (state.nearby.next) state.nearby.next.disabled = end >= total;

        if (total === 0) {
            const emptyRow = document.createElement("tr");
            const emptyCell = document.createElement("td");
            emptyCell.textContent = "No mapped providers match these needs.";
            emptyRow.appendChild(emptyCell);
            state.nearby.list.appendChild(emptyRow);
            return;
        }

        providers.slice(start, end).forEach(function (item) {
            const pin = item.pin;
            const theme = markerTheme(pin);
            const row = document.createElement("tr");
            row.tabIndex = 0;
            row.setAttribute("role", "button");
            row.setAttribute("aria-label", `Show ${pin.name || "provider"} on map`);
            row.addEventListener("click", function () { focusProvider(state, pin); });
            row.addEventListener("keydown", function (event) {
                if (event.key !== "Enter" && event.key !== " ") return;
                event.preventDefault();
                focusProvider(state, pin);
            });

            const cell = document.createElement("td");
            const rowContent = document.createElement("div");
            rowContent.className = "d-flex align-center gap-3 py-1";

            const avatar = document.createElement("div");
            avatar.className = "mud-avatar mud-avatar-medium mud-elevation-1";
            avatar.style.backgroundColor = theme.color;
            avatar.style.color = "white";
            avatar.setAttribute("aria-hidden", "true");
            avatar.textContent = theme.icon;

            const content = document.createElement("div");
            content.className = "flex-grow-1";
            const name = document.createElement("a");
            name.className = "mud-typography mud-typography-subtitle2";
            name.href = pin.url;
            name.textContent = pin.name || "Provider";
            name.addEventListener("click", function (event) { event.stopPropagation(); });
            content.appendChild(name);

            if (item.distance !== null) {
                const distance = document.createElement("p");
                distance.className = "mud-typography mud-typography-caption mud-primary-text";
                distance.textContent = formatDistance(item.distance);
                content.appendChild(distance);
            }
            if (pin.address) {
                const address = document.createElement("p");
                address.className = "mud-typography mud-typography-caption";
                address.textContent = pin.address;
                content.appendChild(address);
            }

            const services = (pin.subcategories || []).length ? pin.subcategories : (pin.categories || []);
            if (services.length) {
                const serviceList = document.createElement("p");
                serviceList.className = "mud-typography mud-typography-caption";
                serviceList.textContent = services.join(" · ");
                content.appendChild(serviceList);
            }

            rowContent.append(avatar, content);
            cell.appendChild(rowContent);
            row.appendChild(cell);
            state.nearby.list.appendChild(row);
        });

        const scrollContainer = state.nearby.list.closest(".mud-table-container");
        if (scrollContainer) scrollContainer.scrollTop = 0;
    }

    function renderNearbyList(state, pins) {
        if (!state.nearby?.list || !state.nearby?.intro) return;
        const providers = nearbyProviders(state, pins);
        state.nearby.providers = providers;
        state.nearby.page = 0;
        state.nearby.intro.textContent = state.userLocation
            ? `${providers.length} mapped ${providers.length === 1 ? "provider" : "providers"} nearest ${state.userLocation.label}.`
            : "Choose a location to sort providers by distance.";
        renderNearbyPage(state);
    }

    function setViewport(state, pins) {
        if (state.userLocation) {
            const nearbyPins = pins.filter(function (pin) {
                return distanceMiles(state.userLocation.coordinates, [pin.longitude, pin.latitude]) <= 25;
            });
            if (nearbyPins.length) {
                const nearbyBounds = new mapboxgl.LngLatBounds();
                nearbyBounds.extend(state.userLocation.coordinates);
                nearbyPins.forEach(function (pin) { nearbyBounds.extend([pin.longitude, pin.latitude]); });
                state.map.fitBounds(nearbyBounds, { padding: 56, maxZoom: 13, duration: 0 });
            } else {
                state.map.easeTo({ center: state.userLocation.coordinates, zoom: 11 });
            }
            return;
        }
        if (pins.length === 0) {
            state.map.easeTo({ center: fallbackCenter, zoom: 10 });
            return;
        }
        if (pins.length === 1) {
            state.map.easeTo({ center: [pins[0].longitude, pins[0].latitude], zoom: 13 });
            return;
        }

        const bounds = new mapboxgl.LngLatBounds();
        pins.forEach(function (pin) { bounds.extend([pin.longitude, pin.latitude]); });
        state.map.fitBounds(bounds, { padding: 48, maxZoom: 13, duration: 0 });
    }

    function updateMarkers(element) {
        const state = mapStates.get(element);
        if (!state) return;

        state.markers.forEach(function (marker) { marker.remove(); });
        state.markers = [];
        state.markerEntries = [];
        const pins = parsePins(element);

        pins.forEach(function (pin) {
            const theme = markerTheme(pin);
            const markerElement = createProviderMarker(pin, theme);
            const popup = new mapboxgl.Popup({
                offset: 28,
                maxWidth: "18rem",
                focusAfterOpen: false,
                closeButton: true
            }).setDOMContent(createPopup(state, pin, theme));
            popup.on("open", function () {
                state.map.easeTo({
                    center: [pin.longitude, pin.latitude],
                    duration: 250
                });
            });
            const marker = new mapboxgl.Marker({ element: markerElement, anchor: "bottom" })
                .setLngLat([pin.longitude, pin.latitude])
                .setPopup(popup)
                .addTo(state.map);
            state.markers.push(marker);
            state.markerEntries.push({ element: markerElement, marker: marker, pin: pin });
        });

        setViewport(state, pins);
        renderNearbyList(state, pins);
    }

    function featureLabel(feature) {
        const properties = feature?.properties || {};
        return properties.full_address || [properties.name_preferred || properties.name, properties.place_formatted]
            .filter(Boolean).join(", ") || "your selected area";
    }

    function hideLocationSuggestions(state) {
        if (!state.controls?.suggestions || !state.controls?.suggestionList) return;
        state.controls.suggestions.hidden = true;
        state.controls.suggestionList.replaceChildren();
        state.controls.input.setAttribute("aria-expanded", "false");
    }

    function chooseLocationSuggestion(state, feature) {
        const coordinates = feature?.geometry?.coordinates;
        if (!validCoordinates(coordinates)) return;
        hideLocationSuggestions(state);
        setUserLocation(state, coordinates, featureLabel(feature), { source: "manual" });
        state.controls.input.focus();
    }

    function showLocationSuggestions(state, features) {
        state.controls.suggestionList.replaceChildren();
        features.forEach(function (feature, index) {
            const item = document.createElement("li");
            item.setAttribute("role", "presentation");
            const button = document.createElement("button");
            button.type = "button";
            button.id = `${state.controls.suggestions.id}-option-${index}`;
            button.className = "lhb-location-suggestion";
            button.setAttribute("role", "option");
            button.textContent = featureLabel(feature);
            button.addEventListener("click", function () { chooseLocationSuggestion(state, feature); });
            button.addEventListener("keydown", function (event) {
                if (event.key === "Escape") {
                    event.preventDefault();
                    hideLocationSuggestions(state);
                    state.controls.input.focus();
                } else if (event.key === "ArrowDown") {
                    event.preventDefault();
                    (button.parentElement?.nextElementSibling?.querySelector("button") || button).focus();
                } else if (event.key === "ArrowUp") {
                    event.preventDefault();
                    const previous = button.parentElement?.previousElementSibling?.querySelector("button");
                    if (previous) previous.focus();
                    else state.controls.input.focus();
                }
            });
            item.appendChild(button);
            state.controls.suggestionList.appendChild(item);
        });
        state.controls.suggestions.hidden = features.length === 0;
        state.controls.input.setAttribute("aria-expanded", features.length ? "true" : "false");
    }

    async function requestLocationSuggestions(state) {
        const query = state.controls.input.value.trim();
        if (query.length < 3) {
            hideLocationSuggestions(state);
            return;
        }

        state.locationSuggestionController?.abort();
        state.locationSuggestionController = new AbortController();
        state.lastLocationSuggestionQuery = query;
        const parameters = new URLSearchParams({
            q: query,
            country: "us",
            language: "en",
            types: "address,street,postcode,place,locality,neighborhood",
            proximity: fallbackCenter.join(","),
            autocomplete: "true",
            limit: "5",
            access_token: state.token
        });
        try {
            const response = await fetch(`https://api.mapbox.com/search/geocode/v6/forward?${parameters}`, {
                signal: state.locationSuggestionController.signal
            });
            if (!response.ok) throw new Error(`Location suggestions failed with status ${response.status}.`);
            const result = await response.json();
            if (state.controls.input.value.trim() !== query) return;
            const features = (result.features || []).filter(function (feature) {
                return validCoordinates(feature?.geometry?.coordinates);
            });
            showLocationSuggestions(state, features);
            if (features.length === 0) state.controls.status.textContent = "No matching locations found.";
        } catch (error) {
            if (error.name !== "AbortError") {
                hideLocationSuggestions(state);
                state.controls.status.textContent = "Location suggestions are temporarily unavailable. You can still set the typed location.";
                console.warn("Location suggestions were unavailable.", error);
            }
        }
    }

    function setUserLocation(state, coordinates, label, options) {
        const settings = options || {};
        hideLocationSuggestions(state);
        state.userMarker?.remove();
        state.userLocation = {
            coordinates: coordinates,
            label: label,
            source: settings.source || "manual"
        };
        const markerElement = document.createElement("div");
        markerElement.className = "lhb-user-location-marker";
        markerElement.setAttribute("role", "img");
        markerElement.setAttribute("aria-label", `Your approximate location: ${label}`);
        state.userMarker = new mapboxgl.Marker({ element: markerElement })
            .setLngLat(coordinates)
            .setPopup(new mapboxgl.Popup({ offset: 16 }).setText(`Your approximate location: ${label}`))
            .addTo(state.map);
        state.controls.input.value = label;
        if (settings.restored) {
            state.controls.status.textContent = `Using your saved location near ${label}. Now choose what you need.`;
        } else if (state.userLocation.source === "ip") {
            state.controls.status.textContent = `We estimated your area from your IP address as ${label}. Enter a location for more precise results.`;
        } else {
            state.controls.status.textContent = `You are near ${label}. Now choose what you need.`;
        }
        state.controls.clear.hidden = false;
        if (settings.persist !== false) saveUserLocation(state.userLocation);
        updateMarkers(state.element);
    }

    function restoreUserLocation(state) {
        const location = loadUserLocation();
        if (!location) return false;
        setUserLocation(state, location.coordinates, location.label, {
            persist: false,
            restored: true,
            source: location.source || "manual"
        });
        return true;
    }

    async function locateByIp(state) {
        if (state.userLocation) return;
        state.controls.status.textContent = "Estimating your area from your IP address…";
        const parameters = new URLSearchParams({
            fields: "success,message,country,region,city,latitude,longitude"
        });
        try {
            const response = await fetch(`https://ipwho.is/?${parameters}`);
            if (!response.ok) throw new Error(`IP location failed with status ${response.status}.`);
            const result = await response.json();
            const coordinates = [Number(result.longitude), Number(result.latitude)];
            if (!result.success || !validCoordinates(coordinates)) {
                throw new Error(result.message || "IP location was unavailable.");
            }
            const label = [result.city, result.region, result.country].filter(Boolean).join(", ") || "your approximate area";
            setUserLocation(state, coordinates, label, { source: "ip" });
        } catch (error) {
            state.controls.status.textContent = "We could not estimate your area from your IP address. Enter a city, ZIP code, or address instead.";
            console.warn("Approximate IP location was unavailable.", error);
        }
    }

    async function findLocationByText(state) {
        hideLocationSuggestions(state);
        const query = state.controls.input.value.trim();
        if (query.length < 2) {
            state.controls.status.textContent = "Enter an address, city, or ZIP code.";
            state.controls.input.focus();
            return;
        }

        state.controls.search.disabled = true;
        state.controls.status.textContent = "Finding that location…";
        const parameters = new URLSearchParams({
            q: query,
            country: "us",
            language: "en",
            types: "address,street,postcode,place,locality,neighborhood",
            proximity: fallbackCenter.join(","),
            autocomplete: "false",
            limit: "1",
            access_token: state.token
        });
        try {
            const response = await fetch(`https://api.mapbox.com/search/geocode/v6/forward?${parameters}`);
            if (!response.ok) throw new Error(`Location search failed with status ${response.status}.`);
            const result = await response.json();
            const feature = result.features && result.features[0];
            const coordinates = feature?.geometry?.coordinates;
            if (!feature || !Array.isArray(coordinates)) {
                state.controls.status.textContent = "We could not find that location. Try a city, ZIP code, or full address.";
                return;
            }
            setUserLocation(state, coordinates, featureLabel(feature), { source: "manual" });
        } catch (error) {
            state.controls.status.textContent = "Location search is temporarily unavailable. You can still browse by need.";
            console.error(error);
        } finally {
            state.controls.search.disabled = false;
        }
    }

    async function approximateLocationLabel(state, coordinates) {
        const parameters = new URLSearchParams({
            longitude: coordinates[0],
            latitude: coordinates[1],
            language: "en",
            types: "neighborhood,locality,place,postcode",
            access_token: state.token
        });
        try {
            const response = await fetch(`https://api.mapbox.com/search/geocode/v6/reverse?${parameters}`);
            if (!response.ok) return "your current area";
            const result = await response.json();
            return result.features?.length ? featureLabel(result.features[0]) : "your current area";
        } catch (error) {
            console.error(error);
            return "your current area";
        }
    }

    function useDeviceLocation(state) {
        if (!navigator.geolocation) {
            state.controls.status.textContent = "This browser does not support device location. Enter a location instead.";
            return;
        }
        state.controls.device.disabled = true;
        state.controls.status.textContent = "Waiting for your browser’s location permission…";
        navigator.geolocation.getCurrentPosition(async function (position) {
            const coordinates = [position.coords.longitude, position.coords.latitude];
            const label = await approximateLocationLabel(state, coordinates);
            setUserLocation(state, coordinates, label, { source: "device" });
            state.controls.device.disabled = false;
        }, function (error) {
            state.controls.device.disabled = false;
            if (error.code === error.PERMISSION_DENIED) {
                state.controls.status.textContent = "Location permission was not granted. Enter a city, ZIP code, or address instead.";
            } else if (error.code === error.TIMEOUT) {
                state.controls.status.textContent = "Your device took too long to provide a location. Try again or enter one instead.";
            } else {
                state.controls.status.textContent = "Your device location is unavailable. Enter a location instead.";
            }
        }, { enableHighAccuracy: false, timeout: 10000, maximumAge: 300000 });
    }

    function clearUserLocation(state) {
        hideLocationSuggestions(state);
        state.userMarker?.remove();
        state.userMarker = null;
        state.userLocation = null;
        removeSavedUserLocation();
        state.controls.input.value = "";
        state.controls.clear.hidden = true;
        state.controls.status.textContent = "Location cleared from this browser. Enter a location, use your device, or refresh to estimate your area from your IP address.";
        updateMarkers(state.element);
        state.controls.input.focus();
    }

    function bindLocationControls(state) {
        const shell = state.element.closest("[data-lhb-provider-discovery]");
        if (!shell) return;
        const controls = {
            input: shell.querySelector("[data-lhb-user-location-input]"),
            search: shell.querySelector("[data-lhb-location-search]"),
            device: shell.querySelector("[data-lhb-device-location]"),
            clear: shell.querySelector("[data-lhb-clear-location]"),
            status: shell.querySelector("[data-lhb-user-location-status]"),
            suggestions: shell.querySelector("[data-lhb-location-suggestions]"),
            suggestionList: shell.querySelector("[data-lhb-location-suggestions] ul")
        };
        if (Object.values(controls).some(function (control) { return !control; })) return;
        state.controls = controls;
        state.nearby = {
            intro: shell.querySelector("[data-lhb-nearby-intro]"),
            list: shell.querySelector("[data-lhb-nearby-list]"),
            next: shell.querySelector("[data-lhb-nearby-next]"),
            page: 0,
            pageSize: 10,
            previous: shell.querySelector("[data-lhb-nearby-previous]"),
            providers: [],
            status: shell.querySelector("[data-lhb-nearby-page-status]")
        };
        state.nearby.previous?.addEventListener("click", function () {
            if (state.nearby.page === 0) return;
            state.nearby.page -= 1;
            renderNearbyPage(state);
        });
        state.nearby.next?.addEventListener("click", function () {
            if ((state.nearby.page + 1) * state.nearby.pageSize >= state.nearby.providers.length) return;
            state.nearby.page += 1;
            renderNearbyPage(state);
        });
        controls.search.addEventListener("click", function () { findLocationByText(state); });
        controls.device.addEventListener("click", function () { useDeviceLocation(state); });
        controls.clear.addEventListener("click", function () { clearUserLocation(state); });
        controls.input.addEventListener("input", function () {
            clearTimeout(state.locationSuggestionTimer);
            state.locationSuggestionTimer = setTimeout(function () { requestLocationSuggestions(state); }, 250);
        });
        controls.input.addEventListener("keydown", function (event) {
            if (event.key === "ArrowDown" && !controls.suggestions.hidden) {
                event.preventDefault();
                controls.suggestionList.querySelector("button")?.focus();
            } else if (event.key === "Escape") {
                hideLocationSuggestions(state);
            } else if (event.key === "Enter") {
                event.preventDefault();
                findLocationByText(state);
            }
        });
        controls.input.closest(".lhb-location-searchbox")?.addEventListener("focusout", function () {
            setTimeout(function () {
                if (!controls.input.closest(".lhb-location-searchbox")?.contains(document.activeElement)) {
                    hideLocationSuggestions(state);
                }
            }, 0);
        });
    }

    function initializeMap(element, attempts) {
        if (mapStates.has(element) || pending.has(element)) return;
        if (!window.mapboxgl) {
            if (attempts < 100) {
                pending.add(element);
                setTimeout(function () {
                    pending.delete(element);
                    initializeMap(element, attempts + 1);
                }, 100);
            } else {
                const status = element.parentElement?.querySelector("[data-lhb-provider-map-status]");
                if (status) status.textContent = "The provider map could not load.";
            }
            return;
        }

        const token = element.dataset.lhbMapboxToken;
        if (!token) return;
        mapboxgl.accessToken = token;
        const map = new mapboxgl.Map({
            container: element,
            style: "mapbox://styles/mapbox/streets-v12",
            center: fallbackCenter,
            zoom: 10,
            cooperativeGestures: true
        });
        const state = {
            controls: null,
            element: element,
            map: map,
            markers: [],
            markerEntries: [],
            nearby: null,
            lastLocationSuggestionQuery: "",
            locationSuggestionController: null,
            locationSuggestionTimer: null,
            token: token,
            userLocation: null,
            userMarker: null
        };
        mapStates.set(element, state);
        if (window.ResizeObserver) {
            state.resizeObserver = new ResizeObserver(function () {
                map.resize();
            });
            state.resizeObserver.observe(element);
        }
        map.addControl(new mapboxgl.NavigationControl(), "top-right");
        map.on("load", function () {
            bindLocationControls(state);
            if (!state.controls) {
                updateMarkers(element);
            } else if (!restoreUserLocation(state)) {
                updateMarkers(element);
                locateByIp(state);
            }
            element.removeAttribute("aria-busy");
            map.resize();
        });
        map.on("error", function (event) {
            console.error("Provider map error.", event.error || event);
        });
        element.setAttribute("aria-busy", "true");
    }

    function initializeAll(root) {
        const scope = root && root.querySelectorAll ? root : document;
        scope.querySelectorAll("[data-lhb-provider-map]").forEach(function (element) {
            initializeMap(element, 0);
        });
    }

    function start() {
        initializeAll(document);
        const observer = new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                if (mutation.type === "attributes" && mutation.attributeName === "data-lhb-provider-pins") {
                    updateMarkers(mutation.target);
                    return;
                }
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType !== Node.ELEMENT_NODE) return;
                    if (node.matches && node.matches("[data-lhb-provider-map]")) initializeMap(node, 0);
                    initializeAll(node);
                });
            });
        });
        observer.observe(document.body, {
            attributes: true,
            attributeFilter: ["data-lhb-provider-pins"],
            childList: true,
            subtree: true
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", start, { once: true });
    } else {
        start();
    }

    return { initializeAll: initializeAll };
})();
