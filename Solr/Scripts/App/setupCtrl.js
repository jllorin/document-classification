'use strict';
angular.module('Solr')
.controller('setupCtrl', ['$scope', '$http', '$mdToast', function ($scope, $http, $mdToast) {

    $scope.global = {};

    $scope.Initialize = function () {
        $scope.global.Directory = "C:\\temp\\FilesToBeProcessed";
        $scope.global.SearchPattern = "*.*";
        $scope.global.newItem = {};

        $scope.schemaTypes = [
            {
                text: "boolean",
                value: "boolean"
            },
            {
                text: "date",
                value: "date"
            },
            {
                text: "double",
                value: "double"
            },
            {
                text: "int",
                value: "int"
            },
            {
                text: "string",
                value: "string"
            }
        ];

        $http.get('/api/Setup/GetSchemaFields')
        .success(function (data) {
            $scope.schemaFields = data;
        });
    }

    $scope.Initialize();

    $scope.IngestFiles = function () {        
        var response = $http.post('/api/Setup/IngestFiles', $scope.global);
        response.success(function (data) {
            var value = "success";
            $scope.showSimpleToast('Ingest Files Finished.')
        });
    };

    $scope.DeleteFiles = function () {
        var response = $http.post('/api/Setup/DeleteFiles', $scope.global);
        response.success(function (data) {
            var value = "success";
            $scope.showSimpleToast('Delete Files Finished.')
        });
    }

    $scope.createFilterFor = function (query) {
        var lowercaseQuery = angular.lowercase(query);
        return function filterFn(schemaField) {
            return (angular.lowercase(schemaField.Name).indexOf(lowercaseQuery) > -1);
        };
    }

    $scope.querySearch = function (query) {
        var results = query ? $scope.schemaFields.filter($scope.createFilterFor(query)) : $scope.schemaFields;
        return results;
    }

    $scope.Save = function () {
        $scope.global.newItem.Name = $scope.global.newItem.searchText;
        var response = $http.post('/api/Setup/AddSchemaField', $scope.global.newItem);
        response.success(function (data) {
            $http.get('/api/Setup/GetSchemaFields')
            .success(function (data) {
                $scope.schemaFields = data;
            });
            $scope.global.newItem = null;
            $scope.global.searchText = '';
        });
    }

    $scope.Delete = function () {
        var response = $http.post('/api/Setup/DeleteSchemaField', $scope.global.selectedItem);
        response.success(function (data) {
            var value = Enumerable.From($scope.schemaFields)
            .IndexOf($scope.global.selectedItem);
            $scope.schemaFields.splice(value, 1);
            $scope.global.selectedItem = null;
            $scope.global.newItem = null;
            $scope.global.searchText = '';
        });
    }

    var last = {
        bottom: false,
        top: true,
        left: false,
        right: true
    };
    $scope.toastPosition = angular.extend({}, last);
    $scope.getToastPosition = function () {
        sanitizePosition();
        return Object.keys($scope.toastPosition)
          .filter(function (pos) { return $scope.toastPosition[pos]; })
          .join(' ');
    };

    function sanitizePosition() {
        var current = $scope.toastPosition;
        if (current.bottom && last.top) current.top = false;
        if (current.top && last.bottom) current.bottom = false;
        if (current.right && last.left) current.left = false;
        if (current.left && last.right) current.right = false;
        last = angular.extend({}, current);
    }

    $scope.showSimpleToast = function (content) {
        $mdToast.show(
          $mdToast.simple()
            .textContent(content)
            .position($scope.getToastPosition())
            .hideDelay(6000)
        );
    };

    $scope.selectedItemChange = function (item) {
        if (item != null) {
            $scope.global.newItem.Type = item.Type;
            $scope.global.newItem.Stored = item.Stored;
            $scope.global.newItem.Indexed = item.Indexed;
            $scope.global.newItem.DefaultValue = item.DefaultValue;
        }
    };
}]);