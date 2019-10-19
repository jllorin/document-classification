'use strict';
angular.module('Solr')
.controller('ruleCtrl', ['$scope', '$http', '$mdToast', function ($scope, $http, $mdToast) {

    $scope.value = { "responseHeader": { "status": 0, "QTime": 37 } };    

    $scope.Initialize = function () {
        $http.get('/api/Rule/GetSearchDatabases').success(function (data) {
            $scope.global.searchDatabases = JSON.parse(data);
        });
        $scope.global = {};
        $scope.global.searchResult = { Result: 'No queries yet' };
        $scope.global.switches = { query: true, classify: false};
        $scope.global.ruleNameField = {};
        $scope.global.classifyField = {};
        $scope.global.selectedSimilarFile = null;
        $http.get('/api/Rule/GetRules',
            {
                params:
                    {
                        includeBlank: true
                    }
            }).success(function (data) {
            $scope.ruleNameFields = data;
            $scope.rule = data[0];
            });
        $http.get('/api/Setup/GetSchemaFields')
        .success(function (data) {
            $scope.classifyFields = data;
        });
    };

    $scope.Query = function () {
        $http.get('/api/Rule/GetSelect',
        {
            params:
                {
                    value: JSON.stringify($scope.rule)
                }
        }).success(function (data) {
            $scope.global.searchResult = data;
            $scope.global.selectFiles = JSON.parse(data)["response"]["docs"];
            $scope.global.fieldsSplit = $scope.rule.Fields.split(',');
            //if ($scope.global.fields != null && $scope.global.fields != '') {
                
            //}
            //else {
            //    $scope.global.fieldsSplit = [];
            //}
        });
    };

    $scope.Classify = function () {
        $scope.global.classifyShowProgress = true;
        var response = $http.post('/api/Rule/ClassifyItem', $scope.rule);
        response.success(function (data) {
            var value = data;
            $scope.global.classifyShowProgress = false;            
            $scope.showSimpleToast('Classification is complete.');            
        });
    }

    $scope.selected = [];
    $scope.options = {
        rowSelection: true,
        multiSelect: false,
        autoSelect: false,
        largeEditDialog: true,
        boundaryLinks: false,
        limitSelect: true,
        pageSelect: true,
        order: 'stream_name[0]'
    };

    function b64toBlob(b64Data, contentType, sliceSize) {
        contentType = contentType || '';
        sliceSize = sliceSize || 512;

        var byteCharacters = atob(b64Data);
        var byteArrays = [];

        for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
            var slice = byteCharacters.slice(offset, offset + sliceSize);

            var byteNumbers = new Array(slice.length);
            for (var i = 0; i < slice.length; i++) {
                byteNumbers[i] = slice.charCodeAt(i);
            }

            var byteArray = new Uint8Array(byteNumbers);

            byteArrays.push(byteArray);
        }

        var blob = new Blob(byteArrays, { type: contentType });
        return blob;
    }

    $scope.OpenFile = function (fileID, contentType) {
        $http.get('/api/Rule/GetFileBytes', 
        {
            params:
                {
                    fileID: fileID
                }
        })
        .success(function (data) {
            //var file = b64toBlob(data, 'application/vnd.ms-excel', 512)
            var file = b64toBlob(data, contentType, 512)
            //new Blob([data], { type: 'application/pdf' });
            var fileURL = URL.createObjectURL(file);
            var win = window.open(fileURL);                   
        });
    }

    $scope.createFilterForRuleName = function (query) {
        var lowercaseQuery = angular.lowercase(query);
        return function filterFn(nameField) {
            return (angular.lowercase(nameField.RuleName).indexOf(lowercaseQuery) > -1);
        };
    }

    $scope.qrySearchRuleName = function (query) {
        var results = query ? $scope.ruleNameFields.filter($scope.createFilterForRuleName(query)) : $scope.ruleNameFields;
        return results;
    }

    $scope.createFilterForClassifyField = function (query) {
        var lowercaseQuery = angular.lowercase(query);
        return function filterFn(nameField) {
            return (angular.lowercase(nameField.Name).indexOf(lowercaseQuery) > -1);
        };
    }

    $scope.qrySearchClassifyField = function (query) {
        var results = query ? $scope.classifyFields.filter($scope.createFilterForClassifyField(query)) : $scope.classifyFields;
        return results;
    }

    $scope.SaveRule = function () {
        if ($scope.global.ruleNameField.selectedItem == null)        {
            $scope.rule.RuleName = $scope.global.ruleNameField.searchText;
        }
        var response = $http.post('/api/Rule/SaveRule', $scope.rule);
        response.success(function (data) {
            $scope.showSimpleToast('Rule is saved.')
            if ($scope.rule.ID == -1) {
                $http.get('/api/Rule/GetRules',
                    {
                        params:
                            {
                                includeBlank: true
                            }
                    }).success(function (data) {
                        $scope.ruleNameFields = data;
                        $scope.rule = data[0];
                        $scope.global.ruleNameField.searchText = '';
                    });
            }
        });
    }

    $scope.DeleteRule = function () {
        var response = $http.post('/api/Rule/DeleteRule', $scope.rule);
        response.success(function (data) {            
            $scope.showSimpleToast('Rule is deleted.')
            $http.get('/api/Rule/GetRules',
                {
                    params:
                        {
                            includeBlank: true
                        }
                }).success(function (data) {
                    $scope.ruleNameFields = data;
                    $scope.rule = data[0];
                    $scope.global.ruleNameField.searchText = '';
                });
        });
    }

    $scope.selectedItemRuleChange = function (item) {
        if (item != null) {
            $scope.rule = item;
        }        
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
    $scope.Initialize();

    $scope.selectedItemClassifyChange = function (item) {
        if (item == null) {
            $scope.global.classifyField.IsExisting = false;
        }
        else {
            var qry = Enumerable.From($scope.rule.ClassifyFields)
                        .Where(function (x) { return x.SchemaIDFk == item.ID })
                        .Select(function (x) { return x })
                        .ToArray();
            if (qry.length != 0) {
                $scope.global.classifyField.IsExisting = true;
                $scope.global.classifyField.Value = qry[0].Value;
            }
            else {
                $scope.global.classifyField.IsExisting = false;
            }
        }
    }

    $scope.SaveClassifyField = function (rule, classifyField) {
        var value = {
            Rule: rule,
            ClassifyField: classifyField
        }
        if (classifyField.IsExisting) {
            var qry = Enumerable.From(rule.ClassifyFields)
                .Where(function (x) { return x.SchemaIDFk == classifyField.selectedItem.ID })
                .Select(function (x) { return x })
                .ToArray();
            qry[0].Value = classifyField.Value;
        }
        else {
            var itemInJSON = {
                ID: -1,
                RuleIDFk: rule.ID,
                SchemaIDFk: classifyField.selectedItem.ID,
                SchemaName: classifyField.selectedItem.Name,
                Value: classifyField.Value,
                IsDirty: true
            }
            rule.ClassifyFields.push(itemInJSON);
        }
        $scope.global.classifyField.searchText = "";
        $scope.global.classifyField.Value = "";
        $scope.global.classifyField.selectedItem = null;
        $scope.showSimpleToast('Classify field is saved.')
    }

    $scope.changeSimilarityStyle = function (file) {
        if ($scope.rule.IsSimilarityShow) {
            if ($scope.global.selectedSimilarFile != null) {
                file.similarityStyle = {}
                $scope.global.selectedSimilarFile.similarityStyle = {
                    'cursor': $scope.rule.IsSimilarityShow ? 'pointer' : 'default',
                    'background-color': '#294451',
                };
            }
            $scope.global.selectedSimilarFile = file;
            $scope.global.selectedSimilarFile.similarityStyle = {
                'cursor': $scope.rule.IsSimilarityShow ? 'pointer' : 'default',
                'background-color': '#3F51B5'
            };
        }
    }

    $scope.similarityCheck = function () {
        if ($scope.global.selectedSimilarFile != null) {
            var value = {
                Rule: $scope.rule,
                File: $scope.global.selectedSimilarFile
            }
            var response = $http.post('/api/Rule/SimilarityCheck', value);
            response.success(function (data) {
                $scope.showSimpleToast('Similarity Check is complete.');
            });
        }
        else {
            $scope.showSimpleToast('Please choose a file basis.');
        }
    }
}]);