/**
 * Quick View Functionality
 * Compatible with Cart.js
 */

const QuickView = {
    currentFoodId: null,

    show: function (foodId) {
        this.currentFoodId = foodId;

        // Show loading overlay
        Swal.fire({
            title: 'Đang tải...',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        // Fetch food details
        $.get(`/Menu/GetFoodDetails/${foodId}`)
            .done((data) => {
                Swal.close();
                this.populate(data);
                $('#quickViewModal').modal('show');
            })
            .fail(() => {
                Swal.close();
                showToast('Lỗi', 'Không thể tải thông tin món ăn', 'error');
            });
    },

    populate: function (food) {
        // Image
        const imgSrc = food.imageUrl || '/images/default-food.jpg';
        $('#qv-image').attr('src', imgSrc).attr('alt', food.name);

        // Details
        $('#qv-name').text(food.name);
        $('#qv-price').text(food.price.toLocaleString('vi-VN') + 'đ');
        $('#qv-description').text(food.description || 'Không có mô tả');
        $('#qv-category').text(food.categoryName || '');
        $('#qv-details-link').attr('href', `/Menu/Details/${food.foodId}`);

        // Availability
        if (food.isAvailable) {
            $('#qv-available-section').removeClass('d-none');
            $('#qv-unavailable').addClass('d-none');
            $('#qv-add-cart').data('food-id', food.foodId);
        } else {
            $('#qv-available-section').addClass('d-none');
            $('#qv-unavailable').removeClass('d-none');
        }

        // Reset quantity
        $('#qv-quantity').val(1);
    },

    init: function () {
        // Quantity controls
        $('#qv-decrease').click(() => {
            const input = $('#qv-quantity');
            const val = parseInt(input.val());
            if (val > 1) input.val(val - 1);
        });

        $('#qv-increase').click(() => {
            const input = $('#qv-quantity');
            const val = parseInt(input.val());
            if (val < 10) input.val(val + 1);
        });

        // Add to cart (using Cart.add from cart.js)
        $('#qv-add-cart').click(function () {
            const foodId = $(this).data('food-id');
            const quantity = parseInt($('#qv-quantity').val());

            // Use Cart.add from cart.js
            Cart.add(foodId, quantity, this);

            // Close modal after adding
            setTimeout(() => {
                $('#quickViewModal').modal('hide');
            }, 1500);
        });
    }
};

// Initialize
$(document).ready(() => {
    QuickView.init();
});

// Export
window.QuickView = QuickView;