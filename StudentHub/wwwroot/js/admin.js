(() => {
    const sidebar = document.getElementById('sidebar');
    const contentSelector = '[data-admin-content]';
    let pageController = null;

    document.getElementById('sidebarToggle')?.addEventListener('click', () => sidebar?.classList.toggle('show'));

    const cleanPath = value => new URL(value, window.location.origin).pathname.replace(/\/$/, '').toLowerCase() || '/';

    const setActiveNav = url => {
        const path = cleanPath(url);
        document.querySelectorAll('.admin-sidebar .nav-link').forEach(link => {
            const target = cleanPath(link.href);
            link.classList.toggle('active', path === target || (target !== '/admin/dashboard' && path.startsWith(target + '/')));
        });
    };

    const bindDeleteForms = (root = document) => {
        root.querySelectorAll('.delete-form:not([data-admin-bound])').forEach(form => {
            form.dataset.adminBound = 'true';
            form.addEventListener('submit', event => {
                if (form.dataset.confirmed) return;
                event.preventDefault();
                const runSubmit = () => {
                    form.dataset.confirmed = 'true';
                    form.requestSubmit();
                };
                if (window.Swal) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Xóa dữ liệu này?',
                        text: 'Thao tác này không thể hoàn tác.',
                        showCancelButton: true,
                        confirmButtonText: 'Xóa',
                        cancelButtonText: 'Hủy',
                        confirmButtonColor: '#dc2626'
                    }).then(result => { if (result.isConfirmed) runSubmit(); });
                } else if (confirm('Bạn chắc chắn muốn xóa dữ liệu này?')) {
                    runSubmit();
                }
            });
        });
    };

    const closeAlertsLater = () => {
        setTimeout(() => document.querySelectorAll('.alert').forEach(el => bootstrap.Alert.getOrCreateInstance(el).close()), 5000);
    };

    const normalizeAssetUrl = value => new URL(value, window.location.href).href.replace(/([?&])v=[^&]+/, '$1').replace(/[?&]$/, '');

    const syncStyles = doc => {
        const current = new Set([...document.querySelectorAll('link[rel="stylesheet"][href]')].map(link => normalizeAssetUrl(link.href)));
        doc.querySelectorAll('head link[rel="stylesheet"][href]').forEach(link => {
            const href = normalizeAssetUrl(link.href);
            if (current.has(href)) return;
            document.head.appendChild(link.cloneNode(true));
            current.add(href);
        });
    };

    const shouldSkipScript = src => {
        const path = new URL(src, window.location.href).pathname.toLowerCase();
        return path.endsWith('/bootstrap.bundle.min.js')
            || path.endsWith('/admin.js')
            || src.toLowerCase().includes('sweetalert2');
    };

    const shouldRunEveryTime = src => {
        const path = new URL(src, window.location.href).pathname.toLowerCase();
        return path.endsWith('/js/hocvuot.js');
    };

    const loadScript = script => new Promise((resolve, reject) => {
        if (!script.src) {
            const inline = document.createElement('script');
            inline.text = script.textContent;
            document.body.appendChild(inline);
            inline.remove();
            resolve();
            return;
        }

        const src = new URL(script.src, window.location.href).href;
        if (shouldSkipScript(src)) {
            resolve();
            return;
        }

        const normalized = normalizeAssetUrl(src);
        const alreadyLoaded = [...document.querySelectorAll('script[src]')].some(existing => normalizeAssetUrl(existing.src) === normalized);
        if (alreadyLoaded && !shouldRunEveryTime(src)) {
            resolve();
            return;
        }

        const clone = document.createElement('script');
        clone.src = script.src;
        clone.async = false;
        clone.onload = () => resolve();
        clone.onerror = () => reject(new Error(`Không tải được script: ${script.src}`));
        document.body.appendChild(clone);
    });

    const runPageScripts = async doc => {
        for (const script of doc.body.querySelectorAll('script')) {
            await loadScript(script);
        }
    };

    const canNavigate = url => {
        return url.origin === window.location.origin
            && url.pathname.toLowerCase().startsWith('/admin')
            && !url.hash
            && (url.pathname.toLowerCase() !== window.location.pathname.toLowerCase()
                || url.search !== window.location.search);
    };

    const loadAdminPage = async (targetUrl, pushState = true) => {
        const url = new URL(targetUrl, window.location.href);
        if (!canNavigate(url) && url.href === window.location.href) return;

        const content = document.querySelector(contentSelector);
        if (!content) {
            window.location.href = url.href;
            return;
        }

        pageController?.abort();
        pageController = new AbortController();
        content.classList.add('is-loading');

        try {
            const response = await fetch(url.href, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                signal: pageController.signal
            });
            if (!response.ok) throw new Error(`HTTP ${response.status}`);

            const html = await response.text();
            const doc = new DOMParser().parseFromString(html, 'text/html');
            const nextContent = doc.querySelector(contentSelector);
            if (!nextContent) throw new Error('Không tìm thấy nội dung trang admin.');

            syncStyles(doc);
            document.title = doc.title || document.title;
            content.classList.remove('is-loading');
            content.classList.add('is-entering');
            content.innerHTML = nextContent.innerHTML;
            window.scrollTo({ top: 0, behavior: 'smooth' });

            requestAnimationFrame(() => content.classList.remove('is-entering'));
            if (pushState) history.pushState({ adminAjax: true }, doc.title, url.href);

            setActiveNav(url.href);
            sidebar?.classList.remove('show');
            bindDeleteForms(content);
            closeAlertsLater();
            await runPageScripts(doc);
        } catch (error) {
            if (error.name === 'AbortError') return;
            window.location.href = url.href;
        }
    };

    document.addEventListener('click', event => {
        const link = event.target.closest('a[href]');
        if (!link || link.target || link.hasAttribute('download') || link.dataset.noAjax === 'true') return;
        if (event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;

        const url = new URL(link.href, window.location.href);
        if (!canNavigate(url)) return;

        event.preventDefault();
        loadAdminPage(url.href);
    });

    document.addEventListener('submit', event => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement)) return;
        const method = (form.method || 'get').toLowerCase();
        if (method !== 'get' || form.dataset.noAjax === 'true') return;

        const url = new URL(form.action || window.location.href, window.location.href);
        if (!url.pathname.toLowerCase().startsWith('/admin')) return;

        event.preventDefault();
        const data = new FormData(form);
        url.search = new URLSearchParams(data).toString();
        loadAdminPage(url.href);
    });

    window.addEventListener('popstate', () => loadAdminPage(window.location.href, false));

    setActiveNav(window.location.href);
    bindDeleteForms();
    closeAlertsLater();
})();

function renderAdminCharts(khoaLabels, khoaValues, hocKyLabels, hocKyValues) {
    new Chart(document.getElementById('khoaChart'), {
        type: 'doughnut',
        data: { labels: khoaLabels, datasets: [{ data: khoaValues, backgroundColor: ['#2563eb', '#16a34a', '#f59e0b', '#dc2626', '#7c3aed', '#0891b2'] }] },
        options: { responsive: true, maintainAspectRatio: false }
    });
    new Chart(document.getElementById('hocKyChart'), {
        type: 'bar',
        data: { labels: hocKyLabels, datasets: [{ label: 'Số lớp', data: hocKyValues, backgroundColor: '#2563eb', borderRadius: 6 }] },
        options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } }
    });
}
