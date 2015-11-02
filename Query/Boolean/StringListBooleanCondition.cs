﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OkayCloudSearch.Query.Boolean
{
    public class StringListBooleanCondition : IBooleanCondition
    {
        public string Field { get; set; }
        public List<string> Conditions { get; set; }
        public bool IsOrConditionParam { get; set; }

        private string Conditional
        {
            get
            {
                return IsOrConditionParam ?
                    Constants.Operators.Or.ToQueryString() : Constants.Operators.And.ToQueryString();
            }
        }

        public StringListBooleanCondition(string field, List<string> conditions, bool isOrConditionParam = true)
        {
            Field = field;
            Conditions = conditions;
            IsOrConditionParam = isOrConditionParam;
        }

        public string GetParam()
        {
            return Field + ":(" + String.Join(Conditional, Conditions.Select(x => "\"" + x + "\"")) + ")";
        }

        public bool IsOrCondition()
        {
            return IsOrConditionParam;
        }

        public bool IsList()
        {
            return true;
        }
    }
}
