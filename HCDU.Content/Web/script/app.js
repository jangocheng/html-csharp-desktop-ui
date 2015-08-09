var app = angular.module('HcduApp', ['ngMaterial']);

app.controller('AppCtrl', [
    '$scope', '$mdSidenav', function($scope, $mdSidenav) {

        $scope.toggleSidenav = function() {
            $mdSidenav('left').toggle();
        };

        $scope.sections = [
            { name: 'about', title: 'About' },
            { name: 'text', title: 'Text Styles' },
            { name: 'rest', title: 'Rest Calls' },
            { name: 'backend-events', title: 'Backend Events' },
            { name: 'dialogs', title: 'Dialogs' }
        ];

        $scope.selectedSection = $scope.sections[0];

        $scope.showSection = function(section) {
            $scope.selectedSection = section;
            $mdSidenav('left').close();
        };

        $scope.getExampleUrl = function() {
            return 'partials/examples/' + $scope.selectedSection.name + '/' + $scope.selectedSection.name + '.html';
        }
    }
]);