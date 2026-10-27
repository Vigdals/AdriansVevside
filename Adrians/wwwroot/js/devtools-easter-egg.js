
(() => {
    "use strict";

    const logo = String.raw``;

    const logoStyle = [
        "color: #73e2a7",
        "background: #0b1119",
        "font-family: monospace",
        "font-size: 12px",
        "font-weight: bold",
        "line-height: 1.5",
        "padding: 14px",
        "border-left: 4px solid #73e2a7"
    ].join(";");

    const headingStyle = [
        "color: #73e2a7",
        "font-size: 15px",
        "font-weight: bold"
    ].join(";");

    const commandStyle = [
        "color: #e3c66b",
        "font-family: monospace",
        "font-weight: bold",
        "font-size: 14px"
    ].join(";");

    console.log("%c" + logo, logoStyle);

    console.log(
        "%c👋",
        headingStyle
    );

    console.log(
        "%c🏆 Achievement unlocked: Sneaky Developer!",
        "color: #e3c66b; font-weight: bold;"
    );

    console.log(
        "Prøv %czugzug()%c",
        commandStyle,
        ""
    );

    // Hemmeleg kommando som kan køyrast frå Console.
    window.zugzug = function () {
        const messages = [
            "⚔️ Zug zug!",
            "⛏️ Work complete!",
            "🪓 Something need doing?",
            "💰 Time is money, friend!"
        ];

        const message = messages[
            Math.floor(Math.random() * messages.length)
        ];

        console.log(
            "%c" + message,
            [
                "color: #f3d27a",
                "background: #172117",
                "font-size: 22px",
                "font-weight: bold",
                "padding: 16px",
                "border-radius: 8px",
                "text-shadow: 0 0 10px #7da86d"
            ].join(";")
        );
    };
})();
