// KittyPhone Storefront & Admin Interactivity

// 1. Hàm hiển thị Toast thông báo giỏ hàng chuyên nghiệp kiểu CellphoneS
function showCartToast(message, isSuccess = true) {
    let container = document.getElementById('kitty-toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'kitty-toast-container';
        container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
    }

    const toastId = 'toast-' + Date.now();
    const borderClass = isSuccess ? 'border-success' : 'border-danger';
    const iconClass = isSuccess ? 'bi-check-circle-fill text-success' : 'bi-exclamation-triangle-fill text-danger';

    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center bg-white border-2 ${borderClass} shadow-lg rounded-3 mb-2" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex align-items-center p-2">
                <div class="p-2">
                    <i class="bi ${iconClass} fs-4"></i>
                </div>
                <div class="toast-body flex-grow-1 py-1 px-2">
                    <div class="fw-bold text-dark" style="font-size: 0.9rem;">${isSuccess ? 'Thông báo KittyPhone' : 'Rất tiếc!'}</div>
                    <div class="text-secondary small">${message}</div>
                    ${isSuccess ? '<div class="mt-1"><a href="/Cart" class="btn btn-sm btn-outline-primary py-0 px-2 fw-semibold" style="font-size: 0.75rem;">Xem giỏ hàng <i class="bi bi-arrow-right"></i></a></div>' : ''}
                </div>
                <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', toastHtml);
    const toastEl = document.getElementById(toastId);
    if (window.bootstrap && bootstrap.Toast) {
        const bsToast = new bootstrap.Toast(toastEl, { delay: 3500 });
        bsToast.show();
        toastEl.addEventListener('hidden.bs.toast', function () {
            toastEl.remove();
        });
    } else {
        setTimeout(function () {
            toastEl.remove();
        }, 3500);
    }
}
window.showCartToast = showCartToast;

// 2. Cập nhật số lượng badge giỏ hàng mượt mà (cả header và mobile nav)
function updateCartBadges(count) {
    const desktopBadge = document.getElementById('cartBadgeCount');
    const mobileBadge = document.getElementById('mobileCartBadgeCount');

    [desktopBadge, mobileBadge].forEach(function (badge) {
        if (badge) {
            badge.textContent = count;
            badge.style.display = 'inline-flex';
            badge.classList.remove('badge-bump');
            // Trigger reflow to restart animation
            void badge.offsetWidth;
            badge.classList.add('badge-bump');
        }
    });
}
window.updateCartBadges = updateCartBadges;

// 3. Mở Modal thông tin & chính sách theo từng mục cụ thể
function openPolicyModal(tabKey, evt) {
    if (evt && evt.preventDefault) {
        evt.preventDefault();
    }
    const modalEl = document.getElementById('modalPolicyInfo');
    if (!modalEl) {
        window.location.href = '/Home/Policy?tab=' + encodeURIComponent(tabKey);
        return false;
    }

    // Kích hoạt tab tương ứng trong modal
    const tabBtn = document.getElementById('m-tab-' + tabKey);
    if (tabBtn) {
        if (window.bootstrap && bootstrap.Tab) {
            const bsTab = bootstrap.Tab.getOrCreateInstance(tabBtn);
            bsTab.show();
        } else {
            const allTabs = modalEl.querySelectorAll('#modalPolicyTabs .nav-link');
            const allPanes = modalEl.querySelectorAll('#modalPolicyTabContent .tab-pane');
            allTabs.forEach(function(t) { t.classList.remove('active'); });
            allPanes.forEach(function(p) { p.classList.remove('show', 'active'); });
            tabBtn.classList.add('active');
            const targetPane = document.getElementById('m-pane-' + tabKey);
            if (targetPane) targetPane.classList.add('show', 'active');
        }
    }

    // Hiển thị modal
    if (window.bootstrap && bootstrap.Modal) {
        const bsModal = bootstrap.Modal.getOrCreateInstance(modalEl);
        bsModal.show();
    } else if (window.$) {
        $(modalEl).modal('show');
    }
    return false;
}
window.openPolicyModal = openPolicyModal;

document.addEventListener("DOMContentLoaded", function () {
    // Bỏ tự động lướt trang xuống danh mục (tránh gây chóng mặt cho người dùng khi bấm chuyển danh mục/bộ lọc)

    // 3. Bắt sự kiện submit form giỏ hàng để thêm bằng AJAX mượt mà
    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (!form || !form.matches) return;

        // Kiểm tra xem có phải là form thêm vào giỏ hàng
        const action = form.getAttribute('action') || '';
        const isCartForm = action.includes('/Cart/AddToCart') || 
                           action.includes('/Cart/Add') ||
                           form.matches('form[action*="/Cart/AddToCart"]') || 
                           form.matches('form[action*="/Cart/Add"]');

        if (!isCartForm) return;

        const buyNowInput = form.querySelector('input[name="buyNow"]');
        if (buyNowInput && (buyNowInput.value === 'true' || buyNowInput.value === 'True')) {
            // "Mua ngay" cho phép submit bình thường để chuyển thẳng tới Checkout
            return;
        }

        e.preventDefault();

        const submitBtn = form.querySelector('button[type="submit"]');
        const originalContent = submitBtn ? submitBtn.innerHTML : null;

        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm text-primary" role="status"></span>';
        }

        const formData = new FormData(form);

        fetch(form.action, {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(function (response) {
            return response.json();
        })
        .then(function (data) {
            if (data.success) {
                updateCartBadges(data.count);
                showCartToast(data.message || 'Đã thêm sản phẩm vào giỏ hàng!', true);
                if (submitBtn) {
                    submitBtn.innerHTML = '<i class="bi bi-check-lg text-success"></i>';
                }
            } else {
                showCartToast(data.message || 'Không thể thêm vào giỏ hàng', false);
                if (submitBtn && originalContent) {
                    submitBtn.innerHTML = originalContent;
                }
            }
        })
        .catch(function (err) {
            console.error('Lỗi khi thêm vào giỏ hàng:', err);
            // Fallback submit bình thường nếu fetch thất bại
            form.submit();
        })
        .finally(function () {
            if (submitBtn) {
                setTimeout(function () {
                    submitBtn.disabled = false;
                    if (originalContent) {
                        submitBtn.innerHTML = originalContent;
                    }
                }, 1000);
            }
        });
    });
});
