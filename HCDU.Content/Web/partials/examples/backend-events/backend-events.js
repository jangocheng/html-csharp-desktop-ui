(function() {

    'use strict';

    var app = angular.module('HcduApp');

    app.controller('BackendEventsExampleCtrl', [
        '$scope', '$http', function($scope, $http) {

            $scope.backendCounter = 0;

            $scope.response = null;

            $scope.testIncrement = function() {
                $http.get('rest/backend-events/increment');
            };

            $scope.testIncrement5Sec = function() {
                $http.get('rest/backend-events/increment5Sec');
            };

            var webSocket = null;

            $scope.onDestroy = function() {
                webSocket.close();
            };

            $scope.init = function() {
                var url = $scope.buildWebSocketUrl("/ws/backend-events");
                webSocket = new WebSocket(url);

                //webSocket.onopen = function (evt) { webSocket.send('Test'); };
                //webSocket.onclose = function (evt) { };
                webSocket.onmessage = function (evt) {
                    $scope.$apply(function() {
                        $scope.backendCounter = JSON.parse(evt.data);
                    });
                };
                //webSocket.onerror = function (err) { };

                //todo: read initial state
            };

            $scope.buildWebSocketUrl = function(path) {
                var location = window.location;
                return "ws://" + location.host + path;
            };

            $scope.$on('$destroy', function() {
                $scope.onDestroy();
            });

            $scope.init();
        }
    ]);
})();