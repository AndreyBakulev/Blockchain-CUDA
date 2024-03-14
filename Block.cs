using System.Security.Cryptography;
using System.Text;
public class Block{
    private int index;
    private long timestamp;
    private string data;
    private string previousHash;
    private string hash;
    private long nonce;
      public Block(string data, Block previousBlock)
    {
        this.index = previousBlock?.GetIndex() + 1 ?? 0;
        this.data = data;
        this.previousHash = previousBlock?.GetHash() ?? "0";
        this.nonce = 0;
        this.hash = "unknown";
        this.timestamp = DateTime.Now.Ticks;
    }
    public int GetIndex(){return index;}
    public long GetTimestamp(){return timestamp;}
    public string GetData(){return data;}
    public string GetPreviousHash(){return previousHash;}
    public string GetHash(){return hash;}
    public long GetNonce(){return nonce;}
    public void SetIndex(int index){this.index = index;}
    public void SetHash(string hash){this.hash = hash;} 
    public void SetNonce(long nonce){this.nonce = nonce;}
    
    public static void PrintBlock(Block b){
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Index: " + b.index);
        Console.WriteLine("Timestamp: " + b.timestamp);
        Console.WriteLine("Data: " + b.data);
        Console.WriteLine("Previous Hash: " + b.previousHash);
        Console.WriteLine("Nonce: " + b.nonce);
        Console.WriteLine("Hash: " + b.hash);
        Console.WriteLine("--------------------------------");
    }
    public static string CalculateHash(string rawData)
        {
            // Create a SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
}