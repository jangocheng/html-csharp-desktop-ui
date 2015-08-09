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