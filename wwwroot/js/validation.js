/**
 * =====================================================
 * VALIDATION.JS - Advanced Form Validation
 * Tác giả: Food Ordering System
 * Mô tả: Client-side validation nâng cao với jQuery
 * =====================================================
 */

(function ($) {
    'use strict';

    // =====================================================
    // 1. CUSTOM ERROR MESSAGES - TIẾNG VIỆT
    // =====================================================
    if ($.validator) {
        $.validator.setDefaults({
            errorClass: 'is-invalid',
            validClass: 'is-valid',
            errorElement: 'div',
            errorPlacement: function (error, element) {
                error.addClass('invalid-feedback');

                // Nếu element nằm trong input-group
                if (element.parent('.input-group').length) {
                    error.insertAfter(element.parent());
                }
                // Nếu là checkbox/radio
                else if (element.is(':checkbox') || element.is(':radio')) {
                    error.insertAfter(element.parent());
                }
                // Mặc định
                else {
                    error.insertAfter(element);
                }
            },
            highlight: function (element, errorClass, validClass) {
                $(element).addClass(errorClass).removeClass(validClass);
                $(element).closest('.form-group').addClass('has-error');
            },
            unhighlight: function (element, errorClass, validClass) {
                $(element).removeClass(errorClass).addClass(validClass);
                $(element).closest('.form-group').removeClass('has-error');
            }
        });

        // Custom error messages tiếng Việt
        $.validator.messages = {
            required: "Trường này là bắt buộc",
            remote: "Vui lòng kiểm tra lại giá trị này",
            email: "Vui lòng nhập địa chỉ email hợp lệ",
            url: "Vui lòng nhập URL hợp lệ",
            date: "Vui lòng nhập ngày hợp lệ",
            dateISO: "Vui lòng nhập ngày hợp lệ (ISO)",
            number: "Vui lòng nhập số hợp lệ",
            digits: "Vui lòng chỉ nhập số",
            creditcard: "Vui lòng nhập số thẻ tín dụng hợp lệ",
            equalTo: "Vui lòng nhập giá trị giống với trường trên",
            maxlength: $.validator.format("Vui lòng nhập tối đa {0} ký tự"),
            minlength: $.validator.format("Vui lòng nhập ít nhất {0} ký tự"),
            rangelength: $.validator.format("Vui lòng nhập từ {0} đến {1} ký tự"),
            range: $.validator.format("Vui lòng nhập giá trị từ {0} đến {1}"),
            max: $.validator.format("Vui lòng nhập giá trị nhỏ hơn hoặc bằng {0}"),
            min: $.validator.format("Vui lòng nhập giá trị lớn hơn hoặc bằng {0}"),
            step: $.validator.format("Vui lòng nhập bội số của {0}")
        };

        // =====================================================
        // 2. CUSTOM VALIDATION METHODS
        // =====================================================

        // Validate số điện thoại Việt Nam (10-11 số, bắt đầu bằng 0)
        $.validator.addMethod("phoneVN", function (value, element) {
            return this.optional(element) || /^0\d{9,10}$/.test(value);
        }, "Số điện thoại không hợp lệ (VD: 0912345678)");

        // Validate username (chỉ chữ cái, số và dấu gạch dưới)
        $.validator.addMethod("usernameVN", function (value, element) {
            return this.optional(element) || /^[a-zA-Z0-9_]{3,20}$/.test(value);
        }, "Tên đăng nhập chỉ chứa chữ cái, số và dấu gạch dưới (3-20 ký tự)");

        // Validate giá tiền (không âm)
        $.validator.addMethod("priceVN", function (value, element) {
            return this.optional(element) || (parseFloat(value) >= 0 && /^\d+(\.\d{1,2})?$/.test(value));
        }, "Giá tiền phải là số dương (VD: 50000)");

        // Validate URL hình ảnh
        $.validator.addMethod("imageUrl", function (value, element) {
            return this.optional(element) || /^https?:\/\/.+\.(jpg|jpeg|png|gif|webp)$/i.test(value);
        }, "URL hình ảnh không hợp lệ (jpg, png, gif, webp)");

        // Validate mật khẩu mạnh (ít nhất 6 ký tự, có chữ và số)
        $.validator.addMethod("strongPassword", function (value, element) {
            return this.optional(element) || /^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*#?&]{6,}$/.test(value);
        }, "Mật khẩu phải có ít nhất 6 ký tự, bao gồm chữ và số");

        // Validate địa chỉ (tối thiểu 10 ký tự)
        $.validator.addMethod("addressVN", function (value, element) {
            return this.optional(element) || (value.trim().length >= 10);
        }, "Địa chỉ phải có ít nhất 10 ký tự");

        // Validate tên (chỉ chữ cái và khoảng trắng)
        $.validator.addMethod("nameVN", function (value, element) {
            return this.optional(element) || /^[a-zA-ZÀ-ỹ\s]+$/.test(value);
        }, "Tên chỉ chứa chữ cái và khoảng trắng");
    }

    // =====================================================
    // 3. REAL-TIME VALIDATION FEEDBACK
    // =====================================================
    // Auto-focus vào field đầu tiên có lỗi (modern JS)
    document.addEventListener("DOMContentLoaded", () => {
        const forms = document.querySelectorAll("form");

        forms.forEach(form => {
            form.addEventListener("submit", (e) => {
                // Nếu form có lỗi (sử dụng class .is-invalid)
                const firstError = form.querySelector(".is-invalid");
                if (firstError) {
                    // Ngăn submit tạm thời (nếu cần)
                    e.preventDefault();

                    // Cuộn mượt đến phần tử lỗi
                    window.scrollTo({
                        top: firstError.getBoundingClientRect().top + window.scrollY - 100,
                        behavior: "smooth"
                    });

                    // Đặt focus mà không gây scroll thêm
                    firstError.focus({ preventScroll: true });
                }
            });

            // Auto-focus vào field đầu tiên có lỗi
            $('form').on('submit', function () {
                var $firstError = $(this).find('.is-invalid:first');
                if ($firstError.length) {
                    $('html, body').animate({
                        scrollTop: $firstError.offset().top - 100
                    }, 500);
                    $firstError.focus();
                }
            });
        });

        // =====================================================
        // 4. PREVENT DUPLICATE SUBMISSIONS
        // =====================================================
        window.FormValidator = {
            // Ngăn submit form nhiều lần
            preventDoubleSubmit: function (formSelector) {
                $(formSelector).on('submit', function (e) {
                    var $form = $(this);
                    var $submitBtn = $form.find('button[type="submit"], input[type="submit"]');

                    // Kiểm tra xem form có đang được submit không
                    if ($form.data('submitting') === true) {
                        e.preventDefault();
                        return false;
                    }

                    // Validate form trước
                    if ($form.valid && !$form.valid()) {
                        return false;
                    }

                    // Đánh dấu form đang submit
                    $form.data('submitting', true);

                    // Disable submit button
                    $submitBtn.prop('disabled', true);

                    // Đổi text button
                    var originalText = $submitBtn.html();
                    $submitBtn.data('original-text', originalText);
                    $submitBtn.html('<span class="spinner-border spinner-border-sm me-2"></span>Đang xử lý...');

                    // Tự động enable lại sau 10 giây (phòng trường hợp lỗi)
                    setTimeout(function () {
                        FormValidator.resetSubmitButton(formSelector);
                    }, 10000);
                });
            },

            // Reset submit button về trạng thái ban đầu
            resetSubmitButton: function (formSelector) {
                var $form = $(formSelector);
                var $submitBtn = $form.find('button[type="submit"], input[type="submit"]');

                $form.data('submitting', false);
                $submitBtn.prop('disabled', false);

                var originalText = $submitBtn.data('original-text');
                if (originalText) {
                    $submitBtn.html(originalText);
                }
            },

            // Hiển thị thông báo lỗi với SweetAlert2
            showError: function (message) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi!',
                        text: message,
                        confirmButtonText: 'Đóng',
                        confirmButtonColor: '#dc3545'
                    });
                } else {
                    alert(message);
                }
            },

            // Hiển thị thông báo thành công
            showSuccess: function (message, callback) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'success',
                        title: 'Thành công!',
                        text: message,
                        timer: 2000,
                        showConfirmButton: false
                    }).then(function () {
                        if (callback) callback();
                    });
                } else {
                    alert(message);
                    if (callback) callback();
                }
            },

            // Confirm trước khi submit
            confirmSubmit: function (formSelector, message) {
                $(formSelector).on('submit', function (e) {
                    e.preventDefault();
                    var $form = $(this);

                    if (typeof Swal !== 'undefined') {
                        Swal.fire({
                            title: 'Xác nhận',
                            text: message || 'Bạn có chắc chắn muốn thực hiện?',
                            icon: 'question',
                            showCancelButton: true,
                            confirmButtonText: 'Xác nhận',
                            cancelButtonText: 'Hủy',
                            confirmButtonColor: '#0d6efd',
                            cancelButtonColor: '#6c757d'
                        }).then(function (result) {
                            if (result.isConfirmed) {
                                $form.off('submit').submit();
                            }
                        });
                    } else {
                        if (confirm(message || 'Bạn có chắc chắn?')) {
                            $form.off('submit').submit();
                        }
                    }
                });
            }
        };

        // =====================================================
        // 5. AUTO INITIALIZATION
        // =====================================================
        $(document).ready(function () {
            // Tự động áp dụng validation cho tất cả forms có data-validation="true"
            $('form[data-validation="true"]').each(function () {
                FormValidator.preventDoubleSubmit(this);
            });

            // Áp dụng cho form checkout
            if ($('#checkoutForm').length) {
                FormValidator.preventDoubleSubmit('#checkoutForm');

                // Validate số điện thoại Việt Nam
                $('#PhoneNumber').rules('add', {
                    phoneVN: true
                });

                // Validate địa chỉ
                $('#DeliveryAddress').rules('add', {
                    addressVN: true
                });
            }

            // Áp dụng cho form login
            if ($('form[asp-action="Login"]').length) {
                FormValidator.preventDoubleSubmit('form[asp-action="Login"]');
            }

            // Áp dụng cho form create/edit món ăn
            if ($('form[asp-action="Create"], form[asp-action="Edit"]').length) {
                FormValidator.preventDoubleSubmit('form[asp-action="Create"], form[asp-action="Edit"]');

                // Validate giá tiền
                $('input[name="Price"]').rules('add', {
                    priceVN: true,
                    min: 0
                });

                // Validate tên món
                $('input[name="Name"]').rules('add', {
                    minlength: 3,
                    maxlength: 100
                });
            }

            // =====================================================
            // 6. INPUT FORMATTING & MASKING
            // =====================================================

            // Format số điện thoại tự động
            $('input[type="tel"], input[name*="Phone"]').on('input', function () {
                var value = $(this).val().replace(/\D/g, '');
                if (value.length > 11) {
                    value = value.substring(0, 11);
                }
                $(this).val(value);
            });

            // Format giá tiền (thêm dấu phẩy)
            $('input[name="Price"], input[type="number"][step="0.01"]').on('blur', function () {
                var value = parseFloat($(this).val());
                if (!isNaN(value)) {
                    $(this).val(value.toFixed(0));
                }
            });

            // Trim spaces khi blur
            $('input[type="text"], input[type="email"], textarea').on('blur', function () {
                $(this).val($.trim($(this).val()));
            });

            // =====================================================
            // 7. DYNAMIC VALIDATION MESSAGES
            // =====================================================

            // Thay đổi message động dựa vào placeholder
            $('input[required], textarea[required], select[required]').each(function () {
                var $this = $(this);
                var label = $this.prev('label').text().replace('*', '').trim();

                if (label) {
                    $this.attr('data-msg-required', 'Vui lòng nhập ' + label.toLowerCase());
                }
            });

            // =====================================================
            // 8. ACCESSIBILITY IMPROVEMENTS
            // =====================================================

            // Thêm aria-label cho các field bắt buộc
            $('input[required], textarea[required], select[required]').attr('aria-required', 'true');

            // Focus vào field đầu tiên khi load trang
            var $firstInput = $('form input:not([type="hidden"]):first');
            if ($firstInput.length && !$firstInput.val()) {
                $firstInput.focus();
            }
        });

    })(jQuery);

    // =====================================================
    // 9. GLOBAL HELPER FUNCTIONS
    // =====================================================

    // Validate form trước khi submit (có thể gọi từ code khác)
    function validateForm(formSelector) {
        var $form = $(formSelector);
        if ($form.length && $form.valid) {
            return $form.valid();
        }
        return true;
    }

    // Clear tất cả validation errors
    function clearValidation(formSelector) {
        var $form = $(formSelector);
        $form.find('.is-invalid').removeClass('is-invalid');
        $form.find('.is-valid').removeClass('is-valid');
        $form.find('.invalid-feedback').remove();
        $form.find('.validation-icon').remove();
    }

    // Console log để debug
    console.log('✅ Validation.js loaded successfully!');
});