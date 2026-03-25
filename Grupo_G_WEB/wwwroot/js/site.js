(() => {
    const storageKey = "crmusic.player.state";

    const player = document.getElementById("musicPlayer");
    const playPauseBtn = document.getElementById("playPauseBtn");
    const nextTrackBtn = document.getElementById("nextTrackBtn");
    const prevTrackBtn = document.getElementById("prevTrackBtn");
    const seekBar = document.getElementById("seekBar");
    const volumeBar = document.getElementById("volumeBar");

    if (!player || !playPauseBtn || !nextTrackBtn || !prevTrackBtn || !seekBar || !volumeBar) {
        return;
    }

    const titleElements = [document.getElementById("globalNowPlayingTitle"), document.getElementById("sideNowPlayingTitle")].filter(Boolean);
    const artistElements = [document.getElementById("globalNowPlayingArtist"), document.getElementById("sideNowPlayingArtist")].filter(Boolean);
    const trackButtons = Array.from(document.querySelectorAll(".track-btn"));
    let currentIndex = -1;
    let pendingSeekTime = null;

    const readState = () => {
        try {
            const raw = localStorage.getItem(storageKey);
            return raw ? JSON.parse(raw) : null;
        } catch {
            return null;
        }
    };

    const setLabels = (title, artist) => {
        titleElements.forEach((element) => {
            element.textContent = title || "Selecciona una cancion";
        });

        artistElements.forEach((element) => {
            element.textContent = artist || "-";
        });
    };

    const setPlayIcon = (isPlaying) => {
        const icon = playPauseBtn.querySelector("i");
        if (icon) {
            icon.className = isPlaying ? "bi bi-pause-fill" : "bi bi-play-fill";
        }
    };

    const markTrackState = (isPlaying) => {
        trackButtons.forEach((button, index) => {
            button.classList.toggle("is-selected", index === currentIndex);
            button.classList.toggle("is-playing", index === currentIndex && isPlaying);
        });
    };

    const persistState = () => {
        const activeButton = currentIndex >= 0 ? trackButtons[currentIndex] : null;
        const payload = {
            index: currentIndex,
            src: player.getAttribute("src") || "",
            title: activeButton?.dataset.title || titleElements[0]?.textContent || "",
            artist: activeButton?.dataset.artist || artistElements[0]?.textContent || "",
            trackId: activeButton?.dataset.trackId || "",
            currentTime: Number.isFinite(player.currentTime) ? player.currentTime : 0,
            volume: Number(volumeBar.value)
        };

        localStorage.setItem(storageKey, JSON.stringify(payload));
    };

    const loadTrack = (index, autoplay) => {
        if (index < 0 || index >= trackButtons.length) {
            return;
        }

        const button = trackButtons[index];
        currentIndex = index;

        if (player.getAttribute("src") !== button.dataset.src) {
            player.src = button.dataset.src || "";
        }

        setLabels(button.dataset.title || "", button.dataset.artist || "");
        seekBar.value = "0";
        markTrackState(false);
        persistState();

        if (autoplay) {
            player.play().catch(() => setPlayIcon(false));
        }
    };

    const restoreState = () => {
        const state = readState();
        if (!state) {
            return;
        }

        if (typeof state.volume === "number") {
            volumeBar.value = String(state.volume);
            player.volume = state.volume;
        }

        if (state.src) {
            player.src = state.src;
            setLabels(state.title || "", state.artist || "");
            pendingSeekTime = typeof state.currentTime === "number" ? state.currentTime : null;
        }

        if (state.trackId) {
            currentIndex = trackButtons.findIndex((button) => button.dataset.trackId === String(state.trackId));
        } else if (typeof state.index === "number") {
            currentIndex = state.index;
        }

        markTrackState(false);
    };

    trackButtons.forEach((button, index) => {
        button.addEventListener("click", () => loadTrack(index, true));
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

    nextTrackBtn.addEventListener("click", () => {
        if (trackButtons.length === 0) {
            return;
        }

        const nextIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % trackButtons.length;
        loadTrack(nextIndex, true);
    });

    prevTrackBtn.addEventListener("click", () => {
        if (trackButtons.length === 0) {
            return;
        }

        const prevIndex = currentIndex <= 0 ? trackButtons.length - 1 : currentIndex - 1;
        loadTrack(prevIndex, true);
    });

    player.addEventListener("play", () => {
        setPlayIcon(true);
        markTrackState(true);
        persistState();
    });

    player.addEventListener("pause", () => {
        setPlayIcon(false);
        markTrackState(false);
        persistState();
    });

    player.addEventListener("ended", () => {
        if (trackButtons.length === 0) {
            setPlayIcon(false);
            return;
        }

        const nextIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % trackButtons.length;
        loadTrack(nextIndex, true);
    });

    player.addEventListener("timeupdate", () => {
        if (!Number.isFinite(player.duration) || player.duration <= 0) {
            return;
        }

        seekBar.value = String(Math.round((player.currentTime / player.duration) * 100));
    });

    player.addEventListener("loadedmetadata", () => {
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

    restoreState();
    setPlayIcon(false);
})();
