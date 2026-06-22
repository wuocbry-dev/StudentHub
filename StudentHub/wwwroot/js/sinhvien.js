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

    window.addEventListener('load', () => {
        const calendarElement = document.getElementById('studentCalendar');
        if (calendarElement && window.FullCalendar) {
            let semester = null;
            try { semester = JSON.parse(calendarElement.dataset.semester || 'null'); } catch { semester = null; }
            const calendar = new FullCalendar.Calendar(calendarElement, {
                locale: 'vi',
                initialDate: calendarElement.dataset.date,
                initialView: calendarElement.dataset.view || 'timeGridWeek',
                firstDay: 1,
                allDaySlot: false,
                nowIndicator: true,
                height: 'auto',
                slotMinTime: '06:00:00',
                slotMaxTime: '22:00:00',
                headerToolbar: { left: 'prev,next today', center: 'title', right: 'dayGridMonth,timeGridWeek,listWeek' },
                buttonText: { today: 'Hôm nay', month: 'Tháng', week: 'Tuần', list: 'Danh sách' },
                events: { url: calendarElement.dataset.endpoint, extraParams: { hocKy: semester || '' } },
                eventDidMount(info) {
                    const props = info.event.extendedProps;
                    info.el.title = `${info.event.title}\n${props.phongHoc}\n${props.giangVien}${props.trungLich ? '\nTrùng lịch' : ''}`;
                }
            });
            calendar.render();
            document.querySelector('[data-bs-target="#scheduleCalendar"]')?.addEventListener('shown.bs.tab', () => calendar.updateSize());
        }

        const chartOptions = {
            responsive: true,
            maintainAspectRatio: false,
            scales: { y: { beginAtZero: true, max: 10, ticks: { stepSize: 2 } } },
            plugins: { legend: { display: false } }
        };
        const createChart = (id, type, label, color) => {
            const canvas = document.getElementById(id);
            if (!canvas || !window.Chart) return;
            const labels = JSON.parse(canvas.dataset.labels || '[]');
            const values = JSON.parse(canvas.dataset.values || '[]');
            new Chart(canvas, { type, data: { labels, datasets: [{ label, data: values, backgroundColor: color, borderColor: color, borderWidth: 2, borderRadius: 6, tension: .3 }] }, options: chartOptions });
        };
        createChart('subjectGradeChart', 'bar', 'Điểm tổng kết', '#2563eb');
        createChart('semesterGpaChart', 'line', 'GPA', '#7c3aed');
    });
})();

function showStudentCheckInAlert(success, message) {
    if (!window.Swal) return;
    Swal.fire({
        icon: success ? 'success' : 'error',
        title: success ? 'Check-in thành công' : 'Không thể check-in',
        text: message,
        confirmButtonText: 'Đã hiểu',
        confirmButtonColor: '#2563eb'
    });
}

document.querySelectorAll('form[data-student-confirm]').forEach(form => form.addEventListener('submit', event => {
    if (form.dataset.confirmed) return;
    event.preventDefault();
    Swal.fire({
        icon: 'question', title: form.dataset.studentConfirm,
        showCancelButton: true, confirmButtonText: 'Xác nhận', cancelButtonText: 'Hủy',
        confirmButtonColor: '#2563eb'
    }).then(result => {
        if (result.isConfirmed) {
            form.dataset.confirmed = 'true';
            form.requestSubmit();
        }
    });
}));
