﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OkayCloudSearch.Builder;
using OkayCloudSearch.Contract;
using OkayCloudSearch.Contract.Result;
using OkayCloudSearch.Enum;
using OkayCloudSearch.Helper;
using OkayCloudSearch.Query;
using OkayCloudSearch.Serialization;

namespace OkayCloudSearch
{

    public class CloudSearch<T> : ICloudSearch<T> where T : SearchDocument, new()
    {
        private readonly string _documentUri;
        private readonly ActionBuilder<T> _actionBuilder;
        private readonly WebHelper _webHelper;
        private readonly QueryBuilder<T> _queryBuilder;
        private readonly HitFeeder<T> _hitFeeder;
        private readonly FacetBuilder _facetBuilder;

        public CloudSearch(string awsCloudSearchId, string apiVersion)
        {
            string searchUri = string.Format("http://search-{0}/{1}/search", awsCloudSearchId, apiVersion);
            _documentUri = string.Format("http://doc-{0}/{1}/documents/batch", awsCloudSearchId, apiVersion);
            _actionBuilder = new ActionBuilder<T>();
            _queryBuilder = new QueryBuilder<T>(searchUri);
            _webHelper = new WebHelper();
            _hitFeeder = new HitFeeder<T>();
            _facetBuilder = new FacetBuilder();
        }

        private R Add<R>(List<T> liToAdd) where R : BasicResult, new()
        {
            List<BasicDocumentAction> liAction = new List<BasicDocumentAction>();

            BasicDocumentAction action;
            foreach (T toAdd in liToAdd)
            {
                action = _actionBuilder.BuildAction(toAdd, ActionType.ADD);
                liAction.Add(action);
            }

            return PerformDocumentAction<R>(liAction);
        }

        private R Add<R>(T toAdd) where R : BasicResult, new()
        {
            var action = _actionBuilder.BuildAction(toAdd, ActionType.ADD);

            return PerformDocumentAction<R>(action);
        }

        public AddResult Add(List<T> toAdd)
        {
            return Add<AddResult>(toAdd);
        }

        public AddResult Add(T toAdd)
        {
            return Add<AddResult>(toAdd);
        }

        // update is like Add but make more sense for a developper point of view
        public UpdateResult Update(T toUpdate)
        {
            return Add<UpdateResult>(toUpdate);
        }

        public DeleteResult Delete(SearchDocument toDelete)
        {
            var action = _actionBuilder.BuildDeleteAction(new SearchDocument { id = toDelete.id }, ActionType.DELETE);

            return PerformDocumentAction<DeleteResult>(action);
        }

        public SearchResult<T> Search(SearchQuery<T> query)
        {
            try
            {
                return SearchWithException(query);
            }catch(Exception ex)
            {
                return new SearchResult<T>{error = "An error occured "+ ex.Message, IsError = true};
            }
        }

        public SearchResult<T> SearchWithException(SearchQuery<T> query)
        {
            var searchUrlRequest = _queryBuilder.BuildSearchQuery(query);

            var jsonResult = _webHelper.GetRequest(searchUrlRequest);

            if (jsonResult.IsError)
                return new SearchResult<T> {error = jsonResult.Exception, IsError = true};

            var jsonDynamic = JsonConvert.DeserializeObject<dynamic>(jsonResult.Json);

            var hit = RemoveHit(jsonDynamic);

            var resultWithoutHit = JsonConvert.SerializeObject(jsonDynamic);

            SearchResult<T> searchResult = JsonConvert.DeserializeObject<SearchResult<T>>(resultWithoutHit);

            searchResult.facetsResults = _facetBuilder.BuildFacet(jsonDynamic);

            if (searchResult.error != null)
            {
                searchResult.IsError = true;
                return searchResult;
            }
            
            _hitFeeder.Feed(searchResult, hit);

            return searchResult;
        }

        private dynamic RemoveHit(dynamic jsonDynamic)
        {
            dynamic hit = null;
            if (jsonDynamic.hits != null)
            {
                hit = jsonDynamic.hits.hit;
                jsonDynamic.hits.hit = null;
            }
            return hit;
        }


        private R PerformDocumentAction<R>(List<BasicDocumentAction> liAction) where R : BasicResult, new()
        {
            string actionJson = JsonConvert.SerializeObject(liAction);

            var jsonResult = _webHelper.PostRequest(_documentUri, actionJson);

            if (jsonResult.IsError)
                return new R { IsError = true, status = "error", errors = new List<Error> { new Error { message = jsonResult.Exception } } };

            R result = JsonConvert.DeserializeObject<R>(jsonResult.Json);

            if (result.status != null && result.status.Equals("error"))
                result.IsError = true;

            return result;
        }

        private R PerformDocumentAction<R>(BasicDocumentAction basicDocumentAction) where R : BasicResult, new()
        {
            var listAction = new List<BasicDocumentAction> { basicDocumentAction };

            return PerformDocumentAction<R>(listAction);
        }
    }
}