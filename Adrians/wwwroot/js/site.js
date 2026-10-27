// Theme selection is local to the browser. The early head script restores the
// saved theme before CSS loads to avoid a dark flash in light mode.
(() => {
    'use strict';

    const toggle = document.getElementById('themeToggle');
    if (!toggle) return;

    const root = document.documentElement;
    const icon = toggle.querySelector('.site-theme-toggle-icon');
    const label = toggle.querySelector('.site-theme-toggle-label');
    const themeColor = document.querySelector('meta[name="theme-color"]');

    function updateButton() {
        const isLight = root.getAttribute('data-bs-theme') === 'light';
        const action = isLight ? 'Aktiver mørk modus' : 'Aktiver lys modus';
        toggle.setAttribute('aria-label', action);
        toggle.title = action;
        if (icon) icon.textContent = isLight ? '☾' : '☀';
        if (label) label.textContent = isLight ? 'Mørk modus' : 'Lys modus';
        if (themeColor) {
            themeColor.content = isLight ? '#f5f8f6' : '#0b1219';
        }
    }

    toggle.addEventListener('click', () => {
        const nextTheme = root.getAttribute('data-bs-theme') === 'light'
            ? 'dark'
            : 'light';

        root.setAttribute('data-bs-theme', nextTheme);
        try {
            localStorage.setItem('adrians-theme', nextTheme);
        } catch {
            // Browsers with restricted storage can still toggle for this page.
        }
        updateButton();
    });

    updateButton();
})();
