﻿#region license
// JunkDrawer
// An easier way to import excel or delimited files into a database.
// Copyright 2013-2017 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using Transformalize.Configuration;
using Transformalize.Contracts;
using Transformalize.Providers.Ado;

namespace JunkDrawer {
    public class Importer : IResolvable {
        private readonly Process _process;
        private readonly IRunTimeExecute _executor;
        private readonly IConnectionFactory _cf;

        public Importer(
            Process process,
            IRunTimeExecute executor,
            IConnectionFactory cf
        ) {
            _process = process;
            _executor = executor;
            _cf = cf;
        }

        public Response Import() {

            try {
                _executor.Execute(_process);
                var entity = _process.Entities.First();

                var starFields = _process.GetStarFields().ToArray();
                var fields = new List<Field>();
                fields.AddRange(starFields[0].Where(f => !f.System).Select(f => new Field { Name = f.Alias, Alias = f.Alias, Type = f.Type, Length = f.Length }));
                fields.AddRange(starFields[1].Where(f => !f.System).Select(f => new Field { Name = f.Alias, Alias = f.Alias, Type = f.Type, Length = f.Length }));

                var arr = fields.ToArray();

                var use = (_cf.AdoProvider == AdoProvider.SqlServer || _cf.AdoProvider == AdoProvider.MySql ? $"USE {_cf.Enclose(_process.Output().Database)};" : string.Empty) + System.Environment.NewLine;

                return new Response {
                    Records = entity.Inserts,
                    Connection = _process.Output(),
                    Fields = arr,
                    View = entity.Alias,
                    Sql = $@"{use}
SELECT
{(string.Join("," + System.Environment.NewLine, arr.Select(f => "    " + _cf.Enclose(f.Alias))))}
FROM {_cf.Enclose(entity.Alias)};"
                };

            } catch (Exception) {
                return new Response();
            }

        }

    }
}
