#region License
/*********************************************************************************
 * JsonParserBenchmarks.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
**********************************************************************************/
#endregion

using BenchmarkDotNet.Attributes;
using Eutherion.Text.Json;

namespace Benchmarks
{
    [RyuJitX64Job]
    public class JsonParserBenchmarks
    {
        [Params(45, 450)]
        public int N { get; set; }

        [Params(
            " \r\n",
            "false ",
            "{[",
            "/**/ [ {\"\" : 8, \"\\n\" : [] } ]"
            )]
        public string Json { get; set; }

        private string repeatedJson;

        [GlobalSetup(Target = nameof(Parse))]
        public void Setup() => repeatedJson = string.Concat(Json, N);

        [Benchmark]
        public RootJsonSyntax Parse() => JsonParser.Parse(repeatedJson);
    }
}
