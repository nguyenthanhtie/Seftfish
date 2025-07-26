$(document).ready(function() {
    // Simple table without DataTables to avoid column count issues
    console.log('Page loaded, binding events...');
    
    // Bind events after DOM is ready
    bindCustomerEvents();
    
    // Simple search functionality
    $('#searchInput').on('keyup', function() {
        var value = $(this).val().toLowerCase();
        $('#customerTable tbody tr').filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });
});

function bindCustomerEvents() {
    // View customer detail - clicking anywhere on the row
    $('.customer-row').on('click', function() {
        var customerId = $(this).data('customer-id');
        loadCustomerDetail(customerId);
    });

    // Add hover effect for better UX
    $('.customer-row').hover(
        function() {
            $(this).addClass('table-active');
        },
        function() {
            $(this).removeClass('table-active');
        }
    );

    // Toggle customer status
    $('.btn-toggle-status').on('click', function() {
        var customerId = $(this).data('customer-id');
        var button = $(this);
        
        if (confirm('Bạn có chắc chắn muốn thay đổi trạng thái của khách hàng này?')) {
                    $.ajax({
                        url: window.nguoidungUrls.toggleStatus,
                        type: 'POST',
                        data: { id: customerId },
                        success: function(response) {
                            if (response.success) {
                                alert('Đã thay đổi trạng thái thành công!');
                                location.reload();
                            } else {
                                alert('Có lỗi xảy ra: ' + response.message);
                            }
                        },
                        error: function() {
                            alert('Có lỗi xảy ra khi thay đổi trạng thái');
                        }
                    });
        }
    });

    // Handle toggle status in modal
    $(document).on('click', '#toggleStatusBtn', function() {
        var customerId = $('#customerModalId').text();
        
        if (confirm('Bạn có chắc chắn muốn thay đổi trạng thái của khách hàng này?')) {
            $.ajax({
                url: window.nguoidungUrls.toggleStatus,
                type: 'POST',
                data: { id: customerId },
                success: function(response) {
                    if (response.success) {
                        // Update modal status immediately
                        updateModalStatus(response.newStatus);
                        // Also update the status in the table
                        location.reload();
                    } else {
                        alert('Có lỗi xảy ra: ' + response.message);
                    }
                },
                error: function() {
                    alert('Có lỗi xảy ra khi thay đổi trạng thái');
                }
            });
        }
    });

    // Handle add customer form
    $('#addCustomerForm').on('submit', function(e) {
        e.preventDefault();
        
        // Reset previous validation
        $(this).find('.is-invalid').removeClass('is-invalid');
        
        // Validate required fields
        var isValid = true;
        var email = $('#newCustomerEmail').val();
        var password = $('#newCustomerPassword').val();
        var role = $('#newCustomerRole').val();
        
        if (!email || !isValidEmail(email)) {
            $('#newCustomerEmail').addClass('is-invalid');
            isValid = false;
        }
        
        if (!password || password.length < 6) {
            $('#newCustomerPassword').addClass('is-invalid');
            isValid = false;
        }
        
        if (!role) {
            $('#newCustomerRole').addClass('is-invalid');
            isValid = false;
        }
        
        if (!isValid) {
            return;
        }
        
        var formData = {
            Email: email,
            MatKhau: password,
            HoTen: $('#newCustomerName').val(),
            GioiTinh: $('#newCustomerGender').val(),
            NgaySinh: $('#newCustomerBirthDate').val(),
            TrangThai: $('#newCustomerStatus').val() === 'true',
            SoDienThoai: $('#newCustomerPhone').val(),
            DiaChiChiTiet: $('#newCustomerAddress').val(),
            VaiTro: $('#newCustomerRole').val()
        };
        
        // Disable submit button
        var submitBtn = $(this).find('button[type="submit"]');
        var originalText = submitBtn.html();
        submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i> Đang xử lý...');
        
        $.ajax({
            url: window.nguoidungUrls.create,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    alert('Thêm khách hàng thành công!');
                    $('#addCustomerModal').modal('hide');
                    location.reload();
                } else {
                    alert('Có lỗi xảy ra: ' + response.message);
                }
            },
            error: function(xhr) {
                var error = 'Có lỗi xảy ra khi thêm khách hàng';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    error = xhr.responseJSON.message;
                }
                alert(error);
            },
            complete: function() {
                submitBtn.prop('disabled', false).html(originalText);
            }
        });
    });

    // Reset form when modal is hidden
    $('#addCustomerModal').on('hidden.bs.modal', function() {
        $('#addCustomerForm')[0].reset();
        $('#addCustomerForm .is-invalid').removeClass('is-invalid');
    });
}

function loadCustomerDetail(customerId) {
    $.ajax({
        url: window.nguoidungUrls.getCustomerDetail,
        type: 'GET',
        data: { id: customerId },
        success: function(data) {
            // Update modal content
            $('#customerModalId').text(data.id);
            $('#customerModalName').text(data.hoTen || 'Chưa cập nhật');
            $('#customerModalEmail').val(data.email);
            $('#customerModalJoinDate').val(data.ngayTao);
            $('#customerModalAddress').val(data.diaChi);
            $('#customerModalOrderCount').val(data.tongDonHang + ' đơn hàng');
            $('#customerModalTotalSpent').val(data.tongChiTieu);
            
            // Update image
            if (data.anhDaiDien) {
                $('#customerModalImage').attr('src', data.anhDaiDien).show();
                $('#customerModalImagePlaceholder').hide();
            } else {
                $('#customerModalImage').hide();
                $('#customerModalImagePlaceholder').show();
            }
            
            // Update status
            updateModalStatus(data.trangThai);
            
            // Show modal
            $('#customerDetailModal').modal('show');
        },
        error: function() {
            alert('Không thể tải thông tin khách hàng');
        }
    });
}

function updateModalStatus(status) {
    var statusBadge = $('#customerModalStatus');
    var toggleBtn = $('#toggleStatusBtn');
    var toggleIcon = $('#toggleStatusIcon');
    var toggleText = $('#toggleStatusText');
    
    if (status === 'Hoạt động') {
        statusBadge.removeClass('bg-warning text-dark').addClass('active-status').text('Hoạt động');
        toggleBtn.removeClass('btn-outline-success').addClass('btn-outline-warning');
        toggleIcon.removeClass('fas fa-unlock').addClass('fas fa-lock');
        toggleText.text('Khóa tài khoản');
    } else {
        statusBadge.removeClass('active-status').addClass('bg-warning text-dark').text('Tạm khóa');
        toggleBtn.removeClass('btn-outline-warning').addClass('btn-outline-success');
        toggleIcon.removeClass('fas fa-lock').addClass('fas fa-unlock');
        toggleText.text('Mở khóa tài khoản');
    }
}

function isValidEmail(email) {
    // Simple email validation to avoid Razor parsing issues
    if (!email || email.length < 5 || email.length > 100) return false;
    var parts = email.split('');
    var hasAt = false;
    var hasDot = false;
    for (var i = 0; i < parts.length; i++) {
        if (parts[i] === '@') hasAt = true;
        if (parts[i] === '.') hasDot = true;
    }
    return hasAt && hasDot;
}
