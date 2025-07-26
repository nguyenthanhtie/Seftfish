$(document).ready(function() {
    console.log('Order history page loaded');
    
    // Filter functionality for orders
    function applyFilters() {
        var selectedStatus = $('#statusFilter').val().toLowerCase();
        var selectedUser = $('#userFilter').val().toLowerCase();
        
        $('#orderHistoryTable tbody tr').each(function() {
            var status = $(this).find('td:eq(5)').text().toLowerCase();
            var email = $(this).find('td:eq(2)').text().toLowerCase();
            
            var statusMatch = selectedStatus === '' || status.indexOf(selectedStatus) > -1;
            var userMatch = selectedUser === '' || email.indexOf(selectedUser) > -1;
            
            if (statusMatch && userMatch) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    }
    
    // Apply filters when dropdown values change
    $('#statusFilter, #userFilter').change(function() {
        applyFilters();
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

    // Confirm order
    $('.btn-confirm-order').click(function(e) {
        e.stopPropagation(); // Prevent row click
        var orderId = $(this).data('order-id');
        if (confirm('Bạn có chắc chắn muốn xác nhận đơn hàng này?')) {
            confirmOrder(orderId);
        }
    });
    
    // Load filter data from database
    loadFilterData();
});

function loadOrderDetail(orderId) {
    // Show loading state
    $('#orderModalId').text(orderId);
    $('#orderCustomerName').text('Đang tải...');
    $('#orderCustomerEmail').text('Đang tải...');
    $('#orderAddress').text('Đang tải...');
    $('#orderDate').text('Đang tải...');
    $('#orderPaymentMethod').text('Đang tải...');
    $('#orderTotal').text('Đang tải...');
    
    var itemsList = $('#orderItemsList');
    itemsList.html('<tr><td colspan="5" class="text-center">Đang tải chi tiết sản phẩm...</td></tr>');
    
    $('#orderDetailModal').modal('show');
    
    // Load order details from server
    $.ajax({
        url: window.nguoidungUrls.getOrderDetail,
        type: 'GET',
        data: { id: orderId },
        success: function(data) {
            // Update modal content
            $('#orderModalId').text(data.id);
            $('#orderCustomerName').text(data.customerName);
            $('#orderCustomerEmail').text(data.customerEmail);
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

function confirmOrder(orderId) {
    // TODO: Implement AJAX call to confirm order
    // This would require a ConfirmOrder action in DonhangController
    alert('Chức năng xác nhận đơn hàng sẽ được triển khai sau');
}

function getStatusClass(status) {
    switch(status) {
        case 'Đang xử lý': return 'bg-warning text-dark';
        case 'Đã xác nhận': return 'bg-info text-white';
        case 'Đang giao': return 'bg-primary text-white';
        case 'Đã giao': return 'bg-success text-white';
        case 'Đã hủy': return 'bg-danger text-white';
        default: return 'bg-secondary text-white';
    }
}

// Load filter data from database
function loadFilterData() {
    $.ajax({
        url: window.nguoidungUrls.getFilterData,
        type: 'GET',
        success: function(data) {
            // Populate user filter
            var userFilter = $('#userFilter');
            userFilter.empty();
            userFilter.append('<option value="">Tất cả khách hàng</option>');
            
            if (data.users && data.users.length > 0) {
                data.users.forEach(function(user) {
                    userFilter.append('<option value="' + user.email + '">' + 
                        (user.hoTen || user.email) + '</option>');
                });
            }
            
            // Populate status filter
            var statusFilter = $('#statusFilter');
            statusFilter.empty();
            statusFilter.append('<option value="">Tất cả trạng thái</option>');
            
            if (data.statuses && data.statuses.length > 0) {
                data.statuses.forEach(function(status) {
                    statusFilter.append('<option value="' + status + '">' + status + '</option>');
                });
            }
        },
        error: function() {
            console.error('Không thể tải dữ liệu bộ lọc');
        }
    });
}
