$(document).ready(function() {
    var currentCategoryId = null;

    // Initialize sortable
    $('#categoryTableBody').sortable({
        handle: '.sortable-handle',
        placeholder: 'sortable-placeholder',
        helper: function(e, tr) {
            var $originals = tr.children();
            var $helper = tr.clone();
            $helper.children().each(function(index) {
                $(this).width($originals.eq(index).width());
            });
            return $helper;
        },
        start: function(e, ui) {
            ui.placeholder.html('<td colspan="6"></td>');
        },
        update: function(e, ui) {
            updateCategoryOrder();
        }
    });

    // Search functionality
    $('#searchInput').on('keyup', function() {
        var value = $(this).val().toLowerCase();
        $('#categoryTable tbody tr').filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });

    // Click on row to view products
    $('.clickable-row').click(function() {
        var categoryId = $(this).data('category-id');
        var categoryName = $(this).data('category-name');
        loadProductsForCategory(categoryId, categoryName);
    });

    // View products button
    $('.view-products').click(function() {
        var categoryId = $(this).data('category-id');
        var categoryName = $(this).data('category-name');
        loadProductsForCategory(categoryId, categoryName);
    });

    // Edit category
    $('.edit-category').click(function() {
        var id = $(this).data('id');
        var name = $(this).data('name');
        var description = $(this).data('description') || '';
        
        $('#editCategoryId').val(id);
        $('#editCategoryName').val(name);
        $('#editCategoryDescription').val(description);
    });

    // Delete category
    $('.delete-category').click(function() {
        var categoryId = $(this).data('id');
        var categoryName = $(this).data('name');
        var productCount = $(this).data('product-count');
        
        var message = `Bạn có chắc chắn muốn xóa danh mục "${categoryName}"?`;
        if (productCount > 0) {
            message += `\n\nCảnh báo: Danh mục này có ${productCount} sản phẩm. Bạn cần di chuyển sản phẩm sang danh mục khác trước khi xóa.`;
        }
        
        if(confirm(message)) {
            deleteCategory(categoryId);
        }
    });

    // Add category form
    $('#addCategoryForm').submit(function(e) {
        e.preventDefault();
        
        var name = $('#categoryName').val().trim();
        var description = $('#categoryDescription').val().trim();
        
        if (!name) {
            $('#categoryName').addClass('is-invalid');
            return;
        }
        
        $('#categoryName').removeClass('is-invalid');
        
        var data = {
            TenDanhMuc: name,
            MoTa: description || null
        };
        
        $.ajax({
            url: window.danhmucUrls.create,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function(response) {
                if (response.success) {
                    $('#addCategoryModal').modal('hide');
                    alert('Thêm danh mục thành công!');
                    location.reload();
                } else {
                    alert('Có lỗi xảy ra: ' + response.message);
                }
            },
            error: function() {
                alert('Có lỗi xảy ra khi thêm danh mục');
            }
        });
    });

    // Edit category form
    $('#editCategoryForm').submit(function(e) {
        e.preventDefault();
        
        var id = $('#editCategoryId').val();
        var name = $('#editCategoryName').val().trim();
        var description = $('#editCategoryDescription').val().trim();
        
        if (!name) {
            $('#editCategoryName').addClass('is-invalid');
            return;
        }
        
        $('#editCategoryName').removeClass('is-invalid');
        
        var data = {
            Id: parseInt(id),
            TenDanhMuc: name,
            MoTa: description || null
        };
        
        $.ajax({
            url: window.danhmucUrls.update,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function(response) {
                if (response.success) {
                    $('#editCategoryModal').modal('hide');
                    alert('Cập nhật danh mục thành công!');
                    location.reload();
                } else {
                    alert('Có lỗi xảy ra: ' + response.message);
                }
            },
            error: function() {
                alert('Có lỗi xảy ra khi cập nhật danh mục');
            }
        });
    });

    // Reset forms when modals are hidden
    $('#addCategoryModal').on('hidden.bs.modal', function() {
        $('#addCategoryForm')[0].reset();
        $('#categoryName').removeClass('is-invalid');
    });

    $('#editCategoryModal').on('hidden.bs.modal', function() {
        $('#editCategoryForm')[0].reset();
        $('#editCategoryName').removeClass('is-invalid');
    });
});

function loadProductsForCategory(categoryId, categoryName) {
    $('#categoryNameInModal').text(categoryName);
    $('#productsList').html('<div class="text-center"><i class="fas fa-spinner fa-spin fa-2x text-muted"></i><p class="mt-2">Đang tải danh sách sản phẩm...</p></div>');
    $('#viewProductsModal').modal('show');
    
    $.ajax({
        url: window.danhmucUrls.getCategoryProducts,
        type: 'GET',
        data: { id: categoryId },
        success: function(products) {
            var html = '';
            if (products && products.length > 0) {
                html = '<div class="row">';
                products.forEach(function(product) {
                    html += `
                        <div class="col-md-6 col-lg-4 mb-3">
                            <div class="card">
                                <div class="card-body">
                                    <h6 class="card-title">${product.name || 'Chưa có tên'}</h6>
                                    <p class="card-text text-success fw-bold">${product.price}</p>
                                    <small class="badge ${product.status === 'Hiển thị' ? 'bg-success' : 'bg-warning text-dark'}">${product.status}</small>
                                </div>
                            </div>
                        </div>
                    `;
                });
                html += '</div>';
            } else {
                html = '<div class="text-center py-4"><i class="fas fa-box-open fa-3x text-muted mb-3"></i><h5 class="text-muted">Chưa có sản phẩm nào</h5><p class="text-muted">Danh mục này chưa có sản phẩm nào</p></div>';
            }
            $('#productsList').html(html);
        },
        error: function() {
            $('#productsList').html('<div class="text-center py-4"><i class="fas fa-exclamation-triangle fa-3x text-danger mb-3"></i><h5 class="text-danger">Không thể tải dữ liệu</h5><p class="text-muted">Vui lòng thử lại sau</p></div>');
        }
    });
}

function deleteCategory(categoryId) {
    $.ajax({
        url: window.danhmucUrls.delete,
        type: 'POST',
        data: { id: categoryId },
        success: function(response) {
            if (response.success) {
                alert('Xóa danh mục thành công!');
                location.reload();
            } else {
                alert('Có lỗi xảy ra: ' + response.message);
            }
        },
        error: function() {
            alert('Có lỗi xảy ra khi xóa danh mục');
        }
    });
}

function updateCategoryOrder() {
    var categoryIds = [];
    $('#categoryTableBody tr').each(function(index) {
        var categoryId = $(this).data('category-id');
        if (categoryId) {
            categoryIds.push(categoryId);
            // Update display order number in the table
            $(this).find('td:eq(1)').text(index + 1);
        }
    });

    if (categoryIds.length > 0) {
        $.ajax({
            url: window.danhmucUrls.updateOrder,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(categoryIds),
            success: function(response) {
                if (response.success) {
                    // Show success message briefly
                    var successMsg = $('<div class="alert alert-success alert-dismissible fade show position-fixed" style="top: 20px; right: 20px; z-index: 9999;"><i class="fas fa-check me-2"></i>Cập nhật thứ tự thành công!</div>');
                    $('body').append(successMsg);
                    setTimeout(function() {
                        successMsg.alert('close');
                    }, 2000);
                } else {
                    alert('Có lỗi xảy ra khi cập nhật thứ tự: ' + response.message);
                    location.reload(); // Reload to restore original order
                }
            },
            error: function() {
                alert('Có lỗi xảy ra khi cập nhật thứ tự');
                location.reload(); // Reload to restore original order
            }
        });
    }
}
