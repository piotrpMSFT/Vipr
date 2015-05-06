﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Vipr.Core.CodeModel;

namespace Vipr.Writer.CSharp
{
    public class Enum
    {
        public Identifier Identifier { get; private set; }
        public string Namespace { get; private set; }
        public string Description { get; private set; }
        public IEnumerable<EnumMember> Members { get; private set; }
        public string UnderlyingType { get; private set; }

        public Enum(OdcmEnum odcmEnum)
        {
            Identifier = NamesService.GetEnumTypeName(odcmEnum);
            Description = odcmEnum.Description;
            // if no Underlying type is specified then default to 'int'.
            UnderlyingType =
                odcmEnum.UnderlyingType == null
                    ? "int"
                    : NamesService.GetPrimitiveTypeKeyword(odcmEnum.UnderlyingType);
            Members = odcmEnum.Members.Select(m => new EnumMember(m));
        }
    }
}