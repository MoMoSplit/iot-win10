$(function () {
    
    $.connection.hub.logging = true;
    if (typeof ($.connection.temperatureHub) !== "undefined") {

        var hub = $.connection.temperatureHub;

        hub.client.receiveTemperature = function (data) {
            console.log(data);

            $('#temperature').html(data.Temperature + ' &deg;C');
            $('#humidity').text(data.Humidity + ' %');

            $('#updated').text(new Date(data.Time).toLocaleString());

        };

        $.connection.hub.start().done(function (obj) {
            console.log("signalr " + obj);
        });

    }
});