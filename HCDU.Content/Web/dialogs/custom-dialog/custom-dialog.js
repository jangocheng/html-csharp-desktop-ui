(function() {

    'use strict';

    var app = angular.module('HcduApp');

    app.controller('CustomDialogCtrl', [
        '$scope', '$http', function($scope, $http) {

            $scope.response = null;

            $scope.selectFolder = function () {
                $scope.testGet('rest/selectFolder');
            };

            $scope.selectNewFolder = function () {
                $scope.testGet('rest/selectNewFolder');
            };

            $scope.showCustomDialog = function () {
                $scope.testGet('rest/showCustomDialog');
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