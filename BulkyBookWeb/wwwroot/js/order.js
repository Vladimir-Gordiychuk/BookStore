var dataTable;

$(document).ready(function () {
    const url = window.location.search;
    const status = new URLSearchParams(url).get('status');
    loadDataTable(status);
});

function yyyymmdd(date) {
    var mm = date.getMonth() + 1; // getMonth() is zero-based
    var dd = date.getDate();

    return [date.getFullYear(),
        (mm > 9 ? '' : '0') + mm,
        (dd > 9 ? '' : '0') + dd
        ].join('.');
}

function loadDataTable(status) {
    dataTable = $('#orderTable').DataTable({
        "ajax": {
            "url": "/Admin/Order/GetAll?status=" + status,
        },
        "columns": [
            {
                "data": "id",
                "width": "5%"
            },
            {
                "data": "name",
                "width": "15%"
            },
            {
                "data": "phoneNumber",
                "width": "10%"
            },
            {
                "data": "applicationUser.email",
                "width": "15%"
            },
            {
                "data": "orderStatus",
                "width": "10%"
            },
            {
                "data": "orderDate",
                "width": "7%",
                "render": (date) => {
                    return yyyymmdd(new Date(date));
                }
            },
            {
                "data": "orderTotal",
                "width": "7%"
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <div class="w-75 btn-group" role="group">
                        <a href="/Admin/Order/Details/${data}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square"></i>
                        </a>
                    </div>
                    `;
                },
                "width" : "5%"
            }
        ]
    });
}