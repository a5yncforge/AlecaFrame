

String.prototype.pad = function(size) {
     var s = String(this);
     while (s.length < (size || 2)) {s = "0" + s;}
     return s;
}

function sendMetric(keyVar,valueVar) {
    try {
        plugin.get().AddMetric(keyVar, valueVar.toString());
    } catch (e) {
        console.log("Failed to add metric: " + e);
    }
}



function doTabHeaderWork(event) {
    var element = event.currentTarget;

    $(element.parentElement).children().removeClass("selected");
    $(element).addClass("selected");

    $("#" + element.getAttribute("tabid")).parent().children().removeClass("shown");
    $("#" + element.getAttribute("tabid")).addClass("shown");
};




//Stack that handles the press of the escape key. When a modal is opened, it registers a function to be called here
escapeKeyHandlersStack = [];
$(document).keydown(function (e) {
    if (e.key === "Escape" && escapeKeyHandlersStack.length > 0) { // escape key maps to keycode `27`
        var todo = escapeKeyHandlersStack.pop();
        todo();
    }
});

setInterval(() => {
    if (escapeKeyHandlersStack.length > 10) {
        escapeKeyHandlersStack = escapeKeyHandlersStack.slice(10);
    }
}, 60000);
