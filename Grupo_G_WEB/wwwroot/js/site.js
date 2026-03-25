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
    const playCollectionButtons = Array.from(document.querySelectorAll("[data-play-collection]"));
    const trackButtons = Array.from(document.querySelectorAll(".track-btn"));

    let currentIndex = -1;
    let pendingSeekTime = null;
    let queueButtons = trackButtons;
    let queueSelector = null;
    let shuffleEnabled = false;
    let repeatEnabled = false;

    const triggerArtworkPulse = () => {
        [sideArtwork, globalArtwork].forEach((element) => {
            element.classList.remove("artwork-pulse");
            void element.offsetWidth;
            element.classList.add("artwork-pulse");
        });
    };

    const formatTime = (value) => {
        if (!Number.isFinite(value) || value < 0) {
            return "0:00";
        }

        const minutes = Math.floor(value / 60);
        const seconds = Math.floor(value % 60);
        return `${minutes}:${seconds.toString().padStart(2, "0")}`;
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
                element.style.backgroundImage = `linear-gradient(rgba(15, 23, 42, 0.18), rgba(15, 23, 42, 0.42)), url("${cover}")`;
                element.classList.add("has-cover");
            } else {
                element.style.backgroundImage = "";
                element.classList.remove("has-cover");
            }
        });
    };

    const markTrackState = (isPlaying) => {
        trackButtons.forEach((button, index) => {
            button.classList.toggle("is-selected", index === currentIndex);
            button.classList.toggle("is-playing", index === currentIndex && isPlaying);
        });
    };

    const renderUpNext = () => {
        queueCount.textContent = `${queueButtons.length} canciones`;

        if (!queueButtons.length) {
            upNextList.innerHTML = `<p class="small-note">La cola aparecera cuando selecciones una cancion.</p>`;
            return;
        }

        const currentButton = currentIndex >= 0 ? trackButtons[currentIndex] : null;
        const activeQueueIndex = currentButton ? queueButtons.findIndex((button) => button === currentButton) : -1;
        const orderedButtons = activeQueueIndex >= 0
            ? [...queueButtons.slice(activeQueueIndex + 1), ...queueButtons.slice(0, activeQueueIndex + 1)]
            : queueButtons;

        upNextList.innerHTML = "";
        orderedButtons.slice(0, 6).forEach((button, index) => {
            const item = document.createElement("button");
            item.type = "button";
            item.className = "up-next-item";
            item.innerHTML = `
                <span class="up-next-index">${index === 0 && activeQueueIndex >= 0 ? "Ahora" : `#${index + 1}`}</span>
                <span class="up-next-copy">
                    <strong>${button.dataset.title || ""}</strong>
                    <small>${button.dataset.artist || ""}</small>
                </span>
            `;

            item.addEventListener("click", () => {
                const indexInTracks = trackButtons.findIndex((trackButton) => trackButton === button);
                if (indexInTracks >= 0) {
                    loadTrack(indexInTracks, true);
                }
            });

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
        }

        renderUpNext();
    };

    const persistState = () => {
        const activeButton = currentIndex >= 0 ? trackButtons[currentIndex] : null;
        localStorage.setItem(storageKey, JSON.stringify({
            index: currentIndex,
            src: player.getAttribute("src") || "",
            title: activeButton?.dataset.title || titleElements[0]?.textContent || "",
            artist: activeButton?.dataset.artist || artistElements[0]?.textContent || "",
            album: activeButton?.dataset.album || albumElements[0]?.textContent || "",
            cover: activeButton?.dataset.cover || "",
            trackId: activeButton?.dataset.trackId || "",
            queueSelector,
            currentTime: Number.isFinite(player.currentTime) ? player.currentTime : 0,
            volume: Number(volumeBar.value),
            shuffleEnabled,
            repeatEnabled
        }));
    };

    const getQueueIndex = () => {
        const currentButton = currentIndex >= 0 ? trackButtons[currentIndex] : null;
        return currentButton ? queueButtons.findIndex((button) => button === currentButton) : -1;
    };

    const moveInQueue = (offset) => {
        if (!queueButtons.length) {
            return;
        }

        if (shuffleEnabled && queueButtons.length > 1) {
            const currentButton = currentIndex >= 0 ? trackButtons[currentIndex] : null;
            const candidateButtons = queueButtons.filter((button) => button !== currentButton);
            const randomButton = candidateButtons[Math.floor(Math.random() * candidateButtons.length)];
            const randomIndex = trackButtons.findIndex((button) => button === randomButton);

            if (randomIndex >= 0) {
                loadTrack(randomIndex, true);
            }

            return;
        }

        const queueIndex = getQueueIndex();
        const targetButton = queueButtons[queueIndex < 0 ? 0 : (queueIndex + offset + queueButtons.length) % queueButtons.length];
        const targetIndex = trackButtons.findIndex((button) => button === targetButton);
        if (targetIndex >= 0) {
            loadTrack(targetIndex, true);
        }
    };

    const loadTrack = (index, autoplay) => {
        if (index < 0 || index >= trackButtons.length) {
            return;
        }

        const button = trackButtons[index];
        currentIndex = index;

        const closestCollection = button.closest(".track-list, .playlist-song-list");
        if (closestCollection) {
            setQueue(`.${Array.from(closestCollection.classList).join(".")}`);
        }

        if (player.getAttribute("src") !== button.dataset.src) {
            player.src = button.dataset.src || "";
        }

        setLabels(button.dataset.title || "", button.dataset.artist || "", button.dataset.album || "");
        updateArtwork(button.dataset.title || "", button.dataset.artist || "", button.dataset.cover || "");
        triggerArtworkPulse();
        seekBar.value = "0";
        currentTimeLabel.textContent = "0:00";
        markTrackState(false);
        persistState();
        renderUpNext();

        if (autoplay) {
            player.play().catch(() => setPlayIcon(false));
        }
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

        if (state.queueSelector) {
            setQueue(state.queueSelector);
        }

        if (state.src) {
            player.src = state.src;
            setLabels(state.title || "", state.artist || "", state.album || "");
            updateArtwork(state.title || "", state.artist || "", state.cover || "");
            pendingSeekTime = typeof state.currentTime === "number" ? state.currentTime : null;
        }

        if (state.trackId) {
            currentIndex = trackButtons.findIndex((button) => button.dataset.trackId === String(state.trackId));
        } else if (typeof state.index === "number") {
            currentIndex = state.index;
        }

        markTrackState(false);
        renderUpNext();
    };

    trackButtons.forEach((button, index) => {
        button.addEventListener("click", () => loadTrack(index, true));
    });

    playCollectionButtons.forEach((button) => {
        button.addEventListener("click", () => {
            const selector = button.dataset.playCollection;
            if (!selector) {
                return;
            }

            const collectionButtons = Array.from(document.querySelectorAll(`${selector} .track-btn, ${selector}.track-btn`));
            if (!collectionButtons.length) {
                return;
            }

            setQueue(selector);
            const firstIndex = trackButtons.findIndex((trackButton) => trackButton === collectionButtons[0]);
            if (firstIndex >= 0) {
                loadTrack(firstIndex, true);
            }
        });
    });

    playCollectionBtn.addEventListener("click", () => {
        if (!queueButtons.length) {
            return;
        }

        const firstIndex = trackButtons.findIndex((button) => button === queueButtons[0]);
        if (firstIndex >= 0) {
            loadTrack(firstIndex, true);
        }
    });

    playPauseBtn.addEventListener("click", () => {
        if (!player.getAttribute("src") && trackButtons.length > 0) {
            loadTrack(0, true);
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
        setPlayIcon(true);
        markTrackState(true);
        persistState();
        renderUpNext();
    });

    player.addEventListener("pause", () => {
        setPlayIcon(false);
        markTrackState(false);
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

    window.addEventListener("beforeunload", persistState);

    setQueue(null);
    restoreState();
    syncModeButtons();
    setPlayIcon(false);
})();
