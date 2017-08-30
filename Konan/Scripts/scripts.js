
$(document).ready(function () {
    $("div[style='opacity: 0.9; z-index: 2147483647; position: fixed; left: 0px; bottom: 0px; height: 65px; right: 0px; display: block; width: 100%; background-color: #202020; margin: 0px; padding: 0px;']").remove();
    $("div[style='margin: 0px; padding: 0px; left: 0px; width: 100%; height: 65px; right: 0px; bottom: 0px; display: block; position: fixed; z-index: 2147483647; opacity: 0.9; background-color: rgb(32, 32, 32);']").remove();
    $("div[onmouseover='S_ssac();']").remove();
    $("center").remove();
});


$(document).ready(function () {
    $('a').tooltip({ delay: { show: 500, hide: 10 } });

    $(".f-description").keyup(function () {
        if ($(this).val() == "" && $("#file-name").text() == "") {
            $('#createPost').attr("disabled", true).addClass('disabled');
        }
        else {
            $('#createPost').removeAttr('disabled').removeClass('disabled');
        }
    });

    $("#file-name").bind("DOMSubtreeModified", function () {
        if ($(".f-description").val() == "" && $("#file-name").text() == "") {
            $('#createPost').attr("disabled", true).addClass('disabled');
        }
        else {
            $('#createPost').removeAttr('disabled').removeClass('disabled');
        }
    });

    //$('.searchResults').bind("DOMSubtreeModified", function () {
    //    $(".result").hide().show("slow");
    //});

    $('#q').keyup(function () {
        $('#searchForm').submit();
    });

    $('#button1').click(function () {
        $.get("/Home/Index", function (html) {
            $("body").html(html);
        })
    });

    if ($('.validation-summary-errors').length) {
        toggler('.editProfile')
    }

    reloadNotifications()

});

function checkProblemData()
{
    if($('#p-title').val() == "" || $('#p-description').val() == "" || $('#p-output').val() == "")
        $('#addProblem').attr("disabled", true).addClass('disabled');
    else
        $('#addProblem').removeAttr('disabled').removeClass('disabled');
}

$("#p-title").keyup(function () {
    checkProblemData()
});

$("#p-description").keyup(function () {
    checkProblemData()
});

$("#p-input").keyup(function () {
    checkProblemData()
});

$("#p-output").keyup(function () {
    checkProblemData()
});


$(document).on('click', '.dropdown-menu', function (e) {
    e.stopPropagation();
});

function toggler(divId) {
    $(divId).slideToggle();
}

function addEffect() {
    $(".pHeader").siblings(".posts").find(".post:first").hide().show("slow")
}

$("textarea").keyup(function (e) {
    while ($(this).outerHeight() < this.scrollHeight + parseFloat($(this).css("borderTopWidth")) + parseFloat($(this).css("borderBottomWidth"))) {
        $(this).height($(this).height() + 1);
    };
});

function deletePost(idP) {
    $.ajax({
        url: "/Post/Delete/",
        type: "POST",
        data: { id: idP },
        async: true,
        success: function (data) {
            $('#' + idP).hide('slow', function () { $(this).remove(); });
        },
        error: function (xhr) {
            //alert("error - delete post");
        }
    });
}

function likePost(idP) {
    $.ajax({
        url: "/Post/Like/",
        type: "POST",
        data: { id: idP },
        async: true,
        success: function (data) {
            el = $('#' + idP + " .glyphicon-thumbs-up");
            if ($(el).hasClass("text-orange")) {
                $(el).removeClass("text-orange")
            }
            else {
                $(el).addClass("text-orange")
            }
        },
        error: function (xhr) {
            //alert("error - like post");
        }
    });
}

function removeNotification(idN) {
    $.ajax({
        url: "/Home/DeleteNotification/",
        type: "POST",
        data: { id: idN },
        async: true,
        success: function (data) {
            reloadNotifications(1)

        },
        error: function (xhr) {
            //alert("error - remove notification");
        }
    });
}

function sharePost(idP, isOnProfile) {
    $.ajax({
        url: "/Post/Share/",
        type: "POST",
        data: { id: idP },
        async: true,
        success: function (data) {
            window.location.replace("/Home/Index/");
        },
        error: function (xhr) {
            //alert("error - share post");
        }
    });
}

function reloadComments(el, id) {
    var idP = $(el).attr("id")
    if (id != null)
        idP = id
    $.ajax({
        url: '/Post/Comments/',
        data: { id: idP, },
        type: 'POST',
        success: function (result) {
            $(el).prevAll("div.post-comm:first").html(result);
            $(el).find("#comment").val("")
        },
        error: function (xhr) {
            //alert("error - load comments");
        }
    });
}

function reloadNotifications(del) {
    $.ajax({
        url: '/Home/Notifications/',
        type: 'POST',
        success: function (result) {
            $(".notifications").html(result);
            if (del == 1) {
                $('.dropdown').find('[data-toggle=dropdown]').dropdown('toggle');
            }
        },
        error: function (xhr) {
            //alert("error - load comments");
        }
    });
}

function reloadSearchResults() {
    var search = $("#q").val()
    $.ajax({
        url: '/Home/SearchResults/',
        data: { q: search },
        type: 'POST',
        success: function (result) {
            $(".searchResults").html(result);
        },
        error: function (xhr) {
            //alert("error - load comments");
        }
    });
}


function reloadPosts() {
    $.ajax({
        url: '/Post/Posts/',
        type: 'POST',
        success: function (result) {
            $(".posts").html(result)
            addEffect()
        },
        error: function (xhr) {
           // alert("error - load comments");
        }
    });
}

$(function () {
    $(document).on('change', ':file', function () {
        var input = $(this),
            numFiles = input.get(0).files ? input.get(0).files.length : 1,
            label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
        input.trigger('fileselect', [numFiles, label]);
    });

    $(document).ready(function () {
        $(':file').on('fileselect', function (event, numFiles, label) {
            $("#file-name").text($('input[type=file]')[0].files[0].name)
        });
    });

});

$(document).on("click", ".delete", function () {
    var postId = $(this).data('id');

    $(".confirmDelete").attr("onclick", "deletePost('" + postId + "')")
});


$(document).on("click", ".confirmDelete", function () {
    $("#deleteModal").modal("hide")
});

$('#addProblem').unbind("click").click(function () {
    var formdata = {
            "Title": $("#p-title").val(),
            "Description": $("#p-description").val(),
            "Input": $("#p-input").val(),
            "Output": $("#p-output").val()
};
    

    $.ajax({
        url: "/Post/CreatePost/",
        type: "POST",
        data: { model: formdata },
        async: true,
        success: function (data) {
            $(".create-article").slideToggle("", function () {
                $("#p-title").val("");
                $("#p-description").val()
                $("#p-input").val("")
                $("#p-output").val("")
                reloadPosts();
            });
        },
        error: function (xhr) {
           // alert("error - add problem");
        }
    });
});

function redirect(url)
{
    window.location.replace(url);
}

$('#createPost').unbind("click").click(function () {
    $body = $("body");
    $body.addClass("loading");

    var formdata = new FormData();
    var fileInput = document.getElementById('file-upload');
    var title = document.getElementById('Title').value;
    var description = document.getElementById('Description').value;

    formdata.append("File", fileInput.files[0]);
    formdata.append("Title", title);
    formdata.append("Description", description);

    var visEl = $('#vis').attr("class");
    var visibility

    if(visEl.indexOf("globe") >= 0)
        visibility = "public"
    else if (visEl.indexOf("user") >= 0)
        visibility = "friends"
    else if (visEl.indexOf("lock") >= 0)
        visibility = "private"
    formdata.append("Visibility", visibility);
    var xhr = new XMLHttpRequest();
    xhr.open('POST', '/Post/CreatePost/');
    xhr.send(formdata);
    xhr.onreadystatechange = function () {
        if (xhr.readyState == 4 && xhr.status == 200) {
            $(".create-article").slideToggle("", function () {
                $("#file-upload").val("");
                $("#file-name").empty()
                $("#Title").val("")
                $("#Description").val("")
                reloadPosts();
            });
        }
    }

    $body.removeClass("loading");
    
});

function deleteComment(idC, el) {
    $.ajax({
        url: "/Post/DeleteComment/",
        type: "POST",
        data: { id: idC },
        async: true,
        success: function (data) {
            var idP = $(el).attr("data-id")
            $.ajax({
                url: '/Post/Comments/',
                data: { id: idP, },
                type: 'POST',
                success: function (result) {
                    $(".post-comm").html(result);
                },
                error: function (xhr) {
                    //alert("error - load comments");
                }
            });
        },
        error: function (xhr) {
            //alert("error - like post");
        }
    });
}

//signalR
//update online users
$(function () {
    var counterHub = $.connection.chatHub;

    counterHub.client.UpdateCount = function (count) {
        //console.log("CHAT HUB - counter is working");
        $(".counter").text(count)
    }

    counterHub.client.UpdateChat = function (object) {
       $('#online').html('')
        $.each(object, function () {
           // $('#o3nline').append("<a style='cursor: pointer;' onclick='openMsg(\"" + this.Id + "\", \"" + this.FirstName + " " + this.LastName + "\")'> <div class=\"header-avatar article-avatar\" style=\"background-image: url('"+this.ImageUrl+"')\"></div>&emsp;" + this.FirstName + " " + this.LastName + "</a>")
            $('#online').append("<div class=\"form-inline custom-form\"><div class=\"form-group\"><div class=\"header-avatar article-avatar\" style=\"background-image: url('" + this.ImageUrl + "')\"></div></div><div class=\"form-group\"><a onclick='openMsg(\"" + this.Id + "\", \"" + this.ImageUrl + "\", \"" + this.FirstName + " " + this.LastName + "\")'>" + this.FirstName + " " + this.LastName + "</a></div></div>")

        })
    }
    $.connection.hub.start().done(function () {
        //console.log("CHAT HUB - started");
    });
})

//notifications 
$(function () {
    var notificationHub = $.connection.notificationHub;

    notificationHub.client.UpdateNotification = function () {
        //console.log("NOTIFICATION HUB - notification reload");
        reloadNotifications(1)

    }

    notificationHub.client.ReloadMsg = function (id) {
        //console.log("NOTIFICATION HUB - msg reloaded");
        reloadMsg(id)

    }

    $.connection.hub.start().done(function () {
        //console.log("NOTIFICATION HUB - started");
    });


})

function closeMsgW(i) {

    if (i == 1) {
        $('#msgW').show();
        $('#message').focus()
    }
    else {
        $('#msgW').hide();
    }

}

function reloadMsg(idA, f) {

    if (f == 1)
        idA = $('#idUser').val()

    $.ajax({
        url: '/Account/Messages/',
        data: { id: idA, },
        type: 'POST',
        success: function (result) {
            $('.conversation-body').html(result)
            $('.conversation-body').scrollTop($('.conversation-body').prop("scrollHeight"))
        },
        error: function (xhr) {
           // alert("error - load messages");
        }
    });
    $('#message').val('')

}

function openMsg(id, img, name) {
    $('#msgW .display-name').html(name);
    $('#idUser').val(id)
    $('#avatar').html('<div class="header-avatar article-avatar" style="background-image: url('+img+')"></div>')
    reloadMsg(id);
    closeMsgW(1)
};

function openChat() {
    if ($('#chat').hasClass('hidden'))
        $('#chat').removeClass('hidden')
    else
        $('#chat').addClass('hidden')

    $('#chat #msgW').hide();

}

function showPosts() {
    $.ajax({
        url: '/Admin/Posts/',
        type: 'POST',
        success: function (result) {
            $('.admin-content').html(result)
        },
        error: function (xhr) {
            //alert("error - load posts ADMIN");
        }
    });
}

function showAccounts() {
    $.ajax({
        url: '/Admin/Accounts/',
        type: 'POST',
        success: function (result) {
            $('.admin-content').html(result)
        },
        error: function (xhr) {
           // alert("error - load accounts ADMIN");
        }
    });
}

function showSolutions(id){
    $.ajax({
        url: '/Problems/GetSolutions/',
        data: {id: id},
        type: 'POST',
        success: function (result) {
            $('#solutionsModal .modal-body').html(result)
        },
        error: function (xhr) {
            //alert("error - load problems");
        }
    });
}

function showProblems() {
    $.ajax({
        url: '/Problems/GetProblems/',
        type: 'POST',
        success: function (result) {
            $('.articles').html(' ')
            $('.problems').html(result)
        },
        error: function (xhr) {
           // alert("error - load problems" );
        }
    });
}

function showOthers() {
    $.ajax({
        url: '/Admin/Others/',
        type: 'POST',
        success: function (result) {
            $('.admin-content').html(result)
        },
        error: function (xhr) {
           // alert("error - load others ADMIN");
        }
    });
}

function changeVisibility(el, no) {
    if (no != null) {
        var classT = $(el).children().first().attr("class")
        classT += " vis"
        $('#' + no + ' .vis').attr('class', classT)
        var visibility

        if (classT.indexOf("globe") >= 0)
            visibility = "public"
        else if (classT.indexOf("user") >= 0)
            visibility = "friends"
        else if (classT.indexOf("lock") >= 0)
            visibility = "private"
        $.ajax({
            url: '/Post/ChangeVisibility/',
            data: { id: no, vis: visibility},
            type: 'POST',
            success: function (result) {
                if($('#' + no + ' .drop').hasClass("open"))
                    $('#' + no + ' .drop').removeClass("open")
                else
                    $('#' + no + ' .drop').addClass("open")
            },
            error: function (xhr) {
               // alert("error - change visibility");
            }
        });
    }
    else {
        var classT = $(el).children().first().attr("class")
        $('#vis').attr('class', classT)
    }
    
}

function sendSolution(id) {
    var c = editor.getValue().replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/(?:\r\n|\r|\n)/g, '\n');
    var i = $('#input').val()
    var o = $('#output').val()


    var postData = {
        Code: c,
        Input: i,
        Id_P: id,
        Language: lang,
        Output: o

    }

    $.ajax({
        url: '/Problems/Solve/',
        data: {
            model: postData
        },
        type: 'POST',
        success: function (result) {
            redirect('/Home/Index')
        },
        error: function (result) {
        }

    });

}

$('#compile').unbind('click').click(function () {

    $body = $("body");
    $body.addClass("loading");
    var c = editor.getValue().replace(/</g, '&lt;').replace(/>/g, '&gt;');
    var i = $('#input').val()
    $(".errors").empty()
    if($('#send').hasClass('hidden') == false)
        $('#send').addClass('hidden')
    $.ajax({
        url: '/Problems/Compiler/',
        data: {
            code: c,
            language: lang,
            input: i
        },
        type: 'POST',
        success: function (result) {
            if (result == false) {
                $body.removeClass("loading");
            }
            else{
                //for(var i = 0; i < result.errors.length(); i++){
                //    $('.errors').append("<div class=\"validation-summary-errors form-alert bg-danger\"><ul><li>"+result.errors[0]+"</li> </ul></div>")
                // }

                if (result.cmpinfo != "" || result.stderr != "")
                {
                    $('#output').addClass('hidden')
                    $('.errors').append("<div class=\"validation-summary-errors cst form-alert bg-danger\"><ul><li>" + result.cmpinfo + result.stderr + "</li> </ul></div>")
                }
                else
                {
                    $('#output').val(result.output)
                    $('#output').removeClass('hidden')
                    if ($('#send').hasClass('hidden'))
                        $('#send').removeClass('hidden')
                }

            

                
                
            }
            $body.removeClass("loading");
        },
        error: function (result) {
           // alert("error - no output")
            $body.removeClass("loading");
        }
        
    });

   
})

$('.edit').click(function () {
    var id = $(this).attr("data-id")
    if(!$('#' + id + " .postContent").hasClass('hidden')){
        $('#' + id + " .postContent").addClass('hidden')
        $('#' + id + " .editPost").removeClass('hidden')
    }
    
})

$('.savePost').click(function () {
    var id = $(this).attr("data-id")
    if ($('#' + id + " .postContent").hasClass('hidden')) {
        $('#' + id + " .postContent").removeClass('hidden')
        $('#' + id + " .editPost").addClass('hidden')

        var title = $('#' + id + " .editPost #item_Title").val()
        $('#' + id + " .postContent .post-title").text(title)

        var desc = $('#' + id + " .editPost #item_Description").val()
        $('#' + id + " .postContent .post-description").text(desc)


       

        $.ajax({
            url: '/Post/Edit/',
            data: { id: id, title: title, description: desc },
            type: 'POST',
            success: function (result) {
            },
            error: function (result) {
            }
        });
    }

})

var lang = "CSHARP";

function changeLanguage(lan)
{
    lang = lan;

    if(lan == "CPP")
    {
        editor.getDoc().setValue("#include <iostream>\n\nusing namespace std;\n\nint main()\n{\n    cout<<\"Hello World!\";\n    return 0;\n}")
        $('.lang .btn').html("Language (C++) <span class=\"caret\"></span>")
        editor.setOption("mode", "text/x-c++src");
    }

    if(lan == "CSHARP")
    {
        editor.getDoc().setValue("using System;\n\npublic class HelloWorld\n{\n    public static void Main()\n    {\n        Console.WriteLine(\"Hello World!\");\n    }\n}\n      ");
        $('.lang .btn').html("Language (C#) <span class=\"caret\"></span>")
        editor.setOption("mode", "text/x-csharp");
    }

    if(lan == "C")
    {
        editor.getDoc().setValue("#include<stdio.h>\n\nmain()\n{\n    printf(\"Hello World!\");\n}\n      ");
        $('.lang .btn').html("Language (C) <span class=\"caret\"></span>")
        editor.setOption("mode", "text/x-csrc");
    }

    if (lan == "CLOJURE")
    {
        editor.getDoc().setValue("(println \"Hello World!\")");
        $('.lang .btn').html("Language (Clojure) <span class=\"caret\"></span>")
        editor.setOption("mode", "text/x-clojure");
    }

    if (lan == "JAVA")
    {
        editor.getDoc().setValue("class TestClass {\n    public static void main(String args[] ) throws Exception {\n        System.out.println(\"Hello World!\");\n    }\n}\n");
        $('.lang .btn').html("Language (Java) <span class=\"caret\"></span>")
        editor.setOption("mode", "text/x-java");
    }

    if (lan == "JAVASCRIPT")
    {
        editor.getDoc().setValue("print ('Hello World!');");
        $('.lang .btn').html("Language (JavaScript) <span class=\"caret\"></span>")
        editor.setOption("mode", "text/javascript");
    }

    if (lan == "HASKELL")
    {
        editor.getDoc().setValue("module Main\n    where\nmain=putStrLn \"Hello World!\"");
        $('.lang .btn').html("Language (Haskell) <span class=\"caret\"></span>")
        editor.setOption("mode", "text/x-haskell");
    }

    if (lan == "PERL")
    {
        editor.getDoc().setValue("use strict;\n\n=comment\n\nwhile(my $fred = <STDIN>) {\n    print $fred;\n}\n=cut\n\nprint 'Hello World!'\nprint 'Hello World!'");
        $('.lang .btn').html("Language (Perl) <span class=\"caret\"></span>")
        editor.setOption("mode", "text/x-perl");
    }

    if (lan == "PHP")
    {
        editor.getDoc().setValue("<?php\n\necho \"Hello World!\";\n\n?>");
        $('.lang .btn').html("Language (PHP) <span class=\"caret\"></span>")
        editor.setOption("mode", "text/x-php");
    }

    if (lan == "PYTHON")
    {
        editor.getDoc().setValue("print \"Hello World\"");
        $('.lang .btn').html("Language (Python) <span class=\"caret\"></span>")
        editor.setOption("mode", "text/x-python");
    }

    if (lan == "RUBY")
    {
        editor.getDoc().setValue("print \"Hello World!\"");
        $('.lang .btn').html("Language (Ruby) <span class=\"caret\"></span>")
        editor.setOption("mode", "text/x-ruby");
    }

    $('.lang .dropdown').removeClass("open")
}


$("#msgW").draggable({
    handle: ".conversation-header"
});