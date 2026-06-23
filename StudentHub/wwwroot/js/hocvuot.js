(() => {
    const limitPanel = document.querySelector('[data-hocvuot-limit]');
    const registerButton = document.querySelector('[data-hocvuot-register]');
    const modalElement = document.getElementById('hocVuotRegisterModal');
    const listElement = document.querySelector('[data-hocvuot-class-list]');
    const loadingElement = document.querySelector('[data-hocvuot-loading]');
    const emptyElement = document.querySelector('[data-hocvuot-empty]');
    const searchInput = document.querySelector('[data-hocvuot-search]');
    const modalHocKy = document.querySelector('[data-hocvuot-modal-hocky]');
    const modalNamHoc = document.querySelector('[data-hocvuot-modal-namhoc]');
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    const isFull = limitPanel?.dataset.hocvuotFull === 'true';
    let allClasses = [];

    const showAlert = (options) => {
        if (!window.Swal) {
            if (options.text || options.title) alert(options.text || options.title);
            return Promise.resolve({ isConfirmed: true });
        }
        return Swal.fire({ confirmButtonColor: '#2563eb', ...options });
    };

    const escapeHtml = (value) => String(value ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');

    const setLoading = (isLoading) => {
        if (loadingElement) loadingElement.hidden = !isLoading;
        if (listElement) listElement.hidden = isLoading;
        if (emptyElement) emptyElement.hidden = true;
    };

    const statusBadges = (item) => {
        if (item.coTheDangKy) return '<span class="badge text-bg-success">Đủ điều kiện</span>';
        const badges = [];
        if (item.daDangKy) badges.push('<span class="badge text-bg-secondary">Đã đăng ký</span>');
        if (item.daDuGioiHanHocVuot) badges.push('<span class="badge text-bg-danger">Đã đủ 5/5</span>');
        if (item.lopDaDay) badges.push('<span class="badge text-bg-warning">Lớp đã đầy</span>');
        if (item.biTrungLich) badges.push('<span class="badge text-bg-warning">Trùng lịch</span>');
        if (!item.daHocMonTienQuyet) badges.push('<span class="badge text-bg-danger">Thiếu tiên quyết</span>');
        if (Number(item.gpaHienTai) < Number(item.gpaToiThieu)) badges.push('<span class="badge text-bg-danger">GPA chưa đủ</span>');
        return badges.join('') || '<span class="badge text-bg-secondary">Không đủ điều kiện</span>';
    };

    const renderClasses = (items) => {
        if (!listElement || !emptyElement) return;
        if (items.length === 0) {
            listElement.innerHTML = '';
            emptyElement.hidden = false;
            return;
        }

        emptyElement.hidden = true;
        listElement.innerHTML = items.map(item => `
            <article class="hocvuot-class-card" data-search="${escapeHtml(`${item.maLop} ${item.maMonHoc} ${item.tenMonHoc} ${item.tenLop}`.toLowerCase())}">
                <div class="hocvuot-class-main">
                    <div>
                        <div class="hocvuot-class-title">
                            <h3>${escapeHtml(item.tenMonHoc)}</h3>
                            <span class="class-code">${escapeHtml(item.maLop)}</span>
                        </div>
                        <div class="hocvuot-class-subtitle">
                            ${escapeHtml(item.maMonHoc)} · ${item.soTinChi} tín chỉ · ${escapeHtml(item.tenLop)}
                        </div>
                    </div>
                    <div class="hocvuot-class-status">${statusBadges(item)}</div>
                </div>
                <div class="hocvuot-class-grid">
                    <span><i class="bi bi-person-workspace"></i><strong>Giảng viên</strong>${escapeHtml(item.giangVien)}</span>
                    <span><i class="bi bi-calendar-week"></i><strong>Lịch học</strong>${escapeHtml(item.lichHocText)}</span>
                    <span><i class="bi bi-door-open"></i><strong>Chỗ còn lại</strong>${item.soChoConLai}/${item.soLuongToiDa}</span>
                    <span><i class="bi bi-award"></i><strong>GPA</strong>${Number(item.gpaHienTai).toFixed(2)} / yêu cầu ${Number(item.gpaToiThieu).toFixed(1)}</span>
                    <span><i class="bi bi-diagram-3"></i><strong>Tiên quyết</strong>${item.daHocMonTienQuyet ? 'Đã đạt' : escapeHtml(item.danhSachMonTienQuyetThieu?.join(', ') || 'Chưa đạt')}</span>
                    <span><i class="bi bi-stars"></i><strong>Phù hợp</strong>${Number(item.diemPhuHop).toFixed(0)}/100 · ${escapeHtml(item.mucDoPhuHop)}</span>
                </div>
                ${item.lyDoKhongTheDangKy ? `<div class="hocvuot-deny-reason"><i class="bi bi-info-circle"></i>${escapeHtml(item.lyDoKhongTheDangKy)}</div>` : ''}
                <div class="hocvuot-class-actions">
                    <button type="button"
                            class="btn ${item.coTheDangKy ? 'btn-success' : 'btn-secondary'}"
                            data-hocvuot-submit
                            data-lop-id="${item.lopHocId}"
                            data-lop-name="${escapeHtml(item.maLop)}"
                            ${item.coTheDangKy ? '' : 'disabled'}>
                        <i class="bi bi-check2-circle"></i> Đăng ký
                    </button>
                </div>
            </article>
        `).join('');
    };

    const filterClasses = () => {
        const keyword = (searchInput?.value || '').trim().toLowerCase();
        renderClasses(keyword
            ? allClasses.filter(item => `${item.maLop} ${item.maMonHoc} ${item.tenMonHoc} ${item.tenLop}`.toLowerCase().includes(keyword))
            : allClasses);
    };

    const loadClasses = async () => {
        if (!registerButton?.dataset.hocvuotListUrl) return;
        setLoading(true);
        try {
            const url = new URL(registerButton.dataset.hocvuotListUrl, window.location.origin);
            url.searchParams.set('hocKy', modalHocKy?.value || registerButton.dataset.hocvuotHocky || '');
            url.searchParams.set('namHoc', modalNamHoc?.value || registerButton.dataset.hocvuotNamhoc || '');
            const response = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!response.ok) throw new Error('Không tải được danh sách lớp học vượt.');
            const data = await response.json();
            allClasses = data.items || [];
            filterClasses();
        } catch (error) {
            allClasses = [];
            renderClasses([]);
            showAlert({ icon: 'error', title: 'Không tải được dữ liệu', text: error.message });
        } finally {
            setLoading(false);
        }
    };

    const submitRegistration = async (button) => {
        const lopHocId = button.dataset.lopId;
        const lopName = button.dataset.lopName;
        const confirm = await showAlert({
            icon: 'question',
            title: `Đăng ký lớp ${lopName}?`,
            text: 'Hệ thống sẽ kiểm tra lại toàn bộ điều kiện trước khi lưu.',
            showCancelButton: true,
            confirmButtonText: 'Đăng ký',
            cancelButtonText: 'Hủy'
        });
        if (!confirm.isConfirmed) return;

        const formData = new FormData();
        formData.append('lopHocId', lopHocId);
        formData.append('hocKy', modalHocKy?.value || registerButton.dataset.hocvuotHocky || '');
        formData.append('namHoc', modalNamHoc?.value || registerButton.dataset.hocvuotNamhoc || '');
        if (tokenInput) formData.append('__RequestVerificationToken', tokenInput.value);

        button.disabled = true;
        const original = button.innerHTML;
        button.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Đang xử lý';
        try {
            const response = await fetch(registerButton.dataset.hocvuotPostUrl, {
                method: 'POST',
                body: formData,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            const data = await response.json();
            if (!response.ok || !data.thanhCong) throw new Error(data.thongBao || 'Đăng ký học vượt thất bại.');
            await showAlert({ icon: 'success', title: 'Đăng ký thành công', text: data.thongBao });
            window.location.reload();
        } catch (error) {
            button.disabled = false;
            button.innerHTML = original;
            showAlert({ icon: 'error', title: 'Không thể đăng ký', text: error.message });
            await loadClasses();
        }
    };

    registerButton?.addEventListener('click', () => {
        if (isFull) {
            showAlert({
                icon: 'warning',
                title: 'Giới hạn học vượt',
                text: 'Bạn đã đạt giới hạn 5/5 môn học vượt trong học kỳ này.'
            });
            return;
        }
        if (!modalElement || !window.bootstrap) return;
        bootstrap.Modal.getOrCreateInstance(modalElement).show();
        loadClasses();
    });

    searchInput?.addEventListener('input', filterClasses);
    modalHocKy?.addEventListener('change', loadClasses);
    modalNamHoc?.addEventListener('change', loadClasses);
    listElement?.addEventListener('click', event => {
        const button = event.target.closest('[data-hocvuot-submit]');
        if (!button) return;
        submitRegistration(button);
    });

    document.querySelectorAll('form[data-hocvuot-confirm]').forEach(form => {
        form.addEventListener('submit', event => {
            if (form.dataset.confirmed) return;
            event.preventDefault();
            showAlert({
                icon: 'question',
                title: form.dataset.hocvuotConfirm || 'Xác nhận thao tác?',
                showCancelButton: true,
                confirmButtonText: 'Xác nhận',
                cancelButtonText: 'Hủy'
            }).then(result => {
                if (!result.isConfirmed) return;
                form.dataset.confirmed = 'true';
                form.requestSubmit();
            });
        });
    });

    if (window.jQuery && window.DataTable) {
        ['#monHocTienQuyetTable', '#goiYHocVuotTable'].forEach(selector => {
            const table = document.querySelector(selector);
            if (!table || table.querySelector('tbody td[colspan]')) return;
            new DataTable(selector, { pageLength: 10, language: { search: 'Tìm kiếm:', lengthMenu: 'Hiện _MENU_ dòng', info: 'Hiện _START_ đến _END_ / _TOTAL_', paginate: { previous: 'Trước', next: 'Sau' } } });
        });
    }
})();
