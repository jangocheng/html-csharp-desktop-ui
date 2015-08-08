(function() {

    'use strict';

    var app = angular.module('HcduApp');

    app.controller('RestExampleCtrl', [
        '$scope', '$http', function($scope, $http) {

            $scope.response = null;

            $scope.testJson = function() {
                $scope.testGet('rest/cars/boxter');
            };

            $scope.testException = function() {
                $scope.testGet('rest/exception');
            };

            $scope.testWebsocket = function() {
                var url = $scope.buildWebSocketUrl("/ws/test");
                var ws = new WebSocket(url);
                ws.onopen = function(evt) { ws.send("Test"); };
                //todo: implement
            };

            $scope.buildWebSocketUrl = function(path) {
                var location = window.location;
                return "ws://" + location.host + path;
            };

            $scope.testGet = function(url) {
                $http.get(url).then(
                    function(response) {
                        $scope.response = response;
                    },
                    function(response) {
                        $scope.response = response;
                    }
                );
            };
        }
    ]);
})();