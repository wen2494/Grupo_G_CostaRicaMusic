(() => {
    const storageKey = "crmusic.player.state";

    const player = document.getElementById("musicPlayer");
    const playCollectionBtn = document.getElementById("playCollectionBtn");
    const playPauseBtn = document.getElementById("playPauseBtn");
    const nextTrackBtn = document.getElementById("nextTrackBtn");
    const prevTrackBtn = document.getElementById("prevTrackBtn");
    const shuffleBtn = document.getElementById("shuffleBtn");
    const repeatBtn = document.getElementById("repeatBtn");
    const seekBar = document.getElementById("seekBar");
    const volumeBar = document.getElementById("volumeBar");
    const currentTimeLabel = document.getElementById("currentTimeLabel");
    const durationLabel = document.getElementById("durationLabel");
    const queueCount = document.getElementById("queueCount");
    const upNextList = document.getElementById("upNextList");
    const sideArtwork = document.getElementById("sideArtwork");
    const globalArtwork = document.getElementById("globalArtwork");

    if (!player || !playCollectionBtn || !playPauseBtn || !nextTrackBtn || !prevTrackBtn || !shuffleBtn || !repeatBtn || !seekBar || !volumeBar || !currentTimeLabel || !durationLabel || !queueCount || !upNextList || !sideArtwork || !globalArtwork) {
        return;
    }

    const titleElements = [document.getElementById("globalNowPlayingTitle"), document.getElementById("sideNowPlayingTitle")].filter(Boolean);
    const artistElements = [document.getElementById("globalNowPlayingArtist"), document.getElementById("sideNowPlayingArtist")].filter(Boolean);
    const albumElements = [document.getElementById("globalNowPlayingAlbum"), document.getElementById("sideNowPlayingAlbum")].filter(Boolean);

    let trackButtons = [];
    let queueButtons = [];
    let currentTrackId = "";
    let queueSelector = null;
    let pendingSeekTime = null;
    let shuffleEnabled = false;
    let repeatEnabled = false;
    let isNavigating = false;

    const formatTime = (value) => {
        if (!Number.isFinite(value) || value < 0) {
            return "0:00";
        }

        const minutes = Math.floor(value / 60);
        const seconds = Math.floor(value % 60);
        return `${minutes}:${seconds.toString().padStart(2, "0")}`;
    };

    const getCurrentButton = () => {
        if (currentTrackId) {
            return trackButtons.find((button) => button.dataset.trackId === currentTrackId) || null;
        }

        const src = player.getAttribute("src");
        return src ? trackButtons.find((button) => button.dataset.src === src) || null : null;
    };

    const triggerArtworkPulse = () => {
        [sideArtwork, globalArtwork].forEach((element) => {
            element.classList.remove("artwork-pulse");
            void element.offsetWidth;
            element.classList.add("artwork-pulse");
        });
    };

    const setToggleState = (button, isActive) => {
        button.classList.toggle("is-toggled", isActive);
        button.setAttribute("aria-pressed", isActive ? "true" : "false");
    };

    const syncModeButtons = () => {
        setToggleState(shuffleBtn, shuffleEnabled);
        setToggleState(repeatBtn, repeatEnabled);
    };

    const readState = () => {
        try {
            const raw = localStorage.getItem(storageKey);
            return raw ? JSON.parse(raw) : null;
        } catch {
            return null;
        }
    };

    const setLabels = (title, artist, album) => {
        titleElements.forEach((element) => {
            element.textContent = title || "Selecciona una cancion";
        });

        artistElements.forEach((element) => {
            element.textContent = artist || "-";
        });

        albumElements.forEach((element) => {
            element.textContent = album || "-";
        });
    };

    const setPlayIcon = (isPlaying) => {
        const icon = playPauseBtn.querySelector("i");
        if (icon) {
            icon.className = isPlaying ? "bi bi-pause-fill" : "bi bi-play-fill";
        }
    };

    const updateArtwork = (title, artist, cover) => {
        const initials = `${title || ""} ${artist || ""}`
            .trim()
            .split(/\s+/)
            .slice(0, 2)
            .map((part) => part[0]?.toUpperCase() ?? "")
            .join("") || "CRM";

        [sideArtwork, globalArtwork].forEach((element) => {
            element.textContent = initials;

            if (cover) {
                element.style.backgroundImage = `linear-gradient(rgba(16, 17, 19, 0.12), rgba(16, 17, 19, 0.34)), url("${cover}")`;
                element.classList.add("has-cover");
            } else {
                element.style.backgroundImage = "";
                element.classList.remove("has-cover");
            }
        });
    };

    const markTrackState = () => {
        const isPlaying = !player.paused;
        const src = player.getAttribute("src") || "";

        trackButtons.forEach((button) => {
            const isCurrent = (currentTrackId && button.dataset.trackId === currentTrackId) || (!currentTrackId && button.dataset.src === src);
            button.classList.toggle("is-selected", isCurrent);
            button.classList.toggle("is-playing", isCurrent && isPlaying);
        });
    };

    const persistState = () => {
        const activeButton = getCurrentButton();
        localStorage.setItem(storageKey, JSON.stringify({
            src: player.getAttribute("src") || "",
            title: activeButton?.dataset.title || titleElements[0]?.textContent || "",
            artist: activeButton?.dataset.artist || artistElements[0]?.textContent || "",
            album: activeButton?.dataset.album || albumElements[0]?.textContent || "",
            cover: activeButton?.dataset.cover || "",
            trackId: currentTrackId || activeButton?.dataset.trackId || "",
            queueSelector,
            currentTime: Number.isFinite(player.currentTime) ? player.currentTime : 0,
            volume: Number(volumeBar.value),
            shuffleEnabled,
            repeatEnabled
        }));
    };

    const renderUpNext = () => {
        queueCount.textContent = `${queueButtons.length} canciones`;
        upNextList.innerHTML = "";

        if (!queueButtons.length) {
            const empty = document.createElement("p");
            empty.className = "small-note";
            empty.textContent = "Sin cola.";
            upNextList.appendChild(empty);
            return;
        }

        const currentButton = getCurrentButton();
        const activeQueueIndex = currentButton ? queueButtons.findIndex((button) => button.dataset.trackId === currentButton.dataset.trackId && button.dataset.src === currentButton.dataset.src) : -1;
        const orderedButtons = activeQueueIndex >= 0
            ? [...queueButtons.slice(activeQueueIndex + 1), ...queueButtons.slice(0, activeQueueIndex + 1)]
            : queueButtons;

        orderedButtons.slice(0, 6).forEach((button, index) => {
            const item = document.createElement("button");
            const cover = button.dataset.cover || "";
            item.type = "button";
            item.className = "up-next-item";

            const coverElement = document.createElement("span");
            coverElement.className = "up-next-cover";
            if (cover) {
                coverElement.style.backgroundImage = `url("${cover}")`;
            }

            const copy = document.createElement("span");
            copy.className = "up-next-copy";

            const title = document.createElement("strong");
            title.textContent = button.dataset.title || "";

            const artist = document.createElement("small");
            artist.textContent = button.dataset.artist || "";

            const marker = document.createElement("span");
            marker.className = "up-next-index";
            marker.textContent = index === 0 && activeQueueIndex >= 0 ? "Ahora" : `#${index + 1}`;

            copy.append(title, artist, marker);
            item.append(coverElement, copy);
            item.addEventListener("click", () => loadTrack(button, true));
            upNextList.appendChild(item);
        });
    };

    const setQueue = (selector) => {
        queueSelector = selector;
        queueButtons = selector
            ? Array.from(document.querySelectorAll(`${selector} .track-btn, ${selector}.track-btn`))
            : trackButtons;

        if (!queueButtons.length) {
            queueButtons = trackButtons;
            queueSelector = null;
        }

        renderUpNext();
    };

    const loadTrack = (button, autoplay) => {
        if (!button) {
            return;
        }

        const nextTrackId = button.dataset.trackId || "";
        const nextSrc = button.dataset.src || "";
        const isNewTrack = currentTrackId !== nextTrackId || player.getAttribute("src") !== nextSrc;
        currentTrackId = nextTrackId;

        const closestCollection = button.closest(".track-list, .playlist-song-list, .hero-gallery");
        if (closestCollection) {
            setQueue(`.${Array.from(closestCollection.classList).join(".")}`);
        }

        if (isNewTrack) {
            pendingSeekTime = null;
            player.src = nextSrc;
            player.load();
        }

        setLabels(button.dataset.title || "", button.dataset.artist || "", button.dataset.album || "");
        updateArtwork(button.dataset.title || "", button.dataset.artist || "", button.dataset.cover || "");
        triggerArtworkPulse();
        seekBar.value = "0";
        currentTimeLabel.textContent = "0:00";
        durationLabel.textContent = "0:00";
        if (isNewTrack) {
            try {
                player.currentTime = 0;
            } catch {
                pendingSeekTime = 0;
            }
        }
        markTrackState();
        persistState();
        renderUpNext();

        if (autoplay) {
            player.play().catch(() => setPlayIcon(false));
        }
    };

    const moveInQueue = (offset) => {
        if (!queueButtons.length) {
            return;
        }

        if (shuffleEnabled && queueButtons.length > 1) {
            const currentButton = getCurrentButton();
            const candidateButtons = queueButtons.filter((button) => button !== currentButton);
            loadTrack(candidateButtons[Math.floor(Math.random() * candidateButtons.length)], true);
            return;
        }

        const currentButton = getCurrentButton();
        const queueIndex = currentButton ? queueButtons.findIndex((button) => button.dataset.trackId === currentButton.dataset.trackId && button.dataset.src === currentButton.dataset.src) : -1;
        const targetButton = queueButtons[queueIndex < 0 ? 0 : (queueIndex + offset + queueButtons.length) % queueButtons.length];
        loadTrack(targetButton, true);
    };

    const bindDynamicControls = () => {
        trackButtons = Array.from(document.querySelectorAll(".track-btn[data-src]"));
        setQueue(queueSelector);

        const state = readState();
        if (state?.trackId) {
            currentTrackId = String(state.trackId);
        }

        markTrackState();
        renderUpNext();
    };

    const restoreState = () => {
        const state = readState();
        if (!state) {
            renderUpNext();
            return;
        }

        if (typeof state.volume === "number") {
            volumeBar.value = String(state.volume);
            player.volume = state.volume;
        }

        shuffleEnabled = state.shuffleEnabled === true;
        repeatEnabled = state.repeatEnabled === true;
        syncModeButtons();

        queueSelector = state.queueSelector || null;
        currentTrackId = state.trackId || "";

        if (state.src) {
            player.src = state.src;
            setLabels(state.title || "", state.artist || "", state.album || "");
            updateArtwork(state.title || "", state.artist || "", state.cover || "");
            pendingSeekTime = typeof state.currentTime === "number" ? state.currentTime : null;
        }

        bindDynamicControls();
    };

    const replacePageParts = (html, url, pushState) => {
        const parsed = new DOMParser().parseFromString(html, "text/html");
        const nextContent = parsed.querySelector(".app-content");
        const nextSidebar = parsed.querySelector(".app-sidebar");
        const content = document.querySelector(".app-content");
        const sidebar = document.querySelector(".app-sidebar");

        if (!nextContent || !content) {
            window.location.href = url;
            return;
        }

        content.replaceWith(nextContent);
        if (nextSidebar && sidebar) {
            sidebar.replaceWith(nextSidebar);
        }

        document.title = parsed.title || document.title;
        if (pushState) {
            history.pushState({}, "", url);
        }

        bindDynamicControls();
        document.body.classList.remove("is-navigating");
        window.scrollTo({ top: 0, behavior: "smooth" });
    };

    const navigateTo = async (url, options = {}) => {
        if (isNavigating) {
            return;
        }

        isNavigating = true;
        persistState();
        document.body.classList.add("is-navigating");

        try {
            const response = await fetch(url, {
                method: options.method || "GET",
                body: options.body || null,
                headers: { "X-Requested-With": "fetch" },
                credentials: "same-origin"
            });

            if (!response.ok) {
                throw new Error(`Navigation failed with ${response.status}`);
            }

            replacePageParts(await response.text(), response.url || url, options.pushState !== false);
        } catch {
            window.location.href = url;
        } finally {
            isNavigating = false;
        }
    };

    document.addEventListener("click", (event) => {
        const trackButton = event.target.closest(".track-btn[data-src]");
        if (trackButton) {
            event.preventDefault();
            loadTrack(trackButton, true);
            return;
        }

        const collectionButton = event.target.closest("[data-play-collection]");
        if (collectionButton) {
            event.preventDefault();
            const selector = collectionButton.dataset.playCollection;
            const collectionButtons = selector
                ? Array.from(document.querySelectorAll(`${selector} .track-btn, ${selector}.track-btn`))
                : queueButtons;

            if (collectionButtons.length) {
                setQueue(selector || null);
                loadTrack(collectionButtons[0], true);
            }
            return;
        }

        const link = event.target.closest("a[href]");
        if (!link || event.defaultPrevented || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey || link.target || link.hasAttribute("download")) {
            return;
        }

        const url = new URL(link.href, window.location.href);
        if (url.origin !== window.location.origin || url.hash && url.pathname === window.location.pathname) {
            return;
        }

        event.preventDefault();
        navigateTo(url.href);
    });

    document.addEventListener("submit", (event) => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || form.target) {
            return;
        }

        const method = (form.method || "GET").toUpperCase();
        const action = new URL(form.action || window.location.href, window.location.href);

        if (action.origin !== window.location.origin) {
            return;
        }

        event.preventDefault();

        if (method === "GET") {
            const data = new FormData(form);
            action.search = new URLSearchParams(data).toString();
            navigateTo(action.href);
            return;
        }

        navigateTo(action.href, {
            method,
            body: new FormData(form)
        });
    });

    playCollectionBtn.addEventListener("click", () => {
        if (queueButtons.length) {
            loadTrack(queueButtons[0], true);
        }
    });

    playPauseBtn.addEventListener("click", () => {
        if (!player.getAttribute("src") && trackButtons.length > 0) {
            loadTrack(trackButtons[0], true);
            return;
        }

        if (!player.getAttribute("src")) {
            return;
        }

        if (player.paused) {
            player.play().catch(() => setPlayIcon(false));
        } else {
            player.pause();
        }
    });

    nextTrackBtn.addEventListener("click", () => moveInQueue(1));
    prevTrackBtn.addEventListener("click", () => moveInQueue(-1));
    shuffleBtn.addEventListener("click", () => {
        shuffleEnabled = !shuffleEnabled;
        syncModeButtons();
        persistState();
    });
    repeatBtn.addEventListener("click", () => {
        repeatEnabled = !repeatEnabled;
        syncModeButtons();
        persistState();
    });

    player.addEventListener("play", () => {
        document.body.classList.add("is-playing");
        setPlayIcon(true);
        markTrackState();
        persistState();
        renderUpNext();
    });

    player.addEventListener("pause", () => {
        document.body.classList.remove("is-playing");
        setPlayIcon(false);
        markTrackState();
        persistState();
    });

    player.addEventListener("ended", () => {
        if (repeatEnabled) {
            player.currentTime = 0;
            player.play().catch(() => setPlayIcon(false));
            return;
        }

        moveInQueue(1);
    });

    player.addEventListener("timeupdate", () => {
        if (!Number.isFinite(player.duration) || player.duration <= 0) {
            return;
        }

        seekBar.value = String(Math.round((player.currentTime / player.duration) * 100));
        currentTimeLabel.textContent = formatTime(player.currentTime);
        durationLabel.textContent = formatTime(player.duration);
    });

    player.addEventListener("loadedmetadata", () => {
        durationLabel.textContent = formatTime(player.duration);
        if (pendingSeekTime !== null && Number.isFinite(player.duration) && pendingSeekTime < player.duration) {
            player.currentTime = pendingSeekTime;
        }

        pendingSeekTime = null;
    });

    seekBar.addEventListener("input", () => {
        if (!Number.isFinite(player.duration) || player.duration <= 0) {
            return;
        }

        player.currentTime = (Number(seekBar.value) / 100) * player.duration;
        persistState();
    });

    volumeBar.addEventListener("input", () => {
        player.volume = Number(volumeBar.value);
        persistState();
    });

    window.addEventListener("popstate", () => navigateTo(window.location.href, { pushState: false }));
    window.addEventListener("beforeunload", persistState);

    bindDynamicControls();
    restoreState();
    syncModeButtons();
    setPlayIcon(!player.paused && Boolean(player.getAttribute("src")));
    document.body.classList.toggle("is-playing", !player.paused && Boolean(player.getAttribute("src")));
})();
