﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.UseDefaultLiteral;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Diagnostics;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.UseDefaultLiteral
{
    public class UseDefaultLiteralTests : AbstractCSharpDiagnosticProviderBasedUserDiagnosticTest
    {
        internal override (DiagnosticAnalyzer, CodeFixProvider) CreateDiagnosticProviderAndFixer(Workspace workspace)
            => (new CSharpUseDefaultLiteralDiagnosticAnalyzer(), new CSharpUseDefaultLiteralCodeFixProvider());

        private static readonly CSharpParseOptions s_parseOptions = 
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_1);

        private static readonly TestParameters s_testParameters =
            new TestParameters(parseOptions: s_parseOptions);

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseDefaultLiteral)]
        public async Task TestNotInCSharp7()
        {
            await TestMissingAsync(
@"
class C
{
    void Foo(string s = [||]default(string))
    {
    }
}", parameters: new TestParameters(
    parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7)));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseDefaultLiteral)]
        public async Task TestInParameterList()
        {
            await TestAsync(
@"
class C
{
    void Foo(string s = [||]default(string))
    {
    }
}",
@"
class C
{
    void Foo(string s = default)
    {
    }
}", parseOptions: s_parseOptions);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseDefaultLiteral)]
        public async Task TestInLocalInitializer()
        {
            await TestAsync(
@"
class C
{
    void Foo()
    {
        string s = [||]default(string);
    }
}",
@"
class C
{
    void Foo()
    {
        string s = default;
    }
}", parseOptions: s_parseOptions);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseDefaultLiteral)]
        public async Task TestNotForVar()
        {
            await TestMissingAsync(
@"
class C
{
    void Foo()
    {
        var s = [||]default(string);
    }
}",  parameters: s_testParameters);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseDefaultLiteral)]
        public async Task TestInInvocationExpression()
        {
            await TestAsync(
@"
class C
{
    void Foo()
    {
        Bar([||]default(string));
    }

    void Bar(string s) { }
}",
@"
class C
{
    void Foo()
    {
        Bar(default);
    }

    void Bar(string s) { }
}", parseOptions: s_parseOptions);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseDefaultLiteral)]
        public async Task TestNotWithMultipleOverloads()
        {
            await TestMissingAsync(
@"
class C
{
    void Foo()
    {
        Bar([||]default(string));
    }

    void Bar(string s) { }
    void Bar(int i);
}", parameters: s_testParameters);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseDefaultLiteral)]
        public async Task TestLeftSideOfTernary()
        {
            await TestAsync(
@"
class C
{
    void Foo(bool b)
    {
        var v = b ? [||]default(string) : default(string);
    }
}",
@"
class C
{
    void Foo(bool b)
    {
        var v = b ? default : default(string);
    }
}", parseOptions: s_parseOptions);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseDefaultLiteral)]
        public async Task TestRightSideOfTernary()
        {
            await TestAsync(
@"
class C
{
    void Foo(bool b)
    {
        var v = b ? default(string) : [||]default(string);
    }
}",
@"
class C
{
    void Foo(bool b)
    {
        var v = b ? default(string) : default;
    }
}", parseOptions: s_parseOptions);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseDefaultLiteral)]
        public async Task TestFixAll1()
        {
            await TestAsync(
@"
class C
{
    void Foo()
    {
        string s1 = {|FixAllInDocument:default|}(string);
        string s2 = default(string);
    }
}",
@"
class C
{
    void Foo()
    {
        string s1 = default;
        string s2 = default;
    }
}", parseOptions: s_parseOptions);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseDefaultLiteral)]
        public async Task TestFixAll2()
        {
            await TestAsync(
@"
class C
{
    void Foo(bool b)
    {
        string s1 = b ? {|FixAllInDocument:default|}(string) : default(string);
    }
}",
@"
class C
{
    void Foo(bool b)
    {
        string s1 = b ? default : default(string);
    }
}", parseOptions: s_parseOptions);
        }
    }
}