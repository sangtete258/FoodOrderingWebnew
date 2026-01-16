/**
 * Mobile Menu Handler
 * Xử lý mobile sidebar cho Admin
 */

document.addEventListener('DOMContentLoaded', function () {
    // Chỉ chạy trên mobile
    if (window.innerWidth < 992) {
        initMobileSidebar();
    }

    // Re-init khi resize
    window.addEventListener('resize', function () {
        if (window.innerWidth < 992 && !document.querySelector('.sidebar-toggle')) {
            initMobileSidebar();
        } else if (window.innerWidth >= 992) {
            removeMobileSidebar();
        }
    });
});

function initMobileSidebar() {
    const sidebar = document.querySelector('.sidebar');
    if (!sidebar) return;

    // Tạo toggle button nếu chưa có
    if (!document.querySelector('.sidebar-toggle')) {
        const toggleBtn = document.createElement('button');
        toggleBtn.className = 'sidebar-toggle';
        toggleBtn.innerHTML = '<i class="bi bi-list"></i>';
        toggleBtn.onclick = toggleSidebar;
        document.body.appendChild(toggleBtn);
    }

    // Tạo overlay nếu chưa có
    if (!document.querySelector('.sidebar-overlay')) {
        const overlay = document.createElement('div');
        overlay.className = 'sidebar-overlay';
        overlay.onclick = closeSidebar;
        document.body.appendChild(overlay);
    }

    // Đóng sidebar khi click vào menu item
    const menuItems = sidebar.querySelectorAll('a:not(.dropdown-toggle)');
    menuItems.forEach(item => {
        item.addEventListener('click', closeSidebar);
    });
}

function removeMobileSidebar() {
    const toggleBtn = document.querySelector('.sidebar-toggle');
    const overlay = document.querySelector('.sidebar-overlay');
    const sidebar = document.querySelector('.sidebar');

    if (toggleBtn) toggleBtn.remove();
    if (overlay) overlay.remove();
    if (sidebar) {
        sidebar.classList.remove('show');
    }
}

function toggleSidebar() {
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.querySelector('.sidebar-overlay');

    if (sidebar && overlay) {
        sidebar.classList.toggle('show');
        overlay.classList.toggle('show');

        // Prevent body scroll when sidebar open
        if (sidebar.classList.contains('show')) {
            document.body.style.overflow = 'hidden';
        } else {
            document.body.style.overflow = '';
        }
    }
}

function closeSidebar() {
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.querySelector('.sidebar-overlay');

    if (sidebar && overlay) {
        sidebar.classList.remove('show');
        overlay.classList.remove('show');
        document.body.style.overflow = '';
    }
}

// Export functions
window.toggleSidebar = toggleSidebar;
window.closeSidebar = closeSidebar;