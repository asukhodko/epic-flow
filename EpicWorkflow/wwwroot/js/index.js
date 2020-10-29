var form = document.getElementById('filterForm'),
    productSelect = document.getElementById('productSelect'),
    sortHidden = document.getElementById('sortHidden'),
    vipfirst = document.getElementById('vipfirst'),
    vipfirst_hidden = document.getElementById('vipfirst_hidden'),

    onSubmit = function (e) {
        e.preventDefault();
        if (vipfirst.checked) {
            vipfirst_hidden.disabled = true;
        }
        form.submit();
    },

    onFilterChange = function () {
        var event = new Event('submit');
        form.dispatchEvent(event);
    },

    onSortButtonClick = function (e) {
        sortHidden.value = e.target.value;
    },

    init = (function () {
        form.addEventListener('submit', onSubmit);
        productSelect.addEventListener('change', onFilterChange);
        vipfirst.addEventListener('change', onFilterChange);
        $('button.sort-button').each(function (i, el) {
            el.addEventListener('click', onSortButtonClick)
        });
    }());