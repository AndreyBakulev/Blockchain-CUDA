using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using CUDA;
using CUDA.Runtime;

public class Blockchain
{
    private LinkedList<Block> chain = new();
    private int difficulty;
    private List<double> times = new();

    public Blockchain(int difficulty)
    {
        this.difficulty = difficulty;
        this.createGenesisBlock();
    }

    public void createGenesisBlock()
    {
        Block genesisBlock = new("Genesis Block", null);
        genesisBlock.SetHash("0");
        chain.AddLast(genesisBlock);
    }

    public void AddBlock(Block b)
    {
        chain.AddLast(b);
    }

    public void removeBlock(Block b)
    {
        chain.Remove(b);
    }

    public Block GetLatestBlock()
    {
        return chain.Last.Value;
    }

    public LinkedList<Block> GetChain()
    {
        return chain;
    }

    public bool ValidateChain()
    {
        for (int i = 1; i < chain.Count; i++)
        {
            Block currentBlock = chain.ElementAt(i);
            Block previousBlock = chain.ElementAt(i - 1);
            if (Block.CalculateHash(previousBlock.GetIndex() + previousBlock.GetPreviousHash() + previousBlock.GetData() + currentBlock.GetNonce()) != currentBlock.GetPreviousHash())
            {
                return false;
            }
        }
        return true;
    }

    [global]
    //the equivalent of calculateHash and happens on gpu
    public static void HashKernel(byte[] data, int dataLength, int nonce, byte[] hash)
    {
        int index = cuda.blockIdx.x * cuda.blockDim.x + cuda.threadIdx.x;
        if (index < dataLength)
        {
            // hashing here
            // Example: hash[index] = data[index] ^ nonce;
        }
    }

    public void StartMiningCUDA()
    {
        Block newBlock = GetLatestBlock();
        int nonce = 0;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        string baseBlock = newBlock.GetIndex() + newBlock.GetPreviousHash() + newBlock.GetData();
        string correctString = new('0', difficulty);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Starting Mining on chain of {difficulty} difficulty using CUDA");

        // Prepare data for hashing
        byte[] data = System.Text.Encoding.UTF8.GetBytes(baseBlock);
        int dataLength = data.Length;

        // Allocate memory on GPU
        CUdeviceptr dataPtr = cuda.Malloc(data);
        CUdeviceptr hashPtr = cuda.Malloc(new byte[dataLength]);

        // Copy data to GPU
        cuda.CopyToDevice(dataPtr, data);

        string hash = "";
        bool hashFound = false;

        while (!hashFound)
        {
            // Launch kernel
            dim3 blockDim = new dim3(256);
            dim3 gridDim = new dim3((dataLength + blockDim.x - 1) / blockDim.x);
            cuda.Launch(blockDim, gridDim).HashKernel(dataPtr, dataLength, nonce, hashPtr);

            // Copy hash from GPU
            byte[] hashBytes = new byte[dataLength];
            cuda.CopyToHost(hashPtr, hashBytes);

            hash = System.Text.Encoding.UTF8.GetString(hashBytes);

            if (hash[..difficulty] == correctString)
            {
                hashFound = true;
            }
            else
            {
                Console.Write($"\rNonce: {nonce}, Hash: {hash}");
                nonce++;
            }
        }

        // Free GPU memory
        cuda.Free(dataPtr);
        cuda.Free(hashPtr);

        watch.Stop();
        double seconds = (double)watch.ElapsedMilliseconds / 1000;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Block Mined in {seconds} seconds using CUDA! \nNonce: {nonce} \nHash: {hash}");
        Console.WriteLine("Please enter the data for a new Block");
        Block newBlock2 = new(Console.ReadLine(), GetLatestBlock());
        newBlock2.SetNonce(nonce);
        newBlock2.SetHash(hash.ToString());
        AddBlock(newBlock2);
        Console.ForegroundColor = ConsoleColor.White;
        times.Add(seconds);
        Console.WriteLine("To Mine the next node, type 1");
        if (Console.ReadLine() == "1")
        {
            StartMiningCUDA();
        }
        else
        {
            double averageTime = 0;
            for (int i = 0; i < times.Count; i++)
            {
                averageTime += times[i];
            }
            Console.WriteLine($"Your average time to find a difficulty {difficulty} hash was {averageTime / times.Count} seconds");
            times.Clear();
            Console.WriteLine("Ok, Goodbye!");
        }

        using (StreamWriter outputFile = new StreamWriter("BlockchainLedger.txt"))
        {
            for (int i = 0; i < GetChain().Count; i++)
            {
                Block currentBlock = GetChain().ElementAt(i);
                outputFile.WriteLine(
                    $"Index: {currentBlock.GetIndex()}, Hash: {currentBlock.GetHash()}, Prev Hash: {currentBlock.GetPreviousHash()}, Nonce: {currentBlock.GetNonce()}, Data: {currentBlock.GetData()},");
            }
        }
    }

    public void RetrieveBlock(int index)
    {
        Block.PrintBlock(GetChain().ElementAt(index));
    }

    public void removeBlock(int index)
    {
        GetChain().Remove(GetChain().ElementAt(index));
        for (int i = index; i < GetChain().Count; i++)
        {
            // mine each block in front of the deleted ones
        }
    }
}
