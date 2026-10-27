(() => {
    "use strict";

    const dashboard = document.querySelector(".public-dashboard");
    if (!dashboard) return;

    const refreshIntervalMs = 5 * 60 * 1000;
    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
    const timeElement = document.getElementById("DashboardClockTime");
    const dateElement = document.getElementById("DashboardClockDate");
    const timeFormat = new Intl.DateTimeFormat("nn-NO", {
        hour: "2-digit",
        minute: "2-digit",
        timeZone: "Europe/Oslo"
    });
    const dateFormat = new Intl.DateTimeFormat("nn-NO", {
        weekday: "long",
        day: "numeric",
        month: "long",
        timeZone: "Europe/Oslo"
    });

    let lastClockTime = "";
    let lastRefresh = Date.now();
    let refreshInFlight = false;

function updateClock() {
    if (!timeElement || !dateElement) return;

    const now = new Date();
    const time = timeFormat.format(now);
    const date = dateFormat.format(now);
    const capitalizedDate = date.charAt(0).toUpperCase() + date.slice(1);

    if (time !== lastClockTime) {
        timeElement.textContent = time;

        if (lastClockTime && !reduceMotion.matches && timeElement.animate) {

            const minute = Number(
                timeFormat.formatToParts(now)
                    .find(part => part.type === "minute")?.value
            );

            if (minute % 1 === 0) {

                const styles = getComputedStyle(document.documentElement);
                const originalColor = getComputedStyle(timeElement).color;
                const accent = styles.getPropertyValue("--color-primary").trim();
                const glow = styles.getPropertyValue("--color-success").trim();

                // Elastisk dobbelpuls
                timeElement.animate([
                    { offset: 0,    transform: "scale(1) rotate(0deg)" },
                    { offset: 0.14, transform: "scale(1.12) rotate(-1deg)" },
                    { offset: 0.30, transform: "scale(0.98) rotate(0.5deg)" },
                    { offset: 0.48, transform: "scale(1.08) rotate(0.5deg)" },
                    { offset: 0.68, transform: "scale(1.025) rotate(0deg)" },
                    { offset: 1,    transform: "scale(1) rotate(0deg)" }
                ], {
                    duration: 1800,
                    easing: "cubic-bezier(0.2, 0.65, 0.2, 1)"
                });

                // Neonfargar og pulserande glød
                timeElement.animate([
                    {
                        offset: 0,
                        color: originalColor,
                        textShadow: "0 0 0 transparent",
                        filter: "brightness(1)"
                    },
                    {
                        offset: 0.14,
                        color: accent,
                        textShadow: `
                            0 0 8px ${accent},
                            0 0 24px ${glow},
                            0 0 45px ${glow}
                        `,
                        filter: "brightness(1.25)"
                    },
                    {
                        offset: 0.30,
                        color: glow,
                        textShadow: `0 0 8px ${glow}`,
                        filter: "brightness(1)"
                    },
                    {
                        offset: 0.48,
                        color: accent,
                        textShadow: `
                            0 0 12px ${accent},
                            0 0 32px ${glow}
                        `,
                        filter: "brightness(1.2)"
                    },
                    {
                        offset: 1,
                        color: originalColor,
                        textShadow: "0 0 0 transparent",
                        filter: "brightness(1)"
                    }
                ], {
                    duration: 1800,
                    easing: "ease-in-out"
                });

            } else {
                // Vanleg diskret animasjon kvart minutt
                timeElement.animate([
                    {
                        opacity: 0.65,
                        transform: "translateY(3px)"
                    },
                    {
                        opacity: 1,
                        transform: "translateY(0)"
                    }
                ], {
                    duration: 220,
                    easing: "ease-out"
                });
            }
        }

        lastClockTime = time;
    }

    if (dateElement.textContent !== capitalizedDate) {
        dateElement.textContent = capitalizedDate;
    }
}

    function updateCountdown(element, timestamp) {
        const target = Number(element.dataset.target);
        if (!Number.isFinite(target)) {
            element.textContent = "--";
            return;
        }

        const remaining = Math.max(0, target - timestamp);
        if (remaining === 0) {
            if (element.textContent !== "Ferdig!") element.textContent = "Ferdig!";
            return;
        }

        const days = Math.floor(remaining / 86400000);
        const hours = Math.floor(remaining / 3600000) % 24;
        const minutes = Math.floor(remaining / 60000) % 60;
        const seconds = Math.floor(remaining / 1000) % 60;
        const value = days + "d " + hours + "t " + minutes + "m " + seconds + "s";
        if (element.textContent !== value) element.textContent = value;
    }

    function tick() {
        updateClock();
        const now = Date.now();
        document.querySelectorAll(".dashboard-countdown[data-target]")
            .forEach(element => updateCountdown(element, now));
    }

    // Short, one-time entrance transitions. No parallax, glowing cursors or tilt.
    const animated = new WeakSet();
    let observer = null;

    if (!reduceMotion.matches && "IntersectionObserver" in window) {
        observer = new IntersectionObserver(entries => {
            for (const entry of entries) {
                if (!entry.isIntersecting) continue;
                observer.unobserve(entry.target);

                if (entry.target.animate) {
                    entry.target.animate([
                        { opacity: 0, transform: "translateY(9px)" },
                        { opacity: 1, transform: "translateY(0)" }
                    ], {
                        duration: 380,
                        easing: "cubic-bezier(0.22, 1, 0.36, 1)",
                        fill: "none"
                    });
                }
            }
        }, { threshold: 0.08, rootMargin: "0px 0px -20px 0px" });
    }

    function observeReveals() {
        if (!observer || reduceMotion.matches) return;
        document.querySelectorAll(
            ".dashboard-header, .dashboard-card, .dashboard-link-card"
        ).forEach(element => {
            if (animated.has(element)) return;
            animated.add(element);
            observer.observe(element);
        });
    }

    // MVC still renders the whole view. Refresh only changed dashboard sections
    // so that the clock, scrolling, focused controls and rest of the page stay put.
    const liveSections = [
        ".dashboard-updated",
        ".dashboard-top-layout",
        ".dashboard-grid-countdowns",
        ".dashboard-vigdalpi-card",
        ".dashboard-grid-links"
    ];

    function comparableMarkup(node, selector) {
        if (selector !== ".dashboard-grid-countdowns") return node.outerHTML;

        const copy = node.cloneNode(true);
        copy.querySelectorAll(".dashboard-countdown[data-target]")
            .forEach(countdown => { countdown.textContent = ""; });
        return copy.outerHTML;
    }

    async function refreshDashboard() {
        if (refreshInFlight || document.hidden) return;
        refreshInFlight = true;

        try {
            const response = await fetch(window.location.pathname + window.location.search, {
                cache: "no-store",
                credentials: "same-origin",
                headers: { Accept: "text/html" }
            });

            if (!response.ok || !response.headers.get("content-type")?.includes("text/html")) {
                return;
            }

            const fetched = new DOMParser().parseFromString(
                await response.text(), "text/html"
            );
            if (!fetched.querySelector(".public-dashboard")) return;

            const updates = [];
            for (const selector of liveSections) {
                const existing = document.querySelector(selector);
                const replacement = fetched.querySelector(selector);
                if (!existing || !replacement || existing.contains(document.activeElement)) {
                    continue;
                }
                if (comparableMarkup(existing, selector) !== comparableMarkup(replacement, selector)) {
                    updates.push([existing, replacement]);
                }
            }

            if (!updates.length) return;

            const applyUpdates = () => {
                for (const [existing, replacement] of updates) {
                    existing.replaceWith(replacement);
                }
                tick();
                observeReveals();
            };

            if (!reduceMotion.matches && typeof document.startViewTransition === "function") {
                document.startViewTransition(applyUpdates);
            } else {
                applyUpdates();
            }
        } catch (error) {
            // Offline or transient errors: preserve the last good dashboard.
            console.debug("Dashboard refresh skipped:", error);
        } finally {
            lastRefresh = Date.now();
            refreshInFlight = false;
        }
    }

    tick();
    observeReveals();
    window.setInterval(tick, 1000);
    window.setInterval(refreshDashboard, refreshIntervalMs);

    document.addEventListener("visibilitychange", () => {
        if (document.hidden) return;
        tick();
        if (Date.now() - lastRefresh >= refreshIntervalMs) {
            void refreshDashboard();
        }
    });
})();
