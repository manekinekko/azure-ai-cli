//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System.Linq;

namespace Azure.AI.Details.Common.CLI
{
    public class TranslateCommandParser : CommandParser
    {
        public static bool ParseCommand(INamedValueTokens tokens, ICommandValues values)
        {
            return ParseCommand("translate", translateCommandParsers, tokens, values);
        }

        public static bool ParseCommandValues(INamedValueTokens tokens, ICommandValues values)
        {
            return ParseCommandValues("translate", translateCommandParsers, tokens, values);
        }

        public static IEnumerable<INamedValueTokenParser> GetCommandParsers()
        {
            return translateCommandParsers;
        }

        #region private data

        private static INamedValueTokenParser[] translateCommandParsers = {

            new NamedValueTokenParser(null,           "x.command", "11", "1", "translate"),

            new ExpectOutputTokenParser(),
            new DiagnosticLogTokenParser(),
            new CommonNamedValueTokenParsers(),

            new NamedValueTokenParser(null,           "x.command.expand.file.name", "11111", "1"),

            new SpeechConfigServiceConnectionTokenParser(),
            new TrueFalseNamedValueTokenParser("service.config.content.logging.enabled", "00011;00110"),

            new NamedValueTokenParser(null,           "service.config.category.id", "0011", "1"),

            new NamedValueTokenParser("--target",     "target.language.config", "100", "1"),
            new NamedValueTokenParser("--languages",  "source.language.config", "100;010", "1"),
            new NamedValueTokenParser("--profanity",  "service.output.config.profanity.option", "00010", "1", "masked;raw;removed"),
            new TrueFalseNamedValueTokenParser("service.output.config.word.level.timing", "000101"),

            new NamedValueTokenParser("--property",   "config.string.property", "001", "2;1"),
            new NamedValueTokenParser("--properties", "config.string.properties", "001", "1", "@;"),

            new NamedValueTokenParser(null,           "audio.input.id.url", "0011", "1"),

            new NamedValueTokenParser("--id",         "audio.input.id", "001", "1"),
            new NamedValueTokenParser("--file",       "audio.input.file", "001", "1", null, null, "file", "audio.input.type"),
            new NamedValueTokenParser("--files",      "audio.input.files", "001", "1", null, null, "audio.input.file", "x.command.expand.file.name"),
            new NamedValueTokenParser("--format",     "audio.input.format", "001", "1", "any;mp3;ogg;flac;alaw;opus", null, "file", "audio.input.type"),
            new NamedValueTokenParser(null,           "audio.input.microphone.geometry", "0001", "1", null, null, "microphone", "audio.input.type"),
            new NamedValueTokenParser(null,           "audio.input.microphone.device", "0010", "1;0", null, null, "microphone", "audio.input.type"),
            new NamedValueTokenParser(null,           "audio.input.push.stream.file", "00100;01100", "1", null, null, "push", "audio.input.type"),
            new NamedValueTokenParser(null,           "audio.input.pull.stream.file", "00100;01100", "1", null, null, "pull", "audio.input.type"),
            new NamedValueTokenParser(null,           "audio.input.type", "011", "1", "file;files;microphone;push;pull;blob"),
            new NamedValueTokenParser(null,           "audio.input.file", "010", "1", null, "audio.input.file", "file", "audio.input.type"),
            new NamedValueTokenParser("--rtf",        "audio.input.real.time.factor", "00110", "1"),
            new NamedValueTokenParser("--fast",       "audio.input.fast.lane", "0010", "1"),

            new NamedValueTokenParser("--phrases",    "grammar.phrase.list", "011", "1", "@;"),
            new NamedValueTokenParser(null,           "grammar.recognition.factor.phrase", "0110", "1"),

            new NamedValueTokenParser(null,           "luis.key", "11", "1"),
            new NamedValueTokenParser(null,           "luis.region", "11", "1"),
            new NamedValueTokenParser(null,           "luis.appid", "11", "1"),
            new NamedValueTokenParser(null,           "luis.intent", "11", "2;1"),
            new NamedValueTokenParser("--allintents", "luis.allintents", "01", "1;0", "true;false", null, "true"),
            new NamedValueTokenParser("--pattern",    "intent.pattern", "01;10", "2;1"),
            new NamedValueTokenParser("--patterns",   "intent.patterns", "01", "1", "@"),

            new NamedValueTokenParser("--participant", "conversation.participant", "01", "2;1"),
            new NamedValueTokenParser("--participants", "conversation.participants", "01", "1", "@"),

            new ConnectDisconnectNamedValueTokenParser(),

            new NamedValueTokenParser(null,           "usp.speech.config", "011", "1"),
            new NamedValueTokenParser(null,           "usp.speech.context", "011", "1"),

            new NamedValueTokenParser(null,           "recognizer.property", "11", "2;1"),
            new NamedValueTokenParser(null,           "recognize.keyword.file", "010", "1", null, null, "keyword", "recognize.method"),
            new NamedValueTokenParser(null,           "recognize.timeout", "01", "1"),
            new NamedValueTokenParser("--recognize",  "recognize.method", "10", "1", "keyword;continuous;once+;once;rest"),
            new NamedValueTokenParser("--continuous", "recognize.method", "10", "0", null, null, "continuous"),
            new NamedValueTokenParser("--once+",      "recognize.method", "10", "0", null, null, "once+"),
            new NamedValueTokenParser("--once",       "recognize.method", "10", "0", null, null, "once"),

            new NamedValueTokenParser("--ini",        "ini.file", "10", "1", "@"),

            new NamedValueTokenParser(null, "transcript.translated.text", "110", "1"),
            new NamedValueTokenParser(null, "transcript.translated.*.text", "1110", "1"),
            new NamedValueTokenParser(null, "transcript.lexical.text", "110", "1"),
            new NamedValueTokenParser(null, "transcript.itn.text", "110", "1"),
            new NamedValueTokenParser(null, "transcript.text", "10", "1"),

            new NamedValueTokenParser(null, "check.sr.transcript.text.wer", "10001", "2;1", null, "check.sr.transcript.text.wer", "true", "output.all.recognizer.recognized.result.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.itn.text.wer", "100101", "2;1", null, "check.sr.transcript.itn.text.wer", "true", "output.all.recognizer.recognized.result.itn.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.lexical.text.wer", "100101", "2;1", null, "check.sr.transcript.lexical.text.wer", "true", "output.all.recognizer.recognized.result.lexical.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.translated.text.wer", "100101", "2;1", null, "check.sr.transcript.translated.text.wer", "true", "output.all.recognizer.recognized.result.translated.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.translated.*.text.wer", "1001101", "2;1", null, "check.sr.transcript.translated.*.text.wer", "true", "output.all.recognizer.recognized.result.translated.*.text"),

            new NamedValueTokenParser(null, "check.sr.transcript.text.in", "10011", "1", "@;", "check.sr.transcript.text.in", "true", "output.all.recognizer.recognized.result.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.text.contains", "10011", "1", null, "check.sr.transcript.text.contains", "true", "output.all.recognizer.recognized.result.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.text.not.in", "100111", "1", "@;", "check.sr.transcript.text.not.in", "true", "output.all.recognizer.recognized.result.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.text.not.contains", "100111", "1", null, "check.sr.transcript.text.not.contains", "true", "output.all.recognizer.recognized.result.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.text", "1001", "2;1", null, "check.sr.transcript.text", "true", "output.all.recognizer.recognized.result.text"),

            new NamedValueTokenParser(null, "check.sr.transcript.itn.text.in", "100101", "1", "@;", "check.sr.transcript.itn.text.in", "true", "output.all.recognizer.recognized.result.itn.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.itn.text.contains", "100101", "1", null, "check.sr.transcript.itn.text.contains", "true", "output.all.recognizer.recognized.result.itn.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.itn.text.not.in", "1001011", "1", "@;", "check.sr.transcript.itn.text.not.in", "true", "output.all.recognizer.recognized.result.itn.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.itn.text.not.contains", "1001011", "1", null, "check.sr.transcript.itn.text.not.contains", "true", "output.all.recognizer.recognized.result.itn.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.itn.text", "10010", "2;1", null, "check.sr.transcript.itn.text", "true", "output.all.recognizer.recognized.result.itn.text"),

            new NamedValueTokenParser(null, "check.sr.transcript.lexical.text.in", "100101", "1", "@;", "check.sr.transcript.lexical.text.in", "true", "output.all.recognizer.recognized.result.lexical.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.lexical.text.contains", "100101", "1", null, "check.sr.transcript.lexical.text.contains", "true", "output.all.recognizer.recognized.result.lexical.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.lexical.text.not.in", "1001011", "1", "@;", "check.sr.transcript.lexical.text.not.in", "true", "output.all.recognizer.recognized.result.lexical.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.lexical.text.not.contains", "1001011", "1", null, "check.sr.transcript.lexical.text.not.contains", "true", "output.all.recognizer.recognized.result.lexical.text"),
            new NamedValueTokenParser(null, "check.sr.transcript.lexical.text", "10010", "2;1", null, "check.sr.transcript.lexical.text", "true", "output.all.recognizer.recognized.result.lexical.text"),

            new NamedValueTokenParser(null, "check.jmes", "10", "1"),

            new TrueFalseNamedValueTokenParser("output.overwrite", "11"),
            new TrueFalseNamedValueTokenParser("output.audio.input.id", "1101;1011"),

            new TrueFalseNamedValueTokenParser("output.all.recognizer.recognizing.result.translated.text", "1001011"),
            new NamedValueTokenParser(null, "output.all.recognizer.recognizing.result.translated.*.text", "10010111", "1;0", "true;false", null, "true"),
            new TrueFalseNamedValueTokenParser("output.all.recognizer.recognized.result.translated.text", "1000011"),
            new NamedValueTokenParser(null, "output.all.recognizer.recognized.result.translated.*.text", "10000111", "1;0", "true;false", null, "true"),

            new TrueFalseNamedValueTokenParser("output.all.result.translated.text", "11011"),
            new NamedValueTokenParser(null, "output.all.result.translated.*.text", "110111", "1;0", "true;false", null, "true"),

            new TrueFalseNamedValueTokenParser("output.each.recognizer.recognizing.result.translated.text", "1001011"),
            new NamedValueTokenParser(null, "output.each.recognizer.recognizing.result.translated.*.text", "10010111", "1;0", "true;false", null, "true"),
            new TrueFalseNamedValueTokenParser("output.each.recognizer.recognized.result.translated.text", "1001011"),
            new NamedValueTokenParser(null, "output.each.recognizer.recognized.result.translated.*.text", "10010111", "1;0", "true;false", null, "true"),

            new TrueFalseNamedValueTokenParser("output.each.result.translated.text", "11011"),
            new NamedValueTokenParser(null, "output.each.result.translated.*.text", "110111", "1;0", "true;false", null, "true"),

            new OutputBatchRecognizerTokenParser(),
            new OutputSrtVttRecognizerTokenParser(),

            new OutputAllRecognizerEventTokenParser(),
            new OutputEachRecognizerEventTokenParser()
        };

        #endregion
    }
}
