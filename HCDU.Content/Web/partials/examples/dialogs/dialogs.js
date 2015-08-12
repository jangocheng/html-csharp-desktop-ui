(function() {

    'use strict';

    var app = angular.module('HcduApp');

    app.controller('DialogsExampleCtrl', [
        '$scope', '$http', function($scope, $http) {

            $scope.response = null;

            $scope.selectFolder = function () {
                $scope.performGet('rest/selectFolder');
            };

            $scope.selectNewFolder = function () {
                $scope.performGet('rest/selectNewFolder');
            };

            $scope.showCustomDialog = function () {
                $scope.performGet('rest/showCustomDialog');
            };

            $scope.performGet = function (url) {
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