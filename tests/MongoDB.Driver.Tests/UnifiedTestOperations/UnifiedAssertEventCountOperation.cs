﻿/* Copyright 2010-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver.Core;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Tests.UnifiedTestOperations.Matchers;
using MongoDB.TestHelpers;

namespace MongoDB.Driver.Tests.UnifiedTestOperations
{
    public sealed class UnifiedAssertEventCountOperation : IUnifiedSpecialTestOperation
    {
        private readonly EventCapturer _eventCapturer;
        private readonly int _count;
        private readonly BsonDocument _event;

        public UnifiedAssertEventCountOperation(EventCapturer eventCapturer, BsonDocument @event, int? count)
        {
            _eventCapturer = Ensure.IsNotNull(eventCapturer, nameof(eventCapturer));
            _event = Ensure.IsNotNull(@event, nameof(@event));
            _count = Ensure.HasValue(count, nameof(count)).Value;
        }

        public void Execute()
        {
            var eventCondition = UnifiedEventMatcher.GetEventFilter(_event);
            var actualEventsCount = _eventCapturer
                .Events
                .Count(eventCondition);

            var becauseMessage = $"{FluentAssertionsHelper.EscapeBraces(_event.ToString())} must be triggered exactly {_count} times";
            actualEventsCount.Should().Be(_count, becauseMessage);
        }
    }

    public sealed class UnifiedAssertEventCountOperationBuilder
    {
        private readonly UnifiedEntityMap _entityMap;

        public UnifiedAssertEventCountOperationBuilder(UnifiedEntityMap entityMap)
        {
            _entityMap = entityMap;
        }

        public UnifiedAssertEventCountOperation Build(BsonDocument arguments)
        {
            EventCapturer eventCapturer = null;
            int? count = null;
            BsonDocument @event = null;

            foreach (var argument in arguments)
            {
                switch (argument.Name)
                {
                    case "client":
                        eventCapturer = _entityMap.EventCapturers[argument.Value.AsString];
                        break;
                    case "count":
                        count = argument.Value.AsInt32;
                        break;
                    case "event":
                        @event = argument.Value.AsBsonDocument;
                        break;
                    default:
                        throw new FormatException($"Invalid {nameof(UnifiedAssertEventCountOperation)} argument name: '{argument.Name}'.");
                }
            }

            return new UnifiedAssertEventCountOperation(eventCapturer, @event, count);
        }
    }
}
