'use strict';
angular.module('Solr')
.controller('homeCtrl', ['$scope', 'adalAuthenticationService','$location', '$mdDialog', '$mdMedia', '$http', function ($scope, adalService, $location, $mdDialog, $mdMedia, $http) {

    $scope.login = function () {
        adalService.login();
    };

    $scope.logout = function () {
        adalService.logOut();
    };

    $scope.isActive = function (viewLocation) {
        return viewLocation === $location.path();
    };

    $scope.Initialize = function () {
        $scope.selectedAward = null;
        $scope.searchTextAward = null;
        $scope.querySearch = querySearch;        
        $scope.selectedAwards = [];
        $scope.transformChip = transformChip;

        $http.get('api/Award/Get').success(function (data) {
            $scope.Awards = data;
        });
    }

    $scope.Initialize();

    $scope.showMainEdit = function (award) {
        if (award == null) {
            award = {
                ID: -1,
                Award: '',
                Description: ''
            };
        }

        $mdDialog.show({
            controller: "awardEditorCtrl",
            templateUrl: '/App/Views/AwardEditor.html',
            parent: angular.element(document.body),            
            clickOutsideToClose: true,
            locals: {
                parameter: award
            }
        })
        .then(function (answer) {
            var response = $http.post('api/Award/Save', answer);
            response.success(function (data) {
                if (answer.ID == -1) {
                    $http.get('api/Award/Get').success(function (allAwards) {
                        $scope.Awards = allAwards;
                        $scope.selectedAwards.push(data);
                    });
                }
            });
        }, function () {
            // Cancelled it
        });     
    };

    $scope.Delete = function (award) {
        var confirm = $mdDialog.confirm()
                  .title('Would you like to delete this award?')                  
                  .ariaLabel('Lucky day')
                  .ok('Yes')
                  .cancel('No');

        $mdDialog.show(confirm).then(function () {
            var response = $http.post('api/Award/Delete', award);
            response.success(function (data) {
                var value = $scope.searchAwards.indexOf(award);
                $scope.searchAwards.splice(1, 1);
            });
        }, function () {
            // Cancelled it
        });
    };

    /**
     * Return the proper object when the append is called.
     */
    function transformChip(chip) {
        // If it is an object, it's already a known chip
        if (angular.isObject(chip)) {
            return chip;
        }

        // Otherwise, create a new one
        return { name: chip, type: 'new' }
    }

    /**
     * Search for vegetables.
     */
    function querySearch(query) {
        var results = query ? $scope.Awards.filter(createFilterFor(query)) : [];
        return results;
    }

    /**
     * Create filter function for a query string
     */
    function createFilterFor(query) {
        var lowercaseQuery = angular.lowercase(query);

        return function filterFn(award) {
            return (award.Award.toLowerCase().indexOf(lowercaseQuery) === 0);
        };

    }
}]);