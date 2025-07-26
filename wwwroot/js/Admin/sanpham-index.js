// Quản lý sản phẩm - JavaScript
$(document).ready(function() {
    // Xử lý click vào dòng sản phẩm hoặc nút xem chi tiết
    $(document).on('click', '.clickable-row, .view-product', function (e) {
        // Nếu là nút view-product thì ngăn sự kiện nổi bọt lên tr
        if ($(e.target).closest('.view-product').length > 0) {
            e.stopPropagation();
        }
        var id = $(this).data('id') || $(this).closest('tr').data('id');
        if (!id) return;
        $('#productDetailModal').modal('show');
        $('#productDetailContent').html('<div class="text-center py-5"><div class="spinner-border text-primary" role="status"></div></div>');
        
        var detailUrl = window.productUrls.detail;
        $.get(detailUrl, { id: id }, function (data) {
            $('#productDetailContent').html(data);
        }).fail(function () {
            $('#productDetailContent').html('<div class="alert alert-danger">Không thể tải chi tiết sản phẩm.</div>');
        });
    });

    // Mở modal Thêm sản phẩm
    $('#addProductBtn').on('click', function (e) {
        e.preventDefault();
        $('#addEditProductModalLabel').text('Thêm sản phẩm mới');
        $('#addEditProductModal').modal('show');
        loadProductForm();
    });

    // Hàm tải form sản phẩm
    function loadProductForm(id = null) {
        $('#addEditProductContent').html('<div class="text-center py-5"><div class="spinner-border text-primary" role="status"></div></div>');
        var url = window.productUrls.addOrEdit;
        if (id) url += '?id=' + id;
        
        $.get(url, function (data) {
            $('#addEditProductContent').html(data);
        }).fail(function () {
            $('#addEditProductContent').html('<div class="alert alert-danger">Không thể tải form.</div>');
        });
    }

    // Submit form Thêm/Chỉnh sửa sản phẩm qua AJAX - chỉ gắn khi cần thiết
    $(document).off('submit', '#addEditProductForm').on('submit', '#addEditProductForm', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();
        
        var $form = $(this);
        var $submitBtn = $form.find('button[type="submit"]');
        
        // Kiểm tra nếu đang trong quá trình submit
        if ($submitBtn.prop('disabled') || $form.data('submitting')) {
            return false;
        }
        
        // Đánh dấu form đang submit
        $form.data('submitting', true);
        
        var formData = new FormData(this);
        
        // Thêm loading indicator
        var originalBtnText = $submitBtn.html();
        $submitBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Đang xử lý...');
        
        $.ajax({
            url: $form.attr('action'),
            type: $form.attr('method'),
            data: formData,
            processData: false,
            contentType: false,
            timeout: 30000,
            success: function (res) {
                if (res.success) {
                    // Thông báo thành công
                    alert('Lưu sản phẩm thành công!');
                    $('#addEditProductModal').modal('hide');
                    location.reload();
                } else {
                    // Hiển thị thông báo lỗi
                    if (res.message) {
                        // Hiển thị thông báo lỗi từ server
                        alert('Lỗi: ' + res.message);
                    } else if (res.html) {
                        // Cập nhật form với dữ liệu mới nếu có
                        $('#addEditProductContent').html(res.html);
                    } else {
                        alert('Có lỗi xảy ra khi lưu sản phẩm!');
                    }
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                // Xử lý lỗi AJAX
                var errorMessage = 'Có lỗi xảy ra khi gửi form!';
                if (textStatus === 'timeout') {
                    errorMessage = 'Yêu cầu quá thời gian chờ. Vui lòng thử lại.';
                } else if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                
                alert('Lỗi: ' + errorMessage);
                console.error('AJAX Error:', xhr);
            },
            complete: function() {
                // Reset trạng thái form
                $form.data('submitting', false);
                $submitBtn.prop('disabled', false).html(originalBtnText);
            }
        });
        
        return false;
    });
    
    // Xử lý nút chỉnh sửa trong modal chi tiết
    $(document).on('click', '.edit-product-detail', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        $('#productDetailModal').modal('hide');
        
        // Mở modal chỉnh sửa
        setTimeout(() => {
            $('#addEditProductModalLabel').text('Chỉnh sửa sản phẩm');
            $('#addEditProductModal').modal('show');
            loadProductForm(id);
        }, 300);
    });

    // Xử lý nút xóa trong modal chi tiết
    $(document).on('click', '.delete-product-detail', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var $btn = $(this);
        
        // Lấy tên sản phẩm để hiển thị trong thông báo
        var productName = $('#productDetailModal .modal-body h4').first().text() || 'sản phẩm này';
        
        // Xác nhận xóa với thông báo rõ ràng
        var confirmMessage = `Bạn có chắc chắn muốn XÓA VĨNH VIỄN sản phẩm "${productName}"?\n\n` +
            `⚠️ CẢNH BÁO:\n` +
            `• Sản phẩm sẽ bị xóa hoàn toàn khỏi hệ thống\n` +
            `• Tất cả ảnh và thông tin liên quan sẽ bị xóa\n` +
            `• Thao tác này KHÔNG THỂ HOÀN TÁC!\n\n` +
            `Nhấn OK để xác nhận xóa vĩnh viễn.`;
        
        if (confirm(confirmMessage)) {
            $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Đang xóa...');
            
            $.ajax({
                url: window.productUrls.delete,
                type: 'POST',
                data: {
                    id: id,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                timeout: 30000,
                success: function (response) {
                    if (response.success) {
                        alert('✅ Xóa sản phẩm thành công!');
                        $('#productDetailModal').modal('hide');
                        setTimeout(() => location.reload(), 500);
                    } else {
                        alert('❌ Lỗi: ' + (response.message || 'Không thể xóa sản phẩm'));
                        $btn.prop('disabled', false).html('<i class="fas fa-trash me-1"></i> Xóa vĩnh viễn');
                    }
                },
                error: function (xhr, textStatus) {
                    var errorMsg = 'Lỗi kết nối máy chủ';
                    if (textStatus === 'timeout') {
                        errorMsg = 'Yêu cầu quá thời gian chờ. Vui lòng thử lại.';
                    } else if (xhr.responseJSON?.message) {
                        errorMsg = xhr.responseJSON.message;
                    }
                    alert('❌ Lỗi: ' + errorMsg);
                    $btn.prop('disabled', false).html('<i class="fas fa-trash me-1"></i> Xóa vĩnh viễn');
                }
            });
        }
    });
    
    $('#refreshPageBtn').on('click', function() {
        location.reload();
    });
});
