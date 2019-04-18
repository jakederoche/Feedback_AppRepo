function GetUserData() {
    var webMethod = "AppServices.asmx/GetUser";

    $.ajax({
        type: "POST",
        url: webMethod,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            
            // Store the msg into a user variable
            user = msg.d

            $("#user_title_fname").html(user.firstName);
            $("#user_title").html(user.firstName + " " + user.lastName);
            $("#user_fname").val(user.firstName);
            $("#user_lname").val(user.lastName);
            $("#user_username").val(user.userName);
            $("#user_dept").val(user.department);


            if (user.admin == "True") {
                $(".admin_item").css('display', 'block');
            }
        },
        error: function (e) {
            alert("Error with loading User Data");
        }
    });

}