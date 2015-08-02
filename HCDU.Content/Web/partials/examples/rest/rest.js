(function() {

    'use strict';

    var app = angular.module('HcduApp');

    app.controller('RestExampleCtrl', [
        '$scope', '$http', function($scope, $http) {

            $scope.requestResult = null;

            $scope.testJson = function() {
                $http.get('rest/cars/boxter').
                    success(function(data) {
                        $scope.requestResult = data;
                    });
            };

            $scope.testException = function() {
                $http.get('rest/exception').
                    success(function(data) {
                        $scope.requestResult = data;
                    });
            };
        }
    ]);
})();