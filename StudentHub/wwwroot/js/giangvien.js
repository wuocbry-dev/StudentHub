document.getElementById('sidebarToggle')?.addEventListener('click', () => document.getElementById('sidebar')?.classList.toggle('show'));

(() => {
    const path = window.location.pathname.replace(/\/$/, '').toLowerCase();
    document.querySelectorAll('.admin-sidebar .nav-link').forEach(link => {
        const target = new URL(link.href).pathname.replace(/\/$/, '').toLowerCase();
        if (path === target || (target !== '/giangvien/dashboard' && path.startsWith(target + '/'))) {
            link.classList.add('active');
        }
    });
})();

document.querySelectorAll('[data-copy]').forEach(button => button.addEventListener('click', async () => {
    await navigator.clipboard.writeText(button.dataset.copy);
    const old = button.innerHTML;
    button.innerHTML = '<i class="bi bi-check"></i> Đã sao chép';
    setTimeout(() => button.innerHTML = old, 1500);
}));

function renderAttendanceQr(url) {
    const target = document.getElementById('qrcode');
    if (!target || !url) return;
    new QRCode(target, { text: url, width: 200, height: 200, colorDark: '#0f172a', colorLight: '#ffffff', correctLevel: QRCode.CorrectLevel.H });
}

function startAttendanceCountdown(startMs, endMs, serverNowMs, isOpen) {
    const value = document.getElementById('sessionCountdown');
    const label = document.getElementById('countdownLabel');
    const panel = document.getElementById('countdownPanel');
    if (!value) return;

    const clientStarted = Date.now();
    let timer;
    const format = ms => {
        const total = Math.max(0, Math.floor(ms / 1000));
        const h = Math.floor(total / 3600);
        const m = Math.floor(total % 3600 / 60);
        const s = total % 60;
        return [h, m, s].map(x => String(x).padStart(2, '0')).join(':');
    };
    const endSession = () => {
        label.textContent = 'Phiên điểm danh';
        value.textContent = 'ĐÃ KẾT THÚC';
        panel?.classList.add('expired');
        document.getElementById('qrcode')?.classList.add('qr-expired');
        const status = document.getElementById('sessionStatus');
        if (status) {
            status.className = 'badge text-bg-secondary mb-3';
            status.textContent = 'Đã hết thời gian';
        }
        document.getElementById('copyCheckIn')?.setAttribute('disabled', 'disabled');
        document.getElementById('sessionToggle')?.setAttribute('disabled', 'disabled');
        if (timer) clearInterval(timer);
    };
    const tick = () => {
        const now = serverNowMs + (Date.now() - clientStarted);
        if (now < startMs) {
            label.textContent = 'Bắt đầu sau';
            value.textContent = format(startMs - now);
            panel?.classList.remove('active');
            return;
        }
        if (now >= endMs) {
            endSession();
            return;
        }
        label.textContent = isOpen ? 'Thời gian còn lại' : 'Phiên đang đóng - thời gian còn lại';
        value.textContent = format(endMs - now);
        panel?.classList.add('active');
    };
    tick();
    timer = setInterval(tick, 1000);
}

function showLecturerFlash(success, error) {
    if (success) Swal.fire({ icon: 'success', title: 'Thành công', text: success, timer: 2200, showConfirmButton: false });
    if (error) Swal.fire({ icon: 'error', title: 'Không thể thực hiện', text: error });
}

document.querySelectorAll('form[data-confirm]').forEach(form => form.addEventListener('submit', event => {
    if (form.dataset.confirmed) return;
    event.preventDefault();
    Swal.fire({
        icon: 'question',
        title: form.dataset.confirm,
        showCancelButton: true,
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy'
    }).then(result => {
        if (result.isConfirmed) {
            form.dataset.confirmed = 'true';
            form.requestSubmit();
        }
    });
}));

document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('gradeTable')) new DataTable('#gradeTable', { language: { url: 'https://cdn.datatables.net/plug-ins/2.3.2/i18n/vi.json' }, pageLength: 25, columnDefs: [{ orderable: false, targets: [2, 3, 4, 5, 8] }] });
    if (document.getElementById('attendanceTable')) new DataTable('#attendanceTable', { language: { url: 'https://cdn.datatables.net/plug-ins/2.3.2/i18n/vi.json' }, pageLength: 25, columnDefs: [{ orderable: false, targets: [3, 4, 5] }] });
});

function updateGradePreview(row) {
    const inputs = [...row.querySelectorAll('.grade-input')];
    const total = row.querySelector('[data-grade-total]');
    const letter = row.querySelector('[data-grade-letter]');
    const hint = row.querySelector('[data-grade-hint]');
    if (inputs.length !== 4 || !total || !letter) return;
    const values = inputs.map(input => input.value.trim() === '' ? null : Number(input.value.replace(',', '.')));
    const valid = values.every(value => value !== null && Number.isFinite(value) && value >= 0 && value <= 10);
    letter.className = 'grade-letter grade-none';
    if (!valid) {
        total.textContent = '-';
        letter.textContent = '-';
        if (hint) hint.textContent = 'Nhập đủ 4 cột';
        return;
    }
    const result = Math.round((values[0] * .1 + values[1] * .2 + values[2] * .3 + values[3] * .4) * 100) / 100;
    const grade = result >= 8.5 ? 'A' : result >= 7 ? 'B' : result >= 5.5 ? 'C' : result >= 4 ? 'D' : 'F';
    total.textContent = result.toFixed(2);
    letter.textContent = grade;
    letter.className = `grade-letter grade-${grade.toLowerCase()}`;
    if (hint) hint.textContent = 'Xem trước - bấm lưu';
}

document.addEventListener('input', event => {
    if (event.target.matches('.grade-input')) updateGradePreview(event.target.closest('[data-grade-row]'));
});

function startAttendanceRefresh(url) {
    const refresh = async () => {
        try {
            const response = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!response.ok) return;
            const data = await response.json();
            data.danhSach.forEach(item => {
                const row = document.querySelector(`[data-attendance-id="${item.id}"]`);
                if (!row) return;
                const select = row.querySelector('[data-attendance-status]');
                const checkin = row.querySelector('[data-checkin]');
                const note = row.querySelector('[data-attendance-note]');
                if (select && document.activeElement !== select) select.value = item.trangThai;
                if (checkin) checkin.textContent = item.thoiGianCheckIn;
                if (note && document.activeElement !== note) note.value = item.ghiChu ?? '';
            });
        } catch { }
    };
    refresh();
    setInterval(refresh, 5000);
}
