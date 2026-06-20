(() => {
    const sidebar = document.getElementById('studentSidebar');
    const overlay = document.getElementById('studentOverlay');
    const toggle = document.getElementById('studentSidebarToggle');
    const closeSidebar = () => {
        sidebar?.classList.remove('show');
        overlay?.classList.remove('show');
        document.body.classList.remove('sidebar-open');
    };
    toggle?.addEventListener('click', () => {
        sidebar?.classList.toggle('show');
        overlay?.classList.toggle('show');
        document.body.classList.toggle('sidebar-open');
    });
    overlay?.addEventListener('click', closeSidebar);
    window.addEventListener('resize', () => { if (window.innerWidth >= 992) closeSidebar(); });

    const path = window.location.pathname.replace(/\/$/, '').toLowerCase();
    document.querySelectorAll('.student-nav-link').forEach(link => {
        const target = new URL(link.href).pathname.replace(/\/$/, '').toLowerCase();
        if (path === target || (target === '/sinhvien/dashboard' && path === '/sinhvien'))
            link.classList.add('active');
    });

    const address = document.getElementById('DiaChi');
    const counter = document.getElementById('addressCount');
    const updateCount = () => { if (counter && address) counter.textContent = address.value.length; };
    address?.addEventListener('input', updateCount);
    updateCount();
})();
