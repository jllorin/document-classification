'use strict';
angular.module('Solr', ['ngRoute', 'ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'ngPrettyJson', 'md.data.table', 'ngSanitize'])
.config(['$routeProvider', '$httpProvider', '$mdThemingProvider', function ($routeProvider, $httpProvider, adalProvider, $mdThemingProvider) {

    $routeProvider.when("/Home", {
        controller: "homeCtrl",
        templateUrl: "/App/Views/Home.html",
        requireADLogin: true,
    }).when("/UserData", {
        controller: "userDataCtrl",
        templateUrl: "/App/Views/UserData.html",
    }).otherwise({ redirectTo: "/Home" });

}]);
