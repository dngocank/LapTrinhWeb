/**
 * KittyPhone Admin DataTable Utility
 * Cung cấp phân trang (10-15 dòng/trang), tìm kiếm thời gian thực và đếm tổng bản ghi chuyên nghiệp
 */
(function ($) {
    'use strict';

    function AdminDataTable(tableElement, options) {
        this.$table = $(tableElement);
        this.options = $.extend({
            pageSize: parseInt(this.$table.data('page-size') || 10, 10),
            pageSizeOptions: [10, 15, 25, 50],
            searchInput: this.$table.data('search-input') || null,
            filterSelects: this.$table.data('filter-selects') || null,
            infoContainer: this.$table.data('info-target') || null,
            paginationContainer: this.$table.data('pagination-target') || null
        }, options);

        this.currentPage = 1;
        this.totalRows = 0;
        this.filteredRows = [];
        this.$allRows = this.$table.find('tbody tr:not(.no-data-row)');

        this.init();
    }

    AdminDataTable.prototype.init = function () {
        var self = this;
        var tableId = this.$table.attr('id') || 'dt_' + Math.random().toString(36).substring(2, 9);
        this.$table.attr('id', tableId);

        // Tạo khung điều khiển trên (Controls bar) nếu chưa có
        var $wrapper = this.$table.closest('.admin-table-wrapper');
        if ($wrapper.length === 0) {
            $wrapper = this.$table.parent();
        }

        // Tạo thanh phân trang và thông tin bên dưới bảng
        if (!this.options.paginationContainer) {
            var footerHtml = 
                '<div class="admin-table-footer d-flex justify-content-between align-items-center flex-wrap gap-2 p-3 bg-white border-top">' +
                    '<div class="d-flex align-items-center gap-2">' +
                        '<span class="text-muted small">Hiển thị</span>' +
                        '<select class="form-select form-select-sm dt-page-size-select" style="width: 75px;">' +
                            self.options.pageSizeOptions.map(function(s) {
                                return '<option value="' + s + '" ' + (s === self.options.pageSize ? 'selected' : '') + '>' + s + '</option>';
                            }).join('') +
                        '</select>' +
                        '<span class="text-muted small dt-info-text ms-2"></span>' +
                    '</div>' +
                    '<nav aria-label="Table pagination">' +
                        '<ul class="pagination pagination-sm mb-0 dt-pagination-list"></ul>' +
                    '</nav>' +
                '</div>';
            
            var $footer = $(footerHtml);
            $wrapper.append($footer);
            this.$footer = $footer;
            this.$infoText = $footer.find('.dt-info-text');
            this.$paginationList = $footer.find('.dt-pagination-list');
            this.$pageSizeSelect = $footer.find('.dt-page-size-select');
        } else {
            this.$paginationList = $(this.options.paginationContainer);
            this.$infoText = $(this.options.infoContainer);
        }

        // Sự kiện đổi số dòng hiển thị mỗi trang
        if (this.$pageSizeSelect) {
            this.$pageSizeSelect.on('change', function () {
                self.options.pageSize = parseInt($(this).val(), 10);
                self.currentPage = 1;
                self.render();
            });
        }

        // Sự kiện tìm kiếm ô search ngoài
        if (this.options.searchInput) {
            $(this.options.searchInput).on('input', function () {
                self.currentPage = 1;
                self.filterAndRender();
            });
        }

        // Sự kiện các dropdown lọc phụ (nếu có)
        if (this.options.filterSelects) {
            $(this.options.filterSelects).on('change', function () {
                self.currentPage = 1;
                self.filterAndRender();
            });
        }

        this.filterAndRender();
    };

    AdminDataTable.prototype.filterAndRender = function () {
        var self = this;
        var searchVal = '';
        if (this.options.searchInput) {
            searchVal = $(this.options.searchInput).val().trim().toLowerCase();
        }

        // Bộ lọc phụ
        var customFilters = [];
        if (this.options.filterSelects) {
            $(this.options.filterSelects).each(function () {
                var $sel = $(this);
                var val = $sel.val();
                var colIndex = $sel.data('filter-col');
                if (val && val !== 'all' && val !== '') {
                    customFilters.push({ colIndex: colIndex, val: val.toLowerCase() });
                }
            });
        }

        this.filteredRows = [];

        this.$allRows.each(function () {
            var $row = $(this);
            var text = $row.text().toLowerCase();
            var matchesSearch = !searchVal || text.indexOf(searchVal) !== -1;
            var matchesCustom = true;

            for (var i = 0; i < customFilters.length; i++) {
                var f = customFilters[i];
                var cellText = '';
                if (typeof f.colIndex !== 'undefined') {
                    cellText = $row.find('td').eq(f.colIndex).text().toLowerCase();
                } else {
                    cellText = text;
                }
                if (cellText.indexOf(f.val) === -1) {
                    matchesCustom = false;
                    break;
                }
            }

            if (matchesSearch && matchesCustom) {
                self.filteredRows.push($row);
            }
        });

        this.totalRows = this.filteredRows.length;
        this.render();
    };

    AdminDataTable.prototype.render = function () {
        var self = this;
        var totalPages = Math.ceil(this.totalRows / this.options.pageSize) || 1;

        if (this.currentPage > totalPages) {
            this.currentPage = totalPages;
        }
        if (this.currentPage < 1) {
            this.currentPage = 1;
        }

        var startIdx = (this.currentPage - 1) * this.options.pageSize;
        var endIdx = startIdx + this.options.pageSize;

        // Ẩn toàn bộ dòng trước
        this.$allRows.hide();

        // Hiển thị các dòng trong trang hiện tại
        for (var i = startIdx; i < endIdx && i < this.totalRows; i++) {
            this.filteredRows[i].show();
        }

        // Xử lý khi không có bản ghi nào
        var $noDataRow = this.$table.find('tbody tr.dt-empty-row');
        if (this.totalRows === 0) {
            if ($noDataRow.length === 0) {
                var colCount = this.$table.find('thead th').length || 8;
                var emptyHtml = 
                    '<tr class="dt-empty-row">' +
                        '<td colspan="' + colCount + '" class="text-center py-5 text-muted">' +
                            '<i class="bi bi-inbox display-6 d-block mb-2 text-secondary opacity-50"></i>' +
                            'Không tìm thấy bản ghi nào phù hợp với bộ lọc.' +
                        '</td>' +
                    '</tr>';
                this.$table.find('tbody').append(emptyHtml);
            } else {
                $noDataRow.show();
            }
        } else {
            $noDataRow.hide();
        }

        // Cập nhật thông tin bản ghi
        if (this.$infoText && this.$infoText.length) {
            if (this.totalRows === 0) {
                this.$infoText.text('(0 bản ghi)');
            } else {
                var fromNum = startIdx + 1;
                var toNum = Math.min(endIdx, this.totalRows);
                this.$infoText.html('Hiển thị <strong>' + fromNum + ' - ' + toNum + '</strong> trên <strong>' + this.totalRows + '</strong> bản ghi');
            }
        }

        // Vẽ các nút phân trang
        this.renderPagination(totalPages);
    };

    AdminDataTable.prototype.renderPagination = function (totalPages) {
        if (!this.$paginationList || !this.$paginationList.length) return;

        var self = this;
        this.$paginationList.empty();

        if (totalPages <= 1) {
            return;
        }

        // Nút Trước (Prev)
        var isFirst = this.currentPage === 1;
        var $prevLi = $('<li class="page-item ' + (isFirst ? 'disabled' : '') + '"><a class="page-link" href="javascript:void(0)" aria-label="Trước"><i class="bi bi-chevron-left"></i></a></li>');
        $prevLi.on('click', function (e) {
            e.preventDefault();
            if (!isFirst) {
                self.currentPage--;
                self.render();
            }
        });
        this.$paginationList.append($prevLi);

        // Các số trang
        var startPage = Math.max(1, this.currentPage - 2);
        var endPage = Math.min(totalPages, this.currentPage + 2);

        if (startPage > 1) {
            this.addPageButton(1);
            if (startPage > 2) {
                this.$paginationList.append('<li class="page-item disabled"><span class="page-link">...</span></li>');
            }
        }

        for (var p = startPage; p <= endPage; p++) {
            this.addPageButton(p);
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                this.$paginationList.append('<li class="page-item disabled"><span class="page-link">...</span></li>');
            }
            this.addPageButton(totalPages);
        }

        // Nút Tiếp (Next)
        var isLast = this.currentPage === totalPages;
        var $nextLi = $('<li class="page-item ' + (isLast ? 'disabled' : '') + '"><a class="page-link" href="javascript:void(0)" aria-label="Tiếp"><i class="bi bi-chevron-right"></i></a></li>');
        $nextLi.on('click', function (e) {
            e.preventDefault();
            if (!isLast) {
                self.currentPage++;
                self.render();
            }
        });
        this.$paginationList.append($nextLi);
    };

    AdminDataTable.prototype.addPageButton = function (pageNum) {
        var self = this;
        var isActive = pageNum === this.currentPage;
        var $li = $('<li class="page-item ' + (isActive ? 'active' : '') + '"><a class="page-link" href="javascript:void(0)">' + pageNum + '</a></li>');
        $li.on('click', function (e) {
            e.preventDefault();
            if (!isActive) {
                self.currentPage = pageNum;
                self.render();
            }
        });
        this.$paginationList.append($li);
    };

    // Tự động kích hoạt khi có class .admin-datatable
    $(document).ready(function () {
        $('.admin-datatable').each(function () {
            new AdminDataTable(this);
        });
    });

    window.AdminDataTable = AdminDataTable;

})(jQuery);
