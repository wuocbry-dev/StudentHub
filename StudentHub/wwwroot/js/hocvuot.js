(() => {
    const limitPanel = document.querySelector('[data-hocvuot-limit]');
    const registerButton = document.querySelector('[data-hocvuot-register]');
    if (!window.Swal) return;

    const isFull = limitPanel?.dataset.hocvuotFull === 'true';
    registerButton?.addEventListener('click', () => {
        Swal.fire({
            icon: isFull ? 'error' : 'info',
            title: isFull ? 'Đã đạt giới hạn học vượt' : 'Đăng ký học vượt',
            text: isFull
                ? 'Bạn đã đạt giới hạn 5/5 môn học vượt trong học kỳ này.'
                : 'Chức năng đăng ký sẽ tiếp tục kiểm tra giới hạn học vượt ở server-side.',
            confirmButtonText: 'Đã hiểu',
            confirmButtonColor: '#2563eb'
        });
    });

    if (limitPanel && isFull) {
        Swal.fire({
            icon: 'warning',
            title: 'Giới hạn học vượt',
            text: 'Bạn đã đạt giới hạn 5/5 môn học vượt trong học kỳ này.',
            confirmButtonText: 'Đã hiểu',
            confirmButtonColor: '#2563eb'
        });
    }

    document.querySelectorAll('form[data-hocvuot-confirm]').forEach(form => {
        form.addEventListener('submit', event => {
            if (form.dataset.confirmed) return;
            event.preventDefault();
            Swal.fire({
                icon: 'question',
                title: form.dataset.hocvuotConfirm || 'Xác nhận thao tác?',
                showCancelButton: true,
                confirmButtonText: 'Xác nhận',
                cancelButtonText: 'Hủy',
                confirmButtonColor: '#2563eb'
            }).then(result => {
                if (!result.isConfirmed) return;
                form.dataset.confirmed = 'true';
                const loadingButton = form.querySelector('[data-hocvuot-loading]');
                if (loadingButton) {
                    loadingButton.disabled = true;
                    loadingButton.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Đang xử lý';
                }
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
