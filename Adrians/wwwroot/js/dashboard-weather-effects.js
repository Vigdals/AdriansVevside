
(() => {
    "use strict";

    const dashboard = document.querySelector(".public-dashboard");
    if (!dashboard) return;

    const reduceMotion = window.matchMedia(
        "(prefers-reduced-motion: reduce)"
    );

    const random = (min, max) =>
        min + Math.random() * (max - min);

    let activeEffect = null;

    // MET sine symbolkodar avgjer kva animasjon me brukar.
    function getWeatherType(symbolCode) {
        const code = (symbolCode || "").toLowerCase();

        // Snø, snøbyer, lett snø, kraftig snø
        if (code.includes("snow")) {
            return "snow";
        }

        // Regn, regnbyer, sludd
        if (code.includes("rain") || code.includes("sleet")) {
            return "rain";
        }

        // Overskya, delvis overskya, tåke
        if (code.includes("cloudy") || code.includes("fog")) {
            return "clouds";
        }

        // Klar himmel, pent vêr
        if (code.includes("clearsky") || code.includes("fair")) {
            return "sun";
        }

        return null;
    }

    function stopActiveEffect() {
        if (!activeEffect) return;

        const current = activeEffect;
        activeEffect = null;

        for (const animation of current.animations) {
            animation.cancel();
        }

        current.layer.remove();
    }

    function playWeatherEffect(type, trigger) {
        if (!type || reduceMotion.matches) return;

        // Nytt klikk avbryt førre effekt.
        stopActiveEffect();

        const layer = document.createElement("div");
        layer.className = `weather-fx weather-fx--${type}`;
        layer.setAttribute("aria-hidden", "true");

        document.body.appendChild(layer);

        const effect = {
            layer,
            animations: []
        };

        activeEffect = effect;

        const width = window.innerWidth;
        const height = window.innerHeight;

        // Animasjonane kan starte frå posisjonen til vêrikonet.
        const rect = trigger.getBoundingClientRect();
        const centerX = rect.left + rect.width / 2;
        const centerY = rect.top + rect.height / 2;

        function createElement(className) {
            const element = document.createElement("span");
            element.className = className;
            layer.appendChild(element);
            return element;
        }

        function animate(element, frames, options = {}) {
            const animation = element.animate(frames, {
                duration: 3000,
                easing: "ease-in-out",
                fill: "both",
                ...options
            });

            effect.animations.push(animation);
        }

        // ---------------------------------------
        // SOL
        // ---------------------------------------
        function createSun() {
            const glow = createElement("weather-fx-sun-glow");

            glow.style.left = `${centerX - 190}px`;
            glow.style.top = `${centerY - 190}px`;

            animate(glow, [
                { opacity: 0, transform: "scale(0.3)" },
                { offset: 0.4, opacity: 1, transform: "scale(1)" },
                { offset: 0.75, opacity: 0.8, transform: "scale(1.25)" },
                { opacity: 0, transform: "scale(1.5)" }
            ]);

            const sun = createElement("weather-fx-sun");

            sun.style.left = `${centerX - 55}px`;
            sun.style.top = `${centerY - 55}px`;

            animate(sun, [
                {
                    opacity: 0,
                    transform: "scale(0.2) rotate(-35deg)"
                },
                {
                    offset: 0.35,
                    opacity: 1,
                    transform: "scale(1.15) rotate(10deg)"
                },
                {
                    offset: 0.65,
                    opacity: 1,
                    transform: "scale(1) rotate(25deg)"
                },
                {
                    opacity: 0,
                    transform: "scale(1.4) rotate(65deg)"
                }
            ]);

            // Små lysglimt rundt sola
            for (let i = 0; i < 12; i++) {
                const spark = createElement("weather-fx-spark");

                const angle = (Math.PI * 2 * i) / 12;
                const distance = random(90, 180);

                spark.style.left = `${centerX}px`;
                spark.style.top = `${centerY}px`;

                const dx = Math.cos(angle) * distance;
                const dy = Math.sin(angle) * distance;

                animate(spark, [
                    {
                        opacity: 0,
                        transform: "translate(0, 0) scale(0.2)"
                    },
                    {
                        offset: 0.4,
                        opacity: 1,
                        transform: `translate(${dx * 0.6}px, ${dy * 0.6}px) scale(1)`
                    },
                    {
                        opacity: 0,
                        transform: `translate(${dx}px, ${dy}px) scale(0.2)`
                    }
                ], {
                    duration: random(1800, 2700),
                    delay: random(0, 250)
                });
            }
        }

        // ---------------------------------------
        // REGN
        // ---------------------------------------
        function createRain() {
            const count = width < 640 ? 28 : 65;

            for (let i = 0; i < count; i++) {
                const drop = createElement("weather-fx-raindrop");

                drop.style.left = `${random(0, width)}px`;
                drop.style.height = `${random(18, 38)}px`;

                const startY = random(-height * 0.6, -40);
                const drift = random(-100, -35);

                animate(drop, [
                    {
                        opacity: 0,
                        transform: `translate3d(0, ${startY}px, 0) rotate(12deg)`
                    },
                    {
                        offset: 0.18,
                        opacity: 0.85
                    },
                    {
                        offset: 0.85,
                        opacity: 0.85
                    },
                    {
                        opacity: 0,
                        transform: `translate3d(${drift}px, ${height + 100}px, 0) rotate(12deg)`
                    }
                ], {
                    duration: random(1800, 2800),
                    delay: random(0, 250),
                    easing: "linear"
                });
            }
        }

        // ---------------------------------------
        // SNØ
        // ---------------------------------------
        function createSnow() {
            const count = width < 640 ? 22 : 45;

            for (let i = 0; i < count; i++) {
                const flake = createElement("weather-fx-snowflake");

                flake.textContent = "❄";
                flake.style.left = `${random(0, width)}px`;
                flake.style.fontSize = `${random(9, 22)}px`;

                const startY = random(-height * 0.5, -30);
                const drift = random(-160, 160);
                const rotation = random(-270, 270);

                animate(flake, [
                    {
                        opacity: 0,
                        transform: `translate3d(0, ${startY}px, 0) rotate(0deg)`
                    },
                    {
                        offset: 0.2,
                        opacity: 0.9
                    },
                    {
                        offset: 0.8,
                        opacity: 0.9
                    },
                    {
                        opacity: 0,
                        transform: `translate3d(${drift}px, ${height + 60}px, 0) rotate(${rotation}deg)`
                    }
                ], {
                    duration: random(2500, 3300),
                    delay: random(0, 200),
                    easing: "linear"
                });
            }
        }

        // ---------------------------------------
        // OVERSKYA
        // ---------------------------------------
        function createClouds() {
            const count = width < 640 ? 3 : 6;

            for (let i = 0; i < count; i++) {
                const cloud = createElement("weather-fx-cloud");

                const size = random(140, 260);

                cloud.style.width = `${size}px`;
                cloud.style.left = `${random(-size, width)}px`;
                cloud.style.top = `${random(5, 55)}%`;

                animate(cloud, [
                    {
                        opacity: 0,
                        transform: "translateX(-120px) scale(0.85)"
                    },
                    {
                        offset: 0.35,
                        opacity: 0.8,
                        transform: "translateX(0) scale(1)"
                    },
                    {
                        offset: 0.7,
                        opacity: 0.8,
                        transform: "translateX(100px) scale(1.05)"
                    },
                    {
                        opacity: 0,
                        transform: "translateX(230px) scale(1)"
                    }
                ], {
                    duration: random(2400, 3200),
                    delay: random(0, 200)
                });
            }
        }

        switch (type) {
            case "sun":
                createSun();
                break;
            case "rain":
                createRain();
                break;
            case "snow":
                createSnow();
                break;
            case "clouds":
                createClouds();
                break;
        }

        // Rydd vekk DOM-element og animasjonar når alt er ferdig.
        void Promise.all(
            effect.animations.map(animation =>
                animation.finished.catch(() => {})
            )
        ).then(() => {
            if (activeEffect !== effect) return;

            layer.remove();
            activeEffect = null;
        });
    }

    // Event delegation:
    // Dashboardet oppdaterer vêrkortet automatisk.
    // Derfor lyttar me på eit stabilt foreldre-element.
    dashboard.addEventListener("click", event => {
        const target = event.target;

        if (!(target instanceof Element)) return;

        const trigger = target.closest(
            ".dashboard-weather-effect-trigger"
        );

        if (!trigger || !dashboard.contains(trigger)) return;

        const type = getWeatherType(trigger.dataset.weatherSymbol);

        playWeatherEffect(type, trigger);
    });

    // Stopp ved redusert rørsle eller når brukaren forlèt sida.
    reduceMotion.addEventListener("change", () => {
        if (reduceMotion.matches) stopActiveEffect();
    });

    window.addEventListener("pagehide", stopActiveEffect);

})();
