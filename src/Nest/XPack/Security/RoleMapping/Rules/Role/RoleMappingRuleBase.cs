﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using Elasticsearch.Net.Utf8Json;

namespace Nest
{
	[DataContract]
	[JsonFormatter(typeof(RoleMappingRuleBaseFormatter))]
	public abstract class RoleMappingRuleBase
	{
		[DataMember(Name ="all")]
		protected internal IEnumerable<RoleMappingRuleBase> AllRules { get; set; }

		[DataMember(Name ="any")]
		protected internal IEnumerable<RoleMappingRuleBase> AnyRules { get; set; }

		[DataMember(Name ="except")]
		protected internal RoleMappingRuleBase ExceptRule { get; set; }

		[DataMember(Name ="field")]
		protected internal FieldRuleBase FieldRule { get; set; }

		public static AnyRoleMappingRule operator |(RoleMappingRuleBase leftContainer, RoleMappingRuleBase rightContainer) =>
			CombineAny(leftContainer, rightContainer);

		public static AllRoleMappingRule operator &(RoleMappingRuleBase leftContainer, RoleMappingRuleBase rightContainer) =>
			CombineAll(leftContainer, rightContainer);

		public static AllRoleMappingRule operator +(RoleMappingRuleBase leftContainer, RoleMappingRuleBase rightContainer) =>
			CombineAll(leftContainer, rightContainer);

		public static ExceptRoleMappingRole operator !(RoleMappingRuleBase leftContainer) =>
			new ExceptRoleMappingRole(leftContainer);

		private static AnyRoleMappingRule CombineAny(RoleMappingRuleBase left, RoleMappingRuleBase right)
		{
			var l = new List<RoleMappingRuleBase>();
			l.AddRangeIfNotNull(AnyOrSelf(left));
			l.AddRangeIfNotNull(AnyOrSelf(right));
			return new AnyRoleMappingRule(l);
		}

		private static AllRoleMappingRule CombineAll(RoleMappingRuleBase left, RoleMappingRuleBase right)
		{
			var l = new List<RoleMappingRuleBase>();
			l.AddRangeIfNotNull(AllOrSelf(left));
			l.AddRangeIfNotNull(AllOrSelf(right));
			return new AllRoleMappingRule(l);
		}

		public static IEnumerable<RoleMappingRuleBase> AllOrSelf(RoleMappingRuleBase rule)
		{
			var all = rule as AllRoleMappingRule;
			return all != null ? all.AllRules : new[] { rule };
		}

		public static IEnumerable<RoleMappingRuleBase> AnyOrSelf(RoleMappingRuleBase rule)
		{
			var all = rule as AnyRoleMappingRule;
			return all != null ? all.AnyRules : new[] { rule };
		}

		public static implicit operator RoleMappingRuleBase(UsernameRule rule) => new FieldRoleMappingRule(rule);

		public static implicit operator RoleMappingRuleBase(DistinguishedNameRule rule) => new FieldRoleMappingRule(rule);

		public static implicit operator RoleMappingRuleBase(GroupsRule rule) => new FieldRoleMappingRule(rule);

		public static implicit operator RoleMappingRuleBase(MetadataRule rule) => new FieldRoleMappingRule(rule);

		public static implicit operator RoleMappingRuleBase(RealmRule rule) => new FieldRoleMappingRule(rule);
	}
}
