/**
 * Loading & Animations Manager
 * Quản lý tất cả loading states và animations
 */

const LoadingManager = {
    // ===== GLOBAL LOADING =====
    show: function (message = 'Đang tải') {
        let overlay = document.getElementById('global-loading-overlay');

        if (!overlay) {
            // Tạo overlay nếu chưa có
            overlay = document.createElement('div');
            overlay.id = 'global-loading-overlay';
            overlay.innerHTML = `
                <div class="loading-content">
                    <div class="loading-spinner"></div>
                    <div class="loading-text"><span class="loading-dots">${message}</span></div>
                </div>
            `;
            document.body.appendChild(overlay);
        }

        // Hiển thị
        setTimeout(() => overlay.classList.add('show'), 10);
    },

    hide: function () {
        const overlay = document.getElementById('global-loading-overlay');
        if (overlay) {
            overlay.classList.remove('show');
        }
    },

    // ===== BUTTON LOADING =====
    buttonStart: function (button) {
        const btn = typeof button === 'string' ? document.querySelector(button) : button;
        if (!btn) return;

        // Lưu text gốc
        if (!btn.dataset.originalText) {
            btn.dataset.originalText = btn.innerHTML;
        }

        btn.disabled = true;
        btn.classList.add('btn-loading');
        btn.innerHTML = '<span class="btn-text">' + btn.dataset.originalText + '</span>';
    },

    buttonStop: function (button) {
        const btn = typeof button === 'string' ? document.querySelector(button) : button;
        if (!btn) return;

        btn.disabled = false;
        btn.classList.remove('btn-loading');
        if (btn.dataset.originalText) {
            btn.innerHTML = btn.dataset.originalText;
        }
    },

    // ===== PROGRESS BAR =====
    progressBar: {
        element: null,

        init: function () {
            if (!this.element) {
                this.element = document.createElement('div');
                this.element.className = 'progress-bar-custom';
                document.body.appendChild(this.element);
            }
        },

        show: function () {
            this.init();
            this.element.style.width = '0%';
            setTimeout(() => {
                this.element.style.width = '30%';
            }, 10);
        },

        progress: function (percent) {
            this.init();
            this.element.style.width = percent + '%';
        },

        complete: function () {
            this.init();
            this.element.style.width = '100%';
            setTimeout(() => {
                this.element.style.width = '0%';
            }, 300);
        }
    },

    // ===== SKELETON LOADING =====
    createSkeleton: function (type = 'card', count = 1) {
        const skeletons = [];

        for (let i = 0; i < count; i++) {
            let html = '';

            switch (type) {
                case 'card':
                    html = `
                        <div class="skeleton-card">
                            <div class="skeleton skeleton-image"></div>
                            <div class="skeleton skeleton-title" style="width: 80%;"></div>
                            <div class="skeleton skeleton-text" style="width: 100%;"></div>
                            <div class="skeleton skeleton-text" style="width: 60%;"></div>
                        </div>
                    `;
                    break;

                case 'list':
                    html = `
                        <div class="skeleton-card">
                            <div class="skeleton skeleton-title" style="width: 70%;"></div>
                            <div class="skeleton skeleton-text" style="width: 100%;"></div>
                            <div class="skeleton skeleton-text" style="width: 90%;"></div>
                        </div>
                    `;
                    break;

                case 'text':
                    html = `
                        <div class="skeleton skeleton-text" style="width: 100%;"></div>
                        <div class="skeleton skeleton-text" style="width: 80%;"></div>
                        <div class="skeleton skeleton-text" style="width: 90%;"></div>
                    `;
                    break;
            }

            skeletons.push(html);
        }

        return skeletons.join('');
    }
};

// ===== ANIMATION HELPERS =====
const AnimationHelper = {
    // Thêm animation class và tự động remove sau khi xong
    animate: function (element, animationClass, callback) {
        const el = typeof element === 'string' ? document.querySelector(element) : element;
        if (!el) return;

        el.classList.add(animationClass);

        const handleAnimationEnd = () => {
            el.classList.remove(animationClass);
            el.removeEventListener('animationend', handleAnimationEnd);
            if (callback) callback();
        };

        el.addEventListener('animationend', handleAnimationEnd);
    },

    // Fade in element
    fadeIn: function (element, callback) {
        this.animate(element, 'fade-in', callback);
    },

    // Fade out element
    fadeOut: function (element, callback) {
        this.animate(element, 'fade-out', callback);
    },

    // Slide in from left
    slideInLeft: function (element, callback) {
        this.animate(element, 'slide-in-left', callback);
    },

    // Slide in from right
    slideInRight: function (element, callback) {
        this.animate(element, 'slide-in-right', callback);
    },

    // Bounce animation
    bounce: function (element, callback) {
        this.animate(element, 'bounce', callback);
    },

    // Shake animation (cho error)
    shake: function (element, callback) {
        this.animate(element, 'shake', callback);
    },

    // Scale in animation
    scaleIn: function (element, callback) {
        this.animate(element, 'scale-in', callback);
    },

    // Pulse animation
    pulse: function (element, callback) {
        this.animate(element, 'pulse', callback);
    },

    // Stagger animation cho multiple elements
    stagger: function (selector, animationClass, delay = 100) {
        const elements = document.querySelectorAll(selector);
        elements.forEach((el, index) => {
            setTimeout(() => {
                el.classList.add(animationClass);
            }, index * delay);
        });
    }
};

// ===== TOAST NOTIFICATIONS =====
const Toast = {
    success: function (message, duration = 3000) {
        return Swal.fire({
            icon: 'success',
            title: message,
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: duration,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer);
                toast.addEventListener('mouseleave', Swal.resumeTimer);
            }
        });
    },

    error: function (message, duration = 3000) {
        return Swal.fire({
            icon: 'error',
            title: message,
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: duration,
            timerProgressBar: true
        });
    },

    warning: function (message, duration = 3000) {
        return Swal.fire({
            icon: 'warning',
            title: message,
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: duration,
            timerProgressBar: true
        });
    },

    info: function (message, duration = 3000) {
        return Swal.fire({
            icon: 'info',
            title: message,
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: duration,
            timerProgressBar: true
        });
    }
};

// ===== AUTO SETUP =====
document.addEventListener('DOMContentLoaded', function () {
    // Auto setup AJAX loading
    if (typeof $ !== 'undefined') {
        // Show loading khi bắt đầu AJAX request
        $(document).ajaxStart(function () {
            LoadingManager.progressBar.show();
        });

        // Hide loading khi AJAX complete
        $(document).ajaxComplete(function () {
            LoadingManager.progressBar.complete();
        });

        // Handle AJAX errors
        $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
            LoadingManager.hide();
            LoadingManager.progressBar.complete();

            if (jqxhr.status !== 0) { // Ignore aborted requests
                Toast.error('Đã có lỗi xảy ra. Vui lòng thử lại!');
            }
        });
    }

    // CHỈ animate những elements có class .animate-on-load
    const animateElements = document.querySelectorAll('.animate-on-load');
    animateElements.forEach((el, index) => {
        el.style.opacity = '0';
        setTimeout(() => {
            AnimationHelper.fadeIn(el);
        }, index * 50);
    });

    // Smooth scroll cho tất cả anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href !== '#' && href !== '#!') {
                e.preventDefault();
                const target = document.querySelector(href);
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            }
        });
    });

    // Form submit với loading
    document.querySelectorAll('form.enable-loading').forEach(form => {
        form.addEventListener('submit', function (e) {
            const submitBtn = this.querySelector('button[type="submit"], input[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                LoadingManager.buttonStart(submitBtn);
            }
        });
    });
});

// Export to global scope
window.LoadingManager = LoadingManager;
window.AnimationHelper = AnimationHelper;
window.Toast = Toast;