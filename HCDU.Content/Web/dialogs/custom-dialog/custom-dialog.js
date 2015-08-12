(function() {

    'use strict';

    var app = angular.module('HcduApp');

    app.controller('CustomDialogCtrl', [
        '$scope', '$http', function($scope, $http) {

            $scope.closeDialog = function () {
                //todo: find better solution for relative URLs
                $http.get('../../rest/closeCustomDialog').then(
                    function(response) {
                        //todo: implement
                    },
                    function(response) {
                        //todo: implement
                    }
                );
            };
        }
    ]);
})();