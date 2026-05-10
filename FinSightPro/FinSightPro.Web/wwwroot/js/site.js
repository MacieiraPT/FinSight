(function () {
    const html = document.documentElement;
    const toggle = document.getElementById('themeToggle');
    const stored = localStorage.getItem('finsight-theme') || 'light';
    applyTheme(stored);

    if (toggle) {
        toggle.addEventListener('click', () => {
            const next = html.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
            applyTheme(next);
            localStorage.setItem('finsight-theme', next);
        });
    }

    function applyTheme(t) {
        html.setAttribute('data-bs-theme', t);
        if (toggle) {
            toggle.innerHTML = t === 'dark'
                ? '<i class="fa-solid fa-sun"></i>'
                : '<i class="fa-solid fa-moon"></i>';
        }
    }

    document.querySelectorAll('input[type="checkbox"][name="IsRecurring"]').forEach(cb => {
        const form = cb.closest('form') || document.body;
        const sync = () => form.classList.toggle('recurring-active', cb.checked);
        cb.addEventListener('change', sync);
        sync();
    });

    window.fsToast = function (message, type) {
        const cls = type === 'error' ? 'bg-danger' : (type === 'warning' ? 'bg-warning' : 'bg-success');
        const container = document.getElementById('toastContainer');
        if (!container) return;
        const el = document.createElement('div');
        el.className = `toast align-items-center text-white ${cls} border-0`;
        el.role = 'alert';
        el.innerHTML = `<div class="d-flex"><div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div>`;
        container.appendChild(el);
        const t = new bootstrap.Toast(el, { delay: 4500 });
        t.show();
    };
})();
