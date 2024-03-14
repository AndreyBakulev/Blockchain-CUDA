public class Program
{
    public static void Main(string[] args){
        Console.WriteLine("Welcome to Andrey's Bitcoin Miner!");
        Console.WriteLine("Please select a difficulty of the blockchain");
        int difficulty = Int32.Parse(Console.ReadLine());
        Blockchain blockchain = new(difficulty);
        Console.WriteLine("For Parrallelism, select 1, and without select 2");
        string choice = Console.ReadLine();
        if (choice == "1"){
            blockchain.StartMiningParallel();
        }
        else if (choice == "2"){
            blockchain.StartMining();
        }
        else{
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid Input");
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }
    }
}
/*
Notes:
validateChain isnt working bc idk
ADDITIONS{
    make this with cuda!
}
LEVELS:
0: single threaded running
1: parallel running
2: parallel running with cuda
3: rust
*/