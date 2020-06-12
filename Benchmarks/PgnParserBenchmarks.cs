#region License
/*********************************************************************************
 * PgnParserBenchmarks.cs
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
using Sandra.Chess.Pgn;
using System.IO;

namespace Benchmarks
{
    [RyuJitX64Job]
    public class PgnParserBenchmarks
    {
        // Benchmark 4 common PGN use cases and one uncommon one:
        //
        // a) The article/lesson/demo use case: a few heavily annotated games.
        // b) The database use case: a huge file with many flat games without variations or comments.
        // c) The problems/studies use case: many set-up positions with few moves, and therefore an emphasis on tag pairs.
        // d) The live PGN use case: a few games with a lot of clock times and other machine generated annotations.
        // e) The errors use case: measure the parser's performance on files with huge stack depth and lots of errors.
        [Params(
            // From: https://chessentials.com/chess-pgn-downloads/
            "Lasker Best Games.pgn",
            // From: https://www.pgnmentor.com/files.html
            "Kasparov.pgn",
            // Problems use case: TODO, becomes more relevant once the FEN tag is supported.
            // From: https://www.tcec-chess.com/archive.html
            "TCEC_Season_16_-_Division_Testing_Lczero_1%_Vs_Qualification1.pgn"
            // Errors use case: TODO.
            )]
        public string PgnFileName { get; set; }

        private string pgn;

        [GlobalSetup(Target = nameof(Parse))]
        public void Setup() => pgn = File.ReadAllText(Path.Combine(
            Path.GetDirectoryName(typeof(PgnParserBenchmarks).Assembly.Location),
            PgnFileName));

        [Benchmark]
        public RootPgnSyntax Parse() => PgnParser.Parse(pgn);
    }
}
