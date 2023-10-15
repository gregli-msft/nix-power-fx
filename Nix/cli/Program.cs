// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Types;

namespace NixPowerFx
{
    public static class CLI
    { 
        private static bool _reset;

        private static RecalcEngine ReplRecalcEngine()
        { 
            var config = new PowerFxConfig();

            config.SymbolTable.EnableMutationFunctions();

            config.EnableSetFunction();

            config.AddFunction(new ResetFunction());
            config.AddFunction(new ExitFunction());

#pragma warning disable CS0618 // Type or member is obsolete
            config.EnableRegExFunctions(new TimeSpan(0, 0, 5));
#pragma warning restore CS0618 // Type or member is obsolete

            return new RecalcEngine(config);
        }

        public static void Main()
        {
            var enabled = new StringBuilder();

            Console.InputEncoding = System.Text.Encoding.Unicode;
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            var version = typeof(RecalcEngine).Assembly.GetName().Version.ToString();
            Console.WriteLine($"Microsoft Power Fx, Version {version}");
            Console.WriteLine("Enter Excel formulas.  Use \"Help()\" for details.");

            REPL();
        }

        // Hook repl engine with customizations.
#pragma warning disable CS0618 // Type or member is obsolete
        private class MyRepl : PowerFxREPL
#pragma warning restore CS0618 // Type or member is obsolete
        {
            public MyRepl()
            {
                this.Engine = ReplRecalcEngine();
                this.AllowSetDefinitions = true;
                this.EnableSampleUserObject();
            }

            public override async Task OnEvalExceptionAsync(Exception e, CancellationToken cancel)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }
        }

        public static void REPL()
        {
            while (true)
            {
                var repl = new MyRepl();

                while (!_reset)
                {
                    repl.WritePromptAsync().Wait();
                    var line = Console.ReadLine();
                    repl.HandleLineAsync(line).Wait();
                }

                _reset = false;
            }
        }

        private class ResetFunction : ReflectionFunction
        {
            public BooleanValue Execute()
            {
                _reset = true;
                return FormulaValue.New(true);
            }
        }

        private class ExitFunction : ReflectionFunction
        {
            public BooleanValue Execute()
            {
                System.Environment.Exit(0);
                return FormulaValue.New(true);
            }
        }
    }
}
