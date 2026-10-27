// Run before the stylesheets to restore light mode without a flash.
(() => {
    'use strict';
    try {
        const savedTheme = localStorage.getItem('adrians-theme');
        if (savedTheme === 'light' || savedTheme === 'dark') {
            document.documentElement.setAttribute('data-bs-theme', savedTheme);
        }
    } catch {
        // Storage may be blocked; the site's existing dark default still works.
    }
})();
