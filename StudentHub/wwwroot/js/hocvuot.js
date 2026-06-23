(() => {
    const limitPanel = document.querySelector('[data-hocvuot-limit]');
    const registerButton = document.querySelector('[data-hocvuot-register]');
    if (!limitPanel || !registerButton || !window.Swal) return;

    const isFull = limitPanel.dataset.hocvuotFull === 'true';
    registerButton.addEventListener('click', () => {
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

    if (isFull) {
        Swal.fire({
            icon: 'warning',
            title: 'Gioi han hoc vuot',
            text: 'Ban da dat gioi han 5/5 mon hoc vuot trong hoc ky nay.',
            confirmButtonText: 'Da hieu',
            confirmButtonColor: '#2563eb'
        });
    }
})();
