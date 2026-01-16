// ========================================
// AJAX CART SYSTEM
// ========================================

const Cart = {
    // 🛒 Add to Cart with Animation
    add: function (foodId, quantity = 1, buttonElement) {
        const $btn = $(buttonElement);
        const originalHtml = $btn.html();

        // Disable button
        $btn.prop('disabled', true).html('<i class="bi bi-hourglass-split spinner-border spinner-border-sm"></i> Đang thêm...');

        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: { foodId: foodId, quantity: quantity },
            success: function (result) {
                if (result.success) {
                    // ✅ Update cart badge
                    Cart.updateBadge(result.cartCount);

                    // ✅ Show success toast
                    showToast('Thành công!', result.message || 'Đã thêm vào giỏ hàng', 'success');

                    // ✅ Animate button
                    $btn.addClass('btn-success').html('<i class="bi bi-check-circle"></i> Đã thêm');

                    // ✅ Update mini cart
                    Cart.refreshMiniCart();

                    // ✅ Reset button after 1.5s
                    setTimeout(function () {
                        $btn.removeClass('btn-success').html(originalHtml);
                    }, 1500);
                } else {
                    showToast('Lỗi', 'Không thể thêm vào giỏ hàng', 'error');
                }
            },
            error: function () {
                showToast('Lỗi', 'Có lỗi xảy ra. Vui lòng thử lại!', 'error');
            },
            complete: function () {
                $btn.prop('disabled', false);
            }
        });
    },

    // 📊 Update Quantity
    updateQuantity: function (foodId, quantity) {
        $.ajax({
            url: '/Cart/UpdateQuantity',
            type: 'POST',
            data: { foodId: foodId, quantity: quantity },
            success: function (result) {
                if (result.success) {
                    // Update item subtotal
                    const $item = $(`[data-food-id="${foodId}"]`);
                    $item.find('.item-subtotal').text(result.itemSubTotal.toLocaleString('vi-VN') + 'đ');

                    // Update cart totals
                    $('#cartTotalAmount').text(result.cartTotal.toLocaleString('vi-VN') + 'đ');
                    $('#cartFinalTotal').text(result.cartFinalTotal.toLocaleString('vi-VN') + 'đ');
                    $('#cartShippingFee').text(result.shippingFee.toLocaleString('vi-VN') + 'đ');

                    // Update badge
                    Cart.updateBadge(result.cartCount);

                    // Refresh mini cart
                    Cart.refreshMiniCart();

                    // Show animation
                    $item.addClass('cart-item-updated');
                    setTimeout(() => $item.removeClass('cart-item-updated'), 500);
                }
            },
            error: function () {
                showToast('Lỗi', 'Không thể cập nhật số lượng', 'error');
            }
        });
    },

    // 🗑️ Remove Item
    remove: function (foodId) {
        Swal.fire({
            title: 'Xác nhận xóa?',
            text: 'Bạn muốn xóa món này khỏi giỏ hàng?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy',
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/Cart/RemoveFromCart',
                    type: 'POST',
                    data: { foodId: foodId },
                    success: function (response) {
                        if (response.success) {
                            // Animate removal
                            const $item = $(`[data-food-id="${foodId}"]`).closest('.cart-item, tr');
                            $item.fadeOut(300, function () {
                                $(this).remove();

                                // Update totals
                                $('#cartTotalAmount').text(response.cartTotal.toLocaleString('vi-VN') + 'đ');
                                $('#cartFinalTotal').text(response.cartFinalTotal.toLocaleString('vi-VN') + 'đ');
                                $('#cartShippingFee').text(response.shippingFee.toLocaleString('vi-VN') + 'đ');

                                // Update badge
                                Cart.updateBadge(response.cartCount);

                                // Refresh mini cart
                                Cart.refreshMiniCart();

                                // Show success
                                showToast('Đã xóa', response.message, 'success');

                                // Check if cart is empty
                                if (response.cartCount === 0) {
                                    location.reload();
                                }
                            });
                        }
                    }
                });
            }
        });
    },

    // 🔄 Refresh Mini Cart Dropdown
    refreshMiniCart: function () {
        $.ajax({
            url: '/Cart/GetCartSummary',
            type: 'GET',
            success: function (html) {
                $('#miniCartDropdown').html(html);
                Cart.initMiniCartEvents();
            }
        });
    },

    // 🔢 Update Cart Badge
    updateBadge: function (count) {
        const $badge = $('#cart-count');

        if (count > 0) {
            $badge.text(count).removeClass('d-none');

            // Pulse animation
            $badge.addClass('cart-badge-pulse');
            setTimeout(() => $badge.removeClass('cart-badge-pulse'), 600);
        } else {
            $badge.text('0').addClass('d-none');
        }
    },

    // 🎯 Initialize Mini Cart Events
    initMiniCartEvents: function () {
        // Increase quantity
        $('.btn-increase-qty').off('click').on('click', function () {
            const foodId = $(this).data('food-id');
            const $qtyDisplay = $(this).siblings('.qty-display');
            const currentQty = parseInt($qtyDisplay.text());
            Cart.updateQuantity(foodId, currentQty + 1);
        });

        // Decrease quantity
        $('.btn-decrease-qty').off('click').on('click', function () {
            const foodId = $(this).data('food-id');
            const $qtyDisplay = $(this).siblings('.qty-display');
            const currentQty = parseInt($qtyDisplay.text());

            if (currentQty > 1) {
                Cart.updateQuantity(foodId, currentQty - 1);
            }
        });

        // Remove item
        $('.btn-remove-item').off('click').on('click', function () {
            const foodId = $(this).data('food-id');
            Cart.remove(foodId);
        });
    },

    // 📍 Initialize on Page Load
    init: function () {
        // Load cart count
        $.get('/Cart/GetCartCount', function (result) {
            if (result.success) {
                Cart.updateBadge(result.cartCount);
            }
        });

        // Load mini cart on dropdown open
        $('#cartDropdown').on('show.bs.dropdown', function () {
            Cart.refreshMiniCart();
        });

        // Add to cart buttons
        $(document).on('click', '.add-to-cart', function () {
            const foodId = $(this).data('food-id');
            const quantity = $(this).data('quantity') || 1;
            Cart.add(foodId, quantity, this);
        });
    }
};

// ========================================
// TOAST NOTIFICATION
// ========================================
function showToast(title, message, type = 'success') {
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });

    Toast.fire({
        icon: type,
        title: title,
        text: message
    });
}

// ========================================
// INITIALIZE ON DOCUMENT READY
// ========================================
$(document).ready(function () {
    Cart.init();
});