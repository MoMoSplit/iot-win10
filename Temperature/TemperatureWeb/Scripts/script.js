$(function () {

    moment.locale('hr-HR');

    $.connection.hub.logging = true;
    if (typeof ($.connection.temperatureHub) !== "undefined") {

        var hub = $.connection.temperatureHub;

        hub.client.receiveTemperature = function (data) {

            console.log(data);

            $('#temperature').html(data.Temperature.toFixed(2) + ' &deg;C');
            $('#humidity').text(data.Humidity.toFixed(2) + ' %');

            $('#updated').text(moment(new Date(data.Time).toLocaleString()).format("LLL"));

        };

        $.connection.hub.start().done(function (obj) {
            console.log("signalr " + obj);
        });

    }
});