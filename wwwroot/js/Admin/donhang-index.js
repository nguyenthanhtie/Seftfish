$(document).ready(function() {
    // Search functionality
    $('#searchInput').on('keyup', function() {
        var value = $(this).val().toLowerCase();
        $('#orderTable tbody tr').filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });

    // Status filter
    $('#statusFilter').change(function() {
        var selectedStatus = $(this).val().toLowerCase();
        $('#orderTable tbody tr').each(function() {
            var status = $(this).find('td:eq(4)').text().toLowerCase();
            if (selectedStatus === '' || status.indexOf(selectedStatus) > -1) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });

    // Click on row to view order detail
    $('.order-row').click(function() {
        var orderId = $(this).data('order-id');
        loadOrderDetail(orderId);
    });

    // Add hover effect for better UX
    $('.order-row').hover(
        function() {
            $(this).addClass('table-active');
        },
        function() {
            $(this).removeClass('table-active');
        }
    );

    // Click on status badge to cycle through statuses
    $('.status-badge').click(function(e) {
        e.stopPropagation(); // Prevent row click
        var orderId = $(this).data('order-id');
        var currentStatus = $(this).text().trim();
        var nextStatus = getNextStatus(currentStatus);
        
        if (nextStatus) {
            updateOrderStatus(orderId, nextStatus);
        }
    });
});

function loadOrderDetail(orderId) {
    // Show loading state
    $('#orderModalId').text(orderId);
    $('#orderCustomerName').text('Đang tải...');
    $('#orderCustomerEmail').text('Đang tải...');
    $('#orderCustomerPhone').text('Đang tải...');
    $('#orderAddress').text('Đang tải...');
    $('#orderDate').text('Đang tải...');
    $('#orderPaymentMethod').text('Đang tải...');
    $('#orderTotal').text('Đang tải...');
    
    var itemsList = $('#orderItemsList');
    itemsList.html('<tr><td colspan="5" class="text-center">Đang tải chi tiết sản phẩm...</td></tr>');
    
    $('#orderDetailModal').modal('show');
    
    // Load order details from server
    $.ajax({
        url: window.donhangUrls.getOrderDetail,
        type: 'GET',
        data: { id: orderId },
        success: function(data) {
            // Update modal content
            $('#orderModalId').text(data.id);
            $('#orderCustomerName').text(data.customerName);
            $('#orderCustomerEmail').text(data.customerEmail);
            $('#orderCustomerPhone').text(data.customerPhone);
            $('#orderAddress').text(data.address);
            $('#orderDate').text(data.orderDate);
            $('#orderPaymentMethod').text(data.paymentMethod);
            $('#orderTotal').text(data.totalAmount);
            
            // Update status badge
            var statusBadge = $('#orderStatus');
            statusBadge.removeClass().addClass('badge ' + getStatusClass(data.status));
            statusBadge.text(data.status);
            
            // Update items list
            var itemsHtml = '';
            if (data.items && data.items.length > 0) {
                data.items.forEach(function(item) {
                    itemsHtml += '<tr>' +
                        '<td>' + item.productName + '</td>' +
                        '<td>' + item.variantSku + '</td>' +
                        '<td>' + item.quantity + '</td>' +
                        '<td>' + item.price + '</td>' +
                        '<td>' + item.subTotal + '</td>' +
                        '</tr>';
                });
            } else {
                itemsHtml = '<tr><td colspan="5" class="text-center">Không có sản phẩm nào</td></tr>';
            }
            itemsList.html(itemsHtml);
        },
        error: function() {
            alert('Không thể tải thông tin đơn hàng');
        }
    });
}

function getNextStatus(currentStatus) {
    var statuses = [
        'Đang xử lý',
        'Đã xác nhận', 
        'Đang giao',
        'Hoàn thành',
        'Đã hủy'
    ];
    
    var currentIndex = statuses.indexOf(currentStatus);
    if (currentIndex === -1) return 'Đang xử lý'; // Default if not found
    
    // Cycle to next status, or back to first if at the end
    var nextIndex = (currentIndex + 1) % statuses.length;
    return statuses[nextIndex];
}

function updateOrderStatus(orderId, status) {
    $.ajax({
        url: window.donhangUrls.updateStatus,
        type: 'POST',
        data: { id: orderId, status: status },
        success: function(response) {
            if (response.success) {
                // Update the status badge immediately without page reload
                var badge = $('.status-badge[data-order-id="' + orderId + '"]');
                badge.removeClass().addClass('badge status-badge ' + getStatusClass(status));
                badge.text(status);
                badge.attr('title', 'Click để thay đổi trạng thái');
                badge.css('cursor', 'pointer');
                
                // Show success message briefly
                var originalText = badge.text();
                badge.text('✓ Đã cập nhật');
                setTimeout(function() {
                    badge.text(originalText);
                }, 1000);
            } else {
                alert('Có lỗi xảy ra: ' + response.message);
            }
        },
        error: function() {
            alert('Có lỗi xảy ra khi cập nhật trạng thái');
        }
    });
}

function getStatusClass(status) {
    switch(status) {
        case 'Đang xử lý': return 'bg-warning text-dark';
        case 'Đã xác nhận': return 'bg-info text-white';
        case 'Đang giao': return 'bg-primary text-white';
        case 'Hoàn thành': return 'bg-success text-white';
        case 'Đã hủy': return 'bg-danger text-white';
        default: return 'bg-secondary text-white';
    }
}
