$(document).ready(function() {
    // Khởi tạo biểu đồ
    initRevenueChart();
    initOrderStatusChart();
    loadPerformanceReport();
    
    // Xử lý sự kiện
    bindEvents();
});

let revenueChart;
let orderStatusChart;

function initRevenueChart() {
    const ctx = document.getElementById('revenueChart').getContext('2d');
    
    // Tạo nhãn tháng (12 tháng gần nhất)
    const labels = [];
    const now = new Date();
    for (let i = 11; i >= 0; i--) {
        const date = new Date(now.getFullYear(), now.getMonth() - i, 1);
        labels.push(`${date.getMonth() + 1}/${date.getFullYear()}`);
    }
    
    revenueChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Doanh thu (₫)',
                data: doanhThuTheoThang,
                borderColor: 'rgb(75, 192, 192)',
                backgroundColor: 'rgba(75, 192, 192, 0.1)',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointBackgroundColor: 'rgb(75, 192, 192)',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top'
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return 'Doanh thu: ' + formatCurrency(context.parsed.y);
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return formatCurrency(value);
                        }
                    }
                }
            },
            elements: {
                point: {
                    hoverRadius: 8
                }
            }
        }
    });
}

function initOrderStatusChart() {
    const ctx = document.getElementById('orderStatusChart').getContext('2d');
    
    const labels = donHangTheoTrangThai.map(item => item.trangThai);
    const data = donHangTheoTrangThai.map(item => item.soLuong);
    
    const colors = [
        '#FF6384', // Đang xử lý
        '#36A2EB', // Đã xác nhận
        '#FFCE56', // Đang giao
        '#4BC0C0', // Hoàn thành
        '#FF9F40', // Đã hủy
        '#9966FF'  // Khác
    ];
    
    orderStatusChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors.slice(0, data.length),
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = ((context.parsed * 100) / total).toFixed(1);
                            return `${context.label}: ${context.parsed} (${percentage}%)`;
                        }
                    }
                }
            }
        }
    });
}

function loadPerformanceReport() {
    $.ajax({
        url: window.thongkeUrls.getBaoCaoHieuSuat,
        type: 'GET',
        success: function(data) {
            renderPerformanceReport(data);
        },
        error: function() {
            $('#performanceReport').html(`
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    Không thể tải báo cáo hiệu suất
                </div>
            `);
        }
    });
}

function renderPerformanceReport(data) {
    const trendIcon = data.phanTramThayDoi >= 0 ? 'fa-arrow-up text-success' : 'fa-arrow-down text-danger';
    const trendColor = data.phanTramThayDoi >= 0 ? 'text-success' : 'text-danger';
    const trendText = data.phanTramThayDoi >= 0 ? 'Tăng' : 'Giảm';
    
    const html = `
        <div class="row text-center">
            <div class="col-6 mb-3">
                <div class="border rounded p-3">
                    <h6 class="text-muted mb-1">Doanh thu tháng này</h6>
                    <h4 class="text-primary mb-0">${formatCurrency(data.doanhThuThangNay)}</h4>
                </div>
            </div>
            <div class="col-6 mb-3">
                <div class="border rounded p-3">
                    <h6 class="text-muted mb-1">So với tháng trước</h6>
                    <h4 class="${trendColor} mb-0">
                        <i class="fas ${trendIcon} me-1"></i>
                        ${Math.abs(data.phanTramThayDoi)}%
                    </h4>
                </div>
            </div>
            <div class="col-6 mb-3">
                <div class="border rounded p-3">
                    <h6 class="text-muted mb-1">Đơn hàng tháng này</h6>
                    <h4 class="text-info mb-0">${data.donHangThangNay.toLocaleString()}</h4>
                </div>
            </div>
            <div class="col-6 mb-3">
                <div class="border rounded p-3">
                    <h6 class="text-muted mb-1">Khách hàng mới</h6>
                    <h4 class="text-success mb-0">${data.khachHangMoiThangNay.toLocaleString()}</h4>
                </div>
            </div>
        </div>
        
        <div class="alert alert-info">
            <i class="fas fa-info-circle me-2"></i>
            <strong>${trendText} ${Math.abs(data.phanTramThayDoi)}%</strong> so với tháng trước
            (${formatCurrency(data.doanhThuThangTruoc)})
        </div>
    `;
    
    $('#performanceReport').html(html);
}

function bindEvents() {
    // Xử lý nút thời gian
    $('#btnThang, #btnTuan, #btnNgay').click(function() {
        $('.btn-group .btn').removeClass('active');
        $(this).addClass('active');
        
        // TODO: Implement different time period views
        showNotification('info', 'Tính năng đang được phát triển');
    });
    
    // Xem báo cáo theo khoảng thời gian
    $('#btnXemBaoCao').click(function() {
        const fromDate = $('#fromDate').val();
        const toDate = $('#toDate').val();
        
        if (!fromDate || !toDate) {
            showNotification('warning', 'Vui lòng chọn khoảng thời gian');
            return;
        }
        
        if (new Date(fromDate) > new Date(toDate)) {
            showNotification('error', 'Ngày bắt đầu không được lớn hơn ngày kết thúc');
            return;
        }
        
        loadCustomReport(fromDate, toDate);
    });
    
    // Xuất Excel
    $('#btnXuatExcel').click(function() {
        const fromDate = $('#fromDate').val();
        const toDate = $('#toDate').val();
        
        if (!fromDate || !toDate) {
            showNotification('warning', 'Vui lòng chọn khoảng thời gian để xuất báo cáo');
            return;
        }
        
        // TODO: Implement Excel export
        showNotification('info', 'Tính năng xuất Excel đang được phát triển');
    });
}

function loadCustomReport(fromDate, toDate) {
    const btn = $('#btnXemBaoCao');
    const originalText = btn.html();
    btn.html('<i class="fas fa-spinner fa-spin me-1"></i>Đang tải...').prop('disabled', true);
    
    $.ajax({
        url: window.thongkeUrls.getDoanhThuTheoNgay,
        type: 'GET',
        data: { tuNgay: fromDate, denNgay: toDate },
        success: function(data) {
            updateRevenueChart(data);
            showNotification('success', 'Đã cập nhật báo cáo theo khoảng thời gian đã chọn');
        },
        error: function() {
            showNotification('error', 'Không thể tải báo cáo. Vui lòng thử lại');
        },
        complete: function() {
            btn.html(originalText).prop('disabled', false);
        }
    });
}

function updateRevenueChart(data) {
    const labels = data.map(item => item.ngay);
    const revenues = data.map(item => item.doanhThu);
    
    revenueChart.data.labels = labels;
    revenueChart.data.datasets[0].data = revenues;
    revenueChart.update();
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

function showNotification(type, message) {
    // Sử dụng notification function từ layout chính
    if (typeof window.showNotification === 'function') {
        window.showNotification(type, message);
    } else {
        alert(message);
    }
}
