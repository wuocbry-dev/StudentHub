(() => {
    const form = document.querySelector('[data-nhapdulieu-upload]');
    const fileInput = document.getElementById('file');
    const fileName = document.querySelector('[data-file-name]');
    const templateLink = document.querySelector('[data-template-link]');
    const loaiDuLieu = document.getElementById('loaiDuLieu');
    const loaiFileMau = document.getElementById('loaiFileMau');
    const allowed = ['.xlsx', '.txt', '.docx'];

    const updateTemplateLink = () => {
        if (!templateLink || !loaiDuLieu || !loaiFileMau) return;
        templateLink.href = `/Admin/NhapDuLieu/TaiFileMau?loaiDuLieu=${encodeURIComponent(loaiDuLieu.value)}&loaiFile=${encodeURIComponent(loaiFileMau.value)}`;
    };

    const showAlert = (title, text, icon = 'warning') => {
        if (window.Swal) Swal.fire({ icon, title, text, confirmButtonColor: '#2563eb' });
        else alert(`${title}\n${text}`);
    };

    fileInput?.addEventListener('change', () => {
        const file = fileInput.files?.[0];
        fileName && (fileName.textContent = file ? file.name : 'Chưa chọn file');
    });

    loaiDuLieu?.addEventListener('change', updateTemplateLink);
    loaiFileMau?.addEventListener('change', updateTemplateLink);
    updateTemplateLink();

    form?.addEventListener('submit', event => {
        const file = fileInput?.files?.[0];
        if (!file) {
            event.preventDefault();
            showAlert('Chưa chọn file', 'Vui lòng chọn file cần kiểm tra.');
            return;
        }
        const lower = file.name.toLowerCase();
        if (!allowed.some(ext => lower.endsWith(ext))) {
            event.preventDefault();
            showAlert('Sai định dạng', 'Chỉ hỗ trợ file .xlsx, .txt, .docx.');
            return;
        }
        const button = form.querySelector('button[type="submit"], button:not([type])');
        if (button) {
            button.disabled = true;
            button.innerHTML = `<span class="spinner-border spinner-border-sm"></span> ${button.dataset.loadingText || 'Đang xử lý...'}`;
        }
    });

    document.querySelectorAll('form[data-nhapdulieu-confirm],form[data-nhapdulieu-cancel]').forEach(confirmForm => {
        confirmForm.addEventListener('submit', event => {
            if (confirmForm.dataset.confirmed) return;
            event.preventDefault();
            const title = confirmForm.dataset.nhapdulieuConfirm || confirmForm.dataset.nhapdulieuCancel || 'Xác nhận thao tác?';
            const icon = confirmForm.dataset.nhapdulieuCancel ? 'warning' : 'question';
            const confirmButtonColor = confirmForm.dataset.nhapdulieuCancel ? '#dc2626' : '#2563eb';
            if (!window.Swal) {
                if (confirm(title)) {
                    confirmForm.dataset.confirmed = 'true';
                    confirmForm.requestSubmit();
                }
                return;
            }
            Swal.fire({
                icon,
                title,
                showCancelButton: true,
                confirmButtonText: 'Xác nhận',
                cancelButtonText: 'Hủy',
                confirmButtonColor
            }).then(result => {
                if (!result.isConfirmed) return;
                confirmForm.dataset.confirmed = 'true';
                confirmForm.requestSubmit();
            });
        });
    });
})();
