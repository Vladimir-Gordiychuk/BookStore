var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#companyTable').DataTable({
        "ajax": {
            "url": "/Admin/Company/GetAll",
        },
        "columns": [
            {
                "data": "name",
                "width": "15%"
            },
            {
                "data": "streetAddress",
                "width": "15%"
            },
            {
                "data": "city",
                "width": "15%"
            },
            {
                "data": "state",
                "width": "10%"
            },
            {
                "data": "postalCode",
                "width": "10%"
            },
            {
                "data": "phoneNumber",
                "width": "10%"
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <div class="w-75 btn-group" role="group">
                        <a href="/Admin/Company/Edit/${data}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square"></i>
                            Edit
                        </a>
                        <a onClick=Delete('/Admin/Company/Delete/${data}') class="btn btn-danger mx-2">
                            <i class="bi bi-trash"></i>
                            Delete
                        </a>
                    </div>
                    `;
                },
                "width" : "15%"
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (response) {
                    if (response.success) {
                        dataTable.ajax.reload();
                        toastr.success(response.message);
                        //Swal.fire(
                        //    'Deleted!',
                        //    'Your product has been deleted.',
                        //    'success'
                        //)
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            });

        }
    })
}