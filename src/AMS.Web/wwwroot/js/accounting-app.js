// accounting-app.js
// Khởi tạo tất cả plugins và global behaviors

$(function () {

  // ============================================================
  // 1. TOASTR CONFIG
  // ============================================================
  toastr.options = {
    closeButton: true,
    progressBar: true,
    positionClass: 'toast-bottom-right',
    timeOut: 4000,
    extendedTimeOut: 2000,
    preventDuplicates: true
  };

  window.ACC = window.ACC || {};

  ACC.toast = {
    success: (msg) => toastr.success(msg),
    error:   (msg) => toastr.error(msg, null, { timeOut: 0 }),
    warning: (msg) => toastr.warning(msg),
    info:    (msg) => toastr.info(msg)
  };

  // ============================================================
  // 2. FLATPICKR — Date picker
  // ============================================================
  flatpickr('.date-picker', {
    dateFormat: 'd/m/Y',
    allowInput: true,
    locale: 'vn'
  });

  flatpickr('.date-range-picker', {
    mode: 'range',
    dateFormat: 'd/m/Y'
  });

  // ============================================================
  // 3. SELECT2
  // ============================================================
  $('.select2-basic').select2({
    theme: 'bootstrap-5',
    width: '100%'
  });

  $('.select2-accounts').select2({
    theme: 'bootstrap-5',
    width: '100%',
    minimumInputLength: 1,
    placeholder: 'Tìm tài khoản...',
    ajax: {
      url: '/ChartOfAccounts/GetActive',
      dataType: 'json',
      delay: 250,
      data: (params) => ({ q: params.term }),
      processResults: (data) => ({
        results: data.map(acc => ({
          id: acc.id,
          text: `${acc.code} - ${acc.name}`
        }))
      })
    }
  });

  $('.select2-customers').select2({
    theme: 'bootstrap-5',
    width: '100%',
    minimumInputLength: 1,
    placeholder: 'Tìm khách hàng...',
    ajax: {
      url: '/Customers/GetActive',
      dataType: 'json',
      delay: 250,
      data: (params) => ({ term: params.term }),
      processResults: (data) => ({
        results: data.map(c => ({ id: c.id, text: c.name }))
      })
    }
  });

  $('.select2-vendors').select2({
    theme: 'bootstrap-5',
    width: '100%',
    minimumInputLength: 1,
    placeholder: 'Tìm nhà cung cấp...',
    ajax: {
      url: '/Vendors/GetActive',
      dataType: 'json',
      delay: 250,
      data: (params) => ({ term: params.term }),
      processResults: (data) => ({
        results: data.map(v => ({ id: v.id, text: v.name }))
      })
    }
  });

  $('.select2-products').select2({
    theme: 'bootstrap-5',
    width: '100%',
    minimumInputLength: 1,
    placeholder: 'Tìm hàng hóa...',
    ajax: {
      url: '/Products/GetActive',
      dataType: 'json',
      delay: 250,
      data: (params) => ({ term: params.term }),
      processResults: (data) => ({
        results: data.map(p => ({ id: p.id, text: `${p.productCode} - ${p.productName}` }))
      })
    }
  });

  // ============================================================
  // 4. KEYBOARD SHORTCUTS
  // ============================================================
  $(document).on('keydown', function (e) {
    const ctrl = e.ctrlKey || e.metaKey;

    if (ctrl && e.key === 'k') {
      e.preventDefault();
      $('#globalSearch').focus().select();
    }

    if (ctrl && e.key === 's') {
      const $activeForm = $('form.acc-form:visible').first();
      if ($activeForm.length) {
        e.preventDefault();
        $activeForm.find('[type=submit]').trigger('click');
      }
    }
  });

  // ============================================================
  // 5. AJAX GLOBAL SETUP
  // ============================================================
  $.ajaxSetup({
    headers: {
      'X-Requested-With': 'XMLHttpRequest',
      'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
    }
  });

  $(document).ajaxError(function (event, xhr) {
    if (xhr.status === 401) {
      window.location.href = '/Account/Login';
    } else if (xhr.status === 403) {
      ACC.toast.error('Bạn không có quyền thực hiện thao tác này.');
    } else if (xhr.status === 0) {
      ACC.toast.error('Mất kết nối. Vui lòng kiểm tra internet.');
    } else if (xhr.status >= 500) {
      ACC.toast.error('Lỗi hệ thống. Vui lòng thử lại sau.');
    }
  });

  // ============================================================
  // 6. CONFIRMATION DIALOGS
  // ============================================================
  $(document).on('click', '[data-confirm]', function (e) {
    e.preventDefault();
    const $btn = $(this);
    const message = $btn.data('confirm');
    const url     = $btn.data('url');
    const method  = $btn.data('method') || 'POST';
    const label   = $btn.data('confirm-label') || 'Xác nhận';

    $('#confirmModal .modal-body p').text(message);
    $('#confirmModal .btn-confirm-ok')
      .text(label)
      .off('click')
      .on('click', function () {
        $.ajax({ url, method, success: () => {
          bootstrap.Modal.getInstance('#confirmModal').hide();
          const redirect = $btn.data('redirect');
          if (redirect) window.location.href = redirect;
          else location.reload();
        }});
      });

    new bootstrap.Modal('#confirmModal').show();
  });

  // ============================================================
  // 7. FORM SUBMIT WITH AJAX
  // ============================================================
  $(document).on('submit', '.acc-ajax-form', function (e) {
    e.preventDefault();
    const $form = $(this);
    const $btn  = $form.find('[type=submit]');

    if (!$form.valid()) return;

    $btn.prop('disabled', true)
        .html('<span class="spinner-border spinner-border-sm me-2"></span>Đang lưu...');

    $.ajax({
      url: $form.attr('action'),
      method: $form.attr('method') || 'POST',
      data: $form.serialize(),
      success: function (res) {
        ACC.toast.success($form.data('success-msg') || 'Lưu thành công');
        const redirect = $form.data('redirect') || res.redirect;
        if (redirect) setTimeout(() => window.location.href = redirect, 800);
      },
      error: function (xhr) {
        const err = xhr.responseJSON;
        if (err && err.errors) {
          $.each(err.errors, function (field, messages) {
            const $field = $form.find(`[name="${field}"]`);
            $field.addClass('is-invalid');
            $field.siblings('.invalid-feedback').text(messages[0]);
          });
        }
      },
      complete: function () {
        $btn.prop('disabled', false).html($btn.data('original-text') || 'Lưu');
      }
    });
  });

});

// ============================================================
// 8. DATATABLES HELPER
// ============================================================
ACC.initDataTable = function (selector, options) {
  const defaults = {
    processing: true,
    serverSide: true,
    language: {
      processing: '<div class="spinner-border spinner-border-sm text-primary"></div>',
      search: '',
      searchPlaceholder: 'Tìm kiếm...',
      lengthMenu: 'Hiển thị _MENU_ dòng',
      info: 'Hiển thị _START_–_END_ của _TOTAL_ bản ghi',
      paginate: {
        previous: '<i class="bi bi-chevron-left"></i>',
        next:     '<i class="bi bi-chevron-right"></i>'
      }
    },
    pageLength: 50,
    dom: '<"d-none"f>rt<"d-flex align-items-center justify-content-between p-3"ip>',
    columnDefs: [
      { className: 'col-amount', targets: '.col-amount' }
    ]
  };

  return $(selector).DataTable($.extend(true, defaults, options));
};

// ============================================================
// 9. CURRENCY FORMATTING HELPERS
// ============================================================
ACC.format = {
  currency: function (amount, currency = 'VND') {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency', currency,
      minimumFractionDigits: 0
    }).format(amount);
  },
  currencyUSD: function (amount) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency', currency: 'USD',
      minimumFractionDigits: 2
    }).format(amount);
  },
  negative: function (amount, currency = 'VND') {
    if (amount < 0) {
      return `(${ACC.format.currency(Math.abs(amount), currency)})`;
    }
    return ACC.format.currency(amount, currency);
  },
  date: function (dateStr) {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('vi-VN', {
      day: '2-digit', month: '2-digit', year: 'numeric'
    });
  },
  percent: function (val, decimals = 1) {
    return `${val >= 0 ? '+' : ''}${val.toFixed(decimals)}%`;
  }
};

// ============================================================
// 10. VOUCHER LINE HELPERS (for voucher form)
// ============================================================
ACC.voucher = {
  addLine: function() {
    const index = $('.journal-line').length;
    const html = `
      <tr class="journal-line" data-index="${index}">
        <td>
          <select class="form-select form-select-sm select2-accounts line-account" 
                  name="Lines[${index}].AccountId" required>
          </select>
        </td>
        <td>
          <input type="text" class="form-control form-control-sm" 
                 name="Lines[${index}].Description" placeholder="Mô tả">
        </td>
        <td>
          <input type="text" class="form-control form-control-sm text-end input-money line-debit" 
                 name="Lines[${index}].Debit" value="0">
        </td>
        <td>
          <input type="text" class="form-control form-control-sm text-end input-money line-credit" 
                 name="Lines[${index}].Credit" value="0">
        </td>
        <td>
          <button type="button" class="btn btn-sm btn-link text-danger" onclick="ACC.voucher.removeLine(this)">
            <i class="bi bi-x-lg"></i>
          </button>
        </td>
      </tr>
    `;
    $('#linesBody').append(html);
    $('.line-account').last().select2({ theme: 'bootstrap-5', width: '100%' });
    this.calcTotals();
  },
  
  removeLine: function(btn) {
    $(btn).closest('tr').remove();
    this.reindexLines();
    this.calcTotals();
  },
  
  reindexLines: function() {
    $('.journal-line').each(function(i) {
      $(this).attr('data-index', i);
      $(this).find('select, input').each(function() {
        this.name = this.name.replace(/\[\d+\]/, `[${i}]`);
      });
    });
  },
  
  calcTotals: function() {
    let totalDebit = 0, totalCredit = 0;
    $('.journal-line').each(function() {
      const debit = parseFloat($(this).find('.line-debit').val().replace(/,/g, '')) || 0;
      const credit = parseFloat($(this).find('.line-credit').val().replace(/,/g, '')) || 0;
      totalDebit += debit;
      totalCredit += credit;
    });
    $('#totalDebit').text(ACC.format.currency(totalDebit));
    $('#totalCredit').text(ACC.format.currency(totalCredit));
    
    const diff = totalDebit - totalCredit;
    $('#differenceAmount').text(ACC.format.currency(Math.abs(diff)));
    
    if (diff === 0) {
      $('#balanceAlert').removeClass('alert-danger alert-warning').addClass('alert-success').html('<i class="bi bi-check-circle me-2"></i>Cân bằng').show();
      $('#btnPost').prop('disabled', false);
    } else {
      $('#balanceAlert').removeClass('alert-success alert-warning').addClass('alert-danger').html('<i class="bi bi-exclamation-circle me-2"></i>Chênh lệch: ' + ACC.format.currency(diff)).show();
      $('#btnPost').prop('disabled', true);
    }
  }
};

// Initialize voucher line events
$(function() {
  $(document).on('input', '.line-debit, .line-credit', function() {
    ACC.voucher.calcTotals();
  });
  
  $(document).on('change', '.line-account', function() {
    const row = $(this).closest('tr');
    if ($(this).val()) {
      row.find('.line-debit, .line-credit').first().focus();
    }
  });
});
