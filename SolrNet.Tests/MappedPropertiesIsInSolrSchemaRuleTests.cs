﻿#region license

// Copyright (c) 2007-2010 Mauricio Scheffer
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Linq;
using Xunit;
using SolrNet.Mapping;
using SolrNet.Mapping.Validation;
using SolrNet.Mapping.Validation.Rules;
using SolrNet.Schema;
using SolrNet.Tests.Utils;

namespace SolrNet.Tests {
    
    public class MappedPropertiesIsInSolrSchemaRuleTests {
        [Fact]
        public void MappedPropertyForWhichSolrFieldExistsInSchemaShouldNotReturnError() {
            var mgr = new MappingManager();
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("ID"), "id");
            mgr.SetUniqueKey(typeof (SchemaMappingTestDocument).GetProperty("ID"));
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("Name"), "name");

            var schemaManager = new MappingValidator(mgr, new[] {new MappedPropertiesIsInSolrSchemaRule()});

            var schemaXmlDocument = EmbeddedResource.GetEmbeddedXml(GetType(), "Resources.solrSchemaBasic.xml");
            var solrSchemaParser = new SolrSchemaParser();
            var schema = solrSchemaParser.Parse(schemaXmlDocument);

            var validationResults = schemaManager.EnumerateValidationResults(typeof (SchemaMappingTestDocument), schema).ToList();
            Assert.Equal(0, validationResults.Count);
        }

        [Fact]
        public void MappedPropertyForWhichDynamicFieldExistsInSchemaShouldNotReturnError() {
            var mgr = new MappingManager();
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("ID"), "id");
            mgr.SetUniqueKey(typeof (SchemaMappingTestDocument).GetProperty("ID"));
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("Name"), "name");
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("Producer"), "producer_s");

            var schemaManager = new MappingValidator(mgr, new[] {new MappedPropertiesIsInSolrSchemaRule()});

            var schemaXmlDocument = EmbeddedResource.GetEmbeddedXml(GetType(), "Resources.solrSchemaBasic.xml");
            var solrSchemaParser = new SolrSchemaParser();
            var schema = solrSchemaParser.Parse(schemaXmlDocument);

            var validationResults = schemaManager.EnumerateValidationResults(typeof (SchemaMappingTestDocument), schema).ToList();
            Assert.Equal(0, validationResults.Count);
        }

        [Fact]
        public void MappedPropertyForWhichNoSolrFieldOrDynamicFieldExistsShouldReturnError() {
            var mgr = new MappingManager();
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("ID"), "id");
            mgr.SetUniqueKey(typeof (SchemaMappingTestDocument).GetProperty("ID"));
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("Name"), "name");
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("FieldNotSolrSchema"), "FieldNotSolrSchema");

            var schemaManager = new MappingValidator(mgr, new[] {new MappedPropertiesIsInSolrSchemaRule()});

            var schemaXmlDocument = EmbeddedResource.GetEmbeddedXml(GetType(), "Resources.solrSchemaBasic.xml");
            var solrSchemaParser = new SolrSchemaParser();
            var schema = solrSchemaParser.Parse(schemaXmlDocument);

            var validationResults = schemaManager.EnumerateValidationResults(typeof (SchemaMappingTestDocument), schema).ToList();
            Assert.Equal(1, validationResults.Count);
        }

        [Fact]
        public void MappedPropertiesIsInSolrSchemaRule_ignores_score() {
            var rule = new MappedPropertiesIsInSolrSchemaRule();
            var mapper = new MappingManager();
            mapper.Add(typeof (SchemaMappingTestDocument).GetProperty("Score"), "score");
            var results = rule.Validate(typeof (SchemaMappingTestDocument), new SolrSchema(), mapper).ToList();
            Assert.NotNull(results);
            Assert.Equal(0, results.Count);
        }

        [Fact]
        public void DictionaryFields_are_ignored() {
            var rule = new MappedPropertiesIsInSolrSchemaRule();
            var mapper = new MappingManager();
            mapper.Add(typeof(SchemaMappingTestDocument).GetProperty("DynamicMapped"), "ma_*");
            var schema = new SolrSchema();
            var fieldType = new SolrFieldType("string", "solr.StrField");
            schema.SolrFields.Add(new SolrField("ma_uaua", fieldType));
            var results = rule.Validate(typeof(SchemaMappingTestDocument), new SolrSchema(), mapper).ToList();
            Assert.Equal(0, results.Count);
        }
    }
}