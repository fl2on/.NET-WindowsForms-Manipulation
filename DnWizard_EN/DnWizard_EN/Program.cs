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
        int length = 10; // Length of the random string
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"; // Allowed characters

        StringBuilder builder = new StringBuilder();

        Random random = new Random();
        builder.Append("Qzxtu_"); // Add "Qzxtu_" to the beginning of the string
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
        AnimatedWriteLine("DnWizard", 50); // Animates the console output of "DnWizard"
        Thread.Sleep(500);
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        string filePath;
        if (args.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Usage: ChangeForm <assembly path>");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Enter the path to the assembly: ");
            Console.ResetColor();
            filePath = Console.ReadLine();
        }
        else
        {
            filePath = args[0];
        }

        // Load the application assembly
        var assembly = AssemblyDef.Load(filePath);

        // Find the class that contains the Main() method
        var typeDef = assembly.ManifestModule.Types.Single(t => t.Name == "Program");

        // Get a reference to the current form object
        var loginFormRef = typeDef.Methods.Single(m => m.Name == "Main")
            .Body.Instructions
            .Single(i => i.OpCode == OpCodes.Newobj && ((IMethod)i.Operand).DeclaringType.GetBaseType()?.FullName == "System.Windows.Forms.Form")
            .Operand as IMethod;

        string formName = loginFormRef.DeclaringType.FullName;

        // Create a list of all available forms
        var availableForms = assembly.ManifestModule.Types
            .Where(t => t.BaseType != null && t.BaseType.FullName == "System.Windows.Forms.Form")
            .ToList();

        // Display a list of the available form names and prompt the user to select one
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[+]");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(" (Current Form: ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(formName);
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(")");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[+]");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(" Available Forms:\n");
        for (int i = 0; i < availableForms.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {availableForms[i].Name}");
        }

        Console.Write("Select a form (");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"1-{availableForms.Count}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("): ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        var input = Console.ReadLine();

        if (int.TryParse(input, out int selectedIndex) && selectedIndex >= 1 && selectedIndex <= availableForms.Count)
        {
            string selectedFormName = availableForms[selectedIndex - 1].FullName;

            // Check if the selected form is the same as the current one
            if (formName == selectedFormName)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: The selected form is the same as the current one.");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }
            // Create a reference to the constructor of the selected form
            var selectedFormTypeDef = availableForms[selectedIndex - 1];
            var selectedFormCtor = selectedFormTypeDef.FindMethod(".ctor");

            var newInstr = Instruction.Create(OpCodes.Newobj, selectedFormCtor);
            var instrs = typeDef.Methods.Single(m => m.Name == "Main").Body.Instructions;
            instrs[instrs.IndexOf(instrs.Single(i => i.Operand == loginFormRef))].OpCode = newInstr.OpCode;
            instrs[instrs.IndexOf(instrs.Single(i => i.Operand == loginFormRef))].Operand = newInstr.Operand;

            // Save changes to the application's assembly
            assembly.Write(GetRandomString() + ".exe");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Form changed successfully.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid selection.");
            Console.ResetColor();
            Console.ReadLine();
        }
        Console.ReadLine();
    }
}