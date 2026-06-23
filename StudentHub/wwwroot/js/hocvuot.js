(() => {
    const limitPanel = document.querySelector('[data-hocvuot-limit]');
    const registerButton = document.querySelector('[data-hocvuot-register]');
    if (!window.Swal) return;

    const isFull = limitPanel?.dataset.hocvuotFull === 'true';
    registerButton?.addEventListener('click', () => {
        Swal.fire({
            icon: isFull ? 'error' : 'info',
            title: isFull ? 'Da dat gioi han hoc vuot' : 'Dang ky hoc vuot',
            text: isFull
                ? 'Ban da dat gioi han 5/5 mon hoc vuot trong hoc ky nay.'
                : 'Chuc nang dang ky se tiep tuc kiem tra gioi han hoc vuot o server-side.',
            confirmButtonText: 'Da hieu',
            confirmButtonColor: '#2563eb'
        });
    });

    if (limitPanel && isFull) {
        Swal.fire({
            icon: 'warning',
            title: 'Gioi han hoc vuot',
            text: 'Ban da dat gioi han 5/5 mon hoc vuot trong hoc ky nay.',
            confirmButtonText: 'Da hieu',
            confirmButtonColor: '#2563eb'
        });
    }

    document.querySelectorAll('form[data-hocvuot-confirm]').forEach(form => {
        form.addEventListener('submit', event => {
            if (form.dataset.confirmed) return;
            event.preventDefault();
            Swal.fire({
                icon: 'question',
                title: form.dataset.hocvuotConfirm || 'Xac nhan thao tac?',
                showCancelButton: true,
                confirmButtonText: 'Xac nhan',
                cancelButtonText: 'Huy',
                confirmButtonColor: '#2563eb'
            }).then(result => {
                if (!result.isConfirmed) return;
                form.dataset.confirmed = 'true';
                const loadingButton = form.querySelector('[data-hocvuot-loading]');
                if (loadingButton) {
                    loadingButton.disabled = true;
                    loadingButton.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Dang xu ly';
                }
                form.requestSubmit();
            });
        });
    });

    if (window.jQuery && window.DataTable) {
        ['#monHocTienQuyetTable', '#goiYHocVuotTable'].forEach(selector => {
            const table = document.querySelector(selector);
            if (!table || table.querySelector('tbody td[colspan]')) return;
            new DataTable(selector, { pageLength: 10, language: { search: 'Tim kiem:', lengthMenu: 'Hien _MENU_ dong', info: 'Hien _START_ den _END_ / _TOTAL_', paginate: { previous: 'Truoc', next: 'Sau' } } });
        });
    }
})();
