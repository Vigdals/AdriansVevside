(() => {
    "use strict";

    if (!document.querySelector(".public-dashboard")) return;

    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
    const random = (min, max) => min + Math.random() * (max - min);
    const sceneDuration = 9000;
    let scene = null;
    let cleanupTimer = null;

    // Include December 1 through January 1, using Sogndal/Oslo time.
    function isChristmasSeason() {
        const parts = new Intl.DateTimeFormat("en-GB", {
            timeZone: "Europe/Oslo",
            month: "numeric",
            day: "numeric"
        }).formatToParts(new Date());

        const month = Number(parts.find(part => part.type === "month")?.value);
        const day = Number(parts.find(part => part.type === "day")?.value);

        return month === 12 || (month === 1 && day === 1);
    }

    function stopChristmas() {
        if (cleanupTimer !== null) window.clearTimeout(cleanupTimer);
        cleanupTimer = null;
        scene?.remove();
        scene = null;
    }

    function addLights(layer) {
        const strand = document.createElement("div");
        strand.className = "christmas-fx-lights";
        const colors = ["#ffbd64", "#ff6b72", "#89d2a3", "#ffdb86", "#86bce1"];
        const count = window.innerWidth < 640 ? 13 : 23;

        for (let i = 0; i < count; i++) {
            const light = document.createElement("span");
            light.className = "christmas-fx-light";
            light.style.setProperty("--bulb", colors[i % colors.length]);
            light.style.animationDelay = `${random(-3, 0)}s`;
            strand.appendChild(light);
        }
        layer.appendChild(strand);
    }

    function addSnow(layer) {
        const snowfall = document.createElement("div");
        snowfall.className = "christmas-fx-snow";
        const count = window.innerWidth < 640 ? 55 : 110;

        for (let i = 0; i < count; i++) {
            const flake = document.createElement("span");
            flake.className = "christmas-fx-flake";
            flake.textContent = i % 5 === 0 ? "✧" : "❄";
            flake.style.left = `${random(0, 100)}%`;
            flake.style.fontSize = `${random(9, 22)}px`;
            flake.style.opacity = random(0.4, 0.9).toFixed(2);
            flake.style.setProperty("--drift", `${random(-140, 140)}px`);
            flake.style.setProperty("--spin", `${random(-120, 120)}deg`);
            flake.style.setProperty("--duration", `${random(4.2, 7.8)}s`);
            // Negative delay means snow is already falling on page load.
            flake.style.setProperty("--delay", `${random(-8, 0)}s`);
            snowfall.appendChild(flake);
        }
        layer.appendChild(snowfall);
    }

    function addTrees(layer, side) {
        const forest = document.createElement("div");
        forest.className = `christmas-fx-forest christmas-fx-forest--${side}`;

        for (let i = 0; i < 3; i++) {
            const tree = document.createElement("div");
            tree.className = "christmas-fx-tree";
            tree.style.setProperty("--tree-scale", String([0.77, 1.08, 0.87][i]));
            tree.innerHTML = '<span class="christmas-fx-tree-star">✦</span>';
            forest.appendChild(tree);
        }
        layer.appendChild(forest);
    }

    function addSanta(layer) {
        const flight = document.createElement("div");
        flight.className = "christmas-fx-flight";

        // Small, self-contained SVG: Santa rides behind four reindeer.
        flight.innerHTML = `
          <svg class="christmas-fx-flight-art" viewBox="0 0 930 190"
               xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
            <defs>
              <g id="christmas-reindeer">
                <path d="M40 60 Q26 51 22 59" fill="none" stroke="#9d6040" stroke-width="6" stroke-linecap="round"/>
                <ellipse cx="65" cy="59" rx="32" ry="19" fill="#985b3d"/>
                <path d="M88 56 Q99 45 101 29 L116 32 Q112 57 93 68Z" fill="#a96b45"/>
                <ellipse cx="117" cy="27" rx="18" ry="12" fill="#ae704a"/>
                <path d="M105 15 L100 4 M106 13 L94 8 M116 14 L121 0 M116 9 L129 3"
                      fill="none" stroke="#c5a17a" stroke-width="4" stroke-linecap="round"/>
                <path d="M122 18 L132 12 L127 24Z" fill="#a96b45"/>
                <path d="M47 73 L34 94 M70 74 L73 96 M85 72 L99 89 M56 75 L55 98"
                      stroke="#724329" stroke-width="7" stroke-linecap="round"/>
                <circle cx="126" cy="26" r="2" fill="#2c231f"/>
                <path d="M84 46 Q98 49 105 45" fill="none" stroke="#dbb978" stroke-width="4"/>
              </g>
            </defs>

            <!-- Gold reins connect the sleigh to the deer. -->
            <path d="M190 118 Q390 93 825 66" fill="none"
                  stroke="#e8c78b" stroke-width="2.5" opacity="0.85"/>

            <!-- Sleigh, runners and gift sack. -->
            <path d="M46 110 Q18 88 40 68 Q57 53 82 69 L103 114Z" fill="#87643d"/>
            <path d="M32 122 Q78 144 193 113 L193 140 Q100 164 35 138Z" fill="#b93439"/>
            <path d="M30 145 Q68 174 172 151 Q195 146 199 127" fill="none"
                  stroke="#f3cd79" stroke-width="8" stroke-linecap="round"/>
            <path d="M45 148 Q62 167 89 166" fill="none" stroke="#f3cd79" stroke-width="6"/>
            <path d="M65 125 L172 116" stroke="#f3cd79" stroke-width="4"/>

            <!-- Santa: hat, face, beard and coat. -->
            <ellipse cx="120" cy="112" rx="34" ry="37" fill="#bf333b"/>
            <path d="M90 129 Q119 144 148 124" fill="none" stroke="#fff1d3" stroke-width="10"/>
            <circle cx="122" cy="68" r="25" fill="#f2c595"/>
            <path d="M101 73 Q96 107 121 107 Q149 103 146 73 Q132 90 121 84 Q111 89 101 73Z"
                  fill="#fff7e7"/>
            <path d="M95 59 Q99 24 139 28 Q156 35 153 55 L95 60Z" fill="#c73940"/>
            <path d="M94 60 Q124 49 154 58" fill="none" stroke="#fff8ea" stroke-width="12" stroke-linecap="round"/>
            <circle cx="152" cy="37" r="10" fill="#fff8ea"/>
            <circle cx="113" cy="65" r="2.4" fill="#43332c"/>
            <circle cx="130" cy="65" r="2.4" fill="#43332c"/>
            <path d="M145 101 Q167 87 175 105" fill="none" stroke="#c73940" stroke-width="15" stroke-linecap="round"/>
            <circle cx="177" cy="107" r="9" fill="#fff8ea"/>

            <!-- Four reindeer; Rudolph is in front. -->
            <use href="#christmas-reindeer" transform="translate(242 55) scale(.83)"/>
            <use href="#christmas-reindeer" transform="translate(390 48) scale(.83)"/>
            <use href="#christmas-reindeer" transform="translate(538 39) scale(.83)"/>
            <g transform="translate(686 29) scale(.83)">
              <use href="#christmas-reindeer"/>
              <circle cx="134" cy="30" r="7" fill="#f65e5f"/>
              <circle cx="134" cy="30" r="12" fill="#f65e5f" opacity="0.22"/>
            </g>
          </svg>`;
        layer.appendChild(flight);
    }

    function playChristmas() {
        if (scene || reduceMotion.matches || document.hidden) return;

        const layer = document.createElement("div");
        layer.className = "christmas-fx";
        layer.setAttribute("aria-hidden", "true");
        document.body.appendChild(layer);
        scene = layer;

        const warmth = document.createElement("div");
        warmth.className = "christmas-fx-warmth";
        layer.appendChild(warmth);

        addLights(layer);
        addSnow(layer);
        addTrees(layer, "left");
        addTrees(layer, "right");
        addSanta(layer);

        const message = document.createElement("div");
        message.className = "christmas-fx-message";
        message.textContent = "God jul! ✨";
        layer.appendChild(message);

        cleanupTimer = window.setTimeout(stopChristmas, sceneDuration + 150);
    }

    // Available in DevTools all year: previewDashboardChristmas()
    window.previewDashboardChristmas = playChristmas;

    if (isChristmasSeason()) playChristmas();

    window.addEventListener("pagehide", stopChristmas);
    reduceMotion.addEventListener("change", () => {
        if (reduceMotion.matches) stopChristmas();
    });
})();
