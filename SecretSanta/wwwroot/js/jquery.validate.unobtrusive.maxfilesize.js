(function ($) {
    $.validator.unobtrusive.adapters.add("maxfilesize", ["maxbytes"], function (options) {
        var params = { maxbytes: options.params.maxbytes };
        options.rules["maxfilesize"] = params;
        if (!!options.message) {
            options.messages["maxfilesize"] = options.message;
        }
    });

    $.validator.addMethod("maxfilesize", function (value, element, param) {
        if (value === "") {
            // no file chosen
            return true;
        }

        var maxBytes = parseInt(param.maxbytes);

        // if the browser supports the HTML5 file api, do filesize validation
        if (!!element.files && !!element.files[0] && !!element.files[0].size) {
            var fileSize = parseInt(element.files[0].size);
            return fileSize <= maxBytes;
        }

        // if the browser doesn't support HTML5 file api, just return true
        // so we don't prevent the user from ever being able to submit the form
        return true;
    });
})(jQuery);