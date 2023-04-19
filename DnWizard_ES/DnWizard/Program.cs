using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Linq;
using System.Text;
using System.Threading;

class Program
{
    static string GetRandomString()
    {
        int length = 10; // Longitud de la cadena de caracteres aleatorios
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"; // Caracteres permitidos

        StringBuilder builder = new StringBuilder();

        Random random = new Random();
        builder.Append("Qzxtu_"); // Agregar "Qzxtu_" al principio de la cadena
        for (int i = 0; i < length; i++)
        {
            int index = random.Next(chars.Length);
            char c = chars[index];
            builder.Append(c);
        }

        return builder.ToString();
    }

    static void AnimatedWriteLine(string text, int speed)
    {
        Console.ForegroundColor = ConsoleColor.Green; // Set the text color
        int originalLeft = Console.CursorLeft; // Save the original cursor position
        for (int i = 0; i < text.Length; i++)
        {
            Console.Write(text[i]);
            Console.Beep(1000 + i * 50, 50); // Add a frequency change to the sound effect
            Thread.Sleep(speed);
            Console.ForegroundColor = Console.ForegroundColor == ConsoleColor.Green ? ConsoleColor.Yellow : ConsoleColor.Green; // Gradient effect
        }
        Console.CursorLeft = originalLeft; // Return to the original cursor position
        for (int i = 0; i < text.Length; i++)
        {
            Console.Write(" ");
            Console.Beep(1000 - i * 20, 50); // Add a frequency change to the sound effect
            Thread.Sleep(speed);
        }
        Console.CursorLeft = originalLeft; // Return to the original cursor position
        Console.ResetColor(); // Restore the console color
        Thread.Sleep(500); // Delay before writing the complete message
        Console.WriteLine(text); // Write the complete message
    }

    static void Main(string[] args)
    {
        Console.Title = "DnWizard 🧙";
        AnimatedWriteLine("DnWizard", 50);
        Thread.Sleep(500);
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        string filePath;
        if (args.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Uso: CambiarFormulario <ruta del ensamblado>");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Introduzca la ruta del ensamblado: ");
            Console.ResetColor();
            filePath = Console.ReadLine();
        }
        else
        {
            filePath = args[0];
        }

        // Cargar el ensamblado de la aplicación
        var assembly = AssemblyDef.Load(filePath);

        // Buscar la clase que contiene el método Main()
        var typeDef = assembly.ManifestModule.Types.Single(t => t.Name == "Program");

        // Obtener una referencia al objeto del formulario actual
        var loginFormRef = typeDef.Methods.Single(m => m.Name == "Main")
            .Body.Instructions
            .Single(i => i.OpCode == OpCodes.Newobj && ((IMethod)i.Operand).DeclaringType.GetBaseType()?.FullName == "System.Windows.Forms.Form")
            .Operand as IMethod;

        string formName = loginFormRef.DeclaringType.FullName;

        // Crear una lista de todos los formularios disponibles
        var availableForms = assembly.ManifestModule.Types
            .Where(t => t.BaseType != null && t.BaseType.FullName == "System.Windows.Forms.Form")
            .ToList();

        // Mostrar una lista de los nombres de los formularios disponibles y solicitar al usuario que seleccione uno
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[+]");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(" (Actual Form: ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(formName);
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(")");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[+]");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(" Formularios disponibles:\n");
        for (int i = 0; i < availableForms.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {availableForms[i].Name}");
        }

        Console.Write("Seleccione un formulario (");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"1-{availableForms.Count}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("): ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        var input = Console.ReadLine();

        if (int.TryParse(input, out int selectedIndex) && selectedIndex >= 1 && selectedIndex <= availableForms.Count)
        {
            string selectedFormName = availableForms[selectedIndex - 1].FullName;

            // Verificar si el formulario seleccionado es el mismo que el actual
            if (formName == selectedFormName)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: El formulario seleccionado es el mismo que el actual.");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }
            // Crear una referencia al constructor del formulario seleccionado
            var selectedFormTypeDef = availableForms[selectedIndex - 1];
            var selectedFormCtor = selectedFormTypeDef.FindMethod(".ctor");

            var newInstr = Instruction.Create(OpCodes.Newobj, selectedFormCtor);
            var instrs = typeDef.Methods.Single(m => m.Name == "Main").Body.Instructions;
            instrs[instrs.IndexOf(instrs.Single(i => i.Operand == loginFormRef))].OpCode = newInstr.OpCode;
            instrs[instrs.IndexOf(instrs.Single(i => i.Operand == loginFormRef))].Operand = newInstr.Operand;

            // Guardar los cambios en el ensamblado de la aplicación
            assembly.Write(GetRandomString() + ".exe");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Formulario cambiado con éxito.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Selección inválida.");
            Console.ResetColor();
            Console.ReadLine();
        }
        Console.ReadLine();
    }
}