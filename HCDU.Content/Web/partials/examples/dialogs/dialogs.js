(function() {

    'use strict';

    var app = angular.module('HcduApp');

    app.controller('DialogsExampleCtrl', [
        '$scope', '$http', function($scope, $http) {

            $scope.response = null;

            $scope.testJson = function() {
                $scope.testGet('rest/selectFolder');
            };

            $scope.testException = function() {
                $scope.testGet('rest/selectNewFolder');
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