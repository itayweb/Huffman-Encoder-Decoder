using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace HuffmanEncoderDecoder
{
    public class HuffmanNode : IComparable<HuffmanNode>
    {
        public string letter;
        public int priority;
        public string code;
        public HuffmanNode parentNode;
        public HuffmanNode leftChild;
        public HuffmanNode rightChild;
        public static List<HuffmanNode> huffList;
        public static Dictionary<string, string> lettersCodes = new Dictionary<string, string>();

        public HuffmanNode(string value)
        {
            letter = value;
            priority = 1;
            code = "";
            parentNode = null;
            leftChild = null;
            rightChild = null;
        }

        public HuffmanNode(HuffmanNode hNode1, HuffmanNode hNode2)
        {
            code = "";

            if (hNode1.priority >= hNode2.priority)
            {
                rightChild = hNode1;
                leftChild = hNode2;
                rightChild.parentNode = this;
                leftChild.parentNode = this;
                letter = hNode1.letter + hNode2.letter;
                priority = hNode1.priority + hNode2.priority;
            }
            else
            {
                rightChild = hNode2;
                leftChild = hNode1;
                rightChild.parentNode = this;
                leftChild.parentNode = this;
                letter = hNode2.letter + hNode1.letter;
                priority = hNode1.priority + hNode2.priority;
            }
        }

        public int CompareTo(HuffmanNode hNode)
        {
            return this.priority.CompareTo(hNode.priority);
        }

        public void IncreasePriority()
        {
            priority++;
        }
    }

    class Program
    {
        // Convert content of text file into a list and filter out the extra words
        // by not adding them and instead adding priority
        public static List<HuffmanNode> ConvertFileToList(string filePath)
        {
            List<HuffmanNode> l = new List<HuffmanNode>();
            try
            {
                FileStream txtFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                for (int i = 0; i < txtFileStream.Length; i++)
                {
                    string txtLetter = Convert.ToChar(txtFileStream.ReadByte()).ToString();
                    if (l.Exists(hNodeTest => hNodeTest.letter == txtLetter))
                        l[l.FindIndex(hNode => hNode.letter == txtLetter)].IncreasePriority();
                    else
                        l.Add(new HuffmanNode(txtLetter));
                }
                txtFileStream.Dispose();
                return l;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Convert every 2 nodes to a parent tree as long as theres at least 2 nodes
        public static void ConvertListToTree(List<HuffmanNode> l)
        {
            while(l.Count > 1)
            {
                HuffmanNode hNode1 = l[0];
                l.RemoveAt(0);
                HuffmanNode hNode2 = l[0];
                l.RemoveAt(0);
                l.Add(new HuffmanNode(hNode1, hNode2));
            }
        }

        // Going through each node in the tree and and set each according to their position
        // (plus 0 for left or plus 1 for right)
        public static void SetCodeToLetter(string code, HuffmanNode node)
        {
            if (node == null)
                return;
            if (node.leftChild == null && node.rightChild == null)
            {
                node.code = code;
                return;
            }
            SetCodeToLetter(code + "0", node.leftChild);
            SetCodeToLetter(code + "1", node.rightChild);
        }

        // Get the text file content
        public static string ConvertTxtFileToString(string path)
        {
            FileStream txtFileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            string result = "";
            for (int i = 0; i < txtFileStream.Length; i++)
            {
                result += Convert.ToChar(txtFileStream.ReadByte()).ToString();
            }
            return result;
        }

        // Convert each letter in original text file to Huffman code using generate dictionary
        public static string GetHuffmanCode(string txtContent)
        {
            string resultCode = "";            
            for (int i = 0; i < txtContent.Length; i++)
            {
                foreach (var letterCode in HuffmanNode.lettersCodes)
                {
                    char testKey = Convert.ToChar(letterCode.Key);                    
                    if (testKey == txtContent[i])
                    {
                        string value = letterCode.Value;
                        resultCode += value;
                    }
                }
            }
            return resultCode;
        }

        // Convert the binary string that generated to byte array accordingly
        public static byte[] StringToByteArray(string str)
        {
            int byteArrSize = 0;            
            if (str.Length % 8 == 0)
                byteArrSize = str.Length / 8;
            else
            {
                if (str.Length > 8)
                {
                    while (str.Length % 8 != 0)
                    {
                        str = str + "0";
                    }
                    byteArrSize = str.Length / 8;
                }
                else
                {
                    while (str.Length % 8 != 0)
                    {
                        str = str + "0";
                    }
                    byteArrSize = 1;
                }
            }
            byte[] byteArr = new byte[byteArrSize];
            for (int i = 0; i < byteArrSize; i++)
            {
                byteArr[i] = Convert.ToByte(str.Substring(i * 8, 8), 2);
            }
            return byteArr;
        }

        // Convert each byte in the byte array to a binary string
        public static string ByteArrayToString(byte[] arr)
        {
            string compressedStr = "";
            foreach (byte bit in arr)
            {
                compressedStr += Convert.ToString(bit, 2).PadLeft(8, '0');
            }
            return compressedStr;
        }

        // Add each letter's and code's node to the dictionary
        public static void HuffmanNodesToDictionary(HuffmanNode node)
        {
            if (node == null)
                return;
            HuffmanNode.lettersCodes.Add(node.letter, node.code);
            HuffmanNodesToDictionary(node.leftChild);
            HuffmanNodesToDictionary(node.rightChild);
        }

        // Remove any excess keys and values in the dictionary 
        public static void FilterDictionary()
        {
            foreach (var letterCode in HuffmanNode.lettersCodes)
            {
                if (letterCode.Value == "")
                    HuffmanNode.lettersCodes.Remove(letterCode.Key);
            }
        }

        // Get the amount of letters in text file content
        public static int CountLetterFromFile(string path)
        {
            int count;
            FileStream txtFileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            count = (int)txtFileStream.Length;
            txtFileStream.Dispose();
            return count;
        }

        /// <summary>
        /// This function takes a file name
        /// to compress using the Huffman encoding method
        /// then generates an encoded file
        /// & helpful map for decoding later
        /// (all in the current directory)
        /// </summary>
        /// <param name="txtNameSrc"></param>
        /// <param name="compressedFileName"></param>

        public static void Compress(string txtNameSrc, string compressedFileName)
        {
            string txtSrcPath = Directory.GetCurrentDirectory() + "..\\..\\..\\..\\" + txtNameSrc + ".txt";
            string compressedFilePath = Directory.GetCurrentDirectory() + "..\\..\\..\\..\\" + compressedFileName + ".huff";
            string helperFilePath = Directory.GetCurrentDirectory() + "..\\..\\..\\..\\" + compressedFileName + "Helper.helper";
            try
            {
                List<HuffmanNode> huffList = new List<HuffmanNode>();
                huffList = ConvertFileToList(txtSrcPath);
                ConvertListToTree(huffList);
                SetCodeToLetter("", huffList[0]);
                FileStream compressedFileStream = new FileStream(compressedFilePath, FileMode.Create, FileAccess.Write);
                HuffmanNodesToDictionary(huffList[0]);
                FilterDictionary();
                string txtContent = ConvertTxtFileToString(txtSrcPath);
                string compressedStr = GetHuffmanCode(txtContent);
                FileStream helperFileStream = new FileStream(helperFilePath, FileMode.Create, FileAccess.Write);
                helperFileStream.Dispose();
                byte[] bitArr = StringToByteArray(compressedStr);
                compressedFileStream.Dispose();
                File.WriteAllBytes(compressedFilePath, bitArr);
                string helperStr = "";
                helperStr += CountLetterFromFile(txtSrcPath).ToString();
                helperStr += "#";
                foreach (var letterCode in HuffmanNode.lettersCodes)
                {
                    helperStr += letterCode.Key;
                    helperStr += letterCode.Value;
                }
                File.WriteAllText(helperFilePath, helperStr);
                HuffmanNode.lettersCodes = new Dictionary<string, string>();
                Console.WriteLine("The txt file has compressed and coded successfully!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// The function takes an encoded file name to decompress
        /// using the Huffman encoding method and the map created earlier
        /// then generates a text file with decoded text in it
        /// </summary>
        /// <param name="compressedNameSrc"></param>
        /// <param name="txtNameDst"></param>

        public static void Decompress(string compressedNameSrc, string txtNameDst)
        {
            string compressedSrcPath = Directory.GetCurrentDirectory() + "..\\..\\..\\..\\" + compressedNameSrc + ".huff";
            string txtDecompressedPath = Directory.GetCurrentDirectory() + "..\\..\\..\\..\\" + txtNameDst + ".txt";
            string helperFileSrcPath = Directory.GetCurrentDirectory() + "..\\..\\..\\..\\" + compressedNameSrc + "Helper.helper";
            try
            {
                FileStream compressedFileStream = new FileStream(compressedSrcPath, FileMode.Open, FileAccess.Read);
                BinaryReader byteReader = new BinaryReader(compressedFileStream);
                byte[] bitArr = File.ReadAllBytes(compressedSrcPath);
                FileStream helperFileStream = new FileStream(helperFileSrcPath, FileMode.Open, FileAccess.Read);
                helperFileStream.Dispose();
                string helperFileStr = File.ReadAllText(helperFileSrcPath).ToString();
                string startIndexStr = "";
                int countStopIndex = 0;
                for (int i = 0; i < helperFileStr.Length; i++)
                {
                    if (helperFileStr[i] == '#')
                    {
                        countStopIndex = i;
                        break;
                    }
                    startIndexStr += helperFileStr[i];
                }
                int startIndex;
                int.TryParse(startIndexStr, out startIndex);
                string compressedStr = ByteArrayToString(bitArr);
                Dictionary<string, string> lettersCode = new Dictionary<string, string>();
                char currentKey = '\0';
                for (int i = countStopIndex + 1; i < helperFileStr.Length; i++)
                {
                    if (helperFileStr[i] >= 'a' && helperFileStr[i] <= 'h')
                    {
                        currentKey = helperFileStr[i];
                        lettersCode.Add(currentKey.ToString(), "");
                    }
                    else
                    {
                        lettersCode[currentKey.ToString()] += helperFileStr[i];
                    }
                }
                string decompressedTxt = "";
                string checkCodeStr = "";
                int checkCorrectCount = 0;
                bool isFinished = false;
                for (int i = 0; i < compressedStr.Length; i++)
                {
                    checkCodeStr += compressedStr[i];
                    foreach (var letterCode in lettersCode)
                    {
                        if (letterCode.Value == checkCodeStr)
                        {
                            decompressedTxt += letterCode.Key;
                            checkCodeStr = "";
                            compressedStr.Remove(0, i + 1);
                            checkCorrectCount++;
                            if (checkCorrectCount == startIndex)
                            {
                                isFinished = true;
                                break;
                            }
                        }
                    }
                    if (isFinished)
                        break;
                }
                FileStream decodedFileStream = new FileStream(txtDecompressedPath, FileMode.Create, FileAccess.Write);
                decodedFileStream.Dispose();
                File.WriteAllText(txtDecompressedPath, decompressedTxt);
                Console.WriteLine("The file has decompressed and decoded successfully!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void Main(string[] args)
        {
            bool isFinished = false;
            while (!isFinished)
            {
                Console.WriteLine("----- Hello! -------");
                Console.WriteLine("Please enter you choice (e for encode/compress file, d to decode/decompress file, or x for exit the program):");
                char key = char.Parse(Console.ReadLine());
                if (key == 'e')
                {
                    Console.WriteLine("You've chose to encode/compress a file, please enter the name of the txt file first, and then the compress file name:");
                    string txtFileName = Console.ReadLine();
                    string compressedFileName = Console.ReadLine();
                    Compress(txtFileName, compressedFileName);
                    Console.WriteLine();
                }
                
                if (key == 'd')
                {
                    Console.WriteLine("You've chose to decode/decompress a file, please enter the name of the compress file first, and then the txt file name:");
                    string compressedFileName = Console.ReadLine();
                    string txtFileName = Console.ReadLine();
                    Decompress(compressedFileName, txtFileName);
                    Console.WriteLine();
                }

                if (key == 'x')
                {
                    Console.WriteLine("Exiting...");
                    isFinished = true;
                }
            }
        }
    }
}
